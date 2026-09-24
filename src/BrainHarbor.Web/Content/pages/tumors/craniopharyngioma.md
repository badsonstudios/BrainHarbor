---
title: "Craniopharyngioma"
description: "A growth that sits next to the pituitary gland, the nerves for sight, and the space the brain's fluid drains through. Why sight, hormones and fluid are the three things a team watches, what the two type names on your report mean, and the warning signs this page carries rules for."
tags: [tumor-type]
sources:
  # WI-540. A TUMOR HUB, so §12.3's seventeen sections and §12.9's proved
  # template govern. The 43-line stub this replaces cited ONE source and it was
  # barred: NCI's /types/brain. §12.1 is explicit that a page citing NCI for a
  # tumor name is a defect, and naming is a large part of what this page does.
  # NCI is not cited here at all. Neither is Cancer Research UK nor the UK
  # Pituitary Foundation, both barred for idiom (WI-538): their prose is British
  # in exactly the sentences a US reader-facing page wants.
  #
  # AND THE READER STILL MEETS BOTH OF THEM IN THE COMPOSED SOURCE LIST, which
  # is a property of the page a front-matter check cannot see and which the
  # rendered read is what found. Block sources merge into every including page
  # and RENDER (§12.10), so the escalation, causes and caregiver blocks put NCI
  # in front of this reader FOUR times and Cancer Research UK TWICE, one of them
  # under a visibly British title. §12.1 still holds -- not one of them is a
  # naming or grading claim, and this page cites neither itself -- but the
  # honest form of the statement is the one above plus this one. WI-539
  # anticipated exactly this leak class for one source and not the other, and
  # the next reader re-opened the question; recorded here so nobody has to.
  #
  # NO BLOCK DIRECTIVE IS WRITTEN IN BRACKETS ANYWHERE IN THIS FRONT MATTER, and
  # that is deliberate. CaregiverSectionTests locates the caregiver block with a
  # raw IndexOf over the WHOLE file, so a bracketed name in a comment becomes the
  # FIRST match and the page is reported as not carrying the block under its
  # heading. WI-539's first draft did exactly that. Block names below are written
  # without brackets for that reason.
  #
  # THE SOURCE PACK FOR THIS ITEM IS AN EXTRACT SET, NOT SAVED PAGES, AND THAT
  # CHANGED HOW IT WAS USED. The fetcher refused whole-page dumps, so each saved
  # file is a model-produced extract, and the item's own notes file quotes those
  # extracts -- checking one against the other is circular. That is the footing
  # §12.8 gives a DOSSIER, not a source. So every claim below was verified against
  # the LIVE publication before it was written, and the log is in
  # .claude/work_files/wi540/source-verification.md. The verification found NO
  # fabrication and FOUR truncations, each clipping the start or end of a
  # sentence -- §12.13's defect, quoting to the end of the convenient sentence
  # rather than the end of the qualification.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK519027/
    title: "Craniopharyngioma - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-18
    # THE BACKBONE. Verified live. Location and structure ("typically arise in
    # the suprasellar region and often extend to involve critical structures such
    # as the hypothalamus, optic chiasm, cranial nerves, third ventricle, and
    # major blood vessels"); the two types and their ages; the genes; the sight
    # pattern; the hormone list; the surgery routes; the stalk dilemma ("the
    # pituitary stalk should be preserved when possible"); and the fluid rule,
    # "When the third ventricle is affected, hydrocephalus can develop" plus
    # "External ventricular drainage may be needed in cases presenting acutely
    # due to hydrocephalus".
    #
    # ALSO THE ONE SENTENCE THAT TIES THE TWO EMERGENCIES TOGETHER FROM A SOURCE
    # RATHER THAN BY INFERENCE: thyroid hormone replacement "can increase the
    # metabolic clearance of glucocorticoids and thus may cause an adrenal
    # crisis", which is why the page tells a reader on replacement to say so
    # before anything is changed.
    #
    # ITS GRADE IS PRINTED IN A ROMAN NUMERAL ("World Health Organization grade I
    # tumors"). This page prints Arabic, and the reason is recorded under the
    # CNS5 entry below.
    #
    # NONE OF ITS NUMBERS IS CARRIED (§12.4 R1, §12.5): not the visual-symptom
    # range, not the hormone-deficiency shares, not the recurrence rate, not the
    # ten-year survival, and not one milligram of the replacement doses.
    #
    # IT HAS NO ACUTE OR EMERGENCY SECTION, and that absence is load-bearing: see
    # the apoplexy ruling under PMC8963871.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK538819/
    title: "Craniopharyngiomas - Endotext - NCBI Bookshelf"
    accessed: 2026-09-18
    # Verified live. The hypothalamic syndrome in one sentence -- hyperphagia and
    # uncontrollable obesity, disorders of thirst and water balance, behavioral
    # and cognitive impairment, loss of temperature control and disordered sleep.
    # Also cyst rupture causing chemical meningitis; the cyst-enlargement risk
    # after radiotherapy; the extent-of-resection debate and the modern shift
    # toward sparing the hypothalamus; and the recurrence horizon.
    #
    # THE SHORT HALF OF THE RECURRENCE HORIZON IS SOURCED TOO, and recording it
    # is the point: "The mean interval for diagnosis of recurrence following
    # various primary treatment modalities ranges between 1 and 4.3 years."
    # /review round 1 read the page's "it usually happens in the first few
    # years" as an unsourced short-horizon claim, because this note carried the
    # 30-year sentence and not this one. The claim was fine; the CLAIM-TO-SOURCE
    # MAP was incomplete, which is §12.13's defect rather than §12.4's -- a
    # reader of the front matter could not check it without re-deriving it.
    # No figure from either sentence reaches the page.
    #
    # THE RECURRENCE SENTENCE THIS PAGE FOLLOWS: "Remote recurrences as late as 30
    # years after initial therapy have been reported." Two children's-hospital
    # sources say instead that recurrences happen up to two years after surgery.
    # One of them contradicts itself inside its own page, prescribing follow-up
    # yearly to ten years and then indefinitely. The long horizon is what this
    # page carries, in words and with no figure.
    #
    # AND IT NEVER MENTIONS BLEEDING INTO THE TUMOR ANYWHERE. Verified live as an
    # absence, deliberately, because that absence is half the apoplexy ruling.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8328013/
    title: "The 2021 WHO Classification of Tumors of the Central Nervous System: a summary"
    accessed: 2026-09-18
    # THE NAMING AUTHORITY (§12.1), verified live, and the source of this page's
    # crosswalk slice: in past editions the adamantinomatous and papillary forms
    # were considered subtypes of one tumor, and they are now classified as
    # distinct tumor types. Also the convention this page follows: CNS5 changed
    # all CNS tumor grades to Arabic numerals, and grades are given within a type
    # rather than across types.
    #
    # WHAT IT DOES NOT SAY, AND THE DISTINCTION MATTERS. CNS5 states NO GRADE for
    # this tumor. So the grade of 1 rests on the clinical sources, while the
    # ARABIC NUMERAL rests on this document's general convention change. Citing
    # CNS5 for "grade 1" as though it graded this tumor would be a citation that
    # does not support the claim in the form it is made (§12.14). Both halves are
    # attributed on the page.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC10033642/
    title: "Neoplasms and tumor-like lesions of the sellar region: imaging findings with correlation to pathology and 2021 WHO classification"
    accessed: 2026-09-18
    # Verified live, and the ONLY source in the pack that prints the grade in an
    # ARABIC numeral, which is why it is cited beside CNS5: a benign slow growing
    # (WHO grade 1) sellar/suprasellar neoplasm. It also corroborates the rename
    # independently, calling the two forms distinct entities formerly considered
    # only subtypes, and names where the tumor starts, along the remnants of
    # Rathke's pouch by the stalk.
  - url: https://together.stjude.org/en-us/conditions/cancers/craniopharyngioma.html
    title: "Craniopharyngioma in Children and Teens - Together by St. Jude"
    accessed: 2026-09-18
    # Verified live, and the plainest sentence in the whole pack, which is why
    # the fluid material rests on it: as it grows, the tumor may block the flow
    # of cerebrospinal fluid, and that causes a fluid build-up in the brain known
    # as hydrocephalus. Also the symptom list a family actually reads, the cyst
    # catheter, hypothalamic obesity named in plain words, and that children with
    # this tumor need ongoing care.
    #
    # IT GIVES NO EMERGENCY GUIDANCE AT ALL, verified live. Nor does any other
    # source in the pack give a what-to-do-tonight list for THIS tumor. The
    # page's own tiers therefore combine general rules with this tumor's
    # mechanism, which is an editorial act and is recorded here as one.
  - url: https://www.aans.org/patients/conditions-treatments/craniopharyngiomas/
    title: "Craniopharyngiomas - American Association of Neurological Surgeons"
    accessed: 2026-09-18
    # Verified live. THE COUNTERWEIGHT TO "BENIGN", and the page's grade section
    # is built on it: histological benign, but locally aggressive, tumors that
    # develop near the pituitary gland at the base of the brain. Also that they
    # usually involve the pituitary stalk, and the two operation routes.
  - url: https://www.childrenshospital.org/conditions-treatments/craniopharyngioma
    title: "Craniopharyngioma - Boston Children's Hospital"
    accessed: 2026-09-18
    # Verified live. The fluid mechanism in a parent's words, the reason surgery
    # comes first, lifelong hormone replacement under an endocrinologist, and the
    # self-blame sentence the shared causes block does not carry: that there is
    # nothing a parent could have done or avoided doing that would have prevented
    # the tumor from developing.
    #
    # ITS TWO-YEAR RECURRENCE SENTENCE IS NOT FOLLOWED. See the Endotext note.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12109346/
    title: "Management of Acquired Hypothalamic Obesity After Childhood-Onset Craniopharyngioma - A Narrative Review"
    accessed: 2026-09-18
    # Verified live, and the verification returned two sentences better than the
    # ones the extract had kept. The first is why this page refuses to imply that
    # eating less fixes it: morbid obesity due to hypothalamic damage is mostly
    # non-responsive to conventional lifestyle modifications such as physical
    # exercise or dietary interventions. The second keeps the page honest in the
    # other direction: treatment for it has thus far been rather disappointing,
    # and no drug treatment is proven effective in randomized controlled trials.
    # Also the temperature, sleep and behavior effects, and that resistance to
    # leptin and insulin and a lower total energy expenditure are part of the
    # mechanism. Its shares are not carried.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8963871/
    title: "Clinical Features of Craniopharyngioma With Tumoral Hemorrhage: A Retrospective Case-Controlled Study"
    accessed: 2026-09-18
    # THE APOPLEXY RULING, AND IT IS THE OPPOSITE WEIGHTING FROM /tumors/pituitary-tumor.
    # Bleeding into THIS tumor is named for THIS tumor, so nothing is stretched
    # from a pituitary-adenoma source: this paper defines the event and says the
    # clinical picture resembles that of pituitary tumor apoplexy.
    #
    # BUT IT IS KEPT RARE AND IT IS NOT THE HEADLINE, and three verified facts are
    # why. The paper calls it a very rare syndrome, in its introduction AND its
    # conclusion. It is one retrospective series with a small number of events.
    # And the two general reference chapters above are verified SILENT on it --
    # StatPearls has no acute section at all and Endotext never mentions bleeding.
    # Its incidence figure is NOT carried (§12.4 R1).
    #
    # IT ALSO CONTAINS NO PATIENT-FACING ACTION GUIDANCE OF ANY KIND. It is a
    # surgical series. So it supports what the event IS and never what to do
    # about it, and the page takes the action from the general rules instead.
  - url: https://medlineplus.gov/ency/article/000357.htm
    title: "Acute adrenal crisis - MedlinePlus Medical Encyclopedia"
    accessed: 2026-09-18
    # Verified live, and the source of the second emergency's ROUTE: go to the
    # emergency room or call the local emergency number, such as 911, if you
    # develop symptoms of acute adrenal crisis. Also that it is life-threatening;
    # that hydrocortisone is needed right away; that many people keep an emergency
    # syringe at home that a family member can inject; and that a medical ID
    # should always be carried.
  - url: https://www.endocrine.org/patient-engagement/endocrine-library/adrenal-insufficiency
    title: "Adrenal Insufficiency | Endocrine Society"
    accessed: 2026-09-18
    # The same source /treatments/steroids and /tumors/pituitary-tumor already
    # carry, cited here so this page's adrenal rule rests on the document those
    # pages rest on rather than on a second account of it (§12.10, one claim at
    # one strength). Physical stress caused by illness, infection, surgery or an
    # accident can suddenly make symptoms much worse, and untreated an adrenal
    # crisis can cause death. Its own next sentence qualifies WHO GETS a crisis
    # rather than whether one kills, and this page implies no frequency either
    # way.
  - url: https://www.stjude.org/care-treatment/treatment/childhood-cancer/brain-tumors/craniopharyngioma.html
    title: "Craniopharyngioma Treatment at St. Jude - St. Jude Children's Research Hospital"
    accessed: 2026-09-18
    # Verified live, and cited for one thing: that a US patient-facing source
    # calls this a brain tumor plainly, near the nerves to the eyes and the
    # pituitary gland. That is what settles the causes-block ruling below. Its
    # share of childhood brain tumors and its ten-year survival figure are NOT
    # carried.
  #
  # BLOCK DECISIONS, ALL FOUR MADE BY OPENING THE BLOCK, and TWO OF THEM REVERSE
  # /tumors/pituitary-tumor. Names written without brackets on purpose, per the
  # ruling at the top of this front matter:
  #
  #   ESCALATION -- IN, inside the symptoms section, with a forward pointer above
  #     it and this page's own two conditional tiers beneath it (§12.10, the
  #     shape WI-534 used for shunts and WI-539 for apoplexy).
  #   MECHANISM -- IN, and this REVERSES the neighbouring hub. It was excluded
  #     there because a pituitary tumor is not in the brain, rarely seizes and
  #     does not obstruct the fluid pathways. All three are different here: the
  #     blocked-fluid paragraph is this tumor's story, the raised-pressure
  #     paragraph is close to this tumor's presentation, and seizures are
  #     reported. The block also carries a scoping clause saying which parts apply
  #     depends on what the tumor is and where it sits. The precedent that settles
  #     it is /tumors/meningioma, which is also outside the brain substance and
  #     includes the block. A short scoping line sits ABOVE the directive because
  #     the block's lobe-by-lobe map is not this tumor's anatomy. Note the map is
  #     NOT "at the end of the block": composed, it arrives under the block's own
  #     heading as a section of its own, which is what the reader meets and what
  #     the scoping line and its guard now say.
  #   CAUSES -- IN, demoted between "if it comes back" and the outlook gate per
  #     §12.9, and WITH NO SCOPING BRIDGE. The neighbouring hub needed one because
  #     the block says "brain tumors" repeatedly on a page whose thesis is that it
  #     is not one. That does not transfer: US patient sources call this a brain
  #     tumor outright, so the block's framing fits this reader and a bridge
  #     copied from there would answer a question this page does not raise.
  #   CAREGIVER -- IN. Contract item 11, and true here.
  #
  #   CROSSWALK -- OUT. The block is wholly the 2021 CNS rewrite: Roman to
  #     Arabic, gene results entering the NAME, and NOS and NEC. Only the first
  #     applies. The two genes here are diagnostic and are not part of this
  #     tumor's name, and this report carries neither of the other two labels.
  #     Including it would put glioma vocabulary into this reader's identity
  #     section, which is WI-514's defect. §12.9 still requires a slice, and this
  #     page writes its own.
  #   TUMOR-BOARD -- OUT, and this was decided by reading it rather than by
  #     habit. The block describes a MEETING about one case. What the sources
  #     describe for this tumor is ongoing care shared between neurosurgery,
  #     hormone doctors and eye doctors, for years. Those are different claims,
  #     and the page makes the second one in its own words.
  #   SPINAL-CORD and POSTERIOR-FOSSA-SYNDROME -- OUT. A tumor in this position is
  #     in neither region. Both blocks pin their including hubs with an exact
  #     assertion and neither list names this page, so nothing needs registering.
  #
  # All four exclusions are PINNED AS AN EXACT CLOSED SET, with mutations that
  # compose each excluded block back in. An excluded block leaves no trace on the
  # page, so nothing else would stop a later item quietly restoring one.
  #
  # WATCHING IS NOT ROUTED HERE, AND THAT REVERSES THE NEIGHBOUR TOO.
  # /treatments/watch-and-wait names pituitary tumors, acoustic neuromas,
  # meningiomas and some low-grade gliomas in its who-is-watched list. It does not
  # name this one, and the sources say an operation is almost always the first
  # step. So this page does not route there and publishes no surveillance
  # interval either way (WI-521).
  #
  # THE HORMONE LATE EFFECTS OF RADIATION BELONG TO /treatments/radiation-therapy,
  # which already covers them in detail, including that follow-up for them is not
  # standardized and that the reader should ask whether their levels will be
  # checked. Routed, not restated (§12.10).
  #
  # THE AVP DEFICIENCY RENAME BELONGS TO /tumors/pituitary-tumor, which carries it
  # with its full reason. This page NAMES it and routes, rather than restating it
  # at a third strength.
  #
  # NO NEW GLOSSARY ENTRIES. Most of this page's vocabulary is a FIRST use in the
  # corpus: the long type names, the chiasm, the stalk, the reservoir and the
  # field-loss pattern all appear nowhere else under Content/, so none meets
  # §12.8's second-use threshold and each is glossed inline. Two DO cross it and
  # are still declined. The nose-route word now appears in one other file, so
  # using it here is the second use -- but an entry would fire a tooltip on that
  # shipped page, which glosses the word inline, and that would manufacture the
  # echo defect WI-539 spent a review round removing. WI-553 owns it. The same
  # applies to the word for an under-active gland.
  #
  # NO PROGNOSIS FIGURES AND NO SURVIVAL FIGURES (§12.2 item 5, §12.5). The
  # sources are thick with them and not one reaches this page.
