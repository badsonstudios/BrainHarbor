---
title: "The extra scans before treatment, and what each one is for"
slug: tests/planning-scans
description: "Nobody is offered a functional MRI on its own. You are told your team wants to add some sequences. What each of the extra scans answers, which one gives you a job to do, and why a map is not the same as a verdict."
tags: [tests, scans, mri, surgery, planning]
sources:
  # SOURCE DISCIPLINE FOR THIS PAGE, written down so it is not re-argued
  # (content-pipeline §12.13). The dossier section behind this page,
  # docs/research/tumor-guides/tests-library.md §3, leans on five sources this
  # project cannot reach and on one attribution that does not hold. §12.14 says
  # a banned citation reads like a fixed claim, so the replacement is recorded
  # next to each ban.
  #
  # (1) UNREACHABLE, SO NOT CITED AND NOT PLANNED AROUND: sciencedirect.com,
  # journals.lww.com (the DTI review), academic.oup.com (the 2HG meta-analysis),
  # ajnr.org (403 since WI-527) and researchgate.net (the neurovascular
  # uncoupling paper). The two claims that actually mattered were both recovered
  # open and are cited below: the DTI tract patterns at PMC8050646, and
  # neurovascular uncoupling at PMC5669348.
  #
  # (2) §3.1 SAYS "intraoperative direct cortical stimulation during awake
  # surgery remains the reference standard for mapping" AND ATTRIBUTES IT TO
  # PMC4757221. That paper says something close to the opposite about fMRI's
  # standing -- its sentence is that the evidence from fMRI and direct cortical
  # stimulation combinations "confirms the spatial accuracy and validity of
  # pre-operative fMRI" -- and PMC5669348 does not say it either. No source in
  # the reachable set states it. What IS stated, at patient level and verbatim,
  # is RadiologyInfo's own limit: "Your physician may recommend additional tests
  # to confirm the results of fMRI if there are critical decisions to make (such
  # as in planning brain surgery)." That is the sentence this page prints, and
  # it is the SAME sentence /treatments/awake-craniotomy (WI-523) already rests
  # its own version on -- one claim, one wording, across the two pages that
  # carry it (§12.8, WI-522).
  #
  # (3) §3.1 SAYS "finger tapping is typical for tumours near the hand-knob
  # area", cited to PMC4757221 and PMC8160093. PMC4757221 describes "left or
  # right thumb apposition with or without plantar flexion of the foot (based on
  # location of the lesion)" and names no anatomy of that kind. The anatomy is
  # not published. What is published is the part that is verbatim and is also
  # the part a reader can use: which task you are given depends on where the
  # tumor is.
  #
  # (4) §3.3's 2-HYDROXYGLUTARATE MATERIAL IS DROPPED. Its meta-analysis is on
  # academic.oup.com, which is dead, and the dossier's own note says the
  # technique needs spectral editing that is "not available everywhere". A
  # research technique for detecting one gene change does not belong in a
  # patient's planning page when /tests/molecular-markers (WI-509) owns IDH and
  # owns it from tissue.
  #
  # (5) §3.5's SSTR/DOTATATE AND SPECT MATERIAL IS DROPPED. Every DOTATATE
  # source in the pack is gated (redjournal.org, sciencedirect.com), and SPECT
  # rests only on NCI patient PDQ. Neither has a reachable patient-level source,
  # and a scan this page cannot describe honestly is a scan it does not mention.
  #
  # WHAT THE PAGE DELIBERATELY DOES NOT OWN. The scope line is drawn at
  # BEFORE-TREATMENT, and it is drawn on purpose, because the same five scans
  # have an after-treatment use that a sibling already ships:
  #   * PERFUSION AND PET USED TO TELL TREATMENT CHANGE FROM GROWTH is
  #     /tests/follow-up-scans (WI-521), which carries pseudoprogression,
  #     radiation necrosis, the twelve-week rule and the honest statement that
  #     the fancier scans "help, and they do not settle it". That page reached
  #     that answer from two sources that disagree, printed the disagreement,
  #     and is more current than this page's dossier section. This page routes
  #     to it and states none of it.
  #     A SOURCE CAN BE QUOTABLE AND STILL BE THE WRONG STRENGTH FOR THIS PAGE.
  #     ACS says, verbatim, "Tumor usually shows up on a PET scan, while scar
  #     tissue does not." The first draft printed it, and `/review` was right to
  #     call it a blocker: the sibling that OWNS the question refuses the binary
  #     ("Those help, and they do not settle it. Older reviews said outright that
  #     they cannot reliably tell the two apart. Newer ones report them doing
  #     better than that."), and §12.10 says two pages must not state one claim
  #     at two strengths. The correctly-cited sentence is the defect. This page
  #     keeps ACS's own hedge -- "more likely to be" -- and routes.
  #   * THE MACHINE, THE NOISE, SMALL SPACES, THE GADOLINIUM DYE AND THE DEVICE
  #     CARD are /tests/mri (WI-506). Every one of these scans happens in that
  #     machine and this page says so once, then links.
  #   * THE NAVIGATION SCAN before surgery is /tests/mri#navigation-scan. It is
  #     a planning scan too, and it is already owned.
  #   * THE TEST IN THE OPERATING ROOM is /treatments/awake-craniotomy (WI-523).
  #     Its step 3 already compresses fMRI into three sentences for a reader in
  #     the middle of an operation decision. This page owns the APPOINTMENT --
  #     what you are asked to do in there and why -- and the two must say the
  #     same thing about fMRI's limit, which is why both rest on the same
  #     verbatim sentence.
  #
  # NO ESCALATION LIST AND NO CAREGIVER SECTION (§12.8, WI-520). Nothing happens
  # to this reader because of these scans: no aftercare, nobody discharged into
  # anyone's care, and no reachable source supports a triage rule for an extra
  # MRI sequence. An invented tier list would contradict the corpus by omission,
  # which is WI-524's defect.
  #
  # THE INSURANCE QUESTION IN §3.7 IS LEFT OUT ON PURPOSE. Three shipped pages
  # already carry a "your insurance" phrasing that PROGRESS.md has logged for
  # /pm as inconsistent; a fourth copy is a fourth thing to keep in step. The
  # access question this page does ask -- whether the scan is done where you are
  # treated -- asserts nothing about who pays.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging/how-diagnosed.html
    title: "American Cancer Society: Tests for Brain Tumors in Adults"
    accessed: 2026-09-13
    # The backbone: one patient-level page carrying all five scans, already
    # cited by /tests/mri. Verbatim, in the page's order:
    # fMRI -- "Functional MRI (fMRI) looks for tiny blood flow changes in an
    # active part of the brain." / "fMRI can help determine which part of the
    # brain handles a function such as speech, thought, sensation, or movement
    # so doctors can avoid these areas when planning surgery or radiation
    # therapy".
    # DTI -- "Diffusion tensor imaging (DTI) or tractography shows where the
    # major pathways (tracts) of white matter (nerve fibers) are in the brain so
    # surgeons can avoid these areas when removing tumors."
    # MRS -- "Magnetic resonance spectroscopy (MRS) measures biochemical changes
    # in an area of the brain and displays them in graph-like results called
    # spectra."
    # Perfusion -- "Magnetic resonance perfusion (also called MR perfusion or
    # perfusion MRI) shows the amount of blood going through different parts of
    # the brain, using a contrast dye injected quickly into a vein." / "Tumors
    # often have a bigger blood supply than normal areas of the brain, and a
    # faster-growing tumor often needs more blood." / "MR perfusion can help
    # determine: The best place to take a biopsy."
    # PET -- "For a PET scan, you are injected with a slightly radioactive
    # substance that collects mainly in tumor cells." / "A PET scan can also be
    # useful after treatment to help determine whether an area that still looks
    # abnormal on an MRI is more likely to be remaining tumor or scar tissue." /
    # "This test is more likely to be helpful for fast-growing (high-grade)
    # tumors than for slower-growing ones."
    # Vessels -- "Magnetic resonance angiography (MRA) and magnetic resonance
    # venography (MRV) look at the blood vessels (arteries or veins) in the
    # brain." / "CT angiography can provide better details of the blood vessels
    # in and around a tumor than MR angiography in some cases."
  - url: https://www.radiologyinfo.org/en/info/fmribrain
    title: "RadiologyInfo.org (RSNA/ACR): Functional MRI (fMRI) of the brain"
    accessed: 2026-09-13
    # The fMRI experience, and the whole reason this section is the longest.
    # Verbatim: fMRI "measures the small changes in blood flow that occur with
    # brain activity". / "While doctors may use fMRI to research many
    # conditions, the FDA has only approved the use of fMRI for surgical
    # planning." / "High-quality images depend on your ability to remain
    # perfectly still and follow breath-holding instructions while the images
    # are being recorded. If you are anxious, confused or in severe pain, you
    # may find it difficult to lie still during imaging." / "fMRI is especially
    # sensitive to head motion." /
    # THE BEST SENTENCE ON THE PAGE, and the one that turns "am I failing this?"
    # into an action: "fMRI requires that you are able to perform the tasks
    # presented to you. If the tasks are too difficult for you or if you have
    # difficulty performing the actions, you should tell the technologist. They
    # may give you different tasks or easier questions." /
    # The caffeine instruction, which appears nowhere else in this corpus:
    # "try to maintain your typical habits for drinking caffeinated beverages
    # (especially coffee). For example, if you normally drink coffee every
    # morning, try not to skip it on the day of your exam. If you rarely drink
    # coffee, try to avoid it on the day of your exam." /
    # The limit: "overall there is less experience with fMRI than with many
    # other MRI techniques. Your physician may recommend additional tests to
    # confirm the results of fMRI if there are critical decisions to make (such
    # as in planning brain surgery)." /
    # Who reads it: "A radiologist, neurologist, or neuropsychologist,
    # specifically trained in functional MRI interpretation, will analyze the
    # images. The doctor will send a signed report to your primary care or
    # referring physician, who will share the results with you."
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC4757221/
    title: "Presurgical fMRI in patients with brain tumors: a comparison of language paradigms and clinical experience"
    accessed: 2026-09-13
    # Cited for the tasks and for what happens when a task is too hard, NOT for
    # the reference-standard claim the dossier hangs on it (see ruling 2 above).
    # Verbatim: the language paradigms are "Word Production", "Reverse Word
    # Reading", "Object Naming", "Word Reading", "Word Generation" and "Verb
    # Generation"; the motor task is "left or right thumb apposition with or
    # without plantar flexion of the foot (based on location of the lesion)";
    # "We applied more simple tasks such as simple word reading or verb
    # generation for these patients based on pre-fMRI test performance, yielding
    # more reliable results"; and "Accurate and reproducible fMRI strongly
    # depends on the patient's cooperation."
    # THE TASK LIST IS THE PAPER'S, NOT THE DOSSIER'S. §3.1 offers "sentence
    # completion" as a language paradigm and the word "sentence" appears in none
    # of the six the paper names. A first draft printed it anyway, under a test
    # whose comment said every task was in this paper or in ACS -- a test comment
    # providing false assurance, which is §12.14's shape one level up. Replaced
    # by "Reading a word backwards", which is Reverse Word Reading, in plain
    # words.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC5669348/
    title: "Implications of neurovascular uncoupling in functional magnetic resonance imaging (fMRI) of brain tumors"
    accessed: 2026-09-13
    # The open replacement for the dead researchgate URL, and the honest limit
    # this page exists to carry. Verbatim: "Neurovascular coupling is the
    # relationship between neural firing and concomitant changes in cerebral
    # blood flow to accommodate changing energy demands." / NVU "has the
    # potential to impair or decrease the BOLD response when mapping these
    # crucial areas and therefore confound interpretation of clinical fMRI
    # data." / "Breath-hold and other hypercapnia challenges permit assessment
    # of CBF and vasodilation in the absence of task-induced neuronal
    # stimulation. Impaired CVR can be an important indicator of NVU."
    # The direction matters and is why the page says the map can be wrong in the
    # direction that matters: the failure is a FALSE NEGATIVE. A working region
    # can fail to light up, so the danger is operating where you should not.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8050646/
    title: "Role of diffusion tensor imaging in evaluation of white matter tracts in patients with brain gliomas"
    accessed: 2026-09-13
    # The open mirror of the Springer article the dossier cites. Verbatim: the
    # five patterns are non-affected, displaced ("abnormal location and/or
    # direction resulting from bulk mass displacement"), edematous, infiltrated
    # and disrupted ("the tract or part of it was not identifiable"); and
    # "Surgical corridor and the extent of surgical excision was planned
    # according to the affection of WM fiber tracts as detected by DTI."
    # The page does NOT publish the five names or the five-way split: a reader
    # cannot act on which pattern they have, and the three that a plain-language
    # sentence can carry -- pushed aside, grown into, broken -- are the ones the
    # paper's own definitions support.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8272438/
    title: "Magnetic resonance spectroscopy of the brain: a practical guide"
    accessed: 2026-09-13
    # Verbatim: choline is "a component of cell membranes. Elevated Cho is a
    # marker of increased cell turnover"; NAA "is a marker of neuronal
    # viability" whose reduction "indicates neuronal destruction or
    # replacement"; creatine "has a role in storage and transfer of energy in
    # neurons" and serves as an internal reference; lactate is "a marker of
    # anerobic metabolism and is not seen in normal adult brain spectra".
  - url: https://www.radiologyinfo.org/en/info/pet
    title: "RadiologyInfo.org (RSNA/ACR): Positron Emission Tomography (PET)"
    accessed: 2026-09-13
    # The PET day, which is the one that is not an extra ten minutes on an MRI.
    # Verbatim: "You will feel a slight pin prick when the nuclear medicine
    # technologist inserts the needle into your vein for the intravenous line."
    # The tracer takes roughly 30 to 60 minutes to collect, and the patient
    # stays quiet and avoids moving or talking during it; the PET scan itself
    # runs about 20 to 30 minutes; "Except for intravenous injections, most
    # nuclear medicine procedures are painless"; no eating for several hours
    # beforehand and water only; the tracer leaves the body over hours to days
    # and drinking water helps.
    # ADDED AFTER /review ASKED WHAT THE PAGE SENDS A READER HOME WITH. Verbatim:
    # "If you are breastfeeding at the time of the exam, ask your radiologist or
    # doctor how to proceed. It may help to pump breast milk ahead of time and
    # keep it on hand for use until the PET radiotracer and CT contrast material
    # are no longer in your body." And: "Women should always tell their doctor
    # and nuclear medicine technologist if they are pregnant or breastfeeding."
    # The first draft closed the PET day with "the amount of radioactive material
    # used is small, which is why it is considered a reasonable trade for the
    # pictures" -- a benefit-risk judgement in the site's voice, sourced to
    # nothing. Replaced by the instruction the source actually gives.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC6351513/
    title: "Joint EANM/EANO/RANO practice guidelines for the use of PET imaging in gliomas"
    accessed: 2026-09-13
    # Why a different tracer exists, verbatim: "FDG PET plays a more limited
    # role than amino acid PET in the imaging of gliomas due to the high
    # physiological uptake of FDG in normal brain grey matter and variable
    # uptake by inflammatory lesions." And what the amino acid version is used
    # for: "Local areas with the highest uptake should be used for biopsy
    # guidance." That is the ONLY before-treatment use this page publishes. A
    # first draft also said PET "can help show how far a tumor reaches", which
    # no fetched verbatim supports -- the tumor-delineation claim came from a
    # summary of this guideline rather than from its text. Dropped.
    # THE DOSSIER'S AVAILABILITY CLAIM IS NOT PUBLISHED. §3.5 says amino-acid
    # PET is "concentrated in academic centres, especially in the US" with no
    # citation, and this guideline says nothing about it. The page asks the
    # question instead of answering it, which asserts nothing and is the more
    # useful half anyway (§12.8, WI-519).
