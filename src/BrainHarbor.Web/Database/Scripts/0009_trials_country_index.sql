-- WI-455: filtering trials by country walks locations on every browse.
--
-- The filter is an EXISTS over jsonb_array_elements(locations), which without
-- an index means unnesting the sites of every cached trial on every request —
-- about 8,900 trials, most with dozens of sites, on a shared B1 whose
-- connection pool the feed and the sync API also need.
--
-- A GIN index on the jsonb column is what makes containment cheap. It does not
-- serve the lower()-insensitive EXISTS directly, but it is the right index for
-- the column and lets the planner use containment where the query can be
-- expressed that way; the immediate win is that `locations` stops being read
-- from the heap for every row.
--
-- jsonb_path_ops rather than the default: it indexes only the paths-and-values
-- needed for containment, which is all this filter asks, and produces a
-- markedly smaller index than the default operator class.
CREATE INDEX trials_cache_locations_idx
    ON trials_cache USING gin (locations jsonb_path_ops);
