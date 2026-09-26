---
title: "Ependymoma"
slug: tumors/ependymoma
description: "A tumor that starts in the lining of the fluid spaces of the brain and spine. Where it grows, what the grade and the gene result mean, why surgery matters so much, and why the scans go on for years."
tags: [tumor-type]
sources:
  # Naming and grading from CNS5-aligned sources ONLY (§12.1). The trap sources are
  # listed in .claude/work_files/wi536-sources/NOTES.md: NCI-CONNECT pairs Arabic and
  # Roman grades; the Brain Tumour Charity says "anaplastic" and grade 1
  # myxopapillary; Dana-Farber says "Classic and anaplastic"; ABTA lists RELA beside
  # ZFTA. None of those is cited here for a name or a grade.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8328013/
    title: "The 2021 WHO Classification of Tumors of the Central Nervous System: a summary - PMC"
    accessed: 2026-09-15
    # Verbatim: classified by "histopathological and molecular features as well as
    # anatomic site"; ZFTA is "the new designation for C11orf95 ... more
    # representative of the tumor type than RELA"; "group PFA and group PFB";
    # "a spinal tumor defined by the presence of MYCN amplification"; site-only
    # names are used "when molecular analysis finds a different molecular
    # alteration ... or when molecular analysis fails or is unavailable";
    # myxopapillary "now considered CNS WHO grade 2 rather than 1, since its
    # likelihood of recurrence is now understood to be similar to conventional spinal
    # ependymoma"; "the term “anaplastic ependymoma” is no longer listed"; a
    # pathologist "can still choose to assign either CNS WHO grade 2 or grade 3";
    # papillary, clear cell and tanycytic are "patterns", no longer subtypes.
    # Ependymal tumors are their own group, (6), inside "Gliomas, glioneuronal
    # tumors, and neuronal tumors".
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8018155/
    # The fetched title has a Unicode hyphen (U+2010) in "cIMPACT-NOW". It is an
    # ASCII hyphen here, like the en dash on the DMG page (NoEmDashInCopyTests).
    title: "cIMPACT-NOW update 7: advancing the molecular classification of ependymal tumors - PMC"
    accessed: 2026-09-15
    # Verbatim: "the classification has dropped the distinction between classic and
    # anaplastic ependymoma"; "C11orf95-RELA fusion genes" (dash normalised);
    # "Sufficient data are currently unavailable for a WHO grade to be assigned to
    # molecularly defined ependymomas."
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12185496/
    title: "Optimizing outcomes in intracranial ependymoma: a contemporary review - PMC"
    accessed: 2026-09-15
    # Verbatim: "Preoperative lumbar puncture is preferred, as postoperative samples
    # may be contaminated by surgical debris"; "Adjuvant focal radiotherapy (RT) is
    # the standard of care for children aged 1-21 years ... except in two key
    # scenarios: children aged 1-3 years, where chemotherapy is administered within
    # clinical protocols to defer RT" (dashes normalised); "In children under 1 year,
    # radiotherapy is generally deferred due to neurodevelopmental risks";
    # "Craniospinal RT is reserved for disseminated disease"; "The benefit of
    # adjuvant focal RT after gross total resection of grade 2 ependymomas remains
    # uncertain"; supratentorial grade 2 after GTR "may have favorable outcomes with
    # observation alone"; YAP1 tumors "predominantly observed in infants";
    # subependymomas "typically require no treatment unless symptomatic or
    # enlarging" and have a "more indolent course relative to other ependymomas"
    # (the comparative behind "Grade 1 is the slowest-growing kind", added for a
    # grade 1 reader in /review round 3); "Ataxia may emerge or worsen postoperatively"; "late progression
    # can occur, supporting surveillance beyond this window"; "Surveillance should
    # continue for 7-10 years post-treatment" (dash normalised); "Reirradiation of
    # recurrent ependymomas may improve outcomes". Its "usually curative" is about
    # SUBEPENDYMOMA only (checked in context) and is not carried for ependymoma. Its
    # "dismal long-term outlook following recurrence" is not carried outside the
    # gate. Survival figures are published here; none carried.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC9249684/
    title: "Ependymoma: Evaluation and Management Updates - PMC"
    accessed: 2026-09-15
    # Verbatim: "PFA ependymomas occur mainly in infants and children, while PFB
    # ependymomas arise mainly in adolescents and adults"; "Proton therapy is
    # increasingly employed especially in children"; "reirradiation may be
    # beneficial in both adults and children at time of recurrence"; "It is still not
    # clear how often and for how long surveillance is needed"; "long-term follow-up
    # is standard".
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC5909649/
    title: "EANO guidelines for the diagnosis and treatment of ependymal tumors - PMC"
    accessed: 2026-09-15
    # PRE-CNS5 (WHO 2016 grades, "PF-EPN-B"): cited ONLY for staging timing, the
    # second operation and follow-up, never for a name or a grade. Verbatim: CSF
    # cytology "following surgery (not earlier than 2-3 wk)" (dash normalised); "A
    # second-look surgery should be considered when residual tumor is demonstrated on
    # postoperative MRI and gross total resection is a realistic goal"; "patients
    # should be followed long term"; "In case of relapse, consideration should be
    # given to re-operation, re-irradiation, and chemotherapy"; spinal: "radiotherapy
    # is reserved for incompletely resected tumors". Also the only cited source for the
    # retired research label "ST-EPN-RELA" (it also writes "PF-EPN-A"/"PF-EPN-B"),
    # cited there as a label a reader's old report may carry, never as a name in use.
  - url: https://pubmed.ncbi.nlm.nih.gov/41423745/
    # ACNS0831 (Children's Oncology Group). PubMed served a bot wall to the fetch
    # script and the OUP full text is 403, so the abstract was read through the
    # Europe PMC REST record; the title is that record's article title. Verbatim:
    # "included patients 1-21 years"; "Patients with complete/near-total resections
    # (GTR/NTR) or complete response (CR) to induction therapy were randomized";
    # "Primary analysis showed no benefit for maintenance chemotherapy."; "Further
    # follow-up is important to assess its effect on late relapses." Hence "children
    # and young adults", "all or nearly all gone before radiation" and "at least so
    # far" (/review round 1). Its size is not carried. Survival figures are in the
    # abstract; none carried.
    title: "Phase 3 randomized trial of postirradiation chemotherapy in patients with newly diagnosed ependymoma: A report from the Children's Oncology Group."
    accessed: 2026-09-15
  - url: https://www.ncbi.nlm.nih.gov/books/NBK538244/
    title: "Ependymoma - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-15
    # Symptoms, staging and recurrence site only. It says "neurofibromatosis type 2",
    # which is not followed. Verbatim: "In infants, hydrocephalus may present with
    # macrocephaly, a tense or bulging anterior fontanelle"; "Supratentorial tumors
    # often present with seizures, focal neurological deficits"; myxopapillary tumors
    # may present with "saddle anesthesia"; "an MRI of the brain and entire spinal
    # axis is indicated"; spinal: "postoperative radiation therapy is usually
    # administered if resection is incomplete"; "Posterior fossa ependymomas tend to
    # recur locally, whereas supratentorial ependymomas tend to be disseminated at
    # relapse"; posterior fossa syndrome is "A well-known complication of posterior
    # fossa surgery"; and the research subgroup labels a methylation report can carry,
    # "PF-EPN-A", "PF-EPN-B", "PF-EPN-SE" and "ST-EPN-SE" (/review round 2: a reader
    # holding one of those could not find it in the crosswalk). Its "trouble
    # swallowing" appears only as a POST-OPERATIVE problem, never as a presenting
    # symptom, which is why the symptom list does not carry it (round 2).
  - url: https://www.cancer.gov/rare-brain-spine-tumor/tumors/ependymoma
    title: "Ependymoma: Diagnosis and Treatment - NCI"
    accessed: 2026-09-15
    # NCI-CONNECT, NCI's rare-tumor program, not the patient PDQ. It writes "grade I,
    # II, or III" beside the Arabic grades and is NEVER cited for a name or a grade.
    # Cited for where it grows by age ("posterior fossa ... more common among
    # children. Those in the spine are more common among adults"), spinal symptoms
    # ("Back pain"; "Numbness and weakness in their arms, legs, or trunk"; "Sexual,
    # urinary, or bowel problems") and spread "through cerebrospinal fluid (CSF)".
    # NOT for a grade: /review round 2 caught this comment citing it for "Grade 3
    # ependymomas are malignant (cancerous)" two lines after saying it never
    # governs a grade. Round 3 corrected the replacement justification, which was
    # also wrong: NEITHER ACS page states it alone. The ACS children's page carries
    # the general grade framing ("Higher grade (grade 3 or 4) tumors ... These tumors
    # are malignant") and the ACS adults' page carries grade 3 FOR EPENDYMOMA
    # ("low-grade (grade 2) tumors to higher-grade (grade 3) tumors"). The page's
    # sentence is the two together.
    # Survival figures are published there; none carried.
  - url: https://www.cancer.gov/rare-brain-spine-tumor/living/related-organizations
    title: "Rare Brain and Spine Tumor Support Organizations - NCI"
    accessed: 2026-09-15
    # The CERN Foundation "is a valuable resource for those affected by ependymoma".
  - url: https://together.stjude.org/en-us/conditions/cancers/ependymoma.html
    title: "Ependymoma in Children and Teens - Together by St. Jude™"
    accessed: 2026-09-15
    # Fetched at /about-pediatric-cancer/types/brain-spinal-tumors/ependymoma.html,
    # which redirects here. Verbatim: "Ependymomas grow from cells that line the
    # fluid-filled ventricles of the brain and central canal of the spinal cord."
    # (a channel down the MIDDLE of the cord, not the fluid around it, which the first
    # draft said); "Ependymomas are most common in children under age 5. But these
    # tumors may happen at any age."; "Back or neck pain"; "Many symptoms of
    # ependymoma are due to hydrocephalus."; "Surgery is the main treatment for
    # ependymoma"; "Radiation therapy is used in children older than age 1."; "In
    # young children, chemotherapy may be used to help delay radiation"; "Ependymoma
    # is hard to cure without successful surgery to remove the tumor."; "late
    # recurrences or relapse (beyond 5 years) may occur"; genetic changes "are not
    # usually passed down from parents"; "The risk for posterior fossa syndrome is
    # lower when an experienced neurosurgeon performs the surgery."
    # /review round 4 added the two quotes this page carried without recording them:
    # "Irritability or confusion" (behind a baby being "more irritable than usual") and
    # the posterior fossa change list "Talking", "Swallowing", "Walking and other types
    # of movement", "Emotions", "Behavior" (behind an at-home tier naming mood AND
    # behavior). "Feeling dizzy" is NCI-CONNECT's and CRUK's, not St. Jude's. Survival
    # figures are published there; none carried.
  - url: https://together.stjude.org/en-us/treatment-tests-procedures/symptoms-side-effects/posterior-fossa-syndrome.html
    title: "Posterior Fossa Syndrome - Together by St. Jude™"
    accessed: 2026-09-15
    # Verbatim: "Symptoms of posterior fossa syndrome usually begin within 1-2 days
    # after surgery. But symptoms always start within the first week after surgery."
    # (dash normalised); "Some children lose the ability to speak"; "Most children’s
    # speech returns over the next few days or weeks. But some continue to have
    # limited speech for months."; "Most children will regain the ability to walk on
    # their own. But some difficulties with speech, motor skills, and/or mood may last
    # longer, even years." (dashes normalised); "Therapies such as speech therapy can
    # help to improve speech and swallowing difficulties." No frequency figure is
    # carried: the sources' figures range widely and none is specific to ependymoma.
    # /review round 1: this page does NOT say it is "not the tumor coming back"; the
    # page says what StatPearls says, a complication of the operation.
  - url: https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/ependymoma
    title: "Ependymoma | Brain and spinal cord tumours | Cancer Research UK"
    accessed: 2026-09-15
    # UK. Verbatim: "Most are grade 2 or grade 3 tumours."; "But for ependymomas,
    # the grade does not always fit with their behaviour."; "In adults, most
    # ependymomas start in the ependymal cells that line the spinal cord."; "neck
    # pain"; "You might have an MRI scan every 3 to 6 months, for two years." Its
    # "feeling or being sick" is not carried.
  - url: https://braintumor.org/brain-tumors/about-brain-tumors/brain-tumor-types/ependymoma/
    title: "Ependymoma"
    accessed: 2026-09-15
    # National Brain Tumor Society. Cited ONLY for the higher-in-the-brain symptoms:
    # "one-sided weakness, difficulty with speech or language, and vision changes.
    # Seizures may occur." (/review round 1: no other cited source had speech or
    # vision for that location.) Survival figures are published there; none carried.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/treating/specific-types-of-tumors.html
    title: "Treating Brain Tumors in Children | American Cancer Society"
    accessed: 2026-09-15
    # Verbatim, and the curability strength this page uses: "They can sometimes be
    # cured by surgery if the entire tumor can be removed, but this is not always
    # possible."; "a second operation may be done in some cases, often after a short
    # course of chemotherapy"; "Radiation therapy is recommended after surgery for
    # most children and teens to try to prevent the tumor from coming back, even if
    # it appears that all of the tumor has been removed."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/types-of-brain-and-spinal-tumors.html
    title: "Types of Brain Tumors in Children | American Cancer Society"
    accessed: 2026-09-15
    # Uses "grade III ... anaplastic" for ependymoma, which is NOT followed. Cited
    # for "Unlike other gliomas, ependymomas usually do not grow into normal brain
    # tissue", for the general grade framing ("Lower grade (grade 1 or 2) tumors tend
    # to grow more slowly"; "Higher grade (grade 3 or 4) tumors ... are malignant"),
    # and, inside the gate, "They may be harder to treat if the tumor cannot be
    # completely removed with surgery, the child is younger when the tumor is found,
    # or certain gene changes are present in the tumor."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/about/types-of-brain-tumors.html
    title: "Types of Brain Tumors and Spinal Cord Tumors in Adults | American Cancer Society"
    accessed: 2026-09-15
    # Verbatim: "higher-grade (grade 3) tumors, which used to be called anaplastic
    # ependymomas"; "Some ependymomas can be removed completely and cured by surgery,
    # but this isn’t always possible because of the way they can spread."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/detection-diagnosis-staging/how-diagnosed.html
    title: "Tests for Brain and Spinal Cord Tumors in Children | American Cancer Society"
    accessed: 2026-09-15
    # The spinal tap: "the lower part of the back over the spine"; "medicine to make
    # them sleep so the lumbar puncture can be done more easily and safely"; "A
    # small, hollow needle"; "The fluid is sent to the lab to look for cancer cells."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/treating/surgery.html
    title: "Surgery for Brain Tumors in Children | American Cancer Society"
    accessed: 2026-09-15
    # "Shunts can be temporary or permanent."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/treating/radiation-therapy.html
    title: "Radiation Therapy for Brain Tumors in Children | American Cancer Society"
    accessed: 2026-09-15
    # General children's page, not ependymoma-specific: "Children younger than 3 years
    # are usually not given radiation because of long-term side effects on brain
    # development" (printed as a disagreement, attributed to "the American Cancer
    # Society's general page", /review round 1); "Problems can include memory loss,
    # personality changes, and trouble learning at school."; "Proton beam radiation
    # may not be available everywhere."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-children/after-brain-tumor-treatment.html
    title: "After Brain Tumor Treatment in Children | American Cancer Society"
    accessed: 2026-09-15
    # "Doctors will monitor growth and development, with special attention to areas
    # that were treated with radiation."
  - url: https://www.ncbi.nlm.nih.gov/books/NBK1201/
    title: "NF2-Related Schwannomatosis - GeneReviews® - NCBI Bookshelf"
    accessed: 2026-09-15
    # The 2022 name "was proposed by Plotkin et al [2022]"; "Intramedullary tumors of
    # the spinal cord, such as astrocytoma and ependymoma".
  - url: https://www.cern-foundation.org/education/recurrence
    title: "Recurrence | CERN Foundation"
    accessed: 2026-09-15
    # The patient-facing counterpoint on radiation a second time: "Patients who have
    # previously received radiation therapy may not be able to recieve addtional
    # radiation." (source typos). Also "Seeking a second opinion at recurrence might
    # give you further insights into treatment options and possible clinical
    # trials."
  - url: https://www.cern-foundation.org/
    title: "Ependymoma Cancer Research Network | CERN Foundation"
    accessed: 2026-09-15
    # "Connecting you with other ependymoma patients and caregivers."
  - url: https://www.cern-foundation.org/education/ependymoma-guide
    title: "Ependymoma Guide | CERN Foundation"
    accessed: 2026-09-15
    # "The focus audience is newly diagnosed patients, care partners, and advocates."
