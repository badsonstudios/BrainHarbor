---
sources:
  - url: https://www.cdc.gov/epilepsy/first-aid-for-seizures/index.html
    title: "First Aid for Seizures | Epilepsy | CDC"
    accessed: 2026-09-08
  # Cited for the sudden-and-complete deficit line, which is the one place this
  # block escalates a symptom its same-day tier also names. CDC lists sudden
  # trouble speaking, sudden one-sided weakness and sudden trouble seeing
  # together and says "Call 9-1-1 right away". /treatments/craniotomy already
  # draws the same line ("They suddenly cannot speak, move, or see"); before
  # WI-563 the hubs did not, which was the under-triage direction.
  - url: https://www.cdc.gov/stroke/signs-symptoms/index.html
    title: "Signs and Symptoms of Stroke | Stroke | CDC"
    accessed: 2026-09-08
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging/signs-and-symptoms.html
    title: "Signs and Symptoms of Brain Tumors in Adults | American Cancer Society"
    accessed: 2026-09-08
  # Cited ONLY for the headache that wakes you from sleep. The ACS page above
  # carries every other same-day symptom but not that one, and it is the red
  # flag a reader is most likely to dismiss at three in the morning.
  - url: https://www.cancerresearchuk.org/about-cancer/brain-tumours/symptoms
    title: "Brain tumour symptoms | Brain neoplasm symptoms"
    accessed: 2026-09-08
  # The two treatment safety rules this block owes the pages it routes into
  # (§12.11). Both are CONDITIONAL, which is what lets them sit in a block on
  # every hub: a reader who is not having chemotherapy and has not had surgery
  # is not addressed by either, and neither is false on any hub.
  #
  # The thresholds themselves are NOT published here. /treatments/chemotherapy
  # owns the fever number and says teams differ; UMass gives a different one
  # again. One threshold, one page, and this block routes to it.
  - url: https://www.cancer.org/cancer/managing-cancer/side-effects/low-blood-counts/neutropenia.html
    title: "Low White Blood Cell Counts | Neutropenia | American Cancer Society"
    accessed: 2026-09-08
  - url: https://www.cancer.gov/about-cancer/treatment/side-effects/infection
    title: "Infection and Neutropenia and Cancer Treatment - Side Effects - NCI"
    accessed: 2026-09-08
  # WI-565: THE TIMING HALF OF THE FEVER CONDITIONAL, which the two sources
  # above do not carry. Neither says anything about when in the day to call -
  # ACS is "Call your cancer care team or get medical help", NCI is "Call your
  # health care team if you have signs of an infection". So "at any hour, not
  # waiting for the morning" was this block's own claim with nothing behind it,
  # on every hub it composes onto. Cited HERE and not only on the sibling
  # because a block's citations are the block's (§12.31).
  - url: https://www.cdc.gov/cancer-preventing-infections/patients/index.html
    title: "Information for Patients Who Are Getting Chemotherapy | Preventing Infections in Cancer Patients | CDC"
    accessed: 2026-10-14
    # Verbatim: "Call your doctor right away, even if this happens in the middle
    # of the night." / "This is considered an emergency. Don't wait until
    # morning." The THRESHOLD is deliberately still not published here -
    # /treatments/chemotherapy owns the number and says teams differ.
  - url: https://www.cdc.gov/cancer-preventing-infections/patients/fever.html
    title: "Watch Out for Fever | Preventing Infections in Cancer Patients | CDC"
    accessed: 2026-10-14
    # For the fallback this tier was missing, and for the check-in instruction on
    # the sibling. Under the heading "What should I do if I think I have an
    # infection?" (recorded because the three quotes WI-565 uses are not in page
    # order, and a later re-fetcher should not read that as a misattribution).
    # Verbatim: "If you go to the emergency room, tell the person checking you in
    # that you are a cancer patient undergoing chemotherapy. If you have a fever,
    # you might have an infection. This is a life-threatening condition, and you
    # should be seen quickly."
    #
    # THE FALLBACK IS THE REAL WI-565 DEFECT, not the citation. Every other
    # "at any hour" in the corpus offered a second route - the shunt tier 14
    # lines below says "call your team, or go to the emergency department", and so
    # do blocks/spinal-cord.md, /treatments/craniotomy and /treatments/shunts. The
    # chemotherapy fever rule, the most time-critical of them, offered none, on
    # this block and in both places on the sibling. "Call your team at any
    # hour" is an assumption about the reader's health system, and because
    # nobody could source it, nobody wrote the branch for the reader at two in
    # the morning whose team does not answer (§12.12: the over-reassuring
    # direction is the more dangerous one).
    #
    # THE FALLBACK SAYS "emergency department", matching the shunt tier below it,
    # and that is NOT the term the rest of the corpus reaches for. Measured at
    # WI-565: "emergency room" 36 times in reader-facing body text against
    # "emergency department" 7 - and those 7 ARE these fallback instructions, so
    # the minority term is exactly the safety-instruction term. (Counting YAML
    # comments too it is 68 against 14, which is the figure /review corrected:
    # half the 14 are commentary ABOUT the instructions.) CDC uses "emergency
    # room" in the sentence quoted above, and /start.md - the front door - uses
    # it in the site's own 911 line.
    #
    # The term was changed to "emergency room" here and then CHANGED BACK, on
    # purpose. The shunt instruction is a sourced safety line pinned byte-for-byte
    # across THREE surfaces (this block, /treatments/shunts,
    # /treatments/craniotomy) by ShuntsPageTests.Instruction, so aligning the
    # term means editing a different safety rule inside an item about the
    # chemotherapy fever clause - §12.21's blast-radius argument. One instruction
    # in two wordings inside ONE block would be a §12.10 defect, so the new
    # fallback matches its neighbour instead. The corpus-wide split is handed to
    # /pm and recorded in §12.41.
    #
    # NOT CITED FOR THE SHUNT TIER'S VENUE. /review checked the claim that the
    # Hydrocephalus Association says "emergency room" there and it is wrong in a
    # way worth recording: HA attaches "go to the emergency room" to suspected
    # INFECTION and gives suspected MALFUNCTION "seek immediate medical
    # assistance" with no venue named (/tumors/atrt's front matter keeps the two
    # apart deliberately). This block's shunt tier is the malfunction rule, and
    # the source line 40 lines below quotes HA as "see your doctor or go to the
    # emergency department". So the term argument above rests on CDC and on the
    # corpus, not on HA.
  - url: https://www.cancerresearchuk.org/about-cancer/treatment/drugs/pcv
    title: "Procarbazine, lomustine and vincristine (PCV) | Cancer Information | Cancer Research UK"
    accessed: 2026-09-08
  # WI-534: the THIRD conditional rule, for a reader with a shunt. This block's
  # same-day tier is right for a reader without one and under-triages one with
  # one: NINDS says "seek medical help immediately" and the Hydrocephalus
  # Association says "see your doctor or go to the emergency department". The
  # full list lives on /treatments/shunts#warning-signs; this block routes to it.
  - url: https://www.ninds.nih.gov/health-information/disorders/hydrocephalus
    title: "National Institute of Neurological Disorders and Stroke: Hydrocephalus"
    accessed: 2026-09-15
  - url: https://www.hydroassoc.org/complications-of-shunt-systems/
    title: "Hydrocephalus Association: Complications of Shunt Systems - Signs and Symptoms"
    accessed: 2026-09-15
  - url: https://www.ummhealth.org/health-library/discharge-instructions-for-craniotomy
    title: "Discharge Instructions for Craniotomy | UMass Memorial Health"
    accessed: 2026-09-08
