---
title: "Where your tumor is, and what that changes"
slug: where-your-tumor-is
description: "You know roughly where it sits, even if nobody has named it yet. This page turns the place on your scan report into plain words. It says what your team is weighing about it, and what to do when a place carries a warning of its own. If what changed is your sight, it says what that means for driving."
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
    # ear level"; the parietal lobe's own AANS sentence is that these lobes "interpret
    # simultaneously, signals received from other areas of the brain such as vision,
    # hearing, motor, sensory and memory" -- NOT the phrase "the brain's primary sensory
    # processing area", which stood inside this Verbatim: list until /review round 12 and
    # appears NOWHERE on the AANS page. It was the note's own paraphrase, and the
    # parenthetical beside it said so while the quotation marks said otherwise. **A
    # paraphrase inside a "Verbatim:" list is a misattribution to a source a reader can
    # follow** -- pre-existing from WI-567, found by the sweep this item ran, fixed here.
    # The occipital lobes are "located at the back of the
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
    # entry also names its types from this site's own pages rather than from a
    # source, and /review round 6 caught what that cost: it said "the two growths
    # here we have written about", and /tumors/meningioma and
    # /tumors/cns-germ-cell-tumor both name that same spot in their own reader text.
    # NAMING N ASSERTS THERE IS NO N+1 -- the third time this item made that
    # mistake, after "one place" and "two places" in the escalation exception. Both
    # entries are open now, and a test bans closed counts in the region entries.
    #
    # WI-577 TOOK THE N+1 THAT ROUND 6 ONLY WROTE DOWN. The pituitary entry named
    # TWO of the FOUR pages whose reader text claims that address, so the return
    # journey had no meningioma on it and no germ cell tumor either -- which is the
    # half of the seam /tumors/meningioma note (24)(c) filed as a backlog item
    # rather than a fifth comment. All four are named now, still in the open form,
    # and WhereYourTumorIsPageTests.TheSellarReturnJourneyNamesEveryPageThat
    # ClaimsTheAddress asserts the list in BOTH directions: every page named here
    # claims the address in its own reader text, and every page that claims it is
    # named here. The fourth one is the one a phrase list misses -- /tumors/cns-
    # germ-cell-tumor says it in plain words ("Just above the hormone gland. The
    # pituitary is a small gland under the brain") with no report-word in the
    # sentence, so the measurement had to be reconciled by hand before the number
    # was pinned. The reader-facing label is "germ cell tumor in the brain", which
    # is the one /tumors/pediatric-brain-tumor already uses; this page publishes no
    # acronyms in a region entry. THE OUTWARD JOURNEY IS STILL 1 OF 4 and is
    # DELIBERATELY left alone: only /tumors/pituitary-tumor links #pituitary,
    # meningioma routes to #skull-base because WI-568 settled that term in its
    # favor, and craniopharyngioma and cns-germ-cell-tumor arrive through
    # #your-sight. Recorded in content-pipeline §12.32 for /pm, not fixed here,
    # because a second route from meningioma to this page is a routing decision for
    # whoever owns /tumors/meningioma's section order.
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
    #
    # WI-577 DELETED THIS PAGE'S GLOSS OF THE EXCEPTION, AND BOTH REASONS ARE
    # worth having. The route used to read "covers the exception it names, which
    # is a place that is hard to take a sample from".
    #   (1) IT WENT STALE THE MOMENT /tests/biopsy's COUNT WAS FIXED. That page
    #   owns the exception list and now names TWO (a place too risky to sample,
    #   and markers alone for one rare group of tumors, per content-pipeline
    #   §12.32), so "the exception it names" was singular about a list of two --
    #   on the page that SENDS the reader there. A gloss of somebody else's list
    #   is a copy of its length.
    #   (2) IT WAS A RESTATEMENT, AND AssertDoesNotRestateTheCorpus CAUGHT IT
    #   RATHER THAN A HAND READ. The new /tests/biopsy paragraph leads "**A place
    #   that is hard to take a sample from.**", which shingled against this
    #   sentence eight words deep in three overlapping runs. §12.17's own rule
    #   applies: a route's label is the destination's title, so the route needed
    #   no gloss in the first place -- "covers the exceptions it names" says the
    #   same thing without copying the answer to the far side of the link.
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
    # hydrocephalus or the tumor" -- the paper's full stop follows "[ 49 ]", so a
    # quotation ending in one matches
    # nothing (/review round 12, the reference-marker case again). Plus the narrow-channel
    # point, on pineal
    # region tumors: "hydrocephalus occurs due to aqueductal compression."
    # Already a trusted corpus source in blocks/mechanism.md.
    # NOT USED, deliberately: "TAE is more common with frontal and temporal
    # tumors and less common with occipital and infratentorial tumors" -- again stopping
    # before the paper's "[ 51 , 52 ]" (/review round 12). That is
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
  # ---- WI-571: the tumors whose only name is a location ----
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11922996/
    title: "Imoto et al: Tectal glioma - clinical, radiological, and pathological features, and the importance of molecular analysis (Brain Tumor Pathology)"
    accessed: 2026-09-26
    # Open access, curled and read live with work_files/wi571/totext.py rather than
    # taken from the research dossier that named it. The source for WHERE the word
    # points. Verbatim, stopping before the reference marker: "Tectal glioma (TG) is a rare
    # glioma originating in the dorsal part of the midbrain, consisting of the superior and
    # inferior colliculi, occurring predominantly in children" -- the paper's full stop
    # follows "[ 8 , 12 ]", so a quotation ending in one matches nothing (/review round 11).
    # AND THE COHORT, which is what the page's "in the small group one study followed" rests
    # on -- recorded at /review round 10, because "small group" had no measurement behind it
    # on a page whose standard is a verbatim quote beside every used claim. Verbatim: "Six
    # cases were identified"; "The median age at diagnosis was 30.5"; "two of the six
    # patients were pediatric cases".
    # AND A TRAP WORTH MORE THAN THIS ITEM, found by failing this note's own grep twice:
    # THE FETCHED TEXT USES NON-BREAKING SPACES BETWEEN A NUMBER AND ITS UNIT, and an EN DASH
    # in the range. The source reads 30.5<U+00A0>years and 6<U+2013>45<U+00A0>years, so
    # "30.5 years" and "6-45" typed with an ordinary space and a hyphen match NOTHING. Every
    # "Verbatim:" quotation in this corpus that spans a number-unit boundary is exposed to
    # that, and a grep that finds nothing looks exactly like a quotation that was invented.
    # §12.19 says a quotation in a ruling is a claim and to grep it like one; this is the
    # other half -- **grep it in the characters the source actually used.** The quotations
    # above stop before the boundary for that reason.
    # AND THAT COHORT IS TWO THIRDS ADULT, which is worth saying next to this source's own
    # "occurring predominantly in children" -- and is the second reason the page has a
    # paragraph headed "And it is not only a childhood word." The first was the JNS series.
    # AND THE SOURCE FOR THE ONLY CLASSIFICATION THING THIS PAGE SAYS, which is that
    # the tissue has differed. Verbatim: "The integrated diagnosis, according to the
    # fifth edition of the World Health Organization Classification of Tumours of the
    # central nervous system, included two cases of PA and one case each of diffuse
    # high-grade glioma; diffuse midline glioma H3 K27-altered; glioblastoma; and
    # circumscribed astrocytic glioma."; "the DNA methylation profile of TG suggests
    # its classification as a distinct entity from other lower grade glioma (LrGG)s";
    # and "TG is typically classified as pilocytic astrocytoma (PA) or lower grade
    # astrocytic glioma". THE LAST TWO DO NOT AGREE WITH EACH OTHER, and that
    # disagreement -- a fact about the paper's own text -- is the whole of what the page
    # says about classification. It names no answer, and it does not say the question is
    # "open": that was the page's inference attributed to the authors with the verb
    # "reports", deleted at /review round 3 and banned in reader text. This note said it
    # too, one round longer, which is round 4's finding.
    # AND THE SENTENCE THE FLUID PARAGRAPH RESTS ON, verbatim: "The clinical course is
    # generally indolent and tends to present with neurological symptoms such as
    # intracranial pressure increases due to hydrocephalus obliterans." (Recorded here at
    # /review round 9. It had been quoted inside the paragraph about Childs Nerv Syst, which
    # at the time was a non-citation -- round 10 turned that into the Igboechi citation below
    # (10.1007/s00381-013-2110-z), so the quotation had been filed under a source the page did
    # not cite. This note said "170 lines below" until /review round 17, when it was 187: a
    # FIFTH cross-reference written as a distance in this item, rotted for exactly the reason
    # round 15 wrote down. Name the thing.)
    # §12.18 permits an address to carry a SYMPTOM and never a DIFFICULTY, and
    # that is the clause the page's fluid sentence sits under.
    # NOT USED, AND IT IS IN THIS SOURCE RATHER THAN IN THE HOSPITAL PAGES THE BACKLOG
    # WARNED ABOUT: "tends to have a good prognosis". §12.5, and the bar is a property
    # rather than a list of two publishers -- the barred wording was waiting in the
    # item's own best source.
    # NOT USED: the symptom list, and the clause an earlier draft cut off is the one that
    # matters -- it is the visual-field one. Verbatim, in the source's own case and stopping
    # before its reference marker: "The most common symptoms are headache, gait disturbance,
    # and ataxia related to hydrocephalus obliterans, along with visual field deficits and
    # cognitive dysfunction". (/review round 11 found this quotation shouting VISUAL FIELD
    # DEFICITS inside the marks -- a modified quotation, which §12.20 had just written a rule
    # against -- and ending on a full stop the paper prints AFTER "[ 8 , 10 , 12 ]", so the
    # string did not exist as typed.) The symptom question is routed to
    # /tumors/all-brain-tumors#where-is-this-coming-from, as it is everywhere else here --
    # and the last clause is WI-573's subject, so THIS source is a second place that ban
    # held, not a decoration on the first. An earlier version of this note stopped at
    # "obliterans", which is the truncation this item records as a defect for the other
    # source ("a NOT-USED list that stops at the first barred clause"). /review round 5.
    # AND THE RATIONALE IS NARROWED TO WHAT THE PAGE ACTUALLY DOES. It used to say this
    # page "never says what a tumor somewhere does to the reader", which is wider than the
    # truth: the section DOES say a blockage and the pressure it raises is how this shows
    # itself, sourced to the sentence above, and §12.18 permits an address to carry a
    # SYMPTOM and never a DIFFICULTY. What the page refuses is the symptom LIST.
  - url: https://link.springer.com/article/10.1007/s10143-021-01653-8
    title: "Management strategies for pediatric patients with tectal gliomas: a systematic review (Neurosurgical Review)"
    accessed: 2026-09-26
    # PMID 34609665. Paywalled for the full text; the ABSTRACT is served in full on the
    # publisher page and was curled and read live, then read a SECOND time through the
    # Europe PMC REST API and found identical but for TWO characters -- offsets 693 and
    # 765, where the API renders the publisher's em dashes as hyphens. /review round 2
    # measured that; the first version of this note said "character-identical", which is
    # the kind of claim that is true of the LENGTHS and gets written as if it were true of
    # the text. Two routes rather than one,
    # because the other two are closed: pubmed.ncbi.nlm.nih.gov serves a reCAPTCHA and
    # europepmc.org's own article page returns 403 to every user agent tried -- which is
    # the same wall §12.8 has a rule for and the same one WI-567 hit on PMC12367934.
    # THE SENTENCE THE WHOLE SECTION TURNS ON, verbatim: "Most tectal gliomas in the
    # pediatric population can be observed through radiographic surveillance and CSF
    # diversion." Plus "CSF diversion was the most performed procedure, occurring in
    # 317 patients (89.3%)." and "For management options, 232 patients were
    # radiologically monitored (65.4%)".
    # THE LICENCE FOR KEYING THIS TO AN ADDRESS AT ALL, named rather than inferred.
    # §12.19: "an address may key **what is aimed at** only where the source itself is
    # scoped by site, and never **how the reader does**" -- lower case inside the
    # marks and the emphasis outside them, because §12.19 prints it that way and a
    # quotation capitalised for emphasis is a modified quotation (/review round 13,
    # the third instance of that defect and the first on this quotation).
    # This review's entire scope IS the site, and what it keys to the site is what a team
    # aims at. Nothing in the section
    # ranks this address against any other, so unlike /tumors/chordoma's licensed pair
    # there is not even a harder half and an easier half to balance.
    # NO FIGURE FROM IT IS PUBLISHED, and the reason is printed qualitatively in the
    # section: "Resection was the most variable treatment option between individual
    # studies, ranging from 2.3 to 100.0%." A spread that wide BETWEEN THE STUDIES THE
    # REVIEW POOLED is the argument for publishing none of the numbers, which is the Wave 6
    # preamble's own argument arriving with a name on it. (This note said "across centres"
    # until /review round 9 -- one clause after the quotation that refutes it, and the
    # reading the test one file over bans by name. The page says "which study was counted"
    # and always has; the audit trail did not.)
    # NOT USED: "Abnormal ocular findings" -- the source continues with an EM DASH and lists
    # "gaze palsies, papilledema, diplopia, and visual field changes". The quotation stops at
    # the dash rather than retyping it as a hyphen, which is what an earlier version did and
    # /review round 12 caught: this publisher prints U+2014 and the EPMC route renders it as a
    # hyphen, a fact recorded two entries above and then not applied here.
    # WI-573 owns visual field loss and the
    # driving consequence, and this item would have taken that item's subject while
    # quoting a source correctly. **NOT the only place it could have**: Imoto says "visual
    # field deficits" in the symptom sentence recorded above, and this file names
    # "extraocular eye movement abnormalities" from the third source below. An earlier
    # version of this note called it "the one place", which is a closed count -- §12.17's
    # error again on this page. A count of one is the easiest closed count to believe,
    # because nobody recounts it. /review round 5.
    # AND ROUND 6 MADE THE FOLLOW-ON CLAIM TRUE INSTEAD OF SOFTENING IT. This note used to
    # add that the page's own suite "bans that string in reader text", and it did not: the
    # closed-count list ran only over sentences containing urgency words, and only inside
    # the fluid section. **A note claiming coverage a guard does not have is §12.17's
    # scoping failure running backwards -- it tells the next reader to stop checking.** The
    # ban is now page-wide, in ThePageNeverAssertsAClosedCount -- its own [Fact] as of
    # /review round 7, because the test it was first written into is about pre-empting later
    # Wave 6 items and that subject expires. (This note named the old host until round 8.
    # §12.20: grep the fix, not just the defect.)
    # NOT USED: "Surgical resection should be reserved for large tumors and/or those
    # that are refractory to other treatment modalities." That is a SIZE rule, and this
    # page refused a size claim twice already (§12.17) in the opposite direction. A
    # reader who measures themselves against "large" has been handed a rule nobody
    # meant them to apply.
    # NOT USED: "generally have a benign clinical course" and "more aggressive tumors".
    # §12.5 and CuratedPage.Characterisations.
    # AND THE REVIEW IS PEDIATRIC THROUGHOUT, which the section says out loud rather
    # than in this comment. This page already warns that two of its three surgery
    # sources are written about adults; this is the mirror of that warning.
  - url: https://doi.org/10.3171/2023.4.peds22485
    title: "Predicting disease progression and the need for tumor-directed treatment in tectal plate gliomas (Journal of Neurosurgery: Pediatrics)"
    accessed: 2026-09-26
    # PMID 37347621. Read LIVE off the publisher page through the DOI, which serves the
    # paper's FULL TEXT to a plain fetch -- Methods, Results and Discussion, not just the
    # abstract, which is more than the two closed routes give for the other clinician-level
    # source on this page. /review round 6 corrected this note, which had said "the whole
    # abstract"; the difference matters because a NOT-USED list drawn from an abstract is
    # incomplete for an article, which is the defect this file records twice already.
    # ADDED AT /review ROUND 1, AND IT CLOSED A CLAIM RATHER THAN DECORATING ONE. The
    # section said "nothing read for this page says the same of adults". This series makes
    # that FALSE: "The median patient age of the full cohort was 24 years" -- the range that
    # follows it is printed with an EN DASH (0 U+2013 73), so a quotation carrying it typed
    # with a hyphen matches the Europe PMC record and NOT the publisher page this entry says
    # it read. /review round 14, the fourth instance of that defect in this item and the
    # second on a fact this file had already written down twice. It is 170 patients against
    # the pediatric review's 355 children. An
    # absence claim is only as good as the last search, and this one had a source behind
    # it within one round.
    # WHAT IS USED, verbatim: "Tectal plate gliomas are rare, slow-growing tumors of the
    # midbrain that are discovered predominantly in the pediatric population. Because of
    # their indolent nature, treatment mainly consists of observation and management of
    # hydrocephalus." -- which is the SAME two-part approach the pediatric review found,
    # from a cohort that is half adult. And "the adult population had more instances of
    # incidental lesions", which is the one thing this page can hand an adult reader that
    # is neither a figure nor an outcome.
    # NOT USED, AND THIS IS THE WAVE 6 ARTIFACT ITSELF, IN A SOURCE THIS PAGE NOW CITES:
    # "lesion involvement of the pons ... significantly associated with worse radiographic
    # PFS" and "involvement of the lesion beyond the tectum" as a predictor of needing
    # treatment. That is a LOCATION KEYED TO AN OUTCOME -- the one artifact every Wave 6
    # item is forbidden to publish (PMC12367934: only a minority of surveyed neurosurgeons
    # apply any eloquence scale). It is recorded by name because the next person to open
    # this source will meet it in the abstract's own results.
    # AND THREE MORE FROM THE SAME SENTENCE, because a NOT-USED list that stops at the
    # first barred clause is the shape /review round 2 caught. "moderate T1 hypointensity,
    # moderate contrast enhancement" is a SCAN FEATURE keyed to an outcome, on the page
    # whose thesis is that a picture cannot name a growth; "extraocular eye movement
    # abnormalities at presentation" is an eye finding, which is WI-573's subject by the
    # same reasoning that kept the review's "visual field changes" out; and "an increase in
    # total lesion size" is a size-linked predictor, which is the size rule recorded for
    # the review arriving a second time from a different paper.
    # NOT USED: the 24% radiological-progression and 25% needed-treatment figures.
    # NOT USED, AND FOUND ONLY BECAUSE THIS FETCH IS THE FULL TEXT: the introduction's
    # "Neurological deficits are less commonly observed but may include nystagmus,
    # diplopia, seizures, and visual deficits." That is a FOURTH place WI-573's ban held.
    # /review round 8 found this entry filed under the SYSTEMATIC REVIEW instead, whose own
    # note says its full text is paywalled -- so the next person opening that paper to check
    # the quotation could not have found it, and could not have got at the text to be sure.
    # **A NOT-USED entry belongs to the source it was drawn from, and the check that it does
    # is to open the source.**
    # NOT USED: the 1-, 5- and 10-year progression-free survival rates, the 94/2/4%
    # follow-up split, and the hydrocephalus rates by age group. §12.5 and the item's own
    # acceptance.
    # NOT USED: "the natural history of these lesions lends to excellent long-term
    # survival". THAT IS THE THIRD SOURCE IN THIS ITEM TO CARRY THE BARRED WORDING the
    # backlog attributed to two hospital pages -- after Imoto's "tends to have a good
    # prognosis" and the review's "generally have a benign clinical course". The bar is a
    # PROPERTY, not a blocklist of publishers (§12.14).
  #
  # CHASED AND UNREADABLE, DO NOT CITE AND DO NOT PUBLISH WHAT IT MIGHT SAY. The
  # backlog names, as unverified, the claim that WHO CNS5 folds most tectal gliomas
  # into "diffuse low-grade glioma, MAPK pathway-altered", and asks for
  # 10.1007/s00401-026-03066-7 to be chased before any classification sentence is
  # written. CHASED. The DOI is real: Tauziede-Espariat, Metais, Aldape et al,
  # "Tectal glioma versus pilocytic astrocytoma: revisiting tumor classification in
  # light of molecular heterogeneity", Acta Neuropathologica volume 152 article 22,
  # Correspondence, published 2026-08-20, PMID 42622715. It cannot be read: the
  # publisher page says "This is a preview of subscription content" and carries no
  # abstract, and Europe PMC has isOpenAccess N, inEPMC N, no PMCID and a null
  # abstract, so there is no second route. §12.17's Moffitt rule therefore applies --
  # a source we cannot open live is a source we cannot verify -- and NO
  # CLASSIFICATION SENTENCE IS PUBLISHED. The claim is neither published nor
  # refuted; it is recorded as still unverified, with the identifiers, so the next
  # item does not chase it from scratch.
  #
  # TWO HOMES FOR AN "ALIAS" WERE CONSIDERED AND BOTH ARE REFUSED, because the word
  # "alias" matching is not an argument.
  #   taxonomy.yml's `also`: refused, and its own header is the reason -- aliases are
  # "aliases the classifier may be given; they never render" and must be "true
  # synonyms, never 'close enough'". A location is not a synonym of a type, and
  # §12.2 item 3 requires WHO CNS5 naming throughout. Adding it would put a place
  # into the closed list of diagnoses the classifier may emit.
  #   The glossary's `also`: a real candidate, because unlike the taxonomy it DOES
  # render ("Also called:" on /glossary) and it DOES feed tooltips. Refused by
  # WI-519's rule -- an entry defined and used in ONE place fires nowhere. This page
  # defines the word and the word appears nowhere else in the corpus, which is the
  # same refusal WI-567 wrote for temporal, parietal and occipital lobe. It buys the
  # search half nothing either: /glossary is a Razor page, not a curated page under
  # Content/pages, so ContentStore.SearchPages never enumerates it.
  #   AND THE SEARCH HALF NEEDS NO MECHANISM AT ALL, measured in the code rather than
  # assumed: SearchPages scores page.Markdown, which Parse sets to the COMPOSED BODY
  # after the front matter is sliced off. So the word in the PROSE is what makes this
  # page findable, a word in one of these comments is invisible to search and to the
  # reader alike, and no new field or index exists to add.
  - url: https://doi.org/10.1007/s00381-013-2110-z
    title: "Igboechi et al: Tectal plate gliomas - a review (Child's Nervous System)"
    accessed: 2026-09-26
    # PMID 23612874, Childs Nerv Syst 29:1827-1833. Not open access; the ABSTRACT was read
    # live twice today -- off the publisher page through the DOI, and again through the
    # Europe PMC REST record -- and the two agree.
    #
    # THIS ENTRY EXISTS BECAUSE /review ROUND 10 CAUGHT THE ITEM RECORDING IT AS UNREADABLE,
    # AND THAT IS THE ITEM'S SHARPEST FINDING. Rounds 2 through 9 carried a note saying the
    # backlog had named this source, that it was "searched and NOT read: it is not open
    # access and has no abstract in EPMC", and that the claim it was named for had therefore
    # been changed rather than kept "on a source nobody opened". The abstract was in hand the
    # whole time, in the JSON this item wrote at round 1. Three things were wrong at once:
    # the reason, the readability, and -- the one that matters -- the conclusion, because
    # this abstract SOURCES the claim round 1 deleted as "an inference wearing a citation".
    # Verbatim, and the emphasis is OUTSIDE the quotation marks because a quotation with
    # words capitalised inside it is a modified quotation -- the clause that matters is
    # "obstruct the aqueduct of Sylvius": "Tectal plate gliomas are generally benign
    # neoplastic lesions arising in the brainstem which can, with local extension, obstruct
    # the aqueduct of Sylvius and lead to hydrocephalus."
    # **An absence claim about a SOURCE is the same liability as an absence claim about the
    # literature.** This item made both. The literature one died in one review round; this
    # one survived ten, because nobody re-opens a source recorded as unopenable.
    #
    # WHAT IT IS USED FOR: nothing in reader text, and that is deliberate rather than
    # leftover. The fluid sentence already says what it needs to in Imoto's words ("This is
    # a spot where the fluid can be held up"), it grades where it needs to, and §12.18's stop
    # rule says an item stops widening once its property holds. **But the aqueduct adjacency
    # is SOURCED, and that is recorded here so the next item does not read round 1's deletion
    # as a prohibition:** it was deleted for being unsourced, and it is not.
    # AND IT IS A FOURTH CORROBORATION OF THE PAGE'S TWO-PART CLAIM, verbatim: management
    # "may range from diligent observation and periodic screening for advancing tumor
    # development, to cerebrospinal fluid shunting in an effort to resolve obstructive
    # hydrocephalus, to radio- and chemotherapy."
    # NOT USED: "generally benign". **That is the FOURTH source in this item to carry the
    # barred prognosis wording the backlog attributed to two hospital pages** -- after
    # Imoto's "tends to have a good prognosis", the review's "generally have a benign
    # clinical course" and the JNS paper's "excellent long-term survival". §12.14: the bar is
    # a property, not a blocklist of publishers, and four sources in one item is the proof.
    # NOT USED: the imaging description, the biopsy-for-definitive-diagnosis claim and the
    # endoscopy list. Clinician level, and /tests/biopsy and /treatments/shunts own the two
    # subjects a reader would need.
    #
    # AND THE OTHER HALF OF ROUND 1's DELETION STANDS: `roof` appears in none of the four
    # sources, so "the roof of the midbrain" was an inference and stays deleted. What the
    # page says is Imoto's own word for the place. Imoto's fluid quotation lives in ITS OWN
    # BLOCK above rather than here (/review round 9, applying to a used quotation the rule
    # this file writes for an unused one), and `aqueduct` is taught in the region entry,
    # where AANS is the source for it.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11913653/
    title: "Brain tumors and fitness to drive: A review and multi-disciplinary approach (Neuro-Oncology Practice)"
    accessed: 2026-09-30
    # WI-573. THE SOURCE FOR THE WHOLE SIGHT-AND-DRIVING SECTION, and it is NOT the
    # source the backlog named. Already cited on
    # /tests/neuro-exam-and-memory-testing for one narrow claim, which is what makes it
    # a corpus source rather than one page's find. (This said "this is its second
    # user" until /review round 3, and the same diff had already made it a third by
    # citing it on /seizures/living-with. A count in prose, falsified by the change
    # that wrote it.)
    #
    # THE BACKLOG NAMED CANCER RESEARCH UK's DRIVING PAGE AND IT DOES NOT CARRY THE
    # CLAIM. Read live 2026-09-30, HTTP 200, "Last reviewed: 08 May 2026" -- the date
    # the backlog gives, so it is the right page. The string "visual field" occurs
    # ZERO times on it and "visual" occurs zero times as a word. It has exactly two
    # vision sentences and both are pituitary-scoped UK regulation. The first makes
    # trouble with eyesight a reason a licence may be withheld and puts a number of
    # months on it -- A WAITING TIME, which WI-560 owns and this item is barred from,
    # and which is NOT QUOTED HERE for the reason two paragraphs down. The second is
    # a DVLA notification rule, and it carries no figure, so it can be quoted: "you
    # must not drive until you have recovered from treatment and do not have problems
    # with your vision." That second one is a driving PROHIBITION that follows an
    # exemption from the notification duty, not a notification rule -- /review round 3
    # corrected this note's description of it after reading the surrounding
    # paragraph rather than the sentence alone. Every other sentence on that page
    # THAT BEARS ON DRIVING is a
    # waiting time or a UK licensing duty, so there is nothing on it this page may
    # publish. (The scope matters: /review round 2 caught the first version saying
    # "every other sentence" full stop, which is false -- the page also carries plain
    # definitions of glioma grades and of what a biopsy is. The CONCLUSION survives
    # the correction; the universal did not.) It is therefore NOT CITED here at all --
    # recorded rather than silently dropped, and recorded with the measurement so the
    # next item does not re-fetch it hoping. (It is also barred for idiom on
    # /tumors/craniopharyngioma, WI-538, which is one of the pages carrying a
    # vision-and-driving sentence -- so the backlog's source could not have
    # served either home. (This said "the corpus's OTHER vision-and-driving
    # sentence" until /review round 3, four lines above the paragraph headed AND
    # "THE OTHER" WAS WRONG. 12.19: a ruling that contradicts itself gets copied
    # one paragraph at a time -- and this one contradicted itself inside one note.)
    #
    # AND "THE OTHER" WAS WRONG: THERE WERE FOUR, AND THREE OF THEM ROUTED NOWHERE.
    # The backlog says /tumors/craniopharyngioma carries "the one" sentence tying
    # vision to driving and the first draft of this note said "the other" -- a CLOSED
    # COUNT, 12.17's most-repeated error, and /review round 1 falsified it by
    # sweeping for sentences carrying a driving token AND a sight word.
    # /tumors/pituitary-tumor, /tumors/hemangioblastoma and /tumors/cns-germ-cell-
    # tumor each carry their own. TWO of the three had no route at all, and the third
    # had one that excluded this reader: /tumors/cns-germ-cell-tumor already linked
    # /seizures/living-with -- WITHOUT the #driving anchor, and only "if seizures are
    # part of it", so the reader whose problem is sight was excluded by the sentence
    # that looked like it was helping. It gains the anchor and a sight route. (This
    # note said "NONE of the three" until /review round 2 checked it against
    # `develop`. A claim about what three other pages do is three claims.) **/tumors/pituitary-tumor was the sharpest miss in the set**:
    # its version is almost word for word the restatement this item deleted from
    # craniopharyngioma ("is set locally. Your team knows the rules where you live; a
    # website does not, including this one"), and it is the page the SELLAR reader
    # most likely starts on -- the same reader this page's #pituitary door now sends
    # into the new section. All four route now, each in its own words, and
    # /tumors/pituitary-tumor's restatement is replaced rather than supplemented.
    # (True on the THIRD attempt. A later round put the first half of that sentence
    # back after the route -- "Either way, your team knows the rules where you
    # live" -- with an "Either way" that had no two ways, and this note went on
    # saying replaced. /review round 5. The corpus assertion cannot see it either,
    # because it bans two literal strings and says so.)
    # /tumors/hemangioblastoma's and /tumors/cns-germ-cell-tumor's own sentences are
    # KEPT, and the line between the two pairs is narrower than the first version of
    # this note drew it. All four ended by pointing the reader at their team, so that
    # is not the difference. **The two that went each added a sentence ABOUT THIS
    # SITE** -- "a website cannot, and that includes this one", "a website does not,
    # including this one" -- which is the site telling the reader what it will not do,
    # in place of telling them where to go. The two that stayed name the factors
    # driving turns on and stop. What all four were missing was the door.
    # (THIS NOTE WAS FALSE FOR TWO ROUNDS. It said /tumors/pituitary-tumor's
    # restatement was "replaced rather than supplemented", and a later round's
    # rewrite of that page PUT THE META SENTENCE BACK -- so the only live instance of
    # "a website does not, including this one" in the corpus was sitting on the page
    # this note said it had cleaned, and nothing asserted its absence. /review round 3
    # found it. **A deletion recorded in a note and not in an assertion is a deletion
    # that can be undone by the next edit to the same file** -- which is 12.19
    # finding 8 with the roles swapped: there the script lied about the file, here the
    # note did. It is asserted now, corpus-wide.)
    # **AND THE CLAIM IS AN ASSERTION NOW, NOT A SENTENCE HERE** -- see
    # EverySightAndDrivingSentenceInTheCorpusCarriesARoute. 12.19: assert the
    # COMPLEMENT, not the includers, because no single page was individually wrong
    # and no page-scoped test could see the gap.
    #
    # AND THE BARRED QUOTATION COULD NOT BE RECORDED VERBATIM, WHICH IS THIS ITEM'S
    # SHARPEST TOOLING FINDING AND IT WAS FOUND BY AN EXISTING GUARD RATHER THAN BY
    # READING. The first draft of this note quoted CRUK's waiting-time sentence in
    # full, the way this corpus records every other refused claim, and
    # SeizureContentTests.NoCuratedPagePrintsADrivingWaitingPeriod turned RED: it
    # reads the RAW file, front matter included, and splits per sentence on "driv".
    # THE GUARD WAS NOT TOUCHED, and it should not be. A comment does not render --
    # but a source title: two lines above it DOES (12.10, 12.20), the ban is on the
    # SHAPE rather than on the rendering, and a figure sitting in a comment is one
    # copy-paste away from prose. **A quotation is not exempt from a ban on the claim
    # it quotes.** The corpus's habit of pasting barred wording verbatim in order to
    # refuse it has a floor, and this is where it is.
    #
    # WHAT IS USED, verbatim, each beside the sentence it carries:
    #   "mostly present with some form of visual field loss, more commonly a
    # homonymous visual field loss (ie, affecting the same side, left or right, of the
    # visual field in both eyes)" -- the common pattern, and the reason the page says
    # "the same side in both eyes" rather than naming a side. **THE GLOSS IS THIS
    # PAGE'S, NOT THE SOURCE'S:** the source's word for that property is
    # "homonymous", and the page teaches "hemianopia" -- which the source lists as one
    # of the FORMS homonymous loss takes -- instead of teaching a fourth word. That is
    # a simplification and it is recorded as one, because an unrecorded simplification
    # reads as a misattribution to the next person who greps the source.
    #   AND THE PAGE EXTENDS hemianopia TO BOTH PATTERNS, WHICH THIS SOURCE DOES NOT.
    # It lists hemianopia only among the forms HOMONYMOUS loss takes; for the other
    # pattern it says "binasal or bitemporal" and never uses the word. The page says
    # "you may hear either one called hemianopia, and that word does not say which",
    # which is standard general vocabulary and is what a reader handed the word on a
    # report needs -- but it rests on general usage rather than on this source's
    # scope, and /review round 5 caught the record documenting every other gloss and
    # being silent on this one. An unrecorded EXTENSION is the same liability as an
    # unrecorded simplification, one direction over.
    #   "including partial or complete hemianopia, quadrantanopia or scotoma" -- the
    # two report words taught. quadrantanopia is NOT taught: a reader handed it can
    # read the hemianopia sentence, and 12.8's vocabulary rule is that a word earns
    # its place by being one a reader is handed.
    #   "cannot be addressed with prescription glasses" -- the glasses paragraph. The
    # full clause names the mechanisms ("optic nerve ... or optic pathway") and the
    # page keeps only the pathway, because the eye-and-retina half is a different
    # subject. **BUT THE SUBJECT OF THAT CLAUSE IS "a decrease in visual acuity", NOT
    # FIELD LOSS**, which /review round 1 caught this note failing to say while the
    # paragraph around it is about the field. The two halves are sourced separately
    # and deliberately: the glasses sentence carries "new lenses cannot put back what
    # is missing" for a pathway problem, and the EYE-CHART sentence rests on a
    # different sentence of the review's -- "Vision requirements for driver licensing
    # primarily consider visual acuity and horizontal field extent" -- which is what
    # makes acuity and field two separate things a licence looks at. Without that
    # second source the eye-chart line was an inference.
    #   "The impact of visual field loss on driving depends on the extent of the
    # defect, the location within the visual field" and, after a possessive this file
    # cannot reproduce, "ability to compensate by using eye and head scanning" -- the
    # three factors. **THE QUOTATION IS SPLIT RATHER THAN RETYPED**: the source prints
    # a RIGHT SINGLE QUOTATION MARK (U+2019) in "the patient's", and an ASCII
    # apostrophe here is a modified quotation. The character is NAMED rather than
    # pasted (12.20), and the split is that ruling's number-unit rule applied to a
    # character boundary -- stop before it rather than reproduce it.
    # **AND THE HAYSTACK IS WHY THIS SURVIVED TWO ROUNDS OF CHECKING.** Both earlier
    # passes verified against work_files/wi573/fitness.txt, and totext.py NORMALISES
    # U+2019 to an apostrophe and the curly double quotes to straight ones -- so the
    # extract agreed with the typo and could never have disagreed. 12.20: a haystack
    # containing a NORMALISING COPY of its subject cannot report a normalisation
    # defect. It was written about an aggregator; here the normalisation was ours.
    # Quotations in this record were re-checked against the raw fetched HTML.
    # NOTE: the
    # "location" in that sentence is location within the VISUAL FIELD, not the
    # tumor's address, which is why this sentence is not a Wave 6 row. An earlier
    # draft of this note had to be corrected for reading it the other way.
    #   "Some drivers with homonymous field defects have been rated as safe to drive
    # in on-road studies." and "However, there is evidence that others have impaired
    # steering stability and lane position, and impaired responses to road hazards."
    # -- BOTH halves, because either alone is the wrong page. The first alone reads as
    # reassurance to drive; the second alone reads as a verdict.
    #   THE SECOND PATTERN, AND THE SCOPE IT FORCED ON EVERYTHING BELOW IT. The source:
    # "Rarely the visual field loss heteronymous (binasal or bitemporal), most commonly
    # from a pituitary tumor near the optic chiasm." **The page presents the two patterns
    # SYMMETRICALLY where the source calls the second rare, and that is a deliberate
    # departure recorded here rather than left to be found.** The reason is the audience:
    # this page's #pituitary door sends the sellar reader into this section, and for that
    # reader the rare pattern is the likely one. A page that says "most often the same
    # side" hands them the wrong answer, which is what /review round 2 found it doing.
    # The address in the source's sentence -- "a pituitary tumor near the optic chiasm" --
    # is NOT published: the page says which one you get depends on where along the pathway
    # the growth presses, and tells the reader to ask. That keys the difference to a
    # structure without handing anyone a lookup, which is what keeps it off the Wave 6 row.
    #   **AND EVERY DRIVING-EVIDENCE SENTENCE BELOW IS HOMONYMOUS-SCOPED, WHICH THE SECOND
    # PATTERN MADE LOAD-BEARING.** /review round 3's blocker: adding the second pattern
    # silently broadened "people with similar losses" and "people with this loss" to cover
    # both, and the source studied only the first. The prose now names the same-side
    # pattern in both places, and "the side you cannot see" became "the side or sides".
    # **A fix's blast radius is every sentence that depended on the old scope** -- round 2
    # changed what the section is about and checked the paragraph it edited.
    #   "In driving simulator studies participants with similar amounts of homonymous
    # field loss exhibited a wide range in detection rates for hazards on the affected
    # side, from almost no detection through to performance similar to that of control
    # drivers" -- the sentence the section turns on, and the only one that makes "two
    # people missing the same amount can drive very differently" a measurement rather
    # than a guess. **THE QUOTATION STARTS AT "In driving simulator studies" ON
    # PURPOSE.** /review round 1 found it starting one word later, at "participants",
    # and the page saying "In driving studies" -- so the trim HID the drop of
    # "simulator", and the page was promoting simulator findings to on-road findings
    # on its load-bearing sentence. The page says "driving simulator studies" now. The
    # on-road evidence here is a DIFFERENT sentence (the safe-to-drive ratings), and
    # conflating the two is what a trimmed quotation made invisible.
    #   "These tests are helpful for diagnosis and disease monitoring but do not
    # evaluate the ability of the patient to compensate for the field loss by
    # scanning" -- why the map does not settle it.
    #   "The Humphrey Field Analyzer and Goldmann perimetry are routinely used to
    # quantify visual field defects." -- the word perimetry. The machine names are NOT
    # published: a reader cannot ask for a brand.
    #   "Wide interstate variability in visual field requirements exists within the
    # United States" -- the jurisdiction claim, and it is US-scoped, which the
    # backlog's UK source could not be.
    #   "uncorrected double vision or diplopia may make it unsafe for a person to
    # drive" -- the double-vision paragraph. diplopia is not taught for the
    # quadrantanopia reason.
    #   "The extent of the visual field loss or double vision can change over time as
    # the patient undergoes treatment or the tumor progresses, thus regular follow-up
    # appointments for vision testing are needed." -- both directions, and the reason
    # to keep the eye appointments.
    #   "patients with visual field loss may benefit from referral to an occupational
    # therapy practitioner for training in adaptive strategies for driving" and
    # "Individuals with visual field defects may require driving training with a
    # certified driving rehabilitation specialist" and "Extended mirrors, which may
    # help broaden the visual field of the affected side, are also available." -- the
    # who-to-ask paragraph. "Extra mirrors can help" is the source's "may help",
    # because "are also available" is not a claim that they work.
    #   "If possible, patients with visual field loss should be given an opportunity
    # to demonstrate their FTD through a driving evaluation in a simulator or on the
    # road." -- being assessed at the wheel. FTD is the source's abbreviation for
    # fitness to drive, and **that gloss is outside the quotation marks on purpose**:
    # /review round 1 asked for the abbreviation to be expanded, the fix put
    # "(fitness to drive)" INSIDE the marks, and an unmarked editorial insertion is a
    # modified quotation by the same rule that puts emphasis outside them. A nit fix
    # created the defect class the same round was fixing twice over.
    #
    # THREE THINGS IN THIS SOURCE REFUSED IN WRITING, each with its figure recorded so
    # nobody re-proposes it:
    #   (1) The review's presentation-rate range for tumors on the visual pathway. A
    # PERCENTAGE KEYED TO A LOCATION -- the subject of that sentence is tumors
    # "infiltrating or compressing the visual pathway" -- which is the one artifact
    # every Wave 6 item is forbidden to publish. The page says the pattern is the
    # common one and prints no figure. The spread is its own second argument: its low
    # end is about half of those patients and its high end is close to nine in ten,
    # which is 12.20's treatment of the 2.3%-100% resection range applied again.
    # THE FIGURES ARE DESCRIBED HERE RATHER THAN QUOTED, and that is this item's own
    # floor being applied rather than stated -- see the barred-quotation note above.
    # /review round 1 caught the first draft quoting both this range and the
    # threshold in (2), and BOTH were MODIFIED quotations: the source prints an EN
    # DASH (U+2013) in the range and a DEGREE SIGN (U+00B0) in the threshold, and the
    # note had typed a hyphen and the word "degree". The characters are NAMED rather
    # than pasted, which is 12.20's rule for exactly this. **A modified quotation of
    # a figure this page may not print is the same defect twice**, and the cheapest
    # fix for both is to stop quoting it.
    #   (2) The two named states, the horizontal-field threshold one of them sets, and
    # the conclusion the review draws from the pair -- that the same eyes are
    # licensable in one and not in the other. THE ARGUMENT IS PUBLISHED AND NEITHER
    # THE STATES NOR THE FIGURE ARE.
    #   AND THE PHRASE ITSELF IS TREATED DIFFERENTLY ON THE TWO PAGES, on purpose.
    # This page quotes "horizontal field extent" in full, because it OWNS the subject
    # and the term is vocabulary rather than a rule. /seizures/living-with elides it,
    # because its reader arrives for seizures and a page that acquires vision-rule
    # vocabulary is one edit from acquiring a vision rule. /review round 3 asked for
    # one policy; the answer is that the bar is scoped to the page that must not grow
    # the subject, which is the same shape as every other per-page source bar here. Naming them would hand a reader in one of them a licensing
    # verdict from a website, which is the thing /seizures/living-with#driving exists
    # to refuse, and the degree figure is a number a reader would measure themselves
    # against. Unnamed, the sentence carries the whole force -- one state sets a width
    # and its neighbor sets none -- and nobody can mistake it for their own answer.
    #   (3) The Swiss consensus requirements for returning to the wheel. Every item in
    # that list is a WAITING TIME or a re-scanning interval, so the list is named here
    # and its figures are not -- the same rule as the CRUK note above, applied in the
    # same file rather than in only one of two places. WI-560 owns waiting times and
    # this item prints none.
    #
    # AND THE ROUTE'S BOUND IS MEASURED, NOT ASSUMED, which is 12.19 finding 6.
    # /seizures/living-with#driving is SEIZURE-SCOPED throughout: "Every US state has
    # rules about driving after a seizure", "Most states ask you to be free of
    # seizures for a set length of time", and its lookup tool is the Epilepsy
    # Foundation's. A reader whose problem is vision and who has never had a seizure
    # is not served by its rules half. The route is still right -- the SHAPE is
    # identical and the practical half is the whole point -- so the bound is written
    # into the prose (the paragraph headed "One thing that page does not cover")
    # rather than left for the reader to discover. **The note is deliberately not
    # quoting that sentence:** it quoted an earlier draft of it for two rounds after a
    # readability pass replaced the wording, and the tests assert the real string
    # while the note was the only copy that lied. 12.20's "correcting a claim in one
    # file does not correct its copies", inside one file. The destination got
    # a door back for the same reason, because a vision reader who lands there and
    # finds only seizure rules would otherwise conclude the question does not apply.
  # ANOTHER SOURCE THE ACCEPTANCE NAMED AND THIS PAGE DOES NOT CITE, recorded
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
reviewed: 2026-09-30
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

