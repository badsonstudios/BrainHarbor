---
title: "Spinal cord tumor"
slug: tumors/spinal-cord-tumor
description: "A tumor growing in or pressing on the spinal cord. It is not a brain tumor. Why most of these grow slowly, the small set of signs that mean going in now, what the operation turns on, and what living with it actually takes."
tags: [tumor-type]
sources:
  # WI-543. A TUMOR HUB, so §12.3's seventeen sections and §12.9's proved
  # template govern. The 48-line stub this replaces cited ONE source and it was
  # NCI's /types/brain -- a BRAIN page cited for a spinal tumor, on the one page
  # in the corpus whose central claim is that this is not a brain tumor. That is
  # the SIXTH stub in a row carrying that same source.
  #
  # NO BLOCK DIRECTIVE IS WRITTEN IN BRACKETS ANYWHERE IN THIS FRONT MATTER.
  # ContentBlocks.Directives walks every line outside a fenced code block --
  # front matter included -- so a bracketed name in a comment registers as a real
  # directive in DirectBlockNames, and CaregiverSectionTests locates the
  # caregiver block with a raw IndexOf over the whole file. Block names below are
  # written without brackets for that reason.
  #
  # THE ITEM'S CENTRAL RULING, AND IT IS NOT THE ONE THE BACKLOG EXPECTED.
  # WI-536 handed over "one claim, two strengths": this hub, /tumors/meningioma
  # and /tumors/brain-metastases file new weakness and bladder trouble as "a
  # reason to be seen quickly", while blocks/spinal-cord.md files the same signs
  # as right-away, at any hour. IT WAS NEVER ONE CLAIM. Two claims wear one
  # sentence, and levelling the three pages would have been as wrong as leaving
  # them split:
  #
  #   * METASTATIC cord compression -- cancer that has spread and is pressing on
  #     the cord -- carries the strong rule, and US patient-facing sources say so
  #     in plain words (ACS, OncoLink below).
  #   * A PRIMARY tumor in or beside the cord does not. "A reason to be seen
  #     quickly" is the supported default. NO fetched source applies an
  #     emergency or emergency-room rule to a named primary cord tumor --
  #     ependymoma, astrocytoma, meningioma, schwannoma, hemangioblastoma --
  #     outside the cauda equina picture. AANS, ABTA, Columbia Neurosurgery and
  #     NCI PDQ each list the four signs and give the reader NO urgency rule at
  #     all. Those are recorded absences, checked deliberately, not gaps.
  #   * THE CARVE-OUT APPLIES TO BOTH. New bladder or bowel trouble, numbness
  #     around the saddle area, or weakness getting worse over hours to days is
  #     emergency care now, whichever kind of tumor it is.
  #
  # The clincher sits inside ONE publisher. ACS says "call your doctor right away
  # or go to the emergency room" for cord compression, and on its own spinal cord
  # TUMOR page says only "see a doctor". ACS grades the two situations
  # differently itself, so the distinction is carried rather than invented.
  #
  # CONSEQUENCE: THIS HUB DOES NOT INCLUDE THE SPINAL-CORD BLOCK. That block
  # asserts right-away for a tumor that is IN the cord, full stop, which is the
  # over-triage direction for the primary tumors that are most of this page's
  # readers. Including it would pull a tier onto the one page that owns the
  # subject and can state the split precisely. The block is NOT re-tiered either:
  # it ships on four hubs, three of which reach the spine by spread, and its own
  # front matter records over-triage there as a knowing choice.
  #
  # EVERY LOAD-BEARING CLAIM WAS VERIFIED AGAINST THE LIVE PUBLICATION, not
  # against this item's research pack. Three agents returned 118 files; the five
  # claims this page turns on were re-fetched and re-read by me, and all five came
  # back character-exact. Log: .claude/work_files/wi543/plan.md.

  # ---- the urgency split, the claim the item turns on ----
  - url: https://www.cancer.org/cancer/supportive-care/advanced-cancer/bone-metastases.html
    title: "Bone Metastases | American Cancer Society"
    accessed: 2026-09-19
    # THE STRONG HALF, for the metastatic reader. Re-fetched live by me.
    # Verbatim: "If you notice symptoms like these, call your doctor right away or
    # go to the emergency room." and "Spinal cord compression is an oncology
    # emergency. If it isn't treated right away, the person can become paralyzed."
    # Its list is "Back pain (sometimes with pain going down one or both legs)",
    # "Numbness of the legs or belly", "Leg weakness or trouble moving your legs",
    # "Loss of control of urine or stool (incontinence) or problems passing urine"
    # -- ONE instruction over all four, so this page does not rank bladder and
    # bowel below weakness either.
  - url: https://www.oncolink.org/cancer-treatment/hospital-helpers/oncologic-emergencies/spinal-cord-compression
    title: "Spinal Cord Compression | OncoLink (Penn Medicine)"
    accessed: 2026-09-19
    # The only US patient-facing source found that names 911: "Call 911 or your
    # care team right away if you have any of these signs." Cited for the ROUTE.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging/signs-and-symptoms.html
    title: "Signs and Symptoms of Brain and Spinal Cord Tumors in Adults | American Cancer Society"
    accessed: 2026-09-19
    # THE SAME PUBLISHER, THE WEAKER HALF, and this is why the split is real
    # rather than a compromise. For the identical symptom list on its spinal cord
    # TUMOR page: "if you have any of these symptoms, especially if they don't go
    # away or get worse, see a doctor so the cause can be found and treated, if
    # needed." Also verbatim, and used for the both-sides point: "Spinal cord
    # tumors can cause numbness, weakness, or lack of coordination in the arms
    # and/or legs (usually on both sides of the body), as well as bladder or bowel
    # problems."
  - url: https://my.clevelandclinic.org/health/diseases/22132-cauda-equina-syndrome
    title: "Cauda Equina Syndrome | Cleveland Clinic"
    accessed: 2026-09-19
    # THE CARVE-OUT, and the strongest US warrant for an emergency-room
    # instruction on a tumor page. Re-fetched live by me. Verbatim: "Visit the
    # emergency room immediately if you experience any of these symptoms." Tumors
    # are named among the causes ("Spinal lesions or tumors."). The window is
    # stated to the patient: "Symptoms start suddenly and get worse quickly.
    # You'll likely need surgery within 24 to 48 hours after symptoms begin."
    # SCOPED to the cauda equina picture -- bladder or bowel, saddle numbness, leg
    # weakness -- NOT to any new symptom. The page carries that scope.
  - url: https://www.orthoinfo.org/en/diseases--conditions/cauda-equina-syndrome/
    title: "Cauda Equina Syndrome | OrthoInfo (American Academy of Orthopaedic Surgeons)"
    accessed: 2026-09-19
    # The second US patient-facing source attaching speed to bladder and bowel
    # specifically: "See your doctor immediately if you have: Bladder and/or bowel
    # dysfunction". "A tumor" is listed among the causes. Also the saddle area in
    # plain anatomical words.
  - url: https://neurosurgery.weillcornell.org/condition/spinal-tumors/symptoms-spinal-tumor
    title: "Symptoms of a Spinal Tumor | Weill Cornell Medicine Neurological Surgery"
    accessed: 2026-09-19
    # A US neurosurgical department applying speed to a NARROW set on a spinal
    # TUMOR page: "Seek treatment immediately if you have: Impaired bladder or
    # bowel function; Paralysis", while numbness and difficulty walking sit in the
    # ordinary symptom list. Cited for the narrowness, which is the page's shape.
  - url: https://www.christopherreeve.org/todays-care/living-with-paralysis/health/causes-of-paralysis/spinal-tumors/
    title: "Spinal Tumors | Christopher & Dana Reeve Foundation"
    accessed: 2026-09-19
    # THE CLEANEST WARRANT FOR TREATING THE TWO CASES DIFFERENTLY, patient-facing
    # and US. Re-fetched live by me. Verbatim: "Primary tumors are those that
    # arise within the spinal cord or supporting structures. These tumors usually
    # slowly progress. Secondary tumors are those that have spread from other
    # parts of the body or metastasize. These tumors progress quickly." Also
    # "Usually, symptoms develop over time, however some develop symptoms
    # suddenly." -- which is why this page says most, not all.

  # ---- what it is, and where it sits ----
  - url: https://www.aans.org/patients/conditions-treatments/spinal-tumors/
    title: "Spinal Tumors | American Association of Neurological Surgeons"
    accessed: 2026-09-19
    # THE THREE COMPARTMENTS IN THE PLAINEST AVAILABLE WORDING, from a US
    # neurosurgical body, and the reason this page can describe them at a 6th
    # grade reading level at all. Re-fetched live by me, verbatim: "These tumors
    # grow inside the spinal cord"; "The tumor is located inside the thin covering
    # of the spinal cord (the dura), but outside the actual spinal cord"; "The
    # tumor is located outside the dura, which is the thin covering surrounding
    # the spinal cord." Also: "Primary tumors originate in the spine or spinal
    # cord, and metastatic or secondary tumors result from cancer spreading from
    # another site to the spine."
    #
    # AND THE PAIN CUES, which are this page's most useful early material:
    # "This back pain is not specifically attributed to injury, stress or physical
    # activity." and "the pain may increase with activity and can be worse at
    # night when lying down."
    #
    # NOTE ON THE RESEARCH PACK: the extract for this source recorded the middle
    # group as containing "filum terminale ependymomas". The LIVE page says plainly
    # "ependymomas". The live reading is used, and the sentence is worded so it
    # does not rest on the narrower gloss.
    #
    # RECORDED ABSENCE, checked: this page gives the reader NO urgency
    # instruction of any kind. That absence is part of the ruling above.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK12643/
    title: "Spinal Cord Tumors | Holland-Frei Cancer Medicine (NCBI Bookshelf)"
    accessed: 2026-09-19
    # Which tumor sits in which place, in one sentence: "Tumors of glial origin
    # (eg, astrocytomas, ependymomas) are usually intradural intramedullary in
    # location, whereas nerve sheath tumors (eg, neurofibromas and schwannomas)
    # are typically intradural extramedullary lesions." And "Meningiomas can be
    # either extradural or intradural extramedullary lesions."
    #
    # CITED FOR THAT AND NOT FOR FREQUENCY (§12.13, per-claim). This chapter also
    # says astrocytomas "comprise 90% of all intramedullary neoplasms in adults",
    # which THREE newer sources contradict -- ependymoma is the commonest. The 90%
    # figure is treated as stale and is not carried in any form.
  - url: https://www.msdmanuals.com/professional/neurologic-disorders/intracranial-and-spinal-tumors/spinal-tumors
    title: "Spinal Tumors | MSD Manual Professional Edition"
    accessed: 2026-09-19
    # The third independent statement of the same scheme, and the list of primary
    # cancers behind the outside-the-covering group: "Most extradural tumors are
    # metastatic, originating as carcinomas of the lungs, breasts, prostate,
    # kidneys, or thyroid." Deliberately printed unranked -- a second source gives
    # a different order, and neither ranking is worth a reader's attention.
    # Also the steroid measure: "Glucocorticoids (eg, dexamethasone, initially
    # intravenous and then oral) are begun immediately to reduce spinal cord
    # edema." No dose is carried; /treatments/steroids owns that.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12094706/
    title: "Spinal Intramedullary Tumors (review, 2025)"
    accessed: 2026-09-19
    # THE TREATMENT SPINE. "Microsurgical resection is the preferred treatment for
    # intramedullary tumors." The pivot this page is built on: "An essential step
    # in the operation is the identification and pursual of a plane of dissection
    # between the tumor and the surrounding spinal cord tissue." Monitoring:
    # "Microneurosurgery, ultrasonic aspiration, and intraoperative
    # neurophysiological monitoring are essential for successful tumor removal."
    # Follow-up: "Because of the risk of asymptomatic and/or late recurrence,
    # patients should be monitored over the long term with serial MRI scanning."
    # The open question, in the authors' own words: "There is still debate in the
    # scientific literature about the relation between overall and
    # progression-free survival and the extent of resection."
    # Makeup of the group inside the cord: "The main types of intramedullary
    # glioma are ependymoma (60-70%) and astrocytoma (30-35%)" -- used as
    # DIRECTION ONLY (ependymoma is the commoner), no percentages printed.
    #
    # RADIATION INDICATIONS ARE CARRIED, DOSES ARE NOT. The source gives Gy
    # figures for each indication and every one is excluded (§12.4).
    #
    # ONE SENTENCE OF THIS SOURCE IS DELIBERATELY NOT USED. Its anaplasia sentence
    # came back from extraction reading "The current SHO classification", an
    # apparent typo for WHO, and was flagged for character-level re-verification.
    # Rather than re-fetch to quote a sentence this page does not need, it is
    # dropped: /tumors/ependymoma already owns the anaplasia crosswalk.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8328013/
    title: "The 2021 WHO Classification of Tumors of the Central Nervous System: a summary (Louis et al)"
    accessed: 2026-09-19
    # THE NAMING AUTHORITY (§12.1), cited for the convention and for what it
    # prints. Verbatim: "Arabic numerals are employed (rather than Roman numerals)
    # and neoplasms are graded within types (rather than across different tumor
    # types)", and it "endorses use of the term 'CNS WHO grade'". Ependymoma is
    # classified by site, spinal among them.
    #
    # CITED AS AN ABSENCE TOO, deliberately: it prints NO grade beside schwannoma,
    # in contrast to meningioma's 1, 2 and 3. This page does not restate that
    # argument -- /tumors/acoustic-neuroma owns it and verified it live -- but the
    # absence is why this page never prints a grade for a nerve sheath tumor.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11718997/
    title: "The Role of Simpson Grading System in Spinal Meningioma Surgery (systematic review and meta-analysis)"
    accessed: 2026-09-19
    # THIS PAGE'S OWN CROSSWALK SLICE, and the reason it has an angle of its own
    # rather than a copy of the block's (§12.11). The Simpson grade describes HOW
    # COMPLETELY the tumor and its attachment came out, and it is NOT a WHO tumor
    # grade -- two different "grade 1"s, on paperwork the same reader may be
    # holding at once. Verbatim: "Grade I: Macroscopically complete tumor
    # resection with removal of affected dura and underlying bone" and "Grade II:
    # Macroscopically complete tumor resection with coagulation of affected dura
    # only".
    # The honest limit, also verbatim: "Solid evidence is lacking in the
    # literature regarding the objective risk of recurrence of spinal meningiomas
    # following higher Simpson resection grades" and "There is no unanimous
    # consent in the pertinent literature with contrasting results, with some
    # studies reporting no difference."
    # ROMAN NUMERALS ARE THIS PAPER'S, NOT THIS SITE'S. It writes WHO grade I and
    # II throughout; the page uses Arabic for WHO grades and keeps the Roman form
    # only where it is naming what old paperwork says.

  # ---- how it presents, and how slowly ----
  - url: https://www.ncbi.nlm.nih.gov/books/NBK442031/
    title: "Intramedullary Spinal Cord Tumors | StatPearls (NCBI Bookshelf)"
    accessed: 2026-09-19
    # The slow course, which is half the urgency ruling: "often asymptomatic for
    # prolonged periods"; "the most common presenting symptom is pain";
    # "Paresthesias are the second most common complaint, followed by motor
    # impairment"; and the late half, verbatim: "In later stages, loss of bowel
    # and bladder function can occur and can present as retention, incontinence,
    # or impotence". Also "Gait disturbances are frequently observed signs that
    # may be misperceived as clumsiness", which is how people actually describe it.
    # Night pain: "Pain can be diffuse or radicular and typically worsens at night
    # when the patient is lying down."
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8088529/
    title: "Intradural extramedullary spinal tumors: outcomes (Surgical Neurology International)"
    accessed: 2026-09-19
    # How long these go on before anybody finds them, for the group that sits
    # beside the cord: "symptomatic on average for nearly a year (e.g.,
    # mean-11.56 months) before diagnosis". NO SINGLE AVERAGE IS PRINTED. The
    # sources give 6 to 37 months, an average of 22.8 months, and this one -- so
    # the page says months, often a year or more, which all of them support.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC10542475/
    title: "Neurologic outcomes for adult spinal cord ependymomas by tumor location"
    accessed: 2026-09-19
    # CITED FOR THE ANATOMY AND NOTHING ELSE (§12.13, per-claim). "the thoracic
    # spinal cord and corresponding canal have the lowest diameter along the
    # spinal axis" is what supports the page's line about the middle of the back
    # being the narrowest stretch.
    # /review round 1 CUT THE SENTENCE THAT FOLLOWED IT. The page said that was
    # "part of why trouble there shows up sooner", which this paper does NOT say --
    # its finding is about worse POST-OPERATIVE outcomes for thoracic tumors,
    # a different claim entirely. The anatomy stayed; the causal half went.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC9249684/
    title: "Ependymoma: Evaluation and Management Updates"
    accessed: 2026-09-19
    # THE RECURRENCE PATTERN: "The predominant pattern of relapse is local."
    # ADDED AT /review round 1, WHICH CAUGHT THIS AS AN UNCITED CLAIM -- and
    # §12.14 names that exact claim as one that shipped behind a ban on an earlier
    # item, so it is the second time this corpus has carried it unsupported.
    # SCOPED ON THE PAGE, TOO. The finding is about EPENDYMOMA, and the first
    # draft generalised it to every tumor this page covers, including meningioma,
    # schwannoma and metastases. The sentence now names the tumor the source
    # studied.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK537200/
    title: "Cauda Equina and Conus Medullaris Syndromes | StatPearls (NCBI Bookshelf)"
    accessed: 2026-09-19
    # That tumors cause this picture, and that it is an emergency: "Both syndromes
    # are neurosurgical emergencies", with the cause named as "either metastatic
    # or a primary central nervous system cancer" -- which is what makes the
    # carve-out apply to BOTH halves of this page's readership rather than only
    # the metastatic one.

  # ---- surgery, recovery, and the claim this page must not make ----
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC13207646/
    title: "Clinical Characteristics and Surgical Outcomes of Intradural Spinal Tumor Resections"
    accessed: 2026-09-19
    # THE DEFINITION of the dip after surgery, which is the most useful single
    # thing this page can tell somebody before an operation: "Transient
    # neurological deficits were defined as new or worsened motor deficits
    # observed in the early postoperative period that resolved by the 6-month
    # follow-up." Its rates are not printed.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC2072903/
    title: "Surgery for intramedullary spinal cord tumors: the role of intraoperative monitoring"
    accessed: 2026-09-19
    # THE OTHER SIDE OF THE CONTRADICTION, AND THE PAGE DOES NOT PICK A WINNER.
    # This source says the transient deficit "invariably recovers over a period of
    # days, weeks, or months after surgery" -- but the phenomenon is DEFINED by
    # recovering, so the sentence is close to circular. The 2025 review above
    # counts everyone who is worse after surgery and reports that such problems
    # "resolve completely in 25% to 41% of cases". They are not measuring the same
    # group, which is why both can be true.
    #
    # SO THE PAGE SAYS: a dip afterwards is common and has a name, it often
    # settles over weeks to months, and it does not always settle completely. It
    # does NOT say function comes back, and prints no figure either way. §12.12:
    # over-reassurance is the more dangerous failure, and this is the sentence on
    # this page most likely to stop being true for the reader holding it.
    #
    # RECORDED ABSENCE: no source found in this session gives PATIENT-FACING
    # guidance on what to do about a temporary worsening after surgery. The thing
    # people most need warning about exists only in the surgical literature, which
    # is why this page says it plainly and tells people to ask before the operation.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11758597/
    title: "Role of Rehabilitation in Spine Tumors"
    accessed: 2026-09-19
    # THE ACTIONABLE GAP, and a sourced reason to tell people to ask: patients
    # with spine tumors "are not routinely admitted to inpatient rehabilitation
    # units or referred to outpatient cancer rehabilitation", despite the benefit
    # evidence. Also the content of rehab: "physical and occupational therapy
    # focusing on strength, sensory training, transfers, endurance, balance, range
    # of motion, and activities of daily living".
  - url: https://msktc.org/sci/factsheets/guide-inpatient-rehabilitation-services-people-spinal-cord-injury
    title: "A Guide to Inpatient Rehabilitation Services for People With Spinal Cord Injury | MSKTC"
    accessed: 2026-09-19
    # What rehabilitation actually involves, in plain words: "you can take part in
    # at least 3 hours of therapy, 5 days per week"; the named team including a
    # physiatrist, "physician who specializes in rehabilitation"; and "Rehab
    # nurses educate patients about bowel and bladder management and how to
    # prevent complications".
    #
    # POPULATION CAVEAT, AND THE PAGE SAYS IT OUT LOUD. This and the four sources
    # below are written for spinal cord INJURY, not tumor. The functional problem
    # is the same one and no tumor-specific patient material of this quality was
    # found; the page tells the reader that rather than implying evidence it does
    # not have. There IS no /treatments/rehabilitation page yet to route to, so
    # this hub carries the material itself.
  - url: https://msktc.org/sci/factsheets/bowel-function-after-spinal-cord-injury
    title: "Bowel Function After Spinal Cord Injury | MSKTC"
    accessed: 2026-09-19
    # "A bowel program includes four parts: timing, diet (including food and
    # fluids), medicines, and techniques to help with bowel movements." and the
    # who-helps answer: "A doctor or nurse designs a bowel program specifically
    # for you and with you." Its fiber and fluid quantities are NOT carried -- they
    # are population figures, and the source's own point is that the program is
    # built for the individual.
  - url: https://msktc.org/sci/factsheets/pain-after-spinal-cord-injury
    title: "Pain after Spinal Cord Injury | MSKTC"
    accessed: 2026-09-19
    # The anti-hype model for this whole section, verbatim: "It is hard to
    # eliminate chronic pain. But people with SCI can manage or lessen their pain
    # enough so that it does not overwhelm their lives." and "Chronic pain is not
    # hopeless. Try not to become discouraged if one treatment doesn't work."
    # Also the reader's own vocabulary: "People use words such as burning,
    # stabbing, electric, numb, or tingling to describe nerve pain."
  - url: https://msktc.org/sci/factsheets/spasticity-and-spinal-cord-injury
    title: "Spasticity and Spinal Cord Injury | MSKTC"
    accessed: 2026-09-19
    # The nuance that makes this worth a paragraph rather than a line: "Spasticity
    # may help with functional tasks such as standing or transferring." So the aim
    # is a balance, not removal. Also "Stretching or doing range of motion
    # exercises at least twice a day will help you reduce muscle tightness" and
    # "When physical treatments are not enough to control spasticity, you may need
    # to try medicine." Its prevalence range is not printed.
  - url: https://www.christopherreeve.org/todays-care/living-with-paralysis/health/secondary-conditions/skin-care/
    title: "Skin Care | Christopher & Dana Reeve Foundation"
    accessed: 2026-09-19
    # THE MECHANISM SENTENCE, re-fetched live by me and the best on the site for
    # explaining why this becomes a deliberate job: "Sensation would signal you to
    # move your body to open the capillaries prior to skin damage but after spinal
    # cord injury, that signal is decreased or absent." The intervals are carried
    # as §12.4 R1 orienting durations, attributed: "you should perform pressure
    # releases every 10 to 15 minutes for 60 seconds while awake", "In bed,
    # turning every two hours or less is needed", "Look at your entire body a
    # minimum of twice a day". And the anti-hype line: "Notice this equipment is
    # called pressure 'reducing' not pressure eliminating."
  - url: https://www.christopherreeve.org/todays-care/living-with-paralysis/health/secondary-conditions/bladder-management/
    title: "Bladder Management | Christopher & Dana Reeve Foundation"
    accessed: 2026-09-19
    # The sentence that defuses the shame framing directly, verbatim: "Bladder
    # programs should not be confused with potty training." and the reason:
    # "Bladder programs are put into place because of an issue with the nerves of
    # the body." Also that infection is treated early: "Infections are easier to
    # treat when caught early."

  # ---- not a brain tumor ----
  - url: https://academic.oup.com/neuro-oncology/article/27/Supplement_4/iv1/8285946
    title: "CBTRUS Statistical Report: Primary Brain and Other CNS Tumors (2018-2022)"
    accessed: 2026-09-19
    # THE STRUCTURAL EVIDENCE for the taxonomy rule WI-412 pinned with a test and
    # this item must not undo. The spinal cord carries its OWN site codes --
    # "Spinal cord and cauda equina C72.0-C72.1" -- listed apart from brain, and
    # the report's own title separates "Brain and Other Central Nervous System
    # Tumors". No source fetched in this session calls a spinal cord tumor a brain
    # tumor. What they support is the CNS framing this page uses instead: the
    # brain and the cord are the two parts of one system, cared for by the same
    # kinds of specialists, at a different site.

  # ---- BRITISH SOURCES: FACTS ONLY, NEVER WORDING OR PATHWAY ----
  # Two British sources were read for facts and are NOT cited here, because
  # nothing on this page rests on them alone. Cancer Research UK is currently the
  # only source giving the plain "top of the spine affects the arms, lower down
  # affects legs, bladder and bowel" explanation, and that FACT is carried in US
  # English and is also supported by the sources above. Macmillan is the only
  # source addressing out-of-hours ("Even if it is the weekend or a holiday"), and
  # that clause is NOT used: no US source reached states an hour-of-day rule, so
  # this page says "right away" and names the emergency room instead. Neither
  # source's pathway is reproduced -- no MSCC coordinator, no GP, no A&E, no 999,
  # no alert card.
  #
  # ONE SOURCE WAS NEVER REACHED AND NOTHING RESTS ON IT: cIMPACT-NOW update 7
  # was blocked at every host tried. It is not cited and no claim here depends on
  # it (§12.14: a banned or unreachable citation reads like a handled claim, so
  # the absence is recorded where the next reader of this file will see it).
