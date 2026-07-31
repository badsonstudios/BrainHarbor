-- Readiness score: how close a finding is to being something a patient can
-- actually get, on a 1-10 scale (10 = approved/standard care today, 1 = lab or
-- idea stage). Set by the pipeline's summarizer and clamped there by research
-- stage, so lab/animal work can never read as near-clinic. The CHECK is a
-- backstop against a bad payload writing an off-scale number to a page a scared
-- reader trusts. readiness_reason is one plain sentence explaining the score.

ALTER TABLE aggregated_items
    ADD COLUMN readiness_score  smallint,
    ADD COLUMN readiness_reason text,
    ADD CONSTRAINT readiness_score_range
        CHECK (readiness_score IS NULL OR readiness_score BETWEEN 1 AND 10);
