---
title: "Targeted drugs: why your tumor's report decides this one"
slug: treatments/targeted-therapy
description: "Most of these drugs need something to aim at, and the gene results from your tumor say whether there is one. One of them works differently. What each of these drugs is for, what it asks of you, and the question worth asking about every one of them: what would it mean to say this worked?"
tags: [treatments, drugs, targeted, molecular]
sources:
  # SOURCE DISCIPLINE FOR THIS PAGE, written down so it is not re-argued
  # (content-pipeline §12.13, §12.14).
  #
  # (1) AVASTIN.COM IS BANNED BY THE CONTRACT ITSELF, NOT JUST BY JUDGEMENT.
  # §12.2 item 6 names it: "replace the manufacturer's site (avastin.com) as a
  # side-effect source before publishing." The dossier
  # (docs/research/tumor-guides/treatment-library.md §9) sources the ENTIRE
  # bevacizumab section to it -- what it is, what it is for, the side effects,
  # and the surgery interval -- and its own source-quality note says to replace
  # it. Replaced in full by ACS's brain-specific targeted therapy page, which
  # carries every one of those claims at patient level.
  #   * ONE FIGURE IS LOST WITH IT AND IS NOT PUBLISHED: the manufacturer's "at
  #     least 28 days before or after surgery". No non-manufacturer source in
  #     the reachable set gives a number. ACS says "usually it can't be given
  #     within a few weeks of surgery", and that is what the page says. §12.14:
  #     banning the citation is not keeping the claim.
  #
  # (2) THE DOSSIER COVERS THREE DRUG FAMILIES AND ACS COVERS SIX. The dossier
  # has bevacizumab, IDH inhibitors and BRAF/MEK. ACS's page adds H3 K27M
  # (dordaviprone), mTOR (everolimus, for a tumor type the dossier never
  # mentions) and NTRK. A page built from the dossier alone would have told a
  # reader with an H3 K27M or NTRK result that there is nothing for them.
  #
  # (3) THE DOSSIER AND ACS DISAGREE ABOUT BRAF, AND BOTH ARE RIGHT ABOUT
  # DIFFERENT THINGS. The dossier says dabrafenib plus trametinib was the first
  # systemic FIRST-LINE therapy approved for pediatric low-grade glioma with a
  # BRAF V600E change, which the FDA approval page states verbatim. ACS says
  # BRAF and MEK inhibitors "might be an option to treat these tumors,
  # typically after other treatments have been tried", which is the general
  # picture across ages and tumor types. §12.8 (WI-511): where two sources
  # disagree, print the disagreement rather than splitting it. The page scopes
  # each.
  #
  # (4) THE VORASIDENIB TRIAL EXCLUDED ANYONE WHO HAD HAD CHEMOTHERAPY OR
  # RADIATION, and the dossier's eligibility bullet does not say so. The FDA
  # page does: "Patients who received prior anti-cancer treatment, including
  # chemotherapy or radiation therapy, were excluded." That is a fact with a
  # consequence for a reader who has had either, and it is on the page.
  #
  # (5) UNREACHABLE OR MARKETING, SO NOT CITED: academic.oup.com (both
  # ivosidenib citations, dead since WI-527), servier.us (the maker's own
  # blog), cancerletter.com, avastin.com per (1), and drugs.com / MedlinePlus
  # drug monographs per PLAN.md §5. IVOSIDENIB IS NOT MENTIONED AT ALL: the
  # dossier recommends one sentence calling it "used mainly for other cancers
  # and sometimes off-label", both of its citations are dead, and naming a drug
  # this page cannot describe is how a reader ends up asking for the wrong one.
  #
  # (6) NCI IS CITED AND IT IS NOT PATIENT PDQ.
  # cancer.gov/about-cancer/treatment/types/targeted-therapies is NCI's general
  # treatment library, not a PDQ summary -- no /patient/ segment and no -pdq
  # suffix -- so §12.1's rule is not in play. It is cited only for framing and
  # for the resistance material, which is what §12.1 allows NCI for anyway.
  # NBK66023, which the dossier uses four times here, IS patient PDQ and is not
  # cited (§12.1, and WI-512 found its which-drug table headed with a retired
  # CNS5 name).
  #
  # WHAT THIS PAGE DELIBERATELY DOES NOT OWN.
  #   * THE WORDS ON THE REPORT are /tests/molecular-markers (WI-509). That page
  #     already ends its BRAF entry with "Finding a change does not mean a drug
  #     is right for you", which is the door this page walks through. It states
  #     none of the marker definitions.
  #   * THE DECISION for any one tumor belongs to the hubs.
  #     /tumors/low-grade-glioma and /tumors/astrocytoma carry vorasidenib as an
  #     option; /treatments/watch-and-wait (WI-522) carries why it exists at all
  #     -- it was tested against a placebo in people who would otherwise have
  #     been watched. This page carries what being ON one is like.
  #   * THE BEVACIZUMAB HONESTY NOTE IS ALREADY SHIPPED, on /tumors/glioblastoma,
  #     sourced to PMC12467656: it can make scans look better and cut steroid
  #     use, and in newly diagnosed glioblastoma it did not help people live
  #     longer. §12.10 forbids two strengths, so this page uses THE SAME WORDING
  #     and the shared sentence is in the restatement allowlist.
  #   * CHEMOTHERAPY is /treatments/chemotherapy (WI-512), which owns
  #     temozolomide, PCV, the blood counts and the fever rule. This page says
  #     once how a targeted drug differs and links.
  #
  # (7) NO NEW GLOSSARY ENTRIES, AND THE REASON, because §12.2 item 9 asks for
  # one either way. The page introduces dordaviprone, everolimus, dabrafenib,
  # trametinib, NTRK, subependymal giant cell astrocytoma and ACCELERATED
  # APPROVAL, and none is in Content/glossary/.
  #   * THE SIX DRUG AND TUMOR NAMES ARE DEFINED WHERE THEY APPEAR and appear
  #     nowhere else in the corpus, so an entry would fire on exactly one page
  #     -- the one that suppresses it. §12.8 (WI-519): an entry defined only
  #     where it is suppressed is decoration, and WI-521's RANO shape is the
  #     same thing one step worse.
  #   * `accelerated approval` IS the one with reach, and it is deliberately
  #     NOT added rather than overlooked. GlossaryMarker runs over the AI feed
  #     summaries too (SummaryRenderer), and that phrase is all over
  #     drug-approval news, so an entry would fire on machine-written text about
  #     cancers this site is not about -- the §12.8 (WI-523) alias trap, where
  #     `mapping` would have given research items a brain-surgery definition.
  #     The page defines it in place instead, in the sentence that needs it.
  # If a later page says any of these words in prose, that is the moment to
  # revisit this.
  #
  # A DOOR WAS REFUSED BY A SIBLING'S OWN PIN, and that is the sibling being
  # right. /treatments/watch-and-wait's glioma paragraph ends on its routing to
  # the placebo explanation, and WatchAndWaitPageContentTests pins that sentence
  # END-ANCHORED (`\.\s*$`) -- the §12.8 (WI-521) rule that a correctly-pinned
  # closing sentence must survive a sentence appended after it. Appending a
  # seventh door there broke it, which is the guard working. The door was
  # dropped rather than the pin loosened: that page already routes vorasidenib
  # readers to two places, and this page is reachable from both.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/treating/targeted-therapy.html
    title: "American Cancer Society: Targeted Drug Therapy for Brain Tumors in Adults"
    accessed: 2026-09-14
    # THE BACKBONE, and the replacement for avastin.com. Brain-specific, patient
    # level, and it carries all six drug families. Verbatim:
    # "Targeted therapy drugs treat cancer by targeting specific changes or
    # features in cancer cells."
    # Bevacizumab -- "works by blocking a protein called vascular endothelial
    # growth factor (VEGF)" / "Its main benefit is that it helps reduce swelling
    # around the tumor, so people getting it can often get lower doses of the
    # steroid dexamethasone." / "given by intravenous (IV) infusion, usually
    # once every 2 weeks." / side effects: "high blood pressure, tiredness,
    # bleeding, low white blood cell counts, headaches, mouth sores, loss of
    # appetite, and diarrhea"; serious: "blood clots, internal bleeding, heart
    # problems, and holes (perforations) in the intestines." /
    # "This drug can also slow wound healing, so usually it can't be given
    # within a few weeks of surgery."
    # Vorasidenib -- "can be used after surgery in people with a grade 2
    # astrocytoma or oligodendroglioma ... if the tumor cells are found to have
    # an IDH1 or IDH2 gene mutation." / "taken by mouth as tablets, typically
    # once a day." / liver: "symptoms such as jaundice (yellowing of the eyes
    # and skin), dark urine, loss of appetite, or pain in the upper right side
    # of your belly. It's important to let your health care team know if you
    # have any of these symptoms."
    # Dordaviprone -- for diffuse midline glioma with H3 K27M "if the tumor is
    # still growing despite prior treatment" / "taken by mouth, typically once
    # a week."
    # Everolimus -- for SEGAs "that can't be removed completely by surgery" /
    # "It may shrink the tumor or slow its growth for some time, although it's
    # not clear if it can help people with these tumors live longer."
    # BRAF/MEK -- "might be an option to treat these tumors, typically after
    # other treatments have been tried" / names dabrafenib + trametinib,
    # vemurafenib + cobimetinib, and tovorafenib / "Some people treated with
    # these drugs develop skin cancers ... Your cancer care team will want to
    # check your skin often during treatment."
    # NTRK -- larotrectinib, entrectinib, repotrectinib, "still growing despite
    # other treatments".
    #
    # THE PER-DRUG SIDE-EFFECT LISTS, quoted because /review was right that a
    # first draft printed five of the six with no quote behind them, and this
    # page's whole front matter is about being able to trace a claim. All six
    # are on this one ACS page, under its own "Side effects of ..." headings:
    # Vorasidenib -- "feeling very tired, headache, nausea, muscle aches or
    # stiffness, diarrhea, seizures, and changes in lab tests showing the drug
    # is affecting the liver."
    # Dordaviprone -- "feeling very tired, headache, nausea and vomiting, and
    # muscle, joint, and bone pain. It can also lower your blood cell counts and
    # lead to changes in lab tests showing the drug is affecting the liver."
    # Everolimus -- "mouth sores, increased risk of infections, nausea, loss of
    # appetite, diarrhea, skin rash, feeling tired or weak, fluid buildup
    # (usually in the legs), and increases in blood sugar and cholesterol
    # levels."
    # BRAF/MEK -- "skin changes, rash, itching, sensitivity to the sun,
    # headache, fever, chills, joint or muscle pain, fatigue, cough, hair loss,
    # nausea, diarrhea, and high blood pressure."
    # TRK inhibitors -- "dizziness, fatigue, nausea, vomiting, constipation,
    # weight gain, and diarrhea."
    # NOT PUBLISHED FROM THIS PAGE: ACS lists bevacizumab's common effects
    # UNRANKED, so "high blood pressure is the common one" was a ranking whose
    # only source was the banned avastin.com bullet, and it is gone. ACS's own
    # word for the second list is SERIOUS, not "less often", and the page uses
    # ACS's word.
    #   * THE SAME RANKING SURVIVED IN A SECOND PLACE AND WAS FOUND BY THE
    #     RENDERED READ, not by a gate: the step-by-step said "because high
    #     blood pressure is the side effect this drug is most watched for",
    #     which is the deleted ranking plus a monitoring claim, 130 lines from
    #     where the ranking had been removed. §12.14 inside one page -- when you
    #     remove a claim, grep the whole page for it.
    #   * BLOOD-PRESSURE MONITORING IS AN INFERENCE AND IS HEDGED FOR IT. ACS
    #     states a monitoring instruction for the SKIN ("Your cancer care team
    #     will want to check your skin often during treatment") and a
    #     report-this instruction for the LIVER, and says nothing about checking
    #     blood pressure. What IS sourced is that the drug raises it. So the
    #     page says "usually checked", gives the reason ACS supports, and the
    #     "What you need first" section tells the reader to ask what else is
    #     monitored.
  - url: https://www.cancer.gov/about-cancer/treatment/types/targeted-therapies
    title: "National Cancer Institute: Targeted Therapy to Treat Cancer"
    accessed: 2026-09-14
    # The gate and the honest limit. Verbatim:
    # "most of the time, your tumor will need to be tested to see if it contains
    # targets for which there is a drug."
    # Under its own heading "Are there drawbacks to targeted therapy?":
    # "Cancer cells can become resistant to targeted therapy. Resistance can
    # happen when the target itself changes and the targeted therapy is not able
    # to interact with it. Or it can happen when cancer cells find new ways to
    # grow that do not depend on the target."
    # And the sentence the page's combination clause rests on, from the same
    # paragraph: "Because of resistance, targeted therapy may work best when
    # used with more than one type of targeted therapy or with other cancer
    # treatments, such as chemotherapy and radiation."
    # NOT a PDQ summary -- see ruling (6) above.
  - url: https://www.fda.gov/drugs/resources-information-approved-drugs/fda-approves-vorasidenib-grade-2-astrocytoma-or-oligodendroglioma-susceptible-idh1-or-idh2-mutation
    title: "FDA approves vorasidenib for Grade 2 astrocytoma or oligodendroglioma with a susceptible IDH1 or IDH2 mutation"
    accessed: 2026-09-14
    # Verbatim, and the exclusion the dossier omits: "Patients who received
    # prior anti-cancer treatment, including chemotherapy or radiation therapy,
    # were excluded."
    # What was measured: "The major efficacy outcome measure was
    # progression-free survival (PFS) ... An additional efficacy outcome measure
    # was time to next intervention." Overall survival is not among them.
    # Approved "following surgery including biopsy, sub-total resection, or
    # gross total resection", for "adult and pediatric patients 12 years and
    # older".
    # NO FIGURES ARE TAKEN FROM THIS PAGE. The hazard ratios, the 17.8 months
    # and the 40 mg are all here and none is published (§12.4, §12.5).
  - url: https://www.fda.gov/drugs/resources-information-approved-drugs/fda-approves-dabrafenib-trametinib-pediatric-patients-low-grade-glioma-braf-v600e-mutation
    title: "FDA approves dabrafenib with trametinib for pediatric patients with low-grade glioma with a BRAF V600E mutation"
    accessed: 2026-09-14
    # Verbatim: "This represents the first FDA approval of a systemic therapy
    # for the first-line treatment of pediatric patients with LGG with a BRAF
    # V600E mutation." And the scope: "pediatric patients 1 year of age and
    # older ... who require systemic therapy", plus "new oral formulations of
    # both drugs suitable for patients who cannot swallow pills".
    # Also the fever signal ACS generalises: pyrexia was the most common adverse
    # reaction at 66%. THE PERCENTAGE IS NOT PUBLISHED; that fever is common
    # enough to warn about is.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12467656/
    title: "Radiotherapy in Glioblastoma Multiforme: Evolution, Limitations, and Molecularly Guided Future"
    accessed: 2026-09-14
    # Already cited by /tumors/glioblastoma for exactly this claim, which is why
    # the wording is shared rather than re-derived (§12.10). Verbatim: two phase
    # III trials showed "a prolongation of PFS (3-4 months) but not of OS", and
    # bevacizumab's "main value ... is transient symptom control and the option
    # for sparing treatment with steroids".
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC13224274/
    title: "The role of dordaviprone in the multidisciplinary management of diffuse midline glioma (Surg Neurol Int, 2026)"
    accessed: 2026-09-14
    # Recovered through PMC because surgicalneurologyint.com is unreachable
    # directly. Carries what PROGRESS.md's WI-535 note asks for, and states it
    # more precisely than "contested": the approval is ACCELERATED, and the
    # trial that would confirm a survival benefit has not reported. Verbatim:
    # "was granted Food and Drug Administration accelerated approval in 2025",
    # and "The ongoing ACTION trial is a phase III, randomized, double-blind,
    # placebo-controlled study ... Its primary goal is to assess whether
    # dordaviprone improves overall and PFS relative to placebo."
    # NO SURVIVAL FIGURES are taken from this paper, which carries several.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12416742/
    title: "FDA Approval Summary: Vorasidenib for IDH-mutant Grade 2 Astrocytoma or Oligodendroglioma following surgery"
    accessed: 2026-09-14
    # Already cited by /tumors/low-grade-glioma, /tumors/astrocytoma,
    # /tumors/glioma and /treatments/watch-and-wait, so the corpus has fetched
    # and read it before. Cited here for the liver monitoring being the ongoing
    # practical burden, and for the interaction warning, both published as
    # shapes rather than figures.
  - url: https://dailymed.nlm.nih.gov/dailymed/fda/fdaDrugXsl.cfm?setid=13e15ee0-d679-4fa9-9430-e2e2170474da
    title: "WELIREG (belzutifan) prescribing information | DailyMed"
    accessed: 2026-09-22
    # WI-547. THE BELZUTIFAN BULLET'S OWN SOURCE, on this page rather than only on
    # the tumor page: a sibling's front matter is not this page's source (WI-544).
    # This is the drug LABEL on DailyMed, not a drug monograph library and not the
    # manufacturer's consumer site, both of which are barred. Verbatim: for adults
    # with von Hippel-Lindau disease who need therapy for "central nervous system
    # (CNS) hemangioblastomas", "not requiring immediate surgery"; "administered
    # orally"; the boxed warning "EMBRYO-FETAL TOXICITY". The label's CNS
    # hemangioblastoma indication exists ONLY inside that VHL bullet, which is why
    # the page's bullet scopes its "only" TO A BRAIN OR SPINAL CORD TUMOR. The label
    # carries two further indications that do NOT require the condition (clear cell
    # kidney cancer, both advanced after other drugs and as an added treatment after
    # kidney surgery, and pheochromocytoma or paraganglioma), so an unscoped "only
    # for people with VHL" would be false of the drug (/review rounds 4 and 5).
    # No dose figure is published here (§12.4).
