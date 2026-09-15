---
title: "Shunts and hydrocephalus: when fluid cannot drain"
slug: treatments/shunts
description: "A tumor can stop the fluid around the brain from draining. A shunt is a thin tube that drains it somewhere else. What the operation involves, what recovery is like, why some people need it fixed or replaced, and the signs it has stopped working, which need help right away."
tags: [treatments, surgery, shunt, hydrocephalus]
sources:
  # SOURCE DISCIPLINE FOR THIS PAGE, written down so it is not re-argued
  # (content-pipeline §12.10, §12.13, §12.14).
  #
  # (1) THE CORPUS ALREADY OWNS THE DEFINITION, AND THIS PAGE DOES NOT RESTATE
  # IT. blocks/mechanism.md (eight hubs) says a tumor can block the fluid and
  # "the usual treatment is a shunt". That block routes here. This page never
  # calls a shunt the only treatment, and never repeats the block's sentence.
  #
  # (2) THIS PAGE OWNS THE SHUNT ESCALATION TIER, AND FOUR OTHER SURFACES ROUTE
  # TO IT WITH THE SAME INSTRUCTION. For a reader WITH a shunt, NINDS: "People
  # should seek medical help immediately if symptoms develop that suggest the
  # shunt system is not working properly"; the Hydrocephalus Association: "It's
  # crucial to see your doctor or go to the emergency department, even if your
  # symptoms don't appear directly linked to hydrocephalus or the shunt." So:
  #   * blocks/escalation.md carries a CONDITIONAL after both fever rules: for a
  #     shunt reader, everything on its same-day list is a right-away call
  #     instead. /review (WI-534) showed why the WHOLE list and not three named
  #     signs: a shunt reader with new confusion was otherwise still same-day.
  #   * /treatments/craniotomy's caregiver lists carry the same line, because the
  #     likeliest shunt reader is recovering from tumor surgery and that page
  #     does not include the block.
  #   * blocks/mechanism.md's raised-pressure paragraph ("worth a phone call
  #     rather than a wait") carries a one-clause shunt exception.
  #   * The instruction sentence is identical on all four and on this page, and
  #     it is in the restatement allowlists.
  #   * RAISED, AND RECORDED AS A RULING: AANS lists fever and abdominal pain as
  #     things to contact a physician about, without saying how fast. They are on
  #     this page's right-away list with everything else, on the Hydrocephalus
  #     Association's instruction to be seen "even if your symptoms don't appear
  #     directly linked". Raising a sign is the safe direction; lowering one is
  #     not.
  #   * THE AMBULANCE CASES ARE STATED HERE, not pointed at. A first draft said
  #     "Get help now keeps that list"; /get-help-now has no such list (/review).
  #     The sentence uses the same cases as /treatments/craniotomy's ambulance
  #     list and blocks/escalation.md, sourced to CDC below.
  #   * The Walton Centre (UK) routes shunt concerns to its nurse specialists in
  #     office hours and otherwise to a GP or the emergency department. That is
  #     UK service design and is not published; it does not contradict the tier.
  #
  # (3) THE DOSSIER'S PATIENT SOURCE IS A CONTENT SITE AND ITS FAILURE FIGURE IS
  # NOT IN ITS PAPER. docs/research/tumor-guides/treatment-library.md §13 sources
  # the procedure, the risks and the warning signs to healthline.com ("replace
  # before publishing"), and cites NBK66023 (NCI patient PDQ). Neither is used.
  #   * Its "27.8% shunt failure; 13% single, 14% multiple revisions" is
  #     attributed to PMC8976775, which says "Twenty-eight patients (33%) had one
  #     or more shunt failures ... 1-, 5-, and 10-year shunt success rates of 77%,
  #     71%, and 67%". NONE of these is published (§12.4 R2).
  #   * PMC6257011 was cited by a first draft for "the blocked kind usually came at
  #     diagnosis". /review: that sentence is the paper's BACKGROUND, not its
  #     finding. Dropped, with the source.
  #
  # (4) TWO DISAGREEMENTS, PRINTED RATHER THAN SPLIT (§12.8, WI-511).
  #   * How long the operation takes: ACS "about an hour"; MedlinePlus "about 1
  #     1/2 hours".
  #   * Whether it is for good: ACS "Shunts can be temporary or permanent"; the
  #     Hydrocephalus Association, about hydrocephalus in general, "you will need
  #     it for the rest of your life".
  #
  # (5) NUMBERS, each attributed in the sentence that prints it (§12.8, WI-507):
  # about an hour / about an hour and a half (ACS / MedlinePlus); one to three
  # days in hospital (ACS); three to six weeks to feel stronger (the Walton
  # Centre, "you should feel stronger"); within about five weeks OF THE SHUNT
  # GOING IN (PMC8827213, "Median time from shunt insertion to shunt failure was
  # 20 days (range 1-35)" among 90-day failures -- measured from insertion, which
  # was itself a median 1.9 months after surgery, so the page says "of going
  # in"); two inches from a magnet (FDA); one to three months for infection and
  # 10 to 14 days of antibiotics (Hydrocephalus Association). OUT: every failure,
  # revision, infection and survival share; the ETV success figure; the Walton
  # Centre's return-to-work week, stitch days and "up to a week" of soreness.
  #
  # (6) INFERENCES, HEDGED AND RECORDED (§12.8, WI-532).
  #   * "Someone who is getting very drowsy may not be the one who notices" --
  #     from NINDS's sleepiness and the Hydrocephalus Association's "Difficulty
  #     waking up or staying awake". Hedged with "may".
  #   * "the staff may want to know what kind of shunt it is" (emergency
  #     department) -- NHS GGC says scan staff need make and model; the emergency
  #     case is an inference and says "may".
  #   * NHS GGC MRI Physics is guidance written for scanning staff, cited only
  #     for what those staff need, not as patient advice.
  #
  # (7) GLOSSARY. `hydrocephalus` has an entry (StatPearls NBK560875) that fires
  # on /tests/ct-scan; this page defines the word, so the tooltip is suppressed
  # here and the entry is kept. No new entry: `shunt` is defined inline by
  # blocks/mechanism.md; `endoscopic third ventriculostomy` appears on this page
  # only (§12.8, WI-519).
  - url: https://www.ninds.nih.gov/health-information/disorders/hydrocephalus
    title: "National Institute of Neurological Disorders and Stroke: Hydrocephalus"
    accessed: 2026-09-15
    # "People should seek medical help immediately if symptoms develop that
    # suggest the shunt system is not working properly. Signs and symptoms of a
    # shunt not working may include: Headache / Double vision or sensitivity to
    # light / Nausea or vomiting / Soreness of the neck or shoulder muscles /
    # Seizures / Redness or tenderness along the shunt tract / Low-grade fever /
    # Sleepiness or exhaustion / Hydrocephalus symptoms coming back".
    # Hydrocephalus symptoms, older children and adults: "Headache / Blurred or
    # double vision". Infants: "A rapid increase in head size / ... A bulge on
    # the soft spot (fontanel) on the top of the head / Vomiting / ... Sleepiness
    # / Irritability / Eyes that are fixed downward ('sun setting')".
    # "A person may need multiple surgeries to repair or replace a shunt
    # throughout their lifetime."
    # ETV: "A doctor makes a tiny hole at the bottom of the third ventricle".
  - url: https://www.hydroassoc.org/complications-of-shunt-systems/
    title: "Hydrocephalus Association: Complications of Shunt Systems - Signs and Symptoms"
    accessed: 2026-09-15
    # "Complications can arise from various factors, including obstruction
    # (blockage), infection, displacement, or mechanical failures."
    # Children and adults: "Headaches / Nausea and/or vomiting / Excessive
    # tiredness / Difficulty waking up or staying awake (this symptom requires
    # urgent attention as it can potentially lead to a coma) / Vision problems
    # (such as double or blurred vision) / Irritability (Changes in behavior or
    # personality) / Decline in academic or job performance / Loss of
    # coordination or balance / Difficulty concentrating or focusing / Return of
    # pre-treatment problems"; also "feeling withdrawn or talking less", "Fluid
    # leaking from the shunt incision sites", "Signs of infection, such as fever,
    # redness, or swelling at the shunt site"; infants "Sunsetting Eyes (A
    # downward deviation of eyes, where the white part of the eye may be seen
    # above the iris)" and "Fussiness".
    # "It's crucial to see your doctor or go to the emergency department, even if
    # your symptoms don't appear directly linked to hydrocephalus or the shunt."
    # "If the shunt suddenly has a problem, serious symptoms might emerge rapidly".
    # Infection "is most likely seen one to three months after surgery"; "the usual
    # course of action is surgery to remove all parts of the shunt", a temporary
    # External Ventricular Drain, antibiotics "for approximately 10-14 days", then
    # "a new shunt is surgically implanted."
  - url: https://www.hydroassoc.org/overdraining-underdraining/
    title: "Hydrocephalus Association: Overdraining and Underdraining with Hydrocephalus"
    accessed: 2026-09-15
    # Overdraining: "Severe headaches that worsen when sitting or standing and
    # improve when lying down." Underdraining: "especially in the morning or after
    # lying down".
  - url: https://www.hydroassoc.org/shunt-systems/
    title: "Hydrocephalus Association: Understanding Shunt Systems"
    accessed: 2026-09-15
    # "you will need it for the rest of your life" (ruling (4)); "The tubing and
    # valve are under the skin and hair, so typically unseen."; "two types of
    # valves: fixed and adjustable (programmable)"; adjustable valves "can be
    # adjusted by your doctor using an external adjustment tool" and "some of
    # these valves may be sensitive to environmental magnets."
  - url: https://www.aans.org/patients/conditions-treatments/hydrocephalus/
    title: "American Association of Neurological Surgeons: Hydrocephalus"
    accessed: 2026-09-15
    # "A limited number of patients can be treated with an alternative operation
    # called endoscopic third ventriculostomy." / "Follow-up diagnostic tests,
    # including CT scans, MRIs and x-rays, are help determine if the shunt is
    # working properly." / report "Redness, tenderness, pain or swelling of the
    # skin along the length of the tube or incision / Irritability or drowsiness /
    # Nausea, vomiting, headache or double vision / Fever / Abdominal pain / Return
    # of preoperative neurological symptoms" / "When a shunt malfunctions, surgery
    # is often needed to replace the blocked or malfunctioning portion of the
    # shunt system."
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/treating/surgery.html
    title: "American Cancer Society: Surgery for Brain Tumors in Adults"
    accessed: 2026-09-15
    # "Removing the tumor can often help with this, but there are also other ways
    # to drain away excess CSF"; "You might have a small tube (called a drain)
    # coming out of the incision to allow excess cerebrospinal fluid (CSF) to
    # leave the skull."; the shunt drains "into the abdomen ... or, less often,
    # into the heart"; "Placing a shunt normally takes about an hour."; "The
    # hospital stay after shunt procedures is typically 1 to 3 days, depending on
    # the reason it is placed and the person's general health."; "Shunts can be
    # temporary or permanent."; ETV is "Another option ... in some cases".
  - url: https://medlineplus.gov/ency/article/003019.htm
    title: "MedlinePlus Medical Encyclopedia: Ventriculoperitoneal shunting"
    accessed: 2026-09-15
    # The encyclopedia, not a drug monograph (PLAN.md §5 does not apply). "The
    # surgeon makes a skin incision behind the ear. Another small surgical cut is
    # made in the belly."; "a few more small cuts, for instance in the neck or
    # near the collarbone"; "A valve is placed underneath the skin, usually behind
    # the ear."; "It takes about 1 1/2 hours."; "The person may need to lie flat
    # for 24 hours the first time a shunt is placed."; risks "Blood clot or
    # bleeding in the brain / Brain swelling / Hole in the intestines (bowel
    # perforation), which can occur later after surgery / Leakage of CSF under the
    # skin / Infection of the shunt, brain, or in the abdomen / Damage to brain
    # tissue / Seizures"; "Follow your surgeon's instructions about how to take
    # care of the shunt at home."
  - url: https://www.ncbi.nlm.nih.gov/books/NBK560875/
    title: "Hydrocephalus - StatPearls - NCBI Bookshelf"
    accessed: 2026-09-15
    # "Obstructive hydrocephalus develops from a block in CSF pathways." /
    # "Communicating hydrocephalus is caused by impaired absorption of CSF."
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC8827213/
    title: "Risk of early failure of VP shunts implanted for hydrocephalus after craniotomies for brain tumors in adults (Neurosurg Rev)"
    accessed: 2026-09-15
    # Oslo, population-based, no declared conflict. "Median time from shunt
    # insertion to shunt failure was 20 days (range 1-35)." See ruling (5).
  - url: https://www.fda.gov/medical-devices/cerebral-spinal-fluid-csf-shunt-systems/magnetic-field-interference-programmable-csf-shunts
    title: "FDA: Magnetic Field Interference with Programmable CSF Shunts"
    accessed: 2026-09-15
    # "The FDA suggests keeping products that contain magnets two or more inches
    # away from the location of magnetic externally programmable CSF shunt
    # valves."; "Patients are advised to use the ear opposite the shunt for
    # devices requiring listening (such as cell phones and earbuds)."; "The FDA
    # does not currently advise patients ... to avoid using specific electronic
    # devices."
  - url: https://www.thewaltoncentre.nhs.uk/ventriculoperitoneal-shunt-post-operative-information/
    title: "The Walton Centre: Ventriculoperitoneal shunt post-operative information"
    accessed: 2026-09-15
    # A UK neurosurgery center, attributed as one wherever it is used. "You may
    # be surprised at how tired you feel once you are at home." / "After about
    # three to six weeks, you should feel stronger" / "you may be able to feel the
    # valve of your shunt behind your ear. You may, if you are thin, be able to
    # feel the tubing from the shunt in your neck." / the stomach wound:
    # "numbness", "altered sensation (feeling) around your wound, often described
    # as shooting pain", "itchy" / programmable valves "can be altered by large
    # magnets such as MRI".
  - url: https://www.mriphysics.scot.nhs.uk/implant-safety-policies/shunts/
    title: "NHS GGC MRI Physics: CSF Shunts"
    accessed: 2026-09-15
    # Staff guidance, see ruling (6). "Programmable CSF shunts - they require make
    # and model identification for the MR Conditions to be determined."
  - url: https://www.cdc.gov/epilepsy/first-aid-for-seizures/index.html
    title: "First Aid for Seizures | Epilepsy | CDC"
    accessed: 2026-09-15
    # The seizure ambulance cases, as in blocks/escalation.md, which cites it.
  - url: https://www.cdc.gov/stroke/signs-symptoms/index.html
    title: "Signs and Symptoms of Stroke | Stroke | CDC"
    accessed: 2026-09-15
    # Sudden trouble speaking, one-sided weakness or trouble seeing: "Call 9-1-1
    # right away", as in blocks/escalation.md.
