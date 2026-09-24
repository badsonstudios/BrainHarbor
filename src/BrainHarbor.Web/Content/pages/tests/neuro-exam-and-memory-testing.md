---
title: "The exam in the room, and the thinking and memory tests"
slug: tests/neuro-exam-and-memory-testing
description: "Two different things that get talked about in the same breath: the short exam your team does at every visit, and the longer day of thinking and memory tests. What each one is, how long each takes, and why neither is something you can fail."
tags: [tests, neurological-exam, thinking-and-memory, newly-diagnosed]
sources:
  # WI-549. Every quote below was re-fetched and read LIVE on 2026-09-23 with
  # work_files/wi549/verify.py, not taken from the research dossier. §12.8
  # ("a dossier is not a source") and the re-fetch rule: a URL that resolves is
  # not a citation that holds.
  #
  # DELIBERATELY NOT CITED, because they could not be read:
  #   hhs.gov HIPAA "employers and health information in the workplace" returned
  #   HTTP 403 on two separate attempts. It was the only candidate source for
  #   "an employer cannot get your report without your authorization", so that
  #   claim is NOT ASSERTED anywhere on this page. It appears only as a question
  #   in "What to ask your team", which asserts nothing.
  #   PMC11443061 served a challenge page and Europe PMC returned 500.
  #   ninds.nih.gov, ascopubs.org EDBK_320813, StatPearls NBK557589 and Johns
  #   Hopkins were all 403 or CAPTCHA-walled. Nothing rests on any of them.
  #
  # THE TWO DURATIONS ARE NOT AVERAGED, and that is the point of the page.
  # The research dossier's "10 to 20 minutes" for the bedside exam is supported
  # by NOTHING and is not used. The sourced figures disagree because they are
  # measuring different things: a structured exam inside a routine visit runs a
  # few minutes, a fuller examination runs far longer. Both are printed, and the
  # reason given is UPMC's own - symptoms and thoroughness. An earlier draft
  # said a long exam "usually means it is the first", which UPMC does not say
  # and which review round 1 caught as an invented reassurance.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging/how-diagnosed.html
    title: "American Cancer Society: Tests for Brain Tumors in Adults"
    accessed: 2026-09-23
    # "reflexes, muscle strength, vision, eye and mouth movement, coordination,
    # balance, and alertness" - what the exam checks.
  - url: https://www.cancer.org/cancer/types/brain-spinal-cord-tumors-adults/after-treatment.html
    title: "American Cancer Society: Living as a Brain or Spinal Cord Tumor Survivor"
    accessed: 2026-09-23
    # "ask about any symptoms, examine you" - the exam recurs at follow-up.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC5464449/
    title: "Nayak et al: The Neurologic Assessment in Neuro-Oncology (NANO) Scale (Neuro-Oncology)"
    accessed: 2026-09-23
    # "median and mean times ... were 4 and 5 minutes"; "gait, strength, upper
    # extremity ataxia, sensation, visual fields, facial strength, language,
    # level of consciousness, and behavior"; "readily performed within the time
    # frame of routine office visits"; "The score defines overall response
    # criteria". The short end of the duration, and the reason the exam is
    # repeated.
    # SCOPE, and review round 2 caught the page overstepping it: "readily
    # performed within the time frame of" says the scale CAN fit a routine
    # visit. It does NOT say clinics DO use it. Checked live: "most brain tumor
    # clinics use" and "is routinely used in clinics" are both absent. So the
    # page attributes to the SCALE ("there is a short, set version made for
    # brain tumor care ... where your team uses that one") and never to
    # adoption, and the 4-to-5-minute figure is printed as the scale's measured
    # time rather than as what a routine visit takes.
  - url: https://www.upmc.com/services/stroke/services/tests/neurological-exam
    title: "UPMC: Neurological Exam"
    accessed: 2026-09-23
    # "30 minutes to an hour" - the long end. Printed WITH the short end; and
    # "depending on your symptoms and the thoroughness of the examination",
    # which is the REASON the page prints for the spread. Recorded at round 4,
    # which caught the prose attributing a reason the source entry did not
    # carry even though the front-matter preamble claimed it was UPMC's own.
  - url: https://braintumorcenter.ucsf.edu/support/neurocognitive-care/frequently-asked-questions
    title: "UCSF Brain Tumor Center: Neurocognitive Care FAQ"
    accessed: 2026-09-23
    # The best source on this page: brain-tumor specific AND patient-facing.
    # "cannot be passed or failed"; "two to three hours"; "series of
    # appointments"; "guide rehabilitative plans"; the domain list.
  - url: https://my.clevelandclinic.org/health/diagnostics/4893-neuropsychological-testing-and-assessment
    title: "Cleveland Clinic: Neuropsychological Testing and Assessment"
    accessed: 2026-09-23
    # 'no one "fails."'; "aren't expected to get everything right"; "Most people
    # find some of the tests to be quite easy and others to be difficult";
    # "compare your results with those of others who are the same age"; "and
    # sometimes, with people who have the same educational background
    # (norm-referenced)" - the schooling half, checked live at round 3; "the
    # testing often takes several hours"; "feel tired, over-stimulated and
    # agitated"; "a psychometrist"; "with your permission" they "share your
    # results with your healthcare team"; the preparation advice this page rests
    # the confounder paragraph on - a "good night's sleep" and "medications as
    # usual"; and "hearing aids" among the things to bring.
    # NOT SUPPORTED, and round 2 caught the page asserting it: "built so that
    # nobody gets everything right" is a claim about TEST CONSTRUCTION. Checked
    # live - neither it nor "designed so that no one" appears here. The page now
    # prints only what this clinic observes about people, not a design claim.
  # VOCABULARY: "psychometrist" is new to the corpus and deliberately gets NO
  # glossary entry. Contract item 9 exists so a tooltip fires site-wide, and
  # this page is the only one that says the word - an entry defined and used in
  # one place fires nowhere (§12.8, WI-519). It is glossed inline instead.
  # "neuropsychologist" DID earn an entry, because /tests/planning-scans says it
  # too and the tooltip fires there.
  - url: https://med.uth.edu/neurosciences/conditions-and-treatments/neuropsychological-evaluations/
    title: "UTHealth Houston McGovern Medical School: Neuropsychological Evaluations"
    accessed: 2026-09-23
    # "most of the day"; "you may finish early"; "break for lunch"; "request
    # other breaks as needed";
    # "You may bring a drink or a snack and a lunch"; and the instruction, in
    # capitals on the source, to bring HEARING AIDS and GLASSES.
  - url: https://renaissance.stonybrookmedicine.edu/neuropsychology/faq
    title: "Stony Brook Medicine Neuropsychology: Frequently Asked Questions"
    accessed: 2026-09-23
    # "consumed by direct, face-to-face testing" and "scoring and writing up
    # reports". Cited for the SPLIT between time you are present for and time
    # you are not. Its 8 to 12 hour total is deliberately NOT printed: most of
    # the upper end is the clinician scoring afterwards, and a reader who saw
    # the number without that qualifier would brace for a day that is not theirs.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC4356068/
    title: "Ownsworth et al: Neuropsychological Assessment of Individuals with Brain Tumor (Frontiers in Oncology)"
    accessed: 2026-09-23
    # "differed from classification of impairment based on the norms" - the two
    # comparisons (against people like you, against your own earlier level) do
    # not always agree. This is what makes the honest version of "no pass or
    # fail" possible.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC11913653/
    title: "Brain tumors and fitness to drive: a review and multi-disciplinary approach (Neuro-Oncology Practice)"
    accessed: 2026-09-23
    # "Neuropsychological assessment has been frequently utilized in reaching"
    # fitness-to-drive decisions. Cited ONLY for that the testing feeds into the
    # conversation. The RULES belong to /seizures/living-with#driving and no
    # waiting time appears on this page.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC10605177/
    title: "Pertichetti et al: Neuropsychological Evaluation and fMRI Tasks in Preoperative Assessment (Brain Sciences)"
    accessed: 2026-09-23
    # "evaluation of cognitive function is essential" for informing management
    # and monitoring long-term effects.
  - url: https://pmc.ncbi.nlm.nih.gov/articles/PMC7658191/
    title: "Employment and Work Ability of Persons With Brain Tumors: A Systematic Review (Frontiers in Human Neuroscience)"
    accessed: 2026-09-23
    # "decline in the ability to meet occupational demands"; and the review's
    # own co-factors, checked live: "depressive symptoms" and "environmental
    # barriers" sit alongside cognitive deficits. The page names mood and the
    # setup of the job for that reason, and does NOT rank cognition first - an
    # earlier draft said thinking was "most tied" to work, which this review
    # does not say. It also does NOT say a report decides anybody's job.