---

### When to call for help right now

Some things should not wait for the next appointment.

**Call an ambulance for any of these.** Call **911**, or your local emergency
number:

- A **first ever seizure**.
- A seizure that lasts more than five minutes.
- One seizure right after another, with no waking up in between.
- Someone who cannot be woken.
- Trouble breathing, or someone who seems to be choking.
- Suddenly not being able to speak, move one side, or see.

A seizure like the ones you usually have, that stops on its own, is not an
ambulance call. [What to do during a seizure](/seizures/what-to-do) draws the
line.

**Call your team the same day for any of these:**

- A headache much worse than usual, or one that wakes you from sleep.
- Throwing up again and again.
- New weakness in the face, an arm or a leg.
- New trouble speaking, or understanding what people say.
- New confusion.
- Sleeping much more than being awake.
- A sudden change in your vision.

**If you are having chemotherapy, a fever is its own rule.** It means calling
your team right away, at any hour, not waiting for the morning. If you cannot
reach anybody, go to the emergency department and say you are having
chemotherapy. An infection during chemotherapy can get dangerous fast, and it is
treatable when it is caught early.
[The fever rule](/treatments/chemotherapy#fever-rule) explains why, and what
number your team will give you.

**In the weeks after brain surgery, a fever is its own rule too**, especially
with a stiff neck or a bad headache.
[What comes after a craniotomy](/treatments/craniotomy) has the rest of that
list.

**If you have a shunt, the signs it has stopped working are their own rule.** For
you, everything on the same-day list above is a right-away call instead, and so
are a few other signs. Get help right away, at any hour: call your team, or go to
the emergency department.
[Signs a shunt is not working](/treatments/shunts#warning-signs) has the full
list.

**If you are not sure whether to call, call.** Nobody on that team minds a
phone call that turns out to be nothing.

**Same day means today, not the next clinic appointment.** Ask your team now for
the after-hours number, and put it somewhere you will find it at two in the
morning rather than somewhere sensible.

[What to do during a seizure](/seizures/what-to-do) has the step-by-step, and
[getting help now](/get-help-now) is the short version to keep on a phone.
