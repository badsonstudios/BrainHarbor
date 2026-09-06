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
