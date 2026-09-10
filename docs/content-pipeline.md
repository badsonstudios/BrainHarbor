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
5. READABILITY Automated gate (§5). Fails the build if grade level > 6.0.
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

### 3a. Shared blocks (WI-501)

Some passages belong on many pages and must say the same thing on all of them
— the WHO CNS5 retired-name crosswalk above all, because WHO and cIMPACT-NOW
move and a copy-pasted crosswalk means 24 places to correct. Those live once,
under `src/BrainHarbor.Web/Content/blocks/`, one Markdown file per block.

A page includes one with a line whose **entire content** is the block name in
brackets:

```markdown
## Old names you may still see

[CROSSWALK]
```

Rules worth knowing before writing one:

- **A block is a fragment, not a page.** No title, no slug, no disclaimers, no
  URL; it is never served on its own.
- **Front matter is optional and may carry only `sources`.** Those merge into
  the front matter of every including page, so a block's citations live in one
  place too. A source with no URL (a print edition) is carried through, not
  dropped.
- **The composed page is what gets graded.** ContentCheck measures reading
  level after includes are resolved. A fragment can sit under 6.0 alone while
  the assembled page goes over it, and the reader only ever meets the assembled
  page.
- **Uppercase, on its own line.** `[crosswalk]` is not a directive and renders
  as literal text; ContentCheck fails the build when a lowercase whole-line
  token names a real block, because that is a typo rather than prose. Inline
  `[text](url)` links and bracketed asides are untouched, and directives inside
  fenced code blocks are left alone so this section can show the syntax.
- **A missing or unreadable block fails the build.** It never renders as an
  empty section: on a medical page, silence reads as "there is nothing to say
  here". At runtime only the pages that include the bad block fail — a typo in
  one block must not take down `/privacy`.
- **A block no page includes is warned about**, because nothing grades it.
- Blocks may include blocks, up to five deep.

## 4. Plain-language style guide (both pipelines)

- Sentences under ~20 words. One idea per paragraph. Question-style headers.
- Second person, active voice. Common word first, medical term in parentheses: *"swelling (your doctor may call this edema)"* — patients WILL hear the jargon from their care team, so teach it, don't hide it.
- Numbers concrete ("about 1 in 3 people"); dollar figures carry "as of [year]".
- Never: dosing instructions, individual prognosis odds, "you should start/stop".
- Curated pages end with **"What to do next"** — retention is poor; the next step must survive.

## 5. Automated gates (CI, curated pages)

| Gate | Tool | Threshold |
|---|---|---|
| Reading level | Flesch-Kincaid script | ≤ 6.0 grade, warn ≥ 5.5 — curated pages AND reader-facing Razor pages (WI-414) |
| Glossary coverage | medical terms used but not in glossary → warn | — |
| Link rot | outbound checker (monthly job) | 0 broken |
| A11y smoke | Playwright + axe-core | 0 serious/critical |
| Review freshness | `review_due` past → report | — |

**Reading level for AI summaries (WI-415, 2026-08-13).** The prompt is the
mechanism; the guardrail is only the backstop.

- `summarize-v4` / `summarize-trial-v2` **ask for 6th grade** explicitly, with
  concrete rules (sentences under ~15 words, the short everyday word over the
  long one). Measured live over golden-set items through the real CLI:
  `summarize-v4` (8 research items) median **4.7**, max **6.5**;
  `summarize-trial-v2` (3 trial items) median **3.4**, max **3.6**. (The previous prompt measured a median of 6.0 block-aware across the
  1,038 published items — a different population, so read it as a direction,
  not a like-for-like delta.)
- **First real pipeline run on the new prompts** (2026-08-13, 51 items):
  `summarize-v4` flagged **2 of 42 (4.8%)**, `summarize-trial-v2` **0 of 9** —
  against 9.8% for `summarize-v3` at the OLD, looser 8.5 gate. A stricter
  ceiling that rejects less, because the writing got simpler rather than the
  bar getting lower. Note the DB stores only a flagged boolean, not the reason,
  so this rate covers all guardrails (numerals, hype, reading level) — see
  WI-417 for run logs that would separate them.
- `Guardrails.MaxGradeLevel` is **7.0**, not 6.0. A flagged summary does not
  publish, so setting the gate *at* the target would flag ordinary variation
  around it and drain the feed into the review queue instead of making it
  easier to read. 7.0 catches genuine outliers.
- `Guardrails.GradeLevel` is **block-aware**: the plain title and the template
  blocks arrive newline-separated and a title has no full stop, so grading the
  raw run merged title into hook and inflated every score by ~0.7 of a grade
  (measured across the 1,038 published summaries: 6.7 reported vs 6.0 real).
- Re-measure against a real pipeline run before tightening further:
  `dotnet test --filter "Category=Live"` re-runs the golden set through the
  real CLI and prints the distribution. **Already-published summaries were
  written by older prompts and are not retro-fixed** — they stay as they are
  until re-summarized.

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

---

## 12. The tumor-guide editorial standard (Phase P5)

Phase P5 writes 53 curated pages: 24 tumor hubs, a 17-page treatment library,
and a 12-page tests library. This section is the standard they are written
against. It exists so the rules live somewhere a future session will find them
instead of in a chat log, and so 53 items do not each re-litigate the same
decisions.

Everything in §§1–8 still applies. This section adds what is specific to the
tumor guides. The research it rests on is committed at
`docs/research/tumor-guides/`; read `SYNTHESIS.md` before drafting any P5 page.

**Why the phase exists, in one measurement:** across 91 US brain tumor centres
and 8 patient organizations, mean Flesch-Kincaid grade level is **11**. Under
10% of centre sites reach 8th grade, and **no patient organization does**. A
library that actually holds 6.0 would be, on the published record, the first.

### 12.1 Source precedence

**This is the rule most likely to be broken by accident, because the offending
source is the one we would naturally lean on hardest.**

| Question | Source that governs | Never use for this |
|---|---|---|
| Tumor **naming** and **grading** | WHO CNS5 / cIMPACT-NOW; NCCN Guidelines for Patients | NCI patient PDQ, ABTA legacy PDFs, StatPearls oligodendroglioma chapter |
| **Brain-metastasis radiation** | ASCO-SNO-ASTRO 2022 | NCI patient PDQ |
| Supportive care, general **framing**, tone | NCI patient PDQ is fine here | |
| **Prognosis figures** | nothing. See §12.5 | NBTS meningioma page (publishes survival percentages) |

**A page that cites NCI for a tumor name is a defect.** NCI's patient PDQ is
pre-CNS5: it still treats "anaplastic astrocytoma" as a live diagnosis,
describes "mixed gliomas ... called oligoastrocytomas", still says
"hemangiopericytoma", uses Roman numeral grades, and predates both
ASCO-SNO-ASTRO 2022 and vorasidenib. Two research tracks flagged this
independently.

Same class of problem elsewhere, so the check is per-claim and not per-domain:
ABTA's downloadable PDFs still use "oligoastrocytoma" and "anaplastic
astrocytoma"; StatPearls' oligodendroglioma chapter gives a correct molecular
definition and then lapses into Roman numerals and a retired term in the same
article.

**NCCN Guidelines for Patients: Brain Cancer — Glioma (2024) is the best
patient-level source in the set and it IS CNS5-aligned** — verified against the
document, not assumed: Arabic grades throughout, `IDH-mutant`, and neither
retired name appears anywhere in it. It is also the best available model for
the "questions to ask your team" block.

**Licensing, and it is not what the research reports say.** They describe NCCN
as "licensing-clean". Its copyright page does not: *"NCCN Guidelines for
Patients and illustrations herein may not be reproduced in any form for any
purpose without the express written permission of NCCN."* So:

- **Read it for facts. Write every sentence ourselves. Cite it with a URL.**
- **Never** copy a sentence, a list, or an illustration from it.
- The risk is specific: a drafting session with a 76-page plain-language PDF
  open, writing plain-language pages, is exactly where borrowed phrasing
  happens without anyone deciding to.
- Same rule already applies to NCI embedded images and AHFS/MedlinePlus drug
  monographs (PLAN.md §5).

### 12.2 The shared acceptance contract

Every P5 content item must satisfy all of these. Item-level acceptance lists
only what is specific to that page.

1. Reading grade **≤ 6.0**, measured by ContentCheck on the **composed** page
   (§3a), CI-gated.
2. **Sources-only.** Every substantive claim traceable to a source in the
   research reports or one added and cited in the page's `sources` front
   matter. No invented facts, no invented numbers.
3. **WHO CNS5 naming and grading throughout.** Arabic numerals; grading happens
   *within* a tumor type. Where an older name was retired, say so: readers
   arrive holding old paperwork.
4. **Source precedence per §12.1.**
5. **No prognosis figures.** No survival statistics, no median survival, no
   five-year rates, anywhere, on any page.
6. **Never AHFS or MedlinePlus drug monographs; never NCI embedded images.**
   Also: replace the manufacturer's site (avastin.com) as a side-effect source
   before publishing.
7. Ends with **questions to ask your care team**.
8. Front matter carries `sources` (with `accessed` dates), `reviewed`,
   `review_due`, and `disclaimers: [medical]`.
9. New vocabulary joins the glossary so tooltips fire site-wide.
10. Existing `/tumors/*` URLs are preserved. No redirects, no renames.
11. **Every tumor hub carries a caregiver section** — a real section, not a
    footnote. Dan's call, 2026-08-30, with the reason: after surgery there is a
    lot of aftercare, and someone living with a person who has a tumor needs to
    know what they will have to deal with. Every comparable site silos
    caregivers into a separate support area; none gives them a lane inside the
    tumor page. **See §12.7 for how one is built.**
12. **Every treatment page with meaningful aftercare carries a caregiver
    section too** — what the person at home actually has to do, what to watch
    for, and when to call.

### 12.3 The standard section order for a tumor hub

Seventeen sections, headings phrased as questions. The design principle:
**answer first, epidemiology never, prognosis by consent, action at the end of
every frightening block.**

| # | Section | Why here |
|---|---|---|
| 0 | The short version (3 to 5 sentences) | Readers consume only 20 to 28% of a page and scan in an F-pattern. PEMAT requires a purpose-evident opening plus a summary. |
| 1 | What is a [tumor]? | The most-asked identity question. |
| 2 | Is it cancer? What does its grade mean? | The documented core confusion. Answer in cell-behaviour terms, never survival terms. |
| 3 | Where does it grow, and why does it cause these symptoms? | Location to symptom mapping answers "why is this happening to me"; listing symptoms alone does not. |
| 4 | What symptoms does it cause? + When should I call for help right now? | Symptom queries outnumber treatment queries 2 to 5 times. |
| 5 | How do doctors find out it is this? (links to the tests library) | "Tests and next steps" beats "Diagnosis" as a framing. |
| 6 | What do the words on my report mean? | The clearest content gap across every comparator. |
| 7 | How is it usually treated? (links to the treatment library) | Surgery, radiation and chemo are ~78% of forum treatment discussion. |
| 8 | What is treatment actually like, and what is normal afterwards? | The loudest gap in the qualitative literature: "the real fight started after I woke up". |
| 9 | Everyday life: driving, work, money, seizures, tiredness, memory | Financial and logistical strain dominated real forum discussion. **Seizures, activities and driving belong to WI-560 — link, do not restate.** Driving rules are jurisdictional, so a per-tumor page must never carry a duration. |
| 10 | Follow-up scans, and what to do while you wait | Scanxiety is common, severe (mean 6/10), and peaks in the wait. |
| 11 | If it comes back | A distinct, named question set. |
| 12 | Outlook — behind a reader-choice gate, no numbers | See §12.5. By here the reader has everything actionable before meeting anything frightening. |
| 13 | For the person caring for someone with this | Contract item 11. |
| 14 | Questions to ask your team (printable) | PEMAT actionability. Every major org has good lists and files them away from the point of need. |
| 15 | Where to get support | Peer connection was the most emphasised finding of the forum study. |
| 16 | Where this came from / last reviewed | No site in the national evaluation met all four JAMA benchmark criteria. Provenance is cheap differentiation. |

