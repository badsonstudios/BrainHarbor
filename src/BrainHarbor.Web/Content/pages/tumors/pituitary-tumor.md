---
title: "Pituitary tumor"
slug: tumors/pituitary-tumor
description: "A growth in the small gland under the brain that controls hormones. Why sight and hormones are the two things to watch, what the names on your report mean, and the two emergencies this page carries rules for."
tags: [tumor-type]
sources:
  # WI-539. THIS IS A TUMOR HUB, so §12.3's seventeen sections and §12.9's proved
  # template govern. The 50-line stub this replaces cited exactly two sources and
  # BOTH were problems, which is where the item started.
  #
  # NO BLOCK DIRECTIVE IS WRITTEN IN BRACKETS ANYWHERE IN THIS FRONT MATTER, and
  # that is deliberate. CaregiverSectionTests locates the caregiver block with a
  # raw IndexOf over the WHOLE file, so a bracketed name in a comment here becomes
  # the FIRST match and the test reports the block as not being under its heading.
  # The first draft of this page did exactly that and went red. §12.8 already
  # records the rule from WI-538 -- a front-matter comment is inside the string
  # every raw-file guard reads, so paraphrase the corpus rather than quoting it
  # here. Block names below are written without brackets for that reason.
  #
  # NCI's /types/brain IS REMOVED. §12.1 is explicit: "A page that cites NCI for a
  # tumor name is a defect." NCI patient PDQ is fine for framing and tone and
  # barred for naming and grading -- and naming is most of what this page does.
  #
  # CANCER RESEARCH UK IS REMOVED. WI-538 barred it for idiom: its prose is
  # British in exactly the sentences a US reader-facing page wants. Note the
  # honest form of the claim -- nothing here is TAKEN from CRUK, but two CRUK
  # entries still reach the reader's source list via the escalation block, which
  # is a property of the COMPOSED page that a front-matter check cannot see.
  #
  # ONE SOURCE IS CITED BUT CANNOT BE SCRIPT-CHECKED, and saying so is the point.
  # cancer.org renders its content in JavaScript: a plain fetch returns a 38 KB
  # navigation shell with an empty <title>, and amp.cancer.org returns the
  # byte-identical shell. It is cited below and read for facts, but its quotes
  # are NOT in the item's checkable pack, so the claim-to-source map covers every
  # OTHER source and not it. Where a checkable source carries the same claim, it
  # is cited alongside.
  #
  # mayoclinic.org RETURNED HTTP 403 AND IS NOT CITED AND NOT USED. An earlier
  # version of this comment said two sources were cited-but-uncheckable and named
  # it as the second, while it appeared in no source list and the page's own test
  # banned it on the known-dead list. That is §12.14's shape one level up: the
  # note describing the problem read as though the problem had been handled.
  # Nothing on this page rests on it.
  #
  # NCI (cancer.gov) REACHES THE READER'S SOURCE LIST VIA THE COMPOSED BLOCKS,
  # three times, through CAUSES and CAREGIVER. §12.1 holds -- not one of them is a
  # naming claim, and this page cites NCI for nothing itself. Recorded because the
  # note above anticipates exactly this leak class for Cancer Research UK and did
  # not for NCI, so the next reader would have re-opened the question.
  - url: https://www.endocrine.org/patient-engagement/endocrine-library/pituitary-tumors
    title: "Pituitary Tumors | Endocrine Society"
    accessed: 2026-09-16
    # THE BACKBONE OF THIS PAGE, and the source for its hardest sentence:
    # "They are not brain tumors and are almost always benign (non-cancerous);
    # cancerous pituitary tumors are extremely rare." Also the three-way split the
    # page is built on ("whether they are caused by the tumor mass or hormonal
    # changes (either too much or too little hormone)"), the mass-effect symptoms
    # ("headaches and trouble seeing, especially problems with peripheral
    # vision"), the low-hormone list, "Abnormal control of eye movements,
    # sometimes causing double vision or inability to completely open one eye",
    # the surgery route ("through your nose and the nasal sinuses"), the scalp
    # alternative, dopamine agonists for prolactinoma, and watching.
    #
    # ITS "about 40% of pituitary tumors" IS NOT CARRIED (§12.3 makes "how common
    # is it" a deliberate omission). Its own citation block is broken -- it cites
    # itself as "Thyroid Cancer" -- which is recorded as a quality signal, not as
    # a reason to drop it.
    #
    # ALSO THE SOURCE FOR THE SIGHT SENTENCE, verbatim: "After surgery, vision
    # problems improve in most people, or they can go away all together." Recorded
    # because that sentence was WRONGLY REMOVED from this page once, as unsourced,
    # on the strength of a search that had truncated and suppressed the line
    # carrying it. Cutting it left a subsection about losing your sight ending on
    # an administrative line, which is what §12.6 forbids. It is carried in
    # DIRECTION-ONLY form ("sight often improves once the pressure is off") with
    # no share attached, per §12.4 R3. Also the treatment claims: "Prolactinoma is
    # mostly treated successfully with drugs called dopamine agonists"; "The most
    # common side effects are nausea and dizziness"; "Sometimes the medication does
    # not work, so your doctor might recommend surgery" -- which is why the page
    # says surgery comes up if the medicine does not work, and not "only if".
    #
    # AND THE RECURRENCE SECTION, which /review round 4 found resting on a
    # verbatim recorded in neither front matter nor the claim map while weaker
    # claims were in both: "If you have a large part of the tumor remaining after
    # surgery or the tumor regrows, you may need more surgery and/or radiation
    # therapy." That one sentence sources both the radiation route and the three
    # choices named under "If it comes back".
    #
    # WHAT IT DOES NOT SUPPORT, corrected at round 4: the page used to say that
    # some hormone tests are timed and some take a morning. This source says only
    # "Additional tests, called stimulation tests might be needed to check your
    # pituitary function" -- and neither the timing nor the morning appears in any
    # of the nine files. The detail is real and sits in
    # docs/research/tumor-guides/tests-library.md 9, but that is a DOSSIER and not
    # a fetched source, which is the exact footing two other sentences were
    # deleted for at round 3's S5. Reworded to what this source carries; WI-551
    # owns the detail once it is fetched. Filed as a nit by the reviewer and
    # promoted on checking, because the rule does not bend for small claims.
    #
    # THE DOOR-FRAMES SENTENCE IS AN ILLUSTRATION, NOT A FINDING, and is kept as
    # one. Bumping into door frames appears nowhere in the pack and nowhere else
    # under Content/. What IS sourced is the deficit it illustrates -- "headaches
    # and trouble seeing, especially problems with peripheral vision" -- and that
    # sight testing happens. It is the most concrete sentence in that section, so
    # it stays, recorded here so the next editor does not hunt for a citation the
    # page never claimed.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK559222/
    title: "Pituitary Apoplexy - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-16
    # §12.13: the StatPearls ban is on the OLIGODENDROGLIOMA chapter for a
    # purpose, not on the domain. This chapter is used for the clinical detail of
    # the emergency and nothing else. Verbatim: "Pituitary apoplexy is a medical
    # and surgical emergency in many cases"; "Delay in recognition or treatment
    # may result in permanent hypopituitarism, irreversible visual loss, or
    # death."; "A sudden onset of headache located behind the eyes is the most
    # common symptom"; "Other symptoms include decreased visual acuity,
    # hemianopia, diplopia, ptosis, nausea and vomiting, altered mental status".
    #
    # THE ENTRY IS THE HEADACHE ALONE, AND THIS SOURCE IS WHY. "A sudden onset of
    # headache located behind the eyes is the MOST COMMON symptom"; everything
    # after it is filed as "Other symptoms include". The tier used to open on a
    # CONJUNCTION -- "a sudden, severe headache with a change in your sight" --
    # which narrowed the entry to readers who had BOTH and sent the commonest
    # presentation, a headache on its own, straight past the rule written for it.
    # /start already files a lone sudden severe headache under CALL 911, so the
    # conjunction also filed one symptom at two strengths on two pages (§12.10).
    # This chapter agrees and is DISJUNCTIVE where the page was not: "Any acute
    # visual change OR significant headache should prompt immediate medical
    # assessment", and its education section lists "sudden severe headache,
    # visual changes, nausea, vomiting, or altered consciousness". So the whole
    # entry now routes to 911, which is what /start says, and the sight change is
    # an escalator among the other signs rather than a condition of entry.
    # /review round 3's B1.
    #
    # THE FREQUENCY, IN DIRECTION-ONLY FORM: "Apoplexy in pituitary adenomas is
    # rare and is estimated to be 0.2% annually." The word "rare" is half of what
    # lets the tier close on "Both of these are uncommon"; the 0.2% is not carried.
    #
    # AND A SIGN THE REVIEWER CALLED VERBATIM, WHICH IT IS NOT. /review round 3's
    # S4 asked for fainting or collapse, citing this chapter's History and
    # Physical. That paragraph uses NEITHER word: what it has is "altered mental
    # status", and, for the adrenal crisis that follows apoplexy, "hypotension,
    # hypothermia, lethargy, and, on occasion, coma". The page therefore carries
    # "passing out", which those support, and does NOT quote "fainting" or
    # "collapse". The finding was right that the sign belonged in the list; the
    # quotation behind it was not exact, and §12.2 wants the claim checked even
    # when the reviewer has the conclusion right.
    #
    # NONE OF ITS NUMBERS IS CARRIED: not the 80% needing hormone replacement,
    # not the 90%/70% deficiency rates, not the 1.5% to 27.7% incidence range,
    # and not the hydrocortisone milligram doses (§12.4 R1 keeps all mg figures
    # off).
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC9170656/
    title: "The NETting of pituitary adenoma: a gland illusion"
    accessed: 2026-09-16
    # AN EDITORIAL EXPRESSING ITS AUTHORS' PERSONAL VIEWS, and this note says so
    # because an earlier version did not. It states as much outright -- "This
    # editorial expresses the personal views of the authors" -- and its authors are
    # the journal's Editor-in-Chief and Editorial Board members. /review's S2 found
    # the page attributing it to THE PITUITARY SOCIETY, which it is not. The actual
    # Society position statement is a separate 2019 paper that this one cites, and
    # that paper was NEVER FETCHED, so it is not cited here either: §12.2 publishes
    # no claim without a live citation, and citing an unfetched paper to rescue a
    # sentence would be the defect it was covering up (WI-538 shipped nine
    # citations written from memory).
    #
    # Every claim from it is ATTRIBUTED on the page and never asserted, and the
    # page states plainly that WHO made the change. §12.13 warns about exactly this
    # shape. Verbatim: "Simply redressing adenomas as NETs does not change
    # histopathology nor the prognosis of pituitary neoplasms."; "the case for
    # terminology change to NET has not been made".
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11226279/
    title: "Does New WHO 2022 Nomenclature of Pituitary Neuroendocrine Tumors Offer an Extra Edge to the Neurosurgeons for Its Management? A Narrative Review - PMC"
    accessed: 2026-09-16
    # The other side of the naming question, and the patient-facing objection:
    # the new label risks "needless frustrations and apprehension among most of
    # the patients diagnosed with benign PAs".
  - url: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC9759163/
    title: "Changing the Name of Diabetes Insipidus: A Position Statement of the Working Group for Renaming Diabetes Insipidus"
    accessed: 2026-09-16
    # THE SECOND RENAME, AND IT PULLS THE OPPOSITE WAY. This one was made to
    # PROTECT patients: "the use of the common term 'diabetes' in both has
    # unfortunately led to confusion for both patients and their caretakers", and
    # "In several patients with central diabetes insipidus, desmopressin
    # treatment was withheld with serious adverse outcomes, including death".
    # Endorsed by eight societies INCLUDING the Pituitary Society, which the text
    # names in full: "Endocrine Society, European Society of Endocrinology,
    # Pituitary Society, Society for Endocrinology, European Society for Paediatric
    # Endocrinology, Endocrine Society of Australia, Brazilian Endocrine Society,
    # and Japanese Endocrine Society", with "John Wass for the Pituitary Society"
    # in the working group.
    #
    # THE "SAME BODY" RATIONALE THAT USED TO SIT HERE WAS WRONG AND IS REMOVED.
    # It read that the Pituitary Society both endorsed this rename and objects to
    # the PitNET one. It does not: the objection is an EDITORIAL of the journal's
    # editors' personal views (see the PMC9170656 note above). The authors overlap;
    # the documents do not. The page can still say the two renames were made for
    # OPPOSITE REASONS -- one argued to frighten people for no benefit, one made
    # because a word was getting people hurt -- but not that one body did both.
    # Recorded at length because a rationale that outlives its correction does not
    # sit inert: it argues for putting the error back.
    #
    # Also the reason a reader sees both names at once: "we propose that for
    # several years we keep the previous name in parentheses".
    # Its survey figures (85%, 87%) are NOT carried.
  - url: https://www.ncbi.nlm.nih.gov/books/NBK470458/
    title: "Arginine Vasopressin Disorder (Diabetes Insipidus) - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-16
    # Why this belongs on THIS page: "Traumatic injury to the hypothalamus and
    # posterior pituitary or neurosurgery with a transsphenoidal approach usually
    # induces AVP-D." Its incidence ranges and its volume figures are NOT carried.
  - url: https://www.endocrine.org/patient-engagement/endocrine-library/adrenal-insufficiency
    title: "Adrenal Insufficiency | Endocrine Society"
    accessed: 2026-09-16
    # THE SECOND EMERGENCY, at patient level and with no doses. Verbatim:
    # "Physical stress caused by illness, infection, surgery, or an accident can
    # suddenly make symptoms of AI much worse, an emergency illness called an
    # adrenal crisis. If left untreated, adrenal crisis can cause death."; "People
    # in adrenal crisis need an injection (shot) of glucocorticoids (medicines
    # that replace cortisol) right away. Then they need to go to the hospital
    # immediately for more treatment."; "If you have AI, you should wear a
    # medical alert bracelet or tag and know the warning signs"; the crisis signs
    # themselves -- "Severe nausea and vomiting", "Dehydration and confusion",
    # "Low blood pressure and fainting"; and "Some people don't know they have AI
    # until they have a sudden worsening of symptoms called an adrenal crisis."
    #
    # THE QUALIFIER THIS BLOCK USED TO STOP ONE SENTENCE SHORT OF, recorded
    # because the next sentence in the source complicates the claim for THIS
    # reader: "Adrenal crisis occurs mainly in people with primary AI." This
    # page's readers have SECONDARY AI, from the pituitary. Read carefully, that
    # sentence qualifies WHO GETS a crisis, not WHETHER a crisis kills, so the
    # death claim stays attributable to this source -- and it is also half of what
    # lets the tier close on "Both of these are uncommon".
    #
    # AN EARLIER VERSION OF THIS NOTE CLAIMED THE PAGE IMPLIED NO FREQUENCY AT
    # ALL. The sentence itself is deliberately NOT quoted here: this page's own
    # front-matter guard bans it, and a guard that reads the RAW file cannot tell
    # a quotation inside a correction from the claim itself -- §12.8's WI-538 rule
    # (a front-matter comment is inside every raw-file guard's haystack), which
    # already bit this item once through CaregiverSectionTests, and bit it a
    # second time on the first attempt at this very note. It sat four hundred
    # lines above a tier that ends
    # "Both of these are uncommon". The note described a rule the page was already
    # breaking, which is §12.14's shape one level up: a note that reads as though
    # the problem had been handled. /review round 3's S1. The frequency word is
    # DIRECTION ONLY and both halves are sourced -- NBK559222 says "Apoplexy in
    # pituitary adenomas is rare", and the sentence above qualifies who gets a
    # crisis for a reader whose AI is secondary. NO FIGURE reaches the page
    # (§12.4 R1). The risk in this population is carried by NIDDK
    # (cited below: "Anything that affects the pituitary's ability to make ACTH
    # can cause secondary adrenal insufficiency", with "bleeding in the pituitary"
    # named among the causes) and by NBK559222, which calls acute adrenal
    # insufficiency "the most immediate and life-threatening complication" of
    # apoplexy. §12.13: quote to the end of the qualification, not to the end of
    # the convenient sentence.
  - url: https://www.niddk.nih.gov/health-information/endocrine-diseases/adrenal-insufficiency-addisons-disease/symptoms-causes
    title: "Symptoms & Causes of Adrenal Insufficiency & Addison's Disease - NIDDK"
    accessed: 2026-09-16
    # Ties the two emergencies together from a source rather than by inference:
    # "Anything that affects the pituitary's ability to make ACTH can cause
    # secondary adrenal insufficiency", with "bleeding in the pituitary" named
    # among the causes. Also the warning that matters most: "Because symptoms of
    # adrenal insufficiency come on slowly over time, they may be overlooked or
    # confused with other illnesses. Sometimes symptoms appear for the first time
    # during adrenal crisis."
  - url: https://www.cancer.org/cancer/types/pituitary-tumors/treating/surgery.html
    title: "Pituitary Tumor Surgery | American Cancer Society"
    accessed: 2026-09-16
    # CITED BUT NOT SCRIPT-CHECKABLE (see the ruling above). Read for: the
    # sphenoid sinus route, "No part of the brain is touched during
    # transsphenoidal surgery", the small cut inside the nose, congestion for up
    # to two weeks, fluid leaking from the nose, craniotomy only "if the pituitary
    # tumor is larger or more complicated", and the rename "Arginine vasopressin
    # deficiency, formerly known as diabetes insipidus". Where the Endocrine
    # Society carries the same claim it is cited alongside and is the checkable one.
  #
  # BLOCK DECISIONS -- ALL FIVE MADE BY READING THE BLOCK, and two of my four
  # starting assumptions were wrong. Names written without brackets on purpose,
  # per the ruling at the top of this front matter:
  #
  #   ESCALATION -- IN, inside the symptoms section, with this page's OWN
  #     conditional tier beneath it (§12.10, the shape WI-534 used for shunts).
  #   CAUSES -- IN, demoted between "if it comes back" and the outlook gate per
  #     §12.9. Cause and self-blame do not depend on where the tumor sits, and its
  #     general sentence is HEDGED, which §12.10 calls scope rather than weasel
  #     wording. /tumors/brain-metastases excluded it because there that sentence
  #     is flatly false; here it holds.
  #   CAREGIVER -- IN. Contract item 11, and true here -- which are two separate
  #     things and both were checked.
  #
  #   MECHANISM -- OUT. It opens "These are the ways a tumor IN THE BRAIN causes
  #     symptoms" and then asserts brain swelling, seizures, and blocked fluid
  #     needing a shunt, followed by a lobe-by-lobe map. A pituitary tumor is not
  #     in the brain, rarely seizes, and does not block the fluid pathways. It has
  #     NO scoping clause: it asserts flatly. That is the test §12.10 sets, and it
  #     is the mirror of WI-538's error, which wrongly excluded the spinal-cord
  #     block by calling a conditional block unconditional.
  #     WI-568 RE-EXAMINED THIS AND THE EXCLUSION STANDS, BUT ONE OF ITS STATED
  #     REASONS NO LONGER DOES. This note used to carry a fourth reason: that the
  #     anatomy which matters here -- the sight nerves directly above, the nerves
  #     to the eye beside it -- was nowhere in the block. WI-568 added a
  #     pituitary/sellar entry to the block's map ("Behind the eyes, at the base
  #     of the brain"), so that reason became false and was DELETED rather than
  #     left to rot -- WI-549's lesson that a comment disagreeing with its own
  #     page is a defect report nobody has read. It is not quoted here, because a
  #     verbatim copy of a retired claim is the next reader's live one; the
  #     deletion is pinned by MechanismBlockTests instead. The three reasons above
  #     are untouched and are on their own sufficient: they are about what the
  #     block ASSERTS of this tumor, not about what it omits. Adding a missing
  #     entry does not repair a block that opens by calling this a tumor in the
  #     brain.
  #   CROSSWALK -- OUT. It is entirely the 2021 CNS rewrite: Roman-to-Arabic CNS
  #     grades, gene results entering the name, NOS and NEC. This report carries
  #     none of those, and this tumor's renames came from the endocrine volume and
  #     the AVP working group instead. Including it would put glioma vocabulary
  #     into this reader's identity section, which is WI-514's defect verbatim.
  #     §12.9 still requires a crosswalk SLICE from every hub, and this page's is
  #     three pairs, written in its own prose.
  #
  # Both exclusions are PINNED BY TESTS, with mutations that compose the block
  # back in -- an excluded block leaves no trace, so nothing else would stop a
  # later item quietly restoring it.
  #
  # WATCHING IS ROUTED, NOT OWNED. /treatments/watch-and-wait already names
  # pituitary tumors in its who-is-watched list, carries the eye-test and
  # hormone-blood-test detail and a per-tumor note, and links back here. It also
  # carries WI-521's ruling that NO per-tumor schedule is printed, so this page
  # publishes no surveillance interval.
  #
  # THE TESTS THEMSELVES BELONG TO WI-551, WHICH DOES NOT EXIST YET.
  # docs/research/tumor-guides/tests-library.md §9.1-9.2 already holds that
  # research: the perimetry protocol, the chin-rest and patched eye, the "it feels
  # like failing" insight, prolactin and IGF-1 screening, timed morning cortisol,
  # dynamic testing. This page writes NONE of it -- one sentence that the tests
  # happen, worded to stand alone so a link can be added later without rework.
  # Its sentence deliberately avoids watch-and-wait's own wording for the same
  # test, which the first draft collided with.
  #
  # TRANSSPHENOIDAL SURGERY BELONGS TO WI-553, WHICH DOES NOT EXIST YET, and the
  # word appears nowhere else in the corpus. SYNTHESIS says that page will own "no
  # head incision, no shaved head, nasal recovery, hormone follow-up". This page
  # owns a proportionate slice -- the route, and why it is not a craniotomy -- and
  # deliberately less than that page will.
  #
  # NO NEW GLOSSARY ENTRIES. §12.2 item 9 asks new vocabulary to join the
  # glossary; §12.8 sets the threshold at the SECOND use in the corpus.
  # "prolactinoma", "acromegaly", "Cushing", "hypopituitarism", "apoplexy",
  # "PitNET" and "transsphenoidal" appear ZERO times anywhere else in Content/, so
  # every one is a first use and none meets the threshold. Each is glossed inline
  # instead. "extra-axial" already exists and fires on its own.
  #
  # "extra-axial" IS THE ONE PAGE TERM WITH NO SUPPORT IN THIS ITEM'S PACK, and
  # /review round 4 was right to ask, given that round 3 deleted two sentences for
  # exactly that. The ruling, made deliberately rather than by omission: the term
  # is GLOSSARY-OWNED (Content/glossary/extra-axial.md carries its own citation),
  # it is anatomy rather than a naming claim, and /tumors/meningioma already uses
  # it -- so §12.1's bar on unsourced NAMING is not engaged and one term keeps one
  # behaviour on two pages. What WAS dropped is the clause "on scan reports",
  # which asserted where the reader would see the word: that is a claim about the
  # reader's paperwork, no source in the pack says it, and meningioma does not say
  # it either. The construction also deliberately avoids meningioma's own
  # "Doctors have a word for that: extra-axial" -- an eight-word run with four
  # content words, which is restatement-guard territory.
  #
  # THE REMAINING TOOLTIP ECHO IS ACCEPTED, NOT MISSED. The page says the gland
  # sits under the brain rather than inside it, and the popover answers "Growing
  # outside the brain itself, not inside it." Round 1 removed the VERBATIM echo,
  # where the page handed the popover its own opening clause back.
  #
  # THAT OLD WORDING IS DELIBERATELY NOT QUOTED HERE, and the reason is the
  # lesson. This page's render guard asserts the old string is ABSENT FROM THE RAW
  # FILE, and a front-matter comment is inside the haystack it reads -- so writing
  # the correction with its evidence attached turned the suite red. The trap has
  # now caught this item THREE times: the bracketed block names in the first
  # draft, the frequency note at round 4, and this comment minutes later. Twice it
  # happened while writing a note whose SUBJECT was the trap. The general shape is
  # worth more than the instance: a correction naturally wants to quote the thing
  # it corrects, and that is precisely what a raw-file guard cannot tell apart
  # from the thing itself. Describe the old wording; do not reproduce it.
  #
  # What remains is the semantic overlap between a term and its definition, which
  # is what a glossary is for. Suppressing the tooltip was rejected in round 1 on
  # corpus grounds -- /tumors/meningioma glosses the same term and lets it fire --
  # and that ruling still holds.
  #
  # NO PROGNOSIS FIGURES AND NO SURVIVAL FIGURES (§12.2 item 5, §12.5). The
  # sources are thick with percentages -- vision-recovery rates, CSF-leak rates,
  # AVP-D incidence, prevalence, survey preferences. None reaches this page.