reviewed: 2026-09-14
review_due: 2027-03-14
disclaimers: [medical]
---

## The short version {#short-version}

A targeted drug is made to act on one particular change inside tumor cells. So
it needs something to aim at, and the gene results from your tumor are what say
whether there is one. If your report does not show that change, the drug will
not help. That is not bad luck about you. It is how those drugs work. One drug
here is the exception, and it is aimed at the swelling rather than at a gene
change. There are
several of these drugs now. They are used at very different points, and the one
question worth asking about every one of them is what it would mean to say it
worked.

## What is a targeted drug? {#what-it-is}

It is a drug built to act on one specific thing inside tumor cells.

Chemotherapy works in a broader way. It goes after cells that are dividing,
wherever they are, which is why it affects healthy parts of you that divide
quickly too. [Chemotherapy: the drugs, your blood counts, and the fever
rule](/treatments/chemotherapy) covers those drugs.

A targeted drug is narrower. It is made to block one protein, or one change in
one gene. Some are pills you take at home. One of the ones used for brain
tumors is given as an infusion, into a vein through an IV line.

**Narrower does not mean gentler.** These drugs have real side effects, and they
are different from chemotherapy's rather than smaller. There is a section on
them below.

## Why does my tumor's report decide this? {#why-the-report}

Because the drug has to have something to act on, and the report is what says
whether it is there.

