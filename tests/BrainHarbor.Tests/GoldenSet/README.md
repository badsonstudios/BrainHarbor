# Golden set (WI-301)

`golden-set.json` is the **quality yardstick** for the M3 pipeline: real
PubMed abstracts paired with the classification (tumor tags, relevance tier,
research stage) and, for ~half, the *ideal* plain-language summary a good run
should produce. Every prompt or model change re-runs the golden set; a
regression blocks the change (content-pipeline.md §10).

> **Status: DRAFT — awaiting Dan's ratification.** The classifications and
> ideal summaries were hand-verified by the assistant from the real abstracts,
> but the golden set is the baseline every future summary is judged against,
> so a human should ratify it before it gates prompt changes. Read a sample of
> the `ideal_summary` blocks against the `raw_summary` and correct anything you
> disagree with — that act *is* the ratification.

## Entry shape

```jsonc
{
  "input":    { source, source_kind, external_id, title, raw_summary, published_at },
  "expected": { tumor_tags[], relevance, research_stage },
  "note":     "why this classification — especially for hard/borderline cases",
  "ideal_summary": {            // present for ~10 patient_relevant items
    plain_title, what_studied, what_found, means, doesnt_mean, stage_label
  }
}
```

- `tumor_tags` — only slugs from `Content/taxonomy.yml`. Tag the most specific
  type; `all-brain-tumors` for broad items; `[]` for `excluded`.
- `relevance` — `patient_relevant` | `early_stage` | `excluded`.
- `research_stage` — `human_trial` | `observational` | `review_guideline` |
  `preclinical_animal` | `preclinical_cell` | `news_other`.

## Rules an ideal summary must satisfy (verified by WI-304's checks)

1. All six blocks present and non-empty.
2. Every number appears verbatim in `raw_summary` (numeral post-check).
3. No banned hype words (breakthrough, miracle, game-changer, cure).
4. Reading level ≤ 8.5 (Flesch-Kincaid).
5. A mandatory "what it doesn't mean" block — the anti-hype guardrail.

## How to add cases (grow toward ~30)

1. Pick a real fetched item that fills a gap — a tumor type, stage, or
   relevance tier that's thin. Aim for balance across the matrix.
2. Read the abstract; assign `expected` by the rules above; write a one-line
   `note` (mandatory for anything borderline).
3. For patient_relevant human items, write the `ideal_summary` — plain,
   numbers from the source only, short sentences.
4. Re-run `dotnet test --filter GoldenSet`.

## Findings from the first pass (worth acting on)

- **Taxonomy gap: spinal-cord tumors — RESOLVED.** Added a standalone
  `spinal-cord-tumor` slug (not a child of any brain type, so it never
  surfaces under a brain filter) and retagged `42047997` and `42107211` to it.
  Note `acoustic-neuroma` is specifically *vestibular* schwannoma, not the
  cervical-spine schwannomas in `42107211` (now `spinal-cord-tumor`).
- **Hard case: technical reviews.** `42031225` (GBM resistance mechanisms) is
  on-topic but dense; classified `patient_relevant` / `review_guideline` as a
  landscape review. The human review gate is where calls like this get made.