reviewed: 2026-09-23
review_due: 2027-03-31
disclaimers: [medical]
---

## The short version {#short-version}

Two different things get talked about in the same breath, and it helps to know
which one you are being sent for.

**The exam in the room** is the short one your team does at almost every visit.
They check your strength, your feeling, your balance and your eyes. It can take
minutes, and a fuller one takes longer.

**The thinking and memory tests** are a different appointment altogether: a
psychologist or a trained tester takes you through word lists, puzzles and timed
tasks, often for two to three hours and sometimes for most of the day.

**Neither is something you can fail.** Both get measured, and being measured is
not the same as being graded. What comes out of them is a description of how you
are doing now, and it is used to work out what help would suit you.

## Two different things, often in the same conversation {#what-they-are}

**The exam in the room** is a set of quick checks on how your brain and nerves
are working right now. Your team looks at your reflexes, your muscle strength,
your vision, how your eyes and mouth move, your coordination, your balance and
how alert you are. The proper name for it is a neurological exam. Every part of
it is you being asked to move, look or answer while somebody watches.

**The thinking and memory tests** are a much longer appointment with a specialist.
The person who reads it is a neuropsychologist, and the proper name for the day
is neuropsychological testing. Often the person who actually hands you the tasks
is a trained tester called a psychometrist, working under that specialist.

