# BrainHarbor — Content Pipeline & Editorial Workflow

Companion to [PLAN.md](../PLAN.md). Two pipelines now:

- **§1–8: Curated static pages** (hand-written, AI-assisted, human-verified) — applies to v1's handful of static pages and all of Phase 2's hub content.
- **§9–11: Automated plain-language summarization** — the v1 product. Runs **locally on Dan's PC** via the Claude Code CLI (no API key), uploads results as *pending*, and **a human approves every item before it publishes** — the review gate plus the pipeline design are the safety system.

Reality throughout: one person (Dan), no clinician on staff (yet).

## 1. The prime directive

**Never publish a medical claim that can't be pointed at a source.** For curated pages, the source is public-domain text the human verified. For automated summaries, the source is the specific abstract/record being summarized — the model is a *translator*, never an *author*, and the original is always one tap away.

## 2. Per-page workflow (curated pages)

```
1. OUTLINE     What questions does this page answer? (steal phrasing from real
               patient forums — that's the search language people actually use)
2. SOURCE      Collect public-domain / linkable sources FIRST (NCI PDQ, SSA,
               openFDA, MedlinePlus Connect). URLs + access date into front matter.
3. DRAFT       AI rewrite with a hard rule: "Use ONLY the supplied source text.
               If the sources don't cover something, write [GAP], don't fill it in."
4. VERIFY      Claim-by-claim pass against sources. Every sentence must trace.
5. READABILITY Automated gate (§5). Fails the build if grade level > 8.5.
6. HUMAN READ  Read aloud once. Would a scared person at 2am understand this?
7. PUBLISH     Merge to main → CI deploys. Git history = editorial audit trail.
8. (LATER)     Friend reviews pages nearest his experience (benefits, LGG, seizures);
               recruit clinician spot-review when feasible. /how-we-write stays honest.
```

## 3. Front matter schema (curated Markdown pages)

```yaml
---
title: "Compassionate Allowances: the fast track"
slug: fast-track
section: benefits
description: "Some brain tumors qualify for disability approval in weeks. Others don't."
tags: [ssdi, glioblastoma, compassionate-allowances]
sources:
  - url: https://www.ssa.gov/compassionateallowances/
    title: "SSA Compassionate Allowances"
    accessed: 2026-07-12
reviewed: 2026-07-12          # shown on page as "Last reviewed"
review_due: 2027-01-15        # drives the stale-content report
volatile_figures: true        # dollar amounts / yearly numbers → annual pass required
reading_grade: 7.2            # stamped by the readability tool
disclaimers: [medical, benefits]
---
```

A CI script walks all pages and reports: overdue reviews, missing sources, `volatile_figures` pages every December (SSA COLA lands October, effective January), broken outbound links (monthly).

## 4. Plain-language style guide (both pipelines)

- Sentences under ~20 words. One idea per paragraph. Question-style headers.
- Second person, active voice. Common word first, medical term in parentheses: *"swelling (your doctor may call this edema)"* — patients WILL hear the jargon from their care team, so teach it, don't hide it.
- Numbers concrete ("about 1 in 3 people"); dollar figures carry "as of [year]".
- Never: dosing instructions, individual prognosis odds, "you should start/stop".
- Curated pages end with **"What to do next"** — retention is poor; the next step must survive.

## 5. Automated gates (CI, curated pages)

| Gate | Tool | Threshold |
|---|---|---|
| Reading level | Flesch-Kincaid script | ≤ 8.5 grade, warn ≥ 7.5 |
| Glossary coverage | medical terms used but not in glossary → warn | — |
| Link rot | outbound checker (monthly job) | 0 broken |
| A11y smoke | Playwright + axe-core | 0 serious/critical |
| Review freshness | `review_due` past → report | — |

## 6. Inline definitions (tooltips)

- Glossary = one Markdown file per term (`term`, `also` aliases, `definition` ≤ 40 words, optional pronunciation).
- Markdig extension marks the **first occurrence per page** → accessible `<button>` tooltip (WCAG 1.4.13: focusable, dismissible, hoverable, touch-friendly; no-JS fallback = link to `/glossary#term`).
- **Also applied to rendered feed summaries** — jargon the summarizer had to keep (e.g. "IDH-mutant") gets tooltipped. New recurring terms in summaries feed the glossary backlog.