reviewed: 2026-09-16
review_due: 2027-03-16
disclaimers: [medical]
---

## The short version

A pituitary tumor is a growth in a small gland that sits under your brain. Almost
all of them are not cancer.

The gland runs your hormones, and the nerves for sight pass just above it. So the
two things a team watches are your hormones and your sight.

Treatment is not always surgery. Some are watched. Some are treated with pills
alone. When there is an operation, it is usually done through the nose.

Two things on this page are emergencies: a sudden, severe headache, and being
ill if you take steroid replacement. Both are further down, and both are worth
knowing before you need them.

## What is a pituitary tumor?

**It is a growth in the gland that tells your other glands what to do.**

The pituitary is about the size of a pea. It sits at the base of the brain,
behind your nose. It makes hormones that travel round the body, and many of them
tell other glands to get to work. That is why it gets called the master gland.

A tumor here is a growth in that gland.

**These are not brain tumors, and that is worth saying plainly.** The Endocrine
Society puts it that way: they are not brain tumors, and they are almost always
not cancer. The gland sits under the brain rather than inside it, and the name
for that is **extra-axial**.

That matters, and it does not settle everything. A growth outside the brain can
still press on the nerves for sight. It can still stop a gland working. What it
tells you is where the trouble comes from, not how much trouble you have.