reviewed: 2026-09-15
review_due: 2027-03-15
disclaimers: [medical]
---

## The short version {#short-version}

A tumor can block the fluid that flows around the brain. When that happens, the
fluid builds up and presses on the brain. An operation can put in a shunt, which
drains the extra fluid to another part of the body. Once you have one, the most
useful thing to know is the list of signs that it has stopped working, because
those need help right away. Your team can tell you whether yours is likely to be
for a while or for good.

## What is it? {#what-it-is}

!%hydrocephalus%A shunt is a thin, soft tube that drains extra fluid away from the brain.

Your brain sits in a clear fluid. The fluid is made inside the brain, flows
through small spaces, and drains away. When it builds up because it cannot
drain, the name for that is **hydrocephalus**.

The top end of the tube sits in one of those fluid spaces. A small valve sits
under the skin, usually behind the ear, and lets fluid out when the pressure is
too high. The rest of the tube runs under the skin, down the neck and chest, into
the belly, where the body soaks the fluid up. Less often, it goes to the heart
instead.

Once it is in, the whole shunt is under the skin, so it usually cannot be seen.

## Why would I need one? {#why}

There are two ways the build-up can happen.

- **Something blocks the way out.** The fluid is made as usual, but a tumor keeps
  it from getting through. Doctors call this obstructive.