reviewed: 2026-09-19
review_due: 2027-03-19
disclaimers: [medical]
---

## The short version

**A spinal cord tumor is a growth in or pressing on the spinal cord.** It is not
a brain tumor, and the words on your report will be different from the ones on
a brain tumor page. **Most tumors that start here grow slowly**, often causing
vague trouble for months before anybody finds them. **Surgery is the main
treatment for one that started here.** Whether all of it can come out depends on
whether it has a clear edge. Some signs mean calling your team the same day.
Some mean calling right away. A few mean the emergency room. They are in their
own section below.

## What is a spinal cord tumor?

Your spinal cord is a thick bundle of nerves running down inside your backbone.
It carries messages between your brain and the rest of your body. **A tumor here
grows in the cord itself, or beside it, or in the bone around it and presses
in.**

**This is not a brain tumor.** The brain and the spinal cord are the two parts
of one system, called the central nervous system. The same kinds of specialists
look after both. Research about them often appears together. But this is a
different place in the body, with its own names and its own treatment. A page
about brain tumors will not answer your questions.

There are two quite different situations, and almost everything on this page
depends on which one is yours.

- **A tumor that started here.** Doctors call this a primary tumor. Most of
  these grow slowly.
- **A cancer that started somewhere else and has spread to the spine.** Doctors
  call this a secondary tumor, or a metastasis. These usually move faster, and
  the section on when to get help is different for you.