reviewed: 2026-09-18
review_due: 2027-03-18
disclaimers: [medical]
---

## The short version

A craniopharyngioma is a growth that sits deep in the head, just above the
gland that runs your hormones. It is slow growing, and it is not cancer.

It causes trouble because of where it is, not because it spreads. Three things
sit right next to it: the nerves for sight, the hormone gland, and the narrow
space the fluid around your brain drains through.

So the three things a team watches are your sight, your hormones, and that
fluid. Most people need an operation, and many need hormone medicine
afterwards, often for good.

Two sets of warning signs here are handled differently, and they are worth
knowing before the night you need them. You will find them in the symptoms
section.

## What is a craniopharyngioma?

**It starts from a few cells that were left behind before you were born.**

Long before birth, a small tube of tissue helps build the gland under your
brain. Nearly all of it disappears. In a few people, some cells stay behind,
and many years later a growth can start from them.

That is why nothing you did caused it, and why it can turn up at any age.

**It is often part solid and part fluid.** Many of these growths have one or
more fluid-filled sacs mixed in with the solid part. Doctors call a sac like
that a **cyst**. The cysts matter as much as the solid part, because a sac that
fills up takes room just as a solid lump does.

It sits in a small space at the base of the brain, right by the gland your
report may call the pituitary. Your team may say it is near the **stalk**,
which is the thin bridge joining that gland to the part of the brain above it.