- **The fluid is not soaked back up.** Nothing is blocking it, but the body does
  not take it back in as it should. Doctors call this communicating.

**A shunt is not the only answer.** The American Cancer Society says taking the
tumor out can often help, and that there are other ways to drain the fluid too.
One is a temporary drain, a small tube that lets the fluid out of the head for a
while. Another is an operation called an **endoscopic third ventriculostomy**, or
ETV, which makes a small new path for the fluid inside the brain instead of using
a tube. The American Association of Neurological Surgeons says it suits a limited
number of people. Ask which of these fits you, and why.

## What happens, step by step {#step-by-step}

1. **You are asleep for the operation.**
2. **A small cut is made on your head**, usually behind the ear, and another on
   your belly. There may be a few more small cuts on the way, in the neck or near
   the collarbone.
3. **The top end of the tube is passed into a fluid space in the brain**, through
   a small opening in the skull.
4. **The valve goes in under the skin**, usually behind the ear.
5. **The rest of the tube is passed under the skin** down to the belly.
6. **You wake up in a recovery area**, then move to a hospital room. You may need
   to lie flat for the first day.

## How long does it take? {#how-long}

**The operation takes about an hour to an hour and a half.** The American Cancer
Society says about an hour, and MedlinePlus says about an hour and a half.