If you do not know which yours is, that is a good first question for your team.

## Is it cancer? What does its grade mean?

**Many of these tumors are not cancer.** A growth beside the cord is often
benign. That means it stays where it is instead of seeding elsewhere. It can
still cause real trouble, because the space it grows in is narrow and what it
presses on matters.

**A grade describes how fast a tumor is likely to grow**, not how serious your
situation is. A grade is now written as an ordinary number, so grade 2 rather
than grade II.

**Some of these tumors carry no grade at all, and that is an answer rather than
a gap.** The rules changed in 2021, and for several kinds the people who write
them decided a grade would not help. If your report has a name and no number
beside it, nothing is missing. Ask your team to read your report with you.

## Where does it grow, and why does it cause these symptoms?

**Where it sits decides almost everything**, so this is worth understanding once.
Doctors sort these tumors into three places.

- **Inside the cord itself.** These grow in the substance of the cord, among
  the nerve fibers.
- **Inside the covering, but outside the cord.** A thin covering called the
  dura wraps the cord. A tumor can sit inside that wrapping and press on the
  cord from outside it.
- **Outside the covering.** Usually this is cancer that has spread from
  somewhere else, most often from the lung, breast, prostate, kidney or thyroid.

This is a surgeon's way of describing anatomy, not a grading system. It is why
the honest phrase is **in or pressing on the cord**. A growth sitting beside the
cord is not in it.

