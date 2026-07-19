-- WI-201: the core schema. Exactly per docs/data-model.md — aggregated_items
-- is the heart of the site, source_sync_state backs the sync API and the
-- admin health page.

CREATE TABLE aggregated_items (
    id            bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    source        text NOT NULL,        -- pubmed | nci_rss | sciencedaily | medrxiv | biorxiv | ctgov
    source_kind   text NOT NULL,        -- research | news | preprint | trial_update
    external_id   text NOT NULL,        -- PMID / DOI / guid / NCT id — the dedupe key
    title         text NOT NULL,        -- original title
    raw_summary   text,                 -- abstract or feed summary (per-source licensing rules)
    url           text NOT NULL,
    published_at  date,
    fetched_at    timestamptz NOT NULL DEFAULT now(),

    -- classification (pipeline stage 2)
    tumor_tags    text[] NOT NULL DEFAULT '{}',  -- slugs from Content/taxonomy.yml
    research_stage text,                -- human_trial | observational | review_guideline |
                                        -- preclinical_animal | preclinical_cell | news_other
    relevance     text NOT NULL DEFAULT 'pending',
                                        -- pending | patient_relevant | early_stage | excluded
    classify_model text,                -- model id used, for auditability

    -- plain-language summary (pipeline stage 3; only for patient_relevant + early_stage)
    plain_title       text,
    plain_summary     text,
    summary_model     text,
    prompt_version    text,
    summary_generated_at timestamptz,
    summary_flagged   boolean NOT NULL DEFAULT false,

    -- publication workflow (human review gate — nothing public without approval)
    status        text NOT NULL DEFAULT 'pending',
                                        -- pending | published | rejected | pulled
    reviewed_at   timestamptz,
    review_note   text,

    slug          text UNIQUE,
    UNIQUE (source, external_id)
);

-- The medical-safety rules from content-pipeline.md, enforced by the database
-- rather than trusted to every future caller:
--   * a preprint can never be patient_relevant (§9 classification rules)
--   * only the documented enum values are accepted
ALTER TABLE aggregated_items
    ADD CONSTRAINT aggregated_items_source_kind_check
        CHECK (source_kind IN ('research', 'news', 'preprint', 'trial_update')),
    ADD CONSTRAINT aggregated_items_relevance_check
        CHECK (relevance IN ('pending', 'patient_relevant', 'early_stage', 'excluded')),
    ADD CONSTRAINT aggregated_items_status_check
        CHECK (status IN ('pending', 'published', 'rejected', 'pulled')),
    ADD CONSTRAINT aggregated_items_research_stage_check
        CHECK (research_stage IS NULL OR research_stage IN (
            'human_trial', 'observational', 'review_guideline',
            'preclinical_animal', 'preclinical_cell', 'news_other')),
    ADD CONSTRAINT aggregated_items_preprint_never_patient_relevant
        CHECK (NOT (source_kind = 'preprint' AND relevance = 'patient_relevant'));

-- NULLS LAST matches the feed's ORDER BY: published_at is nullable, and the
-- Postgres DESC default (NULLS FIRST) would float undated items to the top of
-- the feed. Without the matching index the planner falls back to a sort.
CREATE INDEX aggregated_items_status_published_idx
    ON aggregated_items (status, published_at DESC NULLS LAST);
CREATE INDEX aggregated_items_tumor_tags_idx
    ON aggregated_items USING gin (tumor_tags);

CREATE TABLE source_sync_state (
    source          text PRIMARY KEY,
    last_success_at timestamptz,
    last_error      text,
    cursor          text        -- e.g. last PubMed date window fetched
);