The American Cancer Society says the hospital stay is usually one to three days,
depending on why you needed it and how well you are otherwise.

Recovery takes longer than the stay. One UK neurosurgery center tells its
patients they should feel stronger after about three to six weeks.

## What does it feel like? {#what-it-feels-like}

Sore and tired at first.

**The tiredness can be a surprise.** The same UK center warns people they may be
surprised at how tired they feel once they are home.

**The cuts are sore for a while.** The skin around the belly cut can feel numb or
itchy, or have shooting pains, as it heals.

**You may be able to feel it.** Once the swelling goes down, you may feel the
valve under the skin behind your ear. If you are thin, you may feel the tube in
your neck. That is normal.

**Normal recovery gets slowly better.** A headache that is getting worse,
sleepiness that is getting deeper, or pain that keeps growing is not recovery.
It is a warning sign, and the list is further down this page.

## Is it for good? {#is-it-for-good}

**The American Cancer Society says shunts can be temporary or permanent.** The
Hydrocephalus Association, writing about hydrocephalus in general, says you will
need one for the rest of your life. Ask your team which yours is likely to be.

**A shunt can stop working.** The National Institute of Neurological Disorders and
Stroke says a person may need several operations over their lifetime to repair or
replace one.

**The weeks after it goes in are worth watching.** In one study in Norway, of
adults who had a shunt put in after brain tumor surgery, the shunts that stopped
working in their first three months all did so within about five weeks of going
in. Keep the warning signs somewhere you will see them during that time.

