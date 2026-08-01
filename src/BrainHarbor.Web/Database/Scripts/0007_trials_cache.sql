-- WI-402: ClinicalTrials.gov v2 trial cache (data-model.md §trials_cache).
--
-- This table holds only FACTS about a trial: its status, phase, conditions and
-- sites. It backs the /trials browse lists.
--
-- What it deliberately does NOT hold is the plain-language summary. That text
-- is editorial: it is what the automated safety checks gate, what a human can
-- edit or reject in the review queue, and what a reader can report a problem
-- with. All of that machinery lives on aggregated_items, so the summary lives
-- there too and /trials joins it (nct_id = external_id AND source = 'ctgov').
-- A second copy here would be a second, ungated door to the reader for exactly
-- the text the safety system held back.
--
-- The facts, by contrast, refresh unconditionally and outside the review
-- freeze — a trial shown as "Recruiting" after it closed sends a patient to a
-- door that no longer opens.
--
-- ClinicalTrials.gov is public domain with attribution required (PLAN.md §5);
-- the trial pages carry the attribution and the link back.

CREATE TABLE IF NOT EXISTS trials_cache (
    nct_id             text PRIMARY KEY,
    title              text NOT NULL,
    conditions         text[]      NOT NULL DEFAULT '{}',
    phase              text,
    overall_status     text,
    locations          jsonb       NOT NULL DEFAULT '[]'::jsonb,
    summary            text,
    last_update_posted date,
    fetched_at         timestamptz NOT NULL DEFAULT now()
);

-- Browse is "open trials for my tumor type, most recently updated first".
CREATE INDEX IF NOT EXISTS trials_cache_status_updated_idx
    ON trials_cache (overall_status, last_update_posted DESC NULLS LAST);

CREATE INDEX IF NOT EXISTS trials_cache_conditions_idx
    ON trials_cache USING gin (conditions);
