---
title: "Your CT scan, and the one that started all this"
slug: tests/ct-scan
description: "The fast scan hospitals reach for when they need an answer now. What it is, why you had one instead of an MRI, what the dye and the x-rays actually mean, and why being sent for an MRI afterward is not somebody correcting a mistake."
tags: [tests, ct, scans, newly-diagnosed]
sources:
  # SOURCE DISCIPLINE FOR THIS PAGE, written down so it is not re-argued
  # (content-pipeline §12.13). The dossier section behind this page,
  # docs/research/tumor-guides/tests-library.md §2, is wrong in three places and
  # two of them are the whole of its patient-experience material. §12.14 says a
  # banned citation reads like a fixed claim, so each replacement is recorded
  # next to the ban.
  #
  # (1) §2.2, §2.3 AND §2.4 ATTRIBUTE EIGHT CLAIMS TO
  # https://www.radiologyinfo.org/en/info/headct AND THAT PAGE CARRIES NONE OF
  # THEM. Fetched twice through the summarizer and once in full with a browser
  # user agent (51,913 bytes, "Last reviewed on June 15, 2026"), it has no
  # "What will I experience" section at all. There is no warmth or flushing, no
  # metallic taste, no urge to urinate, no "within 30 minutes", no
  # "less than a minute" for the injection, and no kidney screening on it. Every
  # one of those claims is carried at patient level by ACS's own CT page
  # (imaging-tests/ct-scan-for-cancer), which is cited here and quoted below.
  # The RadiologyInfo page IS cited, for the seven things it does say.
  #
  # (2) §2.1 QUOTES ABTA AS SAYING "A CT is often used to detect the presence of
  # a tumor, and an MRI is used to gather more detailed information about the
  # tumor's size, location, and possible type." Two fetches of
  # abta.org/about-brain-tumors/brain-tumor-diagnosis/ did not return that
  # sentence. What the page does say is "CT scans are commonly used as screening
  # scans, and are preferred for looking at bones or the presence of blood or
  # abnormal brain tissue". The substance survives the quotation; the quotation
  # does not, so ABTA is not cited at all. ACS carries the comparison properly
  # and is already this corpus's source for it on /tests/mri.
  #
  # (3) §2.4 GIVES A HEAD CT WITH CONTRAST AS "about 4 mSv, roughly 16 months of
  # natural background radiation", sourced to webmd.com, and the dossier's own
  # §12 says to range it or omit it. Omitted, and RadiologyInfo says why in as
  # many words: "The radiation dose for this procedure varies." The page
  # publishes the DIRECTION instead, which ACS states and a reader can act on:
  # "The amount of radiation you get during a CT scan is more than a standard
  # x-ray."
  #
  # WHAT THE PAGE DELIBERATELY DOES NOT OWN, because a sibling already does.
  # The reader of this page meets four other scans and this page routes to all
  # four rather than explaining any of them:
  #   * THE MRI ITSELF -- the machine, the noise, small spaces, the gadolinium
  #     dye, the device card -- is /tests/mri (WI-506).
  #   * THE NAVIGATION SCAN before surgery and the scan a day or two after it
  #     are /tests/mri#navigation-scan, which explains both as one section.
  #   * THE PLANNING CT IN THE MASK is /treatments/radiation-therapy (WI-511),
  #     which walks through the whole simulation visit including the mask being
  #     made. RadiologyInfo lists "plan radiation therapy for cancer of the
  #     brain" among the uses of a head CT, so this page says the use exists in
  #     one sentence and sends the reader there for the visit.
  #   * SCANS AFTER TREATMENT, and every word about telling treatment change
  #     from growth, are /tests/follow-up-scans (WI-521).
  #
  # NO ESCALATION LIST AND NO CAREGIVER SECTION, both deliberate (§12.8,
  # WI-520). Nothing happens to this reader because of the scan: there is no
  # aftercare, nobody is discharged into anyone's care, and no source supports a
  # triage rule for a plain head CT. An invented tier list here would contradict
  # the corpus by omission, which is WI-524's defect.
  #
  # WHAT IS ON THE PAGE WITH NO FETCHED SOURCE, named here rather than left to
  # look sourced (§12.8, WI-525). Two sentences describe the MACHINE rather than
  # making a claim about care: that the scanner is a short open ring rather than
  # a tunnel, and that it is much quieter than an MRI. Neither is in any fetched
  # source; both are ordinary description, and they are the reason the page is
  # worth reading for somebody who found the MRI hard. They are written as shape
  # and carry no instruction. `/review` also flagged a third sentence in that
  # paragraph -- "people who find an MRI hard usually find this one easier" --
  # which was NOT ordinary description but an unsourced comparative frequency
  # claim about people (§12.8, WI-511). That one was cut, not annotated.
  #
  # ONE THING LEFT OUT ON PURPOSE AND LOGGED RATHER THAN WRITTEN. RadiologyInfo
  # says "A person who is very large may not fit into the opening of a
  # conventional CT scanner. Or, they may be over the weight limit -- usually
  # 450 pounds -- for the moving table", and the fMRI page says the same of MRI
  # scanners. That is a real access fact and it is missing from /tests/mri as
  # well, so putting it on this page alone would leave the corpus answering it
  # in one place out of two. It belongs to an item covering both scan pages.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging/how-diagnosed.html
    title: "American Cancer Society: Tests for Brain Tumors in Adults"
    accessed: 2026-09-13
    # Verbatim, and the spine of "why a CT and not an MRI": "A CT scan uses
    # x-rays to make detailed cross-sectional images of your brain and spinal
    # cord (or other parts of the body)." / "CT scans aren't quite as good as
    # MRI scans for looking at brain or spinal cord tumors, so they aren't used
    # as often." / "But they may be used if MRI isn't an option for some reason,
    # or if a scan is needed right away." / "CT scans can also show greater
    # detail of the bone structures near the tumor than an MRI." / "CT
    # angiography can provide better details of the blood vessels in and around
    # a tumor than MR angiography in some cases."
    # And the limit this page owes the reader (§12.8, WI-506): "Often, these
    # scans can give the doctor a good idea of what type of tumor it is. But
    # this can only be confirmed by removing some of the tumor tissue in a
    # procedure called a biopsy and then doing lab tests on it."
  - url: https://www.cancer.org/cancer/diagnosis-staging/tests/imaging-tests/ct-scan-for-cancer.html
    title: "American Cancer Society: CT Scan for Cancer"
    accessed: 2026-09-13
    # The patient-experience source, replacing the three RadiologyInfo
    # attributions above. Verbatim: a CT "combines many x-ray pictures to create
    # a detailed view of a specific area in the body"; it is "painless"; with IV
    # contrast you may feel "warmth spreading through your body" and some people
    # "say this feels like they 'wet their pants'", which "is only a feeling,
    # and it goes away quickly"; "You might also get a bitter or metallic taste
    # in your mouth that can last for several hours"; "A CT scan usually takes
    # less than 30 minutes"; "your head may be held still in a special device
    # during a head CT"; "the IV contrast dye can also cause kidney problems";
    # and "The amount of radiation you get during a CT scan is more than a
    # standard x-ray."
  - url: https://www.radiologyinfo.org/en/info/headct
    title: "RadiologyInfo.org (RSNA/ACR): Head CT (Computed Tomography, CAT scan)"
    accessed: 2026-09-13
    # Cited only for what the live page actually says. Verbatim: "CT scanning is
    # painless, noninvasive, and accurate." / "CT exams are fast and simple. In
    # emergency cases, they can reveal internal injuries and bleeding quickly
    # enough to help save lives." / "No radiation remains in a patient's body
    # after a CT exam." / "The x-rays used for CT scanning should have no
    # immediate side effects." / "There is always a slight chance of cancer from
    # excessive exposure to radiation. However, the benefit of an accurate
    # diagnosis far outweighs the risk involved with CT scanning." / "The
    # radiation dose for this procedure varies." / "The risk of serious allergic
    # reaction to contrast materials that contain iodine is extremely rare, and
    # radiology departments are well-equipped to deal with them." / "Unlike MRI,
    # an implanted medical device of any kind will not prevent you from having a
    # CT scan." / "CT is less sensitive to patient movement than MRI." /
    # And the whole of the prep block, which is where slot 7 comes from: "Tell
    # your doctor if there's a possibility you are pregnant. Discuss any recent
    # illnesses, medical conditions, medications you're taking, and allergies.
    # If your exam needs intravenous contrast, your doctor may tell you not to
    # eat or drink anything for a few hours beforehand. If you have a known
    # allergy to contrast material, your doctor may prescribe medications to
    # reduce the risk of an allergic reaction." / "Leave
    # jewelry at home and wear loose, comfortable clothing. You may need to
    # change into a gown for the procedure." / "Compared to MR imaging, the
    # precise details of soft tissue (particularly the brain, including the
    # disease processes) are less visible on CT scans."
    # The uses list is verbatim too, and it is what tells this reader why they
    # were sent: "bleeding caused by a ruptured or leaking aneurysm in a patient
    # with a sudden severe headache", "a blood clot or bleeding within the brain
    # in a patient with symptoms of a stroke", "brain tumors", "enlarged brain
    # cavities (ventricles) in patients with hydrocephalus", "plan radiation
    # therapy for cancer of the brain or other tissues", "guide the passage of a
    # needle used to obtain a tissue sample (biopsy) from the brain".
    #
    # THE BREASTFEEDING ANSWER IS A DISAGREEMENT AND IS PRINTED AS ONE (§12.8,
    # WI-511). Verbatim: "IV contrast manufacturers indicate mothers should not
    # breastfeed their babies for 24-48 hours after contrast material is given.
    # However, the most recent American College of Radiology (ACR) Manual on
    # Contrast Media reports that studies show the amount of contrast absorbed
    # by the infant during breastfeeding is extremely low." /tests/mri says
    # doctors take it as safe to carry on feeding after the MRI dye, and that is
    # not a contradiction: it is a different dye, and this page says so before it
    # says anything else about it.
