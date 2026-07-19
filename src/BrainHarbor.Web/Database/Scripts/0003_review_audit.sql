-- WI-208: who approved what, and when. The human review gate is the site's
-- core safety promise, so the decisions behind it are append-only history
-- rather than just a status column that can be overwritten.

ALTER TABLE aggregated_items
    ADD COLUMN reviewed_by text;

CREATE TABLE review_events (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    item_id     bigint NOT NULL REFERENCES aggregated_items (id) ON DELETE CASCADE,
    action      text NOT NULL,      -- approved | rejected | pulled | reopened
    actor       text NOT NULL,      -- admin email at the time of the decision
    note        text,
    occurred_at timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT review_events_action_check
        CHECK (action IN ('approved', 'rejected', 'pulled', 'reopened'))
);

CREATE INDEX review_events_item_idx ON review_events (item_id, occurred_at DESC);
