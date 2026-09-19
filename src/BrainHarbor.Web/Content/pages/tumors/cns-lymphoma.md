---
title: "CNS lymphoma"
description: "A cancer of white blood cells that starts in the brain, the spinal cord, or the eyes. Why an operation to remove it is not the treatment, why a steroid can take the answer away before anyone has it, and what the medicine that reaches the brain is actually like."
tags: [tumor-type]
sources:
  # WI-542. A TUMOR HUB, so §12.3's seventeen sections and §12.9's proved
  # template govern. The 47-line stub this replaces cited ONE source and it was
  # barred: NCI's /types/brain. That is the FIFTH stub in a row carrying it.
  #
  # NO BLOCK DIRECTIVE IS WRITTEN IN BRACKETS ANYWHERE IN THIS FRONT MATTER.
  # CaregiverSectionTests locates the caregiver block with a raw IndexOf over the
  # WHOLE file, so a bracketed name in a comment becomes the FIRST match and the
  # page is reported as not carrying the block. Block names below are written
  # without brackets for that reason.
  #
  # THIS PAGE IS READ BY A SHIPPED PAGE'S TEST, WHICH IS NEW FOR A HUB REWRITE.
  # SteroidsPageTests.TheDoesNotTreatTheTumorClaimIsScopedAndCarriesItsException
  # opens this file and requires it to match "biopsy before starting steroids"
  # and to contain "/treatments/steroids". That page's central claim -- that a
  # steroid works on swelling and not on the tumor -- is TRUE everywhere on this
  # site except here, and it is scoped by naming this page as the exception. So
  # the phrase is load-bearing on another page's suite, and it is kept verbatim
  # from the stub deliberately rather than reworded.
  #
  # EVERY LOAD-BEARING CLAIM WAS VERIFIED AGAINST THE LIVE PUBLICATION, not
  # against the item's research pack. Three agents returned 81 files; the claims
  # below were re-fetched and re-read by me. The verification changed three
  # things. Log: .claude/work_files/wi542/plan.md.
  #
  # AND THE READER MEETS BARRED AND BRITISH SOURCES IN THE COMPOSED SOURCE LIST.
  # Block sources merge into every including page and RENDER (§12.10), so the
  # escalation, mechanism, causes and caregiver blocks put NCI in front of this
  # reader several times and Cancer Research UK TWICE, one of them under a
  # visibly British title ("Brain tumour symptoms"). §12.1 still holds -- not one
  # of them is a naming or grading claim, and this page cites neither itself --
  # but the honest form of the statement is the one above plus this one. Found by
  # the rendered read, because it is a property of the COMPOSED page that no
  # front-matter check can see.
  #
  # ONE GLOSSARY TERM WAS DESIGNED AROUND RATHER THAN USED, and the rendered read
  # is the only reason it was caught. An earlier draft called leucovorin "a rescue
  # medicine". `rescue medicine` IS a glossary term on this site and it means
  # something else entirely -- a seizure medicine kept at home, given up the nose
  # or under the tongue -- so the popover fired on a chemotherapy sentence and
  # handed the reader a definition that was simply wrong for it. Nothing could see
  # that: the tooltip lives in another file, and every gate reads this page as
  # text. The drug is now named directly and the term is not used.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC9437714/
    title: "Primary central nervous system lymphoma"
    accessed: 2026-09-18
    # THE CENTRAL CLAIM OF THIS PAGE, and the strongest wording of it anywhere in
    # the set. Verified live, verbatim: "Corticosteroids are toxic to lymphoma
    # cells and can cause a complete resolution of PCNSL lesions, resulting in
    # poor diagnostic yield, potentially delaying definitive diagnosis and
    # treatment. Therefore, corticosteroids should be reserved only for
    # life-threatening circumstances."
    #
    # THE FALLBACK, WHICH IS WHY THE PAGE DOES NOT TELL ANYONE TO REFUSE A DRUG:
    # "If it is necessary to start steroids, a brain biopsy should be performed
    # within 24 to 48 hours of the first dose to optimize the diagnostic yield."
    # The honest shape is that the team works around it, not that the patient
    # should argue about it.
    #
    # Also verified and carried: "A majority of patients with PCNSL with ocular
    # involvement have no visual symptoms." That single sentence is why the eye
    # exam is on this page at all.
    #
    # Its periventricular share and its systemic-disease share are NOT carried.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11148853/
    title: "Primary central nervous system lymphoma: EHA-ESMO Clinical Practice Guideline for diagnosis, treatment and follow-up"
    accessed: 2026-09-18
    # THE GUIDELINE, AND ITS GRADES ARE PART OF WHAT IT SAYS. Verified live with
    # the bracketed grades intact, because the strength is the point:
    #   "Corticosteroid therapy before tissue biopsy should be avoided whenever
    #   clinically possible [IV, D]."
    #   "In case of clinical deterioration, urgent biopsy should be carried out
    #   before the start of corticosteroids [IV, A]."
    #   "Tissue samples should be collected by stereotactic biopsy in patients
    #   with brain lesions [IV, A]."
    #   "Tumour resection is not recommended, except in carefully selected
    #   patients with rapidly increasing intracranial pressure who may benefit
    #   from surgical debulking at the time of tumour biopsy [IV, D]."
    #   "Ophthalmological assessment by slit lamp fundoscopy should be carried
    #   out in all patients to exclude intraocular involvement [IV, A]."
    #   "Enrolment in suitable prospective clinical trials should be offered to
    #   every patient with PCNSL [I, A]."
    #
    # NOTE THE ASYMMETRY THE PAGE CARRIES. The avoid-steroids rule is graded D,
    # the WEAKEST tier. The deteriorating-patient exception is graded A. So the
    # page states the rule as what teams try to do rather than as a law, and
    # states the exception as the firmer of the two. Writing it the other way
    # round would invert the evidence.
    #
    # THE REASON TO REPORT NEW SYMPTOMS, verified: "only 20% of relapses are
    # diagnosed on surveillance MRI" [III, B]. The share is NOT printed; the
    # direction is, because it is what makes the instruction worth following.
    #
    # Also verified: annual eye examination in follow-up [IV, B]; and that
    # stratification "should not be made considering exclusively the patient's
    # age but also the ability to tolerate intensified treatments". Its symptom
    # percentages, its eye-involvement share, its creatinine threshold and its
    # neurotoxicity incidence are NOT carried.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK545145/
    title: "Central Nervous System Lymphoma - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-18
    # WHY AN OPERATION IS NOT THE TREATMENT, in one sentence. Verified live:
    # "CNS lymphoma is not amenable to complete surgical resection due to its
    # infiltrative nature, multifocality, and microscopic seedlings protected by
    # the intact blood-brain barrier."
    #
    # And the mechanism of the trap: "Corticosteroids should be withheld prior to
    # diagnostic biopsy owing to their lymphocytolytic effect."; "Cell arrest and
    # apoptosis secondary to corticosteroids can cause 'ghost' or 'vanishing'
    # tumors in up to 50% of cases."; "Inconclusive biopsies are three times more
    # likely in patients treated with corticosteroids than in steroid-naive
    # patients."
    #
    # THE SHARE AND THE MULTIPLE ARE BOTH LEFT OFF. "Up to 50%" and "three times"
    # are the two most quotable figures on this subject and neither reaches the
    # page (§12.4 R2): what a reader acts on is that it happens often enough to
    # change the order of things, not how often.
    #
    # ASKED DIRECTLY AND ANSWERED "NOT PRESENT": this chapter does NOT compare
    # seizure frequency with other brain tumors. The page's seizure comparison
    # therefore rests on the patient organization below and not on this chapter,
    # which is §12.14's rule -- a citation has to support the claim in the form
    # it is made.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC6142465/
    title: "Primary central nervous system lymphoma"
    accessed: 2026-09-18
    # THE PLAINEST FORM OF THE CAVEAT, verified live: "Corticosteroids have
    # lymphotoxic effects and should be avoided, if possible, before stereotactic
    # biopsy as a histopathologic diagnosis can be difficult or impossible to
    # achieve after exposure to these drugs."
    #
    # The second reason resection is not the treatment: "Surgical resection is
    # not part of the standard treatment approach for PCNSL given the multifocal
    # nature of this tumor."
    #
    # The eye examination: "Involvement of the optic nerve, retina, or vitreous
    # humor should be excluded with a comprehensive eye evaluation by an
    # ophthalmologist that includes a slit-lamp examination."
    #
    # And why the body gets scanned even though this is a brain diagnosis:
    # "Because extraneural disease must be excluded to establish a diagnosis of
    # primary CNS lymphoma..." Its median age is NOT carried.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8328013/
    title: "The 2021 WHO Classification of Tumors of the Central Nervous System: a summary"
    accessed: 2026-09-18
    # THE NAMING AUTHORITY (§12.1), CITED FOR A NAME AND FOR AN ABSENCE.
    # Verified live by direct question, twice over:
    #   (1) it lists "Primary diffuse large B-cell lymphoma of the CNS", under
    #       Hematolymphoid tumors, and
    #   (2) IT PRINTS NO GRADE BESIDE IT, in contrast to meningioma's 1, 2 and 3.
    #
    # THAT ABSENCE IS THIS PAGE'S ANSWER TO §12.3's SECTION 2, and it is a
    # different absence from the one WI-541 met. There, a tumor with a grade had
    # its grade left unprinted. Here the entity is classified as a LYMPHOMA
    # rather than graded on the brain-tumor scale at all, so there is no number
    # to withhold. The page says that plainly instead of borrowing one.
    #
    # Also verified and used only for the convention: "WHO CNS5 has changed all
    # CNS WHO tumor grades to Arabic numerals", and neoplasms are "graded within
    # types". And the scope limit that explains why a blood cancer is in a brain
    # classification at all: "WHO CNS5 only includes those lymphoid and
    # histiocytic tumor entities that occur relatively often in the CNS".
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC9214472/
    title: "The 5th edition of the World Health Organization Classification of Haematolymphoid Tumours: Lymphoid Neoplasms"
    accessed: 2026-09-18
    # THE SECOND WHO NAME, ONE YEAR LATER, AND THE REASON THE EYES ARE CHECKED.
    # Verified live: this classification "combines the previous entity of primary
    # DLBCL of CNS with DLBCL of the vitreoretina and testis" under a new
    # umbrella term, and explains why those three sites belong together: "They
    # arise in immune sanctuaries created by their respective anatomical
    # structures (e.g., the blood-brain, blood-retinal, and blood-testicular
    # barriers)".
    #
    # THE PAGE TEACHES CNS5's NAME AND USES THIS ONE ONLY FOR THE EXPLANATION.
    # §12.1 makes CNS5 the naming authority, and a reader holding a report needs
    # the name their report is likely to use. The sanctuary idea is carried
    # because it answers a question the reader actually asks -- why an eye exam
    # for a brain tumor -- and not as a second name to learn.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC13525931/
    title: "Radiological response of primary central nervous system lymphoma after corticosteroid therapy and its predictive value on overall survival: a multicenter study"
    accessed: 2026-09-18
    # THE PAIRING THIS PAGE IS NOT ALLOWED TO BREAK. Verified live, verbatim:
    # "After initial tumor regression following CST, most tumors recur after a
    # short period of time, meaning that CST alone is not a viable treatment
    # option."
    #
    # So every sentence on this page saying a steroid shrinks the tumor carries
    # the fact that it comes back within the next three sentences, and the guard
    # asserts exactly that, per sentence, rather than merely that both ideas
    # appear somewhere on the page. The contract used to say "immediately" and
    # the page never kept it, which is a stale rationale: the next editor reads
    # the contract, not the test. A reader who takes
    # "the steroid shrank it" as treatment has been actively misled, and this is
    # the one page in the corpus where that sentence is even available to
    # misread. §12.12's over-reassuring direction, which is the dangerous one.
    #
    # Also verified: "PCNS-LBCL cells however can be highly sensitive to
    # corticosteroid therapy (CST) potentially reacting with distinct shrinkage
    # of contrast-enhancing tumor in MRI"; "CST pre-treated PCNS-LBCL may show
    # subtotal or total loss of neoplastic B-cells"; and "Therefore, if
    # clinically possible, CST should be avoided until diagnostic tissue has been
    # acquired".
    #
    # ALREADY CORPUS-VETTED: /treatments/steroids cites this same paper for the
    # same exception, so the two pages rest on one source and cannot drift into
    # two strengths of one claim (§12.10).
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8811119/
    title: "Implementation of an Outpatient High-Dose Methotrexate Initiative"
    accessed: 2026-09-18
    # WHAT A TREATMENT ADMISSION ACTUALLY INVOLVES, and the source whose PURPOSE
    # changed the sentence the page prints. Verified live:
    # "To ensure safe administration, HDMTX is most often administered in the
    # hospital setting with an average length of stay around 4-5 days, barring
    # any treatment complications."
    # "Normal renal clearance of MTX is required to quickly eliminate the drug,
    # supported by aggressive hydration and urinary alkalinization."
    # "These doses are potentially lethal without administration of intravenous
    # and/or oral leucovorin to rescue normal cells from apoptosis"
    # "Once methotrexate level is <=0.1 micromol/L, patients are disconnected
    # from IV fluid infusion."
    # "Oral leucovorin is continued and oral sodium bicarbonate is restarted for
    # 3 additional days."
    #
    # AND THE CONTEXT CHECK THAT MATTERED. This paper exists to move the
    # treatment OUT of hospital: "While inpatient administration has been the
    # norm, outpatient administration, has been shown to be safe, effective, and
    # patient centered." So the stay is the NORM AND NOT A UNIVERSAL, and the
    # page says "usually" and tells the reader some places are doing it without
    # an overnight stay. Lifting the duration without the paper's own argument
    # would have published a rule its only source is arguing against.
    #
    # The clearance threshold and the dose are NOT carried (§12.4 R1).
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC10000886/
    title: "A Systematic Review of High-Dose Methotrexate for Adults with Primary Central Nervous System Lymphoma"
    accessed: 2026-09-18
    # WHY THE DOSE IS HIGH, verified live: "Due to its ability to cross the
    # blood-brain barrier, high-dose methotrexate (HDMTX) is the backbone for
    # induction chemotherapy." And the route: "As HDMTX was administered
    # intravenously in all cohorts..."
    #
    # The two effects the page warns about: "Renal toxicity was most common with
    # 30 cohorts reporting its occurrence" and "Mucositis was reported in 15
    # cohorts, all of which were of grade 3-4 severity." Cohort counts are not
    # frequencies for a reader and none is printed.
    #
    # And the honest limit: "Due to its rarity, high-quality evidence from
    # clinical trials regarding treatment for PCNSL is scarce and there is a lack
    # of consensus regarding optimal treatment among the various regimens."
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC6142584/
    title: "Therapy of primary CNS lymphoma: role of intensity, radiation, and novel agents"
    accessed: 2026-09-18
    # WHAT IS GENUINELY UNSETTLED, in the authors' own words. Verified live:
    # "Only 1 randomized trial comparing WBRT with observation after chemotherapy
    # has been published...the G-PCNSL-SG1 trial...presented several
    # interpretative caveats, major protocol violations in one-third of
    # randomized patients, and the predetermined primary end point for
    # noninferiority was not met."
    # "Randomized trials suggest both WBRT and ASCT are effective as
    # consolidation therapies in young patients, with a higher risk of
    # neurotoxicity after WBRT."
    # "Despite the high sensitivity to conventional chemotherapy and
    # radiotherapy, remissions are frequently short lasting."
    #
    # AND THE FOUR-SIDED QUESTION THE PAGE REFUSES TO SETTLE, all verified:
    # retrospective studies showing no benefit from chemotherapy into the spinal
    # fluid; other evidence that "seem[s] to suggest some benefit"; a real harm,
    # reservoir infections; and that "recently reported or ongoing PCNSL trials
    # do not use intrathecal and/or intraventricular chemotherapy". A treatment
    # that is disputed AND being dropped is described as exactly that.
    #
    # Its neurotoxicity incidence, its reservoir-infection share and its doses
    # are NOT carried. The page takes the DIRECTION only (§12.4 R3).
  - url: https://aol.amegroups.org/article/view/7647/html
    title: "Relapsed and refractory primary CNS lymphoma: treatment approaches in routine practice"
    accessed: 2026-09-18
    # RELAPSE, AND THE ABSENCE OF A STANDARD ANSWER. Verified live: "Currently,
    # there are no approved therapies, and no widely accepted 'standard-of-care'
    # approaches for the treatment of refractory or recurrent primary central
    # nervous system lymphoma (rrPCNSL)."
    #
    # The options, in the authors' own words: "Re-treatment with HD-MTX based
    # regimens, use of non-cross resistant chemotherapy regimens, high-dose
    # chemotherapy and autologous stem cell transplantation (HDT-ASCT), and brain
    # irradiation all remain important therapeutic approaches for rrPCNSL."
    #
    # And the caveat that keeps the trials sentence honest: "Whilst clinical
    # trials should always be considered a priority for patients with rrPCNSL,
    # suitable studies may not be accessible for many patients."
    #
    # ITS RELAPSE SHARE IS NOT CARRIED, and neither is its survival sentence.
    # That sentence is the bluntest in the whole source set and it is recorded in
    # the work notes rather than on the page: §12.5 bans the figures, and §12.6
    # bans ending a section on a frightening sentence with nothing after it.
  - url: https://rarediseases.org/rare-diseases/primary-central-nervous-system-lymphoma/
    title: "Primary Central Nervous System Lymphoma - Symptoms, Causes, Treatment | NORD"
    accessed: 2026-09-18
    # THE TEMPO, AND THE COMPARATIVE THE PAGE REFUSED TO MAKE. Verified live:
    # "Symptoms typically develop over weeks (subacute)."
    #
    # THE RESEARCH PASS PROPOSED "faster than most brain tumors" AND IT IS NOT
    # CARRIED, because no source found this session makes that comparison. The
    # nearest patient-facing source compares the speed with neurodegenerative
    # disease, not with other tumors. A claim that is probably true and is
    # nowhere stated is still an unsourced claim (§12.14), and this one would
    # have read as reassuring precision.
    #
    # Also verified: seizures are "less common compared to other brain tumors or
    # brain metastases"; the eye exam matters "as this can occur even in the
    # absence of visual symptoms"; and who is at raised risk, which is the
    # sentence the self-blame block needs scoping against.
  - url: https://braintumor.org/news/lets-talk-about-primary-cns-lymphoma/
    title: "Let's Talk About: Primary CNS Lymphoma | National Brain Tumor Society"
    accessed: 2026-09-18
    # THE ONE US PATIENT-FACING SOURCE THAT STATES THE RULE, verified live:
    # "Because corticosteroids can temporarily shrink lymphoma cells, a biopsy
    # should be performed before corticosteroids are administered to ensure an
    # accurate diagnosis."
    #
    # AND THE SEIZURE COMPARISON, WHICH ONLY THIS SOURCE MAKES: "Seizures, which
    # are more common in a lot of other brain tumors, such as gliomas or brain
    # metastases, are less common in primary CNS lymphoma". Its two percentage
    # bands are NOT printed; the comparison is what scopes the shared block.
    #
    # ASKED DIRECTLY: THIS PAGE GIVES NO PATIENT-FACING ACTION GUIDANCE. It
    # describes when something is "going on" and never tells a reader what to do
    # about it. That answer, repeated across every clinical source, is why this
    # hub carries no tumor-specific urgency rule.
  - url: https://www.cancer.gov/types/lymphoma/patient/primary-cns-lymphoma-treatment-pdq
    title: "Primary CNS Lymphoma Treatment (PDQ) - Patient Version - NCI"
    accessed: 2026-09-18
    # CITED FOR FRAMING AND FOR TREATMENT DESCRIPTION ONLY, NEVER FOR NAMING OR
    # GRADING. §12.1 permits exactly that and bars the rest, and this page takes
    # its name from the CNS5 summary above instead.
    #
    # It has the plainest sentence in the entire set, verified live: "Surgery is
    # not used to treat primary CNS lymphoma." Also "Glucocorticoids are steroid
    # drugs that have an anticancer effect in lymphomas.", the plain-language
    # account of what radiation to the whole brain can do to thinking, and that
    # this "often recurs in the brain, spinal cord, or the eye".
    #
    # AND AN ABSENCE WORTH RECORDING, ASKED DIRECTLY AND ANSWERED NO: this page
    # carries no warning at all about steroids before biopsy. The main US federal
    # patient page on this disease omits the most consequential fact about
    # diagnosing it, which is the clearest statement available of what this hub
    # is for.
  #
  # BLOCK DECISIONS -- ALL EIGHT MADE BY OPENING THE BLOCK. Names written without
  # brackets on purpose, per the ruling at the top of this front matter.
  #
  # ESCALATION -- IN, inside the symptoms section, with a forward pointer above
  # it. Here it is not merely required by §12.9, it is THE safety content: its
  # chemotherapy-fever conditional is this tumor's only sourced urgent rule.
  # MECHANISM -- IN, with a scoping note ABOVE the directive. The block's seizure
  # paragraph over-states for this tumor, so the note says LESS COMMON and never
  # "does not happen", and it preserves the ambulance tier explicitly.
  # CAUSES -- IN, demoted between "if it comes back" and the outlook gate per
  # §12.9, with a page-specific addition. Its "nobody knows the cause" is true
  # for most readers here and INCOMPLETE for one group, so the page adds the
  # immune-system line rather than letting a flat denial stand.
  # CAREGIVER -- IN. Contract item 11, and truer here than on most hubs: this
  # tumor's first signs are often changes in thinking and personality, so the
  # person reading that section may already be deciding things.
  #
  # CROSSWALK -- OUT, for a reason no sibling has used. The block is wholly the
  # 2021 CNS GRADING rewrite: Roman to Arabic, gene results entering the name,
  # NOS and NEC. This page's answer is that there is no brain-tumor grade here at
  # all, so a block teaching grade-number conversion would contradict it.
  # §12.9 still requires a slice, and this page writes its own.
  # TUMOR-BOARD -- OUT, and it was the closest call of the eight. The block is
  # apt here, which is not the test. It describes a one-off meeting; this
  # reader's question is which specialty leads their care, which the block never
  # addresses and this page answers itself.
  # SPINAL-CORD -- OUT, and unlike the last hub this one is arguable, so the
  # reason is recorded rather than assumed. This disease IS defined over the cord
  # as well as the brain. What decides it is that no source verified for this
  # item gives patient-facing cord guidance for this tumor, and the block's own
  # source is about METASTATIC cord compression. Handed to WI-543 with that
  # reasoning.
  # POSTERIOR-FOSSA-SYNDROME -- OUT. Children, cerebellar mutism, after surgery
  # low at the back of the brain. This is an adult tumor and its operation is a
  # needle biopsy.
  #
  # All four exclusions are PINNED AS AN EXACT CLOSED SET, with mutations
  # composing each excluded block back in. An excluded block leaves no trace on
  # the page, so nothing else would stop a later item quietly restoring one.
  #
  # THE BIOPSY ITSELF BELONGS TO /tests/biopsy, which owns the day, the burr
  # hole, the frame, what it feels like and what happens if the sample gives no
  # answer. This page owns only the part that is specific to this tumor: that the
  # sample is the treatment decision rather than a step before removing anything,
  # and the steroid timing. §12.10 says route rather than restate.
  #
  # THE FEVER THRESHOLD IS NOT PRINTED. The shared escalation block routes to
  # /treatments/chemotherapy#fever-rule, which owns the number and says teams
  # differ. That ruling also avoids a real conflict in this item's own sources:
  # two US bodies give one temperature and a third gives another.
  #
  # NO NEW GLOSSARY ENTRIES. Counted across pages, blocks and glossary: this
  # page's distinctive vocabulary is FIRST USE in the corpus. "methotrexate",
  # "rituximab", "intrathecal", "vitreoretinal" and "slit lamp" appear nowhere
  # else under Content/, so none meets §12.8's second-use threshold, and each is
  # glossed inline instead.
  #
  # NO PROGNOSIS FIGURES AND NO SURVIVAL FIGURES (§12.2 item 5, §12.5). No
  # percentages, no doses in milligrams or grams, no blood-level thresholds, no
  # scan intervals. The sources are thick with all of them and not one reaches
  # this page.