reviewed: 2026-09-13
review_due: 2027-03-31
disclaimers: [medical]
---

## The short version {#short-version}

A CT scan uses x-rays to build a detailed picture of the inside of your head. It
is quick, it does not hurt, and it is the scan a hospital reaches for when it
needs an answer now. That is usually why you had one. **A CT can show that
something is there. It cannot say for certain what it is.** An MRI shows the soft
tissue of the brain in far more detail, and naming a tumor takes tissue rather
than pictures. So being sent for an MRI after a CT is not somebody correcting a
mistake. It is the next question being asked.

## What is a CT scan? {#what-it-is}

It is a scan that puts a lot of x-ray pictures together into one detailed view
of a part of the body. You may hear it called a CAT scan. Both words mean the
same machine.

The machine is a ring, not a tunnel. You lie on a table, and the table moves you
through the ring. The ring is short, so most of you stays out in the open room
the whole time.

It is painless.

## Why a CT and not an MRI? {#why-ct}

Because it is fast, and because it works on anybody.

For looking at a brain tumor, a CT is not as good as an MRI. Fine detail in the
brain shows up less clearly on it, including the detail of disease, and that is
exactly the detail your team needs next. None of that is a secret, and it is why
the MRI gets booked. A CT gets used anyway when an MRI is not possible, or when
a scan is needed right away.