You are reading this on a site about brain tumors, and your team may well have
used those words. That is not a mistake either. This tumor sits in the same small
space, gets found on the same scans, and is treated by some of the same people.

## Is it cancer? What does its grade mean?

**Almost always, no.** Cancer of the pituitary is extremely rare.

Most pituitary tumors are what doctors call benign. But that word does more work
than it can carry, so here is what it does and does not mean.

It means the growth is not spreading round your body. It does not mean harmless.
A small growth in this spot can take part of your sight, because of what is
sitting right above it. It can stop a gland working, and that can be serious. Not
cancer and not a problem are two different sentences.

The grade 1 to 4 you may have read about for other brain tumors is not used for
this one. Your report describes what the tumor is made of and which hormone, if
any, it makes. Those are the things that change the plan. If a number does turn
up beside a word you do not know, ask what it measures.

**The useful questions are what it is pressing on, and whether it is making a
hormone.** Both are further down this page.

## Where does it grow, and why does it cause these symptoms?

**Almost everything on this page follows from what is next door.**

The gland sits in a small hollow of bone. Directly above it, the nerves that
carry sight from each eye cross over. To each side run the nerves that move your
eyes.

So a growth here has three ways of causing trouble.

**It can press upward on the nerves for sight.** They cross right above the
gland, which is why the sight that goes first is usually at the outer edges. You
may not notice it early. People often find it by bumping into door frames, or in
a sight test.