# WI-570: TWO PLACES ON THIS PAGE RANKED ONE ADDRESS AGAINST ANOTHER, and the
# second one was found by /review rather than by the sweep, because the sweep
# could only see half of it.
#
# THE SECOND, in "If it comes back, or changes": "After a tumor at the back of
# the brain, it tends to come back in the same place. After one higher up in the
# brain, it more often turns up somewhere else in the brain or spine." Two
# addresses, two answers, and the second is materially worse news than the
# first. The claim is REAL and is in this page's own sources -- StatPearls has
# "Posterior fossa ependymomas tend to recur locally, whereas supratentorial
# ependymomas tend to be disseminated at relapse" -- which is exactly why it is
# recorded here rather than quietly dropped: a true sentence can still be the
# forbidden artifact. §12.18's line is about what an address entry may CARRY,
# not about whether the claim is sourced, and Wave 6 bans the location-keyed
# lookup whatever its evidence.
# WHAT REPLACED IT KEEPS THE INFORMATION AND DROPS THE LOOKUP: "It can come
# back in the place it started, or somewhere else in the brain or spine. Both of
# those happen." Every reader of this page gets the same answer, which is the
# §12.19 row test passing rather than being argued around.
# AND THE HALF THE GUARD COULD NOT SEE IS THE LESSON. The first sentence
# carried "come back" plus an address and fired; the second carried no
# difficulty word at all and was invisible. A property guard standing over one
# sentence of a pair is not standing over the pair.
#
# WI-570: ONE SENTENCE DELETED FROM THE OUTLOOK GATE, and it is the only
# defect the sweep found on its own. (It ran over all twenty-three tumor hubs;
# nine was the backlog's page scope, not the sweep's.) It read "Tumors in the spinal cord in
# adults tend to do better than tumors at the back of the brain in young
# children", and it is the shape content-pipeline §12.18 bans: two addresses
# ranked against each other, so a reader looks their own up and reads their
# outlook off it. The Wave 6 preamble forbids it twice over, because it is
# also a PROGNOSIS claim keyed to a place. It was not even a clean location
# claim -- it carried AGE as a second variable inside the same comparison,
# so neither half could be checked against the other.
# NOTHING WAS LOST BY IT. The sentence two lines above already keys the same
# material to the FACTORS this tumor's own sources name -- how much came out,
# how young the child is, the gene result -- which is the form §12.18 asks
# for, and it is sourced. No source was dropped; no figure was carried.
# WHAT WAS KEPT, and why -- four because the sweep flagged them, one because it
# cannot see it at all -- because a silent
# allowance reads like an oversight (§12.19):
# - "It was moved because it comes back about as often as other ependymomas
#   in the spine" is the history of a WHO regrade, and it REFUSES to rank --
#   it says this kind behaves like its neighbours at the same address.
# - "It can come back in the place it started, or somewhere else in the brain
#   or spine" is what replaced the deleted pair. It names the whole territory
#   instead of handing two addresses two answers, so every reader of this page
#   gets the same one. It reaches the scan only because "spine" is in the
#   territory it names.
# - "Some ependymomas can be cured that way, when all of it can be taken
#   out" is keyed to EXTENT, not to a place. The sweep reaches it only
#   because the short version names two addresses three sentences earlier.
# - "It can also come back, even years later, so the scans go on for a long
#   time" is true of every ependymoma on this page at any address -- it is the
#   argument for long follow-up. It is listed here because /review found it
#   being silenced by the reason written for the sentence above it, back when
#   an allowance was matched against the four-sentence window rather than
#   against the sentence that triggered the scan.
# - "That is not always possible, because of where it sits. When some has to
#   be left behind, it is much harder to cure." The FIRST sentence carries
#   neither an address nor a difficulty word; the difficulty is in the SECOND,
#   which carries no place. Split that way the pair is invisible to the scan
#   from both ends, and it is kept: it is keyed to EXTENT, which is this
#   tumor's factor, and it names no address for a reader to look up. This is
#   the same "read the sentences either side" lesson that the deleted
#   recurrence pair taught, landing on a pair that is CORRECT.
reviewed: 2026-09-15
review_due: 2027-03-15
disclaimers: [medical]
---

