---
title: "Where your tumor is, and what that changes"
slug: where-your-tumor-is
description: "You know roughly where it sits, even if nobody has named it yet. This page turns the place on your scan report into plain words. It says what your team is weighing about it, and what to do when a place carries a warning of its own."
tags: [location, newly-diagnosed, surgery, questions-to-ask]
sources:
  # WI-567. Every quote below was re-fetched and read LIVE on 2026-09-24 with
  # curl and work_files/wi567/totext.py, not taken from a research dossier.
  # Section 12.8 ("a dossier is not a source"): a URL that resolves is not a
  # citation that holds.
  #
  # DELIBERATELY NOT CITED, because it could not be read:
  #   moffitt.org returned HTTP 403 on EVERY path tried, with three different
  #   browser user agents: the location page itself, /cancers/brain-tumor/ and
  #   /cancers/brain-tumor/diagnosis/. It is the publisher blocking us, not a
  #   bad URL. It was read through the Wayback Machine capture 20251011000826
  #   to close the one unverified hole in the "no comparator does this"
  #   conclusion, and that conclusion SURVIVED: Moffitt's location page is
  #   symptoms only, over six regions, with no surgery framing and no urgency
  #   section, and it leaves out the skull base and the pituitary even though
  #   Moffitt has separate type pages for both. Nothing on this page rests on
  #   it. A source we cannot open live is a source we cannot verify.
  #
  # REFUSED, and this is the item's Gate 1 ruling in one line: the backlog asked
  # for "a tumor can be small, slow and low-grade and still be an emergency
  # because of the plumbing". NOTHING verified supports it, and the best source
  # on this page prints the opposite qualifier, so it is not on the page. See
  # the Springer entry below.
  #
  # REFUSED A SECOND TIME, AT /review ROUND 1, AND THIS IS THE HARDER HALF. The
  # first draft of the fluid section carried "being watched is not the opposite of
  # being urgent: some people have a tumor nobody is in a hurry to remove and
  # fluid that needs sorting out this week". That is the SAME refused claim with
  # the words "small" and "low-grade" taken out, and nothing cited here asserts
  # that co-occurrence: ACS describes a drain or shunt going in before or after
  # tumor surgery, which is a tumor that IS being operated on, and Springer
  # describes choosing which to deal with FIRST. Both are about sequencing, not
  # about a tumor nobody intends to remove. Worse, the first version of
  # WhereYourTumorIsPageTests banned the refused VOCABULARY and then required the
  # paraphrase as a canary, so the guard against the refusal depended on the
  # refusal's substance being present. Deleted, and the guard now reads the
  # section for the SOURCED sentences instead. A refusal that only bans the words
  # it was written against is not a refusal.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/treating/surgery.html
    title: "American Cancer Society: Surgery for Brain Tumors in Adults"
    accessed: 2026-09-24
    # The backbone of this page, and the only source that carries the whole
    # surgical range in patient-facing words. Verbatim, all read live:
    #   the three goals: "Get a biopsy sample to determine the type of tumor",
    #   "Remove the tumor (or as much of it as possible)", "Help prevent or
    #   treat symptoms or possible complications from the tumor";
    #   "For some types of tumors, the surgeon may be able to remove all of it,
    #   but for other types this isn't usually possible.";
    #   "Surgery is not usually done to treat certain types of brain tumors,
    #   such as CNS lymphomas, although it may be used to get a biopsy sample
    #   for diagnosis.";
    #   "Surgery to remove the tumor may not be a good option in some
    #   situations, such as when: The tumor is deep within the brain / It's in a
    #   part of the brain that can't be removed, such as the brain stem / The
    #   person can't have a major operation for other health reasons" -- all THREE
    #   items, because the page prints all three and /review round 7 found the
    #   third attributed to this source with only the first two recorded here;
    #   "If a tumor blocks the flow of cerebrospinal fluid (CSF), it can
    #   increase pressure inside the skull. This increased intracranial pressure
    #   (ICP) can cause symptoms like headaches, nausea, and drowsiness, and may
    #   even be life-threatening.";
    #   "Removing the tumor can often help with this, but there are also other
    #   ways to drain away excess CSF and lower the pressure if needed.";
    #   and the two sentences this page's fluid section turns on: a shunt "may be
    #   done either before or after surgery to remove the tumor", and an
    #   external ventricular drain "can be put in place to relieve the pressure
    #   in the days before surgery".
    # NOT USED: "maximal safe resection" and the report words for how much came
    # out. /treatments/craniotomy already owns both, and section 12.10 says
    # route, do not restate.
  - url: https://www.aans.org/patients/conditions-treatments/anatomy-of-the-brain/
    title: "American Association of Neurological Surgeons: Anatomy of the Brain"
    accessed: 2026-09-24
    # The source for the REPORT WORDS, which is what this page adds that
    # [MECHANISM] does not. Verbatim: "The cerebrum or brain can be divided into
    # pairs of frontal, temporal, parietal and occipital lobes."; the frontal
    # lobes are "the largest of the four lobes"; "Broca's area, important in
    # language production, is found in the frontal lobe, usually on the left
    # side."; the temporal lobes are "located on each side of the brain at about
    # ear level"; the parietal lobe is "the brain's primary sensory processing
    # area" (phrasing checked against the page's own parietal section, which
    # says these lobes "interpret simultaneously, signals received from other
    # areas of the brain"); the occipital lobes are "located at the back of the
    # brain and enable humans to receive and process visual information"; the
    # cerebellum is "located at the back of the brain beneath the occipital
    # lobes"; the brainstem "consists of three structures: the midbrain, pons
    # and medulla oblongata"; "The third ventricle connects with the fourth
    # ventricle through a long tube called the Aqueduct of Sylvius."; the
    # posterior fossa is "a cavity in the back part of the skull which contains
    # the cerebellum, brainstem and cranial nerves 5-12"; the ventricle count and
    # the word a report actually prints, recorded at /review round 2 because round
    # 1 rewrote that sentence and left it without a quote behind it: "The
    # ventricular system is divided into four cavities called ventricles" and "Two
    # ventricles enclosed in the cerebral hemispheres are called the lateral
    # ventricles (first and second)." and "The third ventricle is in the center of
    # the brain"; /review round 8 dropped the page's "at the top / in the middle /
    # lower down" phrasings for the brainstem's three parts and the fourth
    # ventricle, because this source names them without placing them; and the
    # pituitary is
    # "attached to the base of the brain (behind the nose) in an area called the
    # pituitary fossa or sella turcica". Already a trusted corpus source on
    # /treatments/shunts and in blocks/mechanism.md.
  - url: https://www.mskcc.org/cancer-care/types/skull-base-tumors
    title: "Skull Base Tumors | Memorial Sloan Kettering Cancer Center"
    accessed: 2026-09-24
    # Verbatim, re-read live today rather than inherited from the block: "the
    # skull base refers to the base or floor of the cranium, the part of the
    # skull on which the brain rests." WI-568 settled this term in
    # /tumors/meningioma's favor after the block contradicted it, and this page
    # must say the SAME thing as both of them: the whole floor, not one spot.
    # AND THE SENTENCE THIS WHOLE PAGE IS ABOUT, found at /review round 1 on a
    # source already cited here: "A skull base tumor refers to the location of the
    # tumor. But skull base tumors are not all the same. There are different types
    # of tumors." It then lists them, and four have pages here: "acoustic
    # neuromas", "chordomas", "craniopharyngiomas", "meningiomas", "pituitary
    # adenomas" (its list also has chondrosarcomas and paranasal sinus cancers,
    # which have no page here). /review round 2: the first version of this said
    # "four" and justified the omission as "only the four a reader can follow",
    # which was false -- /tumors/pituitary-tumor exists and the page links it in
    # the very next entry. Five are named, and the pituitary one says so.
    # This is the one region entry whose list of types is SOURCED. The pituitary
    # entry also names two, from this site's own pages rather than from a source,
    # and /review round 6 caught what that cost: it said "the two growths here we
    # have written about", and /tumors/meningioma and /tumors/cns-germ-cell-tumor
    # both name that same spot in their own reader text. NAMING N ASSERTS THERE IS
    # NO N+1 -- the third time this item made that mistake, after "one place" and
    # "two places" in the escalation exception. Both entries are open now, and a
    # test bans closed counts in the region entries.
  - url: https://www.neurosurgery.columbia.edu/patient-care/conditions/brainstem-glioma
    title: "Columbia Neurosurgery: Brainstem Glioma"
    accessed: 2026-09-24
    # THE SENTENCE THAT MAKES A LOCATION-KEYED TABLE IMPOSSIBLE, and it is why
    # this page can list nine regions without listing nine risks. Verbatim:
    # "Unlike DIPGs, focal gliomas tend not to weave into neighboring tissue,
    # making tumor removal more feasible. Because safe removal is not generally
    # possible for DIPGs, brain tumor surgery is not generally recommended for
    # patients with DIPGs." Two tumors in the same place, opposite answers.
    # Also: "A biopsy is typically not performed because the brainstem is a
    # difficult area to access and the results rarely affect treatment
    # decisions."; and "A patient's age, as well as the tumor location and tumor
    # type, are all considered in order to choose the optimal treatment."
    # The DIPG specifics are NOT restated here. /tumors/dipg owns them,
    # including that centers differ on whether to biopsy.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8563316/
    title: "Goldbrunner et al: EANO guideline on the diagnosis and management of meningiomas (Neuro-Oncology)"
    accessed: 2026-09-24
    # The one thing /treatments/craniotomy does not say: the list of what a
    # surgeon weighs is written down. Verbatim: "EOR is determined by tumor
    # location, consistency, size, and proximity or involvement of critical
    # neurovascular structures." ("EOR" is extent of resection; the acronym is
    # not published on a page written for readers.) And the limit on it:
    # "striving to achieve a gross total resection should not be at the expense
    # of neurological or cognitive function."
    # NOT USED: everything about radiosurgery doses and fractions. Clinician
    # level, and /treatments/stereotactic-radiosurgery owns that subject.
  - url: https://link.springer.com/article/10.1186/s44201-022-00013-6
    title: "Intracranial emergencies in neurosurgical oncology: pathophysiology and clinical management"
    accessed: 2026-09-24
    # THE SOURCE THAT REFUSED A BACKLOG CLAIM. It is the only one read for this
    # item that addresses SIZE, and it prints the opposite qualifier to the one
    # the backlog asked for: "CNS tumors, especially intraventricular tumors,
    # can when large enough block CSF flow and cause obstructive
    # hydrocephalus." So "small, slow and low-grade and still an emergency" is
    # NOT on this page, in the one section where inventing a fact would be
    # least forgivable.
    # What it DOES support, and it carries the whole useful part of that claim:
    # "Hydrocephalus can serve as a determining factor for ICP management,
    # surgical approach, and the decision to deal first with either
    # hydrocephalus or the tumor." Plus the narrow-channel point, on pineal
    # region tumors: "hydrocephalus occurs due to aqueductal compression."
    # Already a trusted corpus source in blocks/mechanism.md.
    # NOT USED, deliberately: "TAE is more common with frontal and temporal
    # tumors and less common with occipital and infratentorial tumors." That is
    # a location-keyed risk statement, which is the one thing every Wave 6 item
    # is forbidden to publish, and it is clinician level besides.
  - url: https://europepmc.org/article/MED/40397319
    title: "Preoperative assessment of tumor eloquence and resectability: an international survey (Journal of Neuro-Oncology)"
    accessed: 2026-09-24
    # Read through Europe PMC because pmc.ncbi.nlm.nih.gov served a reCAPTCHA
    # page for PMC12367934 (section 12.8's rule for a gated source: look it up
    # in Europe PMC). This is the source for "why there is no list here", and
    # NO PERCENTAGE from it is published: the item's own acceptance bans them, so
    # the proportions are stated qualitatively ("most of them", "only a
    # minority"). The two COUNTS that are published are spelled out and
    # attributed in the sentence that prints them, which is §12.8's rule for a
    # figure from one study. Verbatim: "Twelve glioma and glioblastoma cases were
    # presented to assess opinions on tumor location eloquence and preferred
    # surgical approaches." (recorded at /review round 1, which found the page
    # printing "twelve" against a front-matter entry that had not kept the
    # sentence it came from); "157 neurosurgeons
    # from 25 countries responded to the survey."; "Two-thirds (68%) agreed on
    # the need for a standardized definition of eloquence, while only 23%
    # applied existing eloquence grading scales."; "In patient cases,
    # variability was observed at four levels of decision-making: (1) degree of
    # eloquence; (2) preferred surgical modality; (3) use of intraoperative
    # mapping; (4) the preferred mapping modality (asleep or awake)."; and "This
    # lack of consensus limits the reliability of eloquence as a descriptor of
    # tumor location, affecting patient care and comparability across studies."
    # The SECOND OPINION conclusion that follows from it is ROUTED, not written
    # again: /tumors/all-brain-tumors already says "asking another team what
    # they would do is an ordinary thing to do".
  # A SOURCE THE ITEM'S ACCEPTANCE NAMED AND THIS PAGE DOES NOT CITE, recorded
  # rather than silently dropped: NCI's PDQ for health professionals. It was named
  # in the backlog as a source for the surgery framing, on the strength of its
  # giving three locations their own treatment sections -- which is the argument for
  # this page EXISTING rather than a claim any sentence on it needs. Everything the
  # surgery section says is carried by the ACS page, EANO and Columbia, all three
  # patient-facing or read live, and §12.1 bars NCI's PATIENT PDQ for naming and
  # grading while the HP version is clinician-level throughout. So nothing here
  # rests on it, and no sentence was written that needed it. WI-571 should re-ask
  # the question for itself rather than inherit this.
  #
  # HOW SOURCES ARE NAMED ON THIS PAGE, recorded at /review round 3 because the
  # mix looks like inconsistency until the rule is stated. A source a reader could
  # look up and read is NAMED (the American Cancer Society, Columbia's
  # neurosurgeons). A clinician-level paper is described by KIND and not named ("a
  # guideline for one common tumor type", "a review of emergencies in this field",
  # "a survey of neurosurgeons in twenty-five countries"), because a reader who
  # follows a name to a paywalled guideline is worse off than one who was told what
  # kind of thing it was. Every URL is in this front matter either way, which is
  # where provenance belongs (section 12.8: sources are not a section).
  #
  # VOCABULARY, decided with a corpus check rather than by reflex (section
  # 12.8's own rule). This page teaches four new words: frontal, temporal,
  # parietal and occipital lobe. "frontal lobe" already appears in the READER TEXT
  # of TWO other pages (/tumors/astrocytoma and /tumors/oligodendroglioma) with no
  # glossary entry, which is a real gap, so it gets one. Corrected at /review round
  # 2: an earlier count said THREE and included /treatments/awake-craniotomy, where
  # the words are in a front-matter comment -- WI-568's "22 files, four of them
  # comments" error in a new place, and the number is now derived in a test rather
  # than written here. The entry is SUPPRESSED on this page because this page
  # defines the word, and at /review round 3 it was suppressed on
  # /tumors/astrocytoma too, which glosses it inline ("the frontal lobes, the front
  # of the brain") -- so the tooltip fires on /tumors/oligodendroglioma and nowhere
  # else today. That is a per-page decision any edit can reverse, where a missing
  # entry is a gap every later page inherits, and Wave 6's remaining items will say
  # the word. "posterior fossa" is suppressed here for the same reason: this page
  # gives its definition in the sentence the popover would attach to.
  # The other three lobe words appear NOWHERE in the corpus today,
  # and WI-519's rule is that an entry defined and used in one place fires
  # nowhere. They are refused for now and glossed inline instead. The trigger
  # for adding them as a set is recorded: WI-569 and WI-570 put location
  # sections on the glioma family and will be the second user.