**It can press sideways on the nerves that move your eyes.** Then things look
double, or one eyelid droops, or an eye does not move the way the other one does.

**It can change your hormones, in either direction.** Some of these tumors make
too much of one hormone. Others get big enough to stop the normal gland working,
so you end up with too little. Both happen, and they feel completely different.

That three-way split is why two people with the same diagnosis can describe
nothing alike.

## What symptoms does it cause?

**They fall into the three groups above: pressure on sight, pressure on the
nerves that move your eyes, and hormones.**

From pressure, the common ones are headaches and trouble seeing, especially at
the edges of your vision. Eye movements can be affected too, which shows up as
double vision or an eyelid you cannot fully open.

**From too little hormone**, the list is long and vague, which is exactly why it
gets missed: feeling tired, feeling weak, feeling dizzy, dry skin, periods that
change or stop, and loss of interest in sex. None of it points at your head.

**From too much hormone**, what you notice depends on which one.

- **Prolactin.** Periods that become irregular or stop, milk from the breasts
  when you are not feeding a baby, and in men, trouble with erections. A tumor
  making prolactin is called a **prolactinoma**.
- **Growth hormone.** Hands, feet and facial features that slowly change, often
  over years. Doctors call that **acromegaly**.
- **ACTH**, which tells the body to make a stress hormone. Weight gain around the
  face and middle, and other changes. Doctors call that **Cushing's disease**.