## The short version

**An ependymoma is a tumor that starts in the lining of the fluid spaces in the
brain and spine.** In children it is most often low at the back of the brain. In
adults it is more often in the spinal cord. **Surgery is the main treatment, and
how much of it comes out matters a great deal.** Some ependymomas can be cured
that way, when all of it can be taken out. It can also come back, even years
later, so the scans go on for a long time.

## What is an ependymoma?

Your brain has spaces inside it that are filled with fluid, and a thin channel
of the same fluid runs down the middle of your spinal cord. Those spaces are
lined with cells called ependymal cells. **An ependymoma starts in that lining.**

!%glioma%!%supratentorial%It belongs to the [glioma](/tumors/glioma) family, in a group of its own. Many
gliomas grow out into the brain around them. **An ependymoma usually does not.**
That difference is a big part of why surgery matters so much here.

It is most common in children under 5. It can happen at any age, and adults get
it too.

### One name, several kinds

Doctors now name an ependymoma by **where it is** and by **gene results** from
the tumor. Two that look alike under a microscope can behave very differently,
so both parts of the name matter. The section on your report lists the names.

## Is it cancer? What does its grade mean?

**Grade 3 is malignant, which means cancer. Grade 2 is called low grade, which is
about how fast it grows, not about how serious it is.** A grade 2 ependymoma is
still treated with surgery, and it is still followed with scans for years,
because it can come back.