## Is it cancer? What does its grade mean?

**No. It is not cancer, and it does not spread around the body.**

Your report may use the word benign. The word is accurate, and left on its own
it will mislead you, so here is the whole of it.

Benign says the growth is not cancer, and that it will not travel to other
organs. It does not say small, and it does not say easy. Surgeons describe these growths as
benign but locally pushy: they sit among things you cannot do without and they
tend to stick to them. A growth that is not cancer can still take part of your
sight, and can still stop a gland working.

**Harmless is a different word, and it is not the one that applies here.**

If your report carries a grade, it will be the lowest one. Grades run from 1 to
4 and describe how a growth is expected to behave. This one is a grade 1. On an
older report the very same thing is written as grade I.

**What matters is what it presses on, and how your hormones are doing.** Both of
those come further down.

## Where does it grow, and why does it cause these symptoms?

**Almost everything on this page follows from its three neighbours.**

It grows in a small hollow near the base of the brain. **Above it** run the
nerves carrying sight from each eye, crossing at a spot doctors call the
**chiasm**. **Below it** sits the hormone gland. **Behind and above** lies a
narrow channel that the fluid around your brain passes through, and the part of
the brain that controls hunger, sleep, temperature and thirst.

So trouble from this one arrives in three different ways.

**It can press on the nerves for sight.** Because of where those nerves cross,
what fades first is usually the outer edge of vision on each side. People
often do not notice it early. It gets found on a proper eye test, or when
somebody keeps bumping into things they did not see coming.