Which kind of tumor turns up in which place is fairly predictable. The ones that
grow from nerve covering, and the ones that grow from the wrapping, usually sit
inside the covering but outside the cord. The ones that start in the cord's own
supporting cells usually sit inside the cord.

**And the level decides which parts of you are affected.** The cord serves
different parts of the body at different heights. High up, the arms are
involved. Lower down, it is the legs, the bladder and the bowel. The middle part
of your back is the narrowest stretch of the whole canal.

**Starting from where it sits, and two honest limits on that.** [Where your
tumor is, and what that changes](/where-your-tumor-is) is arranged by place
rather than by diagnosis. Its list of regions was written for the head. So
was its list of urgent signs. The page says both of those out loud, and each
time it sends a cord reader back here. What is left over is still yours. It
says why it has no list of which places are dangerous. And it has the questions
to put to a surgeon about a place.

## What symptoms does it cause?

**Pain is usually the first thing people notice**, and it has a pattern worth
knowing. It is often in the back or neck. Two things set it apart from ordinary
back pain. It is **not explained by an injury or by what you have been doing**.
And it is often **worse at night, or when you lie down**. Over time it can become
bad even at rest.

After the pain, altered feeling tends to come next, and then weakness.

- Numbness, tingling or a burning feeling, often in both arms or both legs.
- Weakness, or limbs that feel heavy.
- Trouble walking, or being unsteady. People often put this down to clumsiness
  at first.