## Signs a shunt is not working {#warning-signs}

**If you have a shunt and any of these happen, get help right away, at any hour:
call your team, or go to the emergency department.**

- A headache, especially a new one or one that is getting worse.
- Nausea or throwing up.
- Getting sleepier, or being hard to wake or to keep awake.
- Confusion, or trouble thinking or concentrating.
- Blurred or double vision, or light hurting your eyes.
- Losing your balance, or being clumsier than usual.
- A sore neck or sore shoulders.
- A seizure.
- Changes in mood or behavior, or seeming withdrawn.
- Redness, soreness or swelling along the line of the tube, or fluid leaking from
  a cut.
- A fever.
- Pain in your belly.
- The problems you had before the shunt coming back.
- In a baby: a bulging soft spot on the top of the head, a head that is growing
  fast, eyes fixed downward so the white shows above the colored part, or being
  unusually fussy or sleepy.

**Some things are an ambulance call, shunt or not.** Call **911**, or your local
emergency number, if someone cannot be woken, is not breathing properly, has a
first seizure or one that lasts more than five minutes, or suddenly cannot speak,
move, or see. [What to do when someone has a seizure](/seizures/what-to-do) has
the seizure steps.

**If you are not sure, go.** The Hydrocephalus Association says to see a doctor
or go to the emergency department even when the symptoms do not seem to be about
the shunt. Some problems come on slowly and some come on fast.