**And some words name a place without naming a type at all.** A report or a doctor can
hand you an address where you expected the name of a growth.
[When the word you were given is a place](#a-place-for-a-name) is the section for that,
and the word it works through is **tectal glioma**.

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

**If sight is what changed for you**, that has a section of its own:
[when what changed is your sight](#your-sight), and it covers driving.

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

The gland sits behind the bridge of your nose, below the brain. More than one
kind of growth turns up there, and four of the ones we have written about are
[pituitary tumor](/tumors/pituitary-tumor),
[craniopharyngioma](/tumors/craniopharyngioma),
[meningioma](/tumors/meningioma) and
[germ cell tumor in the brain](/tumors/cns-germ-cell-tumor). Each of those pages
starts from the beginning, and [the tumor types](/tumors) has the rest.

**If sight is your question**,
[when what changed is your sight](#your-sight) covers it, including what it means
for driving.

**This region also carries a warning of its own**, and
[when where it sits makes it urgent: the fluid](#the-fluid) is where that sits.
Read it if this is your region.

## When the word you were given is a place {#a-place-for-a-name}

Some of the words people are handed name a place rather than a type. **Tectal glioma** is
one of those words, and it is the one this section works through. If yours is a different
place-word, the shape of the answer is the same. The word tells your team where to look.
It does not tell them what the growth is.

**What the word says.** **Tectal** points at the **tectum**, which is the back part of the
**midbrain**. The midbrain is one of the three parts of
[the stalk the brain sits on](#brainstem). So the word is an address with *glioma*
attached to it.

**What the word does not say.** It does not say what the growth is made of.
[The place is not a diagnosis](#what-it-means) is the general form of that. In the small
group one study followed, the tissue turned out to be a different thing in different
people. That study's own words for where this group belongs do not agree with each other.

**Where the fluid comes into it.** This is a spot where the fluid can be held up. That same
study says a blockage, and the pressure it raises, is the usual way this shows itself.
[Deep in the middle, where the fluid runs](#ventricles) is the entry that names the tube
the fluid has to pass through. **[When where it sits makes it urgent: the
fluid](#the-fluid) is the section to read today if this is your word.**

**What a team often does about it.** Here is what a review of how this is managed in
children found. Most of these were watched with repeat scans, rather than having the
growth removed. That is not the same as nothing being done. The review says patients often
need treatment for the fluid. A procedure to get the fluid moving was the most common
operation in it. That is the general point the fluid section below makes, arriving here
with a name on it.

**"Most" is a word about a group.** It is never a prediction about one person, including
everywhere this section uses it. What your own team is weighing is a question you can put
to them today.

**And it is not only a childhood word.** Another paper, with adults as well as children in
it, reports the same two parts: watch the growth, and deal with the fluid. It also reports
that in adults these are more often found by chance. So if you are an adult holding this
word, those two parts are usually what a team is working with. Ask your team what is known
for yours.

**And something this page will not print.** No number from any of those papers is on this
page. In the review, how often the growth itself was taken out ran from almost never to
always, depending on which study was counted.
[Why there is no list here](#no-list) makes the same kind of case about the whole page.

**And do not read an operation out of the word either.**
[Can they just take it out?](#can-they-take-it-out) is where this page keeps the range a
team picks from. It says in as many words that a place is not what settles that.

**The word is still worth chasing.**
[Ask what it is being called for now](#finding-your-type) is the first step of the path
from a place to a name. It works the same way whether or not the word you were handed is a
place.

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

## When what changed is your sight, and what that means for driving {#your-sight}

**If sight is the thing that changed, this is the section for it.** The pathway that
carries sight runs a long way through the head, so where a growth sits can change
what you see. Driving is the question people ask first, and it is the one this page
cannot answer for you.

**Get your sight checked before you drive again.** Do that first, whatever the rules
where you live turn out to say.

**Double vision matters here too.** If things appear doubled and that has not been
corrected, it can make driving unsafe on its own.

**What "visual field" means.** Your **visual field** is everything you can see at
once while your eyes are still. Losing part of it is not the same as blurriness. A
piece of what you would normally see is simply not there.

**There is more than one pattern, and which one you have matters.** For some people
the missing piece is on the same side in both eyes. For others it is the outer edge
in both eyes instead. You may hear either one called **hemianopia**, and that word
does not say which. Which one you get depends on where along that pathway the growth
presses, so ask your team which one is yours. A smaller missing patch has its own
word, **scotoma**.

**This is not a glasses problem, and that part matters.** When the pathway itself
is affected, new lenses cannot put back what is missing. So reading an eye chart is
not proof that your field is whole.

**The test for it is a separate test.** It maps the whole of what you can see,
edge to edge, rather than measuring how sharp the middle is. Your team may call it a **field test**, or
**perimetry**. Ask for a field test.

**Two people missing the same amount can drive very differently.** In driving
simulator studies, people who had lost the same side in both eyes ranged widely.
Some spotted almost no hazards on that side. Others matched drivers who had full
vision.

**Most of what has been studied is the same-side pattern.** If yours is the outer
edge in both eyes, or a smaller patch, that does not mean the question is settled.
It means it has to be answered for you.

**Three things matter.** How much is gone. Where in your vision it sits. And how
well you learn to sweep your eyes and head toward the side or sides you cannot see.
That sweeping can be taught.

**So the test does not settle it by itself.** A field test says what is missing. It
does not say how you drive. That is why being assessed at the wheel is a real
option, in a car or in a simulator.

**Whether you are allowed to drive is decided where you live, and places differ more
than people expect.**
One state can set a width of vision you have to have. Another state can set none at
all. The same eyes, two answers.

**Being allowed to drive is not the same as being safe to drive.** A place that sets
no minimum has not told you that you drive safely. Some people who have lost the same
side are assessed as safe to drive, and some are found to steer and react worse.

**So your own answer has to come from where you live, and so does when you could
drive again.**
[Driving and seizures](/seizures/living-with#driving) is where this site puts the
driving question. It also has the part nobody plans for: getting to work and to
appointments without a car.

**One thing that page does not cover.** It was written for seizures, and the rule
about sight is a separate rule. The state lookup on it is about seizures too. For
the sight rule, ask your team or whoever issues your license.

**There are people whose whole job this is.** An eye doctor measures the field.
Ask your team for a referral to an occupational therapist, or to a driving
rehabilitation specialist. They can teach the sweeping and set up an assessment at
the wheel. Extra mirrors can help.

**And it can change in both directions.** How much is missing can shift while you
are treated. A no now is not always a no later, and a yes gets looked at again.
Keep the eye appointments.

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
  covers the exceptions it names, and
  [how can they know what it is without a sample?](/treatments/watch-and-wait#without-a-sample)
  covers the version where nothing is being treated yet. Either way, ask what your
  team is going on instead, and how sure of it they are.
- **Then look it up here.** [The tumor types](/tumors) is the index, grouped so
  you can see which family a word belongs to before you read anything.

If the word you were given is not on that list, that is worth knowing rather than
worrying about. Some of what people are handed is an address instead of a name, and
[when the word you were given is a place](#a-place-for-a-name) is the section for that.

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