## 7. Update cadence

| Content | Trigger | Cadence |
|---|---|---|
| Feed items | local pipeline run (Task Scheduler) + admin approval | daily, self-healing catch-up |
| Benefits dollar figures (Phase 2) | SSA COLA | annual, hard calendar item |
| Tumor/treatment pages (Phase 2) | PDQ revisions | annual pass |
| Program/org links | orgs vanish/merge | monthly link check |
| Glossary | recurring terms in summaries + new pages | continuous |
| Classifier/summarizer prompts | flagged summaries, spot-check findings | versioned; every change re-runs the golden set (§10) |

## 8. AI-assist disclosure & trust

`/how-we-write` states the whole system plainly: how items are found, how they're filtered (and *why* mouse studies don't hit the front page), that summaries are AI-generated to a strict template with human spot-checks, how to report a bad summary, and that corrections are logged. For a site whose core product is AI-generated medical summaries, this page is not boilerplate — it IS the trust story, and the honest version of it beats borrowed authority.

---

## 9. The summarization pipeline (v1 product)

### The template (every item page renders these blocks)

1. **Plain-language title** — de-jargoned, no hype. Original title shown beneath it.
2. **What was studied** — 1–2 sentences: who/what, how many, what was tested.
3. **What they found** — 2–3 sentences, concrete numbers where the source gives them.
4. **What this means — and doesn't mean** — the anti-hype block, mandatory: stage of research, distance from clinical use, explicit "this does not mean X is a cure/available."
5. **How early is this?** — the stage badge, standardized: *Tested in people* / *Early research (animals)* / *Early research (lab cells)* / *Review of existing research* / *News* / *New or updated trial* / *Preprint — not yet checked by other scientists*.
5b. **Readiness score (1–10)** — the patient-facing "how close is this to something I can actually get?" number, with a one-line plain reason. **10** = approved/standard care a doctor can offer today; **1** = a lab result or an idea. The model proposes a score while summarizing, but it is **clamped by the research stage** (`Readiness.Clamp`, mirrored in the golden set's ceilings): `news_other`→10, `human_trial`→8, `review_guideline`→6, `observational`→5, `preclinical_animal`/`preclinical_cell`→2, unknown→5. The clamp only ever lowers a score — erring low is the safe direction, so a lab/animal study can never read as near-clinic no matter what the model returns. Bands: 9–10 available now · 7–8 late human trials · 5–6 early human trials · 4 watched in people · 3 expert review/direction · 2 animal studies · 1 lab/idea.
6. **Provenance box** — source, journal/registry, date, link to original, model disclosure. The wording is honest about how the item was published: human-reviewed items say "reviewed by a person before publishing"; auto-published items say "written by AI and published automatically after passing our automatic safety checks — a person did not review it" (see "Publish mode" below). Both invite a "report a problem".

### Prompt contract (enforced via structured output)

- Input = title + abstract/record ONLY. The model must not add outside knowledge; anything not in the source is prohibited. Fields that can't be filled from the source come back empty, and empty required fields → item stays unsummarized for manual handling (never publish a guess).
- Reading level instruction + banned-phrase list ("breakthrough", "miracle", "game-changer", "cure" unless quoting-with-context).
- Numbers must come verbatim from the source (the classic failure mode is invented percentages) — a post-check script verifies every numeral in the summary appears in the source text; mismatch → `summary_flagged`, held for review.
- Output is JSON (structured outputs) → template fields, so rendering is deterministic and a malformed response can't leak prose onto the site.

### Trials get their own template (WI-402)

A clinical trial is a different summarization problem from a paper, so
`trial_update` items use a second versioned prompt, **`summarize-trial`**, with
its own golden-set cases. The reason is block 3: a paper has a result, an open
trial does not. Asking "what did they find" of a trial description invites the
model to invent an outcome, which is the exact failure the guardrails exist to
catch. In the trial template that block describes **where the trial stands** and
must say plainly that there are no results yet, and the item page relabels the
blocks to match ("Who this trial is for" / "Where it stands").

Three rules follow from the fact that a trial's summary is written once and
never rewritten, while the trial itself changes:

- **The plain title and the hook may not mention enrollment status.** Those two
  lines are what the feed card, the search result and the RSS entry show, and a
  "now recruiting" written today is still there long after it stopped being
  true. Status is rendered separately, read live from `trials_cache`.
- **Readiness is scored by phase alone** (phase 3 → 7, phase 2 → 6, phase 1 →
  5), never above 7: nothing being tested in a trial is approved care, and
  nothing running in people belongs in the animal/lab end of the scale. The
  prompt is given the phase rather than left to infer it.
- **A closed trial leaves the feed, search snippets and RSS**, but keeps its
  permalink, which states plainly that it is not taking new patients. Someone
  looking that trial up still deserves an answer; they just should not be
  invited to a door that no longer opens.

### Publish mode (WI-212)

**The site runs in Auto mode by default** — the human review gate is optional,
not mandatory. How an uploaded item reaches readers:

- **Auto (default):** an item that has a plain-language summary AND was **not**
  flagged by any automated safety check publishes itself immediately. The
  automated checks are the ones above — every numeral traceable to the source,
  no banned hype phrases, reading level within target, required template
  fields present. Anything a check flags (`summary_flagged`), or that isn't
  summarized yet, is held in the review queue for a person. So the queue still
  exists; in Auto mode it holds only the items the machine wasn't sure about.
- **Review:** nothing publishes without a person approving it in the admin
  queue (the original M2 behavior). Set `Publishing:Mode=Review`.

Auto-published items are recorded in `review_events` with actor `auto`, and the
item page **says so** — it does not claim a person reviewed it. This keeps the
audience's trust honest: an auto-published summary tells the reader it was
machine-published and passed automatic checks, and points at the original.

Because auto-publish requires a summary that passed the checks, it is
**safe-by-construction before M3**: with no summarizer yet, nothing has a
summary, so nothing auto-publishes even though the mode is on.

### Classification rules (before summarization)

- Closed taxonomy: the classifier may only emit tumor slugs from `taxonomy.yml`.
- `relevance` tiers: **patient_relevant** (human studies, guidelines, approvals, major trials, credible news) → front page; **early_stage** (animal/cell work) → behind the "show early-stage research" toggle, summarized with extra-strength stage framing; **excluded** (out of scope, duplicates, junk) → never rendered.
- Preprints are never `patient_relevant` regardless of content — early_stage at best, always badged.

## 10. Quality control

The review gate is the primary control: **every item is approved, edited, or rejected by a human in the admin queue before it publishes.** The supporting layers:

- **Golden set:** ~30 hand-verified example items (abstract → ideal classification + summary) checked into the repo. Every prompt or model change re-runs the golden set; regressions block the prompt change.
- **Review discipline:** the queue is a ~5-minute daily habit; when reviewing, read the summary *against the abstract* for at least a sample — approval must mean something. If review lags, the feed pauses rather than publishes unread (acceptable; staleness beats misinformation).
- **Reader flagging:** one-tap "report a problem" on every item → `summary_flagged` queue in admin.
- **Digest:** assembled from already-approved items, then the issue itself gets one more human read before send.
- **Correction log:** corrected summaries note "Updated [date] — an earlier version misstated X" (visible, like a newspaper). Builds trust; costs nothing.

## 11. Known failure modes to design against

| Failure | Defense |
|---|---|
| Hallucinated numbers/claims | source-only prompt + numeral post-check + golden set |
| Hype ("cure" framing) | mandatory "means/doesn't mean" block + banned phrases + stage badge |
| False hope from animal studies | hidden by default, hard badge, extra framing when shown |
| Wrong tumor tag → wrong audience | closed taxonomy + golden set + reader flags |
| Preprint presented as fact | source_kind rule: never patient_relevant, permanent badge |
| Stale/retracted papers | link to original always primary; monthly job checks PubMed retraction notices for summarized PMIDs |
| Model/prompt drift | versioned prompts, model id logged per item, golden set in CI |
