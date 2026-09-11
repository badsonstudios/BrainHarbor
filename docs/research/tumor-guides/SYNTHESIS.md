# Tumor guides — research synthesis

Date: 2026-08-30. Inputs: five parallel deep-research tracks, full reports in this
directory:

- `glioma-family.md` — glioma, LGG, HGG, astrocytoma, oligodendroglioma, glioblastoma
- `meningioma-mets-general.md` — meningioma, brain metastases, the "no name yet" page
- `treatment-library.md` — the treatment pages
- `tests-library.md` — the tests/workup pages
- `patient-questions-and-ia.md` — what to answer, in what order, in what words

Decisions already made with Dan (2026-08-29/30) and assumed throughout:
shared treatment + tests libraries with tumor hubs linking in; explain prognosis
concepts but publish no figures; all 24 tumor types at full depth; deepen the
existing `/tumors/<slug>` pages in place; name drugs and side effects but no
doses or cycle schedules; tests get their own pages; `all-brain-tumors` becomes
the general page; keep the grouped index and add a picker.

---

## 1. The strategic finding

**The readability gap is measured, published, and total.**

Across 91 US brain tumor centers and 8 patient organizations, mean Flesch-Kincaid
grade level was 11. Fewer than 10% of center sites reached 8th grade, and **no
patient organization did** (`academic.oup.com/nop/article/12/5/901/8124733`). A
separate analysis of 180 documents from 50 academic centers found median FKGL
12.5, with treatment content (12.9) harder than diagnostic content (11.6)
(`pubmed.ncbi.nlm.nih.gov/31785430/`). For prognosis material specifically,
neither web pages nor ChatGPT met the AMA 6th-grade or NIH 8th-grade benchmarks
(`pubmed.ncbi.nlm.nih.gov/40543265/`).

A brain tumor library that actually holds 6.0 would be, on the published record,
the first. That is the same shape of finding as P4's "nobody runs a daily
plain-language research feed" — an uncontested niche, not a crowded one.

Supporting audience facts: 36% of US adults sit at Basic or Below Basic health
literacy; cognitive impairment affects 27–83% of adult grade 1–3 glioma patients
and up to ~90% of GBM patients *before* treatment; the shock of diagnosis
independently degrades comprehension. The 6.0 gate is not a stylistic
preference for this audience — it is the access requirement.

---

## 2. Page inventory — 53 pages

Grew from the ~40 estimate. The growth is almost entirely in the libraries, and
it is the research disagreeing with my guesses rather than scope creep.

### 2.1 Tests and workup library — 12 pages

| # | Page | Notes |
|---|---|---|
| T1 | MRI — what it's like and what to ask | Serves diagnosis, planning and surveillance; one page, not three. Covers the navigation/"stealth" scan the day before surgery, which baffles people. |
| T2 | CT scan | Deliberately short. Its job is mostly retrospective — explaining the ER scan that started everything. |
| T3 | Extra scans for planning | fMRI, DTI/tractography, MR spectroscopy, perfusion, PET. Merged: patients never get "an fMRI" alone, they get told "we're adding some sequences". |
| T4 | Biopsy — how tissue is taken | **Lives here, not in the treatment library.** See §4.1. |
| T5 | **Waiting for pathology results** | Split out of T4 deliberately. See §3.1 — highest-value page in the library. |
| T6 | Your pathology report and molecular testing | **Absorbs planned WI-444.** See §4.2. Size as two work items, not one. |
| T7 | Getting ready for surgery — tests and clearance | Bloods, ECG, anesthesia consult, the 2-hour clear-fluid rule, blood thinners. |
| T8 | Neurological and thinking/memory testing | Bedside exam + formal neuropsych. Same clinician conversation, same "am I being judged" anxiety. |
| T9 | EEG and seizure tests | Standalone: triggered by a distinct event, often the first test the patient ever had, carries driving consequences nothing else here does. |
| T10 | Tests for tumors in specific places | Vision / hormones / hearing, three labelled parts. Readers arrive by anchor link from the pituitary and acoustic neuroma pages, not by browsing. |
| T11 | **Follow-up scans and what the results mean** | Canonical home for RANO, pseudoprogression, radiation necrosis, scanxiety. See §4.3. |
| T12 | Lumbar puncture and spinal fluid tests | Minority of readers, distinct procedure, distinct risks. |