reviewed: 2026-09-18
review_due: 2027-03-18
disclaimers: [medical]
---
## The short version

CNS lymphoma is a cancer of white blood cells. Most lymphomas start in the
glands of the immune system. This one starts inside the brain, the spinal cord,
or the eyes instead.

**It is treated differently from almost every other tumor on this site.** An
operation to take it out is not the treatment. A small sample is taken to name
it, and then the treatment is medicine that can reach the whole brain.

**One thing is worth knowing on day one.** A steroid can shrink this tumor fast,
but only for a while, and that can take the answer away before anyone has it. Teams try to get the
sample first. [It is in the section on finding out](#finding-out), below.

The name on your report may be longer than the one at the top of this page. Your
team may also send you to a blood cancer doctor as well as a brain doctor. Both
are normal here, and neither means anything has gone wrong.

## What is CNS lymphoma?

**It is a blood cancer growing in the brain, not a tumor grown from brain
cells.** That one fact explains most of what follows.

Lymphoma starts in a white blood cell called a lymphocyte. These cells are part
of how your body fights infection, and they travel everywhere. Most lymphomas
start in lymph glands. When lymphoma starts in the brain, the spinal cord or the
eyes and is nowhere else, it is called **primary** CNS lymphoma. CNS is short
for central nervous system, which means the brain and the spinal cord together.

**The eyes belong with the brain here, which surprises people.** The brain, the
back of the eye and the testicles are all guarded by a barrier that keeps most
things out, including much of the immune system. A classification used by blood
cancer doctors groups these places together and calls them immune sanctuaries.
That is why a brain diagnosis comes with an eye examination.

**It is rare, and that shapes your care.** Because few people have it, most of
what is known comes from small studies rather than large ones, and specialists
still disagree about parts of the treatment. This page says where they disagree
rather than smoothing it over.

## Is it cancer? What does its grade mean?

**Yes, it is cancer.** The word your team may use is malignant.

**There is no grade for this one, and that is not an oversight.** Most brain
tumors get a number from 1 to 4 that says how the cells look and behave. The
international book that names brain tumors lists this one and **prints no grade
beside it at all**. Beside other tumors it prints 1, 2 or 3.

The reason is that this is a lymphoma. Lymphomas are sorted by what kind of
white blood cell they came from and where they are, not by the brain tumor
scale. So if you are looking for your number and cannot find one, nothing is
missing from your report.

**What takes the place of a grade here is the type of cell and where it is.**
Your report will name the cell. Your scans and an eye examination say where it
is. Those two things, not a number, are what the plan is built from.

## Where does it grow, and why does it cause these symptoms?

**It tends to grow deep, near the fluid spaces in the middle of the brain.**

It also tends to be in more than one place at once, and its edges blend into the
brain around it rather than stopping cleanly. Those two facts are the whole
reason surgery is not the treatment, and they come up again further down.

Where it sits explains the symptoms people notice first. Deep in the brain are
the paths that carry thinking, attention and memory between one part and
another. So the first changes are often in how somebody thinks and behaves,
rather than a single obvious thing going wrong.

**Before you read the next part.** What follows describes how tumors in the
brain cause symptoms in general, and most of it fits this one. One part fits
less well. **Seizures happen with this tumor, but they are less common with it
than with most other brain tumors**, so treat that paragraph as something that
happens to a minority here rather than to most people. That is not a reason to
take a seizure less seriously. **A first ever seizure is an ambulance call, and
that rule below applies to you exactly as it does to anyone else.**

**One more part of it is written for other tumors rather than for this one.** It
says that medicines which bring swelling down do not change the tumor itself.
Here a steroid changes both, and that is why the timing of one has a section of
its own further down this page.

[MECHANISM]

## What symptoms does it cause? {#symptoms}

**The commonest first sign is a change in thinking, memory or personality**, and
it is very often the people nearby who notice it before the person does.

That can look like confusion, forgetting things, losing the thread, or somebody
simply not seeming like themselves. It is easy to put down to stress, low mood,
or getting older.

Other common signs are weakness down one side, trouble with balance and walking,
and the signs of pressure building up inside the head.

**Symptoms usually build over weeks**, not over months or years.

**Eye symptoms belong on this list.** Blurred vision, or floaters, which are
small shapes that drift across what you are looking at. Eye involvement can also
be completely silent. Most people whose eyes are involved notice nothing wrong
with their sight, which is exactly why an eye examination is part of finding out and
not something you ask for only if your sight has changed.

[ESCALATION]

**Use the list above.** It is the one to go by, and this page does not add a
separate set of warning signs for this tumor on top of it.

**The one rule that is genuinely yours arrives with treatment, not with the
tumor.** That rule is in the list above: [the fever
rule](/treatments/chemotherapy#fever-rule), and it is there because this
treatment lowers the white blood cells that fight infection.

## How do doctors find out it is this? {#finding-out}

**A scan raises the question. A small sample answers it.**

**An [MRI scan](/tests/mri) usually comes first.** Sometimes the scan itself is
enough to make a radiologist suspect lymphoma, before anything has been proved.

**Then tissue.** A scan can suggest lymphoma. Only looking at the cells can say
it. What you have is usually a stereotactic biopsy.
[How tissue is taken](/tests/biopsy) goes through that day, start to finish.

**The sample is taken to name it, not to remove it.** This is the part that is
unlike most of this site. For most brain tumors the question is how much can
safely come out. Here the operation is for the answer, and the treatment is
medicine. There is one exception, and it is about pressure rather than cure: if
pressure inside the head is rising fast, a surgeon may take some bulk away at
the same time as the sample.

### Why the steroid timing matters so much {#steroids-and-the-sample}

**A steroid can make this tumor shrink within days. That does not last, and it
can cost you the diagnosis.**

Steroids are given to a great many people with a brain tumor, to bring down
swelling. For almost every other tumor they do nothing to the tumor itself. This
one is the exception. Steroids act on lymphoma cells directly, and the tumor can
shrink or fade off the scan quickly. Doctors have a name for what is left behind
when it does: a ghost tumor.

**The shrinking does not last.** After a steroid, this tumor usually comes back
within a short time. So a steroid on its own is not a treatment for it, however
good the next scan looks. The treatment is the medicine that comes after the
diagnosis.

That matters because a sample taken from a tumor that has already faded may not
hold enough lymphoma to name. Samples taken after a steroid has been started are
more likely to come back with no answer at all.

**This is why teams often want a biopsy before starting steroids, if it is safe
to wait.** [Steroids](/treatments/steroids) is the page for that medicine, and
this is the one tumor on this site where a steroid acts on the tumor itself
rather than only on the swelling around it.

**That rule is less frightening than it sounds, and here is why.**

**It is a rule for your team, not a decision you have to police.** The guideline
that states it grades it as the weakest kind of recommendation, and grades the
exception more strongly: if somebody is getting worse, the answer is to do the
biopsy **sooner**, not to leave them without treatment.

**If a steroid was started before anyone suspected lymphoma, that is common and
it is not a mistake you have to fix.** It usually happens in an emergency room,
days before the word lymphoma is first said out loud. Teams work around it, and
there is more than one way round it: the sample can be taken soon after that
first dose, or the team can wait and look again later. Ask which one applies to
you.

**If you are already on a steroid, do not stop it yourself.** Tell whoever is
treating you that you are on one, and let them decide. Coming off a steroid
suddenly can make you very ill.

**A sample that cannot be named is a known situation with a known path.**
Sometimes the tumor has to be watched until there is enough of it to test again.
Your team will tell you what they are waiting for.

### What else gets checked, and why

**Your eyes.** An eye doctor looks into the back of the eye with a bright
light and a lens, an examination called a slit lamp. It is done for everybody,
not only for people whose sight has changed.

**The fluid around your brain and spine.** A sample of that fluid, taken with a
needle in the lower back, can sometimes show lymphoma cells. It is not always
needed, and a clear result does not rule this out.

**The rest of your body.** This sounds alarming. It is the opposite. To call
something primary CNS lymphoma, doctors have to show it is **not** somewhere
else, because lymphoma that started elsewhere and spread would be treated
differently. Scans of the body, and sometimes a bone marrow sample, are how that
is shown. Blood tests are done at the same time.

## What do the words on my report mean?

**Your report may not say "CNS lymphoma" at all.** The name used by the
international classification of brain tumors is **primary diffuse large B-cell
lymphoma of the CNS**. It is long, and every part of it is doing a job.

- **Primary** means it started here rather than arriving from somewhere else.
- **Diffuse large B-cell** names the cell. B cells are one kind of lymphocyte,
  and these ones look large under the microscope.
- **Lymphoma of the CNS** places it in the brain and spinal cord.

**B cell.** The kind of white blood cell this starts from. Some of the treatment
is aimed at a marker on the surface of those cells, which is why the cell type
is worth knowing rather than skipping over.

**You may meet a second name, from a different rule book.** Blood cancer doctors
updated their own classification a year after the brain one, and grouped this
together with lymphoma of the back of the eye and of the testicles, under a
heading about immune sanctuaries. Two books, two names, one disease. If your
paperwork uses one and a website uses the other, nobody has made a mistake.

**Old names turn up in older writing.** **Reticulum cell sarcoma** and
**microglioma** are both historical names for this disease. If you find them
while reading, you have found something old rather than something new.

**No grade.** As above, there is no grade 1 to 4 for this one.
[Your pathology report](/tests/pathology-report) walks through a report part by
part, though the grade section of it will not apply to yours.

## How is it usually treated?

**The treatment is medicine, and it starts soon after the diagnosis.**

**Surgery does not treat it.** A federal patient guide says it plainly: surgery
is not used to treat primary CNS lymphoma. The reason is the shape of
the disease rather than the skill of the surgeon. It spreads through brain
tissue rather than pushing it aside, it is often in several places, and single
cells sit beyond anything a scan can show. Taking out what can be seen would
leave what cannot.

**Who leads your care may be two people.** A brain specialist and a blood cancer
specialist often share it, because this is a blood cancer in a brain. Ask who
your first call is, because with two teams it is easy for that to go unsaid.

**The main drug is methotrexate, given at a high dose into a vein.** The dose has
to be high for a reason worth understanding: the brain is guarded by a barrier
that keeps most drugs out, and only a high dose crosses it in useful amounts.
This is why it is not a tablet you take at home.

**Other drugs are usually given with it.** Which ones vary. One of them,
rituximab, is an antibody that sticks to a marker on B cells. Whether it adds
benefit here is honestly still argued: one guideline panel could not reach
agreement on recommending it.

**Then a second phase, and this is where specialists disagree most.** After the
first course, a further treatment is usually given to keep it away. There is
more than one accepted way to do it, including a stem cell transplant using your
own cells, more chemotherapy, or radiation to the whole brain. Trials have not
settled which is best.

**What is agreed is the worry about radiation to the whole brain.** It can cause
lasting problems with thinking and memory, and that risk is higher in older
people. Guidelines say it should be avoided or put off in older patients for
that reason. [What radiation to the whole brain
involves](/treatments/radiation-therapy#whole-brain-radiation) is covered on its
own page.

**If the eyes are involved**, treatment can be given into the eye as well as into
a vein.

**Chemotherapy into the spinal fluid is sometimes used and is genuinely
disputed.** Some studies found no benefit from adding it, others suggested some,
it carries a risk of infection where the small device sits, and recent trials
have largely stopped using it. If it comes up, asking why is entirely reasonable.

**Age on its own does not decide this.** What matters more is how well your
kidneys work, because the main drug leaves the body through them, and how well
you are otherwise. Guidelines say plainly that people should not be sorted by
age alone. Where the full treatment is too much, a gentler version is used
rather than nothing.

**Ask about a clinical trial, early.** For this disease the guideline does not
hedge: it says every patient should be offered a suitable trial.
That is unusually strong, and it is because so much here is still being worked
out. [Clinical trials](/treatments/clinical-trials) explains what taking part
involves. Suitable trials are not always available near you, which is worth
knowing so that a "no" does not read as a door closing.

## What is treatment actually like, and what is normal afterwards?

### The hospital stays

**Each round of the main drug usually means a few days in hospital**, and the
reason is not the medicine going in. That part takes a few hours. The days
afterwards are about getting it safely out again.

While you are there:

- **Fluids run into a vein**, in large amounts, to keep the drug moving through
  your kidneys.
- **A medicine makes your urine less acidic**, for the same reason. You may be
  asked to have your urine tested each time you pass it.
- **A second medicine protects you from the first.** It is called leucovorin, or
  folinic acid, and it starts a day or so after the main drug. It shields your
  normal cells while the treatment works on the lymphoma. It is not a sign
  anything has gone wrong.
- **Blood is taken every day** to measure how much of the drug is left. You go
  home when that level has dropped far enough, which is why the stay is a few
  days rather than a fixed number.
- **Some medicines carry on at home** for a few days after you leave.

Rounds usually come every few weeks, so there is a rhythm of going in and coming
out. Ask your own team for your dates in writing.

**Not everybody stays in.** Some places now give this treatment without an
overnight stay. If that is offered to you, it is a real option and not a corner
being cut.

### What it does to you

**Mouth sores are one of the commonest problems this drug causes**, and they can
be bad enough to make eating hard. Say so early instead of putting up with
them. There is treatment that helps.

**Your kidneys are watched closely**, which is what the blood tests and the
fluids are about.

**Blood counts fall**, which brings tiredness and the risk of infection. That is
where the fever rule matters most.

**Tiredness is heavier than people expect** and lifts slowly.

**Thinking can be foggy during treatment.** Some of that is the treatment, some
is the tiredness, and some can be the tumor settling down. Raise it yourself at
the next appointment. Nobody is likely to ask you first.

## Everyday life

**Infection is the thing to plan around**, because the treatment lowers the cells
that fight it. Your team will go through
what that means for crowds, food and visitors during the weeks when counts are
low. None of it is forever.

**Tiredness is the symptom most likely to shape a week.** Sleep does not clear
it. Put whatever matters most into the part of the day when you are at your
best.

**Thinking and memory deserve saying out loud.** If following a conversation or
keeping track of appointments has got harder, that is worth reporting rather
than hiding, because some of it can be helped and all of it is easier when the
people around you know.

**Work and driving are decided case by case.** Driving law is set locally, so no
website can settle it for you, this one included. Put that question to your
team.

**Seizures, if you have had one**, have their own pages:
[what to do during a seizure](/seizures/what-to-do) and
[living with seizures](/seizures/living-with).

## Follow-up scans, and what to do while you wait

Follow-up here means MRI scans, and **eye examinations alongside them**. If your eyes were involved at the start they are checked more
often.

[Reading one scan against the last](/tests/follow-up-scans) is a page of its own.
So is [the wait between a scan and a phone call](/tests/waiting-for-results),
which a lot of people find harder than the appointment itself.

**Tell your team about new symptoms between scans, and this is not a
formality.** When this disease comes back, most of the time it is noticed
because somebody reported something new, and only a minority of returns are
found on a scan that was already booked. So the thing you notice matters as much
as the appointment in the diary.

Ask for the schedule on paper. It should say when the next scan falls, when the
eye appointment falls, and which person to call if something shifts before
either of them.

## If it comes back

**It often comes back, even when the first treatment worked well.** Saying so plainly is kinder than
letting it arrive as a shock.

**There is no agreed standard for what to do next.** The authors of a review
written for doctors say it outright: there are no approved treatments and no
widely accepted standard approach for this situation. That is a real gap, and it
is not the same as nothing being available.

What is used includes going back to the same main drug, using different drugs,
higher dose treatment with a transplant of your own stem cells, and radiation.
Which one fits depends on what you had the first time, how long the gap was, and
how well you are.

**This is the point where a trial is most worth asking about**, for the same
reason the guideline gives it such weight.

## Did I cause this?

[CAUSES]

**For this one there is something to add, because for some people there is a
known reason.** People whose immune system is weakened have a higher chance of
getting this particular tumor. That includes people living with HIV, and people
taking medicines that damp the immune system down after a transplant. If that is
not you, this belongs in the same place as every other brain tumor: nobody knows.
If it is you, it is worth knowing, and it changes nothing at all about whose
fault this is.

## What might happen over time

:::outlook
The numbers attached to this disease are unusually hard to read, and it is worth
knowing why before you decide whether you want them.

This is a rare illness. Most of what is published comes from small studies, and
from treatment given years ago, which is not the treatment being given now. The
people who write those papers say so themselves, and say there is no agreement
on the best way to treat it.

Those numbers describe what happened to a **group** of people. Inside any group there is a
spread. Some people do far better than its middle point, and some do worse. The middle of a spread is not a forecast, and it was
never about one person. When a number is quoted without its spread, it sounds
like a prediction and it is not one.

What your own team can see is more specific than any published number: what the
cells are, where it is, how your kidneys and the rest of you are doing, and how
it responds once treatment starts. That last one is not knowable in advance,
which is the honest reason nobody can answer this on the first day.

Ask whenever it suits you, and ask it again later if the answer you got did not
fit the question you meant. Deciding not to ask is a decision too, and this
section will still be here.
:::

## For the person caring for someone with this

[CAREGIVER]

Beyond that, three things matter especially here.

**You may be the reason the diagnosis gets made.** When the first change is in
thinking, memory or personality, the person living it often cannot see it. If
you have been quietly wondering whether somebody is not themselves, that
observation is medical information. Say it to the team, in front of them, and
say what you have actually seen.

**The fever rule is yours to know too.** Once chemotherapy starts, a raised
temperature means phoning at any hour. Often it is the person alongside who spots
it first, before anybody has thought to reach for a thermometer.
[What a temperature means during chemotherapy](/treatments/chemotherapy#fever-rule)
sets out the number your own team will want you to use.

**The changes in thinking are the hardest part to live with, and they are not
them.** Other people caring for someone with a brain tumor say this is
the part nobody warned them about. It helps to have somebody to say that to who is not the
person you are caring for.

## What to ask your team

- What exactly does my report say the cells are?
- Is it in my eyes or my spinal fluid as well as my brain?
- Who is leading my care, and who do I call first?
- Am I on a steroid now, and how does that affect the sample?
- If my tissue could not be named, what happens next, and when?
- What is my first course of treatment, and how many rounds are planned?
- How long is each hospital stay likely to be, and could I have it without one?
- What is the second phase likely to be, and why that one for me?
- If radiation to the whole brain is suggested, what would it mean for my
  thinking?
- How are my kidneys, and does that change the plan?
- Is there a clinical trial I could join?
- Between appointments, what counts as worth phoning about?
- When is my next eye appointment?

## Where to get support

- [Getting help now](/get-help-now), for the hour when reading has stopped
  helping and a voice would be better.
- The **National Brain Tumor Society** publishes patient material on this
  particular diagnosis, which is unusual for a tumor this rare.
- Lymphoma organizations cover the blood cancer side, including what low blood
  counts mean day to day.
- The specialist nurse attached to your team. With two specialties involved,
  that is often the one person who can see the whole plan at once.