They look at different things, and that is why you can be sent for both:

- The exam in the room is about **what can be seen from the outside**: how you
  move, what you feel, how you see and speak, how alert you are.
- The tests are about **how your thinking is working**: attention, memory,
  language, planning and the speed you work at.

Someone can do well on one and find the other hard. Being sent for one is not a
hint about the other.

## Why am I having this? {#why}

**The exam in the room** is how your team notices change early. It is done at
the start, and then again at visits afterwards, alongside asking how you have
been. Being examined is not itself a sign that something has been found.

**The thinking and memory tests** are usually asked for one of three reasons:

- **Before an operation**, to know what your thinking is like beforehand.
  Doctors say plainly that knowing this is essential for planning care and for
  watching the longer term, and it is easier to see a change later if somebody
  wrote down the starting point.
- **Because something has felt different**, to yourself or to the people around
  you.
- **To work out what would help**, which is the reason people are least often
  told about and often the most useful one.

**One short question is worth asking: what will you do with the result?** An
appointment you know the purpose of is a different experience from one you were
simply sent to.

## What happens, step by step {#step-by-step}

**The exam in the room.** It is mostly you being asked to do small things while
somebody watches.

1. **They ask how you have been.** What you say shapes what they look at
   hardest, so it is worth answering properly rather than politely.
2. **They test strength.** Push, pull, squeeze, hold your arms out.
3. **They tap for reflexes**, usually at the knee, elbow and ankle, with a small
   rubber hammer.
4. **They check feeling**, sometimes with a small sharp point. Can you feel this
   on both sides, and does it feel the same on each.
5. **They follow your eyes and face.** Follow my finger. Smile. Puff out your
   cheeks.
6. **They watch you move.** Touch your nose, then my finger. Walk to the door
   and back.