**It can stop the gland working.** Then you make too little of one hormone or
several. In a child that often shows up as growth slowing down or puberty
arriving late. At any age it can show up as tiredness, feeling cold, or a thirst
that will not settle.

**It can block the fluid.** This is the one that can come on fastest, and it is
why the symptoms section carries a rule for it.

**A note on what follows.** The part below is written about brain tumors in
general. The piece that matters most here is the fluid. In the brain map that
follows, the entry to read is the one about behind the eyes, where the pituitary
sits. The deep middle entry can apply too, because of the fluid. The rest of the
map is about growths elsewhere. That general part also carries advice to make a
phone call rather
than wait when pressure signs are new. Here that is overridden for these signs:
the rule in the symptoms section is stronger, and it is the one to follow.

[MECHANISM]

## What symptoms does it cause?

**They fall into the three groups above: sight, hormones, and fluid.**

**From pressure on the nerves for sight**, the common ones are losing the outer
edges of your vision on both sides, blurred sight, and sometimes double vision.

**When hormones run low**, the signs are broad and easily explained away:
tiredness, weakness, dizziness, weight that shifts, feeling cold all the time,
dry skin, changes to your periods, and less interest in sex. In a child,
slow growth or late puberty is often what gets noticed first. Nothing on that
list points at your head.

