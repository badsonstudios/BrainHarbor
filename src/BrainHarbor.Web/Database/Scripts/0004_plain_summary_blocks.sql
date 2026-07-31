-- WI-304: the plain-language summary is stored as its template blocks
-- (content-pipeline.md §9), so the item page renders them deterministically
-- and the review queue can show summary-vs-source block by block. plain_title
-- (headline) and plain_summary (the one-sentence feed hook) already exist;
-- these hold the body blocks.

ALTER TABLE aggregated_items
    ADD COLUMN plain_what_studied text,
    ADD COLUMN plain_what_found  text,
    ADD COLUMN plain_means       text,
    ADD COLUMN plain_doesnt_mean text;