- Changes in the bladder or the bowel.

Those changes to the bladder and bowel usually come later rather than first, so
waiting for them is not a good way to decide anything.

Because this builds slowly, many people have symptoms for months, often a year
or more, before anyone finds the cause. **That is ordinary here, and it is not
something you missed.**

### When a spinal cord symptom means going in now

**These signs are the spinal cord's own, and two of them change what you should
do.**

**If your cancer has spread to your spine, some new signs mean calling your team
right away or going to the emergency room.** They are new weakness in your legs,
new numbness, new trouble walking, or losing control of your bladder or bowel.
The American Cancer Society calls pressure on the cord an emergency. It says
that untreated, it can cause paralysis. Do not hold this until your next
appointment, and do not wait to see whether it settles.

**If your tumor started here, new weakness in your legs, new numbness or new
trouble walking are a same-day call rather than a trip to the emergency room.**
Call your team today about those three, rather than at the next routine visit.
Most tumors that start in or beside the cord grow slowly, and the patient
organizations that write about them ask you to be seen rather than to go to the
emergency room. **Losing control of your bladder or bowel is not on that list
because it is more urgent, not less. The next paragraph says why.**

**But one picture is emergency care now, whichever kind of tumor you have.** Go
to the emergency room for new trouble controlling your bladder or bowel. Go for
new numbness around the area you would sit on. And go for weakness that is
getting worse over hours or days. Cleveland Clinic says to go at once for that
picture, and names tumors among its causes. That surgery is usually needed
within a day or two of the symptoms starting. That is why timing matters more
here than anywhere else on this page.