**The thinking and memory tests.** This one is an appointment, not an
examination hall.

1. **You talk first.** What you have noticed, what your days are like, what you
   are worried about.
2. **You do tasks, one at a time.** Lists of words to remember. Shapes to copy.
   Puzzles. Things done against a clock. Questions about your mood.
3. **Some of it is easy and some of it is hard.** A clinic that runs these says
   most people find exactly that, so hitting something you cannot do is the day
   going normally rather than going wrong.
4. **You take breaks.** One clinic that runs these days gives a break for lunch
   and says you can ask for others as you need them.
5. **It gets scored afterwards, without you.** The scoring and the writing up
   happen after you go home.

## How long does it take? {#how-long}

The two answers are very different, and an average of them would be no use to
anybody.

- **The exam in the room: minutes.** The short, set version made for brain tumor
  care takes about four or five minutes, and it is designed to fit inside an
  ordinary appointment.
- **The exam in the room, done in full: longer.** One medical center tells
  people to expect anywhere from half an hour to an hour, and says the length
  depends on your symptoms and on how thorough the examination is.
- **The thinking and memory tests: two to three hours, and sometimes most of the
  day.** One brain tumor center gives two to three hours and says it is
  sometimes split over a series of appointments. Another tells people to plan on
  being there for most of the day, and that you may finish early.

**Ask which one yours is when the appointment is made, and ask whether it can be
split.** The answer changes how you plan the day and who comes with you.

Some of the time quoted for testing is not time you are there for. The scoring
and the report are written up afterwards, and that work is counted in some
hospitals' figures.

## What does it feel like? {#what-it-feels-like}

**The exam in the room** feels like being checked over. The reflex hammer is
odd rather than painful. The sharp point used to check feeling is meant to be
felt rather than to hurt. If any of it does hurt, say so.

**The thinking and memory tests** are tiring, and people say so. A clinic that
does them puts it plainly: several hours of this can leave people feeling tired,
over-stimulated and worked up. That is the day being long, not your brain
failing.

Two things make it easier, and both are allowed. Take the breaks rather than
pushing straight through. And say out loud when something is hard, because the
person testing you would rather know than guess.

## Am I being judged? {#being-judged}

**No. But both of them get measured, and you should know how, because being
measured is not the same as being graded.**

**The exam in the room may be scored, and the score is not a mark out of ten.**
There is a short, set version of it made for brain tumor care. Where your team
uses that one, it rates things the examiner can see: walking, strength, speech,
sight and a few more. The number is there so that today can be set beside your last
visit. A change in it is one of the things that tells your team the picture has
moved.

**The thinking and memory tests are scored differently, and here is exactly
how.** Your answers are compared with people of the same age, and often the same
amount of schooling. Where there is an earlier result, they are compared with
that too. The two comparisons do not always agree. Researchers looked at this in
people with brain tumors and found the two ways of judging gave different
answers. That is a known limit, and it is why a number
on its own is never the whole finding.

**So the measuring is real. A pass mark is not.** A clinic that runs these tests
puts it plainly: most people find some of them quite easy and others difficult,
you are not expected to get everything right, and no one fails. There is no
line you fall below that makes you a failure.

**What a low score does is name something.** It turns "I am not coping and I do
not know why" into a specific list, and a specific list is the thing anybody can
act on.

And a score is read next to the week you had. The clinics that run these tests
ask you to get a good night's sleep first. They ask you to take your medicines
as usual. They ask about your mood as part of the testing itself. All of that is
there because a single sitting is a description of a single day, not a verdict
on you.

## What the report is used for {#the-report}

The exam in the room produces no report. The thinking and memory tests do, and
it is a real document.

**It goes to the people treating you.** A clinic that does this testing says the
results are shared with your healthcare team with your permission.

**It is used to get you things**, which is the part most worth knowing:

- **Help with the difficulties it found.** Therapists work on memory,
  concentration, speech and the practical side of a day, and a report naming
  what is hard is how that help gets arranged.