If you recognize something in that list, say so at your next appointment, even if
it seems small. A lot of these are easy to explain away one at a time.

**Two emergencies here have rules of their own.** They come after the lists
below.

[ESCALATION]

**This tumor has two emergencies of its own, and the list above does not cover
them.** Both are worth reading once now rather than looking up at two in the
morning.

**A sudden, severe headache.** Sometimes a pituitary tumor bleeds, or loses its
blood supply. Doctors call it **apoplexy**. The headache comes on suddenly, often
behind the eyes, and it is the most common sign of it. It can come on its own. It
can also come with sight that drops, double vision, a drooping eyelid, feeling
sick, passing out, or feeling confused.
**This one cannot wait for morning.** Call 911, or your local emergency number,
or go straight to the emergency department. This needs a scan and treatment that
a phone call cannot give you. Tell your team as well, at whatever hour it is. It
is treated, and treating it early is what protects your sight.

**If you take steroid replacement, being ill is its own rule.** When the gland
cannot make the hormone that tells your body to produce its stress hormone, a
plain illness can become dangerous. So can an infection, an operation, or an
accident. The Endocrine Society calls that an adrenal crisis, and says that
without treatment it can kill.

**The signs are being very sick to your stomach, confusion, and feeling faint or
passing out.** If those are happening now, that is an emergency: call 911, or
your local emergency number. If you have been given an emergency injection, that
is what it is for, and the Endocrine Society says you still go straight to a
hospital afterwards. Ask your team now for your own plan: what to do when you
are ill, what to do if you are being sick and cannot keep pills down, and
whether you should carry an injection and wear a medical alert bracelet. **Being sick and
unable to keep pills down needs help the same hour, not the next day.**
[Steroids](/treatments/steroids) covers the medicine itself, and carries the same
rule for anyone on replacement.