The grade is only part of the picture.

Most ependymomas are **grade 2** or **grade 3**. Grade 2 tends to grow more
slowly, and grade 3 tends to grow faster.

**For this tumor, the grade does not always match how it behaves.** That is why
your team also looks at where it is and at its gene result.

Two kinds have a grade set by their name. **Myxopapillary ependymoma**, low in the
spine, is grade 2. **Subependymoma** is grade 1. It is often found by chance in
older adults, and it often needs no treatment unless it is causing symptoms or
growing.

**Grade 1 is the slowest-growing kind.** If your report says grade 1, or carries
a name with no grade beside it, that is the whole answer, not a gap in it. Ask
your team what your own report means.

### Can it be cured?

**Some ependymomas can be cured by surgery, when all of it can be taken out.**
That is not always possible, because of where it sits. When some has to be left
behind, it is much harder to cure.

That is a hard sentence, and it is here on purpose. It is the reason your team
puts so much into the first operation, and sometimes offers a second one.

**It can also come back after treatment that went well**, sometimes many years
later. That is why the scans go on for so long. It does not mean anybody got
anything wrong.

You may have read that a glioma of grade 2 or above cannot be cured. That is
about the gliomas that grow out into the brain around them. An ependymoma
usually does not grow that way, so that sentence was not written about this
tumor.