- **Planning an operation**, where the tumor sits near the parts that handle
  speech, movement or memory.
- **The driving conversation.** Testing like this is one of the things that
  feeds into whether driving is safe. The rules themselves are set where you
  live, and they are not ours to print.
  [Driving and seizures](/seizures/living-with#driving) covers who sets them and
  how to find out what yours are.
- **Work.** Thinking and memory are part of what makes work feel possible, and
  so are your mood and the way the job itself is set up. A report does not
  decide anybody's job. What it can do is explain why work has become harder, and
  say what would make it easier. [Work after a diagnosis](/seizures/living-with#work)
  covers how much you have to tell an employer.

**Ask who yours will be sent to before the day**, and ask what happens if you
want a copy. Both are easier to sort out beforehand than afterwards.

## What to sort out beforehand {#what-to-bring}

**For the exam in the room, nothing.** It happens inside an appointment you were
already having. You do not prepare for it and you cannot revise for it.

**For the thinking and memory tests, four things are worth doing.**

- **Bring your glasses and your hearing aids.** Clinics that do this testing ask
  for this, and the reason is plain once you see it. A test of reading or
  listening measures your eyes and ears first. Bring your medicines too if you
  take any while you are there.
- **Bring a drink, a snack and some lunch.** One clinic says you may bring food
  and a drink, and on a long day it matters more than it sounds.
- **Ask whether it can be split.** A brain tumor center that does this testing
  says it is sometimes spread over a series of appointments. If a whole day is
  not realistic for you, that is worth saying when the appointment is made
  rather than on the day.
- **Plan the journey home before you go.** You will be more tired at the end
  than at the start, and it is easier to arrange a ride in advance than to work
  it out when you are worn out.

## Why am I having this again? {#again}

Because one result is a single snapshot, and what your team needs is the change
over time.

Both of these are repeated on purpose. The exam in the room is done at visits
afterwards precisely so that a change can be seen against your own earlier
checks, and the short, set version exists to measure exactly that, where your
team uses it. The tests may be repeated after treatment, or later on, for the
same reason.

**Being asked again is not a sign that something was wrong the first time.** It
is the opposite: a single result has nothing to sit beside, and the second one is
what makes the first one useful.

## Who reads it, and how do I get the result? {#results}

**The exam in the room: there and then.** The person doing it is the person
reading it. If they do not say anything, ask before you leave what they found,
because "no change" is a finding and it is worth hearing out loud.

**The thinking and memory tests: not on the day.** A neuropsychologist reads and
interprets the results after the scoring is done, and there is a written report
at the end of it. Ask whether you come back for the results or whether your own
doctor goes through them with you.

**Ask before you leave when it will be ready and who will explain it.** Waiting
times are set by each hospital, and no website knows yours. What you can get on
the day is the plan: who writes to whom, and which of them you should hear from.
Ask who to contact if nothing arrives.

## What to ask your team {#questions}

- What will this be used for, and would the answer change anything?
- How long should I plan for, and can it be split over more than one visit?
- Should somebody come with me, and will I be able to get myself home?
- Should I bring my glasses, my hearing aids, or a list of my medicines?
- Is there a written report, how do I get a copy, and who else is it sent to?
- Would my report be sent to anyone outside the hospital, and would you need my
  permission first?
- Does anything in this change what help I can be referred for?
- Will this be repeated, and roughly when?

## Where to go next {#where-to-go-next}

- [Getting ready for surgery](/tests/getting-ready-for-surgery) covers the other
  appointments that come before an operation.
- [Your MRI scan](/tests/mri) is the test people are most often sent for
  alongside these.
- [Follow-up scans](/tests/follow-up-scans) goes through the check-ups that
  carry on afterwards, and what the waiting is like.
- [Living with seizures](/seizures/living-with#driving) has driving, and what to
  ask about the rules where you live.
- [Radiation therapy](/treatments/radiation-therapy#side-effects) explains what
  treatment itself can do to thinking and memory.