**You do not have to be on replacement for this to be yours.** Some people find
out they have the problem when a crisis is the first sign of it. If this tumor or
an operation on it has hurt your gland, treat these signs the same way.

Both of these are uncommon. Both are worth telling the people you live with as
well.

## How do doctors find out it is this?

**Blood tests and a scan, and usually a check of your sight.**

Your team will take blood to measure hormone levels, because the answer changes
the plan more than the size does. An [MRI scan](/tests/mri) shows the gland and
what is around it.

If the tumor is near the nerves for sight, you will be sent for a proper sight
test. It is not the one you have at the eye doctor for glasses. It maps the outer
edges of what you can see, which is where loss tends to start here, and it is
repeated over time so small changes show up.

Some people need a further test that checks how the gland responds. Your team
will tell you which.

Unlike most tumors on this site, a piece of tissue is not usually taken first.
Here the blood tests and the scan often give the answer, and any tissue comes
from the operation itself if there is one.

## What do the words on my report mean?

**Your report may carry two names for the same thing, and neither is a mistake.**

**Pituitary adenoma, and PitNET.** The older name is adenoma. The World Health
Organization now prefers pituitary neuroendocrine tumor, shortened to PitNET. You
may see either, or both together.

That change is argued about, and the argument is worth knowing because it is
about you. The editors of the medical journal for this field have objected in
print. They say renaming these growths does not change what they are under a
microscope, and does not change the outlook. Another review put the worry
plainly: the new name risks needless worry for people whose tumor is not
dangerous, because "tumor" sounds worse than "adenoma" did.