### 2.2 Treatment library — 17 pages

| # | Page | Notes |
|---|---|---|
| X1 | Watch and wait / active monitoring | The psychological load is the page's centre of gravity, not a footnote. See §3.4. |
| X2 | Craniotomy | Largest page. Absorbs gross total / subtotal / debulking vocabulary, 5-ALA, navigation, and LITT as a section. |
| X3 | Awake craniotomy and brain mapping | Split from X2: the patient has a *job to do*, unique preparation, unique risks. Merging buries the exact content that reduces fear. |
| X4 | Transsphenoidal (pituitary) surgery | New. Nothing like a craniotomy — no head incision, no shaved head, nasal recovery, hormone follow-up. Inside X2 it would mislead a large group. |
| X5 | Radiation therapy | Hub page. Absorbs IMRT, conformal, fractionation, whole-brain radiation incl. hippocampal avoidance + memantine. |
| X6 | Proton therapy | Split out: the reader's real question is *access* — who gets it, where, insurance, travel — which doesn't fit inside a modality page. |
| X7 | Stereotactic radiosurgery | Split out: the name misleads (people think it's surgery), the day is completely different, different tumor set. |
| X8 | Chemotherapy | One page for TMZ / PCV / lomustine / carmustine wafers. The shared content — blood counts, the fever rule, nausea — is where the value is. |
| X9 | Targeted and other systemic drugs | Led by "your tumor's test result decides this". Vorasidenib, bevacizumab, BRAF/MEK, CNS-penetrant drugs for metastases. |
| X10 | Tumor Treating Fields (Optune) | Standalone: it is a lived-experience decision (18h/day, shaved head, caregiver dependency), not a clinical one. |
| X11 | Steroids | Split from "supportive care". Carries a high-value "this might be the drug, not the tumor" message that needs its own URL. |
| X12 | Anti-seizure medicines | Same reason. Also answers "why won't they give me seizure medicine?" — see §3.5. |
| X13 | Shunts and hydrocephalus | |
| X14 | Rehabilitation | |
| X15 | Palliative care — and what it is not | |
| X16 | Fertility and treatment | New. Time-critical (options close once treatment starts) and systematically missed. Cuts across surgery, radiation and chemo. |
| X17 | Clinical trials as an option | "Ask your team" framing throughout. Never matching. |

**Explicitly NOT separate pages:** whole-brain radiation (section of X5), IMRT
and conformal (planning techniques, sections of X5), individual chemo drugs
(sections of X8), LITT (section of X2 for now), immunotherapy/vaccines/CAR-T
(near-entirely trial-stage in CNS — point at X17).

**Uniform section order for every treatment page:** (1) what it is, in one
sentence — (2) why it might be offered to you — (3) what happens, step by step —
(4) how long it takes, how long recovery takes — (5) what it feels like — (6)
side effects, soon — (7) side effects, later — (8) what tests you need first
(links into the tests library) — (9) questions to ask your team — (10) related
pages.

### 2.3 Tumor hubs — 24 pages

18 deepened in place, 6 written new (ATRT, chordoma, CNS germ cell tumor,
hemangioblastoma, remaining rare types, and `all-brain-tumors` as the general
"we don't have a name for it yet" page).

---

## 3. The content that wins

Things explained badly or not at all anywhere else, ranked by how much distress
they prevent. These are the reason this project is worth doing.

### 3.1 The pathology wait is fully explainable and nobody explains it

Frozen section turns around in ~20–26 minutes and agrees with the final
diagnosis ~90% of the time. Formalin fixation alone is 6–72 hours before
processing starts. Final histology needs ≥2–3 working days. IHC is another round
of slides. FISH ~11 days. Gene panel ~23 days. Methylation array ~23 days — and
these are **batched in groups of about 8, so samples queue**. Integrated
diagnosis median: 15 days in 2021, rising to 21 days in 2024, against a 14-day
benchmark that only 30% of centres meet.

Three things follow, and all three are publishable:

1. The wait is normal, measured, and has a mundane cause (batching).
2. **The answer arrives in layers, and the name of the diagnosis can legitimately
   change between layers.** Warning readers in advance defuses "they changed
   their story."
3. That is the system working correctly, not a mistake.

### 3.2 The retired-name crosswalk

Readers arrive holding paperwork that uses names WHO CNS5 retired in 2021. Two
directions, both of which make people think someone made an error:

- An IDH-mutant tumor is **never** called glioblastoma any more. "Secondary
  glioblastoma" / "glioblastoma IDH-mutant" is now *astrocytoma, IDH-mutant,
  grade 4* — and the rename happened *because* those tumors behave less
  aggressively.
- The reverse: a tumor that looked grade 2 or 3 under the microscope is called
  **glioblastoma, grade 4** if it is IDH-wildtype with TERT promoter mutation,
  EGFR amplification, or +7/−10. "Molecular glioblastoma."

Also: Roman → Arabic numerals (changed explicitly to prevent II/III/IV
transcription errors); grading now happens *within* a tumor type; NF2 renamed
NF2-related schwannomatosis (2022); hemangiopericytoma retired → solitary fibrous
tumor; oligoastrocytoma abolished; NOS vs NEC (test missing vs test done but
results don't fit — emotionally significant and universally unexplained).

### 3.3 "It looks worse but it isn't"

Pseudoprogression: 10–30% of glioblastoma patients on chemoradiation see their
MRI get worse within 12 weeks of finishing radiation, from treatment effect, with
real new symptoms. Radiation necrosis: 7–24% after SRS, looks exactly like
recurrence on MRI. Somnolence syndrome: profound drowsiness 4–6 weeks *after*
radiation ends, self-resolving — families read it as the tumor growing.

### 3.4 Watching is a plan, and the waiting has a measured cost

Meningioma growth data is genuinely reassuring: mean 0.796 cm³/year, mean
doubling time 21.6 years, 66% grow less than 1 cm³/year, growth often plateaus
around 1.5 years. Slow-growth predictors a patient can find in their own report:
calcification on CT, T2 hypo/isointensity, older age.

And the honest other half: watch-and-wait is associated with **4.26× higher risk
of a pathological depression score**. Naming that is a differentiator — every
other site treats it as a footnote.

### 3.5 "Is this the tumor, or the drug?"

The single most useful recurring frame, and it appears in at least five places:

- **Levetiracetam** causes irritability and aggression — families attribute
  personality change to the tumor when it may be the medicine.
- **Dexamethasone** causes proximal muscle weakness — mistaken for progression.
  **CORRECTED BY WI-524: this line used to say "in ~28%".** That figure is
  Sturdza 2008 (88 patients, brain metastases, palliative radiation, reported
  secondhand by cns.org), and the dossier cites it to PMC12406498, which
  contains the word "proximal" zero times. The published range across seven
  sources runs from 2% to 90%, so `/treatments/steroids` publishes no
  frequency and prints the direction the sources agree on instead.
- **SMA syndrome**: transient loss of speech initiation and one-sided movement
  right after surgery near the supplementary motor area, recovering over days to
  weeks. Unwarned patients believe they have been permanently disabled.
- **Cognition feels worse on day 2–3 after surgery, then improves.** One sentence
  prevents a panic.
- Patients and caregivers reported **over 60 distinct symptoms/side effects/
  events**, and attribution was the hardest part.

Also in this family: "why won't they give me seizure medicine?" There is a Level
A recommendation *not* to give antiepileptics prophylactically to a brain tumor
patient who has not had a seizure. Readers experience this as being denied
something.

### 3.6 Two cruel misunderstandings to dismantle

- **"Gross total resection" is defined by the post-op MRI, not by cure.**
  Microscopic disease remains in diffuse glioma.
- **"Low grade" is not "benign", and grade 1 ≠ grade 2.** Grade 1 gliomas are
  circumscribed and often curable by surgery; grade 2 diffuse gliomas are
  malignant and incurable. WHO actively recommends against the term "low-grade
  glioma", and the literature records patients "often told by physicians prior to
  oncology referral that grade II glioma is curable and perhaps not even
  malignant."

Related: registries say **non-malignant**, not benign (CBTRUS; 74% of tumors).
Adopt that word and explain it — it says what the tumor doesn't do without
promising harmlessness.

### 3.7 Somatic vs germline — the #1 family question

IDH, 1p/19q, TERT, EGFR, MGMT are changes *in the tumor*. Not inherited, not
passed to children. ~5% of primary brain tumors have known hereditary factors.
This distinction is not made clearly anywhere on the consumer web.

Validated plain-language framing (developed *with* low-literacy patients, and
aligned with the standardised lexicon adopted across 40+ cancer organizations):

> "The lab looks for changes in those instructions. These changes are like labels
> on your tumor. The labels help your team pick the treatment most likely to
> work. **The labels are about the tumor, not about you.** You did not do anything
> to cause them, and you cannot pass them on to your children."

### 3.8 The self-blame block

Every tumor page needs it. Ionising radiation to the head and a handful of
inherited syndromes are the only accepted risk factors; the vast majority arise
spontaneously.

- **Cell phones:** NCI's bottom line is that the evidence "suggests cell phone use
  does not cause brain or other kinds of cancer in humans"; the radiation is
  non-ionising and "too low to damage DNA"; Danish cohort, Million Women and
  COSMOS all null; glioma incidence flat while phone use exploded. Complication:
  IARC classified RF fields Group 2B in 2011 and patients have already found the
  disagreement — explain what 2B actually means (limited evidence, not
  established harm).
- **Head injury:** genuinely unsettled. Not an established cause; being
  researched; and a head injury often *leads to the scan that finds* a tumor that
  was already there.
- **Stress, diet, power lines:** no causal link established.

### 3.9 Caregivers have no lane anywhere

Care coordination and advocacy were raised **almost exclusively by caregivers,
not patients**. Caregivers performed dressing changes and medication
administration with no formal instruction, feared "offending the physician by
asking too many questions", and six of 25 families regretted not accepting
hospice sooner. Financial strain dominated real forum discussion.

Every comparator site silos caregivers into a "support" section. Giving them a
lane *inside* each tumor page is an unoccupied position. This strengthens rather
than duplicates the planned WI-446.

---

## 4. Conflicts between the tracks, and how I resolved them

### 4.1 Biopsy was claimed by both libraries

The treatment track listed "Biopsy" as a treatment page; the tests track listed
"Biopsy — how tissue is taken" as a test page.

**Resolved: it lives in the tests library (T4).** A biopsy treats nothing — it is
how tissue is obtained. The treatment pages link to it. Keeping it next to
T5 (waiting) and T6 (the report) also builds the deliberate three-page sequence
biopsy → waiting → report, which is the actual order the reader lives through.

### 4.2 WI-444 should be absorbed, not built separately

The planned WI-444 "What your pathology report says" and the molecular-testing
page are the same physical document — molecular results are *sections of* the
pathology report. Under CNS5 the integration is mandatory: the report **is**
histology plus molecular findings combined into one integrated diagnosis.
Splitting them would misrepresent how the diagnosis is made, and would leave a
report page that is a glossary of headings with the substance stripped out.

**Resolved: T6 absorbs WI-444. Close WI-444 in the backlog as absorbed, and size
T6 as two work items.** Mitigate the length problem structurally — report
orientation first, marker glossary second as an anchor-linkable list, and keep
the *waiting* content on T5 where it belongs.

### 4.3 Pseudoprogression / radiation necrosis appeared in four places

Claimed by the high-grade glioma page, the brain metastases page, the radiation
page, and the follow-up scans page.

**Resolved: T11 "Follow-up scans and what the results mean" is the canonical
home.** Everything else links to it. Same rule for steroids (canonical: X11) and
for the retired-name crosswalk (canonical: the glioma umbrella page, with the
per-tumor slice repeated only where it differs).

### 4.4 Section order — two proposals, one evidence-backed

The tumor tracks proposed long per-tumor section lists (up to 23 sections for
glioblastoma). The IA track proposed a 17-section standard order justified by
reading research.

**Resolved: the IA track's order is the skeleton; the tumor tracks' content slots
into it.** The key orderings it gets right and the tumor tracks got wrong:

- **"The short version" (3–5 sentences) goes at the very top.** Readers consume
  only 20–28% of a page and scan in an F-pattern; PEMAT requires a
  purpose-evident opening plus a summary.
- **"Is it cancer? What does the grade mean?" goes second**, not buried. It is
  the documented core confusion.
- **Symptoms and "when do I call for help right now?" sit high** — symptom
  queries outnumber treatment queries 2–5×.
- **"What causes it?" gets demoted or cut** — under 2% of cancer search volume,
  and the honest answer is usually "we don't know", which is bad content for the
  first three screens. (The self-blame block from §3.8 still appears; it just
  isn't near the top.)
- **"How common is it?" gets cut** or reduced to one clause. Filler competing for
  the 20–28% that gets read.
- **Outlook moves to position 12, behind a reader-choice gate.** See §5.
- Headings phrased as questions throughout.

---

## 5. Prognosis without numbers — validated, with a mechanism

The decision holds up better than expected. The clinical literature says
prognosis disclosure requires negotiation, readiness-checking and staged
disclosure across visits — three things a web page structurally cannot do.
Interview evidence from 25 newly diagnosed grade II–IV glioma patients:
**"not all patients want to know it all, one size does not fit all."** Some
wanted full honesty, some generalities, some only positive information.

That mandates **progressive disclosure**, which is a mechanism, not just a
policy. Concretely: put outlook late, behind an explicit reader-choice gate.

> "The next part is about outlook. Some people want to read it. Some people would
> rather not. You can skip it and come back another day. Nothing else on this
> page depends on it."

Draft wording for the concepts (full versions in `patient-questions-and-ia.md`):

**Median** — positive framing plus explicit right tail, the Kirkebøen framework,
which raised hopefulness *and* realism simultaneously:
> "Sometimes doctors line up how long a large group of people lived, from
> shortest to longest. The middle of that line is called the median. Half of that
> group lived less time than the middle. Half lived **longer**. Some people at the
> far end of the line lived much, much longer. The line is a picture of a group.
> It is not a prediction about you."

**Five-year survival** — benchmark, not deadline, plus the staleness caveat:
> "A five-year survival rate counts how many people in a study were still alive
> five years after they found out they had the tumor. It does **not** mean people
> live five years. Many live longer. Researchers picked five years so they could
> compare studies. It is a measuring stick, not a deadline. These counts also come
> from people treated years ago. Treatments have changed since then."

**Grade** — borrow Cancer Research UK's cell-behaviour framing and add the
missing sentence: *"Grade is a description of the cells. It is not a countdown."*

**One argument unique to gliomas, and it is ours to make:** published survival
numbers describe cohorts diagnosed under the *old* classification, and CNS5 moved
tumors between categories. The group a number describes may literally not be the
group the reader is in.

Also from the evidence: a 1,727-adult RCT found stating uncertainty had minimal
effect on understanding, trust, anxiety or decisions — **only stacking three
sources of uncertainty in one paragraph** measurably reduced perceived benefit.
So state uncertainty plainly; just don't pile three hedges together.

---

## 6. Boundary calls — RULED by Dan, 2026-08-30

Grouped from seven scattered flags into three decisions. **All three ruled as
recommended.** These are binding on the drafting prompt; do not re-open them
per page.

- **R1 — orienting durations: IN**, with lomustine reworded to carry the reason
  rather than the interval. All Gy and mg figures stay out.
- **R2 — procedural risk percentages: qualitative in the body.** A number only
  where it genuinely aids a decision, and then behind a disclosure with its
  source.
- **R3 — the two flagged numbers: direction only, no percentages.**

Full reasoning for each below.

### R1 — Orienting durations: IN

The agreed rule was "no doses, no cycle schedules". Some durations orient a
reader in time without prescribing anything.

*Clearly in under any reading:* "the beam takes a few minutes, most of the visit
is set-up"; "hair loss starts 2–3 weeks in and regrows in about 3–6 months";
"first night in ICU"; "staples out roughly 1–2 weeks"; "somnolence around 4–6
weeks after radiation ends".

*Clearly out:* any Gy figure, any mg figure, TMZ 5-on/23-off, PCV cycle
structure.

*The actual question:* "radiation is usually Monday to Friday for about six
weeks"; "Optune at least 18 hours a day"; "lomustine every six weeks"; "take
steroids in the morning". Each is technically a schedule. Each is also the thing
the reader most needs to plan their life around — and for Optune it *is* the
decision.

**Recommendation: in, with lomustine reworded to carry the safety reason rather
than the interval** — *"Lomustine is taken only occasionally, not every day,
because it lowers blood counts for weeks after you take it."*

### R2 — Procedural risk percentages: QUALITATIVE

Not prognosis figures, but they are statistics, they read hard at 6.0, and they
vary wildly by source. Examples: biopsy bleeding 7–10%, awake-craniotomy seizure
~8% (reported range 2.9–54%), shunt failure 27.8%.

**Recommendation: qualitative language in the body** ("bleeding is uncommon but
it is the main risk"), because the source spread is too wide to state a number
honestly. Where a number genuinely helps a decision, put it behind a disclosure
with its source.

### R3 — The two numbers most likely to mislead: DIRECTION ONLY

- **The WBRT cognitive figures** (63.5% vs 91.7% deterioration at 3 months, SRS
  alone vs SRS+WBRT). Flagged independently as the single most likely place for
  this content to accidentally mislead, **because most people in *both* arms
  declined.** Publishing the percentages without that context misleads in the
  reader's favour, which is still misleading.
- **The ~90% glioblastoma relapse rate.** Recurrence, not survival, so it is
  technically inside the rule — but it may carry the same weight.

**Recommendation: direction and affected domains only, no percentages, for both.**
"Recurrence is expected and is planned for" says the useful part of the second
one without the number.

---

## 7. Operational risks

### 7.1 NCI's patient PDQ is pre-CNS5 — flagged independently by two tracks

`cancer.gov/types/brain/patient/adult-brain-treatment-pdq` still uses Roman
numeral grades, treats "anaplastic astrocytoma" as a live diagnosis, describes
"mixed gliomas ... called oligoastrocytomas", still says "hemangiopericytoma",
and its brain-metastasis section is built around WBRT ± surgery for 1–4 lesions —
behind the 2022 ASCO-SNO-ASTRO position. It also does not reflect vorasidenib.

This matters because NCI is the source we would naturally lean on hardest.

**Rule to encode in the drafting prompt: use NCI patient PDQ for supportive-care
and general framing language, never for naming, grading, or radiation
recommendations.** Take naming and grading from CNS5 / cIMPACT-NOW, and
brain-metastasis radiation from ASCO-SNO-ASTRO.

Same class of problem elsewhere: ABTA's own downloadable PDFs still use
"oligoastrocytoma" and "anaplastic astrocytoma"; StatPearls' oligodendroglioma
chapter is internally inconsistent (correct molecular definition, then Roman
numerals and a retired term); NBTS's meningioma page publishes survival
percentages we must not carry over.

### 7.2 The best source is 403-blocked to automated fetching

**NCCN Guidelines for Patients: Gliomas** was named by three separate tracks as
the best CNS5-aligned, patient-level, licensing-clean source available — and it
returned HTTP 403 every time. Same for ABTA, The Brain Tumour Charity, Johns
Hopkins "Understanding My Report", and the RSNA fMRI review.

**Action: Dan opens these in a browser and saves them to
`.claude/work_files/research/sources/` before drafting starts.** Not optional —
the NCCN patient guideline is also the best available model for the
"questions to ask" blocks.

### 7.3 Vorasidenib is a differentiator and a maintenance risk

FDA-approved 6 August 2024 for grade 2 IDH-mutant glioma — the first glioma
approval in decades — and it is absent from most existing patient material
including NCI's. Getting it right is a genuine edge. It also means anything we
write about grade 2 glioma has a shelf life, and the `review_due` frontmatter
needs to mean something.

### 7.4 Review burden

53 pages with `reviewed` / `review_due` frontmatter on a 6-month cycle is roughly
9 page-reviews a month, forever. The existing pages set `review_due` at +6 months.
Worth deciding whether the libraries get a longer cycle than the tumor hubs, and
whether anything automated can flag "this page cites a source that has changed".

---

## 8. Recommended sequencing

53 pages at the 6.0 gate is months of work, not a phase-in-an-evening. Proposed
waves, each of which ends somewhere shippable:

**Wave 0 — foundations.** The standard section template; the six shared blocks
(`[CROSSWALK]`, `[CAUSES]`, `[MECHANISM]`, `[MARKERS]`, `[TREATMENTS]`,
`[PROGNOSIS-WORDS]`); the reader-choice gate component; the new glossary terms;
the drafting prompt encoding the §7.1 source rules and the no-prognosis
constraint. Everything downstream depends on this and it is small.

**Wave 1 — the spine, proven end to end.** T1 (MRI), T5 (waiting for pathology),
T6 (pathology report), X2 (craniotomy), X5 (radiation), X8 (chemotherapy), plus
**one** tumor hub taken all the way — low-grade glioma, since Dan already has the
short version to compare against. Ship, Dan reads it on localhost, adjust the
template before it is replicated 23 times.

**Wave 2 — the rest of the glioma family** (glioma, high-grade glioma,
astrocytoma, oligodendroglioma, glioblastoma) plus the treatment and test pages
they pull in: T4, T7, T11, X1, X3, X11, X12, X17.

**Wave 3 — meningioma, brain metastases, and the general page.** Pulls in T2, T3,
X6, X7, X9, X10, X13.

**Wave 4 — the remaining 17 tumor types**, now inheriting complete libraries.

**Wave 5 — the long tail:** T8, T9, T10, T12, X4, X14, X15, X16.

The picker control and the `/tumors` index update can land any time after Wave 1.

---

## 9. Backlog actions for /pm

1. Open a new phase (P5) for this work; it does not fit inside P4.
2. **Close WI-444 as absorbed into T6** with a pointer to §4.2.
3. **Re-scope WI-451** (survivorship) — T11 now owns follow-up scans, scanxiety,
   pseudoprogression and radiation necrosis. WI-451 keeps fatigue, driving,
   memory, work, and the years-long picture.
4. **Note the overlap with WI-446** (caregivers): §3.9 says caregivers want a lane
   *inside* each tumor page, which strengthens WI-446 rather than replacing it.
   Decide whether the per-tumor caregiver section is part of this phase or of
   WI-446.
5. **WI-450** (`/start`) should link into T5 and T6 at the pathology step.
6. Size T6 as two work items, not one.