**Deliberate omissions.** "How common is it?" is cut, or demoted to one clause
inside section 1 — filler competing for the 20 to 28% that gets read. "What
causes it?" is demoted or cut: cause queries are under 2% of cancer search
volume and the honest answer is usually "we don't know", which is a bad thing
to put in a frightened reader's first three screens. The self-blame block still
appears; it just is not near the top. "What does it look like on an MRI?" is
cut from the patient page — it serves clinicians.

### 12.4 Numbers: the three rulings

Stated once so they are not re-argued per page.

- **R1 — orienting durations are IN.** "Radiation is usually Monday to Friday
  for about six weeks." "Optune at least 18 hours a day" (that one IS the
  decision). "Staples out roughly 1 to 2 weeks." All Gy and mg figures stay
  out. Where an interval only makes sense with its reason, give the reason
  instead: lomustine is *"taken only occasionally, not every day, because it
  lowers blood counts for weeks after you take it."*
- **R2 — procedural risk percentages are qualitative.** "Bleeding is uncommon,
  but it is the main risk." The source spread is too wide to state a number
  honestly: awake-craniotomy seizure risk is reported anywhere from 2.9% to
  54%.
- **R3 — the two numbers most likely to mislead get direction only.** No
  percentages for the whole-brain-radiation cognitive figures, because most
  people in **both** arms declined and the raw numbers mislead in the reader's
  favour. None for the ~90% glioblastoma relapse rate: *"recurrence is expected
  and is planned for"* carries the useful part.

### 12.5 Prognosis without figures, and the reader-choice gate

**Explain the concepts, publish no figures.** This is a deliberate change from
what `/tumors` currently says, and it is validated rather than squeamish:
prognosis disclosure requires negotiation, readiness-checking and staged
disclosure across visits, three things a web page structurally cannot do.
Interviews with 25 newly diagnosed glioma patients produced *"not all patients
want to know it all, one size does not fit all"* — some wanted full honesty,
some generalities, some only positive information.

That mandates a **mechanism**, not just a policy: outlook sits at position 12
behind an explicit choice (WI-503), closed by default, working with JavaScript
off.

> "The next part is about outlook. Some people want to read it. Some people
> would rather not. You can skip it and come back another day. Nothing else on
> this page depends on it."