**When you call, say what the headache is like.** A headache that gets worse when
you sit up or stand, and eases when you lie down, can mean the shunt is draining
too much. One that is worst in the morning or after lying down can mean it is
draining too little. Either kind still means getting help right away.

## Other risks of the operation {#risks}

Like any brain operation, it has risks. They include bleeding or a clot in the
brain, swelling, infection, fluid leaking under the skin, damage to brain tissue,
and seizures. Later on, the tube in the belly can also make a hole in the bowel.

Ask your surgeon how likely each one is for you.

## Living with a shunt {#living-with-it}

**Tell anyone doing an MRI that you have a shunt, and which kind.** Some valves
are fixed. Others are adjustable, which means a doctor can change the setting
from outside the head with a magnet. The magnet in an MRI can change an
adjustable valve's setting, so ask whether yours needs checking afterwards. Scan
staff need its make and model to know how to scan you safely.

**Keep magnets a little way from an adjustable valve.** The FDA suggests keeping
things with magnets in them at least two inches from the valve, such as phones,
tablets, headphones and earbuds, and using the ear on the other side for calls
and earbuds. It does not tell people to stop using them.

**Ask your team about everyday things**, like showers, baths, swimming, flying and
sports, and how to look after the cuts while they heal.

## What you need first {#what-you-need-first}

- **Why you need it**, and whether anything else would do.
- **The details of your shunt and valve**, written down before you leave the
  hospital.
- **Your team's after-hours number.**
- **The warning signs**, where everyone at home can see them.

## For the person alongside them {#caregiver}

[CAREGIVER]

A shunt is small, and keeping an eye on it is a big part of the job.

**Learn the warning signs as well as the person with the shunt does.** Someone who
is getting very drowsy may not be the one who notices, or the one who calls.

**Look at the cuts in the first few months.** The Hydrocephalus Association says
infections are most likely one to three months after the operation. Redness,
swelling or fluid leaking along the tube needs help right away.

**With a baby or young child, go by the change you see.** A child may not be able
to say anything is wrong, so fussiness, sleepiness or losing a skill they had
counts as a warning sign.

**Keep the shunt's details with you when you go out.** If you end up in an
emergency department, the staff may want to know what kind of shunt it is.

## Why do I need another operation? {#another-operation}

**Because a shunt can get blocked, get infected, move, or break.** Fixing it often
means another operation. Sometimes only the part that stopped working is replaced,
and sometimes the whole shunt is.

**An infected shunt is usually taken out.** The Hydrocephalus Association describes
a temporary drain while antibiotics clear the infection, for about 10 to 14 days in
the hospital, and then a new shunt.

Ask your team what the plan would be if yours ever needed changing.

## Who decides, and how will I hear? {#who-decides}

**Your neurosurgery team decides whether you need a shunt, and which kind.**

**After it is in, scans and X-rays are how they check it is working.** Ask how
often you will be seen, and how you will hear the results.

## What to ask your team {#questions}

- Why do I need a shunt, and is it likely to be for a while or for good?
- Could a temporary drain or an ETV do the job instead?
- What kind of valve is it, and is it adjustable?
- Where will the tube run, and what will I be able to feel?
- Who do I call at night if something on the warning list happens?
- Does it change anything about MRI scans, phones or headphones?
- What about showers, swimming, flying and sports?
- How often will it be checked, and how?

## Where to go next {#where-to-go-next}

- [Brain surgery](/treatments/craniotomy) for the operation to take a tumor out.
- [MRI](/tests/mri) for what to tell the scan staff.
- [CT scan](/tests/ct-scan) for a scan used to look at the fluid spaces.
- [Follow-up scans, and what the results mean](/tests/follow-up-scans) for how
  scans are read over time.
- [What to do when someone has a seizure](/seizures/what-to-do) for the seizure
  steps.
- [Get help now](/get-help-now) if you need to talk to a person today.