When your tumor was taken out or sampled, the laboratory looked at its genes.
The National Cancer Institute puts the general rule plainly: most of the time
your tumor has to be tested to see whether it carries a target that a drug
exists for. [The words on your gene results](/tests/molecular-markers) goes
through those words one at a time.

**So there are two separate questions, and people often merge them.** The first
is whether your tumor carries the change. The second is whether a drug for that
change is a good idea for you now. A yes to the first is not a yes to the
second, and your team will have reasons either way.

**If your report shows no target, that is information rather than a door
closing.** It means the drugs matched to a gene change are not the route. It
says nothing about bevacizumab, which is not matched to one, and nothing about
surgery, radiation, chemotherapy, or a trial.
[Clinical trials: is this for me?](/treatments/clinical-trials) is worth reading
either way, because a trial is one of the routes a new targeted drug reaches
people.

## Which drug goes with which result? {#which-drug}

Here is the shape of it. Where you sit in it is a question for your team.

- **A change in IDH1 or IDH2, in a grade 2 astrocytoma or oligodendroglioma,
  after surgery.** The drug is vorasidenib, a tablet. Your own tumor page has
  the decision: [low-grade glioma](/tumors/low-grade-glioma) and
  [astrocytoma](/tumors/astrocytoma).
- **A BRAF V600E change.** There are several pairs of pills here. For children
  with a low-grade glioma, dabrafenib with trametinib is approved as the first
  drug treatment. For adults, the American Cancer Society says these are usually
  an option after other treatments have been tried. Which of those you are is a
  question for your team.
