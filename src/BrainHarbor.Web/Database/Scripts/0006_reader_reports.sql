-- WI-306: a reader can report a problem with a published summary. That report
-- flags the item (summary_flagged) so it surfaces in the admin queue, and is
-- recorded in the same append-only audit trail as review decisions — so
-- "someone flagged this and here's when" is history, not a lost signal.
-- Reporting deliberately does NOT unpublish: one reader must not be able to
-- take a page down; a person decides what to do after seeing the report.

ALTER TABLE review_events
    DROP CONSTRAINT review_events_action_check;

ALTER TABLE review_events
    ADD CONSTRAINT review_events_action_check
        CHECK (action IN ('approved', 'rejected', 'pulled', 'reopened', 'reported'));