**How to write it (WI-503, shipped).** The gate is a Markdig custom container.
The heading stays OUTSIDE it, so the page outline is complete and the reader
meets heading → warning → choice in that order (§12.6, "warn before you
disclose"):

```markdown
## What might happen over time

:::outlook
Doctors use numbers that describe a large group of people…
:::
```

Everything inside renders inside a `<details>` that is **closed on load**. The
warning sentence above and the Show/Hide label are emitted by the component,
not typed per page — 24 tumor hubs cannot each soften them. Nothing else needs
writing.

Three things that follow from the implementation and are worth knowing before
you author one:

- **`:::outlook` is the only container name the site renders**, and the fence
  has to be written exactly: **three colons** (not two), the opener flush with
  the surrounding text (four spaces or a tab makes it a code block), and a
  closing `:::` on a line of its own. Anything else — `:::outlok`,
  `:::Outlook`, `::outlook`, an indented or unclosed fence, `::outlook::`
  inline — **fails the page build**, naming the page and the line.
  That strictness is the point: every one of those renders the outlook section
  as ordinary visible prose (Markdig turns an unknown container into a plain
  `<div>`, and drops the near-misses entirely), with no error and a green
  build. **A gate that fails open is worse than no gate**, so the build
  refuses rather than guesses.
- **The 6.0 reading limit reaches inside the gate.** Content behind a choice is
  still content, and ContentCheck grades the composed page.
- **Printing reflects the reader's choice.** A gate left closed stays closed on
  paper; forcing it open would hand the outlook section to someone who
  declined it.

When explaining *median*, use positive framing plus the explicit right tail
(the Kirkebøen framework, which raised hopefulness and realism at the same
time): establish that it describes a **group**, present the distribution rather
than the midpoint alone, name the right skew, and say plainly that the line is
a picture of a group and not a prediction about one person.

### 12.6 Writing rules specific to these pages

- **Say the outcome, then name the word.** Not "a craniotomy is..." but what
  happens, then the term.
- **Answer in the first sentence under the heading.** Most readers never reach
  the second.
- **Never end a section on a frightening sentence.** Follow it with a concrete
  action or something solid. This is a review rule, not a preference.
- **Warn before you disclose.** Signal that a section contains hard information
  before the reader is inside it.
- **Replace nominalisations with verbs:** "resection" becomes "taking it out".
- **Use concrete, sensory description for procedures:** what you will see,
  feel, hear and smell.
- **Keep the qualifier, shorten it.** Accuracy is usually lost when a hedge
  gets cut, not when a sentence gets simplified.
- **"Is this the tumor, or the drug?"** is a recurring frame worth repeating:
  levetiracetam causes irritability and aggression; dexamethasone causes
  proximal muscle weakness in ~28%; SMA syndrome takes speech and one-sided
  movement away right after surgery and gives them back over days to weeks;
  cognition feels worse on day 2 to 3 after surgery and then improves.
  Unwarned, people read every one of these as the tumor winning.

### 12.7 The caregiver section (WI-558)

Contract items 11 and 12 say every tumor hub, and every treatment page with
meaningful aftercare, carries a caregiver section. This is how one is built, so
53 pages do not each invent it.

**The shared half is a block.** `[CAREGIVER]` (`Content/blocks/caregiver.md`,
WI-501 mechanism) holds what is true whatever the tumor is: you are allowed to
ask questions; ask who your first call is; get the two phone numbers and know
which is "call today" and which is "call an ambulance"; ask to be shown
anything you are sent home to do; say what you notice; look after yourself, and
let people help. It ends by pointing at `/get-help-now`.

**Every tumor hub includes it under the same heading**, section 13 of §12.3:

```markdown
## For the person caring for someone with this

[CAREGIVER]
```

**The per-page half is what the page adds around it.** Page-specific caregiver
material goes *after* the directive, in the same section, and covers only what
is true for this tumor or this treatment: the aftercare that actually falls to
someone else, what "normal" looks like week by week, the changes to expect and
which of them are the drug rather than the tumor, and what to watch for. WI-510
(craniotomy) carries the fullest one on the site; a hub whose reader lives
alongside seizures for years links to `/seizures/what-to-do` and
`/seizures/living-with` rather than restating them.

**Write to the caregiver, in the second person.** A section that says
"caregivers often find..." has already failed the person reading it at 2am. The
one place the block breaks that rule is where it reports a study finding, and
it does so to give the reader permission ("in one study, family carers said
they were afraid of annoying the doctor"), not to describe them from outside.

**Why it is inside the tumor page and not a support silo.** Care coordination
and advocacy are raised almost exclusively by caregivers; caregivers perform
dressing changes and give medicines with no formal instruction; several feared
offending the physician by asking too many questions. No comparator site puts
caregiver content inside the tumor page. Sources are cited in the block's own
front matter and merge into every including page (§3a), so the citations live
in one file too.

### 12.8 The library-page template (WI-506)

§12.3 gives the 17-section order for a **tumor hub**. This is the order for a
**library page** — the 12 tests pages and the 17 treatment pages. It is
deliberately shorter and flatter: a hub answers "what is wrong with me", a
library page answers "what is about to happen to me".

Twelve slots. Most headings are questions, because most of these ARE the
reader's question; sections 3, 5 and 7 are the exceptions, and forcing those
into question form produces worse headings than it prevents.

Sections 6a to 6c are the variable middle. **The order is fixed; which of the
middle three a given page carries is not.** Sections 0 to 5, 9 and 10 appear on
every page; 6a to 8 are carried when the page has the material, and 11 whenever
there is somewhere real to send the reader.

*(Corrected at WI-507: this line first said "0 to 5 and 8 to 10", which
contradicted slot 8's own description — "any page where a reader gets sent back
for a repeat" is a condition, not a universal. WI-507 is the first page with no
repeat to explain.)*

*(Narrowed again at WI-508: **slots 2 and 5 are not universal either.** Both
assume a page with a day and a procedure — "why am I having this?" and "what
does it feel like?" have no referent on a page whose subject is a **document**.
WI-508 drops both. The genuinely universal slots are **0, 1, 3, 4, 9, 10**.)*

| # | Section | Notes |
|---|---|---|
| 0 | The short version | 3 to 5 sentences. Same reason as §12.3: readers consume 20 to 28% of a page. |
| 1 | What is it? | One sentence first, then the detail. Say the outcome, then name the word (§12.6). |
| 2 | Why am I having this? | The reader's real question. For a test that serves several purposes, list them and say the machine is the same each time. |
| 3 | What happens, step by step | A numbered list. Concrete and sensory: what you will see, feel, hear and smell. |
| 4 | How long does it take? | R1 durations belong here (§12.4). |
| 5 | What does it feel like? | Separate from step-by-step on purpose — a reader looking for this should not have to read the procedure to find it. |
| 6a | What is hard about it, and what can be done | The section the comparators skip. For MRI it is claustrophobia. **Never a list of problems with no answers.** |
| 6b | Side effects, soon / Side effects, later | Treatment pages. Usually two headings, and on a tests page there is normally no "later". |
| 6c | The thing people ask about | One heading per genuinely-asked worry that does not fit 6a: the dye on the MRI page, and there will be others. Skip if the page has none. |
| 7 | What you need first, or need to bring | Preparation, clearance, the device card, the things a reader can act on before the day. |
| 8 | Why am I having another one? | Any page where a reader gets sent back for a repeat. It is a frightening moment with a mundane answer, and it is the most-skipped content in the comparators. |
| 9 | Who reads it, and how do I get the result? | **Universal — every one of the 29 pages.** Do NOT head it "When will I know?" unless the page can answer that; timings are local, so the honest answer is "ask before you leave", and the heading must not promise more than the section delivers. |
| 10 | What to ask your team | Shared contract item 7. |
| 11 | Where to go next | Links only to pages that exist. A dead link on a page a frightened reader was sent to is worse than no link. |

**Sources and "last reviewed" are not a section.** They render from front matter
on every curated page, so a hand-written provenance section duplicates them and
then drifts.

**Where a tests page differs from a treatment page.** A tests page usually
carries 6a and 6c and skips 6b; a treatment page usually carries 6b and skips
6c. Nothing else about the order changes, and nothing moves.

**Caregiver sections.** Contract item 12 puts one on every treatment page with
meaningful aftercare, built per §12.7 and placed after section 7. Tests pages
mostly do not carry one — nobody is discharged home to look after someone after
an MRI. Where a test does have an aftercare tail, it gets one.

**Three rules that came out of writing the first one (WI-506):**

- **A library page must say what it cannot do.** MRI can suggest a tumor type;
  only tissue can name it. A reader who does not know that reads the wait for
  pathology as their team stalling.
- **Where a page could send a reader to ask for the wrong thing, it says "ask
  what your centre has" instead of naming the thing.** Open and upright
  scanners are lower field strength and are not equivalent for tumor imaging,
  so "ask for an open MRI" is advice that can cost a reader the picture their
  treatment is planned from. Pinned by a test.
- **Answer the frightening question in both directions.** Gadolinium does leave
  traces in the body and there are no known health effects from it. Dropping
  either half is a different kind of dishonesty.

**Four more from the second one (WI-507, the pathology wait):**

- **Slot 7 is a role, not a heading.** It is written as "what you need first, or
  need to bring", which assumes a page with a day and a procedure. Its actual
  job is *the things this reader can act on*. On a page about a wait, that is
  **"What you can do while you wait"** — same slot, same position, different
  heading. Expect the same on any page whose subject is not an appointment.
- **Slot 4 may be followed immediately by a "why" section.** WI-507 carries
  "Why some of it takes weeks" between 4 and 5. On a page whose whole subject is
  the duration, splitting how-long from why-so-long puts the reader's actual
  question two screens from its answer. Only do this where the duration IS the
  topic.
- **A number that comes from one country's audit is attributed in the sentence
  that prints it**, not in the source list. "In one national study of 21 centres
  in the United Kingdom…" — because R1 permits the number, and nothing else
  stops a reader in Ohio treating it as a promise about their own hospital.
  Pinned by a test that checks the attribution sits in the same *paragraph* as
  the figure.
- **Shared prose between two library pages becomes a block the second time you
  write it, not the fifth.** The tumor-board paragraph was written for WI-506
  and needed again by WI-507; it is now `[TUMOR-BOARD]`
  (`Content/blocks/tumor-board.md`, §3a), and the second copy had already lost
  the best line in the first ("what they decide is advice, not an order"). With
  29 library pages, "we will factor it out later" means 29 versions of it.

**Four more from the third one (WI-508, the report walkthrough):**

- **A page about a document needs a "where the answer sits" section before the
  walkthrough.** Slot 3 is "what happens, step by step"; on a document page it
  becomes "what is in it, part by part", and a reader cannot use that list until
  they know the answer is printed at the top and the evidence underneath.
  WI-508 carries it between slots 1 and 3. It is also where the page says the
  layout **varies** — a walkthrough read as a fixed running order sends a reader
  looking for a section that was never there, and lets them conclude their own
  report is incomplete.
- **Where the page's own subject is a word the reader will search, the heading
  anchors are a published interface.** `/tumors` and feed items are meant to
  deep-link at an exact term, and Markdig derives anchors from heading TEXT, so
  rewording a heading breaks every inbound link silently — the reader lands at
  the top of a long page with no sign anything went wrong. Keep those headings
  short, and pin their ids in a test.
- **The shared-prose rule applies to the tests too.** `EveryLinkOnThePage
  Resolves` had become three near-identical copies by the third library page and
  they had already drifted. It now lives on the shared `CuratedPage` helper.
  Same argument as `[TUMOR-BOARD]`, same threshold: the second use.
- **A rule that fails a correct page is worse than no rule.** WI-508 generalised
  "the body links somewhere outside 'Where to go next'" from the page that
  happened to do it, and it failed the MRI page, whose every link is
  legitimately in that one section. Before a per-page property is promoted to a
  site-wide one, run it against the pages already shipped.

**Six more from the fourth one (WI-509, the marker reference list):**

- **A reference list bends slot 3 into itself, and slot 4 collapses to a link.**
  Slot 3 is "what happens, step by step"; on a page whose subject is a list of
  words it becomes the list. Slot 4 ("how long does it take?") belongs to
  whichever page owns the wait — but the heading still has to answer itself, so
  it gives the shape ("weeks rather than days") and then routes. A heading that
  asks a question and answers only "see that other page" is the WI-506 trap in
  a new coat. Universal slots are still **0, 1, 3, 4, 9, 10**.
- **Write entry anchors explicitly (`### MGMT … {#mgmt}`), never derived.**
  WI-508 established that heading anchors are a published interface; this is the
  mechanism. Markdig derives an id from heading TEXT, so a reworded heading
  silently breaks every inbound link. `UseAdvancedExtensions` brings generic
  attributes, so `{#id}` makes the wording and the interface independent, and a
  test asserts every `###` entry carries one.
- **Where a page defines a word the glossary also defines, suppress the tooltip
  on that page.** `!%term%` (WI-105) turns one term off for one page. A popover
  repeating the paragraph directly beneath it is noise, and fifteen of them is
  the carpet WI-505 measured its way out of. The term still fires everywhere
  else, which is the point of adding it. Assert both directions: absent here,
  present on the words the page does *not* define.
- **Assert prose rules against reader text, not raw source.** Authoring markers
  are not prose. WI-509's own no-percentages test failed on `!%H3 G34%!%BRAF%`,
  which puts the characters "34%" into the file and nothing on the page. Use
  `CuratedPage.ReaderText`, which strips the WI-105 markers the way the renderer
  does.
- **Before adding a phrase to the shared characterisation ban list, run it over
  the whole corpus.** WI-509 proposed "good sign" and "bad sign" and found the
  wait page already saying *"That is normal and it is **not** a bad sign"* —
  correct, and the natural way to write it. Both were dropped. The list is a
  substring check, so a phrase belongs on it only if no correct sentence
  contains it.
- **A uniqueness claim on a page of many entries is a claim about all the
  others.** WI-509's MGMT entry said it was "the one result on this page" used
  for choosing treatment, while the BRAF entry two screens up says there are
  drugs made to act on BRAF changes. That went stale inside a single draft, and
  no gate can see it — only reading the page end to end.

**Collapsible entries were considered and rejected, so WI-510 onward do not have
to re-argue it.** The item asked for each entry behind a disclosure. Three
reasons not to: a `<details>` closed on load defeats deep-linking (fragment
auto-expansion is not universally supported, and the reader lands on a heading
with nothing under it); it prints empty, on the one page people hold beside the
document; and it needs a second `:::` container with an argument, which is a new
fail-open surface — WI-503 documents five ways a mistyped fence publishes
content wide open — guarding words that are descriptions, not prognosis. The
scannability comes from a jump list of anchors at the top instead. Reserve the
reader-choice gate for outlook.

**Cloudflare-gated sources: look up the DOI in Europe PMC.** WI-507 recorded
this; WI-509 confirms it and names the paper, because the dossier sources nearly
every marker claim to one blocked `academic.oup.com` URL. It is Sahm et al,
*Neuro-Oncology* 25(10):1731–1749, the EANO guideline on molecular diagnostic
tools for WHO CNS5, open at **PMC10547522** — the backbone for what every marker
measures and by which method. A citation nobody can open is a citation nobody
can verify.

**Six more from the fifth one (WI-510, craniotomy — and the first TREATMENT
page):**

- **The universal-slot list is a floor, not a ceiling, and three bent pages in a
  row is how a template quietly shrinks.** WI-507, WI-508 and WI-509 each
  dropped slots 2, 5, 7 and 8, correctly, because a wait, a document and a
  reference list have no day and no procedure. WI-510 has all four back and is
  the first page since WI-506 to use all twelve. **Read this section for the
  slot list, not the previous page.** Copying the last page written is how the
  dropped slots would have stayed dropped for the remaining 24. A test on the
  page asserts the four returning slots by name, for exactly this reason.
- **A treatment page carries 6b AND may carry 6c.** §12.8 said a treatment page
  "usually skips 6c". WI-510 carries one — "is there a way to do this without
  opening the skull?" is a genuinely-asked worry, and the alternative (LITT) is
  a section rather than a page by the research's own recommendation. Where a
  treatment has a less-invasive alternative the reader has heard of, 6c is where
  it goes, **with its limits attached in the same breath**: a description of a
  gentler option with no limits is a page that sends readers to ask for the
  wrong operation.
- **The characterisation ban list needs a treatment-page vocabulary, and most
  candidates fail.** A tests page characterises a *result*; a treatment page
  characterises an *outcome*. Ten candidates were run over the corpus (§12.8's
  own rule) and **six were rejected**, all because a correct sentence contains
  them — usually a negation, the WI-509 failure mode. `"good result"` failed on
  the spot against WI-510's own slot 2. The rejected six are recorded in
  `CuratedPage.RejectedCharacterisations` **with the reason**, so the next page
  does not re-do the work and re-reach the wrong answer.
- **Promote a rule at the second page, not the first.** WI-510 wanted a
  "never minimise the operation" rule ("routine operation", "simple operation").
  It is corpus-clean, but it only has an obvious meaning on a page about a
  procedure, so it is a page-local test. WI-511 or WI-512 promotes it to
  `CuratedPage`. This is the other half of "factor at the second use": do not
  factor at the *first* either.
- **US spelling and US clinical words, checked explicitly.** The first draft
  carried `anaesthetist`, `anaesthetic`, `jewellery`, `theatre`,
  `physiotherapist`, `tablets` and "you will be got up". The corpus has **zero**
  British forms (`center` 12, `recognize` 4, `jewelry` 1), no gate looks for
  them, and a reader in Ohio meets a page that sounds like it is about a
  different health system. Grep the page against the corpus before shipping.
- **A page-specific lead-in to a shared block can contradict the block.**
  WI-510's first draft introduced `[TUMOR-BOARD]` with "your case is very likely
  to be discussed" while the block itself says a tumor board "tends to happen
  when a case is complicated". The two other pages that carry it both use one
  hedged line ("Your case may also go to a tumor board"). **Read the block
  before writing the sentence above it**, and match the existing lead-ins:
  the sentence introducing a block is shared prose too, even though it lives on
  the page.

**Five more from the independent review of WI-510, which is the reason to keep
running one:**

- **Deleting a bad citation does not delete the claim it was carrying.** WI-510
  correctly found that the "neurological checks through the night" detail was
  attributed to a source that never mentions it, removed the citation, and left
  the four-sentence sensory paragraph on the page. That is a *worse* state than
  before: an uncited invented claim rather than a miscited one. **When a
  citation falls, re-derive the sentence from what is left, or cut it.**
- **A page's emergency list must not contradict the page it links to.** WI-510's
  ambulance list read "A seizure", six lines above a link to
  `/seizures/what-to-do`, which correctly says most seizures do not need an
  ambulance. Whenever a page carries a "call an ambulance" list, diff it against
  every other such list on the site.
- **The overlap check for a shared block has to be a shingle check, not a
  heading check.** WI-510 asserted the block's four bold lead-ins were absent
  and shipped four genuine duplications that were none of them, including a
  verbatim sentence and a duplicated link. Word shingles over the composed page,
  excluding "Where to go next" (an index by design), catch a restatement in any
  shape.
- **A "both directions" assertion is only worth writing if both directions are
  observable.** WI-510's tooltip test claimed to prove a suppressed term still
  fires elsewhere, by fetching `/glossary` — which renders from the glossary
  directory and cannot see any page's suppression state. No other page uses any
  of the six words in prose, so the intended check was not available at all. It
  now asserts the leak that *is* possible: a `!%term%` marker inside a glossary
  entry or shared block, which would suppress that term everywhere at once.
- **Ending a section on the reassurance is a positional property, so pin the
  LAST sentence.** A three-sentence window only proves the reassurance is
  nearby. WI-510's resection section passed such a window while genuinely
  closing on "would have cost you something you would not want to lose".

**And the thing no gate caught, on this page or any of the four before it.**
Reading WI-510 end to end found eleven defects that every automated check passed
clean: a guessed pronoun for a real named patient in a quoted source, "we do not
publish numbers" in a site voice used nowhere else, a bruising sentence that
parsed and meant nothing ("which can look alarming and is not"), and a "most"
that contradicted a "many ... some do not" two sections later. **A claim about
how many people recover cannot have two different strengths on one page.** That
is the fifth time a human-style read has caught what the suite cannot.

**Seven more from the sixth one (WI-511, radiation therapy — the second
TREATMENT page):**

- **One URL can carry two claims, be wrong about one and right about the
  other.** WI-510's rule says deleting a bad citation does not delete the
  claim. WI-511 found the harder version: the dossier attributes *"somnolence
  usually resolves on its own"* to the Brain Tumour Charity's **jargon-buster**
  page, which is one sentence long and says no such thing — so the first draft
  dropped the URL. But that same page is the **only** source anywhere in the
  set for the **four-to-six-week timing**, which the draft kept. Dropping the
  citation orphaned a number nobody had noticed it was also carrying. **Before
  you drop a source, list every claim resting on it, not just the one that
  failed.** Both pages are cited now, each for what it actually says.
- **Where two sources disagree on a frequency, print the disagreement.** The
  charity calls somnolence syndrome rare; the study the page cites for its
  central finding saw it in most of a small group. The draft split the
  difference with an unattributed "uncommon" — a third answer belonging to
  nobody. §12.8's WI-507 rule (attribute in the sentence that prints the
  figure) extends to this: name whose number it is, and if they conflict, say
  so and give the reader the part both agree on.
- **A number written as a word is still a number.** WI-511's whole-brain
  section said "most people in both groups lost some thinking skills",
  importing R3's reasoning about the SRS comparison and asserting it of the
  CC001 trial, where the per-test rates run 23.3% v 40.4%. "Most" was false of
  the arm the page recommends. The page's own no-percentages test could not see
  it, because it only matched digits — and the corpus writes every number in
  words. **Grade the claim, not the character class.**
- **An escalation list under-triages as easily as it over-triages, and the
  under-triage is the more dangerous direction.** WI-510's blocker was an
  ambulance list that said "a seizure" where most seizures need no ambulance.
  WI-511's was a *call-the-team-today* list that said "there is a seizure" flat,
  six lines from a page saying a **first** seizure is a 911 call. Diff every
  escalation list against `/seizures/what-to-do`, in both directions, whatever
  the list is headed. A test that asserts only "no heading says ambulance"
  proves there is no second list; it says nothing about whether the one list is
  right.
- **Flatten before matching, in the gates as well as the tests.** The corpus is
  hard-wrapped, so a two-word phrase routinely has a newline inside it. WI-511's
  new British-usage gate read raw body text and walked straight past `"a lift"`
  in `blocks/caregiver.md`, where the wrap falls between the words — a British
  idiom on eighteen tumor hubs, missed by the gate written to catch it. Same
  trap as WI-509's fix test, one item later.
- **A negation-aware ban list has to anchor to the CLAUSE.** A bare
  N-character lookback for `not|never` fails both ways: *"it is not painful,
  and it is a simple procedure"* passes (the negation belongs to the other
  clause), and *"there is no such thing as a simple procedure"* fails. The
  working form is `\b(not|never|hardly|no|n't|far from)\b[^.,;:]{0,20}$` — close
  AND on this side of the nearest punctuation.