## Where does it grow, and why does it cause these symptoms?

It grows where the fluid spaces are, and **where it sits decides what you
notice**.

- **Low at the back of the brain.** Doctors call this part the
  !%posterior fossa%**posterior fossa**. It holds the cerebellum, which helps with
  balance, and the brain stem. Most ependymomas in children are here.
- **Higher up in the brain.** Here it is more likely to cause seizures, or
  weakness on one side of the body. Your report may call this
  **supratentorial**.
- **In the spinal cord.** Most ependymomas in adults are here. The kind called
  myxopapillary grows at the very bottom end of the cord.

**Because it grows in the fluid spaces, it can block the fluid.** Many of the
symptoms come from that. The section below explains what it does.

[MECHANISM]

## What symptoms does it cause?

It depends on where the tumor is.

Low at the back of the brain, most symptoms come from the blocked fluid:

- Headaches, often worst on waking up.
- Throwing up, often in the morning.
- Trouble with balance or walking.
- Feeling dizzy, or eyes that flick from side to side.

New or worse trouble with walking or balance is a same-day call, or a right-away
call if there is a shunt, or if the tumor is in the spinal cord or has spread to
the spine.

In a baby, the head can grow bigger than it should, the soft spot on top of the
head can bulge, and the baby can be more irritable than usual. If you see that in
a baby with this tumor, call your team the same day, or right away if there is a
shunt.

