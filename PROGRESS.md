# BrainHarbor — Progress

> **The live state of the project.** Read this first in every session (via
> `/startup`). Updated the moment an item starts, finishes, or hits a blocker —
> a fresh session must be able to resume from this file alone.
> The item definitions live in [docs/backlog.md](docs/backlog.md).

## Current state

| | |
|---|---|
| **Phase** | M3 — Claude classification + plain-language summaries (M0–M2 complete & merged) |
| **Phase** | **M3 MERGED to `main`** (PR #5, 2026-07-31). Next: **M4 — Azure + trials + digest → v1 launch.** |
| **In progress** | **Nothing in progress.** WI-546 (CNS germ cell tumor) shipped 2026-09-22 and is CLOSED — do not reopen it. Next item is **WI-547 (Hemangioblastoma)**, not yet started. |
| **WI-546 (previous)** | **WI-546 is SHIPPED AND LIVE** (PR [#159](https://github.com/badsonstudios/BrainHarbor/pull/159) → `develop` (build-test pass 3m26s), release PR [#160](https://github.com/badsonstudios/BrainHarbor/pull/160) → `main` (build-test 5m11s and 4m15s), merge `7e8bf5a`, build-test success → deploy success, **first attempt — the eighth release in a row to deploy first time**). **The pre-merge smoke was required to FAIL and did**: `controls ok (2)`, then `/tumors/cns-germ-cell-tumor` 404 and the pediatric door missing, exit 1. **Post-deploy smoke clean**: the hub 200 at **61,718 bytes** past its 55,000 floor, all 18 positives present, all 20 absences absent; `/tumors` 17,656 → 17,749 and `/tumors/pediatric-brain-tumor` 59,126 → 59,444 as the two doors landed; every other route byte-identical to the baseline. **WI-547 (Hemangioblastoma) is next.** |
| **WI-543 (done — history)** | **Shipped and live 2026-09-19.** The strength split WI-536 handed over is SETTLED, and **it was never one claim** — two claims wore one sentence. Metastatic cord compression keeps an emergency-room rule (ACS, OncoLink); a PRIMARY tumor in or pressing on the cord gets a same-day call, because no fetched patient-facing source applies an emergency rule to a named primary cord tumor outside cauda equina (AANS, ABTA, Columbia, NCI PDQ list the signs and give no urgency rule — absences recorded in a 40-source table scored BEFORE drafting). ACS grades the two situations differently on its own two pages. A carve-out applies to BOTH and is stated as more urgent, not less. `/tumors/meningioma` re-tiered; `/tumors/brain-metastases` keeps its tier and gains the route it never named — both SHIPPED pages. `SpinalCordBlockTests`' pin **rewritten, not deleted**; WI-528's "three pages, one wording" retired deliberately. Blocks 5 in / 3 out, `[SPINAL-CORD]` EXCLUDED despite the name match. WI-412's taxonomy rule untouched and handproofed. **Proof:** suite 2,313/2,313; ContentCheck 273/0 at grade 5.8; **53 break mutations red on LF AND CRLF**; **6 render guards handproofed**; six rendered reads; privacy 0 findings; five `/review` rounds. **Smoke:** hub 11,164 → 49,261 bytes, controls first, both siblings grew and carry their new wording, 19 absences absent — and the same probe returned exit 1 with 23 failures when run against production BEFORE the merge. |
| **WI-542 (done — history, 2)** | none — **WI-542 is SHIPPED AND LIVE** (PR [#151](https://github.com/badsonstudios/BrainHarbor/pull/151) → `develop`, release PR [#152](https://github.com/badsonstudios/BrainHarbor/pull/152) → `main`, deploy `completed/success`, post-deploy smoke clean: the hub went **11,010 → 50,842 bytes** past its pre-merge baseline, controls green first so the verdict means something). **WI-543 (Spinal cord tumor, deepened) is next and NOT started**, and it inherits a live ruling from this item: `[SPINAL-CORD]` was EXCLUDED here even though CNS lymphoma is defined over the cord, because no source verified this session gives patient-facing cord guidance for it and the block's own source is about METASTATIC cord compression. WI-543 owns that subject and should re-argue it rather than copy the exclusion. |
| **WI-542 (done — history)** | **Shipped and live 2026-09-18.** PR #151 → `develop`, release PR #152 → `main`. **Proof: 119 break mutations red on LF and CRLF (3 first-pass survivors, all weak GUARDS, rewritten rather than the mutations weakened), 7 render guards handproofed, ContentCheck 273/0 at grade 5.7, suite 2,287/2,287, all THREE restatement probes clean with no allowlist, privacy scan 0 findings over 2,647 net-new lines, two `/review` rounds (1 blocker then none), post-deploy smoke clean.** **WI-542 (CNS lymphoma, deepened) — STARTED 2026-09-18**, branch `feature/wi-542-cns-lymphoma` (from develop at `f06f680`). WI-541 is SHIPPED AND LIVE — do not reopen it. **The stub is 47 lines and cites ONE source, `cancer.gov/types/brain` — BARRED by §12.1 for naming. That is the FIFTH stub in a row carrying that same barred source.** **`/treatments/steroids` IS THE DOMINANT COUPLING AND THE DOMINANT HAZARD**, and unlike every predecessor the coupling is ALREADY ASSERTED IN BOTH DIRECTIONS: `SteroidsPageTests.TheDoesNotTreatTheTumorClaimIsScopedAndCarriesItsException` READS this page and requires it to match `biopsy before starting steroids` and to contain `/treatments/steroids`. **Rewriting this stub can turn a SHIPPED page's test red**, so that guard is the first thing the draft must satisfy. The item turns on: biopsy NOT resection, and steroids before biopsy can obscure the diagnosis — to be VERIFIED against live publications, because a dossier is not a source (§12.8). **BASELINES MEASURED BEFORE A WORD WAS DRAFTED** (so the close-out can prove the new guards ran): suite **2,249/2,249** unfiltered, ContentCheck **273 checks / 0 failures**, and **0 restatement collisions on BOTH probes** with no allowlist — the shared guard and the page-local one, each validated against `/tumors/brain-metastases` as a positive control (10 files / 172 and 173 shared shingles), because a zero from a freshly retargeted tool is the most suspicious output this project produces. **VERIFICATION IS COMPLETE AND IT CHANGED THREE THINGS.** Three research agents returned 81 source files, and every load-bearing claim was then **re-fetched and re-read by me against the live publication** rather than taken from the pack (§12.8: a dossier cannot support a published claim). What that caught: (1) the pack's instruction *"the words corticosteroid and steroid do not appear in EANO"* is **false** — they appear as chemotherapy regimen components; the conclusion survives (EANO carries no pre-biopsy steroid guidance, and every recommendation in it is clinician-addressed) but an absence claim has to be asked correctly; (2) the "4-5 day hospital stay" comes from a paper whose PURPOSE is to move this treatment outpatient, so the page says "usually" and names the alternative, rather than publishing a rule its only source argues against; (3) the comparative "presents faster than most brain tumors" is **not carried at all** — no source makes that comparison. **WHO CNS5 names this entity and prints NO grade beside it** (verified live, by direct question), which is this page's answer to §12.3's section 2. **STATE: page written and green.** 31 content + 6 render tests, **37/37**. ContentCheck **273/0** at grade 5.8. **Both restatement probes 0/0 with no allowlist** — after a first draft that scored 38 and 47, twenty-four of them against `/tumors/acoustic-neuroma` because §12.9's template travels with the previous hub's sentences; closed in ONE pass from the uncapped report. **`/review` ROUND 1: 1 blocker (in two halves), 11 should-fix, 5 nits — all addressed; the reviewer also refuted six of its own suspicions with reasoning.** The blocker is the one worth remembering: **the page's own front matter declared that every "a steroid shrinks this tumor" sentence must be paired with "and it comes back", and not one of the four did it** — all four paired the shrinking with the DIAGNOSTIC harm and none with the clinical one, on the one page in the corpus where that sentence can be misread as treatment. **And the guard written to prevent exactly that could not fail**: it collected the matching sentences, asserted the list was non-empty, then checked the pairing words against the WHOLE PAGE, where "comes back" matches the `## If it comes back` heading. It was green on a page carrying precisely the defect its name forbids. **Six defects so far have been composed-page-only**, including a glossary tooltip that fired a *seizure* definition on a chemotherapy sentence, and a fix of mine that APPENDED instead of replacing — leaving the objected-to wording on the page while every gate stayed green. **CURRENT: full suite 2,287/2,287, ContentCheck 273/0 at grade 5.7, all THREE restatement probes clean.** (There are three, not two: `BrainMetastasesPageTests` strips neither headings nor link targets, which both my probes did — found via a red suite, now modelled in `heading-collisions.py`.) Next: `/review` round 2, then the break harness on LF **and** CRLF, handproof, close-out, privacy scan, commit, PR into develop, release PR, deploy smoke. |
| **WI-541 (done — history)** | **Shipped 2026-09-18.** **WI-541 (Acoustic neuroma, deepened) — STARTED 2026-09-18**, branch `feature/wi-541-acoustic-neuroma` (from develop at `100107b`). WI-540 is SHIPPED AND LIVE. **The stub is 1,372 bytes and cites ONE source, `cancer.gov/types/brain` — BARRED by §12.1 for naming, and naming is most of what this page does (acoustic neuroma → vestibular schwannoma → NF2-related schwannomatosis, renamed 2022). That is the FOURTH stub in a row carrying that same barred source.** **`/treatments/watch-and-wait` IS THE DOMINANT COUPLING AND THE DOMINANT HAZARD: 9 occurrences, more than every other file combined.** It already owns this tumor's watching material — the who-is-watched list ("Some acoustic neuromas, which doctors also call vestibular schwannomas"), "Hearing tests are part of watching an acoustic neuroma", the worry/low-mood research, the "many small ones do not grow, but hearing on that side often gets worse" passage — **and it already links BACK to `/tumors/acoustic-neuroma`, so unlike WI-540 the reciprocal door exists before the page is written. WATCHING IS THEREFORE ROUTED HERE, REVERSING WI-540's RULING**, and §12.10 says route rather than restate, which makes that page the first thing to read in full. It also cites EANO 2020 (PMC6954440) and records WI-521's no-per-tumor-schedule ruling, both of which this page inherits. Corpus mentions elsewhere: meningioma 3, ependymoma 2, craniopharyngioma 1, taxonomy 2 (the slug already carries `also: ["vestibular schwannoma"]`, so the rename is in the corpus vocabulary before the page exists). Research agent running against live publications, with the dossier trap named explicitly in its brief. **ALL EIGHT BLOCK RULINGS ARE DERIVED BY OPENING EVERY BLOCK, and they land on the same 4-in/4-out shape as WI-540 — which is flagged as a COINCIDENCE TO CHECK rather than a precedent reused, because two of the four exclusions rest on different reasoning.** IN: `[ESCALATION]` (§12.9 mandates it), `[MECHANISM]`, `[CAUSES]` (demoted), `[CAREGIVER]`. OUT: `[CROSSWALK]` (it is wholly the **2021 CNS** rewrite, while this page's rename is the **2022 NF2 nomenclature** — a different document, year and authority), `[TUMOR-BOARD]` (it describes a MEETING; this tumor's reality is ongoing hearing-versus-growth care), `[SPINAL-CORD]` (scoped to a tumor IN the cord; NF2's spinal risk is routed, not tiered, because composing a cord emergency onto every reader fails §12.10's "would this be true on the hub you have thought about least?"), `[POSTERIOR-FOSSA-SYNDROME]` (children, cerebellar mutism, after posterior-fossa resection). **`[MECHANISM]` IS THE INTERESTING ONE: it fits BETTER here than on WI-540 — its map's cerebellum and brainstem bullets are this tumor exactly — but WI-540's SCOPING NOTE WOULD BE FALSE HERE**, because that note says the map is "about growths elsewhere". The real scoping problem is inverted: the block opens "a tumor **in the brain**" and this one is extra-axial. Its own note, its own guard. **Diffing `[ESCALATION]` shows the gap that matters: it files "a sudden change in your vision" and says NOTHING about hearing, and puts new facial weakness at SAME-DAY.** Whether this page lifts either is for the sources to decide, on WI-540's apoplexy test — does a source give patient-facing ACTION guidance, or merely describe the event. **TOOLING: `wi541/` created; `all-collisions.py` and `shingles.py` retargeted (the copied versions imported from `wi540/` and defaulted to craniopharyngioma and the pediatric hub respectively — a no-argument run would have reported confidently on another item's SHIPPED page).** The five tools that mutate or serve content now REFUSE TO RUN until retargeted; **my first attempt at that guard was itself a defect**, breaking all five with a `SyntaxError` so they refused for the wrong reason and read as corrupt rather than disabled — the second guard-quality defect in two items, after WI-540's `|| echo` that announced a refusal and then proceeded. **COLLISION BASELINE: the stub is 99 shingles, ZERO collisions, with no allowlist — and the instrument was PROVED ABLE TO SAY YES FIRST** (control on `/tumors/brain-metastases`: 10 files, 172 shared shingles). **That control also paid for itself: the shared text clusters in the OUTLOOK-GATE OPENING** — "Doctors sometimes describe outlook using numbers…" is saturated across astrocytoma, diffuse-midline-glioma and meningioma, and §12.9 makes every hub write a gate. Caught before drafting rather than on pass six. **SOURCES ARE IN, AND THE EMERGENCY QUESTION HAS A THIRD DISTINCT ANSWER: THIS PAGE CARRIES EXACTLY ONE RULE, AND IT IS NOT A 911 RULE.** WI-539 had two tiers; WI-540 had two plus a named-but-untiered event; this hub has one. **IN: sudden hearing loss in one ear**, because both sources give explicit patient-directed instructions — NIDCD (NIH), verified live by me: *"you should consider sudden deafness symptoms a medical emergency and visit a doctor immediately"*, and HLAA: *"Any sudden hearing loss, especially in one ear, should be treated as a medical emergency."* **THE ROUTE IS THE RULING: HLAA names primary care, urgent care or an ENT and does NOT say emergency room.** So this tier cannot be modelled on either neighbour, and escalating it to 911 would be as much a defect as under-stating it. **OUT: brainstem compression and hydrocephalus** — EANO 2020 verified live as containing **no patient-facing urgency guidance at all**, every recommendation addressed to clinicians; its "surgery the only option" and StatPearls' "resection is indicated in… brain stem compression" are INDICATIONS FOR SURGEONS, not patient actions, and two patient-facing sources say the opposite (VeDA: *"Except for very large tumors, it is not an emergency to treat most acoustic neuromas"*; the US patient organisation for this tumor gives no urgency guidance at all). **OUT: hemorrhage entirely** — under 1%, presents as collapse, no patient guidance, pure fear with no action. **AND THE GAP IS RECORDED RATHER THAN PAPERED OVER: every source for the rule is written for NEW, UNEXPLAINED sudden hearing loss, and none says what an ALREADY-DIAGNOSED reader should do — which is this page's reader.** **WHO CNS5 VERIFIED AS AN ABSENCE: it prints NO grade beside "Schwannoma"**, so the Arabic-numeral CONVENTION is attributable to CNS5 and the grade 1 ITSELF only to the clinical references — citing CNS5 for it would be §12.14's defect, the same class as the `cancer.gov` citation being removed. StatPearls writes it in ROMAN, which walks into the corpus house rule WI-540 failed on both occurrences. **THE RENAME NEEDS A TWO-STEP ATTRIBUTION:** the paper retired "neurofibromatosis 2" (verified), but its abstract never contains the phrase "NF2-related schwannomatosis" — that is GeneReviews *reporting* Plotkin. **AND VERIFYING PAID TWICE, AS IT DID ON WI-540:** GeneReviews yielded a sentence the research pass never surfaced — *"Individuals younger than age 30 years with a symptomatic unilateral vestibular schwannoma are at high risk… while individuals older than age 30 years… are at very low risk of developing NF2"* — which scopes the inherited-condition material by **sidedness AND age** in one sourced sentence, protecting the one-sided majority while giving the genuinely at-risk young reader something real. **DRAFTED, AND THE FIRST COLLISION RUN WAS BAD IN EXACTLY THE WAY I HAD WRITTEN DOWN AND THEN DONE ANYWAY: 5 files, 66 shared shingles, 55 of them with `/tumors/craniopharyngioma`, which ships with NO ALLOWLIST.** I took the neighbour's SENTENCES, not just its stance — hazard #1 on my own pre-drafting list. In one case I copied craniopharyngioma's own DODGE: it invented "for the day a page is not enough and you need a person" specifically to escape the standard support-door collision, and I reused the invention. **THE FIX WAS ONE PASS BECAUSE THE REPORT WAS UNCAPPED AND THE INVENTORY WAS COMPLETE FIRST** — 66 shingles collapsing into 13 distinct passages, each rewritten as a whole sentence from scratch rather than edited in half. **Result: 66 → 0, and none of the 15 replacements created a NEW collision**, which was the specific risk (two of WI-540's fixes made things worse by rewording inside a saturated formula). **AND THE LOCATION PASS CAUGHT ME FIRST.** My initial `grep -F` reported ZERO for eight fragments that were certainly present — the Roman-numeral sentence alone produced 17 shingles — because the page is hard-wrapped at ~80 columns and line-based search cannot see a phrase spanning a break. That is standing trap #1, and the same mechanism that made WI-540 delete a correctly-sourced sentence. Re-run on FLATTENED text with a **positive control built from the file itself** (a phrase proved findable flattened and unfindable line-based), it showed what the first pass hid: **"from Roman numerals to ordinary ones" appears TWICE**, in the grade section and the report section. Fixing only the one I remembered would have brought the other back next pass — WI-540's eighth reference defect, reproduced. **VERIFIED NOW: collisions 0 uncapped with no allowlist, ContentCheck 273/0 at grade 5.6, the Roman-numeral house rule satisfied in both sentences with a canary proving the check can fire, 17 headings in §12.3 order, exactly the four ruled-in directives, outlook fence closed, and 8 paragraphs rewrapped with "text identical ignoring whitespace" asserted mechanically rather than eyeballed.** **THE SUITE WENT RED 4/2,214, AND THE MOST EXPENSIVE FINDING IS ABOUT MY TOOLING, NOT THE PAGE: THERE ARE TWO DIFFERENT RESTATEMENT GUARDS IN THIS CORPUS AND `all-collisions.py` MODELS ONLY ONE.** It reported 0 collisions; the suite then failed on `/treatments/watch-and-wait` and `/tumors/meningioma`. **Both reports were correct.** The shared `AssertDoesNotRestateTheCorpus` drops WHOLE markdown links, drops the ask-list and "where to go next" sections, and strips digits. The **page-local `Shingles(text, n)`** copied into many page test files drops **headings only** — `Regex.Replace(text, @"\]\([^)]*\)", "] ")` strips the link TARGET and **keeps the link TEXT**, and `[a-z0-9']+` keeps digits. So link text and the whole ask-list participate, and both failures came from exactly there: `watching a tumor step by step sets out` (a door's link text) and `what would make you change the plan what` (an ask-list question). **A clean report from one tool says nothing about the other.** New probe written and validated in both directions (`wi541/page-local-collisions.py`; control on brain-metastases: 10 files, 173 shingles). **IT FOUND MORE THAN THE SUITE: 4 files, 11 shingles** — two invisible to the suite because craniopharyngioma and pituitary-tumor use the SHARED guard. One was **self-inflicted**: my earlier reword to escape craniopharyngioma moved the collision onto pituitary-tumor. **AND PRE-CHECKING BEFORE WRITING PAID IMMEDIATELY** — my first replacement for the watch-and-wait door was pre-checked and STILL collided, because `[Watching a tumor, step by step]` is a corpus-standard door and almost any short continuation forms a shared window. Rewording inside a saturated construction moves the problem; **changing the LINK TEXT removes it**, the same move that cleared the craniotomy door. **The other two failures were straightforward and both instructive:** one British form (`organisations`) in the body, caught by a corpus-wide gate that reads BODY ONLY, so the front matter's verbatim British quotes are correctly safe; and the SRS glossary-reachability list, which failed because this page legitimately became a FIFTH page keeping that entry reachable (term in prose, link hung on other words — the shape that test rewards). **That failure is the mechanism working**, and the page is now registered there with the reason recorded. **VERIFIED: both probes 0 collisions, 0 body lines over 80, British form fixed.** **BOTH GATES WENT GREEN (ContentCheck 273/0 at grade 5.6; suite 2,214/2,214, all four failures cleared including the SRS registration), THEN THE TEST FILE WAS WRITTEN — ~35 tests across `AcousticNeuromaPageContentTests` and `AcousticNeuromaPageRenderTests`.** Two departures are deliberate: the one rule's ROUTE is pinned in BOTH directions (it must reach primary care / urgent care / ENT and must NOT grow a 911 route, because over-escalation here is as much a defect as under-stating), and the file carries **TWO restatement guards** — the shared one with no allowlist, plus a page-local `LinkTextShingles` variant that keeps link text and the ask-list, which is the guard my tooling was blind to and which turned two shipped pages red. **RENDERED READ 1 FOUND THREE DEFECTS THAT NO MECHANICAL GATE CAN SEE, because all of them only exist once the page is COMPOSED AND RENDERED.** (1) **The glossary tooltip echoes the page's own clause**: rendered, it reads "Doctors call this stereotactic radiosurgery *[tooltip: …There is no cutting, despite the name.]* … and despite the name nothing is cut." WI-539 spent a review round removing that exact echo and WI-540's front matter records the ruling — I reproduced it anyway, because the source file shows only my half and the tooltip lives in another file. (2) **The page contradicts the block composed above it**: the refusal paragraph says "What is NOT on this page is a list of warning signs for a large growth", while `[MECHANISM]`, composed ninety lines earlier, has already given a raised-pressure pattern AND an action. The refusal is true of what this page ADDS and false of what the reader has just read — §12.10's mis-scoped-block shape from the opposite direction, a block whose PRESENCE falsifies a sentence written as though it were absent. (3) **The composed source list leaks NCI four times and Cancer Research UK twice** via the shared blocks, one under a visibly British title; §12.1 still holds but craniopharyngioma records that leak explicitly and this page does not. **SUITE WITH THE NEW TESTS: 2,247 TOTAL, 2,244 PASSED, 3 FAILED — AND ALL THREE WERE DEFECTS IN MY OWN TESTS, NOT IN THE PAGE.** No shipped page went red, so both restatement guards and the British-form gate are satisfied; 33 of 36 new assertions held on first write. **Two of the three are defect shapes THIS FILE DOCUMENTS IN ITS OWN COMMENTS, which is the useful part.** (1) `keeps your hearing` was banned as a flat phrase and fired on the page's own honest sentence — *"it is not a promise that waiting keeps your hearing"*. That is §12.9's recorded WI-513 defect, **a ban list that is not negation-aware forbids the passage it exists to protect**, reproduced against the very clause the section carries. Rewritten to assert the property at SENTENCE level — the promise may appear only where the same sentence denies it — with canaries in both directions. (2) `call 911` did not match the escalation block because the block reads `Call **911**`: the bold markers sit INSIDE the phrase, so a flattened-only match cannot see it. §12.8's WI-526 defect, **a bolded word defeating a phrase guard structurally** — and this file's own `Plain` helper exists to prevent exactly that, three lines above the line that walked into it. (3) The fraction branch caught *"about half of the people who have it inherited it from a parent"* — GeneReviews' verified 50/50 split, carried in words. That branch exists to stop OUTCOME and FREQUENCY shares; an inheritance split is a different claim class, and carrying only "it is inherited" is itself a defect because it hides that a de novo case is equally likely. Exempted by its own sentence rather than by loosening the branch, so the branch keeps its teeth, with canaries proving it still catches what it is for. **AND THE THREE RENDERED-READ DEFECTS ARE FIXED:** the page no longer repeats the tooltip's "despite the name" gloss; the refusal paragraph now names what the composed block above it already covers, instead of claiming this page carries no warning signs while `[MECHANISM]` had just given some; and the front matter records the composed-source leak (NCI ×4, CRUK ×2) that only the rendered page reveals. **FULLY GREEN: ContentCheck 273/0 at grade 5.6, and the suite 2,247/2,247 unfiltered — all three test defects repaired, all 36 new tests passing, no shipped page red.** Both collision probes re-run after fix 2's new prose and both returned **0/0**, so the pre-check was confirmed by measurement rather than trusted. That closes draft → ContentCheck → full unfiltered suite. **The `/review` brief is written BEFORE the review rather than after it** (`wi541/review-brief-1.md`), because §12.8 records that a reviewer told what was DECLINED audits the declines, while one who is not told re-raises them or assumes they were accepted. It names the nine deliberate declines (no 911 route, no brainstem checklist, hemorrhage off the page, no figures of any kind, no time figure for the delay clause, CNS5 not cited for the grade, watching and focused radiation routed not owned, no new glossary entries) and — more usefully — **the five places I think this page is weakest**, including two I would rather have challenged than assumed: the already-diagnosed reader, whose sudden hearing change has NO source behind it and is handled by routing; and the outlook gate, which deliberately does NOT use the word "median" because for this tumor the published numbers are about hearing rather than survival. **RENDERED READ 2 IS DONE END TO END, AND IT FOUND ONE MORE DEFECT NO GATE COULD SEE: "mobile phones" — BRITISH REGISTER — THREE LINES BELOW THE COMPOSED BLOCK'S "Cell phones."** Two registers for one object on a US page. **Nothing caught it because `CuratedPage.BritishForms` lists SPELLINGS and this is an IDIOM**, the same class as WI-539's "tablets" and "optician" — and acoustic neuroma is *the* tumor in the mobile-phone literature, so this page will drift back toward it. Fixed, and "mobile phone" added to the page-local ban with an assertion that the page agrees with the block above it. **Read 2 also confirmed the three earlier fixes hold in the composed page**: the refusal now names what `[MECHANISM]` already covers instead of contradicting it, the verbatim tooltip echo is gone, and the NF2 passage scopes by sidedness and age with no inversion. **TWO THINGS ARE DELIBERATELY NOT BEING FIXED BY ME, AND ARE FLAGGED FOR `/review` INSTEAD.** (1) The scoping note tells the reader that two brain-map entries apply — cerebellum and brainstem — but the brainstem entry reads "facial weakness" while this page says facial weakness is UNUSUAL here. Defensible (the map describes a tumor IN the brainstem; this one presses on it, and only when large) but it is the same family as the defect read 1 caught, so the reviewer should rule rather than have me quietly reword. (2) A SEMANTIC tooltip overlap is being ACCEPTED on WI-540's precedent — the verbatim echo was removed, and what remains is the overlap between a term and its definition, "which is what a glossary is for". **Both are written into `wi541/review-brief-1.md` rather than left for the reviewer to rediscover.** **GREEN AFTER THE IDIOM FIX: ContentCheck 273/0 at grade 5.6, suite 2,247/2,247, both collision probes 0/0.** That closes draft → ContentCheck → full unfiltered suite → TWO rendered reads, which between them found FOUR defects — all of them invisible to every mechanical gate because all four only exist once the page is COMPOSED and RENDERED: a glossary tooltip echoing the page's own clause, a sentence falsified by a block composed above it, a composed source list leaking barred sources unrecorded, and a British idiom sitting three lines below the block's US spelling of the same object. **`/review` IS RUNNING**, given the brief rather than the page alone, and warned about the two traps that have cost this project real defects — that a line-based grep cannot match a phrase spanning an 80-column wrap and will silently report zero, and that my own claim-to-source map must be checked against the prose rather than believed. It is asked to rule on the two questions I deliberately parked rather than decided, and to hunt a fourth defective assertion of the family that produced three (a ban list that was not negation-aware, a phrase guard defeated by bold markers, a figures ban catching a legitimate qualitative claim). **`/review` ROUND 1 RETURNED TWO BLOCKERS, four should-fix and five nits — and the page moved in FOURTEEN places, which is the argument for having left `mutations.py` unwritten.** **BLOCKER 1: THE HEADLINE CLAIM WAS UNSOURCED AND THE PAGE REFUTED IT THREE TIMES.** The bold first-screen line and the description both said "what usually shapes the decision is your hearing, not the size on the scan" — supported by NONE of the thirteen sources, while EANO bands treatment by size and the page itself says "Size matters", "the usual choice when a growth is large" and "what your team watches for is size and change". §12.13's two-claims-that-cannot-both-be-true, in §12.12's over-reassuring direction, on the first screen — **and my own render test had PINNED the unsourced form**. **BLOCKER 2 IS THE FOURTH DEFECT OF THE COMPOSED-PAGE FAMILY, AND I HAD MISSED IT TWICE.** The scoping note said "several of the pressure effects listed there are not how this behaves" — vague and unbounded — while the refusal ~110 rendered lines later said the general list "applies to you as much as to anyone". Both cannot be operative, and the vague discount landed on exactly the WRONG paragraphs: what a LARGE growth here really does produce is raised pressure and blocked fluid flow (a cerebellopontine-angle mass obstructing the fourth ventricle), while the ones that genuinely do not fit are seizures and swelling in the surrounding brain. A reader with morning headaches and vomiting had been handed a reason to file them under "not how this behaves" — which made the refusal unsafe AS COMPOSED rather than unsafe in principle. The note is now subtractive and specific, and names what applies only once a growth is large. **THE REVIEWER RULED AGAINST ME ON ONE PARKED QUESTION AND WITH ME ON TWO:** the brainstem map entry does not survive as written (my defence was correct medicine and invisible to the reader), the semantic tooltip overlap is accepted on WI-540's precedent, and the outlook gate PASSES §12.5 because its median guidance is conditional ("when explaining median") and there are no survival medians here to explain. **IT ALSO REFUTED ONE OF ITS OWN SUSPICIONS** — that my page-local shingle mirror added a non-stopword floor the real guards lack — by checking three other test files and finding the floor identical. **AND I CAUGHT THE REVIEWER BEING WRONG ONCE:** it predicted my figures-test exemption regex would still match after the inheritance fix. It would not have, because the corrected sentence says "who have that condition" rather than "who have it" — unfixed, the figures ban would have fired on the very sentence the exemption exists for. **MY OWN EDIT SCRIPT THEN HIT WI-526**: an anchor spanning `again**, usually` could never match because the bold markers sit INSIDE the token, and because the script wrote only after all anchors succeeded, it aborted having written NOTHING while the test file had already moved ahead of the page. Restructured to validate every anchor before applying any, so one bad anchor reports the whole inventory instead of hiding what follows it. **ALL FOURTEEN REVIEW FIXES VERIFIED GREEN: ContentCheck 273/0 at grade 5.5 — IMPROVED from 5.6, because hedging the unsourced efficacy claims and removing the tooltip duplication simplified the prose — suite 2,247/2,247, and BOTH collision probes 0/0 over 3,045 and 3,228 shingles.** The five test edits all pass: the rewritten scoping-note assertions, the ambulance window widened to the end of the refusal, the exemption regex corrected for the new denominator, the dead block-subtraction loop deleted, and the render assertion retargeted off the unsourced headline. Expect-strings re-verified after the reflow with the wrapped-phrase canary still firing, and the new `{#symptoms}` anchor confirmed to agree with its in-page link while the heading list still parses to 17. **`wi541/review-brief-2.md` is written**, marking round 1's twelve settled items so they are audited rather than re-litigated, declaring the TWO places I varied the suggested remedy (dropping the recurrence frequency word entirely rather than hedging it; rewording the SRS aim further than asked, to put distance between prose and popover), and recording the one incidental claim the reviewer got wrong about my own test. **THE RENDERED READ OF THE CORRECTED PAGE FOUND NOTHING NEW, AND BOTH BLOCKERS ARE CONFIRMED FIXED AS COMPOSED — which is the only place either defect ever existed.** Blocker 2's rewrite was checked line by line against the block itself: the note subtracts exactly what the block asserts and this growth does not do ("it does not set off seizures" against "It can set off seizures"; "does not make the brain around it swell" against "It can make the brain around it swell"), and PRESERVES the two that matter — raised pressure and blocked fluid flow — naming them as arriving only once a growth is large. **And the apparent tension with the refusal paragraph resolves correctly**: the refusal's "general list above" is the ESCALATION block's when-to-call lists, a different block in a different section from the one the note scopes, so the two are consistent rather than merely reworded. All twelve other fixes verified as composed: the SRS prose no longer duplicates the popover's session count, the two efficacy claims are hedged, the comparative and the recurrence frequency word are gone, "or a headset" is gone, the cell-phone line now matches the block's register three lines above it, and the corrected inheritance denominator no longer contradicts itself. **`/review` ROUND 2 IS RUNNING**, given review-brief-2.md so it audits round 1's twelve settled items rather than re-deriving them, told which two remedies I varied, and asked for one thing above all: **a fifth defect of the composed-page family**, since every one of this item's five worst defects existed only once the blocks were composed and nothing in the toolchain can see that class. **`/review` ROUND 2 RETURNED NO BLOCKERS — THE SHIP SIGNAL** — plus 5 should-fix and 6 nits, all being applied before the harness. **IT FOUND THE FIFTH COMPOSED-PAGE DEFECT I ASKED FOR, AND IT IS ROUND 1'S BLOCKER IN MINIATURE.** My rewritten scoping note says flatly "it does not set off seizures" — and then endorses the pressure paragraph, which LISTS seizures among its five signs, while the ambulance tier below files "A first ever seizure" unconditionally. A reader could strike one item off the pressure pattern, or — in the dangerous direction — read the denial as a reason a first seizure is not urgent. **It survived fourteen edits because NOTHING PINS that the denial leaves the ambulance tier intact**, which is the test this fix must add. **THREE MORE ARE §12.14 SOURCING DEFECTS — the claim not supported in the FORM it is made:** "Hearing loss is the most common of the three" is a RANKING resting on sources that only give LISTS (the same class this page's own front matter refuses for the grade); "the high notes go first" and "a change in taste" have no support in the map at all, and are true in the literature, which is exactly why they slipped through. **AND BOTH REMEDIES I VARIED LOST SOMETHING**, which is the argument for declaring variations rather than quietly making them: dropping "usually" made the left-behind fragment read as THE cause rather than A cause, and my reworded SRS aim narrows what the owning page states wider ("stopping growth, shrinking, or easing a symptom"), creating one claim at two strengths across a door I route through. **The reviewer also ran my correction to it and confirmed the exemption regex now matches, refuting its own round-1 claim.** **ROUND 2'S FINDINGS ARE APPLIED — 5 should-fix and 5 nits, with TWO NITS DECLINED AND THE REASON RECORDED.** The declines share one shape: the reviewer called two positive assertions redundant with nearby bans, but the unlinked-term check passes if the term disappears from the page entirely, and the `escalated` ban passes if the caregiver summary names no route at all — **each guards a failure its partner cannot see**. **THE EMPHASIS TRAP WAS FIXED AS A CLASS, NOT AN INSTANCE.** Markers sitting INSIDE a token (`and **trouble with balance**.`, `grow again**, from a piece`) defeated literal anchors and aborted the edit run a second time; the matcher now permits `[*_]*` between every character alongside its existing whitespace tolerance. That trap had already cost this item a test defect, a corpus-search false negative and two aborted runs. **AND A STRAGGLER PROVED THE VERIFY-OVER-THE-WHOLE-PAGE RULE AGAIN:** after replacing "straight away" inside the safety rule, a check over the entire flattened body reported the idiom STILL PRESENT — a second occurrence in the short version that no edit had touched. Both it and "come round" are now cleared and pinned page-locally; "being sick" stays for the `/pm` sweep, being in twelve other files. **THEN I BROKE THE PAGE MYSELF AND THE GATE CAUGHT IT.** A one-line tidy-up in the edit script, `re.sub(r'[ \t]{2,}', ' ', t)`, collapsed **every indentation level in the file** — `  - url:` → ` - url:`, all 304 front-matter comments, and four markdown list continuations — so the front matter stopped being valid YAML and the page stopped loading. **ContentCheck failed on the next run and the chained script refused to start the suite**, which is exactly why the cheap gate runs first. `git checkout` was NOT available as a remedy, because the file is uncommitted and reverting would have discarded the entire item, so the repair was surgical and by RULE: YAML's own grammar says what each structural indent must be. **The damage-measurement tool also produced a false positive about itself** — it flagged the page title as wrongly indented because it matched `title:` as a source-entry title; trusting that report would have indented the page title into the sources list. Repair verified: 0 structural errors, 13/13/13 sources, YAML parses, **ContentCheck 273/0 at grade 5.6**. **NEXT: the full suite (never ran this cycle) plus both probes are running; then a rendered read of the changed regions, and `mutations.py` → dryrun → harness on LF AND CRLF → handproof → §12.8 + backlog + close-out → privacy scan → commit → PR into develop → release PR into main → deploy smoke.** **ITEM COMPLETE, NOT YET SHIPPED.** ContentCheck 273/0 at grade 5.6; suite 2,249/2,249 (+2 for the new emphasis guards). **133 break mutations, every one red on LF AND CRLF, first run** (266 ok, 0 survivors, 0 ambiguous anchors, 0 no-ops) across the page, blocks/escalation.md, blocks/mechanism.md and treatments/watch-and-wait.md. **8 render guards proved by hand**, all red, including the two that exist only at the rendered layer (an included block that stops composing, an excluded block composed back in). Two rendered reads clean. Privacy scan 0 findings. **A LATE DEFECT NO GATE COULD SEE:** a /review round-2 fix left a `**` unclosed, which renders as literal asterisks to the reader while ContentCheck, both restatement guards, every `Plain` helper and the render tests all strip emphasis before matching — 2,249 tests green over it. Same prose also carried a British spelling. Both rode in on a fix that HAD been pre-checked, for restatement collisions only: a pre-check answers the one question it implements and its silence is not general clearance. Now guarded page-locally and site-wide (`NoCuratedPageOrBlockLeavesAnEmphasisMarkerUnclosed`), made site-wide on evidence — swept offline over a superset with a planted positive control, zero elsewhere. Remaining: commit, PR into develop, release PR into main, deploy smoke. **LIVE AND SMOKE-VERIFIED:** PR #149 into `develop` (build-test pass 3m36s), release PR #150 into `main`, and the merge commit `373e437` went build-test success → **deploy success** first time. Smoke clean: hub **10,832 → 46,586 bytes**, controls matched first, all 8 positive fragments present, all 4 absences absent; every other route byte-identical except `/tumors` (16,996 → 17,155, the index card picking up the new description). |
| **WI-540 (done — history)** | **Shipped 2026-09-18.** **WI-540 (Craniopharyngioma, deepened) — STARTED 2026-09-18**, branch `feature/wi-540-craniopharyngioma` (from develop at `3df3dd5`). WI-539 is SHIPPED AND LIVE and is not reopened. Deepening a 43-line stub to §12.3's seventeen sections; **§12.3 and §12.9 govern**. Baseline before this item: suite **2,184 / 2,184**. Tools copied to `.claude/work_files/wi540/` — **no `allowlist.txt` exists there, so "no allowlist" is a property of the directory** rather than something to remember (WI-539's ruling, and its reason: the restatement guard is BIDIRECTIONAL, so borrowing `/tumors/pituitary-tumor`'s prose turns THAT page red and an allowlist here cannot fix it). `dryrun.py`/`break-tests.py` retargeted to this item and asserted; `handproof.py` deliberately REFUSES to run until its cases are rewritten, because its WI-539 cases would have mutated the shipped pituitary page. **SCOUTING DONE BEFORE DRAFTING, AND FIVE FINDINGS CHANGE THE SHAPE FROM WI-539'S.** (1) **THERE IS NO SOURCE PACK AND NO CORPUS FOOTHOLD.** `craniopharyngioma` appears NOWHERE under `Content/` except `taxonomy.yml` (a bare label, no `also:` aliases) and its own stub — no sibling links to it, no block names it, `/treatments/watch-and-wait` does NOT list it (it lists pituitary tumors), so **watching is not routed here the way it was for WI-539** and nothing may be assumed from that page. (2) **A FILE NAMED FOR THIS TUMOR IS THE WRONG CHAPTER.** `.claude/work_files/wi510-sources/statpearls-cranio.txt` is the **CRANIOTOMY** StatPearls chapter, not craniopharyngioma — it mentions this tumor twice, only as a target of surgical approaches. §12.8's "check the document, not the filename" arriving before a single word was drafted. The pack is entirely this item's work. (3) **BOTH OF WI-539'S BLOCK EXCLUSIONS MAY FLIP, AND MUST BE DECIDED BY OPENING THE BLOCKS.** `[MECHANISM]` was excluded there partly because a pituitary tumor "does not block the fluid pathways" — but a craniopharyngioma sits at the third ventricle and hydrocephalus is plausibly its story, which would make the block's blocked-fluid paragraph TRUE here; and `[CROSSWALK]` was excluded because the naming came from the WHO **endocrine** volume, while CNS5 **does** classify this tumor (adamantinomatous and papillary as separate types). Neither may be inherited from WI-539. (4) **THE STUB CITES THE SAME BARRED SOURCE** — `cancer.gov/types/brain`, NCI, which §12.1 says is a defect when cited for a tumor name; the third stub in a row to carry it. (5) **THE STEROIDS COUPLING MAY NEED NO EDIT AT ALL.** WI-539's round-2 B2 rescoped that page's conditional onto the MECHANISM ("If your body has stopped making its own steroid… when the pituitary is not making enough on its own"), which already reaches a craniopharyngioma reader with hypopituitarism — so the default is to ROUTE, not to add a third strength. Any adrenal wording must read at ONE strength across this page, `/tumors/pituitary-tumor` and `/treatments/steroids`, and that page's crisis triad already carries 911 + "get to a hospital". **SOURCE PACK BUILT (42 files) AND ITS INTEGRITY IS THE ITEM'S GOVERNING CONSTRAINT.** The fetch layer refused whole-page dumps on copyright grounds, so every `.txt` is a **model-produced topic-targeted EXTRACT, not a saved page** — and `NOTES.md` tags its quotes to those extracts, so script-checking the notes against the `.txt` is **CIRCULAR**: the same extractor wrote both. That is §12.8's DOSSIER footing, not a source. **Standing rule recorded in `wi540/source-verification.md`: no sentence reaches the page on the strength of a `.txt` alone.** **SIX SOURCES VERIFIED AGAINST THE LIVE PUBLICATIONS SO FAR, AND THE PATTERN IS REASSURING IN SUBSTANCE AND NOT IN PRECISION: no fabrication in any check — every quote was really there — but the extractor TRUNCATES QUALIFICATIONS, three confirmed** (CNS5 dropped the opening "In past editions,"; MedlinePlus dropped "(intravenous)/(intramuscular)"; MedlinePlus stopped at "it can be life threatening" where the source continues "**from very low blood pressure and heart problems**"). That last is §12.13's exact defect — quote to the end of the qualification, not the end of the convenient sentence. **One 404 was MINE, not the pack's: I verified a St. Jude page at a guessed URL instead of the one the pack recorded.** **THE CENTRAL SAFETY CLAIM IS DECIDED FROM SOURCES AND IT IS THE INVERSE OF WI-539's.** CP apoplexy IS named for THIS tumor — PMC8963871 defines it and puts 9.73% on it, so nothing is borrowed from a pituitary-adenoma source. **But it is verified "a very rare syndrome" (intro AND conclusion), rests on one retrospective series of 185 with 18 events plus case reports, carries NO patient-facing action guidance at all, and BOTH general reference chapters are verified SILENT on it** — StatPearls does not discuss it and Endotext does not mention hemorrhage anywhere. What the sources actually converge on is **acute obstructive HYDROCEPHALUS / raised pressure** (StatPearls "External ventricular drainage may be needed in cases presenting acutely"; Endotext 20–38%; St. Jude in plain words; two ED case reports) **plus ADRENAL CRISIS** from ACTH deficiency. **So the tier is raised pressure + adrenal crisis, with apoplexy named, kept rare, and NOT the headline. Inheriting WI-539's tier would have mis-ranked this tumor's emergencies.** **AND BOTH OF WI-539's BLOCK EXCLUSIONS FLIP, decided by OPENING the blocks: `[MECHANISM]` is IN** — its blocked-fluid-and-shunt paragraph is this tumor's story, its raised-pressure paragraph is near-verbatim this tumor's presentation, seizures are ~10% so not false, the block DOES carry a scoping clause WI-539's front matter under-reported ("Which of them apply to you depends on what your tumor is and where it sits"), and the decisive precedent is `/tumors/meningioma`, which is ALSO extra-axial and includes it; 13 includers today and **no test pins the set**, so joining breaks nothing. **`[CROSSWALK]` stays OUT** but for a different reason than WI-539's — only its Roman→Arabic third applies, since CTNNB1/BRAF are diagnostic genes NOT in this tumor's name and there is no NOS/NEC — so the page writes its own slice. **`[CAUSES]` needs NO scoping bridge** (WI-539 did): US patient sources call this a brain tumor outright, so the block's framing fits and copying that bridge would solve a problem this page does not have. **`[TUMOR-BOARD]` OUT** (the block is a MEETING; the sources describe ongoing multidisciplinary CARE — a different claim). **`[SPINAL-CORD]`/`[POSTERIOR-FOSSA-SYNDROME]` OUT**, wrong region, and since only those two pin includers exactly and neither names this page, **no registration is owed.** Other rulings: **audience is BIMODAL** (5–14 and 50–74) so the child reader is ROUTED to WI-538's parent hub rather than restated; **watching is NOT routed** (`/treatments/watch-and-wait` does not name this tumor — WI-539's ruling does not transfer); **`/treatments/radiation-therapy` already owns the pituitary-hormone late-effect material**, so route; **the steroids coupling likely needs NO edit** because that page's conditional is already mechanism-scoped; **the recurrence conflict is resolved to the LONG horizon** (Boston Children's "up to two years" contradicts its own indefinite follow-up schedule; Endotext's "as late as 30 years" is verified) and the two-year window must not reach the page; **no glossary entries** — `transsphenoidal` now crosses the second-use threshold but is DEFERRED to WI-553, because an entry fires a tooltip on the shipped pituitary hub that glosses it inline and would manufacture the echo defect WI-539 removed. **Shingle replica VALIDATED against a known-clean run before use** (it reproduces the shipped pituitary result exactly: 2,571 shingles / 0 collisions), which is the check WI-539's replica failed first time; the **current stub already collides** with `/tumors/acoustic-neuroma` on "it is not cancer and it grows slowly". **The outlook gate is the mapped landmine: ELEVEN hubs open with the same formula**, which is where WI-539's hidden fifth collision lived, so this page's gate is written from scratch. Full rulings with evidence in `wi540/plan.md`. **PAGE DRAFTED (43-line stub to a full §12.3 hub) AND THE STRUCTURE VERIFIED INDEPENDENTLY: 17 headings in §12.3's exact order, 2 subsections, block directives exactly MECHANISM/ESCALATION/CAUSES/CAREGIVER, outlook fence opened and closed. ContentCheck 273/0, reading grade 5.6 against the hard 6.0.** **THE RESTATEMENT FIGHT WAS THE ITEM'S REAL COST, AND IT IS NOW GREEN: suite 2,184/2,184 with ZERO collisions corpus-wide and NO allowlist.** The first draft turned **FOUR SHIPPED PAGES RED** — `/tumors/pituitary-tumor`, `/tumors/pediatric-brain-tumor`, `/treatments/targeted-therapy`, `/treatments/steroids` — none of them this page's own test, because this page had none yet. The brief's warning was exactly right and I walked into it anyway: I took the neighbouring hub's SENTENCES, not just its stance (`Not cancer and not a problem are two different sentences`, `The Endocrine Society calls that an adrenal crisis…`, `Your team knows the rules where you live; a website does not`, and more, near-verbatim). **IT TOOK SIX PASSES, AND THE REASON IS THE CAP: both the suite guard and the offline replica `.Take(4)` PER FILE, and they order their sets differently, so each pass fixed everything its report listed and the next pass surfaced a fresh four on the SAME file. Roughly twenty distinct shared runs against the pituitary hub alone, revealed four at a time.** Fixed by writing `wi540/all-collisions.py`, which reuses `shingles.py`'s own functions (so it cannot drift) and prints the intersection **UNCAPPED** — one run gives the whole inventory. **Its zero was not believed until the same tool was run against `/treatments/steroids`, which shares text deliberately, and made to report 2 files / 35 shingles; exit codes checked unmasked, 0 when clean and 1 when found.** Same discipline as the shingle replica, which reproduced the shipped pituitary result exactly (2,571 / 0) before being trusted. **NINE INSTANCES OF ONE ERROR, AFTER I HAD WRITTEN THE RULE DOWN: every collision I "fixed" by editing one half of a borrowed sentence and leaving the half the checker had matched.** The eighth was sharper still — `very thirsty and passing a lot of water` came back a third time because I fixed the two occurrences I could REMEMBER and never grepped my own page for the rest. The remedy that worked was to stop editing inside sentences and rewrite them whole. **TWO OF MY FIXES CREATED WORSE COLLISIONS THAN THEY REMOVED** (the "ask for your own timetable, in writing" formula is shared across THREE pages), which is what forced the technique that ended the loop: **pre-check every candidate replacement against the flattened corpus BEFORE writing it**, excluding this page, with BOTH controls. It caught `periods that become irregular or stop` before a word was written. **THAT PROBE WAS ITSELF BROKEN FIRST TIME AND ITS OWN OUTPUT PROVED IT:** `tr '\n' ' '` leaves the `\r` on a `core.autocrlf=true` checkout, so four phrases I had just read on screen came back ABSENT — trap #1 landing on the check written to avoid trap #1. It had a negative control and no positive one, so it proved it could say *no* and nothing about whether it could say *yes*. **A HOUSE RULE THIS PAGE HAD NEVER MET, ENFORCED FROM ANOTHER PAGE'S TEST FILE:** `PathologyReportPageContentTests.NoCuratedPageWritesAGradeInRomanNumeralsExceptToShowTheOldStyle` allows a Roman grade only where the SAME SENTENCE carries a marker; both of this page's put the marker in the neighbouring sentence. The irony is the lesson — the crosswalk slice exists to explain the Roman-to-Arabic change and the Roman-numeral guard rejected it, which is WI-513's finding that a ban must be negation-aware or it forbids the crosswalk it protects. **`CraniopharyngiomaPageTests.cs` WRITTEN — 30 tests (24 content, 6 render), ALL GREEN**, pinning: both tiers sign by sign, with the fluid tier's entry asserted as a **DISJUNCTION** (narrowing "arrive quickly OR together" to AND would exclude the commonest presentation and no per-sign check could see it) and the adrenal route asserted **POSITIONALLY** outside the injection clause; the apoplexy ruling as **named, rare, and having NO tier or route of its own**, because the paper that establishes it gives no patient action at all; four blocks in and four out as an **exact closed set**, reading the siblings rather than asserting beliefs about them; the Roman-numeral rule page-locally with a positive count beside it; hypothalamic weight gain as mechanism not willpower with its anti-hype half; and watching deliberately NOT routed. **TWO OF MY OWN GUARDS WERE WRONG AND RUNNING THEM IS WHAT FOUND IT — 28/30 FIRST TIME.** (1) The watch-and-wait absence was asserted against the RAW file, and this page's front matter NAMES that URL while explaining the ruling against it: **a ruling has to name what it rules against, and a raw-file guard cannot tell a rationale from a link.** Earlier instances of this trap were fixable by rewording the note; this one was not, so the fix is the rule — a guard asserting what a page does NOT offer must read the READER'S text. (2) My outlook ban on `most people` fired on "Most people are given this answer as a number", which is about how an answer is delivered, not a prognosis; the ban is right for an outlook gate, so the page was reworded instead. Both failed in the SAFE direction. **BOTH RENDERED READS ARE DONE, AND THE RULE BEING *TWO* READS IS WHY THE PAGE IS SAFE: read 1 found five defects, read 2 found seven more, and re-reading those edits IN PLACE found two beyond that — fourteen in total.** `render-read.py` was retargeted from WI-539 and given an asserted PAGES table, because copied unedited it would have captured the pituitary page and reported success; the capture self-verifies (expect strings, a length floor, no unresolved directives, gate closed) and came back clean at 32k chars. **READ 1 (five).** The **glossary tooltip echoed the sentence it landed in** — the popover says "drugs made to target that change" and my next sentence said "medicines aimed at that change exist" (WI-535's and WI-539's defect, now mine). **The fix was decided by COUNTING, not instinct, and it is NOT suppression:** `!%BRAF%` exists only on `/tests/molecular-markers`, which DEFINES the markers itself, while `/treatments/targeted-therapy` lets the tooltip fire — so the corpus rule is suppress-only-if-you-define, and the page instead deleted ITS OWN restatement. One term, one behaviour, reached independently of WI-539's `extra-axial` ruling. Also: **"that word" lost its antecedent** when an earlier collision fix deleted the word *diabetes* it pointed at; **the composed source list leaks NCI FOUR times and Cancer Research UK TWICE** via the shared blocks, which a front-matter check cannot see and the front matter now records (§12.1 holds — none is a naming claim); my fluid tier and the block's shunt conditional read as near-duplicates; and the scoping note said the brain map was "at the end" when composed it is a section of its own. **READ 2 (seven), and it walked past none of what read 1 was too busy to hear.** (1) **THE OUTLOOK GATE OVER-PROMISED AND CONTRADICTED THIS PAGE** — "something a team can check and **change**", where the third item is hypothalamic damage and this page's own treatment section says results there have been "disappointing" with no drug proven; §12.12's reassuring direction, which is the dangerous one. (2) **"Neither of these is common" was UNSOURCED AND PROBABLY FALSE.** WI-539 could say it because apoplexy is verified rare; here the first tier is BLOCKED FLUID, which Endotext reports in a large minority and StatPearls puts headache/nausea/vomiting among the commonest presentations. **Deleted rather than softened**, and the test that sliced the tier on that sentence was moved in the SAME pass. (3) **The fluid tier re-listed the block's same-day signs** thirty lines later, slightly differently, without saying they were the SAME signs lifted — two copies to keep in step, which is WI-539's B2 shape; now it states the relationship while KEEPING the sign-by-sign enumeration, because a tier that loses a sign loses it silently. Plus a fourfold repetition of "carry rules of their own". **THEN TWO MORE, FROM RE-READING THE SIX EDITS IN PLACE RATHER THAN AT THEIR OWN ANCHORS:** deleting the frequency sentence **ORPHANED THE NEXT ONE** — "**Both** are worth explaining" had been leaning on the deleted "these" — which is a NEW variant of §12.8's rule (adding precision can strand a pronoun; **so can taking a sentence away**); and a "This growth… This growth" repetition. **THE DOMINANT PATTERN OF THIS ITEM, STATED PLAINLY: FIVE OF ITS DEFECTS WERE INTRODUCED BY FIXES** — three reference defects, one collision that was worse than the one it replaced, and one dead test anchor I created while the lessons file recording that rule was open. Every late edit is unreviewed prose. **VERIFIED GREEN AFTER THE READ-2 FIX SET: suite 2,214 / 2,214 UNFILTERED, ContentCheck 273/0, grade 5.6 against the hard 6.0, and 0 corpus collisions under the UNCAPPED check with NO allowlist — with the instrument re-proved able to say yes each time (0 for this page, 2 files / 35 shingles for a page that shares text deliberately).** Every replacement sentence this item wrote was pre-checked against the flattened corpus BEFORE being written, excluding this page, with both controls; that technique is what ended the six-pass collision loop and it caught one candidate before a word was committed. **`/review` ROUND 1: 1 BLOCKER, 5 SHOULD-FIX, 4 NITS — ALL ADDRESSED, AND THE BLOCKER IS THIS ITEM'S OWN THESIS LANDING ON ME.** The page said the scan and blood tests usually name this tumor and that "if tissue is examined at all, it comes out of the operation" — while two sections later saying an operation is almost always first and that one of its goals is **to find out exactly what the growth is**, on a page that also teaches a two-type split decided "under a microscope" and asks the reader "Which type is mine?". **Both cannot be true, and the false half was the NEIGHBOUR'S.** That sentence was one of the SIX COLLISION PASSES: I reworded `/tumors/pituitary-tumor`'s "a piece of tissue is not usually taken first" until the restatement guard went quiet and **carried its REASONING across intact**. It is true there (that hub routes to watch-and-wait; many of its readers are never operated on) and false here — where **this item's own RULING 7 says watching is NOT routed precisely because surgery is almost always first.** Three things make it the item's sharpest lesson: **the guard's SUCCESS is what concealed it** (rewording far enough to clear an eight-word-shingle check is the very operation that turns a visible copy into an invisible transplant); **no tool could see it** — ContentCheck reads level, the suite reads assertions, the shingler reads overlap, the rendered read shows composition, and **nothing compares a page's claims against each other**, so it survived six green runs and two end-to-end reads; and **I had already written down the fact that refuted it** before the sentence existed. Fixed with the INVERSE claim rather than a deletion, which also repairs the otherwise unanswerable "Which type is mine?": the scan points to it, and the diagnosis and type name are confirmed on tissue taken during the operation — supported by StatPearls, verified live, "Surgical intervention is indicated to confirm diagnosis". **Also fixed:** including `[MECHANISM]` had imported a WEAKER statement of this tumor's fastest emergency ("worth a phone call rather than a wait") ~90 lines ABOVE this page's right-away tier for the same signs — the cost of RULING 2, invisible to my scoping note, which warned only about the lobe map; fixed in the note rather than the block, whose blast radius is 13 hubs. **My 911 clause re-listed the block's ambulance triggers and DROPPED SIGHT** — on the tumor defined by threatening sight; sudden loss now included, with progressive change deliberately left at the lower strength. **Unsourced directional prognosis inside the gate** ("the numbers are kinder than the word tumor leads people to expect") removed, and the guard widened from a list of phrasings to the PROPERTY (any comparison characterising the distribution) with canaries — this page had been the only gated hub of thirteen to characterise the numbers before explaining them. **The excluded-content loop checked three of four excluded blocks** on the one test whose subject is the blocker-that-is-an-absence. **"nothing in common" was a false absolute** — thirst and passing water are exactly what the two conditions share, which is *why* the name was borrowed. **ONE REMEDY TAKEN DIFFERENTLY, AND RECORDED AS SUCH:** round 1 said "source or drop" the recurrence "usually happens in the first few years"; **I SOURCED it** — Endotext, verified live, "The mean interval for diagnosis of recurrence … ranges between 1 and 4.3 years". The finding was right and the defect was in the CLAIM-TO-SOURCE MAP (§12.13), not the claim: the front matter had recorded the 30-year sentence and not this one, so no reader could check it. **ROUND 1 AUDITED AND UPHELD EVERY DECLINE**: the emergency ranking (no source supports an ACTION for bleeding), no edit owed to `/treatments/steroids` after reading all three pages, the reciprocal pediatric door, the deferred glossary entries, and a figures sweep that found none. **Verified green after the fix set: suite 2,214 / 2,214, ContentCheck 273/0, grade 5.6, 0 uncapped collisions with an honest control.** **FOR `/pm`, TWO HAND-OFFS THE REVIEW ASKED BE CARRIED FORWARD RATHER THAN DONE HERE:** (1) a **reciprocal door from `/tumors/pediatric-brain-tumor` to this hub** — this tumor is bimodal and is ~6% of childhood brain tumors, that hub names the common childhood types and not this one, and nothing pins its link set; declined here only because it edits a shipped page late in an item that already turned four shipped pages red once, and reachability exists via the tumor index (asserted by a render test). (2) **"sight test" is British register** and is on this page AND the shipped `/tumors/pituitary-tumor`, so it is corpus-consistent rather than this item's defect — it belongs to the follow-up idiom sweep, not here. **`/review` ROUND 2 RETURNED NO BLOCKERS — THE SHIP SIGNAL** — plus 7 should-fix and 6 nits, all now applied or ruled on. **It verified the round-1 blocker fix holds**: true, sourced in the form the citation supports (StatPearls "Surgical intervention is indicated to confirm diagnosis"), consistent with the treatment section, the microscope sentence and the ask-list — and the neighbour's imported reasoning confirmed **GONE rather than relocated** (the pituitary sentence's conditional tail, "if there is one", is what made it true there; this page now asserts the opposite and is right to). **S1 WAS THE ONE THAT COULD HAVE COST A READER SOMETHING, AND NO GUARD COULD SEE IT.** The caregiver summary said "signs that arrive quickly; the other is illness while they are **taking** a replacement steroid" — **silently narrowing BOTH rules**: it kept only the first branch of the fluid tier's DISJUNCTION (dropping "or together", the exact axis my own test exists to protect) and re-gated the adrenal rule on replacement **eleven lines after the page deliberately widened it** to anyone whose gland has been harmed. Invisible because every tier guard reads the SYMPTOMS section and the caregiver guard read two phrases. **A caregiver summary is a RESTATEMENT of two safety rules, and nothing checked the summary against the rules** — §12.8's "when a sentence makes a claim about a list, open the list", one level up. Now widened and pinned with canaries. **S2:** the blocker fix landed in the section's LAST paragraph while the bold one-line answer a skimmer actually reads still said "a scan, blood tests, and a proper eye test" (§12.6: answer in the first sentence). **S3:** the BRAF paragraph said the drugs are "sometimes used before surgery", which this page's own account makes unreachable, since the type is not known until the operation — §12.13's "two claims that cannot both be true, three screens apart". Fixed with a conditional ("where the type is already known"), deliberately **NOT** by weakening the blocker fix. **S5:** an unsourced comparative ("come back more often than most people expect") survived OUTSIDE the gate, because the guard round 1 widened runs only on the gate's interior. **S6:** `Assert.Contains("/treatments/steroids", Page)` could not fail for its stated reason — the front matter names that URL — which is **the identical trap this same test file documents for `/treatments/watch-and-wait`, re-committed in the POSITIVE direction**. **S7:** the round-1 override clause was itself unpinned, and an unpinned remedy is one edit from gone; now guarded from both ends, including that the block still SAYS the line the override outranks. **TWO NITS DECLINED WITH REASONS** (the quickly/suddenly distinction rests on the verb but over-escalation is the safe direction; a one-phrasing ban whose property a positive assertion already holds). **AND THE EIGHTH REFERENCE DEFECT OF THIS ITEM WAS MY FIX FOR THE SEVENTH, IN THE SAME SENTENCE.** S4 found the override clause pointing at the brain map ("the general advice **there**", which contains no advice); my replacement opened "**It also** carries the advice…" — and "It" resolved to the brain map again. **Four of the eight were created by fixes.** The pattern is structural rather than careless: repairing a sentence means holding its MEANING in your head, and the meaning supplies the referent the reader does not have. The only counter that worked was mechanical — read the edited sentence together with the one before it, resolving every pronoun to the NEAREST noun rather than the intended one. It caught three of the four; no guard caught any, because nothing in the toolchain models reference. **VERIFIED GREEN WITH THE ROUND-2 FIX SET: suite 2,214 / 2,184 baseline + 30 new, ContentCheck 273/0, grade HELD at 5.6 against the hard 6.0 even after S1 lengthened the caregiver section, 0 uncapped collisions with NO allowlist and the instrument re-proved able to say yes (2 files / 35 shingles on a page that shares text deliberately).** **THE PAGE IS FINAL AND THE TOOLING IS BUILT.** The two rewraps my own edits introduced are done and RE-VERIFIED rather than assumed — a whitespace-only rewrap is not cosmetically safe on this page, because a literal test anchor that now crosses a new line break fails exactly like WI-535's line-wrapped anchor, and both rewrapped regions carry guards. Green after them: **suite 2,214 / 2,214, ContentCheck 273/0, grade HELD at 5.6.** **`mutations.py` IS WRITTEN — 154 MUTATIONS ACROSS SEVEN FILES, AND IT DRY-RAN CLEAN ON THE FIRST PASS: 0 ambiguous anchors, 0 no-ops, 0 duplicate names.** That is the payoff of having deliberately left it unwritten until `/review` returned no blockers: every anchor was harvested from the FINAL text, so none had gone stale. The seven targets are this page, both shared blocks it composes (escalation, mechanism), and the four sibling pages its guards READ rather than believe (steroids, watch-and-wait, pituitary-tumor, meningioma). **EVERY MUTATION NAMES A TEST IN THIS ITEM'S OWN FILE, AND THAT IS A DEPARTURE FROM WI-539**, which also mutated `/treatments/steroids` against THAT page's own test class. That proof is banked — it shipped, it was proved red, and the page has not changed since — so re-asserting it here would make this harness's verdict depend on a method name in a file this item never opened, and a harness that ERRORS on a stale name reports nothing at all, which is worse than reporting less. The sibling files are still mutated; just always against the guard here that reads them. **ONE DEFECT CAUGHT BY READING MY OWN TABLE BEFORE RUNNING IT:** a `blocks-unruled-fifth-added` entry that composed TUMOR-BOARD a second time, duplicating `blocks-tumor-board-restored` — two reds for one property, which overstates coverage in precisely the direction a break harness exists to measure honestly. Removed, with the reason recorded where the entry was. **`handproof.py` IS REWRITTEN and its refuse-to-run guard removed**, and two of its cases are now stronger than the ones they replace: WI-539's fragment guard was VACUOUS (that page carried no `#fragment` link, so it had to be broken by adding one), whereas this page deep-links two anchors and the break is a real one; and WI-539's glossary case was listed as "expected to SURVIVE, and that is the finding", whereas this item's test names the slug, so it is breakable — though not by deleting one of the two "BRAF" occurrences, since the other keeps the tooltip rendering and the guard would be RIGHT to pass. That is a weak MUTATION, not a weak guard (§12.8's WI-537 distinction), and the honest break is the renderer's own page-wide suppression marker. **THE §12.8 ENTRY IS WRITTEN AND VERIFIED IN PLACE** (`docs/content-pipeline.md` 4297, after WI-539's group and before §12.9, with §12.10 intact below it) — sixteen lessons, written from `wi540/lessons-draft.md` rather than from memory, which is why it carries the eight-reference-defect count and the seven half-rewrites rather than a tidied version of them. Done while the harness runs because `docs/` is the one tree neither the build nor the harness touches. **The rest is genuinely blocked on the harness, and one case is worth naming: `git diff` CANNOT be used to size the final change right now, because the harness is writing mutations to disk and the diff would show mutated prose as though it were mine.** The backlog tick is blocked for a different reason — WI-539's entry cites PROVEN counts ("111 break-mutations green on LF and CRLF", "8 render guards handproofed"), and no mutation or handproof number gets written here before a run returns it. **THE BREAK HARNESS PASSED CLEAN ON THE FIRST RUN: ALL 154 BREAKS FAIL CORRECTLY, ON LF AND ON CRLF** — no survivors, no ambiguous anchors, no no-ops, and the unmutated baseline green on both line endings. **AND THE VERDICT WAS READ RATHER THAN INFERRED, BECAUSE THE REPORTED EXIT CODE WAS THE GREP'S, NOT THE HARNESS'S** — this item's own §12.8 lesson, arriving one more time in the turn after it was written: the launch command ended in a `grep`, whose pattern matched `all 154 breaks fail correctly` exactly as readily as it would have matched a survivor list, so exit 0 carried no information at all. The python exit was captured on its own line and the log was opened. **That the harness went green first time is the payoff of the ordering the user fixed**: mutations were written only AFTER `/review` returned no blockers, so all 154 anchors were harvested from final text and not one had gone stale. **HANDPROOF, RENDER, PRIVACY SCAN AND BACKLOG ALL DONE — AND THE HANDPROOF EARNED ITS KEEP BY FINDING SOMETHING READING HAD NOT.** Run 1 came back **7 of 8**, with `ThePageIsReachableFromTheTumorIndex` staying GREEN after I deleted the page's entire description. That was a **weak MUTATION, not a weak guard** (§12.8's WI-537 distinction): `taxonomy.yml` holds only a slug and a label, there is no description in it, and the two assertions read the link and the label. **The mutation was wrong because the TEST'S OWN COMMENT was wrong** — it claimed the listing depends on "taxonomy.yml carrying it AND the page having a description", and I had written the mutation from the comment rather than from the mechanism. That is the stale-rationale defect this item had written into §12.8 twenty minutes earlier: a dead anchor fails loudly, **a false explanation fails never** and sends the next editor to the wrong file. Both the case and the comment are fixed; **run 2 returned all 8 render guards proved by hand**, and the corrected case breaks on the dependency that is actually real. **RENDERED READ 1 FOUND A SECOND ONE OF THE SAME SHAPE:** three test comments cited sources this page does not carry — the Pituitary Network Association, PMC12121368 and NIDDK. The PNA appears in the page exactly once, in the support section as an ORGANISATION, never as a source. Two were pure comment defects (the claims rest on cited AANS, StatPearls, St. Jude and Boston Children's). **The third was worth checking properly rather than tidying away**, because the half it supported — "for some people a crisis is how the problem gets found at all" — is a SAFETY claim, and an uncarried source would have made it a live §12.2 defect. Verified against the LIVE publication per this item's standing rule: the **cited** Endocrine Society page states it verbatim — *"Some people don't know they have AI until they have a sudden worsening of symptoms called an adrenal crisis"* — and also names "pituitary tumors, pituitary surgery, or radiation damage to the pituitary", which carries this page's "if this growth, or an operation on it, has harmed the gland" better than the line the comment had used. **Checking paid in both directions: MedlinePlus does NOT support that claim, so citing it would have been a fresh error.** **RENDERED READ 2 (targeted at the tiers, the gate and the composed joins) found no new defects** — both tiers survive composition, the join into the shared block is clean, the block's shunt clause and this page's tier do not contradict because the tier says "This rule is yours even if you do not have one of those tubes", the override precedes the block's weaker phone-call line, and the merged source list shows NCI ×4 and CRUK ×2, exactly as the front matter records. **VERIFIED: harness 154/154 on LF AND CRLF, handproof 8/8, render capture clean (32,417 chars, floor 12,000), privacy scan 0 findings over 2,329 net-new lines, ContentCheck 273/0 at grade 5.6.** **SHIPPED AND LIVE 2026-09-18.** Commit `07fc3d4` → PR #147 (CI pass 4m6s) squash-merged to `develop` as `748b707` → release PR #148 → `main` `6f863f1` → deploy run `35375547580` green. **THE POST-DEPLOY SMOKE IS CLEAN AND IT IS FALSIFIABLE: `/tumors/craniopharyngioma` went 10,762 → 46,159 BYTES**, all seven positive fragments present, controls 2/2, and **both NEGATIVE probes silent** — "apoplexy" and the unsourced "Neither of these is common" — which is the only way to show that this item's two rulings-that-are-ABSENCES actually shipped. `/tumors` grew 16,780 → 16,994, picking up the new description, independently corroborating what the handproof established about the index. **The smoke was run BEFORE the deploy as a negative control and returned EXIT 1 ("did not land"), not EXIT 3 ("probe broken")**, so the instrument was proved able to say YES before any weight was put on it saying NO — WI-538's inverted first version is the reason that step is not optional. **TWO LATE DEFECTS, BOTH FOUND BY TOOLS RATHER THAN BY READING, AND BOTH THE SAME SHAPE: A RATIONALE THAT WAS FALSE.** (1) The handproof's one survivor: the tumor index depends on `taxonomy.yml` carrying the slug and NOT on the page having a description — a weak MUTATION, not a weak guard, and I had written the mutation FROM the test's own comment, which was wrong. (2) The first rendered read found three test comments citing sources this page does not carry (PNA, PMC12121368, NIDDK); the claims were all carried, and the adrenal one turned out to be stated VERBATIM by the cited Endocrine Society page — *"Some people don't know they have AI until they have a sudden worsening of symptoms called an adrenal crisis"*. Checking paid both ways: MedlinePlus does NOT support it, so citing that would have been a fresh error. **AND A THIRD, IN MY OWN SHELL: the branch-deletion guard printed "refusing to delete" and then deleted anyway** — `|| echo` announces enforcement without enforcing. The outcome was still correct because `git branch -d` refuses unless merged, so GIT enforced what my guard only claimed to; the check itself misfired because #147 was SQUASH-merged, so the commit is not an ancestor of `develop` and never appears in `git branch --merged`. **A guard that reports instead of enforcing is this item's own §12.8 lesson, committed three hours earlier, arriving in the last command of the night.** | |
| **WI-539 (done — history)** | **Shipped 2026-09-17.** **WI-539 (Pituitary tumor, deepened) — STARTED 2026-09-16**, branch `feature/wi-539-pituitary-tumor` (from develop at `0c347a4`). Deepening a 50-line stub to §12.3's seventeen sections. This is a **TUMOR HUB**, so **§12.3 and §12.9 govern** — not §12.11, which shaped WI-538. **Five scoping rulings settled BEFORE drafting, from reading the corpus rather than a source pack (WI-538's first lesson):** (1) **WATCHING IS ROUTED, NOT OWNED.** `/treatments/watch-and-wait` already names pituitary tumors in its who-is-watched list, carries the eye-test and hormone-blood-test detail and a per-tumor note, and links back here — and it carries **WI-521's binding ruling that NO per-tumor schedule is printed**, so this hub must publish no surveillance interval. (2) **THE HUB OWES ITS OWN CONDITIONAL EMERGENCY TIER.** `[ESCALATION]` files "a headache much worse than usual" and "a sudden change in your vision" as SAME-DAY and never mentions double vision, so it **under-triages pituitary apoplexy and adrenal crisis**. Same shape as WI-534's shunt rule; §12.10 says a conditional survives the "hub you have thought about least" test. **This is the item's central safety claim and MUST be sourced, not asserted.** (3) **THE NAMING AUTHORITY IS NOT CNS5.** §12.1 sends naming and grading to WHO CNS5, but adenoma → PitNET was renamed in the WHO **Endocrine and Neuroendocrine** volume (2022), a different book. No prior hub has raised this; the ruling goes in front matter per §12.13. (4) **THE STUB CITES A BARRED SOURCE** — `cancer.gov/types/brain`, the same NCI source WI-538's stub carried, and §12.1 says a page citing NCI for a tumor name is a defect. It also cites Cancer Research UK, which WI-538 took nothing from on idiom grounds. (5) **BOTH "PRIMARY PATHS" ARE UNBUILT.** WI-553 (transsphenoidal) and WI-551 (vision and hormone tests) are unchecked, and `transsphenoidal` appears **zero** times corpus-wide, so neither can be linked and the hub must own a proportionate slice. **NEXT: read SYNTHESIS.md's pituitary section and `wi522-pituitary-tumor.html`, then build the source pack.** **PAGE NOW DRAFTED AND REWRITTEN ONCE: grade 5.3 against the hard 6.0, ContentCheck 273/0.** **ALL FIVE BLOCK DECISIONS WERE MADE BY OPENING THE BLOCK, AND TWO OF MY FOUR STARTING ASSUMPTIONS WERE WRONG.** IN: escalation (with this page's own conditional tier beneath it), causes, caregiver. OUT: mechanism, because it opens "These are the ways a tumor IN THE BRAIN causes symptoms" and then asserts brain swelling, seizures, blocked fluid and a lobe-by-lobe map, none of which is this tumor's story, and it carries NO scoping clause; and crosswalk, because it is entirely the 2021 CNS rewrite and this report has no CNS grade and no NOS or NEC. Both exclusions must be pinned by tests with mutations that compose the block back, since an excluded block leaves no trace. **THE SOURCE PACK COST MORE THAN THE WRITING.** Two of the best patient-level sources cannot be saved at all: cancer.org renders in JavaScript and returns a 38 KB navigation shell with an empty title, and mayoclinic.org returns HTTP 403. My own fetcher reported "13 fetched, 0 FAILED" over EIGHT ACS shells containing none of their prose, which is the third tool this project has caught reporting success over nothing. Fixed with an expect-string column so a shell is a FAILURE, the eight empty files deleted, and nine real sources now carry a 41-quote claim-to-source map verified by a checker PROVED able to fail against a deliberate false control. **THE CAREGIVER GUARD CAUGHT MY OWN RECORDED LESSON.** CaregiverSectionTests locates the block with a raw IndexOf over the whole file, so the bracketed directive names I wrote into front-matter comments became the first match and the page reported the block as not under its heading. That is §12.8's WI-538 rule exactly: a front-matter comment is inside the string every raw-file guard reads. Fixed by de-bracketing the comments rather than loosening a guard eighteen hubs depend on. **RESTATEMENT WENT 16 FILES TO 1, AND THE TOOL TRUNCATES.** Both the guard and the replica take only four collisions per file, so the first run was never a full inventory and fixing the visible four exposed a fifth beneath the cap. Work it to zero; read each pass as the next four. Also: rewording must change the part the CHECKER matched, not the part you remember writing, which is why the first median rewrite still collided. **COLLISIONS NOW ZERO AND THE SUITE IS GREEN: 2183/0 (2155 before), ContentCheck 273/0, grade 5.3.** The last collision was the outlook section opening with a sentence copied WORD FOR WORD from the sibling hub used as its model — only two files in the corpus ever contained it. **The opening sentence is the likeliest thing to collide, because the opening is the part you carry over without re-deciding it.** The reported window began with a bare `outlook` belonging to the `:::outlook` DIRECTIVE line: not a heading, so the shingler keeps it. **THE RESTATEMENT GUARD IS BIDIRECTIONAL** — this page's first draft turned TWO SHIPPED pages red (`/tumors/pediatric-brain-tumor`, `/treatments/watch-and-wait`), and that is the decisive argument for rewording over allowlisting, since an allowlist on the NEW page cannot fix the OLD pages' failures. **This page therefore ships with NO allowlist at all.** Also: the replica loads `allowlist.txt` from its own DIRECTORY with no override, so a per-item copy in `wi539/` makes "no allowlist" a property of the directory rather than something to remember. **`PituitaryTumorPageTests.cs` WRITTEN — 28 tests (22 content, 6 render), all green**, with the emergency tier enumerated SIGN BY SIGN, both block exclusions pinned as an EXACT CLOSED SET (an excluded block leaves no trace, so nothing else would stop a later item restoring one), all three renames named AS renames, and `AssertDoesNotRestateTheCorpus` called with no deliberately-shared entries. **I WROTE A GUARD THAT COULD NOT FAIL AND CAUGHT IT BY READING, NOT BY RUNNING:** the adrenal test sliced the section at `IndexOf("If you take steroid replacement")` then asserted the result `StartsWith` that same string — a tautology invisible to the compiler, the suite AND the break harness, since a test that cannot fail never reports. §12.8's iterate-and-check rule in a new place: **a guard that SELECTS on a string and then ASSERTS that string is checking the selector, not the page.** One real failure, and it was mine not the page's: `EveryLinkOnThePageResolves` demanded five doors inside "Where to get support" because I copied Meningioma's list without checking where THIS page puts its doors — it routes CONTEXTUALLY (MRI from diagnosis, pathology from the report section, watch-and-wait from treatment), which is better placement, now pinned by its own position guard. Checked rather than assumed: `/tumors` back-links exist on only 4 of 16 tumor pages, so the absence is NOT a defect and is pinned as deliberate; "specialist nurse" is on TEN other hubs, so it is corpus convention and is deliberately NOT on this page's British-form ban list. **HARNESS WRITTEN AND DRY-RUN CLEAN: 75 mutations, 0 problems, across `pituitary-tumor.md` and `escalation.md`.** `uniq()` REPLACES WI-538's `rep()`: it asserts the anchor matches EXACTLY ONCE and raises otherwise, because this page's front matter quotes its sources at length and "apoplexy", "transsphenoidal", "adrenal crisis" and "medical alert bracelet" all appear ABOVE the body they also appear in — WI-538's silent survivor is the default here, now a loud refusal. Both runner copies assert they loaded THIS item's table, since copying them unedited would mutate WI-538's pages and report confident success over the wrong item. **HARNESS GREEN: all 75 breaks fail correctly on LF AND CRLF — 150 runs, zero survivors, zero no-ops, zero ambiguous anchors.** Both exclusion mutations turned red, so the blocker-that-is-an-absence is genuinely pinned; so did `escalation-block-taught-about-apoplexy`, the tempting "fix" that would have put a pituitary emergency on every glioma hub; so did all seven apoplexy signs INDIVIDUALLY; and so did `adrenal-condition-unscoped`, which is the guard I rewrote by hand after catching my own tautology, now proved by machine instead of by my reading. **HANDPROOF GREEN: all 8 render guards proved by rebuild.** **AND THE TOOL PROVED ME WRONG ON ONE.** I judged `TheGlossaryFiresOnTheWordThisPageDoesNotDefineItself` too weak to break and wrote it into the handproof as an expected SURVIVOR rather than acting on that reading — it failed correctly, because `extra-axial` is this page's ONLY tooltip, so "some tooltip fires" and "THE tooltip fires" were the same assertion. Had I trusted the reading I would have rewritten a working guard on my own authority and called it a fix. **RENDERED READ 1 OF 2 FOUND TWO REAL DEFECTS, AND NO GUARD COULD HAVE SEEN EITHER.** (1) **THE GLOSSARY TOOLTIP ECHOED THE SENTENCE IT LANDED IN** — the page said "a word for growing outside the brain itself" and the popover answered "Growing outside the brain itself, not inside it", WI-535's defect word for word. **The fix is NOT suppression, and the corpus is why:** `/tumors/meningioma` glosses the same term inline and lets it fire, and there is no `!%extra-axial%` anywhere in Content/, so suppressing here would make one term behave two ways on two pages — §12.10's one-claim-one-strength applied to vocabulary. Reworded instead so the page NAMES the term and the glossary DEFINES it, which also makes the guard's own name true for the first time; and the guard now asserts `def-extra-axial` by slug rather than a bare `def-`, since the renderer emits `popovertarget="def-{slug}"` and the old assertion was specific only by accident of today's corpus. (2) **THE SELF-BLAME BLOCK SAID "BRAIN TUMORS" FIVE TIMES TO A READER JUST TOLD THIS IS NOT ONE** — composed, `[CAUSES]` reads "For most brain tumors, nobody knows the cause", "does not cause brain tumors", "stress causes brain tumors", "a brain tumor", "Brain tumors are not contagious", on a page whose thesis is the opposite. A reader could reasonably conclude the section is not about them, and self-blame is the ONE thing it exists to answer. Fixed with a scoping bridge ABOVE the directive, in this page's own words — precedent is `/tumors/all-brain-tumors`, the only other page that scopes this block, which does it for a different reason (no diagnosis yet), so copying it would have collided. Pinned by a new test and three mutations, because an unpinned bridge is one edit away from gone. **Two non-defects recorded rather than fixed:** two Cancer Research UK entries DO reach the reader's composed source list via the escalation block, exactly as this page's front matter predicted and a front-matter check cannot see; and the shunt, chemotherapy and seizure rules reach a pituitary reader as CONDITIONALS, which §12.10 permits. **RE-VERIFICATION AFTER BOTH RENDERED-READ FIXES IS GREEN: shingles 0 across 2,329 with NO allowlist, dryrun 78 mutations / 0 problems, build clean, grade still 5.3, ContentCheck 273/0, suite 2,184 / 2,184.** It took two passes. **A CORPUS-WIDE HOUSE RULE I HAD NEVER MET FAILED THE NEW SENTENCE:** `NoEmDashInCopyTests` bans em and en dashes in page copy, and the scoping bridge carried one. The guard was right and the page was wrong, so the sentence was split in two rather than the rule bent. Before writing that bridge I had checked the PRECEDENT for the pattern, the RESTATEMENT risk against the page it borrows the pattern from, and the READING LEVEL — three of four. **The lesson is not "avoid em dashes"; it is that a new page joins a corpus whose house rules its author has not read, and the suite is the index to them.** Worth recording what did NOT need changing: all three assertions in the new scoping test survived the rewrite untouched, because each anchors on a fragment that does not span the punctuation. **A guard written about the PROPERTY rather than the PROSE costs nothing when the prose moves** — which is the same principle that made the sign-by-sign emergency enumeration worth the length. **HARNESS RE-RUN GREEN: all 78 breaks fail correctly on LF AND CRLF — 156 runs, zero survivors, zero no-ops, zero ambiguous anchors**, now including the three new mutations that pin the scoping bridge (dropped, claim inverted, and moved BELOW the directive, since a bridge a reader meets on the way out has not bridged anything). **THE RESTORE WAS VERIFIED POSITIVELY RATHER THAN INFERRED, AND THAT IS THE HABIT WORTH KEEPING.** The harness rewrites the page once per mutation per line-ending and restores the original from memory in a `finally`; `git status` showed the file MODIFIED afterwards, which is expected on a feature branch and is also exactly what a leftover mutation looks like — §12.8's WI-515 point is that abandoned mutations read as your own prose. So the check was two-directional: seventeen mutation payloads confirmed ABSENT (`This tumor is one of them`, `Call your team the same day`, the two excluded directives, `recognise`, `0.5 mg`, `About 40%`, the /tumors backlink, the lifted sibling sentence, and the rest) AND five of the page's own sentences confirmed PRESENT at their expected lines. **Absence alone proves nothing: a truncated or half-restored file satisfies it.** Also relevant while a harness is in flight: a tool notice reported the page as changed on disk, which was the harness mid-cycle. Reading it then would have shown a sabotaged page and editing it would have been silently reverted by the `finally` — the file is only meaningful once the marker file is gone. **HANDPROOF RE-RUN CAME BACK 7 OF 8, AND THE EIGHTH WAS MY MUTATION'S FAULT, NOT THE GUARD'S.** `TheGlossaryFiresOnTheWordThisPageDoesNotDefineItself` survived because the mutation changed `**extra-axial**` to `extra-axial` — stripping the EMPHASIS, not the term. `GlossaryMarker` matches the word, so `def-extra-axial` still rendered and the guard was correct to pass. **A WEAK MUTATION READS EXACTLY LIKE A WEAK GUARD**, which is §12.8's WI-537 shape (changing a front-matter `slug:` proves nothing, because routing reads the FILE PATH), and the failure mode is that you "fix" a guard that was working. The break now replaces the term itself, which is what the guard actually reads. **Note this one guard has corrected me TWICE IN OPPOSITE DIRECTIONS** — first predicted to survive and it broke, then assumed to break and it survived. Both times the reasoning was confident and wrong, and both times the tool was asked rather than consulted, so it cost nothing. That is the case for handproofing render guards instead of reasoning about them. Both target files verified restored again: marker gone, all four payloads absent, and the sibling's back-link intact at exactly one occurrence. **READ 2 OF 2 IS DONE (all 394 lines) AND IT FOUND THREE MORE DEFECTS, WHICH IS THE ARGUMENT FOR THE RULE BEING TWO READS.** Read 1 found two; read 2 found three that read 1 walked straight past — not subtler, just ordinary words a first pass was too busy checking structure and safety claims to hear. **BRITISH IDIOM, ON A PAGE WHOSE OWN FRONT MATTER BARS CANCER RESEARCH UK AS A SOURCE *FOR IDIOM*** — shipping it in my own prose would have been the same defect one step further in. `"tablets"` (×8), `"optician"`, `"junctions"`. **The green suite proved nothing**: `CuratedPage.BritishForms` is documented as deliberately incomplete and determiner-bound and could not see any of the three. **THE CORPUS DECIDED IT, BY COUNTING.** "optician" and "junction": ZERO occurrences anywhere else under Content/, so both were mine alone. "tablets": the corpus says **"pills" 56 times across 14 files**, while every other "tablets" hit is the DEVICE sense (iPads, magnets near a shunt) or a verbatim quote inside a front-matter comment — body copy meaning medicine had TWO corpus-wide against my EIGHT. **And the rule is sharper than "ban the word":** TestsLibraryPagesTests records that `"tablet"` was deliberately LEFT OFF the shared list because US labeling says "take one tablet", and *only the plural, meaning pills in general, is the idiom* — so the page-local ban is on `tablets` alone. SteroidsPageTests and TumorTreatingFieldsPageTests had already reached the same conclusion the same way, which confirms this is the corpus's method rather than my improvisation. **Compare `specialist nurse`, checked identically and deliberately NOT changed** because ten hubs use it: same procedure, opposite answer. Fixed atomically across all three surfaces in one pass — 10 page edits, 4 test edits (including the three new page-local bans), 2 mutation anchors — because piecemeal rewording is exactly how WI-538 produced dead anchors; plus 3 new mutations, since an unpinned ban is one edit from gone. Other composed-block observations recorded and NOT acted on, all §12.10-permitted conditionals: the caregiver block's "If they have seizures" on a tumor that rarely seizes, the duplicate `/get-help-now` route, and the two CRUK entries plus a `tumour` spelling in the composed source list. **RE-VERIFICATION AFTER THE IDIOM FIXES IS GREEN, INCLUDING THE RISK THAT MATTERED: shingles 0 across 2,331 with NO allowlist** — moving eight sentences onto "pills", a word the corpus already uses 56 times across 14 files, did NOT walk into the restatement guard, which was the live danger in fixing an idiom by adopting the corpus's own high-frequency vocabulary. Also dryrun 81 mutations / 0 problems, build clean, grade still 5.3, ContentCheck 273/0, suite 2,184 / 2,184. **THE ORDER OF THE REMAINING GUARDRAILS WAS WRONG AND IS NOW CORRECTED.** I ran the break harness twice and the handproof twice before noticing that the guardrail list puts **`/review` AHEAD of the harness**, and for a reason worth stating: review findings change the page, and a ~20-minute harness run plus a handproof spent on a state the reviewer is about to invalidate is pure waste. Worse, running either DURING a review is actively harmful rather than merely wasteful — both mutate the page the reviewer is reading, so it would report findings against deliberately sabotaged text, manufacturing false blockers and hiding real ones. **`/review` is running now (code-reviewer agent, adversarial brief covering the two emergency tiers as the item's central safety claim, sourcing, the three renames, the "benign" and "not a brain tumor" comfort readings, the five block rulings with instructions to argue the other side, tautological guards — since I found one myself — and contradictions with `/treatments/watch-and-wait`, `/tumors/meningioma` and `/tests/pathology-report`).** **`/review` ROUND 1 RETURNED 3 BLOCKERS, 14 SHOULD-FIX, 4 NITS. ALL THREE BLOCKERS ARE CLOSED.** **A1 — THE TIER DOWNGRADED THE BLOCK ABOVE IT.** `blocks/escalation.md` files "suddenly not being able to speak, move one side, or see" under CALL AN AMBULANCE / 911; my apoplexy signs include sight dropping and confusion; and my tier — introduced as "the list above does not cover them", so read as the authoritative one — offered a PHONE CALL. 911 appeared nowhere on the page. It matters past apoplexy: a thunderclap headache with a vision change is also how a subarachnoid hemorrhage presents. **My guard could not see it**: `demoted` knew only same-day/next-day/morning vocabulary, so a fall from AMBULANCE to "call your team" matched nothing and passed — a guard policing only the demotions its author imagined. Fixed in the tier, in the guard, and with two mutations. **A2 — ONE SYMPTOM, TWO STRENGTHS, TWO PAGES.** `/treatments/steroids` files "cannot keep your pills down" as SAME-DAY; this page files it as the same HOUR. Neither was wrong and neither said why: **dexamethasone for swelling and a steroid REPLACING a hormone the body can no longer make are different emergencies**, and that page's list was written for the first. §12.10's remedy is a conditional, not a change of strength — so the sibling now carries one, with the Endocrine Society adrenal source added to ITS front matter (its only endocrine citation), my page routes there for the first time, my test READS the sibling rather than assuming it, and three mutations pin it on that file. **A3 — THE ADRENAL TIER HAD A TRIGGER AND ALMOST NOTHING ELSE**, and every gap was fillable from verbatims already sitting unused in my own front matter: the stress list is "illness, infection, surgery, or an accident" (I had only illness, on a page whose readers have operations), the crisis signs, the injection's purpose, the destination ("go to the hospital immediately"), and the reader who finds out they have the problem when a crisis is the first sign. I had cited a source for a claim I never made. **TWO OF THE REVIEWER'S PROPOSED FIXES WERE REJECTED, AND THOSE ARE THE INSTRUCTIVE ONES.** S6's "surgery is usually first for acromegaly and Cushing's" is nowhere in the fetched Endocrine Society text — publishing it would have been WI-538's citation-written-from-memory defect, so the concern gets an ASK rather than a claim. S12's "run the mechanism bans against `Composed`" would have FAILED: the composed page legitimately carries "shunt" and "seizure" via `[ESCALATION]`, as the rendered read shows, and a guard that fails a correct page is worse than no guard. **A finding can be right while its proposed fix is wrong; verify both.** Also done: the unsourced "sight often improves" promise removed (§12.12 — nothing in the source supports it), "surgery only if the medicine fails" softened to what the source says, "renamed" corrected to "proposed… endorsed by eight societies", the Pituitary Society attribution corrected (the objection is an EDITORIAL of the journal's editors' personal views, not a Society position — the real 2019 statement was never fetched and is not cited), the short version now names both emergencies, and "admitted to hospital" Americanised. **THE BLOCKER-FIX PASS COST TWO SUITE FAILURES AND ONE BROKEN ANCHOR, ALL THREE MINE.** (1) **I BROKE MY OWN ATOMIC-UPDATE RULE WHILE CITING IT**: the Pituitary Society attribution was corrected in the page and NOT in the assertion pinning it, which is the dead-anchor defect I have quoted all item. (2) **THE FIX FOR ONE IDIOM INTRODUCED ANOTHER** — the replacement sentence said "the journal that *specialises*", British spelling written into the edit that was removing British idiom. `CuratedPage.BritishForms` cannot see it, and the STEM cannot be banned because this page correctly says "specialist nurse" and "hormone specialist"; only the verb forms pass the no-correct-sentence-contains-it test. **A ban list is not a spell-checker: it enumerates the forms somebody thought of, and editing prose generates new ones.** (3) Three owed strings carried the page's hard wrap as `\n`, but they are matched against `PlainOf`, which flattens whitespace — a needle with a newline can never match. **`uniq` also caught a fourth problem before it could become a harness mystery**: `apoplexy-demoted-to-same-day` matched ZERO times, because the B1 fix put "Otherwise" in front of "call your team now" and lowercased the C. That is the helper doing exactly what it was written for. **ONE ALARM WAS A FALSE ONE AND IS RECORDED AS SUCH:** checking the edited `/treatments/steroids` with this item's shingle replica reported two colliding files — every one PRE-EXISTING text I never touched, its ambulance paragraph being shared with `[ESCALATION]` DELIBERATELY under §12.10. That page has its own allowlist; I had pushed it through this item's deliberately empty one. **The per-item replica is correct only for the item's own page** — for any other page the suite is the authority, and it passed every steroids test while the replica raised alarms. **THAT VERIFICATION CAME BACK FULLY GREEN (shingles 0, dryrun 87/0 across 3 targets, grades 5.4 and 4.8, suite 2,184/2,184), AND THE WHOLE OF REVIEW ROUND 1 IS NOW ADDRESSED — 3 blockers, 14 should-fix, 4 nits.** Done in this pass: **S1** the front matter claimed TWO sources were cited-but-uncheckable and named `mayoclinic.org` as the second, while it appeared in no source list and this page's own test banned it — §12.14 one level up, a note describing a problem reading as though the problem were handled; it now says plainly that it 403'd and is not used. **N1** NCI reaches the reader's composed source list three times via CAUSES and CAREGIVER — §12.1 still holds (no naming claim), but the front matter anticipated that leak class for CRUK and not for NCI, so the next reader would have re-opened it. **S4** the post-operative fluid rule, whose source (`neurosurgery with a transsphenoidal approach usually induces AVP-D`) was already quoted in my own front matter while the page named the condition only as vocabulary, in the report section, with no action attached. **S6** REJECTED AS WRITTEN and answered differently: "surgery is first-line for acromegaly and Cushing's" is nowhere in the fetched Endocrine Society text, so the gap is closed with an ASK ("which of those three does my hormone point to?") rather than a claim. **S8** the grade sentence tightened so a reader holding a Knosp-graded operative note cannot read it as "your paperwork is wrong" — Knosp itself is NOT published, because no fetched source describes it. **S14** the pathology-report link now says it is written for reports that carry a grade and come from tissue, and yours may have neither. **N3** a probe regex that could not cross a line wrap. **N4** an assertion that was a floor any page clears. **N2 WAS WRONG AND COUNTING FIXED IT.** The nit claimed the corpus holds two conventions for where a scoped tier sits. Grepping all fifteen hubs instead of the two named: FOUR pages put a FORWARD POINTER above the block and the tier BELOW it, saying so outright; the two named carry the same SHARED spinal-cord sentence, a §12.10 case rather than a tier. **The convention is unanimous — tier below, pointer above — and this page had only the first half**, which is WI-538's blocker shape: a reader meeting the most specialised rule on the page with no warning. Pointer added, worded deliberately unlike the siblings because their phrasing is an eight-word run with four content words and would have collided. **OF 21 REVIEW ITEMS, THREE WOULD HAVE MADE THE PAGE WORSE IF APPLIED VERBATIM** (S6, S12, N2) — every underlying finding was useful; only the remedies were wrong. **Two edits in this pass were REFUSED because I reconstructed their anchors from memory instead of reading them** — the recurring defect this row already names — and that refusal is the right failure: `Edit` declines rather than matching something approximate, which is the same property `uniq` gives the mutation table. **VERIFIED GREEN AFTER THE FULL ROUND-1 FIX SET: shingles 0 across 2,557 (up ~100 from the new sentences, none colliding, including the forward pointer worded deliberately unlike the four siblings that carry one), grade HELD at 5.4 against the hard 6.0, ContentCheck 273/0, suite 2,184 / 2,184.** One stale mutation anchor — the third this pass — caught by `uniq` at dryrun rather than surfacing later as a harness mystery: the S1 fix rewrote "TWO SOURCES ARE CITED…" to "ONE SOURCE IS CITED…" and I moved the text without its anchor. Fixed. **HARNESS RUNNING NOW: 90 mutations, LF AND CRLF, across three targets (`pituitary-tumor.md`, `escalation.md`, `steroids.md`), gated behind a clean dryrun.** While it runs, all three content files are untouchable — including for READS, since a read can catch a mutated state — and `mutations.py` is loaded by the running process. **HARNESS GREEN AFTER THE FULL REVIEW ROUND-1 FIX SET: all 90 breaks fail correctly on LF AND CRLF — 180 runs, zero survivors, zero no-ops, zero ambiguous anchors**, across three targets (`pituitary-tumor.md`, `escalation.md`, `steroids.md`). **Restore verified in BOTH directions rather than inferred from `git status`**, which cannot tell a leftover mutation from uncommitted work: all mutation payloads absent from all three files, and one occurrence each of the page's own sentences present. Marker gone. **THEN A DEFECT I FOUND BY ACCIDENT, IN A TRUNCATED EXCERPT A TOOL HAPPENED TO PRINT: CORRECTING A CLAIM IS NOT CORRECTING THE RULING.** /review's S2 was fixed in the page's PROSE while THREE places still recorded the disproved reasoning — the front-matter note on PMC9759163 ("Endorsed by eight societies INCLUDING the Pituitary Society, **the same body that objects to the PitNET rename**"), the note on PMC9170656 ("A POSITION PIECE ARGUING ONE SIDE"), and `PituitaryTumorPageTests.cs:437`, which said the objection was "attributed to the Pituitary Society" **seven lines above the comment written during the fix, which says the opposite**. One test held two contradictory accounts of one source. **§12.13 makes the front matter the first place the next editor looks to find out WHY a sentence is worded as it is, so a rationale that outlives its correction does not sit inert — it argues for putting the error back, with apparent authority, to somebody with no reason to re-verify.** All three corrected; the endorsement half is sourced and stays, now quoting the eight societies in full. Blast radius checked and contained: only this page and `taxonomy.yml` mention the Society, PMC9170656 or PitNET anywhere in Content/, and the taxonomy entry is a bare alias list (`also: ["pituitary adenoma", PitNET]`) carrying no attribution, so no sibling repeats the error. **RE-VERIFIED GREEN after the three stale-rationale corrections: shingles 0 across 2,557 with no allowlist, dryrun 90/0 across 3 targets, grades 5.4 and 4.8, ContentCheck 273/0, suite 2,184 / 2,184.** **THE ORDER IS DELIBERATELY INVERTED FROM WHAT THIS ROW SAID, AND A FRESH SESSION SHOULD NOT PUT IT BACK: `/review` ROUND 2 IS RUNNING NOW, BEFORE THE HANDPROOF.** The rule was learned the expensive way earlier in this item — review findings change the page, so anything that depends on the page's exact bytes belongs AFTER it settles. Round 1 changed the page in about twenty places and made an already-completed harness and handproof obsolete. The handproof is cheaper than the harness but the principle is identical. Stronger still: a mutating tool must NEVER run during a review, because it rewrites the file the reviewer is reading, so the findings would describe sabotaged prose. **Round 2's brief names what changed, and explicitly lists the THREE round-1 remedies I DECLINED — the unsourceable first-line-surgery claim, the `Composed` ban fix that would fail on deliberately shared block text, and the convention split that counting all 15 hubs disproved — inviting it to overturn them from the sources or the corpus.** A reviewer not told what was declined will either re-raise it or assume it was accepted. **`/review` ROUND 2: 2 BLOCKERS, 8 SHOULD-FIX, 4 NITS — ALL ADDRESSED, AND ALL THREE ROUND-1 DECLINES UPHELD.** **B1 — THE ADRENAL TIER'S ONLY DESTINATION WAS BURIED INSIDE A CONDITIONAL**: "If you have been given an emergency injection … you go straight to a hospital", so a reader who is confused and fainting and was never given one got a planning instruction in the future tense and a rule about vomiting. The Endocrine Society states it of EVERYONE in crisis ("need an injection … right away. **Then they need to go to the hospital immediately**"). Fixed with an unconditional 911 route placed FIRST, and **the guard is now POSITIONAL** — a substring check on "you go straight to a hospital" passed whether the destination was conditional or not, the same wrong-axis defect as the demotion guard that missed an ambulance-to-phone-call fall. **B2 — MY OWN ROUND-1 FIX EXCLUDED MOST OF THE PAGE IT EDITED.** The steroids conditional was scoped on a DIAGNOSIS ("replacing a hormone your body can no longer make … after pituitary surgery"), but that page teaches at `#why-taper` that anyone on the medicine has a suppressed axis — so a reader on long-term dexamethasone was told the urgent rule was not theirs, 160 lines after being told their body had stopped making its own. Over-reassurance introduced BY a fix (§12.12). Rescoped on the MECHANISM, which needed no new citation because it rests on that page's own sourced teaching — the reviewer's three supporting quotes came from PMC11451960, which is in no fetched pack, so they were not used. **S7 — I HAD DELETED A SOURCED SENTENCE.** "Sight often improves after the pressure comes off" was cut in the round-1 pass as unsourced; the Endocrine Society says it plainly ("After surgery, vision problems improve in most people, or they can go away all together"), and I had concluded otherwise from a grep capped at 30 hits that suppressed the line as `[Omitted long matching line]`. **Third long-line false negative of this item, and the first to make me remove something correct** — it left a subsection about losing your sight ending on an administrative line, which §12.6 forbids. Restored in direction-only form with the verbatim recorded. **S1 — ANOTHER FIX THAT INTRODUCED A DEFECT**: the post-operative thirst rule I added for round 1's S4 said the symptom IS arginine vasopressin deficiency, while its own citation (NBK470458) says "most cases of polyuria in this setting are **not** due to AVP-D". Certainty reduced to possibility; the action was right and is unchanged. **S2** "happens most often after an operation" is a ranking the source denies (idiopathic is the commonest cause) — reduced to direction. **S3** both pages' verbatim blocks stopped one sentence short of "Adrenal crisis occurs mainly in people with primary AI"; read carefully it qualifies WHO GETS a crisis, not whether one kills, so the death claim stands but neither page may imply frequency, and the qualifier is now recorded in both. **S8** two case policies in one loop meant a sentence-initial "Molecular" walked through a case-sensitive ban. **A FOURTH STALE RATIONALE, WHICH I CREATED WHILE FIXING THE OTHER THREE:** the note I wrote into `steroids.md` still argued that dexamethasone and replacement were "different emergencies, and this list was written for the first" — the exact framing B2 disproved, sitting in the file arguing to narrow the conditional straight back. **Every reviewer quote I could check was exact; the only unverifiable one was routed around rather than trusted.** N2 closed (no "emergency room" survives on the page), N3 fixed by removing a reading-level figure from a comment that had no guard behind it. **THE ROUND-2 FIX SET VERIFIED GREEN, AND THE DIAGNOSIS HELD RATHER THAN JUST THE EDIT: shingles 0 across 2,596 with no allowlist, dryrun 92/0 across 3 targets, grades 5.4 / 4.8 / 5.2, ContentCheck 273/0, suite 2,184 / 2,184 UNFILTERED (render category included — the reviewer's own run had skipped it).** `AntiSeizureMedicinesPageContentTests` went green, which is what confirms the 911 collision really was one word spanning a sentence boundary and not a shared safety claim. **THE ORDER IN THIS ROW WAS WRONG AGAIN AND IS CORRECTED: `/review` ROUND 3 RUNS BEFORE THE HARNESS AND HANDPROOF, NOT AFTER.** The standing rule is review until a round finds NO BLOCKERS; round 1 found 3, round 2 found 2, so a third is owed. Both the harness and the handproof depend on the page's exact bytes, and round 2 moved the apoplexy tier, the adrenal tier, the thirst rule, the sight sentence, the signpost and a shipped sibling — so the 90/90 harness result no longer covers this state and re-running it before a clean review round would be the third time this item paid for that mistake. **Round 3's brief lists what changed, and marks four questions as SETTLED so they are not re-litigated** (surgery-first for acromegaly/Cushing's, the RAW-vs-Composed ban, the scoped-tier convention, and the accidental collision) — each with the evidence that settled it. It also asks the reviewer to state plainly IN ONE SENTENCE if it finds no blockers, since that sentence is the ship signal and an ambiguous review would either stall the item or wave it through. **`/review` ROUND 3's 2 BLOCKERS AND ALL 10 OTHER FINDINGS WERE FIXED IN ONE COORDINATED PASS** (page + tests + mutations together, because splitting a previous pass is what produced five stale anchors). **B1 — THE APOPLEXY TIER'S ENTRY WAS A CONJUNCTION** ("a sudden, severe headache WITH a change in your sight") in three places. NBK559222 calls the headache "the most common symptom" and files the rest under "Other symptoms include"; `/start` files a LONE sudden severe headache under CALL 911. So the page sent the commonest presentation past the rule written for it AND filed one symptom at two strengths on two pages. **Seven signs were enumerated one by one and not one assertion could see it: a guard over a paragraph cannot see the scope of its lead-in**, because narrowing the entry removes nothing the guard reads. The new assertion is over the BOLD LEAD-IN. Knock-on worth noting: once the headache alone is the entry, the split route (911 for sight loss, a phone call for the rest) became a demotion below what `/start` already promises, so **fixing the scope forced the strength UP** — the whole entry now routes to 911. **B2 — A SENTENCE THAT WAS FALSE ABOUT THE PAGE IT WAS PRINTED ON, AND IT SURVIVED TWO REVIEW ROUNDS.** `/treatments/steroids` said "one item on the list below is different for you" while its same-day list also carries "You are confused", "You are being sick again and again" and "You feel faint, dizzy standing up" — the three signs the Endocrine Society names for adrenal crisis and the three this hub files as 911. It named a fatal condition with no signs and no destination. **It survived because every guard on both pages read the CONDITIONAL and none read the LIST it made a claim about;** the new guard READS THE SIBLING'S LIST. Rewritten on the shunt-conditional shape from `blocks/escalation.md`. Also S1 (the front-matter note claimed the page implies no frequency, four hundred lines above "Both of these are uncommon"), S3 (the crisis extension was gated on having been TOLD a hormone was low, excluding exactly the undiagnosed readers both sources describe), S5 (two UNSOURCED sentences deleted), S6 (a canary that was a second negative about the real page; an assertion pinning words not coverage), and four nits. **THREE OF ROUND 3's PROPOSALS WERE DECLINED AND ROUND 4 UPHELD ALL THREE:** its S4 claimed fainting/collapse were VERBATIM in NBK559222 — they are not, the chapter says "altered mental status" and "on occasion, coma", and `fainting`/`collaps`/`syncope` return ZERO hits, so the page carries "passing out" instead (**a reviewer's "verbatim" is a claim like any other, and agreeing with the conclusion is exactly when you stop checking**); no "do not drive yourself" was invented for S2 (zero occurrences corpus-wide and in no source); and the incidentaloma guideline suggested for S5 was NOT cited, because it is in no fetched pack. **`/review` ROUND 4 RETURNED NO BLOCKERS — THE SHIP SIGNAL** — plus 4 should-fix and 8 nits, all now applied or ruled on. **ITS BEST CATCH WAS A GUARD I HAD JUST WRITTEN:** the new conjunction guard banned only a SIGHT-CHANGE conjunct while the tier's own guard banned any conjunction, so "A sudden, severe headache WITH DOUBLE VISION, and being ill…" would have narrowed the summary and the caregiver section and stayed green — the round-3 blocker's exact axis, re-guarded on one wording. Fixed by asserting **ADJACENCY** (a property) rather than enumerating phrasings. Also: the description claimed two emergencies "other brain tumors do not" have, which **this item's own B2 rescope falsified two files away** (the adrenal rule now reaches a reader on long-term dexamethasone for a glioma) and which nothing guarded, §12.3/WI-524 rendering it as the first paragraph a reader meets; a stale test comment ("Confusion: same-day everywhere, and never an ambulance") that had become false sixty lines from the triad it would argue for deleting; and "extra-axial … on scan reports", whose "on scan reports" clause is a claim about the reader's paperwork that no source makes and `/tumors/meningioma` does not make either. **A NIT WAS PROMOTED ON CHECKING: "some hormone tests are timed, and some take a morning" is in NO fetched source** — the detail is real but lives in `tests-library.md`, a DOSSIER and not a source, which is the exact footing round 3's S5 deleted two sentences for. **THE RULE DOES NOT BEND FOR SMALL CLAIMS.** **THREE SELF-INFLICTED FAILURES THIS SESSION, AND TWO WERE THE SAME TRAP:** a front-matter correction that QUOTES the string its own raw-file guard bans — once for the frequency note, once (minutes later) for the glossary-echo note, both while writing a comment whose SUBJECT was that trap. **A correction naturally wants to quote the thing it corrects, and that is precisely what a raw-file guard cannot tell apart from the thing itself.** The third was a stale anchor ("go to the emergency department" after the page moved to "go STRAIGHT to"). A fourth defect was caught by the read-back rather than any guard: sharpening a vague sentence exposed a PRONOUN whose nearest antecedent had become "a report", so the page read that the report follows the operation — **precision and reference are different axes, and improving one can break the other**. **VERIFIED GREEN: suite 2,184 / 2,184 UNFILTERED (render category included), ContentCheck 273 checks / 0 failures, grades 5.4 and 4.8 against the hard 6.0, dryrun 111 mutations / 0 problems across 3 targets, shingles 0 collisions across 2,571 with NO allowlist.** **HARNESS GREEN: all 111 breaks fail correctly on LF AND CRLF — 222 runs, zero survivors, zero no-ops, zero ambiguous anchors.** Restore verified POSITIVELY and in both directions rather than inferred from `git status`: 16 mutation payloads confirmed ABSENT and 11 of the pages' own sentences confirmed PRESENT exactly once, across all three targets, marker gone. Also recorded: a tool notice reported the page as changed on disk MID-HARNESS and invited me to treat it as deliberate — it was transient, and the file has no meaningful state while a mutating tool holds it. **HANDPROOF GREEN: all 8 render guards proved by rebuild**, each failing correctly when its page was broken — including the glossary guard, now that its mutation replaces the TERM rather than its emphasis. Restore verified in both directions on both targets. **SHIPPED: §12.8 entry, backlog checkbox and close-out done; privacy scan 0 findings over 3,361 net-new lines across 7 files (it correctly enumerated the UNTRACKED test file as 1,759 net-new lines, which is the case a throwaway scanner missed in WI-538). PR #145 squash-merged to `develop` (`7210f69`); release PR #146 merged to `main` (`b143cde`); `build-test` SUCCESS on both, deploy job SUCCESS, and the smoke check passed with TWO CONSECUTIVE CLEAN PASSES ("site healthy after deploy").** **THE SMOKE ROUTES DO NOT INCLUDE EITHER CHANGED PAGE**, so shipping was proved separately and in BOTH directions against markers captured BEFORE the deploy: `/tumors/pituitary-tumor` 11,191 to 38,583 bytes with `apoplexy` 0 to 2, `Two emergencies here` 0 to 1, `a sudden, severe headache` 0 to **3** (tier, short version and caregiver — B1 landed in all three), `extra-axial` 0 to 5, `arginine vasopressin` 0 to 6, `Call 911` 0 to 2, `passing out` 0 to 2, `You do not have to be on replacement` 0 to 1; `/treatments/steroids` 35,277 to 36,375 bytes with `the whole same-day list below` 0 to 1, `are what an adrenal` 0 to 1, `get to a hospital` 0 to 1, **and the defective lead-in `one item on the list below` confirmed 0 to 0, which is the direction that matters.** **`PitNET` WAS DELIBERATELY NOT USED AS A MARKER** — it already appeared once on the live stub and would have passed whether or not anything shipped, so a deploy marker has to be proved ABSENT beforehand rather than assumed to be new. **TWO PROBES FAILED DURING THAT VERIFICATION AND NEITHER WAS LET STAND AS EVIDENCE:** a shell `grep -c` that printed nothing at all, and a Python read of `C:\tmp` when Git Bash's `/tmp` resolves elsewhere. **A blank result is the same shape as a broken probe**, and the byte growth was NOT accepted in its place — it shows that SOMETHING shipped, not WHAT. Re-run in one process with no path ambiguity. |
| **WI-538 (done — history)** | Shipped 2026-09-16. `/tumors/pediatric-brain-tumor` rewritten from a 47-line stub into a full cross-cutting page **written to a PARENT** — the first page in the corpus whose reader is not the person with the tumor. **PR #143** (feature → develop, squashed as `2a5d9a5`) and **PR #144** (release → main, which is the deploy). Grade **5.6** against a hard 6.0, suite **2155/2155** (2117 before), ContentCheck **273/0**, **81 break-mutations caught on LF and CRLF**, **8 render guards proved by rebuild**, **five rendered reads**, **4 `/review` rounds** (2 blockers, then 1, then 1, then none), **0 corpus-restatement collisions** across 3,031 shingles, privacy scan **0 net-new findings across 10 files / 2,790 net-new lines**. Also: new glossary entry `embryonal-tumor`; `/tumors/medulloblastoma` gained the suppression marker and LOST an unsourced "it is treatable" inherited from the same source with the same gap; `[SPINAL-CORD]` and `[POSTERIOR-FOSSA-SYNDROME]` both included and registered. **Post-deploy smoke: all checks passed, controls 2/2: the page moved 11,059 -> 58,962 bytes, /glossary grew by the new entry, and the removed over-reassurance is confirmed gone from the sibling.** Lessons in §12.8, headed by the blocker that was an ABSENCE — a warning sign with no tier, invisible to a guard that SELECTS sentences already matching `same[- ]day` — and by the two harness survivors caused by front-matter comments this item wrote itself. |
| **WI-534 (done — history)** | Picked up 2026-09-15, straight after WI-533 went live. **First draft written** on `feature/wi-534-shunts-hydrocephalus`: `/treatments/shunts` + `ShuntsPageTests.cs`; a CONDITIONAL shunt rule added to `blocks/escalation.md` (routes to `#warning-signs`, NINDS + Hydrocephalus Association added to the block's sources) and registered in `EscalationBlockTests`; doors appended to `blocks/mechanism.md` and `/tests/mri`. First draft green (1953 tests, grade 4.6, ContentCheck 262/0). **`/review` round 1 returned 4 blockers, 13 should-fix, 9 nits — saved in full at `.claude/work_files/wi534-sources/review-1.md`, with the first rendered read at `rendered-read-1.md`.** Blockers: the "ambulance list" pointer goes to /get-help-now, which has none; the warning list omits hard-to-wake (HA: "requires urgent attention"), blurred vision, confusion, coordination, infant signs; the recovery section calls tiredness/headache/sore belly normal while the list says right away; the new EscalationBlockTests rule reads RAW hubs and never runs. Verified before fixing: HA list ✓, MedlinePlus "about 1 1/2 hours" vs ACS "about an hour" ✓ (print both), PMC8827213 shunting a median 1.9 months after surgery ✓, FDA "use the ear opposite the shunt" ✓; the reviewer's NINDS "blurred/fixed downward/bulging" and Walton "999 list" are NOT in the fetched text — cite HA instead. Also in scope now: `/treatments/craniotomy`'s caregiver same-day list (headache/confusion/sleeping more) needs a shunt conditional linking `#warning-signs`; meningioma front-matter ruling (13) needs a shunt exception. Harness scripts already copied and repointed to `wi534-sources` (break-tests, dryrun, probe; handproof rewritten for `ShuntsPageRenderTests`). **Fix pass DONE** (page rewritten; block conditional now covers the whole same-day list; craniotomy caregiver line; mechanism pressure clause; meningioma ruling; EscalationBlockTests reads COMPOSED hubs with a positive count; ShuntsPageTests rewritten against every counter-example). Correction to the note above: NINDS DOES carry "Blurred or double vision", the bulging fontanel and "fixed downward" eyes in its general hydrocephalus list — the reviewer was right. **CODE-COMPLETE 2026-09-15:** suite 1958/0, grade 4.8, ContentCheck 262/0, rendered read ×2, 62 break-mutations green on LF and CRLF (one guard beaten and re-proved), four render guards handproofed, docs written (backlog ✓, §12.8 WI-534 entry, log). Next: commit → PR into develop → release PR into main → deploy smoke check. |
| **Next up** | **WI-547 — Hemangioblastoma** *(new page)*. Depends on: WI-513. After it, the taxonomy has no unwritten type left, so `TumorsPageTests`' "We are still writing this one" assertion and WI-548's sweep must be checked against the index BEFORE drafting. **Carry forward for `/pm`, NEW from WI-546:** `/tumors/chordoma`'s front matter leans on the WHO CNS5 Table 3 absence that WI-546 found proves nothing ("grades of selected types"); `BrainTumorPreFilter.cs` matches `germinoma` but not `intracranial germ cell` / `CNS germ cell`; no US patient source covers a child with the thirst condition who cannot reach water, or sodium swings after surgery; the fourth hub to inherit the escalation block's Cancer Research UK and PCV citations; WI-552 owes doors from `/tumors/cns-germ-cell-tumor` and `/tumors/atrt`, and a test goes red when its page appears. **Carry forward for `/pm`, NEW from WI-545:** `blocks/crosswalk.md` says "Three changes affect almost every report" above only TWO bullets, and glues the NOS paragraph onto bullet 2 with no blank line, so it renders as a run-on on all 19 including hubs; `EscalationBlockTests.Hubs` still lists four hubs against ~19 includers, so `AHubIncludesTheBlockRatherThanRetypingIt` never ran for this page; this is the third hub to inherit the escalation block's Cancer Research UK and PCV citations into its visible source list; Pathology Outlines returned HTTP 429 with a 24-hour lockout, so the retired-name slice is partial by record; NCCN patient guidelines return 403. **Carry forward for `/pm`, NEW from WI-544:** `/tumors/pediatric-brain-tumor` says "many children are given medicine" for scan sedation where `/tumors/atrt` and their SHARED St. Jude source say "some"; the specialist-nurse comparative ("can usually get you an answer sooner than anyone else") is unsourced on twelve hubs; the outlook gate's "built from children treated years ago" provenance claim is unsourced on nine; the flat-vs-modal radiation rationale differs between two pages ("affects" vs the source's "can affect"); "most families find harder than the tests" is unsourced on four hubs; `numb\w*` in `AssertNoWarningSignIsNormalised` matches the word "number"; the pediatric hub composes `[MECHANISM]` ("the skull cannot stretch") and then says a baby's skull can, thirty lines apart in one composed page; Nationwide Children's prints the non-existent gene "SMARCB4"; PBTF and Alex's Lemonade return 403 on seven URLs. **Carry forward from WI-543:** the "at any hour" clause in `blocks/spinal-cord.md` has no US source; `AssertNoWarningSignIsNormalised`'s symptom vocabulary has no pain/spasm/stiffness/skin, so three genuine reassurances on the cord page are outside any guard's reach; `EscalationBlockTests.Hubs` lists four hubs while ~19 include the block; a research-pack source-number collision (two files numbered 40). **Still open from WI-542:** the flattened-HTML render-guard defect copied verbatim into `CraniopharyngiomaPageTests.cs:1391` and `PituitaryTumorPageTests.cs:1707` (both SHIPPED); corpus-wide British-idiom sweep ("being sick" ×12 files, "straight away", "come round"); reciprocal door from `/tumors/pediatric-brain-tumor` to `/tumors/craniopharyngioma`; WI-566. |
| **Blockers** | none. WI-401, WI-404 (ESP), WI-408 (soft launch) need Dan's hands (accounts, DNS, money). |

**Branch model (since 2026-08-11): feature → `develop` (default branch) → release PR → `main` → auto-deploy to Azure.** Merging develop into main IS the deploy (CI deploy job + smoke check). Never merge main red.

**Publishing mode: AUTO, fully automatic.** Site publishes summaries that pass the automated safety checks; **no human-review claims anywhere in reader-facing copy** (deliberate — scrubbed 2026-07-31). The review queue still exists in code for flagged/reported items but is never promised to readers. Default model claude-opus-5.

**Feed card imagery (done 2026-08-01, on `main`).** Feed cards show a content-matched **photo backdrop** (faded ~20%) with the item's **readiness score as a dial** floating on top; feed is **2-up**. Images are a small human-vetted Unsplash pool in `wwwroot/img/cards/` (grouped brain/genetics/lab/data/abstract); `CardImages` picks by matching the post's words + stage to a theme — **no AI image generation**. Raw originals git-ignored; see `images/image-tags.yml` + `wwwroot/img/cards/IMAGE-CREDITS.md`. Also fixed a real **Windows pipeline bug** (claude .cmd shim needs cmd.exe) and **guardrail false-positives** (cure negation now sentence-scoped; prompt v3 forbids computed numbers) — found running the pipeline live locally.

**Local run:** the whole system runs on the PC (no Azure needed) — see `docs/run-local.md`. Dev DB holds demo items from live pipeline runs. The two `FeedTests` that used to fail locally against that data (UndatedItemsSortLastNotFirst, EarlyStageAppearsOnlyWhenTheReaderAsksForIt) were fixed in WI-402: they now page until they find their own rows instead of assuming an empty table, so the suite is green on a dirty DB and on a fresh one. `A11ySmokeTests` intermittently failed to start its Kestrel host ("The server has not been started"). WI-403 serialized `KestrelWebApplicationFactory.EnsureServer` (CreateClient is not thread-safe) and wrapped the real cause in a message that names it, so a recurrence is diagnosable instead of mute. Not proven fixed — it was never reproducible on demand.

### WI-535 scouting note (2026-09-15, partial, corpus only; no sources fetched yet) — READ BEFORE DRAFTING

Written while WI-439's suite ran. Nothing drafted, no branch.

- **TWO §12.3 HUBS, NOT A LIBRARY PAGE.** Read §12.3 for the order and §12.9 for the
  proved template (`[CAUSES]` demoted, retired-name crosswalk slice, `[ESCALATION]`
  inside the symptoms section, `:::outlook` gate). Both stubs today are ~40 lines, cite
  only `cancer.gov/types/brain` (NCI, which §12.1 bars for naming), and include neither
  [ESCALATION] nor [MECHANISM]. Brain-stem location means [MECHANISM] likely belongs,
  and then [ESCALATION] must come with it (shunt rule, `EscalationBlockTests` on
  COMPOSED hubs).
- **THE DOSSIER IS ONE LINK.** `docs/research/tumor-guides/glioma-family.md` §"Diffuse
  midline glioma" = PMC11640674, nothing else. Real sourcing is this item's work.
- **DORDAVIPRONE IS ALREADY SHIPPED, AND ALREADY SOURCED.** `/treatments/targeted-therapy`
  lists "An H3 K27M change, in a diffuse midline glioma that has kept growing after other
  treatment. Dordaviprone, taken by mouth." Its front matter carries ACS verbatim ("if the
  tumor is still growing despite prior treatment", "typically once a week") and
  PMC13224274 verbatim: FDA **accelerated** approval in 2025, confirmatory phase III ACTION
  trial (overall survival + PFS vs placebo) not yet reported — which that note says is
  more precise than the backlog's "contested". §12.10: share or route, do not restate at
  a third strength. NO survival figures from PMC13224274.
- **ALREADY OWNED ELSEWHERE:** `/tests/molecular-markers#h3-k27` owns K27M vs
  K27-altered (older reports vs the 2021 rules) — route, don't redefine.
  `/tumors/glioma` crosswalk bullet: DIPG "described where the tumor sat. Most of these
  are now called diffuse midline glioma, H3 K27-altered" — the DMG stub's "Most of these
  tumors have a change called H3 K27" is a different claim; reconcile to one strength.
  `glossary/h3-k27-altered.md` exists.
- **GUARDS THAT FLIP WHEN THE DMG PAGE PASSES 4000 CHARS:**
  `HighGradeGliomaPageTests.ThePageWarnsThatTheChildPagesItRoutesToAreStillStubs` and
  `AstrocytomaPageTests.TheHighGradeHubStillWarnsThatItsOtherDestinationsAreThin` —
  DMG is the LAST thin destination, so `/tumors/high-grade-glioma` lines 80–82 ("still
  short at the moment") must come down in the same change (backlog: "WI-535 retires it").
- **TAXONOMY PINS (WI-412):** DIPG is the pontine CHILD of DMG, not a synonym
  (`TaxonomyTests` 256–273, `taxonomy.yml` 53–62). Pages must say the same.
- **Audience:** DIPG is mostly children, so the reader is often a parent (WI-538's
  framing). Outlook gate is load-bearing here; no figures anywhere (§12.5).
- **SOURCES FETCHED (2026-09-15, by agent, 32 files):** `.claude/work_files/wi535-sources/`
  with `NOTES.md` (verbatim quotes per topic, each tagged with its .txt file; 160 quotes
  script-checked against the saved text). Spot-grepped by hand: FDA exclusion line ✓,
  DailyMed "confirmatory trial(s)" ✓, ACS DIPG/dordaviprone ✓, Together by St. Jude chemo ✓.
- **THE FINDING THAT SHAPES THE DORDAVIPRONE PARAGRAPH:** the FDA approval page says the
  pivotal evidence EXCLUDED "Patients with diffuse intrinsic pontine glioma, primary spinal
  tumors, atypical histologies, or cerebrospinal fluid dissemination", while the indication
  itself has no location limit and ACS's children's page says for DIPG "dordaviprone may be
  an option for tumors that grow after radiation treatment". So the DIPG page must not
  imply the evidence covers pontine tumors. Label: accelerated approval "based on response
  rate and duration of response"; only PMC13224274 names ACTION.
- **DISAGREEMENTS TO PRINT OR RESOLVE:** chemotherapy (St. Jude "not offered as part of
  standard care for DIPG" / Dana-Farber / NCI-CONNECT "sometimes given" / CCS lists drugs);
  re-irradiation (St. Jude "can be done safely" / PMC9144327 2022 "not typically
  recommended"); biopsy (safe / often / sometimes, with risk / skipped if risk too high);
  radiation length (St. Jude 6–7 weeks or ~3 weeks hypofractionated).
- **NAMING TRAPS IN THE SOURCES:** Brain Tumour Charity and Boston Children's say DMG was
  "previously called DIPG" (wrong — DIPG is the pontine subset); ACS conflates DIPG = DMG
  and uses "grade III or IV"; Together by St. Jude "rarely ... grade 2" vs WHO grade 4
  regardless of histology; CRUK/CCS use "brain stem glioma". WHO CNS5 (Louis 2021) governs.
- **THIN:** thalamic symptoms (no source), DIPG-specific sedation, ETV (none for DMG;
  shunts covered by St. Jude, NCI-CONNECT, CCS), liquid biopsy (research only).
- Dead: GOSH, CHOP (403), AANS, three Cancer.net, two Brain Tumour Charity, two Boston
  Children's, CCLG, stjude.org/disease, link.springer.com (200 bot wall).

### WI-534 scouting note (2026-09-15) — SPENT, kept for the record

Branch `feature/wi-534-shunts-hydrocephalus` (from develop at 90b79db). Sources
fetched to `.claude/work_files/wi534-sources/` (text copies alongside). Nothing
drafted yet.

- **SLUG: `/treatments/shunts`** (no sitemap entry exists; backlog title "Shunts and
  hydrocephalus").
- **THE CORPUS ALREADY OWNS THE DEFINITION.** `blocks/mechanism.md` (8 hubs, all of
  which also include [ESCALATION]) says a tumor can block the fluid, "the usual
  treatment is a **shunt**. That is a thin tube that carries the fluid to another part
  of the body, where it is absorbed." `glossary/hydrocephalus.md` exists (StatPearls
  NBK560875) — suppress it on the page, do not delete. The page must not restate the
  block at another strength. Door: append one sentence to that block paragraph.
  `/tumors/ependymoma` is a 39-line stub that says the tumor "can block the flow of
  fluid" and does NOT include [ESCALATION] — see the next bullet before adding a door.
- **THE ESCALATION CONFLICT IS THE ITEM'S SAFETY DECISION.** For a reader WITH a shunt,
  NINDS says "seek medical help immediately if symptoms develop that suggest the shunt
  system is not working properly" (list: headache, double vision/light sensitivity,
  nausea or vomiting, neck/shoulder soreness, seizures, redness or tenderness along the
  tract, low-grade fever, sleepiness, symptoms coming back), and the Hydrocephalus
  Association (complications-of-shunt-systems) says "see your doctor or go to the
  emergency department". `blocks/escalation.md` files headache/vomiting/sleepiness as
  SAME-DAY. Plan: a CONDITIONAL line in the block, after the two fever rules ("If you
  have a shunt, ... right away, at any hour, or the emergency department"), routing to
  `/treatments/shunts#warning-signs`, and a rule added to
  `EscalationBlockTests.AHubThatRoutesIntoATreatmentCarriesThatTreatmentsSafetyRule`.
  The page OWNS the tier (like /seizures/what-to-do), so it does not call
  `AssertNoEscalationList`; the shared instruction wording gets an allowlist entry.
  That rule means a hub linking `/treatments/shunts` must include [ESCALATION] — so NO
  door on the ependymoma stub yet; log it for that hub's item.
- **THE DOSSIER (treatment-library.md §13) IS WEAK.** Its patient source is
  healthline.com (its own note: "replace before publishing"); it also cites NBK66023
  (NCI patient PDQ — avoid). **Its "27.8% shunt failure; 13% single, 14% multiple
  revisions" is NOT what PMC8976775 says**: 28 of 85 (33%) had one or more failures,
  1/5/10-year success 77/71/67%. R2: publish qualitatively anyway. PMC8827213 (same
  Oslo cohort): early failures cluster in the first weeks (median 20 days) — attribute.
  Its ETV and programmable-valve points were marked "[Verify]": NINDS and AANS confirm
  ETV ("a limited number of patients"); FDA confirms magnets can change programmable
  valve settings and "suggests keeping products that contain magnets two or more inches
  away" (actionable number, FDA-attributed, adjustable valves only).
- **A DISAGREEMENT TO PRINT, NOT SPLIT:** ACS surgery page "Shunts can be temporary or
  permanent" vs Hydrocephalus Association "you will need it for the rest of your life";
  NINDS "multiple surgeries to repair or replace a shunt throughout their lifetime".
- **Usable verbatim:** ACS — "Placing a shunt normally takes about an hour", "hospital
  stay ... typically 1 to 3 days", VP into abdomen or "less often, into the heart (VA)".
  Walton Centre (NHS, UK — attribute) — "you may be able to feel the valve of your shunt
  behind your ear", "if you are thin ... feel the tubing ... in your neck". Hydrocephalus
  Association — overdrainage "headaches that worsen when sitting or standing and improve
  when lying down"; underdrainage "especially in the morning or after lying down"; shunt
  infection most likely 1–3 months after surgery. AANS — infant signs (vomiting, head
  growth, "sunsetting eyes"). MedlinePlus ENCYCLOPEDIA (not a drug monograph, so not the
  PLAN §5 ban) — the route behind the ear, down the neck and chest to the belly.
- Candidate second door: `/tests/mri` device/metal section (adjustable valves need a
  check around MRI — confirm source before writing).

### WI-533 scouting note (2026-09-14) — SPENT, kept for the record

The item is done. Everything below held: the §12.10 call went to shared wording,
the glossary entry was suppressed rather than deleted, "four to seven weeks" was
verified at PMC12467656, and the unowned half became the page. See the
2026-09-15 log entry.

- **`/tumors/glioblastoma` ALREADY OWNS MOST OF WHAT THE BACKLOG HANDS THIS
  ITEM.** It carries a whole `### Tumor treating fields, the device you wear`
  section with: the name to ask about, that the device is called **Optune**,
  alternating electric fields through scalp pads, that it runs alongside
  temozolomide after radiation, **"usually started around four to seven weeks
  after radiation ends"**, who it is for (tumor in the upper part of the brain,
  managing reasonably well day to day), skin irritation as the main side
  effect, **"at least eighteen hours a day"**, shaving your head, and the
  framing that this is "a legitimate thing to weigh rather than a test of how
  hard you are trying".
- **THE BACKLOG SAYS "R1 KEEPS THE 18 HOURS BECAUSE IT *IS* THE DECISION" — AND
  THE 18 HOURS ARE ALREADY SHIPPED ON THAT HUB.** So the item's FIRST decision
  is the §12.10 one and it must be made before a word is drafted: either the two
  pages share the wording verbatim with an allowlist entry (the WI-522/WI-532
  answer, and the one that keeps one claim at one strength), or the hub is
  reworded to route. Do NOT let the new page state it a second way.
- **`/tumors/high-grade-glioma` owns the DISAGREEMENT**, and it is the
  anti-hype spine: the European guideline "calls its role controversial and says
  it is not widely available there", with the instruction to ask your team
  rather than assume either way. A page that sells the device without this is
  the §12.13 shape.
- **A GLOSSARY ENTRY ALREADY EXISTS** — `Content/glossary/tumor-treating-fields.md`,
  term `tumor treating fields`, alias `Optune`, sourced to PMC12467656. Its
  tooltip fires site-wide, so the new page will define its own subject in a
  popover unless it suppresses it (`!%tumor treating fields%`). That is the
  WI-519/WI-521 shape — and note WI-532's own ruling that an entry suppressed
  only where it is defined is decoration; here the entry is already earning its
  keep on two hubs, so suppression on the new page is the right call rather
  than deletion.
- **CHECK "four to seven weeks after radiation ends" AGAINST §12.4** when the
  item runs. It is a numeric range already live on the glioblastoma hub, and
  WI-532's lesson is that a figure inherited from a sibling still needs its
  source read.
- What is genuinely UNOWNED, and therefore the page: **carrying the device**
  (the bag, the batteries, the plugging-in at night), **scalp care and what the
  pads actually do to skin over months**, **the shaved head as a visible thing
  other people react to**, and **what it asks of the person alongside them**.
  The backlog's own framing — "a lived-experience decision rather than a
  clinical one" — is right, and the clinical half is already shipped twice.

### WI-531 scouting note (2026-09-13) — SPENT, kept for the record

The item is done. Everything below was the input. All of it held except the
centre count, which turned out to be a secondary citation in another paper's
introduction and is not published at all. See the 2026-09-13 log entry.

- **The scope line for BOTH pages is `/treatments/radiation-therapy` (WI-511),
  which already owns the mask, the simulation visit, fractionation, the weeks of
  weekdays, the tiredness, the skin rules, the hair, somnolence syndrome, the
  late effects, "am I radioactive", whole-brain radiation and re-irradiation.**
  Its "you may hear several names for it" list already carries a one-line
  Proton bullet and a one-line Stereotactic radiosurgery bullet. Those two
  bullets are the doors. The proton page is **not** a second radiation page:
  ACS says the proton patient experience is the photon experience, so the page
  routes for the day and spends itself on **access**.
- **`/tests/follow-up-scans` (WI-521) already owns radiation necrosis after
  SRS**, by name, with the eight-month timing and the explicit note that the
  clearest numbers come from SRS. The SRS page routes and must not restate it.
- **The dossier's proton framing is thinner than ACS's own.** ACS's brain
  radiation page says outright that proton "may be more helpful for brain
  tumors that have distinct edges, such as chordomas", **and that it is not
  clear whether it is as useful for tumors that grow into normal brain, such as
  astrocytomas or glioblastomas** — which is most of this site's readers. ACS's
  Getting Proton Therapy page lists, as a limitation in its own voice, that
  "more research is needed to know if it's better than traditional radiation
  therapy". That pair is the anti-hype spine and the dossier does not have it.
- **Do not publish a dollar figure or "~60% more".** The dossier's cost claim
  is sourced to `scienceinsights.org`; the real comparison (PMC13521102, Mayo
  standardized Medicare rates) is roughly two and a half times, not 60%, and a
  reimbursement rate is not what a reader pays.
- **The access sources the dossier names are marketing, a law firm blog and a
  content farm.** Replaced: **PMC13521102** (Int J Part Ther 2026) carries
  "roughly 45" US centres, "most of these centers are in large cities and
  academic centers", and the finding that people who live FARTHER from a centre
  are more likely to get proton. **PMC11905844** carries the external-review
  appeal route and what actually wins one. **PMC11699354** carries the prior
  authorization burden and the 2026 CMS response-time rule.
- **Brain metastases + SRS is §12.1-governed: ASCO-SNO-ASTRO 2022 only, never
  NCI patient PDQ** — and the dossier sources the whole "which tumors" section
  to NBK66023, which IS NCI patient PDQ. Open at **PMC8917399**; Rec 3.2 and
  Rec 3.3 are the two sentences the page needs.
- **Do not say frame or frameless is standard** (the backlog's own warning), and
  note ACS leans one way — "frameless techniques are now available that make
  this unnecessary" — while Cleveland Clinic presents two systems in use. Say
  both exist and that which one you get depends on the machine at your centre.
- **Do not publish Cleveland Clinic's "tumor control rate was 95%"**, Froedtert's
  "success rate is impressive" / "no loss of hair", or Froedtert's cost
  comparison. §12.13's shape: the richest patient-level source for the day is
  also the one with the marketing in it.

### Open threads (2026-08-13)
- **Daily pipeline is scheduled** ('BrainHarbor Pipeline', 06:00 daily, published
  to `artifacts/pipeline`). Reversible:
  `./scripts/register-pipeline-task.ps1 -Unregister`. Task Scheduler still
  captures no console output, but since WI-417 the run writes its own log to
  `%LOCALAPPDATA%\BrainHarbor\logs` (newest file = last run; path printed at the
  end of every run). Exit code (`Get-ScheduledTaskInfo`) and the admin health
  page are still the quick check; the log is where the per-item detail lives.
- **WI-409 and WI-410 shipped** (home leads with the feed; feed sorting) — both live.
- **Dan's review queue holds 134 items** (106 guardrail-flagged, 20 unclassified
  one-offs). A first pass would show whether Auto mode's bar is right before WI-408.
- **Brand name mismatch is a known, accepted state** (WI-420): the logo says
  "Brain Harbor", the title/og:site_name/RSS/domain say "BrainHarbor". Dan's
  call 2026-08-14 — leave it; not a soft-launch blocker. Don't "fix" it in
  passing.
- **For `/pm`, raised by WI-531, not blocking.** The `A11ySmokeTests` Kestrel
  flake (WI-403) now has a named trigger: two CI runs firing on the SAME SHA, a
  `push` to develop and the `pull_request` for the release, racing each other.
  The push run failed and the PR run passed on identical code, and a re-run of
  the failed job passed with nothing changed. `TestCollectionHygieneTests` stops
  two test CLASSES contending; nothing stops two whole RUNS. Worth either
  concurrency-grouping the workflow or giving the Kestrel host a retry.

- **For `/pm`, raised by WI-532, not blocking.** `out of hours` is British, is
  **not** on `CuratedPage.BritishForms`, and the corpus is split on it: the
  escalation block says "the after-hours number", `tumors/glioblastoma` and
  `tumors/oligodendroglioma` say "after hours", and six files still say the
  British form — `seizures/what-to-do`, `treatments/anti-seizure-medicines`,
  `treatments/chemotherapy` (×4), `tumors/astrocytoma`, `tumors/glioma`,
  `tumors/high-grade-glioma`. §12.10 wants one wording for one instruction.
  The item is: add `out of hours` / `out-of-hours` to the shared list and sweep
  the six in the same change. WI-532 used "after hours" rather than becoming
  the eighth carrier, and did not sweep — nine files is its own item.
- **For `/pm`, raised by WI-529, not blocking.** (1) The `tumor board` glossary
  tooltip prints `blocks/tumor-board.md`'s own opening sentence, so every page
  including the block shows the sentence twice; WI-529 suppresses it page-locally
  and the corpus fix belongs to the block or the entry. (2) `lesion` has no
  glossary entry and is used undefined on `/tests/biopsy`,
  `/tumors/brain-metastases` and `/tumors/meningioma` — WI-529 defines it inline
  and did not add an entry, because no citable definition could be fetched
  (`cancer.gov`'s dictionary is JS-rendered). (3) `blocks/escalation.md` files a
  sudden worst-ever headache as same-day; for a reader with an unread mass that
  may be a bleed, and CDC's stroke page (already one of the block's sources) calls
  a sudden severe headache a 911 call. That is a change to eighteen hubs and needs
  its own item, not a passing edit. Add to the two hubs already logged against
  that block.
- Tiny polish backlog: `data` image theme matches 0 items (widen keywords or reassign slot).
- Namecheap still has a conflicting `@` URL-Redirect record — harmless now that
  the A record answers, but worth deleting.

### M3 shipped (all on `auto/M3`, PR #5)
- **WI-301–304** golden set, CLIwrapper, classify, summarize+guardrails (numeral/banned/reading-level); connection-pool infra fix.
- **Readiness score (1–10)** — Dan's ask: how close a finding is to everyday care, stage-capped, shown on item pages + queue.
- **WI-305** review queue v1 (side-by-side, inline edit, keyboard, readiness badge).
- **WI-306** item permalink pages (6 blocks, readiness, glossary tooltips, provenance, report-a-problem → admin queue).
- **WI-307** feed patient-first with persisted early-stage toggle.
- **WI-308** SEO (sitemap/robots/RSS, OG + JSON-LD) + honest `/how-we-write` rewrite (was falsely claiming mandatory human review).
- **WI-309** site search (Postgres FTS over items + static pages).
- Prompt/style: no em dashes or AI tells; summaries validated live with Opus.
- 513 tests green.

**Readiness score (Dan's ask, built 2026-07-30):** every summary now carries a
1-10 "how close is this to something a patient can get?" score + one plain
reason (`summarize-v2`). Two-layer safety: Opus proposes within a rubric, then
`Readiness.Clamp` caps by research stage (animal/cell→2, obs→5, review→6,
trial→8, news→10; only ever lowers), and `SyncRepository` re-clamps at the
API/DB boundary as a backstop. Migration 0005. Live-validated (mouse study→2,
observational→4, honest reasons). Not yet rendered on a page — the badge lands
with WI-306. Scale is documented in `docs/content-pipeline.md` §9.

## Notes for the next session

- **Approved visual design lives at `docs/design/entry-hub-handoff/`** ("Clear
  & Kind" theme + Entry Hub home, from Claude Design 2026-07-19). It is the
  visual spec for WI-108/WI-109 and restyles later feed/item work (WI-209,
  WI-306). M1 order changed: **WI-108 before WI-102** so the axe/Playwright
  smoke test runs against the final theme. Handoff URL names that differ from
  sitemap.md (/get-help, /start-here) do NOT override the sitemap
  (/get-help-now, /start). The handoff folder is not yet committed — it goes
  in with WI-108's branch.
- **Remaining dead links**: only `/digest` (M4, needs an ESP → WI-404).
  `/research` went live in WI-209, `/trials` in WI-403.
  `/get-help-now`, `/digest`, `/glossary`, `/about`, `/how-we-write`,
  `/start`, `/privacy`, `/terms` are all live, and a `ShellPagesTests` link
  check fails the build if any *other* internal link 404s. Custom 404/500
  pages exist (WI-103), so the dead nav targets degrade gracefully.
- **M0 fully closed 2026-07-19**: PR #1 squash-merged to `main` (ce5929d),
  `auto/M0` deleted; brainharbor.org purchased (WI-001); `.env` populated;
  NCBI + SYNC keys in user-secrets. No open follow-ups.
- Planning and design are **done** — `PLAN.md` + `docs/*.md` are the spec,
  `docs/backlog.md` is the itemized plan (M0–M4; P2a–P3 not yet decomposed).
- Solution note: SDK 10 generated `BrainHarbor.slnx` (new XML solution
  format) rather than `.sln`; `dotnet build/test` handle it fine.
- Next: `/next-item` for WI-101, or `/autopilot M1`.

## Log (newest first)

- **2026-09-22** — **WI-546 is live.** PR [#159](https://github.com/badsonstudios/BrainHarbor/pull/159) into `develop` (build-test pass 3m26s), release PR [#160](https://github.com/badsonstudios/BrainHarbor/pull/160) into `main`, and the merge commit `7e8bf5a` went build-test success → deploy success first time — **the eighth release in a row to deploy on the first attempt**. **The pre-merge smoke was required to FAIL and did** (controls ok, the hub 404, the pediatric door missing, exit 1). **Post-deploy smoke clean**: the hub 200 at 61,718 bytes, 18 positives present, 20 absences absent, both doors landed, every other route byte-identical to the baseline. Committed 2026-09-21 as `9d2e94e`.
  `/tumors/cns-germ-cell-tumor`, a NEW §12.3 hub
  for a tumor the corpus had never mentioned, written for a teenager or young adult and their
  parent. The backlog named the unbuilt spinal tap page (WI-552) as its "primary path", so Gate 1
  ruled, following `/tumors/atrt`, that the spinal fluid and marker material is carried on the
  page and nothing links to a page that does not exist; WI-552 now owes the doors. Blocks 6 in /
  2 out (`[CROSSWALK]` and `[POSTERIOR-FOSSA-SYNDROME]` out, `[SPINAL-CORD]` in because the tumor
  reaches the spine through the fluid). One guarded door sentence added to
  `/tumors/pediatric-brain-tumor`. **Central rulings:** a scored absence (ten pages about this
  tumor, none gives an emergency rule, two blocked and counted as unknown), so every urgent line
  is tied to a sign and credited; the water dangers in both directions; no marker cut-offs; the
  growing-teratoma warning with both halves; no proton superiority; no cure word outside the
  outlook gate. **Proof:** suite 2,416/2,416; ContentCheck 276/0 at 5.6; 120 break mutations red
  on LF AND CRLF (240/240), first run; handproof 10/10 across 7 render guards; restatement 0/0;
  37 sources with verbatims; three `/review` rounds (1 blocker, then none, then none). **The
  item's real lesson:** an absence from a table of "selected types" is not evidence, and a test
  was pinning the unsupported "not graded" claim as true. Also: a failed retarget left the
  previous item's mutation table loaded and the dry run printed a clean result over chordoma,
  caught only by its targets line. Lessons in §12.8.

- **2026-09-20** — **WI-545 is live.** PR [#157](https://github.com/badsonstudios/BrainHarbor/pull/157) into `develop` (build-test pass 3m47s), release PR [#158](https://github.com/badsonstudios/BrainHarbor/pull/158) into `main`, and the merge commit `8215b43` went build-test success → deploy success first time — **the seventh release in a row to deploy on the first attempt**. **The pre-merge smoke was required to FAIL and did:** `controls ok (2)` first, then `/tumors/chordoma` 404, exit 1 — the controls passing is what makes that failure the hub's genuine absence rather than a probe that cannot match anything, so the post-deploy `smoke clean` is evidence and not an untested instrument. **Post-deploy smoke clean:** the hub 200 at **60,105 bytes**, past its measured 55,000 floor; all 15 positive fragments present, all 16 absences absent; `/tumors` grew 17,598 → 17,658 as the index picked up the new page, every other route byte-identical to the pre-deploy baseline.
  `/tumors/chordoma`, a NEW §12.3 hub
  for a tumor that is **mostly not in the brain** — it grows in bone at the skull base,
  the mobile spine and the sacrum, and about a third of readers have it at the tailbone
  end. That drove the block rulings: `[MECHANISM]`, `[SPINAL-CORD]` and
  `[POSTERIOR-FOSSA-SYNDROME]` all EXCLUDED (5 in / 3 out), each asserted against the
  excluded block's own words. **Two central rulings:** the emergency rules are the
  SIGNS', not the tumor's (22 chordoma pages swept, not one gives a 911/ER rule,
  re-confirmed first-hand; the absence is scoped by SUBJECT because NCCN 403s and is
  therefore neither presence nor absence); and **"it must come out in one piece" is
  FALSE at the skull base**, where the consensus group says resection "may be
  necessarily piecemeal", so the treatment section splits by location with a guard
  pinning the split by position. **Proof:** suite **2,378/2,378**, ContentCheck
  **275/0** at grade **5.9**, **71 break mutations red on LF AND CRLF (142/142)**,
  **8 handproof cases across all 7 render guards**, all three restatement guards at
  **0 collisions**, four rendered reads, privacy scan **0 findings over 2,457 net-new
  lines**. **12 sources re-fetched and recorded with verbatims**; six claims reworded
  because the sources did not support the wording. Three `/review` rounds (2 blockers,
  then 1, then none); **two findings REJECTED on evidence**, recorded with verbatims.
  One sibling edited: `/treatments/proton-therapy` gains the reciprocal door it never
  had, now guarded. **The real lesson:** a pipeline's exit status is the last command's,
  so `break-tests.py | tail` reported exit 0 over a genuine survivor — which §12.8
  already carried from WI-542 and this item repeated anyway. Lessons in §12.8.

- **2026-09-19** — **WI-544 is live.** PR [#155](https://github.com/badsonstudios/BrainHarbor/pull/155) into `develop` (build-test pass 3m34s), release PR [#156](https://github.com/badsonstudios/BrainHarbor/pull/156) into `main`, and the merge commit `5f085ed` went build-test success (4m10s) → deploy success (1m10s) first time — **the sixth release in a row to deploy on the first attempt**. A tumor the corpus had never mentioned became a §12.3 hub of ~34 sources, written for the parent of a child usually under 3. **THE CENTRAL RULING IS AN URGENCY ONE BUILT ON A SCORED ABSENCE:** seventeen ATRT-specific pages were fetched and NOT ONE gives an emergency-room or 911 rule, four re-tested by direct question and all silent — so the page invents no tumor-level rule and carries SIGN-level and DEVICE-level ones, each attributed, and says so out loud. `[MECHANISM]` is EXCLUDED because "the skull is a closed box, it cannot stretch" is FALSE for an infant and inverts this page's cardinal sign. **Proof:** suite 2,345/2,345; ContentCheck 274/0 at grade 5.5; **93 break mutations red on LF AND CRLF**; 8 render guards handproofed; fourteen rendered reads; privacy 0 findings over 3,260 net-new lines; **pre-merge smoke required to fail and did** (controls ok, hub 404, exit 1); post-deploy smoke clean at 56,965 bytes. **Eleven `/review` rounds** — blockers in 1–10, none in 11. **The item's real lesson: a guard can be vacuous because of MARKDOWN rather than logic.** `SentencesOf` splits on `(?<=[.!?])\s+` and `ReaderText` does not strip `**`, so in a bolded bullet list the character after "call 911." is an asterisk, no split happens, and one "sentence" runs on through the NEXT bullet. The assertion that the shunt INFECTION rule keeps its emergency room had been passing on the venue belonging to the bullet ABOVE it, for the whole item — invisible to review, to the suite and to the dry run. Only mutation could see it. Two of that run's three survivors were the mirror case: weak MUTATIONS, because a property recorded in two places cannot be broken by a single-point edit. Lessons in §12.8.
- **2026-09-19** — **WI-543 is live.** PR [#153](https://github.com/badsonstudios/BrainHarbor/pull/153) into `develop` (build-test pass 4m25s), release PR [#154](https://github.com/badsonstudios/BrainHarbor/pull/154) into `main`, and the merge commit `7f2f6ab` went build-test success (4m4s) → deploy success (1m4s) first time — **the fifth release in a row to deploy on the first attempt**. A 48-line stub became a §12.3 hub of ~25 sources, and the item **settled the safety-tier contradiction the corpus had knowingly carried since WI-536**: it was never one claim. Two claims wore one sentence, and levelling the three pages would have been as wrong as leaving them split — metastatic cord compression keeps the emergency room, a primary cord tumor gets a same-day call, and a carve-out applies to both and is stated as MORE urgent, not less. Two SHIPPED sibling pages moved with it. **Proof:** suite 2,313/2,313; ContentCheck 273/0 at grade 5.8; 53 break mutations red on LF AND CRLF; 6 render guards handproofed; six rendered reads; privacy 0 findings; five `/review` rounds (in rounds 2, 3 and 4 the blocker was created by the previous round's fix). **The item's real lesson:** three harness survivors, and the split between them. TWO were dead guards — a recovery-figure ban whose `` after `%` could never match, and a citation ban bound to a string neither of that paper's URLs contains. The THIRD was a weak MUTATION: it deleted a cross-reference while the operative tier sentence survived, so green was correct. "Weak mutation" is the verdict that lets an item move on, so it was made to earn evidence — *does a reader of the mutated page still get the instruction?* **And a third verdict-tool defect:** bash `grep -F` CRASHED (SIGABRT, exit 134) while choosing smoke fragments, printed nothing, and `| wc -l` reported a clean `0` for every string — including "emergency room", which occurs eight times. A dying tool piped into a counter is indistinguishable from a real negative. Lessons in §12.8.
- **2026-09-18** — **WI-541 is live.** PR [#149](https://github.com/badsonstudios/BrainHarbor/pull/149) into `develop` (build-test pass 3m36s), release PR [#150](https://github.com/badsonstudios/BrainHarbor/pull/150) into `main`, and the merge commit `373e437` went build-test success → deploy success first time — the fourth release in a row to deploy on the first attempt. A 1,372-byte stub citing one BARRED source became a §12.3 hub: 13 sources, all verified against the live publication. **A third distinct emergency shape** — exactly ONE rule, routed to primary care / urgent care / ENT and explicitly NOT 911, unlike both sibling hubs; brainstem compression and hydrocephalus named but deliberately not tiered, because EANO carries no patient-facing urgency guidance and two patient-facing sources say the opposite. Grade attributed in **two halves** (CNS5 prints no grade for schwannoma). Watching ROUTED to `/treatments/watch-and-wait`, reversing WI-540. **Proof:** ContentCheck 273/0 at grade 5.6; suite 2,249/2,249; **133 break mutations all red on LF AND CRLF, first run**; 8 render guards proved by hand; two rendered reads; privacy 0 findings. **Smoke:** hub 10,832 → 46,586 bytes, controls first, 8 positives present, 4 absences absent. **The item's real lesson:** a `/review` fix left a `**` unclosed — literal asterisks to a patient — and 2,249 tests stayed green, because every guard in the corpus strips emphasis before matching. The same prose also carried a British spelling. Both rode in on a fix that HAD been pre-checked, for collisions only: **a pre-check answers the one question it implements, and its silence is not general clearance.** Now guarded site-wide. Lessons in §12.8.
- **2026-09-15** — **WI-536 is live.** PR #139 into `develop` (build-test 3m46s), release PR
  #140 into `main`, and the merge commit's `main` run went build-test success → deploy success
  first time (`c13014c`) — the third release in a row to deploy on the first attempt.
  **Smoke:** 8/8 routes 200 with full bodies; the live ependymoma page carries the spinal-cord
  block, the ambulance carve-out, the PF-EPN crosswalk labels, the grade 1 answer and the baby's
  irritability line, with no unresolved fence or directive; `/tumors/meningioma` does not carry
  the block's tier, so the enforced split holds in production.
  **What shipped:** `/tumors/ependymoma` as a full §12.3 hub (grade 5.4) from 51 fetched sources;
  **`blocks/spinal-cord.md`**, a SCOPED shared block (the §12.10 call WI-535 deferred) included
  after `[ESCALATION]` only by hubs whose tumor can sit in the cord, covering a tumor that has
  spread to the spine and naming neck pain; `/tumors/glioma`'s "grade 2 and above ... not curable"
  scoped "for most gliomas" because it contradicted this page; glossary `posterior-fossa`,
  `posterior-fossa-syndrome`.
  **Verification:** 2065 tests, ContentCheck 271/0, 142 break-mutations caught on LF and CRLF,
  nine render guards handproofed, six rendered reads, four `/review` rounds (2 blockers, then 2,
  then 1, then none).
  **Lessons in §12.8:** a fix is where the next defect comes from (the glioma pointer broke
  something twice); a source's "cured" can belong to a different tumor in the same family
  (subependymoma); a deferral that lives in a document is not enforced; a sentence-window guard
  cannot see a paragraph; a naive grep invents "unsourced" when files carry non-breaking spaces.
  **For `/pm`, not blocking:** WI-543 inherits the strength split (now pinned by a test, with the
  map corrected — brain-metastases already opens with "its own emergency"); DMG's idiom guard
  still has the case-sensitive hole fixed on the ependymoma copy; tumor-board and NOS/NEC tooltip
  echoes remain corpus-wide.

- **2026-09-15** — **WI-535 is live.** PR #137 into `develop` (its `pull_request` run was late to
  register — "no checks reported" twice — and then passed: 2019/0, ContentCheck 267/0), release
  PR #138 into `main`, both release `build-test` runs green, and the merge commit's `main` run
  went build-test success → deploy success first time (the second release in a row since the
  WI-439 fix). **Smoke check:** `/`, `/research`, `/get-help-now`, `/tumors/diffuse-midline-glioma`,
  `/tumors/dipg`, `/tumors/high-grade-glioma`, `/treatments/targeted-therapy`, `/glossary` all 200
  with full bodies. Content probes: DIPG carries "left out DIPG" and "right away if there is a
  shunt"; DMG carries the spinal-cord rule and "With today's treatments, it is not curable.";
  `/tumors/high-grade-glioma` no longer says "short at the moment".

- **2026-09-15** — **WI-535 code-complete — `/tumors/diffuse-midline-glioma` and `/tumors/dipg`,
  two stubs rewritten as one item into full §12.3 hubs.** Grade **4.9** and **5.0**, **2019
  tests** (1959 before), ContentCheck **267/0**, **128 break-mutations caught on LF and CRLF** on
  the fifth harness run, ten render guards proved by rebuild, five rendered reads, **three
  `/review` rounds (3 blockers → 1 → none)**.
  **What the pages decide:** DMG is the umbrella (name, grade, curability at the sibling hubs'
  strength, chemotherapy note, dordaviprone, trials); DIPG owns the pons (MRI-only diagnosis
  and the biopsy choice, radiation 6–7 or ~3 weeks, the stretch after radiation, anesthesia,
  palliative care, end-of-life text behind the gate). Dordaviprone is accelerated, response-
  based, from efficacy studies of mostly adults that excluded DIPG and spinal tumors — "trials
  are still checking whether it helps people live longer", on both pages.
  **Safety:** a page-local right-away rule for a tumor IN the spinal cord (reason attributed to
  CRUK's metastatic-compression page); tiredness never explained away (St. Jude lists it as a
  late-stage DIPG symptom); new swallowing trouble same-day; and every same-day line either page
  writes says "right away if there is a shunt", because the block above makes the whole tier
  right-away for a shunt reader (round 2's blocker).
  **Also:** glossary `pons`, `thalamus`, `palliative-care`; door on `/treatments/targeted-therapy`
  (whose unsourced "a larger trial" was fixed at source); `/tumors/high-grade-glioma`'s honesty
  note retired. Lessons in §12.8 (indication vs evidence population; outlook leaking as a
  pointer, euphemism or kindness; a same-day line under a conditional block; a hard sentence
  softened behind the gate is still a second strength; heading lines merge into sentences).
  **For `/pm`, not blocking:** WI-538 inherits the parent audience and palliative framing;
  tumor-board and NOS/NEC tooltip echoes remain corpus-wide; "feeling sick" is still in
  [MECHANISM].

- **2026-09-15** — **WI-439 is live.** PR #135 into `develop` (CI 1959/0), release PR #136 into
  `main`; both release runs green, and the merge commit's `main` run went build-test success →
  deploy success, first time — the first release since the fix, on the path that used to skip
  the deploy. Smoke check: `/`, `/research`, `/trials`, `/search`, `/get-help-now`,
  `/tumors/glioma`, `/tumors/dipg`, `/tumors/diffuse-midline-glioma`, `/treatments/shunts` all
  200 with full bodies. Test-only change; no page changed.

- **2026-09-15** — **WI-439 code-complete — the Kestrel flake was a race in the test factory,
  found in the ASP.NET Core source.** Weighed before WI-535 and taken, because it fired three
  times today and on `main` it silently skips the deploy.
  **ROOT CAUSE:** `KestrelWebApplicationFactory.CreateHost` built both hosts from ONE
  `DeferredHostBuilder`, which gives every host it builds the same `_hostStartTcs`.
  `DeferredHost.StartAsync` starts nothing. It awaits that shared signal while the app's own
  `app.Run()` starts the host on an entry-point thread. So `_kestrelHost.Start(); testHost.Start();`
  made the second call a no-op, and both entry points ran DbUp and the seeder concurrently.
  When the TestServer lost, the first `CreateClient()` got "The server has not been started".
  That explains WI-503's 9-of-10 (only the first test in the class drew it) and why it kept
  landing on `TheReaderChoiceGateOpensAndClosesWithJavaScriptDisabled`. The factory never
  "poisoned itself". A retry would have hidden the cause.
  **FIX:** start the TestServer host first (a genuine `Start()`), then build the Kestrel host and
  wait on its OWN `ApplicationStarted`, polling for disposal so a failed start reports in
  ~300 ms, not 90 s. The error says honestly that the Kestrel entry point's exception is
  dropped by the shared signal. A half-started factory stops both hosts.
  **PROOF:** `KestrelWebApplicationFactoryStartTests` (3 tests, test-only start levers).
  Against the old `CreateHost` the first fails every time with the EXACT production message.
  5 mutations, one per guard, were each caught on LF and CRLF. Full suite **1959/0** three
  times (1956 + 3). The Kestrel classes ran 10× green.
  **`/review`:** 0 blockers. 3 should-fix, all taken: the Kestrel start exception was lost behind
  a 90 s wait and a wrong message; the half-started factory leaked both hosts; the doc comments
  overstated. The harness's own parser matched no test on its first run ("Total tests:" at
  normal verbosity) and was fixed, then re-run.
  **For `/pm`, not blocking:** `TestCollectionHygieneTests.UsesAWebHost` cannot see a factory
  created inside a test body. Mvc.Testing 10's `UseKestrel()`/`StartServer()` might retire the
  dual host — untried.

- **2026-09-15** — **WI-534 is live.** PR #133 into `develop`, release PR #134 into `main`, deploy
  succeeded first time on the merge commit. **The release PR's two same-commit `build-test` runs
  split again**: one passed, the other hit the `A11ySmokeTests` Kestrel flake
  (`TheReaderChoiceGateOpensAndClosesWithJavaScriptDisabled`, 1955 passed / 1 failed), and a
  re-run of the failed job passed with nothing changed. That is THREE firings today (WI-533's merge
  commit on a lone run, and this one on a paired run) — the `/pm` item wants a start retry or the
  root cause, not only a concurrency group.
  **Smoke check:** `/treatments/shunts` 200 with the `#warning-signs` anchor, the lead instruction
  and the ambulance sentence; `def-hydrocephalus` absent there and present on `/tests/ct-scan`; no
  unresolved directive or marker. `/tumors/glioma` composes the escalation block's shunt rule with
  its `#warning-signs` link, and the [MECHANISM] door and pressure clause.
  `/treatments/craniotomy` carries the shunt line; `/tests/mri` carries the door. Six other pages
  200.

- **2026-09-15** — **WI-534 code-complete — `/treatments/shunts`, and the item that created an
  escalation tier.** Grade **4.8**, **1958 tests** (1929 before), ContentCheck **262/0**,
  **62 break-mutations green on LF and CRLF** — the first run found ONE guard that could not
  fail (the escalation rule's "was evaluated" count, satisfied by the block's own
  `#warning-signs` link), rewritten and re-proved — plus four Kestrel render guards proved by
  rebuild. No new glossary entry
  (`hydrocephalus` suppressed here, kept for `/tests/ct-scan`).
  **THE SAFETY DECISION:** for a reader WITH a shunt, NINDS says "seek medical help
  immediately" for symptoms the shared [ESCALATION] block files as same-day. The page owns the
  shunt tier; `blocks/escalation.md` gains a third CONDITIONAL (everything on its same-day list is
  a right-away call for a shunt reader) routing to `#warning-signs`; `/treatments/craniotomy`'s
  caregiver lists and `blocks/mechanism.md`'s raised-pressure line carry the same instruction;
  `EscalationBlockTests` now reads COMPOSED hubs with a positive count. Doors on
  `blocks/mechanism.md` (8 hubs) and `/tests/mri`. Meningioma's front-matter escalation ruling
  updated.
  **SOURCES:** the dossier's patient source was healthline.com and its "27.8% shunt failure" is
  not in PMC8976775 (33%); neither printed. Re-sourced to NINDS, the Hydrocephalus Association
  (three pages), AANS, ACS, MedlinePlus's encyclopedia, StatPearls, FDA (magnets: two inches, the
  other ear), the Walton Centre (UK, attributed), NHS GGC MRI Physics (staff guidance) and CDC.
  Two disagreements printed: operation time (ACS an hour / MedlinePlus an hour and a half) and
  permanence (ACS temporary or permanent / Hydrocephalus Association for life).
  **`/review` round 1: 4 blockers, 13 should-fix, 9 nits** (saved at
  `.claude/work_files/wi534-sources/review-1.md`), all answered — the blockers were a pointer to an
  ambulance list on `/get-help-now`, which has none; a warning list missing hard-to-wake,
  confusion, blurred vision and balance; a recovery section calling three warning signs normal;
  and an escalation-test rule over raw hub files that could never run. Rendered read ran twice.
  **For `/pm`, not blocking:** the `/tumors/ependymoma` stub says its tumor "can block the flow of
  fluid" and includes neither [ESCALATION] nor [MECHANISM], so it has no shunt door and no shunt
  rule — its hub item must add both together. `feeling sick` corpus sweep is still open (WI-533).

- **2026-09-15** — **WI-533 is live.** PR #131 into `develop`, release PR #132 into `main`.
  `build-test` was green on the feature PR and on both develop runs, and **FAILED ON THE MERGE
  COMMIT TO `main`**: the `A11ySmokeTests` Kestrel flake
  (`TheReaderChoiceGateOpensAndClosesWithJavaScriptDisabled`, "The Kestrel test host did not
  start"), one test failing and 1926 passing. The deploy job was skipped, so the live site was
  briefly behind `main`. `gh run rerun --failed` passed with nothing changed, and the deploy
  succeeded.
  **NEW EVIDENCE FOR `/pm`: this time there was ONLY ONE run on that commit.** WI-531 and
  WI-532 blamed two runs racing on the same commit (a push and a pull_request). This merge
  commit had a single push run and still failed, so a concurrency group alone will not fix it.
  The Kestrel host needs a start retry, or the root cause, which is still unknown.
  **Smoke check:** `/treatments/tumor-treating-fields` returns 200. The shared "at least
  eighteen hours a day", the EANO sentence and the temozolomide condition are all on the page.
  `def-tumor-treating-fields` is absent and `def-temozolomide` is present. No unresolved
  directive or authoring marker. Both hubs return 200 and carry the link, and their own
  tooltip still fires. Nine other pages return 200.

- **2026-09-15** — **WI-533 code-complete — `/treatments/tumor-treating-fields`, and the item
  whose clinical half was already shipped twice.**
  Grade **4.4**, **1929 tests** (1895 before), ContentCheck **261/0**, **103 break-mutations
  green on LF and CRLF** (every `/review` counter-example among them; a first run was
  misdirected at WI-532's table and killed — §12.8 has it), plus the
  three Kestrel render guards proved by script. No new glossary entry: the existing
  `tumor treating fields` entry (alias Optune) is suppressed on this page and kept for the two
  hubs. Doors on `/tumors/glioblastoma` (in the device paragraph) and
  `/tumors/high-grade-glioma` (in the EANO paragraph).
  **THE §12.10 CALL WAS MADE BEFORE DRAFTING: SHARED WORDING.** The page uses the glioblastoma
  hub's "at least eighteen hours a day" and no other strength, and the high-grade-glioma hub's
  EANO sentence verbatim (allowlisted); both pinned by tests that READ the hubs. Who it suits
  and when it starts stay on the hub. **"Four to seven weeks" was read at its source** — verbatim
  in PMC12467656, "initiated 4–7 weeks post-radiotherapy" — and not restated.
  **THE DOSSIER'S MAIN SOURCE IS A ZERO-BYTE PAGE THAT ANSWERS 200** (`virtualtrials.org`,
  Sucuri), and its other is `optunegio.com`, the manufacturer. Re-sourced to ACS (two pages),
  CADTH's HTA, EANO, PMC12467656, the Brain Tumour Charity, a skin-care review and two
  patient-experience studies (Chicago, Novocure-funded; Germany, no conflict), each cited only
  for what it can carry. The manufacturer's contraindication list is not published.
  **`/review` returned 2 blockers, 24 should-fixes and 10 nits.** Blockers: "From then on it
  runs day and night" (a 24-hour second strength past a guard that banned every NUMBER), and
  temozolomide addressed to the recurrence reader, who is not on it. Should-fixes included a
  survivorship artifact ("most still kept wearing it" from a study that enrolled only people
  two months in), 55% printed as "most", "some doctors" flattened to "the doctors", a hat
  offered as concealment, and the page restating itself seven times. Two findings rejected
  with the verbatim in the front matter (a CADTH quote not in CADTH; "guidelines" plural is
  supported by three named). The guards were rewritten against every counter-example and each
  is a `review-` mutation.
  **The rendered read (twenty-second item running) ran twice.** Round one found ten, including
  the alarms ranked "one of the most disturbing parts" when the source only rated them
  disturbing. Round two found "The cap and the bag are always there" — a third idiom for
  twenty-four hours — and a self-restatement created by a `/review` fix.
  **For `/pm`, not blocking:** `feeling sick` (British for nauseous) is on
  `/treatments/chemotherapy` and seven other files, and is not on `CuratedPage.BritishForms`;
  add `virtualtrials.org` to the known-dead list; both door hubs sit at 5.8 and 5.7.

- **2026-09-14** — **WI-532 is live.** PR #129 into `develop`, release PR #130 into `main`,
  **build-test green first time on both**, deploy succeeded (5m25s).
  `/treatments/targeted-therapy` returns 200 on brainharbor.org with no authoring marker and
  no unresolved block directive; the spine sentence and all four rendered-read fixes are on
  the live page and the site-voice meta is gone. **All six doors point here from their
  siblings, and `/treatments/watch-and-wait` correctly has ZERO** — the seventh door stays
  refused in production. Both deep-link anchors resolve (`#what-worked-means` here,
  `#fever-rule` on the chemotherapy page). Fourteen other pages smoke-checked at 200.
  **The release PR fired TWO `build-test` runs on the identical SHA again** — the
  `A11ySmokeTests` race named in the WI-531 entry — and this time **both passed** (3m42s and
  3m33s). The trigger is confirmed as real and still unfixed; it simply did not bite. Still
  for `/pm`: a workflow concurrency group.

- **2026-09-14** — **WI-532 code-complete — `/treatments/targeted-therapy`, and the item
  where a guard proved only that a file existed.**
  Grade **5.4**, **1895 tests** (1860 before), ContentCheck **260/0**, **92 break-mutations
  green on LF and CRLF**, plus the three Kestrel render guards proved by a script that
  mutates, rebuilds, confirms red and restores. No new glossary entries (all seven candidates
  argued and dropped), six doors, and a seventh REFUSED by a sibling's own end-anchored pin.
  **§12.2 ITEM 6 BANS `avastin.com` BY NAME AND THE DOSSIER SOURCES THE ENTIRE BEVACIZUMAB
  SECTION TO IT** — what the drug is, what it is for, the side effects and the surgery
  interval. Replaced wholesale by ACS's brain-specific page, and **the manufacturer's "at
  least 28 days before or after surgery" is not published**: ACS says "within a few weeks"
  and the page says outright that there is no one number that fits everybody.
  **The dossier covers three drug families and ACS covers six** — a page built from the
  dossier alone would have told a reader holding an H3 K27M, NTRK or mTOR result that there
  is nothing for them. **The vorasidenib trial EXCLUDED anyone who had had chemotherapy or
  radiation**, verbatim on the FDA page and absent from the dossier, which matters because
  four other pages carry vorasidenib as an option. Dordaviprone's approval is ACCELERATED
  with the ACTION trial unreported. `ivosidenib` is not named at all — both its citations are
  on the dead `academic.oup.com`, and naming a drug the page cannot describe is how a reader
  asks for the wrong one.
  **The backlog's headline claim held AND was not the whole spine.** Bevacizumab is the
  named anti-hype case, but four drugs here were judged on four different measures, so the
  central section is *what would it mean to say this worked?*.
  **THE BREAK HARNESS FOUND THREE GUARDS THAT COULD NOT FAIL**, identically on LF and CRLF,
  *after* `/review`'s six blockers, sixteen should-fixes and all 71 walk-throughs had been
  answered. **Two were real.** `Assert.Matches(@"(?i)\bWhat is measured\b", sibling)` — the
  guard proving `/tests/molecular-markers` still owns the marker definitions — stays green
  when that page loses **all fifteen** of its `**What is measured.**` entries, because it
  also says *"Each one below says what is measured"* in ordinary lowercase prose. **A
  case-insensitive anchor on a phrase the sibling also uses as prose is not an anchor; it
  proves the file exists.** And a bare `Assert.Contains("/tumors/low-grade-glioma", sibling)`
  passed after the route was deleted from the paragraph that refused the door, because that
  page's "Where to go next" index carries the slug four screens later — presence-not-position,
  third recurrence. **The third was the harness's own input**: the corpus-restatement break
  planted *"It is useful for what it is useful for."*, verbatim from `/tumors/glioblastoma`,
  and `Shingles()` skips any eight-word window with fewer than three CONTENT words. A
  mutation made of function words is not a mutation.
  **Finding the first one took four probes because every count run while chasing it was
  case-SENSITIVE while the assertion was `(?i)`** — so `RepoRoot()`, the build output, a
  stale assembly and the heredoc backspace bug were all suspected and all innocent. What
  broke the deadlock was replacing the whole sibling with a four-line stub, which failed the
  assertion and proved the read was live.
  **THEN `/review` BEAT BOTH REPLACEMENTS WITH WORKED COUNTER-EXAMPLES, which is the item's
  sharpest lesson: a guard rewritten because the harness beat it is a NEW guard that has had
  no harness run against it.** The door guard's replacement measured 400 characters from the
  pin, and failed BOTH ways — `IndexOf` takes the first occurrence page-wide, so an unrelated
  link to the same hub elsewhere on that page turned it RED on a correct edit, while `Math.Abs`
  accepts a route AFTER the pin, so moving the route into the next paragraph (121 characters)
  stayed GREEN. It slices the paragraph now, and the door ban is scoped to that paragraph so a
  later "Where to go next" entry there is not failed against a reason that does not apply.
  The markers guard's replacement counted the convention with a floor of `>= 10`, and
  **deleting the only two entries this page routes readers to — IDH and BRAF — drops fifteen
  to thirteen**, so the floor stayed green through exactly the change its message claimed to
  catch; it asserts the two anchors by name now. Four mutations encoding those
  counter-examples are in the table, which is why the count is 92 rather than 88.
  `/review` also caught that "Your blood pressure is checked" was still an inference — ACS
  states a monitoring instruction for the SKIN and a report-this instruction for the LIVER and
  says nothing about blood pressure, while the drug raising it IS sourced — so it is hedged to
  "usually checked" with the inference recorded in the front matter.
  **The end-to-end read of the RENDERED page (twenty-first item running) found seven**, the
  worst a sourcing defect: **a ranking deleted in one section had survived in another, in
  different words.** The front matter records killing *"high blood pressure is the common
  one"* as an avastin.com-only ranking, and 130 lines earlier the step-by-step still said
  *"because high blood pressure is the side effect this drug is most watched for"*. Also
  **site-voice meta one item after WI-531 removed the identical construction** (*"so the site
  says one thing about them"*, plus two more of the family in the same section); **a page
  restating ITSELF**, which `AssertDoesNotRestateTheCorpus` skips by design, twice; **`out of
  hours`**, British and not on the shared list, which would have made this the eighth page
  carrying it; and a pronoun correct about pills that reads as the patient in a caregiver
  section. **Recorded as NOT a defect so the next read does not re-flag it:** glossary
  popovers are hidden until activated, so a definition appearing mid-sentence in flattened
  text does not interrupt the rendered sentence. Seventeen lessons in **§12.8**.
  The `A11ySmokeTests` Kestrel-flake trigger and the `out of hours` spread are still for
  `/pm`, not blocking.

- **2026-09-13** — **WI-531 is live.** PR #127 into `develop`, release PR #128 into `main`,
  deploy succeeded. `/treatments/proton-therapy` and `/treatments/stereotactic-radiosurgery`
  return 200 on brainharbor.org with no authoring marker and no unresolved block directive;
  the `stereotactic radiosurgery` tooltip is correctly suppressed on its own page while
  `radiation necrosis` fires in the section that routes it. Ten other pages smoke-checked at
  200.
  **CI WAS NOT GREEN FIRST TIME, AND THE FAILURE WAS REAL.** The first `build-test` failed on
  eight render tests with *"Connection string 'BrainHarbor' not found"* — both new render
  classes took a raw `WebApplicationFactory<Program>` and never pushed the test connection
  string in. **That passes on any machine with `dotnet user-secrets` set and fails only on a
  runner that has none**, so the full suite, ContentCheck and a 125-mutation break harness all
  called the item finished. Fixed in both, and guarded corpus-wide by a SOURCE scan (the
  wrapping lives in a constructor body, which reflection cannot see). §12.8 has it as a new
  class.
  **The release run then hit the `A11ySmokeTests` Kestrel flake** (WI-403, "not proven fixed").
  Two CI runs fired on the identical SHA — a `push` to develop and the `pull_request` — and the
  push one failed while the PR one passed. A re-run of the failed job passed with nothing
  changed. **This is the first evidence naming the trigger: two whole CI RUNS racing, which is
  `TestCollectionHygieneTests`' contention story one level up.** Logged for `/pm`.

- **2026-09-13** — **WI-531 code-complete — `/treatments/proton-therapy` and
  `/treatments/stereotactic-radiosurgery`, two NEW pages in one item, and the item where a
  PREFERRED SOURCE'S OWN "LIMITATIONS" HEADING WAS THE SPINE.**
  Grades **5.4** and **5.0**, **1860 tests** (1775 before), ContentCheck **259/0**,
  **125 break-mutations green on LF and CRLF**. No new glossary entries (both candidates
  argued and dropped), doors on six sibling pages.
  **THE DOSSIER'S PROTON FRAMING IS THINNER THAN ACS'S OWN, AND THAT IS THE HEADLINE.**
  `treatment-library.md` §6 frames proton around pediatric and slow-growing tumors. ACS's own
  proton page has a heading called **"What are the limitations of proton therapy?"** whose last
  line is *"More research is needed to know if it's better than traditional radiation therapy"*,
  and its brain page says **it is not clear whether protons are as useful for tumors that grow
  into normal brain, naming the astrocytoma and the glioblastoma** — which is most of this
  site's readers. EANO says the guideline version. Two preferred sources, and the dossier has
  neither. **Read the source's caveats section before you read the dossier's framing.**
  **THE CENTRE COUNT IS NOT PUBLISHED.** "Roughly 45" is a sentence in PMC13521102's
  *introduction* carrying somebody else's citation — the bundled-claim shape §12.8 (WI-512)
  records three times, one layer up. A first draft printed it, called the paper "a recent
  review" when it is a single-centre retrospective analysis, and rounded a 250-mile comparison
  cut-off into an invented *"more than two hundred miles"*. ACS's own "a limited number, more
  being built" is printed instead, and the page routes the reader to ask where their nearest one
  is. What IS first-party in that paper — the travel comparison — is what the access section
  rests on.
  **The dossier's whole access section is a centre's blog, a law firm's blog and a content
  farm.** Replaced by **PMC11905844** and **PMC11699354**, which carry it at research level: the
  outside review is a legal right, its decision is **binding on the plan**, it **costs the
  patient nothing**, and what wins one is guidelines, studies, trial eligibility and a
  personalised letter. **`NBK66023` is NCI patient PDQ and §12.1 forbids it for
  brain-metastasis radiation**, so the "which tumors" section rests on ASCO-SNO-ASTRO 2022
  (**PMC8917399**) — Rec 3.2 and Rec 3.3, with all three of the guideline's scope words kept.
  `/review`: **six blockers, twenty-seven should-fixes, and SIXTY-SEVEN OF SIXTY-NINE ATTACK
  SENTENCES WALKING THROUGH THE GUARDS**, each with the exact sentence that beat it. The worst
  blocker is a safety one: the SRS page's single invented tier, an infected pin site, shipped
  with **no timing word at all**, two sections above the block, while `/treatments/craniotomy`
  files a warm wound, a leaking wound and a fever as **same day**. A page inventing a tier
  LOWER than the corpus files the identical symptoms, and the guard could not see it because a
  downgrade check has nothing to downgrade when nothing was escalated. Two more: an invented
  etymology for the word "surgery" on the page whose whole job is correcting that word, and an
  invented "a few usually means somewhere between two and five".
  **ONE REVIEW FINDING WAS WRONG AND IS RECORDED AS SUCH:** PMC13521102 was called a
  multi-institution analysis; its Methods say "a single facility" and its Discussion says "a
  large, urban, academic center", so the page's framing was right. The same finding's other
  half — the invented mileage — was right and is fixed.
  **THEN THE HARNESS FOUND EIGHT GUARDS THAT COULD NOT FAIL**, identically on LF and CRLF,
  *after* all sixty-seven walk-throughs had been answered. Five real: **`the operation` had been
  dropped from the determiner list** when that guard was narrowed, so "go home the same day as
  the operation" walked the page's central safety property; the frame-or-mask choice was
  asserted by PRESENCE while the page says the phrase four times; the second-share check was
  section-scoped; **a `!%term%` marker inside a SHARED BLOCK suppresses that tooltip on every
  page that composes it** and the test only read each page's own text (WI-510's recorded leak,
  unseen until now); and a reachability floor of `>= 3` read a drop from four to three as
  healthy. Two mutations were too weak, and **one still named a test this item had renamed** —
  which is why the harness treats "filter matched NO test" as a failure.
  **The end-to-end read of the RENDERED pages (twentieth item running) found six**, including
  **a navigation instruction pointing at the wrong section because the composed `[ESCALATION]`
  block brings its own heading and moves what the pointer points at**; the same claim in two
  consecutive paragraphs, created by the `/review` fix that added it; "Where to go next"
  re-committing a contradiction the body had just been corrected for; and two pieces of
  site-voice meta (*"because no source gives a set answer"*).
  **A new British family: everyday NOUNS.** `car park` reached a draft. Every previous sweep
  looked at spellings and idiom. `petrol`, `motorway` and `dual carriageway` added with it —
  and **`straight away` was proposed by `/review` and REJECTED**, because it is live on
  `/treatments/chemotherapy` in four places including the fever rule, as was `chemist`, which
  is a substring of `chemistry`. Fifteen lessons in **§12.8**, one of which is a new class: a test that passes on every developer machine BECAUSE the machine is configured. Both render classes booted the app with no connection string -- green here, red on the CI runner, and invisible to the suite, ContentCheck and the harness alike. There is a corpus guard now, and it is a SOURCE scan because the wrapping lives in a constructor body that reflection cannot see.

- **2026-09-13** — **WI-530 is live.** PR #125 into `develop`, release PR #126 into `main`,
  build-test green first time on both, deploy succeeded. `/tests/ct-scan` and
  `/tests/planning-scans` return 200 on brainharbor.org with no authoring marker and no
  unresolved block directive; the `radiologist` tooltip renders in the section that owns it.
  Ten other pages smoke-checked at 200.

- **2026-09-13** — **WI-530 done — `/tests/ct-scan` and `/tests/planning-scans`, two NEW pages in
  one item, and the item where a cited page had been rewritten out from under its citations.**
  Grades **4.5** and **4.7**, **1775 tests** (1724 before), ContentCheck **257/0**,
  **154 break-mutations green on LF and CRLF**. One new glossary entry (`functional MRI`),
  doors on six sibling pages.
  **THE HEADLINE FINDING IS A NEW CLASS OF CITATION DEFECT.** The dossier's §2.2, §2.3 and §2.4
  attribute **eight** claims to `radiologyinfo.org/en/info/headct` — the warmth and flushing, the
  metallic taste, the urge to urinate, "within 30 minutes", the kidney screening. Fetched in full
  with a browser user agent (51,913 bytes, "Last reviewed on June 15, 2026"), **that page has no
  "What will I experience" section at all.** Not gated, not 403, not a JavaScript shell: a live
  page restructured since the research was gathered. Every one of the eight is carried verbatim by
  ACS's own CT page and was moved there. Two other §2 defects: the ABTA quotation is not on the
  ABTA page (ABTA dropped entirely; ACS carries the comparison), and the ~4 mSv dose is omitted
  per §12.4 R2 with the DIRECTION published instead.
  **On the planning page, §3.1's "direct cortical stimulation remains the reference standard" is
  not in the paper it is cited to** — PMC4757221 says close to the opposite about fMRI's standing —
  so it is banned as a CLAIM, not only as a citation. `researchgate.net` and `journals.lww.com`
  were both recovered open (PMC5669348 for neurovascular uncoupling, PMC8050646 for the DTI tract
  patterns). 2-HG, SPECT and DOTATATE dropped: no reachable patient-level source.
  **THE SCOPE LINE IS A TIME, NOT A TOPIC.** All five planning scans have an after-treatment use,
  and `/tests/follow-up-scans` (WI-521) already ships that half from two sources that disagree. So
  the page is scoped to before-treatment and routes — seven banned branches, each with its own
  canary.
  `/review`: **three blockers, twenty-six should-fixes, and twenty-two executed guard
  walk-throughs of which TWELVE were beaten.** The worst blocker is a new shape:
  **ACS says, verbatim, "Tumor usually shows up on a PET scan, while scar tissue does not" — and
  printing it was the defect**, because the sibling that owns the question refuses the binary.
  §12.10 is about strengths, not sourcing, and this is the first time the stronger version was the
  one with the citation. The second: **a door appended to `/treatments/craniotomy` restated a claim
  one notch stronger than the same page states it 115 lines later**, undoing a fix WI-523 recorded.
  The third: the guard written to keep the after-treatment material off the page **matched nothing**,
  so its redaction list was dead code documenting a decision it had never made.
  **The end-to-end read (eighteenth item running) found twenty**, including an unsourced
  causation absolution in a kind voice (*"Whatever you are carrying today, it is not that"*), one
  claim said twice fifteen lines apart, and **`grey` — a whole colour family no previous corpus
  sweep had asked about**, now on the shared British-forms list. **The one nothing else could
  see is about WHERE A TOOLTIP LANDS:** a glossary term fires on its FIRST occurrence, and a draft
  introduced `radiologist` inside the PET section's breastfeeding advice — so the rendered page
  defined a radiologist in the middle of instructions about pumping milk, and slot 9, the section
  that exists to say who reads your scan, rendered with no tooltip at all. The markdown contains
  neither the tooltip nor its position. There is a test now, and its first version failed a
  correct page because it used a bare `IndexOf` while `GlossaryMarker` matches whole words.
  **The harness then caught a stale test name in its own mutation table** — which is exactly why
  it treats "filter matched NO test" as a failure rather than a pass.
  Two helpers promoted to `CuratedPage` the day they were written, because the item's second page
  needed them: the corpus-wide restatement check and the escalation-shape check. Seventeen lessons
  in **§12.8**.

- **2026-09-13** — **WI-529 is live.** PR #123 into `develop`, release PR #124 into `main`,
  both CI jobs green first time, deploy succeeded. `/tumors/all-brain-tumors` returns 200 on
  brainharbor.org with no directive or authoring marker leaked and the outlook gate closed;
  nine other pages smoke-checked at 200.

- **2026-09-13** — **WI-529 done — `/tumors/all-brain-tumors`, a NEW page rather than a
  deepening, and the only page in the corpus written for a reader with no diagnosis at all.**
  Grade **5.1**, **1724 tests** (1691 before), ContentCheck **253/0**, **147 break-mutations
  green on LF and CRLF**. Twelve source rulings.
  **THE BIGGEST CALL WAS A SCOPE ONE, AND IT WENT AGAINST THE RESEARCH PACK.** The backlog
  hands this item the seven-step pathway, and §C.4's turnaround-time table carries the
  sentence *"this is, editorially, the most valuable section in the whole brief"*.
  **`/tests/waiting-for-results` shipped all of it twenty-two items ago** — the lab queue,
  the durations, the UK national audit, the batching, the name changing — and `/tests/biopsy`
  owns the two operations and the "without one" exception. What is genuinely unowned is
  **the part before there is tissue**, which is exactly where this reader is standing. So the
  page owns **steps 1 to 4** and routes 5 to 7, and a test reads both siblings and goes red if
  their prose reappears here.
  **THE CENTRAL SAFETY PROPERTY IS THAT THE PAGE MAY NEVER SAY WHAT THE THING IS**, in its
  own voice, anywhere — which every other `/tumors/` page is allowed to do. Two shared blocks
  say "your tumor" six times between them, so the page carries one explicit scoping note
  above the first of them, and the blocks are not edited (§12.10, the WI-514 blast radius).
  **`[CROSSWALK]` IS EXCLUDED FOR A THIRD REASON, WHICH IS NEITHER "UNNECESSARY" NOR
  "FALSE": it is addressed to somebody else.** It opens *"If your paperwork was written
  before that"* and closes *"Seeing an older name on your own report"*, and this reader is
  defined by not having a report. **`[CAUSES]` IS INCLUDED — the opposite call from WI-528
  one item earlier**, because its opening is true here and asserts nothing about this
  reader's scan.
  **Two of the research pack's claims are not in the paper it cites them to.** §C.8's
  "major disagreements occurred in 12% of cases" and "novel molecular tests contributed in
  55% of major-disagreement cases" are attributed to PMID 25972322 — which is *"Consultative
  issues in surgical neuropathology: a retrospective review of the RATIONALE FOR SUBMITTING
  cases"*, counts why 508 cases were referred, and reports **no disagreement rate at all**.
  No figure published. And §C.1's "consensus within two working days" is sourced only to
  academic.oup.com, which is dead — so step 3 is hedged rather than described, because the
  claim is UK-shaped as well as unsourced.
  **Five of the pack's claims are sourced to NCI patient PDQ**, which §12.1 bans on a tumor
  hub. All five are carried at patient level by the American Cancer Society instead, verified
  by fetching rather than assumed. `appliedradiology.com` (403) carries the whole
  ring-enhancing differential and was replaced; **`link.springer.com` is NOT dead** and was
  re-fetched at 627 KB with the claim present, which is the second item to reach that answer.
  `/review`: **three blockers, fifteen should-fixes.** The blockers were `[MECHANISM]`'s FIRST
  SENTENCE being false here (*"most people are told what they have long before anyone
  explains what it is doing"*, three lines under *"Nobody knows yet whether that is what you
  have"*) and the scoping note not reaching it; an unsourced *"and usually are"* appended to
  the reassurance about the reader's own symptoms; and a definition of *grade* that
  contradicted the page it links to one click later.
  **A CORPUS SWEEP FOR DOUBLED-CONSONANT BRITISH FORMS FOUND THREE LIVE SPELLINGS ON FOUR
  SHIPPED FILES** — `travelled` ×3 on `/tumors/brain-metastases`, `judgement` on two pages,
  and `labelled` on `/tests/waiting-for-results` and in a **glossary entry, whose tooltip
  fires site-wide**. Every previous sweep looked at `-ise`, `-our` and idiom; nobody had asked
  about this family. All fixed at source and added to the shared guard.
  **The harness found three guards it could not exercise, and two were real.** A canary aimed
  at a front-matter COMMENT rather than the sentence; a mutation placed below a composed block,
  which cannot move what the block's heading points at; and **a Razor view, which the harness
  structurally cannot mutate** because views compile into the test assembly and it runs
  `--no-build` — proven by hand instead and the absence recorded in the table.
  **The end-to-end read (seventeenth item running) found three**, all duplication a shingle
  check cannot see: the caregiver slice repeating the block's own *"not going behind their
  back"*, the `tumor board` tooltip printing the block's opening sentence immediately above
  the block's opening sentence, and "there is no screening test" said twice. **And fixing the
  second one shipped a fail-open for one test run**: `!%tumor board%[TUMOR-BOARD]` stops the
  directive composing, because the composer matches it only on a line of its own, and the
  literal rendered inside a `<p>`. There is now a mutation for it.
  Thirteen lessons in **§12.8**.

- **2026-09-12** — **WI-528 done — `/tumors/brain-metastases`.** The 260-word stub became a
  full §12.3 seventeen-section hub. Grade **5.7**, **1691 tests**, ContentCheck **252/0**,
  **123 break-mutations green on LF and CRLF**. Ten source rulings, and **two of them are
  against the backlog item itself**. **(1) THE ITEM'S HEADLINE NUMBER IS WRONG.** It claims
  "33-66% of brain metastases are the first sign of cancer"; the largest series that asks the
  question gives **19.0%** (PMC6267666, n = 2419), and the nearby 25% and ~15% answer different
  questions. No figure published; the page carries the shape and the reassurance inside it.
  **(2) "YOUR ORIGINAL ONCOLOGIST STILL LEADS" IS AN OVER-CLAIM** — the source names a
  different team and no leader at all, so the page says what is supported and gives the action
  (ask who is coordinating, because it is now more than one team).
  **AND TWO SHARED BLOCKS ARE WRONG ON THIS HUB RATHER THAN MERELY UNNECESSARY.** `[CAUSES]`
  opens "For most brain tumors, nobody knows the cause", which is false when the cause is the
  cancer in the reader's chart; `[CROSSWALK]` explains naming rules this reader's report does
  not follow. Both excluded, **neither block edited**, and the page writes its own self-blame
  section — including the inherited-risk question the block was carrying, which has a different
  answer here.
  `/review`: **three blockers, sixteen should-fixes, eighteen proven guard walk-throughs** with
  every regex executed rather than eyeballed. The sharpest blocker was a real safety defect:
  new leg weakness plus new bladder trouble was filed as a thing to raise at the next
  appointment, when in somebody with metastatic cancer that pair is cord compression and the
  window is hours. It now carries the corpus's own sentence, word for word, as a scoped line
  above `[ESCALATION]` — three pages, one wording.
  **Then the harness found four more guards that could not fail**, after all eighteen
  walk-throughs were answered. Twelve lessons in **§12.8**, including the one that explains why
  WI-527's CRLF bug survived its own harness: the harness asks only whether a mutated page
  FAILS the named test, and a test broken on CRLF fails for free.
  **The end-to-end read (sixteenth item running) found five**, two of which nothing else could
  see: a glossary tooltip asserting the neuro-oncologist "leads the team" three lines from the
  section that refuses to say who leads, and a second tooltip putting whole-brain radiation at
  "more than one tumor" while the page said "too many to aim at one at a time". Both glossary
  entries corrected to be true everywhere.

- **2026-09-12** — **WI-527 done — `/tumors/meningioma`.** The 198-word stub became a full
  §12.3 seventeen-section hub. Grade **5.6**, **1663 tests**, ContentCheck **252/0**, **98
  break-mutations green on LF and CRLF**, `[MECHANISM]` `[ESCALATION]` `[CROSSWALK]` `[CAUSES]`
  `[CAREGIVER]` all composed rather than restated. Eleven source rulings, three serious.
  **The item's best-looking reassurance is dead at source:** the dossier's three "features that
  predict slower growth" (older age, calcification, hypointense T2) are cited to PMC10180371,
  which says the opposite or nothing — under an editorial note arguing for them *because they
  are reassuring*, which is §12.13's tell. None published. **The contraceptive sentence is
  dangerous simplified:** CUH says pills *"other than those containing the drug CPA"*, and CPA
  IS in some combined pills — so the page carries the exception and tells the reader to take the
  box to her team. **The CNS5 grading backbone had no reachable source** (ajnr.org 403,
  academic.oup.com dead) and `[CROSSWALK]` already carried both the claim and its citation, so
  the page defers to the block. The eleven-year surveillance endpoint was recovered open at
  PMC12137216. `/review`: four blockers, all fixed.
  **THEN THE HARNESS FOUND THIRTY-FIVE GUARDS THAT COULD NOT FAIL** — identically on LF and
  CRLF, *after* `/review` had returned sixty-six walk-throughs and been answered. A substring
  allowlist exempting whole sentences, a fraction list that knew no fifths, adjacency-only
  quantifiers, a dose guard blind to "grays", `\bgrade\b` blind to "graded", a section-scoped
  endpoint guard blind to the short version, a digit-only ban inside the `:::outlook` gate, two
  unnamed block directives, and an escalation ban list made of clinical nouns that missed
  "numbness in the saddle area". All eleven lessons are in **§12.8**.
  **The end-to-end read (fifteenth item running) found two more:** an ambiguous pronoun that
  told the reader their tumor shrinks, and an unsourced majority claim that survived by being
  allowlisted for the guard that would have caught it. Both fixed at source.

- **2026-09-12** — **WI-526 done — `/treatments/clinical-trials`.** Grade 4.9, 28 tests,
  and the first page that had to share a subject with a Razor page: `/trials` does the finding,
  this page does *should I be thinking about this at all*. The spine is **a trial is not a last
  resort** — five windows, three of them before recurrence — with the honesty halves that phase 1
  is not designed to test whether it works and that neither you nor your doctor picks your group.
  Four dossier defects, including a framing rule whose own citation did not contain it.
  Nine lessons in §12.8, and a door added above the search on `/trials`.

- **2026-09-12** — **WI-525 done — `/treatments/anti-seizure-medicines`.** Grade 5.2, 33 tests,
  16 front-matter rulings. The spine is **levetiracetam mood change may be the drug** — the
  timing tell, and that it reverses — plus *"why won't they give me one?"* (Level A against
  prophylaxis). **The backlog's headline claim verified, first time in four items** — and the
  same guideline holds a scope trap: perioperative prophylaxis is Level C, so printing only the
  Level A tells craniotomy patients their team broke a rule. Three dossier defects, including an
  interaction claim cited to a paper mentioning none of the drugs.

- **2026-09-11** — **WI-524 done — `/treatments/steroids`.** Grade 4.7, 1572 tests, ContentCheck 250/0,
  72 break-mutations on LF and CRLF, ten inbound doors, no new glossary terms. **The backlog's own
  "~28% proximal muscle weakness" is not published and was never in the paper the dossier cites it
  to**; corrected at source in SYNTHESIS, content-pipeline §12.6 and the dossier. Five `/review`
  blockers, fourteen of eighteen mutations walked the first suite, and the end-to-end read found the
  page explaining the taper without saying why stopping suddenly is dangerous. Nine lessons in §12.8.

- **2026-09-10** — **WI-523 done — `/treatments/awake-craniotomy`.** Grade 4.2, 1542 tests, ContentCheck
  249/0, 101 break-mutations on LF and CRLF, `brain mapping` glossary entry, seven doors. Two `/review`
  blockers (a "not missed out" sentence against its own source's conclusion; the spine's "it comes back"
  leaking from the test into tumor removal), a frequency guard that could never fire, and shell-heredoc
  backspaces in a C# regex. Ten lessons in §12.8.

- **2026-09-07** — **WI-515 code-complete — `/tumors/high-grade-glioma`, the
  second grouping page, and the item where `/review` caught §12.10's own defect
  being re-committed.** A 194-word stub became the §12.3 seventeen-section hub.
  Reading grade **5.6** (joint highest with the umbrella; the vocabulary is
  why), **1275 tests** (1233 before), ContentCheck **226/0**, two glossary terms.
  **All 91 breaks proven on an LF copy AND a CRLF copy.**
  **The blocker was the §12.10 defect, on the page least able to afford it.**
  The draft said "these tumors grow into the brain around them ... they are not
  curable" directly under "Yes. A high-grade glioma is cancer" — asserting
  infiltration and incurability of **every** high-grade glioma. CNS5 defines a
  supercategory of **circumscribed astrocytic gliomas**, "gliomas with more
  well-delineated borders separating them from the surrounding brain
  parenchyma", and two of them are grade 3. Both siblings scope the claim;
  the page whose whole thesis is "two tumors here can be very different" did
  not. Written up in **§12.11**.
  **The safety gap was a missing fever rule.** Every reader of this page is
  offered chemotherapy and `/treatments/chemotherapy` opens by calling a fever
  "the one thing to know before you start" — and this page's escalation section
  had no fever line at all. It now has one, linked to `#fever-rule`, with a test
  that reads the sibling so a softening there goes red here.
  **Five more bad dossier citations, which makes twenty-five across nine items.**
  (1) §3.3's raised-pressure comparative ("more commonly seen in high-grade
  gliomas") is **not in the Springer paper** — its only "more commonly seen"
  sentence is about hematologic malignancies. My draft had turned it into "the
  swelling is heavier", an unsourced quantity claim; the paper describes the
  *nature* of the swelling (infiltrative edema "in a zone of infiltrating tumor
  cells") and that is what shipped. Corrected in `glioma-family.md` at source.
  (2) §3.5's "roughly 10-30% will have **transient worsening**" is attributed to
  a paper containing that phrase **zero** times, whose actual figure (12-65%) has
  a different denominator. (3) **NBK559184**, cited twice for general high-grade
  treatment, is the StatPearls **oligodendroglioma** chapter, which §12.1 bans;
  radiation necrosis re-sourced to ACS at patient level. (4) NBTS is *still*
  cited for the "secondary glioblastoma" retirement it does not contain — WI-514
  found this and the dossier kept it; EANO says the rename verbatim. (5) Both
  `academic.oup.com` URLs **re-confirmed 403** today; open replacements found via
  Europe PMC (PMC6655498, PMC13186461). PMC3643853's "recurrence is usually
  local" was **dropped** rather than published off a 21-patient single-centre
  study.
  **The page's own thesis sentence had no clean source, and the fix was to say
  so.** Nothing fetchable defines "high-grade glioma = grade 3 and 4" except
  StatPearls NBK441874, which writes "WHO grade III and IV" — the notation this
  page forbids. Resolved by **widening the front-matter scope note deliberately
  and visibly**: the range is taken from there with the numerals translated,
  which is exactly what the crosswalk teaches. The notation is the defect, not
  the range. A test fails if the note is deleted while the URL stays.
  **My own tests were weak in six places and the break harness found every
  one.** Two alternations where either branch alone passed (the stub warning,
  the right tail); a first-two-sentences check that let the cancer answer be
  buried in a subordinate clause; a "twelve weeks" assertion that survived
  deleting the rule because the page says the words twice; a crosswalk check
  that passed on a mention without the new name; and `Regex.Matches` with a
  **consuming** window, so the first occurrence swallowed the second and a loop
  over "every occurrence" only ever saw one.
  **Three shared-helper traps, all the same shape — a step that quietly stops
  doing anything.** `CuratedPage.Section` **flattens before it returns**, so a
  splitter fed its output yields ONE chunk and every per-bullet assertion became
  a whole-section assertion that passed with all three chemotherapy regimens
  collapsed onto one tumor. Flattening a YAML comment leaves the next line's
  `#` inside the sentence. And a tooltip was asserted for a term that appeared
  only in a **heading** — the mirror of WI-512's no-op suppressions.
  **`slug:` routes nothing.** ContentStore routes by file path, so
  `ThePageIsServedAtItsExistingUrl` cannot be broken by any in-file mutation.
  Discovered by mutating the slug and watching the test stay green; there is now
  a reachable assertion instead, and the mutation table records why the other
  one is absent.
  **A harness footgun cost four editorial fixes.** The break harness holds the
  originals in memory and restores them in a `finally`, so edits made while it
  runs in the background are silently reverted — and a killed run leaves
  whichever mutation was applied sitting on disk looking like your own prose. It
  now writes a marker file and refuses to start if a previous run did not
  finish. **Never background it and then edit the content.**
  **Also:** the citation discipline held on the page and **failed on a glossary
  entry**, which shipped a hand-composed title for a URL nobody had fetched
  (fetched now, real `<title>` pasted) — the rule is *fetch every URL in
  everything the item ships*; the device paragraph now **names** tumor treating
  fields instead of telling the reader to ask about an unnamed thing, and prints
  EANO's disagreement about it; the "where does it grow" heading now answers its
  own first question before `[MECHANISM]`; four unsourced quantifiers removed;
  and a prognosis guard copied from the low-grade page arrived with a `with`
  exemption that would have let "live many years with this" through here.
  **Print verified**: the outlook heading and the component's warning print, the
  gated body does not, and no glossary popover text leaks into the body.

- **2026-09-07** — **WI-514 code-complete — `/tumors/glioma`, Wave 2's first
  hub, and the first two blocks written for all 24 pages.** A 166-word,
  4-section stub became the §12.3 seventeen-section hub. Reading grade **5.6**
  (the highest of any hub, and the vocabulary is why: a router page for the
  glioma family cannot avoid *oligodendroglioma* and *anaplastic astrocytoma*),
  **1233 tests** (1195 before), ContentCheck **223/0**, two new glossary terms.
  **All 57 breaks proven on an LF copy AND a CRLF copy.**
  **Four more bad dossier citations, which makes twenty-two across eight items.**
  (1) The oligoastrocytoma elimination is attributed to PMC9723092, which
  mentions the word only historically; the claim is carried properly by the CCO
  clinical-practice review. (2) The "secondary glioblastoma was retired" claim
  is attributed to NBTS, whose glioblastoma page contains neither
  "secondary glioblastoma" nor "retired" — **the word "retired" appears zero
  times in all six naming sources**; that framing is the dossier's, not any
  source's. The rename is carried verbatim by CNS5. (3) The quoted phrase
  "molecular glioblastoma" appears in none of the three papers, though the
  substance is verbatim in PMC10216527. (4) **PMC12406498, cited for "steroids
  improve someone within a day or two", is a UK pharmacovigilance audit of
  steroid COMPLICATIONS** and says nothing of the kind. Also: `moffitt.org` is
  Cloudflare-gated and `mdpi.com/2072-6694/14/10/2507` returns a **1-byte**
  JavaScript shell, so the grade-vs-stage and infiltration-route claims were
  re-sourced. A test fails if any of them reappears — **on the blocks as well as
  the page**, because block `sources` render in the reader's source list.
  **`/review` found five blockers, and the two worst were both "this block is
  not actually shared".** `[MECHANISM]` said gliomas "grow through brain tissue
  instead of pushing it aside" and that "they got it all" is not the same as
  cured — **false, and frightening, for a fully resected grade 1 meningioma**,
  on up to 24 hubs. `[CROSSWALK]` was the glioma rename table, and switching
  `/tumors/low-grade-glioma` to it put **glioblastoma and DIPG entries into a
  grade-2 patient's identity section** — a regression dressed as factoring. Both
  blocks are now universal and both pages carry their own slice, which is what
  SYNTHESIS §4.3 said all along. **This is written up as a new §12.10.**
  **Three more blockers worth naming.** The "Is it cancer?" section answered the
  *grade* question first and routed the cancer question to five child pages —
  which are Wave-0 stubs containing the words cancer, malignant, benign and
  curable **zero times in their bodies** — while insisting it "is not a dodge".
  It now answers on the page, with *benign in the brain does not mean harmless*.
  Curability was stated at two strengths: WI-513 wrote "they are not curable" on
  the child page with a paragraph on why the blunt version had to stay, and the
  umbrella — the page more people land on first — rendered it as "a long-term
  condition". And the scanxiety paragraph put the peak **before** the scan when
  the research says scan-to-**result**, attached an invented "about a week", and
  told the reader patients have a name for it without giving the name.
  **My own tests were wrong three times, each in a way the project has hit
  before.** A ±220-character negation window read "no longer" out of the
  *adjacent crosswalk block*, so a retired name used as a live diagnosis beside
  it passed — now scoped to the list item (WI-511's clause anchor). A test named
  for the block asserted words that had moved onto the page, so emptying the
  block left it green. And asserting the phrase "not curable" passed on a page
  whose only use of the words was the softening line beneath it, so deleting the
  hard sentence stayed green — WI-512's presence-versus-position defect again.
  All three found by the break harness, not by reading.
  **Also:** the escalation test now **reads the sibling pages** instead of
  hard-coding what they are believed to say (and strips markdown emphasis first,
  or `**first ever** seizure` reads as absent); the page had no link test at all
  despite being the router, and now has link, fragment, gate and composition
  checks against the running site; four of six tooltip suppressions were
  no-ops, two naming terms that **do not exist in the glossary** — `adult-type`
  and `pediatric-type`, the page's headline vocabulary, now added; and a
  post-gate paragraph duplicated `ReaderGate.WarningText`, **required by my own
  test**, which would have shipped a spec violation to 22 more hubs.
  **One review finding was wrong and is recorded as such:** `link.springer.com`
  was called bot-gated, but it returns 200 with 83KB and the claim verbatim on
  two independent fetches with the browser header set. Kept.

- **2026-09-07** — **Template signed off and the release PR opened — PR #90,
  `develop` -> `main`, WI-509 through WI-513 in one deploy window.** Dan's
  instruction, twice, in the same session. **Both calls were made sight-unseen.**
  The site was started at `localhost:5177` and `/tumors/low-grade-glioma`,
  `/treatments/radiation-therapy` and `/treatments/chemotherapy` all returned
  200 — but the pages were not read before approval. The standing rule (memory
  `test-locally-before-deploy`, established when Dan's own local read caught the
  "lie awake listening" fragment that every gate passed) was knowingly set
  aside. **Written down as a departure, not a precedent.** The next release gets
  the local read.
  **The PR is open, not merged.** Merging is the deploy and that is still Dan's
  hand on the button. Deploy window has run ~92s to ~2m on the last four
  measurements, with the same signature each time (`/research`, `/trials`,
  `/search`, `/get-help-now` all 500 with empty bodies while `/` stays up) —
  worth watching again on this one.
  **The crosswalk question is ANSWERED — WI-514 owns the canonical block.** Dan
  called it the same day, upholding what WI-513 actually shipped over WI-501's
  contradicting note. **Two layers:** `blocks/crosswalk.md` (which does not yet
  exist — `Content/blocks/` holds only `tumor-board.md`, `caregiver.md`,
  `causes.md`) carries the CNS5-wide renames every hub owes; a page keeps its
  own slice for names specific to its diagnosis, as `/tumors/low-grade-glioma`
  does for oligoastrocytoma in inline prose at lines 73-85. **When WI-514 writes
  the block, reconcile that page** — its Roman-numeral line is the overlap and
  would otherwise be said twice. Recorded on WI-501, WI-513 and WI-514 in the
  backlog, so no session re-argues it.

- **2026-09-05** — **WI-513 done — `/tumors/low-grade-glioma`, and THE TEMPLATE
  IS PROVED. This is the stop.** 202 words and 4 sections became the full §12.3
  seventeen. Reading grade **5.3**, 1195 tests (1163 before), ContentCheck
  219/0. Five glossary terms. **The hub template is now written down at
  content-pipeline.md §12.9** — a new section, deliberately separate from
  §12.8, because §12.8 is the LIBRARY template and seven pages in a row used it.
  **Two firsts.** This is the first page in the corpus ever to use the
  **`:::outlook` reader-choice gate** — WI-503 built it ten items ago and
  nothing had used it, so its five documented fail-open modes had never met a
  real page. Verified in the rendered HTML (heading outside, closed on load, no
  fence leaked, the words not also sitting outside) and **in print**: §12.5 says
  a gate left closed stays closed on paper, and the PDF confirms the heading
  prints and the body does not. It is also the **first tumor hub to link into
  the tests and treatment libraries**, which is what the seven Wave 1 pages were
  built for.
  **Review found two REQUIRED blocks missing from my first draft, and this is
  the item where that matters most.** §12.3's "Deliberate omissions" paragraph
  says the **self-blame block** "still appears; it just is not near the top" —
  the one instruction in that paragraph that reads like an aside, and I read it
  as one. It is now a new shared block, `Content/blocks/causes.md`, included as
  `[CAUSES]` and demoted to just before the outlook gate. And contract item 3's
  **retired-name crosswalk** was absent: the readers of this page are precisely
  the ones holding a pre-2021 report saying "oligoastrocytoma" or "grade II",
  and the page said nothing about either.
  **A test in that draft would have foreclosed the crosswalk on all 23 remaining
  hubs.** It banned "oligoastrocytoma", "anaplastic" and "mixed glioma" as
  substrings, passed, and would have been copied — forbidding the
  highest-value block on the site with a comment saying it enforced CNS5. There
  are two different things a retired name can be doing: used as a live
  diagnosis, or named as retired. Same for Roman numerals, and the site-wide
  Roman guard's allowance had to stop being pinned to one page by slug.
  **Four of six citation TITLES were fabricated.** Every URL was fetched and
  every claim checked against the fetched text — the discipline held on
  content and failed on titles, which I wrote from the dossier's description of
  each paper. Titles render as the visible link text under "Sources", so a
  fabricated title is a fabricated citation on the reader's screen, and no test
  can catch it. PMC10216527 is "From Theory to Practice: Implementing the WHO
  2021 Classification", not what I called it; PMC6587541 is "On high-risk,
  low-grade glioma"; PMC9723092 is "Major Features of the 2021 WHO
  Classification"; PMC7527157 is the RTOG 9802 genomic analysis. All corrected.
  **The right tail was on the wrong side of the gate.** "Some people live many
  years with a grade 2 glioma, working and driving and raising children" sat in
  section 2, where a reader who declined outlook met the most hope-preserving
  sentence on the page anyway. That is exactly what the gate exists to prevent.
  It is inside now, and the gate **teaches** *median* — my first draft's test
  BANNED the word, which would have cemented across 23 copies a gate that
  publishes no figures and explains none of the vocabulary §12.5 asks for.
  **§12.1's per-claim rule bit hardest here, and mostly held.** The page's
  bluntest content — that a grade 2 diffuse glioma is malignant and not curable,
  and that patients are routinely told "this is the good kind" before referral —
  comes from a source that is **pre-CNS5**: it cites the 2007 WHO edition, uses
  Roman numerals, contains "oligoastrocytoma" and mentions IDH **zero** times.
  It is cited for what patients get told and never for naming or grading. Review
  found two places the line slipped (a count of tumor types, and a WHO
  recommendation stated in the present tense) and both are now dated or
  re-sourced. The incurability claim itself checks out against a 2025 FDA
  review, which says it in as many words.
  **Two more unsourced claims found by reading, and one bad dossier citation
  corrected at source.** The radiation-then-PCV sequence and "recurrence is
  expected" rested on nothing in this page's front matter; the first got
  PMC7527157, the second was re-derived from the incurability claim already
  cited. And `glioma-family.md` §2.3 attributes "most of these tumors do not
  cause neurologic deficits at diagnosis" to an article containing **zero**
  occurrences of "deficit" — corrected in the dossier so Wave 2 does not
  inherit it.
  **Two shared test helpers were quietly asserting the wrong template.**
  `AssertLinksResolve` hard-coded §12.8's "where-to-go-next" as the onward
  section, which a §12.3 hub does not have. Parameterised — and as a **separate
  method, not an overload**, because adding a same-prefix `params` overload made
  C# reinterpret every existing caller's first required link as a section id and
  turned seven pages red at once. And nothing checked `#fragment` links at all:
  `AssertLinksResolve`'s regex stops at the `#`, so this page's draft link to
  `/tests/pathology-report#grade` came back a healthy 200 while landing the
  reader at the top of a long page. That anchor is explicit now.
  **All 60 breaks proven on an LF copy AND a CRLF copy.** The harness caught
  five weak assertions of mine this round, including a CDKN2A/B check that
  passed on a link paragraph, a library-door check that passed on the support
  index, and an ordering test that used set membership rather than order — on
  the one page whose entire purpose is to fix the shape.

- **2026-09-04** — **WI-512 done — `/treatments/chemotherapy`, and P5 WAVE 1 IS
  COMPLETE.** Seventh of the 29 library pages, third treatment page. Reading
  grade **5.4**, 1163 tests (1120 before), ContentCheck 209/0. Six glossary
  terms (procarbazine, vincristine, lomustine, carmustine wafer, neutropenia,
  nadir).
  **The fever rule is the page, and it is the one place the site's number
  discipline points the other way.** Every other rule here pushes toward
  omitting a figure; a neutropenic fever needs a number a reader can act on at
  2am. **100.4 °F is published**, sourced to ACS, hedged with "ask your team for
  their number", and paired with the nadir timing so the reader knows which week
  to be careful in.
  **Four of the dossier's sources are unusable, and one is a licensing breach
  rather than a citation error.** `drugs.com` hosts **AHFS monographs**, which
  PLAN.md §5 forbids outright — the dossier cites it for the antibiotic claim.
  **PMC3601076**, cited for temozolomide lymphopenia, is a **mouse study**
  (third rodent citation in six items). The Cancer Care Ontario lomustine
  monograph is behind a WAF and returns 106 bytes. And **NBK66023** is NCI
  patient PDQ, whose which-drug table is headed with **"Anaplastic
  astrocytoma"**, a retired CNS5 name — copying that table would have put a
  retired diagnosis on a treatment page. EANO carries the lomustine timing
  properly; PMC12803824 carries the antibiotic recommendation.
  **The ticket's carmustine-wafer framing was wrong, and reading the paper is
  what found it.** The item cites the 2022 review titled *"Is It Still an
  Option?"* as grounds for a sceptical mention. That paper **answers its own
  title yes**: pooled analysis in favor, with careful patient selection. EANO is
  genuinely more cautious. The page prints the disagreement rather than
  inheriting either framing — the WI-511 somnolence shape, one item later.
  **`/review` found four blockers, and two of them are new classes of defect.**
  (1) The PCV tolerance study is **Irish, not British** — I wrote "in the United
  Kingdom" and every author is at Beaumont Hospital, Dublin. WI-507's own
  attribution rule, failing on the country itself. (2) *"The wafers can make
  later scans harder to read"* is **supported by nothing**: the cited paper
  contains "imaging" once, in a PFS definition, and "MRI" never. It came from a
  dossier bullet that **bundles it with a true claim under one citation** — and
  my test had pinned the invented half. (3) The caregiver escalation **tiers**
  contradicted two sibling pages: chest pain is an *ambulance* on
  `/treatments/craniotomy` and was a phone call here; confusion is *same-day* on
  `/treatments/radiation-therapy` and was *straight away* here. Nobody had
  contradicted a list — three pages had sorted the same symptom into three
  tiers. (4) **Procarbazine's alcohol and food reaction was missing**, flagged
  by the dossier as "a genuine safety item", with the non-AHFS source (CRUK)
  already in my own front matter. It is the one thing on a page about
  capsules-you-take-at-home that a reader can get wrong tonight.
  **Reading the page end to end found the contradiction the tests could not.**
  The body says a fever means calling "straight away, at any hour"; the
  caregiver section filed the same fever under a list headed **"the same day"**.
  Both tests on that section passed throughout, because they asserted the
  urgency words were PRESENT — and presence was never the property. Position
  was. There are now three tiers and a test that pins which list the threshold
  sits in.
  **I lost a full round to MSB3027 and nearly reported a green suite that had
  never compiled my tests.** A dev server held `BrainHarbor.Web.exe`, every
  build failed on the copy step, and my check was `grep -c "error CS"` — which
  matches C# errors only and reported 0. `dotnet test` ran the previous
  assembly and returned green twice before I noticed the test COUNT had not
  moved. The harness now fails loudly on a stale assembly, and §12.8 says grep
  for "error", not "error CS".
  **Slot 5 was dropped and put back.** I argued four drugs given four ways share
  no answer to "what does it feel like?". Review disagreed and was right: a
  CYCLE has one shape, and the material was already on the page scattered across
  two other sections. It is now "What does a cycle feel like?", and its point is
  the one thing a reader cannot infer — **you feel worst when you feel
  finished**, because the nadir lands in the week after the dose.
  **Two of my six tooltip suppressions suppressed nothing**, and both
  `DoesNotContain` assertions passed for the wrong reason: `neutropenia`
  appeared in no page's prose anywhere in the corpus, and `carmustine wafer`
  never matched this page's plural. Both glossary entries were unreachable
  site-wide. Fixed with an `also:` alias and by naming neutropenia in the prose,
  where a reader arriving with the word on a printout will meet it.
  **All 75 breaks proven on an LF copy AND a CRLF copy**
  (`.claude/work_files/wi512-sources/break-tests.py` + `mutations.py`). One of
  them found that a **verbatim C# string does not process escapes**, so my
  "only one temperature threshold" regex hunted for a literal backslash and
  passed on a page carrying two. §12.8 gained six rules plus the slot-5 note.

- **2026-09-04** — **WI-511 done — `/treatments/radiation-therapy`, the second
  page of the TREATMENT library, and the first gate on the site that looks for
  British usage.** P5 Wave 1, sixth of the 29 library pages. Reading grade
  **5.4**, 1120 tests (1070 before), ContentCheck 197/0. Seven glossary terms
  (radiation oncologist, simulation, radiation mask, linear accelerator,
  somnolence syndrome, whole-brain radiation, memantine). Full twelve slots,
  read from §12.8 rather than copied from craniotomy.
  **The dossier was wrong three more times, which makes eighteen bad citations
  across seven items — and one of them produced this item's blocker.** (1) The
  somnolence "usually resolves on its own" claim is attributed to the Brain
  Tumour Charity's **jargon-buster** page. Fetched, that page is **one sentence
  long** and says no such thing; the claim is on the charity's *adults*
  side-effects page. (2) **PMC7017115**, cited for the cognitive late effect, is
  a mechanistic review largely about the **mouse brain** — WI-507's rodent
  fixation error in a new coat; ACS says it at patient level. (3)
  **`ascopubs.org` returns a 1,830-byte JavaScript shell**, so the whole-brain
  claim rests on **PMC7106984** (NRG CC001) directly. NBK66023 stays out: §12.1
  forbids NCI patient PDQ for brain-metastasis radiation.
  **The blocker was the harder version of WI-510's own rule, and I walked into
  it.** "Deleting a bad citation does not delete the claim" — so the first draft
  dropped the jargon-buster URL. But that page is the **only source anywhere in
  the fetched set for the four-to-six-week timing**, which the draft kept. One
  URL, two claims, wrong about one and the only source for the other. Both BTC
  pages are cited now, each for what it says, and the page prints the
  disagreement between them: the charity calls somnolence rare, the study saw it
  in most of a small group, and the draft had split the difference with an
  unattributed "uncommon" belonging to nobody.
  **A number written as a word is still a number.** The whole-brain section said
  "most people in both groups lost some thinking skills", importing R3's
  reasoning about the **SRS** comparison and asserting it of **CC001**, where the
  per-test rates are 23.3% v 40.4%. "Most" is false of the arm the page
  recommends. The page's own no-percentages test could not see it — it matched
  digits, and this corpus writes every number in words.
  **The seizure line under-triaged against the page it links to.** The
  call-today list said "There is a seizure" flat, while `/seizures/what-to-do`
  says a **first** seizure is a 911 call. WI-510's blocker was an ambulance list
  that over-escalated; this is the same rule failing in the more dangerous
  direction, and the test named for the decision could not see it because it
  only asserted that no heading said "ambulance".
  **The identical WI-510 defect shipped again in the first draft: a guessed
  gender for a real named patient.** The NBTS transport quote is from Tommy M.,
  whose pronouns the source never states, and my draft wrote *"One **man**… to
  drive **him**"*. WI-510 recorded that exact lesson six weeks of context ago and
  it did not prevent the repeat. Reading the page end to end is what caught it,
  along with three unsourced comparative frequency claims and an **invented
  reassurance** closing the mask section (*"almost everybody gets through the
  course"* — no source says it, on the section written for the most frightened
  reader on the page).
  **Two rules promoted to `CuratedPage`, which §12.8 asked for at the second
  treatment page — and both were wrong on the first attempt.**
  `AssertNeverMinimises` needed a **clause anchor**, because a bare lookback
  waves through "it is not painful, and it is a simple procedure" and fails
  "there is no such thing as a simple procedure". `BritishForms` shipped five
  entries that are substrings of correct US words (`specialis`→**specialist**,
  `characteris`→**characteristic**, `organis`→**organism**, `realis`→
  **realistic**, `analyse`→**analyses**) plus `radiotherapy`, which is not a
  British spelling at all but standard US vocabulary inside *Stereotactic Body
  Radiotherapy*. It also had to **flatten before matching**: the first version
  read raw text and walked past `"a lift"` in `blocks/caregiver.md`, where the
  hard wrap falls between the two words — a British idiom on **eighteen tumor
  hubs**, missed by the gate written to catch it. Fixed in the block, and
  `/tests/mri`'s "arrange a lift home" with it.
  **Running the ban list over the whole corpus for the first time found a phrase
  sitting over a correct sentence for eight items.** §12.8 asks for a corpus scan
  before *adding* a phrase; nobody had asked it of the phrases already there.
  `"bad news"` fails on `/seizures/what-to-do` — *"a seizure is **not**
  automatically bad news about the tumor"* — and nothing went red because only
  two pages assert the list and neither uses it. `"bad news"` demoted;
  **`"good news"` kept**, on review's push-back, because retiring a working
  guard for symmetry with a broken one is a net loss. The lists now defend
  themselves site-wide.
  **Also:** slot 8 (re-irradiation) has **no patient-level source anywhere** —
  ACS, MSK and NBTS are all silent — so it rests on the EANO guideline and is
  published at that strength: an option after roughly a year, indications
  controversial, no trial settles it. The page carries **no ambulance list** on
  purpose (the shared block owns the escalation and links the seizure page; a
  third copy is a third copy to keep in step). **All 70 breaks proven on an LF
  copy AND a CRLF copy** (`.claude/work_files/wi511-sources/break-tests.py`),
  including two written specifically to prove the clause anchor and the flatten.
  Print checked by rendering to PDF and reading the text back: all glossary
  words survive as words, no popover text leaks into the body.
  **`/review` was run and found four blockers** — the orphaned timing, the
  seizure under-triage, a mask claim contradicted by the page's own step list
  ("on for the few minutes of your treatment", when the mask goes on before
  setup imaging), and the CC001 "most". It also found three of my tests were
  theatre before I ran it, and I had already fixed three others myself.

- **2026-09-04** — **WI-510 done — `/treatments/craniotomy`, the first page of
  the TREATMENT library, and the first library page since WI-506 to use all
  twelve slots. PR #84.** P5 Wave 1, fifth of the 29 library pages, and the one
  that opens the `/treatments/<slug>` scheme WI-506 set. Reading grade **5.0**, 1070
  tests (1039 before), ContentCheck 183/0. Six glossary terms (bone flap,
  craniectomy, SMA syndrome, 5-ALA, laser ablation, levetiracetam).
  **The template had quietly shrunk over three items, and this is the item that
  put it back.** WI-507, WI-508 and WI-509 each correctly dropped slots 2, 5, 7
  and 8, because a wait, a document and a reference list have no day and no
  procedure. Copying the last page written would have kept them dropped for the
  remaining 24. A craniotomy has a day, so all four came back, and there is now
  a test that names them. §12.8's first new rule says it out loud: **read the
  standard for the slot list, not the previous page.**
  **The dossier was wrong three more times, which makes twelve bad citations
  across six items.** (1) "Most patients spend the first night in the ICU" is
  attributed to **ABTA**, which does not contain the string "intensive care" at
  all; the claim is true and is in StatPearls NBK560922 — *"Intensive care unit
  availability should be discussed preoperatively since most patients will
  require this level of care after a craniotomy"* — so the citation was
  repointed, not the claim dropped. (2) "Frequent neurological checks (name,
  date, squeeze my hands, follow my finger) through the night" is attributed to
  **NBTS**, and NBTS says nothing about neurological checks anywhere; I read the
  whole article. The invented script is not on the page, and what is there is
  written at the level StatPearls and ACS actually support. (3) The
  gross-total-resection definition — **the single most important sentence on
  this page** — rests on **PMC5358612, which is a conference abstract** (PP32, a
  single-centre poster). ACS carries it properly, and recently: an MRI or CT
  1 to 3 days after the operation to confirm how much came out. Also dropped
  **PMC7093492**, a survival meta-analysis the dossier cites for "GTR is not
  always the goal" — wrong on the claim, and wrong on a page that publishes no
  prognosis figures. A test fails if any of them reappears.
  **Reading the page end to end found eleven defects every gate passed clean,
  and one of them was about a real person.** The NBTS "floating consciousness"
  quote is from a named patient whose pronouns the source never states, and my
  draft wrote *"apart from **his** own body"*. Also: *"We do not publish numbers
  here"* used a site voice that appears on **no other page** — every "we" in the
  corpus is the reader talking to their team in a question; *"swelling and
  bruising around your eyes, which can look alarming and is not"* is a sentence
  that parses and means nothing, which is exactly the caregiver fragment Dan
  caught on 2026-08-31; and *"Most of what shows up right after surgery gets
  better"* contradicted *"many ... some do not"* two sections later. **A claim
  about how many people recover cannot have two different strengths on one
  page.** Fifth time a human-style read has caught what the suite cannot.
  **My lead-in to a shared block contradicted the block.** I introduced
  `[TUMOR-BOARD]` with "your case is very likely to be discussed", while the
  block says a tumor board *"tends to happen when a case is complicated"*. Both
  other pages carrying it use one hedged line. **The sentence above a block is
  shared prose too**, even though it lives on the page. Written into §12.8.
  **Seven British spellings shipped in the first draft and nothing looks for
  them** — `anaesthetist`, `anaesthetic`, `jewellery`, `theatre`,
  `physiotherapist`, `tablets`, "you will be got up". The corpus has **zero**
  British forms. A reader in Ohio would have met a page that sounds like it is
  about a different health system.
  **Six ban-list candidates were rejected before shipping, and the reasons are
  recorded so the next page does not re-do the work.** `CuratedPage.
  Characterisations` gained three treatment-page shapes; `"good result"` failed
  on the spot against this page's own slot 2 ("it changes what a good result
  looks like"), and `"completely gone"`/`"all clear"` were dropped because **the
  page dismantling a phrase has to be able to print it**. They now live in a new
  `RejectedCharacterisations` array with the reason for each. The
  "never minimise the operation" rule is deliberately **page-local**: §12.8's
  "factor at the second use" has an other half, which is do not factor at the
  first. WI-511 promotes it.
  **All 37 breaks proven, on an LF copy AND a CRLF copy** (harness at
  `.claude/work_files/wi510-sources/break-tests.py`, extended so a mutation can
  name a *different file* — the "reachable from /start" test is about a link on
  another page, and mutating craniotomy.md could never have broken it). One test
  was a false positive on first run and is recorded as such: the
  "never ask the surgeon to take it all out" regex spanned a heading and matched
  **the page's own questions list**, where "what is the goal of my operation: to
  take it all out, to take some out" is exactly right.
  Also: `CuratedPage.Section` now tolerates an explicit `{#anchor}` so a caller
  names a section by the words a reader sees; print checked by rendering to PDF
  and reading the text back (all glossary words survive); 6 tooltips suppressed
  where the page defines the word itself, 3 firing where it does not.
  **Not yet reviewed by an independent pass** — same standing no-agents
  instruction as WI-509, so that is now two consecutive pages without one, and
  an independent pass caught a blocker on each of WI-506, WI-507 and WI-508.

- **2026-09-03** — **WI-509 done — `/tests/molecular-markers`, and the hardest
  editorial line on the site is now held by a machine.** P5 Wave 1, fourth of
  the 29 library pages. Reading grade **4.1**, 1039 tests (1018 before),
  ContentCheck 170/0. Fifteen entries — IDH, 1p/19q, ATRX, TERT, CDKN2A/B, EGFR,
  chromosome 7 and 10, H3 K27-altered, H3 G34, BRAF, MGMT, Ki-67, TP53, gene
  panels, methylation profiling — three lines each: what is measured, what your
  team does with it, what it does not tell you.
  **The entries are visible anchored sections with a jump list, not
  collapsibles, and that was a change to the ticket.** A `<details>` closed on
  load defeats the item's own harder criterion (a reader searching "what is
  1p/19q" lands on it — fragment auto-expansion is not universally supported, so
  they land on a heading with nothing under it); it prints empty, on the one page
  people hold beside the document; and it needs a second `:::` container taking
  an argument, which is a **new fail-open surface** — WI-503 documents five ways
  a mistyped fence publishes content wide open — guarding words that are
  descriptions, not prognosis. Gating them also tells the reader they are
  frightening, which is the opposite of the page. Recorded in §12.8 so WI-510
  onward do not re-argue it.
  **The Cloudflare-gated source the dossier hangs everything on is open, and it
  is now named for the remaining 28 pages.** `tests-library.md` §5.2 sources
  nearly every marker row to one `academic.oup.com` URL. By DOI in Europe PMC it
  is **Sahm et al, Neuro-Oncology 25(10):1731-1749, the EANO guideline on
  molecular diagnostic tools for WHO CNS5, open at PMC10547522** — and it carries
  what all fifteen markers measure, by which method, including the MGMT cut-off
  problem. A test fails if `academic.oup.com` ever appears in this page's front
  matter: a citation nobody can open is a citation nobody can verify.
  **Verifying my own sources turned up four wrong citation titles in ALREADY
  SHIPPED glossary entries, across 16 files, and one is the WI-505 failure mode
  exactly.** `mgmt-methylation.md` cited PMC12467656 as *"MGMT promoter
  methylation and response to alkylating chemotherapy"*. That URL is
  **"Radiotherapy in Glioblastoma Multiforme: Evolution, Limitations, and
  Molecularly Guided Future"** (Biomedicines 2025) — a fabricated title on an
  unrelated paper, on the most treatment-sensitive marker definition on the site.
  Repointed to the EANO guideline, which actually carries the claim. Also:
  *"Molecular markers in adult diffuse glioma"* is really Thomas et al's 2021 WHO
  update review (3 entries); *"Major changes in the 2021 WHO classification"* is
  really *"Major **Features** of..."*, Smith et al (11 entries); and the BRAF
  citation is really Houghton et al on MAPK inhibitors. **Nine bad citations
  across five items now.** A wrong citation that RESOLVES is the worst failure
  mode on a medical site.
  **Two more dossier errors.** Its Ki-67 caveat cites PMC10644968, which is about
  **thyroid, lung and breast** cancer; the caveat is real but had to come from a
  neuropathology source that says it of brain tumors (no standardised method for
  counting stained nuclei or choosing the area, so poor interobserver
  reproducibility). That is now the Ki-67 entry's third line, and it is the
  honest, non-prognostic thing to tell someone comparing two reports.
  **One clause of the somatic/germline block is deliberately absent, and it is
  the one the ticket quotes.** "You cannot pass them on to your children" is
  verbatim-supported by NCI: *"Somatic mutations cause most cancers and can't be
  passed on to family members."* **"You did not do anything to cause them" has no
  source** — SYNTHESIS §3.7 presents the whole paragraph as validated framing,
  but the paper it credits (PMC8062319) is a breast-cancer genetic-counselling
  lexicon and does not contain it. The section holds without it, and gains the
  honest other half: a tumor test does occasionally turn up something a person
  was born with, and the team tells you and offers counselling first. Leaving
  that out would make the reassurance the kind that stops being true.
  **My own test caught the authoring machinery.** Suppressing fifteen tooltips
  writes `!%H3 G34%!%BRAF%` into the file, which puts the characters "34%" into
  the source — and the page's no-percentages rule fired on text no reader will
  ever see. Fixed in the shared helper (`CuratedPage.ReaderText`), not the page:
  a prose rule asserted against raw source is asserting against something that is
  not the prose.
  **A phrase was rejected from the shared ban list before it shipped.** I
  proposed adding "good sign" and "bad sign" to `Characterisations`, then ran the
  candidates over the corpus per §12.8 — the wait page already says *"That is
  normal and it is **not** a bad sign"*, which is correct and is the natural way
  to write it. Both dropped; the other ten additions are clean corpus-wide. A
  rule that fails a correct page is worse than no rule, and this time the rule
  was caught before it existed.
  **Reviewing my own prose found the one defect no gate could.** The MGMT entry
  said it was "the one result on this page" used for choosing treatment — while
  the BRAF entry two screens up says there are drugs made to act on BRAF changes
  and a team may test for it with treatment in mind. **A uniqueness claim on a
  page of fifteen entries is a claim about the other fourteen**, and it went
  stale inside a single draft. Fixed and pinned. (Note: the code-reviewer agent
  was NOT run — standing instruction this session not to launch agents. An
  independent pass caught a blocker on each of WI-506, WI-507 and WI-508, so this
  page has had less scrutiny than its three predecessors. Worth a look.)
  **Anchors are written explicitly now, not derived.** `### MGMT ... {#mgmt}`,
  fifteen of them, with a test that fails if an entry is added without one.
  WI-508 established that heading anchors are a published interface; generic
  attributes are the mechanism that makes the wording and the interface
  independent. All 15 verified on the rendered page.
  **All 15 new tests proven by breaking them — 30 breaks, on an LF copy AND a
  CRLF copy**, converted in Python binary mode (harness at
  `.claude/work_files/wi509-sources/break-tests.py`). Also: 3 glossary terms
  (TP53, H3 G34, chromosome 7 gain and chromosome 10 loss), tooltips down to 3 on
  the page rather than 15 because the words it defines are suppressed there and
  fire everywhere else, and §12.8 gained six rules plus the recorded rejection of
  collapsibles.

- **2026-09-03** — **WI-508 done — `/tests/pathology-report`, the walkthrough of
  the document WI-507 left the reader holding.** P5 Wave 1, third of the 29
  library pages. Reading grade **5.2**, 1018 tests (998 before), ContentCheck
  163/0. Twelve headings; the wait, the layers, the addendum's meaning, second
  opinions, the tumor board and the patient portal are all **links** to WI-507,
  and a test fails if that prose reappears here.
  **The dossier was wrong twice more, and one is the WI-505 failure mode
  exactly.** `tests-library.md` §5.4 sources both "what a brain tumor report
  contains" AND "one of the most important documents guiding treatment
  decisions" to **PMC4300589**. Fetched, that paper is *"Brain tumors: Special
  characters for research and banking"* (Adv Biomed Res, 2015) — a biobanking
  and cytogenetics review that says neither thing. **Johns Hopkins carries both
  sentences verbatim** and is what shipped; a test fails if PMC4300589 ever
  appears in this page's front matter. The CAP PDF the dossier cites
  (`documents.cap.org/.../how-to-read-pathology-report.pdf`) is **dead** —
  returns nothing at all. **Seven bad citations across four items now.**
  **WI-507's open thread is CLOSED, and the source is a good one.**
  `MyPathologyReport.ca`'s "Amendment: Definition" is patient-level, written and
  reviewed by practising pathologists, and defines an amendment against an
  addendum. It supplies the two things WI-507 could not say: the commonest
  reason is a **small correction such as a typing mistake**, not a wrong
  diagnosis, and *"amendments reflect the pathology system working as
  intended"*. That paragraph on `/tests/waiting-for-results` is now full, and
  `amended report` is a glossary term.
  **Every naming and grading claim rests on WHO CNS5 itself** (Louis et al,
  PMC8328013) rather than a summary of it, including the Roman-to-Arabic reason
  in the authors' own words, grading within a tumor type, a gene result setting
  the grade where the cells look lower, and NOS/NEC. **Johns Hopkins is cited
  for what a report CONTAINS and never for anything evaluative** — its own
  glossary calls IDH "associated with a better prognosis" and its sample report
  says "(WHO grade IV)". That is the dated paperwork this page translates, not a
  source to write from.
  **Review caught two of my tests being theatre, and one of them guarded the
  most sensitive sentence on the page.** The NOS/NEC check asserted the word
  *"worse"* was PRESENT in the opening, as a proxy for a reassurance — so
  *"NOS usually means the tumor is worse than they first thought"* passed it.
  That is the exact sentence the test exists to prevent. Now polarity-aware. The
  rename test likewise did nothing to stop a retired-name crosswalk being pasted
  in beside the mechanism, which would have given WI-513's canonical content a
  second home.
  **And a real editorial error in the section the page exists for.** I wrote
  that the extra words a name gains are "not a new problem" — while the grade
  section says a gene result can set the grade even when the cells look lower
  grade. For the reader whose molecular results turned a grade 2 into a grade 4,
  that reads as the site telling them nothing happened. It now says "gain words,
  lose words, **or change grade**" and links to the grade section.
  **Two things beyond the ticket, deliberately.** (1) A **site-wide
  Roman-numeral grade guard** — WI-505 made that mechanical for the glossary
  only, nothing checked the 50-odd curated pages, and this page is the first
  that could break it (it prints "grade III" once, on purpose, to teach the old
  notation; that is the single allow-listed sentence). Review then caught that
  my scan read `Content/pages/` but not `Content/blocks/` — **a "grade IV" in
  `caregiver.md` is a grade IV on eighteen tumor hubs**, invisible to a
  pages-only scan. Fixed and proven by planting one. (2) `EveryLinkOnThePage
  Resolves` had become **three near-identical copies that had already drifted**;
  §12.8's own rule is "factor at the second use", so it is now a shared
  `CuratedPage` helper. Doing that exposed a rule I had overgeneralised: "the
  body links outside 'Where to go next'" is true of this page and **false of the
  MRI page**, whose every link legitimately sits in that section. A rule that
  fails a correct page is worse than no rule. Written into §12.8.
  **The line-ending check I ran first was wrong, and it mattered.** `grep -c
  $'\r'` told me the new files were CRLF; they were LF, and the existing
  checkout is CRLF. So the tests had only ever been proven on LF. Re-proven on
  both, with a binary-mode conversion — `perl -pi` on msys reads and writes in
  text mode, so it silently did nothing. **22 breaks run, all fail correctly.**
  **The IgnoreCase trap bit for the third time** (after WI-505 and WI-506): my
  NOS/NEC negation regex was case-sensitive, and the page's sentence starts
  *"Neither is a grade"* with a capital N. A negation is very often
  sentence-initial, which is exactly where the capital letter is.
  Also: §12.8 gained four rules and a correction — **slots 2 and 5 are not
  universal** (both assume a day and a procedure; neither has a referent on a
  page about a document), a document page needs a "where the answer sits"
  section before the walkthrough and must say the layout varies, heading anchors
  are a published interface once other pages deep-link them, and the
  shared-prose rule applies to the tests too. 3 glossary terms (immunohisto­
  chemistry, gross description, amended report), 10 tooltips firing on the real
  page, all 10 surviving in print.

- **2026-09-03** — **WI-507 done — `/tests/waiting-for-results`, the page
  SYNTHESIS §3.1 says is the highest-value in the phase and that no comparator
  has.** P5 Wave 1, second of the 29 library pages. Reading grade **4.8**, 998
  tests (981 before), ContentCheck 156/0. The spine is the one the research
  asked for: the answer arrives in **layers**, the name of the diagnosis can
  legitimately change between them, and saying so in advance is what defuses
  "they changed their story". Batching (about eight samples a run) is the
  mundane cause the page hands the reader instead of a vacuum.
  **The dossier was wrong twice more, and one of the errors would have put a
  rodent-tissue figure on a patient page.** `tests-library.md` §4.5 sources
  "6 to 72 hours" of fixation to a CellCarta guide that says on its face it
  covers **rodent** tissue; Leica — already cited on the same page — says human
  diagnostic specimens fix for **6 to 24 hours**, which is what shipped, and
  CellCarta is off the page. §4.4 attributes the 504/558 (90.3%)
  frozen-section agreement figure to **PMC4322495**, which is a different and
  smaller study; the real source is **PMC4287923**. A test now fails if
  PMC4322495 ever appears in this page's front matter. Also: "runs overnight"
  and "another day or more" were the dossier's words and no cited source's, so
  both are gone.
  **Every turnaround number in the ticket sits behind a 403.** The
  `academic.oup.com` audit is Cloudflare-gated; the same paper is open at
  **PMC13161907** and every figure checks out. Worth knowing for WI-508/509:
  when an `academic.oup.com` URL blocks, look it up by DOI in Europe PMC.
  **Review caught a claim of mine that was backwards, in the one section the
  page exists for.** I wrote that when the quick answer during surgery does not
  match the final one, the difference is "more often about the grade". The
  source's own breakdown of its 54 disagreements says 22 were grading — so a
  *different tumor entirely* is the commoner case. Telling a frightened reader
  to brace for the smaller shock is worse than telling them nothing. It now says
  both, in the order the source supports.
  **And a claim that was true only for gliomas.** "Your team cannot finish the
  diagnosis without the gene tests" is exactly what the audit says about glioma
  and explicitly contrasts with skull base, pituitary and metastatic disease.
  Left as written, it told meningioma and brain-mets readers to expect three
  weeks that were never coming.
  **My own test was vacuous on every Windows checkout, and it took breaking it
  twice to see.** The attribution test splits the section into paragraphs to
  check that a UK-audit figure never drifts into a paragraph that fails to
  attribute it — using `Split("\n\n")`. This repo has `core.autocrlf=true`, so a
  real checkout hands it `\r\n\r\n`, **nothing splits, the whole section arrives
  as one paragraph, and every figure finds an attribution belonging to a
  different one.** It passed with the attribution deleted. CI is Linux/LF, so CI
  would have stayed green forever. That is WI-501's trap in a new place: the
  first break I wrote happened to be CRLF (Python text mode on Windows) and the
  second happened to be LF, and the difference between the two results is the
  only reason I looked. Now CRLF-tolerant, and re-proven **on a deliberately
  CRLF copy of the page**.
  **`[TUMOR-BOARD]` is now a shared block**, and the second copy had already
  drifted before anyone noticed: my draft dropped "what they decide is advice,
  not an order", which is the best line in the MRI version. Written out of both
  pages into `Content/blocks/tumor-board.md` and verified composing on both
  against the running app, with the block's NBTS citation merging into this
  page's source list. With 29 library pages, "factor it out later" means 29
  versions of it — so §12.8 now says the second use, not the fifth.
  **Deliberately thinner than the ticket in one place:** it asks for "addendums
  and amended reports". Addendum is fully sourced (ACS). **Amended** has no
  patient-level source that defines it — the only papers that do are about
  error rates, and citing those here would imply mistakes are common — so the
  page names the situation and says what to ask rather than defining the
  mechanics. Recorded in the backlog item; revisit if a source turns up.
  Also: 3 glossary terms (neuropathologist, addendum, gene panel), 8 tooltips
  firing on the real page, no caregiver block (a tests page — one paragraph to
  whoever is waiting alongside, and the link), and four new rules written into
  **§12.8**, one of which corrects a line in the template's own table that
  contradicted slot 8's description.

- **2026-08-31** — **WI-506 done — `/tests/mri`, and the library-page template
  is now written down at content-pipeline.md §12.8.** P5 Wave 1, the first of
  the 29 library pages. Reading grade **4.0**, 981 tests (967 before),
  ContentCheck 149/0. URL scheme set here for the whole phase: `/tests/<slug>`
  now, `/treatments/<slug>` to follow.
  **The ticket said ten sections. The first real page needed thirteen, and the
  doc now says twelve slots with a variable middle.** Three of them had no slot
  at all, and one is universal that the ticket missed entirely: **"Who reads it,
  and how do I get the result?"** applies to all 29 pages. Leaving it out would
  have had 29 sessions each rediscover it. §12.8 also records the heading trap
  underneath it — my first draft headed that section *"When will I know the
  result?"* and then never said when, because timings are local and no source
  gives one. The heading asked a question the section could not answer, which is
  the same defect class as the caregiver fragment Dan caught on 2026-08-31, and
  it took a reviewer reading the words to see it.
  **Every source URL was fetched before it was cited, and the dossier was wrong
  twice.** `tests-library.md` §1.6 attributes the kidney and iodine claims to
  RadiologyInfo's *Brain MRI* page; that page was re-reviewed on 2026-06-15 and
  **no longer mentions kidneys or iodine at all** — the claims live on the *MRI
  Safety* page. Citing the dossier's URL would have been a link that resolves
  and does not say it, which is the WI-505 failure mode exactly. The FDA
  gadolinium-retention URL in the dossier **404s**.
  **Review caught the same class of error in my own work, and it was the
  blocker.** I wrote the navigation-scan section from `tests-library.md` and
  cited NBTS's surgery page for it. NBTS says exactly one thing on the subject —
  *"patients will also likely have to undergo a preoperative MRI"* — and nothing
  about maps, markers or why there is a second scan. ACS *does* describe markers
  on the scalp creating a map of the inside of the head, **but says it about a
  needle biopsy, not a craniotomy.** Six attempts at a citable neuronavigation
  source returned four 404s and one page about feminizing surgery. Fixed by
  finding a source that does say it: **ACS's "Surgery for Brain Tumors" page** —
  *"MRI or CT scans can be done before surgery to map the area of tumors deep in
  the brain"* — and rewording the marker sentence to "for some procedures", which
  is what the evidence supports.
  **Two page-level judgement calls worth carrying to the other 28.** The page
  never says "ask for an open MRI" — open and upright scanners are lower field
  strength and not equivalent for tumor imaging, so that advice can cost a reader
  the picture their treatment is planned from; it says ask what your center has,
  and a test fails if the phrase ever appears. And it answers the gadolinium
  question in both directions (traces do stay, there are no known health effects)
  because dropping either half is its own kind of dishonest.
  **R2 held itself:** the source says one in twenty people need a sedative, and
  that number is not on the page.
  **Two of my own tests were theatre until review, and both are now proven by
  breaking them.** The claustrophobia and device-card tests asserted substrings
  against the *whole page*, so the reassurance could drift into the footer and
  the card advice could sit under three paragraphs of exclusions with both still
  green — which is the exact page they exist to prevent. Now section-scoped, and
  checked against the first and last sentences of that section. The link-check
  canary had the same shape: the layout contributes ~13 links, so `checkedLinks
  > 0` could never fire and deleting "where to go next" would have passed.
  **WI-435 confirmed live, and it cost me a run.** `dotnet run --project
  tools/BrainHarbor.ContentCheck --nologo` swallows `--nologo` as its positional
  pages root and reports **"pages root MISSING — no pages were checked"** while
  exiting on an unrelated glossary failure. I graded nothing and did not notice
  until I read the warning line. CI invokes it with no arguments so CI is fine;
  a human at a terminal is not.
  **A note for WI-439, because I nearly repeated the mistake that ticket is
  about.** `A11ySmokeTests.TheReaderChoiceGateOpensAndClosesWithJavaScript
  Disabled` failed **three full-suite runs in a row**, which is well outside the
  ~1-in-5 recorded. I assumed I had broken it, stashed my changes and ran clean
  (967 green) — and the stash/pop then converted my new files to CRLF, which
  surfaced a **real** bug in my own test: a `$`-anchored multiline regex that
  passes on LF and fails on CRLF. That is WI-501's exact trap, and CI being
  Linux/LF means it would have stayed green while a Windows clone failed. Fixed;
  six consecutive green runs since. So the A11y failures were the flake after
  all, in a longer burst than the ticket records.
  Also: 3 glossary terms (tumor board, radiologist, neuronavigation), 4 tooltips
  firing on the page rather than a carpet, verified at 390px and by printing to
  PDF that all four terms survive as words.

- **2026-08-31** — **P5 Wave 0 RELEASED to production — PR #74, eight items in
  one deploy window.** WI-501 (shared blocks), WI-502 (the §12 standard), WI-503
  (the outlook gate), WI-504 (the hand-fetched sources), WI-505 (40 glossary
  terms), WI-558 (the caregiver section on all 18 tumor hubs), WI-559 and
  WI-560 (the two seizure pages). One deploy for the wave rather than four, as
  Dan called it on 2026-08-30.
  **Dan reviewed it locally first, and found one thing**: the caregiver block
  opened with *"or lie awake listening"* — I meant listening **for a seizure**
  and never wrote the second half, so it reads as a fragment ("lie awake
  listening at the doctor"). Fixed in the block, which fixed all 18 pages at
  once (**PR #76**), along with *"One was told to stop playing doctor"*, which
  has the same shape of defect: a real quote from the study with nobody named,
  so it reads as a typo. **Both were invisible to every gate we have** —
  reading grade, ContentCheck, 967 tests — because they are grammatical
  sentences that happen not to mean anything. That is the third time a human
  reading the words has caught what the suite cannot, after WI-412a, WI-437 and
  WI-438 did it with screenshots.
  Verified live after the deploy, not just green in CI: `/seizures/what-to-do`,
  `/seizures/living-with`, `/tumors/glioma` (carrying the corrected caregiver
  line), `/get-help-now` and `/sitemap.xml` all 200, and the new **"Seizures"
  footer link** is on the home page.
  **Deploy window, fourth measurement: ~92 seconds** (01:39:08 to 01:40:40 UTC,
  seven failing rounds, clean on attempt 8 and confirmed on 9). Same signature a
  fourth time: `/research`, `/trials`, `/search` and `/get-help-now` all 500
  with empty bodies while `/` stays up. **Series: 2m00s, 1m23s, 1m24s, 1m32s.**
  WI-431b's shape is settled and its precondition was already met — this is
  Dan's call on whether to spend for Standard-tier slot swaps, and while
  unlaunched the answer is still "batch the releases", which is exactly what
  this one-window release did.
  **Next is WI-506** — Dan's pick, and the right one: it sets the page template
  the rest of Wave 1 inherits.

- **2026-08-30** — **WI-559 + WI-560 done, in one PR, and that was the right
  call. P5 Wave 0 is complete. PR #73.** Two pages: `/seizures/what-to-do` (the
  emergency) and `/seizures/living-with` (the other 364 days). Dan's reason for
  writing them together — one subject split by whether it is happening right
  now, and writing them apart decides that split twice — held up in practice:
  the boundary got decided once, and each page ends by handing the reader to
  the other.
  **Every source URL was fetched and read before it was cited, and the first
  thing that came back was a correction.** I had planned to cite the NHS "when
  to call 999" page for the ambulance list. It names road accidents, strokes and
  heart attacks, and **nothing about seizures, breathing or unresponsiveness**.
  Dropped. The list on the page is now Epilepsy Foundation plus CDC, both
  fetched and read line by line. Nine of the sixteen sources across the two
  pages needed their real URL found first — several paths in the ticket and the
  research reports 404 today.
  **epilepsy.com and cdc.gov both 403 automated fetching**, which is the WI-504
  problem again and would have meant asking Dan to fetch by hand. A full browser
  header set (Accept, Accept-Language, Sec-Fetch-*, Upgrade-Insecure-Requests)
  gets 200 from both. The script is at
  `.claude/work_files/seizure-sources/fetch.sh` — it prints the page title and
  dumps readable text, which is exactly the WI-505 discipline made cheap. **Use
  it before hand-fetching anything in Wave 1.**
  **WI-560 holds both sides, in the order the ticket demanded.** Most people are
  told to do less than they need to, AND exertion-triggered seizures are real
  for a minority — "if you have noticed that pattern in yourself, you are not
  imagining it" is on the page, and there is a test that fails if it leaves.
  The organising sentence leads: **it is rarely the activity, it is what you
  would fall into, onto, or from**, followed by the three questions a reader can
  apply to a hobby nobody wrote a page about.
  **No page anywhere prints a driving duration, and that is now mechanical.**
  `NoCuratedPagePrintsADrivingWaitingPeriod` scans every curated page sentence by
  sentence for "driv" near a duration, in digits AND spelled out ("six months"),
  and it counts what it scanned so it cannot pass by finding nothing. Proven by
  breaking it: adding "You must not drive for six months" failed the build with
  the sentence quoted.
  **Print found a real bug that had been live since WI-101.** `print.css` hides
  every `<button>` as interactive chrome. A glossary term IS a `<button>` — that
  is how the definition pops over without JavaScript (WI-105) — so **every
  glossary word was silently deleted from every printed page site-wide**. The
  seizure sheet printed "if they have a their doctor gave them for this"; the
  missing words were *rescue medicine*, *seizure action plan* and *glioma*. On
  the one page the ticket says people print for the fridge. Fixed with
  `button.term`, verified by re-printing to PDF and reading the text back, and
  pinned by a test. **A screenshot would never have caught this** — it only
  exists on paper.
  **The suite caught my own orphan.** I put both pages in `sitemap.xml`, and
  `EverySitemapPathIsReachableByALinkFromTheHomePage` (written after WI-412
  shipped orphaned) failed: nothing on the home page linked to either. Rather
  than weaken the rule I followed the site's own convention — `/tumors` is
  listed and its 18 type pages are not, because crawlers follow the link. So
  `/seizures/what-to-do` is in the sitemap (people type it into a search box
  while it is happening) with a **footer link on every page**, and
  `/seizures/living-with` is reached from it.
  Also: `[CAREGIVER]` gained the two seizure links — one edit to one file, and
  all 18 tumor hubs have them, which is WI-501 doing the job it was built for.
  Five glossary terms added (tonic-clonic seizure, focal seizure, status
  epilepticus, rescue medicine, seizure action plan); `status epilepticus` was
  rewritten after the ungated grade line reported 8.8, and came back at 5.4.
  Signposts from `/start` and `/get-help-now`. 967 tests (951 before),
  ContentCheck 142/0, pages at grade 3.6 and 4.8.

- **2026-08-30** — **WI-558 done — the caregiver section is a shared block, and
  it is on all 18 tumor hubs today.** P5 Wave 0. **PR #72.** Contract item 11
  said every hub carries one; Dan asked for prevalent rather than tucked away.
  So the block went on all 18 now instead of arriving one hub at a time as each
  is deepened — a one-line include costs nothing per page, and it puts 18
  composed pages under the 6.0 gate immediately (they land at 2.9 to 3.9).
  The shared half is what is true whatever the tumor is: permission to ask
  questions, who your first call is, the two phone numbers and which one is
  "call today" versus "call an ambulance", asking to be shown anything you are
  sent home to do, saying what you notice, looking after yourself. Second
  person throughout.
  **Every source URL was fetched and its title read before it was written
  down**, per WI-505. That paid immediately: I had intended to cite the NHS
  "when to call 999" page for the ambulance line, and **reading it showed it
  names only road accidents, strokes and heart attacks** — no seizures, no
  breathing, no unresponsiveness. It cannot carry an ambulance list, so it was
  dropped and the block routes the reader to their own team instead. WI-559's
  first-tier seizure sources will carry it.
  **Three claims softened after re-reading the sources I did keep**, all the
  same class of error — a source that supports a weaker claim than the sentence
  I wrote. NICE's "key worker" is a model its committee was familiar with, not
  a prevalence fact ("some teams", not "many teams"); the untrained dressing
  changes are now attributed to the study that found them rather than asserted
  as general truth; and caregivers noticing cognitive change first is "can", not
  "often" (the paper says "in several situations").
  **The block does NOT link to the seizure pages yet** — they do not exist until
  WI-559/560, and a dead link on a page a frightened reader is being sent to is
  worse than no link. Adding them is a one-line edit to one file that all 18
  hubs inherit, which is exactly what WI-501 was built for.
  **One WI-501 test had to change, and its replacement is stronger.**
  `NoShippedPageRendersDifferentlyWithBlocksAvailable` asserted that adding the
  block mechanism changed no shipped page — true when no page included one, and
  necessarily false now. Rendering identically with and without the block would
  mean the include did nothing. Split into the two properties that are actually
  load-bearing: a page including no block is unaffected, and a page that DOES
  include one **fails by name** without it rather than silently losing the
  section. Standard written up at `content-pipeline.md` §12.7 (appended, not
  inserted — §12.1 to §12.6 are cited by number). 951 tests (942 before),
  ContentCheck 132/0.

- **2026-08-30** — **WI-560 filed: living with seizures day to day. Dan's own
  example, and it exposed a real hole in the phase as filed.** His ex-wife has
  a low-grade glioma with seizures, controlled on medication; she has worked
  out for herself that anything strenuous can bring on small ones, so she
  manages what she does, and she cannot drive. Nothing on the site helps with
  any of that. Checked the phase before filing: **WI-559** is the emergency,
  **WI-451** is late effects, **WI-525** is the medicines, and section 9 of a
  tumor hub is one paragraph per tumor. **Nobody owned the practical list.**
  **The research finding that shapes the page, and it cuts both ways.** Most of
  the restriction people with seizures are handed is not evidence-based — people
  with epilepsy are measurably less active than the general population because
  of stigma, overprotection and clinicians' own uncertainty, exercise generally
  *reduces* seizure frequency, and the guidance position is individualised
  counselling instead of blanket restriction. **And exertion-triggered seizures
  are real for a minority**: in a series of 400 people only **two** identified
  physical activity as a precipitant, but where it happens it is reproducible
  and tracks the degree of exertion. So the page must hold both. A page that
  only says "exercise is good for you" tells the reader in that minority she is
  wrong about her own body — and she is not, she has observed it repeatedly.
  That is exactly the case Dan described, and it is the failure mode the ticket
  is written to prevent.
  **The organising idea worth keeping: it is rarely the activity that is
  dangerous, it is what you would fall into, onto, or from.** Give a reader that
  sentence and they can reason about a hobby nobody wrote a page about; a list
  of banned things cannot, and goes stale immediately.
  **Driving gets a section and never a number.** US rules vary enormously — 28
  states set a fixed seizure-free period (median 6 months, range 3 to 12), 23
  leave it to clinical judgement, Florida is 2 years with reconsideration at 6 —
  every state requires notifying the DMV, and in some the reporting duty falls
  on the doctor. Explain the shape, link the state tool, print no duration. Same
  line WI-451, WI-525 and WI-559 already hold.
  Also re-scoped **WI-451** (sheds "seizures and driving" to WI-560), amended
  **WI-558** (the caregiver block links to both seizure pages — it is often the
  caregiver doing the over-restricting), and added a note to
  **content-pipeline.md §12.3 section 9** so 24 tumor hubs link rather than each
  restate driving rules. Phase is now 60 items.

- **2026-08-30** — **WI-505 done — 40 glossary terms, 43 in total, and the
  glossary can now cite its sources.** P5 Wave 0. **PR #71.** The P5 vocabulary a reader
  arrives holding: the markers (IDH, MGMT, 1p/19q, ATRX, TERT, CDKN2A/B,
  H3 K27-altered, EGFR, BRAF, Ki-67, methylation profiling), the report words
  (integrated diagnosis, NOS, NEC, CNS WHO grade, frozen section,
  microvascular proliferation), the imaging words (extra-axial, dural tail,
  mass effect, vasogenic edema, FLAIR, contrast dye, pseudoprogression,
  radiation necrosis, RANO) and the treatment words (craniotomy, awake
  craniotomy, resection extents, debulking, fractionation, SRS, temozolomide,
  dexamethasone). Verified live: tooltips fire 3-5 per page on the real feed
  and tumor pages, not carpeted.
  **THE THING TO REMEMBER FROM THIS ITEM. Nine of my thirteen StatPearls
  citations were wrong.** I wrote NBK ids from memory and they resolved to
  real, unrelated chapters — `NBK534244`, cited on four surgery definitions,
  is *Carbon Dioxide Angiography*; `NBK560574` (awake craniotomy) is *Nevus of
  Ota and Ito*; `NBK470259` (frozen section) is *Erythema Multiforme*. Review
  caught the smell (two consecutive ids, paraphrased titles); fetching each
  one proved it. **A wrong citation that RESOLVES is the worst failure mode on
  a medical site, because the link works and nothing looks broken.** Every URL
  now shipped was fetched and its title read first, and
  `EveryShippedTermCitesASource` was tightened from "non-blank string" (which
  `url: TBD` passes) to a real https URL with a title. It still cannot check
  that a URL says what we claim — only a human or a link check can.
  **Sources are a new field.** `sources:` in glossary front matter, rendered on
  `/glossary` as a list matching the curated pages, warned on by ContentCheck.
  Dan's call, chosen over recording citations in the PR body only, because a
  citation that lives in a merged PR is a citation nobody can follow.
  **Six more medical corrections from review, all real.** Pseudoprogression
  said "it is the treatment working on the tissue" — an efficacy inference, not
  what the word means, and self-contradicted by the next sentence; on the one
  term that fires unasked in the feed, that is the WI-503 gate being bypassed.
  Radiation necrosis claimed "your team has ways to tell the difference" when
  telling it from recurrence is a known unsolved problem. `glioma` said grades
  measure how fast a tumor grows, contradicting the two entries that correctly
  say gene results can set the grade. `integrated diagnosis` dated it to 2021
  (it is 2016; CNS5 expanded it). Contrast dye claimed to show a tumor's
  "edges", which is exactly what `diffuse glioma` says a scan cannot do. Awake
  craniotomy promised "you feel no pain" without qualification.
  **Four aliases removed for firing in the wrong place**, all the same class of
  bug: `steroids`→dexamethasone (broader than the drug), `fractions`→
  fractionation (ordinary English — "a fraction of patients responded"),
  `brain swelling`→vasogenic edema (broader, and steroids are wrong for some
  of what it covers), bare `EGFR`→amplification (means the mutation more
  often). `H3 K27M` renamed to **`H3 K27-altered`**, the CNS5 wording, since
  K27-altered also covers EZHIP and EGFR routes.
  **A reading-grade line, reported not gated** (FK on a 25-word definition is
  too noisy to fail a build on — the same reason `CheckRazorPage` refuses under
  25 words). It earned itself immediately: it showed `glioblastoma` at **13.6**
  after a rewrite that made it shorter and so concentrated the polysyllabic
  words. A pass on everything ≥8 brought the set to **mean 5.3, median 5.6,
  max 8.5** — the max is `1p/19q co-deletion`, floored by the word
  "oligodendroglioma", which is the term being defined.
  **Two of my own tests were weaker than they looked.** The Roman-numeral check
  had no `IgnoreCase`, so `\bgrade` never matched "Grade" — it would have
  missed "CNS WHO Grade IV", the likeliest way anyone would write it. And the
  marker rule was a hardcoded 11-slug list, which exempts every future term by
  default; inverted to an allowlist of the four terms whose definition
  legitimately includes behaviour, so WI-509's new markers are covered on
  arrival. 942 tests (909 before), ContentCheck 132/0.

- **2026-08-30** — **WI-503 done — outlook now sits behind a choice the reader
  makes, and a mistyped gate fails the build instead of publishing the
  prognosis.** P5 Wave 0. **PR #70.** Authors write `:::outlook` … `:::` in a curated page;
  it renders as a `<details>` that is **closed on load**, with the §12.5
  warning sentence and the Show/Hide label emitted by the component rather than
  typed per page, so 24 tumor hubs cannot each soften them.
  **It could not have been "just write the HTML":** `ContentStore` renders with
  `DisableHtml()`, so a `<details>` typed into a .md file renders escaped. It
  had to be a Markdig component, which is what made the failure mode below
  possible in the first place.
  **The whole item is one safety property, and my first implementation did not
  have it.** Markdig renders an unknown `:::name` as an anonymous `<div>` — so
  `:::outlok` would publish the outlook section wide open, no error, green
  build. I validated the parsed tree, which catches that one. **Review found
  four more that the tree cannot see, because the typo stops a container being
  built at all**, and I reproduced every one before fixing it:
  `::outlook` (one colon short) renders as a plain paragraph; a tab or four
  spaces before the fence makes it an indented CODE block, which renders the
  words in full **and** hides them from the 6.0 reading check, since
  ContentCheck grades inline text and a code block has none; an unclosed fence
  swallows the rest of the page — caregiver section, support links — into a
  disclosure the reader was told is only about outlook; and `::outlook::`
  inline is a different node type entirely. Fixed by auditing the raw source
  alongside the tree: every `:::`-like line must be accounted for by a
  container that actually parsed. Nine tests, written first and watched to
  fail.
  **Two more fail-open paths, both about renderer registration.** Setting the
  pipeline up twice on one renderer put Markdig's default renderer back (its
  extension re-inserts at index 0 when it finds its renderer missing, and my
  `Contains` early-return then bailed) — not live today, since every call site
  allocates a renderer per page, but "reuse the renderer" is an ordinary
  optimisation and the symptom is silent open prognosis. And registering the
  gate extension **before** `UseAdvancedExtensions()` loses the same way, which
  is a one-line mistake with no visible symptom. Rather than leave that to a
  comment, the pipeline now **renders a probe gate at start-up and refuses to
  build if it did not come out closed** — a mis-ordered pipeline fails loudly
  instead of publishing.
  **I also had the print story wrong.** I checked `site.css` for `@media print`,
  found none, and concluded the site had no print styles — `print.css` has
  existed since WI-101, loaded `media="print"` **after** site.css, so my rules
  were both in the wrong file and losing ties. Moved. Printing reflects the
  reader's choice: a gate left closed stays closed on paper (forcing it open
  hands the outlook section to someone who declined it), and the toggle is
  hidden once open, since "Hide the part about outlook" is an instruction that
  cannot be followed on paper. Verified by screenshot in print emulation, both
  states, not by reading the CSS.
  909 tests (875 before), ContentCheck 50/0 — all 50 existing pages pass the
  new fence audit, so no false positives. Eyeballed at desktop and 390px,
  closed/open/keyboard-focused, with JavaScript off.
  **`Content/blocks/` still ships empty and no tumor content was written** —
  WI-513 writes the first outlook section.

- **2026-08-30** — **WI-439: its central assumption is contradicted, with a
  captured `.trx`.** Chased down while finishing WI-503, because the flake
  attached itself to the new no-JS gate test twice and "it is probably the
  known flake" is exactly the assumption this file has been burned by before.
  Reproduced at **3 failures in ~16 full-suite runs** (~1 in 5) and captured
  the message rather than inferring it: `EnsureServer()` → `CreateClient()` →
  *"The server has not been started"*, thrown from `InitializeAsync`, no axe
  rule involved. **But 9 of the other 10 `A11ySmokeTests` passed in that same
  run**, and they share one `IClassFixture` factory instance — so they called
  `EnsureServer()` on the *same* object either side of the failure. WI-439 says
  "the factory poisons itself", and therefore that a plain retry "fixes
  nothing" and the fix must dispose and rebuild. **A permanently poisoned
  factory would have failed all ten.** The bad state is transient, so the cheap
  fix may be the right one. Recorded in the ticket; not fixed here.

- **2026-08-30** — **WI-502 done — the tumor-guide editorial standard is
  written down, at `docs/content-pipeline.md` §12.** Appended as §12 rather
  than inserted near the curated-content rules, because §2/§3/§4/§5/§6/§9/§10
  are cited by number from CLAUDE.md, the backlog and the startup references;
  renumbering would have broken every one of them silently.
  Covers source precedence, the 12-item shared contract, the 17-section order
  with the evidence for each position, the R1/R2/R3 number rulings,
  prognosis-without-figures plus the reader-choice gate, and the writing rules
  specific to these pages.
  **Two things came out of reading the NCCN guideline instead of the research
  summary of it.** (1) **It is CNS5-aligned**, verified against the document:
  Arabic grades throughout, `IDH-mutant`, and neither "anaplastic astrocytoma"
  nor "oligoastrocytoma" appears anywhere in 76 pages. That gives the
  precedence rule a clean shape — **NCCN governs naming and grading, NCI
  patient PDQ is framing-only** — and a page that cites NCI for a tumor name is
  now documented as a defect. (2) **It is not "licensing-clean", which is what
  the research reports call it three times.** Its copyright page forbids
  reproducing text or illustrations in any form without written permission. The
  safe reading is also how the site already works: read for facts, write every
  sentence ourselves, cite with a URL, never copy one. §12.1 states it, beside
  the existing NCI-images and AHFS rules from PLAN.md §5. **Worth a line in
  WI-433** (the lawyer questions) if that ever gets picked up.
  Also merged **PR #68** (WI-501 + the WI-504 record) into `develop` first, so
  the block mechanism is in before anything is written on top of it.

- **2026-08-30** — **WI-504 done — Wave 1 is UNBLOCKED.** Dan fetched the
  403-blocked sources by hand into `.claude/work_files/research/sources/`
  (git-ignored; third-party documents). Got: the **NCCN Guidelines for
  Patients: Brain Cancer — Glioma 2024** (~76 pages — the blocker, and the
  model for the "questions to ask" section every P5 page ends with);
  **Kurokawa et al. 2022 "Major Changes in 2021 WHO Classification of CNS
  Tumors"**; **Johns Hopkins "Understanding My Report"**; and **The Brain
  Tumour Charity "What causes brain tumours?"**. ABTA deliberately dropped —
  pre-CNS5 naming, already barred by §4 of the shared contract from governing
  naming or grading, and NCCN covers the rest better.
  **Two corrections to my own instructions, both of which cost Dan time.**
  (1) I labelled RadioGraphics `rg.210236` "the RSNA fMRI review" and told him
  to skip it if it was paywalled. It is the **CNS5 changes paper** — the single
  best crosswalk source in the set, and exactly what WI-501's `[CROSSWALK]`
  block is for. `tests-library.md` §13 uses that wrong label for a different,
  un-URL'd article; `glioma-family.md` files it correctly. He saved it anyway.
  (2) I gave him a dead URL for the causes page and a table whose `.pdf`
  filenames read as things to download — ABTA and BTC publish no PDFs, the
  pages have to be printed to one. Fixed both in the checklist.
  **Worth knowing for Wave 1: all four extract with `pdftotext`** (already at
  `/mingw64/bin/pdftotext`) — `pdftotext -f 1 -l 20 <file> -`. The `Read` tool
  cannot open them; it needs poppler's `pdftoppm`, which is not installed here.

- **2026-08-30** — **WI-501 done — curated pages can include shared blocks, so
  the retired-name crosswalk lives in one file instead of 24.** P5 Wave 0; a
  code item, no tumor content written. A directive is a line whose entire
  content is `[BLOCK-NAME]`, resolved from `Content/blocks/` before Markdig
  parses, which is what makes block text ordinary page text everywhere
  downstream — glossary tooltips mark it, site search finds it, and ContentCheck
  grades the **composed** page rather than a fragment that can pass alone while
  the assembled page fails.
  **A block may carry `sources` front matter, which merges into every including
  page.** Without that, the block's citations get copy-pasted into 24 files and
  the drift problem this item exists to kill just moves to the source list.
  **`Content/blocks/` ships EMPTY, deliberately.** WI-513 writes the crosswalk
  together with the page that includes it — writing it now would either fire the
  orphan-block warning on `develop` from day one (training everyone to ignore
  the gate) or force a Wave 1 content edit into a Wave 0 item.
  **Review caught two blockers I would have shipped, both proven by execution.**
  (1) **CRLF silently dropped every directive.** .NET's multiline `$` does not
  match before `\r`, and `core.autocrlf=true` on this repo, so a fresh clone
  hands the parser CRLF files and `[CROSSWALK]` renders as literal bracket text
  — no exception, no missing-block failure, and ContentCheck grades the bracket
  text as prose and passes. CI is Linux/LF, so **CI would have stayed green
  while the site was wrong**, the exact inversion of the test-locally rule.
  Every test I had written used `\n`. Fixed by scanning line by line instead of
  one regex over the document. (2) **One malformed block 500'd the whole site
  and silently emptied search** — `Load` threw for any bad block, taking down
  `/about` and `/privacy`, which include nothing, and `SearchPages` swallows
  `FormatException` per file so search returned zero results site-wide with no
  error. Blocks now carry their parse errors: only pages that include a broken
  block fail, and they fail by name.
  Also from review: blocks spliced without blank lines fused into the
  neighbouring paragraph, which turns the crosswalk **table** into pipe-mangled
  prose with no error; directives expanded inside fenced code blocks; URL-less
  print sources were silently discarded on a site whose rule is "every claim
  must trace". Added a Fail for a mistyped lowercase `[crosswalk]` (it renders
  literally, so the build should say so), a composed-size cap, and a regression
  test asserting **every shipped page renders byte-identically** with blocks
  available — the general property, rather than three hand-picked strings.
  Verified end-to-end against the running app, not only in tests: composition
  from the real content root, a block edited **while the app was running**
  changing the page, the missing-block failure, and CRLF files composing
  correctly. 875 tests (842 before), ContentCheck 50/0.
  **Not fixed here: WI-435** (ContentCheck swallows `--nologo` as its positional
  pages root). Separate ticket; CI invokes with no arguments.

- **2026-08-30** — **Phase P5 filed: 57 tickets (WI-501…WI-557), tumor guides.**
  Dan's ask: for every tumor type, everything a newly diagnosed person should
  know — what it is, how it starts, what it does to the brain — plus the
  treatments and **the tests you have to get through before each one**. Five
  parallel deep-research tracks ran first; **the reports are committed at
  `docs/research/tumor-guides/`** (moved out of `.claude/work_files/`, which is
  git-ignored — a backlog referencing scratch files is useless to a future
  session). **Read `SYNTHESIS.md` before touching any P5 item.**
  **The finding that justifies the phase:** the readability gap is measured and
  published. Across 91 US brain tumor centres and 8 patient organizations, mean
  Flesch-Kincaid grade level is **11**; under 10% of centre sites reach 8th
  grade and **no patient organization does**. A library that actually holds 6.0
  would be, on the published record, the first — the same shape of uncontested
  niche as P4's daily research feed.
  **Architecture: three layers.** 24 tumor hubs (18 deepened in place, same
  URLs; 6 new), a 17-page treatment library, a 12-page tests library. Writing
  treatments into each tumor page instead would be 24× the work and 24 places
  to fix an error.
  **Dan's calls, recorded in the phase header so they are not re-litigated per
  page:** prognosis concepts explained but **no figures published** (a
  deliberate change from `/tumors`' current line, and it comes with a mechanism
  — a reader-choice gate, WI-503, because the evidence says not everyone wants
  to know); drugs named with side effects but no doses or cycle schedules;
  orienting durations IN but prescriptive ones out; procedural risk percentages
  qualitative (awake-craniotomy seizure is reported from 2.9% to 54% — the
  spread is too wide to state a number honestly); and direction-only for the two
  numbers most likely to mislead.
  **Two operational findings worth knowing before drafting.** (1) **NCI's patient
  PDQ is pre-CNS5** — it still says "anaplastic astrocytoma", "oligoastrocytoma"
  and "hemangiopericytoma", uses Roman numerals, and predates both
  ASCO-SNO-ASTRO 2022 and vorasidenib. It is the source we would naturally lean
  on hardest, so the phase encodes a source-precedence rule: NCI for framing,
  never for naming or grading. (2) **The best available source is 403-blocked to
  automated fetching** — three tracks independently named NCCN Guidelines for
  Patients: Gliomas as the best CNS5-aligned, patient-level, licensing-clean
  source, and none could reach it. **WI-504 is a `[user]` item and it blocks
  Wave 1.**
  **Wave 1 deliberately stops** after one tumor hub (WI-513, low-grade glioma —
  chosen because the short version already exists to compare against) for a
  local review, before the template is copied 23 times. Standing rule since
  2026-08-29.
  **Housekeeping done at the same time:** **WI-444** closed as absorbed into
  WI-508/509 (the pathology report and the molecular results are the same
  physical document, and CNS5 makes their integration mandatory); **WI-451**
  re-scoped, since WI-521 now owns follow-up scans and scanxiety; **WI-446**
  strengthened with the caregiver evidence and left carrying one open question
  Dan should answer *before* WI-513 sets the template — do tumor hubs each carry
  a caregiver section, or does it all live in WI-446? **WI-450** re-pointed at
  WI-507/508. And **P2c and P2d are marked superseded** — they were reserved for
  exactly this work.
  **WI-501 is load-bearing and is a code item, not writing:** curated content is
  flat Markdown with no include mechanism, so without it the retired-name
  crosswalk gets copy-pasted into 24 files, and that is the content most likely
  to need correcting later.
  **Same day, caregiver question answered — and it grew the phase to 59.** Dan:
  make the caregiver section prevalent, because surgery is followed by a lot of
  aftercare and because someone living with a person who has a tumor needs to
  know what they will have to deal with. His example was a partner with a tumor
  who has seizures. So every tumor hub carries a real caregiver section
  (contract item 11), treatment pages with meaningful aftercare carry one too
  (item 12, WI-510 heaviest — a caregiver is discharged into wound care and
  medicines with no training), **WI-558** sets the standard and the shared block,
  and **WI-559** writes "what to do when someone has a seizure". That last one is
  the find: seizures are the commonest way a brain tumor announces itself and
  the symptom people live with for years, and **the site had nothing on what to
  actually do during one.** It is the one page where a reader may be acting
  rather than reading, so format is a safety feature — scannable in a panic,
  legible on a fridge, never behind a disclosure. Both land before WI-513, since
  the template proof has to prove them. **WI-446 re-scoped** from "the only place
  caregiver content lives" to "the fuller path all those sections link into".

- **2026-08-16** — **The deploy window, measured at last — and my earlier
  reassurance about it was wrong.** WI-431's hardened smoke check caught the
  window on its first live run (the WI-412 release) and produced the evidence
  eight earlier sightings never did: **two minutes**, not one — 22:18:21 to
  22:20:19, nine failed rounds — with `/research`, `/trials`, `/search` and
  **`/get-help-now`** all 500 while `/` answered 200 throughout. The deploy
  still went green, correctly: it waited, confirmed health, then passed.
  **The bodies were empty**, which corrects what this file said on 2026-08-15.
  I recorded then that a visitor in the window still sees the calm error page
  with the helpline band and the CareLine number. That is false. An empty 500
  means the request never reaches the application, so the exception handler
  never runs and nothing of ours renders — the visitor gets the browser's own
  error screen, on the route a distressed reader is most likely to want. It also
  rules out an application exception: this is start-up or swap behaviour at the
  platform, not a bug in the code.
  **Recommendation recorded in WI-431b, and deliberately not acted on:** deploy
  at quiet hours and batch releases (free, and free of consequence while
  unlaunched), and collect two or three more measurements — the check now logs
  them automatically — before spending on Standard tier. The previous release
  had no window at all, so one sample of two minutes is not a trend, and buying
  a tier on it would be guessing with money.

- **2026-08-29** — **WI-462 — country counts follow the other filters, live.**
  Dan spotted that the numbers beside each country did not change when he
  picked a tumor type or stage. Real bug: `AvailableCountriesAsync` was a
  separate query that knew only about `includeClosed`, so the menu promised 27
  and the list delivered 4. **A count that disagrees with the list it produces
  is worse than no count.**
  He offered "hide them or update them" and preferred updating. Taken — but the
  fix was not to patch the second query. **Both the counts and the list now
  build from one shared `BuildFilter`**, so they cannot drift apart again by
  construction, and the test asserts the AGREEMENT across four filter
  combinations rather than fixed numbers. A fixed number would pass while the
  two silently diverged.
  Two calls: the counts ignore the COUNTRY selection (otherwise circular —
  picking Finland would zero every other country and you could never widen from
  the menu), and the live refresh fires on tumor type / stage / include-closed
  but **not** the country checkboxes, which would close the picker under your
  hand mid-selection. Countries apply on the button, which is also the no-JS
  path.
  Verified on the real cache: all → 311 (US 210); glioblastoma → 88 (US 64);
  glioblastoma + Phase 3 → 5 (US 4); ticking US then listed exactly 4.
  842 tests, ContentCheck 50/0. Reviewed locally before deploying.
  Also: WI-439's Kestrel flake fired 1 run in 5 here, and this time I captured
  the message instead of assuming — "The Kestrel test host did not start",
  not an axe violation.

- **2026-08-29** — **WI-458…WI-461 — a review round on trials and research, all
  reviewed locally before deploying.** Dan asked for that process change after
  four releases went out in one session on my screenshots alone: *"let me test
  locally before we deploy… you're a big fuck-up today."* Fair. Written to
  memory as a standing rule for reader-facing work.
  **WI-458** the country picker: always starts closed (it auto-opened after
  every search — my call in WI-457, and the reasoning did not survive contact
  with using it), moved beside the other filters, button below and sized to its
  words, labels above controls, "Select" then up to three countries then "...".
  **The browse form had no htmx at all**, which is why submitting jumped to the
  top; it now swaps in place, and the pager finally gets the `HxTarget` the
  partial was built to take.
  **WI-459** trials render as cards like the research feed, minus the photo.
  Found while building it: in the LOCAL dev database zero of 518 trials had a
  plain-language summary. **Corrected after deploying: on PRODUCTION 54 of 60
  do** — the fallback fires on ~10%, not 100%. I generalised one database to
  the system. The description falls back to the registry's own text, labelled
  "From the trial team:" and cut at a sentence end — never mid-sentence, since
  truncating "...did not improve survival" halfway is how a card says the
  opposite of the study.
  **WI-460** trials removed from `/research` and its filter; old
  `?kind=trial_update` links fall back to the whole feed. Home still shows them
  — asked, and Dan said fine. Research filters restacked to match trials.
  **WI-461** each of the two trial-finding methods gets its own tinted panel
  holding only heading + controls, with the results below on the page
  background. Headings now "Trials near you" / "Trials by countries"; button
  "Search trials".
  **Two corrections to my own work this round, both from measuring instead of
  trusting myself.** I "fixed" the scroll jump with `show:none` and wrote a
  comment claiming that was the fix — it was not; **my Playwright test was
  causing the jump** by scrolling to an element before clicking it. And I
  thought the country control looked washed out beside the selects; computed
  styles said identical. I was looking at a stale screenshot.
  **And a real bug I had twice waved away as "intermittent".**
  `TheAdminHealthPageShowsTheCounts` seeded a `/seen-<guid>` row with 42 views
  and never cleaned up. The admin page lists the **top 12** by
  `views DESC, path` — after 45 accumulated runs there were 45 rows all tied at
  42, so the tiebreaker was the random GUID and whether this run's row made the
  cut was a coin flip that got worse every run. It read as a mystery flake; it
  was the test poisoning its own database. Purged, cleanup added at both ends,
  and a second littering test fixed too. **Seven consecutive green suites, zero
  rows leaked.** The lesson: "passes in isolation" is not a diagnosis.
  840 tests, ContentCheck 50/0.

- **2026-08-29** — **WI-457 done — multi-select countries, and a signpost from
  research to trials.** Dan after using WI-455: he wants China AND Japan AND a
  few others, and wants readers on `/research` told that trials exist.
  Checkbox group in a `<details>` rather than a JS widget or `<select multiple>`
  — the site works with JavaScript off, and a native multi-select is ctrl-click
  on desktop and a cramped scroller on a phone. Same disclosure pattern as the
  mobile nav. Collapsed by default (44 countries would push the trials off
  screen), auto-opened when a filter is active so a shared link explains itself.
  Ordered by trial count. URL key unchanged, so single-country links from
  WI-455 still work — pinned by a test.
  Several countries mean EITHER, not both: a trial has to run somewhere the
  reader can reach, not everywhere they ticked. Verified on the real cache —
  China+Japan returns 38 where the two alone are 32 and 10, so overlapping
  trials are counted once rather than duplicated.
  The `/research` signpost sits ABOVE the filters, not below the feed: nobody
  should read twenty cards before finding out they are on the wrong page for
  "can I join a trial". A test pins that ordering.
  **A test-hygiene bug of mine worth remembering:** the new tests used NCT7778
  IDs while `CleanupAsync` only deletes `NCT7777%`, so rows leaked into the TEST
  database and collided on the second run. Renumbered into the cleaned range.
  Note the tests run against `brainharbor_test`, not the dev `brainharbor` DB —
  I cleaned the wrong database first.
  837 tests, ContentCheck 50/0.

- **2026-08-29** — **WI-455 done — filter trials by country.** 41 countries with
  trial counts, built from the cache so the menu can never offer a dead choice.
  Matches ANY of a trial's sites.
  **The multi-country trap was real, not theoretical.** Of the 20 trials under
  Germany in the live cache, **17 run in more than one country** — the first is
  a 14-country study. Matching only the first location would have hidden or
  mislabelled most of them. Cards say "Runs in N countries" rather than
  implying the filtered one.
  Counts are trials not sites (40 US hospitals is one US trial) and honour the
  closed filter, so a count of 12 never leads to a list of 3. Non-US readers are
  now told the ZIP box is US-only and pointed at the country filter, with the
  honest caveat that it gives no distances — the first time this site has been
  usable outside the US. GIN index (`jsonb_path_ops`) on `locations`, migration
  0009. Verified against the real 8,900-trial cache, not just seeds: menu count
  and filter result agree exactly, 0.02s filtered. 830 tests, ContentCheck 50/0.
  **A bug in a comment I wrote, worth recording because it was about privacy.**
  `PageUrl` claimed it deliberately strips the reader's ZIP from pager links. It
  does not — it is built from `FilterUrl`, which appends one. Corrected to
  describe reality, and the real question named: the handler sets
  `no-store`/`no-referrer` because the URL carries a location, but nothing stops
  a reader SHARING that URL. Not changed, just no longer misdescribed.

- **2026-08-26** — **WI-455 filed: filter trials by country. This is next.**
  Dan asked for country filtering on research AND trials. Checked the data
  first, and the two halves are not the same job:
  - **Trials: already there.** `trials_cache.locations` is jsonb with
    `{facility, city, state, country, lat, lon}` per site — verified against
    real rows. Nothing new to fetch; a filter, a control and an index.
  - **Research: no location data anywhere.** `FetchedItem` carries no
    affiliation at any layer, so it would mean parsing PubMed's
    `AffiliationInfo`, inferring a country from free text, a new column and a
    backfill of 1,000+ items — for a weak payoff, since a paper's country is
    where the authors work, not whether the finding applies to the reader.
  **Dan's call once he saw the cost: trials only.** The research half is
  recorded as decided-against under WI-455 rather than left as an open ticket,
  so it does not get re-proposed.
  Two traps written into the ticket: a trial with sites in eight countries is
  not "a US trial" (match ANY location, and say how many countries it runs in),
  and this must not become "trials near you" for non-US readers — the ZIP box is
  US-only ZCTA centroids, so a reader in Germany filtering to Germany still gets
  no distance and the page must not imply otherwise.

- **2026-08-26** — **Phase P4 filed: 13 new tickets (WI-442…WI-454)** from Dan's
  ask for ways to improve or expand the site, after a web survey of comparable
  sites.
  **The finding that shaped the whole phase: nobody else runs a daily
  plain-language brain tumor research feed.** ABTA, National Brain Tumor Society
  and The Brain Tumour Charity all offer CareLines, support groups, mentor
  matching, financial aid and conferences — real services, none of them this
  one. So the strategy recorded is DEEPER for this audience, not broader across
  cancers.
  Also worth carrying: a 2026 blinded randomised trial found ChatGPT-4o wrote
  Cochrane plain-language summaries at least as well as human researchers
  (PMC12302524) — citable on `/how-we-write`, and it turns "AI writes these"
  from an apology into a claim with evidence behind it. And forum research
  (PMC12119380) documents that CAREGIVERS carry high unmet needs, which is where
  WI-446 comes from.
  **Found while surveying, not asked for: the reader-report loop is broken at
  the last step (WI-442).** A reader types why a summary is wrong,
  `ReportProblemAsync` stores it in `review_events.note`, the item is flagged —
  and the queue never displays the note. On an Auto-publish medical site the
  reader may be the only human who compared the summary against the source, and
  their message goes into a table nobody reads. Hours to fix; my pick for next.
  **WI-443 splits the database-restore rehearsal out of WI-407's checklist**
  because it is the most dangerous line in the backlog: 1,000+ published
  summaries, the audit trail and the view counts, on a shared PG17 server, and a
  restore has never once been performed.
  WI-454 parks the general-cancer sister site WITH its measurements, so the
  research is not redone: all sources already cover every cancer (~4 query
  strings), but volume is 8,331 PubMed records/month vs 470 — ~18× — and the
  space is crowded by NCI, ACS and Cancer Research UK. Better shape if revisited
  is another underserved SPECIFIC cancer.
  Not duplicated as new tickets, since already tracked: **WI-406** (retraction
  check), **WI-434** (glossary), **WI-404/405** (digest).

- **2026-08-23** — **WI-441 — page-open counts, admin-only.** Dan asked whether
  visitors were being tracked. They were not: no analytics package, no App
  Insights linked to the app, only 3 days of App Service HTTP logs nothing
  read. Azure's `Requests` metric is free and already there (93-day retention,
  Portal → App Service → Metrics) but counts every stylesheet, bot and
  smoke-check poll — load, not readers.
  **The constraint:** `/privacy` promised "we do not run analytics and we do
  not build a profile of you", live. And the metric Dan actually asked for —
  new vs repeat — REQUIRES persisting an identifier across days; there is no
  privacy-preserving trick around it, which is why Plausible and Umami
  deliberately cannot do it either. His call: give that up rather than weaken
  the promise.
  Built `page_views (viewed_on, path, views)` and nothing else — no IP, no user
  agent, no session id, no referrer, no sub-day timestamp. Buffered in memory,
  flushed every 60s so a page never waits on a write. Assets, htmx fragments,
  bots, curl, the smoke check, `/admin`, `/api`, redirects and errors are all
  excluded, so the number means something. **The query string is dropped before
  storage** — on `/trials` it can carry a reader's ZIP.
  Privacy copy rewritten and made stronger rather than merely accurate: the new
  bullet states the counts cannot tell a new reader from a returning one and
  that this was chosen. Verified end-to-end on a running site (4 real loads + 5
  pieces of noise → exactly `/`=2, `/research`=1, `/trials`=1), not only in
  tests. 821 tests, ContentCheck 50/0.

- **2026-08-23** — **WI-440 — the site works on a phone now.** Dan asked for a
  hamburger menu on the home page and "things like that", then for the rest of
  the pages. Measured first: no horizontal overflow anywhere and no text under
  the 16px floor, so the body content was already sound — the damage was all in
  chrome and forms. A reader on a 390px phone scrolled past **1107px** before
  reaching `<main>`, more than a full viewport, on every page. Now ~890px, with
  the header down from 245px to 137px.
  Hamburger via `<details>`/`<summary>` (no JavaScript, per the standing
  constraint), icon-only masthead on phones, **"Get Help Now" deliberately kept
  OUTSIDE the menu** at every width — PLAN.md §3 is "always one tap to a human"
  and a disclosure makes it two. That is pinned by a test. Filter forms now
  stack as label-above-control grids; on `/research` the wrapped row had been
  separating every label from its control, which is worse than no label at all.
  Checkboxes went 13px → 22px inside a 44px row.
  **Gap closed in the gate:** every axe scan ran at desktop width, where the
  hamburger is `display: none` — so the whole phone layout, including a menu
  panel that did not previously exist, was outside the accessibility gate.
  Scans now run at 390px, menu closed and open.
  **Three bugs hit, none visible to any test:** a tap-target regression I
  introduced myself (40.5px band links, chasing four pixels); a specificity
  fault where `.site-name img` (0-1-1) outranked `.site-name__lockup` (0-1-0)
  so both logos rendered; and `display: contents` on `<details>` laying the
  desktop nav out 0px wide. The last was fixed by restructuring rather than
  patching — the nav is now a sibling of the toggle. **That is four sessions
  running where the screenshot found what the suite could not.**
  Desktop unchanged; all rules scoped to `max-width: 40rem` except checkbox
  sizing, which was too small everywhere. 806 tests, ContentCheck 50/0.

- **2026-08-22** — **The WI-403 Kestrel flake cost a production deploy; filed as
  WI-439.** The WI-438 release merged to `main`, `build-test` went red on
  `A11ySmokeTests` failing to start its host, and because `build-test` gates the
  `deploy` job **the deploy never ran** — the merge looked done and production
  stayed on the old build with pagination still broken. A re-run of the same
  commit passed and deployed. Not an axe failure and nothing to do with the
  pagination work; it passed on the feature PR and on `develop` first.
  **Lead worth keeping:** the failing test took **1 ms** in CI. A real host
  start takes seconds, so nothing was attempted — `CreateClient()` handed back
  an already-broken cached host. The factory poisons itself after one failure,
  which means a retry loop around `EnsureServer()` would spin on the same dead
  host and fix nothing; a retry has to dispose and rebuild. That is in the
  ticket, because it is the kind of thing that gets "fixed" wrongly in ten
  minutes otherwise.
  Also: **my deploy-window poller missed the window a second time** (8s single
  route vs a ~75s event). Read the smoke-check log for those numbers, not the
  watcher. This release is NOT counted as a fourth measurement — there is no
  clean reading for it.

- **2026-08-21** — **WI-438 — pagination never worked, on `/research` OR
  `/trials`.** Dan: "When I click Show More, it sticks on page 1." Reproduced,
  and it was broader than reported: `?page=N` was ignored on both pages
  (`/trials` served the same first rows on pages 1, 2 and 3 of 16).
  **Root cause: `page` is a RESERVED route-value key in Razor Pages.** Routing
  stores the page path in it, so a handler parameter of that name binds the
  ambient ROUTE value, fails to parse as an `int`, and falls back to the default
  — silently, no exception, no warning. Every other query parameter on the same
  handler bound fine, which is exactly why it read as a paging bug rather than a
  binding one. Fixed with `[FromQuery(Name = "page")]` plus a C# parameter
  deliberately not named `page`.
  **Why no test caught it:** the existing test set the model's state directly
  and asserted the "Show more" URL contained `page=2`. It proved the link was
  BUILT right and never followed it. The replacement requests page 2 for real
  and asserts a disjoint set of items — the only shape that can catch this.
  Replaced "Show more" with a real pager (Back · 1 … 6 7 [8] 9 10 … 16 · Next,
  "Page 8 of 16"), shared by both pages, unit-tested window logic, 1-based URLs,
  out-of-range clamped to the last real page, htmx opt-in so `/trials` can share
  it and just navigate.
  **A third defect, found only by screenshot:** a duplicate `.pager` rule ~600
  lines further down site.css overrode the new component and squeezed the status
  line into a three-line column. Tests green throughout. **That is now three
  times running that the picture caught what the suite could not** (WI-412a's
  orphan page, WI-437's phone-width flip, this). Screenshot anything visual
  before calling it done.
  Scope note: Dan asked about `/research`; `/trials` was fixed too because the
  same defect was proven live there. `/admin/queue` left alone (internal).
  804 tests, ContentCheck 50/0.

- **2026-08-19** — **WI-437 — the journey path replaces BOTH the readiness dial
  and the stage badge.** New design handoff from Dan
  (`design_handoff_brainharbor_journey`). Cards and the item page used to carry
  a 4-mark badge ("how well tested") and a 1-to-10 dial ("how close to a
  patient") in different units; now one four-stage path — Lab cells → Animals →
  Review → Tested in people. **This also closes WI-429.**
  The handoff worried about mapping a 0-10 score onto four stages; that did not
  apply, because `ResearchStage` already WAS the four stages, so the component
  is driven straight off the enum and a content author can never type a number.
  **Dan's calls:** replace both indicators; keep the card photos; re-point the
  `?sort=readiness` sort at the stage ladder ("Furthest along") rather than drop
  it; and — after seeing it live — lay the path OVER the photo where the dial
  sat, prominent, not as a row of dots underneath.
  **Two defects neither markup nor tests could show:**
  (1) *in the handoff itself* — `role="img"` on the `<ol>` orphans every `<li>`;
  axe called it SERIOUS across 28 nodes, and this renders on every card on every
  page, so it would have shipped site-wide. Fixed with `role="presentation"`.
  **Worth telling the designer.**
  (2) *mine* — at phone width the overlaid path flipped vertical and swallowed
  the whole photo; my override tied on specificity and lost on source order.
  Every test was green and the structure was correct; only a screenshot showed
  it. **Same lesson WI-412a taught in a different costume: a test that checks
  structure cannot see what a page looks like.** Screenshot anything visual.
  Contrast computed by hand rather than trusted to axe (which cannot reason
  about a translucent plate over an arbitrary photo): 11.8:1 to 15.1:1 for the
  current label, 9.2:1 to 11.4:1 for the rest — AAA either way.
  **`readiness_score` is untouched** in the DB, pipeline, sync contract and
  review queue, so this is reversible without a migration; two tests guard
  against the number reappearing on a reader page. The stage badge survives for
  the admin queue and style guide only. 781 tests, ContentCheck 49/0.

- **2026-08-19** — **Deploy window, third measurement: 84 seconds** (WI-436
  release, PR #49). 20:49:10 → 20:50:34 by an independent poll of `/start`; the
  smoke check agrees (six failing rounds, first clean at 20:50:40, second
  consecutive at 20:50:52). Same signature a third time: inner pages 500 while
  `/` stays up.
  **Series: 2m00s, 1m23s, 1m24s. WI-431b's precondition is now met** — three
  measurements, and the last two are within a second of each other. The shape is
  settled: roughly 85 seconds of 500s on every page except `/`, every deploy.
  **This is Dan's call now, and it is a real one:** ~85s of hard 500s on
  `/get-help-now` is the part that matters, because that is the page a
  distressed reader is pointed to from the band on every page and from `/start`.
  While unlaunched it costs nothing. At soft launch (WI-408) it is a defect.
  Options unchanged: deploy at quiet hours + batch releases (free), or Standard
  tier for slot swaps (money, removes it).
  **Also this release: CI hung for 35 minutes** on `Install Playwright browsers`
  (`playwright.ps1 install --with-deps chromium` — the `--with-deps` half shells
  out to apt-get). Cancelled and re-ran; both passed. First occurrence, so not
  filed yet, but note the step has no `timeout-minutes`, so a hang there blocks
  until the job's 6-hour default and a stalled pipeline looks exactly like a slow
  one. If it recurs, that is the fix.

- **2026-08-19** — **WI-436 — /start rewritten from Dan's copy.** Dan supplied
  new text for the Just Diagnosed page; implemented at reading grade 4.2 (gate
  is 6.0). `/start` also added to `sitemap.xml` — it was reachable from the home
  page door but invisible to search engines, the WI-412a orphan pointed the
  other way, on the page a newly diagnosed person is most likely to search for.
  **Four deviations from the supplied copy, all told to Dan:** the closing "not
  medical advice" paragraph was left out (it is verbatim what
  `disclaimers: [medical]` already renders — it would have printed twice); the
  CareLine's weekday hours were KEPT although his copy dropped them (Mon–Fri
  8:30–5:00 CT verified — without them a reader calls at 9pm Saturday and gets
  silence); the "find the nearest emergency room" link had no target in the copy
  so it points at a Google Maps search; and his opening paragraph became the
  `description` front matter, which renders as the lead under the H1.
  **New ticket WI-435, and a correction to my own reporting:** ContentCheck
  takes the pages root as a positional argument, so `--nologo` was swallowed as
  that path — the tool then printed `WARN pages root MISSING`, graded ZERO
  Markdown pages, and exited 0 saying "passed (22 checks, 0 failures)". I had
  quoted that partial number as verification on PR #46. CI invokes it correctly
  (48 checks) so nothing shipped ungated, but a gate that reports success after
  checking nothing manufactures confidence, which is worse than no gate on the
  check standing between an AI-drafted tumor page and a frightened reader.
  Also noted for later: `FrontMatter.Description` renders visibly but is not
  wired to `<meta name="description">`, so every curated page shares the
  site-wide default.

- **2026-08-16** — **Deploy window, second measurement: 83 seconds** (WI-412a
  release, PR #47). 23:54:01 → 23:55:24 by an independent poll of `/tumors`
  running alongside the workflow; the smoke check agrees (six failing rounds,
  last 500 at 23:55:11, first clean at 23:55:26, then the second consecutive
  clean pass at 23:55:38 → "site healthy after deploy"). Same signature as the
  first: `/research`, `/trials`, `/search`, `/get-help-now` all 500 while `/`
  stayed up, and **the bodies were empty again** — the request is not reaching
  the app, so this is platform start-up/swap behaviour, confirmed twice now and
  not an application exception.
  **Series so far: 2m00s, 1m23s.** Two samples, same shape, roughly a minute and
  a half. That is enough to call it a consistent behaviour but WI-431b's advice
  stands unchanged — one more measurement before spending on Standard tier, and
  the cheap mitigations (deploy at quiet hours, batch releases) cost nothing
  while the site is unlaunched.
  Worth noting the check earned its keep on its second run: it measured the
  window rather than passing straight through it, which is exactly what WI-431
  was for.

- **2026-08-16** — **WI-412a — /tumors shipped ORPHANED; navigation added.**
  Dan: "There's no navigation to get to the tumors page." He was right, and the
  gap was wider than the nav: no header link, no footer link, `/tumors` missing
  from `sitemap.xml`, and no "What is this?" link from the `/research` tumor
  filter — which was WI-412's own acceptance criterion. The page worked
  perfectly and nothing on the site pointed at it, so only a typed URL reached
  it. Fixed all four. **Why the tests missed it:** the existing link check walks
  the links that exist and proves they do not 404 — it is structurally blind to
  a page no link points at. The new test asserts the general property instead of
  this one instance: every path `sitemap.xml` advertises must be reachable from
  the home page. **Second bug, caught only by looking at the rendered page:** the
  "What is this?" link first shipped as a multi-line Razor implicit expression,
  which terminates at the newline — it rendered "What is
  System.Collections.Generic.List`1[BrainHarbor.Web.Content.TumorType]" while the
  test, which asserted only the `href`, stayed green. The test now asserts the
  whole anchor including its visible text. Lesson worth keeping: a test that
  checks the attribute a human never reads will not notice the text a human
  only ever reads.

- **2026-08-16** — **WI-412 done — /tumors, "what is this?" for 18 of 24 types.**
  Dan asked for all of it in one go, shipped live, and said he would review it
  there. The index is built from `taxonomy.yml` — the same file the research
  filter reads — so the list and the descriptions cannot drift apart.
  **Deviation, deliberate:** each type gets its own page (`/tumors/glioblastoma`)
  rather than only an anchor on one page. People arrive from a search engine
  having just been handed a word by a doctor; a page about their diagnosis beats
  an anchor part-way down a list. Anchors still exist for `/research` to link.
  Descriptions live in `Content/pages/tumors/*.md`, NOT in `taxonomy.yml` —
  that file is rendered into the classifier prompt on every call, so prose there
  would cost tokens per item and escape the reading gate. As curated pages they
  inherit the 6.0 CI gate, glossary tooltips, the medical disclaimer, `sources`
  front matter and site search.
  **Content discipline, because this is the riskiest writing on the site:**
  descriptions only. No survival figures, no prognosis, no treatment
  recommendations — those vary per person and are where a wrong word does real
  harm. WHO CNS5 naming held throughout (grade 4 ≠ glioblastoma; DIPG is the
  pontine subset; spinal cord tumor is not a brain tumor and has its own
  heading, pinned by a test). Every page ends with questions to ask a care team.
  Grades 2.4-5.4. 777 tests.
  Fixed on the way: `ShellPagesTests` pinned the TOTAL graded-page count at 6,
  so it failed the moment curated content was added even though every page
  passed — now it names the six shell pages and asserts every `.md` under the
  pages root is graded, so a new folder cannot escape the gate.
  **Two things for Dan:** six rare types still say "We are still writing this
  one" (honest, and they link to the feed for that type), and these pages were
  drafted by AI yet read as the site's own writing — `/how-we-write` covers the
  FEED pipeline and says nothing about curated pages. Worth a decision.

- **2026-08-15** — **WI-431 done — the deploy smoke check now asks whether the
  SITE is healthy, not whether something answered.** Eight deploys today each
  served 500s on `/research`, `/get-help-now`, `/trials` and `/search` for about
  a minute while `/` returned 200 throughout — and the check went green every
  time, because it hit only `/`, on the azurewebsites hostname, and exited on the
  FIRST 200 (which during App Service's old/new overlap can come from the
  instance being replaced). Now: all five main routes, on `brainharbor.org`, and
  **two consecutive clean passes** so one lucky hit cannot pass it.
  **It also prints the failing response body**, which matters as much as the
  gating: eight sightings produced no evidence of the CAUSE because nobody ever
  captured one. The next occurrence will. Until then, "it is the app restarting"
  is inference, not diagnosis — do not treat it as known.
  Both paths verified locally against the live site before shipping (healthy →
  exit 0 after two passes; bad host → body captured, counter reset, exit 1), and
  the release that carries this change is itself the first real exercise of it.
  **CORRECTED 2026-08-16 — this was wrong.** The entry originally said a visitor
  caught in the window still gets the calm custom error page with the helpline
  band and CareLine number on screen. The first real capture shows the 500
  bodies are EMPTY: the request never reaches the application, so ASP.NET's
  exception handler never runs and no BrainHarbor page renders. The visitor sees
  the browser's own error screen. See WI-431b for the measurement.
  Filed **WI-431b** (`[user]`): whether to buy Standard tier for slot swaps —
  explicitly NOT to be decided until the body capture identifies the cause,
  since a start-up or migration-lock cause may be free to fix in code.

- **2026-08-15** — **WI-432 done — /terms was still claiming a human reads every
  summary.** Dan asked whether the site needs legal cover; looking for it turned
  up a real defect rather than a missing paragraph. `/terms` said "We check each
  summary before it goes up", which reads as human review. Publishing mode is
  Auto and nobody reads each one; every other page was scrubbed of that
  implication on 2026-07-31 and this page was missed — the one page most likely
  to be read as a promise. Now: AI writes them, they must pass automatic checks,
  and a person does not read every one.
  Also added: the site says plainly it is for learning and for asking a care
  team better questions, and a "We are not part of these groups" section for
  ABTA, NCI and ClinicalTrials.gov, since the site points at all three and none
  of them checked what we wrote. Grade 4.3, 773 tests.
  **Deliberately NOT written** (Dan: "don't include anything you're not sure
  about"): warranty disclaimer, limitation of liability, governing law, and the
  operating-entity question. Filed as **WI-433**, a `[user]` item, with the
  context a lawyer would need. Also recorded there: publicly accessible is not
  public domain — PubMed abstracts often carry publisher copyright, which is why
  the pipeline summarizes and links and keeps raw abstracts admin-only.

- **2026-08-15** — **WI-429 + WI-428 done — the homepage card gets its readiness
  dial back, and the item page finishes the handoff.**
  **WI-429 is Dan's call and it reverses the handoff.** That design specified a
  plain card (badge, title, hook, meta) with no photo and no readiness dial.
  Dan, after seeing both live: the `/research` card is better and the homepage
  should match it. The dial is the reason — the badge says how well TESTED a
  finding is, the dial says how close it is to something a patient can actually
  get, and the homepage was answering only the first question. The `PlainCard`
  flag is gone; ONE card renders on both pages, so they cannot drift again.
  Recorded as deviation 3 in `docs/design/README.md`.
  **WI-428 turned out to be nearly done already.** Every item-page style the new
  handoff specifies (`.means-block` + head/icon, `.original-title`, `.term`,
  `.provenance`, `.ai-note`) already matched value for value — the page was
  built from the previous handoff and this one barely changed it. The one real
  gap: the badge now repeats under "How early is this?", beside the words that
  explain it, because by then the reader has passed the whole summary and
  scrolling back up to count marks is how you lose someone. Kept against the
  handoff: the readiness callout (Dan wants readiness more prominent, not less)
  and "What this means, and what it doesn't" (the handoff writes that heading
  with an em dash, which the site's own copy rule forbids). 773 tests.

- **2026-08-15** — **WI-430 done — a suggestions address, with a steer attached.**
  Dan wants `support@brainharbor.org` on the site before he shares it publicly,
  for ideas on making the site better rather than for support. It now appears in
  three places: a box at the foot of the home page, a line in the footer of
  every page, and a section on `/about`; `/privacy` says what happens to an
  email that arrives.
  **The safety-relevant part is the copy, not the address.** The mailbox is
  called support@ but is not a support line, and "support" is precisely what a
  frightened reader would write to at 2am expecting help — then wait days for an
  answer. So every prominent placement states that the inbox is not a way to get
  medical or urgent help and prints the ABTA CareLine number inline, not as a
  link away. A test pins that steer, not just the address, because the address
  without the steer is the part that could hurt someone.
  Reading grades: home 3.3, about 3.4, privacy 3.8. 773 tests.

- **2026-08-15** — **Homepage redesign live — "Harbor Banner"** (WI-428a; Dan
  brought a finished handoff back from Claude Design, reviewed it locally and
  approved). The shape is the point: a hero paragraph, three doors and a
  full-width AI panel used to stack up and push the first real update about a
  screen and a half down. Now one navy hero band (lockup as the `h1`, watermark,
  wave edge), **two** doors instead of three ("browse all research" cut, because
  the feed underneath already does that job; the crisis door stays), then
  updates — **8 cards**, not 4 — with "See all" as a filled button in the
  section head and a large outlined one beneath. **Absorbs WI-425.**
  **Evidence badge is 4 marks now, on a ladder with no gaps.** The old one ran
  5, 4, 2, 1 with nothing at 3, so the step from "review of existing research"
  to "animal study" was twice the step anywhere else for no reason a reader
  could infer; a test now fails if a gap reappears. Ten steps (the readiness
  dial this replaces on the feed) is a finer distinction than anyone can feel,
  and a fraction invites "my treatment is 60% done".
  **Two deliberate deviations from the handoff, both safety copy, both pinned
  by tests.** The handoff gives the hero no copy at all and drops "a person does
  not check every one" from the notice. Kept both: the AI admission leads in the
  band (a reader must not get through eight summaries before learning who wrote
  them) and the non-review sentence went into the bottom notice, because "checks
  run before it publishes" does not tell anyone that no person read theirs.
  New `Banner` layout section so a full-bleed band can sit before `main`.
  Reading grade **3.7** (was 4.1), axe clean, 772 tests.
  **Left deliberately unfinished:** the research item page restyle (**WI-428**)
  — the badge change already reaches it, so it is consistent, just not restyled.
  And the homepage's plain card (no photo, no readiness dial) is NOT yet applied
  to `/research`, which still has both plus a "Most ready to use" sort that
  would otherwise sort by an invisible number (**WI-429** — Dan's call).
  `docs/design/README.md` records which handoff governs what.

- **2026-08-14** — **WI-427 done — negation never worked for contractions, and
  that was the bigger half of the queue.** Dan shipped WI-426, reloaded, and
  still saw hype flags. Cause: negation was a word LIST matched against
  `[A-Za-z]+` tokens, which strip the apostrophe — so "doesn't" became
  "doesn" + "t" and the list's `doesn't` / `isn't` / `n't` entries could never
  match anything at all. **Every contraction read as un-negated**, and the block
  those sentences live in is CALLED "what this doesn't mean", so it is about the
  commonest phrasing in the corpus. It hit `cure` as well, which means this had
  been silently mis-flagging since WI-401 — WI-426 fixed which phrases got the
  negation test, this fixed whether the test ever worked.
  Measured before: `doesn't`, `isn't`, `won't`, `can't` all flagged. After: all
  clean, while "this is an **important** breakthrough" and "Doctors call this a
  breakthrough" stay flagged, which is correct.
  Fixed with a negation REGEX rather than a token list, since the apostrophe is
  part of the word; straight and curly both accepted. **The `n't` branch
  requires the apostrophe** — a bare `\w+nt` matches "importa-n-t" and would
  have quietly excused hype sitting right after it. Both directions pinned by
  tests. 771 tests.

- **2026-08-14** — **WI-426 done — the hype check was flagging summaries for
  DENYING hype.** Dan found it within minutes of WI-418 making the reasons
  visible, which is the best argument for WI-418 there is. The negation
  exemption had been wired to "cure" alone (WI-401); every other banned phrase
  was a bare keyword match. So "this is not a breakthrough" and "this is not a
  game-changer" were flagged AS hype — and the block they live in, "what this
  doesn't mean", exists precisely to write sentences like that. **The guardrail
  was punishing summaries for obeying the anti-hype rule**, holding them out of
  Auto publish and filling the review queue. Verified live before fixing: 5 of
  6 negated samples flagged, only "cure" clean.
  Fixed by applying the existing sentence-scoped negation check to every phrase.
  A real "this IS a breakthrough" is still caught, and a denial in one sentence
  does not license a claim in the next or in the next block.
  **Plus bulk approve in the queue** (Dan asked to "approve all ~137"). It
  approves every pending item that no check flags — exactly what Auto mode
  publishes by itself. It deliberately does NOT approve everything: an item
  flagged for an untraceable number stays for a person (that is the site's
  central factual promise, and where a model may have invented a survival
  figure), and so does an item with no summary (approving it publishes an empty
  page to a patient — the ~20 classify failures are exactly this). The audit row
  records who clicked and that it was a bulk action.
  **Note: I could not do the approving myself** — prod Postgres refuses
  connections from here (firewall admits Azure services only) and the admin UI
  needs Dan's TOTP. The button puts the action behind his login, which is where
  it belongs anyway. 761 tests.

- **2026-08-14** — **WI-418 done — the review queue says WHICH check flagged an
  item** (Dan's ask: he opened the queue, found 137 items marked "read this one
  closely", and could not tell why any of them was there).
  **Solved by re-checking, not by a migration.** The checks are pure text
  analysis and every summary is already stored, so the reason was *recoverable*:
  the queue re-runs them over the stored blocks at render time. That means the
  whole existing backlog is explained the moment this deploys — a
  store-it-going-forward design would have left those 137 exactly as opaque as
  they were.
  `Guardrails` moved out of the Pipeline into a new shared **`BrainHarbor.Safety`**
  project both apps reference. Copying the rules into the site was the obvious
  shortcut and the wrong one: a second implementation of the same rule is
  precisely the defect WI-415 spent a day fixing and WI-416 still exists to
  finish. `SummaryText` now owns the block assembly for the same reason — a
  title has no full stop, and joining it into the hook is what inflated every
  reading grade by ~0.7.
  The re-check **joins `trials_cache`**: the summarize-trial prompt scores
  readiness BY phase, so "Phase 2" legitimately appears in a trial summary and
  without the join every trial would report its own phase as an invented number
  and send Dan chasing a ghost.
  **Two limits stated in the UI rather than hidden:** it reflects TODAY's rules
  (the reading ceiling moved 8.5 → 7.0 on 2026-08-13, so an item flagged then
  may pass now), and a reader-reported item has no automated reason — that case
  now says so out loud instead of rendering an empty box.
  Verified through the repository against the real SQL including the new join,
  plus unit tests over invented numbers, hype, the trial phase, the
  no-summary case, and that the queue assembles blocks identically to the
  pipeline. **Not verified in the browser**: `Admin:Email`/`Admin:Password` were
  never set in this machine's Web user-secrets, so the LOCAL admin area has
  never been reachable here (prod is unaffected). Follow-up **WI-424** filed for
  recording the reason at flag time — the queue answers "what fails today", not
  "what failed that night". 750 tests.

- **2026-08-14** — **WI-422 done — the home page says plainly that AI can be
  wrong** (Dan's ask; reviewed locally and approved). An `.ai-caution` block
  between the hub and "Latest updates": *"AI can make mistakes. AI writes every
  summary here. Our safety checks catch many mistakes, but they miss some.
  Always read the study we link to, and talk with your care team before you act
  on what you read."* The hub already named AI as the writer and said a person
  does not check every one; what was missing was the admission that the writer
  is FALLIBLE, plus what to do about it — an admission with no action is not
  much use. **Above the feed, never below it** — a caution a reader meets after
  the summaries has already failed — and a test pins that ordering along with
  the sentence and both actions. Styled with the palette's existing attention
  treatment (`--color-notice*`, the closed-trial fill), NOT a red alarm: the
  palette's own rule is no reds outside true warnings, and this audience is
  frightened enough. Larger than body text, lead sentence on its own line,
  meaning carried by the words so it survives high-contrast mode, a failed
  stylesheet and print. **Measured grade 4.1 by ContentCheck** — which is a
  real gate, not a courtesy: Razor pages have been failed above 6.0 since
  WI-414. Checked at desktop and 390px. 742 tests.
  **Two stale records fixed while here:** WI-414's backlog checkbox was never
  ticked though it shipped in PR #19, and the note above claiming ContentCheck
  does not gate the home page was wrong from that same date.

- **2026-08-14** — **WI-421 done — the brand owns the top-right of the home
  hero** (Dan's ask, with a screenshot; he reviewed the rendered result and
  approved the size). `.hub` is now a two-column grid: copy left,
  `lockup-no-tagline.svg` in a 24rem column top-right, three doors spanning
  both columns beneath. **The no-tagline lockup deliberately** —
  `lockup-horizontal.svg` carries the tagline "Real brain tumor research, in
  plain language." *as artwork*, which is word for word the h1 next to it, so
  the full lockup would print that sentence twice side by side. `alt=""` +
  `aria-hidden`: a decorative repeat, per the brand README, since the header
  already announces the name. Sized in rem so it grows with large-text mode;
  hidden in print (the masthead already identifies the page). **Collapses to
  one column below 60rem and the logo disappears** — note that media-query
  `rem` is the browser's 16px, NOT the site's 18px base, so that is 960px —
  because the "talk to someone now" door must not be pushed down a phone
  screen for a logo the header shows two inches above. Accepted trade: the h1
  wraps to two lines above the breakpoint. Verified at 1440/1100/900 px and in
  large-text mode. 741 tests, axe clean.
  **Session note:** the tool-approval queue went unavailable mid-item, so this
  sat written-but-unbuilt for a while; PROGRESS carried the exact resume steps
  until it cleared.

- **2026-08-14** — **WI-419 done — the site wears the real logo** (Dan's ask;
  he supplied a finished logo kit). Header lockup (`lockup-no-tagline.svg`,
  `alt="Brain Harbor"` — the kit is explicit that the alt is the NAME, never
  "logo"), favicon SVG + 32px PNG, apple-touch icon, PWA manifest at the web
  root, `theme-color`, and **`og:image`, which the site did not have at all** —
  every shared link was unfurling as a grey box. Absolute URL, because
  `og:image` silently ignores a relative one. Logo height in **rem** so it
  rides the large-text scale (checked in both modes); print sized in points so
  a printed page gets a masthead, not a banner. The kit's teal is byte-identical
  to the existing `--color-accent` (#0d6a86), so nothing needed recoloring.
  **Verified the kit's opacity claim rather than trusting it**: iOS paints alpha
  in an apple-touch-icon BLACK, the files are colour-type RGBA (so the channel
  proves nothing), and a wrong icon is invisible until it reaches a phone home
  screen — so the test decodes the top scanline and asserts every pixel is
  opaque. One existing test broke legitimately:
  `MarkdownLinksAndImagesInSummaryBlocksAreNeutralized` asserted the WHOLE page
  had no `<img>` as a proxy for "injected markdown did not render"; the header
  logo is now a legitimate one, so it is scoped to `<main>` and additionally
  proves the payload survives as inert TEXT (a reviewer has to see what the
  model produced) without ever becoming a `src`. **NOTE: the download folder
  also holds an UPDATED full design bundle** (README/index/research-item/CSS all
  differ from `docs/design/entry-hub-handoff/`) — deliberately NOT applied; it
  is a separate, larger piece of work. Filed **WI-420**: the wordmark says
  "Brain Harbor" and the site title/og:site_name/RSS/domain say "BrainHarbor",
  so a screen reader and a sighted reader get different names on one page —
  Dan's call which wins. 741 tests.

- **2026-08-13** — **WI-413 done — the CLI says WHY it failed, so an outage and
  an odd item stop looking alike** (PR
  [#21](https://github.com/badsonstudios/BrainHarbor/pull/21), merged to
  `develop`). `ClaudeCli` now returns `Unavailable` (never answered: spawn
  failure, timeout, non-zero exit, or stdout that is not its documented
  envelope) vs `UnusableOutput` (answered; answer unusable). **The envelope is
  the CLI's own output — a garbled MODEL answer still arrives inside a
  well-formed one — which is what makes that the right line to draw.**
  `ClassifyDecision.Unavailable` covers a failed taxonomy fetch too (the site
  being unreachable is identical for every item), `SummaryResult` carries the
  same flag, and the runner stops on the FIRST unavailable. **The streak
  counter and the whole `deferred` list are deleted** — net fewer lines.
  **Where the signal is genuinely ambiguous, it now asks instead of guessing:**
  a timeout looks exactly like a dead CLI, and stopping on one would hold the
  cursor so the same slow item leads the window tomorrow, forever — so an
  `Unavailable` verdict triggers one trivial health prompt. Alive means the
  item is merely odd (queue it, advance the cursor); no answer means stop.
  **Review caught three real defects, two of them introduced by this change:**
  envelope failures were tagged `UnusableOutput`, which would have let a CLI
  printing a banner mark a whole window unclassifiable with NO bound (worse
  than the streak it replaced); the timeout stall above, which violated this
  item's own acceptance criteria; and the skip path froze trial facts for a
  day, though facts need no model call. **Live testing caught a fourth:** the
  facts-only pass fetched every source during an outage — four minutes, almost
  all of it on sources with no facts — now gated by
  `ISourceFetcher.ProducesTrialFacts`, eleven seconds. Verified live with a
  nonexistent `claude`: stopped on item 1, cursor held, nothing uploaded,
  exit 1. Also fixed WI-417's log encoding (UTF-8 **with** BOM — PowerShell
  5.1's `Get-Content` assumes ANSI without one, so every em dash read back as
  mojibake in the reader `run-local.md` hands you). `artifacts/pipeline`
  republished so tonight's 06:00 run uses this code. 733 tests.

- **2026-08-13** — **WI-417 done — the daily run leaves evidence behind.**
  Everything the pipeline printed already said what was wanted (which item was
  excluded and why, which summary was flagged and by which check); Task
  Scheduler simply threw it away. Now a per-run file lands in
  `%LOCALAPPDATA%\BrainHarbor\logs\pipeline-<date>-<time>.log` — **outside the
  repo deliberately**, since `artifacts/pipeline` is rewritten by every
  re-publish. Named to the second, not the day, so a manual run cannot clobber
  the 06:00 one; AutoFlush on, because the run whose log matters most is the
  one Task Scheduler kills at the 2-hour limit. Hand-rolled ~150-line provider
  rather than a Serilog dependency: retention, redaction and the printed path
  were all going to be custom anyway. **Retention is three limits, not one** —
  30 days, 100 files, 32 MB per run: the age limit alone cannot bound a task
  re-triggered in a loop, and neither bounds a runaway loop *inside* one run.
  **Secrets**: the `HttpClient → Warning` filter (the NCBI key rides in the
  query string) is unchanged but now **pinned by a test** that builds the real
  logging graph — nothing would have noticed the line being deleted, and the
  URIs now go to a file, not a console that scrolls away; on top of it a
  redactor scrubs configured key values and key-shaped text on the way to disk.
  Also **the reason the item was really asked for**: guardrail reasons carry a
  `FlagKind` now, so a run ends with "flagged because: reading level 1,
  invented numbers 1" instead of a bare count — the thing the DB cannot answer,
  because it stores a `summary_flagged` boolean and no reason. Verified with a
  real run against a dead endpoint (exit 4, 162 lines, stack trace, path
  printed last, neither configured key present in the file). **Known limit,
  documented not papered over**: a crash before the host builds writes no file;
  there the exit code is still the only signal.
  **Review caught six, three of them real bugs:** (1) the tally was declared
  INSIDE the try, so the catch-all zeroed it — and the likeliest exception is
  the upload at the very end, meaning the run that did two hours of LLM work
  and then failed to upload would report "summarized 0"; (2) the size cap
  measured UTF-16 chars against a byte limit, and these log lines are full of
  em dashes, so the file could run past its own ceiling (now the stream's
  position); (3) pruning could delete another RUN's live log — on Windows that
  delete succeeds against an open handle and the other process writes into
  nothing (now: never touch a file written in the last 10 minutes, and the
  writer no longer shares Delete). Also: the sink was never disposed in
  production (a provider added as an instance is disposed by neither the
  container nor LoggerFactory — harmless only because AutoFlush is on, which
  the next performance tweak would have quietly broken), the truncation write
  sat outside the try so a full disk could throw out of `ILogger.Log` and
  replace the exit code with a crash, and two runs starting in the same second
  left one with no log at all (now suffixed). Added a directory-wide 256 MB
  ceiling: 100 files x 32 MB is 3.2 GB, which is not what "pruned for you"
  should mean. Follow-up **WI-418** filed for putting the reason in the
  database, where the review queue can show it and the 137 already-pending
  items can be explained. 703 tests.

- **2026-08-13** — **The feed now runs itself, and the new prompts are proven
  in production.** Daily scheduled task registered ('BrainHarbor Pipeline',
  06:00, runs as Dan, StartWhenAvailable so a sleeping PC catches up rather
  than skipping; `-Unregister` reverses it). Triggered once to prove the chain
  rather than trust it: **exit code 0**, all six sources, ~51 new items.
  First production numbers for WI-415's prompts: `summarize-v4` flagged
  **2 of 42 (4.8%)** and `summarize-trial-v2` **0 of 9**, against 9.8% for the
  old `summarize-v3` at the OLD looser 8.5 gate — a stricter ceiling that
  rejects LESS, because the writing improved rather than the bar dropping.
  Prod now 1,179 published / 137 pending. **Task Scheduler captures no console
  output**, so the flag REASONS are lost (the DB stores only a boolean) —
  filed as **WI-417** (Dan's ask): per-run log files with rotation, secrets
  never logged.

- **2026-08-13** — **WI-415 done — AI summaries now written for 6th grade.**
  Three parts. (1) **The grader was measuring wrong**: `AllProse` joins the
  plain title and six blocks with newlines and a title has no full stop, so
  title ran into hook and every score was inflated ~0.7 of a grade (6.7
  reported vs 6.0 real across the 1,038 published items). Now block-aware, like
  the page grader. The SAME defect existed in the hype check — a negation in
  the title could excuse a "cure" claim in the hook — fixed and tested.
  (2) **The prompts now ask for 6th grade** with concrete rules (sentences
  under ~15 words, short everyday word over long): `summarize-v3→v4`,
  `summarize-trial-v1→v2`. Measured live through the real CLI: research median
  **4.7** (max 6.5), trials median **3.4** (max 3.6). (3) **Gate set to 7.0**,
  not 6.0 — deliberately: the prompt is the mechanism, the gate is a backstop,
  and gating AT the target would flag ordinary variation and drain the feed
  into the review queue (a flagged item does not publish). Was 8.5.
  New opt-in `Category=Live` test re-runs the golden set through the real CLI
  and prints the distribution — **CI now filters `Category!=Live`** and the
  body no-ops without `BRAINHARBOR_LIVE_TESTS=1`, because a [Trait] alone
  excludes nothing and it would otherwise have failed every PR build. One
  golden "ideal summary" was itself at 7.1 and got simplified — the yardstick
  has to meet the bar. **Published summaries are NOT retro-fixed**; they keep
  their older prompts' wording until re-summarized. Follow-up **WI-416**: the
  two graders still differ (medical-term allowance, hiatus rule), so the page
  6.0 and summary 7.0 are not strictly comparable. 675 tests.

- **2026-08-13** — **WI-401 DONE — the backfill finished; brainharbor.org is a
  real site.** All 6 sources green, 0 classification failures. **1,130 items
  published**, 134 pending (20 of them one-off classify failures for a human),
  106 held by the guardrails. `/trials` shows 207 trials, the feed 185 trial
  items. **The self-healing resume proved itself**: ctgov's cursor had been
  held back by the WI-401 fail-fast, so this run refetched that window,
  classified the 74 it had not reached, and advanced the cursor — no cleanup,
  no lost work, exactly what the last two attempts needed and did not have.
  WI-401 acceptance fully met: shared App Service + Postgres (~$1–3/mo
  incremental vs the ~$30 budgeted), DNS + managed TLS, deploy-on-merge with a
  smoke check, admin 2FA in prod, pipeline pointed at prod, feed backfilled.

- **2026-08-13** — **Home page now says who wrote what; filter bars fixed**
  (PR #14, released via #15, live on brainharbor.org). Dan's catches from the
  live site. (1) The hero read "we read new research and news, then explain
  it" — sounding like people — while AI was named only in the footer and on
  item pages. It now leads **"Scientists do the research. AI puts what they
  found into plain words"**; naming AI *without* that first sentence invites
  the worse misreading, that AI did the science. Also states plainly that
  summaries publish on their own and that a person does not check every one.
  Short sentences deliberately: the same content as one paragraph measures
  reading grade 8.3, this measures ~4.8. **Note, SUPERSEDED the next day by
  WI-414: ContentCheck did not gate the home page then** (it scanned only
  `Content/pages/*.md`; home is a Razor view), so reading level here was
  measured by hand. It gates reader-facing `.cshtml` now, failing above 6.0.
  Title + social description match;
  a test pins both halves of the disclosure. (2) `.feed-filters` had no CSS
  rule at all, so the submit button wrapped alone while the long toggle sat
  among the dropdowns — now one control row ending in the button, toggle
  beneath, on /research and /trials alike. 662 tests.

- **2026-08-12** — **brainharbor.org has real content.** First release through
  the new `develop → main` flow (PR #13) deployed itself and passed the smoke
  check, then the backfill ran with the WI-401 resilience fixes: **1,038 items
  published live**, 134 pending, 106 held by the guardrails (invented numerals,
  banned hype words, reading level > 8.5 — all doing their job on real data).
  PubMed alone: 1,099 fetched, 117 excluded as off-topic, 852 auto-published.
  **The fail-fast proved itself in production**: the usage limit died during
  `ctgov`, so that source stopped, its cursor stayed EMPTY, and the run ended
  with five clean sources instead of a poisoned database. Finishing it needs
  one more run and no cleanup at all. The 20 one-off classify failures went to
  the review queue as designed.

- **2026-08-12** — **WI-401 backfill: two failed attempts, both root-caused and
  fixed.** (1) **Hidden HTTP caps:** `AddStandardResilienceHandler()` replaces
  `HttpClient.Timeout` with an infinite one and applies its OWN 30s-total /
  10s-attempt defaults — so the `client.Timeout = 60s` line sitting right above
  it was decoration. It killed PubMed's catch-up fetch, then killed the sync
  UPLOAD after 3h20m of finished classify+summarize work (0 rows landed).
  Fixed with a shared `AllowLongRequests` helper on every resilient client, the
  inert `client.Timeout` lines deleted, and — because no test had ever touched
  `Program.cs`'s DI graph, which is how this reached prod — a new
  `PipelineHttpTimeoutTests` that asserts the timeouts the handler ACTUALLY
  runs with. (2) **Usage-limit death:** when the limit expires every item comes
  back Unclassified, and uploading those made the server *know* them, so no
  later run would ever classify them (532 rows hand-deleted across two
  attempts). Now a streak of 3 stops the source — processed items still upload,
  the cursor is HELD, the next run resumes the window — and because the
  classifier is shared infrastructure, the first source to prove it dead
  **latches the whole run** (small sources would never reach the streak alone).
  One-off failures still go up for a person, unchanged. Known residual filed as
  **WI-413** (counting is the wrong signal; the CLI should say *why* it failed).
  661 tests.

- **2026-08-11** — **WI-411 done — dedicated test database** (/next-item).
  DB tests now default to **`brainharbor_test`** in the same local/CI Postgres
  server — one word changed in `TestDatabase.cs`; DbUp's EnsureDatabase
  creates it on first run, so no compose/CI changes. Verified live: suite run
  created it (650/650), second run idempotent, dev-DB row counts identical
  before/after; dev DB had zero leftover test rows. The dirty-database rule
  (far-future seeds, page-until-found) now has ONE canonical home on
  `DatabaseFixture`'s doc comment; per-class comments point there. Dates kept
  as insurance. Deferred (review nit): the fixture guard accepts a REMOTE
  db named *test* — tighten when WI-401 makes remote DBs real.

- **2026-08-11** — **WI-410 done — sort the research feed** (/next-item).
  /research sortable by date (default, unchanged), readiness (highest first,
  **unscored last** — nullable score, the published_at NULLS LAST trap), and
  kind (research → news → trials → preprint grouping, newest first within a
  group, decided explicitly in SQL). Plain select in the existing GET filter
  form (no-JS + htmx for free), composes with tumor/early filters, canonical
  `?sort=` in the URL (garbage normalized away; input never reaches SQL —
  a whitelist switch picks among fixed ORDER BY strings). Review's two copy
  catches applied: "Closest to helping you" was a personal promise the
  anti-hype rules forbid → "Most ready to use"; "By type" → "By kind" (one
  word per concept). **/trials does not get the control** (no readiness score
  there; update recency already meaningful). Live-verified descending dials.
  650 tests.

- **2026-08-10** — **WI-409 done — home page leads with the feed** (/next-item,
  PR [#8](https://github.com/badsonstudios/BrainHarbor/pull/8) squash-merged).
  Home now renders the newest 4 published items ("Latest updates", same
  `_FeedCard` partial and safety rules as /research: published-only, closed
  trials excluded, early-stage hidden unless the reader's persisted WI-307
  cookie opts in — parse shared via `Research.IndexModel.ReadEarlyChoice` so
  the pages can't drift). Cards sit BELOW the three doors (deviation from the
  backlog's "above", approved: the crisis-help door must not scroll away).
  The false "research feed … coming soon" sentence now refers only to the
  digest, and a test fails if home ever claims the feed is coming while
  published items exist. Section omitted entirely at zero published items.
  Also fixed 4 **pre-existing** TrialsPageTests failures on `main` (Dan's 8/1
  live near-me testing put 20+ real trials in trials_cache and the 7/20-dated
  seeds fell off browse page 1) with far-future seed dates, and filed
  **WI-411** (dedicated test DB) so that idiom stops spreading. 640 tests.

- **2026-08-01** — **WI-403 done — the trial finder; M4 autopilot run ENDS
  here** (everything left needs Azure). `/trials` browse over `trials_cache`
  with tumor-type and phase filters, and `/trials/{nct-id}` pages.
  **Near me is a live, keyless `filter.geo` query to ClinicalTrials.gov at
  request time** (architecture.md §7), from either a typed ZIP or browser
  geolocation. The ZIP form is the PRIMARY path and geolocation is progressive
  enhancement on top: this audience should not have to grant a permission
  prompt (or run JavaScript) to find a trial. ZIP → point uses the Census ZCTA
  gazetteer shipped as a file (33,791 rows, public domain); the ZIP is used for
  the outgoing query only, never stored or logged. The live call **fails soft**
  by design — a slow registry degrades to "we could not reach ClinicalTrials.gov
  just now, here is the browse list", never an error page.
  Tumor-type filtering matches the registry's own condition strings against the
  taxonomy's labels and aliases (walking the tree, so "glioma" finds
  glioblastoma), because `trials_cache` holds trials that were never classified
  and so have no tumor_tags — that was the open question WI-402 left.
  Two safety rules pinned by tests: **only a PUBLISHED item may lend its
  plain-language text to a trial page** (the join must not become a side door
  around the review gate), and the registry's own words are always labelled as
  the registry's, never as our plain-language writing. Attribution + link back
  on every trial page (PLAN.md §5 licence requirement).
  Live-verified: 25 open brain-tumor trials within 50 miles of Columbus, 14 for
  glioblastoma, nearest sites resolved correctly. `/trials` added to the axe
  scan and to sitemap.xml.
  **Review caught four blockers, three of them the same shape as WI-402's:**
  (1) the live call did NOT fail soft on the case it was built for —
  `HttpClient.Timeout` throws `TaskCanceledException`, which IS an
  `OperationCanceledException`, so the exception filter meant to let real
  cancellation through was letting the 8-second timeout through too, giving the
  reader a 500. (2) an unknown status was rendered as "this trial is not taking
  new patients" — a fabricated claim directly above a sentence admitting we
  cannot tell (the exact rule `FeedRow.TrialHasClosed` exists to enforce; now
  three states, not two). (3) the outgoing near-me URL contains the reader's
  coordinates, and `IHttpClientFactory` logs request URIs at Information — so
  every search wrote a location to the logs while the page promised "we do not
  store it" (`RemoveAllLoggers`, plus `no-store`/`no-referrer` on those
  responses, and `/privacy` now says plainly what happens to a ZIP).
  (4) near-me searches the WHOLE registry but linked to `/trials/{id}`, which
  404s for anything outside our fetch window — on a fresh database nearly every
  result. Those rows now link to the registry.
  Also: registry count instead of our page size in the heading, closed trials
  don't show their frozen hook, deep `?page=` clamped, unknown-status trials no
  longer vanish from browse, the tumor menu drops slugs that match nothing, and
  near-me/browse now share one definition of a tumor type (with the label
  quoted — "DIPG (pontine)" carries live Essie grouping characters).
  635 tests.

- **2026-08-01** — **WI-402 done — trials fetcher** (autopilot M4):
  ClinicalTrials.gov v2 fetcher, `trials_cache` (migration 0007), a
  `trial_update` feed item for trials someone can still join, and a
  trial-specific summarization prompt. Live-verified against the real registry
  (80 trials in a 5-day window: all mapped, 79 with site coordinates, correct
  50/30 open-vs-closed split, cursor advanced clean).
  **The design changed twice under review, both times for the same reason —
  a trial's FACTS and its plain-language text obey opposite rules:**
  (1) the first cut wrote `plain_summary` into `trials_cache` through the
  unfrozen facts path, which would have carried summaries the safety checks
  FLAGGED, or a human REJECTED, to readers anyway. The cache now holds no
  plain-language column at all; editorial text lives only on
  `aggregated_items`, and facts move through their own `POST /api/sync/trials`.
  (2) refreshing the cache fixed browse but not the pages a reader lands on —
  a published trial page, its feed card, its search snippet and its RSS entry
  all kept saying "now enrolling" forever, because a known trial is never
  re-summarized. The item page now reads status live from the cache; closed
  trials leave the feed, search snippets and RSS but keep their permalink,
  which says plainly that they are not taking new patients.
  Also: facts upload BEFORE classification (an off-topic verdict no longer
  swallows a status change), fact-only trials create no review-queue rows, a
  known trial costs no LLM call, the truncation guard can no longer walk the
  cursor backwards, 5xx/network failures retry (a 400 does not), and three real
  trials were added to the golden set — a new versioned prompt was otherwise
  ungated. Stripped real investigator names, phones and emails from the
  recorded fixtures before committing (public repo). 590 tests.

- **2026-07-31** — **Autopilot M4 started** (branch `auto/M4`). WI-401 (Azure)
  is `[user]` + real money, so it is skipped; WI-402/403 (trials) are pure code
  and buildable without cloud. WI-404/405/406/407/408 all depend on WI-401, so
  the run stops after the trials feature.

- **2026-07-30** — **WI-303 golden-set accuracy run — DONE (by the assistant)**:
  the local `claude` CLI is invocable here, so ran the classify-v1 prompt
  against all 20 ratified golden items. **Stage 20/20 (100%), relevance 18/20
  (90%), primary-tag 18/20 (90%)**, exact-tag 13/20 (65%). The 2 relevance
  misses are borderline "excluded" reviews the model kept (safe direction);
  tag misses are completeness, not wrong tags. Note: `claude -p` used
  **Haiku 4.5** — consider a stronger model for classify/summarize before Auto
  mode. Recorded in the golden-set README. WI-303 acceptance now fully met.

- **2026-07-20** — **WI-302 done** (autopilot M3): Claude Code CLI wrapper.
  Invokes `claude -p --output-format json` (prompt on stdin), unwraps the JSON
  envelope, parses the model's JSON into the expected shape, validates, and
  retries ONCE on garbled output — failing fast on timeouts, auth-style exit
  codes, and validation (deterministic). A bad call NEVER returns a value
  (never a guess). Versioned PromptTemplate with a strict placeholder guard.
  Review caught a blocker (spawn failure threw instead of failing safe) +
  process-handling fixes (bounded stdin write, full stdout drain, kill on
  cancellation) — all fixed; real-runner "CLI not installed" test added. 410.
- **2026-07-20** — **Autopilot M3 started** (branch `auto/M3`, PR #5). Repo
  made public → scrubbed a personal email (NCBI contact → role address) and
  added a PII/secrets scan to the commit-push-pr + autopilot skills. Dan's
  call on the M3 quality gate: **build in Review mode** — full capability, but
  real AI summaries wait in the queue for Dan to judge before auto-publish;
  he flips Publishing:Mode=Auto when confident. Golden set ships as a DRAFT
  for Dan to ratify.
- **2026-07-20** — **WI-301 done** (autopilot M3): golden set — 20 real PubMed
  abstracts hand-classified (11 patient_relevant, 5 early_stage, 4 excluded)
  with ideal 6-block summaries for 10, numbers verbatim from source. Rubric +
  validation tests (real taxonomy slugs, documented vocab, complete
  summaries, every case has a rationale). Flagged a taxonomy gap
  (spinal-cord tumors) for later. DRAFT pending Dan's ratification. 393/393.
- **2026-07-20** — **WI-212 done — auto-publish mode (Dan's request)**: the
  human review gate is now **optional**. `Publishing:Mode` config, **Auto by
  default**: a summarized item that passes the automated safety checks
  publishes itself (slug generated, `review_events` row with actor `auto`);
  flagged or not-yet-summarized items stay pending for a person; Review mode
  restores mandatory review. The item page is **honest** — auto-published
  items say "written by AI and published automatically… a person did not
  review it," not "reviewed by a person." Chose "hold only the flagged ones"
  (Dan's pick) so the automated guardrails (numeral post-check, banned-phrase
  scan, reading level — all M3/WI-304) gate every auto-publish. **Safe-by-
  construction until M3**: no summarizer yet → nothing has a summary → nothing
  auto-publishes, even though the mode is on. Design docs (PLAN,
  content-pipeline §"Publish mode", data-model, architecture, both CLAUDE.md)
  updated — human review is a mode now, not a hard requirement. 384/384.
  (On `auto/M2`, extends PR #4.)

- **2026-07-20** — **WI-211 done — M2 COMPLETE**: live shakedown against the
  real PubMed, NCI, ScienceDaily, medRxiv and bioRxiv endpoints, from an
  empty database. The loop works: 5/5 sources → 1,360 items pending → approve
  one → exactly 1 visible on /research, 1,359 still behind the gate; a second
  run ingested **0 duplicates** and left the published item published (the
  WI-202 human-decision fix proven for real).
  **Three real bugs only a live run could find, all fixed:**
  (1) the pre-filter's keep-bias is right for sources that already selected
  for us, but bioRxiv/medRxiv return every field — it passed **91%** of the
  firehose (protein folding, chondrogenesis) into the review queue. Added a
  SourceScope so firehose sources require a POSITIVE brain-tumor match:
  bioRxiv 2863→77, medRxiv 784→11, total 4871→1360.
  (2) the feed filtered to relevance='patient_relevant', but nothing is
  classified until M3 — so approving an item in M2 did nothing visible.
  Unclassified-but-approved items are now shown; early-stage stays behind the
  toggle. (3) ScienceDaily stamps dates "EDT", which .NET won't parse — all
  48 items were undated, sorting to the bottom of the feed forever and never
  advancing the cursor. Now 0 undated of 48.
  Also hardened the feed ordering tests to stop assuming an empty table —
  the suite now passes *with* 1,360 real rows present. 374/374.

- **2026-07-19** — **WI-210 done** (autopilot M2): source health + the
  scheduled task. Added POST /api/sync/failure so a broken source actually
  writes last_error — until now nothing ever did, so a source that died a
  week ago would still show its last success. /admin/health lists every
  source with plain-language staleness ("5 days ago", "never"), flags
  failures first, and calls out any expected source that has never reported
  at all. The pipeline reports its own failures (best-effort — reporting must
  not break the run) and raises a desktop toast on finish. Task Scheduler
  registration script uses StartWhenAvailable so a sleeping PC catches up
  rather than skipping the day. 360/360.

- **2026-07-19** — **WI-209 done** (autopilot M2): the public feed. Two
  safety rules are enforced in the repository rather than a view — only
  status='published' is ever visible, and early-stage animal/cell work is
  hidden unless the reader ticks the box (a mouse-study headline reads as
  false hope). Tumor filters walk the taxonomy tree, so browsing "glioma"
  surfaces glioblastoma; filter values are normalized against a fixed set and
  never concatenated into SQL. Item permalinks render the badge with a
  plain-language explanation of what it means, and refuse to invent a summary
  when there isn't one. A pulled item's permalink 404s exactly like one that
  never existed. Tests pin that raw source text never reaches a public page.
  /research is now live — only /trials remains dead. 348/348.

- **2026-07-19** — **WI-208 done** (autopilot M2): the review queue — the
  human gate itself. Pending items with the badge a READER would see (same
  mapper the public feed uses, so the decision is made on what actually
  publishes), source text behind a details toggle for comparison, htmx
  approve/reject with a no-JS form fallback. Every transition writes an
  append-only review_events row (who, what, when, note) because "every
  published summary is human-reviewed" needs to be auditable, not assumed.
  Status transitions are guarded, so two open tabs can't double-apply, and
  slugs are generated from the plain-language title on approval with
  collision handling. Flagged items sort first. 22 new tests. 330/330.

- **2026-07-19** — **WI-207 done** (autopilot M2): admin auth — ASP.NET
  Identity (the only EF Core usage; its tables live in an `identity` schema so
  DbUp and EF never collide), ONE account seeded from config with no
  registration or password-reset endpoint, TOTP 2FA enrolment (manual key, no
  JS/QR dependency), hard lockout, anti-forgery on every admin POST, POST-only
  logout. Folder-level authorization means a new admin page is protected by
  default rather than by remembering an attribute. 12 boundary tests. Note:
  the seeder logs loudly and continues if the password is rejected — a weak
  config value must not silently leave the review queue unreachable. 308/308.

- **2026-07-19** — **WI-205 + WI-206 done** (autopilot M2): NCI + ScienceDaily
  RSS fetchers with per-source licensing enforced in the type system
  (FeedTextPolicy; ScienceDaily is headline+teaser+link only, and the enum
  now fails closed), and medRxiv/bioRxiv preprints with source_kind forced to
  "preprint" at all three layers. Review probed the LIVE APIs and found two
  silent breakages: the NCI feed URL 404d (would have failed every run
  forever — corrected to the publishedcontent path, verified 200/10 items),
  and the preprint API pages at 30 not 100, so the fetcher read 30 of ~745
  records and then advanced the cursor past the rest. Paging now follows the
  API's own total and a truncated window advances only to the newest record
  actually read. Also: relevance is judged on the FULL description before the
  licence truncates it (the teaser cut was dropping breast-cancer items whose
  brain-metastases mention fell past the cut), empty feeds warn instead of
  looking healthy, and PubMedPreFilter is renamed BrainTumorPreFilter now
  that three sources share it. 296/296.

- **2026-07-19** — **WI-204 done** (autopilot M2): PubMed fetcher (paged
  esearch + efetch XML, self-healing reldate window, NCBI throttling and key)
  and the hard-rule pre-filter. **The pre-filter decides what patients never
  see, and it was silently dropping real research** — found across my own
  tests and review: a trailing `\b` meant prefixes never matched plurals (so
  "brain metasta" missed "brain metastases" and breast-cancer brain-mets
  research vanished); multi-word terms assumed a literal space (missed
  "brain-tumor", "tumor-treating fields"); the notice rule ate ordinary
  titles starting "Response to"/"Withdrawal"/"Correction of"; the keep list
  lacked the words the audience uses ("brain mets", "CNS involvement",
  "leptomeningeal", and *"brain cancer"* itself); and broad neurology rules
  dropped late-effects research (stroke after cranial irradiation, dementia
  after whole-brain radiotherapy). All 15 titles are now regression tests.
  Also: pagination with **cursor held back** when a window is truncated
  (otherwise the remainder is invisible forever), esearch errors throw rather
  than burning the window, ArticleDate preferred for ahead-of-print,
  OtherAbstract excluded, and the NCBI key no longer lands in logs. 269/269.
- **2026-07-19** — **WI-203 done** (autopilot M2): Pipeline host (user-secrets
  config + validation, structured console logging, distinct exit codes for
  Task Scheduler, retry/backoff), typed sync client (chunking, cursor only on
  the last chunk, actionable auth errors, never logs the key), ISourceFetcher
  abstraction, and a runner with per-source isolation. Review caught a
  **blocker**: `Enumerable.Chunk` yields no chunks for an empty list, so the
  "advance the cursor when nothing is new" call made no HTTP request at all —
  a source's window could never move forward and would refetch a
  forever-growing range. My unit test had passed because it asserted the
  *stub's* recorded call, not the real client (mock hiding the bug). Added a
  real `/api/sync/cursor` endpoint + real-server tests. Also: AlwaysUpload so
  ClinicalTrials.gov updates aren't dropped by the new-only filter, full
  contract round-trip test, unknown-arg rejection, args no longer bind config
  (would put the key in the process list). 203/203.
- **2026-07-19** — **WI-202 done** (autopilot M2): sync API (state/check/items)
  with API-key auth (constant-time, fails CLOSED → 503 if unconfigured),
  key-partitioned rate limiting, per-item validation, and an idempotent
  upsert. Two real bugs found before commit: (1) API 401s were being
  re-executed into the HTML status page — machine clients got markup, and a
  POST re-execute degraded to a bogus 400; (2) **review blocker** — a
  classify-only rerun could null the plain_summary of an already-*published*
  item, leaving a live patient page contentless with no human involved.
  Content is now frozen once a human reviews it (`Frozen` count in the
  response). Also: cursor no longer advances on all-rejected batches (would
  skip that window forever), single-source cursor rule, field bounds +
  source whitelist, null-body 400s, DateOnly Dapper handler. 181/181.
- **2026-07-19** — **WI-201 done** (autopilot M2): aggregated_items +
  source_sync_state migration with CHECK constraints (incl. preprint can
  never be patient_relevant, enforced in the DB); taxonomy.yml as a **tree**
  (22 types, parent/child) + TaxonomyStore with alias resolution, Matches()
  ancestor walk, and a FilterTags gate that reports rejected tags.
  Review caught two **medically wrong aliases** — "grade 4 glioma" mapped to
  glioblastoma (WHO CNS5 grade 4 also covers IDH-mutant astrocytoma and H3
  K27-altered DMG) and DIPG treated as a synonym for diffuse midline glioma
  rather than its pontine subset. Both would have shown patients research
  about a different disease. Also fixed the DbUp journal race **in prod**
  (advisory lock, not just serialized tests) and the NULLS LAST feed index.
  data-model.md updated to match. 147/147.
- **2026-07-19** — **M1 MERGED**: Dan ran the site, visual review passed
  ("everything is looking good"); running it surfaced one real gap — no
  shipped page used a glossary term, so the tooltip was invisible; fixed with
  a real-pipeline sample on /dev/styleguide. PR #3 squash-merged to `main`
  (0f6be65), `auto/M1` deleted. Autopilot M2 starting.
- **2026-07-19** — **M1 COMPLETE** (autopilot): all 8 items shipped on
  `auto/M1`, 112/112 tests, ContentCheck clean, 0 build warnings. Awaiting
  Dan's visual review + merge of PR #3. Nothing merged to `main` by autopilot.
- **2026-07-19** — **WI-107 done** (autopilot): six curated shell pages
  (/about, /how-we-write, /start, /digest, /privacy, /terms) at reading
  grades 2.5–4.8; disclaimer partial rendering from front-matter flags;
  scaffolded Razor Privacy page deleted so /privacy is curated content.
  Review caught 3 blockers in the COPY, all fixed: /start had no emergency
  red flags before its calming copy (now leads with 911 signs), three "what
  to do next" CTAs pointed at the unbuilt /research, and a typo'd disclaimer
  flag rendered an empty box instead of the medical disclaimer (now a
  ContentCheck failure). Privacy/how-we-write claims trimmed to what the
  code actually does today.
- **2026-07-19** — **WI-106 done** (autopilot): tools/BrainHarbor.ContentCheck
  — Flesch-Kincaid gate (fail >8.5, warn ≥7.5) with block-aware sentence
  extraction (headings/bullets don't inflate the grade — review measured
  +1.6 grades before the fix), medical-hiatus syllable rule, front-matter
  validation, overdue review_due + missing-source warnings, 40-word glossary
  limit, loud warning on missing roots; CI step added (runs on all content —
  intentionally stronger than changed-only). 22 new tests. 98/98.
- **2026-07-19** — **WI-105 done** (autopilot): GlossaryStore (term files per
  content-pipeline §6, snapshot reloads), GlossaryMarker Markdig extension
  (first occurrence per page → native-popover button tooltip, WCAG 1.4.13;
  paragraphs only; %%term%% + !%term% escapes), /glossary A–Z, 3 seed terms.
  Review caught 2 real bugs pre-commit: terms split across source line wraps
  never matched (soft-break merge added) and "non-IDH-mutant" got a wrong
  tooltip (hyphen-aware boundaries) — both pinned with tests. 76/76.
- **2026-07-19** — **WI-104 done** (autopilot): ContentStore (Markdig with
  DisableHtml + YamlDotNet front matter per content-pipeline §3, mtime-keyed
  cache, slug-regex traversal guard, IO races → 404); catch-all Razor route
  renders /{slug} and /{section}/{slug} with provenance block; Content:Root
  config override for tests; publish glob for Content/pages. 22 new tests
  (parsing, routing, cache lifecycle, HTML-escape).
- **2026-07-19** — **WI-109 done** (autopilot): ResearchStage enum +
  StageBadge mapper (single source of truth incl. server-built aria-labels),
  _StageBadge (dot-meter/glyph per handoff) + _FeedCard partials,
  /dev/styleguide (dev-only, 404 in prod) rendering all 7 badge kinds + 4
  sample cards; axe scan of the styleguide added to the E2E gate. DB
  taxonomy→enum mapping decision recorded on the enum + WI-209.
- **2026-07-19** — **WI-103 done** (autopilot): helpline band on every page
  (aside landmark, CareLine tel link, → /get-help-now); /get-help-now with
  988, Crisis Text Line, CareLine, NCI, CancerCare as one-tap buttons; custom
  404 + calm Error page via status-code re-execute (direct /status/N hits
  404; large-text toggle points at the original URL on error pages). Nav +
  home "dead links" note: /get-help-now is now live.
- **2026-07-19** — **WI-102 done** (autopilot): large-text mode (22px base)
  via cookie-persisting middleware, plain-link toggle in the header (proven
  with JS disabled in Playwright); axe-core smoke tests on the shell in both
  text modes, 0 serious/critical; Kestrel dual-host test factory; CI installs
  Chromium. Review found an open-redirect blocker (protocol-relative path) —
  fixed + regression-tested; Secure cookie flag + shared URL helper applied.
- **2026-07-19** — **WI-108 done** (autopilot): Clear & Kind theme folded into
  site.css (new palette, band/card/badge tokens, 72rem container + 46rem read
  column), nav-cta pill, footer link list + ai-note, home rebuilt as Entry Hub
  (three doors → /start, /research, /get-help-now), print.css flattens the new
  surfaces incl. print-safe dot-meter ink (review Should-fix applied). Review:
  approve, no blockers. Needs Dan's visual eyeball at end of run.
- **2026-07-19** — **Design chosen & planned in**: static mock-up generated
  (`.claude/work_files/mockup/`), run through Claude Design by Dan; approved
  handoff ("Clear & Kind" + Entry Hub) moved to `docs/design/entry-hub-handoff/`.
  Backlog updated via /pm: new WI-108 (adopt theme + Entry Hub shell) and
  WI-109 (stage-badge dot-meter + feed-card partials); WI-102/103/107/209/306
  amended to depend on / reference the handoff. Next up is now WI-108.
- **2026-07-19** — **WI-101 done**: design tokens (18px-base scale, AA/AAA
  palette, spacing), real `_Layout` shell (landmarks, v1 nav, footer
  disclaimer), print.css (print-to-PDF verified), WI-005 htmx demo deleted,
  WebApplicationFactory render test added. Review clean, fixes applied.
  PR [#2](https://github.com/badsonstudios/BrainHarbor/pull/2).
- **2026-07-19** — **M0 closed**: WI-001 done (Dan bought brainharbor.org);
  PR #1 squash-merged to `main` (ce5929d) after Dan's review; `auto/M0`
  deleted. Secrets follow-ups all resolved same day.
- **2026-07-18** — **Autopilot M0 run COMPLETE**: WI-002..WI-006 shipped on
  `auto/M0` (PR #1, draft), CI green on the tip. WI-001 `[user]` outstanding.
- **2026-07-18** — **WI-006 done** (autopilot): GitHub Actions CI — build +
  test (Release) on push/PR to main, Postgres 16 service container so the
  Database-category tests run in CI too.
- **2026-07-18** — **WI-005 done** (autopilot): Htmx.Net + TagHelpers (htmx
  2.0.10 vendored), demo partial with no-JS fallback (curl-verified both
  paths); Dapper `IDbConnectionFactory` (NpgsqlDataSource DI); dev
  SYNC_API_KEY set in both apps' user-secrets. ⚠️ Dan: (1) populate
  `.claude/.env` from `.env.example` (autopilot may not touch it), (2) get a
  real NCBI_API_KEY and set it in Pipeline user-secrets + `.env`.
- **2026-07-18** — **WI-004 done** (autopilot): docker-compose (Postgres 16 @
  5433, named volume, healthcheck), DbUp on dev startup with 0001 baseline,
  connection string in user-secrets, DB smoke test. Verified on a fresh
  container; code review clean (fixes applied).
- **2026-07-18** — **WI-003 done** (autopilot): BrainHarbor.sln + Web (Razor
  Pages, net10.0) + Pipeline (console) + Tests (xUnit); build + test green.
- **2026-07-18** — **WI-002 done** (autopilot): private repo created + first
  commit pushed. ⚠️ Note: `gh` resolves to **badsonstudios**, not `danheinz`
  (`/users/danheinz` 404s — account renamed?). Repo is at
  `github.com/badsonstudios/BrainHarbor` (private ✓, `.env` untracked ✓).
  Dan: confirm the account; then docs/references mentioning `danheinz` get updated.
- **2026-07-18** — **Autopilot M0 started** (unattended run, branch `auto/M0`).
  WI-001 `[user]` (buy brainharbor.org) skipped — Dan's item, does not gate M0.
- **2026-07-18** — Domain changed to **brainharbor.org** (from .net); all docs
  updated. WI-001 now = buy brainharbor.org.
- **2026-07-18** — Workflow installed: `.claude/` adapted from ClaudeMon
  (skills: startup, pm, next-item, check-code, review, commit-push-pr, explain,
  deep-research; agents: code-reviewer, debugger, deep-research-agent; env
  hook, scripts, settings). Backlog created: M0–M4 decomposed into 34 work
  items. This file created.
- **2026-07-18** — Architecture pivot: pipeline moved local (console app +
  Task Scheduler + Claude Code CLI, no Anthropic API key); site gets a sync
  API + admin review queue; every published summary human-approved; Hangfire
  removed. Docs updated.
- **2026-07-18** — Decisions: brainharbor.net; weekly digest; local-first dev,
  Azure deferred to M4; private GitHub repo; toolchain verified (.NET 10 SDK,
  git, Docker, gh as danheinz).
- **2026-07-12** — Aggregation-first pivot (feed + plain-language summaries is
  the v1 product; static hub moved to Phase 2). Stack changed to Razor Pages +
  htmx on .NET 10 (Htmxor dead). Full design-doc set written: PLAN.md +
  docs/{architecture, sitemap, content-pipeline, data-model, roadmap}.

<!--
Maintenance rules (for the assistant):
- Starting an item  → set "In progress" (item + timestamp + current step).
- Finishing an item → move to Log with date, one-line outcome, PR link;
  update "Next up"; check the box in docs/backlog.md.
- Blocker/stopping mid-item → record exactly where things stand under
  "In progress" + "Blockers" so a cold session can resume.
- Keep "Notes for the next session" current; prune stale notes.
- Never delete Log entries; newest first.
-->
