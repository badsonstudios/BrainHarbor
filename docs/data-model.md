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
    raw_summary   text,                 -- abstract or feed summary — PIPELINE INPUT ONLY, never rendered publicly
    url           text NOT NULL,
    published_at  date,
    fetched_at    timestamptz NOT NULL DEFAULT now(),

    -- classification (pipeline stage 2)
    tumor_tags    text[] NOT NULL DEFAULT '{}',  -- slugs from repo taxonomy (glioma, gbm, meningioma…)
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

    -- publication workflow (Auto mode publishes clean summaries; flagged ones
    -- and, in Review mode, everything waits for a person. reviewed_by='auto'
    -- marks a machine-published item — see content-pipeline.md §"Publish mode")
    status        text NOT NULL DEFAULT 'pending',
        -- pending | published | rejected | pulled (was live, taken down)
    reviewed_at   timestamptz,
    review_note   text,                 -- e.g. visible correction note after an edit

    slug          text UNIQUE,          -- permalink for the item page (SEO surface)
    UNIQUE (source, external_id)
);
CREATE INDEX ON aggregated_items (status, published_at DESC NULLS LAST);
CREATE INDEX ON aggregated_items USING gin (tumor_tags);
```

**`raw_summary` is never rendered on a public page.** Abstracts can carry
publisher rights, so the site summarizes and links rather than republishing
(PLAN.md §5). The column exists solely as input to the M3 summarizer, and to
show a reviewer the source text beside the generated summary in the admin
queue. Any change that puts it in front of a reader is a licensing decision,
not a UI decision.

**CHECK constraints (added in WI-201).** The enum-ish columns are constrained
to the values documented above, and one medical-safety rule is enforced by the
database rather than trusted to every future caller:

```sql
CHECK (NOT (source_kind = 'preprint' AND relevance = 'patient_relevant'))
```

That is content-pipeline.md §9's "preprints are never patient_relevant"
made unbypassable. A preprint that later appears in a journal arrives as a new
`research` row (or has its `source_kind` updated), both of which the constraint
allows. `tumor_tags` is `NOT NULL` so GIN containment queries never have to
distinguish NULL from `'{}'`. `published_at DESC NULLS LAST` matches the feed's
ordering — the Postgres default would float undated items to the top.

Item lifecycle: the **local pipeline** uploads finished items (classified + summarized) via the sync API. In **Auto mode (the default, WI-212)** a summarized item that passed the automated safety checks lands as `published` immediately (`reviewed_by='auto'`); a flagged or not-yet-summarized item lands as `pending` for a person. In **Review mode** everything lands `pending` and Dan approves/edits/rejects in the **admin review queue**. The public feed renders only `published` items — `patient_relevant` (and `pending`-relevance until M3 classifies them) by default, with the "show early-stage research" toggle for `early_stage`; `excluded` relevance is never uploaded at all (filtered locally, saving Claude time). See content-pipeline.md §"Publish mode".

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
    nct_id             text PRIMARY KEY,
    title              text NOT NULL,
    conditions         text[]      NOT NULL DEFAULT '{}',
    phase              text,                  -- "Phase 2", "Not applicable"
    overall_status     text,                  -- "Recruiting", in plain words
    locations          jsonb       NOT NULL DEFAULT '[]'::jsonb,
        -- [{facility, city, state, country, lat, lon}]
    summary            text,                  -- the registry's own text, never shown raw
    last_update_posted date,
    fetched_at         timestamptz NOT NULL DEFAULT now()
);
```

"Near me" = browser geolocation / ZIP → lat-lon → live `filter.geo` query; the cache backs browse lists and "newly posted trials" feed entries (which also land in `aggregated_items` as `trial_update`).

**Facts vs. editorial content (WI-402).** This table holds facts only, and there is deliberately **no plain-language column here**. The plain-language text is editorial: it is what the automated safety checks gate, what a person can edit or reject in the review queue, and what a reader can report a problem with. All of that machinery lives on `aggregated_items`, so the text lives there too and `/trials` joins it (`nct_id = external_id AND source = 'ctgov'`). A second copy in this table would be a second, ungated door to the reader for exactly the prose the safety system held back.

The facts move through their own endpoint, **`POST /api/sync/trials`**, separate from `POST /api/sync/items`, because they obey the opposite rule: they refresh on every run regardless of the review freeze. A trial shown as "Recruiting" after it closed sends a patient to a door that no longer opens. Facts are uploaded *first*, so the worst case of a partial run is a trial whose facts are known before its feed item exists.

**What becomes a feed item.** Every trial the fetcher sees refreshes this table, so browse stays truthful. Only trials someone can still join (not yet recruiting, recruiting, enrolling by invitation, available) and that we have not seen before become feed items — the rest get no `aggregated_items` row at all, which keeps an administrative edit to a trial that finished in 2011 out of both the feed and the review queue. A trial we already know is never re-summarized: its summary says who the trial is for, and its live status is read from this table at render time (including on the already-published item page).

Trials get their own summarization prompt (`summarize-trial`, with its own golden-set cases): the research template asks "what did they find", and an open trial has found nothing yet.

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

**The taxonomy is a tree** (WI-201). Each entry may declare a `parent`, so
`glioblastoma → high-grade-glioma → glioma`. Rules that follow from that:

- The classifier tags the **most specific** type it can justify. It does not
  also emit the ancestors — `TaxonomyStore.Matches` walks up at query time, so
  filtering `glioma` returns glioblastoma items.
- `all-brain-tumors` is the catch-all for items that apply broadly (caregiving,
  side effects, general treatment news); it matches every filter.
- **Aliases must be true synonyms.** Naming follows WHO CNS5 (2021). "Grade 4
  glioma" is *not* an alias for glioblastoma — CNS5 grade 4 also covers
  IDH-mutant astrocytoma and H3 K27-altered midline glioma, so it maps to
  `high-grade-glioma`. Likewise DIPG is the pontine subset of diffuse midline
  glioma, not a synonym for it. Getting this wrong shows a patient research
  about a different disease with a different prognosis; when in doubt, add a
  slug rather than an alias.
- Unknown tags are dropped before they reach the database, and the rejected
  values are reported (`TagFilterResult.Rejected`) so a recurring unknown term
  becomes evidence for a new entry instead of silent data loss.

## PII notes

- v1's only PII is **subscriber emails** — the most sensitive kind of "low-sensitivity" data there is, because the list itself reveals *who is tracking brain-tumor research* (i.e., likely patients/caregivers). Treat the list like health data: double opt-in, one-click unsubscribe, never share/sell (say so on /privacy), purge unconfirmed rows after 7 days, hard-delete on unsubscribe after a 30-day grace.
- Stories PII rules (Phase 3): never render email; hash IPs; purge declined submissions after ~90 days; EXIF-strip photos; unpublish immediately on removal request, hard-delete on demand.
- Postgres Flexible Server automated backups (7-day PITR default); monthly `pg_dump` to Blob for the subscribers and (later) stories tables — the irreplaceable ones.