Higher up in the brain: seizures, weakness on one side, trouble with speech, or
changes in vision.

In the spinal cord: back or neck pain, numbness or weakness in the arms or legs,
and trouble with the bladder or bowel. Low in the spine, it can cause numbness
in the area you sit on. For a tumor in the spinal cord, or one that has spread to
the spine, those signs have their own rule, at the end of the section on when to
call for help, below.

[ESCALATION]

[SPINAL-CORD]

## How do doctors find out it is this?

- **[An MRI scan](/tests/mri)** of the brain **and the whole spine**. The spine
  is checked because this tumor can spread through the fluid. Checking does not
  mean it has.
- **The operation.** Surgery to take the tumor out is usually where the piece of
  tumor comes from. It is tested for gene changes, because the gene result is part
  of the name.
- **A spinal tap.** A small needle goes into the lower back to take out a little
  of the fluid. The lab looks for tumor cells in it. A young child may be given
  medicine to sleep through it.

**When the spinal tap happens varies.** Some teams do it before the operation if
they can. After surgery, fluid taken too soon can be muddied by the operation, so
the sources say to wait. They give different waits, from about 10 days to about 3
weeks. Ask your team when yours will be.

## What do the words on my report mean?

[CROSSWALK]

The name on your report usually says where the tumor is, and then a gene result
if one was found.

- **Posterior fossa ependymoma, group PFA** or **group PFB.** Low at the back of
  the brain. Group PFA is found mostly in babies and young children. Group PFB is
  found mostly in teenagers and adults.
- **Supratentorial ependymoma, ZFTA fusion-positive.** Higher up in the brain.
  Fusion-positive means a gene called ZFTA has joined onto another gene.
- **Supratentorial ependymoma, YAP1 fusion-positive.** Higher up in the brain,
  with a different gene joined. It is rare, and found mostly in babies.
- **Spinal ependymoma**, or **spinal ependymoma, MYCN-amplified**, which names a
  gene result in a tumor in the spinal cord.
- **Myxopapillary ependymoma.** Low in the spine. Grade 2.
- **Subependymoma.** Grade 1.
- **Posterior fossa ependymoma** or **supratentorial ependymoma**, with no group or
  gene after it. The name was given by place alone. That happens when the gene
  test was not done, did not work, or found something else.