**Right away is the part that matters here.** A CT is fast and simple, and in an
emergency it can find bleeding quickly enough to save somebody's life. It is the
scan that answers the frightening questions first. Is there bleeding? Is there a
clot? Are the fluid spaces inside the brain bigger than they should be, the way
they are with hydrocephalus? Hospitals use a head CT to look for all of those,
and for a tumor.

So if yours was done in an emergency room after a seizure or a bad headache, the
fast scan was the right scan. Nobody chose the weaker test. They chose the one
that could rule out the things that needed ruling out that night.

There are other things a CT is better at than an MRI:

- **Bone.** A CT shows the bone near a tumor in more detail than an MRI does.
  That matters when a tumor sits close to the skull.
- **Blood vessels, sometimes.** A CT can be used to make a picture of the
  vessels in and around a tumor, and in some cases it shows them better than the
  MRI version does.
- **Devices.** Having a device fitted does not stop you having a CT. It can stop
  you having an MRI, or at least slow it down while the staff check.

**What no scan can do is name it.** Pictures narrow the list, and sometimes they
narrow it a long way. Only a piece of the tumor itself, looked at in a lab, can
say for certain. [How tissue is taken](/tests/biopsy) is the step after the
pictures, when they have gone as far as pictures go.

## What happens, step by step {#step-by-step}