**Bladder and bowel changes are never less urgent than weakness.** If you are
unsure which of these applies to you, call your team and describe what has
changed.

**The list below is the general one, written for any tumor.** Some of it is about
the brain rather than the cord, so not all of it will be yours. The rules about
seizures, about a fever during chemotherapy, and about a shunt are worth knowing
if any of those apply to you.

[ESCALATION]

## How do doctors find out it is this?

**The test that finds it is nearly always an MRI scan.** [MRI scans](/tests/mri)
covers what having one is like.

**For a tumor that started here, the operation is usually where the exact answer
comes from.** The piece taken out goes to the laboratory, so the diagnosis and
the treatment often arrive together. If your cancer spread to the spine, the
picture is usually clearer already, because the first cancer has a name. Ask
your team whether a sample is part of the plan for you, and what they expect to
learn from it.

[Your pathology report](/tests/pathology-report) walks through the document you
get back, and [waiting for results](/tests/waiting-for-results) is about the gap
in between.

## What do the words on my report mean?

[CROSSWALK]

### Two different things are both called grade 1

**This one catches people out, and it is worth reading twice.** If you have had
an operation, you may be holding two documents that both use the word grade, and
they mean completely different things.

- **A tumor grade** describes how the tumor is likely to behave.
- **A Simpson grade** describes how completely the surgeon was able to remove
  the tumor and the covering it was attached to. Grade 1 there means the tumor
  and the affected covering and bone all came out.