A name may also have **grade 2** or **grade 3** beside it. For the names built on a
gene result, a grade is often left off. There is not yet enough evidence to set
one.

[Your pathology report](/tests/pathology-report) walks through the document
itself.

### If your older paperwork says something different

- **Anaplastic ependymoma.** An older name for a faster-growing ependymoma. It is
  no longer used. A report written now usually has grade 3 beside the name
  instead.
- **Classic ependymoma.** Reports used to split classic from anaplastic. That
  split is no longer used.
- **RELA fusion-positive**, or **C11orf95-RELA.** Older names for the ZFTA kind
  above.
- **Myxopapillary ependymoma, grade 1.** It is grade 2 now. The tumor did not
  change. It was moved because it comes back about as often as other ependymomas
  in the spine.
- **Papillary, clear cell** or **tanycytic ependymoma.** These now describe how the
  cells look. They are no longer separate kinds.
- **PF-EPN-A**, **PF-EPN-B** or **ST-EPN-RELA.** Research ways of writing the same
  names, and they turn up on gene test reports. PF-EPN-A is group PFA, PF-EPN-B is
  group PFB, and ST-EPN-RELA is the ZFTA kind. **SE** in one of these names, such as
  PF-EPN-SE, means subependymoma.

## How is it usually treated?

[TUMOR-BOARD]