- **A substring ban list is a stemming problem, and stemming bugs read as
  correct rules.** WI-511's British-spelling list shipped five entries that are
  substrings of correct US words — `specialis` matches **specialist**,
  `characteris` matches **characteristic**, `organis` matches **organism**,
  `realis` matches **realistic**, `analyse` matches **analyses**. It also
  carried `radiotherapy`, which is not a British spelling at all but standard US
  vocabulary inside named techniques (Stereotactic Body Radiotherapy). Every one
  of those would have failed a correct page. **Run a candidate list against real
  English, not just against the corpus** — corpus-clean today says nothing about
  the page nobody has written yet.

**And run the ban lists you already have over the whole corpus, not just the
page in hand.** §12.8 asks for a corpus scan before *adding* a phrase. Nobody
had ever asked it of the phrases already on `CuratedPage.Characterisations`.
WI-511 ran it and found `"bad news"` sitting over a correct sentence on
`/seizures/what-to-do` — *"a seizure is **not** automatically bad news about the
tumor"* — eight items after that page shipped. Only two pages assert the list
and neither uses the phrase, so nothing ever went red. The list now defends
itself site-wide (`CuratedProseHousekeepingTests`) instead of waiting for a page
that happens to check it. `"bad news"` was demoted; **`"good news"` was kept** —
review pushed back on dropping the pair, correctly, because retiring a working
guard for symmetry with a broken one is a net loss.

**Sixth consecutive item where reading the page end to end found what no gate
could,** and the independent review found four blockers on top of that. This
time the human-style read caught: a guessed gender for a real named patient
(*"One **man** treated for an astrocytoma… to drive **him**"*, where the source
names Tommy M. and states no pronouns — **the identical defect WI-510 shipped
and recorded**), three unsourced comparative frequency claims, a caregiver
section restating the skin rules, the hair advice and the pill-box line already
on the page or on `/treatments/craniotomy`, and an invented reassurance closing
the mask section (*"almost everybody gets through the course"* — no source says
it, on the section written for the most frightened reader on the page). **The
recorded lesson did not prevent the repeat.** Read the page.

**Six more from the seventh one (WI-512, chemotherapy — and the end of Wave 1):**

- **A dossier claim bundled with a true one inherits its citation.** The
  research doc puts wafer consequences in a single bullet: *"wafers can
  complicate later clinical-trial eligibility AND make imaging harder to
  interpret"*, cited to PMC9259966. The trial half is in that paper verbatim.
  The imaging half is not — the paper contains "imaging" once, in a definition
  of progression-free survival, and "MRI" not at all. Both shipped as one
  sentence with one citation, and a test then pinned the invented half under a
  name calling it a consequence a reader can act on. **Check a citation against
  each CLAUSE, not each bullet.**
- **An escalation tier is a site-wide property, not a page-wide one.** §12.8
  already said to diff a page's ambulance list against every other such list.
  That is not enough. WI-512 filed chest pain and breathlessness under
  "call your team straight away" while `/treatments/craniotomy` files chest pain
  under **"call an ambulance — these cannot wait"**, and filed confusion under
  "straight away" while `/treatments/radiation-therapy` files near-identical
  wording under "the same day". Nobody had contradicted a *list*; three pages
  had sorted the same symptom into three different **tiers**. The reader who
  meets all three pages at once — post-craniotomy, on chemoradiation — is the
  ordinary reader. **Diff the tiers, per symptom, across the corpus.**
- **A source can be cited for the position it argues against.** WI-512's
  antibiotic section cited a 2026 systematic review and then wrote "it is the
  standard thing to do" — while that paper's whole argument is that universal
  prophylaxis is no longer supported and roughly half the pooled patients never
  received it. That is worse than not citing it: the citation resolves, and
  contradicts the sentence it is attached to. **Read the conclusion, not just
  the abstract's background.**
- **A verbatim C# string does not process escapes, and a test regex written in
  one will silently never match.** `@"°"` is six literal characters, not a
  degree sign. WI-512's "only one temperature threshold" test hunted for a
  backslash and passed on a page carrying two. The break harness caught it; no
  amount of reading would have.
- **Grep the build for "error", not "error CS".** WI-512 lost a full round to
  MSB3027: a dev server held `BrainHarbor.Web.exe`, every build failed on the
  copy step, the check in use matched C# errors only and reported zero, and
  `dotnet test` ran the PREVIOUS assembly — returning green with 34 new tests
  that had never been compiled. The suite was reported green twice before the
  test count was noticed not to have moved. **Check the test COUNT changed when
  you add tests**, and fail the harness loudly on a stale assembly.
- **A suppression is only meaningful if the word is there to suppress.** Two of
  WI-512's six `!%term%` markers suppressed nothing: `neutropenia` appeared
  nowhere in reader prose on any page, and `carmustine wafer` never matched the
  page's plural because the glossary matcher is whole-word. Both
  `DoesNotContain("def-…")` assertions passed for the wrong reason, and both
  glossary entries were unreachable site-wide. **Assert the term appears in the
  prose before asserting its tooltip does not.**