reviewed: 2026-09-24
review_due: 2027-03-31
disclaimers: [medical]
---

## The short version {#short-version}

You may know roughly where it sits before anyone tells you what it is. That is a
normal way round, and this page is for that moment.

**Where it sits does not name it.** A name usually comes from tissue, tested in a
lab, and where it sits helps decide whether taking a piece is worth the risk at
all.

**What where it sits does shape is the plan**: whether an operation is offered at
all, whether all of it or part of it can come out, and whether the first job is
the tumor or the fluid around your brain.

**The clearest case where the place itself makes something urgent is the fluid**,
and [when where it sits makes it urgent](#the-fluid) is that section. It is not the
only reason to call anybody, and it says what to do when a place or a type has a
warning of its own.

**There is no list here of which places are dangerous**, and
[the reason](#no-list) is worth the two minutes.

## What "where it is" tells you, and what it does not {#what-it-means}

A scan gives your team two things at once: a picture, and a place read out of it.
The place is the part of the report that reads like directions.

**The place is real information and you can hold on to it.** It explains a lot
about what you have noticed, and it shapes what an operation would involve.

**The place is written down in the report on your scan**, and when that report
comes back is set locally rather than by any rule. [Your MRI scan](/tests/mri)
covers that appointment and says so plainly.

**The place is not a diagnosis.** Many different growths can sit in the same
spot, and they are treated differently once they are named.
[Why the scan cannot say on its own](/tumors/all-brain-tumors#why-the-scan-cannot-say-on-its-own)
is the fuller answer to why a picture cannot close that gap.

So two people can be told "it is in the brain stem" and be given two different
plans. That is not one of them being brushed off. It is the same place holding two
different things.

**If your team has given you a word for the type**, its own page is more use to
you than this one. [The tumor types we have written about](/tumors) lists them.
If they have not, that is an ordinary position to be in during the first week, and
the rest of this page is written for you. Where it says "your tumor", read it as
"whatever this turns out to be".

**And if you are here about your child**, read "you" and "your report" as "your
child" and "your child's report". The words on a child's scan report are the same
words, and that is the part of this page that transfers cleanly.

**One part of it needs a warning, and it is the surgery section.** Two of the three
sources behind it are written about adults, and childhood tumors are not treated
the same way. The fluid section is not adult-only, and the signs of it in a baby
are different from the signs in an adult.
[Brain tumors in children](/tumors/pediatric-brain-tumor) has that list, and it is
the page to read alongside this one.

## Why your team keeps coming back to where it sits {#why-it-matters}

Because it is one of the few things a surgeon can plan from before there is
tissue.

A guideline for one common tumor type spells out what that planning weighs. How
much can be taken out, it says, is decided by where the tumor is, how firm it is,
how big it is, and how close it sits to blood vessels and nerves that matter.
Four things at once, and the place is one of them.

**What is next to it counts too.** A tumor in an open part of the brain with
nothing important beside it is a different operation from one the same size
wrapped around a big blood vessel. The scan is read for both.

**If more than one spot was found, ask about each of them.** More than one spot
does not by itself say where they started, and that fork changes the whole plan.
[Did it start in my brain, or somewhere else?](/tumors/all-brain-tumors#primary-or-secondary)
is where it goes.

**And a place is something you can ask about today.** The grade may turn out
differently than expected, and the plan may change once there is tissue, but where
the scan found it is already known. That is why your team keeps returning to it,
and why there is no reason to hold the question back.

## The regions, in plain words and in your report's words {#the-regions}

Nine places, in the order this site uses elsewhere. Each one gives the plain
name first and then the word your scan report is likely to use, because the word
on the paper is often the only clue you were handed.

**These nine are places in the head.** A tumor in or pressing on the spinal cord
is a different subject with different rules, including different reasons to call
for help, and [tumors of the spinal cord](/tumors/spinal-cord-tumor) is the page
for it.

**If the word on your report is not one of these nine, that is normal and it does
not mean yours is unusual.** Reports use finer words than these. Some name a spot
inside one of the nine, and some name a line rather than a place: **midline**, for
instance, means at or near the center line, and more than one of the nine sits on
that line. Ask which of these nine yours is in, or nearest to. It is a short question
and it does not need an appointment of its own.

**These entries do not list symptoms.** That question has a better answer of its
own, region by region, and it lives on the page for the first week:
[what a tumor in each place tends to
cause](/tumors/all-brain-tumors#where-is-this-coming-from).

### The front of your brain {#front}

Above and behind your forehead.
!%frontal lobe%Your report may call this the **frontal lobe**. There are two, one on
each side, and they are the largest of the four lobes.

You may also see **Broca's area** written down. That is a part inside the front of
the brain that helps put words together, and in most people it is on the left
side.

### The side of your brain, near your ear {#side}

Your report may call this the **temporal lobe**. There are two, and they sit at
about ear level on each side of your head. Other pages here call the same place the
side of the brain near the temple, which is the same spot.

### The upper back part of your brain {#upper-back}

Your report may call this the **parietal lobe**, and there are two of these as
well. It is the main place where what your body feels gets put together with
everything else your brain knows.

### The back of your brain {#back}

At the very back of your head. Your report may call this the **occipital lobe**.
There are two, and they are where what your eyes send gets turned into seeing.

### Low at the back, under everything else {#cerebellum}

Your report may call this the **cerebellum**. It sits at the back of the brain,
underneath the lobes above it.

!%posterior fossa%It may also name the space rather than the part. The
**posterior fossa** is a hollow in the back of the skull that holds the
cerebellum, the brain stem and several of the nerves to the face and head.

### The stalk the brain sits on {#brainstem}

Your report may call this the **brain stem**, or it may name one of the three
parts it is made of instead: the **midbrain**, the **pons** and the **medulla**,
which a report may print in full as the medulla oblongata. A report may use one of
those smaller words instead of the big one, so one that never says "brain stem" can
still be about it.

**You may have read that nothing can be done here. Do not settle that from a
location.** [Can they just take it out?](#can-they-take-it-out) is where this page
takes that question.

### Deep in the middle, where the fluid runs {#ventricles}

Your report may call these the **ventricles**. There are four, and it may name
which one: the two **lateral ventricles**, one in each half of the brain, the
**third ventricle** in the center of the brain, and the **fourth ventricle**.

**One word here is worth knowing.** The third and fourth ventricles are joined by
a long tube called the **aqueduct**, which a report may print in full as the
aqueduct of Sylvius.
[When where it sits makes it urgent: the fluid](#the-fluid) is the section about
what happens if the flow through there is held up, and it is worth reading if this
is your region.

### The floor of the skull {#skull-base}

Your report may call this the **skull base**. It is an umbrella word, and it
means the whole floor of the skull, the part the brain rests on. That is how
Memorial Sloan Kettering puts it, and it is worth knowing the word covers a wide
area rather than one spot.

**So "skull base" on its own does not tell you much**, and the same center says
why in almost those words: the term describes where a tumor is,
and the tumors found there are not all the same thing. Its list of the ones that
grow on that floor includes five we have written about:
[acoustic neuroma](/tumors/acoustic-neuroma), [chordoma](/tumors/chordoma),
[craniopharyngioma](/tumors/craniopharyngioma),
[meningioma](/tumors/meningioma) and
[pituitary tumor](/tumors/pituitary-tumor). That last one has its own entry just
below.

The useful question is which part of the floor yours is on, and you can put it to
your team in those words.

### Behind your nose, at the base of your brain {#pituitary}

Your report may call this the **pituitary**, or it may name the small hollow the
gland sits in. That hollow has two names on paper: the **pituitary fossa** and
the **sella turcica**. Anything called **sellar** is about the same area.

The gland sits behind the bridge of your nose, below the brain. More than one kind
of
growth turns up there, and two of the ones we have written about are
[pituitary tumor](/tumors/pituitary-tumor) and
[craniopharyngioma](/tumors/craniopharyngioma). Each of those pages starts from the
beginning, and [the tumor types](/tumors) has the rest.

**This is one of the places on this page with a warning of its own**, and
[when where it sits makes it urgent: the fluid](#the-fluid) is where that sits.
Read it if this is your region.

## How long before this turns into a plan? {#how-long}

Not on the day of the scan, and that is the honest answer rather than a brush
off.

Here is the shape of it. The place is written down first, in the report on the
scan. What to do about it comes later, and for a case that is complicated or hard
to read it comes out of a meeting where the people who would treat you look at
your scans together. Where a piece of the tumor is going to be tested, what it
shows usually settles the rest.

So there are usually two conversations rather than one: what we can see, and then
what we are going to do. Knowing a second one is coming stops the first one
sounding like the whole answer.

**This page prints no timings, because they are not this page's to print.**
[Waiting for results](/tests/waiting-for-results) owns the wait. It has the
figures, where there are honest ones, and it says which parts vary by hospital.

## When where it sits makes it urgent: the fluid {#the-fluid}

**A tumor sitting where the fluid has to pass can block it, and a blockage does
not wait.** That is the whole of this section, and it can matter today.

The fluid around your brain has to pass through some narrow gaps, and a tumor at
one of them can hold it up.
[How a tumor in the brain causes symptoms](/tumors/all-brain-tumors#where-is-this-coming-from)
explains that part in full, and it is on the page for the first week rather than on
a page you need a diagnosis to find.

What matters here is what comes next. The American Cancer Society says a blocked
flow raises the pressure in the skull, that it can cause headaches, feeling sick
and drowsiness, and that it can become life-threatening. That is the reason
nobody leaves it alone.

**And the blockage can be dealt with on its own, without taking the tumor out.**
There is more than one way to drain the fluid off and bring the pressure down, and
none of them is an operation on the tumor itself.

**Which means the first operation may be about the fluid rather than the tumor.**
A drain or a shunt can go in before the operation on the tumor, or after it. In a
review of emergencies in this field, doctors describe deciding which to deal with
first, the fluid or the tumor, as a real decision they make. So being told "we
are going to deal with the pressure first" is not a delay and it is not a change
of subject. It is the order of jobs.

[Shunts and hydrocephalus](/treatments/shunts) is the whole of that subject: what
the operation is and what having one is like afterwards. If you already have one,
[signs a shunt is not working](/treatments/shunts#warning-signs) is a stronger
list than the general one below, and it is written for you rather than for
everybody.

**What to do with this today:** the list below is the general one for anybody
with a tumor in the brain. It says which signs mean an ambulance, which mean
calling your team today, and it does not treat those as the same thing. **Some
places carry rules of their own on top of it, and two of those are just below.**

[ESCALATION]

**The list above is the general one, and some places have rules of their own.**
Read the one that applies to you today, while nothing is happening.

**If your report says pituitary or sellar**, read
[pituitary tumor](/tumors/pituitary-tumor) once now. A tumor of the gland itself
can bleed or lose its blood supply, which brings on a sudden severe headache, and
**that one
is a 911 call rather than a wait for morning**. That page has both of its rules in
full, and [craniopharyngioma](/tumors/craniopharyngioma) has its own version.

**If it is in or pressing on your spinal cord**, parts of the list above were
written for a tumor in the brain, and the signs that matter most for you are
different ones. [Tumors of the spinal cord](/tumors/spinal-cord-tumor) sorts them,
and it does not treat them all as one kind of urgent.

**And those two are not the whole of it.** Several tumor types add a rule of their
own on their own page, on top of the list above. **Wherever you have been given a
stronger rule than the one above, the stronger one wins.** Over-reacting to a sign
costs you a phone call. Under-reacting can cost more than that.

So: start from the list above, take your own type page's rule over it wherever that
one is stronger, and if you cannot tell which is yours, tell your team what has
changed and let them place it. All of that is easier to read today than on the day you need
it.

## Can they just take it out? {#can-they-take-it-out}

This is the question underneath all of it, and there is a real answer, but it is
a range rather than a yes or a no.

The American Cancer Society groups the reasons for operating into three. To get a
piece of it so it can be named. To take out the tumor, or as much of it as can
come out. And to deal with symptoms the tumor is causing. They are not the same
operation, and one operation can be for more than one of them.
[Why am I having one?](/treatments/craniotomy#why-am-i-having-one) is the fuller
list, with a fourth reason on it and the question to put to your surgeon.

**Five things your team can land on.** Where the tumor sits is one of the things
that decides which.

- **All of it comes out.** For some types the surgeon can remove the whole
  thing. For others that is not usually possible, and which group yours is in
  depends on what it turns out to be.
- **As much as is safe comes out.** The guideline for one common type says it
  straight: aiming to get all of it must not cost you how you think or how your
  body works. [What happens in an operation on the
  brain](/treatments/craniotomy#how-much-came-out) explains how much came out,
  what the words in the report mean, and why a partial operation is not a failed
  one.
- **A piece comes out, to find out what it is.** For some tumor types surgery is
  not usually the treatment, and an operation is done only to get tissue.
  [How tissue is taken](/tests/biopsy) is that appointment.
- **No operation on the tumor.** The American Cancer Society gives three examples
  of when removing it may not be a good option. When it is deep inside the brain.
  When it is somewhere that cannot be taken out, such as the brain stem. And when
  somebody cannot have a major operation for other health reasons. **Those are
  examples, not a list of places**, and the paragraph below about the brain stem is
  why that difference matters. Treatment then starts somewhere else, and what it starts with
  follows what the tumor turns out to be rather than where it sits.
- **Nothing yet, and watching instead.** Not every tumor is treated when it is
  found. [Watch and wait](/treatments/watch-and-wait) goes through what is
  actually being watched, how often, and what happens if it changes.

**And the same place can give two different answers.** Columbia's neurosurgeons
describe this in the brain stem. One kind of tumor there does not spread into the
tissue around it, and removing it can be worth doing. Another kind cannot safely
be removed, and surgery is not usually recommended at all. Same place, opposite
plans, because the tumor is not the same tumor.

Which is why this page ranks no place against another. If one spot can hold both
of those answers, a spot on its own was never the answer.

**And none of the five is the surgeon giving up.** All five are plans, including the
one nobody wants to hear. A decision not to operate is a judgment that going in
would cost you more than it gave you, and it is made with the years after in view
rather than the day itself.

## Why there is no list here of which places are dangerous {#no-list}

You have probably looked for one. Every instinct says there should be a table:
this spot is fine, that spot is serious. This page does not have one, and the
reason is not squeamishness.

**Surgeons do not agree on it.** A survey of neurosurgeons in twenty-five
countries asked them to rate how much different parts of the brain matter. Their
answers varied. Most of them said the field needs one agreed definition of that,
and only a minority use any of the scales that already exist for it. The authors
say plainly that this lack of agreement limits how much that rating can be relied
on to describe where a tumor sits.

There is a word for a part of the brain that matters in that sense, and
[what can go wrong in an operation](/treatments/craniotomy#what-can-go-wrong)
teaches it, because that is the page where a reader meets it.

**The same twelve cases produced different answers.** The survey put twelve cases
to those surgeons. They differed on how much the spot mattered, on which
operation they would do, on whether they would map the brain during it, and on how
they would do that mapping. Four separate things, on the same twelve cases.

So a table on this page would be presenting one group's opinion as a fact about
your brain. That is not a gap in this site. It is the honest shape of what is
known.

**What that leaves you with is better than a table anyway.** The person who can
tell you what your location means is the surgeon looking at your scan. Put the
question to them in those words. If their answer is the thing this page cannot
give you, that is because it genuinely is theirs to give.

And if two surgeons can weigh the same case differently, wanting a second read of
yours follows from that rather than from anything being wrong.
[Asking somebody else to look](/tumors/all-brain-tumors#asking-somebody-else-to-look)
goes through how that works, before there is tissue and after.

## How to find out what yours is called {#finding-your-type}

A place plus a word is usually enough to find your way around. Here is how to get
the word.

- **Ask what it is being called for now.** Teams often have a working answer
  before the final one. Ask what is on the scan report and ask them to spell it.
- **Ask for a copy of the report on your scan.** It is the document the place is
  written in. Ask how to get one, because hospitals differ on how they hand it
  over.
- **If a piece is taken, the lab result is the name.**
  [Your pathology report, part by part](/tests/pathology-report) goes through the
  document it arrives in. **And if no piece is taken, you get a working answer
  rather than a confirmed one.**
  [Is there a way to find out without one?](/tests/biopsy#is-there-a-way-to-find-out-without-one)
  covers the exception it names, which is a place that is hard to take a sample
  from, and
  [how can they know what it is without a sample?](/treatments/watch-and-wait#without-a-sample)
  covers the version where nothing is being treated yet. Either way, ask what your
  team is going on instead, and how sure of it they are.
- **Then look it up here.** [The tumor types](/tumors) is the index, grouped so
  you can see which family a word belongs to before you read anything.

If the word you were given is not on that list, that is worth knowing rather than
worrying about. Some of the words people are handed are descriptions of a place
rather than names of a tumor, and we are still writing about those.

## Who decides, and how will you hear? {#who-decides}

**The person who weighs where it sits is a neurosurgeon.** They are the one who
can look at your scan and say what it means for an operation, and they are the
right person to put the question to.

**The decision is often not made by one person alone.** That is what the meeting
above is, and
[the meeting about your case](/tumors/all-brain-tumors#the-meeting-about-your-case)
explains who is in the room and what comes out of it.

**Before you leave, get two things: how the answer reaches you, and who to
chase.** Whether that is a phone call, a letter or the next appointment is a
local arrangement, and it differs from hospital to hospital. Ask what to do if
nothing arrives, and write down a name.

## What to ask your surgeon {#questions}

- Where exactly is it, and can you show me on the scan?
- What is right next to it that you are thinking about?
- Is an operation on the table, and what would the aim of it be?
- If all of it cannot come out, what decides how much does?
- Is the fluid draining normally, and is that something we need to deal with
  first?
- Does where it sits change what happens before the operation, like extra scans
  or being awake for part of it?
- If you are not operating, what are we doing instead, and when?
- Would another surgeon be likely to see this the same way?
- What would make you change this plan?
- Which of my symptoms come from where it sits, and which come from the
  pressure?

## Where to go next {#where-to-go-next}

- [The tumor types](/tumors) is the way in if you have a word for yours.
- [If you have just been told there is something on your
  scan](/tumors/all-brain-tumors) is the page for the first week, and it is where
  what a tumor in each place tends to cause is explained.
- [What happens in an operation on the brain](/treatments/craniotomy) is the
  operation itself, start to finish, and the report words that follow it.
- [Being awake for part of an operation](/treatments/awake-craniotomy) explains
  why a team suggests it and what it is like.
- [The extra scans before treatment](/tests/planning-scans) goes through the extra
  appointments that sometimes come before an operation, and what each one is for.
- [Shunts and hydrocephalus](/treatments/shunts) is the fluid, in full.
- [Waiting for results](/tests/waiting-for-results) is the wait between the scan
  and the plan.