**Surgery is the main treatment.** The surgeon aims to take all of it out, as long
as that can be done without harming the parts of the brain or spine around it.
[How much came out](/treatments/craniotomy#how-much-came-out) explains the words
the team will use afterwards. After surgery at the back of the brain, some
children have **posterior fossa syndrome**, which the next section explains.

**A second operation** is sometimes offered, if the scan after surgery shows
tumor left behind and taking the rest out looks possible. It may come after a
short course of chemotherapy.

**[Radiation](/treatments/radiation-therapy)** usually follows surgery for
children and teens, aimed at the place where the tumor was. The American Cancer
Society recommends it for most, to try to stop the tumor coming back, even when
all of the tumor seems to be out. If tumor cells are found in the fluid, radiation
may cover the whole brain and spine.

- **Babies and young children.** Radiation can harm a growing brain, so sources
  draw an age line, and they draw it in different places. St. Jude and a 2025
  review say radiation is used from about age 1. That review adds that some
  children aged 1 to 3 have chemotherapy first, to put radiation off. The American
  Cancer Society's general page says radiation is usually not given before age 3.
  Ask your team where they draw the line, and why.
- **Not every tumor gets radiation.** Doctors are not sure it helps after a grade
  2 tumor is fully taken out. A child whose grade 2 tumor higher in the brain was
  all removed may be watched with scans instead. For a tumor in the spinal cord,
  radiation is usually given only if some was left behind.
- **[Proton therapy](/treatments/proton-therapy)** is used more and more for
  children, to spare the healthy brain around the tumor. It is not available
  everywhere.

**[Chemotherapy](/treatments/chemotherapy) has a smaller part.** A trial in
children and young adults, whose tumor was all or nearly all gone before
radiation, found that adding chemotherapy after radiation did not help, at least
so far. It is still used to put off radiation in young children, or to shrink a
tumor before a second operation. If it is offered, it is fair to ask what it is
for.

**If the fluid is blocked**, [a shunt](/treatments/shunts) or another small
operation can let it drain. Some shunts are only needed for a while.

**Ask about [clinical trials](/treatments/clinical-trials)** for your kind of
ependymoma, and what testing they need.

## What is treatment actually like, and what is normal afterwards?

The library pages carry the detail: [what an operation is
like](/treatments/craniotomy), [what radiation is
like](/treatments/radiation-therapy) and [proton
therapy](/treatments/proton-therapy).

### After surgery at the back of the brain {#posterior-fossa-syndrome}

Some children get this after surgery for an ependymoma at the back of the brain.

[POSTERIOR-FOSSA-SYNDROME]

**Before surgery, it is fair to ask** how often your surgeon operates in this part
of the brain. St. Jude says the risk of it is lower with an experienced surgeon.

### After surgery on the spine

Pain, numbness and tiredness can go on for a while after spinal surgery. Before
you go home, ask your team which of those to expect, and which mean calling them.
**New or worse pain in the back or neck, and weakness or numbness that is new or
worse, is a right-away call**, under the spinal cord rule above.

## Everyday life: school, work, seizures and tiredness

- [Living with seizures](/seizures/living-with) covers the practical side, and
  [what to do during a seizure](/seizures/what-to-do) is worth reading before you
  need it.
- **School and learning.** Radiation to a young brain can affect memory and
  learning later on. Hearing and hormones can be affected too. Doctors check
  growth and development for years afterwards. Ask early who at the hospital
  helps with school.
- **Driving rules depend on where you live.** Ask your team directly.
- **Tiredness** can come from surgery and radiation, and it can also come from
  the tumor or from fluid that is not draining. Sleeping much more than being
  awake is a same-day call, or a right-away call if there is a shunt.

Ask early who at the hospital helps with work and money.

## Follow-up scans, and what to do while you wait

**The scans go on for years after this tumor.** It can come back without causing
any symptoms, and sometimes long after treatment ended. One recent review
suggests scans for about 7 to 10 years. Another says nobody is yet sure how often
or for how long. Scans are often every few months at first, and further apart
later. Your team sets yours.

Keep going to the scans even after years of clear ones.

[Follow-up scans](/tests/follow-up-scans) goes through what each scan is compared
with, and [waiting for results](/tests/waiting-for-results) covers the gap before
the answer.

## If it comes back, or changes

It can come back in the place it started, or somewhere else in the brain or
spine. Both of those happen.

There are still choices. Your team may talk about **surgery again**, radiation
again, chemotherapy, or a clinical trial.

**Radiation a second time is something sources describe differently.** Recent
reviews say it can be an option for adults and for children. The CERN Foundation
says someone who has had radiation before may not be able to have more. Ask your
team whether it is an option for you, and what it would be for.

**Some people get a second opinion at this point.** The CERN Foundation says it
can bring up other treatments and trials.

**No choice here is the wrong one.** Ask what each option is for, and write the
answers down.

## Did I cause this?

[CAUSES]

**Is it passed down in families?** Usually not. Some ependymomas in the spinal
cord happen in people with an inherited condition called **NF2-related
schwannomatosis**. Older letters call it neurofibromatosis type 2, and the new
name was suggested in 2022. If a relative has that condition, it is worth asking
your team whether anyone else should be checked.

## What might happen over time

:::outlook
Doctors sometimes talk about outlook using numbers. It helps to know what those
numbers are before you choose whether to look at them.

The numbers describe people who were treated years ago. Before 2021, these tumors
were sorted mostly by how the cells looked, not by where they were or by gene
results. So an older figure may mix kinds that behave very differently.

The word you will run into is **median**. A median is the middle of a group. If
you lined everybody up, the median is the person standing in the middle. It is
not a prediction about any one person.

With this tumor the kind matters a lot. The outlook tends to be harder when the
tumor could not all be taken out, when the child is younger, or with certain
gene results. It is also usually harder once it has come back after surgery and
radiation. And a median cannot show you the people who do far better than the
middle.

Your own team knows your situation, and a page written for everybody does not.
They can talk it through with you whenever you want to.
:::

## For the person caring for someone with this

[CAREGIVER]

Three things specific to this diagnosis.

**Many people caring for someone with an ependymoma are parents of a young
child.** Everything above still applies, and it is fine to ask the team to
explain things more than once.

**After surgery at the back of the brain, watch talking and swallowing.** If your
child comes home still having trouble swallowing, ask to be shown how to give
food and drink before you leave. New or worse trouble swallowing after you are
home is a same-day call, or a right-away call if there is a shunt. Choking, or
trouble breathing, is an ambulance call.

**Keep the scan dates somewhere safe.** The scans go on for years, and you may be
the one keeping track of them.

## What to ask your team

- Where exactly is the tumor, and what is its full name, with any gene result?
- Was all of it taken out? What did the scan after surgery show?
- Would a second operation help?
- Has the whole spine been scanned, and has the fluid been checked?
- Is radiation recommended, and why? What would happen if we waited?
- Could proton therapy be an option?
- If chemotherapy is offered, what is it for?
- Before surgery at the back of the brain: how often do you do this operation?
- How long will the scans go on, and how often?
- Is there a clinical trial for this kind?
- What would make you want to see us before the next appointment?
- Who do we call after hours, and what number is it?

## Where to get support

- [Getting help now](/get-help-now) if you need somebody today.
- [The CERN Foundation](https://www.cern-foundation.org/), a program of the
  National Brain Tumor Society, is set up for people affected by ependymoma. It
  has a guide for people who are newly diagnosed, and it connects people with
  others who have been through it.
- [Together by St. Jude](https://together.stjude.org/en-us/conditions/cancers/ependymoma.html)
  has pages for families of children and teens, including one on posterior fossa
  syndrome.
- [What to do during a seizure](/seizures/what-to-do), worth reading before you
  need it.
- [Living with seizures](/seizures/living-with) for the practical side.
- Your team's specialist nurse, who is usually the fastest way to get a question
  answered.