So if your report says PitNET and you read it as bad news, that reading is one
that specialists themselves predicted and argued against.

**Pituitary carcinoma, and metastatic PitNET.** The same change renamed the rare
cancerous form. Both names mean the same rare thing.

**Diabetes insipidus, and arginine vasopressin deficiency.** This one is the
opposite story, and it is a good one.

Damage to the back of the gland can leave you very thirsty and passing a lot of
water. It has nothing to do with diabetes of the sugar kind. That shared word
caused real harm: people were admitted to the hospital, staff assumed the wrong
illness, and treatment was held back. In 2022 a working group proposed a new
name, arginine vasopressin deficiency, so the word "diabetes" would stop causing
that mix-up. Eight hormone societies endorsed it, including the Pituitary
Society.

They also said they would keep the old name in brackets for some years. So a
report that says "arginine vasopressin deficiency (cranial diabetes insipidus)"
is doing exactly what was intended.

Arginine vasopressin deficiency often follows the operation through the nose,
which is why it is here.

**Two renames on one page, made for opposite reasons.** One, doctors argue,
frightens people for no benefit. The other was made because a word was getting
people hurt. Your paperwork may carry both.

[Your pathology report](/tests/pathology-report) goes through the document
itself. It is written for reports that carry a grade and come from a piece of
tissue, and yours may have neither.

## How is it usually treated?

**Three routes, and which one you get depends mostly on whether the tumor makes a
hormone, and which.**

**Pills, for some.** A tumor making prolactin is usually treated with medicine
rather than surgery. The Endocrine Society says these tumors are mostly treated
successfully that way. The pills lower the hormone and often shrink the growth,
and the common side effects are feeling sick and feeling dizzy. Surgery comes up
if the medicine does not work.