**A thirst you cannot satisfy, with far more trips to the bathroom**, is its own
thing, and it is common here. It happens when the back of the gland can no longer
hold water in the body. An older name for it borrowed the word diabetes. What
most people mean by that word is a completely different illness.

**From the fluid backing up**, the signs are headaches, feeling sick and
throwing up, being much sleepier than usual, and being confused. Headaches from
this are often worse in the morning.

**Two sets of signs here carry rules of their own.** Both sit just below these
lists.

[ESCALATION]

**This tumor has two rules the list above does not carry.** Both are better
learned now than looked up in a hurry.

**If these signs arrive quickly, or arrive together, treat them as urgent.** A
headache that is getting worse, throwing up again and again, being much sleepier
than usual, being confused, or your sight changing quickly. Those same signs
appear on the same-day list above, and they are the ones this growth has a
reason to cause, because it sits right where the fluid drains, and a blocked
drain can need treating quickly.
**These need help right away, at any hour, not a wait for the same day.** Phone
your team whatever the hour, or head for the emergency department. If someone
cannot be woken, if speech or one side suddenly stops working, if sight suddenly
goes, or if a first seizure happens, call 911, or your local emergency number,
without waiting to reach anybody else. This is treated, and treating it early is
the whole point.
[Shunts and hydrocephalus](/treatments/shunts#warning-signs) has the full list
of signs for anyone who already has a tube draining that fluid. This rule is
yours even if you do not have one of those tubes.

Rarely, one of these growths bleeds inside itself, and that brings the same
signs on suddenly. It is uncommon enough that it is not what a headache usually
means. It is in this list because the answer is the same either way: get seen.

**If you take a steroid as replacement, being ill changes the rules.** A body
that cannot make its own stress hormone has no reserve to draw on, so an everyday
illness can turn serious. An infection, an operation or an injury does the same.
Doctors call it an adrenal crisis, and the Endocrine Society says it can kill if
it is not treated.

**Watch for being very sick to your stomach, confusion, feeling faint, and
sudden weakness.** If that is happening now, do not wait: call 911, or your
local emergency number. A hospital is where this gets treated. If you
carry an emergency injection, this is the moment it is for, and you go in
afterwards anyway. Ask your team now for a written plan of your own: what to do
on days when you are ill, what to do when medicine will not stay down, whether an
emergency injection belongs in your bag, and whether to wear medical alert
jewelry.
[Steroids](/treatments/steroids) goes into the medicine itself, and sets out the
identical rule for anyone whose body has stopped making its own.

**This can be yours before anyone has put you on replacement.** For some people a
crisis is how the problem gets found at all. If this growth, or an operation on
it, has harmed the gland, take these signs just as seriously.

Both rules are worth explaining to whoever you live with.

## How do doctors find out it is this?

**A scan, blood tests, a proper eye test, and in the end the tissue itself.**

An [MRI scan](/tests/mri) shows where the growth sits and what it is touching. A
CT scan is sometimes added, because the specks of calcium these growths often
contain show up well on it, and that helps tell this apart from other things
that grow in the same spot.

Blood tests show which hormones are low, and that shapes what happens next.

When the growth sits close to those nerves, a proper eye test is arranged. It is
not the sight test you have for glasses. It charts the edges of your vision,
which is where loss begins here, and repeating it later is how small changes get
noticed.

The scan usually points to it, and the blood tests show what the gland is doing.
But the diagnosis and the type name are confirmed on tissue taken during the
operation, and that is one of the reasons an operation comes first.

## What do the words on my report mean?

**Your report may carry a long type name, and it is a description rather than
bad news.**

**Adamantinomatous, and papillary.** These are the two kinds. The first is the
one seen most often in children, and it usually holds cysts and specks of
calcium. The second is seen almost only in adults. They look different under a
microscope and they carry different gene changes.

**Until recently they were treated as two versions of one growth.** The World
Health Organization now counts them as two separate types, because they turned
out to differ in who gets them, how they look on scans, and what is going on in
their genes. So a newer report may name yours more precisely than an older one
would have. Nothing about your growth changed.

**A gene name may appear too.** One of these types usually carries a change in a
gene called **BRAF**. That is not part of the growth's name, and it is not
something you inherited or passed on. What it changes is the treatment, which is
why the treatment section returns to it.

**Grade 1, and grade I on an older report.** Both mean the same thing. The rules
for writing grades changed in 2021, from Roman numerals to ordinary ones, and
plenty of paperwork still carries the earlier style.

**Diabetes insipidus, and arginine vasopressin deficiency.** These are two names
for the same thing: the trouble with thirst and passing water described above.
The name was changed on purpose, because sharing the word "diabetes" with a
completely different illness was getting people hurt.
[Pituitary tumor](/tumors/pituitary-tumor) tells that story in full, and a
report saying both names at once is doing exactly what was intended.

[Your pathology report](/tests/pathology-report#what-the-grade-means) goes
through the document itself, though yours may not carry everything that page
describes.

## How is it usually treated?

**An operation is almost always the first step, and what is aimed for has
changed.**

**Surgery.** The goal is to take pressure off the nerves for sight and to
unblock the fluid, and to find out exactly what the growth is. There are two
routes in, and which one is used depends on where the growth sits and how far it
reaches. One goes up through the nose and the sinus behind it. The other is
through an opening made in the skull.
[What comes after an operation](/treatments/craniotomy) covers the general
aftercare.

**How much is taken out is a real decision, and it is worth asking about.**
Taking out every last piece lowers the chance of it coming back. But these
growths stick to things you cannot do without, and pulling tissue away from the
part of the brain that controls hunger, sleep and temperature can cause lasting
problems of its own. Many teams now deliberately leave a piece alone rather than
risk that, and treat what is left another way. Surgeons are also advised to
spare the stalk where they can. There is no single right answer, and it depends
on your own scan.

**Radiation**, often after surgery, treats what was deliberately left behind or
what has come back. [Radiation therapy](/treatments/radiation-therapy) covers
the course itself and what to ask about afterwards.

**Draining a cyst.** When the growth is mostly a fluid sac, a thin tube can be
put into it to let the fluid out. Sometimes a small reservoir is left under the
scalp so the sac can be drained again without another operation.

**Medicine aimed at a gene change.** For the adult type that carries the BRAF
change, drugs aimed at that change can shrink the growth. Where the type is
already known, they are sometimes given first, to make the operation easier.
This is newer than the other options. The reports so far are small, side
effects are common enough that some people stop, and how long the benefit
lasts is not yet settled. It is a fair thing to ask whether your type carries
that change.

## What is treatment actually like, and what is normal afterwards?

### Hormones afterwards

**Many people need hormone medicine afterwards, and often for good.** That is
not a sign that something went wrong. It is an expected outcome of treating a
growth in this spot, and the missing hormones can be replaced.

Which ones you need depends on what the growth and the treatment affected. A
hormone doctor takes this over and stays involved for the long term.

**If one of them is a steroid, go back and read the illness rule above**, and
make sure your plan is written down before you go home. Tell any doctor
treating you that you take it, especially before an operation or if you become
ill.

**Say so if thirst becomes constant and you are passing far more water than
before.** It is common after an operation in this spot. It is treatable, and it
is much better said than worked out at home.

**Sight often gets better once the pressure comes off**, and it will be tested
again after the operation. How much comes back depends partly on how long the
nerves were under pressure.

### When hunger and weight change, and it is not willpower

This one deserves its own explanation, because it is the part most often
misunderstood by everyone around you.

The area next to this growth controls hunger and how much energy your body
burns. If it is damaged, by the growth or by treating it, hunger can become very
hard to control and weight can climb fast.

**This is not a lack of effort, and saying so is not kindness, it is the
finding.** Weight gain from damage in that spot mostly does not respond to
ordinary advice about eating less and moving more. The same damage can also
upset sleep, body temperature and mood.

Teams do take it seriously, and there are things to try. It is honest to say
that results so far have been disappointing, and that no drug for it has yet
been proven to work in the strongest kind of trial. Ask who on your team looks
after this, and ask early rather than after a year of being told to try harder.

## Everyday life

From day to day, this comes down to hormones, sight, and how much energy you
have.

If you take hormone replacement, it needs a daily routine, and a separate plan
for the days you are sick. That written plan is the thing to keep where you can
find it in a hurry.

If your vision has altered, get it tested before you go back to driving. The
sight you lose first is exactly the sight you lean on at the edges of the road. Whether
you can drive, and from when, is decided where you live. Your own team will know
what applies to you; a website cannot, and that includes this one.

Tiredness, sleep that is out of step, and trouble holding attention are all
common here, and they are worth saying out loud at appointments rather than
absorbing at home.

## Follow-up scans, and what to do while you wait

Follow-up here is scans, blood tests and eye tests together, and it goes on for
a long time.

[Follow-up scans](/tests/follow-up-scans) covers how each new scan is compared
with the one before. [Waiting for results](/tests/waiting-for-results) is for the
wait itself.

Have your own schedule written down, and know what would bring a scan forward.

## If it comes back

These growths come back often enough that follow-up is built around it.

It usually comes back in the same place. The choices are the same as before:
another operation, radiation, draining a cyst, or in some cases medicine aimed
at a gene change. Which of them fits depends on the first round of treatment and
on what remains.

**Coming back is not only an early risk.** It usually happens in the first few
years, but it has been reported many years later, which is why check-ups carry
on long after you feel finished with all this. If you have been told follow-up
goes on indefinitely, that is why.

Ask what each option is meant to achieve, and write it down while you are being
told.

## Did I cause this?

[CAUSES]

**For this growth there is a more definite answer than usual.** It starts from
cells left behind before you were born, so there was nothing to prevent and
nothing to avoid. If you are a parent combing back through the pregnancy for a
mistake, there is no mistake waiting to be found.

## What might happen over time

:::outlook
Outlook here is usually given as a number. Those numbers describe groups of
people treated years ago, and not you.

The word you will meet is **median**. Put everyone in order of how things went
for them, and the median marks the middle of that line. Half the group falls on
each side. It says nothing about which side you are on, and it hides the people
who did far better than the middle.

There is a second thing worth knowing here, and it is the part numbers hide. For
this growth the question is usually not how long. It is how well, and that turns
on your sight, your hormones, and whether the area controlling hunger and sleep
was affected. Each of those is something a team can measure, and some of it can
be treated.

Ask whenever you are ready. Deciding not to is a choice too, and it is not
final.
:::

## For the person caring for someone with this

[CAREGIVER]

Three things are specific to this one.

**Learn both urgent rules alongside them.** One is signs that arrive quickly or
together; the other is illness in anyone whose gland has been harmed, whether or
not they are on a replacement steroid. You may well spot either before they do,
and neither can wait until morning.

**Hunger and weight changes are not a lack of effort.** If the area that
controls appetite was affected, hunger can be very hard to control, and ordinary
advice about eating less does not work. Treating it as willpower is the most
common way this goes wrong at home.

**A failing gland can look like a change of character.** Being tired, flat, cold
or uninterested can all come from hormones that have stopped arriving, rather
than from someone who has stopped trying. Say it to the team rather than keeping
it to yourself.

## What to ask your team

- Which hormones are low now, and which will be checked later?
- Has my sight been mapped properly, and when will it be done again?
- Is the fluid draining normally on my scan?
- What is the plan: an operation, radiation, draining a cyst, or a mix?
- How much are you aiming to take out, and what are you deliberately leaving?
- Which type is mine, and does it carry the gene change that has a medicine?
- Will I need hormone replacement, and for how long?
- If I need a steroid, what is my plan for days when I am ill?
- Should I carry an emergency injection and wear medical alert jewelry?
- Who looks after appetite and weight if that becomes a problem?
- What would make you want me to call before the next appointment?
- Who do I call after hours, and what is the number?

## Where to get support

- [Getting help now](/get-help-now), for the day a page is not enough and you
  need a person.
- [When your child has a brain tumor](/tumors/pediatric-brain-tumor), if the
  person with this is your child. It covers what changes when the patient is a
  child, from holding still for a scan to school and the long tail of check-ups.
- The **Pituitary Network Association** publishes patient material on growths in
  this part of the head and the hormone problems they cause.
- The specialist nurse on your team, who is usually the quickest way to a real
  answer.