**And the slot that was dropped and put back.** WI-512 dropped slot 5 ("what
does it feel like?") arguing four drugs given four ways share no answer. Review
disagreed and was right: three of the four are capsules at home, and a CYCLE has
one shared shape — unremarkable on the day, sick that evening, flat by the end
of the week, and **the nadir arriving exactly when you feel finished**. The
material was already on the page, scattered between two other sections, which is
the WI-506 trap. It is now slot 5, headed as a role rather than copied ("What
does a cycle feel like?", the WI-507 precedent). It would have been the first
treatment page to drop slot 5 while both siblings carried it — and §12.8's own
warning is that three bent pages in a row is how a template quietly shrinks.
**When you drop a universal-ish slot, check whether the material exists anyway
somewhere worse.**

**Eight more from the eighth one (WI-519, the biopsy — and the first library
page written after three §12.3 HUBS in a row):**

- **The first draft had no risk section at all, and only the end-to-end read
  found it.** The page described a needle going into someone's brain, told the
  person driving them home to call an ambulance if they suddenly could not
  speak, and never once said what could go wrong or why. Every gate passed:
  reading grade, ContentCheck, 1,411 tests. §12.8's WI-506 rule is to answer
  the frightening question in **both** directions; a page that answers it in
  **neither** is the worse failure, and it is invisible to a suite that can only
  check what is present. **Seventh consecutive item where reading the page end
  to end caught what nothing else could.**
- **A tests page CAN carry a caregiver section, and this is the first one that
  does.** §12.8 said tests pages mostly do not, "where a test does have an
  aftercare tail, it gets one" — and this is that test: someone goes home the
  same day with a hole in their skull and a person who has to watch them
  overnight. One misfit is worth knowing before the next one: `[CAREGIVER]` says
  ask what counts as an ambulance "for this tumor", and the reader of a biopsy
  page **does not have a diagnosis yet**. It was left alone rather than reworded
  on twenty including pages for one preposition, but that is the §12.10 question
  in miniature and the next page to notice it should decide it properly.
- **A dossier bullet's citation covers the clause it came from, not the
  bullet.** §4.3 sources "a small shaved patch, a small incision, a small hole
  in the skull ('burr hole'), and needle passes" to ACS in one breath. ACS
  contains the string "shav" **zero** times and never writes "burr hole". This
  is WI-512's bundled-claim shape for the third time, and the fix generalises:
  **where a source does not support a detail the reader will still ask about,
  move it into the questions list.** "Will any of my hair be shaved, and where?"
  asserts nothing and is more useful than the sentence would have been.
- **Check the study design, not just the sentence.** The dossier's "a
  meta-analysis found no significant difference in diagnostic yield between the
  two systems" rests on `surgicalneurologyint.com`, which returns a **24-word
  JavaScript shell**, and on PMC10219353 — which is not a meta-analysis but a
  72-patient single-centre comparison (42 frameless, 30 frame-based) whose own
  discussion asks for further study. Per §12.14 the test bans the URL **and the
  word**: no source on this page may be described as a meta-analysis, because
  none of them is one.
- **Not writing the shared paragraph is also an option, and sometimes the right
  one.** This page was about to copy `/treatments/craniotomy`'s risk framing
  verbatim ("There is no percentage on this page, and that is deliberate. The
  published figures ... vary so widely"), which is exactly §12.8's own
  factor-at-the-second-use trigger. It was neither copied nor factored, because
  **the second page had a better reason than the first**: the reported bleeding
  rates after a biopsy run from 2.3% to 72% and the source says outright that
  the spread is about what each study **counted** as a hemorrhage, not about
  risk. A block would have flattened a real difference into one generic
  sentence. **Before factoring a paragraph, check the second page has the same
  reason, not just the same shape.**
- **A glossary entry defined only on the page that suppresses it is
  decoration.** WI-519 nearly added a `burr hole` entry: the page defines the
  word in prose, so §12.8 says suppress the tooltip here — and no other page
  uses the word, so the entry would have fired nowhere at all. Dropped. The two
  entries this page **does** suppress were checked the other way first, and the
  check has a trap in it: `stereotactic biopsy` and `neuronavigation` appear in
  **no other page's prose**, and both are reachable anyway, through their
  aliases (`needle biopsy` on `/tumors/astrocytoma`, `navigation scan` on
  `/tests/mri`). **Grep the aliases, not the term.** Both greps here were wrong
  the first time. This is also the first page in the corpus where WI-510's
  "both directions observable" test is actually available, so it is written.
- **Linking a word turns its tooltip off.** `GlossaryMarker` marks paragraphs
  and skips links, so `That operation is a [craniotomy](/treatments/craniotomy)`
  renders with no popover. The tooltip test caught it. The fix is to say the
  word in prose and hang the link on different words ("A craniotomy, step by
  step"), and it will recur on every page that names a procedure it also links
  to.
- **A one-directional coverage check only sees the direction it was written
  for, and it is usually the safe one.** WI-519's tier test asserted that every
  symptom **this page carries** sits in the same tier on
  `/treatments/craniotomy`. It could not see a symptom the sibling carries and
  this page had **dropped** — and three had been: chest pain, a fall or a knock
  to the head, and pain the medicine is not touching. That is the under-triage
  direction §12.8 (WI-511) already calls the more dangerous one, and the test
  written after that lesson still only checked over-triage. **When you diff two
  lists, diff them both ways.** The reverse check is a bullet COUNT, not a
  phrase match: the first attempt compared each sibling bullet's opening words
  and failed on its own correct page, because craniotomy writes "A headache
  that is new" where this page writes "The headache is new". Counting forces a
  deliberate edit without becoming a rewording detector.
- **A test named for a defect reads like the defect was handled — §12.14, one
  level up.** `TheAmbulanceListDoesNotOverEscalateASeizureAgainstTheSeizurePage`
  asserted three things and none of them was this page's own seizure bullet.
  Review mutated the bullet to a flat "A seizure" — WI-510's blocker
  **verbatim** — and all eighteen tests stayed green. The name was doing the
  reassuring, exactly as a banned URL does.
- **§12.8's own recorded lesson does not stop the weaker check being written
  again.** The caregiver-duplication guard here was the **bold lead-in** check
  that this section already describes as insufficient ("WI-510 asserted the
  block's four bold lead-ins were absent and shipped four genuine duplications
  that were none of them"), while the shingle version already existed in three
  sibling test files. Review pasted three verbatim block sentences into the
  page and the suite stayed green. **When §12.8 says a check is insufficient,
  the fix is usually already written somewhere — copy it, do not re-derive it.**
- **A vocabulary ban is a filter, not an ownership proof, and it should say so.**
  The guard stopping this page restating WI-507's lab pipeline was five nouns;
  review wrote a genuine restatement using none of them ("set hard, cut into
  slices thinner than a hair, stained, and read under a microscope") and it
  passed. The list is wider now and the comment states the limit outright,
  because nothing can mechanically decide whether one page has restated
  another's subject. Auto-deriving the list from the sibling's own steps was
  tried and rejected: those steps' content words include *tissue*, *needle*,
  *piece*, *surgeon*, *gene*, *tests*, *report* and *tumor*, every one of which
  this page needs.
- **A cause borrowed from a sibling page can be the wrong cause.** The risk
  section said "where the tumor sits is what decides most of that", which is
  true on `/treatments/craniotomy` and is imported reasoning here: the paper
  this page cites concludes that **lesion size** and **bleeding during the
  procedure** are the two factors, and names location only as one of six from
  other people's literature. Worse, the wrong cause was propping up a
  reassurance ("part of why your team chose this route"). §12.14's rule again:
  the sentence was checked against the dossier and not against the paper.
- **Narrowing a source's prohibition is a defect even when the words look
  bigger.** CRUK says "You must not drive after having a brain biopsy"; the
  draft wrote "you must not drive **yourself**", under the heading "someone to
  get you home", which reads as a rule about that afternoon. It also never
  reached the caregiver section, where `/treatments/craniotomy` deliberately
  puts driving because that is the person who can enforce it. Same shape: a
  blanket restriction quietly scoped to the safer-sounding case.
- **A new library page nothing links to is half shipped.** `/tests` has no
  index, so a page's only way in is the pages that mention it. This one is
  linked from `/tests/mri`, `/tests/waiting-for-results`,
  `/treatments/craniotomy` and all six adult glioma hubs, whose "how do doctors
  find out" sections already said "needle biopsy" as flat text. **The hub
  additions are appended sentences, not rewrites of existing ones**, so no hub's
  own assertions could break — and the words `needle biopsy` deliberately stay
  unlinked there, because linking them would have turned off the one live
  tooltip for the `stereotactic biopsy` entry.

**Nine more from the ninth one (WI-520, getting ready for surgery — the first
page in the corpus that publishes an instruction a reader can follow tonight):**

- **A string-shaped test is not a claim-shaped test, and this is the item that
  proves how far apart they are.** `/review` ran sixteen adversarial mutations
  against this page's first test suite and **thirteen went through green**, six
  of them against tests **named** for the exact defect being reintroduced. A
  four-literal ban on *"steroids are generally continued"* was defeated by the
  singular. A day-count guard requiring `N days (before|prior)` was defeated by
  **"five days ahead"**, then by a drug that was not on its list, then by the
  questions list it never read. An age regex written to catch the dossier's own
  *"above roughly age 50–60"* could not match it, because the dossier puts an
  adverb and the word "age" between the preposition and the number. **When a
  guard is about a claim, ban the shape: a quantity next to a meaning, checked
  per sentence.** Every one of the thirteen is now in the mutation table and
  fails on LF and CRLF.
- **Selecting a sentence on a word that the sentence next door also contains
  will pick the wrong sentence and pass.** The European fasting median has to be
  attributed in the sentence that prints it (§12.8, WI-507). The guard selected
  the sentence containing `"twelve"` — which the *attribution* also contains, in
  "twelve **European** countries" — so splitting the two apart left the figure
  unattributed and the test green. **Select on the FIGURE (`twelve hours`), not
  on a word the figure happens to share with its label.**
- **A section-scoped position test cannot see the short version, and the summary
  is where most readers stop.** The fasting section is built correctly: the
  hospital's own times first, the two-hour figure second, an action last. The
  **summary** carried the permission with no caveat at all — and the test
  written for the summary **pinned the uncaveated sentence in place**. §12.3's
  own number is the argument: readers consume 20 to 28% of a page. **Any rule
  about where a dangerous sentence sits has to be asserted over the whole
  reader text, not one section.**
- **Print the scope the source attaches to a rule, not just the rule.** The
  ASA's two-hour clear-liquid interval is "for **elective procedures**", from a
  document titled "Application to **healthy patients**", in a paper most of
  whose length is about the people it does not hold for — GLP-1 receptor
  agonists, gastroparesis, labor. The front matter quoted the qualifier; the
  prose dropped it. That is §12.6's "keep the qualifier, shorten it" and it
  matters more than usual here, because the section then **arms the reader to
  push back on a longer fasting time**. For somebody on a weight-loss injection
  the longer time may be the right one, and without the scope the page has told
  them it is institutional habit.
- **A number a reader can act on is a different risk from a number that
  misleads, and §12.4 needs both readings.** R2 keeps procedural risk
  percentages off the site because the spread misleads. The per-drug
  antiplatelet stop-day counts are not like that — they are real, precise and
  correctly sourced. They stay off **because a frightened reader CAN follow
  them**, next to a heading saying not to. The page keeps one number, "for some
  of them it is around a week", and it only ever pushes in the safe direction:
  **ask sooner**. Ask which way a number pushes if the reader acts on it alone.
- **Ban the URL and re-derive the claim, on the same pass.** Sixth item running
  for §12.14, and this time it was caught by the end-to-end read rather than a
  gate: the dossier hangs "dexamethasone and anti-seizure medicines are
  generally continued" on an **MRI page** whose only medication sentence is
  "take your regular medications as usual". The citation was banned in a test —
  and the section opened with *"Most of what you take, you keep taking"*, which
  is that sentence in my own words, three tests below the ban.
- **A test that reads the raw markdown cannot see a shared block.** The
  escalation-absence guard ran on the page source, where the caregiver section
  is the eleven characters `[CAREGIVER]`, so it proved a phrase absent from a
  page nobody reads while the **composed** page says "what counts as 'call an
  ambulance'" inside the block. Compose first, then subtract the block text if
  what you mean is *this page's own prose*. Related: `CuratedPage.ReaderText`
  strips front matter **by index**, so fed a `Section()` it returns
  `section[3..]` — three characters off the front, silently breaking any
  assertion about how a section opens. It cost this item a round and then
  appeared again thirty lines from the comment warning about it.
- **The absence of an escalation list is the right answer for a page whose
  reader has no symptoms yet, and it needs a shape check rather than a
  phrase check.** Nothing here can be diffed against `/seizures/what-to-do`,
  because nothing has happened yet; the one escalation-shaped event before the
  day is becoming unwell, and no reachable source supports a triage rule for it,
  so the caregiver section **asks the question instead of answering it**
  (§12.8, WI-519). Guarding that with four banned headings failed immediately —
  `/review` wrote a fifth. The working check is structural: an urgency-flavoured
  bold lead-in introducing a run of symptom bullets.
- **The `[CAREGIVER]` "for this tumor" question, decided.** WI-519 flagged the
  block's *"what counts as 'call an ambulance' **for this tumor**"* as a misfit
  on a page whose reader has no diagnosis yet, and left it for the next page to
  settle. Settled: **it stays, and it is not a misfit here.** This reader has a
  planned brain operation, so they have a diagnosis; WI-519's reader is the
  narrower case, mid-diagnosis, and one preposition on twenty including pages is
  not worth a fork for it. If a later page genuinely has no diagnosed reader at
  all, that page reopens it.
- **PLAN.md §5 bans AHFS *drug monographs*, which MedlinePlus hosts under
  licence — it does not ban medlineplus.gov.** A MedlinePlus **Medical Test**
  page is NLM's own public-domain writing, and it is the only patient-level
  source in this page's set that says an ECG is painless and says plainly why
  somebody has one before an operation. Recorded because the easy mistake is to
  widen a ban to the domain, which is §12.13's shape exactly, and a test now
  asserts both halves: the lab-tests URL present, `medlineplus.gov/druginfo`
  absent.

**Thirteen more from the tenth one (WI-521, follow-up scans — the page that
publishes no numbers at all, on the subject the backlog asked for numbers on):**

- **R2's reasoning reaches a FREQUENCY, not only a procedural risk, when the
  spread is about what each study counted.** The backlog asked for
  "10-30% of glioblastoma patients"; the dossier says "20-30% ... range 12% to
  64%"; the ASCO Post says "28% to 66%"; PMC10412732 says "approximately 36% in
  a recent meta-analysis" and separately reports a biopsy series at 12.4%. Five
  answers, none of them each other, because each counted a different thing
  (radiological change, confirmed change, tissue-proven change). **The page
  publishes none of them — and says so.** "There is no reliable figure to put
  here, because studies that counted it in different ways came back with very
  different answers" is content. A page that simply omits a frequency leaves
  the reader to supply their own, and the one they supply is usually worse than
  the truth.
- **A frequency guard looks FORWARD from the number, and it needs its own
  quantity vocabulary.** English always puts the population after the count
  ("a third of people", "one in three patients"). Looking backwards as well
  flagged *"a lot of people read it as one on the drive home"*, where the
  population belongs to a different clause. And the quantity list cannot be
  shared with the schedule guard: that one needs `few`, `several`, `once`,
  `twice` ("every few months", "twice a year"), and a frequency guard holding
  them fails *"for most of them the worst part was not the scan"*, which is
  correct, sourced, and on three tumor hubs already.
- **Invert a guard only after checking the correct page survives it.**
  "Every quantity in this section must be a length of time" is right for the
  two look-alike sections and wrong for the report-words section, which has to
  be able to say *"two things about those rules"*, *"ask one question"*, *"a
  tumor that got a little smaller and one that got a little bigger"*. Widening
  an exclusion list until those pass hollows the guard out one idiom at a time.
  What a threshold actually looks like is **a magnitude preposition next to a
  quantity** (`by|at least|more than|over` + number), which has no innocent use
  and catches `"it shrank by at least half"` on the first try.
- **THE RESTATEMENT CHECK HAS TO RUN OVER THE WHOLE CORPUS.** This page's first
  version checked three hand-picked tests pages and reported zero — while
  sharing **108 eight-word shingles with `/tumors/high-grade-glioma`**, whole
  paragraphs of pseudoprogression and radiation-necrosis prose lifted wholesale,
  and the two copies had already drifted inside a single commit ("dead tissue"
  there, "damaged tissue" here). A guard that checks the siblings you thought of
  is a guard that finds nothing. Three supports make a corpus-wide run usable:
  **strip headings and link targets first** (slot 9's heading is identical on
  all 29 library pages by prescription, and a URL fragment tokenises into eleven
  words), and **require three content words** (eight function words in a row
  collide by chance — "and it is not a sign that your").
- **A deliberately shared sentence is a short, commented allowlist, and the
  comparison runs the other way round.** §12.10 says two pages must not state
  one safety claim at two strengths, so where a claim is load-bearing on a hub
  and on its canonical page the right answer is **identical words**. The list
  here is four entries and every one is a claim or a link label. The first
  implementation asked `shingle.Contains(allowedSentence)` — backwards, since
  the shingle is the shorter string — and did nothing at all.
- **Find the LIST first, judge the lead-in second.** The escalation-shape guard
  copied from WI-520 was a single regex anchored on a `**bold**` lead-in, and
  `/review` beat it three ways: a `###` heading instead of the bold, an ordinary
  sentence between the lead-in and the bullets, and three bullets instead of
  four. Inverted, it cannot be walked around, because the bullets are what make
  it a list. Two things to get right: the trigger words must be **urgency**
  words only (`team`, `doctor` and `nurse` fail this page's own "What to ask
  your team" list), and the lead-in must be **the preceding paragraph**, not a
  character window — a hard-wrapped bullet breaks the run, so a long list
  arrives as several runs and a window reads the list's own earlier bullets as
  its introduction.
- **Fixing the canonical page exposes what the hub was missing.** RANO 2.0 gives
  three routes out of the twelve-week rule; `/tumors/high-grade-glioma` listed
  the two harsher ones (outside the field, or a sample) and omitted the
  mandatory confirmation scan, which is the **most reassuring** one and the one
  a reader is actually living through. Writing the canonical page is when that
  gets noticed, so budget an edit to the hubs when you write one.
- **`SentencesOf` cannot split a sentence that ends `.**`.** It splits on
  terminal punctuation *followed by whitespace*, and a closing bold marker is
  not whitespace — so a claim and the sentence that qualifies it arrive as one
  chunk, and `sentences[claimAt + 1]` is the wrong sentence. Use index
  proximity for an adjacency assertion about a bolded claim.
- **A last-sentence pin has to anchor to the END of the sentence.** §12.8
  (WI-510) says pin the last sentence rather than a three-sentence window;
  that is not enough on its own. `/review` appended *", but if the next scan
  shows the same thing, it is usually growth"* to the closing sentence of the
  most frightening section on the page, and the pinned words were still inside
  the final sentence while the section now ended on the fear. `\.?\s*$`.
- **An ordering test on FIRST OCCURRENCES proves nothing about a restatement.**
  `IndexOf(scope) < IndexOf(figure)` was satisfied while `/review` appended
  *"Around eight months after radiation is when it usually turns up"* to the end
  of the section — the SRS-only figure generalised to everybody, in the sentence
  after the one saying it is not a rule for everybody. Assert the figure appears
  **exactly once**, and that its scope is in **its own sentence**.
- **A glossary entry can be unreachable and look completely fine.** `RANO` is in
  the glossary and appears in **no page's prose anywhere in the corpus** — this
  is the first page to say the word, and this page defines it, so §12.8 says
  suppress the tooltip here. The entry therefore fires nowhere. It is kept
  rather than deleted (the word is on real reports and the entry is correct) but
  the state is **pinned by a test** rather than believed: the next page that
  says RANO in prose makes it live and that test goes red, which is the prompt
  to delete the test. WI-519 dropped an entry in this position; this one is
  older than the page that would have justified it.
- **A page about results has to say what a scan cannot do, and the end-to-end
  read is what finds it missing.** §12.8's WI-506 rule again: a follow-up scan
  shows what is big enough to see, so a scan with nothing left to measure is a
  real and welcome thing **and** is not the same as saying nothing is there.
  Without both halves the page either promises cure or tells somebody their
  clear scan means nothing.
- **NINTH CONSECUTIVE ITEM WHERE READING THE PAGE END TO END CAUGHT WHAT NO GATE
  COULD.** On this one it found: the missing "what a scan cannot do" section; a
  reader whose report says **progression** reaching the bottom of the page with
  nowhere to go; *"anyone who gives you one is guessing about you"*, which
  undercuts the one authority the page keeps telling the reader to ask;
  *"they are not allowed to give you a result"*, a policy claim no source
  supports where a role fact was available; *"your team **will** start again
  from the top with you"*, a guarantee about somebody else's behaviour next to
  the correctly-hedged version of itself; and a sentence that had been edited
  into nonsense — *"the rules your team works to build a step in for it"* —
  which reading grade 5.3, ContentCheck 245/0 and 1,477 tests all passed.

**Twelve more from the eleventh one (WI-522, watch and wait — the page whose
headline claim did not survive its own source):**

- **The backlog's headline claim can be the defect, so read the paper's Methods,
  tables and Conclusion before the abstract.** The item's spine was
  *"watch-and-wait carries 4.26x higher risk of a pathological depression
  score"*. The paper (PMC7761113) is a cross-sectional survey of 31 people being
  watched and 31 after a perfect operation; 4.26 is a **univariate odds ratio**
  (95% CI 1.19-15.25) that the **abstract mislabels "Multivariate"**; anxiety did
  not differ; and its own conclusion is that distress is high **"independent of
  management strategy"**. Three other studies found no watch-and-wait excess, and
  the dossier's own note under the figure said *"Do not publish the odds
  ratio"*. The page prints what the studies agree on (worry is common either
  way, in meningioma), names the one study and the disagreement in consecutive
  sentences (WI-511), and prints no number. **The cost stayed the page's centre
  of gravity. What changed is the claim.**
- **A page serving several tumor types splits its reassurance by tumor.** The
  "reassuring growth data" is meningioma data; diffuse low-grade gliomas "grow
  continuously and usually transform" (Jakola 2017). The growth section is one
  paragraph per tumor, and the guard is positional: reassurance-shaped sentences
  may live only in the paragraphs of the tumors they are true of. Everything
  else in the section is checked, and anywhere outside it a reassurance sentence
  must name a tumor it is true of **and must not also name a glioma**. /review
  beat an any-tumor-name check with "Whether it is a meningioma or a glioma, the
  growth usually slows down".
- **A cohort's best sentence can hide its main result.** Strømsnes gives "None
  developed symptoms prior to intervention" and "Intervention was avoided in
  > 40%", and the first draft built its meningioma paragraph from those, while
  the same paper treated **54% of its patients**. After "slows down or stops",
  "some tumors kept growing and were treated" read as the exception. A
  proportion that changes the picture goes in as a comparison ("more people
  ended up having treatment than not") when it cannot go in as a number.
- **Practice is not evidence.** "Experts also differ on whether to operate on a
  glioma found by chance" rested on a German practice survey inside Cochrane's
  excluded-studies table, while the guideline the page cites says "Surgical
  resection is the first step ... even in incidentally discovered tumors".
  WI-512's shape again, a source cited for the position it argues against, and
  on the one reader it matters most to: somebody watched without an operation.
- **Writing the canonical page found three defects on a hub.** The low-grade
  hub said starting radiation early "buys nothing" (EORTC 22845 says it buys a
  longer stretch before regrowth, and fewer seizures), said watching is "harder
  to live with than treating" (no source; the ones checked here say otherwise),
  and **150 lines higher said the same comparative in different words**, "hard
  in a way that treatment is not", which a two-phrase ban could not see. The
  ban is claim-shaped now. §12.8 (WI-521) said budget an edit to the hubs; this
  one needed three.
- **One claim, one wording, when the claim is this close to prognosis.** EORTC
  22845's survival half was on three pages in three strengths ("lived about as
  long", "has not been shown to help them live longer", "has not been shown to
  change how long people live"). It is now identical words on all three, in the
  restatement guard's allowlist, and a test reads all three pages.
- **A trigger stated as absolute can contradict the data two screens down.** The
  summary, step six and the growth section all said "if it grows, the plan
  changes", above "most meningiomas that are watched do grow a little". The
  trigger in the sources is **sustained** growth. Found by the end-to-end read.
- **Check what else is offered to exactly this page's reader.** The first draft
  never mentioned vorasidenib, which was tested against a dummy pill **because**
  watching after surgery was the standard for those patients (the FDA summary
  says so). A watched grade 2 glioma reader who does not know to ask is the
  reader the page failed. Also found by the end-to-end read: **the tenth
  consecutive item** where that read caught what no gate could, and this time
  it also caught a guarantee about the team ("your team **will** think again"),
  WI-521's defect, again.
- **A list finder that needs two consecutive marker lines finds nothing when
  every bullet wraps.** WI-521's escalation-shape guard assumed a wrapped list
  arrives as several runs; /review wrote three bullets that each wrapped, and it
  arrived as none. List items now include their indented continuation lines,
  and with that change the guard must read the **body**, or the front matter's
  `- url:` / `title:` source list becomes a "list" of its own. Fixed in both
  suites that carry it.
- **A negation inside a superlative is not a negation.** "There is **no** better
  way to ease the worry than talking to a counselor" passed the clause-anchored
  negation check (WI-511), because it uses "no" to mean "most".
- **A sentence written to pass a test gets checked against the source too.** The
  first version of the never-treated sentence read "for some, **most often with
  a meningioma**, it never turns into treatment": a tumor name was added to
  satisfy the scoping check, and it created an unsourced comparative (by the
  page's own sources, small pituitary tumors change least). It now names the
  three tumors the sources support.
- **The machine can be the flake.** Midway through, a scheduled disk backup
  (`ReflectBin`) saturated the host disk. Postgres checkpoint syncs took up to
  125 seconds (`docker logs`), Npgsql threw "Exception while reading from
  stream", and a test host hung for twelve minutes. Nothing was wrong with the
  change. Run `--filter "Category!=Database"` for the content suites while it
  lasts, put `--blame-hang-timeout` on full runs, and check the render tests
  green unmutated before trusting the harness. A render mutation that "fails"
  on a database timeout proves nothing.

Two dossier details worth knowing next time: the "675 untreated meningiomas"
cohort was a **page number** in a reference (J Neurosurg 110:675-684), and a
source listed as 403 can come back (The Brain Tumour Charity returned 200 on
2026-09-10). Re-fetch before you drop.

### 12.9 The tumor-hub template, proved (WI-513)

§12.8 is the LIBRARY-page template. This is what WI-513 learned taking one
**tumor hub** all the way to §12.3's seventeen sections, which is the shape the
remaining 23 hubs inherit. Read this before drafting one, and read **§12.3** for
the order — not §12.8, whose twelve slots the seven Wave 1 pages made familiar.

**The two blocks that are easy to leave out, because nothing prompts you.**
WI-513's first draft had neither, and both are required by the contract:

- **The self-blame block.** §12.3's own "Deliberate omissions" paragraph says
  "the self-blame block still appears; it just is not near the top", and it is
  the one instruction in that paragraph that reads like an aside. It is now
  `Content/blocks/causes.md`, included as `[CAUSES]`, and **demoted** — WI-513
  places it between "if it comes back" and the outlook gate. Late enough not to
  greet a frightened reader with "nobody knows what caused this", early enough
  that they meet it before outlook.
- **The retired-name crosswalk slice.** Contract item 3: "where an older name
  was retired, say so: readers arrive holding old paperwork." Research §0.3
  calls it the single highest-value block on the site. The canonical full
  crosswalk belongs to the glioma umbrella page (SYNTHESIS §4.3); **every hub
  still owes a slice** naming the retired terms its own readers are holding.

**The escalation list is `[ESCALATION]` and a hub must INCLUDE it, never re-type
it (WI-563).** `Content/blocks/escalation.md` carries the ambulance tier, the
same-day tier, the "not every seizure is an ambulance" line, both conditional
fever rules and the after-hours instruction. Put the directive **inside the
symptoms section**; a test reads that section, not the page, so an
`[ESCALATION]` that drifts to the bottom goes red. The block was factored out of
three byte-identical hand-copies at the fourth use, three uses after §12.8's
rule said to, and the fever sentence beneath it had already diverged between two
of them.

**A ban list on retired names has to be negation-aware, or it forbids the
crosswalk.** WI-513's first test banned "oligoastrocytoma", "anaplastic" and
"mixed glioma" outright. It passed — and would have made the crosswalk
impossible on all 23 hubs that copied it, foreclosing the highest-value block on
the site with a test whose comment said it was enforcing CNS5. There are two
completely different things a retired name can be doing on a page: used as a
live diagnosis (forbidden) or NAMED as retired (required). Same for Roman
numerals, and the site-wide Roman guard's allowance had to stop being pinned to
one page by slug for the same reason.

**The reader-choice gate, now that one page has actually used it.** WI-503 built
it; nothing used it for ten items. What using it teaches:

- **The gate is for outlook, and the right tail belongs INSIDE it.** WI-513's
  draft put "some people live many years, working and driving and raising
  children" in section 2, where a reader who declined outlook met the most
  hope-preserving sentence on the page anyway. That is precisely what the gate
  exists to put behind a choice. The section it came from still needs a landing
  (§12.6), so give it a non-prognostic one.
- **A gate that publishes no figures still has to teach the vocabulary.** §12.5
  is headed "explain the concepts, publish no figures", and the Kirkebøen
  framing wants four moves when explaining *median*: establish it describes a
  **group**, give the **distribution** rather than the midpoint, name the
  **right skew**, and say plainly it is not a prediction about one person.
  WI-513's first draft did one of the four and its own test BANNED the word
  "median" — which would have cemented, across 23 copies, a gate that explains
  nothing.
- **Verified rather than assumed, because every fail-open mode builds green.**
  Check the rendered HTML (heading outside, `<details>` not open, no `:::`
  leaked, the words not also present outside the gate) and the PDF: §12.5 says a
  gate left closed stays closed on paper, and WI-513 is the first item that
  could confirm it.

**A shared test helper written for one template will quietly assert it on the
other.** `AssertLinksResolve` hard-coded `id="where-to-go-next"` — §12.8's last
section — because every caller until now was a library page. A §12.3 hub ends
with "Where to get support". The id is a parameter now
(`AssertLinksResolveIn`), and it is a **separate method, not an overload**:
adding `(client, url, string, params string[])` beside
`(client, url, params string[])` made C# prefer the new one for every existing
caller and silently reinterpret their first required link as a section id.

**`AssertLinksResolve` cannot see fragments, and the first page to deep-link
another one proved it.** Its regex stops at the `#`, so a link to an anchor that
does not exist resolves as a healthy 200 and lands the reader at the top of a
long page. `AssertFragmentLinksResolve` is the check; WI-513's own draft pointed
at `/tests/pathology-report#grade`, which was not an anchor on that page.

**And the citation discipline has a second half nobody had written down.**
"Fetch every source URL before citing it" has been the rule since WI-505, and
WI-513 followed it — every claim was checked against the fetched text. Four of
its six citation **titles** were still wrong, written from the research
dossier's description of a paper rather than from the page that had just been
fetched. Titles render as the visible link text under "Sources", so a fabricated
title is a fabricated citation on the reader's screen, and no test can catch it.
**Paste the `<title>` the fetch script prints. Every time.**

### 12.10 Scoping a shared block (WI-514)

WI-501 built the include mechanism and every block it shipped was used by one
page. WI-514 wrote the first two blocks meant for **all 24 hubs**, and both were
wrong on the first attempt in the same way. Read this before writing or
including one.

**The test: would this sentence be true on the hub you have thought about
least?** Not "is it true of gliomas". A block is prose you are asserting on
every page that includes it, and the failure is silent — nothing in ContentCheck
or the test suite can tell you a block's words are wrong for the page pulling
them in. The reader is the only detector, and by then it is on 24 pages.

- **`[MECHANISM]`'s first draft said "they grow through brain tissue instead of
  pushing it aside" and "'they got it all' and 'cured' are not the same
  sentence".** True of diffuse gliomas. **False, and frightening, for a fully
  resected grade 1 meningioma or pilocytic astrocytoma** — tumors that do push
  tissue aside and are cured by surgery. It also said "the most common ways a
  **glioma** shows up", which is simply not about meningioma at all. The
  infiltration material now lives on `/tumors/glioma` under its own heading, and
  the block carries only what a closed skull does to anything inside it.
- **`[CROSSWALK]`'s first draft was the glioma rename table.** Every entry was a
  glioma name, so a meningioma hub including it would have inherited nothing
  relevant. Worse, `/tumors/low-grade-glioma` was switched from its own
  three-name slice to the full block, which put **glioblastoma and DIPG entries
  into a grade-2 patient's identity section**. That is a regression dressed as
  factoring.

**So the split is: the block carries what is universal, the page carries its own
slice** — which is what SYNTHESIS §4.3 says ("canonical: the glioma umbrella
page, **with the per-tumor slice repeated only where it differs**") and what the
WI-514 backlog entry says. The universal crosswalk is the 2021 rewrite, Roman to
Arabic, gene results becoming part of the name, and NOS/NEC. The specific
renames belong to the family page that owns them.

**A hedge in a block is not weasel wording, it is scope.** "Some of what you are
feeling **may not** be the tumor itself" reads weaker than "is not" and is the
correct sentence, because swelling is not every tumor's story. Tests have to
match the hedged wording; one of WI-514's failed on exactly this and the test
was what needed changing.

**Two test-shaped traps this produced.**

- **A test named for a block that asserts the page.** `TheCrosswalkBlock
  ComposesIntoThisPageCarryingEveryRetiredName` checked words that had moved
  onto the page, so emptying the block left it green. If a test's subject is the
  block, assert something only the block says.
- **A test asserting a phrase where the claim was the property.** Checking that
  "not curable" appears passed on a page whose only use of the words was the
  softening line beneath it ("Not curable is not the same as untreatable"), so
  deleting the hard sentence stayed green. Same family as WI-512's urgency
  tests, which asserted presence when position was the property.

**And the surface a front-matter check cannot see.** Block `sources` merge into
every including page and **render in the reader's source list**. A banned or
dead URL added to a block ships onto 24 pages while a test that reads only the
page's own front matter stays green. Check the blocks too.

**Cross-page consistency tests must read the other page.** WI-514's escalation
test named `/seizures/what-to-do` in its comment, hard-coded what that page was
believed to say, and never opened it — a consistency check that could not see an
inconsistency. It reads the siblings now. (And strip markdown emphasis before
matching: the seizure page writes `**first ever** seizure`, and a regex walking
past the asterisks reports the sibling has stopped saying it.)

**A CONDITIONAL survives the "hub you have thought about least" test, and that
changes what belongs in a block (WI-563).** §12.12 recorded the escalation
tiers as universal and the fever line's routing as page-local. That was wrong,
and the item reversed it after finding the defect the split would have
preserved: **`/tumors/glioma` linked `/treatments/chemotherapy` and contained
the word "fever" zero times, and `/tumors/low-grade-glioma` mentioned
chemotherapy four times with "fever" only inside a link label.** Two live pages
with §12.11's defect, which WI-515 was blocked for nearly shipping once.

The reason a conditional is different: *"**If you are having chemotherapy**, a
fever is its own rule"* is true on every hub and false on none, because a reader
it does not apply to is not addressed by it. An unconditional claim has to be
true of the page; a conditional only has to be true of the reader it names. So
the block also carries the surgery half — a post-craniotomy fever is not
chemo-conditional and every hub routes into `/treatments/craniotomy` too.

**Two limits on that, which the next hubs will hit.** A conditional is still
*prose asserted on 24 pages*, so it must be checked against the hubs that do not
exist yet: a **spinal-cord** hub would inherit an ambulance tier headed "A first
ever seizure" with no cord-compression line, and a **pituitary** hub would file
sudden vision change as same-day when apoplexy is an emergency. The block is
right for the glioma family it was written from; **it is not automatically right
for a hub whose emergencies are different**, and the test that requires it
(`AHubThatRoutesIntoATreatmentCarriesThatTreatmentsSafetyRule`) fires on a
chemotherapy link, not on whether the tiers fit. Read the tiers against the
tumor before including, and add the page's own line beneath the block if its
emergency is not in the list.

**A block's threshold belongs to one page.** This block's own six sources carry
**three different fever numbers** (100.4 °F on ACS, 100 °F on UMass, 37.5 °C on
CRUK). `/treatments/chemotherapy` publishes one, sourced, and says teams differ;
the block routes to it and publishes none. A number in a block is that number on
every hub.

**The idiom check has to run on the block, and "British" is wider than
spelling.** WI-563's first draft shipped *"Being sick over and over"* in the
same-day tier — correct in the source, and in US English it reads as *being
unwell*, not *vomiting*, on a line that is an escalation trigger. Spelling gates
do not catch idiom. `out of hours`, `straight away`, `straight after` and
`come round` are all in the corpus and all British; the block uses the US forms
and the rest are a follow-up sweep.

**When a guard is copied, its holes are copied too.** The repo had two
implementations of the British-forms check: one **strips** the exemptions and
scans, one `continue`s past a form whose exemption appears anywhere in the file.
The second disables the entire check for that form — one mention of "The Brain
Tumour Charity" would switch off the `tumour` check for a whole file. WI-563
copied the weaker one and `/review` caught it. Use the strip-then-scan form.

**A test that iterates over matches passes on a page with none (WI-517).** The
retired-name guard loops over every occurrence of a retired term and checks each
one is marked as retired. On a page that never says the word, `Regex.Matches`
returns empty, the loop body never runs, and the test is green — so it cannot
tell *"correctly marked as retired"* from *"absent"*. WI-517 shipped a hub with
**no crosswalk slice at all** behind exactly that test, on the one page whose
central fact IS a retired name (oligoastrocytoma was not renamed, it was
eliminated, and the 1p/19q test is what eliminated it). **Every
iterate-and-check guard needs a positive assertion beside it**, and the positive
one has to pin the *structure* (the bullet), not the *presence* of the word —
deleting the bullet left the word alive in a later paragraph and the first fix
passed anyway.

**The corollary for any guard: prove it can fire.** The Roman-numeral check now
asserts a canary against `blocks/crosswalk.md`, which teaches the old notation
on purpose. Without it, a regex that matches nothing anywhere looks identical to
a clean page.

**A §12.6 landing check must be scoped to the paragraph, not the section
(WI-517).** `SentencesOf(Section(heading))[^1]` is the *section's* last
sentence, and a hub section with `###` subsections under it ends somewhere else
entirely — so the check that the curability paragraph does not end on
"not curable" was reading the end of a different subsection and could never
fire. Split on the first `###`.

### 12.11 What a grouping page owes its umbrella (WI-515)

`/tumors/high-grade-glioma` is the second **grouping** page — a label covering
several diagnoses rather than naming one — after `/tumors/low-grade-glioma`.
Between them and `/tumors/glioma` there are now three pages saying overlapping
things to overlapping readers, and this is what that costs.

**The page carries its own slice, and the slice has to have an angle.** §12.10
established the split for shared *blocks*. The same rule governs two *pages*:
the umbrella's retired-name slice is the whole glioma family, so a grouping page
that copies that list has added a second maintenance site and no information.
This page's slice is one word — **anaplastic**, which before 2021 was how a
report said grade 3 — because that is the word its readers are actually holding.
The test asserts the ANGLE (`grade number does that job now`), not just that the
names appear, because a slice that is a copy passes a names-only check.

**Assert the non-duplication against the block's own words, not a literal
list.** `TheSliceDoesNotRepeatWhatTheSharedCrosswalkBlockAlreadySays` reads
`blocks/crosswalk.md`, picks the sentences the block owns, and fails if the page
also says them. Written that way, moving a sentence *into* the block later turns
the duplicate red instead of creating one silently. A hard-coded list would have
gone stale the first time the block was edited — which is the whole reason the
block exists.

**Three shared-helper traps, all the same shape: a step that quietly stops
doing anything.**

- **`CuratedPage.Section` flattens before it returns.** A splitter handed its
  output finds no newlines, yields ONE chunk, and every per-chunk assertion
  becomes a whole-section assertion. WI-515's three-regimen test passed with all
  three regimens collapsed onto one tumor until `BulletNaming` was re-pointed at
  a raw-section helper. Identical to WI-507's `Split("\n\n")` on CRLF: a
  splitter with nothing to split on does not fail, it stops being a splitter.
  **Anything that splits needs raw text, and needs a canary that fails when the
  split yields nothing.**
- **Flattening is not enough for a YAML comment.** It leaves the next line's
  `#` inside the sentence, so a wrapped comment flattens to
  `"... on a scan. NEVER # for naming or grading"`. Strip the comment markers
  first, then flatten.
- **A tooltip can only fire on PROSE.** WI-515 asserted a `def-radiation-necrosis`
  tooltip for a term that appeared only in a **heading**. That is the exact
  mirror of WI-512's and WI-514's no-op suppressions, and both directions need
  checking: a suppressed term the prose never uses suppresses nothing, and an
  expected tooltip for a term the prose never uses can never fire.

**A router that links to pages nobody has written yet must say so, and the test
should read the destinations.** WI-514's blocker was routing the cancer question
to five Wave-0 stubs while insisting it "is not a dodge". The fix generalises:
`ThePageWarnsThatTheChildPagesItRoutesToAreStillStubs` measures the destinations
and requires the honesty note **only while they are thin** — so when WI-516,
WI-517 and WI-518 land, the test fails until the note comes down. The warning
removes itself rather than becoming a lie nobody noticed.

**The unsourced comparative is this phase's most persistent defect class.** Five
items in a row have now shipped a draft containing one. WI-511 asserted "most
people in both groups" of the wrong trial arm; WI-515's draft said the swelling
around a high-grade glioma "is heavier", which is a comparative claim about how
much, and **the dossier sentence it came from is not in the paper it cites**
(§3.3, corrected at source). A comparative needs a source that makes the
comparison. If the source describes a *nature* ("infiltrative edema ... in a
zone of infiltrating tumor cells"), publish the nature. Every one of these has
been caught by reading the page, never by a gate.

**Never run the break harness in the background and then edit the content.** It
holds the originals in memory and restores them in a `finally`, so any edit made
while it runs is silently reverted — and if the run is killed, whichever
mutation was applied at that instant stays on disk looking like your own prose.
WI-515 lost four editorial fixes and shipped a mutated section order into a test
run before noticing. The harness now writes a marker file and refuses to start
if a previous run did not finish.

**Review found the §12.10 defect re-committed on this very page, which is the
strongest argument for running it.** The draft said "these tumors grow into the
brain around them … with today's treatments they are not curable" directly under
"Yes. A high-grade glioma is cancer" — so it asserted infiltration and
incurability of *every* high-grade glioma. CNS5 defines a whole supercategory of
**circumscribed astrocytic gliomas**, "gliomas with more well-delineated borders
separating them from the surrounding brain parenchyma", and two of them are
grade 3. Both siblings scope the claim; the page whose entire thesis is "two
tumors in this group can be very different" did not. **When you write a claim
about "these tumors" on a grouping page, name which ones.**

**Three more things a hub owes, all found by review rather than by a gate.**

- **A page that routes readers into a treatment owes that treatment's safety
  rule.** Every reader of this hub is offered chemotherapy, `/treatments/
  chemotherapy` opens by calling a fever "the one thing to know before you
  start", and this page's escalation section had no fever line at all. A hub is
  not just a description; it is a place people arrive before treatment starts.
- **Name the thing you tell the reader to ask about.** "A device worn on the
  head that treats the tumor with electrical fields — ask whether it is relevant
  to you" gives a reader nothing to say out loud or type into a search box.
  §12.6's first rule ("say the outcome, then name the word") applies to devices
  and regimens, not only to procedures.
- **A heading with two questions in it owes two answers.** "Where does it grow,
  and why does it cause these symptoms?" was answered only by `[MECHANISM]`,
  which covers the *why*. The *where* has to be on the page, before the block,
  for the reader who stops after the first paragraph.

**Do not carry a shared guard's EXEMPTIONS across pages with it.** The
prognosis-figure regex was copied from the low-grade page complete with a
`(?![^.]{0,60}with)` lookahead, which exists there to allow "live many years
*with* a grade 2 glioma" — and which is precisely the phrasing most likely to
leak a prognosis claim onto a grade 3 and 4 page. Copying a rule copies its
holes; re-derive the exemption list per page.

**Fetch every URL in everything the item ships, not every URL on the page.** The
citation discipline held on the page and failed on a **glossary entry**, which
shipped a hand-composed title for a URL nobody had fetched. Glossary entries,
shared blocks and page front matter are all citation surfaces.

### 12.12 Three names, one word (WI-516)

`/tumors/astrocytoma` is the first hub where **three unrelated groups share one
name** — IDH-mutant diffuse, circumscribed, and pediatric-type — and where the
answer to "can this be cured" is genuinely different across them. §12.11 said a
claim about "these tumors" on a grouping page must name which ones. This is what
happens when it does not.

**The scoping defect has a direction, and the reassuring one is worse.** WI-515
shipped a draft asserting incurability of *every* high-grade glioma: wrong, and
frightening. WI-516 shipped the mirror image — "these are usually grade 1, and
an operation that takes all of it out can be the end of it" — of the whole
circumscribed family. CNS5's own table grades that family **1, 1, 2–3, 2–3, not
established, 3**. Four of six entities are not grade 1 and are not cured by
surgery. **Over-reassurance is the more dangerous failure**, because it is the
kind of sentence that stops being true later, at the worst possible moment,
which is precisely the thing this whole phase exists to prevent. Bind the
promise to the entity the source grades, and say the group is mixed.

**A retired name and the route it describes are not the same fact.** The page's
headline claim was "if your old report said **secondary glioblastoma**, you are
in the right place" — in bold, in the description, in the short version and in
the heading. Grepped across every source fetched for three consecutive items,
**the phrase appears only in bibliography entries and in no body text at all**.
What CNS5 says is about IDH status: "IDH-mutant astrocytomas are no longer
referred to as glioblastomas". "Secondary" described how the tumor *arose*, and
not every secondary glioblastoma is IDH-mutant — so an IDH-wildtype reader
holding that word was told, first and in bold, that they were on the right page.
**Route by the line the reader can look up**, not by the phrase they arrived
with.

**Check the negation list a retired-name guard accepts.** WI-516's allowed
"is now" and "are now" as proof a name was being *named* rather than *used* —
which passes "Anaplastic astrocytoma is now treated with radiation first". The
markers that carry the check are the ones that are explicitly about the naming:
`retired`, `no longer`, `once said`, `older phrase`, `meant grade`, `was
renamed`. A guard whose allowance list is broad enough to admit the defect is a
guard that has stopped guarding.

**The IgnoreCase trap, for the fourth time, in five places at once.** §12.9
records it biting WI-505, WI-506 and WI-508. WI-516's harness planted
`**Grade III.**` and **five** copies of the Roman-numeral guard stayed green,
across four test files, because all five were written case-sensitive — and a
grade is most often written sentence-initially, which is exactly where the
capital is. All five now carry `RegexOptions.IgnoreCase`. **When a guard is
copied, its holes are copied too** (§12.11 said this about exemptions; it is
equally true of flags).

**Two more mutation-strength lessons, both "the mutation was too weak, not the
test".** Deleting one sentence of a two-sentence bullet left the negation marker
in the same chunk, and removing one of three mentions of a glossary term left
the tooltip firing. A mutation has to remove the property, not an instance of
it.

**Worth raising with `/pm`:** the twelve-line ambulance/same-day escalation
block is now hand-copied byte-identical across four hubs, and the fever sentence
beneath it has *already* diverged between two of them. With 21 hubs to go it is
the strongest `[ESCALATION]` block candidate on the site. §12.10's split
applies: the tiers are universal, the fever line's routing is not.

> **Done at WI-563, and the split above was wrong.** The block exists at
> `Content/blocks/escalation.md`. The fever line went **into** it, not onto the
> page — see §12.10 for why a conditional survives the "hub you have thought
> about least" test, and for the two live pages that were routing readers into
> chemotherapy with no fever rule at all.

### 12.13 When the richest source is the one you may not use (WI-517)

`/tumors/oligodendroglioma` is the page where §12.1's source-precedence rule
costs the most. The single best source on this tumor is the StatPearls
oligodendroglioma chapter — and §12.1 names **that exact chapter** as one never
to use for naming or grading, because it gives a correct molecular definition
and then lapses into Roman numerals and a retired term in the same article.

**The resolution is per-claim, and it has to be written down where the reader of
the front matter will see it.** The chapter carries symptoms, imaging and
treatment mechanics; naming and grading come from the CNS5-aligned sources. A
comment in the front matter says so, and the tests look for the *vocabulary that
would leak in* if the line slipped — a Roman numeral, a retired name used as a
live diagnosis — because nothing can mechanically check a claim-to-source
mapping.

**Note the ban is on the chapter for a purpose, not on the domain.**
`/tumors/astrocytoma` and `/tumors/high-grade-glioma` ban this URL outright in
their own front matter and are right to: there it was the *wrong tumor's*
chapter cited for general high-grade treatment. Right chapter for the right
claims is a different thing from right chapter for the wrong ones.

**Three claims the dossier makes that its own sources do not.** Fetched and
checked: "patients often have a long history of seizures before diagnosis"
appears **zero** times in the chapter it is attributed to; PMC10475770, cited
for "transformation is not associated with worse outcomes in
oligodendrogliomas", is about transformation *patterns* and says nothing of the
kind; and "no inherited syndrome is characteristically associated with
oligodendroglioma" — which the dossier flags as *"say this, because it is
reassuring and true"* — has no citation anywhere. **That parenthetical is the
tell.** A dossier note arguing for a claim on the grounds that it is reassuring
is the §12.12 direction with the reasoning left visible.

**And the claim that was overstated by one word.** The source says 1p/19q allows
"prediction of the best drug **response**" — how well the tumor responds. The
draft rendered that as predicting "**which drugs** it will respond to best",
which is drug *selection*, under a heading naming PCV. The source that actually
makes the PCV-specific claim is the one this page dropped as unreachable, so
that was WI-510's rule live on the page: the citation went and the claim stayed.
It also contradicted the section directly beneath it, which presents PCV versus
temozolomide as genuinely open. **Two claims that cannot both be true, three
screens apart, is the shape to look for when a page argues for a treatment.**

**A superlative is not a comparative.** "Relative chemosensitivity and indolent
clinical course **among** diffuse gliomas" became "the slower-moving of them and
the one that responds best" — a first-place ranking on two axes that no source
ranks, inside the outlook gate, where the reader chose to be. The
`Characterisations` ban list did not fire because it held `"responds better"`
and `"responds well"` but not the superlative. It does now.

### 12.14 Banning a citation is not fixing a claim (WI-518)

WI-518's test bans three URLs by name, each with a comment explaining that the
paper does not say what the dossier claims. **And the page shipped two of those
claims in its prose anyway** — steroids working "quickly" (the only source is
the steroid-complications audit WI-514 already rejected) and "it usually comes
back at or near the same place" (the only source is the 21-patient study WI-515
dropped).

This is WI-510's rule at one remove, and it is worse than the original because
the ban **provides false assurance**. A test that names a bad URL reads like the
claim was handled. **When you ban a source, grep the page for the claim it was
carrying.** Both of these survived a `/review` pass on the item that wrote the
ban.

**A proximity window is not a scope.** WI-518's retired-name guard checked for a
marker word ("older", "superseded", "used to") within ±220 characters of the
superseded name. The break harness planted the name in an unrelated symptom
bullet and it passed — 440 characters of a page whose whole subject is old names
will find a marker almost every time. Tightening it to the sentence plus the
next one *still* passed, off "especially common in **older** people" in the
following bullet. **The markers are ordinary English words, so any adjacency
allowance leaks.** It is same-sentence now, and the one legitimate use that
needed the allowance (a heading explained beneath it) was reworded to carry its
own marker instead. Same family as WI-511's clause anchor.

**A self-removing note goes stale in the direction nobody guards.** The
`/tumors/high-grade-glioma` honesty note said "those child pages are short at
the moment" and its guard required the note while *any* destination was thin.
Filling in three of four made the sentence false — a reader was told the
glioblastoma page was thin and pointed back to high-grade-glioma as "the fuller
one" — and nothing went red, because the guard had no upper bound tying the
note's *unscoped plural* to the *number* still thin. The note now names the one
page it is true of, and the guard requires the named destination to be one of
the thin ones.

**Check the source, not the dossier's quotation of it.** `/review` flagged this
page's shorter-radiation paragraph as an unsourced comparative, quoting the
dossier's "a safe, well-tolerated **alternative**". The paper itself says
hypofractionation "**is the preferred standard of care** for elderly (≥70 years)
or frail patients ... offering comparable overall survival", and EANO
independently says it "has **similar activity** to irradiation with 60 Gy in 30
fractions". The dossier had quoted a weaker sentence from the same paper. The
page was right; the finding was rejected with the verbatim recorded in the front
matter so the next reader does not re-open it.