- **Swelling around a fast-growing glioma, or a tumor that has come back.**
  Bevacizumab, given as an infusion. It blocks a protein that tumors use to
  build blood vessels, which brings the swelling down. That is its target,
  rather than a change on your report.
- **An H3 K27M change, in a diffuse midline glioma that has kept growing after
  other treatment.** Dordaviprone, taken by mouth.
  [Diffuse midline glioma](/tumors/diffuse-midline-glioma#dordaviprone) says what
  its early approval does and does not show.
- **A tumor called a subependymal giant cell astrocytoma that cannot be fully
  removed.** Everolimus, a pill.
- **An NTRK gene change.** These are rare in brain tumors. There are three pills
  used when a tumor is still growing after other treatment.
- **A brain or spinal cord tumor caused by von Hippel-Lindau disease, in an
  adult.** The drug is belzutifan, a pill. For a brain or spinal cord tumor, it
  is only for people with that inherited condition. It is used when a tumor
  needs treating and surgery is not the next step. [Hemangioblastoma](/tumors/hemangioblastoma) is the tumor page. It
  carries the warnings for this drug, including that it must not be taken in
  pregnancy.

**If your cancer started somewhere else and spread to the brain, the drug is
usually chosen from that first cancer's own test results**, not from the brain
tumor's. [Cancer that has spread to the brain](/tumors/brain-metastases) covers
how those two teams work together.

## What happens, step by step {#step-by-step}

There are two shapes, and which one you get depends on the drug.

**If it is a pill or capsule:**

1. Your tumor is tested, if it has not been already.
2. You have blood tests before you start, and every other medicine and
   supplement you take is gone through. Bring the list rather than the memory.
3. You collect the drug and take it at home. Some are once a day. One is once a
   week.
4. You come back for blood tests and appointments on a rhythm your team sets.
5. Scans continue on whatever schedule your team has you on.

**If it is an infusion:**

1. The same tests and medicine review first.
2. You come in for the infusion. Bevacizumab is usually given every couple of
   weeks.
3. Your blood pressure is usually checked, because this drug can raise it.

**Nobody starts a drug aimed at a gene change without the testing being done.**
Bevacizumab is the exception, because it is not matched to one. If you are told
a targeted drug is being considered, it is fair to ask whether the test result is
back and whether you can have a copy.

## How long does it take? {#how-long}

**Usually for as long as it is helping and you are tolerating it, with no end
date set at the start.** That is different from a course of radiation or a set
number of chemotherapy cycles, and it takes people by surprise.

- **A pill carries on at home** until something changes it.
- **An infusion runs on a repeating rhythm** rather than to a finish line.

Ask what your own stopping point would look like, and what would make your team
change course. Those two answers make an open-ended treatment something you can
plan around.

## What is it like to be on one? {#what-it-feels-like}

Quieter than people expect, and open-ended in a way the other treatments are
not.

**On a pill, most days feel like nothing at all.** You take it and get on with
your day. There is no hospital visit attached to the treatment itself, which is
a relief for some people and oddly unsettling for others, because it does not
feel like being treated.

**The blood tests are the rhythm of this, not the drug.** They are frequent
early on and usually settle into a pattern. That pattern, rather than the pill,
is what you end up planning around.

**The open end is the part people find hardest.** There is no set number of
weeks written down at the start, so there is no countdown and no last day to
aim at. Some people find that easier than a course with an end. Some find it
much harder. Both are ordinary, and it is worth saying out loud to somebody
rather than carrying it.

**An infusion is a different shape of day**, closer to an appointment: you come
in, you are checked over, it goes in, you go home.

## What would it mean to say this worked? {#what-worked-means}

This is the most useful question on the page, and the answer is genuinely
different for each of these drugs.

**It is worth asking because "it works" can mean several different things.** A
drug can shrink a tumor on a scan. It can hold a tumor still. It can delay the
next treatment. It can make you feel better. It can help people live longer.
Those are five different things, and a drug can do one and not the others.

**Bevacizumab is the clearest example, and its own page says it plainly.** It
can make scans look better and cut down the need for steroids. But in the trials
in newly diagnosed glioblastoma it did not help people live longer.
[Glioblastoma](/tumors/glioblastoma) carries that in full. Reducing swelling is
worth having on its own terms. It is just not the same claim.

**Vorasidenib was measured on time, not on living longer.** The trial that led
to its approval looked at how long people went before the tumor grew, and at how
long before they needed the next treatment. Living longer was not one of the
things it measured. That is not a criticism of the drug. It is what the trial
was built to answer, and it is worth knowing which question got answered.

**That trial also left out anyone who had already had chemotherapy or
radiation.** Everyone in it had had surgery and nothing else. So if you have
already had either, you are not the person the trial studied, and what the drug
would do for you is a genuinely open question rather than a settled one. It is
worth asking your team about directly, because the answer is theirs and not this
page's.

**Dordaviprone has what is called an accelerated approval.** That means it was
allowed onto the market on early results, with a trial still running to
find out whether it helps people live longer. That trial has not
reported. A reader offered this drug should know it is genuinely new ground.

**For everolimus, the American Cancer Society says it out loud:** it may shrink
the tumor or slow it down for a while, and it is not clear whether it helps
people with these tumors live longer.

**So ask your team this, about whatever drug is on the table: what are we hoping
this does, and how will we know?** It is a fair question, it has an answer, and
the answer changes what the next scan means to you.

## Side effects {#side-effects}

Different from chemotherapy's, and different between the drugs. Ask for the
written list for the drug you are actually on. In general:

**Bevacizumab.** High blood pressure, tiredness, bleeding, low white cell counts,
headaches, mouth sores, less appetite and diarrhea.

**It has a shorter list of serious ones, and those are the ones to know what to
do about.** They are blood clots, bleeding inside the body, heart problems, and
a hole in the bowel.

- **Chest pain, or trouble breathing, is an ambulance call.** 911, or your local
  emergency number. That is where the rest of this site files it too.
- **Sudden bad pain in your belly, blood in your stool, or bleeding that will not
  stop, means calling your team the same day.**

Those two lines are the same answer [brain surgery](/treatments/craniotomy) and
[chemotherapy](/treatments/chemotherapy) give for the same symptoms.

**Vorasidenib.** Tiredness, headache, feeling sick, aching muscles, diarrhea,
and seizures. The one that needs watching is the liver, which is why the blood
tests are regular.

**Call your team if your eyes or skin turn yellow, your urine goes dark, you
lose your appetite, or you get pain in the upper right of your belly.** Those
can be signs of a liver problem.

**The BRAF and MEK pills.** Skin changes, rash, itching, burning easily in the
sun, headache, fever, chills, aching joints, tiredness, cough, hair loss,
feeling sick, diarrhea and high blood pressure.

**Fever is common enough with these that it is worth expecting, and it needs a
plan rather than a guess.** Ask your team, before you start, what temperature
they want to hear about and what number to call after hours. Write it down.
A fever on one of these drugs is not the same event as a fever during
chemotherapy, which has its own rule on
[the chemotherapy page](/treatments/chemotherapy#fever-rule). So ask for the
rule that applies to the drug you are actually on.

**The BRAF and MEK pills also ask for your skin to be watched.** Some people on
these drugs get skin cancers. Your skin should be checked regularly while you
are on them, so ask how often, and tell your team right away about any new spot
or any patch that looks different.

**Dordaviprone.** Tiredness, headache, feeling sick and being sick, and aching
muscles, joints and bones. It can lower your blood counts and affect the liver.

**Everolimus.** Mouth sores, more infections, feeling sick, less appetite,
diarrhea, rash, tiredness, swelling in the legs, and rises in blood sugar and
cholesterol.

**The NTRK pills.** Dizziness, tiredness, feeling sick and being sick,
constipation, weight gain and diarrhea.

Whichever of these you are on, keep that written list somewhere you will find it
again.

## If you are having surgery, bevacizumab has its own rule {#surgery}

This is the part people most often do not know about, and it is the least
explained.

Bevacizumab slows down wound healing. So it usually cannot be given in the weeks
around an operation.

**There is no one number that fits everybody.** The gap depends on the operation
and on you. What is agreed is that it usually cannot be given within a few weeks
of surgery.

**So ask for your own dates, and ask both ways.** How long before the operation
does it stop, and how long after it do you wait. Write the answers down. If any
operation is being planned, including a dental one, tell whoever is planning it
that you are on this drug.
[Getting ready for surgery](/tests/getting-ready-for-surgery) covers the rest of
what happens before an operation.

## What you need first {#what-you-need-first}

Before any of these starts, expect:

- **The gene results from your tumor.** This is the gate for every drug on this
  page except bevacizumab, which is not matched to a gene change.
  [The words on your gene results](/tests/molecular-markers) explains what is
  being looked for and how long it takes. **Ask for a copy of the report.**
- **Blood tests**, including liver tests for several of these drugs.
- **Blood pressure checks**, for bevacizumab. Ask what else will be monitored
  while you are on it, because the answer is your team's rather than this
  page's.
- **A full list of everything you take**, including things you buy yourself and
  anything herbal. Some of these drugs are affected by other medicines, and this
  is the appointment to get that sorted rather than later.
- **Your surgery plans, if you have any**, for the reason in the section above.

## For the person alongside them {#caregiver}

[CAREGIVER]

A drug taken at home moves a lot of the work onto the house, and that is the
part nobody warns you about.

**Pills at home mean somebody has to keep track.** There is no nurse handing
them over. Work out together where the pills live, whether an alarm helps, and
what to do about a missed one before it happens rather than after. Ask the team that
last question directly, because the answer is not the same for every drug.

**Ask for the blood-test pattern in advance.** It is a diary problem rather than
a weekly surprise once somebody has written it down.

**A few things are worth telling the team about quickly rather than waiting.**
Yellow eyes or skin, dark urine, or pain in the upper right of the belly. A
fever, which is common enough on the BRAF and MEK pills to have its own plan.
A new spot on the skin, if they are on those. Most of these turn out to be
nothing, and telling somebody early is what makes that true.

**Ask what the drug is hoping to do.** It is the question in the middle of this
page, and it is often easier for the person who is not the patient to ask it out
loud. It changes what every scan afterwards means.

## What if it stops working? {#resistance}

It happens, it has a name, and it is not a failure of yours.

Tumor cells can become resistant to a targeted drug. The National Cancer
Institute describes two ways. The target itself can change, so the drug no
longer fits it. Or the cells find a different way to grow that does not use that
target at all.

It is also why, as the National Cancer Institute puts it, these drugs may work
best alongside another targeted drug or alongside chemotherapy and radiation.
And it is why the scans keep coming.

**If a drug stops working, that is a conversation about what is next rather than
the end of the list.** Ask what the options are, whether another targeted drug
applies, and whether a trial does.
[Follow-up scans, and what the results mean](/tests/follow-up-scans) covers what
a changed scan does and does not settle.

## Who decides, and how will I hear? {#who-decides}

Two things have to line up, and they are decided at different moments.

**Whether your tumor carries a target** is the laboratory's answer, and it
arrives with your gene results.
[Waiting for your pathology results](/tests/waiting-for-results) covers that
wait.

**Whether a drug for it is right for you now** is your team's, usually with the
rest of the people who meet about your case.

Ask how you will be told, and whether the gene results land in your online
records before anybody talks you through them. A gene result is a hard thing to
meet by yourself on a small screen. If you would rather hear it from a person
first, say so now rather than afterwards.

## What to ask your team {#questions}

- Has my tumor been tested for IDH and BRAF, and can I have a copy of the whole
  report? Some results on it are used for other decisions rather than for these
  drugs.
- Is there a targeted drug for my tumor's changes?
- What are we hoping this drug does, and how will we know whether it is doing
  it?
- Is this instead of radiation and chemotherapy, or as well as?
- What blood tests will I need, and how often?
- What should make me call you, and what can wait?
- If I need an operation, when does this drug stop and when does it start again?
- What happens if it stops working?
- If the answer is no targeted drug, what is the route instead?

## Where to go next {#where-to-go-next}

- [The words on your gene results](/tests/molecular-markers) for what is on the
  report that decides all of this.
- [Chemotherapy: the drugs, your blood counts, and the fever
  rule](/treatments/chemotherapy) for the broader drugs, and the fever rule.
- [Clinical trials: is this for me?](/treatments/clinical-trials) because a
  trial is one of the routes to a new targeted drug.
- [Steroids](/treatments/steroids) for the medicine bevacizumab can reduce the
  need for.
- [Follow-up scans, and what the results mean](/tests/follow-up-scans) for what
  a scan on treatment does and does not settle.
- [Get help now](/get-help-now) if you need to talk to a person today.
