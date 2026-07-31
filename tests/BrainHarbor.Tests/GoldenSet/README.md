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
6. A `readiness` score (1-10) that respects its stage ceiling — the same cap
   `Readiness.Clamp` enforces in the pipeline (e.g. an animal study ≤ 2). The
   yardstick can't bless the model over-promising.

## How to add cases (grow toward ~30)

1. Pick a real fetched item that fills a gap — a tumor type, stage, or
   relevance tier that's thin. Aim for balance across the matrix.
2. Read the abstract; assign `expected` by the rules above; write a one-line
   `note` (mandatory for anything borderline).
3. For patient_relevant human items, write the `ideal_summary` — plain,
   numbers from the source only, short sentences.
4. Re-run `dotnet test --filter GoldenSet`.

## Accuracy run — recorded (WI-303)

First live run of the `classify-v1` prompt through the local `claude` CLI
against all 20 items (2026-07-30). **The CLI used `claude-haiku-4-5`** for
`claude -p` — worth noting, since a stronger model would likely do better on
tag completeness.

| Metric | Result |
|---|---|
| Research stage exact | **20/20 (100%)** |
| Relevance tier exact | **18/20 (90%)** |
| Tumor tags — ≥1 correct (primary) | **18/20 (90%)** |
| Tumor tags — exact set | 13/20 (65%) |

The disagreements are borderline editorial calls, not errors, and the human
review gate catches them:

- **Relevance (2 misses):** both are items I marked `excluded` where the model
  kept them — `42157432` (a general photodynamic-therapy review that lists GBM
  among many cancers) and `42323004` (Teneurin-4 across solid tumors, brain in
  passing). The model is more inclusive on brain-adjacent reviews; erring
  toward "show it for review" is the safe direction.
- **Tags (exact 65%, overlap 90%):** misses are almost all *completeness*, not
  wrong tags — the model gets the primary type but sometimes drops a secondary
  one (e.g. omits `oligodendroglioma` when the item names it). Because feed
  filters walk the taxonomy tree (a `glioma` filter catches its children), a
  missing child tag rarely changes what a reader sees.

Verdict: the classifier is sound on this yardstick. Two things to consider
before Auto mode: (1) pin a stronger model than Haiku for classify/summarize
if tag completeness matters; (2) the borderline "excluded" cases are exactly
where the human review gate earns its keep.

## Summary run with Opus — recorded (WI-304)

Ran `summarize-v1` through `claude --model claude-opus-5` on a representative
subset (2026-07-30). **Quality was excellent** — accurate, honest, with strong
"what this doesn't mean" anti-hype blocks. One real finding: Opus writes
*thorough* summaries that packed in research jargon (hazard ratios, confidence
intervals, long sentences), pushing several over the 8.5 reading-level ceiling
(grade 10+). That's the guardrail working — but the fix is upstream: the prompt
was tightened to forbid statistics/jargon ("say 'a higher risk of dying', not
'hazard ratio 5.29'") and demand very short sentences. Re-running the flagged
items dropped them from grade **10.8/10.0/10.6 → 5.7/7.1/6.6** with no loss of
accuracy. Lesson for Auto mode: the reading-level guardrail is load-bearing —
keep it, and keep tuning the prompt against it.

## Findings from the first pass (worth acting on)

- **Taxonomy gap: spinal-cord tumors — RESOLVED.** Added a standalone
  `spinal-cord-tumor` slug (not a child of any brain type, so it never
  surfaces under a brain filter) and retagged `42047997` and `42107211` to it.
  Note `acoustic-neuroma` is specifically *vestibular* schwannoma, not the
  cervical-spine schwannomas in `42107211` (now `spinal-cord-tumor`).
- **Hard case: technical reviews.** `42031225` (GBM resistance mechanisms) is
  on-topic but dense; classified `patient_relevant` / `review_guideline` as a
  landscape review. The human review gate is where calls like this get made.
