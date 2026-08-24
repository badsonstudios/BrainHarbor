-- WI-441: a page-view tally, and deliberately nothing more.
--
-- Dan's ask was "how many people are coming to the site", and the honest
-- answer is that you cannot count PEOPLE without persisting an identifier —
-- a cookie or a fingerprint — which is exactly what /privacy promises we do
-- not do. So this counts OPENINGS, not visitors.
--
-- What is deliberately absent, and must stay absent:
--   * no IP address, hashed or otherwise
--   * no user agent
--   * no session, visitor or device id
--   * no referrer
--   * no timestamp finer than the DAY
--
-- A row here says "this path was opened N times on this date". Two rows cannot
-- be linked to each other, and no row can be linked to a person. That is what
-- keeps the privacy promise true rather than merely narrowly-worded: this is a
-- counter, not a log of visits.
--
-- Day granularity is a privacy choice as much as a storage one. Per-hour
-- counts on a site this small would start to expose WHEN a single reader was
-- here, which is the shape of a profile even without a name on it.

CREATE TABLE page_views (
    viewed_on   date NOT NULL,
    path        text NOT NULL,
    views       bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (viewed_on, path)
);

-- The admin page reads "the last N days, newest first", so lead with the date.
CREATE INDEX page_views_recent_idx ON page_views (viewed_on DESC);
