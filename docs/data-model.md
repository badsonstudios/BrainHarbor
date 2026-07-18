# BrainHarbor — Data Model

Companion to [PLAN.md](../PLAN.md). Two stores: **curated content = Markdown in the repo** (schema in [content-pipeline.md](content-pipeline.md) §3), **dynamic content = PostgreSQL** — which is now a **v1 requirement**, since aggregation is the core product.

## v1 PostgreSQL schema

### aggregated_items — the heart of the site

```sql
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
    tumor_tags    text[] DEFAULT '{}',  -- slugs from repo taxonomy (glioma, gbm, meningioma…)
    research_stage text,                -- human_trial | observational | review_guideline |
                                        -- preclinical_animal | preclinical_cell | news_other
    relevance     text NOT NULL DEFAULT 'pending',
                                        -- pending | patient_relevant | early_stage | excluded
    classify_model text,                -- model id used, for auditability

    -- plain-language summary (pipeline stage 3; only for patient_relevant + early_stage)
    plain_title       text,             -- de-jargoned headline
    plain_summary     text,             -- structured: what was studied / found / means & doesn't mean
    summary_model     text,
    prompt_version    text,             -- which prompt template produced this
    summary_generated_at timestamptz,
    summary_flagged   boolean NOT NULL DEFAULT false,  -- reader hit "report a problem"

    -- publication workflow (human review gate — nothing public without approval)
    status        text NOT NULL DEFAULT 'pending',
        -- pending | published | rejected | pulled (was live, taken down)
    reviewed_at   timestamptz,
    review_note   text,                 -- e.g. visible correction note after an edit

    slug          text UNIQUE,          -- permalink for the item page (SEO surface)
    UNIQUE (source, external_id)
);
CREATE INDEX ON aggregated_items (status, published_at DESC);
CREATE INDEX ON aggregated_items USING gin (tumor_tags);
```

Item lifecycle: the **local pipeline** uploads finished items (classified + summarized) via the sync API as `status='pending'` → Dan approves/edits/rejects in the site's **admin review queue** → approval flips to `published`. The public feed renders only `published` + `relevance='patient_relevant'` by default, with the "show early-stage research" toggle for `early_stage`; `excluded` relevance is never uploaded at all (filtered locally, saving Claude time). Because of the gate, **every published summary is human-reviewed**.

### subscribers + digest — the email loop

```sql
CREATE TABLE subscribers (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email           text NOT NULL UNIQUE,
    tumor_prefs     text[] DEFAULT '{}',       -- empty = everything
    confirmed_at    timestamptz,               -- double opt-in; NULL = unconfirmed, purge after 7d
    confirm_token   text NOT NULL,
    unsubscribe_token text NOT NULL,
    created_at      timestamptz NOT NULL DEFAULT now(),
    unsubscribed_at timestamptz
);

CREATE TABLE digest_issues (
    id           bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sent_at      timestamptz,
    item_ids     bigint[] NOT NULL,     -- what was included (audit + "past issues" page)
    subject      text NOT NULL,
    recipients   int
);
```

> If a hosted ESP (Buttondown/Kit) is used per [architecture.md](architecture.md) §5, `subscribers` lives at the ESP and this table shrinks to `digest_issues` only. Keep the schema here in case of a later self-managed switch.

### trials_cache — ClinicalTrials.gov v2

```sql
CREATE TABLE trials_cache (
    nct_id        text PRIMARY KEY,
    title         text NOT NULL,
    conditions    text[],
    phase         text,
    overall_status text,               -- recruiting etc.
    locations     jsonb,               -- [{facility, city, state, lat, lon}]
    summary       text,
    plain_summary text,                -- same summarization treatment as papers
    last_update_posted date,
    fetched_at    timestamptz NOT NULL
);
```

"Near me" = browser geolocation / ZIP → lat-lon → live `filter.geo` query; the cache backs browse lists and "newly posted trials" feed entries (which also land in `aggregated_items` as `trial_update`).

### sync bookkeeping

No job framework on the server (the pipeline runs on Dan's PC). One small table backs the sync API and the admin health page:

```sql
CREATE TABLE source_sync_state (
    source          text PRIMARY KEY,
    last_success_at timestamptz,
    last_error      text,
    cursor          text        -- e.g. last PubMed date window fetched
);
```

`GET /api/sync/state` reads it; each successful upload updates it. The admin page renders "PubMed last synced N days ago" from this — staleness is always visible, never silent. Dedupe needs no extra table: `POST /api/sync/check` answers "is this new?" straight from `aggregated_items` keys, and the upload upsert is idempotent regardless.

## Later-phase tables

### stories — moderated patient stories (Phase 3, unchanged requirements)

```sql
CREATE TABLE stories (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    status          text NOT NULL DEFAULT 'pending',
        -- pending | approved | approved_with_note | declined | withdrawn
    display_name    text NOT NULL,
    email           text NOT NULL,             -- never rendered publicly
    author_role     text NOT NULL,             -- patient | caregiver | family
    tumor_type      text,
    tumor_grade     text,
    treatment_tags  text[] DEFAULT '{}',
    diagnosis_year  int,
    body_md         text NOT NULL,
    body_published_md text,
    editorial_note  text,
    photo_blob_path text,
    consent_publish     boolean NOT NULL,
    consent_edit        boolean NOT NULL,
    consent_scope       text NOT NULL,
    consent_captured_at timestamptz NOT NULL,
    slug            text UNIQUE,
    submitted_at    timestamptz NOT NULL DEFAULT now(),
    reviewed_at     timestamptz,
    reviewed_by     text,
    decline_reason  text,
    removal_requested_at timestamptz,
    submit_ip_hash  text
);
```

Plus append-only `story_events (story_id, event, actor, at, detail)` for moderation audit.

## Curated-content taxonomy (repo, not DB)

Tumor types and section slugs are defined once in `Content/taxonomy.yml` and referenced everywhere: classifier output (`tumor_tags`), feed filters, subscriber prefs, and (later) curated-page front matter and story tagging. One source of truth; the classifier is constrained to emit only these slugs.

## PII notes

- v1's only PII is **subscriber emails** — the most sensitive kind of "low-sensitivity" data there is, because the list itself reveals *who is tracking brain-tumor research* (i.e., likely patients/caregivers). Treat the list like health data: double opt-in, one-click unsubscribe, never share/sell (say so on /privacy), purge unconfirmed rows after 7 days, hard-delete on unsubscribe after a 30-day grace.
- Stories PII rules (Phase 3): never render email; hash IPs; purge declined submissions after ~90 days; EXIF-strip photos; unpublish immediately on removal request, hard-delete on demand.
- Postgres Flexible Server automated backups (7-day PITR default); monthly `pg_dump` to Blob for the subscribers and (later) stories tables — the irreplaceable ones.