So "grade 1" on an operation note can describe a thorough operation rather than
your tumor. If you see the word twice and it does not add up, that is the
likeliest reason, and it is worth raising with your team.

**How much of the covering needs to come out is genuinely unsettled.** Surgeons
do not all agree, the studies disagree with each other, and some find no
difference at all. If your team has a view, it is reasonable to ask what it is
based on.

### If your paperwork says something different

- **A name with no grade beside it.** For several kinds, including the ones that
  grow from nerve covering, the 2021 rules print no grade. Older paperwork
  sometimes gives one anyway. [Acoustic neuroma](/tumors/acoustic-neuroma)
  explains that in full for the nerve covering kind.
- **A grade in Roman numerals.** Older paperwork writes grade 1 as grade I, and
  it is the same grade.
- **A name that mentions where it is.** Some names now include the part of the
  spine the tumor is in. The same kind of tumor can behave differently in
  different places.

[Ependymoma](/tumors/ependymoma) has the detail for the commonest kind that
starts inside the cord, including the one whose grade was changed in 2021.

## How is it usually treated?

**For a tumor that started in or beside the cord, surgery is the main
treatment**, and for many people it is the only one they need. **If your cancer
spread to the spine, the plan often starts differently**: a steroid to take the
pressure down, then radiation or surgery depending on the tumor and on how
quickly things are changing.

**What the operation can achieve turns on one thing: whether there is a clear
plane between the tumor and the cord.** Finding and following that edge is the
key step of the operation. Some tumors have one, and can often be taken out
completely. Others grow into the cord with no clear boundary. Taking more then
carries more risk of harm. **This is why two people with tumors in the same place
can be offered quite different operations.** It is a fair question to ask
directly: does mine have an edge you can work along?

**The team watches the cord during the operation.** Monitoring nerve signals
while operating is standard here, and its purpose is to warn the surgeon before
damage is done rather than after.

**Radiation is not routine.** It is used when some of the tumor had to be left
behind, or for the faster-growing grades. [Radiation therapy](/treatments/radiation-therapy)
covers what a course involves. Your team sets the dose and the timing.

**A steroid is often started quickly when a tumor is pressing on the cord**, to
bring down swelling. It treats the pressure, not the tumor.
[Steroids](/treatments/steroids) covers what taking one is like and the rules
about stopping.

**Some tumors are watched rather than treated.** A small one causing no trouble
may be followed with scans instead. [Watching a tumor, step by
step](/treatments/watch-and-wait) covers what that plan feels like day to day.

**For some of these there is no agreed best treatment, and saying so is more
useful than pretending.** For tumors that grow into the cord without a clear
edge, specialists openly disagree about how much surgery helps and when to
operate. That is a reason to ask what your team thinks and why. It is also why a
second opinion is reasonable rather than rude.!%tumor board%

[TUMOR-BOARD]

[Clinical trials](/treatments/clinical-trials) covers how to think about a trial
if one is mentioned.

## What is treatment actually like, and what is normal afterwards?

### The dip after surgery

**Many people are worse for a while after this operation before they are
better, and almost nobody is warned.** New or increased weakness in the days
after surgery is common enough to have a name, and it often settles over weeks
to months. **The emergency rule above still applies after your operation.**
Weakness that keeps getting worse, new numbness around the area you would sit
on, or new trouble controlling your bladder or bowel means the emergency room
now, whichever kind of tumor you have. A dip being common does not make those
signs something to sit at home with.

**It does not always settle completely, and anyone who tells you it always does
is going further than the evidence.** The studies disagree, partly because they
are counting different groups of people. What is fair to say is that a dip is
common, and that it can go on improving for months. Where you end up is not
known on the day you wake up.

**So ask before the operation**: what should I expect in the first days, what
would worry you, and who do I call once I am home?

### Rehabilitation

**Rehabilitation is an ordinary next step here, not a sign anything went wrong.**
It is team-based care aimed at function: strength, balance, transfers, and the
ordinary activities of a day. Inpatient rehabilitation usually means taking part
in at least three hours of therapy a day, five days a week.

The team has a shape worth knowing. A rehabilitation doctor leads it. Physical
and occupational therapists work on movement and on daily tasks. Rehabilitation
nurses teach bladder and bowel care. A psychologist and a social worker are part
of it too. The social worker usually plans what happens when you go home.

**Ask about rehabilitation rather than waiting to be offered it.** People with
spine tumors are not routinely referred to rehabilitation units in the way that
people with similar problems from other causes are. That is documented, and it
is a reason to raise it yourself.

## Everyday life: work, moving around, and the things nobody mentions

Much of what helps here was worked out for people whose spinal cord was damaged
in other ways. **The cause is different and the problem is the same one**, so
that is where the good practical material comes from. Your own team can tell you
what applies to you.

**Bladder and bowel care.** If the nerves to your bladder are affected, the
signals between it and your brain do not arrive properly. A bladder program is
put in place because of a nerve problem. As one patient organization puts it,
**it should not be confused with potty training.** A bowel program has four
parts: timing, diet, medicines, and the techniques that help. A doctor or nurse
designs it with you rather than for you. Infections are easier to treat when
caught early, so report one rather than waiting.

