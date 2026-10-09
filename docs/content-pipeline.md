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
images:                       # WI-561 — one entry per picture in the body (§3b)
  - src: /img/figures/pathology-report-parts.svg
    credit: "Made for BrainHarbor"
    license: "CC0 1.0 (public domain)"
    license_url: https://creativecommons.org/publicdomain/zero/1.0/
    source_url: https://example.org/where-it-came-from   # omit for our own work
    width: 1200
    height: 675
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

### 3b. Images (WI-561)

A picture is one line of ordinary Markdown, **alone in its own paragraph**:

```markdown
![Three boxes in a row, joined by arrows.](/img/figures/steps.svg "What the lab does with the tissue, in order.")
```

The brackets are the **alt text** (what is in the picture, for someone who
cannot see it); the quoted title is the **caption** (what it means for the
reader). The credit and the licence live in the page's `images:` front matter,
keyed by the same `src`. It renders as a real `<figure>` with a
`<figcaption>` carrying the caption and then the credit.

Everything below **fails the page by name** the way a missing shared block
does, because every one of them renders perfectly and goes wrong silently:

- **Alt text is required.** An empty `alt=""` only with `decorative: true` in
  the entry, so it is a decision rather than an omission (WCAG AA).
  A decorative image carries no caption either — and still renders its credit.
- **A caption is required.** A figure with no caption is a picture a reader
  has to interpret alone.
- **The alt text and the caption may not be the same words**, and the alt text
  may not start by naming the medium ("Photo of…"): a screen reader has
  already said "image".
- **`credit`, `license`, `license_url`, `width` and `height` are required.**
  The pixel size is what lets the browser hold the space open — figures below
  the first are lazy-loaded, and one arriving without a reserved box moves the
  line the reader is on.
- **The file is served from this site.** A remote `src` tells another server
  who is reading the page, and rots on their schedule.
- **Never an NCI embedded image**, and nothing shipped with the AHFS or
  MedlinePlus drug monographs (PLAN.md §5). Enforced on the image's own
  `source_url`, not on the page's citations: cancer.gov's *text* is cited all
  over the corpus and only its *pictures* are barred.
- **A declared image no line shows, or a file in `wwwroot/img/figures/` no page
  declares, fails.** Both directions — and the declared `width`/`height` are
  checked against the file itself.
- **Anything that reads as image syntax and did not parse as an image fails**,
  wherever on the line it is. The usual cause is a double quote inside the
  caption; write the caption in single quotes. Unaudited, that line renders as
  literal bracket text to a patient.
- **A figure is a top-level block**, or one inside a `:::outlook` gate. Not a
  bullet, a quote or a table cell: those render a figure in a shape nothing
  styled or measured, and a 1200px picture in a table cell takes a 390px
  screen sideways.
- **One declaration, one appearance.** A page may not show the same picture
  twice.

**The caption and the alt text are graded like any other sentence** (§5, 6.0
limit) — the caption is prose the reader reads, and the alt text is prose a
screen reader reads aloud.

**A shared block may not carry an image.** A block's front matter carries only
`sources`, so the credit would have to be declared by a page whose author
never wrote the picture. It fails at parse time, by name.

**Which pictures each page wants, and where each one goes, is
`docs/images-needed.md`** (WI-562, §12.34): 464 slots over 50 pages resolving to 148
figures, with the draft alt text and caption for each already graded at the 6.0 limit.
It chooses no pictures. Two consequences of §3b are recorded there rather than here
because they are facts about the inventory: the words a shared block contributes still
earn a slot, hosted by each INCLUDING page at the directive line; and **no slot is
placed inside an `:::outlook` gate on any page**, though the rules above permit one.

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
| Images (WI-561) | `ContentFigures`, via the page parse | alt text, caption, credit, licence, pixel size — all required; the caption and the alt text are graded at the same ≤ 6.0 (§3b) |

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
  weakness in the big muscles of the thighs and upper arms, more likely the
  higher the dose and the longer the course (**this example used to read "in
  ~28%", and WI-524 found that figure is not in the paper the dossier cites it
  to; see §12.8**); SMA syndrome takes speech and one-sided
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
  working form is `(?:\bnot|\bnever|\bhardly|\bno|n't|\bfar from)\b[^.,;:]{0,20}$` — with
  **the word boundary INSIDE the alternation, which WI-571's `/review` round 18 had to correct
  here and in four guards.** A leading `\b` binds the whole group, and there is no word boundary
  inside *isn't*, so `n't` can never match: in a POSITIVE assert like these that does not weaken
  the rule, **it makes it FALSE-FAIL a correct page that negates with a contraction** — which is
  the §12.8 failure this very paragraph is about. This sentence is the one a future item greps
  for, so it is the copy that mattered most and the one the first sweep never looked at. Close
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

**Ten more from the twelfth one (WI-523, awake craniotomy — the page whose
spine is true of one step and false of the operation):**

- **A sentence that is true of a step is false of the procedure, and one word
  can move it across.** "You may lose a word for a moment, and it comes back —
  that is the test working" is true of the electrical test (bursts under four
  seconds, "transient disruption ... as expected"). It is false of a change that
  shows up while tumor is being removed, which may be permanent and is the
  signal to stop. The first draft's step 7 said the team "keeps **testing** you"
  during removal, so the spine's own scope word now covered the phase where it
  is false — and the guard accepted any sentence containing "the test". The fix
  is in both places: the removal step says outright that a change then "is not
  the test. It may not pass", and the guard scopes a return claim only by words
  that belong to the test alone (the current, a spot).
- **The front matter can claim what the page does not do.** This page's source
  notes said it "says separately that a change while tumor is coming out is the
  signal to stop, which is a different thing". The page said the first half and
  never the second. Front-matter rulings are checked against the page, not only
  against the sources.
- **Read a meta-analysis's Implications section, not just its Limitations.**
  The first draft used PMC12941676's limitations (observational, selection bias,
  one small trial the other way) to tell a reader they had "not missed out" if
  awake surgery was not offered. The same paper concludes it "should be strongly
  considered" for high-grade gliomas near eloquent areas. WI-512's shape — a
  source cited for the position it argues against — aimed at the reader it
  matters most to: a speech-area tumor at a low-volume hospital. The page now
  prints the suggestion, why it is unsettled, and the reviewers' own conclusion,
  and tells that reader it is fair to ask.
- **R2's silence is for spreads, not for small numbers the sources agree on.**
  The seizure rate runs 2.9% to 54% across five sources, so no number and a
  sentence saying why. The failed-awake rate is about 2%, 0-6% and 6.4% — they
  agree — and the first draft's "none of them has a number, the studies disagree"
  covered it too, which was false and left the reader to supply a worse figure
  (WI-521). It now says "This is uncommon", and the no-number sentence sits
  inside the seizure paragraph it is true of.
- **Every claim-shaped guard needs a canary.** The frequency guard was two C#
  string literals joined with `+`; the first was `$@"..."` and the second
  `@"..."`, so `{CountWord}` in the second half was literal text and that half
  could never fire. It passed this page and the page before it. It was found
  only by adding `Assert.Matches(guard, "It happens to about one in ten
  people.")` — a known-bad sentence the guard must catch before its pass on the
  page means anything. (§12.10's "prove it can fire", at the regex level.)
- **The shell heredoc rule is about backslashes, not about mutation tables.**
  An edit to the test file made through a bash heredoc turned twelve `\b`s into
  backspace characters (0x08). They are invisible to grep and to the eye, the
  file compiled, and four regexes silently stopped matching word boundaries. Any
  edit whose text contains a backslash goes through the Write or Edit tool, or a
  script file written with them.
- **A window around a story is not a pronoun guard.** The first guard checked a
  story's sentence and the next; /review put "she" two sentences in. NBTS
  genders two of its five people and not the other three, and tracking which is
  which is where the guess gets in (WI-510, WI-511). The page now writes all five
  neutrally and the guard is page-wide.
- **A glossary alias is site-wide, and "site" includes machine-written text.**
  The `brain mapping` entry first carried the alias `mapping` so it would fire on
  `/treatments/craniotomy`. `SummaryRenderer` runs the glossary marker over the
  AI feed summaries too, so any research item "mapping tumor cells" would have
  got a brain-surgery definition. Aliases must be unambiguous in research
  English, not only in the corpus; the craniotomy page now says "brain mapping".
- **Writing the canonical page found one claim at two strengths on a sibling.**
  `/treatments/craniotomy` said the pre-op scans "map where those areas sit";
  this page says a scan "can suggest" and the test in the room checks it
  (RadiologyInfo: further tests "to confirm the results of fMRI" before brain
  surgery). The sibling now says "sometimes ... to suggest". WI-521's rule again:
  budget an edit to the siblings.
- **An efficacy claim with moderate evidence gets a positional allowance, not a
  ban.** "Fewer new problems afterwards if they are awake" is moderate-certainty
  in the meta-analysis, so banning it would misstate the evidence the other way.
  The guard allows an efficacy phrase in exactly one place: a sentence beginning
  "Some studies suggest", immediately followed by "But most of those studies
  cannot settle it." Anywhere else — "the test lowers that risk", "being awake
  makes that much less likely" — it fails.

**The end-to-end read, eleventh item running,** caught eight defects before
review saw the page: a hospital stay called "much the same" as an asleep
operation when both sources that compare them say shorter (the sentence was
written to avoid printing a comparison, and made the opposite one); "there is no
reason to ask for it"; "three answers, not two", which assumed the reader had
read another page; "most surgeons only use it when really needed", which was one
surgeon; "most of the fear is the idea of feeling the operation", which was one
patient; and three guarantees about what the team does. `/review` then found two
blockers and twelve should-fixes, and fifteen mutations that walked the first
suite — all fifteen are now in the harness table.

**Nine more from the thirteenth one (WI-524, steroids — the page whose headline
figure was not in the paper it was cited to):**

- **A guard scoped to the section named for the defect is a guard with a door
  next to it.** The no-frequency rule for steroid myopathy was checked inside
  "The weakness in your legs and arms"; `/review` put *"Proximal weakness turns
  up in about twenty-eight in every hundred people on a high dose"* into "The
  rest of the side effects", two screens down, and every test stayed green. A
  page-wide guard then fires on the page's own legitimate sentences, and that is
  the point: the exemptions become a **short, named allowlist** (five entries
  here, each with the source quote that earns it) rather than a pattern hole
  anybody can walk through. Four of the eight sentences it flagged were genuine
  defects — *"almost everybody"*, *"most people get some of these"*, *"plenty of
  people"*, *"many people are given"* — none of them sourced, none of them
  visible to a section-scoped check.
- **`ReaderText` strips the front matter, so every body-scoped guard is blind to
  the title and the description — and `ContentPage.cshtml` renders the
  description as the first paragraph under the heading.** This page's
  description promised *"why it has to come down slowly"*, which is the exact
  claim the page's own taper section exists to correct, and its title promised
  *"when it IS the drug and not the tumor"*, the verdict grammar this item
  deleted from a sibling. Both shipped past 24 tests. **Assert over
  `title + description` as well as over the body**, with the same regexes.
- **A front-matter reader must tolerate CRLF.** `^title: "(.+)"$` does not match
  `"\r\n`, so on a Windows checkout the headline guards ran over an empty string
  and passed. The break harness found it by reporting a mutation as correctly
  failing on CRLF while it walked through green on LF — **a break that fails on
  only one line ending is a bug report about the test, not about the page**.
- **Check the POLARITY of the sentence, not the presence of a safe word.** The
  dose-change guard asked whether a matched sentence contained "your team", and
  `/review` beat it with *"it is reasonable to halve your dose until your team
  can see you"*. A permission sentence naming the team is still permission. The
  working form asserts a prohibition marker **and** the absence of a permission
  marker (`it is fine|reasonable|you can|if you feel ready`).
- **Assert the match COUNT before iterating over matches.** Two guards here ran
  their scope checks over zero matches — the page writes "stop **a** steroid"
  where the regex demanded "stop **your** steroid" — so the property each was
  named for was never checked at all. §12.10 records this for a page with no
  matches; the harder version is a page whose correct prose the regex cannot
  see. `Assert.True(matches.Count >= n)` with a message saying what it means.
- **A tier list invented for one page contradicts the corpus by omission.** ABTA
  gives its eight steroid warnings one undifferentiated "contact your doctor";
  this page sorted them into three tiers and filed **a fall** in the one that
  does not need calling today, while `/treatments/craniotomy` and `/tests/biopsy`
  file it as same-day and `/treatments/chemotherapy` files it higher still — 50
  lines under this page's own "a steroid makes bones easier to break". **When a
  source gives one flat tier and the corpus already sorts the symptom, follow
  the corpus.** The same list dropped four of `[ESCALATION]`'s seven same-day
  symptoms; a reverse check is a **bullet count plus the named concepts**
  (§12.8, WI-519), and this page's did not exist until review asked for it.
- **A scope sentence governs the list it introduces, so check it against the
  worst item in that list.** *"Nearly all of them fade after the steroid stops"*
  sat above bullets naming thin skin, weaker bones and hip pain, none of which
  fade. §12.12's over-reassuring direction, produced by a sentence that is true
  of the first four bullets.
- **Print the speed, not the share, when the shares disagree.** The first draft
  published one guideline's "three quarters improve" — the reassuring number, of
  two that disagree (the page's other source reports 50% in its own cohort), and
  widened past its population (people with raised pressure from a primary brain
  tumor) to everyone on a steroid. The page now says most people feel better
  within hours, that not everybody improves, and that the counts disagree.
- **A rule that is true of a long course can be false of a three-day one, and
  the sibling will be carrying the false version.** "A steroid must never be
  stopped suddenly" is right for the reader on it for months and wrong for the
  post-operative reader, whose fast taper is "discontinued 3 days after
  resection" in this page's own guideline. The rule that holds in both cases is
  **"not on your own"**. `/treatments/craniotomy` said the absolute in two
  places, four lines above the new door to this page, and its own tests pinned
  the wording — so correcting a claim here meant correcting a sibling's prose
  **and** repointing that sibling's tests at the property rather than the words.

**The end-to-end read, twelfth item running,** found the thing no gate could:
the page said the steroid is tapered and never said **why** stopping suddenly is
dangerous, which is the item's own requirement and the one fact that makes the
rule stick. `/review` then found five blockers and eleven should-fixes, and
walked **fourteen of eighteen** mutations through green; all eighteen are in the
harness table, which stands at 72 breaks on LF and CRLF.

**Ten more from the fourteenth one (WI-525, anti-seizure medicines — the page
whose hardest job was holding two halves of one guideline apart):**

- **A guideline can contain its own scope trap, and the page has to carry both
  halves or it accuses somebody's team.** PMC8563323 gives **Level A** for "newly
  diagnosed brain tumors who have not had a seizure ... should not prescribe
  AEDs", and, three recommendations later, **Level C, insufficient evidence**
  for the peri- and postoperative period. A page printing only the first tells
  every reader handed levetiracetam for their craniotomy that their team defied
  the strongest grade of advice there is. §12.12's shape, except that both
  halves are in the same document — so **read the whole recommendation list,
  not the one the backlog quoted.** The same guideline's short-course rule is
  scoped the same way ("treated surgically **who have not had a seizure**"), and
  those two readers are indistinguishable from where they stand, which is why
  the page hands them a question instead of an answer.
- **The backlog's headline claim CAN be right, and checking it is still the
  work.** WI-521, WI-522 and WI-524 each found theirs defective, which is a
  pattern that starts to feel like a rule. This one was verbatim. What the check
  bought was not a correction but the guideline's own *reason* — side effects,
  interactions, no measurable benefit — which is what turns a refusal into an
  explanation on a page whose second spine is a reader who feels denied
  something.
- **A test can be satisfied entirely by the front matter.** `Everything` is
  `title + description + body`, which WI-524 added so the headline could not
  ship the defect. One item later a count over `Everything` was the only thing
  asserting the page's load-bearing safety rule, and `/review` enumerated its
  two hits: the title string, and the short version. The phrase **"on your own"
  appeared nowhere in the body at all.** Widen the corpus a guard reads and the
  guard gets easier to satisfy — so **assert the property against `Body`, and
  use `Everything` only for the bans.**
- **Check the AGENT, not the modal.** WI-524's lesson was that a permission
  sentence naming the team is still permission. The harness walked the
  successor guard, on both line endings, with *"When you lower the dose or try a
  different one, the mood change usually lifts"* — which contains no modal at
  all. A sentence does not need a permission word to be permission; it only
  needs the reader as the subject of the verb.
- **Two closed lists of eight words is not a guard.** `/review` beat the
  dose-permission check four separate ways — *"it is **safe** to skip"*, *"**hold**
  tonight's dose"* (a verb that was not listed, so the polarity half never ran),
  *"you are **allowed to** miss one"*, *"it is **fair to** leave the next one"* —
  and beat the driving check four more, including two that published a waiting
  time. The fix is to invert the rule: instead of hunting for bad words, require
  every sentence in scope to **earn its place** (it forbids the thing, or it
  hands it to somebody who is not the reader). Shapes nobody has thought of then
  fail by default instead of passing by default.
- **A membership check is not a tier check.** The escalation guard read the
  ambulance and same-day lists and asserted which bullets were in them. `/review`
  kept every bullet where it was and softened the **instruction** attached to
  one — *"This is rare, and it can usually wait until the morning"* on a
  blistering rash, *"There is no rush about this one"* on the mood change, a
  hedge inside the five-minute rule — and all of it stayed green. **Assert the
  absence of downgrade language inside each tier**, not just the presence of the
  bullet.
- **A reverse check by bullet COUNT rewards a downgrade.** Moving "trouble
  breathing" out of the ambulance tier and into the same-day tier as its own
  bullet took the same-day count from seven to eight, and the count check read
  that as healthy. Count plus named concepts is §12.8's own WI-519 rule; the
  missing half is that the concepts have to be named **per tier**.
- **WI-524's "guard with a door next to it" was re-committed one item later, in
  its purest form.** This page's tier guard read
  `Content/blocks/escalation.md`, asserted that *the block* files new weakness
  and new confusion as same-day, and never looked at the page — under a comment
  saying "This page could have quietly filed them lower". It had. The page
  carried neither symptom in any tier, nor the corpus's sudden-deficit ambulance
  line, and the test was green. **Read the sibling to learn where the corpus
  files a symptom; then assert THIS page files it there.**
- **Naming the safe drug and leaving the risky class anonymous is the wrong way
  round.** §12.4 R1 bans a multiplier, a milligram and a drug-by-drug table. It
  does not ban a drug's name. The page named levetiracetam and dexamethasone
  and then described the enzyme-inducing drugs as "a few of the long-established
  ones", so a reader on carbamazepine could not tell whether the paragraph was
  about them. The source names all three.
- **Write down what is on the page with no fetched source.** `/review` asked
  where the six-step "what happens" spine came from, and the honest answer was
  ordinary clinical practice. On an item that found three dossier citation
  defects, an unsourced section that *looks* sourced is the same failure one
  layer up. It is now named in the front matter, and it is written as shape
  rather than instruction: no dose, no interval, no duration.

**The end-to-end read, thirteenth item running,** found eight more, and the worst
was an unsourced **"It is not common"** attached to the sentence about thoughts
of self-harm — the over-reassuring direction (§12.12) on the single most
dangerous claim on the page, invisible to a frequency guard that has no word for
"not common". It also caught the page's own caregiver paragraph restating the
`[CAREGIVER]` block ten lines below it in different words (the shingle check
cannot see a restatement that shares no eight-word run), and a British "a
fortnight ago" in a quoted family sentence. `/review` then returned four
blockers, thirteen should-fixes and **twenty-six proven guard walk-throughs**,
eighteen of them verified by executing the regex rather than reading it. The
harness table stands at **117 breaks on LF and CRLF**.

**And the heredoc rule failed for a fourth consecutive item, inside the item
that wrote it down.** Two `\b`s became backspace characters (0x08) in the new
test file, invisible to grep and to the eye, and the canary caught it because
the regex stopped matching word boundaries. The rule is not "avoid heredocs for
mutation tables" — it is **any edit whose text contains a backslash goes through
Write/Edit or a script file written with them**, and it applies to one-line
`python - <<` invocations exactly as much as to a multi-line table.

**Nine more from the fifteenth one (WI-526, clinical trials — the page that had
to explain a subject another page already half-explained):**

- **When a Razor page already covers the subject, write down the split before
  writing a word.** `/trials` does the FINDING (a ZIP search, a country filter,
  a registry list) and explains the subject in three sentences. The curated
  corpus mentioned trials **sixteen times across nine pages and explained them
  nowhere** — six were a bullet in an options list, five a question in a
  questions list. So the split is *what exists and where* against *should I be
  thinking about this at all*. Write it into the front matter: the next editor
  will otherwise restate the finder, and **no site-wide rule can stop them**,
  because a Razor page is outside `CuratedPage.AllPages()` and every corpus-wide
  guard is blind to it. The restatement check has to read it explicitly, and
  the harness proved that: lifting the finder's opening paragraph verbatim
  walked through on both line endings.
- **A one-sentence correction can over-shoot into a different contradiction.**
  The short version first said the earliest trial window is "before any
  treatment starts", which is false because surgery is a treatment. The
  end-to-end read caught it; the fix said "before anything at all has been
  done", which `/review` then caught contradicting the page's own "a confirmed
  diagnosis **from tissue**" eight screens later — a diagnosis *is* the output
  of surgery, and `/treatments/watch-and-wait` already records that a biopsy
  counts as surgery. Two passes, two wrong answers, because both were written
  against the sentence rather than against the page. The third says "before the
  operation that takes the tumor out", and the prerequisites list now names
  which windows it is true of.
- **Emphasis markers and line wraps defeat a phrase guard, and that is
  structural rather than lexical.** `/review` beat the matching guard with
  *"you **do**\nqualify"* — four asterisks and a wrap between two words every
  second-person pattern needs adjacent. No amount of vocabulary fixes that.
  Every phrase-shaped guard now runs over text with `[*_]` stripped, which is
  the same argument §12.8 (WI-509) makes for asserting against reader text
  rather than raw source: emphasis is authoring markup and a reader never meets
  it.
- **A pattern escape is a door; a short named allowlist is not.** The matching
  guard exempted any sentence containing screening language, so that a correct
  sentence ("Tests to check you fit what the trial is looking for") would pass.
  `/review` put the exemption word and the defect in ONE sentence: *"the trials
  **looking for** that are the ones **you fit**"*. The working form bans the
  assertion unconditionally and names the one legitimate sentence. WI-524 said
  this about section scopes; it is equally true of vocabulary escapes.
- **A membership check is not a tier check.** The escalation guard banned four
  phrases and counted the composed page's one legitimate "call an ambulance".
  `/review` inserted a complete page-local tier list — an urgent lead-in plus
  *"A fever. / New weakness. / A seizure."* — and everything stayed green. On a
  trials page that is the worst available outcome: `/treatments/chemotherapy`
  owns the fever threshold and the hubs file new weakness as same-day, so a
  reader told to call the trial team first has been routed away from the page
  with the number. The replacement is **structural**: walk the raw page and fail
  on an urgency-flavoured line followed by two or more symptom bullets. Shape,
  not vocabulary.
- **Pinning a sentence's first clause leaves its tail free.** The guard held
  `Three of those five doors are open early` and `/review` replaced the rest
  with *"and **none of them** shut once treatment begins"* — flatly false, and
  over-reassuring. It also wrote *"two of those **five** shut"*, which is wrong
  arithmetic, and the numbers guard's allowlist entry then exempted the sentence
  from the count check as well. **Pin the whole sentence, end-anchored**, and
  make the allowlist entry long enough not to exempt its own replacement.
- **Count list items by marker, not by numeral.** The five-windows check used
  `\d\. `, so a sixth window added as a `-` bullet, or as prose, left "five
  points" and "three of those five" both passing. Count
  `^\s*(?:\d+\.|[-*]) ` over the raw section and assert the exact number.
- **A superlative is not a comparative — eight items after §12.13 said so.**
  The phase guard held `safer|riskier` and missed *"Phase 1 trials are the
  **riskiest**, and phase 3 **the one to hope for**"*. And a vocabulary ban
  cannot see a paraphrase: the NBTS selling line this page exists to refuse
  ("early access to potentially beneficial treatments") came back as *"put a
  treatment in your hands years before it reaches the ordinary clinic"*, past a
  substring list that banned the literal phrase. On a hype-prone subject the
  ban has to be **claim-shaped** — no access frame, no exclusivity frame, no
  better-than-standard-care frame — not a word list.
- **A door needs to be near the sentence it was appended to, or the guard
  proves nothing.** `TheDoorsOnTheSiblingPagesAreAppendedSentences` asserts the
  door AND the sibling's pre-existing sentence, which is §12.14's answer to a
  door that replaced a paragraph. On `/tumors/low-grade-glioma` the pinned
  sentence was 190 lines from the door, so replacing the door bullet outright
  stayed green; on `/treatments/chemotherapy` the pin sat three sentences in
  front of the replaced text and survived it. Assert the **distance** between
  the two, and take the nearest door when a page has more than one.

**The end-to-end read, fourteenth item running,** found nine, and two are worth
naming. **Three separate sections each told the reader they were the most
important one** — "the single most useful thing on this page", "This matters
more than anything else on this page", "This is the part most worth
understanding" — all three in one draft, which is §12.8 (WI-509)'s uniqueness
claim going stale before the page ever shipped. And **the caregiver paragraph
restated the `[CAREGIVER]` block in different words ten lines below it**, which
is the lesson WI-525 wrote down, repeated at the next opportunity: the shingle
check cannot see a restatement that shares no eight-word run, so the page's own
caregiver claims have to be pinned by name.

`/review` returned four blockers, thirteen should-fixes and **forty-seven proven
guard walk-throughs**, eighteen verified by executing the regex. The harness
table stands at **115 breaks on LF and CRLF**.

**Eleven more from the sixteenth one (WI-527, meningioma — the first §12.3
tumor hub outside the glioma family, and the item where the harness did the
reviewing):**

- **A 98-mutation table found THIRTY-FIVE guards that could not fail**, after
  `/review` had already returned sixty-six walk-throughs and been answered.
  Identically on LF and CRLF, so not a flake. This is the strongest argument the
  project has for the harness being non-optional: `/review` reads the guards and
  reasons about them; the harness *runs* them, and the two disagree by a third
  of the table. Build the table against `/review`'s own walk-throughs and then
  keep going.
- **A substring allowlist exempts the whole sentence, which is a free ride for
  everything else in it.** `Most meningiomas are grade 1 and grow slowly` was
  allowlisted; the harness appended *"and almost all people who have one die of
  something else"* and the sentence was exempt. Same trick landed a percentage
  on the end of the page's own refusal to print one. **Redact the allowed phrase
  and match the remainder** — and prove the redaction with a canary, because a
  redaction that removes too much is the same defect wearing the opposite coat.
- **A quantifier guard that knows halves, thirds and quarters knows almost no
  fractions.** "Roughly a fifth", "a twentieth", "a tenth", "the large
  majority", "a tiny minority", "a handful in every hundred", "about half the
  time" — seven shares, none of them a percentage, all of them through. A share
  does not need the word "of", either: *"half **the** people who have the
  condition"* is the commonest phrasing there is.
- **Adjacency is a one-word door.** `most (people|patients|tumors)` misses "most
  **grade 2** tumors". Allow a two-or-three-word gap in every quantifier guard.
- **Ban the threshold shape, not the unit.** "Fifty-four grays in thirty visits"
  walked past a dose guard holding `\d+ Gy`: the spelled compound is not a count
  word and `gray\b` does not match the plural. "Under twenty millimetres", "at
  least three dividing cells", "more than one tablet a day" and *"about the size
  of a walnut or smaller"* all went the same way. One branch —
  `(more than|less than|at least|up to|under|over|above|below) <count>` — kills
  four of them, and taking the unit out of a size threshold does not stop it
  being one.
- **An inflection is a door.** `\bgrade\s+(I{1,3}V?|IV)\b` misses "gradED I, II
  and III" and "gradES I to III". `grade[sd]?`. Five copies of this guard in the
  corpus were case-sensitive until WI-516; this is the same rule failing on the
  other end of the word.
- **A section-scoped guard proves nothing about the rest of the page —
  WI-524's rule, applied to a guard that had not been given it.** The eleven-year
  surveillance endpoint is pinned, scoped and canaried inside the follow-up
  section; the harness put it UNSCOPED in the short version and every assertion
  passed, because every assertion was reading a different section. The fix is a
  containment check: find the claim page-wide, assert every instance lives in the
  one section that scopes it.
- **A digit ban is not a prognosis ban.** Inside the `:::outlook` gate, every
  digit is required to be a grade — and *"people in that group live as long as
  anybody else"* and *"almost everybody in that group is done with it"* contain
  no digit. §12.5's gate is consent to read, not a licence to publish, and the
  ban has to cover the claim in words.
- **A contract item no test names is not a contract item.** `[MECHANISM]` and
  `[CROSSWALK]` were composed into the page and listed in no assertion; deleting
  the mechanism block and its heading broke nothing.
- **Count the section's own claims when a paraphrase is the failure mode.** The
  caregiver guard says in its own comment that no shingle check can see a
  paraphrase — and then let one through, because it pinned three claims without
  asserting there were only three. The section promises "three things"; assert
  **three**.
- **A ban list made of clinical nouns misses the reader's own words.** The
  shared `[ESCALATION]` block is checked for "spinal cord", "bladder", "bowel";
  the harness added *"losing control of your water, or numbness in the saddle
  area"* to the block file and every hub in the corpus silently acquired a
  spinal line. `AssertEscalationTiers` cannot see it either — it diffs the block
  against siblings, and they all read the same file.

**The end-to-end read, fifteenth item running,** found two, and both are the
kind only a person meets. **An ambiguous pronoun in the reassuring direction:**
"Growth tends to slow down after the first couple of years, and after several
more **it** becomes very small" — *it* is the growth rate, but the nearest noun
a reader is tracking is their tumor, and the sentence sits under "the long-term
studies of tumors like that are reassuring". Read straight through, it says a
watched meningioma shrinks. It does not. §12.12's more dangerous direction, and
no guard in the suite can see a pronoun. The sentence now names its subject and
closes the wrong reading by hand ("That is the growth slowing, not the tumor
shrinking"). And **an unsourced majority claim survived every guard by being
allowlisted for one**: "Most watched meningiomas grow a little" had no ruling in
the front matter, sat in a section that already routes to the sibling carrying
the study where MORE people ended up treated than not, and was on this page's
own allowlist so that its share guard would pass. The action survived the edit;
the share did not. **If a claim needs an allowlist entry, ask what it is doing
on the page before you write the entry.**

The harness table stands at **98 breaks on LF and CRLF**, and the guard count
went from 63 that could fail to 98.

**Twelve more from the seventeenth one (WI-528, brain metastases — the hub where
two of the shared blocks are WRONG rather than merely unnecessary):**

- **A SHARED BLOCK CAN BE FALSE ON A HUB, not just unnecessary, and that is a
  different problem with a different fix.** `[CAUSES]` opens *"For most brain
  tumors, nobody knows the cause."* On a metastasis hub the cause is the cancer
  in the reader's own chart, so the block would hand a frightened reader a lie
  in a soothing voice. `[CROSSWALK]` fails the same way for a different reason:
  it explains the 2021 rules for naming tumors that START in the brain, and this
  reader's report follows their first cancer's rules. **Neither block was
  edited** — doing that would push this page's answer onto six glioma hubs
  (§12.10, the WI-514 blast radius) — and the page writes its own section. The
  test proves the exclusion by asserting the block still says what it says, and
  by banning the claim SHAPE in the page's own voice, because `/review` wrote
  *"Nobody can say why the cancer chose your brain"* past a shingle check and a
  literal-string render check in one line.
- **Ask what the block was carrying for the reader before you drop it.**
  `[CAUSES]` also answers *"could I have passed this on?"*, and on this hub that
  question has a **different and better** answer — a few cancers do run in
  families, and there is a referral behind it. Excluding a block is a decision
  about a paragraph; the QUESTIONS it answered still have to go somewhere.
- **Thirteen through nineteen were missing from every copy of `CountWord` in
  the corpus.** `one…twelve, twenty, thirty…`. The figure WI-528 exists to
  refuse is **nineteen** percent, and `/review` published it in one line.
  `dozen` and `couple` went in at the same time.
- **`leads` is not `lead`.** The guard banning "your original oncologist still
  leads" was beaten by *"takes the lead"* — the same claim, one letter shorter —
  and by *"your CANCER doctor stays the one who decides"*, because the second
  branch demanded the noun sit next to "your". One missing inflection, one
  missing word gap, and the refused claim is publishable.
- **A drug NAME LIST is not a drug ban** (§12.14 again, applied to drugs). The
  guard held thirteen hard-coded names and `/review` walked "entrectinib"
  through it. Targeted cancer drugs are named by **suffix convention**, so the
  shape is bannable: `\w{4,}(?:tinib|ciclib|parib|zomib|nib|mab)`.
- **An allowance of one is an allowance of one MORE, when the page's own
  sentence is not in the banned shape.** The uniqueness guard asserted
  `claims.Count <= 1` — but the sentence the page keeps says what a section
  *contains*, not that it beats the others, so it matched nothing and the
  harness spent the free slot twice. Ban the shape at **zero** and pin the
  legitimate sentence separately.
- **`CuratedPage.Section` flattens.** Anything that needs to see a PARAGRAPH —
  counting a section's own claims, for instance — has to cut the markdown
  itself. And count paragraphs rather than bold runs: `/review` added a fourth
  caregiver claim with no bold on it and the count stayed at three, which is
  WI-527's lesson defeated by deleting the markup the count was reading.
- **`List.IndexOf` returns -1, and `-1 < everything`.** Three section-order
  assertions were `>`, so deleting a whole section — and with it every route to
  the seizure pages and the driving authority — left the order test green.
  **Assert every slot is PRESENT by name first**, then assert the order.
- **A floor is not a count.** `leads.Count >= 5` let a sixth treatment option in
  that contradicted the page's own medicine section. And count
  **paragraph-leading** bold (`(?m)^\*\*`), because emphasis inside a paragraph
  is not a list item.
- **A guard scoped to `Body` cannot see the description, and the description is
  the first paragraph the reader meets.** The British-form scan ran on `Body`,
  so `description: "...a tumour that has spread..."` was green — on a page whose
  own `Headline` helper exists because of exactly that (WI-524). Phrase guards
  run on `Plain`; only directive checks want `Body`.
- **A dead-domain list cannot express §12.1 when the page must cite the domain.**
  This page cites `cancer.gov` for its framing sentence, so the banned-domain
  loop structurally cannot hold "never NCI patient PDQ on a tumor hub".
  `/review` added the adult-brain PDQ URL and every check passed. The rule needs
  its own pattern: `cancer\.gov/[^\s"]*(?:/patient/|-pdq\b)`.
- **The harness cannot see a test that is broken on CRLF.** It asks only whether
  a mutated page FAILS the named test, and a test whose regex cannot match a
  CRLF checkout fails for free — so it reports `ok` for every mutation pointed
  at it. WI-527's meningioma outlook guard shipped that way
  (`^## heading\s*\n\n:::outlook` cannot match `\r\n\r\n` at all: greedy `\s*`
  eats both breaks and every shorter split leaves a `\r` where a `\n` is
  required). **`\r?\n` everywhere, never a bare `\n` and never `\s*` standing in
  for a blank line** — and the only thing that catches a violation is running the
  suite on a CRLF working copy.

**The end-to-end read, sixteenth item running,** found five, and two of them
could not have come from anywhere else. **A glossary tooltip contradicted the
section it rendered inside**: `neuro-oncologist` said *"They often lead the
team"*, three lines from the section that refuses to say who leads, and the
markdown contains neither sentence. **And `whole-brain-radiation` said it is
used "when there is more than one tumor"** while the page said "too many to aim
at one at a time" — one claim, two strengths, and the page carrying it was the
one written for readers with several. Both entries were corrected to be true
everywhere rather than patched for this hub. Also: *"the next section maps each
part of the brain"* pointed two sections early, because the map lives inside
`[MECHANISM]` and a composed block moves the thing it points at.

`/review` returned three blockers, sixteen should-fixes and **eighteen proven
guard walk-throughs**, every regex executed rather than eyeballed. The harness
table stands at **123 breaks on LF and CRLF**, and it found four more guards
that could not fail after all eighteen were answered.

**Thirteen more from the eighteenth one (WI-529, `all-brain-tumors` — the only
page in the corpus written for a reader with NO diagnosis, and the first NEW
page rather than a deepening):**

- **A SIBLING CAN ALREADY OWN THE ITEM'S HEADLINE CONTENT, and the research pack
  will not know.** The backlog hands WI-529 the seven-step pathway, and §C.4's
  turnaround-time table is the sentence *"this is, editorially, the most valuable
  section in the whole brief"*. `/tests/waiting-for-results` shipped all of it
  **twenty-two items earlier** — the lab queue, the durations, the UK national
  audit, the batching, the name changing — and `/tests/biopsy` owns the two
  operations. What was actually unowned is **the part before there is tissue**,
  steps 1 to 4, which is exactly where this reader is standing. Read the
  siblings before you read the dossier, or you will write the dossier.
- **A shared block can be ADDRESSED TO SOMEBODY ELSE, which is a third thing
  from unnecessary and from false.** `[CROSSWALK]` opens *"If your paperwork was
  written before that"* and closes *"Seeing an older name on your own report"*.
  Nothing in it is untrue here. **This reader is defined by not having a
  report** — contract item 3 exists because readers arrive holding old
  paperwork, and this is the one page whose reader does not. Excluded, block
  unedited, and the one half that IS this reader's (gene results are part of the
  name, which is why the name takes weeks) routed to the sibling that already
  says it.
- **And `[CAUSES]` was the OPPOSITE call from WI-528's, on the next item.**
  There its opening is false because the cause is in the reader's chart; here it
  is true and asserts nothing about this reader's scan. §12.10's test cuts both
  ways, and the answer is per hub, not per block.
- **A scoping note has to cover the block's FRAMING, not only its vocabulary.**
  The first note taught the reader to read "your tumor" as "whatever this turns
  out to be" — and left `blocks/mechanism.md`'s FIRST SENTENCE, *"Most people
  are told what they have long before anyone explains what it is doing"*, three
  lines under the page's own *"Nobody knows yet whether that is what you have"*.
  `/review` caught it. The note now names that sentence and says *"for you that
  is the other way round"*, and the guard requires both halves.
- **AN AUTHORING MARKER ON A DIRECTIVE LINE STOPS THE DIRECTIVE COMPOSING.**
  `!%tumor board%[TUMOR-BOARD]` was a page-local fix for a cosmetic problem, and
  it fails open: the composer matches a directive only on a line of its own, so
  the block silently stopped composing and the literal string `[TUMOR-BOARD]`
  rendered inside a `<p>` on the reader's screen. Put the marker at the end of a
  neighbouring sentence — it is page-wide, so the position is free. There is now
  a mutation for it.
- **Three live British spellings nobody had swept for, on four shipped files.**
  `travelled` (three times on `/tumors/brain-metastases`), `judgement`
  (`/treatments/anti-seizure-medicines`, `/tumors/meningioma`) and `labelled`
  (`/tests/waiting-for-results` and `glossary/amended-report.md`, which is a
  tooltip that fires site-wide). Every previous sweep looked at `-ise`, `-our`
  and idiom; **doubled consonants and `-ement` are a whole family nobody had
  asked about**, and none of the three is a substring of its US form, so they
  were safe to add.
- **A canary aimed at a front-matter COMMENT proves nothing.** The
  non-duplication canary for `/treatments/watch-and-wait`'s scan interval
  matched `every few months` — in the RULING that explains the sentence, not in
  the sentence, which is hard-wrapped between "few" and "months". `Sibling()`
  reads `ReaderText`, which strips front matter. The harness reported `ok`.
  **Aim a canary at the body, and check the wrap.**
- **A mutation BELOW a composed block does not move what the block's heading
  points at.** Inserting a `## ` after `[MECHANISM]` left the composed order
  unchanged, because the block brings its own `## ` with it. The mutation had to
  go above the directive. The mutation was too weak, not the test (WI-516).
- **THE HARNESS CANNOT MUTATE A RAZOR VIEW.** Views compile into the test
  assembly, the harness runs `dotnet test --no-build`, and a change to
  `Pages/Tumors.cshtml` is invisible to the running host — so it reported `ok`
  for a guard it had never exercised, which is the same shape as WI-528's
  broken-on-CRLF test. Prove those by hand: break, **rebuild**, confirm red,
  restore, and record the absence in the mutation table.
- **Widening a guard's SCOPE is not the same as widening its subject, and the
  wrong one fails a correct page.** The registry share ban was section-scoped;
  `/review` was right that "most brain tumors are not malignant" elsewhere would
  pass. Run page-wide it fired on ACS's own *"Most brain tumors are found
  because they start to cause something"*, which is correct and sourced. The fix
  is to narrow the SUBJECT — quantifiers within 50 characters of
  malignant/non-malignant/benign — and keep the full figure ban in its section.
- **The vocabulary §12.5 REQUIRES the gate to teach is a duration to any regex
  that reads it.** `five-year survival` is `five` plus `-year`, and it tripped
  the guard written to stop this page republishing the sibling's turnaround
  times. Redact the taught term and scan the remainder (§12.8, WI-527), with the
  canary that proves the redaction did not swallow the sentence.
- **A guard can be STRICTER INSIDE THE GATE THAN OUTSIDE IT, which is the wrong
  way round.** The worded prognosis claims were banned where the reader had
  consented and allowed where they had not, so *"some of the things on that list
  people live with for years"* passed everywhere on the page. Both halves run in
  both places; only the vocabulary is gate-only.
- **`(?m)^:::outlook[ \t]*$` cannot match a CRLF checkout** — WI-527's bug, in
  the file that records WI-527's bug, caught by `/review` before the page was
  ever tracked. `[ \t]` does not consume a `\r` and multiline `$` does not match
  before one. `\s*$`.

**The end-to-end read, seventeenth item running,** found three, all of them
duplication a shingle check cannot see because the wording differs. **The
caregiver slice repeated the block's own reassurance** — the block says telling
the team *"is not going behind their back"* and the page's third claim ended
*"that is not going behind anybody's back"*, twelve lines apart in one section.
**The glossary tooltip for `tumor board` printed the block's opening sentence
immediately above the block's opening sentence**, which is a corpus-wide problem
made unmissable here because this page gives the block a heading of its own.
And **"there is no screening test for them" was said twice**, three hundred
lines apart, once as a fact and once as absolution; the second now leans on the
absolution alone.

`/review` returned three blockers, fifteen should-fixes and a page of guard
walk-throughs. The harness table stands at **147 breaks on LF and CRLF**.

**Twelve more from the nineteenth one (WI-530, `/tests/ct-scan` and
`/tests/planning-scans` — two pages in one item, and the item where a source
page had been rewritten out from under its citations):**

- **A SOURCE CAN STOP SAYING WHAT IT WAS CITED FOR, AND NOTHING IN THE PROJECT
  WILL NOTICE.** The dossier's §2.2, §2.3 and §2.4 attribute **eight** claims to
  `radiologyinfo.org/en/info/headct` — the warmth and flushing, the metallic
  taste, the urge to urinate, "within 30 minutes", "less than a minute" for the
  injection, the kidney screening. Fetched in full with a browser user agent
  (51,913 bytes, "Last reviewed on June 15, 2026"), **that page has no "What
  will I experience" section at all.** It is not gated, not 403, not a
  JavaScript shell: it is a live page that has been restructured since the
  research was gathered. This is a different failure from every citation defect
  the project has recorded — the dossier was probably right when it was written.
  All eight claims are carried verbatim by ACS's own CT page and were moved
  there. **A URL that resolves is not a citation that holds. Re-fetch, and read
  what is there now.**
- **Two fetches through the summarizer can agree and both be incomplete.** Both
  returned "those sections are not present"; only the raw fetch proved it,
  because the summarizer cannot distinguish "the page does not say this" from
  "the converter dropped it". When a fetch reports an ABSENCE that a claim rests
  on, pull the bytes.
- **A page written for a reader who has already had the test needs a different
  first move.** Most `/tests/` pages are read before the appointment. The CT
  page is read after it, by somebody who was told there was something there, so
  the section it most needs is not "what happens" but **"the fast scan was the
  right scan"**. ACS says a CT "aren't quite as good as MRI scans", which is
  true and has to be printed; printed without "if a scan is needed right away",
  it tells an emergency-room reader their hospital chose the weaker test. Both
  halves, in the same section, with a test.
- **A radiation section must not end on the fear, and the pin is END-anchored.**
  RadiologyInfo's risk line is two sentences and the second is the answer: "the
  benefit of an accurate diagnosis far outweighs the risk". The order is
  asserted (fear stated, then answered) and the section's LAST sentence is
  pinned, because WI-521 showed a correctly-pinned reassurance surviving a
  sentence appended after it.
- **A reassurance can be an unsourced causation claim wearing a kind voice.** A
  draft closed the radiation section with *"Whatever you are carrying today, it
  is not that"* — which is a claim that the scan did not cause the tumor, for
  every reader, from no source. `/tumors/meningioma` makes that claim properly
  and only about imaging doses versus childhood radiotherapy. Replaced with the
  sourced half ("no radiation stays in your body afterward"), and there is now a
  page-wide ban on the causation shape, checked against the sibling's wording.
- **Where two sources disagree and BOTH are right, the page must say which
  question each is answering.** The manufacturers say do not breastfeed for a
  day or two after iodine contrast; the ACR says the amount reaching a baby is
  extremely low. `/tests/mri` already tells its reader it is safe to carry on
  after the MRI dye. Printing either CT answer alone reads as a correction of
  the sibling, so the page prints the disagreement, names both sources, says
  **"This is about the CT dye"**, and links. The guard bans resolving it in
  EITHER direction, and reads the sibling so a change there goes red here.
- **THE SCOPE LINE FOR A MERGED PAGE IS A TIME, NOT A TOPIC.** fMRI, DTI,
  spectroscopy, perfusion and PET all have a before-treatment use and an
  after-treatment use, and `/tests/follow-up-scans` (WI-521) already ships the
  after-treatment half — pseudoprogression, radiation necrosis, the twelve-week
  rule, and the honest "they help, and they do not settle it" reached from two
  sources that disagree. So the split is not "which scans" but **"which side of
  treatment"**. Written into the front matter (§12.8, WI-526) and guarded by
  seven named branches, **each with its own canary**, plus a proven route.
  The first draft had a redaction list instead, and `/review` proved the regex
  matched nothing on the page even with the redactions removed — so the
  allowlist exempted nothing and its comment documented a decision the guard had
  never made. **A redaction you have not watched fire is dead code that reads
  like care.**
- **Name the direction a test fails in, or the limit is decoration.** The fMRI
  section's real content is that neurovascular uncoupling produces a **false
  negative**: near a tumor, a working region can stop producing the signal, so
  it looks quiet, **and a quiet spot on the map looks like a spot that is safe
  to operate on**. "The scan is not always right" tells a reader nothing and
  lets them conclude either that it is useless or that it is proof. The guard
  asserts the direction and the consequence separately.
- **The most useful sentence on a page can be a permission, and it has to
  survive the softening as well as the deletion.** RadiologyInfo: if a task is
  too hard, tell the technologist, and they may give you different tasks or
  easier questions. PMC4757221 found simpler tasks gave *more reliable* results.
  A guard that only asserts the sentence is present is beaten by appending
  *"Even so, try your best, because a poor result means a repeat"* — WI-525's
  tier lesson, applied to a single claim. Ban the downgrade shapes inside the
  section as well.
- **Where a source does not support a distribution claim, ask the question
  instead of hedging it.** Amino-acid PET is real and its uses are in the
  guideline; "concentrated in academic centres" is the dossier's, uncited. A
  draft softened it to "the answer is not the same everywhere", which is still
  an assertion about availability. It now asks whether the scan is done where
  the reader is treated, and the guard is **shape-based**: any sentence putting a
  place-noun near a scarcity or coverage word is an availability claim, numeral
  or not. It started as a word list and `/review` beat it in one line with
  *"Not every hospital has one"*.
- **`grey`, and a whole colour family nobody had swept for.** WI-529 swept
  doubled consonants and `-ement`; every sweep before it looked at `-ise`,
  `-our` and idiom. A draft of the DTI section wrote "your brain's grey parts".
  Corpus-clean at the time of adding, and safe because `grey` is not a substring
  of `gray` — the US form loses a letter rather than gaining one, so it cannot
  hide the British one. Added to `CuratedPage.BritishForms`.
- **The restatement check is now shared, and it needs two more exclusions than
  WI-521 gave it.** A corpus-wide shingle run on a NEW page reports the corpus
  colliding with itself unless you also strip **whole markdown links, label
  included** (a link label is a page TITLE and the same door appears on a dozen
  pages) and the **"What to ask your team"** section (§12.2 item 7 requires one
  everywhere, and "Can I have a copy of my pictures?" is the question every scan
  page owes its reader). With those in, it found nine genuine near-duplications
  in one draft — including a whole "who reads it" paragraph and three separate
  restatements of `/tests/mri`. Promoted to `CuratedPage.Shingles` and
  `AssertDoesNotRestateTheCorpus` **the same day it was written**, because the
  item's second page needed it: §12.8's threshold is the second use, not the
  fifth. `AssertNoEscalationList` went the same way, for the same reason.

**And five more from `/review`, which returned three blockers, twenty-six
should-fixes and TWENTY-TWO EXECUTED GUARD WALK-THROUGHS — twelve of them
beaten, each with the exact sentence that beat it:**

- **A CORRECTLY-CITED SENTENCE CAN BE THE WRONG STRENGTH FOR THE PAGE IT LANDS
  ON.** ACS says, verbatim, *"Tumor usually shows up on a PET scan, while scar
  tissue does not."* The planning page printed it and it was the item's worst
  defect — because `/tests/follow-up-scans` owns that question and refuses the
  binary (*"Those help, and they do not settle it. Older reviews said outright
  that they cannot reliably tell the two apart. Newer ones report them doing
  better than that."*), reached from two sources that disagree. §12.10 is about
  strengths, not about sourcing, and every previous item hit it between a page
  and a sibling. **This is the first time the STRONGER version was the one with
  the citation.** The page now keeps ACS's own hedge — "more likely to be" — and
  routes, and the binary is a banned branch with a canary.
- **A DOOR ADDED TO A SIBLING IS PROSE ON THAT SIBLING, AND IT CAN UNDO A
  PREVIOUS ITEM'S FIX.** The new door on `/treatments/craniotomy` said the extra
  scans "work out where those parts sit"; the same page says "to **suggest**
  where those areas sit" 115 lines later, and §12.8 (WI-523) records that page
  being corrected FROM the stronger verb TO the weaker one. §12.8 (WI-510)
  already says the sentence above a block is shared prose; the same is true of
  the sentence a door is appended to. **Diff a new door against the page it
  lands on, not only against the page it points to.**
- **Widening a guard needs the correct page re-run, immediately.** Four of the
  branches added in response to `/review` fired on correct prose on the first
  build: `most` within 35 characters of `scans` caught *"Most of the other scans
  here show what tissue looks like"*, and the feeding guard caught the questions
  list's *"I am breastfeeding."* §12.8 (WI-521) says invert a guard only after
  checking the correct page survives it — that holds for every widening, not
  only for inversions.
- **An unsourced frequency claim can be about the READERSHIP.** *"For most
  people who read this page, the CT was easy"* is a claim about how many people
  feel a thing, and the share guard caught it rather than any reading did.
  Rewritten as a conditional (*"If you are reading this after the scan…"*),
  which asserts nothing about anybody.
- **An escalation-shape guard's timing words must not be gated behind a verb,
  and a list ITEM cannot be a lead-in.** `same day|today|tonight` sat behind
  `call|ring|phone|dial`, so *"Get seen the same day if any of these happen:"* —
  a complete same-day tier — walked through. Ungating them then made a questions
  bullet reading "Why a CT **today**…" followed by nine more bullets look like an
  urgent list, so marker lines are skipped: the lead-in is the preceding
  PARAGRAPH (§12.8, WI-521). Both halves are canaried inside the shared helper,
  against synthetic pages rather than against the real one.

**The end-to-end read, eighteenth item running, found the one thing nothing else
could — and it is about WHERE a tooltip lands.** A glossary tooltip fires on a
word's **first** occurrence, so the position of that occurrence decides which
paragraph gets the definition. A draft introduced `radiologist` inside the PET
section's breastfeeding advice, and the rendered page put the definition of a
radiologist in the middle of instructions about pumping milk — while **slot 9,
the section that exists to explain who reads your scan, rendered with no tooltip
at all**. The markdown contains neither the tooltip nor its position, so no gate
and no reading of the source can see it; only the rendered page can. There is a
test now, and **its first version failed a correct page**: it used a bare
`IndexOf`, and "the radiologists' own guidance" contains the term as a substring
while NOT firing the tooltip, because `GlossaryMarker` matches whole words.
**Model the matcher, not the string.**

Also from that read: RadiologyInfo's *"the x-rays used for CT scanning should
have no immediate side effects"* is verbatim and was still cut from the step
list, because **"immediate" opens a question the reader cannot answer from
there** — the radiation section answers it thirty-nine lines later, and a
frightened reader does not necessarily get that far. The half that survives is
the one they can use: no radiation stays in your body afterward.

**Fifteen more from the twentieth one (WI-531, `/treatments/proton-therapy`
and `/treatments/stereotactic-radiosurgery` — two pages in one item, and the
item where a preferred source's own LIMITATIONS section was the spine):**

- **THE PREFERRED SOURCE MAY ALREADY CARRY THE ANTI-HYPE, AND THE DOSSIER MAY
  NOT.** `treatment-library.md` §6 frames proton therapy around pediatric and
  slow-growing tumors, which is true and is not the thing a reader needs. ACS's
  brain radiation page says, verbatim, that proton "may be more helpful for
  brain tumors that have distinct edges, such as chordomas" **and that "it's
  not clear if it is as useful for tumors that typically grow into or mix with
  normal brain tissue, such as astrocytomas or glioblastomas"** — which is most
  of this site's readers. ACS's own proton page has a heading called "What are
  the limitations of proton therapy?" whose last bullet is *"More research is
  needed to know if it's better than traditional radiation therapy."* EANO says
  the guideline version: *"RCTs are required to determine the tolerability,
  safety and efficacy of these approaches compared with standard
  radiotherapy."* Two independent preferred sources, and the dossier has
  neither. **Read the source's own caveats section before you read the
  dossier's framing.**
- **A NUMBER IN A PAPER'S INTRODUCTION IS SOMEBODY ELSE'S NUMBER.** "Roughly 45
  proton centers in the United States" is a sentence in PMC13521102's intro
  carrying citation 5 — the bundled-claim shape §12.8 (WI-512) records three
  times, one layer up. A first draft published it, attributed it to "a recent
  review" (the paper is a single-center retrospective analysis, not a review —
  §12.8, WI-519), and rounded a 250-mile comparison threshold into "some of
  them lived more than two hundred miles away", which is in no source at all.
  **Nothing is published now.** ACS says "a limited number ... but more are
  being built" in its own voice, the page prints that and routes the reader to
  ask where their nearest one is, and what the paper's OWN data supports — that
  the proton group had travelled much farther on average — is what the travel
  section rests on. A count that goes stale while the page sits there is worse
  than no count.
- **THE ACCESS SOURCES FOR A COMMERCIAL TREATMENT ARE MARKETING, AND THERE IS A
  RESEARCH LITERATURE UNDERNEATH THEM.** The dossier's insurance material rests
  on a proton center's own blog, a law firm's blog and a content farm. Europe
  PMC has **PMC11905844** (a multistate analysis of external-review decisions
  for proton therapy) and **PMC11699354** (a review of prior-authorization
  burden in radiation oncology) — both open, both peer-reviewed, and between
  them they carry the whole section: the appeal is a legal right, the outside
  reviewer's decision is **binding on the plan**, the review **costs the patient
  nothing**, and what actually wins one is guidelines, published studies, trial
  eligibility and a personalised letter. Search the literature before accepting
  a dossier's non-clinical sources.
- **A SHARE CAN BE PUBLISHED WHEN IT ONLY PUSHES ONE WAY, AND IT GOES IN THE
  SENTENCE THAT SCOPES IT.** The external-review overturn rate is the single
  figure on either page. It is written in words, it is a little over four in
  every ten rather than "close to half" (which rounded 42.1% toward hope), and
  the study, the three states and the population are in the SAME sentence — not
  in the two around it, which is what a first draft did while the front matter
  claimed it was attributed (§12.8, WI-523: the front matter can claim what the
  page does not do). It is published because a reader who supplies their own
  number supplies "appeals never work", and because the only action it can
  produce is *appeal* (§12.8, WI-520: ask which way a number pushes).
- **A DISPARITY FINDING IS ACTIONABLE, AND IT IS NOT A STATISTIC.** Five studies
  agree that who receives proton therapy is not decided by the tumor alone, and
  that Black patients receive it less often than white patients with comparable
  cancers. On a patient page that is not an epidemiology note: it is the reason
  the page says **ask directly, do not wait to be offered.** No odds ratio, no
  percentage, and the action sits in the same paragraph.
- **THE NAME CAN BE THE DEFECT, AND A PAGE THAT CORRECTS IT MUST NOT SOURCE THE
  CORRECTION FROM NOWHERE.** "Stereotactic radiosurgery" tells people they have
  been offered an operation. A first draft explained the name with an invented
  etymology ("the edge of the treated area is as sharp as a blade's"), which is
  a worse state than not explaining it (§12.8, WI-510). Cleveland Clinic
  actually says it: as accurate as a surgical knife, without cutting into your
  body. **Also invented and cut: "a few usually means somewhere between two and
  five"**, which no source gives.
- **A GUARD ABOUT A NAME HAS TO BE ABOUT THE SUBJECT, NOT THE WORD.** The SRS
  page's central safety property is that it never calls this treatment an
  operation — and it discusses operations constantly, because an operation is
  the other treatment. A determiner list of three (`this|the|your`) was beaten
  by **"Gamma Knife surgery"**, which is the literal TITLE of the source the
  page cites, and by "their surgery", "a surgery", "on the day of surgery" and
  "Radiosurgery is an operation done with beams". Widened to every determiner,
  it then failed three CORRECT sentences on the first build. The working form
  keeps the determiners that point at the thing in hand and drops the
  indefinite article.
- **A SHARED BLOCK CAN BE RIGHT ON A PAGE AND STILL LEAVE A HOLE IN IT.**
  `[ESCALATION]`'s two fever rules are scoped *"If you are having
  chemotherapy"* and *"In the weeks after brain surgery"*. An SRS reader is
  neither, and comes home with pin holes in their scalp — which no other page
  in the corpus has, so the block cannot cover them. A first draft wrote the
  rule with **no timing word at all**, two sections above the block, while
  `/treatments/craniotomy` files a warm wound, a leaking wound and a fever as
  **same day**. That is a page inventing a tier LOWER than the corpus files the
  identical symptoms — the under-triage direction (§12.8, WI-511) — and the
  guard could not see it because a downgrade check has nothing to downgrade
  when nothing was escalated. **Assert the TIER WORD, put the page's own line
  beneath the block (§12.10), and say out loud which of the block's lines is
  not addressed to this reader.**
- **A POINTER WRITTEN BEFORE A BLOCK POINTS AT THE WRONG PLACE AFTER IT.** The
  page said the pin-site rule was "below the list at the end of this section" —
  but `[ESCALATION]` brings its own heading, so the rule lands in a new section.
  §12.8 (WI-528) records a composed block moving what a pointer points at; this
  is the same defect in a navigation instruction, and **only the rendered page
  shows it.**
- **`the same` IS NOT AN EQUIVALENCE CLAIM, AND `goes? away` IS NOT `go away`.**
  Two widenings failed correct prose on the first build for the same reason: a
  token that is also ordinary English. `the same` fired on "you go home,
  usually the same day", "the same mesh mask", "call your team the same day"
  and the page's own "stays the same size"; the fix is to require the noun that
  makes it a comparison. And `goes? away` matches "goes" and "goe" and never
  "go", so a promise guard could not match its own canary. §12.8 (WI-530)'s
  rule — re-run the correct page immediately after widening — applied five
  times on this item, and every time the guard was wrong rather than the page.
- **AN ESCALATION LIST DOES NOT NEED AN URGENCY WORD TO BE ONE.** `/review`
  planted three complete tiers on the proton page and all three walked through
  the shared `AssertNoEscalationList`: *"Ring the number on your appointment
  letter if any of these happen:"*, *"Get in touch with your radiation team if
  any of these happen:"*, *"Your team needs to hear about any of these before
  your next visit:"*. None contains a timing word or an urgency word. The shape
  is a CONTACT verb plus a conditional hand-off into a list, and it is now a
  branch of the shared helper with all three as canaries.
- **A BARE COUNT IS NOT A SHARE, AND AN ATTRIBUTION LOOKS LIKE ONE.** The
  page-wide quantifier guard ported from WI-524 fired on ASCO's *"one or two of
  them"* (a count of metastases) and on *"One hospital tells people..."* —
  which is the attribution form §12.8 (WI-507) **requires**. `CountWord` came
  out of the share vocabulary, and `of them` with it, because it points at
  whatever the last noun was rather than at people.
- **THE BRITISH FAMILY NOBODY HAD SWEPT IS EVERYDAY NOUNS.** `car park`. Every
  previous sweep looked at spellings (`-ise`, `-our`, doubled consonants,
  `grey`) and at idiom (`a lift`, `a drip`). A US reader parks in a parking lot,
  buys gas, and drives on a highway; `car park`, `petrol`, `motorway` and `dual
  carriageway` are corpus-clean and none is a substring of a US word. **Two
  candidates were run and REJECTED with the reason recorded:** `straight away`,
  which `/review` proposed and which is LIVE on `/treatments/chemotherapy` in
  four places including the fever rule, and `chemist`, which is a substring of
  `chemistry` and `immunohistochemistry` — WI-511's stemming defect exactly.
- **THE SLOT ORDER TEST CAN ASSERT THE PAGE AGAINST ITSELF.** The SRS page ran
  slot 8, slot 9, slot 7, caregiver — so the reader who needed to know what to
  bring met it after the section about being treated a second time — and its
  own order test listed the slots in the PAGE's order, which made the "order is
  fixed" assertion vacuous. All three sibling treatment pages run **7 →
  caregiver → 8 → 9**. §12.8 (WI-510)'s own lesson, committed inside the test
  written to enforce it: **build the required list from the standard, then read
  it against the page.**

- **A TEST CAN PASS ON EVERY DEVELOPER MACHINE *BECAUSE* THE MACHINE IS
  CONFIGURED, AND FAIL ONLY WHERE IT IS NOT.** This is a new class of defect for
  the project and it is the only one here that no local check of any kind could
  reach. Both of this item's render classes took a raw
  `WebApplicationFactory<Program>` through a primary constructor and never
  pushed the test connection string into it. A developer machine has that
  string in `dotnet user-secrets`, so the host boots and all eight tests pass;
  the CI runner has none, so `Program.Main` throws *"Connection string
  'BrainHarbor' not found"* before a page is served. The full suite,
  ContentCheck **and a 125-mutation break harness** all reported the item
  finished. Every other render class in the corpus already wraps the factory —
  the primary constructor is what dropped it, so both are explicit constructors
  now with the reason written at the point somebody would delete it. There is a
  corpus guard now (`EveryClassTakingARawFactoryPushesTheTestConnectionString
  IntoIt`), and it is a **source scan rather than reflection**, which is the
  opposite choice from the collection-hygiene test beside it: the wrapping
  happens inside a constructor BODY, and reflection cannot see one. **The
  general rule: when a test depends on ambient machine configuration, the thing
  to assert is that the test supplies its own.**

**And the end-to-end read of the RENDERED pages, twentieth item running,** found
six things no gate could: the pointer above; the same claim in two consecutive
paragraphs, created by the `/review` fix that added it (*"proton plans are among
the most likely to be flagged"*, twice in four lines); "Where to go next"
re-committing the contradiction the body had just been corrected for (*"All of
it applies to proton therapy"*, four screens under *"The first difference is the
room"*); *"often ... often"* inside one sentence in the section that reframes
what success looks like; a pronoun with no antecedent (*"arrange a driver
whichever they had"*); and **two pieces of site-voice meta** — *"because no
source gives a set answer"* and *"so the site says one thing about it"* — which
are §12.8 (WI-510)'s "we do not publish numbers" in a new coat. A reader has no
use for what our sources do or do not give.

**WI-532 — `/treatments/targeted-therapy`, and the item where a guard proved
only that a file existed.** Grade **5.4**, **1895 tests** (1860 before),
ContentCheck **260/0**, **88 break-mutations green on LF and CRLF**, three
render guards proved by a script rather than by hand. No new glossary entries;
six doors, and a seventh refused.

- **A CONTRACT ITEM CAN BAN A DOMAIN BY NAME AND THE DOSSIER CAN SOURCE A WHOLE
  SECTION TO IT.** §12.2 item 6 names `avastin.com`, and
  `treatment-library.md` §9 sources the ENTIRE bevacizumab section to it — what
  the drug is, what it is for, the side effects, and the surgery interval.
  Replaced wholesale by ACS's brain-specific targeted-therapy page, which
  carries every one of those claims at patient level. **The manufacturer's "at
  least 28 days before or after surgery" goes with it and is not published**:
  no non-manufacturer source in the reachable set gives a number, ACS says
  "usually it can't be given within a few weeks of surgery", and that is what
  shipped, with the page saying outright that there is no one number that fits
  everybody. §12.14 in its purest form.
- **THE DOSSIER COVERS THREE DRUG FAMILIES AND THE PERMITTED SOURCE COVERS
  SIX.** ACS adds H3 K27M (dordaviprone), mTOR (everolimus) and NTRK. A page
  built from the dossier alone would have told a reader holding one of those
  three results that there is nothing for them. **A replacement source is not
  automatically the narrower one — read its scope before you assume what it
  costs you.**
- **AN EXCLUSION IS A FACT WITH A CONSEQUENCE.** The vorasidenib trial
  "excluded" anyone who had had chemotherapy or radiation, verbatim on the FDA
  page and absent from the dossier's eligibility bullet. Four other pages carry
  vorasidenib as an option; without this, a reader who has had either reads all
  of them as addressed to them.
- **AN ACCELERATED APPROVAL IS A DIFFERENT CLAIM FROM AN APPROVAL**, and
  "contested" is the wrong word for it. Dordaviprone was approved on early
  results with the confirmatory ACTION trial still running, and that trial has
  not reported. Say the shape, not the doubt.
- **THE BACKLOG'S FRAMING WAS SMALLER THAN THE PAGE.** The item names
  bevacizumab as "the anti-hype teaching case". Four drugs here were judged on
  four different measures — the scan, time before growth, early results, and
  ACS's own "not clear if it can help people live longer" — so the central
  section is *what would it mean to say this worked?* rather than one drug's
  cautionary tale. The backlog's headline claim being right does not make it
  the whole spine.

**The break harness found three guards that could not fail, after `/review`
had returned six blockers and sixteen should-fixes and all 71 guard
walk-throughs had been answered. Two were real and the third was the harness's
own input.**

- **A CASE-INSENSITIVE ANCHOR ON A PHRASE THE SIBLING ALSO USES AS PROSE IS NOT
  AN ANCHOR. This is a new class.** `Assert.Matches(@"(?i)\bWhat is measured\b",
  sibling)` was written to prove `/tests/molecular-markers` still owns the
  marker definitions this page routes to. That page can lose **all fifteen** of
  its `**What is measured.**` entries and the guard stays green, because it also
  says, in ordinary lowercase prose, *"Each one below says what is measured,
  what your team does with it"*. The guard proved the file existed, which is not
  what the comment above it claimed. **Pin the CONVENTION, case-sensitively, and
  count it.**
- **And the four probes it took to find that are the other half of the lesson.**
  Every count run while chasing it was case-SENSITIVE while the assertion was
  `(?i)`, so the file read as clean at every step and the read path itself came
  under suspicion instead — `RepoRoot()`, the build output, a stale assembly,
  even the heredoc backspace bug, all checked and all innocent. **The experiment
  that broke the deadlock was replacing the whole sibling with a four-line
  stub**: that failed the assertion, proved the read was live, and sent the
  search back to the only thing left, which was the pattern. When a guard will
  not fail and the mechanism checks out, suspect the comparison before the
  plumbing.
- **PRESENCE IS NOT POSITION, THIRD RECURRENCE, THIS TIME IN A DOOR GUARD.**
  `Assert.Contains("/tumors/low-grade-glioma", sibling)` was meant to prove that
  `/treatments/watch-and-wait` still offers the route this page is reachable by
  instead of the door its pin refused. Deleting that route from the vorasidenib
  paragraph left the guard green, because the same page's "Where to go next"
  index carries the identical slug four screens later. Distance-checked against
  the pinned sentence now, the same way a door is.
- **A MUTATION MADE OF FUNCTION WORDS IS NOT A MUTATION.** The
  corpus-restatement break planted *"It is useful for what it is useful for."* —
  verbatim from `/tumors/glioblastoma` — and the guard stayed green, because
  `Shingles()` skips any eight-word window carrying fewer than THREE content
  words and that sentence is eight function words around two. The guard was
  fine; the harness was lying about it. **§12.8's "too weak is not a pass"
  applies to the harness's own inputs: when a break does not fail, ask first
  whether the mutation reached the mechanism at all.**
- **THEN `/review` BEAT BOTH REPLACEMENTS, WITH WORKED COUNTER-EXAMPLES, AND
  THAT IS THE SHARPEST LESSON OF THE ITEM.** A guard rewritten in response to a
  harness failure is a NEW guard and has had no harness run against it. Both of
  these were written to close a demonstrated hole and both opened another:
  - The door guard's replacement measured **400 characters** from the pinned
    sentence. `IndexOf` returns the FIRST occurrence page-wide, so an unrelated
    link to the same hub earlier in the body turned the test red with a message
    accusing the vorasidenib paragraph of something it had not done — a false
    RED on a correct edit. And `Math.Abs` accepts a route AFTER the pin, so
    moving the route into the very next paragraph (121 characters) stayed green
    — the same false GREEN, one paragraph over. **A character distance is a
    proxy for "the same paragraph". Slice the paragraph and the proxy is not
    needed**, and the ban on the door can be scoped to that paragraph too, so a
    later item adding this page to that sibling's "Where to go next" index — a
    correct edit that does not touch the end-anchored pin — does not fail
    against a reason that does not apply to it.
  - The markers guard's replacement counted the sibling's convention with a
    floor of `>= 10`. **Deleting the two entries this page actually routes
    readers to — IDH and BRAF, the two its own front matter names — drops
    fifteen to thirteen**, so the floor stays green through exactly the change
    its failure message claims to catch. A headcount is not the property. Assert
    the anchors the page depends on **by name**, and keep the count only as a
    second signal with a message that admits the corpus-rename case.
  Four mutations encoding those counter-examples are now in the table, so the
  harness owns them from here: **when a guard is rewritten, the harness runs
  again on the new one, and the new one gets its own mutations.**
- **Seventh item running for the heredoc rule**, which `mutations.py` was
  written with the Write tool to obey. And the three Kestrel render guards the
  harness structurally cannot reach — it runs `--no-build`, and the host serves
  Content copied at BUILD time — are now proved by a committed script that
  mutates, REBUILDS, confirms red, restores and rebuilds, rather than by hand
  and by memory.

**And the end-to-end read of the RENDERED page, twenty-first item running,
found seven, one of which is a safety-adjacent sourcing defect nothing else
could see.**

- **A RANKING DELETED IN ONE SECTION SURVIVED IN ANOTHER, IN DIFFERENT WORDS.**
  The front matter records killing *"high blood pressure is the common one"*
  because ACS lists bevacizumab's common effects UNRANKED and the only source
  for a ranking was the banned manufacturer bullet. A hundred and thirty lines
  earlier, the step-by-step still said *"because high blood pressure is the side
  effect this drug is most watched for"* — the same unsourced ranking with an
  unsourced monitoring claim wrapped around it. **§12.14's shape inside a single
  page: when you remove a claim, grep the whole page for it rather than the
  section you happened to notice it in.**
- **SITE-VOICE META, ONE ITEM AFTER WI-531 REMOVED THE IDENTICAL
  CONSTRUCTION.** *"so the site says one thing about them"* — WI-531's entry
  above names *"so the site says one thing about it"* by name. Two more of the
  family were in the same section: *"they are the reason that section of the
  list exists"* and *"they are the reason this page gives you a rule"*. A page
  explaining its own editorial decisions is talking to us, not to the reader.
- **A PAGE CAN RESTATE ITSELF AND NO CHECK SEES IT.**
  `AssertDoesNotRestateTheCorpus` compares against every OTHER page and skips
  its own, by design. *"frequent early on and usually settle into a pattern"*
  appeared verbatim in two sections, and the "How long does it take?" opener and
  its own first bullet said the same sentence three lines apart.
- **`out of hours` IS BRITISH AND IS NOT ON THE SHARED LIST.** The escalation
  block says *after-hours*. Six shipped files already carry the British form
  (`seizures/what-to-do`, `treatments/anti-seizure-medicines`,
  `treatments/chemotherapy` ×4, `tumors/astrocytoma`, `tumors/glioma`,
  `tumors/high-grade-glioma`) while `tumors/glioblastoma` and
  `tumors/oligodendroglioma` say *after hours* — a §12.10 one-instruction,
  two-wordings split that `CuratedPage.BritishForms` is structurally blind to
  because the phrase is not on it. This page would have been the eighth
  carrier. Logged for `/pm` as an add-to-the-list-and-sweep item; not fixed in
  passing, because it is nine files.
- **A pronoun that is correct about pills reads as the patient in a caregiver
  section** — *"Work out together where they live"*, three lines under a
  sentence about the person being cared for.
- **GLOSSARY POPOVERS ARE NOT A TOOLTIP-POSITION DEFECT, and this is recorded
  so the next end-to-end read does not re-flag it.** They render as `popover`
  elements, hidden until activated, so a definition that appears mid-sentence in
  the FLATTENED text does not interrupt the rendered sentence — three of them
  land inside load-bearing claims here and none of them hurts. WI-530's finding
  was about which SECTION gets the tooltip and about a definition being wrong
  where it landed, which are different things and still apply.

**WI-533 — `/treatments/tumor-treating-fields`, and the item whose clinical half
was already shipped twice.** Grade **4.4**, **1929 tests** (1895 before),
ContentCheck **261/0**, **103 break-mutations green on LF and CRLF** (34 of them
`/review`'s counter-examples and one from the second rendered read), three render
guards proved by script. No new
glossary entry; the existing one suppressed here and kept for the two hubs.

- **DECIDE §12.10 BEFORE DRAFTING, AND SHARE THE PHRASE, NOT THE CLAUSE.**
  `/tumors/glioblastoma` already shipped the device, the 18 hours, the shaved head
  and the start interval; `/tumors/high-grade-glioma` shipped EANO's disagreement.
  The page reuses "at least eighteen hours a day" and the EANO sentence verbatim.
  A six-word phrase is shorter than a restatement shingle, so it needs no
  allowlist entry — **it needs a test that reads the hub instead**. And the
  restatement check still fired, on the words AROUND the phrase: "it at least
  eighteen hours a day and" matched the hub's sentence because the draft had
  mirrored its clause. The EANO sentence is fifteen words and IS allowlisted.
- **A SECOND STRENGTH ARRIVES AS AN IDIOM, NOT A NUMBER.** Every numeric second
  strength was banned before drafting, and the page still said "From then on it
  runs day and night", "wearing something that never comes off" (both caught by
  `/review`) and, after that fix, "The cap and the bag are always there" (caught by
  the rendered read). Each one says twenty-four hours.
- **A 200 IS NOT A PAGE.** `virtualtrials.org/optune.cfm`, the dossier's source for
  the 18 hours, the pad changes and the support-person bullet, answers HTTP 200
  with a zero-byte body behind a Sucuri firewall. Check the body size, not the
  status code. Its other source was `optunegio.com`, the manufacturer, which is
  §12.2 item 6's rule by extension — and the contraindication list it alone
  carries is not published.
- **ONE TREATMENT, TWO READERS, AND A SENTENCE TRUE OF ONLY ONE.** The device is
  used alongside temozolomide when newly diagnosed and INSTEAD of chemotherapy at
  recurrence. "Some of what you feel will be the temozolomide" was addressed to
  both, and the recurrence reader is not on it — `/review`'s second blocker. A
  conditional ("If you are also taking temozolomide") is the fix, and a guard now
  requires it on every sentence that addresses the reader about the drug.
- **THE WORD HAS TO MATCH THE SHARE, AND THE STUDY DESIGN HAS TO ALLOW THE WORD.**
  From one 30-patient study: 55% became "about half", not "most"; 3% of 30 became
  "one person", not "a few people"; the adaptation time is the median and range
  (14 days, 0–90), not the mean (24). And "most still kept wearing it" was
  deleted outright: **the study enrolled only people who had already completed two
  months**, so everyone in it was still wearing it. A survivorship artifact is not
  a finding.
- **FLATTENING "SOME" INTO "THE".** "The doctors called it a good option, and also
  said it may not be the right choice" — the source's nine clinicians ALL called it
  a good option and SOME said it may not suit everyone. A bold lead ("Doctors who
  offer it say both things") then generalised nine doctors at one centre to all.
- **A SOURCE'S INSTRUCTION CAN BE FLIPPED BY WHERE IT LANDS.** PMC7399624 says wear
  breathable headwear *to avoid overheating*. Placed under "You decide what to
  share", a hat became concealment and cooling at once. It lives beside the warmth
  paragraph now.
- **FUNDED SOURCES, CITED FOR WHAT THEY CAN CARRY.** Two of the three
  patient-experience sources have manufacturer ties. They are cited for skin-care
  practice and for what participants said, never for effect; the finding leaned on
  hardest (head shaving was the reason decliners gave most often) cuts against the
  funder. The reason the hours matter is the independent HTA's, with its hedge.
- **CHECK THE REVIEW'S QUOTATION TOO.** §12.14's last rule, one layer up: `/review`
  quoted CADTH as saying "Optune is visible even with headwear", which is not in
  the fetched text, and flagged "the main cancer guidelines" as one guideline when
  the cited paper names three (NCCN, ASCO, ASTRO 2025). Both rejected with the
  verbatim in the front matter; the hat sentence was fixed anyway, to what the
  source does say.

**`/review` beat the first guards with worked counter-examples, and every one is a
`review-` mutation in the harness now.**

- **A CASE-INSENSITIVE REDACTION ERASES THE CONTEXT IT SITS IN.** The hours guard
  blanked the shared phrase before scanning, so "at least eighteen hours a day on
  most days" and "…, or as many as you can manage" were green. The phrase is
  replaced by a TOKEN now, and a qualifier hung on either side of the token fails.
- **A GUARD THAT PASSES ON A REPEAT FAILS THE FIX FOR THE REPEAT.** The duration
  slot opens "At least eighteen hours a day", capitalised; the ordinal `Contains`
  passed only because a bullet repeated the phrase in lowercase — so removing that
  self-restatement, a correct §12.8 (WI-532) edit, would have turned it red.
- **A PARAGRAPH IS NOT THE SUBSECTION.** The hub check read the one paragraph
  holding the phrase; "worn most of the day" added to the paragraph above it, in the
  same `###` device subsection, stayed green. It slices the subsection.
- **A LIST OF CLAIMS, EACH WITH ITS ONE STUDY.** "Every 'most people' paragraph
  names a study" was a floor that accepted either study, so swapping "German" for
  "Chicago" stayed green, and "The majority said" was never selected. Each claim is
  listed with the study it belongs to, the other study's name is banned from its
  paragraph, and unlisted share-shaped claims fail.
- **INFLECTIONS AGAIN.** `lives?` does not match "lived"; the lookbehind allowed
  "thought to" and failed CADTH's own "believed to".
- **A PAGE CAN RESTATE ITSELF SEVEN TIMES.** Plug into the wall ×3, wet pads ×2,
  visiting nurse ×3, implants ×3, expensive ×2, training ×2, scans ×2. Each
  practical point is pinned to the one section that owns it. **And the fix for the
  hub-framing paraphrase created an eighth**, which only the second rendered read
  saw.
- **A COPIED HARNESS RUNS THE TABLE IT LOADS, AND `ok` DOES NOT SAY WHICH.** The
  first WI-533 run printed 54 `ok` lines for WI-532's mutations against WI-532's
  page: `break-tests.py` had its docstring updated and its `sys.path` line left
  pointing at `wi532-sources`, while `dryrun.py` — which DID point at the new table —
  reported 103 clean mutations. Killed mid-run it left a live mutation on
  `/treatments/targeted-therapy` and eight siblings LF-normalised, restored from git.
  **Read the first mutation NAMES a run prints before trusting its `ok`s, and grep
  every copied script for the old item number, not just the one you ran.**

**WI-534 — `/treatments/shunts`, and the item that created an escalation tier.**
Grade **4.8**, **1958 tests** (1929 before), ContentCheck **262/0**, **62
break-mutations green on LF and CRLF** (one guard beaten on the first run, rewritten
and re-proved),
four render guards proved by rebuild. No new glossary entry; `hydrocephalus`
suppressed here and kept.

- **A TREATMENT CAN CHANGE THE TIER OF A SYMPTOM THE CORPUS ALREADY TIERS.** The
  shared [ESCALATION] block files a bad headache, repeated vomiting, confusion and
  much more sleep as same-day, which is right for a reader without a shunt. For a
  reader WITH one, NINDS says "seek medical help immediately" and the Hydrocephalus
  Association says the emergency department. The page OWNS the shunt tier (as
  `/seizures/what-to-do` owns seizures, so it does not call
  `AssertNoEscalationList`), and every surface a shunt reader can meet routes to it
  with the IDENTICAL instruction: a third conditional in [ESCALATION] (§12.10,
  WI-563's shape), a line under `/treatments/craniotomy`'s caregiver lists (the
  likeliest shunt reader, on a page that does not include the block), and a clause
  on [MECHANISM]'s "worth a phone call rather than a wait". A threshold phrase that
  is right for most readers can be wrong for one, and the fix is a conditional
  wherever that reader lands, not a softer sentence for everyone.
- **A CONDITIONAL THAT NAMES SIGNS UNDER-TRIAGES THE ONES IT DOES NOT NAME.** The
  first block line listed three signs. A shunt reader with new confusion on a hub
  read the same-day list, found confusion there, and found no shunt override for
  it. The line now flips the WHOLE same-day list for a shunt reader.
- **A POINTER TO A LIST IS A CLAIM ABOUT THE OTHER PAGE.** "Anything on the
  ambulance list is still an ambulance call ... Get help now keeps that list" —
  `/get-help-now` has no ambulance list. Diff a pointer against the page it points
  at, exactly as a door (§12.8, WI-530); the cases are stated in the paragraph now,
  checked against `/treatments/craniotomy`'s list by reading it.
- **A RECOVERY SECTION CAN NORMALISE THE WARNING SIGNS.** "The tiredness can be a
  surprise", a sore belly cut and "headaches after the operation vary" sat above a
  list saying headache, sleepiness and belly pain need help right away. A tired,
  impaired reader picks the reassurance. Recovery and warning are told apart by
  DIRECTION: normal recovery gets slowly better; a headache getting worse,
  sleepiness getting deeper or pain that keeps growing is not recovery.
- **A TEST OVER RAW FILES CANNOT SEE A ROUTE THAT ARRIVES THROUGH A BLOCK.**
  `AHubThatRoutesIntoATreatmentCarriesThatTreatmentsSafetyRule` checked
  `text.Contains(route)` on each hub FILE. No hub file links `/treatments/shunts`;
  they reach it through [MECHANISM], so the new rule never ran. It reads the
  COMPOSED hub now, with a positive count that the shunt rule was evaluated.
- **THE DOSSIER'S NUMBER IS NOT ITS PAPER'S NUMBER, AGAIN.** "27.8% shunt failure;
  13% single, 14% multiple revisions" is attributed to PMC8976775, which says 33%
  (28 of 85). Neither is published (R2). And a number's START POINT is part of it:
  PMC8827213's early failures came "within about five weeks" of the shunt GOING IN,
  which was itself a median 1.9 months after surgery — "the weeks after surgery"
  would have told a reader shunted at three months that the watch was over.
- **A PAPER'S BACKGROUND IS NOT ITS FINDING.** PMC6257011's "obstructive, typically
  at diagnosis ... communicating, usually later" is its opening sentence, and a
  first draft printed it as "in one study, the blocked kind usually came at
  diagnosis". Dropped with the source.
- **CHECK YOUR OWN SEARCH BEFORE REJECTING A FINDING.** The reviewer quoted NINDS for
  blurred vision, a bulging soft spot and eyes "fixed downward"; a grep came back
  empty and the finding was nearly recorded as unverified. They are in NINDS's
  general hydrocephalus list, which the malfunction-list grep had not reached.
  WI-533 recorded checking the review's quotation; this is the other half.
- **TWO OPERATION TIMES, PRINTED SIDE BY SIDE** — ACS "about an hour", MedlinePlus
  "about 1 1/2 hours". §12.8 (WI-511) applies to durations as much as frequencies.
- **FIFTEEN SELF-RESTATEMENTS ON ONE PAGE** (make and model ×4, who to call ×4,
  "is on the list" ×4 — one of them false —, temporary drain ×3, for now or for good
  ×3, …). Pinned one owner per point, and the review's "on the list" pointer that was
  false became a ban.
- **A RULE'S OWN LINK CAN SATISFY THE CHECK FOR THE RULE.** The composed-hub fix
  above was beaten by the break harness on its first run: the block's shunt rule
  links `/treatments/shunts#warning-signs`, so every composed hub contained the
  route through the very paragraph the test was looking for, and deleting
  [MECHANISM]'s actual door left the "rule was evaluated" count above zero — a
  positive count that could not go to zero. A route now means the treatment page's
  path NOT followed by a fragment. Third item running where the guard rewritten in
  response to a finding needed its own harness run (§12.8, WI-532).
- **The harness copy was checked this time**: every copied script was grepped for
  its `sys.path` before the first run (§12.8, WI-533).

**WI-535 — `/tumors/diffuse-midline-glioma` and `/tumors/dipg`, two hubs written as
one because their sources keep merging them.** Grade **4.9** and **5.0**,
**2019 tests** (1959 before), ContentCheck **267/0**, **128 break-mutations on LF and
CRLF**, all caught on the fifth and final harness run (three guards were beaten and
rewritten on the runs before it), ten render guards proved by rebuild across both
pages, five rendered reads and **three `/review` rounds** (3 blockers, then 1, then
none). Glossary:
`pons`, `thalamus`, `palliative-care`. Doors: `/treatments/targeted-therapy` to the
DMG `#dordaviprone` section; the `/tumors/high-grade-glioma` "still short" note
retired, because DMG was its last thin destination.

- **AN APPROVAL'S INDICATION AND ITS EVIDENCE CAN COVER DIFFERENT PEOPLE.** The FDA's
  dordaviprone indication has no location limit. The efficacy studies "excluded"
  DIPG and primary spinal tumors and were mostly adults, while the label's SAFETY
  population included DIPG and ACS says it "may be an option" for DIPG. A page that
  quotes only the indication tells a DIPG parent the evidence is about their child;
  one that says "no evidence" overstates the other way. Scope the gap to what was
  measured (whether it shrinks tumors), and read the eligibility paragraph and the
  Drug Trials Snapshot, not only the first sentence of the approval.
- **A SYMPTOM ON A LATE-STAGE LIST IS NOT A SIDE EFFECT TO EXPLAIN AWAY.** Both first
  drafts said tiredness "comes from the treatment". Together by St. Jude lists
  "Fatigue or drowsiness" under "Common symptoms of late stage DIPG", and the shared
  block's same-day tier has "Sleeping much more than being awake". WI-534's recovery-
  section lesson, found this time in "Everyday life", which nobody reads as a warning
  section. The sentence now names both causes and the tier.
- **"WORTH TELLING THE TEAM" IS NOT A TIER.** New swallowing trouble had no timing, and
  "coughing while eating" was in no source. St. Jude: "Problems swallowing. This may
  lead to aspiration pneumonia." Same-day, with the reason, beside the unchanged
  choking ambulance line.
- **OUTLOOK OUTSIDE THE GATE ARRIVES AS A POINTER, A EUPHEMISM OR A KINDNESS.** Three
  shapes on one page, none of them a banned word: a support bullet describing the
  Registry's pages "on care near the end of life"; palliative care that should
  start "early, not at the end"; and a caregiver paragraph about families who "use
  the time" with wish-granting organizations. The gate held the end-of-life
  paragraph and all three undid it for a reader who declined. Rendered read 2 and
  `/review` found the third independently. The gate test now checks a phrase list,
  case-insensitively, against the WHOLE page minus the gate.
- **A SOURCE'S SCOPE TRAVELS WITH THE TIER IT JUSTIFIES.** The spinal-cord override
  cites Cancer Research UK, whose page is about METASTATIC compression ("the cancer
  has spread to the spine"). The right-away tier is kept because it is the safe
  direction; the reason is attributed to what the source covers, not asserted of a
  glioma inside the cord. Its first wording also said "newly weak" under a symptom
  line reading "weakness that keeps getting worse": WI-534's "a conditional that
  names signs under-triages the rest", in the TENSE this time.
- **A NEW GLOSSARY ENTRY CAN ECHO THE SENTENCE IT LANDS IN.** `thalamus` beside "which
  sits deep in the center of the brain", `pons` beside "the part of the brain stem
  that carries the nerves for ...", `palliative care` beside its own definition. The
  tooltip renders inline, so the rendered page read each sentence twice. WI-529 logged
  the shape for `tumor board`; the item that writes an entry is where it is created.
- **AN INSTRUCTION LOSES ITS CAVEAT ON THE WAY TO THE PAGE.** PMC10778507 says trial
  evaluation comes first "unless radiation therapy is urgently needed". "Ask about
  trials before radiation starts" without it can delay radiation for a child whose
  symptoms need it now. A test now requires the caveat on every "ask at the start".
- **A POINTER'S TIMING CAN CONTRADICT THE PAGE IT SITS ON.** Both pages linked
  `/treatments/radiation-therapy#why-am-i-having-radiation-again`, which says a
  second course "usually needs a long gap first, and around a year or more". St. Jude
  re-irradiates DIPG at recurrence. The pointer is gone and the disagreement is
  printed with PMC9144327's own reason.
- **THE UNSOURCED COMPARATIVE WAS ON A SHIPPED SIBLING.** "A larger trial" had been on
  `/treatments/targeted-therapy` since WI-532. No fetched file gives ACTION's size, and
  ACTION is in newly diagnosed patients. Fixed at source; §12.11's class, sixth item.
- **THE HARNESS BEAT THREE GUARDS OVER THREE RUNS, ALL LOOSENESS OF THE SAME KIND.** A
  Roman-numeral regex without IgnoreCase passed "**Grade IV.**"; a gate guard asserting
  the WORD "median" passed with the definition paragraph deleted, off "a median cannot
  show you them" one paragraph later; and the spinal rule's worsening clause, added
  after rendered read 1, was never asserted. A vocabulary guard asserts the
  definition sentence, and a clause added for safety gets its own assertion.
- **`SentencesOf(Flatten(ReaderText(section)))` returned "es." for "**Yes.".** Use
  `SentencesOf(Flatten(Section(...)))`, as HighGradeGliomaPageTests does.
- **A SENTENCE SPLITTER FED READER TEXT MERGES A HEADING INTO THE SENTENCE UNDER IT.**
  Two page-wide guards written for `/review` round 2 went red on correct prose:
  "### Clinical trials, and why to ask early" joined "Some trials start at diagnosis"
  and read as an uncaveated "ask about trials early", and "what is normal afterwards?"
  joined "may be given medicine to sleep" and read as a normalised warning sign. A
  heading ends with no full stop, so the splitter cannot see its end. Strip markdown
  heading lines before splitting, in every guard that reasons about sentences.
- **FIX SCRIPTS THAT CHECK EVERY ANCHOR BEFORE WRITING ABORT SAFELY.** Two of them hit
  a line-wrapped anchor and wrote nothing, which is the point; the cost was a chained
  suite run over unchanged content, because `&&` stopped the script and `;` did not
  stop the suite.
- **A TIER A PAGE WRITES ITSELF MUST HONOUR THE CONDITIONALS OF THE BLOCK ABOVE IT.**
  Round 1's fixes added two same-day lines of the page's own (much more sleep;
  symptoms coming back after radiation). The [ESCALATION] block on the same page
  says that for a reader with a shunt "everything on the same-day list above is a
  right-away call instead", and the DIPG page mentions shunts. `/review` round 2 found
  it: a fix for one blocker created the next. WI-534's "a conditional wherever that
  reader lands" includes the lines the item writes after the block, and a test now
  requires every same-day sentence the page writes to say what changes for a shunt.
- **A HARD SENTENCE SOFTENED BEHIND THE GATE IS STILL A SECOND STRENGTH.** "Today's
  treatments do not usually cure it", inside the outlook gate, sat beside St. Jude's
  "DIPG has no cure at this time" and `/tumors/glioma`'s "not curable", which the
  sibling hubs state OUTSIDE the gate with a shared landing sentence. Moving a claim
  behind the gate does not exempt it from §12.10; it moves to where the siblings put
  it, at their strength, and a test reads the sibling.
- **A FIX CAN MISATTRIBUTE WHAT IT RE-SOURCES.** Round 1 added PMC9144327's reason for
  not recommending re-irradiation as "a 2022 review found it slowed the tumor". The
  review was citing a SIOP analysis. Report what a review SAYS, not a finding it quotes.
- **THE CONFLATION IS IN THE MAINSTREAM SOURCES.** The Brain Tumour Charity and Boston
  Children's say DMG was "previously called DIPG"; ACS writes "DIPG, also called
  diffuse midline glioma"; Together by St. Jude says DIPG can "rarely" be grade 2.
  WHO CNS5 governs; the conflation is quoted once and corrected in the next sentence,
  and a test holds the correction to that sentence.

**WI-536 — `/tumors/ependymoma`, and the item that decided where a second page-local
rule goes.** Grade **5.4**, **2064 tests** (2019 before), ContentCheck **271/0**,
**142 break-mutations caught on LF and CRLF**, nine render guards proved by rebuild,
six rendered reads, four `/review` rounds (2 blockers, then 2, then 1, then none). Glossary: `posterior-fossa`, `posterior-fossa-syndrome`. Sources:
51 files, 241 script-checked quotes.

- **A SECOND HUB NEEDING A PAGE-LOCAL RULE IS A BLOCK, BUT NOT NECESSARILY THE SHARED
  ONE.** WI-535 wrote the spinal-cord right-away rule page-local; ependymoma was its
  second use. It became `blocks/spinal-cord.md`, included after `[ESCALATION]` only by
  hubs whose tumor can sit in the cord — **not** a conditional inside `escalation.md`,
  which two live tests (meningioma, brain-metastases) deliberately pin free of spinal
  words, and where a cord paragraph would be noise on every hub in the list people read
  at two in the morning. §12.10's conditional rule says what MAY go in the shared block;
  it does not say everything conditional should.
- **A CONDITIONAL'S OPENING CLAUSE DECIDES WHO IS COVERED, AND THE FIRST ONE COVERED TOO
  FEW.** "If the tumor is in the spinal cord" left out the reader whose tumor SPREAD to
  the spine — on two pages that both say it spreads through the fluid, and with a source
  that is itself about spread ("Metastatic means that the cancer has spread to the
  spine"). Now "in the spinal cord, or has spread to the spine".
- **A PAGE CAN NAME A SIGN THE RULE IT POINTS AT DOES NOT.** The page listed "back or
  neck pain" and routed to a rule saying "back pain", while the rule's own source says
  the pain "could be anywhere in your back, spine or neck". A test reads the BLOCK now
  and requires every sign the page's spinal line names.
- **A POINTER'S "JUST AFTER" MUST MATCH THE RENDERED PAGE.** `[SPINAL-CORD]` composes
  after the whole when-to-call block, sign-off links included, so "just after the list
  below" pointed at the wrong place on both hubs.
- **A SOURCE'S "CURED" CAN BELONG TO A DIFFERENT TUMOR IN THE SAME FAMILY.** ABTA's "can
  be cured by surgery alone" and a 2025 review's "usually curative" are both about
  SUBEPENDYMOMA. Read the heading the quote sits under. The strength for ependymoma is
  ACS's "sometimes ... if the entire tumor can be removed".
- **A SIBLING HUB'S HARD SENTENCE CAN BE WRONG FOR A MEMBER OF ITS OWN FAMILY, AND THE
  FIX IS WHERE THE NEXT DEFECT COMES FROM.** `/tumors/glioma` said grade 2 and above
  grow into the brain and are not curable, on the page that lists ependymoma as a
  glioma. The first fix inserted the exception between "no edge to cut around" and "They
  are malignant ... not curable", so rendered, "They" read as ependymoma. The second
  said "one exception to this paragraph", which excepted ependymoma from "malignant"
  too — and grade 3 ependymoma IS malignant. A pointer that makes no claim of its own,
  after the hard sentence, is the version that survives.
- **"SOME ARE AND SOME ARE NOT" WENT PAST EVERY SOURCE.** No cited source calls a grade 2
  ependymoma not-cancer, and the claim clashed with the page's own spinal rule ("When
  cancer presses on the spinal cord"). The answer leads with the grade instead.
- **A TRIAL RESULT CARRIES ITS POPULATION.** ACNS0831 randomized ages 1-21 after gross or
  near-total removal, and says "Further follow-up is important to assess its effect on
  late relapses". On a page about late recurrence, "did not help" without "at least so
  far" is the wrong sentence.
- **AN AGE LINE IS A DISAGREEMENT, AND "OTHER SOURCES SAY UNDER 3" READS AS ITS
  OPPOSITE.** The cited review agreed with St. Jude (radiation from about age 1, with 1
  to 3 year olds given chemotherapy to defer it); only ACS's GENERAL page says 3. Name
  who says what, and do not turn a patient-education sentence into a claim about one
  hospital's practice ("St. Jude gives").
- **A REASSURANCE NEEDS A SOURCE AS MUCH AS A WARNING DOES.** "It is not the tumor coming
  back" was in no fetched file — the kind of sentence nobody asks for a citation for.
  StatPearls calls posterior fossa syndrome "A well-known complication of posterior fossa
  surgery", so the page says that.
- **REVIEW QUOTES FAIL IN BOTH DIRECTIONS, AND A NAIVE GREP INVENTS "UNSOURCED".** Two of
  `/review` round 1's own quotes were not in the sources. In the other direction, the
  first verification script reported zero hits for phrases that WERE in the files:
  non-breaking spaces and curly quotes. Normalise (NFKC, quotes, dashes, whitespace)
  before concluding anything is absent (§12.8, WI-535's "grep the quotation AND the
  source").
- **A SENTENCE-WINDOW NORMALISATION GUARD CANNOT SEE A PARAGRAPH.** "Most children slowly
  get better" sat three sentences from the symptoms it reassures about, so the guard
  never checked the posterior fossa paragraph at all — and its "at least two
  neighbourhoods" count was satisfied elsewhere, which is what made it look healthy. It
  is paragraph-scoped now and names the paragraphs it must have checked. Also:
  **reassurance is not a tier**, and while it sat in the answered list the harness could
  delete it with nothing failing.
- **AN ORDINAL IDIOM BAN MISSES THE START OF A BULLET.** "- Being sick, ..." walked
  through a case-sensitive `being sick`. The same hole is still in the DMG copy of that
  guard.
- **A MUTATION'S ANCHOR MUST BE UNIQUE, OR A WEAK MUTATION READS EXACTLY LIKE A WEAK
  GUARD.** `', even years'` matched the short version first, so the harness proved
  nothing about the paragraph it was aimed at.
- **A THEORY THAT RE-TYPES ITS OWN LIST AS `InlineData` STOPS COVERING THE LIST.**
  `MemberData` from the same array, so a third including hub cannot skip the check.
- **A TIER THE PAGE WRITES CAN UNDERCUT THE BLOCK ABOVE IT, NOT ONLY MISS ITS
  CONDITIONAL.** WI-535's blocker was a same-day line that ignored the shunt rule. This
  one was worse: the posterior fossa section told a parent to call "the same day" if a
  child stops talking, while `[ESCALATION]`, composed higher up the same page, files
  "Suddenly not being able to speak" as an **ambulance** call. Two tiers for one sign,
  and the page's was the weaker (WI-512). The page carves the sign out and says the
  ambulance rule still stands, then tiers only the signs it actually covers.
  `/treatments/craniotomy` is not the precedent to copy: it describes SMA syndrome with
  no tier at all, so nothing competes with the block.
- **ANSWERING "IS IT CANCER?" FOR ONE GRADE LEAVES THE REST TO INFER THE OPPOSITE.**
  Round 1 correctly killed "Some are and some are not" as beyond source; the replacement
  answered for grade 3 only, three words before the page says most are grade 2 or 3. The
  contrast ("grade 3 IS cancer ... grade 2 is low grade") invites exactly the "low grade
  means not serious" reading §12.3 exists to prevent. No source calls a grade 2
  ependymoma cancer or not-cancer, so the answer is in behaviour terms: low grade is
  about how fast it grows, and it is still surgery and years of scans.
- **A SYMPTOM CAN BE SOURCED ONLY AS A COMPLICATION.** Every "swallowing" hit in the
  cited files is post-operative (St. Jude's surgery complications, the posterior fossa
  syndrome page, the 2025 review's cranial-nerve damage). The one source listing it as a
  presenting symptom was the naming-trap source the page bars by domain. It was in the
  symptom list anyway, with a tier attached.
- **A BLOCK THAT COMPOSES LAST OWNS THE SECTION'S LANDING.** `[SPINAL-CORD]` displaced
  the escalation block's deliberate ending ("If you are not sure whether to call, call")
  and closed the whole when-to-call section on the frightening half. §12.6 is a review
  rule, not a preference: the attribution moved ahead of the instruction so the last
  sentence is the action, and a test pins it.
- **A TEST WHOSE TWO ASSERTIONS NEVER MEET.** `EverySpinalSignThePageNames
  IsInTheBlocksRule` checked that a word was on the page and that the word appeared
  somewhere in the block — so a sign the block had DEMOTED out of the right-away tier
  still passed, which is the drift the test was written to catch. It matches the
  right-away sentences now.
- **A RECORDED JUSTIFICATION CAN BE FALSE OF ONE OF THE PAGES IT CITES.** The block's own
  comment said both including pages tell the reader the tumor can spread through the
  fluid. `/tumors/diffuse-midline-glioma` says it only in a front-matter comment. The
  clause is still right there (§12.10: a conditional only has to be true of the reader it
  names), but the reasoning in the file was wrong, and a later item would have trusted it.
- **A CROSSWALK CAN MISS THE STRINGS ON THE REPORT ITSELF.** PF-EPN-A, PF-EPN-B and
  ST-EPN-RELA are how two of the page's own sources write the groups, and they turn up on
  methylation reports; "PF-EPN-A" does not obviously map to "group PFA" for a frightened
  reader.
- **A DEFERRAL THAT LIVES IN A DOCUMENT IS NOT ENFORCED.** The block files new leg
  weakness, numbness and bladder trouble as right-away; `/tumors/spinal-cord-tumor`,
  `/tumors/meningioma` and `/tumors/brain-metastases` file the same signs as "a reason to
  be seen quickly", each with its own source and its own `/review` behind it. One claim,
  two strengths (§12.10) — and WI-536 does not own the subject, so it goes to WI-543 with
  the stub it rewrites. What changed is that the split is now **asserted by a test that
  reads all three siblings**, so drift on either side fails the build instead of waiting
  to be rediscovered. Record a deferral as a guard, not as a sentence in the backlog.
- **NARROWING A TIER CAN STRAND A SIGN.** Round 2 replaced "if any of it" with three named
  signs, and the fourth one the same paragraph lists (behavior) was left with no
  instruction, on a page whose shared block covers "New confusion" but not behavior. The
  normalisation guard could not see it either: its symptom pattern had `mood` and not
  `behavio`.
- **A REPLACEMENT JUSTIFICATION NEEDS THE SAME CHECK AS THE CLAIM (§12.14).** Round 2
  moved a grading citation off NCI-CONNECT and wrote "both ACS pages carry that claim".
  Neither does alone: one carries the general grade framing, the other carries grade 3 for
  ependymoma, and the sentence is the two together.
- **A GRADE SECTION MUST SAY SOMETHING TO THE GRADES IT DOES NOT RULE ON.** After two
  rounds the section answered for grade 3 and grade 2, while the page itself names a
  grade 1 type and says the molecular names often carry no grade at all. No source calls
  grade 1 benign, so the answer is scope ("the slowest-growing kind", and a name with no
  grade is not a missing answer), not a verdict.
- Smaller, each found by a gate rather than a reader: a copied front-matter quote carries
  **en dashes** (NoEmDashInCopyTests reads comments); a **glossary definition may not
  rank** a finding ("slowly get better" failed on "better" and "slow"); a **subsection
  heading is reader text**, so "### Can it be cured?" is cure-family text outside the
  gate; a front-matter comment that says a source "is NEVER cited for a grade" can cite
  it for one four lines later; and the scouting brief's own StatPearls ID was a hernia
  chapter.

**WI-537 — `/tumors/medulloblastoma`, and the item where the tests kept protecting the
defects.** Grade **5.6**, **2117 tests** (2065 before), ContentCheck **271/0**, **166
break-mutations caught on LF and CRLF**, nine render guards proved by rebuild, **four
rendered reads**, **three `/review` rounds (2 blockers, then 1, then none)**. Sources: 62
files, 225 script-checked quotes. New block: `blocks/posterior-fossa-syndrome.md`.

- **THREE ROUNDS, THREE TESTS PINNING A CLAIM THAT WAS WRONG. This is the item's main
  lesson.** Round 1: a test asserting an **unsourced** sentence — the page claimed
  posterior fossa syndrome happens "more so than after most other operations there", a
  comparative no cited source makes and which St. Jude contradicts in kind ("also can
  happen after surgery to remove other kinds of brain tumors in the posterior fossa. Those
  include astrocytoma and ependymoma"). Round 2: a test asserting an **unattributed**
  claim — the medulloblastoma bullet added to `/treatments/chemotherapy` traced to nothing
  in THAT page's front matter, whose ten sources were all adult or drug-specific. Round 3:
  a test asserting an **overstated** claim — "**most often** cisplatin, vincristine and
  cyclophosphamide", a frequency ranking of regimens the sources do not make (they say
  "Common regimens include", "typically", "include"). §12.10's existing traps are about
  asserting the wrong THING (presence where position was the property); this is asserting a
  thing that should not be on the page at all, and the guard then makes it harder to
  remove. **The question that catches all three: if this sentence were wrong, would this
  test tell me, or would it enforce it?**
- **A CLAIM CAN BE TRACED ON THE PAGE THAT MAKES IT AND UNTRACED ON THE SIBLING YOU
  EDITED.** This item diagnosed and fixed exactly that defect for its new block after a
  rendered read, then shipped it on `/treatments/chemotherapy`, because the check was run
  on the block and not on the sibling page the same change touched. §12.2 item 2 is
  per-page: when an item adds a claim to a sibling, that sibling's own front matter is the
  thing to check.
- **A WORD CAN COLLIDE WITH ITSELF ACROSS A PAGE, AND PINNING BOTH SENTENCES ENFORCES THE
  CONTRADICTION.** "Grade 4 ... describes the tumor's **kind**" used "kind" to mean *type
  of tumor*, while every other use on the page means *molecular group* ("the kind changes
  the treatment plan", "which kind the tumor is decides who gets offered testing"). Read
  with the page's own vocabulary it claimed the grade tells you the group, which the
  section denies six lines later — in the section a frightened parent reads first, with
  both sentences pinned by tests. No gate can catch this. Only reading the page as a reader
  does.
- **BANNING VOCABULARY IS NOT BANNING THE CLAIM, AND A FIX CAN RECREATE THE DEFECT IT
  FIXED (§12.14, twice in one item).** Round 1 replaced an ungated "most children with that
  kind do well" with "because that kind responds to today's treatment" — the same
  group-level outcome claim with the adverb removed, which then slipped past the "responds
  well/best/better" bans round 1 had just extended. And the page's outlook gate carries the
  same claim a third way, as "Treatment works well enough for the WNT kind", which no
  vocabulary regex was going to see. Guard **position** (this claim family belongs inside
  `:::outlook`), not phrasing. The warning now rests on a fact instead: every
  medulloblastoma is grade 4 whatever its kind, so the number cannot be about one child.
- **THE CHARACTERISATION LIST GROWS AN INFLECTION HOLE EVERY FEW ITEMS, AND THE FIX CAN
  OVERSHOOT.** "do well" was missing beside "does better"/"responds well"/"responds best",
  the third such hole after "responds best" (WI-517) and "aggressive" (WI-528). But as a
  bare substring it fails the standard this list applies to everything else: these
  pediatric hubs are written in a school register where "do well at school" is the natural
  sentence, so it belongs in `RejectedCharacterisations` with the reason, like "went well"
  and "back to normal".
- **`Assert.Contains(url, frontMatter)` PASSES ON A COMMENT.** Three attribution assertions
  would have stayed green with the `- url:` entry deleted and the URL surviving in a nearby
  comment — and this corpus really does mention URLs in front-matter comments, so it is a
  live way to lose a citation silently. Count `- url:` ENTRIES, and separately assert the
  COMPOSED source list, because reaching the reader's source list is the property.
  Relatedly: **`Compose(...).Sources` is block sources only** and does not include the
  page's own front matter, so a test asking it for a page source fails — or worse, passes
  by accident on a hub where some block cites the same URL.
- **A SECOND HUB NEEDING THE SAME PASSAGE IS A BLOCK, AND THE CARVE-OUT IS WHY IT IS ONE.**
  What makes the posterior fossa paragraphs a block rather than two copies is the ambulance
  carve-out: `[ESCALATION]` composes higher up every including page and files "Suddenly not
  being able to speak" as an ambulance call, so a page describing a syndrome whose first
  sign is exactly that must say the ambulance rule still stands. Factored, every includer
  gets it for free. Scoped like `blocks/spinal-cord.md`, not folded into `escalation.md`:
  it is about one operation, not every tumor.
- **FACTORING CAN SILENTLY DROP A HEDGE.** WI-536's page-local wording was "**usually**
  starts in hospital". The block lost "usually", which both contradicted the block's own
  next paragraph (it tiers onset AFTER you are home) and widened an unhedged claim onto a
  second hub. §12.10: a hedge in a block is scope, and the scope grows with every includer.
  Diff factored prose against the original word for word.
- **FIXING ONE HUB'S SOURCE LIST CAN BREAK THE OTHER'S.** A rendered read showed the only
  support for the block's complication sentence on `/tumors/medulloblastoma` was a
  StatPearls chapter titled "Ependymoma", so the medulloblastoma cohort study was added to
  the block — creating the mirror problem on `/tumors/ependymoma`. Moving the impairment
  sentence INTO the block was the tempting fix and the wrong one: that finding is
  medulloblastoma survivors, and a block asserts its prose on every includer. The answer is
  a **tumour-neutral** source whose title is true on every hub, with the specific one kept
  page-local.
- **A TOOLTIP CAN FIRE TWICE INSIDE ONE NAME, AND ONLY A RENDERED READ FINDS IT.** The page
  suppressed the LONGER glossary term on the theory that longest-name-wins protects the
  shorter one. It does, at one position. The page names "posterior fossa syndrome" TWICE
  (the section, and the support list signposting St. Jude), tooltips are
  first-occurrence-only, so by the second mention the longer term was spent and the bare
  "posterior fossa" matched inside the phrase. Suppress the SHORTER term.
- **PROMOTING A GUARD EXPOSES HOLES THAT WERE INVISIBLE ON A RAW PAGE.** The normalisation
  guard had to move to the COMPOSED page because the reassurance it checks moved into the
  block, and read raw it went green by losing its subject. Composed, it fired immediately
  on `[ESCALATION]`'s own bullet "A headache much worse **than usual**", where "usual" is
  part of the warning rather than a reassurance about it, and it could not see that a **list
  item's tier lives in its lead-in paragraph** — the same structural point
  `AssertNoEscalationList` records from the other direction. It is
  `CuratedPage.AssertNoWarningSignIsNormalised` now, with a mandatory positive half.
- **REVIEW QUOTES FAIL IN BOTH DIRECTIONS, THREE TIMES IN ONE ITEM, AND THE HYPHEN FAMILY
  IS WIDER THAN §12.8 SAID.** A verifier that folded U+2013 and U+2014 but not **U+2010**
  reported a CNS5 phrase absent and then crashed printing it: fold U+2010 through U+2015. A
  review then asserted a drug trio was "verbatim ONLY in an adult paper", which was false —
  it is in at least four pediatric files, including a table row for the exact under-3
  radiation-sparing context the sentence was about, and following the claim would have cost
  the better citation. **Verify a reviewer's negative claims, not only its positive ones.**
  And a **recorded justification deserves the same check as a claim**: this item wrote a
  ban-list reason quoting a sentence that is not on the page and never was.
- **AN ANALOGY CAN MOVE A THRESHOLD, AND THE DIRECTION IS THE WHOLE QUESTION.** "A piece
  bigger than a small coin" stood in for a residual-tumor threshold that is an AREA of
  1.5 cm². A dime's face is about 2.5 cm², so the analogy raised the bar by roughly seventy
  percent in the REASSURING direction and turned a two-dimensional measurement into a
  three-dimensional "piece". Where §12.4 R1 keeps the figure out, route to the measurement
  rather than inventing a picture of it.
- **CHECK WHETHER A NAME IS ACTUALLY RETIRED BEFORE CALLING IT RETIRED.** The draft said
  group 3 and group 4 "used to be called" that. They are current CNS5 names
  ("Medulloblastomas, non-WNT/non-SHH (Groups 3 and 4)"; St. Jude "Group 3 tumors are the
  second most common"), and the page used them in the present tense thirty-five lines
  earlier. On the report-reading section that told a parent holding a "Group 4" report the
  label was obsolete.
- **AN UNDER-TRIAGE INHERITED FROM THE PREVIOUS ITEM, FIXED ON BOTH HUBS AT ONCE.** Both
  hubs filed "new or worse trouble with walking or balance" as SAME-DAY while
  `blocks/spinal-cord.md`, composed lower on the same page, files the same observable sign
  as RIGHT AWAY for a tumor in or spread to the cord. WI-512's shape, dangerous direction.
  The clause went onto **both** hubs in one change, worded per hub, because fixing only the
  newer page would have created the two-strengths defect the block exists to prevent.
- **HARNESS HYGIENE, TWO NEW WAYS TO FOOL YOURSELF.** A mutation anchored on a LITERAL dies
  silently when the prose rewraps: restoring the "usually" hedge turned one into a no-op,
  which `dryrun.py` caught and a harness run would have reported as `ok`. Locate paragraphs
  by regex. And **do not pipe the harness through a small `tail`** — a 313-line run through
  `tail -180` shows 23 LF lines against 155 CRLF ones and reads like a half-run. Also:
  handproof's served-at-its-URL case passed when mutated because **the route is the file
  path, not the front-matter `slug:`** (`ContentStore.GetPage` maps the url onto
  `Root + urlPath + ".md"`, and `ContentFrontMatter.Slug` is read by nothing in the
  routing) — a weak MUTATION, not a weak guard, and reading it the other way would have
  meant "fixing" a healthy test.
- Smaller, each caught by a gate or a read rather than by a person: an **en dash in a copied
  front-matter quote** (NoEmDashInCopyTests reads comments); **"Straight after surgery"**, a
  British idiom §12.10 names and no spelling gate catches; **restating a sibling's owned
  sentence**, caught by `/treatments/proton-therapy`'s own restatement test; and a
  four-"or" tier sentence that could not be split because splitting breaks the
  same-day/shunt adjacency the shunt guard requires, so it took a comma instead.
- **A PRE-COMMIT PRIVACY SCAN THAT GREPS FOR A TOKEN WILL FLAG THE SENTENCE REPORTING THAT
  THE TOKEN IS ABSENT.** This item's scan cost three triage passes on its own output: three
  hits on the literal word "secrets" inside the sentence "Privacy/secrets scan done"; one on
  the `sk-` key pattern stemming into "ri**sk-s**tratification" in a source title; one on a
  GitHub account name that survived only in the sentence recording its removal; and one on a
  credentials filename written while reporting that no such file was in the diff. Two rules
  come out of it. **Compare net-new against HEAD, per file** — whole-file counts invent
  findings (a long-standing file mentioning a name ninety times reads as ninety new ones),
  and a raw added-line grep is useless on `PROGRESS.md`, whose single-line table rows show
  as wholly "added" the moment any part of them is edited, so every pre-existing mention
  inside the row looks new. And **do not narrate a redaction using the literal token**, or
  the repository inherits a permanent false positive for the next item to re-triage.

**WI-538 — `/tumors/pediatric-brain-tumor`, the first page in the corpus whose reader
is not the patient.**

- **THE SOURCE PACK'S HEADLINE FACT WAS ALREADY SHIPPED ON A SIBLING, AND THAT IS
  WI-529'S FIRST LESSON LANDING AGAIN.** The scouting brief called "pediatric-type
  describes the tumor's biology, not the reader's age" the clearest single fact for
  this page. `/tumors/glioma` already says it almost verbatim, and
  `glossary/pediatric-type.md` says it a second time. Five more route-versus-own
  checks came back OWNED: the radiation mask and planning visit
  (`/treatments/radiation-therapy`'s most detailed section), the post-operative drain
  (`/treatments/craniotomy`, nearly word for word the sentence the source pack
  offered), proton cost, insurance and travel (`/treatments/proton-therapy`, which
  carries explicit no-cost-multiplier and no-center-count rulings), when to stop
  eating before surgery (`/tests/getting-ready-for-surgery`), and the shunt warning
  list (`/treatments/shunts`). What was genuinely unowned is the PARENT'S position:
  medicine to sleep for a scan (`/tests/mri` has no sedation content at all), daily
  anesthesia for radiation, telling your child and telling their brothers and
  sisters, school law, and **chemotherapy safety at home, which no page in the corpus
  mentions**. Read the siblings before the source pack, or you write the source pack.

- **AN AUTHORING MARKER CAN SILENTLY DISABLE A SIBLING'S TEST, AND THE MECHANISM IS
  RAW TEXT SUBTRACTED FROM READER TEXT.**
  `MedulloblastomaPageContentTests.TheRetiredNameAppearsOnlyAsRetiredAndTheTrialNames
  AreNotDiagnoses` builds its `retired` string from the RAW section and subtracts it
  from `ReaderText(Page)`. `ReaderText` strips `!%…%`. The moment this item put a
  suppression marker inside that section, the raw string stopped being a substring of
  the reader text, `.Replace(retired, "")` removed NOTHING, and the PNET sentence the
  subtraction exists to take out of scope stayed in it — so a correct page went red.
  A marker is page-wide, so its position is free (WI-529): move the marker rather
  than weaken another item's test. Worth recording because the fragility is still
  there for any guard that mixes a raw section with reader text.

- **"AN ALL-ZERO PROBE RUN IS PROOF THE PROBE IS BROKEN" IS AMBIGUOUS AS WRITTEN, AND
  THE AMBIGUITY POINTS THE DANGEROUS WAY.** §12.8 carries that rule from WI-537, and
  this item implemented it literally in the post-deploy smoke: if no positive fragment
  matched anywhere, print "PROBE BROKEN -- do not report a failed deploy". Then the
  smoke was run BEFORE deploying, as a negative control, and it did exactly that. But
  nothing had deployed, so the truthful answer was "the deploy has not landed". **A
  totally failed production deploy would have been reported as a broken test, and the
  real alarm suppressed** -- the precise inversion of what a smoke exists for. Zero
  matches has two causes and the rule named only one.
  **The fix is CONTROL FRAGMENTS: text present BOTH before and after the change**, on a
  page the item barely touches. Then the cases separate: controls miss -> the probe is
  broken; controls hit while the new fragments miss -> the deploy did not land. The
  corrected run reports controls 2/2 and exits on real failures instead.

  **And the practice that found it is worth more than the fix: RUN THE SMOKE BEFORE
  DEPLOYING AND REQUIRE IT TO FAIL.** A smoke that has never failed is one nobody has
  tested, and it will be trusted at exactly the moment it is least examined. The
  pre-deploy run also proved each probe can FIRE -- including the negative probe, which
  correctly reported the unsourced "it is treatable" still live in production. A
  negative probe that has never once matched is indistinguishable from a typo.

- **A FRONT-MATTER COMMENT TURNS A FIRST-OCCURRENCE MUTATION INTO A NO-OP, AND THE
  HARNESS THEN REPORTS A SURVIVING GUARD.** Twice in this item, and both decoys were
  comments this item wrote itself. `rep(old, new)` replaces with `count=1`, so a
  mutation lands on the FIRST occurrence of its anchor. Run 1: the anchor
  `'does not apply.'` first appears in the `[SPINAL-CORD]` ruling in YAML. Run 2: the
  bare marker `!%embryonal tumor%` first appears in the comment recording where that
  marker lives and why it is not beside the term. In both cases the file CHANGED — which
  is all `dryrun` verifies, since it only asks whether a mutation is a no-op on the file
  — while the text the guard reads was untouched, so the guard stayed correctly green and
  the harness printed a SURVIVOR. **A survivor reads identically whether the guard is
  weak or the mutation is. Ask which occurrence the anchor actually hit before touching
  the guard** — the wrong diagnosis here weakens a guard to accommodate a broken probe.
  Anchor every mutation on text that is unique in the BODY.

  **And the same comment made the GUARD weak in the opposite direction.** The assertion
  read the RAW file, so the front-matter comment ALONE satisfied it: delete the real
  marker, keep the sentence explaining it, and the check still passes. WI-537 recorded a
  URL in a comment making a check pass for the wrong reason; this is that, in a second
  place, on a page's own authoring marker. Now scoped to the body — and it must use
  `Body`, **not** `ReaderText`, because `ReaderText` strips the very markers it is
  asserting on. The duplicate literal is gone from the comment as well, so the trap
  cannot be re-armed by someone re-adding the mutation's old anchor.

  **AND THE TRAP IS NOW CHECKED MECHANICALLY RATHER THAN REMEMBERED**, which is the
  durable half of this lesson. `anchor-audit.py` asks each mutation's OWN CLOSURE where
  it will actually bite — `rep` closes over `old`, while `ws` and `after` close over a
  COMPILED pattern that can locate its own first match across a hard wrap exactly as the
  mutation will — and compares that offset against the end of the front matter. All 81
  audited: 3 bites land in YAML, every one named `front-matter-*` and deliberate.
  **It also covers the half the harness structurally CANNOT see.** The harness only
  reveals a front-matter bite when the guard reads the body, because then the guard
  correctly stays green and a survivor is printed. When the guard reads the RAW file,
  the same broken mutation still turns it red — looking perfectly healthy while proving
  nothing about the prose it names. No run, of any length, would ever have surfaced
  that; only asking the mutation where it lands does.

- **A FILTERED HARNESS RUN REPORTED THE ENTIRE TABLE AS PROVED.** `break-tests.py`
  filtered inside its loop and then printed `len(MUTATIONS)` unconditionally, so a
  run of ONE mutation ended with "all 70 breaks fail correctly, on LF and on CRLF".
  §12.8 already records not piping the harness through a small `tail` because that
  reads like a half-run; **this is the dangerous direction, because a partial run
  reads like a complete one**, and WI-536 and WI-537 both used filtered runs. Select
  first, report what actually ran, and make a filtered run say so in its own summary.

- **THE CORPUS RESTATEMENT CHECK HAS THREE OUTCOMES AND ONLY ONE OF THEM IS
  "REWORD".** It found 17 collisions, then 9 more after the first round of fixes.
  (1) REAL DUPLICATION — the caregiver slice repeated the `[CAREGIVER]` block
  composed twelve lines above it, which is precisely the defect WI-529's rendered
  read found in this same section, caught mechanically this time; reworded, and the
  page now carries two per-page claims instead of three. (2) DELIBERATE SHARING THAT
  MUST NOT BE REWORDED — the shunt clause is identical on four hubs because §12.10
  wants one claim at one strength, and WI-537 recorded that changing either side
  creates the two-strengths defect the block exists to prevent; it belongs in the
  helper's allowlist. (3) PRESCRIBED VOCABULARY — §12.5 dictates what an outlook gate
  must teach, so a new gate collides with all eight existing ones; rewording beat
  allowlisting there because it also sharpened what makes this gate different (on a
  cross-cutting page, outlook belongs to the tumor's name and not to an age group).
  **The allowlist is for claims that MUST match, not for sentences that happen to.**

- **REPLICATE AN EXPENSIVE GUARD OFFLINE WHEN YOU ARE ITERATING AGAINST IT.** The
  restatement check is pure text analysis over markdown files, so a short Python
  replica of `CuratedPage.Shingles` plus the intersection answered in under a second
  what a 90-second suite cycle was answering. It reported 0 collisions across 2,883
  shingles and the real suite then agreed. Two more rounds of reword-and-hope became
  none.

  **AND VALIDATE THE REPLICA AGAINST A KNOWN-CLEAN RUN BEFORE TRUSTING IT — THIS ONE
  FAILED THAT CHECK.** Saved as a script at the end of the item, it was first run
  against a page the suite had JUST reported clean. It claimed collisions with EIGHT
  other pages, and every colliding phrase was shared-BLOCK prose: "a tumor and the
  irritated brain around it" against `/seizures/what-to-do`, "at any hour call your
  team or go" against `/treatments/shunts` and `/treatments/craniotomy`. The cause is
  one word in the call site. The guard is invoked as
  `AssertDoesNotRestateTheCorpus(Page, Slug, ...)` with the **RAW** page, so
  `[MECHANISM]` stays a literal directive and the blocks' words are never in the
  comparison at all. Composing the page first — which seemed so obviously right that
  the first draft did it without asking — injects every shared block and then collides
  with every hub that includes the same ones. It counted 5,018 shingles where the suite
  sees about 3,000, which was the tell. **A replica that disagrees with its guard does
  not fail loudly; it sends you rewording innocent sentences,** at the same cost as the
  three false negatives recorded below and with more apparent authority, because it
  looks like a measurement. Validate against a known result before the first real use;
  and if the guard takes an allowlist, copy its entries from the TEST rather than from
  memory, because entries are matched by CONTAINMENT and a half-remembered one silently
  suppresses nothing.

- **READ-ONLY PREDICTION BEATS WAITING FOR THE FAILURE OUTPUT.** Two defects were
  found by grepping the item's own new files while a suite run was in flight and
  editing was unsafe: **`practise` twice in reader text** (a corpus-wide guard that
  lives inside `RadiationTherapyPageTests.cs` then confirmed it), and three
  assertions anchored on a literal `\n`, **one of which could never match because its
  continuation line is indented two spaces inside a list item**. Reading the page's
  actual hard wraps turned a whole failure cycle into zero.

- **A CLAIM WHOSE SOURCE COULD NOT BE READ IS NOT A SOURCED CLAIM.** The FDA's
  pediatric anesthesia warning is the best-known statement of the under-3 concern and
  it could not be fetched at either of two URLs (the server refuses the fetcher). It
  is cited NOWHERE on the page; St. Jude states the same point first-hand and carries
  it instead. The dead URLs are recorded in the front matter so the next item does
  not re-derive the claim from memory.

- **A PAPER CAN DISAGREE WITH ITSELF ON ONE PAGE, AND THAT IS THE ARGUMENT FOR
  PUBLISHING NO NUMBER AT ALL.** The anesthesia age threshold is "≥ 12" in one
  paper's abstract, "≥13" in its results and "until age 13" in its discussion; a
  second paper uses 4 for general anesthesia and 7 for deep sedation. §12.4's usual
  remedy is to attribute the figure in the sentence that prints it. Here the honest
  output is the SHAPE — younger children usually need it, older children often do
  not, and some older children still do — and the guard bans the ages the sources
  argue over while deliberately allowing the sourced under-3 developmental point,
  which is a different claim about a different thing.

- **A SOURCE CAN BE BARRED FOR NAMING AND STILL BE ESSENTIAL, AND IT CAN CONTRADICT
  ITSELF INSIDE ONE PAGE.** ACS's children's types page puts "Pediatric-type diffuse
  high-grade gliomas" (CNS5) and "grade III or IV" (pre-CNS5) in the same sentence,
  and uses Arabic grades in its own explainer directly above the Roman ones in its
  list. Resolved per-claim and written into the front matter where the next reader of
  it will see it (§12.13). **Cancer Research UK went further and is cited nowhere at
  all**: its children's pages are the best written in the set, and its idiom sits in
  the exact sentences this page wants — "GP" thirteen times, and the symptom list
  itself written as "feeling or being sick".

- **THE SHARED `[ESCALATION]` BLOCK IS ADULT-SHAPED, AND A PARENT PAGE IS WHERE THAT
  SHOWS.** Nothing in its tiers covers a head growing too fast, a bulging soft spot,
  or a baby who is only irritable. §12.10's remedy applied literally: include the
  block, then add the page's own tier BENEATH it, carrying the mechanism that makes
  the list make sense (a baby's skull is not fixed shut, so pressure can build for
  longer before anything shows) and the sign that stands in for a child too young to
  say it hurts — a worsening headache showing up only as a child who is anxious,
  irritated or whiny.

- **THE CAREGIVER SECTION INVERTS ON A PAGE ALREADY WRITTEN TO THE CAREGIVER.**
  Everywhere else `[CAREGIVER]` is the one part addressed to somebody other than the
  patient; here the whole page is, so the section says so out loud rather than
  reading as an editing mistake. Its per-page claims then have to be diffed against
  the block, which is exactly where one of them turned out to be the block's own
  advice repeated.

- **A PAGE'S OWN SOURCING RULING CAN BE FALSE OF THE RENDERED PAGE, BECAUSE BLOCK
  SOURCES REACH THE READER'S SOURCE LIST.** This page's front matter declared, at
  length and with reasons, that "Cancer Research UK is not cited at all" — and the
  rendered page shows the reader TWO CRUK entries, one of them under the visibly
  British title "Brain tumour symptoms". They arrive from `blocks/escalation.md`.
  §12.10 already records that block sources merge in and render; the new half is that
  a page's front-matter JUSTIFICATION about its own sourcing is a claim about the
  COMPOSED page, and a test that reads only the page's own front matter will confirm
  the wrong half of it forever. The claim is now "nothing on this page is TAKEN from
  CRUK", and the test reads the BLOCK, so if the block ever drops CRUK the comment
  goes red instead of quietly becoming a half-truth. WI-536's "a recorded
  justification deserves the same check as a claim", found this time by a rendered
  read rather than by review.

- **AN EXISTING GLOSSARY TOOLTIP CAN RESTATE THE SENTENCE YOU JUST WROTE.** WI-535
  recorded one direction: a NEW entry echoing the sentence it lands in. This is the
  mirror, and it is easier to miss, because the entries were already shipped and
  correct. The page glossed "pediatric-type" and "adult-type" in its own words, both
  terms already have entries saying the same thing, and the rendered paragraph
  therefore said it three times in a row. §12.8's WI-509 rule resolves it — where a
  page defines a word the glossary also defines, suppress it there — but the rule
  only fires if you read the page RENDERED, since the source markdown looks clean.

- **THE §12.8 SELF-RESTATEMENT CLASS RECURRED AGAIN, AND AGAIN ONLY THE RENDERED READ
  SAW IT.** The page's self-blame closing landed on "turning this over at three in
  the morning", four lines under the `[CAUSES]` block's own "turning this over at
  three in the morning". The wording differs enough that the corpus shingle check
  cannot see it — which is precisely what WI-529's rendered read found in its
  caregiver slice. A composed block plus a page's own slice will keep producing this,
  and the only detector is reading the two together as a reader does.

- **A SHARED BLOCK'S VOICE NEEDS SCOPING, NOT ONLY ITS CLAIMS.** The scoping note
  above `[MECHANISM]` handled "your tumor". But `[ESCALATION]` composes six screens
  lower and its same-day tier is also written to the patient — "a headache that wakes
  **you** from sleep" — and that is the most safety-critical content on the page. A
  parent should not have to carry a translation that far, so the page now carries a
  one-line reminder directly above the directive. WI-529's lesson said a scoping note
  must cover the block's FRAMING as well as its vocabulary; the extension is that it
  must cover **every** patient-voiced block the page includes, at each one.

- **A POINTER TO A BLOCK NATURALLY ECHOES THE BLOCK, WHICH IS EXACTLY WHAT THE
  NON-DUPLICATION GUARD FORBIDS.** One sentence — telling a parent that some signs
  carry a different rule if the tumor reaches the spine — took THREE attempts to word,
  and both failures were the same mistake. The obvious way to point at a conditional
  block is to restate its condition, so the first draft opened "for a tumor in the
  spinal cord, or one that has spread" (collided with `/tumors/ependymoma`) and the
  second opened "is in the spinal cord, or has" (collided with `blocks/spinal-cord.md`
  itself, the very block it points at). **A page that routes to a block is structurally
  drawn toward the block's own opening clause.** Write the pointer from the READER's
  situation ("if the tumor has reached your child's spine") rather than from the
  block's wording, and expect the restatement check to referee it.

  **And the same pull can arrive as a well-meant consistency request.** A `/review`
  nit asked the cord lead-in to match the block's anatomy — "in the spinal cord" rather
  than the looser "sits in the spine" — which is correct on the medicine and imported
  the block's exact opening clause along with it. The collision that followed was then
  misdiagnosed twice, because the obvious suspect was the pointer. **Matching a block's
  anatomy is not the same as matching its sentence:** take the vocabulary, not the
  clause, and re-run the corpus check after any edit made for consistency with a block.

  **AND THE THIRD FORM OF THE SAME PULL IS A SIBLING'S TRUE SENTENCE BECOMING THIS
  PAGE'S FALSE ONE.** The pointer told a parent the cord rule came "at the end of this
  section" — which is exactly what `/tumors/ependymoma` and `/tumors/medulloblastoma`
  say, and it is TRUE on those pages, because the block is last there. Here three
  paragraphs follow it (the scoping confirmation, the baby tier, the pre-verbal tier),
  so a parent taking the instruction literally scrolls past the rule being pointed at
  and lands on the baby paragraph. **Copying a sibling's shape copies its
  PRECONDITIONS, and nothing checks them.** Caught by `/review` round 3; now banned by
  a guard rather than merely corrected, because the siblings still say it and copying
  them is how it arrived.

- **CHANGING A SENTENCE MEANS FINDING EVERY REFERENCE TO IT, INCLUDING INSIDE ONE
  TEST.** Updating the voice reminder's two `Assert.Contains` strings left an
  `IndexOf("The lists that follow")` four lines below them searching for text that no
  longer existed; it returned -1 and the position assertion failed. Same class as the
  eight dead mutation anchors earlier in this item — prose moved, a second reference to
  it did not — but inside a single method, where it is easiest to believe you have
  finished. Grep the old phrase after every reword, not just the file you were editing.

  **AND GREP THE WHOLE SENTENCE, NOT A MEMORABLE PIECE OF IT.** `/review` round 3 asked
  for the cord pointer to be reworded. Before touching it I swept the repo for "at the
  end of this section" — its SECOND sentence — found the mutation anchor, moved that,
  and believed the sweep complete. The guard quoted the FIRST sentence ("Some of these
  signs carry a different rule") and went red on the next run. **A search for part of a
  changed sentence is not a search for the sentence:** take the head AND the tail, or
  normalise the whole thing and search that. This was the fifth false negative of the
  item and the fourth distinct mechanism — line wrapping, then wrapping again, then
  punctuation, then a too-narrow fragment — which is the argument for making the search
  mechanical rather than remembering four separate traps.

- **A SCOPING FIX CAN FAIL BY NOT REACHING FAR ENOUGH, AND ONLY THE NEXT RENDERED
  READ SHOWS IT.** Read 1 found `[ESCALATION]`'s tiers in patient voice and added a
  reminder directly above the directive: "the list below is written to the person who
  has the tumor". Read 3 found that same defect one block further down. Composed, this
  section carries TWO patient-voiced blocks, and between the reminder and the second
  one a parent passes both tiers, both fever rules, the shunt rule and two sign-off
  links — "the list below" had plainly ended, and the cord block then opens "**For
  you**, an arm or a leg that is newly weak". §12.14 says a fix can recreate the
  defect it fixed; this is the quieter version, where the fix is correct and merely
  too narrow. **Scope to the SECTION, not to the next paragraph, and count how many
  composed blocks the reader actually passes.**

- **I READ A POSITION GUARD AS A WIDER CONSTRAINT THAN IT IS, AND THE SIBLINGS ALREADY
  HAD THE ANSWER.** `SpinalCordBlockTests` forbids anything BETWEEN `[ESCALATION]` and
  `[SPINAL-CORD]`. I recorded that as "the page-owned lead-in CANNOT precede the
  block", and wrote that reason into the page's front matter, a test comment, the
  progress log and this file. It is false: the guard says nothing about a
  forward-pointing sentence earlier in the section, and **both sibling hubs carry
  exactly that** — "those signs have their own rule, at the end of the section on when
  to call for help, below." So the scoping could always have arrived before the alarm,
  and a parent was meeting "**For you**, an arm or a leg that is newly weak" before
  learning the rule might not be theirs. `/review` round 2 found it.
  **Read what a guard actually asserts, not what its subject suggests — and when a
  shared block constrains layout, check how the hubs that already include it solved
  the same problem.** The page now points forward before the block and confirms after
  it, which is the sibling shape.

- **A REASSURANCE WRITTEN TO FIX AN OVER-WARNING BECAME AN UNSOURCED CLAIM POINTING THE
  WRONG WAY.** Round 1 asked whether the section over-warns; the answer added "Most
  childhood brain tumors never do" to the sentence that decides whether the right-away
  cord rule is yours. No source in the set quantifies how often childhood tumors reach
  the spine — and the page's own cited ACS source points the other way for the tumors
  it teaches: embryonal tumors "tend to grow quickly and **often spread through the
  CSF**", with "embryonal tumor" glossed as the childhood word two sections earlier.
  §12.14 again, in its most dangerous form: a fix for a tone problem created a
  §12.12 over-reassurance on a triage sentence. It now says only that some childhood
  tumors can reach the spine and some cannot, with a guard banning the frequency shape.

- **`ReaderText` ON A SECTION SILENTLY EATS THREE CHARACTERS, AND IT IS LIVE ON FOUR
  OTHER PAGES.** §12.8 (WI-520) records `ReaderText(Body(page))` as the trap; the same
  helper applied to a SECTION is the same bug, because it strips front matter by
  seeking `\n---`, finds none, and returns `body[3..]`. It does not throw, and it does
  not fail a `Contains` assertion anchored further in, so a test written this way goes
  green carrying a latent defect — **this item's did, on the very run that was meant to
  catch it.** Found by reading my own test, not by any gate.

  A sweep of the whole test directory then found **six live instances on four shipped
  files**, all currently green: `ReaderText` on a section in `BiopsyPageTests` and
  `SteroidsPageTests`, and the documented `ReaderText(Body(page))` form three times
  each in `AstrocytomaPageTests` and `HighGradeGliomaPageTests`. The rule is written
  down in `TestsLibraryPagesTests` and was re-committed four times anyway, which says
  the helper's shape is the problem rather than anyone's care: `ReaderText` accepts a
  string and cannot tell a page from a section.

  **Raised as its own backlog item rather than fixed here, and NOT filed under the
  existing idiom sweep.** WI-538 is a content item; those are six pre-existing guards
  on four unrelated pages, each needing its own harness proof. The nearest open sweep
  (WI-564) is specifically about British idiom — WI-537 appended to it legitimately
  because that entry already named the exact word and file it was adding, and there is
  no equivalent justification here. Filing a test-helper bug under an idiom sweep
  because it is the nearest open item is how a defect gets lost. The durable fix is to
  make the helper refuse — throw when handed a string with no front-matter delimiter,
  so the -1 branch cannot silently return `body[3..]` again. **Compose, then section,
  then flatten; `ReaderText` belongs on a whole page or not at all.**

- **QUOTING A SHARED BLOCK IN A FRONT-MATTER COMMENT TRIPS THAT BLOCK'S OWN
  NON-DUPLICATION GUARD.** Explaining *why* the cord block is included meant quoting
  its opening clause in the page's front matter — and
  `SpinalCordBlockTests.EveryIncludingHubComposesTheRuleAndNoneRetypesIt` reads the
  RAW file, front matter included, so the page "retyped" a sentence no reader will
  ever see and a correct page went red. WI-537 recorded the same surface making a
  check PASS for the wrong reason (`Assert.Contains(url, front)` satisfied by a URL in
  a comment); this is it making one FAIL for the wrong reason. Two fixes were
  available — paraphrase the comment, or move the guard onto `ReaderText` — and the
  paraphrase won, because loosening a guard four hubs depend on to accommodate one
  page's commentary is the larger risk. **A front-matter comment is inside the string
  every raw-file guard reads. Paraphrase the corpus; do not quote it there.**

- **MY OWN VERIFICATION GREP PRODUCED A FALSE NEGATIVE, AND I NEARLY DISMISSED A
  CORRECT FINDING ON IT.** §12.8 already records that REVIEW quotes fail in both
  directions, so this item verified every load-bearing claim before acting — and two
  of `/review` round 1's claims did not survive that check, which looked like the
  rule working. One of them was my error, not the reviewer's: a sweep for an
  ependymoma age comparative across all 100 source files returned one irrelevant hit,
  and I was one step from recording the finding as unsourced. Re-checking with a
  different pattern found the sentence exactly where the reviewer said it was.

  **It then happened a SECOND time in the same item, and the second one cost more.** A
  grep for the siblings' forward-pointing scope sentence returned nothing, so I recorded
  that a page-owned lead-in "cannot precede" the cord block — and wrote that false
  reason into the page's front matter, a test comment, the progress log and this file,
  where it stood for several rounds until `/review` round 2 quoted the sentences back at
  me. Both hubs carry them.

  **It happened a THIRD time, and the third one proves the rule is broader than
  wrapping.** Hunting the last restatement collision, a whitespace-tolerant search for
  the offending 8-gram reported it absent from the page — so I reworded a different
  sentence, twice, while the real culprit sat untouched. The page says "tumor is in the
  spinal cord**,** or has"; my search joined the words with `\s+` and a COMMA is not
  whitespace. `Shingles` strips every non-letter before matching, so the guard saw a
  collision my search structurally could not.

  **The root cause is mechanics, and the rule is: normalise the way the GUARD
  normalises.** Not "tolerate line breaks" — letters only, lowercased, exactly as
  `Shingles` does (`[^A-Za-z] -> ' '`). A verification that is more literal than the
  check it is verifying will keep exonerating the guilty sentence. This corpus is
  hard-wrapped AND punctuated, and a line-based or whitespace-only search cannot match
  a sentence across either. Both misses were
  multi-line sentences. The corpus's own tooling already knows this — `CuratedPage`
  flattens before every prose assertion, `ws()` in the mutation table joins anchor words
  with `\s+` for exactly this reason, and §12.8 records a canary that failed because the
  phrase it aimed at was wrapped between two words. The verification habit had simply
  not inherited it. **Search prose whitespace-tolerantly (flatten first, or join the
  words with `\s+`), and treat an empty result against a specific quoted claim as
  evidence about the search until proved otherwise** — the same shape as the
  broken-probe and dead-scan lessons below.

- **A RECORDED REASON CAN BE WRONG ABOUT THE BLOCK RATHER THAN ABOUT THE PAGE.**
  WI-536 established that a justification deserves the same check as a claim; those
  cases were all reasons that misdescribed the PAGE. This one misdescribed a shared
  BLOCK: the item excluded `blocks/spinal-cord.md` on the grounds that it "asserts a
  cord rule outright" and therefore fails §12.10's "hub you have thought about least"
  test. It does not assert anything outright — it opens **"If the tumor is in the
  spinal cord, or has spread to the spine"**, which is the CONDITIONAL form §12.10
  (WI-563) explicitly says survives that test, and which the same page used two
  sections later to justify including `[POSTERIOR-FOSSA-SYNDROME]`. (The stale version
  of that same argument survived in this file to the very end of the item — see the final
  bullet.) The page argued
  both sides of one rule and nothing caught it, because the test pinned the
  conclusion rather than the reasoning. Cost: a pre-diagnosis parent whose child had
  new leg weakness met the shared block's SAME-DAY tier where the corpus files that
  reader RIGHT AWAY — §12.10's dangerous direction. **Before excluding a block, quote
  its opening clause and check whether it is conditional.**

- **A SHARED GUARD CAN ENCODE A PROXY INSTEAD OF ITS PROPERTY, AND THE FOURTH
  INCLUDER IS WHERE THAT SHOWS.** `SpinalCordBlockTests` asserted the including hub's
  section heading was literally "## What symptoms does it cause?" while its own
  comment said the property was "the section is the symptoms section". Three hubs
  written for patients shared that wording, so the proxy held. The first includer
  written for a PARENT heads it "What symptoms look like in a child", and the literal
  assertion would have forced it to adopt a patient-voiced heading or drop a safety
  block. Generalised to `^## What symptoms`, which is still tight and is the property.

- **A REVIEW NIT CAN BE ACTIVELY WRONG, AND THE HARNESS CAN PROVE IT.** Round 1 asked
  for the sibling page's suppression marker to be moved next to the term it
  suppresses, which reads better and is what the other page does. It is also exactly
  what broke that page's own retired-name guard earlier in this item, for the
  raw-versus-reader-text reason recorded above — and the harness already carries a
  mutation that puts the marker back to prove the failure is real. **Rejected with the
  mutation as the evidence**, and the reason written into the sibling's front matter so
  the next reader does not re-propose it. A nit that reads as tidying can be a defect
  in a costume.

- **A PRE-COMMIT SCAN THAT DIES MID-RUN REPORTS NOTHING, AND IT LOOKS LIKE A
  RESULT.** The privacy scan printed "files in the diff: 6" and then threw:
  `subprocess.run(..., text=True)` decodes with the Windows default cp1252, which
  raised `UnicodeDecodeError` on a tracked file containing `§`, killed the reader
  thread, returned `None`, and crashed `re.findall`. Every per-file comparison it
  exists to make never ran. **Decode UTF-8 explicitly and never trust a captured
  stream's default codec on Windows** — and treat a scan that produced a header but
  no findings as broken rather than clean, which is the same shape as §12.8's rule
  that an all-zero probe run is proof the PROBE is broken.
  Two design notes that made the corrected run usable, both from WI-537's three
  triage passes: it compares **net-new against HEAD per file** (whole-file counts
  invent findings), and its pattern list deliberately omits the bare word *secrets*,
  which is what made WI-537's scan flag the sentence reporting its own result. Result:
  0 net-new across all 6 changed files, with no false positives to triage.

- **BOTH SHARED BLOCKS ARE INCLUDED — AND THIS FILE SPENT MOST OF THE ITEM SAYING
  OTHERWISE.** The bullet that stood here read "`[SPINAL-CORD]` EXCLUDED,
  `[POSTERIOR-FOSSA-SYNDROME]` INCLUDED, and the difference is whether the block scopes
  itself". It was written before the exclusion was reversed (see the reversal bullet
  above) and then left standing, so the draft argued both sides of the same call for
  several rounds. Pasted, §12.8 would have carried a lesson contradicting the item that
  filed it, and taught the next hub to exclude a safety block from a page whose reader is
  a parent. **The correct statement is that both are included, and both for the SAME
  reason:** each opens with a conditional that scopes itself — "If the tumor is in the
  spinal cord, or has spread to the spine", and "after surgery low at the back of the
  brain" — which is the form §12.10 (WI-563) says survives the "hub you have thought
  about least" test. This page is the third includer of the posterior-fossa block and the
  fourth of the cord block, and the first of either that is not a single tumor. Both
  inclusions are pinned by an exactly-asserted `IncludingHubs` list and by the page's
  closed-set directive guard. **A lessons draft is itself a recorded justification, so
  WI-536's rule applies to it: when a conclusion is reversed, hunt it down everywhere it
  was written — including the file you intend to paste from.** This item wrote the same
  false reason into four places once already and needed `/review` to find it; that is
  twice, and the second time the reviewer was me, one read before pasting.

- **AN ITERATE-AND-CHECK GUARD CANNOT SEE AN OMISSION, AND THAT IS WHERE THE BLOCKER
  WAS.** The pre-verbal paragraph named a warning sign — a worsening headache showing
  up only as a child who is anxious, irritated or whiny — and then stopped, in the one
  section where TIERS are the content. The only instruction in it belonged to a
  different sign (cannot be woken → ambulance), so a parent at 2am with an irritable
  toddler chose between the ambulance sentence beside it and nothing at all. The shared
  block could not rescue it: `[ESCALATION]` does file a worsening headache as same-day,
  but in PATIENT voice, and the page closes that voice two paragraphs earlier. **No
  guard could see it**, and the reason is structural rather than careless:
  `EverySameDayLineThePageWritesItselfHonoursTheShuntRule` SELECTS sentences matching
  `same[- ]day` and then checks each one for the shunt clause, so a sentence carrying no
  tier at all never enters the filter. The guard was green because the defect was an
  ABSENCE. WI-517 recorded that an iterate-and-check guard is green on a page it never
  looked at; this is the sharper version — green on a SENTENCE it never looked at, in
  the most safety-critical section of the page. **A guard that iterates over matches can
  only police the matches. To catch a missing tier you must enumerate the SIGNS and
  demand a tier for each one.**

- **A SOURCE ADDED TO ANSWER A SOURCING NIT CAN BACK THE OTHER HALF OF THE SENTENCE —
  AND THE FRONT-MATTER COMMENT WILL SAY SO, IF ANYONE READS IT.** Round 2 objected that
  "there is treatment for it" rested on only generic support, and the fix cited St.
  Jude's endocrine late-effects page. That page carries the DAMAGE ("Some cancer
  treatments, particularly radiation to the brain, may damage these glands") and a
  definition of growth hormone deficiency that ends at "Learn more." — the treatment
  detail lives on a child page that was never fetched. So the quote recorded in the
  page's own front matter was itself evidence that the fix had NOT worked, sitting in
  the file for a full round until `/review` round 3 read it back. §12.14 in its quietest
  form: the citation is real, the quote is real, and neither one touches the disputed
  clause. **When you add a source to settle a sourcing objection, quote the clause that
  supports THE DISPUTED HALF — and if no such clause exists, the claim goes, not the
  objection.**

- **ONE INTENSIFIER CAN COVER TWO NAMES, AND THE UNSUPPORTED HALF RIDES IN ON THE
  SUPPORTED ONE.** "Glioblastoma and meningioma are **far** more common in adults" is
  two claims sharing a magnitude word. Meningioma's is sourced — ACS says "much less
  common in children and teens than in adults". Glioblastoma's is not: the only source
  cited for it says "more common in adults compared to AYA and children", and the
  page's own front matter quotes that WEAKER wording hundreds of lines above the
  sentence that strengthens it. `/review` round 3 worked this very sentence, verified
  the meningioma half, and left the other unchecked — because a compound subject reads
  as ONE claim and gets ONE check. **Split the sentence when the evidence differs per
  name.** Worth recording because the item had already written the rule it was
  breaking into that same front matter: "the magnitude word is the SOURCE's rather than
  a softening of it." Found at round 4, in a sentence two rounds had already touched.

- **A DELIBERATELY SHARED CLAUSE HAS A CEILING, AND COMPOSED, THIS ONE REACHES THE
  READER FIVE TIMES.** Twice page-owned (the baby tier and the pre-verbal tier added at
  round 3), plus `blocks/escalation.md`, `blocks/mechanism.md` ("If you have a shunt, it
  means getting help right away") and `blocks/posterior-fossa-syndrome.md" ("or right
  away if there is a shunt"). Each attaches to a DIFFERENT trigger, so none is redundant
  and §12.10's one-claim-at-one-strength rule is being honoured rather than abused —
  `/review` round 4 ruled the two consecutive page-owned ones should stay, because a 2am
  reader meets one paragraph or the other (their child is either a baby or a toddler who
  cannot talk yet, not both) and making the second refer back would cost a lookup on the
  most safety-critical content on the page. **But the count is only visible on the
  COMPOSED page and nothing measures it.** Five is the ceiling — the argument against a
  sixth, not against these five.

- **AN UNSOURCED CLAIM MAY ALREADY HAVE BEEN INHERITED, SO FIXING ONE PAGE LEAVES IT
  SHIPPED.** The same sentence lived on `/tumors/medulloblastoma` ("This is checked for
  years, and it is treatable"), drawn from the same file with the same gap, shipped by
  WI-537 the day before. Fixing only the page under review would have left a known
  over-reassurance live on a page this very item links to twice, and would have made the
  corpus disagree with itself about whether the claim is supportable. **When a sourcing
  defect is found, grep the CORPUS for the claim before fixing the page.** The cost is
  one grep; the alternative is knowingly shipping the defect you just documented.

**WI-539 — `/tumors/pituitary-tumor`, the first hub in the corpus for a tumor that
is not in the brain, and the item whose blocker was a CONJUNCTION.**

Observed: grade **5.4** against the hard 6.0; ContentCheck **273 checks / 0
failures**; suite **2,184 / 2,184 UNFILTERED** (2,155 before this item); **111**
break-mutations failing correctly on LF **and** CRLF across THREE targets
(`pituitary-tumor.md`, `blocks/escalation.md`, `treatments/steroids.md`) — 222
runs, zero survivors, zero no-ops, zero ambiguous anchors; **8** render guards
proved by rebuild; **2** end-to-end rendered reads; **0** corpus-restatement
collisions across **2,571** shingles with **NO allowlist at all**; **4** `/review`
rounds, finding 3, 2, 2 and finally 0 blockers. Sources: **9 cited** — 8
script-checkable plus cancer.org, cited and declared uncheckable — from 41
script-checked quotes; mayoclinic.org returned 403 and is not cited and not used.
One SIBLING page edited: `/treatments/steroids`, which gained the
replacement-steroid conditional and its first endocrine source. New glossary
entries: **none**, and that is a ruling rather than an oversight.

- **THE RESTATEMENT GUARD IS BIDIRECTIONAL, AND A NEW PAGE CAN TURN SHIPPED PAGES
  RED.** The first draft of this hub failed four tests and **two of them belonged
  to other pages** — `/tumors/pediatric-brain-tumor` and
  `/treatments/watch-and-wait`, both green for days. This page has no tests of its
  own yet, so nothing of mine failed; what failed was theirs, because
  `AssertDoesNotRestateTheCorpus` compares each page against the whole corpus and
  the corpus had just grown. §12.10 describes a block's blast radius; this is the
  other direction, and nothing recorded it. **It is also the decisive argument for
  rewording over allowlisting:** an allowlist entry on the NEW page cannot fix the
  OLD pages' failures, because their tests are not reading the new page's
  allowlist. Reword.

- **A FETCHER THAT REPORTS "13 FETCHED, 0 FAILED" CAN HAVE SAVED EIGHT EMPTY
  SHELLS.** `cancer.org` renders its content in JavaScript: a plain HTTP fetch
  returns a 38 KB navigation shell with an empty `<title>`, and the
  `amp.cancer.org` variant returns the byte-identical shell. Eight American Cancer
  Society pages saved that way, reported as successes, and contained **none of
  their own prose** — so every quote taken from them was unverifiable while
  looking filed and safe. **This is the third tool in two items caught reporting
  success over nothing**, after the privacy scan that died having printed a header
  and the smoke test that would have called a failed deploy a broken probe. The
  fix is the same shape each time: make the tool assert what it produced. The URL
  list now carries a third column naming a string that MUST appear in the saved
  text; if it does not, the entry is reported `SHELL` and nothing is written. It
  caught a second shell (`pituitarysociety.org`) on its first run after the fix.

- **THE BEST PATIENT-LEVEL SOURCE FOR A TUMOR MAY BE UNSAVABLE, AND THE PAGE HAS
  TO SAY SO.** `cancer.org` is JavaScript-rendered and `mayoclinic.org` returns
  HTTP 403 to automated fetching. Both are legitimate, both were read for facts,
  and neither can be script-checked. SYNTHESIS §7.2 records "the best source is
  403-blocked" as a risk; this is that risk twice in one item, in two different
  forms. The resolution is §12.13's: write the limitation into the front matter
  where its reader will see it, prefer a checkable equivalent where one exists —
  the Endocrine Society's patient library replaced most of it — and never let a
  claim-to-source map imply coverage it does not have.

- **A COLLISION REPORT IS NEVER A COMPLETE INVENTORY, BECAUSE THE GUARD TRUNCATES
  PER FILE.** Both `AssertDoesNotRestateTheCorpus` and the offline replica do
  `.Take(4)`, so each reports **at most four collisions per page**. The first run
  named 16 files; the triage table built from it had nine patterns; fixing them
  dropped it to one file — which then showed a FIFTH collision with the pediatric
  page that had been hidden beneath the cap the whole time, in a sentence never
  considered for rewording. **Work it to zero, and read each pass as "the next
  four" rather than "the remaining four".** What was hiding under the cap is the
  tell: this page's `:::outlook` section opened with a sentence copied word for
  word from the sibling hub used as its model — *"Outlook is usually talked about
  in numbers"* — and only two files in the corpus have ever contained it. **The
  opening sentence is the likeliest thing to collide, because the opening is the
  part you carry over without re-deciding it.** The reported window began with a
  bare `outlook` that belongs to the `:::outlook` DIRECTIVE line: it is not a
  heading, so the shingler keeps it and `[^A-Za-z]` turns it into a word. Fixed by
  rewriting the first seven words and leaving the tail the previous run had already
  proved clean.

- **WHERE THE REPLICA LIVES DECIDES HOW STRICT IT IS, SO GIVE EACH ITEM ITS OWN
  COPY.** `shingles.py` loads its suppressions from
  `Path(__file__).with_name("allowlist.txt")` — there is no flag, no argument, and
  no way to ask it for an unfiltered run. Run from `wi538/`, it silently applied
  that item's two deliberate entries to THIS page, which ships under a ruling of no
  allowlist at all. The entries were pediatric-specific so nothing was actually
  masked, but the instrument was wrong for the page and a zero from it would have
  meant less than it appeared to. Copying the script into `wi539/`, where no
  `allowlist.txt` exists, makes `allowed = []` a property of the DIRECTORY rather
  than something the next person has to remember to pass. Each page is then checked
  under its own contract: the shipped page keeps its allowlist, the new one gets
  none.

- **WHEN REWORDING TO CLEAR A SHINGLE, CHANGE THE PART THE CHECKER MATCHED, NOT
  THE PART YOU REMEMBER WRITING.** The outlook gate's median explanation collided
  with eight hubs. The rewrite replaced the memorable clause — "line everybody up,
  and the median is the person standing in the middle" — and kept the opening, "It
  means the middle of a group", which is the half the checker had actually
  matched. It still collided. The same instinct that makes a sentence memorable is
  what makes you rewrite the wrong half of it.

- **A FRONT-MATTER COMMENT IS INSIDE THE STRING EVERY RAW-FILE GUARD READS — AND
  THE RULE FOUND A NEW GUARD.** §12.8 already records this from WI-538, where a
  block quoted in a comment tripped `SpinalCordBlockTests` and a suppression
  marker in a comment made a harness mutation a no-op. Here it was
  `CaregiverSectionTests`, which locates the block with a raw
  `IndexOf("[CAREGIVER]")` over the whole file. Five bracketed directive names
  written into this page's front matter — documenting the block decisions — made
  the first match a comment, so `directive < heading` and the page was reported as
  not carrying the block under its standard heading. **Note which guard did NOT
  fire:** `EveryTumorHubIncludesTheCaregiverBlock` passed, because it uses
  `DirectBlockNames`, which requires the directive to be the entire line. Same
  file, same block, two guards, one protected and one not. Fixed by de-bracketing
  the comments — not by loosening a guard eighteen hubs depend on.

- **§12.10's TEST CUT THE OTHER WAY THIS TIME, AND THAT IS WHY IT IS TRUSTWORTHY.**
  WI-538 wrongly EXCLUDED `[SPINAL-CORD]` by calling a conditional block
  unconditional, and `/review` reversed it. Here the same test excluded
  `[MECHANISM]` and `[CROSSWALK]` correctly. The mechanism block opens *"These are
  the ways a tumor IN THE BRAIN causes symptoms"* and then asserts brain swelling,
  seizures and blocked fluid needing a shunt, followed by a lobe-by-lobe map —
  with **no scoping clause at all**. A pituitary tumor is not in the brain, rarely
  seizes, and does not obstruct the fluid pathways. The crosswalk block is
  entirely the 2021 CNS rewrite, and this report has no CNS grade and no NOS/NEC.
  **The discipline that made the difference was opening the block rather than
  reasoning about it: two of four starting assumptions were wrong**, and both
  wrong ones were blocks I had already written into the plan as included.

- **A COUNTER-ARGUMENT RECORDED IN ANOTHER PAGE'S TEST DESERVES ANSWERING, NOT
  IGNORING.** `AllBrainTumorsPageTests` says in as many words that *"Neither
  [MECHANISM] nor [CAREGIVER] is FALSE here — the closed-skull material is true of
  anything that takes up room."* That is a real argument against this item's
  exclusion, made by an earlier item, and it holds **on that page**, whose reader
  has no diagnosis yet and for whom anything intracranial is still possible. This
  reader has a name, and the name's defining fact is that it sits outside the
  brain. Recording why the counter-argument does not transfer is what stops the
  next item reversing this one on the strength of half the reasoning.

- **THE SOURCES HANDED ME REASSURANCE THAT WOULD HAVE BEEN §12.12 WITH A CITATION
  ATTACHED.** The Endocrine Society says pituitary tumors *"are not brain tumors
  and are almost always benign (non-cancerous)"*. True, and the natural way to
  open a page. But this tumor can take a person's sight, cause a life-threatening
  adrenal crisis, and leave them on hormone replacement — none of which "benign"
  prepares anyone for. `/tumors/meningioma` had already solved it: *"the word
  'benign' does more harm than good here… The useful questions are where it is and
  what it is doing, not which side of that word it falls on."* **Take a sibling's
  STANCE and write your own sentences** — the restatement guard will object to
  borrowed prose, and rightly.

- **TWO RENAMES ON ONE PAGE, MADE FOR OPPOSITE REASONS, AND SAYING SO IS THE MOST
  USEFUL THING THE CROSSWALK SLICE CAN DO.** Adenoma became PitNET, and the
  Pituitary Society argues against it in print: renaming *"does not change
  histopathology nor the prognosis"*, and the new label risks *"needless
  frustrations and apprehension among most of the patients diagnosed with benign
  PAs"*. Diabetes insipidus became arginine vasopressin deficiency, endorsed by
  eight societies **including the Pituitary Society** — because the shared word
  "diabetes" was getting people hurt: *"desmopressin treatment was withheld with
  serious adverse outcomes, including death"*. One rename is argued to harm
  patients; the other was made to protect them; both land on the same person's
  paperwork. The renaming group also proposes keeping *"the previous name in
  parentheses"* for years, which is why a reader sees both at once and is not
  looking at a disagreement between doctors.

- **THE SHARED ESCALATION BLOCK UNDER-TRIAGES THIS TUMOR'S TWO EMERGENCIES, AND
  NEITHER IS IN IT.** `[ESCALATION]` files "a headache much worse than usual" and
  "a sudden change in your vision" as SAME-DAY, and never mentions double vision.
  **Pituitary apoplexy** is sudden bleeding into the tumor — *"a medical and
  surgical emergency in many cases"*, where *"delay… may result in permanent
  hypopituitarism, irreversible visual loss, or death"*. **Adrenal crisis** is what
  happens when the gland stops making the hormone that drives cortisol, and an
  ordinary illness becomes dangerous. §12.10's remedy applied literally: include
  the block, then add the page's own CONDITIONAL tier beneath it, in the shape
  WI-534 used for shunts. Both halves sourced before being written — the item's
  central safety claim was never going to rest on clinical memory.

- **I WROTE A GUARD THAT COULD NOT FAIL, INTO THE FILE WHOSE JOB IS GUARDING.**
  The adrenal-crisis test sliced the section at
  `IndexOf("If you take steroid replacement")` and then asserted the result
  `StartsWith` that same string. It is a tautology: it passes on every page, in
  every state, forever. **Nothing downstream could have caught it.** It compiles.
  It passes. It passes a BREAK HARNESS too — the harness asks whether a mutated
  page makes some test fail, and a test that cannot fail simply never reports, so
  a mutation it should have caught is attributed to whichever guard did fire, or
  shows as a survivor with no obvious owner. Only reading the file back found it.
  This is §12.8's iterate-and-check rule in a new place: **a guard that SELECTS on
  a string and then ASSERTS that string is checking the selector, not the page.**
  WI-538 met it as a filter that never saw a sentence with no tier; here it is an
  index that guarantees its own answer. Replaced with the property actually
  wanted — the conditional clause opens its own BOLD lead-in, so a reader skimming
  emphasis meets the scope and not the bare warning — plus a canary, because a
  guard with no canary is the previous sentence waiting to happen. **The harness
  then proved the replacement by machine:** `adrenal-condition-unscoped` — the
  mutation that strips the conditional clause off the bold lead-in — turns it red
  on both line endings. The tautology could not have been caught that way; only
  reading it could.

- **ASSERTING THE ANCHOR IS UNIQUE FIXED THE CLASS, NOT THE INSTANCE.** WI-538's
  `rep()` takes the FIRST match and its one survivor came from that. This item
  replaced it with `uniq()`, which matches across whitespace (the page is hard
  wrapped, so most anchors cross a line break) and **raises when the anchor
  matches more than once**. Result: 75 mutations dry-ran with **zero ambiguous
  anchors and zero no-ops on the first attempt**, then **all 75 broke correctly on
  LF and on CRLF** — 150 runs, no survivors. On a page whose front matter quotes
  its sources at length, so that "apoplexy", "transsphenoidal", "adrenal crisis"
  and "medical alert bracelet" all appear ABOVE the body text they also appear in,
  that is not luck: it is the difference between a helper that silently picks one
  and a helper that refuses. **A tool that can only fail loudly is worth more than
  a tool that usually succeeds.**

- **I CALLED A GUARD WEAK, PUT THE CASE IN THE TOOL INSTEAD OF ACTING ON IT, AND
  THE TOOL PROVED ME WRONG.** `TheGlossaryFiresOnTheWordThisPageDoesNotDefineItself`
  asserts only that SOME tooltip renders (`def-` appears in the HTML). Reading it,
  I judged that close to unfalsifiable: this page glosses seven terms INLINE and
  adds no glossary entry, so removing any one should leave the others firing. I
  wrote it into `handproof.py` as an expected SURVIVOR, with a note saying a weak
  guard found by a tool is worth more than a weak guard found by an opinion.
  **It failed correctly.** Stripping `**extra-axial**` removed `def-` from the
  page entirely, because `extra-axial` is this page's ONLY glossary tooltip —
  every other piece of its vocabulary is a first corpus use that fires nothing. So
  "some tooltip fires" and "the extra-axial tooltip fires" are the same assertion
  here, and the guard was load-bearing all along.
  **Had I trusted the reading, I would have rewritten a working guard on my own
  authority and called it a fix.** The rule: when you suspect a guard is weak,
  MAKE THE TOOL ANSWER rather than reasoning to a verdict — it costs one case in a
  list and it is the only thing that can contradict you.
  The guard still gets the term's name, but for a NARROWER reason than the one I
  started with: it is specific only by accident of today's corpus, and the day any
  glossary entry fires on this page, `def-` keeps appearing even if `extra-axial`
  has gone. That is §12.8's (WI-512) rule once more — presence was never the
  property.

- **THE CORPUS REVERSED MY FIX, AND THE REVERSAL WAS THE RIGHT ANSWER.** The
  rendered read caught the `extra-axial` tooltip echoing the sentence it landed
  in: the page said *"a word for growing outside the brain itself"* and the
  popover answered *"Growing outside the brain itself, not inside it"* — WI-535's
  defect, word for word. The obvious remedy was WI-538's: suppress with
  `!%extra-axial%`. **Checking first showed that would have been wrong.**
  `/tumors/meningioma` glosses the same term inline and lets the tooltip fire, and
  `!%extra-axial%` appears NOWHERE in Content/. Suppressing here would make one
  glossary term behave two different ways on two pages, which is §12.10's
  one-claim-one-strength problem in vocabulary rather than in prose. The
  proportionate fix was smaller and better: **reword the page so it NAMES the term
  and the glossary DEFINES it.** That removed the echo, kept one behaviour for one
  term, and made the guard's own name (`...TheWordThisPageDoesNotDefineItself`)
  true for the first time — it had been asserting the opposite of what it was
  called. Note also what could NOT be reused: meningioma's own construction,
  *"Doctors have a word for that: extra-axial"*, is an eight-word run with four
  content words, so borrowing the sibling's sentence would have tripped the
  restatement guard. **A precedent tells you what to DO; it rarely tells you what
  to WRITE.**

- **A SHARED BLOCK CAN BE TRUE, INCLUDED FOR GOOD REASONS, AND STILL ADDRESS
  SOMEBODY ELSE.** Composed, `[CAUSES]` says "brain tumors" five times — *"For
  most brain tumors, nobody knows the cause"*, *"does not cause brain tumors"*,
  *"stress causes brain tumors"*, *"a brain tumor"*, *"Brain tumors are not
  contagious"* — on the one page in the corpus whose thesis is that this is NOT a
  brain tumor. Every sentence is accurate and the inclusion ruling was right;
  §12.10's block test asks whether the block is FALSE here, and it is not. **The
  test it does not ask is whether the reader will recognise themselves in it**, and
  a reader who decides the self-blame section is about somebody else has been lost
  by the one section that exists to reach them. **Only the COMPOSED page shows
  this** — the raw page contains the single string `[CAUSES]`, so no source-level
  guard, shingle check or reading-level gate can see it, and the block's own page
  looks perfect in isolation. Fixed with a bridge sentence above the directive,
  the remedy `/tumors/all-brain-tumors` already uses for the same class of problem
  and a different reason. **Add "does this block's framing fit this reader" to the
  §12.10 checklist, beside "is it true here" — and answer it on the rendered
  page.**

- **I CHECKED THE NEW SENTENCE THREE WAYS AND MISSED THE FOURTH.** Before writing
  the scoping bridge I checked the precedent for the pattern (`/tumors/all-brain-
  tumors`), the restatement risk against that page's wording, and the reading
  level. `NoEmDashInCopyTests` — a corpus-wide house rule I had not met — failed it
  on an em dash. The guard was right and the page was wrong, and the fix was one
  sentence split in two. **The lesson is not "avoid em dashes"; it is that a page
  joins a corpus with house rules its author has not read, and the suite is the
  index to them.** Worth noting what did NOT need changing: all three assertions in
  the new test survived the rewrite, because each anchors on a fragment that does
  not span the punctuation. A guard written about the PROPERTY rather than the
  prose costs nothing when the prose moves.

- **VERIFY A MUTATING TOOL'S RESTORE IN BOTH DIRECTIONS, BECAUSE `git status`
  CANNOT TELL YOU.** The break harness rewrites the page once per mutation per
  line-ending and restores the original from memory in a `finally`. When it
  finished, `git status` showed the page MODIFIED — which is correct on a feature
  branch with uncommitted work, and is also exactly what a leftover mutation looks
  like. §12.8's WI-515 point is precisely that an abandoned mutation **reads as
  your own prose**; the crash marker tells you a run DIED, not that a completed
  run put everything back.
  So the check has to be positive and two-directional: seventeen mutation payloads
  confirmed ABSENT, *and* five of the page's own sentences confirmed PRESENT at
  their expected lines. **Absence alone proves nothing — a truncated or
  half-restored file satisfies it perfectly.** Both greps are read-only and cost
  one call.
  Related, and the same hazard from the other side: while the harness was running,
  a tool notice reported the page as CHANGED ON DISK and invited me to treat the
  new state as deliberate. It was deliberate and it was TRANSIENT. Reading the file
  then would have shown a sabotaged page; editing it would have been silently
  reverted by the `finally`. **While a mutating tool holds a file, the file has no
  meaningful state — do not read it, do not edit it, do not "fix" it.** Wait for
  the marker to clear, then verify.

- **A GUARD THAT SURVIVES MAY BE INDICTING THE MUTATION, NOT THE GUARD — AND THE
  SAME ONE CORRECTED ME TWICE, IN OPPOSITE DIRECTIONS.** The glossary render guard
  went into the handproof as an expected SURVIVOR, on my reading that seven
  inline-glossed terms made "some tooltip fires" unfalsifiable. It FAILED
  correctly: `extra-axial` is this page's only tooltip. Then, having strengthened
  it to name the slug, I rewrote its mutation to match the reworded sentence and
  assumed the new break would bite. It SURVIVED — because I had mutated
  `**extra-axial**` to `extra-axial`, which removes the EMPHASIS, not the term.
  `GlossaryMarker` matches the word, so `def-extra-axial` still rendered and the
  guard was right to pass. §12.8 already carries this shape from WI-537, where
  changing a front-matter `slug:` proved nothing because routing reads the FILE
  PATH: **a weak mutation reads exactly like a weak guard, and the failure mode is
  that you "fix" a working guard.**
  The test for telling them apart is mechanical: ask what the guard actually reads,
  then check the mutation changes THAT. This guard reads rendered HTML for a
  glossary slug, so the break must remove the word the glossary knows — not its
  formatting, not its front matter, not its emphasis.
  **Two wrong predictions about one guard, in opposite directions, both caught by
  the tool.** That is the argument for handproofing render guards rather than
  reasoning about them: the reasoning was confident and wrong twice, and cost
  nothing because the tool was asked instead of consulted.

- **THE SECOND RENDERED READ EARNED ITS KEEP: READ 1 FOUND TWO DEFECTS, READ 2
  FOUND THREE MORE.** They were not subtler than the first two — they were
  ordinary words sitting in plain sight that a first pass, busy checking structure
  and safety claims, simply did not hear. **"tablets" (×8), "optician", and
  "junctions"**: British idiom on a page whose own front matter bars Cancer
  Research UK as a source FOR IDIOM. Shipping it in my own prose would have been
  that same defect one step further in. This is the argument for the rule being
  TWO end-to-end reads rather than one careful one; the second read is not a
  formality.

- **LET THE CORPUS DECIDE IDIOM, AND IT ANSWERS MORE PRECISELY THAN YOUR EAR
  DOES.** The green suite proved nothing: `CuratedPage.BritishForms` is documented
  as deliberately incomplete and determiner-bound, so it could not see any of the
  three. What decided it was counting.
    * **"optician"** and **"junction"**: ZERO occurrences elsewhere under
      `Content/`. Mine alone, so they are outliers, not house style.
    * **"tablets"**: the corpus says **"pills" 56 times across 14 files**. Every
      other "tablets" hit is either the DEVICE sense (iPads on
      /tumors/pediatric-brain-tumor, magnets near a shunt) or a verbatim source
      quote in a front-matter comment. In body copy meaning medicine, the corpus
      has two — and this page had eight.
  **And the rule is sharper than "ban the word".** `TestsLibraryPagesTests`
  records that `"tablet"` was deliberately LEFT OFF the shared list, because US
  labeling says "take one tablet" and *only the plural, meaning pills in general,
  is the idiom*. So the page-local ban is on `tablets` alone. Two pages
  (`SteroidsPageTests`, `TumorTreatingFieldsPageTests`) had already reached the
  same conclusion the same way, which is the confirmation that this is the
  corpus's method and not my improvisation.
  **Compare "specialist nurse", which I checked the same way and did NOT change**
  — ten hubs use it, so it is convention. Same procedure, opposite answer. That is
  what makes it a procedure rather than a preference.

- **RUN `/review` BEFORE THE EXPENSIVE VERIFICATION, NOT AFTER — THE GUARDRAIL
  LIST ALREADY SAYS SO AND I PAID TWICE TO REDISCOVER IT.** The order given is
  ContentCheck, `/review` until a round finds no blockers, break harness, handproof,
  two rendered reads. I treated it as a checklist rather than a sequence and ran
  the harness twice and the handproof twice first, on the "verify as you go"
  instinct. Both runs were made obsolete by later page edits — the two rendered-read
  fixes, then ten more from the idiom sweep. **Review findings change the page, so
  anything that costs twenty minutes and depends on the page's exact bytes belongs
  AFTER the page settles.**
  There is also a hazard beyond waste, and it is the sharper reason: **a
  content-mutating tool must never run DURING a review.** The harness and the
  handproof rewrite the very file the reviewer is reading, so the reviewer would
  report findings against deliberately sabotaged prose — manufacturing false
  blockers and masking real ones, with nothing in either output to show why. The
  rendered reads are different and can come early: they are read-only and they find
  the defects that CAUSE page edits, which is exactly what you want before the
  expensive checks.
  Corrected order for the next item: draft → ContentCheck → suite green → rendered
  reads → `/review` to no blockers → harness → handproof → ship.

- **A GUARD CAN POLICE ONLY THE DEMOTIONS ITS AUTHOR IMAGINED.** The page's
  apoplexy tier offered a PHONE CALL for signs the shared block files under CALL
  AN AMBULANCE / 911 — a downgrade sitting four paragraphs below the block, in the
  paragraph introduced as "the list above does not cover them" and therefore read
  as the authoritative one for this tumor. The word "911" appeared nowhere on the
  page. **My own guard passed it**: `demoted` was built from same-day/next-day/
  in-the-morning vocabulary, so a fall from *ambulance* to *call your team*
  matched none of its branches. This is the third form of the same defect in two
  items — WI-538's filter that never saw a sentence with no tier, this item's
  tautological `IndexOf`/`StartsWith`, and now a ban list enumerating the wrong
  axis. **When a guard defends a TIER, assert the tier's floor positively (the
  route must be present) rather than banning the words for lower tiers.**

- **A FINDING CAN BE RIGHT WHILE ITS PROPOSED FIX IS WRONG — VERIFY BOTH.** Review
  round 1 was excellent and two of its remedies would have made things worse.
  *"Run the mechanism/crosswalk bans against `Composed`, where they also describe
  what the reader receives"*: they would have FAILED, because the composed page
  legitimately carries "shunt" and "seizure" from `[ESCALATION]`'s conditionals —
  a guard that fails a correct page is worse than no guard (§12.8, WI-508). The
  finding underneath was still right: the comment claimed those raw-text bans
  guarded the EXCLUSION, when only the exact closed set does. *"Say surgery is
  usually first for acromegaly and Cushing's"*: clinically plausible, and absent
  from every fetched source — publishing it would have been WI-538's
  citation-written-from-memory defect, nine of which shipped. The concern was real,
  so it became an ASK ("which route does my hormone point to?") rather than a
  claim. **Treat a review as evidence about where to LOOK, not as instructions.**
  **The count, because the number is the persuasive part: of 21 items in round 1,
  THREE would have made the page worse if applied verbatim** — the unsourceable
  first-line-surgery claim, the `Composed` fix that would have failed on block
  text shared on purpose, and a corpus convention that did not exist. Every one of
  the three findings was still worth having: each pointed at something real (a
  reader with no signpost, a comment claiming more than its test did, a page
  missing a forward pointer). **A review's hit rate on PROBLEMS and its hit rate
  on SOLUTIONS are different numbers, and only the first one is the reason to run
  it.**

- **CORRECTING THE PROSE IS NOT CORRECTING THE RULING.** The S2 fix removed
  *"The Pituitary Society … has objected in print"*
  from the page, because the fetched document turned out to be an editorial of
  the journal's editors' personal views. **The front matter still records the
  disproved rationale**: the note on PMC9759163 says the AVP-D rename was
  "Endorsed by eight societies INCLUDING the Pituitary Society -- **the same body
  that objects to the PitNET rename**, which is why the page can say the two
  renames were made for opposite reasons", and the note on PMC9170656 still calls
  it "A POSITION PIECE ARGUING ONE SIDE".
  **THREE sites carried the disproved reasoning, and all three are now corrected**
  — the endorsement half is sourced and stays (now quoting all eight societies in
  full); the "same body" reason is gone; PMC9170656 is described as what it says
  it is; and `PituitaryTumorPageTests.cs:437` no longer says the objection is
  "attributed to the Pituitary Society".
  **That last one is the sharpest version of the problem: it sits SEVEN LINES
  ABOVE the comment written during the fix, which says the opposite and correctly.
  One test now contains two contradictory accounts of the same source**, and
  whichever a future editor reads first will look authoritative. Grepping the
  corpus shows the blast radius is otherwise contained — only this page and
  `taxonomy.yml` mention the Society, PMC9170656 or PitNET at all, so no sibling
  page repeats the error.

- **A FIX CAN INTRODUCE THE DEFECT IT WAS FIXING — TWICE IN ONE ROUND.** Review
  round 2 found both, and both were sentences round 1 had asked for:
    * The post-operative thirst rule, added to close round 1's S4, told the reader
      the symptom **is** arginine vasopressin deficiency. Its own citation
      (NBK470458) says *"most cases of polyuria in this setting are **not** due to
      AVP-D"* — so a fix written to give a post-operative reader an action handed
      them a diagnosis the source calls the less likely one.
    * The conditional added to `/treatments/steroids` to close round 1's B2 scoped
      on a DIAGNOSIS, and so told every reader on long-term dexamethasone that the
      urgent rule was not theirs — 160 lines after that page teaches *"your body
      stops making its own while you are taking this one"*. Over-reassurance
      (§12.12) introduced into a SHIPPED page by a correction.
  **The pattern: a fix is new prose, and new prose gets the same scrutiny as the
  draft did — it has not been reviewed merely because it was requested.** Both
  were caught only because a second review round ran over the fixes themselves.

- **"NOT FOUND" IS ONLY EVIDENCE IF THE SEARCH COULD HAVE FOUND IT.** I removed
  *"Sight often improves after the pressure comes off"* as unsourced. It is
  sourced, in this item's own pack: *"After surgery, vision problems improve in
  most people, or they can go away all together."* My grep had capped at 30 hits
  and printed the line carrying it as `[Omitted long matching line]`. **Third
  long-line false negative of this item, and the first that made me DELETE
  something correct** rather than merely miss it — and the cost was a subsection
  about losing your sight ending on an administrative sentence, which is the §12.6
  defect. **Before concluding absence, check the search could have shown presence:
  raise the limit, drop the pattern's specificity, or Read the region.** The
  asymmetry matters — a false negative that stops you ADDING something is cheap;
  one that makes you REMOVE something is not.

- **QUOTE TO THE END OF THE QUALIFICATION, NOT THE END OF THE CONVENIENT
  SENTENCE.** Both pages' verbatim blocks recorded the Endocrine Society's *"If
  left untreated, adrenal crisis can cause death"* and stopped there. The very
  next sentence is *"Adrenal crisis occurs mainly in people with primary AI"* —
  and this page's readers have SECONDARY AI. Read carefully it qualifies WHO GETS
  a crisis rather than whether one kills, so the claim survived; but a verbatim
  block that ends exactly where the source starts complicating things is §12.13's
  defect in the recording rather than in the prose, and the next editor would
  never know to look.

- **A REVIEWER'S QUOTATION CAN BE UNVERIFIABLE, EVEN WHEN ITS FINDING IS RIGHT.**
  Round 2's B2 rested on three verbatims attributed to PMC11451960, which is in no
  fetched pack — so they could not be checked, and publishing on them would have
  been a citation written from memory at one remove. The FINDING was correct and
  provable another way: the target page already teaches the mechanism itself, with
  its own sources. **Route around an unverifiable quote rather than inheriting
  it.** Round 1's lesson was that a finding can be right while its remedy is wrong;
  this is the sharper sibling — a finding can be right while its EVIDENCE is
  unusable. (Corollary, from N3: a number written into a comment has no guard
  behind it and goes stale the first time anyone edits the file. State the
  property; let the checker hold the number.)

- **AN ACCIDENTAL COLLISION CAN LOOK EXACTLY LIKE A DELIBERATE SHARED CLAIM, AND
  THE ALLOWLIST IS THE WRONG TOOL FOR IT.** Adding the 911 route to this page's
  two emergency tiers turned a SHIPPED page red: `/treatments/anti-seizure-
  medicines` was reported as restating `call 911 or your local emergency number
  for`. Everything about that pointed one way — it is an emergency ROUTE, the kind
  of safety claim §12.10 says must read identically wherever a reader meets it;
  `blocks/escalation.md` already establishes the wording corpus-wide;
  `MeningiomaPageTests` allowlists the cord-compression sentence across three
  pages for exactly that reason; and **the failing guard's own error message
  offers `DeliberatelyShared` as the remedy.** I had the entry half-written.
  It was an accident. Their sentence ends *"…or your local emergency number,
  **for**:"* and mine read *"…or your local emergency number. **For** the rest of
  it…"* — the shared run **spans my sentence boundary**, and exists only because
  the next sentence happened to start with "For". The block's own wording has no
  "for" in it at all. One word fixed it ("With any of the rest").
  **The tell is cheap and general: a colliding run that spans a FULL STOP, or that
  starts or ends mid-clause, is two sentences touching — not one claim.** A real
  shared claim collides as a whole clause that reads like a sentence on its own.
  Had I allowlisted it, I would have enshrined a coincidence as a deliberate
  §12.10 ruling, inside another page's test, and broken this page's no-allowlist
  ruling to do it — while leaving the actual duplication in place for the next
  editor to puzzle over.
  **Check whether the run is a claim before deciding it is shared on purpose.**
  Why this is worse than an ordinary stale comment: §12.13 makes the front matter
  the place the NEXT editor looks to find out why a sentence is worded the way it
  is. A rationale that outlives its correction does not sit inert — **it argues
  for putting the error back**, with apparent authority, to somebody who has no
  reason to re-verify it.
  **When a review corrects a CLAIM, grep the front matter for the REASON.** I
  found this by accident, in a truncated excerpt a tool happened to print; the
  front matter on this page is 200 lines and I had not re-read it after the fix.

- **CHECK THE DOCUMENT, NOT THE FILENAME.** The page said *"The Pituitary Society
  … has objected in print"* to the PitNET rename. The fetched source
  (`pituitary-society-net-or-not.txt` — the filename I had been reading as
  confirmation) says *"This editorial expresses the personal views of the
  authors"*, written by the journal *Pituitary*'s Editor-in-Chief and board. The
  actual Society position statement is a DIFFERENT paper it cites, from 2019, which
  was never fetched. The authors overlap; the documents do not. Softened to what
  the fetched text supports, and the 2019 statement is NOT cited, because citing an
  unfetched paper to rescue a sentence is the defect it would be covering up. The
  other half survived verification: the AVP-D position statement names its eight
  endorsing societies in full, and the Pituitary Society is among them — so "two
  renames, opposite reasons" holds, while "the same body" did not.

- **THE FIX FOR ONE IDIOM INTRODUCED ANOTHER, AND NO BAN LIST COULD HAVE CAUGHT
  IT.** The sentence rewritten to correct the Pituitary Society attribution came
  out as *"the journal that **specialises** in these tumors"* — British spelling,
  written into the very edit that was cleaning up British idiom, three fixes after
  I had counted the corpus to justify banning `tablets`, `optician` and
  `junction`. `CuratedPage.BritishForms` could not see it, and **the stem cannot be
  banned**: this page correctly says "specialist nurse" and "hormone specialist",
  so a `specialis` ban would fail a correct page (§12.8, WI-508). Only the verb
  forms — `specialises`, `specialised`, `specialising` — pass the test that a
  phrase belongs on a ban list only if no correct sentence contains it.
  **The general point: a ban list is not a spell-checker.** It can only ever
  enumerate the forms somebody thought of, and the moment you edit prose you are
  generating new forms. The defence is the rendered read and the corpus count, not
  a longer list.

- **RUNNING THE RIGHT TOOL ON THE WRONG PAGE PRODUCES CONFIDENT NONSENSE.** After
  editing `/treatments/steroids`, I checked it with `wi539/shingles.py` and got
  two colliding files and eight shared runs. Every one was PRE-EXISTING text I had
  not touched — its ambulance paragraph, shared with `[ESCALATION]` **on purpose**
  under §12.10's one-claim-one-strength — and that page has its own allowlist
  recording exactly that. I had pushed it through this item's deliberately EMPTY
  one. Same lesson as "where the replica lives decides how strict it is", arriving
  from the other direction: **the per-item copy is correct only for the item's own
  page.** For any other page, the suite is the authority, and it passed every
  steroids test while the replica was raising alarms.

- **A REVIEWER'S READING OF THE CORPUS DESERVES THE SAME COUNT AS ITS READING OF
  THE PAGE — AND THE TRUTH WAS MORE USEFUL THAN THE CLAIM.** Review round 1 filed
  a nit saying the corpus now holds TWO conventions for where a hub's scoped tier
  sits relative to `[ESCALATION]` (this page below it; `/tumors/meningioma` and
  `/tumors/brain-metastases` above), and that nothing records which is which.
  Grepping all fifteen hubs instead of the two named shows there is no split:
    * FOUR pages — `/tumors/pediatric-brain-tumor`, `/tumors/medulloblastoma`,
      `/tumors/diffuse-midline-glioma`, `/tumors/ependymoma` — put a FORWARD
      POINTER above the block and the tier itself BELOW it, and say so in as many
      words: *"those signs have their own rule, at the end of the section on when
      to call for help, below."*
    * The two the reviewer named carry the same SHARED spinal-cord red-flag
      sentence above the block — a §12.10 one-claim-one-strength case shared
      across three pages, not a page-specific tier at all.
  **So the convention is "tier below, pointer above", and it is unanimous.** This
  page follows the first half and omitted the second, which is not a stylistic
  difference: it is WI-538's own blocker shape, a reader meeting the most
  specialised rule on the page with no warning that it was coming. The nit was
  pointing at something real and had the reason and the remedy both wrong.
  **Counting is cheap; a convention inferred from two examples is a guess.**

- **A CONJUNCTION IS A SCOPE, AND EVERY GUARD WAS WATCHING THE CONTENTS INSTEAD.**
  The apoplexy tier opened *"A sudden, severe headache **with a change in your
  sight**"*. Seven signs were enumerated one by one, the route was asserted, the
  demotion vocabulary was banned, the closer was pinned — and not one of those
  assertions could see that the ENTRY had been narrowed to readers who had BOTH.
  NBK559222 calls the headache *"the most common symptom"* and files the rest
  under *"Other symptoms include"*; `/start` files a lone sudden severe headache
  under CALL 911. So the page sent the commonest presentation past the rule
  written for it, and filed one symptom at two strengths on two pages.
  **A guard over a paragraph cannot see the scope of its lead-in**, because
  narrowing the entry removes nothing the guard reads. The new assertion is over
  the BOLD LEAD-IN, which is also the only part a reader skimming emphasis meets.
  Note the knock-on: once the headache alone is the entry, the split route
  (911 for sight loss, a phone call for everything else) became a demotion below
  what `/start` already promises, so **fixing the scope forced the strength up**.
  A scoping defect is not cosmetic; it changes what the rule is.

- **A SENTENCE CAN BE FALSE ABOUT THE PAGE IT IS PRINTED ON, AND SURVIVE TWO
  REVIEW ROUNDS.** `/treatments/steroids` said *"If your body has stopped making
  its own steroid, **one item on the list below** is different for you"*. Its
  same-day list carries "You are confused", "You are being sick again and again"
  and "You feel faint, dizzy standing up" — the three signs the Endocrine Society
  names for adrenal crisis and the three `/tumors/pituitary-tumor` files as a 911
  call. The sentence named a fatal condition, gave no signs and no destination,
  and told a reader to wait until the same day for its textbook presentation.
  **It survived because every guard on both pages read the CONDITIONAL, and none
  read the LIST it made a claim about.** The remedy is the shape, not the string:
  the conditional now escalates the whole list (the shunt-conditional shape from
  `blocks/escalation.md`), and the guard READS THE SIBLING'S LIST and asserts the
  triad is in it. **When a sentence makes a claim about a list, the guard has to
  open the list** — otherwise it is checking the author's memory of it.

- **A BAN LIST POLICES THE ONE WORDING ITS AUTHOR IMAGINED — THE FOURTH TIME ON
  THIS ITEM, AND THE REVIEWER CAUGHT THIS ONE.** The new guard for the entry above
  banned a SIGHT-CHANGE conjunct (`headache with a change in your sight`) in the
  short version and the caregiver section, while the tier's own guard banned any
  conjunction at all. So *"A sudden, severe headache **with double vision**, and
  being ill…"* narrowed the two places most readers actually reach and left
  everything green — the round-3 blocker's exact axis, re-guarded on one wording.
  Fixed by asserting **ADJACENCY**: the entry is immediately followed by the
  second emergency, so anything inserted between them is a narrowing whatever
  word introduces it. **State the property; do not enumerate the phrasings.**

- **THE RAW-FILE GUARD TRAP CAUGHT ME THREE TIMES IN ONE ITEM, TWICE WHILE
  WRITING ABOUT IT.** §12.8 already records that a front-matter comment sits
  inside the string every raw-file guard reads. It bit the bracketed block names
  in the first draft; then the round-4 note correcting the frequency claim, which
  QUOTED the banned sentence while explaining that it was wrong; then, minutes
  later, the note recording the glossary-echo ruling, which quoted the string a
  render guard asserts is absent. **The general shape is worth more than the
  instances: a correction naturally wants to quote the thing it corrects, and
  that is exactly what a raw-file guard cannot tell apart from the thing itself.**
  Knowing the rule is not protection — I cited it in the comment that broke it.
  Describe the old wording; never reproduce it.

- **A REVIEWER'S "VERBATIM" IS A CLAIM LIKE ANY OTHER.** Round 3 asked for
  fainting and collapse in the apoplexy signs, citing NBK559222's History and
  Physical as carrying them verbatim. That paragraph says *"altered mental
  status"* and, for the crisis that follows, *"hypotension, hypothermia,
  lethargy, and, on occasion, coma"*. `fainting`, `collaps`, `syncope` and `loss
  of consciousness` return ZERO hits in the chapter. **The FINDING was right —
  the sign belonged in the list — and the evidence offered for it was not real.**
  The page carries "passing out", which the actual text supports. This is the
  third variant of one rule on this item: round 1 found a finding right with a
  wrong REMEDY, round 2 a finding right with unusable EVIDENCE, and here a
  finding right with a fabricated QUOTATION. **Check the quote even when you
  agree with the conclusion** — agreeing is exactly when you stop checking.

- **A DOSSIER IS NOT A SOURCE.** *"Some hormone tests are timed, and some take a
  morning"* was filed by the reviewer as a nit and promoted on checking: neither
  the timing nor the morning appears in any of the nine fetched files, which say
  only *"Additional tests, called stimulation tests might be needed"*. The detail
  is real and sits in `docs/research/tumor-guides/tests-library.md` — a research
  dossier, not a fetched source, and therefore the same footing two sentences
  were deleted for one round earlier. **The rule does not bend for small claims,
  and the tell is that the support you reach for is a project file rather than a
  source file.**

- **A FIX ON ONE PAGE FALSIFIED A CLAIM ON ANOTHER — IN THE ONE FIELD NOTHING
  GUARDS.** The description ended *"the two emergencies this tumor has that other
  brain tumors do not"*. Rescoping the steroids conditional onto the SUPPRESSED
  AXIS made the adrenal rule true for a reader on long-term dexamethasone for a
  glioma, so the uniqueness claim became false — falsified by this item's own
  fix, two files away. It also filed this tumor among "other brain tumors" on the
  one page whose thesis is that it is not one. §12.3/WI-524 renders the
  description as the first paragraph a reader meets. **A uniqueness claim is a
  claim about every other page, so it ages whenever any of them changes** — and
  the description is the least-guarded prose on the page.

- **SHARPENING A VAGUE SENTENCE EXPOSED A PRONOUN.** Told that a sentence was
  vaguer than its source, I replaced *"It is one of the things that most often
  follows an operation on the pituitary"* with *"It often follows the operation
  through the nose"* — and the nearest antecedent, one line up, had become **a
  report**. The page now read that the report follows the operation. The vague
  version carried its own noun and so could not be misread. **Precision and
  reference are different axes, and improving one can break the other**; the
  read-back caught it, no guard could have. Named the subject instead.

- **THE SAME SENTENCE WAS TRUE ON ONE PAGE AND FALSE ON ANOTHER.** Both this
  page's and `/treatments/steroids`' front matter carried *"the page must not
  imply frequency, and it does not"*, copied across when the shared source was
  added. On the sibling it is true. Here it sat four hundred lines above a tier
  ending *"Both of these are uncommon"* — §12.14's shape, a note reading as
  though the problem had been handled. The frequency word is correct and sourced
  (*"Apoplexy in pituitary adenomas is rare"*), so **the NOTE was the defect, not
  the prose**. Checked per page rather than assumed from the shared wording,
  which is the only reason the sibling's copy was left alone.

- **ROUND 4 FOUND NO BLOCKERS, AND ALL THREE DECLINES HELD.** The review rounds
  ran 3 blockers, then 2, then 2, then 0. What made the fourth round useful was
  telling it what had been DECLINED and why — the non-verbatim quotation, the
  transport instruction with no source behind it, and the guideline that was not
  in the pack — and asking it to overturn them. It verified all three
  independently (including re-deriving the zero-hit result after its own first
  grep produced four false negatives on long lines) and then spent its effort on
  four findings I had not seen. **A reviewer told what was declined audits the
  declines; a reviewer not told re-raises them or assumes they were accepted.**

**WI-540 — `/tumors/craniopharyngioma`, and the item whose nearest neighbour was
its biggest hazard.** A 43-line stub replaced by a §12.3 hub for a tumor sitting
in the same small space as `/tumors/pituitary-tumor` and threatening the same two
things. Almost every ruling had to be RE-DERIVED rather than inherited, and the
ones that looked most transferable were the ones that flipped.

- **A DOSSIER CAN WEAR A SOURCE PACK'S FILENAMES.** The fetch layer refused
  whole-page dumps, so all 42 `.txt` files were model-produced extracts — and the
  pack's own notes quoted those extracts, so script-checking one against the other
  is **circular**: one extractor produced both, and the check can only prove it
  agreed with itself. §12.8's tell is "the support you reach for is a project file
  rather than a source file", and here the project file was *shaped like* a source
  file, down to a SOURCE/URL header. Verifying nine sources against the live
  publications found **no fabrication and four truncations**, each clipping the
  start or end of a sentence — one of them stopping at *"it can be life
  threatening"* where the source continues *"from very low blood pressure and
  heart problems"*, which is **§12.13's exact defect produced mechanically by a
  tool rather than by an editor's convenience**. A pack can be sound in substance
  and unsafe in precision at once, and those need different remedies. **And
  verification pays twice:** re-reading PMC12109346 returned two sentences BETTER
  than the ones the extract kept, and they became the page's most important
  non-obvious claim and its anti-hype counterweight.

- **A SOURCE THAT NAMES YOUR PREDECESSOR'S EMERGENCY FOR YOUR TUMOR IS NOT A
  SOURCE THAT RANKS IT.** WI-539's hub is built around apoplexy. Bleeding into
  THIS tumor is genuinely named for this tumor, so nothing had to be stretched —
  and it is still not the headline, because it is verified *"a very rare
  syndrome"* in the paper's introduction AND conclusion, rests on one
  retrospective series, contains **no patient-facing action guidance at all**, and
  **both general reference chapters are verified SILENT on it**. What the sources
  converge on instead is acute obstructive hydrocephalus and adrenal crisis. The
  event is named, kept rare, and given no tier or route of its own. **Decide the
  central safety claim from the sources every time; the neighbour's answer is a
  hypothesis, not a starting point.**

- **A PREDECESSOR'S RULING CARRIES ITS REASONS, AND THE REASONS TRANSFER — NOT THE
  VERDICT.** Both of the neighbour's block exclusions flipped here, and only
  OPENING the blocks showed it. `[MECHANISM]` was excluded there because a
  pituitary tumor "is not in the brain, rarely seizes, and does not obstruct the
  fluid pathways"; all three are different here, and the block's blocked-fluid
  paragraph is this tumor's central mechanism. Reading it also showed the
  neighbour's front matter had **under-reported the block** — it records "no
  scoping clause at all" where the block does scope itself. Re-derive the verdict
  from the reasons, and read the artefact rather than the note about it.

- **THE RESTATEMENT GUARD IS BIDIRECTIONAL, AND AN ALLOWLIST CANNOT SAVE YOU.**
  The first green-suite run after drafting was 2,179 passed, 5 failed — and **none
  of the failures was this page's own test**, because it had none yet. Four were
  `ThePageDoesNotRestate…` on four SHIPPED pages. A new page turned four live
  pages red, and an allowlist here could not have fixed any of them, because none
  of those tests reads this page's allowlist. The cause was specific: I took the
  neighbour's **sentences**, not just its stance. **"Take the stance, write your
  own sentences" is easy to agree with and hard to execute, because the borrowed
  sentence is the one that already sounds finished.**

- **A CAPPED REPORT IS NEVER AN INVENTORY.** Closing those failures took **six
  editing passes**, and every pass fixed everything its report listed while the
  next reported a fresh set on the same file. They were not new; they were
  underneath the cap the whole time — both `AssertDoesNotRestateTheCorpus` and the
  offline replica `.Take(4)` **per file**, and they order their sets differently,
  so three reports of the same defect disagreed. Twenty distinct shared runs on
  one sibling, revealed four at a time. The remedy is ten lines
  (`wi540/all-collisions.py`): import the replica's own functions so it cannot
  drift, and print the intersection **uncapped**. **And it was validated before it
  was believed** — run first against a page that shares text deliberately and made
  to report a non-zero count. **Prove the instrument can say "yes" before you
  accept it saying "no".**

- **I REWROTE THE HALF I REMEMBERED WRITING, NOT THE HALF THE CHECKER MATCHED —
  SEVEN TIMES, AFTER WRITING THE RULE DOWN.** Each time I changed one clause of a
  colliding sentence and left the clause the guard had actually matched, so it
  reappeared in the next pass. **The count is the lesson, not the instances:** one
  is carelessness, seven is a structural property of editing your own prose — you
  edit toward the phrase you remember choosing, and the guard matched the phrase
  you never thought about. More care did not fix it. **Rewriting the whole
  sentence from scratch did**, because any edit that leaves a clause intact leaves
  the clause that was copied. **The eighth was its parent:** I fixed the two
  occurrences of a reported phrase I could recall and it returned a third time,
  from a section I had never associated with it. **A reported phrase is a property
  of the whole page, and memory enumerates instances — grep your own page and fix
  every hit in one pass.**

- **A FIX IS NEW PROSE, AND TWO OF THESE MADE THINGS WORSE.** Rewording one
  collision produced a sentence colliding with **two** pages instead of one,
  because the construction itself ("ask for your own timetable, in writing") is
  corpus-saturated across three pages. **Rewording inside a saturated formula
  moves the collision; it does not remove it — abandon the construction.** What
  ended the cycle was cheap: flatten every page in the corpus and grep each
  candidate replacement **before** writing it, excluding the page under test, with
  BOTH controls. A guard run before you write costs one command; the same guard
  run after costs a build, a 90-second suite, and another half-rewrite.

- **THE SHINGLER STRIPS DIGITS, SO EVERY PAGE'S 911 ROUTE COLLIDES WITH EVERY
  OTHER.** `[^A-Za-z]` turns `911` into a space, so the one instruction §12.10
  most wants at a single strength corpus-wide — and therefore worded near
  identically everywhere — is the one the guard reliably flags. The workable
  discipline is to keep the route identical and **make the words immediately after
  it differ**. **One claim at one strength does not mean one sentence, and the
  guard can only see the sentence.**

- **A NEW PAGE JOINS A CORPUS WHOSE HOUSE RULES ITS AUTHOR HAS NOT READ.** A guard
  living in `PathologyReportPageContentTests` scans every curated page and allows
  a Roman grade only where that same sentence also says it is the older style.
  Both of this page's Roman sentences put the marker in the NEIGHBOURING sentence,
  and both failed. The irony is the useful part: **the crosswalk slice exists to
  explain the Roman-to-Arabic change, and the guard against Roman numerals
  rejected it** — WI-513's shape again, where a retired-name ban must be
  negation-aware or it forbids the crosswalk it protects. The suite is the index
  to the house rules, and both times the guard was right and the page was wrong.

- **A GUARD ASSERTING WHAT A PAGE DOES NOT OFFER MUST READ THE READER'S TEXT,
  NEVER THE RAW FILE — AND THE SAME TRAP HAS A POSITIVE DIRECTION.** This page
  deliberately does not route to `/treatments/watch-and-wait`, and explaining that
  ruling **requires naming the URL**. The absence guard read the raw file and
  found the URL inside the very comment justifying its absence. Unlike the earlier
  instances this one cannot be fixed by rewording the note, because the ruling is
  *about* that address — so the fix has to be in the guard. Then `/review` found
  the mirror image: `Assert.Contains("/treatments/steroids", Page)` **could not
  fail for its stated reason**, because the front matter names that URL too. The
  same file documented the trap in the negative direction while re-committing it
  in the positive one. **Both directions need the reader's text.**

- **A DEAD ANCHOR FAILS LOUDLY; A STALE COMMENT NEVER FAILS AT ALL.** I reworded a
  sentence and left the assertion pinning the old wording — the atomic-update rule
  broken in the same pass that quotes it, and the third consecutive item to do it.
  Its quieter sibling is worse: I changed a slice terminator and left the comment
  above it describing the string that no longer exists. A dead anchor is caught by
  the next run; a stale comment simply sends the next editor looking for something
  that is not there. The cheap check is mechanical — after editing, grep the test
  file for the OLD string and decide which of three things each hit is: an
  assertion (dead anchor), a comment (a lie), or a deliberate history note (fine).
  **A count of 1 is not a verdict.**

- **EIGHT REFERENCE DEFECTS IN ONE ITEM, FOUR OF THEM CREATED BY FIXES — AND ONE
  FIX REPRODUCED THE DEFECT IT WAS REPAIRING, IN THE SAME SENTENCE.** §12.8
  already carries "sharpening a vague sentence exposed a pronoun". This item adds
  two shapes. **A DELETION strands pronouns too:** removing an unsourced sentence
  left the next one's "Both" with nothing to point at, because the deleted
  sentence had been carrying the antecedent. And the fourth: told that an override
  clause pointed at the wrong paragraph, I rewrote it opening "It also carries…"
  — where "It" resolved to the same wrong paragraph. **The pattern is structural,
  not careless: repairing a sentence means holding its MEANING in your head, and
  the meaning supplies the referent the reader does not have.** The only counter
  that worked was mechanical — read the edited sentence together with the one
  before it, resolving every pronoun to the NEAREST noun rather than the intended
  one. It caught three of four; no guard caught any, because nothing in the
  toolchain models reference. What made them findable at all was reading the edits
  **in place and in sequence** rather than each at its own anchor: an edit checked
  at its own line looks correct, and the defect lives in the join.

- **CLEARING THE RESTATEMENT GUARD IS ITSELF THE MECHANISM THAT HIDES BORROWED
  REASONING.** The item's only `/review` blocker: the page claimed tissue may
  never be examined while saying two sections later that surgery is almost always
  first *to find out what the growth is*. Both cannot be true. The sentence came
  from the neighbour, where it IS true — that hub routes to watch-and-wait and
  many of its readers are never operated on. It was one of the six collision
  passes: I reworded it until the shingle guard went quiet and **carried the
  reasoning over intact**. Three things make it worth recording. The guard's
  SUCCESS is what concealed it — rewording far enough to clear an eight-word check
  is exactly the operation that turns a visible copy into an invisible transplant.
  **No tool could see it:** ContentCheck reads level, the suite reads assertions,
  the shingler reads overlap, the rendered read shows composition, and **nothing
  compares a page's claims against each other** — two sections forty lines apart
  contradicted one another through six green runs and two end-to-end reads. And I
  had already written down the fact that refuted it, in my own plan, before the
  sentence existed. **When a fix borrows a sibling's sentence, re-derive the CLAIM,
  not just the wording: ask what makes it true on the page it came from, and
  whether that premise holds here.**

- **A SUMMARY OF A SAFETY RULE IS A RESTATEMENT OF IT, AND NOTHING WAS CHECKING
  THE SUMMARY AGAINST THE RULE.** `/review` round 2's only finding that could have
  cost a reader something. The caregiver section quietly narrowed BOTH emergency
  rules: it kept one branch of the fluid tier's disjunction, and it re-gated the
  adrenal rule on *taking* a replacement steroid **eleven lines after the page
  deliberately widened it** to anyone whose gland has been harmed. Neither
  narrowing was visible to any guard, because every tier assertion reads the
  SYMPTOMS section and every caregiver assertion read two phrases. This is §12.8's
  "when a sentence makes a claim about a list, open the list" one level up:
  **wherever a rule is restated for a second audience, guard the restatement
  against the rule.**

- **A PIPE HIDES THE EXIT CODE, AND HERE IT DESTROYED EVIDENCE.** `dotnet test |
  tail -30` makes `$?` the status of `tail`: it reported exit 0 on a run with FIVE
  failures, and the truncation meant **two of the five failure names were never
  written down and were unrecoverable** — the suite had to be re-run in full to
  learn what they were, and one of them was the Roman-numeral house rule, which is
  not a restatement failure and would otherwise have been missed entirely.
  **Redirect to a file, capture `$?` on the next line, then grep the file.** This
  is §12.8's "a tool must assert what it produced" from the other side: the tool
  was honest and the measurement was not.

- **TOOLS COPIED FORWARD MUST REFUSE TO RUN UNTIL RETARGETED.** Three tools in two
  items have now reported success over the wrong thing. `handproof.py` was
  deliberately left **raising** until its cases named this item's page, because an
  emptied case list would have printed "all 0 render guards proved by hand" —
  `all()` over nothing is True, so success over zero work looks identical to
  success. The same reasoning made a duplicate-effect entry worth deleting from
  this item's own mutation table: two reds for one property **overstate coverage
  in exactly the direction a break harness exists to measure honestly.**

#### WI-541 — acoustic neuroma, and the defect no gate in the corpus could see

- **THERE ARE TWO DIFFERENT RESTATEMENT GUARDS IN THIS CORPUS, AND A CLEAN REPORT
  FROM ONE SAYS NOTHING ABOUT THE OTHER.** `CuratedPage.AssertDoesNotRestateTheCorpus`
  drops **whole** markdown links and the ask-list before comparing. The page-local
  `Shingles` copied into many test files strips only the link *target*, so **link
  TEXT survives**, and it keeps digits. This item scored **zero** on the shared
  guard and still turned `/treatments/watch-and-wait` and `/tumors/meningioma` red
  — through a door's link text and an ask-list question. The offline replica
  modelled one guard and reported "0 collisions", which was **true and
  incomplete**. Model every guard that will read the page, or say which one you
  modelled.

- **AN UNCLOSED EMPHASIS MARKER IS INVISIBLE TO EVERY EXISTING GATE.** ContentCheck
  measures reading grade; both restatement guards drop `[*_]`; every page suite's
  `Plain` helper strips them *deliberately* (WI-526); the render tests read
  flattened HTML. So `**` that opens and never closes ships two literal asterisks
  to a patient while **2,249 tests stay green** — which is exactly what this page
  did, in prose added by a `/review` fix. Now guarded site-wide
  (`NoCuratedPageOrBlockLeavesAnEmphasisMarkerUnclosed`), and made site-wide **on
  evidence**: the property was first run offline over a strict superset of the
  guarded files, with a planted positive control, and found zero elsewhere.

- **A PRE-CHECK ANSWERS THE ONE QUESTION IT IMPLEMENTS, AND ITS SILENCE IS NOT
  GENERAL CLEARANCE.** Both of this item's late defects — a British spelling and
  that unclosed marker — rode in on the *same* round-2 prose, which had been
  pre-checked for restatement collisions and come back clean. A collision probe
  has nothing to say about spelling, register or well-formed markdown. New prose
  must clear **every** gate it will eventually meet, not the one that was
  convenient to run.

- **OVER-ESCALATION IS A DEFECT, NOT A SAFE DEFAULT.** This page's sources produced
  a **third distinct emergency shape**: exactly ONE rule, routed to primary care,
  urgent care or an ENT and explicitly **not** 911 — unlike the tiers on
  `/tumors/pituitary-tumor` and `/tumors/craniopharyngioma`. Brainstem compression
  and hydrocephalus are deliberately **not** tiered, because EANO contains no
  patient-facing urgency guidance at all and two patient-facing sources say the
  opposite. Modelling the neighbour's tier would have invented an emergency.

- **CITE THE DOCUMENT FOR WHAT IT SAYS, NOT FOR WHAT IT IS AUTHORITATIVE ABOUT.**
  WHO CNS5 is the naming authority and **prints no grade for schwannoma**. So the
  grade is attributed in two halves: the Arabic numeral to CNS5's convention
  change, the grade itself to the clinical references. Citing the naming document
  for the grade would be a citation that does not support the claim in the form it
  is made (§12.14) — the same defect class as the barred citation this item removed.

- **NEVER APPLY A GLOBAL WHITESPACE TRANSFORM TO A WHOLE CONTENT FILE.** A
  `[ \t]{2,}` collapse intended for prose flattened **all** YAML indentation and
  four list continuations; ContentCheck failed and the chained script correctly
  refused to run the suite. `git checkout` was unavailable — the file was
  uncommitted, so restoring it would have destroyed the whole item — so the repair
  was surgical, by YAML's own rules. The damage-measuring tool then produced its
  own false positive, flagging the page title as wrongly indented.

- **EMPHASIS MARKERS INSIDE TOKENS DEFEAT LITERAL ANCHORS.** ``Call **911**``,
  ``and **trouble with balance**.`` and ``grow again**, from a piece`` each
  defeated an exact-match edit, aborting two runs. Fixed as a **class**, with a
  matcher permitting `[*_]*` between every character, rather than three times by
  hand. Related: **validate every anchor before writing any of them** — a partial
  edit run leaves the file in a state no tool was written to expect.

- **WRITE BREAK MUTATIONS FROM THE TEST BODY, NEVER THE TEST NAME.** WI-540's only
  survivor came from the one mutation written off a test's *comment*, which
  described the guard incorrectly. All 27 guard bodies here were read before an
  anchor was chosen. A name is a summary, and a summary is not a specification.

- **THE EXIT-CODE TRAP, FROM THE OTHER SIDE.** A trailing `grep -c` turned a fully
  green run — ContentCheck 273/0, suite 2,249/2,249 — into a `failed`
  notification, because grep found no matches and returned 1. The grep was also
  **meaningless**: `dotnet test` does not print passing test names at default
  verbosity, so it could never have matched. The honest evidence that the two new
  guards ran was the total moving 2,247 → 2,249. Read the verdict from the log,
  and check that a check *could* have succeeded.

- **PROOF: 133 break mutations, every one red on LF AND on CRLF, first run** — no
  survivors, no ambiguous anchors, no no-ops, across the page, two shared blocks
  and the sibling this hub routes to. The CRLF half is not ceremony: a test whose
  regex cannot match a CRLF checkout fails *for free*, so the harness would report
  `ok` for every mutation aimed at it (§12.8, WI-528). Both halves passed, so the
  reds are real reds.

- **AND 8 RENDER GUARDS PROVED BY HAND, because the harness structurally cannot.**
  It runs `--no-build`, and the host serves Content copied to the test output at
  BUILD time, so a source mutation is invisible to a render test. Each was proved
  the long way — mutate, rebuild, confirm red, restore, rebuild. Two of the eight
  exist only at that layer: an INCLUDED block that stops composing, and an
  EXCLUDED block composed back in. The content-side guard reads directive *names*
  out of the source file, so it would stay green if composition itself broke.

#### WI-542 — CNS lymphoma, and the guard that was green on the defect it forbade

- **A TEST WAS GREEN ON A PAGE CARRYING EXACTLY THE DEFECT ITS NAME FORBADE, AND
  THAT IS THE ITEM IN ONE LINE.** This page's front matter declares a contract:
  every sentence saying a steroid shrinks this tumor must carry the fact that it
  comes back, because this is the one page in the corpus where "the steroid
  shrank it" can be misread as treatment. **Not one of the four shrink sentences
  did it** — all four paired the shrinking with the DIAGNOSTIC harm and none with
  the clinical one. The guard written to prevent precisely that collected the
  matching sentences, asserted the list was non-empty, and **then never used the
  list**, checking the pairing words against the whole page — where `comes back`
  matches the `## If it comes back` heading. Name asserted a per-sentence
  property; body asserted a page-wide OR.

- **AND THAT SHAPE REPEATED FOUR TIMES IN ONE ITEM: A GUARD ASSERTING A *TOKEN*
  WHERE THE CLAIM WAS A *STRUCTURE*.** The break harness found three more, each
  red on neither line ending: `Contains("Primary")` survived deleting the bullet
  that EXPLAINS what "primary" means, because the word persists inside the full
  report name; a fever-rule guard searched the WHOLE sibling page for a phrase
  that also lives in its caregiver section, so the section the deep link actually
  lands on could be gutted while the guard stayed green; and an outlook-gate
  guard asserted the bare word `spread`, which two later clauses keep alive after
  the sentence that TEACHES it is deleted. **This is the dominant failure mode of
  a phrase-matching suite.** Assert the sentence that carries the property, not a
  word that co-occurs with it.

- **THE EXIT-CODE TRAP, IN THE DANGEROUS DIRECTION.** §12.8 records WI-541 having
  a trailing `grep` turn a green harness run into a `failed` notification. The
  inverse is worse and happened here: `break-tests.py` printed
  `BREAKS THAT DID NOT FAIL: …` for three survivors and **reported exit 0**,
  because it had been piped through `tail` and **a pipeline's status is the last
  command's**. "Exit 0" is exactly the evidence a close-out cites. **Never pipe a
  verdict-bearing tool**; if you must, read `PIPESTATUS[0]`.

- **THERE ARE THREE RESTATEMENT GUARDS IN THIS CORPUS, NOT TWO.** WI-541's
  headline lesson was an undercount. `BrainMetastasesPageTests.Shingles` strips
  **neither headings nor link targets**, so the §12.3-MANDATED caregiver heading,
  plus the literal `caregiver` directive token, plus a page's opening two words
  form one window. Both other probes reported **zero** and the full suite then
  went red. Six hubs already open that section with "Three things" and three with
  "Two things", invisible to one another because their own tests use a different
  variant — so variant 3 is a tripwire that fires on whichever new page picks its
  neighbour's opening words, not a guard. Modelled offline now; handed to `/pm` to
  decide which variant is correct, because it cannot be all three.

- **A GLOSSARY TOOLTIP CAN CONTRADICT THE SENTENCE IT FIRES IN, AND THAT IS A NEW
  SHAPE.** WI-541 met the ECHO form, where a popover repeats the page's own
  clause. Here a draft called leucovorin "a rescue medicine" — a glossary term
  meaning *a seizure medicine kept at home, given up the nose or under the
  tongue* — so a chemotherapy paragraph rendered with a definition that was
  simply wrong for it. An echo wastes attention; **a contradiction tells the
  reader something untrue**. A second instance: the shared `whole-brain-radiation`
  entry gives the brain-metastases rationale, which is wrong where the treatment
  is consolidation. **A definition written from the first page that needed it
  becomes an assertion on every later page that uses the word.**

- **EIGHT DEFECTS EXISTED ONLY IN THE COMPOSED PAGE.** Beyond the tooltips: a
  block composed ~110 lines ABOVE the steroid section told the reader that
  swelling medicines change how you feel "without the tumor being any different",
  which is true on the other 23 hubs and false on the one page whose central
  section exists to say so; a refusal paragraph falsified itself by claiming
  "nothing tumor-specific is added beneath it" immediately above a tumor-specific
  paragraph; and a caregiver lead reworded to escape a shingle left "Three of
  them" with no antecedent once the block's seven items sat above it. **None was
  visible to ContentCheck, any collision probe, or a green suite.**

- **A FIX AIMED AT A MECHANICAL GATE IS THE MOST LIKELY KIND TO DAMAGE MEANING**,
  and an APPEND-style edit cannot be verified by asserting the new text is
  present. One remedy here appended where it should have replaced: the page
  briefly said the same thing twice and **still carried the wording the review had
  objected to**, while the suite, ContentCheck and all three probes stayed green
  and the new assertions passed happily beside the sentence they were meant to
  displace. **A guard for a replacement must assert the OLD wording is ABSENT** —
  §12.14's banned-source rule, arriving through the editing tool.

- **WHEN YOU WIDEN A BAN, RE-ASK WHETHER A CORRECT SENTENCE CONTAINS IT.** Three
  times in this item a ban fired on correct prose: a grade ban forbade the page's
  own *"there is no grade 1 to 4 for this one"*; an interval ban widened on review
  advice immediately caught *"Blood is taken every day"*, which is ward routine
  and §12.4 R1 orienting duration. The figures ban failed in the opposite
  direction — its canary *"three times more likely"* matched no branch, so the ban
  could not fire at all. **Only a canary distinguishes too-wide from too-narrow**,
  and each exemption now asserts it still matches something, or it has quietly
  become a hole.

- **AN ABSENCE CLAIM IS A CLAIM, AND A DOSSIER'S IS NO BETTER THAN A REVIEWER'S.**
  The research pack instructed *"do not attribute the steroid caveat to EANO — the
  words corticosteroid and steroid do not appear in that guideline"*. **They do**,
  as chemotherapy regimen components. The conclusion survived; the reason given
  for it did not. A word-presence test is not an absence-of-guidance test.
  Relatedly: the "4–5 day hospital stay" was lifted from a paper whose PURPOSE is
  to move the treatment OUT of hospital, so the page says "usually" and names the
  alternative rather than publishing a rule its only source argues against.

- **PROOF: 119 break mutations, every one red on LF AND on CRLF** — no survivors,
  no ambiguous anchors, no no-ops, across the page, two shared blocks, and BOTH
  siblings this hub is coupled to. Not first-run: the first pass produced three
  survivors, and all three were **weak guards rather than weak mutations**
  (§12.8's WI-537 distinction), so the guards were rewritten rather than the
  mutations weakened. The CRLF half is not ceremony — a test whose regex cannot
  match a CRLF checkout fails *for free*, so the harness would report `ok` for
  every mutation aimed at it (§12.8, WI-528).

- **AND 7 RENDER GUARDS PROVED BY HAND, because the harness structurally cannot.**
  It runs `--no-build`, and the host serves Content copied to the test output at
  BUILD time, so a source mutation is invisible to a render test. Each was proved
  the long way — mutate, rebuild, confirm red, restore, rebuild. Two exist only at
  that layer: an INCLUDED block that stops composing, and an EXCLUDED block
  composed back in. Note the anchor choice: `#fever-rule` appears TWICE on this
  page, so the `count == 1` assertion would have aborted the run — the
  whole-brain-radiation deep link was used instead. Picking the obvious anchor
  would have looked like a broken script rather than a shared link.

#### WI-543 — spinal cord tumor, and the split that was never one claim

- **THE CONTRADICTION WAS TWO CLAIMS WEARING ONE SENTENCE.** WI-536 handed over a
  "strength split": three hubs filed new weakness and bladder trouble as *a reason
  to be seen quickly* while `blocks/spinal-cord.md` filed the same signs as *right
  away, at any hour*, and the obvious reading is that one of them is wrong. Neither
  was. **Metastatic cord compression and a primary tumor in the cord are different
  populations with different sourced urgency**, and ACS grades them differently on
  its own two pages — "call your doctor right away or go to the emergency room" for
  cord compression, plain "see a doctor" on its spinal-cord-tumor page. Picking one
  tier would have over-triaged one reader or under-triaged the other. **Before
  reconciling two rules, check they are answering the same question.**

- **A RECORDED ABSENCE IS EVIDENCE, AND IT HAS TO BE SCORED BEFORE YOU DRAFT.** No
  fetched patient-facing source applies an emergency rule to a *named primary* cord
  tumor outside cauda equina: AANS, ABTA, Columbia and NCI PDQ each list the signs
  and give no urgency rule at all. That absence is what licensed the weaker half,
  and it is only trustworthy because the 40-source table in `NOTES-urgency.md` was
  filled in independently of the sentence it later justified. **An absence noticed
  while defending a draft is a rationalisation; an absence recorded before drafting
  is a finding.**

- **RETIRE A SHIPPED RULE OUT LOUD, OR IT BREAKS QUIETLY.** WI-528's "three pages,
  one wording" was built when all three pages were treated as one claim, so settling
  the split necessarily retired it. It was rewritten with its reasoning, never
  deleted — the backlog said *rewritten, not deleted*, and that instruction is the
  only thing standing between "we changed our minds with evidence" and "a guard went
  red and someone removed it".

- **A SAFETY GUARD COUNTED A *NEGATED* TIER AS AN ANSWER.**
  `AssertNoWarningSignIsNormalised` matched tier phrases by MENTION, so
  *"this is not an ambulance call"* and *"rather than going to the emergency room"*
  satisfied the very check that exists to prove a warning sign was tiered. The hole
  predates this item — `blocks/escalation.md` ships "is not an ambulance call" on all
  19 hubs — but adding the emergency-room tier made it live on a page calling the
  helper. **A guard that cannot see a negation reads a denial as a promise.**

- **WIDENING WHAT COUNTS AS *ANSWERED* IS THE PERMISSIVE DIRECTION.** The same set
  knew ambulance, right-away and same-day but not the corpus's STRONGEST
  instruction, so a paragraph sending the reader to the emergency room scored as
  untiered and the guard demanded a *weaker* phrase before it was satisfied. Every
  added phrase was checked to be stronger than the ones already there, counted
  across eight files rather than assumed — and "seen quickly" was deliberately NOT
  added, because that one is weaker.

- **CommonMark STRIPS ONE TO THREE LEADING SPACES, SO AN INDENTED LEAD-IN RENDERS
  IDENTICALLY.** A `continuation` skip in the escalation-list guard therefore made
  it walk-around-able on six pages: indent the sentence and the guard stops looking
  while the reader sees no difference. **Any guard keyed on source indentation is
  keyed on something the reader never receives.**

- **THE TESTS WERE WRONG ROUGHLY TWICE AS OFTEN AS THE PROSE THEY PROTECT** — about
  24 defects in the guards against 12 in the page. And in `/review` rounds 2, 3 and
  4 the blocker was *created by the previous round's fix*, which is WI-536's "a fix
  is where the next defect comes from" arriving three times in one item.

- **TWO MORE DEAD BANS, BOTH UNPROVABLE BY INSPECTION.** A recovery-figure ban read
  `\b\d{1,3}\s*(?:%|percent)\b` — **`%` and the following space are both non-word
  characters, so that `\b` could never match**, and the ban had been incapable of
  firing since it was written. A citation ban read `^\s*-\s*url:.*cimpact` when
  **neither of that paper's URLs contains "cimpact"** (`europepmc.org/article/MED/32502305`,
  `pubmed.ncbi.nlm.nih.gov/32502305`), so it policed a form the real source cannot
  take. Both were found by planted mutations, not by reading. Note the related trap:
  the FIRST version of that ban forbade the bare word and fired on the §12.14
  sentence naming cIMPACT-NOW as unreachable — **the prose the rule requires.**

- **AND THE MIRROR CASE: A SURVIVOR THAT WAS A WEAK *MUTATION*.** The standing rule
  presumes the guard is at fault, and twice here it was — which is exactly why the
  third case needed evidence rather than assertion, because "weak mutation" is the
  verdict that lets an item move on. `dip-paragraph-loses-its-tier` deleted a
  *cross-reference* ("the emergency rule above still applies") while the sentence
  after it still named three signs and said "means the emergency room now". Green
  was correct. **The test to apply: does a reader of the mutated page still get the
  instruction?** Rewritten to §12.12's dangerous direction instead — keep the signs,
  downgrade the tier — and deliberately leaving the reassurance in place, because
  stripping it would make the guard skip the paragraph and pass for the wrong reason.

- **FOUR DEFECTS EXISTED ONLY IN THE COMPOSED PAGE**, again invisible to source
  review: `[ESCALATION]` composing ABOVE the cord rule, `[CAUSES]` scoping below the
  block, a tumor-board tooltip echo, and a duplicate `###` heading created by the
  first of those fixes.

- **PROOF: 53 break mutations, every one red on LF AND on CRLF**, every anchor
  written from a test's BODY. Not first-run — three survivors, diagnosed
  individually rather than in a batch, each fix RE-RUN against the mutation that
  exposed it, because a fix that has not been re-run is not a fix.

- **AND 6 RENDER GUARDS PROVED BY HAND**, including the block ruling from BOTH
  directions — an INCLUDED block (`[TUMOR-BOARD]`) that stops composing, and the
  EXCLUDED `[SPINAL-CORD]` composed back in. That exclusion is the one a later
  editor would "correct" believing they were fixing an omission, and an exclusion
  leaves no trace on the page, so it can only be pinned from the other side.

- **RUN THE SMOKE PROBE *BEFORE* THE DEPLOY, AND REQUIRE IT TO FAIL.** New here, and
  it should be standard. Against live production, with the stub still served, the
  retargeted probe printed **controls ok** and then **exit 1 with 23 failures** —
  the hub under its floor, no growth, 13 fragments missing, both re-tiered siblings
  static. Two hours later the identical command against the identical host printed
  `smoke clean`. **A probe that has never been seen to fail has not been shown to
  be capable of failing**, and "it passed" is otherwise indistinguishable from "it
  cannot tell". This is the falsification half of the CONTROLS idea, which only
  ever proved the probe could match text that was *there*.

- **AND THAT PRE-DEPLOY RUN IMMEDIATELY FOUND THREE DEAD FRAGMENTS.** Of 16 positive
  probes on the hub, **three did not fail** when the page was still a stub:
  `This is not a brain tumor`, `Call an ambulance` and `You are allowed to ask
  questions`. The stub already said the first and already composed the blocks
  carrying the other two. They still prove composition happened, but they **cannot
  distinguish a landed deploy from an unchanged one**, and had the whole probe been
  built from strings of that kind it would have reported a clean pass over a deploy
  that never arrived. A positive fragment is only evidence of a deploy if the page
  did not already contain it.

- **A THIRD VERDICT-TOOL DEFECT, AND THE WORST-BEHAVED ONE YET.** §12.8 records two
  exit-code traps; this is the third in the family. Choosing smoke fragments, bash
  `grep -F` **crashed** — SIGABRT, exit 134 — printed nothing at all, and the
  `| wc -l` it fed reported a clean `0` for **every** string, including
  "emergency room", which occurs eight times in the file, and "911", which the
  composed escalation block says out loud. Reading those zeros as absences would
  have banned text that is legitimately present and failed a healthy deploy. **A
  dying tool piped into a counter is indistinguishable from a real negative.**
  The same session also showed `gy` matching five times inside "surgery" and
  "oncology". Count fragments with the matcher the probe itself will use, and never
  pipe a verdict-bearing tool (§12.8, WI-541 and WI-542).

- **A TRANSIENT 5xx WILL BE READ AS A FAILED DEPLOY.** The pre-deploy run reported
  HTTP 500 on `/` and `/research`; three re-checks fifteen seconds apart returned
  200 with byte counts **identical to the baseline**, so the fault was the probe's
  own burst of requests, not the site. The probe now retries once on 5xx **and
  prints that it did**, so a flaky response cannot masquerade as a failed deploy
  and a genuinely broken deploy cannot hide behind the retry.

- **A GUARD CAN BE VACUOUS BECAUSE OF MARKDOWN, NOT LOGIC (WI-544).**
  `SentencesOf` splits on `(?<=[.!?])\s+`, and `ReaderText` strips the `!%…%`
  markers but NOT `**`. So in a bolded bullet list the character after
  *"call 911."* is `*` rather than whitespace, no split happens, and one
  "sentence" runs on through the NEXT bullet. An assertion that the shunt
  **infection** rule keeps its emergency room therefore passed on the venue
  belonging to the bullet ABOVE it — for the entire item. Strip the infection
  venue and 2,345 tests stayed green. **It was invisible to review (the
  assertion reads correctly), to the suite (it passes), and to the dry run (the
  anchor resolves). Only mutating the page and demanding red could see it.**

- **A PROPERTY WITH TWO HOMES CANNOT BE BROKEN BY A SINGLE-POINT MUTATION.** Two
  of that run's three "survivors" were weak *mutations*, not weak guards:
  `NAMES NO VENUE` is recorded in two source entries, and `median` appears twice
  inside the outlook gate, so removing one left the property standing and the
  guard correctly green. `uniq` is still the right default — an ambiguous anchor
  usually means the mutation is aimed at the wrong text — but a guard that
  asserts a PROPERTY rather than a TOKEN needs an `every`. **A mutation that
  leaves the property standing is a harness survivor by construction, and the
  dry run cannot detect it: it only proves anchors resolve.**

- **A REMOVAL DOES NOT AUTOMATICALLY EARN A MUTATION.** After deleting two
  pack-grounded staging words, the reflex was to add a mutation restoring them.
  Nothing asserted their absence, so it would have left its guard green. The
  repair that suggested itself — write a word-ban so the mutation has something
  to break — is §12.14 inverted: the defect was the unsourced *definition*, not
  the vocabulary, and the ban would have blocked a future editor who found a real
  source. **Mutations prove guards can fail; they do not commemorate edits.**

- **REACH FAILS IN BOTH DIRECTIONS, AND THE TOO-WIDE KIND IS WORSE.** Too narrow
  guards nothing: a ban keyed to the exact sentence already fixed protects
  against nothing, and `\bvomit\b` cannot match "vomiting". Too wide fails
  correct prose — `"ring your"` is a substring of **"bring your"**, `"rings"`
  matches **"coverings"**, and a widened surveillance-interval ban fired on the
  sibling hub's own *"Ask again each year."* **The second kind is more dangerous,
  because the reflex when it fires is to weaken the guard rather than question
  the widening.** Both directions want canaries: positive ones proving the ban
  fires, negative ones proving it does not fire on the prose it must allow.

- **SIBLING-PAGE GROUNDING: A CLAIM THAT REALLY IS SOURCED, JUST NOT HERE.** Two
  rounds found claims whose verbatims lived on `/tumors/pediatric-brain-tumor`'s
  front matter rather than this page's — harder to notice than a pack-grounded
  claim precisely *because* it is genuinely sourced somewhere in the repository.
  A third variant is block-composed grounding, which is legitimate (block sources
  merge and render) but still has to be written down. **Recording a ROUTE is not
  recording a SOURCE, and a note asserting that a claim is grounded is itself a
  claim.**

- **WRITING A LESSON DOWN IS NOT APPLYING IT**, demonstrated twice inside one
  item. The front matter said *"fixing the flagged instance and not the class is
  its own recurring failure"* one round before the same class was fixed one
  bullet and left in the two beside it. And a note saying *"the capture is for
  reading, the source is for editing"* was written one round before two more
  edits were composed from the re-wrapped capture — one of them the safety
  blocker, which therefore silently did not land on the first attempt.

- **A REVIEW'S VERIFICATION DESERVES THE SAME CHECK AS ITS CLAIMS.** A round
  proposed widening a ban page-wide, stating it had checked and the only false
  positive was a questions list. Run over the rendered capture, the candidate
  fired on **three legitimate sourced sentences**. Taken on trust it would have
  turned the suite red on correct prose — and the natural next move, weakening
  the ban, would have quietly undone the guard.

#### WI-545 — chordoma, and the exit code that hid a real survivor

- **A PIPELINE'S EXIT STATUS IS THE LAST COMMAND'S, AND THIS SECTION ALREADY SAID
  SO.** §12.8 records WI-542 losing a run to `break-tests.py | tail`. This item
  piped the same tool through `tail` anyway, and the harness reported **exit 0**
  while its own output carried `BREAKS THAT DID NOT FAIL`. The survivor was real.
  **Writing a lesson down is not applying it** — the second time that has been
  recorded here, which is itself the finding. The fix is mechanical, so use it:
  redirect to a file, `echo $?` on the next line, and read the file. Never let a
  verdict reach you through a pipe.

- **A SURVIVOR CAN BE A GUARD THAT A NEIGHBOURING ENTRY QUIETLY SATISFIES.** The
  one survivor of 142 runs was `TheChondrosarcomaContrastKeepsAllThreeDiscriminators`,
  which asserted `does not carry brachyury` against the WHOLE report section. The
  **parachordoma** paragraph three bullets up says the same words for a different
  look-alike, so the chondrosarcoma entry could lose its discriminator entirely
  and the guard stayed green, on both line endings. A guard asserting a TOKEN
  where the claim is a STRUCTURE, green on the defect its own name forbids. The
  guard was rewritten to the bullet's scope, with a second assertion so the
  parachordoma copy cannot stand in for it; the mutation was not weakened.

- **SCOPE AN ABSENCE BY SUBJECT, NEVER BY PUBLISHER.** This page's central ruling
  is a scored absence: no chordoma source gives a 911 or emergency-room rule. The
  first draft wrote that as "we looked through the chordoma organizations and *the
  hospital pages*, and not one of them says…" — and then quoted **three Cleveland
  Clinic rules four paragraphs later**. The claim was not merely loose, it was
  falsified by the page's own next screen. The true claim is about subject: no page
  *about this tumor* gives the rule. Both the publisher-scoped form and its
  anaphoric variant ("not one of them") are now banned with canaries.

- **A COUNT AGAINST A STALE ARTEFACT IS NOT EVIDENCE, AND IT FAKES AN ABSENCE.**
  `cauda equina` counted **0** in the rendered capture after the sentence naming it
  had been added — because the capture predated the edit. An absence measured
  against a stale file is indistinguishable from a real one. Re-capture before
  counting, and treat any surprising zero as a question about the artefact first.

- **A CORRECT FIX CAN CREATE A BIDIRECTIONAL RESTATEMENT COLLISION.** Review
  correctly required "nearly always comes back in the same place" to be weakened to
  the source's "usually" — and the corrected sentence landed on wording
  `/tumors/craniopharyngioma` already carried, turning a SHIPPED page red. Twice in
  this item a new page broke an old one (the first draft shared 61 windows across
  eight files). The guard runs in both directions and an allowlist cannot fix it,
  because the other page's test never reads this page's.

- **SMOKE NEGATIVES ARE PER PAGE AND MUST BE COUNTED, NEVER CARRIED FORWARD.**
  Four bans inherited from the previous item would each have failed a healthy
  deploy here. The sharpest: `"advice, not an order"` was a NEGATIVE there because
  that page EXCLUDED `[TUMOR-BOARD]`, and is a required POSITIVE here because this
  page includes it. `"percent"`, `"tumour"` and `"Cancer Research UK"` are all
  legitimately present, the last two inherited into the composed source list from
  `blocks/escalation.md`. Count every candidate through the capture before banning it.

- **THE FRONT MATTER IS A DECOY FOR MUTATION ANCHORS.** Two anchors matched twice
  and the dry run refused them: the page records its sources' verbatims, so a body
  phrase ("should be done from the back") and even the support phone number appear
  above the body as well. `uniq` folds whitespace across the WHOLE file. Lengthen
  the anchor to text that exists only in reader prose.

- **A CLAIM SOURCED ON ANOTHER PAGE IS STILL UNSOURCED HERE, AND IT TOOK TWO ROUNDS
  TO CLEAR.** Round 1 found load-bearing claims whose verbatims lived only in the
  research pack; round 2 found three more of the same class, one of them inside a
  sentence round 1 had just edited without re-checking its source. **12 sources were
  re-fetched and recorded with verbatims**, and six claims were reworded because the
  source did not support the wording that had been written.

- **A REVIEW'S FINDINGS DESERVE THE SAME VERIFICATION AS ITS CLAIMS, AND TWO WERE
  REJECTED ON EVIDENCE.** Review read ">half recur" as widened from the consensus
  paper's treated cohort; the Chordoma Foundation states it of **all** chordoma
  tumors, patient-facing, and the citation was what was missing. Review also read
  the caregiver comparative as a cross-measure comparison wrongly credited to the
  Foundation; the Foundation makes that comparison **in its own survey**. Both
  rejections are recorded in the front matter with their verbatims so the next
  reader does not re-open them.

- **PROOF: 71 break mutations, every one red on LF AND on CRLF (142/142) on the
  clean run**, after one first-run survivor that was a weak guard and was rewritten;
  **8 handproof cases across all 7 render guards**, including both directions of the
  block ruling (an included block that stops composing, and an excluded one composed
  back in); suite **2,378 / 2,378**; ContentCheck **275 / 0** at grade **5.9**; all
  three restatement guards at **0 collisions**; four rendered reads; three `/review`
  rounds (2 blockers, then 1, then none); privacy scan **0 findings over 2,457
  net-new lines**.

#### WI-546 — CNS germ cell tumor, and the table that proved nothing by leaving a tumor out

- **AN ABSENCE FROM A TABLE OF "SELECTED TYPES" IS NOT EVIDENCE OF ANYTHING.** The
  first draft told readers these tumors "are not usually given a grade", because the
  WHO CNS5 summary's grade table leaves them out. `/review` round 1 read the table's
  own title: *"CNS WHO grades of selected types, covering entities for which there is
  a new approach to grading, an updated grade, or a newly recognized tumor"*.
  Medulloblastoma and craniopharyngioma, both graded, are missing from it too. The
  same paper says a germinoma "can be assigned a CNS WHO grade 4 designation". The
  claim had no source at all, and a test was pinning it as TRUE. **Before reading a
  table's silence, read what the table says it covers.** WI-545's chordoma front
  matter leans on the same table; its reader-facing sentence ("lists chordoma and
  prints no grade") is literally true, so it is recorded for `/pm`, not reopened.

- **A RETARGET THAT FAILS LEAVES THE PREVIOUS ITEM'S TABLE LOADED, AND IT REPORTS
  CLEAN.** A `sed` retarget of `dryrun.py` failed on an escaping error. The dry run
  then imported WI-545's table from WI-545's directory and printed **"71 mutations, 0
  problems"** — a clean result over chordoma. The only tell was its last line,
  `targets: chordoma.md, proton-therapy.md`. The runner's import check asserted
  `'chordoma' in CHORDOMA`, which the stale table satisfies by construction: §12.8
  already records (WI-542, `render-read.py`) that **a guard keyed to the thing being
  carried forward cannot detect that it was carried forward.** The runners now
  assert this item's page, and the retarget itself asserts the previous item's name
  is gone. Read the targets line of every clean result.

- **A FETCH CAN RETURN HTTP 200 AND STILL BE AN ABSENCE-SHAPED LIE.** The live
  verifier met two: cancer.org sends **gzip nobody asked for**, which urllib does not
  inflate, so 13 real quotes read as missing; and PubMed Central answers bursts of
  requests with a **"checking your browser" page and a success code**, so present
  quotes read as missing on a second run. Both now read as BLOCKED, never absent.
  **And the evidence log repeated the trap in prose**: it claimed every load-bearing
  claim was re-fetched live when the proton paper's fetches had all hit the block.
  Review caught it; the paper was re-verified through Europe PMC's full-text service,
  and the route is recorded where the claim is.

- **A POSITIVE-HALF FAILURE ON AN EMPTY SET IS THE GUARD WORKING.** Two rewrites to
  answer `AssertNoWarningSignIsNormalised` ("the usual sign is weakness", "tiredness
  is common") left no paragraph pairing a sign with a reassurance at all. The guard
  then examined nothing, and its `mustHaveChecked` half failed with an empty
  collection. **That is the WI-517 lesson firing on schedule**: a normalising guard
  that examines nothing is green on anything. The fix was not a third rewrite. The
  true reassurance (tiredness *is* common) came back WITH its tier beside it, which
  is what the guard asks of a reassurance.

- **A FIX CAN OVERSHOOT ITS SOURCE IN THE OTHER DIRECTION.** Round 1 found the
  markers-alone rule stripped of its scope (the consensus states it under "Strategy
  for NGGCT"). The fix said high markers point to the other kinds, "not to a
  germinoma" — which round 2 found the same source contradicts: a germinoma "can
  also secrete small amounts of hcg", and there is "no consensus regarding the cutoff".
  A family told their germinoma raised hCG would have read it as a wrong diagnosis.
  **Each round's fixes are the next round's findings**, which is why this item ran
  three rounds (1 blocker, then none, then none) and not one.

- **A GLOSSARY POPOVER CAN FIRE INSIDE A PROPER NAME, AND ONLY A RENDERED READ SEES
  IT.** The first rendered read found the hydrocephalus definition injected into the
  middle of "the Hydrocephalus Association", on the ETV rule. Every content guard was
  green, because the source text was correct. It was the page's only popover, so the
  suppression's paired positive (§12.10: suppression is page-wide, so an absence-only
  test passes if the glossary stops firing) had to be moved to **another page**
  (`/tests/ct-scan`, the one page that uses the word unsuppressed).

- **A LONG MUTATING RUN MUST BE DETACHED, AND ITS UNMUTATED COPY MUST EXIST BEFORE
  IT STARTS.** The harness restores pages in a `finally`, and a tool timeout kills the
  process without running it, leaving an uncommitted page carrying a mutation that
  `git checkout` cannot undo. The clean copy was the **build output**: the harness
  builds once, then every run is `--no-build`, so `bin/.../Content` holds the pages as
  they were before the first mutation. Both runs went detached, with the exit code
  written to a file by the process itself.

- **PROOF: 120 break mutations, every one red on LF AND on CRLF (240/240), first
  run**; **10 handproof cases across all 7 render guards**, three of them from both
  directions (the block ruling, both tooltip suppressions, both doors); suite
  **2,416 / 2,416**; ContentCheck **276 / 0** at grade **5.6**; both restatement
  variants at **0 collisions**; two full rendered reads, re-captured after each
  review round; three `/review` rounds (1
  blocker, then none, then none); **37 sources** recorded with verbatims, re-fetched
  live except where an entry says otherwise.

#### WI-547 — Hemangioblastoma, and the fix that overshot in the other direction

- **A SAFETY TIER CAN BE WRONG IN BOTH DIRECTIONS, AND FIXING ONE INVITES THE
  OTHER.** The brainstem paragraph went through three versions in three review
  rounds. Round 1: it named the danger ("symptoms can be more serious") and gave
  the reader NO tier, while dropping its source's own urgency half — The Brain
  Tumour Charity says such a tumor has "more chance of symptoms being severe and
  needing emergency attention". Round 2's fix added "use the ambulance list below
  if it comes on suddenly" — a suddenness gate no source gives, over a list whose
  five other bullets are unconditional, including *trouble breathing or choking*,
  which is this location's signature danger: **under-triage**. Round 3's fix then
  said "anything on the ambulance list is an ambulance call however it started" —
  false of the one bullet the block deliberately gates ("**Suddenly** not being
  able to speak, move one side, or see"), whose gradual forms the same block files
  as same-day: **over-triage**, and invisible to `AssertEscalationTiers`, which
  polices that boundary between pages but cannot see it restated in free prose.
  The fourth version attaches the onset rule to the bullet that is genuinely
  unconditional and to nothing else. **When a tier is corrected, re-read the
  shared block bullet by bullet and ask which way the correction now points.**

- **A GUARD THAT READS A WHOLE SUBSECTION CANNOT SEE A CREDIT GO MISSING FROM ONE
  CLAIM.** The only break-harness survivor, on LF and CRLF: the watching guard
  asserted `Contains("VHL Alliance")` over its subsection, and that subsection
  credits the VHL Alliance TWICE — once for "usually not treated until symptoms"
  and again for the three treatment choices. A mutation stripping the credit from
  the CLAIM left the other standing and the guard stayed green. The mutation was
  kept and the GUARD rewritten to pin each credit to the sentence it belongs to.
  Same shape as WI-544's two-homes lesson, arriving this time through a guard
  rather than through a mutation.

- **THE LF+CRLF HARNESS PROVES MUTATIONS ARE CAUGHT; IT CANNOT PROVE THE
  UNMUTATED PAGE IS GREEN ON CRLF.** The baseline suite, run before a word was
  drafted, found a SHIPPED WI-546 guard red on this checkout:
  `NoGradeIsPrinted...` cuts the grade section out of the composed page with
  `Composed.Replace(RawSection(...), "")`. Composition emits LF; `RawSection`
  keeps the checkout's CRLF; the cut matched nothing, so the grade section's own
  "grade 4" stayed in and the test failed on correct text. CI is LF, so it shipped
  green, and the harness's CRLF half could not see it — **a test that always fails
  reports every mutation as caught.** Fixed here, with an assertion that the cut
  actually shrank the page. Run the plain suite on a CRLF checkout of the new page
  BEFORE the harness, not only after.

- **RUN THE FULL SUITE AS THE LAST ACTION BEFORE A REVIEW HANDOFF, NOT THE
  SECOND-LAST.** Round 3 opened by finding the suite red: a reading-grade reword
  of the sibling page's new bullet, made after the last `dotnet test`, had broken
  the guard that pins it. The review was therefore reading a diff whose state I
  had described wrongly. ContentCheck is a separate CI step from `dotnet test`,
  so the final gate is BOTH, after the last edit.

- **CITE EVERY PAGE YOU COUNT IN A SCORED ABSENCE.** The absence sentence claimed
  sixteen pages were "read and used"; two of the sixteen appeared nowhere in
  `sources`, so a reader chasing the claim would have found fourteen. A page that
  carries an absence is evidence, and evidence gets a citation — including the one
  read only so its survival figure could be *rejected*. Where the count is
  filtered, say so in the reader text: readable pages here DO give a sign-level
  rule (Mayo's syndicated text), excluded by provenance rather than by silence.

- **A LANDMINE RECORD WITH NOTHING BEHIND IT IS THE SAME DEFECT AS A CLAIM WITH
  NO SOURCE.** Ruling 7 listed a five-year survival figure on a site whose page
  carrying it was never fetched. It was demoted to UNVERIFIED in that source's own
  entry. Meanwhile two real landmines inside cited captures went unrecorded until
  round 3 (StatPearls' ">90% at five years", Brain Tumour Research's "curative"
  and its recurrence range), and one cited source flatly contradicts the page's
  "it stays where it started" — recorded in ruling 3a, with the reasons the
  absolute stands, rather than left for the next editor to rediscover (WI-518).

- **SCOPE A DRUG CLAIM TO THE TUMOR THE PAGE IS ABOUT.** "It is only for people
  with that inherited condition" was true of the CNS hemangioblastoma indication
  and false of the drug: the same label carries two further indications that
  require no such condition. The fix is one clause — *"For a brain or spinal cord
  tumor, it is only for…"* — and the front-matter record names the other
  indications so the next editor does not re-broaden it.

- **PROOF: 94 break mutations, red on LF AND on CRLF (188/188) after one
  first-run survivor that was a WEAK GUARD, rewritten rather than the mutation
  weakened**; suite **2,457 / 2,457**; ContentCheck **277 / 0** at grade **4.8**;
  both restatement variants at **0 collisions** with no allowlist; two rendered
  reads; **FIVE `/review` rounds** (3 blockers, then 1, then 2, then none, then
  none) — rounds 2 and 3 each found their blocker *inside the previous round's
  fix*, which is why the rule is re-review after fixing, not review once.

**WI-548 — the sweep, and the item whose three defects were all in its own
machinery.** The first item in this phase that writes almost no prose and almost
all instrument. Its full ruling is §12.15; these are the lessons that generalise.

- **"DERIVE, NEVER TYPE" IS NOT ENOUGH — DERIVE FROM AN INDEPENDENT SOURCE.** The
  item's own doctrine, applied literally, made a guard WEAKER than the hand-typed
  floor it replaced: the render sweep's expected count was computed by calling the
  same heading extractor the loop had just walked, so if that extractor ever
  stopped finding headings both sides went to zero and the assertion passed while
  the test's whole subject evaporated. A canary derived from the walk it is
  checking proves nothing. The floor now comes from the obligation table, which
  knows nothing about how headings are read off a file — and the fix was proved by
  stubbing the extractor and watching it go red, not by re-reading it.

- **A COUNT THAT DISAGREES WITH ITS OWN PROSE IS A DEFECT REPORT NOBODY HAS READ
  YET.** The umbrella page silently dropped one of §12.3's seventeen questions
  INSIDE the sweep built to make that impossible — six covered plus ten routed is
  sixteen. It had been visible for a whole round as a stale number: the comments
  said eleven routed questions and the array held ten. Nobody, including me,
  treated the mismatch as evidence.

- **AN EXEMPTION TABLE NEEDS A DISTINCTNESS CHECK, NOT JUST A MEMBERSHIP CHECK.**
  Asserting that a mapped heading EXISTS is satisfied by any heading that exists,
  so a question can be discharged by pointing at a heading already spent on a
  different question. Making the arithmetic work by pointing at something real is
  the same move as an exception whose only reason is that the page fails.

- **A THROW INSIDE AN ACCUMULATION LOOP DISCARDS THE REPORT THE LOOP EXISTS FOR.**
  Found in three different tests across three rounds, each time in the fix for the
  previous one. A sweep collects findings from 23 pages; one `Assert` inside the
  loop means page 3's ambiguity hides pages 4 to 23. Collect, then assert — and
  when reordering so the readable report fires first, check whether the numeric
  canary underneath is still reachable, because in one of the two tests it was
  not.

- **AN INTEGRITY CHECK CAN CHECK THE WRONG THING AND LOOK CLEAN.** A killed break
  harness left a mutation on a shipped page, reading as ordinary prose. The marker
  file flagged the interrupted run — and the first integrity check compared the
  BACKUPS against their own checksums rather than against the live files, so it
  reported all-clear. Diff the live file against the backup; a checksum file
  proves only that the checksum file is intact.

- **A GUARD CAN OVERRULE THE ITEM THAT WROTE IT, AND THAT IS THE SYSTEM WORKING.**
  The new section's best source was Cancer Research UK, which that page is BARRED
  from by a WI-537 ruling re-checked twice. The shipped guard went red on the new
  citation. The ruling was honoured rather than overturned for one convenient
  quote, the section was rebuilt on St. Jude, ACS and Dana-Farber — all re-fetched
  and read live — and the rebuild IMPROVED the content, because it forced the
  vocabulary split (relapse / recurrence / progression) the research pass had
  named as the corpus's actual gap. The same page's British-forms guard then
  caught "fits" in "ask which word fits your child": reworded, guard untouched.

- **PROOF: 36 break mutations, red on LF AND on CRLF (72/72), no survivors and no
  ambiguous anchors on the first run**; the plain suite **2,469 / 2,469 on a fully
  LF corpus AND on a fully CRLF corpus** (the WI-547 lesson discharged directly:
  the harness proves mutations are caught, not that the unmutated corpus is
  green); **3 handproof cases** against the one render guard, one per claim it
  makes; ContentCheck **277 / 0**; **FIVE `/review` rounds** (5 blockers, then 2,
  then 2, then none, then none) — rounds 2, 3 and 5 each found their finding
  *inside the previous round's fix*.

**WI-549 — `/tests/neuro-exam-and-memory-testing`, the first page whose subject
is TWO CO-EQUAL THINGS THAT ARE NOT THE SAME TEST.** (Not the first page with
two subjects at all: `/tests/planning-scans` carries six scan types and
`/tests/getting-ready-for-surgery` several appointments. What is new is that
neither subject is the dominant case — planning-scans has MRI add-ons plus one
outlier to route, and this page has two peers.) The
bedside neurological exam runs minutes; formal neuropsychological testing runs
hours.
The backlog put them on one page because they are the same conversation and the
same "am I being judged?" anxiety — which is a ruling about the READER'S
FEELING, not a claim that the procedures are alike.

- **SPLIT BY SLOT, and state the rule before drafting.** §12.8's slots are
  carried **once where the two subjects give one honest answer and twice where
  they give two** — never averaged. Two shipped pages already had the shape:
  `/tests/planning-scans` slot 4 gives the common answer then names the outlier
  and routes; `/tests/getting-ready-for-surgery` slots 4 and 5 carry a bullet
  per sub-test. **Read those before inventing one.** The failure this exists to
  prevent is a single blended answer standing in for two that differ by two
  orders of magnitude, which lies to both readers.
- **THE OPERATIONAL PROPERTY IS ATTRIBUTION, NOT COUNTING.** "Once or twice" is
  not testable — the drafted page names both subjects in the summary too. What
  a guard can hold is: **every duration must sit in a span naming exactly one
  subject.** It took three review rounds to get there, and each intermediate
  version shipped a hole:
  1. **Token presence** (both labels appear, "minutes" appears, "hours"
     appears) — defeated by a paragraph that named both and said "an average of
     the two is close enough".
  2. **Label pairing** (a span naming the exam has minutes; a span naming the
     testing has hours) — defeated three ways: a duration attached to NEITHER
     label, one label carrying the other's number, and a blend written in the
     page's own short forms ("the testing") which the full-label matcher could
     not see.
  3. **Per-duration attribution**, driven off every duration match, with spans
     re-split into sentences when they name both. **Keep every defeated shape as
     a literal canary case**; a guard is only as good as the shapes it has been
     proved to reject, and the ones that escaped are the only proof available.
- **A CEILING RULE HAS TO BE NARROWED, NOT DROPPED.** Banning any hours figure
  from the exam's span failed the page's own sourced sentence, because a full
  exam really does run up to an hour and the two ranges legitimately overlap
  there. The reflex was to delete the rule — and deleting it reopened the hole
  exactly, letting "the exam in the room, in full: most of the day" pass clean.
  The honest ceiling is **an hour, singular**, so the rule keys on plural
  "hours", "several hours" and "most of the day". §12.8's too-wide warning cuts
  both ways: **the correction for a too-wide guard is a narrower guard, not no
  guard.**
- **A DOSSIER IS NOT A SOURCE, AND THE UNCITED LINE IS THE ONE THE ITEM TURNS
  ON.** `docs/research/tumor-guides/tests-library.md` §7 carries a URL on every
  claim except three — the bedside exam's "10 to 20 minutes", the testing's
  "several hours", and "there is no pass or fail". Those were the page's
  spine. **"10 to 20 minutes" is supported by nothing**: the live sources say a
  structured brain-tumor exam runs about four or five minutes and a fuller one
  half an hour to an hour. It is banned by name, with a canary proving the ban
  can fire.
- **FEASIBILITY IS NOT ADOPTION.** "Readily performed within the time frame of
  routine office visits" says a scale CAN fit a visit. It does not say clinics
  DO use it. The page said clinics use it and score you — attached to a duration
  a reader plans an appointment around. Checked live: no adoption claim exists
  in the paper. Attribute to the INSTRUMENT and hedge the use
  ("where your team uses that one"). The over-read survived one review round in
  a second section after being fixed in the first, **with a front-matter comment
  still swearing the page did not make the claim** — a comment that disagrees
  with its own page is a defect report nobody has read.
- **"A CLINIC SAYS EXACTLY THAT" IS A CITATION, AND IT HAS TO BE EXACT.** The
  page claimed the battery is *built* so nobody gets everything right, then
  attributed it to a clinic that only observes that people find some items easy
  and some hard. A claim about test CONSTRUCTION is not a claim about people.
- **WHERE THE CORPUS ALREADY HAS THE EASY VERSION, THIS PAGE OWES THE HARD
  ONE.** "You cannot fail this" appears three times in the corpus and every one
  is about a SCAN, where the reader does nothing and a machine takes a picture.
  On a scored test that sentence is false. This page concedes the measuring in
  the same breath as the answer — the opening both answers "no" and admits the
  scoring — then says there is no line you fall below. **Order is the property,
  and a guard for it must anchor to the subject being conceded**: a bare
  `\bscored\b` matched the page's FIRST use of the word, which belonged to the
  other test, so deleting the real concession left the guard green.
- **A CLAIM WHOSE SOURCE COULD NOT BE READ BECOMES A QUESTION.** hhs.gov 403'd
  twice, so the only candidate source for "an employer cannot get your report
  without your authorization" was unreadable. The claim is asserted nowhere and
  appears only in "What to ask your team", which asserts nothing. The blocked
  URLs are recorded in the front matter and cited nowhere — **and the test
  asserts the RECORD as well as the absence**, because a first version went
  greener when the record was deleted.
- **A RULING IN `work_files/` SHIPS NOWHERE.** `.claude/work_files/` is
  git-ignored, so the Gate 1 ruling, its corrections and its pre-committed
  fallbacks live only in this section. Write the durable half here before the
  PR. (The fallback that mattered: *if no citable source carries a duration,
  publish the SHAPE and not the number.* It was pre-committed at Gate 1 and
  still had to be enforced by review three rounds later, on a repeat interval
  no source carried.)

- **A CEILING RULE NEEDS ITS MIRROR.** The rule keeping the testing day's
  figures off the exam was written, defeated, deleted, reinstated and narrowed
  across three rounds — and for all of that it still only ran in ONE direction.
  A span naming the testing and carrying a minutes-only figure passed clean,
  telling a reader booked for a half-day to expect five minutes, and the
  allow-list could not backstop it because the offending phrase is licensed
  **for the other subject**. Whenever a guard encodes "this figure belongs to
  A, not B", write B's half at the same time.

- **PROOF: FIVE `/review` rounds** (4 blockers, then 3, then 4, then none, then
  none), and **rounds 2, 3, 4 and 5 each found their findings inside the
  previous round's fix** — the anti-averaging guard alone was defeated four
  times before it held, one round's fix (deleting a too-wide rule) reopened the
  exact hole the next round found, and the *replacement* rule then rejected the
  page's own sourced ceiling written numerically. Every defeated shape is kept
  as a literal canary case, reject and accept, which is the only durable record
  of what the guard has actually been proved to catch. **32 break mutations red
  on LF AND on CRLF (64 / 64), no survivors and no ambiguous anchors** on the
  second run; the plain suite **2,503 / 2,503 on a fully LF corpus AND on a
  fully CRLF corpus**; **4 handproof cases**, one per claim only a rendered read
  can check; ContentCheck **280 / 0**; the page grades **5.2**; all three
  restatement variants clean; **twelve sources, every quote re-fetched and read
  live**, with **seven** blocked publishers recorded in the front matter and
  cited nowhere.

- **THE HARNESS FOUND TWO THINGS FIVE REVIEW ROUNDS DID NOT**, and both were
  about what a guard cannot see rather than what it says. A planted
  *"Ask again in a fortnight"* SURVIVED: the word is unmistakably British and
  was on no list, so the gate that exists to catch exactly that could not. And
  a disagreement guard written as an OR over two phrasings could not be broken
  at all, because the flattened page carries BOTH — §12.8's "a property with two
  homes cannot be broken by a single-point mutation", walked into by the item
  that was citing it. The fix for the second is not a longer OR: it is to forbid
  the OPPOSITE claim, since a page saying two comparisons agree is what would
  actually mislead a reader. **Review reads what the page says; only a mutation
  shows what the guard can see.**

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

### 12.15 What "the full section template" means, per page class (WI-548)

WI-412 shipped the tumor library with the index saying out loud how many types
were still unwritten. WI-547 took that count to zero, and left this item the
half that is harder to state: **every slug in `taxonomy.yml` resolves to a page
carrying the full section template — not merely to a file that exists.**

**The unit of obligation is a QUESTION, not a heading string.** §12.3's
seventeen are the questions a reader arrives with. A page discharges one of them
in exactly one of three ways:

1. **Answers it under the standard heading** — the default, and what twenty of
   the twenty-three pages do.
2. **Answers it under its OWN heading**, because the standard wording would be
   false for this reader.
3. **ROUTES it** to the page that owns the answer (§12.10: route, don't
   restate).

**It may never simply be dropped.** That is the whole strengthening, and it is
what stops the exception list from being "the pages that currently fail". An
exception here is not a skip — it is a *different, equally complete* obligation
that the test still checks. Every class is defined by a property of the **slug**,
so its reason would still hold for a page nobody has written yet.

| Class | The slug names… | Default | Owes |
|---|---|---|---|
| **Hub** | one diagnosis a person can be told they have | **yes** | all seventeen, standard headings, §12.3 order |
| **Secondary hub** | a tumor that did not start in the CNS | no | seventeen, with §2 and the self-blame heading substituted |
| **Axis hub** | a cross-cutting axis (age), not a histology | no | seventeen, re-voiced to the reader; grade routed |
| **Umbrella** | no diagnosis at all — §12.11's pre-diagnosis catch-all | no | its own ladder, plus every section that is about the READER |

- **Secondary hub** (`brain-metastases`). WHO CNS5 grades tumors that START in
  the CNS, so a deposit from a lung or breast cancer has no CNS grade — the page
  says as much in its own front matter — and "is it cancer?" is already answered
  by the word *metastasis*. §2 therefore moves to "Why it is still called by the
  other cancer's name", and self-blame is re-voiced to "Did I let this happen?"
  because this reader had a previous cancer: the guilt is about a missed symptom,
  not about having caused a tumor.
- **Axis hub** (`pediatric-brain-tumor`). The reader's child has a specific
  diagnosis, which has its own page, so grade is routed. Every other question is
  owed, but re-voiced: the reader is a parent, so "my report" becomes "the
  report" and "it" becomes "your child".
- **Umbrella** (`all-brain-tumors`). The reader has not been given a name — that
  is what the class *means* — so every "what is *it*" question is unanswerable
  and the page owes the pre-diagnosis ladder plus routes.

**A new taxonomy entry is a Hub.** The three non-default classes are a closed set
pinned in `TumorHubTemplateSweepTests`. A new slug cannot acquire an exemption by
omission, and deliberately cannot declare one in its own front matter: acquiring
one means editing a test, which is the most-reviewed place to put it and where a
reason has to be written down. **A declaration that lives in content is a
declaration nobody reviews.**

**`spinal-cord-tumor` is not an exception, and the reason is worth keeping.**
`taxonomy.yml` is emphatic that it is not a brain tumor, which reads like the
strongest exemption case in the corpus — and it carries all seventeen sections
in order. **A ruling about FRAMING is not a ruling about TEMPLATE**, and
assuming otherwise would have bought an exemption for a page that never needed
one. Every candidate exception was measured before it was granted.

**The failure mode this item was built to avoid, and it surfaced anyway.**
An exception list whose only justification is "these pages go red" is a
restatement of the bug. The test of a class is whether its reason would bind a
page nobody has written. Applied honestly, that test **refused** one of the
three candidate exemptions: `/tumors/pediatric-brain-tumor` said *nothing
whatsoever* about the tumor coming back — no section, no sentence, not even a
route; `grep -i` over `comes back|come back|came back|grows back|relaps|recurr`
across all 1,012 lines returned a single hit, inside a front-matter comment
about an education plan. Recurrence is diagnosis-specific, so routing it would
have been defensible; silence was not. **The class was not widened to fit the
page — the page was written to fit the class.**

**Two floors, both derived by measurement rather than taste.**

- **A required section must carry a body, not just a heading** (120 composed
  characters). Across the 403 `##` sections the tumor corpus carried in source
  before this item, the smallest composed body is 203 characters and the next is
  207, so the floor has roughly 40% headroom and still catches a bare heading or
  a one-line stub.
- **Measure the COMPOSED page, not the source.** Five pages discharge "Did I
  cause this?" with the bare `[CAUSES]` directive: eight characters on disk, a
  full section to a reader. A raw-text floor would have failed correct pages —
  the §12.11 trap in a new costume.

**And the rule caught the test that enforces it — twice, in the same item.**
Two of the three defects this item's own review found were in the machinery, not
the corpus, and both were the exact failure the section above describes:

- **The umbrella silently dropped a question, inside the sweep built to make that
  impossible.** Six of §12.3's seventeen were covered by the umbrella's own
  template and ten were routed. Six plus ten is sixteen. The missing one was §2,
  "is it cancer / what grade", and it was in neither list — the state this section
  says may never exist, living in the test that asserts it. It had been visible
  for a whole round as a stale number: the comments said eleven routed questions
  and the array held ten. **A count that disagrees with its own prose is a defect
  report nobody has read yet.** The fix is a recorded MAPPING — the umbrella
  answers §2 in two halves, "why 'benign' is the wrong comfort word here" and
  "why nobody has given you a stage" — plus the arithmetic check that would have
  caught it: routed and answered must be disjoint and together equal all
  seventeen.
- **A canary that "derived" its floor from the walk it was checking.** Replacing a
  typed `> 300` with a derived count made the render sweep strictly WEAKER: both
  sides came from the same heading extractor, so if that extractor ever stopped
  finding headings, both went to zero and the assertion passed while the test's
  whole subject evaporated. **"Derive, never type" is not enough — derive from an
  INDEPENDENT source.** The floor now comes from the obligation table, which knows
  nothing about how headings are read off a file, and the fix was proved by
  stubbing the extractor and watching it go red.

**An exemption table needs a distinctness check, not just a membership check.**
Asserting that a mapped heading EXISTS is satisfied by any heading that exists,
so a question could be discharged by pointing at a heading already spent on a
different question — making the arithmetic work by pointing at something real,
which is the same move as an exception whose only reason is that the page fails.

**Hard-code the wording; assert the shape against the doc.** §12.3's table cells
are prose ("What is a [tumor]?"), not the literal headings, so parsing them into
matchers needs a mapping layer that becomes the thing to keep in step, with
failures nobody can read. The matchers are explicit, and a separate guard reads
§12.3 out of this file and goes red if its shape changes — its seventeen numbered
rows — **and if any of the twelve rows whose doc wording IS the heading is
reworded**. Five rows are deliberately unpoliced and the guard says which and
why: row 1 is parameterised ("What is a [tumor]?"), row 9 lists topics rather
than naming the heading, rows 12 and 14 word the question differently on purpose,
and row 16 is rendered from front matter and has no heading at all. Asserting a
match that was never intended is how a guard starts failing on a correct
document.

**And that guard walked straight into standing trap #1 on its first run.** It
searched this file for "The self-blame block still appears", which is certainly
present — and `docs/` is hard-wrapped, so the phrase spans a line break and a
substring search cannot see it. Same mechanism as WI-541's location pass, which
reported zero for eight fragments that were all there. **Prose read out of a
wrapped document must be flattened first**, and the positive control proving it
is now BUILT FROM THE FILE — a phrase this document's own wrapping splits — so
it cannot go red the day somebody reflows a paragraph.

**A sweep is also the cheapest moment to check whether the last item's defect
propagated, and to cite the pages counted in the absence.** WI-547 fixed one
shipped `Composed.Replace(RawSection(...), "")` — composition emits LF while the
raw section keeps the checkout's endings, so the test was red on every CRLF
checkout and green in CI. It does **not** recur: `Composed.Replace(` has exactly
two call sites in the suite, `CnsGermCellTumorPageTests.cs` and
`GliomaPageTests.cs`, and both are the correct `"\r\n"` → `"\n"` normalisation;
no line anywhere combines `Composed` with `RawSection` across 150 `RawSection`
uses in 14 files.

**A hand-typed floor in a sweep is a slack guard.** `CaregiverSectionTests`
asserted `>= 18` tumor hubs from the day it was written; the corpus reached 23
five items later and the guard never noticed, so the check against "the
enumeration quietly stopped finding pages" had five pages of slack in it. It now
reads the count out of `taxonomy.yml`, which is the list the sweep is really
about and cannot go stale the next time a type is added. **A sweep's own
not-vacuous canary must be derived, never typed.**

### 12.16 Editing a block that is already live (WI-568)

§12.10 is about **writing** a shared block. This is about **changing one that
eighteen pages already compose**, which is a different job: the words are not
arriving on a blank page, they are landing in eighteen contexts that were each
written against the block as it read yesterday.

**COUNT THE INCLUDERS BY GREP, AND COUNT THE RIGHT THING.** The backlog said
`[MECHANISM]` was live on "~20 hubs" in three places. It is **18**. A bare grep
for the word `MECHANISM` returns **22** files, and the four extras mention it
only in front-matter comments — none of them includes the block, and one is a
page that *refused* it in writing. An approximate blast radius is not a blast
radius: it is four pages you might edit that are not in scope, or two you might
skip that are. The number is now asserted (`MechanismBlockTests`), not written
down.

**THE PAGES THAT BREAK ARE NOT THE PAGES THAT NAME THE BLOCK, AND THAT IS THE
WHOLE LESSON.** This item searched the prose above all eighteen `[MECHANISM]`
directives and found **four** files whose text the edit falsified. `/review`
found two more, and they were missed because the search was shaped like the
first thing it found. There are **three shapes**, and a grep can only see the
first:

1. **The page NAMES AN ENTRY in the block.** *"the cerebellum entry is this
   growth's own territory"* — `/tumors/acoustic-neuroma`. Findable: the note
   quotes the block, so it greps.
2. **The page RESTATES THE BLOCK'S RULE in its own words.**
   `/tumors/all-brain-tumors` said *"Where a thing sits in the brain decides
   what you feel"* — the very claim the item exists because it is false — two
   lines above the directive, on the hub written for the **pre-diagnosis**
   reader, who is the reader most likely to have the presentation it is false
   for. It quotes nothing, so it greps as nothing.
3. **The page's own prose COLLIDES with the new words while referring to
   neither.** `/tumors/cns-lymphoma` says the tumor grows *"deep, near the fluid
   spaces in the middle of the brain"* and that the first changes are in
   thinking — fourteen lines above a new bullet that leads on pressure, because
   PCNSL is not an obstructive presentation. It looks like an ordinary
   paragraph. Only §12.10's own question finds it: **would this sentence be true
   on the hub you have thought about least?**, asked page by page rather than
   grep by grep.

**SEARCHING FOR ONE SHAPE OF COLLISION IS HOW THE WORST ONE GETS MISSED.**

**A CONTRADICTION ABOUT A TERM IS NOT DUPLICATION, AND ONLY ONE OF THEM CAN
WAIT.** The first draft taught *"Your team may call this the **skull base**"* on
the bullet about the spot below and behind the ear. `/tumors/meningioma` — a
page the block composes onto — already teaches the term thirteen lines above as
*"an umbrella word for the ones growing on the floor of the skull and the ridge
behind the eyes"*, and the block puts behind-the-eyes in a **different** bullet.
Two definitions of a word **the reader carries to their appointment**, on one
composed page. It was also unsourced. MSKCC settles it (*"the base or floor of
the cranium, the part of the skull on which the brain rests"*), the block now
teaches the term as the whole floor and scopes its bullet as one part of it, and
the remaining *duplication* — meningioma listing the same regions in its own
words — is handed to the item that owns that page. **Defer overlap; never defer
a contradiction.**

**A CHANGED BLOCK CHANGES THE SOURCE LIST ON EVERY INCLUDING PAGE.** §12.10
already says block `sources` merge and render. What this item adds is the cost:
*"NIDCD: Vestibular Schwannoma (Acoustic Neuroma) and Neurofibromatosis"* now
appears under Sources on `/tumors/dipg`, where a parent reading
"Neurofibromatosis" has a question nobody asked for. **The citation is not the
defect and must not be dropped** — NIDCD really is the source of that bullet's
wording, and removing a real citation to tidy a title trades honesty for
presentation. `/tumors/atrt` and `/tumors/pediatric-brain-tumor` had each already
recorded this; this block made it three files and **zero fixes**, so it is now
`WI-574` rather than a fourth comment. **Recording a defect for the third time is
not a mitigation.**

**A BLOCK'S PROSE HAS EIGHTEEN HOMES, SO THE OBVIOUS COMPOSED-PAGE TEST IS
WORTHLESS.** `Assert.Contains("skull base", composedMeningioma)` **passes with an
empty block**, because that page says the words itself; same for "pituitary" on
four of the eighteen. WI-549's rule — *a property with two homes cannot be broken
by a single-point mutation* — is the governing one, and the needles must be
phrases **only the block says**, verified by grep against every including page
before use, asserted in both directions (`Contains` on the block,
`DoesNotContain` on the page, exactly one occurrence composed). The region NAMES
are exactly the wrong needles.

**AND THE HARNESS FOUND WHAT FIVE REVIEW ROUNDS COULD NOT.** The Gate 1 ruling
was that the rule stays verbatim, and the guard for it sliced *the sentence
containing the rule*. The rule is **two sentences**. A mutation appending
*", unless the fluid is blocked."* to the second one — which is option (B) the
ruling refused in writing, the rule qualified in place — **survived on LF and on
CRLF**, through five rounds that all read the guard. **The correction for a
too-NARROW guard is a WIDER guard**: the paragraph, not the sentence, because the
paragraph is what the reader reads as "the rule". Review reads what the page
says; only a mutation shows what the guard can see.

### 12.17 A page for an axis the site does not have (WI-567)

§12.8 is the library-page template. This is about writing a page whose subject is
neither a test, a treatment, a wait, a document nor a reference list, on a site
organised by two axes when the reader arrives holding the third. And it is about
**eleven review rounds and twenty-four blockers**, most of which were found inside
the previous round's fix. The rounds are the lesson as much as the page is.

**THE RULING: DO NOT EXTEND THE BLOCK, ANSWER THE OTHER HALF OF ITS QUESTION.**
`[MECHANISM]` is live on eighteen hubs and owns **location to symptom** ("the
symptom tells you where, the scan tells you what"). `/where-your-tumor-is` owns
**the word on the report, to plain language, to what the team is weighing**. So a
region entry here teaches a WORD: the plain name, the report's word, and a route
where there is a sourced one. It never says what a tumor there does to the reader.
That is not tidiness — it is the structural reason the page cannot become the
location-to-risk table Wave 6 forbids. **An entry that teaches a word has nowhere
for a risk to go. An entry made of "name + what happens to you here" grows one,
one review round at a time.**

The door WI-568 deferred therefore runs **both ways**, and the page is better for
it: the block sends a reader here for what a place changes, and this page sends
them to the block's own hub for what a place explains.

**A CLOSED COUNT IS A CLAIM ABOUT EVERYTHING YOU DID NOT NAME, AND THIS ITEM MADE
IT FIVE TIMES — INCLUDING ONCE IN THE FIRST DRAFT OF THIS PARAGRAPH, WHICH SAID
THREE.** The escalation exception said *"One place has warnings this
general list does not cover"* — false, because `blocks/spinal-cord.md` exists for
that reason. Round 4 made it *"Two places"* — false the same way, and the case it
omitted was that section's own fluid reader, whom `/tumors/craniopharyngioma`
re-tiers by position. Round 6 found a third: *"the two growths here we have written
about"*, when `/tumors/meningioma` and `/tumors/cns-germ-cell-tumor` both name that
spot in their own reader text. Round 11 found a fourth and a fifth, both of them
inside the fixes for the first three: *"This is one of the **two places** on this
page with a warning of its own"*, in the entry that had just been opened up; and
*"it names **the places** whose warnings the general list does not cover"* in the
short version, plus *"the list above is yours **as it stands**"*, which is the same
claim as a completeness assertion rather than as a number. Two of the eighteen hubs
that link to the page escalate the shared list for a region that IS on it
(`/tumors/hemangioblastoma` for the cerebellum, `/tumors/cns-germ-cell-tumor` for
the deep middle), so every one of those five was false.

**Write "some", "includes", "two of the", and let the reader's own word be the one
you did not list.** It is a ban now — of the numbers AND of the completeness
assertions, because round 11 proved the number is not the only way to close a
set.

**A GUARD SCOPED TO WHERE THE DEFECT WAS FOUND IS GREEN WHERE IT ARRIVES.** This
was the single most repeated failure of the item, and it recurred *after* being
written down:

- a region-scoped ranking guard, while *"the location with the worst reputation of
  all"* sat in the surgery section;
- a section-scoped duration ban, while two more unsourced durations stood in the
  section above it;
- a body-scoped everything, while the front-matter `description` — which
  `ContentPage.cshtml` renders as the first paragraph a reader meets — carried a
  blocker for three rounds;
- a section-scoped tumor-board hedge, while the flat version of the same sentence
  sat 190 lines later, contradicting the block it linked to.

**Scope a guard to the property, not to the paragraph the defect was in.** And
bring the title and description into the page's own flattened text, because they
are reader-facing prose that ContentCheck does not grade and most pages' guards
cannot see.

**A BAN LIST OF THE WORDINGS YOU JUST DELETED IS NOT A REFUSAL.** Rounds 1 and 2
each banned the phrasing they had removed; the page shipped a third and a fourth
that matched neither. The page's own front matter says it out loud now: *a refusal
that only bans the words it was written against is not a refusal.* The fix is to
name the PROPERTY — here, that no exclusivity claim attaches to urgency — collect
the sentences that could carry it, assert a floor on how many were collected, and
allow the negated form, which is usually the correct sentence.

**AND A REFUSAL HAS TO BE RE-APPLIED TO ITS OWN PARAPHRASE.** Gate 1 refused the
backlog's *"small, slow and low-grade and still an emergency"*, because the one
source that addresses size says tumors block CSF *"when large enough"*. Round 1
found the same claim back with the size words removed — and found the test
**requiring** it as a canary, so the guard against the refusal depended on the
refusal being present. Round 2 found one surviving clause of it (*"or instead of it
for now"*). The claim was refused three times before it stayed refused.

**THE UNREACHABLE COMPARATOR: READ IT, THEN CITE IT NOWHERE.** `moffitt.org`
returns 403 on every path with three browser user-agents. The Wayback Machine
capture closed the item's one unverified hole — Moffitt's location page is
symptoms-only over six regions, so "no comparator answers *my tumor is here, so
what happens to me?*" is now **checked against an archived capture** rather than
assumed. That is enough for a competitive check and not enough for a citation, and
the page draws exactly that line: it is cited nowhere, because a source we cannot
open live is a source we cannot verify. **The 403s are recorded
in the front matter**, because an absence nobody wrote down gets re-investigated by
the next item.

**THREE TOOLING TRAPS, ALL OF WHICH PRODUCED A GREEN GUARD OVER A REAL DEFECT.**

- **A WI-105 authoring marker broken across a LINE WRAP is inert and prints its own
  characters**, and **the cause is not the regex**. `GlossaryTooltips.SuppressMarker`
  is `!%(.+?)%`, so the obvious diagnosis is that `.` does not match a newline — and
  a `RegexOptions.Singleline` "fix" would change nothing. What actually happens is
  that `CollectAndStripSuppressions` walks `LiteralInline`s, Markdig has already
  split the wrapped paragraph at the soft break, and that pass runs **before**
  `MergeSoftBreakRuns` puts it back together. The marker never exists as one string
  for the pattern to see. So it suppresses nothing, renders `!%frontal lobe%` to the
  reader, and builds green. Now guarded corpus-wide in `ShippedGlossaryTests`,
  together with its sibling — an **unterminated** marker, which fails the same way by
  a third route. **Write the cause down only after reading the code that produces
  it**; an outcome correctly observed and wrongly explained is how the next person
  fixes the wrong thing.
- **`CuratedPage.ReaderText` cannot be used on a FRAGMENT.** It calls `Body`, which
  looks for the end of the front matter, finds nothing, gets `-1` and slices from
  index 3. Four call sites were reading every section and every region entry minus
  its first three characters — including the guard for this item's central ruling.
  The property those guards exist to check is §12.6's "answer in the first sentence
  under the heading", which is the one thing that reading path cannot see. Use a
  fragment-safe helper.
- **On a CRLF checkout, `$` after a literal `}` matches nothing.** The working tree
  is CRLF under `text=auto`; a pattern anchored that way found **zero** region
  entries, which made one guard fail loudly and another pass vacuously. Normalise
  once, in one place, and have every pattern read that.

**AND THE RULE THAT CATCHES ALL THREE: AN ITERATE-AND-CHECK GUARD MUST SAY OUT LOUD
HOW MUCH IT LOOKED AT.** `Assert.Equal(9, entries.Count)`, `preamble.Length > 400`,
`urgentWindows.Count >= 4`, `checkedFiles > 100`. Every one of those floors was
added after the guard had been green over nothing.

**THE PART OF A SECTION NO GUARD READS IS THE PART BETWEEN THE HEADING AND THE
FIRST SUB-HEADING.** The region guard matched `### … {#anchor}` blocks, so four
paragraphs of preamble were covered by a length check — and that is exactly where
round 6's fix for *the reader whose report word is none of the nine* landed. That
reader is why Wave 6 exists. Deleting the paragraph was green.

**A BAN LIST THAT FORBIDS THE CORRECT SHAPE IS WORSE THAN NO BAN LIST — AND A LINK
LABEL CAN STILL CARRY A CLAIM.** Three guards in this item fired on legitimate
prose: `"urgent"` FIRED, on the link label *"[When where it sits makes it urgent:
the fluid]"*, which is a route and therefore the shape the ruling asks for;
`"one in "` fired too, on *"the two lateral ventricles, one in each half of the
brain"*, and was dropped from the list; and `"small"` was word-bounded
**pre-emptively**, against *"smaller channels"*, a sentence the section does not
carry and might. The distinction is worth keeping straight: two of those were
caught by the suite, and the third was caught by asking §12.8's question of a list
before shipping it.

The first of those was fixed twice, and the second fix is the right one. Stripping
whole links made the guard blind to a claim written INTO a label
(*"[a tumor here is reached through the nose](/x)"*), so the label stays in scope
and the over-broad word comes off the list instead. **Strip the URL, keep the
label.**

**With one recorded exception, because two of this item's three guards do the
opposite deliberately.** Where a guard's banned vocabulary is words a route's label
legitimately uses — *"urgent"* in the exclusivity scan, *"tissue"* in the tissue
hedge — the whole link is stripped, because keeping the label would forbid the
correct shape and no claim can hide in a label made of a destination's own title.
The rule is therefore: **keep the label wherever a claim could be written into
it, strip the whole link where the ban words are the destination's own.** State
which you did and why, next to the guard.

And record every rejected phrase WITH the correct sentence that rejected it, so the
next item does not re-reach the wrong answer.

**AND WHAT THE HARNESS FOUND THAT TWELVE REVIEW ROUNDS COULD NOT.** Two things, and
both were invisible to reading:

- **A ban list that was removed from the page but never added to the guard.** Round 12
  deleted an unsourced likelihood claim from a region entry (*"A tube is an easy thing
  to block"*) and added the matching entries to the guard's ban list **in the same
  script** — a script that aborted on an earlier assertion and wrote nothing. The
  sentence was gone; the ban was not. Two further review rounds read that list and saw
  a list that looked complete. The harness put the sentence back and the guard let it
  through, on LF and on CRLF. **A fix and its guard written in one script share that
  script's failure mode**; apply them separately, or assert the guard afterwards.
- **A SINGLE UNREADABLE SENTENCE CANNOT BE CAUGHT BY A WHOLE-PAGE AVERAGE**, and this
  is now measured rather than suspected. A planted 36-word sentence
  (*"the heterogeneity of intraoperative eloquence determinations consequently
  precludes reproducible preoperative stratification of anatomically defined
  neurosurgical risk"*) moved this page from grade **5.6 to 5.7** against a 6.0 gate,
  and ContentCheck passed 283/0. It is also outside `dotnet test` entirely, because
  ContentCheck is a separate tool. So the reading-level gate is a page-average gate by
  construction, and on a site whose audience may be cognitively impaired, one sentence
  is enough to lose a reader. It is recorded here as a **known survivor with its
  reason** in the harness rather than deleted from it, because a mutation aimed at
  nothing is a permanent red that teaches nothing after the first run, and deleting it
  would hide the finding. **For `/pm`:** a per-sentence or per-paragraph grade check
  would need a corpus sweep before it could be gated, because it will not be this page
  alone.

**WHAT REVIEW FOUND THAT NOTHING ELSE COULD.** The suite, ContentCheck and the
8-gram restatement probe were green at every one of the eleven hand-offs. The probe
is structurally blind to three things this item kept producing: a sentence restated
in **different words** (five hits against `/treatments/craniotomy` alone, each one
substitution apart); anything inside `## Where to go next` and `## What to ask your
team`, which it strips **by literal heading** — so a page that re-heads slot 10, as
this one does with *"What to ask your surgeon"*, loses the exemption and gains the
comparison, which is a thing to know before a shared question trips it; and **the
page restating itself**, because it skips the page under test, so the same twenty
words in slot 4 and slot 9 were invisible.

**Carried forward, for `/pm`:** `/treatments/craniotomy` states the tissue rule
flat (*"Only a piece of the tumor itself, looked at in a lab, can give it a
name"*), which this page had to hedge because the exception is keyed to location —
the hedge is a corpus property, not a page property. `/tests/biopsy` says there is
*"one exception worth knowing"* where the corpus now asserts two. Those two are
recorded here and belong to whichever item next touches those pages.

**TAKEN BY WI-577, and the measurement moved the scope of the first one** — §12.32.
Both findings were correct. What this section could not see is that the rule has
**two forms** wearing nearly the same words: a *certainty* form that is true with no
exception, and a *naming* form that the two exceptions falsify. Five pages carry one
or the other, and only the naming form needed the hedge. `/treatments/craniotomy` is
fixed; `/tumors/low-grade-glioma` and `/tests/planning-scans` are the other two
naming-form instances and are recorded in §12.32 for `/pm`, because the third sits
inside a deliberate three-page `AssertDoesNotRestateTheCorpus` allowlist and hedging
one of three would re-create the two-strengths defect that allowlist prevents.

**Two others became work items rather than a fourth recording**, because that is
the threshold WI-574's own entry sets. **WI-576**: `AssertDoesNotRestateTheCorpus`
walks `pages/` and `blocks/` and not `glossary/`, so a tooltip and a page can drift
apart unseen. **WI-575**: the front-matter `description` is reader-facing prose that
ContentCheck does not grade and most pages' guards cannot see — WI-524 and WI-528
each hit it before this item, and here it cost a `/review` blocker three rounds of
survival inside one line nothing was reading.

### 12.18 Where a tumor TYPE page carries location (WI-569)

§12.17 settled what a page *about* location owns. This is the other half: what a
page about a **type** may say when the reader asks where theirs sits — and it is
the ruling WI-570 copies onto nine more hubs, so it is written as a rule and not as
a description of one page.

**THE SPLIT: LOCATION MATERIAL ON A TYPE PAGE GOES THREE WAYS AND EACH WAY HAS
EXACTLY ONE HOME.**

1. **What the place EXPLAINS (the symptom)** belongs to `[MECHANISM]`, which
   already composes on eighteen hubs. The type page's address entry adds **only
   the give-away this type has that the block cannot say**. Where the block
   already says it, the entry says nothing and the page keeps one copy.
2. **What the REPORT'S WORD means (the vocabulary)** belongs to the type page,
   because meningioma reports name the address — *convexity*, *sphenoid wing*,
   *tuberculum sellae* — and `/where-your-tumor-is` carries only nine general
   regions. Plain words first, the report's word second, which is §12.17's shape.
3. **What the place CHANGES (the plan)** goes in one sub-heading, keyed on **the
   factors the type's own guideline names** and never on the addresses. A place
   appears only where the source names it, and only as a change in **what the team
   aims at** — never as a change in what happens to the reader.

**AND THE LINE THAT MAKES THE FORBIDDEN TABLE STRUCTURALLY IMPOSSIBLE, ONE NOTCH
OVER FROM §12.17's.** That section could say a region entry has nowhere for a risk
to go because it teaches only a word. A type page cannot borrow that, because its
entries legitimately carry symptoms — the block it composes does, on the same page.
So:

> **An address entry may carry a SYMPTOM. It must never carry a DIFFICULTY.**

A symptom is a fact about what the reader notices and it belongs to them. A
difficulty is a fact about an operation, and **the moment one address carries one,
every other address needs one for the list to look finished.** That is the risk
column growing, one review round at a time.

**THE SHAPE DOES NOT HAVE TO BE INVENTED. IT ARRIVES WITH THE SOURCE.** Mayfield's
meningioma page — already cited on `/tumors/meningioma` — closes with *"Convexity,
parasagittal, and sphenoid wing meningiomas usually are completely removable…
Optic, cavernous sinus, and skull base meningiomas have a higher rate of
complication and are more difficult to completely remove."* The page carried a
compressed version of that sentence for three items before Wave 6 gave anyone the
words for what was wrong with it. **Check the closing paragraph of every patient-
education comparator for this, because it is where they all put it.**

**FOUR SENTENCES ON A LIVE PAGE, AND THE ITEM WAS NOT LOOKING FOR THEM.** The
backlog asked for a new section. What the item found first was that the page
already keyed an outcome or a difficulty to a place four times — *"harder to take
out completely, and they have more complications when they are"*, *"A small
meningioma against the nerve to your eye **will** take your sight"*, *"A grade 1
meningioma **in a bad place**"*, and the short version's softer copy of the eye
sentence. Three of those four were outside the section the item was commissioned
to write. **The sweep comes before the section.**

**WHAT SURVIVES THE BAN, AND WHY IT IS NOT A LOOPHOLE.** *"Where it grows around a
nerve or a large vein, it can be genuinely difficult to remove"* stays, because a
difficulty attached to a **structure** is EANO's *"proximity or involvement of
critical neurovascular structures"* — the FACTOR. A factor gives a reader nothing
to look their own address up in. An address gives them a row. **The ban is on the
row.**

**THE GUARD FOR IT HAS TO BE A PROPERTY, AND IT TOOK A VERSION PER REVIEW ROUND —
TEN OF THEM.** Every version was green on the shipped page, and every one before
the last was green on the defect too. The narrative below is keyed to ROUNDS
rather than to version numbers, because an earlier draft numbered the versions in
this section and in the test, the two drifted apart, and a reader had two
different accounts of what "version five" contained. Round 5's is the last one
named below; rounds 6 to 10 are narrated in the paragraphs that follow.

- **Version one** was a list of the wordings the item had just deleted, which
  §12.17 already names as not-a-refusal — and two sentences of the banned property
  were live on the page while it passed.
- **Round 2's fix** allowed any hit whose preceding 250 characters mentioned a nerve
  or a vein. The reasoning was right and the implementation was **proximity**, so
  appending *"Tumors on that floor are harder to remove completely"* to a paragraph
  about cranial nerves would have passed with the whole suite green.
- **Round 3's** banned a sentence carrying the vocabulary where it or the
  sentence before it names an address. Its vocabulary was called
  "difficulty-or-outcome" and **contained no outcome word at all** — no *risk*,
  *riskier*, *dangerous*, *safer*, *worse outlook*, *survival*, *prognosis*, *do
  better*. `/review` round 3 wrote eight sentences of the banned property and all
  eight passed, including *"A meningioma on the skull base has a worse outlook"*.
  Its structure allowance also ran on the SENTENCE while the address ran on the
  two-sentence WINDOW, so a row split across a full stop walked through it — and
  the allowance fired on **zero** shipped sentences, because both legitimate ones
  name no address in the first place. A door held open for nobody.
- **Round 4's** deleted the allowance, evaluates one property, and carries the
  outcome lexicon. It catches the eight attack sentences round 3 wrote and fires on
  none of the page's own — **and round 4 then wrote five more that passed.** Three
  of those five got through because the address lexicon carried only the REPORT
  words (*sphenoid*, *petroclival*) while the page leads with the plain ones
  (*near the brainstem*, *near the pituitary*); the other two were near-neighbours
  of entries already on the list — *hard to reach* beside *harder to*, *the easy
  ones* beside *easier to*. **Round 5's** added the plain words and the
  neighbours, and widened the window to FOUR sentences, because round 4 also
  split a row across four using this page's own convexity bullet as the
  carrier — an address entry's address is in its bolded lead and a difficulty
  can be three sentences later in the same bullet.

**And "back to the start of the bullet" was tried first and was worse than a
fixed window.** The text under test is flattened, so nothing marks where a
bullet ENDS — a bullet-start pointer that never resets gives every later
sentence on the page a window reaching back to the last address entry. It fired
nineteen times on prose it had no business reading.

**AND A SPLIT ROW HAS TO CARRY A BACK-REFERENCE, which is what makes a
multi-sentence window safe at all.** Widening to four AND widening the lexicon
produced, together, a false positive on the page's own *"Where the whole tumor
cannot safely come out, the guideline's advice is to plan the smaller
operation"* — a general principle standing three sentences after a paragraph
that mentions the floor of the skull. **Proximity cannot tell a split row from
two unrelated sentences. An anaphor can.** *"Those are harder to take out
completely"* is a row; a sentence with no back-reference is not talking about the
address three sentences up. So the rule is: the whole row in one sentence, OR the
vocabulary plus a back-reference with the address inside four.

**But the lesson is not "do not widen the window", and the measurement says so.**
On this page two examined sentences sit **three** and **four** sentences after an
address sentence, both of them inside the new subsection, so a four-sentence
window reaches an address for the nearer of them. Neither fires, and the reason
is the
back-reference gate rather than distance. **The window was never the binding
constraint; the lexicon was, every single time — but the headroom is three, not
nine, so a wider window without the gate would need an allowance written back.**
The first version of this paragraph said ten, which was measured against the
wrong text and was caught by the round after the one that wrote it. Measure the
headroom; then measure it again against what shipped.

**AND ROUND 5 THEN WROTE SEVENTEEN AND SIXTEEN PASSED — AND ROUND 6 WROTE TEN
MORE AND SIX OF THOSE PASSED.** Six on compartment and
lobe names the page does not itself use (*over the temporal lobe*, *on the
tentorium*, *at the cerebellopontine angle*), eight on harm and outcome verbs no
version had ever carried (*a poor outcome*, *live longer*, *leaves more behind*,
*permanent damage*, *impossible to remove*), and **two that no property guard can
reach at all** — the pair this item was commissioned to delete. *"A small
meningioma against the nerve to your eye will take your sight"* keys an outcome to
a STRUCTURE, which the ban must allow; *"A larger one on the top of your head may
cause nothing for years"* carries no loaded word, because it is the harmless half
of a contrast whose other half did the damage.

**So the four deleted sentences are pinned by name, BEHIND the property and not
instead of it.** §12.17's objection is to a ban list being the whole guard. One
sitting behind a working property is a belt, and for a pair no property can see it
is the only thing there is.

**AND THE THING THAT ENDS THE CYCLE IS A POSITIVE CONTROL.** Seven rounds, nine
lexicons, **one hundred planted sentences** (8, 5, 17, 10, 16, 24, 20), and every
version was verified by a human writing attacks by hand. The guard now runs its scan as a
local function over `Plain` AND over `Plain + planted`, and asserts the second one
FINDS something — thirteen planted controls, each one a shape an earlier version
let through, including a three-sentence split and a bolded bullet lead. **A
property guard that has never been seen to fail has not been shown to work.** That
is the sentence WI-570 should copy before it copies any lexicon.

**AND THE LAST HOLE WAS THE PAGE'S OWN FORMAT.** The back-reference gate listed
*those*, *these*, *the ones*, *they*, *them* — and no **deictics**. `SentencesOf`
splits on sentence ends, so every bolded bullet lead is its own sentence and what
follows it refers back with *here*, *there*, *it* or *one*. Six of round 6's ten
attacks walked through on the format the page is written in, which is the format
WI-570 copies onto nine more hubs. **Attack a guard in the shape of the text it
guards, not in the shape of English you would write by hand.**

**Three things generalise.** A guard's vocabulary has to be attacked, not read —
write the banned sentences and run them. An exception that fires on nothing is not
protecting anything, it is only holding a door open. And when two guards test one
property in two files, **the newer one is not automatically the stronger one**: the
lexicon this item needed already existed in `WhereYourTumorIsPageTests`, and WI-570
is where it gets promoted, because that item needs it on nine hubs at once.

**And one word had to be QUALIFIED rather than banned.** A bare `\bworse\b` fires
on *"back pain that is typically worse at night"*, which is a symptom and not a
rank. The ban is on *worse outlook*, *worse than*, *worse for*. §12.17: a ban list
that forbids the correct shape is worse than no ban list.

**A GIVE-AWAY THE BLOCK CARRIES FOR A DIFFERENT ADDRESS IS ONE THE BLOCK CANNOT
SAY FOR THIS ONE.** The item stripped the sphenoid-wing entry's *"double vision,
numbness in the face"* on the reasoning that the block covered them — and it does,
under **brainstem** and under **the floor of the skull**, neither of which is where
that reader would look. Worse: the entry opened *"Behind the eyes, on the wing of
bone there"*, and the block's **pituitary** bullet opens *"Behind the eyes, at the
base of the brain"*. The routing sentence the item had just added therefore sent a
sphenoid-wing reader to a paragraph about hormones, periods and puberty. **Before
routing a reader into a shared list, match the words they were given against the
words that list uses**, and check the whole composed page rather than the entry.

**A ROUTING PROMISE IS A RULE, AND A RULE HAS TO BE TRUE OF EVERY ENTRY THAT
FOLLOWS IT.** *"Each entry here names what a meningioma at that address tends to
give away"* was false for two of eight. That is WI-568's failure shape — a rule
that is wrong for the reader who follows it — reproduced by the item that cites it.
Write the upper bound: *each entry adds only what the shared list cannot say
about this type at that address*. A conditional (*where an address has a
give-away of its own, the entry names it*) was tried first and was still false,
because the page's own later prose named a give-away the entry did not carry. An
upper bound cannot be falsified by a sentence somewhere else on the page.

**AND CHECK THE RULE AGAINST THE SHARED LIST'S OWN SCOPE.** `[MECHANISM]`'s nine
regions are all inside the head, so "what a tumor in each part of the brain tends
to do is below" is silently false for the spinal entry. The page says so in the
entry. Likewise *"each part of the brain"* closes a set of nine that has no
sphenoid wing, no falx and no convexity — a closed count with no number in it,
which is §12.17's fifth failure in a new costume.

**§12.10 CUTS BOTH WAYS AND THE NEWER PAGE MAY BE THE OWNER.**
`/where-your-tumor-is` shipped a day before this item and already carried the EANO
four-factor sentence in anonymised form (*"A guideline for one common tumor type
spells out what that planning weighs"*) and already carried *"must not cost you how
you think or how your body works"*. The type page therefore **routes** for the
factors rather than restating them — and the route is worth more than the
restatement was, because it tells the reader that the anonymous guideline is
theirs. Recorded because the instinct runs the other way: the claim is *about*
meningioma, so the meningioma page feels like its home.

**THE OTHER RESTATEMENT IS THE PAGE AND ITSELF, AND NOTHING CAN SEE IT.** The
8-gram probe skips the page under test (§12.17). This item wrote *"[Spinal cord
tumors](/tumors/spinal-cord-tumor) is the page for that half"* into a new paragraph
while the same sentence sat thirty lines below it, and wrote *"what is left is then
either watched or treated with radiation"* while the treatment section said it
already. Both were found by reading, twice, by two different rounds. **Sweep the
page against itself by hand, at a shorter window than eight.**

**A LINK LABEL SHARED BY TWO PAGES POINTING AT THE SAME DESTINATION IS A CORPUS
CONVENTION, NOT A RESTATEMENT.** The first version of this rule said "a label that
is the destination's own title", and `/review` round 5 checked all three and found
it true of one: `/treatments/craniotomy` is titled *"Brain surgery (craniotomy)…"*
and the anchor's heading is *"How much did you get out?"*, so the shared label is
neither. **Check a rule against every case it licenses, not against the one that
suggested it.** Three
turned up in one item — `/where-your-tumor-is`, `/treatments/craniotomy#how-much-
came-out`, `/where-your-tumor-is#the-fluid` — each shared with a page that links to
the same place with the same words. Exempt the label and reword the words *after*
it; making the label deliberately different costs the reader a recognisable link
and buys nothing. §12.17's "strip the URL, keep the label" is what makes these
visible at all, and that is the right trade: a claim can be written into a label.

**AND TWO WAYS A GUARD READ NOTHING.** `CuratedPage.Section` **flattens** its
return value, so `(?m)^- \*\*` found zero entries and asserted nothing about a list
of eight. `CountWord` has **no word boundaries**, so `"often spot"` matched as
`"ten"` + `" spot"` and the guard fired on a sentence about caregivers. Both were
found by running the suite once — which is the argument for a floor on every
iterate-and-check guard, and for `Assert.True(count >= n)` rather than a comment
saying how many there should be.

**A COUNT BAN THAT HAS TO BE SILENCED SIX TIMES IS NOT A BAN.** Scanning the whole
page for `both|neither|those two|the two` fires on *"the two halves of the brain"*,
*"If those two words are on your report"*, *"Those two things get mixed up"* and
three more — all correct English about things that are not places. The page-wide
scan is kept for the shape that names a place (`<count> places/addresses/spots`)
and the bare-demonstrative scan is scoped to the location material, **with the
reason written next to it**, because §12.17 is explicit that a ban list which
forbids the correct shape is worse than no ban list.

**A SECOND DOOR TO A PAGE THAT ALREADY HAS ONE CAN BREAK THE FIRST DOOR'S
GUARD.** `/review` round 3 asked for a `/treatments/stereotactic-radiosurgery`
link beside the new fractionation sentence — reasonable, and it turned
`StereotacticRadiosurgeryPageRenderTests.TheDoorsOnTheSiblingPagesAreAppended
SentencesNearWhatTheyWereAppendedTo` red. That guard (WI-526, WI-530) finds the
FIRST occurrence of the door on the sibling page and asserts it sits near the
sentence it was appended to, which is how it proves a door was added rather than
a paragraph replaced. A new, EARLIER link makes the first occurrence the wrong
one. The link was dropped rather than the guard loosened: **a corpus guard that
is inconvenient for one page's nit is not the thing that should move.**

**THE FIX THAT WAS APPROVED, ASSERTED AND NEVER WRITTEN.** §12.17 records that a
fix and its guard written in one script share that script's failure mode. This item
found the next layer: **round 6's edit script printed `anchor 1: 1 hit ok` and then
aborted on anchor 2, so NOTHING was written** — and the follow-up script, written to
apply the remaining anchors, silently dropped the first one. Two rounds of review
then read a page that still carried the sentence round 6 had deleted, because the
deletion existed only in a review report and a script's stdout. **A dry run that
reports `ok` is a report about the ANCHOR, not about the file.** Re-grep the file for
what you deleted, or assert the absence in the suite; and when a multi-edit script
aborts, re-run the WHOLE script rather than writing a new one for what is left.

**AND THE SIXTEEN SENTENCES A DOCUMENTED EXCLUSION LET THROUGH.** The guard excluded
bare *risk*, *damage* and *harm* as too noisy, and wrote the exclusion down, which is
better than leaving it silent. It was still too wide: *carries a higher risk*, *the
risk is greater for*, *does more damage* and a whole family of comparatives nobody
had listed (*kinder*, *gentler*, *a smaller operation than*, *less of a job*,
*simpler*, *a longer recovery*) all walked through. **§12.18's own rule about
*worse* — qualify, do not ban — is the rule, and it had not been applied to the
words the exclusion note named.** Qualifying them cost zero false positives, measured.

**AND THE RESIDUAL, MEASURED RATHER THAN HOPED AT — AND IT DOES NOT CLOSE.** One
shape is STRUCTURAL: *"Where it sits sets the ceiling on what surgery can achieve"*
carries the vocabulary and names no address. Widening the address lexicon with
*where it sits | the address | the spot* was tried and **costs three false
positives, one of them the page's own refusal** (*"Nobody can read your outcome off
the address"*), so it is written down rather than taken.

**The LEXICAL residual is the bigger one, and it is not closed.** Round 9 wrote
twenty fresh sentences and **eighteen passed** — almost all of them one word from
an entry already in the list (*bigger chance* beside *higher chance*, *bigger job*
beside *bigger operation*, *more than one operation* beside *second operation*,
*sets a limit* beside *ceiling*), plus four address words nobody had listed (*optic
chiasm*, *sagittal sinus*, *internal auditory canal*, *pineal region*). Ten cost
nothing to add and were added. **The next twenty will find twenty more.**

**So the lesson for WI-570 is which half is load-bearing.** The positive controls
and the pinned deletions are; **the lexicon is a floor, not a fence.** Two sentences
carried no loaded word at all — *"the surgeon can usually only get part of it"*,
*"a partial removal is the rule on the floor of the skull"* — which is Mayfield's
ranking written in plain English, and no word list reaches that by induction.

**AND TWO CROSS-PAGE MEASUREMENTS, because WI-570 ports this lexicon to nine hubs.**
Run unchanged, the guard fires **zero** times on `/treatments/craniotomy` — where
*harder* and *easier* each occur twice and survive only because no address word
shares their sentence — and **once, FALSELY, on `/where-your-tumor-is`**, on
*"When it is somewhere that cannot be taken out, such as the brain stem"*, which is
correct ACS-sourced prose. **Port the lexicon, and re-measure it per page before
trusting it.**

**And round 10 attacked version ten with twenty-five more: twenty-four passed.**
That is not a regression — it is the same finding a seventh time, and it is why
the paragraph above says the lexicon is a floor. **Stop adding words when the
positive controls and the pinned deletions are in place; the next round will
always find twenty more.**

**Round 8's other measurement: bare `harder` and `easier` subsume TWO of the
qualified entries above them** (`harder to`, `easier to`) and cost nothing; `hard
to remove`, `difficult to remove` and `more difficult` are different words and
survive on their own. The redundant pair is kept deliberately, so the diff reads.
The in-sentence address requirement was doing the protecting all along, not the
qualification — worth knowing before the next item spends a round qualifying a word
it could have banned.

**AND THE PLANNED-SUBTOTAL PRINCIPLE NOW HAS THREE WORDINGS ON THREE PAGES THAT
LINK EACH OTHER — AND THIS ITEM NEARLY WROTE A FOURTH INSIDE THE THIRD.** Its own
craniotomy fix first said "sometimes settled before the day" AND, two sentences
later, "a smaller operation is planned from the start" — one idea, twice, four
lines apart, on the page §12.18 names as the owner, caught by a hand read at round
10 and by nothing else. `/treatments/craniotomy` owns it (*"a subtotal resection is not
a failed operation"*), `/where-your-tumor-is` says it generically (*"aiming to get
all of it must not cost you how you think or how your body works"*), and this item
added the meningioma-specific third (*"Taking out less, on purpose, can be the
plan rather than a disappointment"*). All three route to the same craniotomy
anchor with the same label. No shingle check can see a paraphrase, so this is
recorded rather than detected: **WI-570 should route, not write a fourth.**

**READ TOGETHER BY WI-577, AND THE COUNT WAS THREE ON THREE WHERE IT IS SIX ON SIX**
— §12.32. The three named here turned out to be **already right**: all three route to
the same anchor, and — the half this paragraph did not check — **all three agree on
strength**, so §12.10 is satisfied and the split is recorded rather than collapsed
(collapsing it would delete the EANO quote from a page EANO is cited on). The three
it missed do **not** route: `/tumors/craniopharyngioma`, `/tumors/acoustic-neuroma`
and `glossary/subtotal-resection.md`. The glossary one's tooltip is **suppressed** on
the owner page (`!%subtotal resection%` is WI-105's suppression marker and a render
test already asserts the popover is absent), so all three are the same plain routing
question and none of them is §12.26's — see §12.32. For `/pm`.

**CARRIED FORWARD.** `/tumors/meningioma` says weakness in a **leg** is the
parasagittal give-away and no reachable source read for this item carries the limb
— Mayfield's falx entry names weakness with no limb, EANO says nothing, StatPearls
served a reCAPTCHA. The claim predates WI-569 and is **left as found and written
down** rather than deleted on a hunch or dressed in an unsourced explanation; this
item's own `/review` round 1 added exactly such an explanation and round 2 removed
it. Re-ask, do not inherit.

### 12.19 The location obligation across the corpus (WI-570)

§12.18 settled how ONE tumor-type page carries location. This is that ruling applied
to all twenty-three, and the item it names is the one that changed shape when the
sweep ran first.

**THE ITEM WAS COMMISSIONED TO WRITE NINE SECTIONS AND WROTE NONE.** The backlog
scoped WI-570 as "apply WI-569's pattern to the rest" — the glioma family,
ependymoma, CNS lymphoma, brain metastases. Running §12.18's property over all
twenty-three hubs before writing a word found that **all nine were already carrying
their own location prose**, with the address claim in their own words, and that what
was actually missing was somewhere else entirely. §12.18 says the sweep comes before
the section. It does, and sometimes it replaces it.

**WHAT THE SWEEP FOUND: ONE DEFECT, AND IT WAS NOT IN A LOCATION SECTION.**
`/tumors/ependymoma`'s outlook gate read *"Tumors in the spinal cord in adults tend
to do better than tumors at the back of the brain in young children."* Two addresses
ranked against each other by outcome — the Wave 6 preamble forbids it twice over,
since it is also a prognosis claim keyed to a place — and it was not even a clean
one, because AGE sat inside the same comparison as a second variable, so neither half
could be read against the other. Deleted. Nothing was lost: the sentence two lines
above already keys the same material to this tumor's own factors (how much came out,
how young the child is, the gene result), which is the form §12.18 asks for.

**WHAT THE SWEEP DID NOT FIND, AND AN AUDIT DID: FIVE HUBS WITH NO ROUTE AT ALL.**
The route from a type page to `/where-your-tumor-is` lives in the closing paragraph
of `blocks/mechanism.md`. Eighteen hubs compose that block. The other five —
`low-grade-glioma`, `atrt`, `chordoma`, `pituitary-tumor`, `spinal-cord-tumor` — had
no route to the location page anywhere. **That is §12.10's blast radius running the
other way, and it is the more dangerous direction: a block is a silent OMISSION on
every page that does not include it, because no single page is individually wrong
and no page-scoped test can see a gap.** The five are not an arbitrary set; they are
exactly the complement of the include list, which is what makes the rule assertable.

> **An obligation whose only home is a shared block is dropped on every page that
> does not compose it. Assert the complement, not the includers.**

`LocationObligationSweepTests` is that assertion: a closed per-hub table saying which
of the two each hub got, failing on a page with no entry rather than skipping it, and
asserting the include and the ruling against each other in both directions.

#### The row test, which is what nine hubs needed and one did not

§12.18's line — *an address entry may carry a SYMPTOM and must never carry a
DIFFICULTY* — was written for a page whose address list is eight bullets. Across
twenty-three hubs it is not sufficient on its own, because the corpus is full of
sentences that name a place and an operation in the same breath and are correct.
So:

> **Could a reader read their OWN address off this sentence and get a different
> answer from a reader with a different one? If yes it is a row. If no — if it is
> true of the type wherever it sits, or keyed to a factor, an age, a gene or a
> structure — it is the type's own fact and it belongs on the type's page.**

That is what tells `/tumors/cns-lymphoma`'s *"those two facts are the whole reason
surgery is not the treatment"* — true of every reader of that page — apart from
Mayfield's closing paragraph, which hands a different answer to each address. It is
the same instrument §12.18 used to keep *"where it grows around a nerve or a large
vein"*: a factor gives a reader nothing to look themselves up in, and **the ban is on
the row.**

**AND `/tumors/chordoma` IS THE LICENSED EXCEPTION, NAMED RATHER THAN INFERRED.**
§12.18 part 3 says the plan passage is keyed on the factors the guideline names *"and
never on the addresses"*. Chordoma's is keyed on two addresses, in two sub-headings —
the skull base, and the spine and sacrum — and it is right, because **its source's own
scope is the site**: the Chordoma Foundation writes the en-bloc goal for the mobile
spine and the sacrum, and the expert consensus group states that at the skull base
removal may have to be piecemeal. The page splits them on purpose and says why
(*"this is the part most likely to be flattened into one rule"*), attaches a mechanism
to each, labels the harder half *"and that is not a failure"*, and — the part that
settles it — **neither half is better than the other**: the spine half carries the
harsher consequence. The rule, then, is not "never key on an address"; it is that an
address may key **what is aimed at** only where the source itself is scoped by site,
and never **how the reader does**.

**THE RESIDUAL ON THAT PAGE, MEASURED AND LEFT.** Three sentences later chordoma says
*"How high up the tumor sits decides how much of that function is at risk, and your
team can usually tell you beforehand."* By the row test a reader **can** read their
own address off that, and the answer is about what happens to them. No property guard
reaches it — *how high up* is not an address token and bare *at risk* is deliberately
not a difficulty token (see the exclusion note). It is kept: it is sourced, it is what
a reader needs before an irreversible operation, and it points at a conversation
rather than at an outcome. It is written down here because the claim "the sweep found
one defect" is true of a lexicon, not of the corpus, and §12.18's rule is that the
lexicon is a floor.

#### What changed in the guard, and what the promotion cost

§12.18 closes by saying WI-570 is where the guard gets promoted. It is now
`CuratedPage.AssertNoPlaceIsRankedAgainstAnother`, run over all twenty-three hubs
plus `/where-your-tumor-is` and `/treatments/craniotomy`; `/tumors/meningioma`'s own
test calls it instead of carrying a second copy of the lexicon.

**The lexicon changes — eleven address tokens, nine difficulty tokens and three
qualifications — each measured over the whole corpus before it was taken**, which is
the only way §12.18 permits one. They arrived over four review rounds rather than in
one edit, and the order is part of the record:

- **`the` OR `your`, on WI-569's own skull and head tokens.** This corpus writes in the
  second person. `base of the skull` could not see *"the base of your skull"*, and
  that one word was hiding `/tumors/chordoma`'s entire skull-base subsection. Zero
  false positives.
- **`upper part of the|your brain` and `back of the brain`.** Round 4's
  report-words-versus-plain-words finding in a fourth costume, and the worst one yet:
  these are the words the test's own pinned claims use for four of the nine hubs
  (`glioblastoma`, `low-grade-glioma` and `high-grade-glioma` for *the upper part of
  the brain*, `ependymoma` for *the back of the brain*), so
  *"A glioma in the upper part of the brain usually comes out whole"* walked straight
  through. Cost: zero for the first, and ONE for the second when it was taken — the
  sourced recurrence pair on `/tumors/ependymoma`, **which this item then deleted**. So
  `back of the brain` costs **zero today**, and what it is still load-bearing for is the
  `worst` qualification below, which only fires because of it. §12.18: a rationale that
  stopped being true is how one gets copied, so the rationale says which half is live.
- **And then a FIFTH costume, because taking two of them was not taking the set.**
  `/review` round 2 wrote twenty-five fresh attacks in the corpus's own register and
  **twenty-four passed**, on the region names this site teaches everywhere.
  `blocks/mechanism.md`'s own bullets are *"Side of the brain, near the temple"* and
  *"Upper back part of the brain"*, and they reach the **eighteen** hubs that include
  that block; the guard could not see either. (An earlier draft of this bullet quoted
  *"the side of your brain, near your ear"* and *"the upper back part of your brain"*
  and put them on twenty and nineteen pages. Those are `/where-your-tumor-is`'s own
  `###` HEADINGS and each occurs ONCE in the whole corpus. The TOKENS were right
  either way, because they carry both determiners — but the positive controls had been
  written off the quotation instead of off the corpus, so three of them exercised only
  the `your` branch while eighteen composed pages are written in the `the` branch.
  **A quotation in a ruling is a claim. Grep it like one.**) Nine more tokens went in, **every one measured at
  zero across all twenty-three hubs plus both control pages** — and then `/review`
  round 3 took **two of the nine back out**, which is the part to copy:

  > **"Costs zero rows" is not the same test as "is an address."**

  `\bneck\b` has 42 matches under `Content/` and **forty-one of them are not
  addresses** — they are symptom and procedure prose (*"a stiff neck"*, *"pain in the
  back or neck"*), one of them inside the shared escalation block, so it was on ~20
  pages. (The single real address is `/tumors/hemangioblastoma`'s *"the neck is the
  most common part of the spine for one"*. One in forty-two is the argument; an earlier
  draft said none in forty-two, and `/review` counted them.) It
  cost zero rows by luck, and it did real damage: it gave `/treatments/craniotomy` two
  address matches and thereby **falsified the "zero addresses" measurement written
  three files away in the same review round that added the token** (see finding 6
  below). `near the surface` matches **nothing anywhere** — "measured at zero" is
  trivially true of a dead token, and a dead token inflates the apparent width of a
  lexicon a reviewer is trusting. **Nine went in; two came out and one was REPLACED, so seven stayed.** Out: `neck`
  and `near the surface`, for the reasons above. **Replaced: bare `lower back`, and it
  is `neck` all over again one round later** — 11 matches under `Content/` and EIGHT are
  procedure or symptom prose, seven of them *"a needle in the lower back"*, the
  lumbar-puncture site. The only real addresses were `/tumors/hemangioblastoma`'s three
  *"the lower back part of the brain"*, which the bare token caught by being an
  accidental PREFIX of a different phrase — so the phrase is what went in. **AND IT HAD
  A POSITIVE CONTROL, written for it one round earlier, in a register no page in this
  corpus uses** (*"A growth in the lower back is more dangerous than one higher up"*),
  so the ablation test reported the token protected right up to the round somebody read
  it. **A control can keep a bad token alive.** The seven that stayed: *side of the|your
  brain* and *upper back part of the|your brain* — the two the attacks above were
  written from — plus *the stalk the brain sits on*, `thalam`, *cauda equina*,
  *top/bottom end of the cord* and *lower back part of the|your brain*. **THE ELEVEN IN FULL**, since the count is
  quoted elsewhere and the enumeration has to reach it: *upper part of the|your
  brain*, *back of the brain* (batch one); those seven (batch two); and *tailbone*
  and *sacrum*, taken for the sacral end of `/tumors/chordoma`, the only place in the
  corpus where an address has no other word for itself.
  The lesson is not "widen once more". It is that **the pinned claims and the shared
  block are where to read the lexicon off**, because those are the words the corpus is
  actually written in, and a lexicon derived from one page's prose is a lexicon for one
  page.
- **AND THEN THE BOTTLENECK MOVED, which is the measurement WI-571 should start
  from.** With the address side widened, `/review` round 3 wrote twenty-five fresh
  attacks using **only addresses the guard already knew**, varying the difficulty
  phrasing — and **twenty-four passed**, the same 96% the round before had measured
  against the address half. Almost all sat one word from something already banned:
  *do worse* beside *do better*, *better place* beside *worse place*, *returns* beside
  *comes back*, *slower recovery* beside *longer recovery*. Nine went in at zero cost,
  and **two of them were qualified rather than banned bare**: `poor`, which has
  **nine live correct uses in reader text** (*"Poor balance"* twice, *"a poor fit"*,
  *"a poor word for this"*, *"a poor thing to hear on its own"*), and `returns`, which
  has **seven**, six of them innocent (*"Some need time away and then return"*, *"the day
  your child returns"*, *"the treatment section returns to it"*). §12.18 had to learn
  qualify-do-not-ban twice, both times after the false positive arrived; `poor` is the
  first time the rule was applied before it did. **And `returns` is the one that shows
  why the rule needs an assertion and not an intention:** the qualification was recorded
  as applied in one round and was found MISSING FROM THE FILE in the next — finding 8. **Four more measured at zero and were
  deliberately NOT taken** (*takes more out of*, *undertaking*, *bounce back*, *best
  case*): they are idiom rather than rank vocabulary, and §12.18's closing instruction
  is the ruling — *stop adding words once the positive controls and the pinned
  deletions are in place; the next round will always find twenty more.*
- **AND A TOKEN WITH NO POSITIVE CONTROL IS NOT PROTECTED.** Ablating each of the
  eleven new address tokens one at a time turned **no** control red except for the two
  covering `upper part of the brain`. Nine could have been deleted by a later edit in
  silence — which is exactly what the control list exists to stop, and the control list
  said so in its own doc comment while being false of nine-elevenths of the item's own
  work. **Add the control in the same edit as the token, or the token is decoration.**
  **AND THEN WRITING THAT RULE DOWN DID NOT APPLY IT.** Round 3 wrote the sentence
  above and added three controls; round 4 re-measured and found **eight of the eleven
  still unprotected**, plus one of the three new controls carrying TWO new tokens, so
  each masked the other under single-token ablation and it went red for neither. **A
  control carrying two of the things it is testing tests neither.** So the rule is now
  a test — `EveryAddressTokenThisItemAddedHasAControlThatFailsWithoutIt` removes each
  token from the lexicon and requires a control to go red — and the test was itself
  checked by deleting one control and watching it fail. A rule in a comment is a hope;
  a rule in an assertion is a rule.
- **AND THE CONTROLS WERE BEING SCANNED IN THE PAGE'S CONTEXT, WHICH WAS DOING THE
  WORK.** A control appended to the page under test sits inside a four-sentence window
  that reaches back into that page's own prose — so four controls still "passed" on
  `/tumors/craniopharyngioma` with the very tokens they depend on removed, because the
  window found a real address in the page behind them. Fixed with a three-sentence
  spacer carrying neither lexicon. **Isolate a planted defect from the text it is
  planted in, or the text is what passes the control.** This is one failure found in
  three successive rounds at three depths: the count, then the fragment, then the
  context.
- **Three were measured and NOT taken.** `front of the brain` costs two false
  positives, one of them on `/tumors/meningioma` — the page WI-569 measured at zero.
  `middle of the|your brain` costs one. `\bcord\b` costs four — and the fourth is a
  sentence WI-570 itself wrote,
  `/tumors/spinal-cord-tumor`'s *"it says why it has no list of which places are
  dangerous"*, the neatest demonstration available that a measurement is only true
  of the corpus it was taken on. Recorded rather than
  taken, with their numbers, so the next item re-measures instead of re-proposing —
  §12.18: measure the cost before the widening, not after.
- **`worst` is qualified**, and **the rationale is conditional, which is the part to
  copy.** Bare `\bworst\b` costs nothing against WI-569's lexicon; it only fires on
  *"Headaches, often worst on waking up"* once `back of the brain` is an address,
  which WI-570 then made it. §12.18 put a date on the `worse` re-measurement because
  *"a rationale that stopped being true is how one gets copied"*; a rationale that was
  never true of the shipped configuration is the same failure one step earlier, and
  `/review` caught the first draft of this note asserting the unconditional form.

#### Eight ways this went wrong, seven of them silently

Every one was found by running the thing rather than reading it, or by re-reading the
file instead of the report about it. Seven were silent; the other (5) failed loudly and
is here because a binding that had happened to find a file would not have.

1. **`CuratedPage.Section` on a COMPOSED page stops at the block's own heading.**
   `blocks/mechanism.md` contains `## Why your symptoms are the ones you have`, and
   `Section` cuts at the next `## `. So on all eighteen composing hubs, "the location
   section of the composed page" is only the part ABOVE the include — and every one
   of them puts its type-specific prose BELOW it. **Compose for a prose PROPERTY over
   a whole page; never for a structural cut.**
2. **`ReaderText` on a SECTION eats three characters.** It calls `Body`, which slices
   from the front matter's closing `---`; a section has none, `IndexOf` returns -1,
   and the slice starts at index 3. Strip the markers from the page, then cut.
3. **An allowance matched against the four-sentence WINDOW silences sentences nobody
   read.** On `/tumors/ependymoma` two written reasons were covering three rows: the
   third trigger's window happened to contain the fragment excusing the first. The
   prose was innocent and the mechanism was not. **Match an allowance against the
   TRIGGER — the sentence carrying the vocabulary — not against the window it was
   judged in.**
4. **A positive control that asserts only that the row COUNT went up can pass for the
   wrong reason.** With the lexicon artificially narrowed, two bullet-lead controls
   still "passed" on `/tumors/craniopharyngioma`, because the window reached back into
   that page's own prose and found a real address. **Assert the PLANTED row is the one
   that was found.** §12.18's positive controls are the load-bearing half of the
   guard; a control that can be satisfied by the page it is planted in is not one.
   **AND THE FIRST FIX FOR IT WAS HALF A FIX**, which is the part worth copying: the
   required fragment was the control's TRIGGER sentence, and a window always contains
   its own trigger — so three multi-sentence controls still passed on that same page.
   A control has to name **both halves**, the difficulty and the address, and require
   them in one window. An identifier that the thing under test supplies for free
   identifies nothing.
5. **A `params string[]` overload beside a `string` overload is ambiguous at exactly
   one call site.** `Plain("where-your-tumor-is.md")` bound to the slug overload and
   went looking for `where-your-tumor-is.md.md`. It failed loudly; a binding that
   happened to find a file would not have.
6. **The promotion kept one floor and dropped the other, and the dropped one was
   load-bearing for a published claim.** `/treatments/craniotomy` has 21 difficulty
   matches and **zero addresses** — so §12.18's much-quoted *"fires ZERO times on
   /treatments/craniotomy"*, which WI-570 used as its licence to trust the ported
   scanner on nine hubs, is a fact about the lexicon and not about the page. No row
   could have been found there whatever it said. The zero is real and it is not
   evidence. Every caller now states its own minimum, and craniotomy's `0` is written
   down as structural. **A guard measured on a page it cannot fire on has been
   measured on nothing** — and this one had been quoted forward twice.
7. **An allowance fragment that matches two triggers silences a sentence its reason
   was not written about.** That is finding 3 one layer down, and it is invisible the
   same way: the suite is green either way. Assert each kept row matches **exactly
   one**.
8. **AND THE ONE THAT HAPPENED THREE TIMES IN THIS ITEM ALONE: A MULTI-ANCHOR EDIT
   SCRIPT THAT ABORTS WRITES NOTHING, AND ITS ROUND SUMMARY SAYS IT WROTE.** §12.17
   records this; §12.18 found the next layer (the follow-up script silently dropping
   the first anchor). Here it happened three times, and the third cost a whole review
   round: the `returns` qualification was recorded as applied, reviewed as applied,
   and **was not in the file** — one script had aborted on a later anchor and written
   nothing. The abort is correct behaviour and is not the bug. **The bug is believing
   the round's own summary over the file.** Re-grep for what you changed, in the file,
   after the write; and when a script aborts, re-run the WHOLE script rather than
   writing a new one for what is left.

#### The routing promise failed SIX times in one item, twice inside its own fix, and the sixth was not about wording

This is the failure §12.18 named and this item cited, and it is worth its own heading
because it did not stop happening when it was pointed at.

1. `/tumors/low-grade-glioma` said the location page *"sets out what each part does"*.
   **That page refuses to, in its own words:** *"These entries do not list symptoms.
   That question has a better answer of its own, region by region, and it lives on the
   page for the first week."*
2. `/tumors/pituitary-tumor` sent a reader to *"the range a team picks from"*. That
   range is five **surgical** options with no medicine in it, while the pituitary page
   itself says twice that a prolactinoma is usually treated with a pill. The route
   would have walked that reader past their own first-line treatment.
3. `/tumors/chordoma` said the destination *"fits a chordoma up there and not one lower
   down"*. **Two** of that page's parts are head-scoped — its regions list and its
   urgent-signs list, each of which says so in its own words — and the rest of it is
   location-general, so the sentence told the two thirds of that page's readers whose
   tumor is in the spine or sacrum that the page was not for them. (A count of "ten of
   twelve sections" stood here until round 7. It was dropped rather than corrected: the
   fluid section is intracranial throughout and carries no scope statement of its own,
   so it is a third candidate and the arithmetic turns on a judgement rather than on
   the page's own words. **Bound by naming the parts the destination scopes out loud; a
   number invites the next reader to recount it and get a different answer.**
   Over-including a call-us section is also the safe direction, which the destination
   itself prefers.) (The first version of this bullet
   said *"only its regions list is head-scoped"*, which is the claim 4 and 5 below
   exist to refute, stated two paragraphs above them. **A ruling that contradicts
   itself gets copied one paragraph at a time.**)
4. and 5. **The fixes for 2 and 3 then over-corrected in the opposite direction, and
   this pair is the one that mattered.** `/tumors/chordoma` became *"Only its list of
   places is about the head. The rest of it works wherever your tumor is"* and
   `/tumors/spinal-cord-tumor` *"The rest of it is still yours"* — and it is **not
   only the regions list** that is head-scoped. The destination says so: *"If it is in
   or pressing on your spinal cord, parts of the list above were written for a tumor
   in the brain, and the signs that matter most for you are different ones."* **The
   over-claim was in the URGENT-SIGNS direction**, which is the one direction a
   mistake on this site is not allowed to run.

   **WHAT SHIPPED IS NOT A DELETION IN EITHER CASE, and the difference is the whole
   lesson.** *"The rest of it works wherever your tumor is"* is still live on
   `/tumors/chordoma`, and it is correct there — because the two sentences in front of
   it now name **both** head-scoped parts, so "the rest" is a defined remainder rather
   than a claim about a page nobody has counted. `/tumors/spinal-cord-tumor`'s version
   was reworded outright (*"What is left over is still yours"*) because its sentence
   sat after only one of the two bounds. **An unbounded remainder is the defect; the
   fix is to define what it is the remainder OF, not to stop saying it.** (An earlier
   draft of this very paragraph recorded the chordoma sentence as removed, which would
   have told the next item that a sentence currently serving a reader was a mistake —
   §12.18's "a note that describes text a later round replaced", running backwards.)

6. **AND THE SIXTH IS NOT ABOUT WORDING AT ALL, WHICH IS WHY THREE ROUNDS OF READING
   THE SENTENCES DID NOT FIND IT.** `/tumors/chordoma` names three sites — skull base,
   spine, sacrum. Its route sat at the END of `### In the spine and at the tailbone`,
   led with *"The floor of the skull"*, and deep-linked
   `/where-your-tumor-is#skull-base`. So the two thirds of that page's readers whose
   tumor is lower down finished THEIR subsection on a lead about the skull and landed
   inside the destination's head-only regions list, in the one entry that is not
   theirs. **Every sentence in that paragraph was true by then.** The prose had been
   corrected over three rounds while the filing stayed wrong.

   > **A route can be word-perfect and still be addressed to the wrong reader. On a
   > page with more than one site, check the FILING and the ANCHOR as well as the
   > promise.**

   The paragraph moved up into the section preamble, where it speaks to all three
   sites, and the anchor went — a deep link into a head-only list is the same defect
   in a different costume. Both the placement and the absence of the anchor are now
   asserted, because nothing had been guarding either.

**The pattern, and it is not carelessness.** A route is written while looking at the
page being edited, and **it is a claim about a page that is not on the screen** — so
the natural failure is to describe the destination from memory of why you linked it.
Both the under-claim and the over-claim come from the same place. **Open the
destination, read the section you are promising, and write the upper bound; then check
the bound against the destination's own scope statements, which is where it says what
it is NOT for.**

#### The second deletion, which the sweep could only see half of

`/review` round 2 found what the lexicon had missed, one section away from the defect
the sweep did find. `/tumors/ependymoma`'s "If it comes back" section read:

> *"After a tumor at the back of the brain, it tends to come back in the same place.
> After one higher up in the brain, it more often turns up somewhere else in the brain
> or spine."*

Two addresses, two answers, and the second is materially worse news than the first.
**The claim is true and it is in that page's own sources** — StatPearls: posterior
fossa ependymomas *"recur locally, whereas supratentorial ependymomas tend to be
disseminated at relapse"*. It had been pinned by a test, because an earlier item's
`/review` round asked for the split by site as a nit.

> **A true, sourced, previously-approved sentence can still be the forbidden artifact.
> §12.18's line is about what an address entry may CARRY, not about whether the claim
> has evidence — and Wave 6 bans the location-keyed lookup whatever its evidence is.**
> That is the whole reason the constraint is written as a property of the SHAPE.

What replaced it names the territory without handing two addresses two answers: *"It
can come back in the place it started, or somewhere else in the brain or spine. Both of
those happen."* Every reader gets the same answer, so the row test passes rather than
being argued around, and nobody loses the fact that it can turn up in the spine. The
pinning test now asserts the replacement **and** that the split does not come back,
which is how an old review nit gets retired rather than silently reversed.

**AND THE HALF THE GUARD COULD NOT SEE IS THE FINDING.** The first sentence carried
*come back* plus an address and fired. **The second carried no difficulty word at all
and was invisible.** So one written allowance was standing over one half of a pair —
and its reason, written to excuse the visible half, claimed the passage *"changes what
the team WATCHES, not what happens to the reader"*, which **nothing on the page said**.
A property guard standing over one sentence of a pair is not standing over the pair,
and an allowance is the place where that goes unnoticed: it looks like the sentence has
been considered. **Read the sentences either side of every kept row.**

#### The residual, listed rather than implied — and it is TWO lists, not one

`/review` round 3's correction, and it matters because the two categories need
opposite things from the next reader.

**(a) Kept rows the guard DOES see.** Thirteen, on seven hubs, each with a written
reason in `LocationObligationSweepTests`, each asserted to still fire and to match
exactly one trigger. These are safe in the ordinary way: a reviewer can read the
reason against the sentence, and a lexicon that narrows turns them red.
`/tumors/hemangioblastoma`'s *"In the brainstem, symptoms can be more serious"* is one
of these — a symptom with its mechanism attached, sourced, and carried deliberately in
the under-triage-safe direction, since that page's own `/review` round 1 called it a
BLOCKER when a draft took *"more serious"* and dropped *"needing emergency
attention"*.

**(b) Sentences NO property guard reaches. These are the list that matters**, because
nothing will ever raise them again:

- `/tumors/chordoma`: *"How high up the tumor sits decides how much of that function is
  at risk, and your team can usually tell you beforehand."* *How high up* is not an
  address token and bare *at risk* is deliberately not a difficulty token. Kept: it is
  sourced, it points at a conversation rather than at an outcome, and it is what a
  reader needs before an irreversible operation.
- `/tumors/ependymoma`: *"That is not always possible, because of where it sits. When
  some has to be left behind, it is much harder to cure."* **Split across two
  sentences, and that is why nothing reaches it**: the first carries neither an address
  nor a difficulty word, and the second carries the difficulty and no place. It is kept
  because it is keyed to EXTENT, which is this tumor's own factor, and names no address
  for a reader to look up. This is the item's own *read the sentences either side*
  lesson landing on a pair that is CORRECT — the same shape as the recurrence pair it
  deleted, with the opposite verdict. (An earlier draft quoted only the first sentence
  and said it "carries the vocabulary", which is true of neither half on its own.
  §12.18's structural residual is a different shape — *"Where it sits sets the ceiling
  on what surgery can achieve"* — measured there at three false positives to close, one
  of them a page's own refusal, and left open. Still open.)
- `/tumors/spinal-cord-tumor`'s outlook gate: *"What happens next turns on four things:
  the kind of tumor, where it sits, how much came out, and how bad things were before
  treatment."* Factor-keyed, and *where it sits* is one factor among four rather than a
  lookup — so not a row — but it is an outlook passage naming place, and no guard
  touches it.
- **AND ONE THAT IS A SHAPE RATHER THAN A SENTENCE: THE WINDOW IS FOUR.** A row spread
  over FIVE sentences is invisible even though every word in it is already in both
  lexicons: *"- On the floor of the skull. It is a crowded place. Nerves for the eyes
  run through it. Big vessels do too. Tumors there are harder to take out
  completely."* This is a different class from a missing word, and it is the only one
  of twenty-five fresh attacks at round 4 that was. **Widening the window is not the
  fix** — §12.18 measured that and found a fixed window plus the back-reference gate
  is what keeps the false-positive rate at zero; the headroom it measured was three,
  not nine. The width is named here so the next item knows what it costs rather than
  rediscovering it: five sentences of bulleted prose between an address and its
  difficulty walks through.

**(c) LEXICON BRANCHES NO ABLATION REACHES — a third kind, and the one that took
FOUR review rounds to stop getting wrong.** Several address tokens carry
`(?:the|your)`, because the corpus writes a region one way in a shared block (*"Side of
the brain, near the temple"*) and the other way in a page heading (*"The side of your
brain, near your ear"*).
`EveryAddressTokenThisItemAddedHasAControlThatFailsWithoutIt` ablates whole TOKENS, so
it cannot see a branch being trimmed.

**AND THE REAL LESSON IS ABOUT THE RECORD, NOT ABOUT THE BRANCHES.** Rounds 9, 10, 11
and 12 each found the previous round's account of which branches were covered wrong —
four rounds on one paragraph, every time an arithmetic slip in a hand-maintained
census. **A census of a thing the code already enumerates is a count in prose beside a
count in code**, which is the failure this very item asserts against everywhere else
(`Assert.Equal(9, …)`, `(13, …)`, `(11, …)`, `(30, …)`). So the census was deleted and
the iterated set of
`NarrowingTheOrYourInTheAddressLexiconTurnsAControlRed` is the record: whatever it
covers is covered, and adding a token there is how a branch gets watched. What is left
in prose is only what the code cannot say:

> **A branch can be alive in the corpus, load-bearing, and still invisible to ablation,
> because ablation measures the CONTROLS and not the pages.** And a branch INSIDE a
> control can be invisible too, if that control carries a second address token which
> masks it.

`base of the brain` was the case worth acting on rather than recording: it reaches all
eighteen `[MECHANISM]` hubs and had no control at all, so it got one. Four branches are
left uncovered, each needing a control written to carry exactly ONE address — which is
§12.18's stop rule deciding where this item ends and the next one starts.

**The claim "the sweep found one defect" is true of a lexicon, not of the corpus.**
Writing the residual down is what keeps that distinction visible to the next item.

#### Where the item's own new prose came from

Five routes, one per hub, each worded for its own reader; the shared link label is
left identical on purpose (§12.18: making a label deliberately different costs the
reader a recognisable link and buys nothing), and the words AFTER it differ. Two of
the five collided there on the first suite run — both closed with *"is the page for
the address rather than the diagnosis"*, which is also a seven-word neighbour of
`/tumors/meningioma`'s own sentence — and the corpus restatement probe caught all
three in one run. **That probe is the reason the shared-label convention is safe at
all; without it the convention is just a licence to copy.**

One report word was added: `/tumors/ependymoma`'s middle location bullet said *"Higher
up in the brain"* and stopped, while the bullet above it teaches *posterior fossa*.
It now teaches **supratentorial**, which is §12.18 part 2 applied to the one entry
that had dropped it. (The first version of this sentence said *"the bullets on either
side"*; the bullet BELOW teaches *myxopapillary*, which is a subtype name and not a
report word for a place. A justification deserves the same check as a claim.)

### 12.20 When the only name a reader has is a place (WI-571)

§12.17 built a page for the location axis. §12.18 and §12.19 settled what a TYPE page may
carry about location, and where the obligation lives across the corpus. This is the
remaining reader: the one handed a word that **is** an address, with no type behind it.
*"Tectal glioma"* is that word, and the ruling starts by refusing the obvious home for it.

**IT CANNOT BE TUMOR TYPE #24, AND THE REASON IS IN `taxonomy.yml`'s OWN HEADER.** That
file says `also` lists *"aliases the classifier may be given; they never render"* and that
aliases *"must be true synonyms, never 'close enough'"*. A location is not a synonym of a
type, and §12.2 item 3 requires WHO CNS5 naming throughout — CNS5 has no entity of that
name. Adding it would put a **place** into the closed list of **diagnoses** the classifier
may emit, which is the one list on this site whose closedness is a safety property.

> **A word that names a place gets a section, never a slug.**

#### The "search alias" needed no mechanism, and that is a measurement

The backlog asked for *"a search alias, so the word a reader was actually given finds
something"*. There is nothing to build, and the reason is in the code rather than in a
hope. `ContentStore.SearchPages` (WI-309) scores every file under `Content/pages` as
`title.Contains(term) ? 5 : 0` plus `body.Contains(term) ? 1 : 0`, where `body` is
`page.Markdown` — which `Parse` sets to the **composed body**, after the front matter has
been sliced off. Three consequences, and each has a test because `/review` round 6 found
that only the first one did:

- **The word in the PROSE is the whole mechanism.** No field, no index, no alias table.
- **A word in a front-matter source comment is invisible to search** — and invisible to
  the reader, by the same mechanism. The two failure modes coincide for once, which is
  worth knowing because they usually do not.
- **`/glossary` is not searched at all.** It is a Razor page, not a curated page under
  `Content/pages`, so `SearchPages` never enumerates it.

**AND THE SCORING HAS A CONSEQUENCE THE ITEM HAD TO MEASURE.** Scoring is per-term, so a
search for *"tectal glioma"* puts every page with *glioma* in its TITLE above the one page
that carries *tectal* in its body. That is fine while `SearchModel.PageLimit` exceeds the
number of glioma-titled pages, and **that comparison is a test rather than a sentence here
precisely because both sides of it move.** The reader who types the word they were handed
falls off the end of the list when the pages that outrank it fill the limit — and a
sentence here saying which page number that is would be the count-in-prose this ruling
spends two sections on. (`/review` round 6 caught the first version saying *"the day
somebody adds a sixth"*, which is wrong and was also the count.)

**AND A MULTI-WORD QUERY IS NOT A PHRASE QUERY**, which the same scoring implies and which
cost a test one round: `SearchPages` splits on whitespace and scores per term, so a search
for a two-word glossary phrase can match a page on its common word alone. The negative
assertion for "the glossary is not searched" therefore has to be a single word.

#### Both "alias" homes are refused, on their merits rather than because the word matched

The glossary's `also` is a REAL candidate and deserves a real answer: unlike the taxonomy
it **does** render (*"Also called:"* on `/glossary`) and it **does** feed tooltips
(`GlossaryTooltips` adds `term.Aliases` to the trigger names). It is refused by **WI-519's
rule — an entry defined and used in ONE place fires nowhere.** An entry would be suppressed
on the page that defines the word, and no other page says it, so it would fire nowhere —
exactly the refusal WI-567 wrote for `temporal`, `parietal` and `occipital lobe`. (Stated
as a conditional, because it is one: there is no entry and no `!%…%` marker, and `/review`
round 2 caught the first draft writing that the suppression already happens.)

**The trigger for revisiting is DERIVED, not remembered.** The test counts the corpus's
reader-text occurrences of the word outside this page and requires zero, so the day a
second page says it, the refusal expires loudly and the entry gets written. §12.19's
lesson, applied before it cost anything: never write a count in prose about something the
code can enumerate.

#### The row test on a reader who has exactly one address

This is the hard half. §12.19's instrument is *could a reader read their OWN address off
this sentence and get a different answer from a reader with a different one?* — and **a
reader whose only word is a place is by definition reading about one address**, so the ban
is easy to trip and easy to miss. Every sentence had to be asked.

Most of the section passes without argument: where the tectum is, is anatomy; the sentence
routing to *"The place is not a diagnosis"* is the refusal of a row.

**AND THE FLUID SENTENCE IS PERMITTED BY §12.18, NOT BY §12.19's STRUCTURE ALLOWANCE**,
which is `/review` round 6's correction and the distinction matters more than it looks.
*"This is a spot where the fluid can be held up"* names what a reader notices happening
where their tumor sits, and §12.18's line is that **an address entry may carry a SYMPTOM
and must never carry a DIFFICULTY** — which is the clause the page's front matter has cited
all along. Reaching instead for §12.19's row-test allowance (*"keyed to a factor, an age, a
gene or a structure"*) is an over-read, because the structure in that sentence IS the
reader's own address. And the allowance is dangerous in the other direction too: §12.18
records it leaving an OUTCOME keyed to a structure **unreachable by any property guard** —
*"A small meningioma against the nerve to your eye will take your sight"* — which is why
that sentence had to be deleted by name rather than caught. **The allowance does not permit
such a sentence; it makes the guard unable to bar it.** (Round 5 corrected an earlier
version of this clause that said §12.18 was *"licensing"* it, which inverts the holding;
round 6 found the clause was also being applied to the wrong sentence. Two copies of one
justification, giving two different reasons — the front matter's is the right one.)

**The management claims are the row-shaped ones, and they are licensed as a cluster rather
than singly**: every sentence that says what a team aims at for this address. They include
that most were watched rather than having the growth removed, that patients often need
treatment for the fluid, that a fluid procedure was the most common operation, and that the
second paper reports the same two parts. (An earlier version said *"One sentence is
row-shaped"*, and its replacement said *"Four are"* — which is the same shape one increment
along, in the paragraph naming that error. **The property is the record; the count is the
defect.** Rounds 6 and 7.) The licence:

> §12.19: *"an address may key **what is aimed at** only where the source itself is scoped
> by site, and never **how the reader does**."*

The systematic review behind *most of these were watched with repeat scans* is scoped to
nothing but the site — its subject IS tectal gliomas — and what it keys to the site is what
a team aims at. **Nothing in the section is ranked against another address**, so unlike
`/tumors/chordoma`'s licensed pair there is not even a harder half and an easier half
needing to balance. The licence is written into the front matter and into the test, because
§12.19's own rule is that the exception is **named rather than inferred**.

**AND THE GUARD'S SILENCE OVER THIS SECTION IS STRUCTURAL, NOT EVIDENCE — WHICH IS §12.19
FINDING 6 ARRIVING A THIRD TIME, INSIDE THE ITEM WHOSE SUBJECT IS THE ROW TEST.** The first
draft of the test docstring said the ranking guard and the corpus sweep *"both run over this
prose and both stay green … which is the measurement rather than the intention."* `/review`
measured it: the section matches `LocationDifficulty` **zero** times, so no window in it can
carry a difficulty word and no row could have been found there whatever it said. That is
§12.19's sentence about `/treatments/craniotomy`'s famous zero — *a guard measured on a page
it cannot fire on has been measured on nothing* — and the ZERO it was written about had
already been quoted forward twice before this. **The sentence transfers; the zero does
not.** Craniotomy's is zero
ADDRESSES with twenty-one difficulty matches; this section's is zero DIFFICULTIES with
several addresses. Both make the guard unable to fire, from opposite ends, and a note that
says "the same zero" invites the next reader to check only the half that was checked last
time.

> **When a guard is green over new prose, ask whether it could have fired. Assert the zero,
> and plant a row to prove the guard reaches that text at all.**

#### AND THE LESSON THAT GENERALISES: THE ITEM'S OWN NEW WORDS WERE OUTSIDE ITS OWN GUARD

Planting that row is what found it. **The planted row would not fire**, because the address
lexicon had never heard of *midbrain* or *tectum*. A row written in the new section's own
vocabulary was invisible — not through an attack, not through a costume, but because
nothing had told the guard those words exist.

**AND THE TWO WORDS GOT THERE BY DIFFERENT ROUTES, WHICH IS THE PART WORTH COPYING.**
*tectum* was genuinely new: WI-571 taught it. ***midbrain* was not.** WI-567's own
`#brainstem` entry on this page has said *"the **midbrain**, the **pons** and the
**medulla**"* in reader text since it shipped — **four work items, two lexicon widenings
(WI-569 and WI-570), twenty-odd review rounds and every ablation test ago** — and the
address lexicon never had it. Nothing found it until a planted row needed it. (An earlier
version of this paragraph said both words were ones *"this corpus had never said"*, which
is false of *midbrain* and made the safe half of the finding the only half.
`/review` round 7.)

> **An item that teaches the corpus a new place-word has widened the corpus past its own
> guard: add the word to the address lexicon in the same edit that teaches it. And the
> harder half — A REPORT WORD CAN SIT IN READER TEXT FOR FOUR ITEMS WITHOUT ENTERING THE
> LEXICON, because **nothing in this family derives the LEXICON from the pages.** The
> page-facing checks — the corpus sweep, the `minAddresses` floors — read the pages
> *through* it, so a place-word the lexicon lacks is invisible to them too: deleting
> `\bmidbrain\b` costs three of seventy-two matches and no floor fires. Sweep the words the
> corpus already teaches against the lexicon; do not wait for an attack to name one.**

Both tokens were measured over the whole corpus before being taken, which is the only way
§12.18 permits a widening. `\bmidbrain\b` and `\btect` match on **one file** — this page —
and nothing else in the corpus begins a word either way. Both cost **zero rows**, and the
reason is narrower than it first looked: the new section carries no difficulty vocabulary,
and the `#brainstem` entry that has said *midbrain* since WI-567 carries none either.

**And the counts are quoted at the scope the lexicon actually reads**: `\bmidbrain\b`
matches **three times in reader text** and `\btect` **four**. Everything else either token
matches on that page is in the FRONT MATTER, which the strip removes before any guard sees it —
and the two halves of that are not the same: `midbrain`'s are all in source comments, while four
of `tect`'s are in source **`title:` values**, which `ContentPage.cshtml` renders into the
reader's source list. **So those four are invisible to every guard in this item and visible to
every reader**, which is the blind spot `TheSectionForTheReaderWhoseOnlyWordIsAPlace` records by
name. (An earlier version of this sentence said "source comments" of both tokens, which is the
claim-in-two-homes shape this section is written about, with the wrong version in the ruling.
`/review` round 16.)

**THE WHOLE-FILE COUNTS WERE HERE AND ARE DELETED, AND WHY IS THE BEST EXAMPLE THIS ITEM
PRODUCED OF ITS OWN RULE.** `/review` round 2 corrected the "twenty files" claim below and,
in the same edit, wrote whole-file counts of *six* and *thirteen* — which the note it was
adding in that edit made wrong. The note it added said *tectal*, *midbrain* and *roof of the
midbrain* in its own text, so the file counts became 8 and 14 the moment the sentence
quoting them was written. (Round 11: two of the three strings this
sentence used to quote no longer exist in the file, because round 10 replaced that note with
a citation whose `title:` uses a hyphen. **A ruling that quotes a file quotes a moving
target**; what is durable is the mechanism, so the words are named rather than quoted.)

> **A count in prose about something the code can enumerate does not merely drift. It can
> be falsified by the sentence that states it.**

So they are deleted rather than re-derived: they are the scope no guard reads, they move
whenever anybody edits a source comment, and the reader-text numbers were right both times.

**The bare form was measured too and it is the cautionary half.** `tect` without the word
boundary matches **34 files** under `Content/` — 17 of them in reader text — and the
matches are dominated by ***protect*** and its inflections (34 occurrences) and
***detection*** (20) — substring counts and CASE-INSENSITIVE, which are the relevant ones
because the whole subject of this paragraph is what a bare, case-insensitive token matches;
a case-sensitive pass over the same scope gives 32 and 19, which is what would make a
re-checker think the numbers had drifted; the word-bounded count for
*protect* is 18, and quoting that one reversed the ranking until `/review` round 5. A `*.md`
scope, since
`Content/` also holds two `.cs` files that match. (An earlier version of
this paragraph said "twenty files including the shared escalation block", and all three
parts of that were wrong: no scope gives twenty; it named *detect* and *detected*, which
are the rare ones at one and three; and `blocks/escalation.md`'s only match is inside a
source URL in its FRONT MATTER, so it is stripped before composition and the token could
never have reached a single composed page through it. **The example chosen to show the
widest blast radius was the one place the token cannot go.** §12.19's own line — *a
quotation in a ruling is a claim; grep it like one* — and this claim had already been
copied into four files.)
Each token got a control carrying exactly ONE address in the same edit, and
`EveryAddressTokenWi571AddedHasAControlThatFailsWithoutIt` is the acceptance.

#### The four branches §12.18's stop rule left, closed

§12.19 left two address tokens whose `(?:the|your)` branches no ablation could reach, and
the cause was **masking** rather than absence: the only control carrying each token also
carried a second address token, so narrowing the first could never show a loss.
`(?:top|front|back) of (?:the|your) skull` was masked inside its control by `\bfloor\b`;
`(?:top|back) of (?:the|your) head` by `\bcliv`. Four controls carrying exactly one address
each closed it, and every `(?:the|your)` token in the lexicon is now watched.

**The completeness claim is computed, and it is not keyed to a spelling.** The first version
counted occurrences of the literal `(?:the|your)`, which is a ban list in disguise: a later
token written `(?:your|the)` or `(the|your)` adds an unwatched branch and the check stays
green. It counts `your` instead — a word that appears in this lexicon only inside determiner
alternations — so the invariant holds however the alternation is spelled.

#### What was barred, and the barred wording was in the item's own sources

The backlog barred Dana-Farber's and Boston Children's cure-rate and *"excellent
prognosis"* language. **All FOUR of the sources this item read carry the same
shape:** Imoto et al write *"tends to have a good prognosis"*; the systematic review's
abstract opens *"generally have a benign clinical course"*; the series added at `/review`
round 1 concludes *"the natural history of these lesions lends to excellent long-term
survival"*; and the 2013 review added at round 10 opens *"generally benign neoplastic
lesions"*. (Three, until round 10 found the fourth source — and round 11 found this sentence
still saying three while two other homes said four. **Adding a source means re-counting every
sentence that counts them.**)

> **A bar has to be a PROPERTY, not a blocklist of publishers.** §12.14 said banning a
> citation is not fixing a claim; this is the same rule from the other end — the claim was
> never the publishers' invention, it is what the literature says, and a page that only
> refuses two hospital websites has refused nothing.

Also barred and recorded by name, because the next person to open these sources meets each
one in an abstract:

- **Every figure from the review.** Its own resection rate runs *"from 2.3 to 100.0%"*
  across studies for one tumor. The page prints that qualitatively — *from almost never to
  always, depending on which study was counted* — and routes to the section that already
  owns the argument. (`/review` caught the first draft writing *"nearly always"* for 100%
  and *"which hospital"* for what the source counted across **studies**. Understating the
  top of that range weakens the item's own case, since the WIDTH of the spread is the whole
  reason no number is published.)
- **A location keyed to an outcome, in a source this page now cites.**
  `10.3171/2023.4.peds22485` reports *"lesion involvement of the pons … significantly
  associated with worse radiographic PFS"* and involvement *"beyond the tectum"* as a
  predictor of needing treatment. That is the Wave 6 artifact itself.
- **A size rule.** The review says resection *"should be reserved for large tumors"*. This
  page refused a size claim twice already (§12.17) in the OPPOSITE direction, and a reader
  who measures themselves against *"large"* has been handed a rule nobody meant them to
  apply.
- **`visual field` and driving.** WI-573 owns both, and **the three sources here that carry
  symptom or presentation lists each offered the chance to break that ban while quoting
  correctly**: the review's abstract
  names *"visual field changes"* among the common findings, Imoto's symptom sentence says
  *"along with visual field deficits and cognitive dysfunction"*, and the third source
  names *"extraocular eye movement abnormalities at presentation"* **and, in its
  introduction, *"Neurological deficits are less commonly observed but may include
  nystagmus, diplopia, seizures, and visual deficits"*** — because the DOI serves that
  paper's FULL TEXT, not just its abstract, which `/review` round 6 noticed while the item's
  own notes were calling it an abstract. So the ban held **four** times across three
  sources. That is what a ban on another item's subject is for. (An earlier version of
  this bullet said *"this is the one
  place"* — a closed count, §12.17's error again, and the page's own suite bans that string
  in reader text. **A count of one is the easiest closed count to believe, because nobody
  recounts it.** The Imoto quotation in the front matter had also been truncated one clause
  short of the words that falsify it, which is the same defect this item records for the
  review.)

#### The unverified classification claim: chased, and it stays unpublished

The backlog flagged as unverified the claim that CNS5 folds most tectal gliomas into
*"diffuse low-grade glioma, MAPK pathway-altered"*, and asked for
`10.1007/s00401-026-03066-7` to be chased before any classification sentence. **Chased. The
DOI is real and the paper cannot be read.** Tauziède-Espariat, Métais, Aldape et al,
*"Tectal glioma versus pilocytic astrocytoma: revisiting tumor classification in light of
molecular heterogeneity"*, Acta Neuropathologica 152, article 22, Correspondence, published
2026-08-20, **PMID 42622715**. The publisher page says *"This is a preview of subscription
content"* and carries no abstract; Europe PMC has `isOpenAccess: N`, `inEPMC: N`, no PMCID
and a null abstract. There is no second route.

**So §12.17's Moffitt rule applies and no classification sentence is published.** The claim
is neither published nor refuted — it is recorded as still unverified, with its identifiers,
so the next item does not chase it from scratch.

**WHAT THE PAGE SAYS INSTEAD IS AN OBSERVATION ABOUT THE OPEN-ACCESS PAPER'S TEXT, NOT A
CONCLUSION ATTRIBUTED TO ITS AUTHORS — AND THE DIFFERENCE TOOK TWO ROUNDS TO GET RIGHT.**
Imoto et al. say both *"TG is typically classified as pilocytic astrocytoma (PA) or lower
grade astrocytic glioma"* and *"the DNA methylation profile of TG suggests its
classification as a distinct entity from other lower grade glioma (LrGG)s"*. Those two do
not agree about which named category the group belongs to, and **that disagreement is a
fact about the paper's own words**, which a reader can check. So the page says the tissue
turned out to be a different thing in different people, and that the paper's own words
about where the group belongs do not agree with each other. It names no answer.

It said *"where the group belongs among the named types is still an open question"* until
`/review` round 3, attributed to the paper with the verb *reports*. Imoto never says that;
it is the inference a reader would draw, which is a different thing from a finding.

> **An observation about what a source SAYS can be published where a conclusion drawn FROM
> it cannot. The OBJECT of the verb is the whole difference: *says* and *reports* are right
> for what is in the paper, and wrong for what a reader would conclude from it.**

This maxim took two rounds to stop being a ban list. Round 4 wrote *"'reports' is the one
that launders one into the other"*, which condemns a verb this page uses correctly twice
four paragraphs away — *"Another paper … reports the same two parts"* and *"It also reports
that in adults these are more often found by chance"*, both genuine findings of that paper.
Round 5 fixed the clause after the colon and left *"the verb is the whole difference"*
standing in front of it, which round 6 pointed out is self-contradicting: if the same two
verbs are right or wrong depending on what they are attached to, **the verb is exactly not
the difference.** §12.17's ban-list lesson, arriving inside a maxim about attribution.

**AND THIS PARAGRAPH IS WHERE THAT FIX FAILED TO LAND, WHICH IS ROUND 4's ONLY BLOCKER.**
Round 3 deleted the sentence from the page and left it in the ruling and in the page's own
front matter — the ruling still saying the page said it, and still calling it *"the one thing
the open-access source carries"*, which is the attribution being removed. The suite bans the
phrase in reader text, so §12.20 was describing prose its own guard forbade.
`TestsLibraryPagesTests.cs` carries the line this broke, written in the same round:
**correcting a claim in one file does not correct its copies.** Grep the fix, not just the
defect.

#### Grepping a quotation is not enough: grep it in the characters the source used

§12.19's standing rule is that **a quotation in a ruling is a claim; grep it like one.** This
item applied that rule and it still nearly published a quotation nobody could check, because
the rule is incomplete.

`/review` round 10 asked for the open-access paper's cohort to be recorded — the section says
*"in the small group one study followed"*, and *small group* had no measurement behind it on a
page whose standard is a verbatim quote beside every used claim. The note written to close that
**failed its own grep twice**: first on the en dash in *6–45*, then on the **non-breaking
space** between the figure and its unit. The fetched text reads `30.5` + **U+00A0** +
`years`, and the range as
`6` + **U+2013** + `45` + **U+00A0** + `years`. The characters are NAMED here rather
than pasted, and that is not fussiness: pasted, they are indistinguishable from a space
and a hyphen, so a paragraph explaining the trap would demonstrate nothing and would
quietly put three invisible characters into a design doc. (The first version of this
paragraph did exactly that, including a stray U+200B nobody asked for.) Typed with an
ordinary space and a hyphen, the quotation matched nothing.

> **A grep that finds nothing looks exactly like a quotation that was invented.** So the rule
> needs its second half: grep a quotation in the characters the SOURCE used, and where a
> quotation would span a number-unit boundary, stop it before the boundary rather than retype
> the whitespace.

Every item in this corpus that quotes a figure is exposed to this, which is why it is here and
not in one page's front matter.

**AND IT PROMPTED A SWEEP OF ALL OF THEM, which is the part to copy.** Rather than fix the one
note, this item ran every source quotation in the page's front matter against every fetched
source text for both items that wrote them — `work_files/wi571/verify-quotes4.py`, whose
printed output is the record rather than a number repeated here. (Versions 2 and 3 are the
artifacts of the two defects below; **naming the broken one as the record is what `/review`
round 13 caught this paragraph doing.**) **The residue falls into six
categories, and the list was rewritten twice before it was derived from the items rather than
the items sorted into it:**

1. **Quotations of CORPUS ARTIFACTS rather than of sources** — this page's own reader text, a
   sentence from another page, an earlier draft the note records as deleted, a repo file
   (`taxonomy.yml`'s header — where the nested `'close enough'` is a single-quote substitution
   for the file's own double quotes, which is itself a modified quotation forced by nesting and
   recorded here rather than left to be found), a §12.x ruling, or the backlog's own refused
   claim. **This is the
   largest group and no version of the list had it**, because the question the list was written
   to answer was *which source is this from* and most of these are not from a source at all.
2. A source neither item could save locally — the Europe PMC article, whose HTML 403s.
3. A legitimate elision (`…`) or a reconstruction from a bulleted list, where the ` / `
   separators are the note's own.
4. **A full stop the paper prints after a REFERENCE MARKER** — *"…occurring predominantly in
   children."* against `…in children [ 8 , 12 ].` Several of these were live across two items, and
   the earlier four-category list had concluded *"no quotation on the page is unsupported"* over
   them. All four now stop before the marker.
5. **An artefact of the FETCHED TEXT's own extraction** — the ACS page renders as `(CSF) ,` with
   a space before the comma, which the publisher does not print. **The quotation is right and
   the haystack is wrong**, which is the one residue category where the fix is to leave the
   quotation alone.
6. Segments the check sets aside before looking — **a mechanism, not a kind**, which is why it
   sits last: it drops anything under twenty characters or four words, or carrying a marker only
   this file's prose uses. It cannot recognise the file's own prose as such, so most of that
   lands in category 1 instead. (`/review` round 14: an earlier version of this entry claimed it
   could, which read as a category when it is a filter.)

> **A residue you have categorised is only as good as the categories. If a list of reasons
> explains every item, check that it was written from the items rather than the items sorted
> into the list.** This section's own list failed that twice — once because the extractor was
> hiding a third of the subject, and once because, with the subject recovered, the two largest
> groups had no category.

**AND THE SWEEP ITSELF HAD A BLIND SPOT COVERING ABOUT A THIRD OF ITS SUBJECT, WHICH IS THE
FINDING THAT OUTRANKS EVERY QUOTATION IT FOUND.** The extractor was
`re.findall(r'"([^"]{25,})"')`, and a length filter applied AT EXTRACTION re-phases the
pairing: the page has a quotation shorter than the floor (*"twelve"*), so from there every
CLOSING quote pairs with the next OPENING one — the real quotations became the "between" text
and the commentary became the candidates, which a later filter then dropped as commentary.
**About a third of the subject was never looked at**, including the sentence this section calls
*the sentence the whole section turns on*. Re-reading the recovered residue produced three more
defects: an em dash typed as a hyphen, two more reference-marker full stops, and a phrase inside
a *"Verbatim:"* list that the source it names does not contain. (The exact candidate counts
before and after are in the scripts' output, not here — round 11 had already corrected this
section for giving two different numbers for one run, and round 13 found the replacement pair
falsified by the very fixes that prompted it. **Run the script.**)

> **A check that filters its candidates while pairing them is not measuring what you think.
> Extract first, filter second — and make the coverage LOUD: assert that every delimiter is
> accounted for, so a silent re-phasing fails instead of reporting a smaller job done.**

This is §12.17's rule for an iterate-and-check guard (*say out loud how much you looked at*)
applied to a scratch script — and the reason it matters here is that the sweep's output was
being used to underwrite a completeness claim about the page's whole audit trail.

**AND THE THIRD VERSION HAD A WORSE PROBLEM THAN THE SECOND, WHICH IS THE ONE TO CARRY
FORWARD.** Its haystack was a glob over both items' working directories, and **nine of the
twenty-four files it loaded were not publisher fetches at all** — six rendered captures of
BrainHarbor's own pages, a ContentCheck log, a pre-deploy baseline and a regex scratch file. So
a quotation could be "verified" against this site's own rendered output, and two were: a
sentence this page attributes to itself by kind, and one it borrows from
`/tumors/all-brain-tumors`. Both belong in residue category 1, and the sweep was quietly
absorbing the category this section calls the largest.

> **A check whose haystack contains its own subject cannot report a miss.**

And the floor written to prove the haystack was complete — *abort if fewer than twenty texts* —
**was being met by the padding**: there are fifteen real fetches, so dropping the nine
non-sources makes the old version abort.

> **A floor satisfied by the thing that makes the measurement wrong is worse than no floor.**

The source set is a whitelist now, asserted against the directory, and the run prints the files
it deliberately excluded — so a fetch added later is either listed on purpose or reported.
(Fifteen fetches, and **three of them are walls rather than text**: two reCAPTCHA pages
(`nsr-pubmed.txt`, `statpearls-hydro.txt`) and the Acta paywall preview (`acta.txt`), kept as
evidence that
the route was tried. Naming them matters because two OTHER fetches carry the same publisher's
"preview of subscription content" banner while still holding their abstracts, so a reader
re-deriving the split from the banner alone gets five. Naming the split matters because
that fifteen is now doing the work the discredited floor used to do, and a number doing that
work is the next padded floor.)

**AND THE FOURTH VERSION STILL MISSED ONE, WHICH IS ROUND 13's LESSON ONE LEVEL DOWN.** The
whitelist holds TWO ROUTES of the same paper for two of the sources, and **on one of those pairs
the API route normalises the publisher's dashes to hyphens.** (One pair only: the other
aggregator fetch is a reCAPTCHA wall, so it normalises nothing and cannot exercise the rule. The
check watches both; only one can fire today, and saying so is the difference between a guard and
a guard somebody believes in.) So a
quotation typed from the normalising copy verifies against the normalising copy, and the
divergence from the route the note *says* it read is silent. That is exactly how a fourth
en-dash defect survived to `/review` round 14, on a fact this page's front matter had already
written down twice.

> **A haystack that contains a NORMALISING COPY of its subject cannot report a normalisation
> defect.** Name the file each quotation matched, and say so when the only match is the
> aggregator's.

Three real defects came out of the sweep: the non-breaking space above, the two reference
markers, and a quotation with words CAPITALISED inside the quotation marks for emphasis —
**which is a modified quotation, and it fails the grep for the same reason.** The emphasis
belongs outside the marks. The CAPS one was found twice, because the first fix corrected one
copy and §12.20 said it was fixed while the page's own copy still shouted.

The sweep's own weakness is written down too, because a scratch check with a high
false-positive rate teaches nothing: its first version reported most of its candidates
"unmatched" by treating the prose *between* two quotations as a quotation. A filter and both
items' source sets cut that to a residue readable by hand. **State what a check cannot see
before quoting its number** — and do not quote its number here at all, which is round 11's
correction: two sentences in this section gave two different counts for one script run, in the
section whose own maxim is that a count in prose can be falsified by the sentence that states
it. Run the script.

#### A sweep written from the instance finds the instance

The item's last three rounds were one defect, found three times, because each sweep for it was
written from the copy in front of the author.

A negation guard is written as *"do not fire if a negation is nearby"*, and the natural
expression is `\b(?:not|never|no|n't|hardly|far from)\b`. **The leading `\b` binds the whole
group**, and there is no word boundary inside *isn't* — so `n't` can never match, and the
alternative added to rescue contracted negations does nothing.

- **Round 16** found it in one lookbehind on this item's own page guard and fixed it.
- **Round 17** found a second live copy by grepping the FIX rather than the defect — in
  `ProtonTherapyPageTests`, where the guard **fired on *"Protons aren't better than x-rays"***,
  the contracted form of the sentence its own docstring says it exists to protect. It then swept
  with `\(\?<!\\b\(\?:` — a lookbehind containing a non-capturing group, which is exactly what
  both known instances had been — got a clean report over the rest, and wrote that claim into a
  comment.
- **Round 18** swept over `.cs` **and** `.md` and found **five more**: **four** positive
  `Regex.IsMatch` guards with CAPTURING groups, which round 17's pattern could not match, and
  **the prescription in §12.8 itself**, which called the broken pattern *"the working form"*.

> **A sweep written from the instance finds the instance. Sweep for the DEFECT, in every file
> type, and remember that the RULING is where the next copy will be written from.**

**AND THE FOUR FOUND LAST ARE THE WORST OF THE SIX, because the consequence runs the other way.**
Those are POSITIVE asserts — *a negation must be present* — so a dead alternative does not weaken
them, **it makes them FALSE-FAIL a correct page that negates with a contraction.** §12.8's own
rule is that a rule which fails a correct page is worse than no rule, and
`TestsLibraryPagesTests` names the rescued sentence in the FALSE FAIL list above its own pattern:
*"isn't a minor procedure" — all correct, all flagged.* The widening written to rescue it had
never worked. Every one is now boundary-inside-the-alternation, and the suite is unchanged at
2,601 — **which is the measurement that says no page was relying on the bug.**

**A second hole came out of the same const and closed for free.** `ProtonTherapyPageTests`'
comment said *"a negation inside a superlative is not a negation, which is why 'no better way' is
not in the list"* — while `\bno` was in the list, so *"There is no better option than proton
therapy"* was suppressed. Dropping `\bno` costs nothing measurable. **A comment that says a case
is handled, beside code that suppresses it, is worse than no comment: it stops the next reader
looking.**

**AND ROUND 18's SWEEP WAS THE THIRD ONE WRITTEN FROM THE WRONG THING, which is the last and
sharpest turn of this.** Its criterion was *"an alternative that cannot BEGIN a word"* — and
`n't` begins with `n`, a word character, so **the criterion did not describe the defect at all.**
`/review` round 19 fed that script the five pre-fix files from `HEAD` — every instance this item
ever found — and it printed *"none"* and exited 0.

> **A sweep whose criterion is not the defect returns a clean bill it cannot issue. Validate a
> sweep against the BROKEN tree, not the fixed one.**

The defect is a **word-internal** alternative, and that is measurable rather than a judgement
about first characters:

> **An alternative `A` is unreachable behind a group-leading `\b` when `A` occurs in the corpus
> and `\bA` never does.**

Fed the same five pre-fix files, that criterion catches all five. **Its residue still needs
reading, and saying so is the point** — two benign classes come out of it: **suffix alternations**,
where the `\b` is correctly in front of the stem and the alternatives are endings
(`\b(?:plan|scan)ned`), and `BrainHarbor.Safety`'s **apostrophe-free spellings** (`arent`,
`doesnt`), which exist for text a model might emit and so never appear at a word boundary in
curated prose. Earlier versions of this paragraph claimed the sweep reported *none*;
**a sweep's clean run is only worth what its criterion is worth**, and this one's output is a list
to read rather than a verdict. `work_files/wi571/sweep-boundary.py` carries the criterion, the
validation against the broken tree, and both benign classes.

#### Two roundings the item got wrong first, both in the direction that matters

1. **"Watched rather than TREATED" was false, and it ran in the under-triage direction.**
   The first draft said most of these children were *"watched with repeat scans rather than
   treated"*. The review's very next sentence is *"However, patients often need treatment
   for obstructive hydrocephalus"*, and CSF diversion happened in **89.3%** of them. 65.4%
   monitored supports *most were watched*; *rather than treated* is false of a group where
   nine in ten had a procedure — and `treated` is the SOURCE's own word for the thing they
   needed. On the one page that owns the fluid escalation, that understatement is the one
   direction a mistake here may not run. The contrast is now with having the **growth**
   removed, which is what the review actually contrasts, and the sentence that keeps it
   honest (*"That is not the same as nothing being done"*) is pinned.

2. **AN ABSENCE CLAIM DID NOT SURVIVE ONE REVIEW ROUND.** The section said *"nothing read
   for this page says the same of adults"*. `/review` falsified it inside a round by finding
   a 170-patient series whose median age is 24 — half of it adult — reporting the SAME
   two-part approach (*"treatment mainly consists of observation and management of
   hydrocephalus"*). §12.17's rule is that an unreachable source must be recorded so the
   next item does not re-investigate it; **the other half of that rule is that an absence
   is worth re-searching before it is published.** An absence claim is only as good as its
   last search, and this one had a source behind it within a round. The page now hands the
   adult reader the two-part shape instead of a shrug.

#### Two guards that could go green about the same deletion

Worth its own note, because neither was individually wrong.

The regions preamble was allowed to name the word only as a door, expressed as
*if the preamble says `tectal` then it must also carry the route*. That is a **no-op the
moment the word goes** — and the only unconditional assertion of the route was scoped to
`Section(RegionsHeading)`, which per §12.19 finding 1 spans all nine `###` entries. So an
edit dropping the word from the preamble and moving the link down into a region entry left
**both** green and left the preamble's reader with nothing.

> **A conditional guard and a loosely-scoped one can be green about the same deletion.
> Assert both halves unconditionally, against the same slice of text.**

And the retired ban needed a replacement, not a deletion.
`ThePageDoesNotTakeOverTheWorkOfTheItemsThatFollowIt` forbade `tectal` on this page
because WI-571 owned the word; WI-571 rewrote it rather than deleting it (WI-547's
precedent). But *"require the word in the section"* is a different property from *"forbid it
everywhere else"*, and nothing was stopping tectal material spreading into the fluid or
surgery sections — the two least able to take a location-keyed plan claim. Containment is
now asserted by occurrence count: every instance on the page is either the section or the
preamble's one door.

#### The reading grade, measured after every write

§12.17 measured that a single hard sentence moves this page's average by 0.1 and passes
ContentCheck. So this item enforced a per-sentence **word ceiling** of 24 and re-measured
after every write — which is how it caught its own regression: the fix for the *"rather than
treated"* blocker pushed the longest sentence in the section from 22 words back to 28.
**A correctness fix undoing a readability fix is a trade nobody notices unless the number is
re-measured**, and the page shipped at grade **5.4**, below the **5.6** it started at.

**AND THE CEILING IS A SHIPPED TEST, NOT A SCRATCH SCRIPT** —
`NoSentenceInTheSectionRunsPastTheCeilingAPageAverageCannotSee`. It was tooling until
`/review` round 6 asked the obvious question: the section that regressed twice had no guard.
This is NOT the corpus-wide per-sentence check §12.17 hands to `/pm`, which needs a sweep
first because it will not be this page alone. It is one section's ceiling, and it is the
cheapest possible answer to a gate that averages.

### 12.21 The consequence a reader can act on, and the source that did not carry it (WI-573)

§12.17 built the location page. §12.18, §12.19 and §12.20 settled what a TYPE page may
carry about location, where the obligation lives across the corpus, and what to do for
the reader whose only word is a place. This is the other end: **the most concrete "what
does this mean for me" answer the location axis has**, which is sight, and the thing a
reader will act on this afternoon, which is driving.

#### The backlog named a source and the source does not carry the claim

WI-573's acceptance named Cancer Research UK's driving page as the source for the
vision-and-driving link. **It was read live before a word was written, and the string
`visual field` occurs ZERO times on it; `visual` occurs zero times as a word.** It has
exactly two vision sentences, both pituitary-scoped UK regulation: one names a number of
months (a waiting time, which WI-560 owns and this item is barred from) and the other is a
driving prohibition attached to an exemption from a notification duty. Every other
sentence on it that bears on driving is a waiting time or a UK licensing duty.

So the named source is **cited nowhere**, and the claim is carried by
`PMC11913653` — a brain-tumor fitness-to-drive review the corpus ALREADY cites on
`/tests/neuro-exam-and-memory-testing` for one narrow claim, open access, and
US-inclusive where the named one is UK-only.

> **Read the named source before planning around it. An acceptance criterion that names a
> source is a hypothesis about that source, and it is cheaper to falsify on day one than
> to work around on day three.** The right answer was already in the corpus, cited once,
> for something else.

#### Five reviews, five blockers, and every one was made by the previous fix

This is the shape worth copying, because it is not carelessness and it did not stop when
it was noticed.

1. Round 1 added a door from the `#pituitary` region entry, on the sound reasoning that
   the sellar reader is the likeliest driving reader.
2. Round 2 found that the door aimed that reader at a sentence saying the loss is *"most
   often the same side in both eyes"* — true of brain tumor patients as a group and
   **false for exactly that reader**, whose loss is the outer edge on both sides, which
   `/tumors/craniopharyngioma` and `/tumors/pituitary-tumor` both say in their own words.
   The source carves it out in its very next sentence.
3. Round 2's fix named both patterns. Round 3 found that **every driving-evidence sentence
   below it is HOMONYMOUS-scoped in the source** — *"participants with similar amounts of
   homonymous field loss"*, *"Some drivers with homonymous field defects have been rated as
   safe to drive"* — so describing a second pattern silently broadened them to a population
   nobody studied, in the **reassuring** direction, and *"the side you cannot see"* was
   singular for a reader who has two.
4. Round 3 re-scoped the evidence and moved the sight paragraph on `/seizures/living-with`
   up to sit under the state lookup. Round 4 found the clause it was rewritten with —
   *"what you can see RATHER THAN seizures"* — is exclusive, so the reader with **both**
   read it, concluded the paragraph was about somebody else, and used a seizure lookup.
5. Round 4's positional guard for that move could not fail (below). Round 5 found the
   **correction** to the permissive reading sitting AFTER the outbound link.

> **A FIX'S BLAST RADIUS IS EVERY SENTENCE THAT DEPENDED ON THE OLD SCOPE.** Each round
> changed what a passage was about and checked the passage it edited. The question to ask
> after a content fix is not "is this paragraph right" but "what did this paragraph used
> to mean for the ones around it".

And the shape of the last one generalises past this item:

> **A CORRECTION PLACED AFTER AN EXIT IS A CORRECTION THE READER CAN MISS.** Pinning a
> safety sentence's STRING is not pinning its PLACE. Where a page states a permissive
> fact, corrects it, and links away, the order of those three is a safety property and
> has to be asserted as one.

#### A positional assertion is only as good as its two landmarks

The guard written to lock that paragraph in place was wrong **twice**, and the break
harness caught it both times.

- First it searched the whole raw file for `Epilepsy Foundation` — which begins five
  source `title:` lines in the FRONT MATTER. So the landmark resolved to byte ~300 and the
  assertion was true wherever the paragraph sat.
- Then it bracketed with `## Work`, the next heading. `lookup < sight < work` is satisfied
  anywhere in the last two thirds of the section, **including the exact place the item had
  moved the paragraph out of**.
- It brackets the DOOR now as well as the lead sentence, because the regression that
  defeated version two was to split the paragraph and move only the link.

> **Bracket the thing that has to be adjacent, not the section it sits in.** And the
> reason all three versions shipped green: **a guard with no mutation aimed at it has been
> reviewed, not tested.** No mutation moved that paragraph until round 4 asked for one.

#### A floor is a constant and the thing it measures is not

The sight section's per-sentence floor was raised **five times** — 45, 50, 54, 58, 62 —
and every raise but the last was loose again within a round, because each readability pass
SPLIT sentences and grew the section (53 → 57 → 61 → 63). The same thing happened to the
route-count floor, which was written at eleven in the review round that created the
twelfth.

> **Run the break harness after the LAST prose edit.** A floor raised against an earlier
> draft has been raised against nothing. This item learned it five times and the harness
> reported it five times; what finally helped was a one-second check
> (`check-mutations.py`) that runs the mutators alone, with no `dotnet test`, so drift is
> found before a four-minute run is worth starting.

#### A quotation is not exempt from a ban on the claim it quotes

The corpus records a refused claim by pasting it verbatim into the front matter. **That
collides with any guard that reads the whole file**, and three of this item's guards do.

- `SeizureContentTests.NoCuratedPagePrintsADrivingWaitingPeriod` reads the RAW file and
  splits per sentence on `driv`. It went red on this item's own note, which had quoted
  CRUK's waiting-time sentence in order to record that it is barred. **The guard was not
  touched.** The note names the shape instead.
- The same floor then caught the item **twice more**, on two other pages, after it had been
  written down — once on a `horizontal field extent` quotation and once on a hyphenated
  description of the same phrase.

> **A rule stated in one file is not applied in the next.** The item that writes the rule
> is the one most likely to break it again, because it believes it has dealt with it.

The barred figures are DESCRIBED rather than quoted, which also closed two **modified
quotations** `/review` found: the source prints an EN DASH (U+2013) in its percentage
range and a DEGREE SIGN (U+00B0) in its field threshold, and the note had typed a hyphen
and the word *degree*. Both characters are NAMED here rather than pasted, per §12.20.

#### A haystack that normalises cannot report a normalisation defect — and this time the normaliser was ours

§12.20 records this about an aggregator's copy of a paper. **It is equally true of this
item's own extractor.** `totext.py` replaces U+2019 with an ASCII apostrophe, so two
rounds of quotation checking against `fitness.txt` could not see that *"the patient's
ability to compensate"* had been typed with the wrong character. Round 3 found it by
checking the raw HTML. The quotation is SPLIT either side of the apostrophe rather than
retyped, which is §12.20's number-unit rule applied to a character boundary.

**And a nit fix created the same defect class in the round that fixed two of them:**
expanding `FTD` to `FTD (fitness to drive)` put an unmarked editorial insertion INSIDE
quotation marks. A gloss belongs outside the marks, for the same reason emphasis does.

#### The obligation was on four pages, not one, and three of them routed nowhere

The backlog says `/tumors/craniopharyngioma` carries *"the one"* sentence tying vision to
driving. **A sweep found four.** `/tumors/pituitary-tumor` carried almost word for word
the restatement this item deleted from craniopharyngioma, and it is the page the SELLAR
reader most likely starts on — the same reader the new `#pituitary` door sends into the
new section. `/tumors/hemangioblastoma` and `/tumors/cns-germ-cell-tumor` carried their
own; the third had a route that was **conditional on seizures**, so the reader whose
problem was sight was excluded by the sentence that looked like it was helping.

> §12.19: **assert the COMPLEMENT, not the includers.** No single page was individually
> wrong, so no page-scoped test could see the gap.
> `EverySightAndDrivingSentenceInTheCorpusCarriesARoute` is that assertion, and it carries
> a second property the item needed three rounds to add: **a carrier owes the reader an
> INSTRUCTION as well as a door.** Two of the four pages said sight bears on driving and
> stopped there.

**And the count is not written down anywhere.** Two docstrings each stated how many pages
route to the driving anchor; they gave two different numbers and both were wrong by a
factor of three. Their replacements then said *"the enumeration says eleven"* — in two
files, about an enumeration that existed in neither. It exists now.

#### A third configuration of §12.19's zero, and a guard that catches its own author

§12.20 names two ways the ranking guard can be structurally silent:
`/treatments/craniotomy` has zero ADDRESSES with twenty-one difficulty matches; WI-571's
section has zero DIFFICULTIES with several addresses. **This section has zero of both** —
the weakest possible silence, and the likeliest to be read as a clean bill. It is a
property of the subject rather than an accident: the section is about a SYMPTOM and what
to do about it, and the one sentence that could have keyed a place to it was deliberately
not written. Both zeros are asserted and a row carrying both halves is planted.

**AND NAMING A SET IS ONLY STRONGER THAN COUNTING IT IF THE NAMES COME FROM THE
MEASUREMENT.** The carrier floor was changed from a count of five to five named slugs —
and the first version of the list held six, the sixth added from memory. The assertion
written to stop a count being satisfied by *any* five caught its own author on the first
run. (Naming fixes "any five"; it does **not** fix the fact that on two of those pages the
route sentence is a carrier by itself, because the shared route label contains *sight*.
That residue is recorded rather than papered over.)

#### Trap 5 landed four times, and never once through a heredoc

The recorded form is *a heredoc eats backslashes, so `\b` arrives as a literal 0x08*.
This item used the Write tool throughout and hit it anyway: **a plain Python
triple-quoted string does the same thing**, because `\b` is a valid Python escape while
`\s` and `\d` are not and survive with a warning. A duration regex was emitted into C#
with two real backspace bytes where it needed a word boundary, and it could not match
anything.

> **What caught it was the POSITIVE CANARY.** `Assert.DoesNotMatch(duration, section)`
> passed happily — a pattern that matches nothing never fires — and the test would have
> reported "no waiting time in this section" on the strength of a dead regex. §12.18: a
> guard that has never been seen to fail has not been shown to work. **Write both canaries
> beside every scan, and write them as literals so no page mutation can silence them.**

#### The needle rule, extended past smoke needles

This corpus has now been bitten five times by a needle that came from somewhere other than
the rendered artifact — WI-568 a draft, WI-570 a line break before a bolded word, WI-571
sentence-initial CASE, and **twice in this item**: a test assertion retyped from the
sentence it used to be in, and an instruction needle broken by a hard wrap.

> **The rule is not about smoke. Any string a check looks for must be copied out of the
> artifact the check will read.** WI-573 puts its needles in one file that both the
> rendered read and the smoke import, and that file asserts every needle against the local
> capture before either tool may use it — so a typo fails against a file on disk instead of
> against production.

#### Two smaller things worth carrying

- **`open(path, "wb")` truncates before the arguments are evaluated.** A line-ending
  detector inlined into the write call read a file that had just become empty and reported
  LF for every CRLF file. Compute the ending first. Invisible in `git diff` under
  `text=auto`, which is why it survived.
- **A refusal can be too broad.** A smoke refusal on `Either way` fired on pre-existing
  innocent prose three hundred lines from the edit. Refuse the CLAUSE that was removed, not
  the connective it happened to start with — §12.18's qualify-do-not-ban, in a scratch tool.

### 12.22 The line nothing graded, and the two ways a fix can switch a gate off (WI-575)

§12.17 to §12.21 are about what a page says. This is about the one line most readers
actually read, which no guard could see: the front-matter `description`.
`ContentPage.cshtml` renders it as the **first paragraph under the heading** and
`ContentStore.SearchPages` renders it again as the **blurb under every search hit** —
and `ContentChecker` graded `page.Markdown`, the composed body, while
`CuratedPage.ReaderText` strips the front matter by design. Four items fell through the
gap: WI-524, WI-528, WI-567 (which paid a `/review` blocker that survived three rounds
inside that one line) and **WI-573, twice in one item**.

#### The hole was real and the corpus fell through it in exactly one way

Swept with the widened text, **all 55 headlines are clean on every rule the corpus
enforces on its body** — no British form, no em or en dash, no smart quote, no
non-breaking space, no percentage, no Roman-numeral grade. Every numeral is legitimate
(WHO grades in Arabic, *"about 1 in 4"*, and `H3 K27`, which is a mutation name and the
case a digit ban gets wrong for a reason that has nothing to do with grades).

> **So the pain those four items felt was GUARDS NOT READING the description, not the
> description being wrong.** That is worth measuring before deciding what to build: the
> fix was structural — one promoted helper and one corpus sweep — and not a content job.

**The one rule nothing applied is the one that finds something, and it finds a lot.**
41 of 49 gradeable descriptions read above the 6.0 limit; median 8.4, max 19.7. The cause
is **not vocabulary** — the words are simple (*"Why you were put on one"*, *"what the
drug does to sleep, appetite, mood"*). **45 of the 55** are a contents list in one or two
comma-spliced sentences, 15 of them a single sentence, and the longest sentence runs to a
median of 27 words across the corpus (28 across the gradeable 49) and a maximum of 51.
Flesch-Kincaid's words-per-sentence term is doing all of it. That is **WI-578**, raised with the measurement attached so it is sized rather
than guessed.

#### Reported, not gated — and a non-gate is not nothing

Gating at 6.0 today would fail the build on 41 pages, and a gate that does that becomes
the thing people route around. The grade is reported at **Warn** (it was Info for a
round, which renders as `ok` and discharged *"says out loud"* by printing `ok` beside the
worst line in the corpus), and **drift is gated** by two numbers that catch different
regressions:

- the **count** over the limit, because the corpus may not grow the backlog; and
- the **worst grade**, because a count cannot see one description getting worse — push
  any of the 41 from 8.4 to 40.0 and the count does not move.

A ceiling at today's maximum gates nothing *as a value ratchet*; as the **other half** of
a count it gates the regression the count is blind to. Both fall on their own as WI-578
lands, because both are upper bounds.

> **And the down-count names the too-short tally beside it.** A description that drops
> under the 25-word grading floor stops being graded rather than getting better, so
> "40, down from 41" and "40, because one got shorter" must not read the same.

#### TWO WAYS A FIX SWITCHED THE GATE OFF, and both were invisible

This is the part to copy, because the gate was dead twice and the suite was green both
times.

**1. A value round-tripped through a human-readable string.** The grade was recovered by
parsing it back out of the finding's message. The message was formatted with
`CurrentCulture`; the parser used `InvariantCulture`. On any comma-decimal locale
*"reading grade 8,4"* matched nothing, every description scored −1, the count became 0,
and ContentCheck reported *"0 above the limit, down from 41"* and exited 0. Forever.

> **A value with two representations has one that depends on the machine.** The
> defence written for the round-trip — *"one place produces it, one place parses it"* —
> was true and still wrong, because the two places disagreed about culture. The fix is
> not an invariant format string; it is to stop round-tripping. The grade rides on the
> record.

**2. And the fix for that switched it off a different way.** The new field was populated
in exactly one producer. The ratchet's precondition asked for findings that *have* a
grade and are *not* descriptions — which, with `GradeFinding` still using the
three-argument constructor, is the empty set. So `pagesWalked` was always 0, and the
count, the ceiling, the silent-death failure and the progress line were all dead on the
real corpus.

> **THE FIX FOR A SILENTLY DISABLED GATE SILENTLY DISABLED IT A SECOND WAY, and seven
> tests written in the same round stayed green through it** — because each built a
> synthetic input whose body rows carried a grade the real producer never sets.
> **A test builder that does not match its producer tests the builder.** Branch coverage
> with a hand-made input proves the branches, not the wiring.

The fix is to stop *inferring* a fact the caller knows: `CheckAll` counts the pages it
walked and passes the number. And one end-to-end test now runs the real `CheckAll` over
the shipped corpus and asserts the ratchet was reached — the single assertion that would
have caught both rounds, and the thing none of the seven could.

#### A normaliser upstream of a check destroys its subject

This landed **twice in one item**, in two different normalisers, and the break harness
caught both.

- **`Flatten` is `\s+` and .NET's `\s` matches U+00A0**, so flattening replaced the
  non-breaking space the typography check hunts for with a plain space. The check was
  green because its subject had been erased upstream.
- **Both corpus sweeps replaced `\r\n` with `\n` before calling the promoted helper**,
  so the only two whole-corpus callers were immune to the `\s*$` CRLF trap the helper
  exists for — while a comment claimed they were that coverage.

> **Ask what a check's subject IS. If it is a character or a line ending, nothing may
> normalise the text on the way in.** And the corollary for the coverage claim: reading
> the raw bytes exercises the anchor **on a CRLF checkout only** — a Windows working
> tree, not the Linux CI runner. That is real coverage and not universal coverage, and
> the ending suite is what covers the other case.

#### What the anchor is load-bearing for, measured

The `\s*$` in the promoted regexes is the lesson twenty-three hand-written parses were each narrating in
their own words. Replacing it with `"$`:

| corpus | result |
|---|---|
| fully **LF** | the whole suite passes |
| fully **CRLF** | **208 failures** |

> **The defect is completely invisible on one line ending and catastrophic on the other.**
> That is why the ending suite is not optional on this corpus, and it is the number the
> promotion is worth.

#### The promotion, and what "byte-identical" was right about

The backlog said nineteen **byte-identical** copies of a private `Headline` property.
Whitespace-normalised there are **five** distinct variants; with comments stripped there
is **one implementation, nineteen times over**. The claim was right about what matters and
wrong as stated, and the five different narrations of one CRLF lesson are themselves the
argument for promoting it.

Counting every hand-written front-matter parse rather than just the properties, the first
pass removed all of them but one — and the survivor was found by `/review` in a file whose
own `Headline` had already been promoted, which is §12.20's *correcting a claim in one file
does not correct its copies* in a file the correction had touched. **Zero remain.**

> **The promotion took the BEST version, not the most common one.** Nineteen copies
> asserted the title and description together with no context; the twentieth call site
> asserted them separately with the page's slug in each message. A promotion that keeps
> the weaker form because it had more copies is a vote rather than a decision.

#### Two smaller things worth carrying

- **A set defined by "every finding that looks like X" is redefined by every new kind of
  X.** `EveryShippedPagePassesTheReadabilityGate` broke for the third time this way —
  WI-505 added glossary grades, WI-575 added description grades — and each fix added
  another exclusion. It is defined positively now: a finding whose file is exactly a page
  path. And its set-difference assertions compare against the **directory**, not against
  another 55-element subset of the same 55 files, which is what the previous pair did and
  why they could never fail.
- **A ban list can be too broad in a scratch tool too.** A smoke refusal on `Either way`
  fired on innocent pre-existing prose three hundred lines from the edit; a bare Roman
  numeral fires on `IV` meaning **intravenous**, which is live on
  `/treatments/chemotherapy`. Refuse the clause, not the word it happens to start with.

### 12.23 Splitting a sentence is not shortening it, and a sliced item needs a third number (WI-578, slice 1)

§12.22 made the front-matter `description` graded for the first time and found 41 of 49
gradeable ones above the 6.0 limit, median 8.4, max 19.7. It raised **WI-578** with the
measurement attached. This is WI-578's **first slice**: all thirteen descriptions under
`treatments/`.

#### The diagnosis held exactly, and that is worth recording because it was a prediction

§12.22 claimed the cause was **sentence length and not vocabulary**. Slice 1 is the test
of that claim, and it passed without a single word of vocabulary work:

| | before | after |
|---|---|---|
| descriptions over 6.0 in `treatments/` | 12 of 13 | **0 of 13** |
| worst grade in the directory | **19.7** | **5.3** |
| median longest sentence | 33 words | **17 words** |
| words removed | — | **none** |

> **Every word count stayed equal or ROSE.** The only edits were a comma or a colon
> becoming a full stop, plus the connective word a new sentence needs — *and*, *And*,
> *That*, *it*. Flesch-Kincaid's words-per-sentence term was doing all of it, and
> dividing it by four was the whole fix.

And the corpus-wide too-short tally **never moved off 6**, which is the number that makes
the gain legible: §12.22 put it beside the down-count precisely so that *"29, down from
41"* and *"29, because twelve got shorter"* cannot read the same. It is the difference
between a rewrite and a truncation, printed.

#### THE GUARD THAT REFUSED A SHORTENING WAS THE SCRIPT'S OWN, and it fired

The edit script asserts `len(new.split()) >= len(old.split())` per description. On the
first run it **rejected `/treatments/proton-therapy`** at 50 → 49 words: turning
*"…your tumor, and the harder question…"* into *"…your tumor. The harder question…"* had
eaten the word *and*.

> **A sentence boundary that swallows a word is a shortening wearing a split's clothes,**
> and it is invisible by inspection — the prose reads better, the grade improves, and the
> description is one word closer to the floor where grading stops. The fix was to keep the
> word: *"And the harder question…"*. **Twelve of thirteen passed the guard; writing it
> was worth it for the thirteenth.**

#### A SLICED ITEM NEEDS A THIRD NUMBER, because the other two are blind to a swap

§12.22's ratchet is a **count** over the limit and a **ceiling** on the worst grade, and
it argues correctly that each catches what the other cannot. Slicing the item opens a
third hole that neither sees:

> **Fix a `tumors/` description in the same commit that lets a `treatments/` one regress
> to 9.0, and the count still reads 29 while the worst still reads 13.0.** Both halves of
> the ratchet green, and the shipped slice silently unwound. **A count locks in a TOTAL;
> it cannot lock in a SLICE.**

So `DescriptionsCleanDirectories` lists the directories a slice has finished, and every
description under one is gated at **Fail** — §12.22's promised Warn-to-Fail promotion
applied one slice at a time instead of all at once at the end. It is reported **per
page**, because when the news is which page broke, *“29 is still 29”* is not something
anybody can act on.

#### AND THE FIRST VERSION OF THAT GATE COULD BE UNWOUND BY TRUNCATION, SILENTLY

This is the part to copy, because it is the third item running in which a gate was green
while switched off, and this time the hole was **the one the code's own comment had been
warning about since WI-575.**

The gate was built inside `DescriptionRatchet`, over the ratchet's `graded` list — the
findings whose `Grade` is not null. **A description under the 25-word floor is not
graded.** So:

| what a truncation does | what the gate saw |
|---|---|
| description drops to 20 words | leaves `graded`, so the slice gate **cannot see it** |
| `DescriptionsOverTheLimit` | 29 → 28, which reads as **progress** |
| `WorstDescriptionGrade` | untouched |
| too-short tally | 6 → 7, inside an **Info** that `Program.cs` renders as `ok` |
| exit code | **0** |

> **ROW 2 IS CORRECTED BY §12.24, and read that before using this table.** The
> down-count is only available to a page OUTSIDE a finished slice. Inside one
> every description is already under the limit, so shortening one cannot lower
> `DescriptionsOverTheLimit` — it moves the floor tally and nothing else. This
> table ran the un-sliced case and the in-slice case together, and the prove
> scripts for BOTH slices then credited a truncation with a fall that the paying
> fix beside it had produced.

> **The slice was unwindable by shortening, with every gate green.** And
> `GradeDescription`'s own comment has said since WI-575 that *scoring it as progress
> would let a page buy its way out of grading by getting shorter*. The sentence was
> right, it was three lines above the floor check, and **the new gate was built on the
> wrong side of it.** Writing a hazard down does not guard it.

Worse, the only thing that actually caught a shortening during slice 1 was the
`len(new) >= len(old)` assertion in the item's own **git-ignored scratch script** — which
this section spends a whole heading praising, and which will not exist for slice 2.

The fix moves the whole decision **out of the corpus-level ratchet and into
`GradeDescription`**, per page, where it can see all three ways a description leaves the
graded set: over the limit, under the floor, and blank. Three further things fell out of
that move, and each was its own defect:

- **The log had been printing both verdicts for the same page.** The grade finding said
  `(not gated — WI-578)` and the ratchet's Fail two lines later said it was gated.
  That is §12.22's round-4 two-opposite-stories defect, recreated between a different
  pair of findings by the fix for a different hole. A message must never contradict its
  own level: `(not gated)` is now printed only where it is true.
- **The gate had been sitting behind two corpus-size early returns it does not depend
  on.** *“Is this page over the limit?”* needs no calibration against
  `CorpusWhenMeasured`, but the ratchet `yield break`s on a corpus smaller than 55 — so
  **deleting one page switched the slice gate off.** Per page, no corpus-level condition
  can suppress it.
- **Suppressing the parity Info threw away the instrument for diagnosing the very
  failure.** `tooShort` is how a truncation is recognised, and it was reported only in
  the two Infos that a regression suppresses. The totals are printed either way now, as
  a **verdict-free Warn**: facts with no "unchanged" or "lower the constant" framing
  cannot contradict a Fail.

> **ASK WHICH SET A GATE IS BUILT OVER, AND WHAT LEAVING THAT SET LOOKS LIKE.** A
> ratchet over graded findings cannot see a page that stops being graded, and "stops
> being graded" was the cheapest way to break the thing it guards. §12.22 said the same
> about `descriptions.Count` vs `graded.Count` one item earlier, for the whole-corpus
> instrument; this is the identical question asked of a per-directory one, and it was
> not asked.

Two things about it generalise:

- **A clean-directory list is a claim about the corpus that lives in a constant**, so the
  corpus is what checks it. `EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus` runs the
  real `CheckAll` and asserts both halves: every description under a listed directory
  passes, **and the directory is non-empty**. A typo like `treatment/` would otherwise
  leave the test iterating an empty list and passing — §12.19's *a guard measured on a
  page it cannot fire on has been measured on nothing*, in the form a `foreach` makes easy
  to ship.
- **The passing line had to be suppressed too.** A swap holds the count at parity, so
  without gating the Infos on the regression list the run would print *"both unchanged
  since this ratchet was last lowered"* directly beside a Fail naming the page that just
  broke. That is §12.22's round-4 defect — two findings telling opposite stories —
  recreated between a different pair of branches by the fix for a different hole.

#### AND A MUTATION AIMED AT A TEST THAT BUILDS ITS OWN INPUT TESTS NOTHING

The break harness earned its run twice in this item. The second time it reported the
truncation mutation as a **survivor on both line endings** — which read as a hole in the
gate that had just been built to close exactly that hole.

It was not. The mutation named
`TruncatingADescriptionInsideAFinishedSliceFailsInsteadOfGoingUngraded`, which drives a
**synthetic page through `CheckPage`**. Editing a shipped page cannot reach it. The guard
that actually sees a truncated page on disk is
`EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus`, whose
`onDisk == inDirectory.Count` assertion exists for precisely this case: thirteen pages in
`treatments/` must produce thirteen GRADED descriptions, and a truncated one produces
none.

> **This is the MIRROR of §12.22's builder/producer lesson.** There the input was wrong
> for the producer, so seven tests passed over a dead gate. Here the target was wrong for
> the input, so one live gate reported as dead. Both are the same question asked from
> opposite ends: **does this test actually see the thing this mutation changes?**

A unit test that constructs its own page is not a defect — it is how the in-slice and
out-of-slice branches get compared with the PATH as the only difference. What is a defect
is pointing a content mutation at one and reading the result as evidence about the
corpus. Every such test is now listed in the mutation table's header with its real-corpus
counterpart named beside it, which is the same discipline §12.22 applied to the guards no
page mutation can reach.

#### A PROBE FOR "THE NUMBER THE OTHERS CANNOT SEE" MUST LAND INSIDE THE BAND THEY ACCEPT

The script that proves the new gate on the real corpus failed its own check first. Its
regression un-split the whole description into one sentence, which grades **17.6** — above
the 13.0 ceiling — so the run failed twice and proved nothing about the directory gate.
Merging only two of the four sentences grades **8.3**: above 6.0, below the ceiling, and
paid for by a fix elsewhere so the count holds at 29.

> **A mutation aimed at the gap between two guards has to fit in the gap.** One that also
> trips a guard either side of it demonstrates the guard either side of it. The run is
> only evidence for the new number when it is the *only* thing that fired — which is why
> the prove script asserts the count did NOT fire and the ceiling did NOT fire, rather
> than just asserting a non-zero exit.

#### The messages stopped naming the item that measured them

Three of the ratchet's messages said *"when WI-575 measured them"*. That was true for one
item and goes stale on **every slice** — and a message a reader of the log acts on is the
worst place to keep a fact with an expiry date. They say *"when this ratchet was last
lowered"* now, and the WI attribution lives in the constants' docstrings, where lowering
the number and updating the provenance are the same edit.

#### What is left, and what the next slice should take

**29 remain: 8 in `tests/`, 21 in `tumors/`.** Slice 1 was `treatments/` on purpose — it
held the top *two* grades (19.7 and 18.5), so one slice moved **both** halves of the
ratchet, 41 → 29 and 19.7 → 13.0.

> **Pick the next slice by which number it moves.** `tests/` holds the new worst
> (13.0, `/tests/getting-ready-for-surgery`) and is 8 pages; `tumors/` is 21 pages whose
> worst is 11.1. A slice that moves only the count leaves `WorstDescriptionGrade`
> measuring a page nobody touched, and §12.19 is the reason that matters.

The full Warn-to-Fail promotion in `GradeDescription`, and deleting the `(not gated)`
marker, happen when the last of the 29 lands — not before.

### 12.24 Which entries a guard can fire on, and the harness that could not see its own mutation (WI-578, slice 2)

WI-578's **second slice**: the eight over-limit descriptions under `tests/`, chosen
over the 21-page `tumors/` because `tests/` held the corpus's worst (13.0,
`/tests/getting-ready-for-surgery`) and so moves **both** halves of the ratchet.

| | before | after |
|---|---|---|
| descriptions over 6.0 in `tests/` | 8 of 10 | **0 of 10** |
| worst grade in the directory | **13.0** | **5.2** |
| corpus count over 6.0 | 29 | **21** |
| corpus ceiling | 13.0 | **11.1** |
| corpus too-short tally | 6 | **6** |
| words removed | — | **none** |

The diagnosis §12.22 made and §12.23 confirmed held a second time, and more
sharply: **five of the eight were a SINGLE sentence** of 28 to 35 words. Nothing
but the sentence boundaries changed, every word count stayed equal or rose, and
the too-short tally never moved off 6 — which is the number that distinguishes a
rewrite from a truncation, printed beside the gain for exactly this purpose.

#### THE SLICE THAT MADE "SPLIT, DO NOT SHORTEN" LOAD-BEARING RATHER THAN A SLOGAN

Slice 1's descriptions ran 38 to 56 words, so a careless split had room to lose a
word without approaching ContentCheck's 25-word floor. Slice 2's did not:

> `/tests/getting-ready-for-surgery` started at **28 words against a floor of
> 25**, and `/tests/waiting-for-results` at 30. A "fix" that dropped three words
> would not have improved the page — it would have taken it **out of the graded
> set entirely**, which since §12.23 is a Fail inside a finished slice and was a
> silent down-count before it.

The guard that refuses a shortening fired zero times here, and that is the right
outcome to record beside §12.23's one rejection: the constraint was known going
in, so the drafting aid printed the floor word count next to the grade on every
candidate.

#### ASK WHICH *ENTRIES* A GUARD CAN FIRE ON, NOT JUST WHICH PAGES

This is the slice's real finding, and it is §12.19 asked one level up.

`DescriptionsCleanDirectories` went from one entry to two. Every test of the
finished-slice gate built its probe path from `DescriptionsCleanDirectories[0]`,
so on the day `tests/` was added, *"an over-limit description inside a finished
slice fails"* was **proved for `treatments/` and assumed for `tests/`**.

And the shipped-corpus test could not make up the difference:

> **A directory that PASSES looks identical gated or ungated.** The grades are
> the grades. Narrow the matcher to the first entry and every description under
> `tests/` still grades 5.2 or better, so every assertion in
> `EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus` — including its
> `onDisk == inDirectory.Count` non-vacuity check — stays **green with the gate
> switched off for half the list**.

Two changes close it. The three in-slice Fail probes became `[Theory]` over
`FinishedSlices`, so each listed directory is proved rather than the first one.
And the one observable difference on a clean corpus — the `(gated — a finished
slice)` marker — became the subject of its own test,
`EveryFinishedSliceIsActuallyGatedOnTheShippedCorpus`. It is a separate test
because it is a separate claim: *these directories read at sixth grade* and
*these directories are gated* fail for different reasons and should not share a
name. Both markers are named constants now, for the reason `DescriptionMarker`
is: a test that types the message tests its own typing — and the new test asserts
the two do not overlap, because its whole claim is *the marker is the only
observable difference* and that claim dies quietly if one marker contains the
other.

> **AND ONE MUTATION AIMED AT THE NEW CORPUS TEST PROVES THE NEW CORPUS TEST.**
> /review: the `[Fact]` → `[Theory]` conversion is the change this section is
> named after, and the first version of the mutation table pointed the narrowing
> mutation only at `…IsActuallyGated…`. The same narrowing is aimed at
> `ADescriptionOverTheLimitInsideAFinishedSliceFails` as well now, where the
> `tests/` case gets a Warn where it asserts a Fail **while the `treatments/`
> case still passes** — which is exactly the half-proof the conversion exists to
> remove, made visible.

And a third branch had the same shape of hole, found in the same pass. Of
`GradeDescription`'s three failing branches — over the limit, under the floor,
blank — the first two each had an out-of-slice control pinning the *un*-gated
level. **The blank one had none**, so promoting its out-of-slice `Warn` to `Fail`
left the entire suite green: §12.19's rule broken in the one place three sibling
branches made it easy to miss, because two thirds of the pattern was there.
`TheSameBlankDescriptionOutsideAFinishedSliceOnlyWarns` is the control, and that
promotion is a mutation in the table now.

Writing it turned up a fourth thing. The shared failure suffix ended *"the cause
is almost always sentence length: SPLIT the sentence rather than shortening it"*
and was appended to the **blank**-description failure — advice about a sentence
that does not exist, on a page that has no description at all. The shared part is
now only what is true of all three branches, and the advice is added by the two
that have a sentence in them. `NotGatedMarker` is also
the thing WI-578's acceptance criteria say to delete at the end, and as a
constant that deletion is a compile error at every site rather than a search for
a parenthetical.

#### AND THE BREAK HARNESS COULD NOT SEE ITS OWN MUTATION, measured rather than argued

The gate reaching the second entry cannot be broken by editing a page — only by
narrowing the matcher — so the mutation had to target `ContentChecker.cs`. The
harness takes an arbitrary path and restores it byte-for-byte, so that looked
free. It was not:

> Every mutation this harness has applied in eight items edited a **markdown**
> file, which `dotnet test --no-build` reads off disk at runtime. A **.cs**
> mutation is *compiled*, not read. Applied to disk and run with `--no-build`,
> the binary under test is the **unmutated** one — the named guard passes in
> 632 ms and the harness logs a **survivor**.

That is a hole in the harness wearing the costume of a hole in the gate, and it
is the third consecutive item in which the instrument, not the subject, was the
defect. `rebuild()` now runs whenever a mutated path ends in `.cs` — after
applying, after restoring, after each line-ending flip, and once in the `finally`
so a mutated assembly is never left in `bin/`. A mutation that fails to compile
exits rather than scoring, because a broken mutation is not a caught one.

> **The number proving this was obtained by doing it**: mutate on disk, skip the
> build, run the one test, watch it pass. §12.23's lesson was that writing a
> hazard down does not guard it; this one's is that a harness inherited across
> eight items carries assumptions about its *targets* that nobody restated when
> the target class changed.

#### Two of the eight needed a second pass, and both were about the reader

The split is mechanical; what a split *does to a pronoun* is not.

- `/tests/molecular-markers` first read *"What each gene test on a brain tumor
  report measures. What your team uses **it** for…"*. The new full stop put a
  sentence boundary between that *it* and *each gene test*, leaving *a brain
  tumor report* as the nearest noun. **This is the same defect /review caught in
  `/treatments/proton-therapy` one slice earlier**, created the same way — by a
  boundary, not by a word choice — which is worth recording because it means the
  hazard is a property of splitting rather than of either page. *"what your team
  uses **each one** for"* carries the antecedent inside the sentence and costs a
  word rather than saving one.
- `/tests/getting-ready-for-surgery` first read *"The checks before a brain
  operation. Blood tests, a heart trace, and the anesthesia visit."* The colon
  that used to attach the list to the sentence before it was gone, and the
  attachment went with it. *"These are blood tests…"* puts it back for one word.

> **A split can lose something that is not a word.** The script's guard counts
> words, and both of these passed it: the clause survived, the count rose, the
> grade fell. What went missing was the *link* a colon or a nearby antecedent was
> carrying — invisible to any count, and exactly the kind of thing an audience
> reading sentence by sentence pays for.

#### THE PROBE BAND NARROWS EVERY TIME THE CEILING RATCHETS

§12.23 ruled that a probe for the gap between two guards must fit in the gap.
Slice 2 is the demonstration that the gap **moves**: the ceiling fell 13.0 → 11.1
with this slice, so the band is `(6.0, 11.1]` and strictly narrower than the band
slice 1's probes were drawn in.

Two things followed, both caught by measuring instead of eyeballing:

- The harness's regression mutation was first aimed at `follow-up-scans` with a
  hand-written comment claiming grade 9.3. It graded **6.2** — in band by 0.2.
  Worse, that page *cannot* do better: its words are short, so its grade is
  carried almost entirely by sentence length, and every consecutive merge of its
  sentences grades 2.7, 3.8, **6.2** or 13.2 — too mild, barely over, or over the
  ceiling. (12.9 is its original *colon* text, which is not a comma merge of the
  shipped one.) Retargeted at `ct-scan`, whose **pre-slice text grades 8.8**,
  mid-band.
- **The cheapest in-band probe is the text the slice replaced.** Both the harness
  mutation and `prove-slice-gate.py`'s regression are now simply the description
  as it shipped the day before, which needs no invention and is in band by
  construction for any page whose old grade was under the new ceiling.

`prove-slice-gate.py`, retargeted to `tests/`, still proves both halves on the
real corpus: a swap (`/tests/pathology-report` back to its 10.9 pre-slice
sentence, paid for by splitting `/tumors/ependymoma` 6.1 → 3.6) holds the count
at 21 and the ceiling at 11.1, and the run fails **only** on the directory gate,
naming the page.

#### AND THE TRUNCATION SIGNATURE §12.23 DESCRIBED IS NOT AVAILABLE INSIDE A FINISHED SLICE

The first version of this slice's prove script printed
`20 above the 6.0 limit (recorded 21) … 7 under the 25-word floor` and called it
the signature of a truncation: the limit count falling while the floor count
rises. **/review found that the fall was the ependymoma FIX, still applied from
phase 1, and not the truncation at all.** Measured with nothing else changed, a
truncated page reads `21 above … 7 under`. The count does not move.

> **It cannot move.** Inside a finished slice every description is ALREADY under
> the limit, so shortening one cannot lower the over-the-limit count. The
> "reads as progress" half of §12.23's table belongs to the **un-sliced
> backlog** — where a shortening still lowers the count, still reads as a gain,
> and still is not gated. §12.23 printed both halves as one table and so told a
> story about a page in a finished slice using a number only an un-gated page can
> produce. **In a gated directory the floor count is the entire instrument**,
> which is the real reason the totals print on a failing run.

The tool's own guidance sentence carried the same error — *"a truncation shows up
here as the floor count RISING while the limit count falls"*, in the Warn that
prints **only** when a finished slice has failed, which is exactly the run where
somebody is diagnosing one. A message a reader of the log acts on is the worst
place for a causality nobody checked. It names the floor count alone now and says
why the limit count will not move.

> **This is the third item running in which the instrument, not the subject, was
> the defect** — and the second time in two slices that a number read as proof of
> one thing and was produced by another. §12.23's lesson was that writing a hazard
> down does not guard it. This one's is narrower and meaner: **a probe that
> changes two things cannot attribute what it measures to either.** One edit at a
> time, or say which edit the number belongs to.

#### What is left

> **"THREE THINGS" WAS THE WRONG COUNT AND ONE OF THE THREE WAS WRONG AS WRITTEN —
> see §12.25 before acting on the paragraph below.** `GradeDescription`'s Warn
> branch did not go: only the GRADE's did. The directory list covers 46 of the 55
> descriptions, not all of them, and the two branches that gate a description
> LEAVING the graded set still need their out-of-slice level — six descriptions are
> legitimately under the word floor and every one is outside every listed
> directory. Deleting all three branches, as this paragraph reads, fails the build
> on six pages WI-578 was never about.

**21 remain, and all 21 are in `tumors/`.** The next slice is the last one, which
makes three things one edit rather than three: the count reaches 0, the ceiling
goes, and `GradeDescription`'s Warn branch and the `(not gated — WI-578)` marker
are deleted — the full promotion to Fail that §12.22 promised and §12.23 deferred.
`tumors/` is 21 pages and its worst is 11.1 (`hemangioblastoma` and
`pediatric-brain-tumor`, **tied** — which is why `WorstDescriptionGrade` is a
ceiling and not a page name). It is worth splitting again if it does not fit one
sitting; the ratchet supports that, as long as whatever lands next lowers the
ceiling rather than only the count.

### 12.25 The set a gate was never built over, and a list that was its own oracle (WI-578, slice 3)

WI-578's **third and last slice**: the 21 over-limit descriptions under `tumors/`,
and with them the promotion §12.22 promised, §12.23 deferred and §12.24 scheduled.

| | before | after |
|---|---|---|
| descriptions over 6.0 in `tumors/` | 21 of 23 | **0 of 23** |
| worst grade in the directory | **11.1** | **5.6** |
| corpus count over 6.0 | 21 | **0** |
| corpus ceiling | 11.1 | — (constant deleted) |
| corpus too-short tally | 6 | **6** |
| words removed | — | **none** |

The diagnosis §12.22 made as a prediction has now held three times for three
different directories. Forty-two descriptions rewritten across the item, **not one
word removed from any of them**, and the corpus-wide too-short tally read **6**
before the first slice and reads 6 after the last — which is the number that
separates a rewrite from a truncation, printed beside the gain for exactly this
purpose. The grade is gated at `Fail` corpus-wide, `DescriptionsOverTheLimit` and
`WorstDescriptionGrade` are deleted, and `DescriptionRatchet` is
`DescriptionCorpusReport` because nothing in it ratchets any more.

#### THE SET THE GATE WAS NEVER BUILT OVER, found before a line was written

§12.19 asked which **pages** a guard can fire on. §12.24 asked which **entries**.
This slice is the same question asked of the **complement**: *which pages are in no
entry at all?*

`GradeDescription`'s own comment said that when `tumors/` landed,
`DescriptionsCleanDirectories` would have "swallowed the whole corpus" and every
branch would be a Fail. It never swallows the corpus:

> With all three slices listed the list covers **46 of the 55** descriptions. The
> other **nine** are the seven root-level pages and the two under `seizures/` — and
> **six of those nine are under the 25-word grading floor today**: `terms.md` at 7
> words, `about.md` 8, `digest.md` 12, `privacy.md` 12, `how-we-write.md` 20,
> `seizures/what-to-do.md` 20.

So the promotion had to be **per branch, not wholesale**. Of
`GradeDescription`'s three failing branches:

- **over the limit** → `Fail` **corpus-wide**. This is the acceptance criterion, and
  it is safe because the count is 0 everywhere, not merely inside the listed
  directories.
- **under the word floor** → unchanged: `Fail` inside a listed directory, `Info`
  outside. Six live pages are legitimately short and all six are outside.
- **blank** → unchanged, for the same reason and for its control's sake.

**Deleting all three, as the plan read, would have failed the build on six pages
WI-578 was never about.** It is an item about descriptions that read ABOVE sixth
grade; `terms.md` being seven words long was never its subject. And the tally those
six produce is not a backlog — it is the stability signal the whole item's claim
rests on.

> **THE REASON THE PLAN WAS WRONG IS THAT IT NAMED A BRANCH BY ITS LEVEL.** "Delete
> the Warn branch" sounds like one thing. `Warn` was the out-of-slice level of two
> different branches with two different subjects, and only one of them was about the
> grade. **Ask what a branch is ABOUT, not what level it currently emits** — a level
> is shared by every branch that happens to be equally serious today.

`DescriptionsCleanDirectories` therefore survives the item, with its meaning
narrowed: it no longer gates the grade, it gates the two ways a description can
**leave the graded set**. That is the escape §12.23 found and it is invisible to any
grade gate, so it outlives the one that replaced the count.

#### AND THE LIST WAS ITS OWN ORACLE, which is the fifth silent-off in this item

`/review` found this, and it is the finding worth most. Every test of
`DescriptionsCleanDirectories` derived its expectation **from the list**: the
theories iterate it, both shipped-corpus tests `foreach` over it, `Assert.NotEmpty`
is satisfied at two entries, and the marker's negative control treats whatever is
unlisted as legitimately unmarked.

> **So deleting `"tumors/"` un-gated 23 pages with the whole suite green and
> ContentCheck exiting 0.** The theories lose a row rather than failing one. The
> `foreach`es iterate two directories, both of which still pass. And the 23 now
> unmarked `tumors/` lines move into the set the negative control expects to be
> unmarked. **A list that is both the subject and the oracle cannot be tested by
> itself.**

It matters more after this slice than before it. While the grade was gated per
directory, dropping an entry at least changed an over-limit page's **level**; now
that the grade is corpus-wide, this list's only job is a thing that is invisible on
a corpus where nothing has gone ungraded.

The fix is to assert the **converse**, computed from the corpus rather than from the
constant: *a directory whose descriptions already all pass must be listed.* It reds
on deleting an entry, and on finishing a directory and forgetting to add it — the
same omission from the other end — and it needs no maintenance when a slice lands,
because the only thing a new slice changes is which side of the test a directory
falls on. Exactly satisfied today: `treatments/`, `tests/` and `tumors/` qualify;
`seizures/` does not, because `what-to-do.md` is 20 words and therefore ungraded.

> **The mutation that proves it is a mutation the harness did not have.** Narrowing
> the matcher (§12.24's mutation) is caught because production output changes while
> the list does not. **Deleting the entry changes both**, which is why nothing
> derived from the list could see it.

#### THE SWAP PROBE RETIRED, and four single-edit phases replaced it

Both earlier slices proved the directory gate on the real corpus with a **true
swap**: a regression inside the finished slice **paid for** by a fix in the
un-sliced backlog, so the corpus count held at parity and only the directory gate
could fire. The last slice removes both halves of what made that work:

- **There is no backlog left to pay with.** The count is 0, so every regression
  raises it and no edit anywhere holds it at parity. (Both earlier slices used
  `/tumors/ependymoma` as the payer, and that page is inside this slice.)
- **And there is no count to hold at parity**, because the count constant is
  deleted.

So the isolation the swap bought — *only the directory gate fired* — is bought a
different way: **four phases, one edit each, and each says which guard its
assertions are evidence for.** §12.24's rule is then satisfied by construction
rather than by a paying fix, because no probe changes two things.

| phase | one edit | evidence for |
|---|---|---|
| 1 | `/where-your-tumor-is` merged to 10.8 — in **no** listed directory | **the promotion.** Before this slice the same run exited 0 with a Warn |
| 2 | `/tumors/hemangioblastoma` back to its pre-slice 11.1 | **the 21 splits.** *Not* the directory list — it fails on its grade and would fail identically unlisted |
| 3 | the same page **truncated** under the word floor | **the third entry.** The only phase whose verdict the list produces |
| 4 | the same truncation on `/where-your-tumor-is` — **outside** every entry | **the third entry discriminating.** Must exit **0** |

Phases 1 and 2 get the **identical** verdict on pages either side of the directory
boundary, which is what "corpus-wide" means and is not something a single phase can
show. Phases 3 and 4 are the **same edit in two places expecting different
verdicts**, which is §12.19 met per page on the real corpus rather than on a
synthetic one — and phase 4 is the one that keeps `terms.md` alive.

> **Phase 2 is the one worth naming out loud, because it is the phase that proves
> least.** It is the obvious probe — regress a page in the slice you just finished —
> and with the grade gated corpus-wide it is evidence about the *rewrite*, not about
> the directory list at all. A probe aimed at the thing you just built will tend to
> fire; what it fires *through* is the question.

#### THE PROBE BAND STOPPED NARROWING, because its upper edge was deleted

§12.24 ruled that the band a probe must land in **narrows every time the ceiling
ratchets**: 19.7, then 13.0, then 11.1. The ceiling **was** the upper edge, and this
slice deletes it — every value it could have gated is above 6.0 and fails on sight
now. So the band is `(6.0, ∞)` and only the lower edge is checkable.

Which leaves exactly one of §12.24's two failure modes live, and it is the one that
nearly shipped: **too mild.** Slice 2's first probe graded 6.2 against a 6.0 limit —
in band by 0.2 and by accident. The tooling still prints the number.

#### THE PROMOTION CREATED A CLIFF, and /review's own prose fix gave the fix a subject

A hard `Fail` at 6.0 with nothing below it meant a description at **5.9 printed
`  ok`** — and a one-word edit took it to a broken build with nothing in between.
The **body** gate has had a three-level shape over the same `WarnGrade` since
WI-414; the two gates now agree about what "close to the limit" means instead of
disagreeing by a whole level.

> **And it has a live subject on the day it lands, which §12.19 requires.**
> `/tumors/glioblastoma` reads **5.6** — because /review's second round moved it
> from 5.4 while fixing a pronoun. **A guard whose first subject is produced by the
> review round that asked for the guard** is a better outcome than one shipped at a
> threshold nothing reaches.

Its probe and its control are **the same 29 words with one comma made a full stop**:
5.9 and 4.0, no word added or removed. The pair is the item's own thesis used as a
fixture, so neither vocabulary nor length can be what moved the level.

#### A COUNT DEFINED BY SUBTRACTION, three lines under the comment forbidding it

The floor tally was `descriptions.Count - graded.Count - blank` — a set defined by
what it is **not** — and the paragraph immediately below it states §12.22's rule
against exactly that, for the *blank* count, which had been fixed for the same
reason one slice earlier.

> **And the number it produced is the one this file calls "the entire instrument"**
> for telling a truncation from a rewrite inside a gated directory (§12.24). A
> fourth ungraded branch would have inflated it in silence. It is matched positively
> now, against a shared constant, **and the three counts are asserted to
> partition** — which moves the failure mode from "one number is quietly wrong" to
> "the numbers do not add up", and only a check can say which.

The general form: **fixing one set defined by exclusion does not fix its siblings**,
even when they sit three lines apart and the fix's own comment is the rule.

#### A MESSAGE GOES STALE WHEN THE THING IT CONTRASTS ITSELF WITH REACHES ZERO

Three messages survived the promotion saying things that stopped being true, and
each is a different way for that to happen:

- **A contrast with an empty set.** `"…a REGRESSION rather than part of the
  remaining backlog"` was right for 41, 29 and 21 descriptions. At 0 it sends a
  reader looking for a list that does not exist.
- **A tense.** `"Every description in the corpus HAS READ at or under this limit
  since WI-578"` is a present-perfect claim printed on a finding that proves one
  does not, so it was false at the moment it appeared — and false a second way about
  the six that are not graded at all. The fact that is true on that run is the one
  about **when WI-578 finished**.
- **A `because` that outlived its guard.** §12.24 corrected the truncation-signature
  sentence and the corrected version was still conditional on *where it printed*: it
  printed only when a finished slice had failed, where every description really was
  under the limit. This slice made the totals print on **every** run — so
  `"the limit count will NOT move, because a page has to be under the limit before it
  can be shortened out of grading"` became a universal claim, and the clause after
  `because` is simply false. §12.23's "reads as progress" half was available again,
  inside the message written to deny it.

> **§12.24's lesson was that a message a reader acts on is the worst place for a
> causality nobody checked. This slice's is one turn further on: a causality that was
> checked is only checked FOR THE RUNS IT PRINTED ON.** Widen where a message prints
> and you have made a new claim. The sentence is conditional on `over == 0` now.

#### FIVE OF THE 21 SPLITS LOST SOMETHING A WORD COUNT CANNOT SEE

§12.24 recorded two shapes, from two pages, and predicted that 23 descriptions was
23 chances at them. `/review` found **five**, and they were the same two shapes plus
one that is new:

- **A pronoun whose antecedent moved to the previous sentence** — the third, fourth
  and fifth instance of the defect first caught on `/treatments/proton-therapy`.
  `/tumors/glioblastoma` was the sharpest: *"And **it** explains why **it** keeps
  coming back"* put two `it`s with two different referents in one short sentence,
  where the original had one. `/tumors/meningioma`'s second `it` was left with *the
  brain* as its nearest noun, so it read as a gap that shows the brain.
  `/tumors/hemangioblastoma`'s *"**It** is usually found low at the back of the
  brain"* followed *"full of tiny blood vessels"*.
- **A frame the colon was carrying** — `/tumors/pediatric-brain-tumor`'s list hung
  off *"when the person with the brain tumor is a child:"*, and one item standing
  alone became *"Why scans and radiation may need medicine to sleep"*, which says the
  scans need the medicine.
- **AND THE NEW ONE: A RATIO CAN LOSE ITS DENOMINATOR.**
  `/tumors/hemangioblastoma`'s *"the inherited condition linked to about 1 in 4"* was
  a clause inside a list; as its own sentence, **1 in 4 of what?** The page's own
  front matter says 1 in 4 **people with one of these tumors** have von
  Hippel-Lindau. *"1 in 4 of these tumors"* was refused as a fix because it is a
  **different denominator** — VHL patients often have more than one tumor — which is
  the kind of number a split can quietly re-base.

> **A split moves a clause out of the reach of everything that was qualifying it**,
> and a word count sees none of it: the clause survives, the count rises, the grade
> falls. Three slices in, the inventory is a pronoun, an attachment, a subject, and
> now a denominator. The common factor is that **the sentence a clause used to live
> in was doing work for it.**

Every one of the five fixes **added** words, and the split script's refusal of a
shortening was applied to them too — which matters because these are the corpus's
lowest-margin descriptions and a fix is exactly when a word goes missing.

#### A NEEDLE THAT INCLUDES A SENTENCE'S FIRST WORD BREAKS EVERY TIME A BOUNDARY MOVES

One test in the whole suite broke on the 21 rewrites, and the way it broke is worth
keeping. `CnsLymphomaPageRenderTests` asserted the rendered page contained *"**why** a
steroid can take the answer away"* — a clause that ran on from a comma. The split
made it the start of a sentence, so the only thing that changed was a capital W.

> The needle was the claim **plus the punctuation it happened to follow**. On a
> corpus where sentence boundaries are the thing being edited — three slices
> running — that is a needle that breaks for a reason unrelated to what it is
> guarding. It is the clause alone now.

#### What is left: NOTHING. WI-578 IS CLOSED.

All 55 descriptions read at or under 6.0, the worst is 5.6, the grade is gated at
`Fail` corpus-wide, and both upper-bound constants are deleted. What remains of the
machinery is the part no grade gate can replace: `DescriptionsCleanDirectories`
gating the two ways a description leaves the graded set, the floor tally that tells
a rewrite from a truncation, `CorpusWhenMeasured` asserting the sweep saw the whole
corpus, and the dead-instrument Fail.

> **The one thing to carry forward is the question this slice is named after.** Three
> sections have now asked a version of it and each found something: which **pages**
> can this guard fire on (§12.19), which **entries** (§12.24), and which pages are in
> **no entry at all** (§12.25). The third one is the one that catches a plan written
> as "and then delete the branch".

A per-sentence reading-grade check for BODY text still needs a corpus sweep before
it can be gated; a single unreadable sentence moved a page 5.6 → 5.7 in WI-567 and a
whole-page average cannot see it. That is unrelated to this item and still open.

### 12.26 A definition the reader meets twice, and the fix that was copied instead of shared (WI-576)

`CuratedPage.AssertDoesNotRestateTheCorpus` is §12.10's restatement probe. It walked
`pages/` and `blocks/` and never `glossary/` — raised by WI-567, which had just
shipped `glossary/frontal-lobe.md` whose definition was all but word-for-word the
page's own region entry. A glossary definition fires as a tooltip on every page that
says the term, so it has a block's blast radius, and §12.10's whole argument for
including blocks applies to it unchanged.

**The obvious fix was to widen the set, and the measurement said the set was not the
hole.** Asked in the order §12.19/§12.24/§12.25 ask it:

- **Which set is this gate built over?** `AllPages()` — and it is read by **nine
  call sites in seven files**, of which exactly one is the restatement probe.
  Widening it would have moved eight other guards' subject sets with nothing
  saying so. It was left alone.
- **Which entries can it fire on?** Nineteen. Only 19 of the 55 pages call the
  probe, so **36 pages are in no entry at all** — §12.25's third question, and this
  time it decided the whole design.
- **And the answer that followed:** of the eight real restatements in the corpus,
  **all eight were on pages in the silent 36.** Widening only the set would have
  found none of them. So the gate is corpus-wide and keyed on nothing but the
  corpus, in `ShippedGlossaryTests`; the probe gains the glossary half too, through
  the same helper, because that is the file a page author reads.

#### The rule: the reader, not the file

A page sharing eight words with a glossary entry is not the defect. **A page
restating a definition its own reader can also open as a tooltip** is.
`GlossaryMarker` fires an entry on the first occurrence of its term or an alias, in
paragraphs only, never inside a link, and not at all if `!%name%` appears anywhere on
the page — so whether the reader meets a definition at all is a property of the page,
and it is **computed** rather than allowlisted. The numbers, swept once:

| | |
|---|---|
| raw page/block × glossary collisions | **31** |
| the reader meets the definition **once** (legitimate) | **23** — and **13** already carried the suppression, written by hand, by authors with no guard asking for it |
| the reader meets it **twice** (real restatements) | **8** |
| of those 8, on pages the probe could fire on | **0** |

The thirteen hand-written suppressions are the strongest evidence the property is
real: it is what careful authors were already doing. And the two collisions the probe
*could* see were both legitimate — `tumors/atrt`×`nec` never says the term, and
`where-your-tumor-is`×`frontal-lobe` carries `!%frontal lobe%`. **WI-576's third
acceptance criterion is a page that defines a term inline and suppresses its tooltip,
and it passes through the real renderer rather than through an exemption.**

#### Two shapes, two different fixes — and telling them apart is the ruling

The eight split cleanly, and the split is the part to carry forward:

- **If the overlapping sentence lives on more than one page, or in a block, the
  ENTRY is the copy — rewrite the entry.** `glossary/tumor-board`'s definition *was*
  `blocks/tumor-board`'s opening sentence; `glossary/pons` was a sentence
  `/tumors/dipg` and `/tumors/diffuse-midline-glioma` both ship verbatim. That is
  §12.10's own subject — one fact, several homes, one owner — and the block is the
  owner. **Three entries rewritten fixed seventeen of the twenty-two reported rows,
  took no tooltip away from any reader, and left every existing test that asserts
  those tooltips fire still asserting it.** All three read easier than before
  (tumor board 5.5 → 5.2, posterior fossa syndrome 5.5 → 5.2, pons ungraded at 17
  words → 4.2 at 20) — **after /review, which caught the first drafts going the
  other way.** Two of them had risen (5.8 and 6.1, the second over the corpus line)
  while the item's own notes claimed all three had fallen. **Rewriting prose to
  break a shingle is still rewriting prose, and it is graded like any other.**
- **If one page defines the term inline and the tooltip would echo that very
  sentence, suppress on that page.** This is WI-535's rule already in the corpus,
  quoted in `EpendymomaPageTests`: *"defined inline where it first appears; its
  tooltip would read the sentence twice"*. Three suppressions (`!%focal seizure%`,
  `!%transformation%`, `!%vorasidenib%`), and each had an in-corpus precedent —
  both glioma siblings already suppress `transformation`, `/tumors/low-grade-glioma`
  already suppresses `vorasidenib`.

**THE FIRST ATTEMPT GOT THIS WRONG, AND IT GOT IT WRONG BY REACHING FOR THE
SUPPRESSION EVERY TIME.** It put `!%tumor board%` and `!%posterior fossa syndrome%`
on fifteen pages, and seven tests across seven files went red — every one of them a
deliberate, reasoned assertion that **those tooltips should fire**.
`EpendymomaPageTests` fires `posterior-fossa-syndrome` on purpose, because the page
*names* the term before the subsection that describes it, so the tooltip is a gloss at
first mention rather than an echo. **A guard going red on seven files that each wrote
down why is the guard being wrong, not seven files.** Read them before overriding them.

#### Three things the item found on the way, each one somebody else's trap

**A FIX CAN BE COPIED, AND THEN IT HAS THE HOLE A COPY HAS.** Five pages already
wrote `!%tumor board%` by hand, one line above their `[TUMOR-BOARD]` include. Five
authors reached the right answer independently and **twelve including pages never got
it** — the suppression was copied instead of shared. The temptation is to move the
marker into the block, and that is refused: three test files assert
`DoesNotContain("!%")` over `SharedSources()`, because a marker in a block
*"suppresses that term on every including page at once, silently"* (WI-510's review).
That rule stays. **What changes is that hand-copying is now safe, because this item
ships the gate that reds on the omission** — which is the only honest reason to keep
doing something that has already failed once.

**A NEEDLE AND A HAYSTACK DERIVED DIFFERENTLY, AND THE REPLACE THAT FAILED
SILENTLY.** `WatchAndWaitPageContentTests.TheMeningiomaReassuranceNeverReachesAGliomaReader`
excluded the growth section by `Body.Replace(Flatten(Section(GrowthHeading)), " ")`.
`Body` is `ReaderText` — authoring markers stripped. `Section()` reads the **raw**
page — markers present. **One `!%term%` anywhere in that section made the needle
unmatchable, and a `Replace` that matches nothing fails silently**, leaving the whole
section in the text a rule written to *exclude* it then ran over. It had been correct
only because no marker had ever sat there; this item put the first one in and found it.
Fixed by deriving the needle the same way as the haystack **and** by asserting the
removal happened, because a removal nobody checks is a removal that can stop
happening. §12.24's "both canaries beside every scan", arriving as a `Replace`.

**"FIRES NOWHERE" IS NOT "WAS DELETED", AND IT IS MOSTLY NOT NEW.** Two of the three
suppressions take their entry's tooltip from one firing page to none. WI-567 already
ruled on that shape for `frontal-lobe` and the ruling is reused rather than
re-reasoned: the entry is **kept**, because a suppression is a per-page decision an
edit can reverse while a missing entry is a gap every future page inherits — and
because `/glossary` renders `GetTerms()` unconditionally, so a suppressed tooltip is
not a deleted definition. The decision is pinned in a test that asserts the tooltip is
gone, the definition still exists, **and the owning page still carries the sentence
that makes it the owner** — the third one added at /review, which found the one edit
in this area that really hurts a reader: delete the defining sentence and leave
`!%term%` in place, and every guard in the repo goes quiet at once (the restatement
gate has no overlap left to report, and the reader of the page that says the word has
no definition anywhere on it). **A premise a decision rests on has to be asserted,
not asserted about.**

It is deliberately **not** a corpus-wide gate: **26 of the 105 shipped entries fire
no tooltip anywhere, and 24 of those were already in that state before this item.**
Gating all 26 would have meant inventing twenty-four reasons this item cannot source.
Raised as its own item instead.

**AND THAT NUMBER WAS 27 UNTIL /review CHECKED IT, WHICH IS ITS OWN LESSON.** The
sweep was run while the item's first attempt — the fifteen block-driven suppressions
— was still on disk. Those were reverted; the measurement was not re-run, and it went
into this section and into a new backlog item whose whole scope was built on it.
`posterior-fossa-syndrome` was in the list and fires on two pages. **A measurement is
only true of the tree it was taken on.** Re-measure after a revert, not just after an
edit — a revert changes the corpus exactly as much as an edit does, and it is the one
that feels like it changed nothing.

#### What is recorded as shared, with the reason, and what was raised

Two collisions are neither restated definitions nor fixable without making the site
worse, so they are recorded the WI-509 way — a reason beside each, and the **size of
the overlap pinned**, so a pair-level exemption cannot absorb the next collision
between the same two files. **And the converse is asserted from the corpus**: every
record must still match a real collision, or it is reported stale and deleted. That is
the shape WI-578 left standing for `DescriptionsCleanDirectories`, for the same
reason — a list only ever read as *"skip these"* cannot tell you one of them stopped
being true.

- `tumors/atrt` × `glossary/nec` — a **coincidence**. The entry closes *"Ask your
  team what it means for your plan"*; the page says it about an M number on a staging
  line, and NEC is not in that paragraph at all (the tooltip fires from `[CROSSWALK]`
  much further down the composed page). *"Ask your team what it means for your X"* is
  the instruction §12.2 item 7 puts on every page, which is why `Shingles()` already
  strips the "What to ask your team" section; this is the same phrase outside it.
  Rewording a correct sentence to settle a coincidence is how a guard starts shaping
  the content instead of checking it — WI-511's *"not automatically bad news"* call.
- `tumors/high-grade-glioma` × `glossary/cdkn2a-b-deletion` — **a shared fact with
  four wordings**, and deciding who owns it is bigger than this item. The 2021 rule
  that a gene result can set the grade on its own is stated by
  `/tests/molecular-markers`, `/tests/pathology-report`, this hub and the entry.
  Suppressing would lose the entry's unique content (what the genes *do*), so it is
  **raised as its own item** — the shape WI-569 used for the planned-subtotal three
  wordings it handed to WI-577.

#### What this gate does NOT catch, said plainly

**For the three rewritten entries the remedy changed the WORDS, not the OWNER, and
/review was right to press on it.** `/tumors/dipg` still ships *"The pons carries the
nerves for vision, hearing, speech, swallowing and movement"* and the `pons` tooltip
still fires there, so that reader still meets the same fact twice — the two copies
merely no longer share eight consecutive words. The same is true of `tumor-board` and
`posterior-fossa-syndrome` against their blocks.

**So be clear about what the gate is.** It catches a *verbatim* restatement that a
reader meets twice, which is the form that reads as a glitch and the form a shingle
can see. It does **not** catch the same fact in two voices, and **a paraphrase does
not reduce drift risk — it is the beginning of drift.** The pons pair had already
diverged on two points before anyone looked (*vision* vs *eye movement*, *a part of*
vs *the middle part of*), which is §12.10's hazard arriving inside this item's own fix.
The principled remedy is to give each entry only what no page says, or to suppress on
the owning page; both were out of scope here, the first because it needs a source per
entry and the second because seven tests say those tooltips must fire. **The residual
duplication is real and is handed to WI-580, not written off.**

**Also raised rather than recorded a second time:** the glossary as the probe's
*subject*. Nothing ever passes a glossary entry as the subject, so
glossary-vs-glossary and glossary-vs-block collisions are in no entry in either
direction — **four remain** after this item's rewrites fixed two
(`adult-type`/`pediatric-type` at 7 windows and `h3-g34`/`h3-k27-altered` at 10 are
deliberately parallel pairs, and whether a parallel pair *should* be parallel is a
content ruling, not a gate).

#### And it recurred during this item's own deploy

The post-deploy smoke returned one failure and **the site was right.** `/tumors/dipg` was reported as missing the new `pons` definition; the live tooltip panel contains it, carrying the source's hard wrap: `...hearing, speaking,\nswallowing and balance...`. **The needle had been typed from the glossary FILE rather than lifted from the rendered capture, and it spanned the wrap.** The other two definition needles passed only because they happened to sit on a first line, so that was luck rather than method.

`GlossaryTerm.Definition` keeps the source newline and `TermTooltipRenderer` escapes but does not re-wrap, so **any** multi-word needle into a definition can cross one. Flatten whitespace on both sides — the general fix, not a patch for one needle. This is WI-571's recorded defect in a third costume: there a needle included a sentence's first word, here it crossed a wrap inside a tooltip. **"I copied it from the source file" is not "I took it from the rendered artifact".**

> **The question to carry forward is the one that changed this item's design.** Three
> sections asked which pages a guard can fire on (§12.19), which entries (§12.24), and
> which pages are in no entry at all (§12.25). WI-576 asked the third one and got an
> answer that moved the gate to a different file: **every real defect was outside the
> set the obvious fix would have widened.** Ask it before writing the fix, not after.

### 12.27 A rule, its instances, and the measurement that set the item's scope wrong (WI-579)

WI-576's glossary sweep reported `tumors/high-grade-glioma` against
`glossary/cdkn2a-b-deletion`, could not resolve it either way, recorded it as
deliberately shared and handed it here as **"the 2021 grading rule has four wordings
on four files that link each other"**. The claim is that since CNS5 a gene result can
set a tumor's grade on its own, even when the cells look lower grade.

**IT IS NOT FOUR FILES AND THE FIRST MEASUREMENT OF THIS ITEM SAID SO.** WI-576 found
the set with the shingle `the grade on its own`. That phrase finds **five** — it had
already missed `/tumors/low-grade-glioma` — and the ones it cannot see state the rule
best:

| | |
|---|---|
| `/tests/pathology-report` | "A grade used to come only from what the cells looked like. **Now a gene result can set the grade on its own**, even when the cells under the microscope look lower grade." |
| `/tests/molecular-markers` | "In some gliomas this finding **sets the grade on its own**, even when the cells look like a lower grade." |
| `/tumors/high-grade-glioma` | "Because a gene result can now **set the grade on its own**, even when the cells look lower grade." |
| `/tumors/low-grade-glioma` | "For some tumors, a gene result now **sets the grade on its own**." |
| `/tumors/glioma` | "Grade is no longer only about how the cells look. **Gene results can now set the grade.**" |
| `/tumors/astrocytoma` | "A tumor can be **graded 4 on the gene result alone**, even when the cells did not look that way." |
| `/tumors/glioblastoma` | "Since 2021, a tumor can be called a glioblastoma **on its gene results alone**." |
| `/tumors/meningioma` | "Either of those two changes makes it grade 3, **whatever the cells look like**." |

**Eight, and not one of the last four carries the phrase the item was scoped with.**
This is WI-569's §12.18 finding — *a lexicon is a floor, not a fence; two of a
hundred planted sentences carried no banned word at all* — arriving as **a work
item's own scope**, which is a place it had not been seen before. A measurement
taken with a phrase sets the size of the work, and nobody re-measures a number that
came with the assignment. Re-measure it.

#### The ruling: a rule and its instances are different facts, and only the rule has one owner

The eight are not eight copies of one sentence, and the reason the item did not
delete seven of them is the ruling:

- **The RULE** — *a grade used to come from the cells alone; since 2021 a gene result
  can set it on its own* — is one fact about **grading**, and it has one home:
  **`/tests/pathology-report#what-the-grade-means`**.
- **An INSTANCE** — *an IDH-mutant astrocytoma is grade 4 if both copies of CDKN2A/B
  are missing; a meningioma is grade 3 on a TERT promoter change or a CDKN2A/B loss;
  a tumor with no IDH change and one of three findings is a glioblastoma* — is a fact
  about **that tumor**, and it belongs to the page whose reader is holding that
  report. This is §12.10's own split ("the block carries what is universal, the page
  carries its own slice, **repeated only where it differs**") applied to a fact that
  has no block.
- **An ANSWER** is the third reason and the one that keeps three general restatements.
  `/tumors/low-grade-glioma`, `/tumors/high-grade-glioma` and `/tumors/glioblastoma`
  each state the general rule under a heading written in the reader's own words about
  a grade that surprised them — *"Why does my report say grade 4 when the scan looked
  low grade?"*, *"…when the scan looked milder?"*, *"But my tumor did not look grade
  4"*. **An answer to the question a section is headed with cannot be a link.**
  Replacing the answer with the route the page already carries leaves a headed
  question unanswered, which is worse for that reader than a duplicated sentence.

`/tumors/glioma` is recorded as an instance even though its sentence *is* the general
rule, because the glioma family spans grades 1 to 4: on the umbrella, "what this
applies to here" and "in general" are the same sentence (§12.11).

#### Why `/tests/molecular-markers` is NOT the owner

The backlog called the marker page "the obvious owner" and said to explain it if not.
It is not, on three counts:

1. **The rule is about grading, not about a marker.** The marker page states the same
   shape of thing **four separate times** — CDKN2A/B, TERT, EGFR, chromosome 7/10 —
   and means a different fact each time. A page that states a rule once per marker is
   stating the markers.
2. **It already routes here.** The sentence after its CDKN2A/B instance is
   *"[What the grade means](/tests/pathology-report#what-the-grade-means) explains how
   grades are worked out and why the numbering changed in 2021."* The marker page had
   already decided who owned the rule.
3. **The reader who needs the rule is holding a grade, not a gene panel.** Putting it
   on the marker page puts it behind a document many readers never receive.

And `/tests/pathology-report` is the only one of the eight that states the rule **as a
change**, with the before-half. *"A gene result can set the grade"* alone is a true
fact about grading that leaves the reader's actual question — *so why does my report
disagree with what I was told about the cells?* — unanswered. **The before-half is
what makes it the rule**, and it is what the other seven are allowed to omit.

#### §12.10's route half was never the hole, and the hole was a premise nobody asserted

Measured before anything was edited: **all eight already link to
`/tests/pathology-report` in their own prose.** Two reach the grading section by
fragment. So no page needed a route added, and the item that looked like "nine pages
to rewrite" was nothing of the kind.

What was missing was a **written owner** and a gate on the premise. **Nothing in the
repo asserted that the owner still states the rule.** Delete those two sentences and
seven pages go on linking to a section that no longer answers, the whole ruling
becomes false, and not one test changes colour. That is WI-576's finding in a second
costume — there, *"the page defines the term inline"* lived in a message string — and
it is the same instruction: **assert the premise a decision rests on, not facts about
it.**

#### What was actually edited, which is four sentences

- **`glossary/cdkn2a-b-deletion`** was the one real double-exposure: its second
  sentence was the rule, and it fires as a tooltip on every page that says the term,
  including the hub that answers its own headed question with the same words. Per
  §12.26's first shape — *if the sentence lives on more than one page, the ENTRY is
  the copy* — the sentence went and the first one stayed, because *"two genes that put
  the brakes on cell division"* is the only place in the corpus that says what the
  genes **do**, which is why suppressing the tooltip was refused. **The entry went
  from grade 7.7 to 3.7.** With the overlap gone, WI-576's `DeliberatelyShared`
  record was deleted — deliberately, not by test failure, though its converse check
  would have reded on it.
- **THE GENE COUNT WAS WRONG ON THREE PAGES, NOT ONE.** The backlog named
  `/tumors/high-grade-glioma`. Scanning for the **shape** rather than the sentence
  found `/tumors/astrocytoma` saying the same *"a gene called CDKN2A/B"*, and
  `/tumors/meningioma` saying *"a gene called CDKN2A or CDKN2B"* — **a singular
  article in front of two gene names, which is the same error wearing the plural's
  clothes** and is the one a sentence-shaped search cannot see. The meningioma fix
  also repaired that sentence's pronoun: *"Either one"* sat between two gene names
  and two "things" and could attach to either pair.
- **AND THE FIRST FIX FOR THE TWO GLIOMA HUBS WAS ITSELF A CLAIM THE SOURCES DO NOT
  CARRY.** They were corrected to *"both copies of two genes, CDKN2A/B, are
  missing"* — which asserts both copies of **both** genes, where the criterion is
  homozygous deletion of CDKN2A **and/or** CDKN2B, and where `/tumors/meningioma`
  now correctly said *"or"*. So the corpus contradicted itself about one criterion.
  Rewriting the hubs to say *"or"* was **also** refused, because §12.1 governs: the
  repo's own cited quotes carry the report label (*"homozygous CDKN2A/B deletion…
  diagnostic of a CNS WHO grade 4 tumor"*) and not the and/or semantics, so
  spelling it out would outrun the citation. **What shipped is that the two hubs
  carry the label the reader's report carries and make NO count claim at all**
  — *"both copies of CDKN2A/B missing"* — while the count and what the genes do stay
  with `glossary/cdkn2a-b-deletion` and `/tests/molecular-markers`. Which is this
  section's own ruling applied to its own defect: the hub carries its instance, the
  detail has an owner. The singular-gene ban keeps its teeth either way, and the
  converse canary (*something, near the term, says there are two*) fell from five
  files to three — the three that own the fact.

  > The and/or nuance is now the only thing in this area with no home: no cited
  > source in the repo states it, and three files word it three ways
  > (*"two neighboring genes are gone"*, *"CDKN2A or CDKN2B"*, *"two genes"*).
  > Raised for `/pm` rather than settled here on an uncited claim.

#### One defect, three times, in one item — and it is the lesson worth carrying

**A POSITIVE CONTROL THAT DOES NOT DISCRIMINATE BETWEEN THE PATTERNS IT IS EVIDENCE
FOR PINS NOTHING.** WI-579 shipped that mistake three separate times and each one was
caught by a different instrument, which is the only reason all three are known:

| | found by | what it cost |
|---|---|---|
| the floor scan's single caught control | **the break harness** | a survivor: 34 of 36 |
| `saysTwo` counting per *(file, pattern)* | **reading the code** | a canary whose message named the wrong unit |
| `ThePairAsOneGene`'s two controls, both matching only its first pattern | **`/review`** | three of four patterns pinned by nothing; deleting two left the suite green |

The fix is the same in all three and it is cheap: **one control per pattern, each
measured to match exactly one, plus an assertion that the control table is in the
vocabulary's order** — a reorder leaves every per-row match intact and only the order
can see it. The expensive part is never writing the control; it is discovering, later,
which pattern a control actually pinned.

**AND A VOCABULARY DERIVED FROM THE CORPUS HAS TO BE MATCHED THE WAY THE CORPUS
WRITES IT.** `/review` found that `grade is no longer only` matched **zero of 168
files**: the corpus's only occurrence is sentence-initial and bolded — *"**Grade is no
longer only about how the cells look.**"* — and the pattern had been typed from a
mid-sentence paraphrase. A dead pattern in a list of eight is invisible behind a
**summed** floor, because any seven of them still reach the total. Two fixes, and the
second is the general one:

- the scan ignores case, which also closes the same hole in
  `(?:whatever|however) the cells look` and `the grade does not come only`;
- **the summed floor is replaced by eight per-file counts.** `hits >= 16` could not
  see a pattern die, and — because it skipped every match on a recorded page — could
  not see a **ninth statement added to a page that already had one** either. Eight
  exact numbers catch an addition, a removal and a dead pattern, and name the file.

**A WHOLE-FILE READ IS NOT A READ OF THE PROSE.** The route check used `Raw()`, which
includes front matter — hundreds of lines of source notes on these pages. A
`# cross-ref: /tests/pathology-report` comment up there would have satisfied it while
the reader had no link at all. It reads `ReaderText(Raw())` now, still uncomposed so
WI-570's inherited-route finding stays covered.

**AND A CANARY CAN BE VACUOUS IN THE EXACT WAY IT EXISTS TO PREVENT.** "No file
contains `zzqqxnotinthecorpusxqqzz`" passes just as happily when the helper returns
`""` for all 168 files — which is the failure the canary pair was written to catch. It
is a round-trip now: the literal is absent from a real file and **present** when
appended to one, which proves the helper preserves text rather than proving nothing.

**WHAT WAS REFUSED, and why it belongs here rather than in the code.** `/review`
proposed making the `AnAnswer` reason mechanical by asserting the heading contains
" my " — true of all three answers and of none of the other five. Declined: that is
the question-mark test wearing a different word. A heading can be in the reader's
voice without "my" (*"Is it cancer? What does its grade mean?"*), so the assertion
would eventually red on correct writing, which is WI-509's rule about a guard that
fails a correct page. The honest alternative is the one taken: the **failure message**
now says that updating the heading string is not the fix and that a renamed heading
means the reason has to be re-decided.

#### Two more things this item got wrong on the way

**A POSITIVE CONTROL SATISFIED BY A DIFFERENT PATTERN THAN THE ONE UNDER TEST PINS
NOTHING, AND THE BREAK HARNESS IS WHAT SAID SO.** The floor scan shipped with one
caught control — *"In a few tumor types a gene result sets the grade on its own,
whatever the cells look like"* — asserted as `Assert.Contains(vocabulary, p =>
IsMatch(control, p))`. Blanking the pattern `sets? the grade on its own` left the
sentence caught by `(?:whatever|however) the cells look`, the assertion passed, and
**the mutation survived on both endings**: 34 of 36 caught, and the two misses were
one defect in the guard rather than anything about the corpus. §12.18 says a guard
never seen to fail has not been shown to work; this is one turn further on — **a
guard seen to fail for the wrong reason has not been shown to work either.** Now one
control per pattern, each *measured* to match exactly one, plus an assertion that the
control table is in the vocabulary's order, because a reorder leaves every per-row
match intact and only the order can see it.

**A CHARACTER TEST IS NOT A PROPERTY, AND THE PREMISE WAS GOING TO BE ASSERTED
WRONG.** The reason three pages may restate the rule is that the section is headed
with the reader's own question — so the obvious gate is "the heading above the
sentence ends with `?`". Measured before it was written: **four of the eight sit under
statement headings**, including `/tumors/glioma`, whose `##` *is* a question while the
`###` the sentence lives under is not, and `/tumors/glioblastoma`, whose heading is an
objection with no question mark at all. "Ends with a question mark" is a test about a
character; "is written in the reader's own words" is a judgement. **The record names
the heading per statement instead**, and the gate asserts the sentence is still under
*that* heading — mechanical, and it reds when either half moves. §12.24's "refuse the
clause, not the word it starts with", arriving as punctuation.

**TWO GRADERS DISAGREED AND THE REPO'S WAS RIGHT.** Grading all 105 glossary
definitions with `ReadabilityAnalyzer` reported **38 over 6.0, worst 16.1**;
ContentCheck reported **35 of 101, worst 9.3**. The difference is not a bug: ContentCheck
grades a definition only at **20 words or more**, with the reason written beside it —
*"Flesch-Kincaid on a 25-word definition is too noisy to fail a build on"*. The three
extra were short entries where FK is noise (`pcv` is three drug names). **The figure
to carry is the tool's.** WI-416 ("one reading-level grader, not two") is about
exactly this, and the cost of the second grader here was a wrong number that nearly
reached a doc.

#### WI-571's needle defect in a fourth costume, and this one passed instead of failing

The pre-deploy smoke reported two of this item's OLD sentences as **already absent
from production** — a site that certainly still shipped them. The cause is
`GlossaryMarker`: `CDKN2A/B` is an alias of the entry, so on a page where the tooltip
fires the first occurrence is replaced with button markup **in the middle of the
sentence**:

```
both copies of a gene called <button ... popovertarget="def-cdkn2a-b-deletion">CDKN2A/B</button><span id="def-cdkn2a-b-deletion" popover>…
```

A needle spanning the term cannot match the rendered page. The first two costumes
were WI-571's (a needle including a sentence's first word) and WI-576's (a needle
crossing the source's hard wrap *inside* a tooltip panel); **the new half is that it
failed in the safe-looking direction.** An absence check that cannot see the thing it
is looking for prints `ok`, and a pre-deploy run whose job is to FAIL reads as
if those two needles discriminated when they could not have. The post-deploy run
would then have printed `ok` for the same two needles whatever shipped.

So: **a needle must stop short of any term the glossary can fire on**, and the fix
that generalises is not a shorter needle but a **counted** one — the smoke now
asserts each prose needle appears **exactly once**, which is what turns "present
somewhere" into "this sentence changed". Case and punctuation did the
discriminating here: the short needle `both copies of two genes,` does not match the
panel's own `Both copies of two genes that put the brakes…` on the same page.

> **And the finding that is somebody else's item:** a glossary definition is
> reader-facing prose that ContentCheck **prints a grade for and does not gate** —
> `(not gated)` — and **35 of the 101 it grades are over 6.0**. That is WI-575's hole
> in a third surface and WI-578's shape exactly, which took page `description` from 41
> over the limit to 0 and gated it. Raised, not fixed here.

### 12.28 The glossary as the probe's SUBJECT, and the two reader surfaces it has (WI-580)

§12.26 made every page's prose checkable against every glossary definition and
said plainly what it had not done: **nothing ever passes a glossary entry as the
subject**, so an entry that restates another entry, or a block, is in no entry in
either direction. It handed four collisions here, and closed by handing over the
residual duplication its own fix had left — `/tumors/dipg` still shipping a
sentence the `pons` tooltip restated, with the two copies no longer sharing eight
words because the entry had been rewritten.

**The four re-measure unchanged on today's tree**, which was worth checking and
not worth assuming: WI-579 edited `glossary/cdkn2a-b-deletion` in between, and
§12.26's own closing lesson is that a measurement is only true of the tree it was
taken on.

| | | |
|---|---|---|
| `glossary/adult-type` × `glossary/pediatric-type` | 7 windows | entry × entry |
| `glossary/h3-g34` × `glossary/h3-k27-altered` | 10 windows | entry × entry |
| `glossary/astrocyte` × `glossary/oligodendrocyte` | 2 windows | entry × entry |
| `glossary/status-epilepticus` × `blocks/escalation` | 2 windows | entry × block |

#### The ruling has two halves because an entry has two reader surfaces

§12.26's rule is **the reader, not the file**, and it worked because a page's
prose and a tooltip panel are one screen: whether the reader meets a definition
at all is a property of the page, and it is computed rather than allowlisted. Ask
the same question of an entry as the SUBJECT and it splits, because an entry
reaches a reader two different ways.

- **`/glossary` renders `GetTerms()` UNCONDITIONALLY** — every entry, every
  definition in full, grouped A–Z (`Pages/Glossary.cshtml`). So for an
  **entry × entry** collision the answer to *"does a reader meet both?"* is
  **always yes**, and it cannot be computed away. `h3-g34` and `h3-k27-altered`
  are not merely both on that page; they are **adjacent on it**, both under "H".
- **A tooltip fires per page**, which is the surface §12.26 already governs. So
  for an **entry × block** collision the question is whether a page that
  INCLUDES the block also fires the entry's tooltip — and that is computable.

**The measurement is what forces the split, and it is counter-intuitive in both
directions.** Of the seven entries in the four collisions, **five fire no tooltip
on any of the 55 pages** (they are in WI-581's set of 26). Ask only the tooltip
question and all four collisions return *"a reader meets both on zero pages"* and
the item is closed having found nothing. That answer is **wrong for the three
entry × entry pairs** — `/glossary` is the surface — and it is **right for the
entry × block one**, for a reason nobody predicted:

> **`blocks/escalation` has the blast radius and ZERO reader exposure.** The block
> composes onto **25 pages** and *status epilepticus* is said on **none of them**.
> The one page that fires the tooltip, `/seizures/living-with`, does not include
> the block. The backlog asked for this collision to be checked first and
> separately *because* of the blast radius; checking it first is what found that
> the blast radius is the reason there is nothing there. **A block's reach is a
> reason to look, not a finding.**

#### Half one: a parallel pair MAY share its frame, and must share it verbatim

A pair of definitions that exists to be told apart *should* read in parallel. If
`adult-type` says *"sorted by their biology, not by the age of the person who has
them"* and `pediatric-type` says the same thing in different words, a reader
holding both — and on `/glossary` every reader holds both — **cannot tell whether
the difference in wording is a difference in meaning.** The shared frame is what
makes the difference legible; it is the control variable.

So a bare shingle gate over `glossary/` is refused, and refused on the merits
rather than as an exemption: it would demand that correct writing be made worse,
and the only way to satisfy it is to paraphrase one side — which is the move
§12.26 closed by naming. **A paraphrase does not reduce drift risk; it is the
beginning of drift.** The pons pair is the proof, in this corpus, inside §12.26's
own remedy.

What is gated instead is the **conditions**, not the overlap:

1. **The overlap size is PINNED, exactly.** A record says these two share exactly
   *n* windows. Reword either side and *n* moves and the test reds. **That is the
   drift detector a shingle allowlist cannot be** — `AllowedShingles` sets
   forgive an overlap, and forgiving it is the same as not seeing it.
2. **The converse is asserted from the corpus.** Every record must still match a
   real collision at exactly its recorded size, or it is reported stale. The shape
   WI-578 left standing for `DescriptionsCleanDirectories` and §12.26 for
   `DeliberatelyShared`, for the identical reason: *a list only ever read as "skip
   these" cannot tell you one of them stopped being true.*
3. **Each side must say something the other does not.** The premise the licence
   rests on is that the pair HAS a distinguishing half; a pair that does not is
   one entry with two names. §12.27: **assert the premise a decision rests on, not
   facts about it.**

All three pairs are licensed under that rule, and the frame each shares is the
right one: the biology-not-age frame for the two type labels, the histone frame
for the two H3 findings, the helper-cell-in-the-brain-and-spinal-cord frame for
the two glial cells. Each already carries its distinguishing half, and `h3-g34`
carries an explicit cross-reference to its partner, which is the pair doing its
job out loud.

#### Half two: for an entry × block pair the discriminator is POSITION, and it is computable

§12.26 refused a suppression on `/tumors/ependymoma` and wrote the reason in a
comment: the page *"NAMES the term before the subsection that describes it, so the
tooltip is a gloss at first mention rather than an echo."* **That is not a fact
about one page. It is the rule**, it is positional, and `GlossaryMarker` fires on
the FIRST occurrence only, so it is decidable:

> For a page that includes the block which owns a fact — **if the first occurrence
> a tooltip could fire on comes BEFORE the block, the tooltip is a gloss at first
> mention and must fire. If it comes at or after the block, the tooltip echoes
> prose the reader has just read, and the term is suppressed on that page.**

Measured over the composed pages, the split is bimodal and not close. The echo
cases put the term **2 characters** past the block's start — it is the block's own
second word, `A tumor board is a meeting…`. The gloss cases put it **13 or 28
characters** before — the sentence immediately above the include — or thousands of
characters earlier, in a different section.

| | includers | gloss (keep) | echo (suppress) | already suppressed by hand | term never said |
|---|---|---|---|---|---|
| `blocks/tumor-board` | 17 | 3 | **9** | 5 | 0 |
| `blocks/posterior-fossa-syndrome` | 4 | 3 | 0 | 0 | 1 |
| `blocks/escalation` | 25 | 0 | 0 | 0 | 25 |

**The rule agrees with every file that wrote down a reason, and that is the test
it had to pass.** §12.26: *a guard going red on seven files that each wrote down
why is the guard being wrong, not seven files.* All three
`posterior-fossa-syndrome` includers assert that tooltip fires on purpose
(`EpendymomaPageTests`, `MedulloblastomaPageTests`), and the position rule says
gloss on all three — **zero suppressions, arrived at by measurement rather than by
exempting the tests.** `/tests/mri` asserts `def-tumor-board` fires as contract
item 9, and it is one of the three glosses. Not one reasoned file reds.

**And it corrects §12.26's own number.** That section recorded *"five authors
reached the right answer independently and twelve including pages never got it."*
Three of the twelve are the ependymoma shape — a decision, not an omission. The
figure was a subtraction (17 − 5) and §12.24's warning applies to it unchanged:
**a set defined by subtraction is redefined by every new kind of X.** Nine pages
got the suppression here, not twelve.

**The gate is a COMPOSITION rule, not a shingle rule, and that is the point of
it.** *No shingle check can see a paraphrase* — `glossary/tumor-board` and
`blocks/tumor-board` state the same three facts in two voices and collide on zero
windows. The position rule never looks at the words. It asks where the reader's
first meeting with the term is, which is the question §12.26's rule was always
asking, one level up. **A paraphrase is gated by gating the composition.**

The marker stays hand-written on each page: three test files assert
`DoesNotContain("!%")` over `SharedSources()`, because a marker in a block
suppresses that term on every including page at once, silently (WI-510). §12.26
kept that rule and said hand-copying was now safe *"because this item ships the
gate that reds on the omission"* — and for `tumor-board` it did not, because the
entry had been paraphrased out of the shingle gate's view. **This is the gate that
actually reds on the omission.**

#### The residual duplication, decided per pair — and the live medical error in it

§12.26 handed over three pairs where the remedy had changed the WORDS and not the
OWNER. All three are entry-vs-page or entry-vs-block duplications of a FACT, which
is why no gate had reported them since.

**`glossary/pons` × `/tumors/dipg` × `/tumors/diffuse-midline-glioma` — and the
two pages were WRONG.** The entry, corrected at WI-576's /review, reads *"The nerve
paths for **eye movement**, hearing, speaking, swallowing and **balance** pass
through it."* Both pages still ship *"The pons carries the nerves for **vision**,
hearing, speech, swallowing and **movement**."* The visual pathway does not pass
through the pons; CN VI and the pontine gaze centre move the eye, and CN VIII
carries hearing **and balance**.

**The pages refute themselves, which is the evidence that settles it without an
appeal to anatomy.** Each page's own symptom list — sourced, and three sections
below the claim — says *"Double vision, or eyes that do not move together"* and
*"Trouble with balance or walking."* So the page already tells the reader the
symptom is eye MOVEMENT and already names balance as a consequence **with nothing
in its cause sentence to explain it**. The cited source is the same on both pages
and on the entry, and its own symptom list is *"Eye problems such as blurred
vision, double vision, drooping eyelids, uncontrolled eye movements"* and *"Loss of
balance"*. Its anatomy sentence does say *vision* — **so this is §12.1 with the
source on the wrong side of the correction**, and the resolution is that *vision*
is the source's loose label for the symptom a parent sees, while the source's own
symptom list names the mechanism. Publishing *vision* invites a reader to expect
sight loss, which is not this tumor's story and which the page's own list
contradicts.

**AND `movement` WAS DROPPED, WHICH WAS THIS ITEM'S OWN ARGUMENT RUN IN REVERSE.**
The first fix replaced *vision* with *eye movement* (right) and also dropped
*movement* (wrong), on the reasoning that the entry /review shipped at WI-576 does
not carry it and each page's symptom list covers weakness anyway. **/review refused
that and was right.** Both pages still list *"Weakness in an arm and a leg"*, so a
cause sentence without *movement* cannot explain a symptom the same page names --
which is **precisely the self-refutation that settled *vision*,** pointed the other
way. And unlike *vision*, the source's *movement* is not loose labelling: the
corticospinal tract runs through the basis pontis, and long-tract weakness is part
of this tumor's classic triad. So restoring it does not outrun the citation, **it
is the citation** -- and it is undoing this item's own deletion rather than adding
a sixth claim, because the pre-item pages already said *movement*.

> **The lesson is narrower and more useful than "check the sources".** The symptom
> list is what refuted *vision* and it is what refuted dropping *movement*, in the
> same two pages, in the same session, to opposite conclusions. **A page's own
> downstream list is a two-sided test**: it convicts a cause the page does not
> support, and it convicts a cause list that cannot explain what the page goes on to
> name. Only one side of it was used the first time.

It ships as a **second sentence** rather than a sixth list item: *"So do the paths
for movement in your arms and legs."* Two reasons, both this corpus's own rules --
*"eye movement ... and movement"* in one list is a comprehension risk at a
6th-grade reading level, and both clauses say **paths**, so no nerve-versus-tract
distinction is asserted that the source does not draw (§12.1, WI-579's finding one
section up). The entry takes the same sentence, because the three copies have to
agree or the set gate reds -- and its grade **fell from 4.2 to 3.7**.

**The owner is the PAGE on those two, and the entry keeps firing elsewhere.** Both
pages define the term inline at its very first mention — *"a tumor in the pons,
part of the brain stem"* — which is §12.26's second shape exactly, and both answer
the anatomy under a heading that asks for it: *"Where does it grow, and why does it
cause these symptoms?"* §12.27's third reason governs: **an answer to the question
a section is headed with cannot be a link**, and it cannot be a tooltip either. So
`!%pons%` is suppressed on both, and the entry still fires on
`/where-your-tumor-is`, where it names the pons as one of three parts of the brain
stem and explains none of them — **there the entry is the only explanation that
reader gets**, which is why trimming the entry instead was refused.

**And the suppression takes the pair out of the shingle gate's view, so the premise
is asserted instead.** §12.26 named this exact hazard: delete the defining sentence
and leave `!%term%` in place and every guard goes quiet at once. The gate here is
not a shingle — **it is the SET of functions each copy names**, extracted from the
page and from the entry and asserted equal. That is what catches *vision* against
*eye movement* and a missing *balance*; a shingle cannot, a word ban is a floor and
not a fence (§12.18), and set equality is the one form that sees a paraphrase of a
list.

**`glossary/tumor-board` × `blocks/tumor-board`** — the block is the owner on the
nine echo pages, by the position rule above. The entry is unchanged and keeps
firing on the three gloss pages and on the two pages that fire it without including
the block (`/tests/follow-up-scans`, `/tests/pathology-report`), where it is the
only copy.

**`glossary/posterior-fossa-syndrome` × `blocks/posterior-fossa-syndrome`** — no
change, and the reason is the rule rather than an exemption: all three includers
name the term before the block, so all three tooltips are glosses.

**`glossary/status-epilepticus` × `blocks/escalation`** — recorded with its two
windows pinned and no edit, because **no reader meets both** and the two copies
each have a reader who needs them whole. An entry defining *status epilepticus*
without the five-minute rule defines nothing, and an ambulance tier without it is
not a tier. The hazard here is **disagreement, not repetition**, and the pinned
count is the instrument for disagreement: change the block's threshold without
changing the entry's and the count moves and the test reds. Putting 25 suppression
markers on the corpus to protect no reader was the alternative, and it is refused.

#### What /review found, and four of the six were assertions that could not fail

Six findings, every one confirmed by measurement before anything was edited, and
**four of them are the same defect class WI-579 shipped three times** (§12.27): an
assertion that reads as proof and cannot fail. It is now five items running, so it
is worth stating as a checklist rather than as a story.

| what it was | why it could not fail |
|---|---|
| `Frame.Except(Frame).Count() == 0`, the control for the parallel-pair premise check | `X.Except(X)` is empty for **every** `X`, including the empty set. The premise check had still never been seen to fire. The fix is an **asymmetric** probe: `Frame` against `Frame + " Adults usually have one of these."`, so one side has nothing of its own and the other does. |
| `markersSeen <= markersOnDisk`, the suppression gate's "floor" | A **ceiling**. Measured 109 against 111, and a scan degraded to one marker on one page satisfies both `1 > 0` and `1 <= 111`. A *derived* number cannot replace it either, because a degraded scan degrades both sides. It has to be a **literal**: 109 markers on 35 pages. |
| the per-direction collision counts | Counted over the **record array**, which is a fact about a literal 200 lines up and can only move when somebody edits that literal — while the comment called it the collision count. Tallied from the measured pairs now. |
| `Assert.Contains("brain stem", firstMention)` | Two splitting defects: headings were not stripped, and `(?<=[.!?]) ` does not split after a **bold-terminated** sentence, because the `.` is followed by `*`. So `firstMention` was a heading plus two sentences. **This version was itself the fix for a weaker one** — and the attack still walked through it: *"\*\*DIPG is a fast-growing tumor in the pons.\*\* It is a part of the brain stem, and …"* leaves the reader who meets the word with no gloss in that sentence, and the assertion returned true. §12.10's phrase-versus-position shape, surviving the fix written to remove it. |

**AND THE DELETION OF THREE GUARDS TOOK A COVERAGE NOBODY NOTICED.** Each of the
three page-local copies carried `Assert.NotEmpty(suppressed)`. The corpus-wide gate
that replaced them is conditional on a marker **existing**, so it cannot see one
**deleted** — and `ShippedGlossaryTests` cannot either, because /review measured
**five markers whose entry overlaps its page by zero shingles**
(`!%transformation%`, `!%chemoradiation%`, `!%pseudoprogression%` on
`/tumors/high-grade-glioma`, `!%astrocyte%` on `/tumors/astrocytoma`,
`!%diffuse glioma%` on `/tumors/glioma`). Delete any of them and every guard in the
repo stays green. **That is this section's own argument — a shingle cannot see a
paraphrase — arriving inside this item's own supersession**, and it is the strongest
case for the one line that fixes it: an exact count closes it corpus-wide, for all
109 markers rather than for the three pages that happened to have a copy.

> **The rule to carry: when a wider gate supersedes a narrower one, enumerate what
> the narrow one ASSERTED, not what it was for.** §12.26 asked for WI-576's
> entry-reachability test to be *"superseded or absorbed, not left beside a wider
> gate saying the same thing"*, and this item did that for three copies — correctly,
> and while silently dropping a fourth assertion none of them was named for. A
> supersession is a merge, and the thing to diff is the assertion list.

Two smaller ones, both measured and both the same shape as each other: the
function-set extractor used `IndexOf` (so *"under supervision"* reported **Vision**
and *"causing imbalance"* reported **Balance**) and consumed only the **first**
occurrence (so *"Eye movement and eye movement"* reported a spurious **Movement**,
which reds the gate on correct writing — §12.8's worse-than-no-rule direction).
**A set comparison is only evidence if the extractor does set MEMBERSHIP**, which
means whole-word matching and every occurrence consumed. The lookarounds are
`GlossaryMarker.BuildMatchers`' own, and all three shipped sentences were measured
before and after to confirm the fix moved no set.

> **The question to carry forward.** §12.26 asked which pages are in no entry at
> all and the answer moved its gate to a different file. This item asked **which
> reader SURFACE the subject reaches**, and the answer split one gate into two:
> a tooltip is per-page and computable, `/glossary` is unconditional and is not.
> **Before writing a gate over a set, ask how many ways a member of that set
> reaches a reader** — because a property that is computable on one surface can be
> vacuous on the other, and five of the seven entries here reach no reader through
> a tooltip at all.

### 12.29 An entry that reaches no reader through a tooltip, and the three reasons why (WI-581)

§12.26 measured that **26 of the 105 shipped glossary entries fire no tooltip on
any of the 55 pages** and refused to rule on them, because the number had at
least three causes inside it and a rule for the number would have been a rule
for none of them. §12.28 then pointed its corpus-wide suppression gate at all 55
pages and handed over **six markers that suppress nothing**, each with a measured
cause. This section is the ruling, and it is **one rule per cause**.

**The 26 re-measure unchanged on today's tree**, which was worth checking rather
than assuming: §12.26's own closing lesson is that a measurement is only true of
the tree it was taken on, and WI-576's first figure said 27. 105 entries, 55
pages, 79 firing, 26 not, and the same 26 slugs.

#### The partition is computed POSITIVELY, and it has four cells because three was a guess

| | cause | before | after |
|---|---|---|---|
| **(a)** | at least one page says the term and **suppresses** the tooltip there | 23 | **25** |
| **(b)** | **no page says the term**, in any name the entry matches on | 2 | **1** |
| **(c)** | a page writes the term, but **only where no tooltip can fire** | 1 | **0** |
| **(d)** | said in PROSE, nothing suppresses it, and still silent | 0 | **0** |

Cause (d) is the one the backlog did not ask for and §12.24 did: *a set defined
by subtraction is redefined by every new kind of X*. Written as three causes, the
third is an `else` branch, and the next kind of X lands in it silently. So the
classifier decides (d) **first** — the term is in a page's prose, no marker
explains the silence, and the tooltip still fires nowhere — which is neither a
decision nor a position but **the matcher failing to match what the page wrote**:
a `%%term%%` escape span, a name the entry has no alias for, a **longer glossary
name that claimed the position**, or a surface `GlossaryMarker` does not walk. It
is asserted EMPTY by name, and because two of the four cells are empty the
classifier is shown putting something in **all four** on probe pages one edit
apart (§12.18: a property guard that has never been seen to fail has not been
shown to work) — including `CauseOf` itself and not only the primitives under it,
which is what /review found the first version of that probe leaving out.

> **AND CAUSE (d) HAD TO BE ASKED PER PAGE AS WELL, WHICH IS THE ONE PLACE THIS
> ITEM'S FIRST DRAFT WAS WRONG RATHER THAN INCOMPLETE.** As a cell of a partition
> over the 26, (d) can only ever be non-empty for an entry with **no owning page**
> — one of 26 today — because an entry with an owner is cause (a) whatever else is
> true of it. /review planted `%%transformation%%` on `/tumors/glioma`, the
> textbook (d) condition by the very mechanism the cell names first, and **the
> whole suite stayed green**: that entry has three owners. So the property is a
> property of a **(page, entry) PAIR**, and it is gated over all **5,775** of them:
> no page writes a glossary term in its prose and leaves the reader with no
> tooltip for it anywhere on that page.
>
> **The one sanctioned shape is COMPUTED rather than allowlisted**, which is
> §12.28's rule about pinning conditions instead of forgiving instances.
> `FindEarliestMatch` takes the LONGEST name at a position, so a term sitting
> inside a longer glossary name that fires on that page is not silent — the reader
> got a **more specific** tooltip at exactly that spot. Measured over the corpus:
> **two pairs, and both are that**, `glioma` inside *"In a diffuse glioma"* on
> `/treatments/craniotomy` and `posterior fossa` inside *"**posterior fossa
> syndrome**"* on `/tumors/atrt`. They are counted and named rather than skipped,
> so a third arrival is a decision somebody makes.

**Cause (a) is measured by the two-render method, and the other three are not —
which is worth stating rather than generalising.** Remove one `!%term%`, render
through `ContentStore.Parse`, ask whether the tooltip appeared: that is what
§12.28 shipped, it is the only form that sees what a scan cannot, and it is what
decides OWNERSHIP. Causes (b), (c) and (d) are then separated by asking WHERE the
term is, which is a scan over the composed page — and the scan has to model two
things `GlossaryMarker` does: it walks `ParagraphBlock`s only, so a heading is
not a place a tooltip can fire, and it skips anything under a `LinkInline`, **text
and destination both**. /review priced the second half of that at **43 (page,
entry) pairs**: these strings are composed MARKDOWN, `/` is neither `\w` nor `-`,
so `craniotomy` matches inside `(/treatments/craniotomy)` while Markdig keeps a
URL in `LinkInline.Url` where the marker can never see it. A page deep-linking
`/glossary#flair` — which is what the tooltip's own fallback link does — would
have been reported as a defect that was not there.

#### Rule for (a), the sanctioned shape: the page that suppresses is the page that says the word

**25 of the 26, and the clustering is the argument.** `/treatments/craniotomy`
owns eight of them, `/treatments/chemotherapy` four, `/treatments/radiation-therapy`
four. This is not an accident and it is not neglect: a library page that teaches
a vocabulary suppresses that vocabulary, because a tooltip repeating the
paragraph underneath it is noise — the carpet WI-505 measured its way out of.

WI-567's ruling is quoted rather than re-derived: **the entry is KEPT**, because
*"the suppression is a per-page decision that can be reversed by an edit, while a
missing entry is a gap every future page inherits."* What this section adds is
the **premise that ruling rests on, asserted** (§12.27): each of the 30
(entry, page) pairs carries **the sentence in which the reader's first meeting
with the term happens**, derived from the rendered artifact rather than
transcribed.

> **That field is not called "the definition", and the difference is the honest
> part.** It is the sentence `GlossaryMarker` would have fired in. For 27 of the
> 30 it names the term and explains it in one breath — *"That piece is called a
> bone flap."*, *"The lowest point is called the nadir"*, *"You may see this
> called neutropenia"*, *"It has a name: somnolence syndrome"*. For three it is a
> bolded list label on `/treatments/craniotomy` (*"- **Debulking.**"*) with the
> explanation beside it rather than inside it. Claiming the pin proves "the page
> explains it" would be §12.28's own defect class — an assertion that reads as
> more proof than it is — so it is written down as what it is.

The hazard this closes is the one §12.26 named: **delete the defining sentence
from the owning page, leave `!%term%` in place, and every shingle guard in the
repo goes quiet.** Thirty pins is the direct answer, and the indirect one is
stronger: with all six no-ops resolved, §12.28's suppression gate has **no
exemption list left**, so a page that stops saying a word it suppresses reds
corpus-wide, for all 109 markers.

#### Rule for (b), and it is the one that says what a glossary is for

**`flair` is a word no page of this site writes, and that is not a defect.** It
is the entry that proves the glossary is a **reference work rather than an index
of the site's own prose**: FLAIR is a word a reader arrives with, off their own
MRI report, and deleting the entry takes the definition away from the only reader
who was ever going to look it up. WI-519 raised this shape — *"an entry defined
and used in one place fires nowhere"* — and the answer is that the surface it
reaches them on is `/glossary`, not a page.

So **nothing is fixed for cause (b)**. What is gated is the thing that would make
it a lie: **no suppression marker may name a cause-(b) term.** A marker for a
word the page never writes suppresses nothing and reads as a decision somebody
made — which is exactly what `!%radiation mask%` was, and §12.28 called it the
purest of the six.

> **`/treatments/radiation-therapy` was cause (b) and should not have been.** The
> page describes the mask at length — 19 occurrences of *"mask"*, including *"a
> mesh mask is molded to your face"* — and never once wrote the entry's term. So
> the reader who has just been told what the thing is leaves without the word
> their appointment letter will use. It now says *"You may see it called a
> radiation mask"*, in the corpus's own naming idiom and inside the numbered list
> that already describes it, sourced by the citation the entry already carries
> (Roswell Park's *"Your Radiation Mask, What to Expect"*). The marker became a
> real suppression and the entry moved to cause (a). **The fix was on the page,
> and the marker was right all along about what it meant.**

#### Rule for (c): it is a finding about the PAGE, and the page's own standard says so

**`h3-g34` was the only cause-(c) entry, and the finding is not about the
entry.** Its two occurrences were an index link and a `### H3 G34` heading on
`/tests/molecular-markers`, and §12.11 already rules on both positions in both
directions: a heading is not prose, and no tooltip can fire in one. A reader who
skims that page's prose meets the word twice in places that cannot explain it.

**The page convicts itself, which is what makes this a page finding rather than
an editorial opinion.** `/tests/molecular-markers` carries fifteen suppression
markers in one run — it is the page that defines every marker term. Measured at
the start of this item: **ten of the fifteen fire without their marker and five
do not.** Ten sections say the term their `###` heading is; five did not. The
blanket had been written as the complete set while the prose had slack in it, and
the five are `CDKN2A/B homozygous deletion`, `EGFR amplification`, `H3 G34`,
`gene panel` and `methylation profiling`.

Each of the five now says its term in the prose it is headed with, in the page's
own existing construction (*"is written"*, *"are called"*, *"is called"*). All
fifteen markers now suppress something, the page's standard is met in fifteen
places out of fifteen, and `h3-g34` moved from cause (c) to cause (a) — **the
tooltip still fires nowhere, and that is now a decision rather than an
accident.**

> **AND IT CORRECTS §12.28's RECORDED CAUSE FOR ONE OF THE SIX.** `gene panel` was
> recorded there as cause (c), *"link text in the index list, and the `### Gene
> panels` heading"*. **That phrase is in no reader-facing position on that page,**
> because the heading and the index link both say *"Gene panels"* — plural — and
> `GlossaryMarker.BuildMatchers` uses `(?<![\w-])…(?![\w-])` rather than `\b`
> precisely so a hyphen is a word-joiner, so the trailing `s` rejects the match.
> It was cause (b) on that page, not cause (c). **The recorded cause was read off
> the page by eye and the matcher disagreed**, which is the same lesson as
> §12.28's `IndexOf` extractor one layer out: *a position is only evidence if the
> thing that decides it is the thing that renders.*
>
> The other four records were right, and the correction is deliberately not
> widened to them — /review caught this paragraph claiming TWO. §12.28's `EGFR
> amplification` record reads *"Same blanket. Measured: zero occurrences in this
> page's prose"* and makes **no position claim at all**, so there is nothing in it
> to correct; `H3 G34` and `methylation profiling` both really do match their
> heading and their index link. WI-536's rule, landing inside the ruling that
> cites it: *a justification can be false of the page it cites.* (And both phrases
> DO occur on that page, inside the fifteen-marker run itself — the claim that
> matters is reader-facing prose, so that is what this says.)

#### What was superseded, and the assertion list it was diffed against

WI-576's `TheEntriesWhoseTooltipThisItemSuppressedEverywhereAreStillReachable` is
**deleted**, which §12.26's fourth acceptance criterion asked for and §12.28
worked a precedent for. §12.28's supersession rule is that **a supersession is a
merge and the thing to diff is the ASSERTION LIST, not the purpose** — it learned
that by silently dropping an `Assert.NotEmpty` none of three deleted guards was
named for. So, item by item:

| the old test asserted | where it is now |
|---|---|
| the entry fires nowhere (two named slugs) | the partition, which **computes** the set of 26 instead of taking two names for it |
| the entry is still shipped | over all 26 |
| its definition is not empty | over all 26, plus a floor on its LENGTH — a definition that only restates the term is an empty row with extra steps |
| the owning page carries the sentence that defines the term (2 hand-written needles) | the 30-pair owner table, needles **derived** from the rendered artifact and each asserted UNIQUE on its page |
| most of the glossary still fires (`> 70`) | the literal **79**, beside the literal 26 |

And one assertion the old test did not have, which is the premise itself rather
than a fact about it: **`/glossary` is asserted to carry all 105 entries with
their definitions, against the served page.** Nothing in the repo did that over
the shipped glossary — `GlossaryPageTests` runs on a fixture entry — so a change
that filtered `GetTerms()` would have taken 26 definitions off the site with no
other surface to find them on, and stayed green. §12.27: assert the premise a
decision rests on.

> **The question to carry forward.** §12.28 asked which reader SURFACE the
> subject reaches. This item asked the next one down: **when a property is true of
> no member of a set, is that a decision or an accident — and which object is the
> finding about?** Twenty-five times it was a decision about the page, once it was
> a fact about the word, and once it was an accident of where the page put it. The
> three want three different owners, and a rule written for the count would have
> had to pick one of them for all 26.

### 12.30 A definition is prose, and the surface where the two limits meet (WI-582)

§12.26 established that a glossary definition has **a block's blast radius** — it fires as
a tooltip on every page that says the term. §12.28 made it the restatement probe's
**subject**. §12.29 found that for **26 of the 105** entries `/glossary` is the reader's
whole encounter with the word. Three sections had therefore established that a definition
is reader-facing prose on two surfaces, and nothing graded it: `CheckGlossaryTerm` printed
every grade as `reading grade N.N (not gated)` and failed on none.

**35 of the 101 it grades read above 6.0**, worst **9.3**, median 5.5.

| | before | after |
|---|---|---|
| definitions over 6.0 | **35 of 101** | **0 of 101** |
| worst grade in the glossary | **9.3** (`lomustine`) | **6.0** (`rescue-medicine`, untouched) |
| worst grade among the 35 | **9.3** | **5.9** (`chemoradiation`) |
| words across the 35 | 1,099 | **1,187** |
| words removed | — | **none** |
| entries in the 5.5–6.0 approach band | 17 | **19** |
| entries under the 20-word grading floor | **4** | **4** (the same four) |

#### The diagnosis held a fourth time, and the fourth surface has a CEILING

§12.22 claimed the cause was **sentence length and not vocabulary**. §12.23, §12.24 and
§12.25 tested it on three directories of page descriptions — 42 rewritten, not one word
removed. This is the fourth test and the first on a different artifact:

> **22 of the 35 grew, 13 held exactly level, and none fell.** `lomustine` went **9.3 →
> 3.4** and the word *lomustine* does not appear in its definition: it was bound by
> ***occasionally***, an ordinary five-syllable word that became *now and then*.
> `anesthesiologist` went 6.3 → **3.0** at exactly its old word count. The edits are a
> comma or a colon becoming a full stop, plus the connective a new sentence needs.

**AND THE GATE'S TEETH ARE WEAKER THAN THAT NUMBER LOOKS, which /review priced and
which belongs beside it.** Flesch-Kincaid rewards `", and"` → `". And"` with a large
grade drop for close to no change in comprehension, and **six of these entries now open
a sentence with *And*** — a pattern that appears in **zero** glossary entries before this
item. It is not new to the corpus: shipped page prose and the page `description`s WI-578
rewrote use it **78 times**, and §12.23 named it in writing as one of the sanctioned
edits (*"plus the connective word a new sentence needs — and, And, That, it"*). **So it
is house style with a ruling behind it, and the honest caveat is that "6.1–9.3 →
2.6–5.9" overstates what moved for the reader on those six.** The entries where
something real moved are the ones that gained a SENTENCE rather than a connective:
`lomustine` 2 → 4 sentences, `tumor-treating-fields` 3 → 5, `circumscribed` 2 → 3.

**And two hypotheses were killed by measurement before a word was written, both of which
look obviously right.** A reader looking a word up already has it in front of them off a
prescription or a report, so grading the definition for containing it looks like measuring
the wrong thing. It is not what is happening: **masking each entry's own term and aliases
to a one-syllable token moves the median by 0.1 and takes ZERO of the 35 under the limit.**
Only the four acronym-expansion entries move at all (`rano` −2.7, `nec` −1.8,
`radiologist` −1.8, `nos` −1.7), because **a definition mostly does not repeat its own
term** — the term is the heading on `/glossary` and the button text in a tooltip. Masking
every *other* glossary term a definition names takes 35 → **29** and the worst stays 9.3 —
and the argument for it fails anyway, because **a tooltip does not nest**:
`TermTooltipRenderer` writes the definition with `WriteEscape`, so a glossary term inside
a definition is not clickable and the reader is not one hop from its meaning.

**WHAT IS NEW ON THIS SURFACE IS THE 40-WORD CEILING** (§6). A page description had no
upper bound, so "split and never remove a word" had unlimited room. A definition lives in
a **20-to-40-word window**, and splitting costs words — a new sentence needs *And*, *That*,
*It*. Three entries now sit at exactly 40 (`brain-mapping`, `h3-g34`,
`tumor-treating-fields`) and two are where the two limits genuinely bind:

- **`h3-g34` was at 40 words BEFORE**, so its fourth sentence boundary had to cost
  **nothing at all**. Paid for by *one of the proteins that DNA wraps around* →
  *a protein DNA wraps around*: minus three words to buy *A histone is*, with the
  indefinite article keeping the hedge the plural carried. 6.1 → **4.3**.
- **`chemoradiation` is the entry the ceiling pins hardest**, at 5.9 from 7.8. It is 39
  words of *radiation*, *chemotherapy*, *glioblastoma* and *IDH-wildtype*, so its
  syllables-per-word floor puts 6.0 at about five words per sentence — eight sentences in
  forty words. It has six. **That is reported at Warn rather than nudged**, which is the
  honest shape: the entry passes and is visibly close.

  > **And it is the one place this item paid for a grade with a WORD, which /review
  > caught and which is recorded rather than quietly kept.** Its last clause was
  > *"depends on your **diagnosis**"* and now reads *"depends on your **tumor type**"* —
  > three syllables for four. The narrowing is small (the entry's own example,
  > *glioblastoma, IDH-wildtype*, **is** a tumor type under WHO 2021) but it is real, and
  > it is not optional: **restoring *diagnosis* at the same 39 words grades 6.5 and fails
  > the build.** Every other vocabulary swap across the 35 traded an ordinary word for an
  > ordinary word; this is the single case where the 40-word ceiling and the 6.0 limit
  > had no solution between them that kept the original noun.

> **No medical term was removed from any of the 35, no entry is exempted, and there is no
> exemption list.** The acceptance criterion worried that `pcv`, `lomustine`,
> `procarbazine` and `carmustine-wafer` *"cannot be written under 6.0 while still naming
> their subject"*. Two of the four are **under the word floor and not graded at all**, and
> the other two were written with their drug names intact: `lomustine` 9.3 → 3.4,
> `carmustine-wafer` 8.4 → 3.3 (bound by *dissolvable* and *operation*, both ordinary).
> **The drug-name worry did not survive reading the drug-name entries.**

#### The ruling, and the floor that is an exemption rather than a loophole

**A definition is held to 6.0, the same as a page**, with the three-level shape
`GradeFinding` has had for page bodies since WI-414 and `GradeDescription` took on at
WI-578 slice 3, on the same `WarnGrade`. A third surface disagreeing by a whole level
about what *"close to the limit"* means is three definitions of one idea.

**The grading floor is 20 words and `MinimumWordsToGrade` is 25**, and the comment beside
the old inline literal said *"the same reason `CheckRazorPage` refuses to grade under 25
words"* — **true of the REASON and not of the NUMBER**, and a reader of that sentence would
reasonably have read 25. The reason is shared and still stands. The numbers differ because
the artifacts do: raising this one to 25 would take **8 of the 105 entries** (grading 2.6
to 5.7 today) out of the graded set in exchange for tidiness. It is a named constant now,
with those eight listed beside it by name.

So **four entries are not graded** — `memantine` at 12 words, `procarbazine` 18, `pcv` 19,
`stereotactic-radiosurgery` 19 — and they are **NAMED rather than subtracted**
(§12.19/§12.24/§12.25). The fact that makes the floor an instrument instead of a hiding
place is pinned beside them: **three of the four would FAIL on sight if they grew into the
graded set** (`pcv` 16.1 — it is three drug names; `memantine` 10.4; `procarbazine` 7.6),
and `stereotactic-radiosurgery` would pass at 4.9. Crossing the floor is a broken build,
not an escape. **And those three are exactly the gap between the tool's number and a naive
sweep's**: grading all 105 directly reported 38 over the limit and a worst of 16.1, and
WI-416 is about precisely that — the second grader's cost was a wrong number that nearly
reached a doc.

#### THE HOLE THIS ITEM WAS PLANNED AGAINST, and the two places it is closed

§12.23 records a gate shipping on the wrong side of a word floor **once already**, with
`GradeDescription`'s own comment having warned since WI-575 that *scoring it as progress
would let a page buy its way out of grading by getting shorter*. **Writing a hazard down
does not guard it.** Here it is measured on this corpus rather than argued, by
`prove-gate.py` phase 2:

> Trim `vasogenic-edema` from 27 words to **19** and ContentCheck **exits 0**. The entry
> leaves the graded set, the over-the-limit count stays 0, nothing fails, and the only
> trace is one extra name in a line rendered as `  ok`.

So the set is gated where a corpus claim belongs — in the suite, against the real corpus —
and it is gated **as a SET and not as the number 4**. A count is satisfied by four out and
four in: trim one 38-word definition under the floor while `pcv` is grown over it and the
count never moves. `TheUngradedSetIsExactlyTheFourEntriesUnderTheWordFloor` reds in **both
directions**, and the break harness proves both (mutations 04 and 05).

**THE RESIDUAL, STATED RATHER THAN CLOSED.** The word floor covers the **35 entries this
item rewrote**, not all 101 graded ones. Truncating one of the other **66** from 38 words
to 21 passes everything: the grade improves, the entry stays graded, and the set gate
fires only below 20. That is the item's planned scope — a floor is a claim about a
measurement and there is no measurement of the other 66 to anchor one to — but it is the
residual of the hazard and not its closure, and the next item that rewrites a definition
should add its own rows.

**And the limit is effectively 6.049.** `FleschKincaidGrade` rounds to one decimal before
the `> FailGrade` compare, so an entry at 6.04 prints 6.0 and passes. Pre-existing for
page bodies since WI-414 and for descriptions since WI-578; it is load-bearing on 101
more strings now, and `rescue-medicine` sits at a measured 6.000.

**And the set gate does not see the other truncation**, which is why there is a second
instrument. A definition cut from 38 words to 24 is still graded, still under 6.0, still
over the floor — and has stopped saying something. §12.23 calls it *a shortening wearing a
split's clothes*, and notes that the only thing which caught one during slice 1 was a
`len(new) >= len(old)` assertion in that item's **git-ignored scratch script**. It is in
the suite this time: a **35-row word-floor table**, per entry, with the word count and the
grade each one held before. The two column checksums (1,099 and 1,185) are the part that
matters — the per-row assertions are all local, so **dropping one row's floor to let a
truncation through passes every one of them**, and only the checksum reds. Mutation 19 is
that exact edit, on the one row where every local assertion survives it.

#### Two instruments on an overlapping set is not a supersession

WI-581 floors the **26 glossary-only** definitions' **length in characters** at the term's
length + 20, to rule out a row that is only the term restated. This item floors **35
definitions' words**. **Ten entries carry both**, and §12.28's rule is that the thing to
diff is the assertion list:

| | WI-581's character floor | WI-582's word floor |
|---|---|---|
| set | the 26 that fire no tooltip anywhere | the 35 that read above 6.0 |
| property | the definition is not an empty row | the rewrite was not a truncation |
| unit | characters (term + 20) | words (26–40, per entry) |
| reaches entries the other does not | **16** | **25** |

On the overlapping 10 the character floor is slack by two orders of magnitude — the
tightest margin is `subtotal-resection` at **107 characters** (145 against a floor of 38) —
so it could not catch a truncation the word floor misses. And the 16 are why it is still
worth keeping. **Neither subsumes the other and nothing is deleted**; the relationship is
asserted rather than described, including the 107.

#### AND THAT NUMBER IS WHY `endings.py` EARNED ITS RUN, after four items of faith

The separate LF/CRLF suite run has been part of this harness since WI-569 and had never
found anything. It found this:

> The margin assertion first read `Definition.Length` straight and pinned **108**. A
> shipped definition is **hard-wrapped**, so its character count is one higher per line on
> a CRLF checkout than on an LF one — and **CI is Linux**. The test passed on the machine
> that wrote it, passed the whole 23-mutation break harness on both endings (every
> mutation in that table reds it either way, so neither pass distinguished anything), and
> **failed the LF suite at 107**.

**A WORD count is immune to this and a CHARACTER count is not**, which is the general
form: whitespace is whitespace to a splitter, and a byte is a byte to `Length`. The
measurement normalises line endings now and the number is 107 on both. The three loose
`> term.Length + 20` floors either side of it cannot flip on one character and are left
alone — the hazard is an EXACT character count, not any character count.

> **And the break harness could not have found it**, which is the part worth keeping. It
> asks whether a mutated tree FAILS a named test, and a test that already fails on an
> ending fails for free — WI-528 wrote that lesson down and this is the first time it has
> had a subject. Two instruments, two kinds of blindness.

#### What the drift detector caught, and the one the gates sent back

**§12.28's parallel-pair record earned its design on the first draft.** `adult-type` ×
`pediatric-type` and `h3-g34` × `h3-k27-altered` are licensed to share a frame **verbatim**
— all four read above 6.0, so all four were rewritten — and the first draft gave each pair
**two different frames**. That is the drift the record exists to catch, arriving by the
exact route §12.28 predicted: a fix for something else. `h3-g34`'s 10 windows are re-pinned
at **12**.

> **And the other pair was rewritten too and did not move**, which is the right behaviour
> and worth knowing. Its shared sentence was split at a comma, and
> `ShinglesOfReaderText` strips punctuation — so *"biology, not by the age"* and
> *"biology. Not by the age"* are the same windows. **A sentence split is invisible to
> this gate; a reworded frame is not.** The gate is broken from both sides anyway
> (mutations 07 and 08), because the invisible half is also where a real reword could
> hide.

**§12.26's page-restatement gate sent one draft back, and the fix is the one §12.28 names
as the wrong one.** `vorasidenib`'s first rewrite ended *"And it acts on the IDH change
itself."* — the back half of `/tumors/oligodendroglioma`'s own sentence (*"It is taken by
mouth and it acts on the IDH change itself"*), on a page that fires that tooltip. The
overlap was **seven words and sub-threshold before this item**; the connective a split
needs is what pushed it to eight.

> **The available fix was to change one word.** *acts* → *works*, and the window stops
> matching. That is paraphrasing to escape a shingle gate, which §12.28 calls **the
> beginning of drift** and §12.26 closed by naming. The entry was restructured instead —
> it names its own subject as the actor (*"The medicine acts on that change itself"*)
> rather than echoing the page's pronoun chain. **A gate that is satisfiable by a
> one-word dodge will be dodged; the record of refusing it once is worth more than the
> commit.**

#### The Warn band has 18 live subjects, and the counter-argument lost on the merits

§12.25 added the approach band to `GradeDescription` because a hard Fail at 6.0 with
nothing below it is a **cliff** — a description at 5.9 printed `  ok`, and one word took it
to a broken build. The same cliff exists here, and the same band answers it. But it lands
with **19 of 101 definitions in it** rather than §12.25's one, and `Program.cs` reasons in
its own comments that *a gate printing dozens of expected warnings trains people to skip
it*.

> It is shipped anyway, and the number is **printed rather than tidied away**. What WI-578
> refused to report as `ok` was a grade-**19.7 FAILURE**; a definition at 5.7 is correct
> writing near a line. The **17 page-body warnings already in this tool's output** are the
> same thing and have been treated the same way since WI-414. The alternative — inventing
> a third threshold for a third surface — is how one idea gets three definitions. The
> total went 21 warnings → **40**, with 0 failures.

**And `rescue-medicine` reads exactly 6.0**, untouched by this item: the one entry that
passes with no margin at all, and the band's clearest subject.

> **The question to carry forward.** §12.19 asked which pages a guard can fire on, §12.24
> which entries, §12.25 which are in no entry at all, §12.28 which reader surface, §12.29
> whether a property true of no member is a decision or an accident. This item asked:
> **what does leaving the gated set look like, and is there an instrument pointed at the
> leaving rather than at the set?** There were two ways out here and they need different
> instruments — one is a SET equality and one is a word count — and the number 4 could not
> see either.

### 12.31 A block's citations are the block's, and a flat list says otherwise (WI-574)

§12.10's last word on shared blocks is about citations: *"Block `sources` merge into every
including page and **render in the reader's source list**. A banned or dead URL added to a
block ships onto 24 pages while a test that reads only the page's own front matter stays
green. Check the blocks too."* That fold is right, and this section does not undo it. What
it rules on is where those citations then **render**, which three files recorded as wrong
and none fixed.

`/tumors/atrt` recorded it first and called it *"a corpus issue rather than one page's
accident"*. `/tumors/pediatric-brain-tumor` recorded it second. WI-568 made it third,
adding *"NIDCD: Vestibular Schwannoma (Acoustic Neuroma) and Neurofibromatosis"* to the
source list of eighteen hubs — including `/tumors/dipg`, where a parent reading
"Neurofibromatosis" in their child's source list has a question nobody asked for. The
backlog closed the third recording with the only rule that mattered: **recording it a
fourth time is not one of the options.**

**MEASURED FIRST, and the measurement moved the item.** 711 citation entries on 39 of the
55 pages are a block's rather than the page's own, across 8 blocks: escalation 247,
caregiver 222, mechanism 125, causes 66, crosswalk 20, tumor-board 16,
posterior-fossa-syndrome 9, spinal-cord 6. The worst two pages are
`/tumors/pediatric-brain-tumor` (44 own + 32 block = 76) and `/tumors/medulloblastoma`
(28 + 32); `/tumors/dipg` is 17 + 29.

#### The defect is the attribution, NOT the citation — and the difference decides the fix

The obvious reading of this defect is "an irrelevant citation on a page it has no business
being on." **That reading is wrong, and the measurement is what kills it.**
`blocks/mechanism.md` renders its entire location list on every including page, and one
entry of that list is *"The floor of the skull, below and behind the ear"* — hearing,
balance, the nerves to the face. NIDCD **is** that entry's source, and that entry **is** on
`/tumors/dipg`. The citation is honest, it is load-bearing, and the text it supports is
genuinely in front of the reader.

So nothing here is dead, foreign or droppable. **What was false was the attribution.** One
undifferentiated `Sources:` list makes every entry read as *what this page's subject traces
to*, and on a tumor hub that turns a block's skull-base citation into a claim about the
reader's own tumor.

That resolves the two options the backlog left open:

- **Attribute a block's sources to the block in the rendered list** — TAKEN. It is one
  change in one renderer, it drops no citation, and it fixes the sentence the reader
  actually mis-reads.
- **Let a block scope a source to the claim it supports** — REJECTED, and not on effort.
  Scoping is 711 editorial decisions that still have to render somewhere, so it does not
  answer the rendering question; it postpones it. And it buys the reader nothing the first
  option does not.

#### The trap: the fix that would switch eight blocks' gates off

The direct way to stop block citations rendering as the page's own is to stop folding them
into `frontMatter.Sources`. **That would be the worst available outcome**, because the fold
is what every downstream consumer and every §12.10 citation check reads. Un-folding would
make a dead URL in a block invisible to the page checks again — the exact hole §12.10's own
paragraph was written about — for eight blocks at once, while every test stayed green.

This is WI-582's carry-forward in a new costume: **a gate's scope can be narrower than its
name.** So the ruling is split deliberately:

> **`FrontMatter.Sources` remains the UNION, byte for byte unchanged.** `ContentPage` gains
> `OwnSources` and `BlockSources`, which **partition** that union elementwise and in order.
> The partition exists for the RENDERER alone. No gate's input changes.

`TheTwoListsPartitionTheFrontMatterExactly` asserts the partition on all 55 pages, which is
what makes "no citation was dropped, duplicated or reordered" a measured claim rather than
an intention.

#### Ownership is decided on the DECLARED side, and the corpus has the discriminating case

`/tumors/acoustic-neuroma` includes `[MECHANISM]`, so it inherits the NIDCD citation — and
it **also declares that same URL itself**, under its own title (*"Vestibular Schwannoma
(Acoustic Neuroma) and Neurofibromatosis"*, no `NIDCD:` prefix), because on that page the
citation is squarely about the subject. `SameSource` matches on URL, so the page's own
declaration wins: the citation stays in its **own** list, under its **own** wording, and is
not repeated below.

That is why the filter reads the declared list rather than the block list, and it is why the
NIDCD title folds onto **17** pages and not 18. A filter written the other way round passes
every other assertion in the item and reds only here. **The discriminating case occurs
naturally in the corpus**, which is worth more than a fixture: it cannot drift out of
relevance while the code it discriminates still ships.

**"Includes a block" and "renders a block's citation" are different sets.** Forty pages
include a block; `/tests/mri` is the fortieth and renders none, because the single source
its block carries is one that page already declares. Pinned, so the two sets cannot quietly
collapse into one again.

#### The gate that was off, and this item turns it on

`ContentPage.cshtml` carried **three words** of reader-facing prose before this item —
`Last reviewed:` and `Sources:`. `CheckRazorPage`'s floor is 25, so the file was reported
*"too little to grade"* and was **held to no reading level at all**. That is §12.22's and
§12.30's defect on a third surface: a line the reader meets that nothing grades.

The new label is written to cross the floor **on purpose** — 39 words, grading **4.0**,
inside both the 6.0 limit and the 5.5 warn band. Which means shortening it back under 25
would switch the gate off again, so the test asserts the gate's STATE and not just the
number: the finding must be a grade rather than the "too little to grade" Info.

**AND THE LABELS ARE GRADED A SECOND TIME, ON THEIR OWN — because here the gate's scope is
WIDER than the test's name**, which is the same defect as §12.25's running backwards.
`CheckRazorPage` grades every word of prose in the file, and today that *is* the four
labels. Add forty words of easy prose anywhere else in `ContentPage.cshtml` and the label
could be rewritten to grade 9 while the file average stayed Info and a test called
`TheProvenanceLabelsAre...BelowSixthGrade` stayed green. So the labels are pinned as a
literal array and graded alone (1.9) beside the file-level check. A gate whose subject is
*part* of a file needs an assertion over that part.

> **A COLON ENDS A SENTENCE FOR THE EXTRACTOR AND NOT FOR THE GRADER, and this item is the
> first place it could have mattered.** `RazorTextExtractor.Flush` treats `:` as an
> already-present terminator and appends nothing; `ReadabilityAnalyzer`'s sentence regex is
> `[.!?]+(?=\s|$)`. So colon-ended prose blocks **fuse** into one grader-sentence.
> **Measured before being assumed: it affects 17 of the 19 graded Razor pages, the deltas run
> 0.1–1.6, and NO page passes or fails because of it** — on a page with many real
> sentences, one fused pair barely moves words-per-sentence. It is therefore recorded as a
> latent hazard and NOT fixed here: a change to the grader's sentence rule re-grades the
> whole corpus and belongs to its own item. What this item does instead is write its label
> as real sentences ending in periods, so it does not rely on the seam. `/pm`.

#### What /review caught, and it was a FIX FOR MISATTRIBUTION THAT MISATTRIBUTED

`/review` (code-reviewer, 1 round): **1 blocker, 2 should-fixes, 5 nits, and the blocker was
reader-facing prose this item had just written.**

The shared label's third sentence read *"A few of them name other illnesses."* It was put
there to defuse exactly the alarm this section exists to remove — a DIPG parent meeting
"Neurofibromatosis". **It is FALSE on 14 of the 39 pages that render it**, independently
measured both ways: those fourteen (`/tests/biopsy`, `/tests/getting-ready-for-surgery`,
`/tests/waiting-for-results` and eleven `/treatments/*` hubs) fold citations from
`caregiver` and `tumor-board` ONLY, and all seven of those titles are about brain tumors,
cancer and caregiving. Nothing else.

**And on the other 25 it was worse than imprecise.** The only folded citations that name
another condition are `escalation`'s — CDC Stroke, CDC Seizure First Aid, Neutropenia,
Hydrocephalus — and that block cites them to tell the reader what to watch for **in
themselves**. The sentence told a reader that the four most action-bearing citations on the
page were about somebody else's illness. It also **ended the page on a mild alarm**, which
§12.6 forbids as a review rule rather than a preference.

> **THE RULING: a sentence added to explain a shared citation is itself prose asserted on
> every including page.** It is subject to §12.10's own test — *would this be true on the
> hub you have thought about least?* — and the label failed it on fourteen. A block's
> citations needed ATTRIBUTION, not a characterisation of what they contain: the renderer
> knows which list a citation is in and cannot know what the titles say. The third sentence
> is now *"They are here so you can check any part of this page"* — true on all 39, and an
> action rather than a warning.

The two should-fixes were both real and both taken: the labels are now graded on their own
(above), and each `<ul>` carries `aria-labelledby` pointing at its label, because a screen
reader navigating by list met two unlabelled lists of links and lost the attribution that
is the whole point of the item.

Of the nits, two were taken as correctness rather than polish. The front-matter list is now
replaced **unconditionally**: guarding it left `/tests/mri` — which includes a block whose
only citation it already declares — holding the deserialized list, the one case the comment
there claimed never happens. And the partition assertion now compares the lists by
**reference identity** rather than through a `(url, title)` projection, which pins all three
fields including `accessed` and reds on a swap for an equal-looking copy. The flat-list
assertion was also made markup-free: `DoesNotContain("<p>Sources:</p>")` only reds on a
byte-identical revert, and no `.cshtml`, page body or block body contains the bare string
`Sources:` — measured, because that is what makes the stronger form safe.

#### The residual, stated rather than glossed

**One shared group, not eight.** The reader learns that a citation belongs to a shared part
of the page, not WHICH shared part. Per-block attribution would need a reader-facing name
for each of the eight blocks — a content decision, not a rendering one — and the block
names (`mechanism`, `crosswalk`) are internal. The misattribution this section was
commissioned to fix is gone either way; naming the sections is an improvement on top, and
it is `/pm`'s to scope.

**A page's citations are still unchecked for title quality, and the asymmetry is measured
rather than asserted.** `CheckGlossaryTerm` **warns** on a glossary source whose URL has no
title (WCAG 2.4.4 — the link would have no accessible name), and `Glossary.cshtml` falls
back to the host so nothing ships nameless. `CheckPage` has **no** such check for a page's
sources — it only warns when the list is empty — and the two source lists this section adds
render `<a href="@source.Url">@source.Title</a>` with **no fallback**, so an untitled
citation would ship as a link with no text at all.

**It is LATENT, not live: measured at 0 of 1,401 page sources and 0 of the 8 blocks'
sources missing a title.** So this item adds no fallback and no gate — a guard with no
possible subject is the "prove it can fire" problem above, and the honest move is to record
the asymmetry with its measurement. `/pm`.

> **The question to carry forward.** §12.29 asked whether a property true of no member is a
> decision or an accident; §12.30 asked what leaving a gated set looks like. This item
> asked: **whose claim is this sentence, and does the surface the reader reads it on say so?**
> A citation can be perfectly honest, perfectly live, perfectly on-topic for the text it
> supports — and still assert something false, because the list it renders in has only one
> heading. Five recordings described the symptom; none asked who the list belonged to.

### 12.32 A seam has two sides, and a count is a claim about somebody else's list (WI-577)

§12.18 recorded a sellar seam it could not own: `/tumors/meningioma` had just claimed
the address *(near the pituitary and the crossing of the optic nerves)*, and
`/where-your-tumor-is#pituitary` — the page whose entire job is getting a reader from a
word on a report to a page about their tumor — did not name it. §12.17 recorded two
craniotomy and biopsy findings and said in writing that they *"belong to whichever item
next touches those pages"*. WI-569 touched `/treatments/craniotomy` for one clause and did
not take them, which is a tripwire firing into nobody's hands. This is both halves taken,
and the measurement moved the scope of three of the four acceptance bullets.

**THE RULING, IN ONE LINE: AN OPEN COUNT IS HONEST ABOUT A LIST BEING PARTIAL. IT DOES NOT
MAKE A PARTIAL LIST A ROUTE.** WI-567 round 6 found the sellar entry saying *"the two
growths here we have written about"* and fixed it to *"two of the ones we have written
about"*, because naming N asserts there is no N+1. That was correct, it shipped with a
ban list behind it, and it left the actual defect standing: the entry still **listed two
of the four** pages whose reader text claims that address. The reader it failed is
unchanged by the grammar of the sentence above them. **A hedge fixes what a list says
about itself; only naming the page fixes where the reader can go.**

#### The measurement, and the three numbers it changed

`.claude/work_files/wi577/measure.py` reads **reader text only** — the body after the
second `---` — because every phrase in all three questions is also quoted verbatim in
front-matter source notes, where it is a record rather than a claim.

**FOUR pages claim the sellar address, not two.** `/tumors/pituitary-tumor`,
`/tumors/craniopharyngioma`, `/tumors/meningioma` and `/tumors/cns-germ-cell-tumor`. **The
fourth is the one a phrase list cannot find**, and it is why this item's test derives the
set instead of describing it: cns-germ-cell-tumor says it in plain words — *"Just above the
hormone gland. The pituitary is a small gland under the brain"* — with **no report-word in
the sentence at all**, so a sweep for *sellar*, *sella turcica*, *pituitary fossa* and
*tuberculum* returns three pages and reads as complete. The derived form that does find all
four and nothing else is `\bpituitary\b` over reader text **with links stripped whole**,
and the stripping is not a refinement: a route's label is the destination's title, so
without it every page linking `/tumors/pituitary-tumor` counts as claiming the address and
the reverse direction passes over a dozen pages while asserting nothing.

**THE OUTWARD JOURNEY IS WORSE THAN THE RETURN ONE — 1 of 4 — AND IS LEFT ALONE ON
PURPOSE.** Only `/tumors/pituitary-tumor` links `#pituitary`. Meningioma routes to
`#skull-base` three times, which WI-568 settled deliberately in that term's favour;
craniopharyngioma and cns-germ-cell-tumor reach the page only through `#your-sight`. That
is a section-order decision for whoever owns those hubs, not a defect, so it is written
down here rather than fixed in an item whose acceptance is the return journey. **A seam
has two sides and they do not have to be closed by the same hand — but the side you are
not closing has to be named.**

**THE PLANNED-SUBTOTAL PRINCIPLE HAS SIX WORDINGS ON SIX FILES, NOT §12.18's THREE ON
THREE — AND THE SPLIT IS RECORDED RATHER THAN CONSOLIDATED.** Read together, the three
§12.18 names are **already right**: `/treatments/craniotomy` owns it (*"a subtotal
resection is not a failed operation"*), `/where-your-tumor-is` quotes EANO's limit
(*"aiming to get all of it must not cost you how you think or how your body works"*), and
`/tumors/meningioma` adds the meningioma-specific third (*"Taking out less, on purpose, can
be the plan rather than a disappointment"*). All three link `#how-much-came-out` with the
same label, and — the half §12.18 did not check — **all three agree on STRENGTH**:
craniotomy's *"sometimes settled before the day, and sometimes a decision made for you in
the moment"* and meningioma's *"sometimes decided beforehand and sometimes in the room"*
are one claim at one strength, which is what §12.10 actually asks. Each wording is keyed to
its own page's source, so collapsing them would delete the EANO quote from a page EANO is
cited on. **The three §12.18 missed do not route**: `/tumors/craniopharyngioma` (*"Many
teams now deliberately leave a piece alone rather than risk that"*),
`/tumors/acoustic-neuroma` (*"a piece was deliberately left behind to protect a nerve,
which is a recognized choice rather than a mistake"*) and
`glossary/subtotal-resection.md` (*"Surgeons stop short when going further would risk
something you would not want to lose"*).

**AND THE GLOSSARY ONE IS NOT §12.26'S QUESTION, WHICH IS THIS SECTION'S ONE REVERSAL
OF ITSELF** (`/review`). The first version of this paragraph said the glossary
definition *"fires as a tooltip on the owner page itself"*, and cited as the evidence
that `/treatments/craniotomy` writes `!%subtotal resection%Subtotal resection.` forty
lines above the owner sentence. **`!%term%` is the SUPPRESSION marker, not a firing
one** — WI-105, §6: *"`!%term%` anywhere on the page suppresses that term for the whole
page"*. So the cited evidence proves the opposite of the claim it was cited for, and
`CraniotomyPageRenderTests.TheWordsThisPageDefinesItselfDoNotAlsoFireATooltip` already
asserts `def-subtotal-resection"` is **absent** from the served page, in both
directions, on the §12.8 reasoning that a popover repeating the paragraph beneath it is
noise. There is no definition-and-prose-on-one-screen collision on that page, so §12.26
has no question to answer here: the glossary wording is simply a **fourth un-routed
instance** whose tooltip is deliberately suppressed where the owner sentence lives. What
is left for `/pm` is the routing, the same as the other two. For `/pm`.

*(A note claiming coverage a guard does not have tells the next reader to stop checking
— `WhereYourTumorIsPageTests`' own ruling. A note claiming a COLLISION that a guard
positively forbids is the same defect pointing the other way: it hands `/pm` an item
that cannot exist.)*

#### The tissue rule has TWO forms, and only one of them is false

This is the finding that reset §12.17's scope, and it is the reason the fix is on one page
rather than five. Five pages carry what looks like one sentence. They carry two claims:

| form | pages | true? |
|---|---|---|
| **certainty** — *"give you the name FOR CERTAIN"*, *"can say FOR CERTAIN"* | `/tests/mri`, `/tests/ct-scan`, `/treatments/watch-and-wait` | yes, with no exception |
| **naming** — *"can give it a name"*, *"can name it"*, *"It does not name it"*, *"What no scan can do is name it"* | `/treatments/craniotomy`, `/tumors/low-grade-glioma`, `/tests/planning-scans`, `/tests/ct-scan` | **no — the corpus asserts two exceptions** |

**`/tests/ct-scan` IS IN BOTH ROWS, AND `/review` IS WHY.** The first version of this
table filed it under *certainty* alone, on the strength of its *"can say for certain"*
sentence. But that sentence is the **second** one: the paragraph **leads** with a bolded,
flat **naming** claim — ***"What no scan can do is name it."*** — and that one is
falsified by the first exception **this very item published** on `/tests/biopsy`
(*"a scan may sometimes give enough information on its own"*). A page can carry both
forms, and sorting it by the sentence that is true hides the one that is not. So the
naming-form set carried to `/pm` is **three** pages, not two.

**The two exceptions, counted rather than assumed.** A place too risky to take a sample
from (`/tests/biopsy`, instantiated by `/tumors/dipg`'s *"If there was no biopsy, the
diagnosis comes from the MRI report"*), and markers alone
(`/tumors/cns-germ-cell-tumor`, from the EANO/SNO/Euracan consensus: *"reliance on tumor
markers in serum and CSF alone for diagnosis, thereby avoiding the need for a biopsy"*).
**`/treatments/watch-and-wait#without-a-sample` is NOT a third**, and it was checked
because the obvious reading says it is: it *upholds* the rule (*"Only a sample of tissue
can say for certain"*) and describes living with a working name, which is the
**consequence** of going without tissue rather than an exception to needing it.

**ONE OF THE THREE NAMING-FORM PAGES IS FIXED AND TWO ARE RECORDED, AND THE REASON IS A
TRAP WORTH KNOWING BEFORE TOUCHING THEM.** `/tests/planning-scans`'s sentence sits inside
the eight-word shingle that `CtScanPageTests` **and** `PlanningScansPageTests` both put on
`AssertDoesNotRestateTheCorpus`'s allowlist, whose own comment reads: *"load-bearing on
four pages. §12.10 says two pages must not state one safety claim at two strengths, so
where it is load-bearing on both, identical words are the right answer."* **Hedging one of
that trio would re-create the exact defect the allowlist exists to prevent.** So the trio
is a single decision and it needs its own item, while `/treatments/craniotomy` — the page
§12.17 named — is hedged here to the same strength as the page that owns the exception
list.

**AND THE REASON IS THE TWO-STRENGTHS ONE, NOT A SHINGLE ONE.** This paragraph also said
craniotomy was *"the only one of the three outside the shingle"*, and `/review` showed
that is false: `AssertDoesNotRestateTheCorpus` exempts a shingle when it is a **substring
of an allowlist entry**, and craniotomy's *"only a piece of the tumor itself, looked at in
a lab, can give it a name"* shares 8-grams with both allowlisted strings, so it is
**inside** that family too — and it was hedged here **invisibly to the gate**, because a
leading *"For most brain tumors, "* leaves the shared windows untouched. Hedging
planning-scans would be equally invisible. **So the gate was never what stood in the way.**
What stands in the way is §12.10 itself: one safety claim stated at two strengths across a
trio whose allowlist comment says the identical words are deliberate. That argument is
sound on its own and is the whole reason; the shingle framing was a mechanical-sounding
restatement of it that would have sent the next reader to check the wrong thing.

For `/pm`: `/tumors/low-grade-glioma`, `/tests/planning-scans` and `/tests/ct-scan`'s
lead sentence.

#### A count is a claim about somebody else's list

`/tests/biopsy#is-there-a-way-to-find-out-without-one` said *"There is **one** exception
worth knowing."* It was wrong on **three pages at once**, because this page **owns** the
exception list and two others route into its anchor without enumerating, by design:
`/tumors/all-brain-tumors` (*"There is a narrow exception, for a thing sitting somewhere
that would be too risky to take a piece of"*) and `/where-your-tumor-is` (*"if no piece is
taken, you get a working answer rather than a confirmed one"*). **An undercount on the page
that owns a list is an undercount everywhere that routes to it — which is also why fixing
it there fixes it everywhere.**

**And it is now OPEN rather than right.** *"There are exceptions worth knowing, and two of
them come up often enough to name here"* — because a corrected closed count is the same
defect waiting for the next item, which is precisely what *"one exception"* was. The ban is
page-scoped, not corpus-wide: `/treatments/steroids` and `/tumors/cns-lymphoma` each say
*"There is one exception"* correctly about their own subject, and §12.8's rule is that a
ban entry belongs on a list only if no correct sentence contains it.

**The second exception is ROUTED, and the narrowness of its source is why.** The consensus
sentence sits under that paper's *"Strategy for NGGCT"* heading, and the same paper says
*"Only marker-negative tumors should be biopsied"*. So `/tests/biopsy` says *"one rare
group of tumors"*, *"can sometimes be enough"*, attributes the group **in the sentence that
prints the claim**, and sends the reader to the page that owns the scope. Stating the scope
on the page that forwards the question is the two-strengths defect in its most tempting
form, because the forwarding page is the one a reader reaches first.

#### What the gates caught that a hand read did not

**A SENTENCE ADDED TO `/tests/biopsy` WENT RED ON `/where-your-tumor-is`.** The new
paragraph leads *"**A place that is hard to take a sample from.**"*, and
`AssertDoesNotRestateTheCorpus` reported it against `/where-your-tumor-is`'s route gloss —
*"covers the exception it names, which is a place that is hard to take a sample from"* —
eight words deep in three overlapping runs. **That gloss had TWO things wrong with it and
the gate could only see one.** It was a restatement, and it was **stale the moment the
count was fixed**: *"the exception it names"* was singular about a list of two, on the page
that sends the reader there. §12.17's own rule settles it — a route's label is the
destination's title, so the route needed no gloss in the first place. **A gloss of somebody
else's list is a copy of its length, and it goes stale when they change it.**

**A BAN LIST FORBADE THE CORRECTED SHAPE.** The craniotomy guard's first version asserted
`DoesNotContain("Only a piece of the tumor itself, looked at in a lab, can give it a
name.")` and went red on the fix, because *"For most brain tumors, only a piece of the
tumor itself, looked at in a lab, can give it a name"* **contains** the banned string. This
repo's other instance of this guard has already ruled on it (`ThePageNeverAssertsAClosedCount`:
*"a ban list that forbids the correct shape is worse than no ban list"*), and the remedy is
the same — a **windowed negative lookbehind** over the hedges, with the word boundary
**inside** the alternation.

**A CHECKSUM MOVED, AND THE SHAPE OF THE MOVE IS THE PROOF NOTHING BROKE.** The one new
citation took WI-574's `TheCorpusWideTotalsAreWhatWi574Measured` from 690 own / 1,401 union
to **691 / 1,402**, with the **block total UNCHANGED at 711**. That asymmetry is the whole
check: a source declared on the DECLARED side must land in `OwnSources` and nowhere else.
And the direction matters — WI-574's ruling is that **narrowing** the union is the one
change that gate exists to prevent, because every §12.10 citation gate reads the union.
**Updating a checksum is not the same as re-deriving one:** the new figures are the old ones
plus the citation in the diff, arithmetic first and measured second. An item that finds
itself editing those constants with nothing in its diff to account for the difference has a
code defect, not a stale number.

#### What `/review` caught, and every finding was about the guard rather than the list

One round. **It re-measured the whole inventory from the corpus and every claim the file
makes about the pages was right** — 464 rows, 148 figures, all 464 positions present, no
figure twice on a page, no slot in a gate. **Every finding was in the test, or in a
number quoted about the test's own subject**, which is the shape to expect when the
deliverable is a list and the only thing holding it up is a parser.

**1. A RULE ENFORCED ON A FILENAME IS A RULE A RENAME WALKS OUT OF.** The shared-scan
guard selected on `StartsWith("pd-scan-")`, so it covered 23 of the 32 kind-4 figures and
would have missed a diagnosis scan renamed to `pd-mri-glioblastoma`. Worse, it made the
DOC's stated rule false: `pd-hydrocephalus-ct` is kind 4 and is deliberately on two
pages. The guard now reads the KIND, with the mechanism pictures as a named exemption
carrying its reason — and the exemption is checked to still be repeating, so it cannot
become a place to park a name. **An exemption is an obligation, not a waiver.**

**2. TWO MEASURED NUMBERS WERE WRONG, AND BOTH CAME FROM THE SAME BUG.** `mechanism` is
854 words, not 864, and the raw-file total is 351 slots, not "about 342" — both were
measured before the word count was moved onto the composed body, by a walk that **dropped
the text following a `[BLOCK]` directive**. The numbers now come out of the same
measurement the test makes, and they are in the file's prose because they are the
argument for counting the composed page at all.

**3. A SELF-TEST THAT RE-IMPLEMENTED THE PREDICATE PROVED THE WRONG THING.** The
banned-source scan has a planted line to show it can fail — and the planted line was fed
to a copy of the check written inline in the assertion, not to the loop. So the one thing
that could kill the guard, the `- **Getting it:**` prefix it filters on, was the one thing
not covered: rename that label and 69 figures lose cover in silence. The scan is a
function now, called with the real notes AND the planted line, and the number of
`Getting it` lines is pinned at one per figure. *(§12.33 finding 1, re-dug: two checks in
sequence, and the second covering for the first.)*

**4. AN `Assert.Equal` ON TWO RECORDS WITH A LIST MEMBER IS ALWAYS FALSE.** The
CRLF-vs-LF test compared `WaveSummary` records, whose `Rows` is a list — compared by
REFERENCE, so identical inventories failed and different ones would have failed
identically. It flattens to strings now, with an assertion that the flattening can tell
two inventories apart. **A comparison that is always false proves nothing in either
direction**, which is the mirror image of a guard that cannot fail.

**5. THE THREE HAND-COPIED LISTS.** The draft alt texts were checked against a private
copy of `ContentFigures.AltPrefixesToAvoid`, and the banned-source scan against a third
hand-written copy of the NCI/AHFS/MedlinePlus ban. A sixth prefix added in production
would have left 148 drafts checked against a list that no longer existed. Both production
lists are public now and the test reads them. *(`BannedImageSourceHosts` was already
public for this reason and had no consumer — now it has one.)*

**6. 150 DERIVED NUMBERS IN A FILE WHOSE PREMISE IS THAT UNGUARDED LISTS ROT.** The kind
table, both wave totals, all 148 `used by` lines, every `Lands at` filename and each
page's "N listed" were unchecked. They are pure functions of the file's own tables, so
there is no corpus-drift cost to pinning them and no argument for leaving them loose.
Pinned. The WORD counts are still deliberately not pinned, and the file says so.

**7. AND TWO §3B CONTAINERS THE INVENTORY COULD HAVE VIOLATED.** `ContentBlocks`
deliberately supports an INDENTED `[BLOCK]` directive so a block can be spliced into a
list item — and 97 of these slots sit at a directive line. Indent one and §3b fails the
figure, 37 slots at once in `caregiver`'s case. A directive is now only a valid position
at the margin, and a directive inside an `:::outlook` gate counts as being in the
outlook. Neither shape exists in the corpus today; both are mutation-proved.

**The break harness, after all of that: 58 of 58 red (29 mutations x LF and CRLF), no
survivors**, and the two the reviewer predicted would survive (the indented directive and
an un-negated cancer.gov pointer) are mutations 21 and 17.

#### Carried forward

**`/tumors/all-brain-tumors` glosses one of two exceptions** (*"There is a narrow
exception, for a thing sitting somewhere that would be too risky to take a piece of"*). The
indefinite article keeps it open, so it is not the `/where-your-tumor-is` defect and it is
**left as found** — but it is now a page that names one of a pair while routing to the list,
and whichever item next touches it should decide whether to drop the gloss the way this one
did next door.

> **The shape, across the five recordings before it.** §12.17 and §12.18 each wrote down a
> finding on a page they did not own; §12.27 found that a rule's instances were more
> numerous than the item naming them; §12.31 found a citation that was honest and still
> asserted something false. This item asked: **does the fix for a count fix the reader, or
> only the sentence?** The open form was the right fix to the sentence and changed nothing
> for the reader standing on the paragraph. A list can be grammatically honest about being
> partial and still be the wrong list.

### 12.33 A picture is a claim about a licence, and three of its four failures render perfectly (WI-561)

There was no image support on a curated page at all before this item. Markdig would
emit a bare `<img>` from `![]()`, nothing styled it, nothing carried a caption or a
credit, nothing sized it on a phone, and `print.css` had no rule for it. WI-562 is the
item that goes and finds ~30 real pictures; sourcing thirty images before any of that
existed would have been thirty images with nowhere to go.

**THE RULING, IN ONE LINE: AN IMAGE HAS FOUR WAYS TO BE WRONG AND THREE OF THEM RENDER
PERFECTLY.** A picture with no alt text looks finished and is invisible to a screen
reader. A picture with no credit looks finished and is somebody else's legal problem
(PLAN.md §5). A caption nothing grades looks finished and is reader-facing prose
outside the 6.0 gate — WI-575's `description` hole re-dug one surface over. Only the
fourth, a broken file path, shows a reader anything is wrong. **So every one of the
three silent ones fails the page by name**, at parse time, the way a missing shared
block has since WI-501 — which makes it a build failure in CI (ContentCheck turns a
`FormatException` into a Fail) *and* a loud failure at runtime, rather than a gate a
hand-edited page could walk around.

#### The authoring form, and why the credit is not next to the picture

One line of ordinary Markdown, alone in its paragraph: the brackets are the **alt
text**, the quoted title is the **caption**, and the `images:` front matter carries
`credit`, `license`, `license_url`, optional `source_url`, and `width`/`height`, keyed
by `src` (§3b). The split is deliberate: the Markdown line carries what the READER
needs and the front matter carries what the SITE needs, where ContentCheck and the
suite can see it per page. `wwwroot/img/cards/IMAGE-CREDITS.md` is the counter-example
— one file, nothing mechanically tying a credit to a picture, which is exactly the
shape WI-502 and WI-505 ruled against for citations.

**A `<figure>` may not live inside a `<p>`**, so the paragraph Markdig built is
REPLACED by a figure block rather than decorated — and the alt text is flattened to
plain characters at the same moment. That is not tidiness either: the alt text goes
into an HTML ATTRIBUTE, and anything that put markup in there (a glossary tooltip's
`<button>`, say) would be read out as angle brackets. The figure pass therefore runs
**after** `GlossaryMarker.Mark`, which strips the `%%` escapes from the same literals
and skips anything under a link — and an image IS a link, so the marker cannot reach
inside one in the first place. Two independent reasons it is safe, and the second one
is the one a test pins.

#### The five findings this item did not expect

**1. THE PRINT CAP LOST THE CASCADE, AND ONLY THE PDF SAID SO.** `print.css` wrote
`figure img { max-block-size: 4in }` against `site.css`'s `.figure__image
{ max-block-size: 70vh }`. One class beats two element names, so the screen rule won
and the inch cap did nothing — the picture measured **467px tall in the print layout**
with a correct-looking rule loaded. `max-block-size: 70vh` is meaningless on a sheet,
where the height IS a sheet. This is WI-560's lesson with the mechanism named: **a
print override written at a lower specificity than the screen rule it is overriding is
invisible everywhere except on paper**, which is why the rest of `print.css` is
`!important` throughout.

**2. THEN THE CAP SQUASHED THE PICTURE, AND THE SAMPLE HID IT.** Capped at 4in the
drawing printed into an **828×384 box for a 1200×675 picture**. The cause is a rule
nobody reads twice: **an HTML `width` attribute is a presentational hint that acts as a
SPECIFIED width**, so `height: auto` alone leaves the width fixed and a height cap
distorts instead of scaling. The fix is `inline-size: auto` on both axes, which puts
the attributes back to being only an aspect ratio to reserve space with. **The style
guide's sample is an SVG and letterboxed itself inside the distorted box, so the PDF
looked right** — a photograph, which is most of what WI-562 brings, would have been
visibly stretched. A sample that cannot exhibit the defect is a sample that certifies
it, so the ratio is now asserted from the printed layout rather than read off the
picture.

**3. A CAPTION WITH A DOUBLE QUOTE IN IT STOPS BEING AN IMAGE.** `![a scan](x.svg "a
"grade 2" tumor")` ends the title early, the line is no longer an image, there is
nothing in the parse tree to find, and a patient reads the raw Markdown off the page.
This is `ReaderGate.AuditFenceLines`' hole in a second component — *the likeliest typo
stops the node from being built at all* — so it gets the same remedy: the raw source is
audited beside the tree, and a line starting `![` with no figure to account for it
fails the page. (Write the caption in single quotes.)

**4. THE AUDIT HAS TO RUN BEFORE THE UNUSED-DECLARATION CHECK**, because an image line
that did not parse also leaves its front-matter entry looking unused. "You declared a
picture that is not there" is a true sentence about the wrong defect when the picture
IS there and the caption broke the line. Ordering two true error messages is not
cosmetic: the first one printed is the one an author acts on.

**5. THE DEV STYLEGUIDE ALREADY OVERFLOWS 390px**, before any figure was on it, so
"nothing scrolls sideways" would have gone red for somebody else's layout and said
nothing about this component. The phone test asks the question a figure can actually
answer — **did it make the page any wider** — by measuring `scrollWidth`, removing the
figure, and measuring again. A control, for the same reason WI-567 and WI-569 took
byte-identical control pages: without one, the number is not about the thing under
test.

#### What is required, and the three rules that are not obvious

- **`width` and `height` are required**, and not for tidiness: every figure below the
  first is lazy-loaded, and a lazy image with no reserved box shoves the text under it
  down the screen when it arrives — on a phone that moves the line the reader is on,
  for the reader this site is built for.
- **The first figure is eager and the rest are lazy.** "Below the fold" cannot be
  computed from Markdown, but it can be ordered: the first figure may be the picture a
  reader is already looking at, and deferring that one delays the only image some
  readers ever see. Everything after it is below the fold on a 390px screen.
- **`source_url` is the one optional field, and that is a reading of the acceptance
  rather than a gap in it.** The item asks that every image carry "a source and a
  licence". `credit` is the source in the sense that matters — *who made this, and who
  are we relying on* — and it is required. A URL is required for the LICENCE, which is
  the thing a reader or a lawyer has to be able to read. What is optional is "where we
  got it", because a diagram drawn for this site has nowhere to point, and a required
  field that half the corpus has to fake is a field that stops meaning anything.
- **The NCI/AHFS/MedlinePlus ban is enforced on the IMAGE's `source_url`, not on the
  page's citations.** cancer.gov is a cited source on most tumor hubs; only its
  *pictures* are barred (PLAN.md §5, §12.2 rule 6). A ban written one level up would
  have been a different, false rule — and it is §12.14's shape ("banning a citation is
  not fixing a claim") pointed the other way: ban the reuse, keep the citation.
- **An image inside a shared block is not supported**, in writing. A block's front
  matter carries only `sources`, so the credit would have to be declared by a page
  whose author never wrote the picture. It fails at parse time by name, and a suite
  check keeps every block free of image lines so the failure is never discovered by a
  reader.

#### What the break harness found, and all four were about the tests

Round 1 ran **78 breaks (39 mutations × LF and CRLF) and 70 went red**. The four
survivors appeared on both endings, and not one of them was a hole in the mechanism —
every one was a guard that could not be shown to work:

1. **A TEST THAT PASSED FOR THE WRONG REASON.** The empty-alt test wrote `![](src)`
   with no caption, so deleting the alt check entirely still threw — on the MISSING
   CAPTION, one check later — and a bare `Assert.Throws` cannot tell two
   `FormatException`s apart. **Two checks in sequence means the second one can cover
   for the first**, and the only defence is to assert the message. (§12.17's "assert
   the absence, do not infer it", in the exception channel.)
2. **A STRING SLICE IS NOT A NESTING CHECK.** Swapping `</figcaption>` and `</figure>`
   produces malformed markup in which the credit is still *between* `<figcaption` and
   `</figcaption>`, so the slice-based test passed. The property is an ORDER of four
   landmarks, and it is now asserted as one.
3. **A GUARD THAT COULD NOT FAIL.** "A figure on its own prints on one sheet" was
   printing the style guide's LANDSCAPE sample, which the column width caps long
   before it is tall enough to spill — so the harness removed the 4in height cap
   outright and the PDF was still one sheet. The test now makes the picture portrait
   before printing it, which is the shape the cap exists for. *(WI-569: a property
   guard that has never been seen to fail has not been shown to work.)*
4. **TWO DECLARATIONS OF ONE DECISION, NEITHER PROVABLE.** `inline-size: auto` was
   written in `site.css` AND in `print.css`. `site.css` carries no media attribute, so
   its copy already applies on paper and **no single edit to either file could bring
   the squash back**. The duplicate is deleted: the decision lives in `site.css`, where
   the screen rule it has to beat lives, and `print.css` restates only what genuinely
   differs on paper. **Defence in depth and an unprovable guard look identical from
   inside the code; the harness is what tells them apart.**

A fifth finding came out of the same pass, from reading rather than from a survivor:
**the alt-prefix ban forbade a correct shape.** "Diagram of…" was banned alongside
"Photo of…", but a screen reader announces *image*, not *diagram*, so naming the KIND
of a drawing is information (WAI's own tutorials write "Chart: …"). Only the words
that duplicate the `img` role are banned now, and there is a test for the allowed
side — `ThePageNeverAssertsAClosedCount`'s rule, one component over: **a ban list that
forbids the correct shape is worse than no ban list.**

#### What `/review` caught, and every one of them was a page that RENDERED

One round, one blocker and seven should-fixes, and the shape they share is the
item's own ruling pointed back at it: **the failures that matter here are the ones
with no symptom.**

**THE BLOCKER: the raw-source audit was defeated by two characters.** It counted
lines whose first non-space character was `![`, so `- ![a scan](x.svg "a "grade 2"
tumor")` — or any prose in front of the image — shipped the raw Markdown to a reader.
That is finding 3 above, surviving the fix written for it. The audit now scans the
PARSED TREE as well, and the tree pass had a second defect that only probing the real
parser found: **Markdig splits a failed image into `![` and the rest**, so a
literal-by-literal scan matched neither half and the whole pass was dead. The
literals of a block are joined first. Both passes stay, because neither sees what
the other does — a four-space indent makes the line an indented CODE block with no
inlines at all, which is ReaderGate's recorded case and only the line count catches
it.

**A null key was a 500, not a message.** `images:` with nothing under it makes
YamlDotNet assign null OVER the property initializer, and the figure pass then threw
`NullReferenceException`. That is not a worse error, it is a different failure mode:
ContentCheck catches only `FormatException`, so the GATE would crash instead of
failing the page; `ContentStore.GetPage` catches only `IOException`, so a reader
would get a 500 on a medical page; and `SearchPages` catches only `FormatException`,
so one dangling key would take **site search down for every page**. The property
null-coalesces on read now. *(`sources`, `tags` and `disclaimers` have the same
hole and are left as found — for `/pm`.)*

**THE LICENSING BAN WAS ONE-DIRECTIONAL, on the one rule that is somebody else's
legal problem.** It read `source_url`, which is optional — so an NCI diagram
committed to the repo and credited honestly as "National Cancer Institute", with no
source URL, rendered on a green build. The credit is now scanned too. The field a
person fills in honestly is the field to read.

**A figure was accepted anywhere a paragraph can live** — a bullet, a blockquote, a
TABLE CELL — credited, but in a shape nothing styled or measured. The table cell is
the one that bites: a 1200px picture in an auto-layout `<td>` is the classic way to
send a 390px screen sideways, and the phone test only ever sees the top-level case.
A figure must now be a top-level block or inside a `:::outlook` gate, and there is a
test for the ALLOWED side as well as the forbidden one.

**And two more assertions that could not fail**, after the harness had already found
three: the print cap was asserted on the landscape sample, which prints 374px against
a 384px cap and so never reaches it; and `Assert.Contains(". ", …)` as the
"two sentences, not one run-on" check is true of every output `ExtractSentences` can
produce. Both are now exact measurements — the cap on a genuinely tall picture, the
grader on the whole string.

*(Also fixed, each one small and each one silent: an HTML entity in alt text was
dropped rather than transcoded, losing characters inside the one attribute a blind
reader has; `src` accepted `/appsettings.json` and `/img/../secret.svg`; one
declaration could render two figures while the corpus sweep asserted one; a missing
`FigureExtension` dropped a picture AND its credit with no error, so the pipeline now
verifies itself at start-up the way the reader-choice gate does; and nothing compared
the declared `width`/`height` against the actual file, which defeats the entire
stated reason those fields are required — the test now reads the SVG, PNG and JPEG
headers.)*

#### Carried forward

**The reverse direction of the file sweep cannot be mutation-proved.** "A file in
`wwwroot/img/figures/` that no page declares" needs a file to APPEAR, and the break
harness only edits existing files. It was proved by hand instead (drop a stray file
in, watch the test go red, delete it) and that is recorded in this item's
`mutations.py` rather than left as a blind spot.

**No real image ships with this item**, by design — the one picture on the site is a
hand-drawn placeholder under `/img/dev/`, served only by `/dev/styleguide`, which 404s
outside Development. WI-562 is next, and the thing to carry into it is finding 2: the
first real photograph is the first time the shape of a figure is tested by something
that cannot letterbox itself.

### 12.34 A slot is not a file, and the rule that scaled itself scaled 17x (WI-562)

WI-562 is the inventory: for every curated page, how many figures it wants, which
heading each one follows, which of the four kinds it is, and one sentence of draft alt
text. **It chooses no pictures** — that is deliberate, and it is the whole item. The
deliverable is `docs/images-needed.md`, held to the corpus by
`ImagesNeededInventoryTests`.

**THE RULING, IN ONE LINE: THE DENSITY RULE IS NOT WRONG, THE CORPUS IS SEVENTEEN TIMES
BIGGER THAN THE TABLE IT WAS MEASURED ON.** One figure per ~500 words, minimum one per
page, over the corpus as it stands is **461 slots across 50 reader-facing pages** —
against the "~30 real pictures" §12.33 wrote down eight days earlier. The ticket
anticipated growth in as many words (*"the rule moves it from one slot to four without
anyone editing this ticket"*); it just did not anticipate 217,148 words of reader-facing
body text against the 13,133 its own table adds up to. A hub is not four slots now. It
is eleven to fifteen.

What makes 461 finite is that **a slot is not a file**: they resolve to **148 distinct
figures**, because the corpus is templated (§12.3's seventeen sections × 23 hubs, the
test/treatment pages' repeated shapes, and eight shared blocks composed in dozens of
times). One photograph of two people talking at a kitchen table discharges 37 slots.

#### The four findings, and three of them are about counting

**1. THE COUNT IS ONLY RIGHT IF YOU COUNT THE WORDS A READER READS.** The raw `.md`
files total **351** slots; with the §3a blocks composed in it is 461. `caregiver`
alone is 413 words on **37 pages**, `mechanism` 854 on **18**. Counting the files rather
than the pages would have left the longest unbroken stretches on the site uncovered —
which is the exact defect the item exists to fix, and worse, those stretches are
*identical* on every page that includes them, so the omission would have been
systematic rather than scattered.

**2. §3B BARS AN IMAGE IN A BLOCK, SO THE WORDS A BLOCK CONTRIBUTES DEMAND A SLOT THE
BLOCK CANNOT HOST.** The slot is hosted by each *including page*, at the directive line,
declared in that page's own front matter. That is not a workaround — it is what makes a
shared figure correct rather than a compromise: **the same words get the same picture**,
and the credit still travels with the file on every page that shows it (§12.33's reason
for putting the credit in front matter at all).

**3. A SHARED PHOTOGRAPH IS HONEST. A SHARED SCAN OF A DIAGNOSIS IS A FALSE CLAIM.** An empty waiting
room is an empty waiting room on all 50 pages. An MRI of a glioblastoma on the
meningioma page says *this is what yours looks like*, and it is wrong. So reuse is free for
everything except a picture **of the diagnosis**: `pd-scan-*` has one member per hub, 23
of them, each licence-checked on its own, and no member appears twice. **The other nine
kind-4 pictures illustrate a MECHANISM rather than a diagnosis and may repeat** —
`pd-hydrocephalus-ct` is big fluid spaces, true on the shunts page and on the location
page, and neither claims it is the reader's scan. `/review` was right that enforcing
this on the `pd-scan-` NAME made it a rule about filenames that a rename walks out of;
the guard reads the KIND and carries the mechanism pictures as a named exemption with
its reason, in the shape `TumorHubTemplateSweepTests` ruled on — **an exemption is an
obligation, not a waiver**, so an exempted picture that stops repeating has to be
deleted from the list rather than left parked in it. This is the one place in the inventory where the per-page work
does not collapse — and it is the slowest wave per picture for exactly that reason.
Pointed the other way, it is also what shrank the location family from 23 drawings to
**one master map, 13 shaded variants and a second labelling**: a region is a region, so `dia-region-sellar`
is honest on both the pituitary and the craniopharyngioma page.

**4. NO FIGURE GOES INSIDE AN `:::outlook` GATE, ON ANY PAGE.** §3b permits one. The
outlook section's subject is how long people live, and the only thing a picture there
could illustrate is a prognosis — the one claim the whole anti-hype apparatus exists to
keep off these pages. Every hub's *"What might happen over time"* is deliberately empty
of slots. **The first draft of the guard for this measured nothing**: it looked for
headings *between* the fences and found none on any of the 55 pages, because the corpus
writes the heading ABOVE the fence every time. A check that returns an empty set over
the whole corpus passes forever, so the rewritten one asks which heading *owns* the
gate, and a second test asserts a floor of 20 pages having that shape before the first
one is trusted (WI-569: a property guard that has never been seen to fail has not been
shown to work).

#### Why a list gets a test, and what it does not pin

A list about 55 files that nothing reads goes stale the week after it is written. The
guard holds six things: coverage closed in **both** directions (a page added later
cannot acquire "no pictures needed" by being forgotten); the density floor re-measured
from the live corpus with blocks composed in; every slot position checked against the
page, so a renamed heading fails rather than leaving a position nobody can find; every
draft alt text and caption graded at the same **6.0** the page is and held to §3b's
figure rules; no slot in an outlook gate; and one `pd-scan-*` per page. It also reads
the sourcing notes for a pointer at a banned source — `ContentFigures` enforces the
NCI/AHFS/MedlinePlus ban on a committed image's `source_url` **and** its `credit`, but
nothing would have caught a *note* telling the next person to go to cancer.gov for a
brain diagram, and the note is read weeks before the front matter is written.

**What it deliberately does not pin: the word counts the file quotes.** They are a
measurement at a named commit, and pinning them would turn every ordinary content edit
anywhere in the corpus into a red build in this one file. The floor is pinned; the
snapshot is documented.

**Five pages get no slots and that is a ruling, not an omission.** `/about`, `/digest`,
`/how-we-write`, `/privacy`, `/terms`: "minimum one per page" is a rule about pages a
reader reads **for care**, not about the terms of service. The set is closed in the
test, each member carries a written reason, and a new page cannot join it.

#### Carried forward

**No picture is sourced here**, by design — that is the next job, and the inventory
orders it into three waves (49 figures in wave 1, which is where every shared-block
drawing sits). Carried from §12.33 and still true: **the first real photograph is the
first time the shape of a figure is tested by something that cannot letterbox itself.**
79 of the 148 figures are a DRAW or a BUILD rather than a search, and the catalogue
opens each of those entries with the word, so nobody discovers it halfway through.

**WI-572's diagram is `dia-brain-regions`,** the master of the location family and the
most load-bearing drawing on the site: 14 further files and 33 slots hang off it.