reviewed: 2026-09-13
review_due: 2027-03-31
disclaimers: [medical]
---

## The short version {#short-version}

Before an operation or radiation, your team may add more scans. **Most of them
are extra minutes in the same MRI machine**, each one answering one specific
question about where things sit. One is different from all the others: in a
[functional MRI](#fmri) you have a job to do while you are in there, and you
cannot fail it. **What these scans make is a map, not a verdict.** They help
your team plan around the parts of your brain you would notice losing, and for a
critical decision your doctor may ask for more tests to confirm what one of them
shows.

## What are these extra scans? {#what-they-are}

Mostly they are not separate appointments at all. They are extra sets of
pictures taken during an MRI you were having anyway. The room is the same, the
machine is the same, the noise is the same, and the dye is the same.

What changes is the question being asked.

[Your MRI scan](/tests/mri) is the whole of that appointment, from the safety
form to the trip home, and everything on it still holds when a sequence is
added. So this page does not repeat it.

Here is what your team may add, and what each one answers:

- [**Functional MRI**](#fmri): which part of your brain does which job. The one
  where you have something to do.
- [**DTI**](#dti): where the wiring runs between those parts.
- [**MR spectroscopy**](#mrs): what the tissue is made of, chemically.
- [**Perfusion**](#perfusion): how much blood is going through it.
- [**PET**](#pet): what the tissue is doing. This one is its own appointment.
- [**Pictures of the blood vessels**](#vessels): where the arteries and veins
  run before anyone opens the skull.

If radiation is your treatment rather than an operation, the planning scan you
meet is a different thing again: a CT taken in a mask made for you, at a visit
of its own. [Radiation therapy](/treatments/radiation-therapy) walks through
that one, and none of the scans below replace it.

## Why am I having one? {#why}

Because somebody has a specific question, and an ordinary MRI does not answer
it.

These are not ordered automatically. A surgeon or a radiation oncologist asks
for the one that answers the thing they need to know:

- Which part of the brain handles speech, thought, sensation or movement here,
  so that the plan can steer around it.
- Where the main nerve pathways run, so that an operation does not cut through
  one.
- How much blood is going through the tissue, which is one of the things that
  suggests how fast it is growing.
- What the tissue is made of, which can help suggest what kind of tumor it is.

**The question worth asking is a short one: what is this scan for, and would the
answer change the plan?** Your team should be able to answer it directly, and it
turns another appointment into a step you can see the point of.

## What happens on the day {#the-day}

For everything except a PET, this is the answer:

1. **It is usually the same appointment.** The extra pictures are taken during
   your MRI, often without the day looking any different to you.
2. **Same table, same frame, same noise.** You lie on your back with a frame
   around your head, exactly as before.
3. **A line in your arm, if the scan needs dye.** A perfusion scan does. Others
   do not.
4. **You hold still.** For most of these there is nothing for you to do at all.
   The scanner does the work and you lie there.
5. **Except one.** In a functional MRI you have tasks to do, and there is a
   whole section about that below.

A PET scan is the exception to all of it. It happens in a different department,
on a different day, and [its own section](#pet) walks through it.

## How long does it take? {#how-long}

For the MRI add-ons, think in extra minutes rather than another appointment. How
many depends on how many sequences your team asked for.

The person running the scanner can tell you before you go in how long yours
should take, and it is worth asking, because the answer is often shorter than
people brace for.

A PET is longer, and most of the extra time is waiting rather than scanning. See
[the PET section](#pet) for the shape of that day.

## What does it feel like? {#what-it-feels-like}

Like more of the same. The main cost is time spent lying still.

If lying still is the part you find hard, that is worth saying before you go in
rather than after. [Your MRI scan](/tests/mri) has what your center can offer,
and all of it applies here.

## The scan where you have a job: functional MRI {#fmri}

!%functional MRI%This is the one that is unlike the others, because it is the
only one where what you do changes what the scan can show.

**What it is.** A functional MRI measures the small changes in blood flow that
happen when a part of your brain is working. Doing a task makes a part of your
brain work. The scan watches where the blood goes while you do it.

**What it is for.** It helps your team work out which part of your brain handles
a job such as speech, thought, sensation or movement, so they can plan around
those areas. That is the only use it is approved for in the United States:
planning an operation.

**What you actually do.** Simple things, one at a time, while you lie in the
scanner:

- Tapping a thumb and finger together, on one hand or the other.
- Moving a foot.
- Reading words.
- Reading a word backwards.
- Thinking of words without saying them out loud.
- Naming objects you are shown.

**Which tasks you get depends on where your tumor is.** A tumor near the part
that runs your hand gets a hand task. A tumor near the speech areas gets word
tasks. You may also be asked to hold your breath for a few seconds at a time.
That one is not testing your brain. It is checking the blood vessels, and
[the next part](#fmri-limit) explains why that matters.

**You cannot fail this.** Of everything on this page, it is the part to carry
into the room with you. The scan needs you to be able to do the tasks, so if one
is too hard, or you are having trouble doing it, **tell the person running it**.
They can give you different tasks or easier questions. That is not you failing
the test. In one study of people having this scan before surgery, swapping to
simpler tasks gave **more reliable results**, not worse ones. Saying "I cannot
do that one" makes the scan better.

**Holding still matters more here than usual.** This scan is especially
sensitive to head movement, and the radiology societies' own patient guidance
names who finds that hardest: anyone who is anxious, confused or in a lot of
pain. If that
is you, say so beforehand. It is a reason to prepare rather than a reason to be
brave.

**Keep your coffee normal.** This is an odd instruction and a real one. If you
drink coffee every morning, do not skip it on the day. If you hardly ever drink
it, do not have one that morning. Caffeine changes blood flow, and blood flow is
what this scan measures.

### The limit, and it is a real one {#fmri-limit}

A tumor can break the normal link between a part of the brain working and the
blood flowing to it. Doctors call that **neurovascular uncoupling**.

Here is why it matters to you. The scan does not watch your brain working. It
watches the blood. So near a tumor, **a part of your brain that is working
perfectly well can fail to light up**, and a quiet spot on the map looks like a
spot that is safe to operate on. That is the wrong way round to be wrong.

The breath-holding runs are one of the ways a team checks for this. Holding your
breath changes the blood flow everywhere without asking your brain to do
anything, so it shows whether the blood vessels in that area can still respond
at all.

There is also less experience with this scan than with most other kinds of MRI.
So your doctor may recommend more tests to confirm what a functional MRI shows,
when there is a critical decision to make such as planning brain surgery.

For a tumor near a speech or movement area, the test that confirms it is often
done in the operating room rather than beforehand. In an
[awake craniotomy](/treatments/awake-craniotomy) the surgeon checks the same
question directly, on the day, one small spot at a time.

## The map of the wiring: DTI {#dti}

**What it is.** The working parts of your brain are joined to each other by
bundles of fibers that carry signals between them. Think of them as cables. A
DTI scan shows where the major ones run. You may also hear it called
tractography.

**What it is for.** So a surgeon can see the cables and avoid them while taking
a tumor out. It can also show what the tumor has done to them: pushed a bundle
aside, grown into it, or broken it. Those are different problems with different
answers, and surgeons use the difference to plan both the route in and how much
they can safely take.

**What you do.** Nothing. No tasks, no extra injection. It is more minutes on
the same table.

## The chemistry graph: MR spectroscopy {#mrs}

**What it is.** Every other scan on this page makes a picture. This one makes a
graph. It measures the chemicals in a patch of brain and draws them as a set of
peaks.

**What the peaks mean.** Healthy brain tissue has a normal mix. Tumor tissue
usually does not, and the pattern of what is high and what is low is the useful
part:

- **Choline** is part of the wall around a cell. More of it means cells are being
  made and replaced faster than normal.
- **NAA** is found in healthy nerve cells, so less of it means nerve cells have
  been damaged or crowded out.
- **Creatine** is used as a steady reference to measure the others against.
- **Lactate** is not seen in a normal adult brain at all. It turns up where
  tissue is short of oxygen.

**What it is for.** It can help suggest what kind of tumor something is. It does
not name it. Only a piece of the tumor itself, looked at in a lab, can do that.
The gene results that go with the name come from that same piece, not from this
graph. [The words on your gene results](/tests/molecular-markers) takes them one
at a time.

**What you do.** Nothing extra. More minutes in the same machine.

## The blood supply scan: perfusion {#perfusion}

**What it is.** A scan that shows how much blood is going through different
parts of the brain.

**What it is for.** Tumors often have a bigger blood supply than normal areas of
the brain, and a faster-growing tumor often needs more blood. That makes the
blood supply a useful thing to know. It can also help decide the best place to
take a piece from, if tissue is being taken. [How tissue is
taken](/tests/biopsy) covers that step.

**What you do.** This is the one that needs dye. It goes into the line in your
arm quickly, during the scan, and you are asked to stay very still while that
set of pictures is taken.

Perfusion is used after treatment as well, for a different question entirely.
[Follow-up scans, and what the results mean](/tests/follow-up-scans#looks-worse)
is where that belongs, and it is not the same conversation as this one.

## The tracer scan: PET {#pet}

**What it is.** You are injected with a slightly radioactive substance that
collects mainly in tumor cells. The scanner then finds where it has gone. Most
of the other scans here show what tissue looks like. This one shows what it is
doing.

**What it is for.** Before treatment, the job it does here is helping choose
where to take a piece from. The spot taking up the most tracer is the one worth
sampling. It is more likely to help with fast-growing tumors than with
slow-growing ones.

It is also used after treatment, to help work out whether an area that still
looks odd on an MRI is more likely to be tumor or scar tissue. That is a whole
question of its own, and it does not have a clean yes or no in it.
[Follow-up scans, and what the results mean](/tests/follow-up-scans#looks-worse)
is the page that goes through what the fancier scans can and cannot settle.

**There is more than one kind of tracer, and it is worth asking which.** The
usual one is a form of sugar. In the brain that causes a problem, because normal
brain tissue uses a lot of sugar anyway, so a tumor can be hard to pick out
against everything around it. There is another kind, made from an amino acid
instead, which normal brain takes up much less of. Ask whether the kind your
team has in mind is done where you are being treated, or whether you would be
going elsewhere for it.

**The day is different from the others.** This is its own appointment:

1. You are usually asked not to eat for several hours beforehand, and to drink
   water only.
2. A small needle goes into a vein in your arm. You feel a pin prick.
3. Then you wait, and this is the part nobody expects. The tracer needs roughly
   30 to 60 minutes to collect where it is going. You spend that time sitting
   quietly, asked not to move about or talk, because moving a muscle sends the
   tracer to that muscle.
4. The scan itself takes about half an hour.
5. Afterward you can get on with your day. The tracer leaves your body over
   hours to days, and drinking water helps it along.

Apart from the injection, none of it hurts.

**If you are breastfeeding, raise it when you book.** The advice is to ask the
radiology department or your own doctor how to go about it, and it may help to
pump some milk beforehand and keep it for use until the tracer and any CT dye
have left your body. Tell them if there is any chance you are pregnant, too.

## Pictures of the blood vessels {#vessels}

Short, because there is not much to it from where you are lying.

A surgeon may want to know where the arteries and veins run before opening the
skull. There are MRI versions of that picture, and there is a CT version, which
in some cases shows the vessels in and around a tumor better than the MRI
version does. [Your CT scan](/tests/ct-scan) covers what a CT is like.

You do nothing during these. They are more pictures taken while you lie there.

## What is hard about these, and what helps {#whats-hard}

Three things, and each has something you can do about it.

**More time lying still.** That is the real cost of most of these. Ask the
person running the scan how long yours will take and whether there are breaks
between sets of pictures. There usually are, and knowing that changes the whole
hour.

**Another injection.** It is the same kind of line that goes in for an MRI with
dye. If your veins are hard to find, say so at the start rather than after the
third attempt, and ask whether the line can be put in by somebody who does the
difficult ones.

**Being asked to perform.** The functional MRI is the one that worries people,
because it feels like a test with a pass mark. It is not one. Ask to be told the
tasks in advance so you can think about them at home, and tell them on the day
if one is too hard. Both of those make the scan better.

And if being sent for more scans is itself the frightening part,
[follow-up scans](/tests/follow-up-scans#again-so-soon) has the section about
that, and it holds here too.

## What to sort out before the day {#before}

- **Ask what the scan is called and what question it answers.** You are allowed
  to write the name down and look it up.
- **Ask whether you have to do anything during it.** For most of them the answer
  is no, and knowing that is worth the question on its own.
- **Ask whether it needs dye or a tracer**, and whether that changes anything
  about eating, drinking or driving.
- **Ask whether it is done where you are being treated**, or whether you would
  be going somewhere else for it.
- **Tell them about any device or metal in your body.** These are MRI scans, so
  the same rules apply. [Your MRI scan](/tests/mri) covers the device card, and
  bringing it solves most of it.
- **Keep your caffeine normal on the day**, if a functional MRI is one of them.
- **For a PET, check the eating and drinking rules**, because that one usually
  has some.
- **If small spaces are hard for you**, say so in advance rather than on the
  day. [Your MRI scan](/tests/mri) lists what your center can offer.

## Who reads it, and how do I get the result? {#results}

How long it takes depends on where you are, so ask before you leave.

A functional MRI is read by a radiologist, a neurologist or a neuropsychologist
trained to read them, which is a wider set of people than usual. The rest of the
scans on this page are read by a radiologist, as your first MRI was.
[Your MRI scan](/tests/mri#who-reads-my-scan-and-how-do-i-get-the-result)
has more on who does what.

**The thing worth asking about here is different from a normal scan.** These
scans are asked in order to make a plan. So the answer may reach you as part of
a conversation about what is going to happen, rather than as a result on its
own. Ask which it will be: do I hear about this separately, or at the
appointment where we decide.

## What to ask your team {#questions}

- What question is this scan answering, and would the answer change the plan?
- Do I have to do anything during it, and can I know the tasks beforehand?
- Does it need dye, or a tracer, or another needle?
- Is it done here, or would I be going somewhere else?
- How long will I be in there, and are there breaks?
- I find lying still hard. What can you offer me?
- For a functional MRI: what happens if I cannot do one of the tasks?
- How much does the plan rest on this scan, and is anything checked again during
  the operation?
- Do I hear the result separately, or at the planning appointment?
- Can I take away a disc or a link to these images?

## Where to go next {#where-to-go-next}

- [Your MRI scan](/tests/mri) for the appointment itself, all of which still
  applies to every scan on this page.
- [Your CT scan](/tests/ct-scan) for the fast scan that usually came first.
- [An awake craniotomy](/treatments/awake-craniotomy) if your team has raised
  being awake for part of the operation, which is where the same question gets
  checked directly.
- [Brain surgery, step by step](/treatments/craniotomy) for the operation these
  scans are planning.
- [Radiation therapy](/treatments/radiation-therapy) for its own planning visit,
  which is a different appointment from these.
- [How tissue is taken](/tests/biopsy) for the step that gives the tumor a name.
- [Follow-up scans, and what the results mean](/tests/follow-up-scans) for what
  these same scans are used for after treatment.
- [The glossary](/glossary) for any word here you want in one line.