1. **Getting changed.** You may be asked to put on a gown and take off anything
   metal. [What to tell them before the day](#before) is the short list of
   things worth saying at the desk.
2. **A thin tube into a vein**, but only if dye is being used. Not everybody
   has it. It goes in with a small needle, the same as having blood taken.
3. **Lying down.** You lie on your back on a table. For a head scan, your head
   may be held still in a padded holder. It is there to stop small movements
   spoiling the pictures.
4. **Through the ring.** The table moves you through. You are asked to hold
   still while it runs.
5. **The dye.** Where one is used, it goes in through that tube.
6. **Afterward.** There is nothing to recover from, and **no radiation stays in
   your body after a CT**.

## How long does it take? {#how-long}

A CT scan usually takes less than 30 minutes, and the scan of your head is the
short part of that. Most of the time is getting you ready and getting you into
position.

The appointment is longer than the scan. That is normal, and it is not a sign
that anything difficult is happening.

## What does it feel like? {#what-it-feels-like}

Mostly it feels like lying still on a hard table for a few minutes.

It does not hurt. It is much quieter than an MRI, and you are not shut inside
anything. If the MRI was the part you dreaded, that is worth knowing before you
go in.

Where dye is used, the needle that puts the tube in feels like a blood test.

## The hard part is usually not the scan {#the-hard-part}

If you are reading this after the scan rather than before it, the scan itself is
probably not the part that is bothering you. Somebody said there was something
there, and then everything slowed down.

Those days have a page of their own.
[Something showed up on your scan](/tumors/all-brain-tumors) was written for
them: what you have really been told, the seven steps and which one you are on,
and what is worth asking for this week.

And if the waiting is bigger than that, it is not something to carry quietly
until the next appointment. [Get help now](/get-help-now) is people, today.

## The dye they may put in your arm {#the-dye}

**It is not the same dye as the one used for an MRI.** The CT one contains
iodine. The MRI one does not. If somebody has told you that you reacted to one
of them, it is worth knowing which, because they are different liquids with
different rules.

Not every CT uses dye. When one does, it is there to make certain things stand
out that would otherwise blend in.

**What it feels like.** As it goes in you may feel warmth spreading through your
body. Some people say it feels like they have wet themselves. It is only a
feeling and it passes quickly. You may also get a bitter or metallic taste in
your mouth, and that one can last a few hours.

**Reactions.** A serious allergic reaction to the iodine dye is extremely rare,
and radiology departments are well set up to deal with one. If you have ever
reacted to scan dye, say so before the day, because there is medicine that can
be given first to lower the risk.

## What about the radiation? {#radiation}

This is the question people ask last, quietly, and it deserves a straight
answer in both directions.

A CT does use x-rays, and the amount of radiation from one is more than from a
plain x-ray. There is always a slight chance of cancer from too much radiation.
That is true, and it is why these are ordered for a reason rather than as a
precaution.

The other half is just as true. **The benefit of an accurate diagnosis far
outweighs the risk of the scan.** The dose from a head CT varies, which is
exactly why this page will not put a number on it: a figure from one machine
tells you nothing reliable about yours. And no radiation stays in your body
afterward, so there is nothing left over from the scan you already had.

If you are going to have a lot of scans, the useful thing is not to worry about
the one you already had. It is to ask two questions going forward: how many of
these am I likely to need, and would an MRI answer the same question. Both are
ordinary questions and neither of them is rude.

## What to tell them before the day {#before}

Most of this is a short conversation at the desk, and all of it is easier before
the scan than after it.

- **Any kidney trouble.** The dye can cause kidney problems, so they need to
  know.
- **Any reaction to scan dye in the past**, and which scan it was for, because
  the CT dye and the MRI dye are different liquids.
- **Any chance you are pregnant.** Say it before the scan rather than after.
- **Whether you are breastfeeding**, and here the advice genuinely splits. The
  companies that make the dye say not to feed for a day or two afterward. The
  American College of Radiology says studies show the amount a baby takes in
  through breastmilk is extremely low. Those are two real answers from two real
  sources, and you are allowed to know that before you decide. Ask which one the
  department follows. This is about the CT dye.
  [Your MRI scan](/tests/mri) answers the same question about the MRI dye, and
  the answer there is not the same one.
- **Anything you take, and any recent illness or allergy.**
- **Eating and drinking.** If you are having dye, you may be told not to eat or
  drink for a few hours beforehand. Rules differ between hospitals, so go by
  what yours tells you.
- **Leave jewelry at home** and wear loose, comfortable clothes. You may be
  asked to change into a gown.

## Why am I having another scan? {#another}

Because each scan is asked a different question. Being sent for more of them is
ordinary, and each of the reasons below is a different question rather than a
doubt about the last answer.

- **An MRI, after the CT.** The usual next step, and the reason is at the top of
  this page: the MRI shows the brain in far more detail.
  [Your MRI scan](/tests/mri) is that appointment, start to finish.
- **A scan just before an operation, and one just after it.** Both catch people
  out and neither means something has changed.
  [Your MRI scan](/tests/mri#navigation-scan) explains the two together.
- **A CT for radiation planning.** If radiation is part of your treatment, one
  of these is done as part of a planning visit, with a mask made for you at the
  same appointment. [Radiation therapy](/treatments/radiation-therapy) walks
  through that whole visit.
- **A CT to guide a needle.** A CT can be used to steer the needle when a piece
  of tissue is being taken. [How tissue is taken](/tests/biopsy) covers that.
- **Extra scans to plan an operation.** Sometimes your team adds scans that
  answer one specific question about where things sit.
  [The extra scans before treatment](/tests/planning-scans) goes through them.
- **Scans once treatment is over.** A different job again: comparing, rather
  than finding.
  [Follow-up scans, and what the results mean](/tests/follow-up-scans) is that
  whole subject, vocabulary included.

## Who reads it, and how do I get the result? {#results}

How long it takes depends on where you are, so ask before you leave. What
follows is who does what, so you know what the wait is made of.

The pictures go to a radiologist, who reads scans of every kind for a living.
Their official report goes to whoever ordered the scan, and that person is the
one who talks it through with you.

If your CT was done in an emergency room, the report may come back quickly and
somebody may speak to you the same night. What they can tell you then is what
the pictures show. It is usually not the whole answer, because the whole answer
needs the MRI and often needs tissue.

Three things are worth asking before you go home: who reads this, who tells me,
and what happens after that.

## What to ask your team {#questions}

- Why a CT today, and will I still need an MRI?
- Is this scan with dye or without, and why?
- I have reacted to scan dye before. Which one was it, and what do you do about
  that?
- I have kidney trouble. Does that change anything?
- There is a chance I am pregnant. What do you want to do?
- I am breastfeeding. What does this department advise, and why?
- How many scans like this am I likely to have?
- Could an MRI answer the same question instead?
- What should I do if I feel unwell after the dye, and who do I call?
- Who reads this scan, and who tells me what it says?
- Can I take away a disc or a link to the images, in case somebody else needs to
  look at them?

## Where to go next {#where-to-go-next}

- [Something showed up on your scan](/tumors/all-brain-tumors) if you have been
  told there is something there and it does not have a name yet.
- [Your MRI scan](/tests/mri) for the scan that usually comes next, and for the
  machine, the noise and the dye.
- [The extra scans before treatment](/tests/planning-scans) if your team has
  mentioned adding more scans to plan an operation or radiation.
- [How tissue is taken](/tests/biopsy) for the step after the pictures.
- [Radiation therapy](/treatments/radiation-therapy) for the planning CT in the
  mask, and for the treatment itself.
- [Follow-up scans, and what the results mean](/tests/follow-up-scans) for the
  scans that carry on afterward.
- [Just diagnosed? Start here](/start) if this is all new.
- [The glossary](/glossary) for any word on your report you do not recognize.
- [Get help now](/get-help-now) if you need to talk to a person today.