**Watching, for some.** If the tumor is small, not growing and not causing
symptoms, your team may follow it with scans and blood tests instead of treating
it. [Watching a tumor, step by step](/treatments/watch-and-wait) covers what that
involves and why it is a plan rather than a delay.

**Surgery, for some.** It is usually advised when the tumor is affecting your
sight or threatens to, when it is pressing on other nerves, when it is growing,
or when it is causing headaches.

[Radiation](/treatments/radiation-therapy) is used less often here, usually when
some tumor is left behind or it comes back.

## What is treatment actually like, and what is normal afterwards?

### The operation goes through your nose

**It is not like the brain operations described elsewhere on this site.**

The surgeon reaches the gland through your nose and the sinus behind it. There is
no cut in your scalp and no shaved head. The route is called
**transsphenoidal**, after the sinus it passes through.

Afterwards, expect your nose to feel blocked and sore for a while, the way a bad
cold feels. Tell your team about clear fluid running from your nose, which needs
checking.

For a few people the tumor is too large or awkward for that route, and the
surgeon goes through the scalp instead. Your team will say which one is planned
and why.

[What comes after an operation](/treatments/craniotomy) covers the general
aftercare, though the nose route is gentler than the operation that page
describes.

### Hormones afterwards

Some people need hormone pills afterwards, sometimes for good, because the
gland cannot make enough on its own. That is not a complication that went wrong.
It is a known outcome and it is treatable.

If one of those is a steroid replacement, read the emergency rule further up this
page again, and ask for your own written plan before you leave.

**Tell your team if you are very thirsty and passing a lot of water.** That often
follows an operation on the pituitary. It can be arginine vasopressin deficiency,
the second of the renames above, and there are other reasons for it too, which is
why it is worth saying rather than working it out at home. Either way it is
treated.

Sight often improves once the pressure is off, and your team will check it after
the operation.

## Everyday life

Day to day, life here turns on your hormones and your sight, the same as the rest
of this page.

Two things are worth planning for. If you take hormone replacement, you need a
routine for it and a plan for days when you are ill. And if your sight changed,
have it checked properly before you drive, because the part that goes first is
the part you use at intersections.

What you are allowed to drive, and when, is set locally. Your team knows the
rules where you live; a website does not, including this one.

## Follow-up scans, and what to do while you wait

Follow-up here is scans and blood tests together, and often a sight test as well.

Scans continue for years and spread further apart if nothing changes.
[Follow-up scans](/tests/follow-up-scans) explains how one scan is read against
the last. [Waiting for results](/tests/waiting-for-results) is about the days in
between.

Ask your team for your own timetable, and ask what would make it change.

## If it comes back

Some tumors regrow, and there is usually something to do about it. The choices
are the same three as before: pills, another operation, or radiation. Which one
depends on what was used the first time and what is left.

Ask what each choice is meant to achieve, and make notes while you are being
told.

## Did I cause this?

**What follows is written about brain tumors in general.** This tumor is not
one, as the top of this page says. But the question is the same, and so are the
answers. It is worth reading as yours.

[CAUSES]

## What might happen over time

:::outlook
This question usually gets answered with numbers. For this tumor they are not the
frightening kind people expect after the word "tumor". Even so, they describe
groups of people treated years ago, not you.

Doctors talk about the **median**. Sort everyone with this tumor by how they did,
and the median is the halfway mark. Half sit on each side of it. It tells you
nothing about which side you are on.

What actually shapes things here is narrower than a number: whether the tumor is
making a hormone, whether it is pressing on your sight, and whether the gland
still works. All three are things your team can measure and act on.

Ask when you want to know. Choosing not to is also a choice, and it is one you
can change later.
:::

## For the person caring for someone with this

[CAREGIVER]

Two things are specific to this tumor.

**Learn the two emergency rules with them.** A sudden, severe headache, and being
ill while on steroid replacement. You may be the one who notices first, and
neither can wait for morning.

**Hormone changes can look like mood or personality.** Tiredness, low mood and
loss of interest can all come from a gland that is not working, not from someone
giving up. It is worth saying to the team rather than absorbing at home.

## What to ask your team

- Is my tumor making a hormone, and which one?
- Has my sight been checked, and when will it be checked again?
- Is the plan pills, surgery, or watching, and why that one?
- My tumor makes a hormone. Which of those three does that point to for me?
- If it is surgery, will it be through the nose?
- Will I need hormone replacement afterwards?
- If I need steroid replacement, what is my plan for days when I am ill?
- Should I carry an emergency injection or wear a medical alert bracelet?
- What would make you want me to call before the next appointment?
- Who do I call after hours, and what is the number?

## Where to get support

- [Getting help now](/get-help-now), for the day when you need a person rather
  than a page.
- The **Pituitary Society** publishes patient material on these tumors and the
  conditions they cause.
- The **Endocrine Society** has a patient library covering the hormone side, and
  a directory for finding a hormone specialist.
- The specialist nurse on your team. Most people find that is the fastest route
  to a real answer.