**Pain.** Nerve pain is common and people describe it as burning, electric,
stabbing or tingling. Ordinary painkillers often do not touch it, which is not a
sign that you are imagining it. The honest framing is the one a rehabilitation
service uses. It is hard to get rid of chronic pain completely. But it can
usually be reduced enough that it does not take over your life. If the first
treatment does not work, that is common. It is not the end of the list.

**Stiffness and spasms.** Muscles can stiffen or jump on their own. Stretching
and range of motion exercises at least twice a day help, and medicine can be
added when they are not enough. **Getting rid of it completely is not always the
aim**, because that stiffness sometimes helps with standing, transferring or
gripping. It is a balance, and it is worth telling your team which spasms are
actually a problem for you.

**Skin.** This is the one people underestimate. Normally, feeling tells you to
shift position long before your skin is damaged. After the cord is affected,
that signal is reduced or gone. So it becomes a deliberate job. The usual advice
is to relieve pressure for about a minute every ten to fifteen minutes while you
are up. In bed, turn every two hours or less. And look over your whole body at
least twice a day. Special cushions and mattresses reduce pressure. **They do
not eliminate it**, and no equipment does.

**Work and driving.** Both depend on what you can do rather than on the
diagnosis, and both are worth raising early. Driving rules differ from place to
place, so ask your own team and licensing authority rather than relying on any
page.

## Follow-up scans, and what to do while you wait

**Scans go on for a long time here, and there is a reason.** These tumors can
come back years later. They can start to come back without causing any symptom
at all. So a scan is the thing most likely to notice first. For
[ependymoma](/tumors/ependymoma), the commonest kind inside the cord, a tumor
that comes back most often does so where the first one was.

[Follow-up scans](/tests/follow-up-scans) covers what the reports mean and the
part most people find hardest, which is the wait rather than the scan.

Tell your team about a new symptom when it happens rather than saving it for the
next appointment.

## If it comes back, or changes

**It does not mean anyone made a mistake.** Coming back late is a known pattern
with these tumors, and it is exactly why the scans carry on for years.

What happens next depends on where it is, what was done last time, and what can
be done safely now. Surgery again is sometimes possible. Radiation may be offered
if it was not used before. Ask what the goal of any new treatment is. The answer
is not always the same as it was the first time.

## Did I cause this?

**If your tumor spread to the spine from a cancer elsewhere, the answer below is
written for a different question than yours.** You may find yourself going back
over whether it was caught late instead. Spread to the spine is a feature of the
illness, not evidence that anybody was slow. The people looking after you are
used to that question, and it is worth asking out loud.

[CAUSES]

## What might happen over time

:::outlook
What happens next turns on four things: the kind of tumor, where it sits, how
much came out, and how bad things were before treatment.

The thing that most shapes daily life afterwards is usually not the tumor's name
but how you were moving and feeling when treatment started. That is one of the
reasons teams do not like waiting once symptoms are progressing.

Many people with a slow-growing tumor that came out completely go back to
ordinary life, and are followed with scans for years. For tumors that grow into
the cord, more is left uncertain, and specialists say so themselves rather than
hiding it.

Your own team can see things a page cannot: your scans, what came out, and how
you have been since. They are the people to ask what this means for you.
:::

## For the person caring for someone with this

[CAREGIVER]

Four things specific to this diagnosis.

**Know the signs that mean the emergency room.** New trouble controlling their
bladder or bowel, new numbness around the area they sit on, or weakness that
keeps getting worse means going now rather than waiting for an appointment. You
may well be the one who notices first.

**The practical load here is physical, and it is not small.** Helping somebody
move, transfer or turn is a daily job, and people who do it often hurt their own
backs. Ask to be shown how to do it before you are doing it alone, and ask
whether equipment would help. That is not a luxury request.

**You may be the one who notices the skin.** Looking over the parts somebody
cannot see or feel is one of the few jobs that needs another pair of eyes.
Catching a red patch early prevents a long problem.

**The honest limit of the advice.** Most of the good practical material on
bladder, bowel and skin care was written for people whose spinal cord was
damaged in other ways. It is used because the problem is the same one, not
because it was studied in people with tumors.

## What to ask your team

- Did my tumor start here, or has it spread from somewhere else?
- Where exactly is it, and is it inside the cord or pressing on it from outside?
- What is its full name on the report, and is there a grade?
- Is there a clear edge between the tumor and my cord?
- What is the goal of the operation: taking all of it, or taking pressure off?
- What should I expect in the first days after surgery, and what would worry you?
- Will I be referred for rehabilitation, and who decides?
- Which symptoms mean calling you today, which mean calling right away, and
  which mean the emergency room?
- Who do I call after hours, and what is the number?
- How long will the scans go on, and how often?
- Is radiation likely for me, and if so, why?
- Is there a clinical trial for this kind?

## Where to get support

- [Getting help now](/get-help-now), which lists who to call and when.
- [The Christopher & Dana Reeve Foundation](https://www.christopherreeve.org/)
  has practical guides on bladder, bowel, skin and pain. It also runs an
  information service, staffed by people who answer questions like these daily.
- [The Model Systems Knowledge Translation Center](https://msktc.org/sci/factsheets)
  publishes plain-language factsheets on rehabilitation, pain, spasticity and
  daily life.
- [Ependymoma](/tumors/ependymoma) and [acoustic neuroma](/tumors/acoustic-neuroma)
  if your report names one of those.
- Your team's specialist nurse. On most days they are the quickest route to an
  answer about your own case.
