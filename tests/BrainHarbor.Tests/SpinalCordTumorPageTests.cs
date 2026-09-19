using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-543: <c>/tumors/spinal-cord-tumor</c>, deepened from a 48-line stub to §12.3's
/// seventeen sections.
///
/// THE ITEM'S CENTRE IS A SAFETY-TIER RULING, NOT A TUMOR DESCRIPTION. WI-536 deferred
/// "one claim, two strengths" here: <c>blocks/spinal-cord.md</c> files new weakness and
/// bladder trouble as right-away, while this hub, /tumors/meningioma and
/// /tumors/brain-metastases filed the same signs as "a reason to be seen quickly".
/// IT WAS NEVER ONE CLAIM. Metastatic cord compression carries an emergency-room rule in
/// US patient-facing sources; a PRIMARY tumor in or beside the cord does not, and no
/// fetched source applies one outside the cauda equina picture; and a fast carve-out
/// applies to both. Evidence log: <c>.claude/work_files/wi543/plan.md</c>.
///
/// So the guards below assert the SPLIT as three separate properties. A page that kept
/// only the reassuring half would satisfy any one of them and would be §12.12's dangerous
/// direction — over-reassurance is the failure that stops being true later, at the worst
/// moment.
/// </summary>
public sealed class SpinalCordTumorPageContentTests
{
    private const string Slug = "tumors/spinal-cord-tumor";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a spinal cord tumor?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string DiagnosisHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life: work, moving around, and the things nobody mentions";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string RecurrenceHeading = "If it comes back, or changes";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CareHeading = "For the person caring for someone with this";
    private const string QuestionsHeading = "What to ask your team";
    private const string SupportHeading = "Where to get support";

    private static string Page => CuratedPage.Read("tumors", "spinal-cord-tumor.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string Plain => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section of the RAW page, unflattened, for assertions about paragraphs and
    /// positions. <c>CuratedPage.Section</c> flattens before it returns, and §12.11
    /// records what that costs: a splitter handed flattened text finds no newlines,
    /// yields one chunk, and every per-chunk assertion quietly becomes a whole-section
    /// assertion. <c>\r?\n</c> throughout, never a bare <c>\n</c>.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    /// <summary>
    /// The only runs this page shares with another on purpose: EMERGENCY THRESHOLDS.
    /// §12.10 is explicit that rewording one of these IS the defect — "a rewrite is a
    /// rewording of an emergency threshold" — and /tumors/meningioma and
    /// /tumors/brain-metastases carry the same class of entry for the same reason.
    ///
    /// Kept to the PHRASE rather than the sentence, because the corpus helper excuses any
    /// eight-word run contained INSIDE an entry: a longer string than the page actually
    /// carries quietly exempts collisions that were never there.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        "calling your team right away or going to the emergency room",
        "numbness around the area you would sit on",
    ];

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$").Select(m => m.Groups[1].Value.Trim())];

    // ------------------------------------------------------------- the ruling

    [Fact]
    public void TheUrgencySplitCarriesBothStrengthsAndTheCarveOutThatAppliesToBoth()
    {
        // THE CLAIM THE ITEM TURNS ON, asserted as THREE properties rather than one.
        // Presence of any single half is not the property; carrying all three is. A draft
        // that dropped the metastatic half would still read like a complete page.
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(SymptomHeading)));

        // 1. THE METASTATIC HALF. ACS: "call your doctor right away or go to the emergency
        //    room"; OncoLink: "Call 911 or your care team right away".
        Assert.Matches(new Regex(
            @"If your cancer has spread to your spine[^.]{0,120}"
            + @"calling your team\s+right away or going to the emergency room",
            RegexOptions.IgnoreCase), section);

        // 2. THE PRIMARY HALF, which is the one no source supports escalating to the
        //    emergency room. Four patient-facing pages list these signs and give NO urgency
        //    rule at all.
        //
        //    IT IS SAME-DAY, NOT "SEEN QUICKLY", AND /review FOUND WHY. The first draft
        //    said "a reason to be seen quickly... this week", and the composed [ESCALATION]
        //    block twelve lines below files "New weakness in the face, an arm or a leg" as
        //    SAME DAY — strictly stronger. That recreated WI-536's "one claim, two
        //    strengths" defect INSIDE a single composed page, on the very item sent here to
        //    settle it. Same-day is already sourced by the block on all 19 hubs, it is the
        //    safe direction (§12.12), and it retires an invented window: this item's own
        //    research file records "No time window verified for primary tumors from symptom
        //    onset."
        Assert.Matches(new Regex(
            @"If your tumor started here[^.]{0,140}same-day call",
            RegexOptions.IgnoreCase), section);
        // THE WINDOW IS BANNED, NOT THE PHRASING IT ARRIVED IN. The first version banned
        // the literal "this week rather than" — the exact deleted wording — and /review
        // round 2 found "this week" alive 280 lines away in the ask list, where it also
        // offered a two-tier schema that omitted the same-day tier the page actually uses.
        // A ban written against one sentence is not a ban on the claim.
        Assert.DoesNotMatch(new Regex(@"\bthis week\b", RegexOptions.IgnoreCase), Plain);
        Assert.Matches(new Regex(@"\bthis week\b", RegexOptions.IgnoreCase),
            "Which symptoms mean calling you this week?");

        //    AND THIS PAGE'S OWN TIER IS NEVER WEAKER THAN THE COMPOSED BLOCK'S FOR THE
        //    SAME SIGN. Asserted on the COMPOSED page, because that mismatch does not exist
        //    in the source file — which is exactly how it survived the first round.
        // This one only proves the block COMPOSED — the tier comparison itself is the
        // `weakerTier` ban below, and the comment that used to sit here claimed more than
        // the assertion could see (/review round 4, nit 7).
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        Assert.Contains("Call your team the same day", whole, StringComparison.Ordinal);

        // CROSSES ONE SENTENCE BOUNDARY, AND HAS A CANARY. `[^.]{0,160}` stopped at the
        // first period, so a regression split across two sentences — "…are a same-day
        // call. Seen quickly is what the sources support." — walked through. This was the
        // ONLY ban in the file without a canary, which is how that went unnoticed.
        // ONE SENTENCE BOUNDARY, WITHOUT DEPENDING ON CASE. The first version crossed it
        // with `\.(?!\s*[A-Z]{2})` — under `RegexOptions.IgnoreCase`, which makes `[A-Z]`
        // match lowercase too, so "Se" of "Seen" satisfied `[A-Z]{2}`, the lookahead
        // failed, and the guard could not see the regression it was written for. Its own
        // canary is what caught that; without one it would have sat here reading as a
        // working cross-sentence check.
        // "next routine visit" IS NOT ONE OF THE MARKERS, AND THAT IS DELIBERATE. Once the
        // pattern could cross a sentence boundary it matched this page's own correct
        // instruction — "Call your team today about those three, rather than at the next
        // routine visit" — where the phrase is what the reader is steered AWAY from. Same
        // negation problem as the shared `answered` set, reproduced here one edit later.
        // "seen quickly" and "this week" are unambiguous weak tiers whichever way they are
        // used; "this week" is also banned page-wide with its own canary further down, so
        // dropping it here loses no cover.
        var weakerTier = new Regex(
            @"tumor started here[^.]{0,200}\.?[^.]{0,140}\b(?:seen quickly|this week)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(weakerTier,
            "If your tumor started here, those signs are a same-day call. Seen quickly is the rule.");
        Assert.Matches(weakerTier, "If your tumor started here, this week is soon enough.");
        Assert.DoesNotMatch(weakerTier, whole);

        // 3. THE CARVE-OUT, and it must say it applies to BOTH. This is the half that
        //    stops property 2 from reading as "a cord tumor is never urgent".
        Assert.Matches(new Regex(
            @"one picture is emergency care now, whichever kind of tumor you have",
            RegexOptions.IgnoreCase), section);

        // The carve-out names all three of its signs. Named individually, because a
        // shortened list is the §12.12 direction and the sign most often dropped is the
        // one two US sources single out.
        foreach (var sign in new[]
                 {
                     @"new trouble controlling your bladder or bowel",
                     @"numbness around the area you would sit on",
                     @"weakness that is\s+getting worse over hours or days",
                 })
        {
            Assert.Matches(new Regex(sign, RegexOptions.IgnoreCase), section);
        }

        // AND BLADDER AND BOWEL IS NEVER RANKED BELOW WEAKNESS. ACS puts all four signs
        // under one instruction and two US patient pages attach speed to bladder/bowel
        // specifically, so nothing here may demote it.
        Assert.Matches(new Regex(
            @"Bladder and bowel changes are never less urgent than weakness",
            RegexOptions.IgnoreCase), section);

        // THIS PAGE'S OWN RULE COMES BEFORE THE SHARED BLOCK, AND THE FIRST RENDERED READ
        // IS WHY. Composed, [ESCALATION] puts TWENTY lines above this reader: three
        // ambulance bullets about seizures, a same-day list of headache, confusion, vision
        // and face weakness, a fever rule for "the weeks after brain surgery" pointing at
        // craniotomy, and a conditional about shunts. None of it is FALSE — they are
        // ambulance calls for anyone, and the last two are explicit conditionals — so
        // §12.10's truth test passes and the block stays. What was wrong was ORDER: a cord
        // reader had to wade through brain emergencies to reach bladder and bowel.
        //
        // The source file shows none of this. Only a rendered read does.
        var raw = RawSection(SymptomHeading);
        var own = raw.IndexOf("### When a spinal cord symptom means going in now",
            StringComparison.Ordinal);
        var shared = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        Assert.True(own >= 0 && shared >= 0, "the symptoms section has lost one of its two rules");
        Assert.True(own < shared,
            "the shared escalation block now composes ABOVE this page's own cord rule, so a "
            + "spinal cord reader meets seizures, shunts and craniotomy before bladder and bowel");

        // And the general list is introduced rather than dumped, so its brain content reads
        // as "not all of this is yours" instead of as this page's own advice.
        Assert.Matches(new Regex(
            @"The list below is the general one, written for any tumor", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(raw)));
    }

    [Fact]
    public void ThePrimaryTumorHalfIsNotWrittenAsReassurance()
    {
        // §12.12: the reassuring direction is the dangerous one, and this page LOWERS a
        // tier relative to the shared block. So the weaker half must never appear without
        // its counterweight, and "quickly" must be given a meaning rather than left to the
        // reader to interpret as "whenever".
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(SymptomHeading)));

        // "Quickly" IS GONE, AND SO IS THE WINDOW IT CARRIED. The first draft said
        // "Quickly still means this week rather than the next routine visit" — an invented
        // number this item's own research file rules out ("No time window verified for
        // primary tumors from symptom onset"). What replaces it is a tier the corpus
        // already sources, so the instruction is concrete without being made up.
        // THE TIER IS STATED HERE, NOT DEFERRED TO THE SHARED LIST. /review round 2
        // blocker: the page said "the same-day list further down covers them" for FOUR
        // signs, and that list carries ONE of them — new limb weakness. New numbness and
        // new trouble walking are not on it at all. A reader with new trouble walking
        // followed the instruction, found nothing, and could reasonably conclude it was not
        // urgent, which is the under-triage direction on the page that lowered a tier.
        Assert.Matches(new Regex(
            @"Call your team today about those three, rather\s+than at the next routine visit",
            RegexOptions.IgnoreCase), section);

        // THE SAME-DAY SENTENCE MUST NOT CLAIM A SIGN THE CARVE-OUT SENDS TO THE EMERGENCY
        // ROOM. /review round 3 blocker, and the third generation of one defect: round 2
        // fixed an under-triage by stating the tier over all FOUR signs, which swept
        // bladder and bowel — the carve-out's own sign, with no narrowing clause to tell
        // the two uses apart — into "rather than a trip to the emergency room", twelve
        // lines above "Go to the emergency room for new trouble controlling your bladder
        // or bowel". Weakness and numbness are safe because the carve-out NARROWS them
        // ("getting worse over hours or days", "around the area you would sit on"); the
        // bladder sign had no such narrowing, so the two sentences described one event and
        // gave opposite instructions.
        //
        // Nothing compared the two paragraphs before this: both were green on it at once.
        var sameDaySentence = Regex.Match(section,
            @"\*\*If your tumor started here.*?\*\*", RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(sameDaySentence), "the same-day sentence has gone");
        foreach (var carveOutOnly in new[] { "bladder", "bowel" })
        {
            Assert.DoesNotContain(carveOutOnly, sameDaySentence, StringComparison.OrdinalIgnoreCase);
        }
        // THE SHAPE, NOT THE SENTENCE. The first version banned the literal wording that
        // was deleted, so any reworded deferral — "the list below covers them", "see the
        // same-day list" — would have walked straight through. My own evidence log names
        // this class one entry earlier: a ban on one sentence is not a ban on a claim.
        // THE VERB IS OPTIONAL, AND BOTH ORDERS COUNT. /review round 4: requiring a
        // covers/includes/has verb meant "See the same-day list below" and "They are
        // covered by the list below" walked straight through — the same word-order
        // dependence this file has now fixed three times, in a ban whose own comment names
        // the example it was missing.
        // SCOPED TO A *LIST*, NOT TO ANY "SECTION ... BELOW". Dropping the verb entirely
        // was an over-correction: it matched this page's own legitimate pointer in the
        // short version, "They are in their own section below." The defect being guarded is
        // deferring SIGNS to a LIST that does not carry them, so the noun that matters is
        // "list"; a section pointer is ordinary navigation.
        // THE COVERAGE CLAIM IS THE DEFECT, NOT THE WORD "LIST". Round 4 dropped the verb
        // to catch "See the same-day list below", and the result fired on this page's own
        // correct sentence, "The list below is the general one, written for any tumor" —
        // which introduces the shared block rather than deferring any sign to it. The verb
        // was doing real work; what it needed was the OTHER WORD ORDERS beside it, which is
        // the third time this file has had to learn that a ban must match both directions.
        // WIDENED AGAINST PROBES IT WAS NOT WRITTEN FOR. /review round 5 threw fourteen at
        // the previous form and five walked through — "Those are covered by the list
        // below", "They appear on the same-day list below", "Check the list further down
        // for them". Every earlier version of this ban was checked only against the
        // sentence that prompted it, which is how it has now been wrong three rounds
        // running.
        var deferred = new Regex(
            @"\b(?:see|check|look at)\b[^.]{0,25}\blist\b[^.]{0,20}\b(?:below|further down)\b"
            + @"|\blist\b[^.]{0,40}\b(?:covers?|includes?|has|lists?|carries)\s+"
            + @"(?:them|those|these|that|all of them)\b"
            + @"|\b(?:they|those|these|both|all three|all four|"
            + @"the (?:signs?|four signs|three signs))\b[^.]{0,25}"
            + @"\b(?:are on|are covered by|are in|appear on|appear in|are listed)\b"
            + @"[^.]{0,25}\blist\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(deferred, "Those are covered by the list below.");
        Assert.Matches(deferred, "They appear on the same-day list below.");
        Assert.Matches(deferred, "Check the list further down for them.");
        Assert.Matches(deferred, "The same-day list below covers them.");
        Assert.Matches(deferred, "They are on the list further down, which includes them.");
        Assert.Matches(deferred, "See the same-day list below.");
        Assert.Matches(deferred, "They are covered by the list below.");
        Assert.DoesNotMatch(deferred, "They are in their own section below.");
        Assert.DoesNotMatch(deferred,
            "The list below is the general one, written for any tumor.");
        Assert.DoesNotMatch(deferred, Plain);

        // The page must not tell anyone a cord symptom can wait. Proven able to fire.
        // BOTH ORDERS, because a permission can sit on either side of the symptom. The
        // first version only matched permission-then-symptom, and its own second canary
        // ("New numbness can wait...") is symptom-then-permission — so the canary failed
        // and showed the ban was half the size its name claimed.
        const string Permission =
            @"no need to|does not need to|do not need to|no rush|can wait|nothing to worry about";
        const string Sign = @"weakness|numbness|bladder|bowel|walking";
        var waitable = new Regex(
            $@"\b(?:{Permission})\b[^.]{{0,60}}\b(?:{Sign})\b"
            + $@"|\b(?:{Sign})\b[^.]{{0,60}}\b(?:{Permission})\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(waitable, "There is no need to worry about new weakness.");
        Assert.Matches(waitable, "New numbness can wait until the next appointment.");
        Assert.DoesNotMatch(waitable, Plain);

        // And the slow-growth fact, which is true and is the evidential basis for the
        // weaker tier, is never turned into permission to wait. Both sources are explicit
        // that faster onset does worse AND that longer delay does worse.
        Assert.DoesNotMatch(new Regex(
            @"slow[- ]growing[^.]{0,60}\bso\b[^.]{0,60}\b(?:wait|no hurry|no rush)\b",
            RegexOptions.IgnoreCase), Plain);
    }

    [Fact]
    public void ThePageDoesNotPromiseThatFunctionComesBackAfterSurgery()
    {
        // THE MOST DANGEROUS CLAIM ON THIS PAGE, and the sources contradict each other:
        // one says the transient deficit "invariably recovers" (but the phenomenon is
        // DEFINED by recovering, so the sentence is close to circular), while a 2025
        // review counts everyone worse after surgery and reports complete resolution in a
        // minority. They are not measuring the same group, which is why both can be true.
        //
        // BOTH DIRECTIONS ARE ASSERTED. The hedge must be present AND the promise absent:
        // a check for only the hedge passes a page that also carries the promise two
        // paragraphs later, which is exactly how WI-542 shipped an APPENDED fix.
        var after = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(AfterHeading)));

        Assert.Matches(new Regex(
            @"It does not always settle completely", RegexOptions.IgnoreCase), after);
        Assert.Matches(new Regex(
            @"anyone who tells you it always does\s+is going further than the evidence",
            RegexOptions.IgnoreCase), after);
        Assert.Matches(new Regex(
            @"where you end up is not known\s+on the day you wake up", RegexOptions.IgnoreCase), after);

        // THE POST-OPERATIVE RULE CARRIES ALL THREE CARVE-OUT SIGNS. /review round 3: it
        // named two and dropped saddle numbness — and cauda equina after spinal surgery is
        // exactly the emergency this paragraph exists for. The three-sign loop in the
        // urgency test is scoped to the SYMPTOMS section, so it structurally could not see
        // a restatement 130 lines away: an assertion scoped to one section while the claim
        // lives in two.
        foreach (var sign in new[]
                 {
                     @"Weakness that keeps getting worse",
                     @"numbness around the area you would sit\s+on",
                     @"new trouble controlling your bladder or bowel",
                 })
        {
            Assert.Matches(new Regex(sign, RegexOptions.IgnoreCase), after);
        }

        // THE PROMISE, BANNED ACROSS THE WHOLE PAGE. Written as a shape rather than a
        // phrase: /review beat phrase bans on three previous items.
        // THE QUANTIFIER HALF IS PART OF THE BAN, and /review round 1 is why. The page
        // said "improvement over months is usual" — a reassuring quantifier on the one
        // sentence the front matter itself flags as most likely to stop being true for the
        // reader holding it — and the first version of this regex, which looked only for
        // recovery VERBS, sailed straight past it.
        var promise = new Regex(
            @"\b(?:most people|nearly everyone|almost everyone|you will|people)\b[^.]{0,80}"
            + @"\b(?:get (?:their )?function back|recover fully|make a full recovery|"
            + @"return to normal|be back to normal|fully recover"
            // "go back to ordinary life" IS NOT BANNED, AND THAT IS A CORRECTION TO
            // /review round 4's suggestion. The outlook gate ALREADY carries "Many people
            // with a slow-growing tumor that came out completely go back to ordinary
            // life" — correctly scoped, as that round itself said — so a page-wide ban on
            // the phrase fires on the page's own honest sentence. Banning a phrase that has
            // a correct use is the §12.8 mistake this corpus rejected "good sign" and
            // "bad news" for. The unscoped shapes below are banned instead.
            + @"|back to (?:their|your) old selves|walk(?:ing)? again|"
            + @"end up where (?:they|you) started)\b"
            // THE UNSCOPED "ORDINARY LIFE" PROMISE, WHICH REMOVING THE PHRASE LEFT
            // UNGUARDED. Round 4 had me ban it outright and it fired on the outlook gate's
            // legitimate, scoped sentence; round 5 found that taking it out again left
            // NOTHING in its place. The distinguishing feature is distance to the
            // qualifier: the gate puts 52 characters between "people" and the claim, an
            // unscoped promise puts about twelve.
            + @"|\b(?:most people|nearly everyone|almost everyone|everyone|people|you)\b"
            + @"[^.]{0,25}\bgo(?:es|ing)? back to (?:ordinary|normal) life\b"
            // BOTH ORDERS, AND THE CANARY BELOW IS WHY THIS COMMENT EXISTS. The first
            // version of this alternative required quantifier-then-verb, so
            // "Improvement over months is usual" — the very sentence it was written to
            // catch — did not match. That is the IDENTICAL hole found in the "can wait"
            // ban earlier in this same file, fixed there with a bidirectional
            // alternation, and then reproduced here in a ban widened because /review
            // asked for it. A quantifier can sit on either side of the verb.
            + @"|\b(?:usual|usually|most|typically|generally)\b[^.]{0,60}"
            + @"\b(?:improve\w*|recover\w*|settles? completely)\b"
            + @"|\b(?:improve\w*|recover\w*|settles? completely)\b[^.]{0,60}"
            + @"\b(?:usual|usually|most|typically|generally)\b"
            // BOUND TO THE SUBJECT, NOT DROPPED. Removing `comes? back` outright — because
            // it fired on "a tumor that comes back most often" — left "Function usually
            // comes back" unbanned, which is the ONE claim this page's front matter says it
            // must never make. The collision was by SUBJECT (tumor vs function), and
            // narrowing by TOKEN threw away the guard along with the false positive.
            + @"|\b(?:function|strength|feeling|movement|power)\b[^.]{0,40}\bcomes? back\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(promise, "Most people get their function back after this operation.");
        Assert.Matches(promise, "You will make a full recovery.");
        Assert.Matches(promise, "Improvement over months is usual.");
        Assert.Matches(promise, "Strength usually recovers.");

        // `comes back` IS NOT IN THE QUANTIFIER HALVES, AND THAT IS DELIBERATE. It was,
        // and it fired on "a tumor that comes back most often does so where the first one
        // was" — the RECURRENCE-LOCATION claim, which /review round 2 had just made say
        // "most often" instead of "nearly always". On this page "comes back" means the
        // tumor returning, not function returning, so pairing it with a quantifier caught
        // a correct sentence. A ban that fails the correct case is the defect this corpus
        // keeps re-learning. The promise being guarded is about RECOVERY, and
        // improve/recover/settle carry it.
        Assert.Matches(promise, "Function usually comes back.");
        Assert.Matches(promise, "Movement comes back for most people.");
        Assert.Matches(promise, "Most people go back to ordinary life.");
        Assert.Matches(promise, "Nearly everyone goes back to ordinary life.");

        // AND THE OUTLOOK GATE'S OWN SENTENCE MUST SURVIVE, because it is scoped and true.
        // A ban that fails the correct case is the defect this corpus keeps re-learning.
        Assert.DoesNotMatch(promise,
            "Many people with a slow-growing tumor that came out completely go back to "
            + "ordinary life, and are followed with scans for years.");
        Assert.DoesNotMatch(promise,
            "For ependymoma, a tumor that comes back most often does so where the first one was.");
        Assert.DoesNotMatch(promise, Plain);

        // And no figure either way. Both candidate numbers are in the sources and neither
        // belongs on the page.
        //
        // THE BREAK HARNESS FOUND THIS BAN DEAD. It read `(?:%|percent)\b`, and `%` and the
        // space after it are both non-word characters, so the boundary could never match:
        // "About 41% recover completely." walked straight through. It had been unable to
        // fire since the day it was written.
        //
        // AND IT IS THE SAME DEFECT TWICE IN ONE FILE. The numbers test below had the
        // identical `%` boundary, its canary exposed it, and I fixed it there — then left
        // this one, because this ban had NO CANARY AT ALL. A ban without a canary is a ban
        // unproved, and it took a planted mutation to find what a canary catches for free.
        var recoveryFigure = new Regex(
            @"\b\d{1,3}\s*(?:%|percent\b)[^.]{0,60}\b(?:recover|resolve|deficit)\b"
            // AND THE FIGURE WRITTEN IN WORDS. §12.8 (WI-527) records that a digit ban is
            // not a prognosis ban, and this page's own outlook gate bans half/third/quarter
            // for that reason. Kept to unambiguous fractions so it cannot fire on the
            // orienting durations §12.4 R1 deliberately allows here.
            + @"|\b(?:half|a third|a quarter|two thirds)\b[^.]{0,40}"
            + @"\b(?:recover|resolve|get better)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(recoveryFigure, "About 41% recover completely.");
        Assert.Matches(recoveryFigure, "Around 34 percent resolve within six months.");

        // A DIGIT, NOT A WORD. The first version of this canary read "Nine percent have a
        // worse deficit afterwards" against a pattern requiring `\d{1,3}` — a canary that
        // COULD NOT FIRE, shipped in the very edit whose lesson was that a ban without a
        // working canary is a ban unproved. It failed loudly, which is the whole point of
        // having one; the spelled-out branch above is what that mistake was reaching for.
        Assert.Matches(recoveryFigure, "9 percent have a worse deficit afterwards.");
        Assert.Matches(recoveryFigure, "About half recover completely.");
        Assert.DoesNotMatch(recoveryFigure, Plain);
    }

    // ------------------------------------------------------- the block rulings

    [Fact]
    public void ExactlyFiveBlocksAreIncludedAndTheThreeExclusionsAreAClosedSet()
    {
        // THE RULING THAT WOULD OTHERWISE BE AN ABSENCE. An excluded block leaves NO trace
        // on the page — no heading, no directive, nothing to grep — so a later item can
        // restore one and every other guard in this file stays green.
        //
        // An EXACT SET, not a handful of DoesNotContain checks: a new block added in two
        // years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "crosswalk", "escalation", "tumor-board"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        // Each included block sits in the section that gives it its meaning. POSITION is
        // the property (WI-512): a directive that drifts into another section still
        // composes, and still reads as unrelated to the thing it qualifies.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheMechanismBlockIsExcludedBecauseItIsAboutAClosedSkull()
    {
        // §12.10's test — would this be true on the hub you have thought about least? —
        // fails outright here, on the one page whose central claim is that this is not a
        // brain tumor. Every sentence of that block is about the skull: "The skull is a
        // closed box", brain swelling, pressure inside the head, seizures, blocked fluid,
        // and a map of frontal/temporal/parietal/occipital/cerebellum/brainstem.
        //
        // Asserted against the BLOCK'S OWN WORDS rather than a literal list, so that moving
        // a sentence into or out of that block later turns this red instead of leaving a
        // stale copy behind (§12.11).
        var mechanism = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"))));

        foreach (var blockOnly in new[]
                 {
                     "The skull is a closed box",
                     "It can block the flow of fluid",
                     "the symptom tells you where, the scan tells you what",
                 })
        {
            Assert.Contains(blockOnly, mechanism, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        // AND THE PAGE WRITES ITS OWN MECHANISM INSTEAD, because excluding a block without
        // replacing what it did would leave the §12.3 section-3 question unanswered. The
        // canal, not the skull.
        var where = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(WhereHeading)));
        Assert.Matches(new Regex(
            @"narrowest stretch of the whole canal", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(
            @"the level decides which parts of you are affected", RegexOptions.IgnoreCase), where);
    }

    [Fact]
    public void TheSpinalCordBlockIsExcludedAndThePageSaysTheTierItselfInstead()
    {
        // THE EXCLUSION THIS ITEM ARGUED HARDEST, and the one a later editor is likeliest
        // to "correct" back in, because the block's name matches this page's subject
        // exactly. It asserts right-away for a tumor IN the cord, full stop — which is the
        // over-triage direction for the primary tumors that are most of this page's
        // readers, and which no patient-facing source supports.
        //
        // Asserted on the COMPOSED page, not on directive names: blocks nest up to five
        // levels, so a direct-name check would stay green if an included block later
        // pulled this one in.
        // EACH STRING IS PROVED TO EXIST IN THE BLOCK BEFORE IT IS PROVED ABSENT HERE.
        // /review round 1: without the positive half, rewording blocks/spinal-cord.md
        // silently retires the guard on the exclusion this item argued hardest — the
        // absence check would keep passing because the words had simply gone.
        var spinalBlock = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"))));

        foreach (var blockOnly in new[]
                 {
                     "right-away call, not a same-day one",
                     "new signs from the cord are their own rule",
                     "Cancer Research UK calls it an emergency",
                 })
        {
            Assert.Contains(blockOnly, spinalBlock, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        // The includer list stays four hubs and this page is not one of them.
        Assert.DoesNotContain("spinal-cord", ContentBlocks.DirectBlockNames(Page));

        // The posterior fossa block is excluded for the plainest reason of the three: it
        // opens on surgery "low at the back of the brain", which is not this operation.
        Assert.DoesNotContain("posterior-fossa-syndrome", ContentBlocks.DirectBlockNames(Page));

        const string PosteriorFossaOnly = "A day or two after surgery low at the back of the brain";
        var fossaBlock = CuratedPage.Flatten(CuratedPage.ReaderText(File.ReadAllText(
            Path.Combine(CuratedPage.BlocksRoot, "posterior-fossa-syndrome.md"))));
        Assert.Contains(PosteriorFossaOnly, fossaBlock, StringComparison.Ordinal);
        Assert.DoesNotContain(PosteriorFossaOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSelfBlameBlockIsScopedForTheReaderWhoseCancerSpreadHere()
    {
        // §12.10 again, and this block needed a scoping sentence rather than exclusion.
        // It opens "For most brain tumors, nobody knows the cause", which is brain-framed
        // on the page that exists to say this is not a brain tumor — and it is silent on
        // the reader whose tumor arrived from a cancer elsewhere, who is not asking the
        // same question at all.
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(CauseHeading)));

        Assert.Matches(new Regex(
            @"spread to the spine\s+from a cancer elsewhere", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"a feature of the illness, not evidence that\s+anybody was slow",
            RegexOptions.IgnoreCase), section);

        // THE SCOPE COMES BEFORE THE BLOCK, NOT AFTER IT, and that is what the first
        // rendered read changed. The scoping paragraph originally sat BELOW [CAUSES], so
        // the reader whose cancer spread to the spine met five paragraphs of "For most
        // brain tumors, nobody knows the cause" before being told the section was written
        // for a different question. On the page whose central claim is that this is not a
        // brain tumor, that is the wrong way round. /tumors/meningioma scopes the same way
        // — "If your meningioma is on your spinal cord, the list below is not yours" —
        // before the general content, not after it.
        //
        // POSITION IS THE PROPERTY (WI-512), so it is asserted by position.
        var scoped = section.IndexOf("written for a different question", StringComparison.Ordinal);
        var directive = section.IndexOf("[CAUSES]", StringComparison.Ordinal);
        Assert.True(scoped >= 0, "the spinal scoping line has gone from the self-blame section");
        Assert.True(directive > scoped,
            "the self-blame block now composes ABOVE its scoping line, so the reader whose "
            + "cancer spread to the spine meets the brain-tumor framing first");

        // §12.6: never end a section on the frightening half. The block's own last line is
        // an action ("worth saying out loud to somebody on your team"), which is why the
        // scoping paragraph could move above the directive without leaving the section
        // ending badly.
        //
        // CHECKED ON THE COMPOSED SECTION, NOT THE RAW ONE, and that is the whole point of
        // the rule. Read raw, this section's last "sentence" is the literal token
        // "[CAUSES]" — so the first version of this assertion passed only while the
        // scoping paragraph sat BELOW the directive, and started failing the moment the
        // paragraph moved up, for a reason that had nothing to do with what a reader meets.
        // §12.6 is about the page as read, so it has to be asserted against the page as
        // composed (§12.10, WI-514's trap in miniature).
        var composedSection = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, CauseHeading));
        var sentences = CuratedPage.SentencesOf(composedSection);
        // "saying", not "asking|saying": the looser form matched this page's own scoping
        // paragraph as well as the block's closing line, so it would have passed with the
        // block gone (§12.10 — if the subject is the block, assert something only the
        // block says). /review round 1.
        Assert.Matches(new Regex(@"worth saying out loud", RegexOptions.IgnoreCase),
            sentences[^1]);

        // And §12.9's demotion: the block sits BELOW "if it comes back" and ABOVE the
        // outlook gate.
        var order = Headings();
        Assert.True(order.IndexOf(RecurrenceHeading) < order.IndexOf(CauseHeading));
        Assert.True(order.IndexOf(CauseHeading) < order.IndexOf(OutlookHeading));
    }

    // ------------------------------------------------------ naming and the report

    [Fact]
    public void TheCrosswalkSliceIsTheTwoGradeOnesAndNotASiblingsSlice()
    {
        // §12.11: a page that copies the umbrella's slice has added a maintenance site and
        // no information. The obvious angle here — the myxopapillary regrade — is ALREADY
        // OWNED by /tumors/ependymoma, and the schwannoma grade argument is already owned
        // by /tumors/acoustic-neuroma, which verified CNS5's printed absence live.
        //
        // So this page's slice is the collision neither of them can carry: a reader
        // holding an operative note that says "Simpson Grade I" has two documents using
        // the word grade for completely different things.
        var report = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(ReportHeading)));

        Assert.Matches(new Regex(@"Two different things are both called grade 1",
            RegexOptions.IgnoreCase), report);
        Assert.Matches(new Regex(
            @"Simpson grade\*{0,2} describes how completely the surgeon", RegexOptions.IgnoreCase),
            report);
        Assert.Matches(new Regex(
            @"can describe a thorough operation rather than\s+your tumor",
            RegexOptions.IgnoreCase), report);

        // The honest limit, which the source states outright and which stops the slice
        // reading as advice about which operation to want.
        Assert.Matches(new Regex(
            @"How much of the covering needs to come out is genuinely unsettled",
            RegexOptions.IgnoreCase), report);

        // AND IT ROUTES RATHER THAN RESTATES. Both siblings are read, not assumed (§12.10).
        Assert.Contains("/tumors/ependymoma", report, StringComparison.Ordinal);
        Assert.Contains("/tumors/acoustic-neuroma", report, StringComparison.Ordinal);

        var acoustic = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "acoustic-neuroma.md")));
        Assert.Matches(new Regex(
            @"lists this growth without\s+printing a grade beside it", RegexOptions.IgnoreCase),
            acoustic);

        var ependymoma = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "ependymoma.md")));
        Assert.Contains("It is grade 2 now", ependymoma, StringComparison.Ordinal);
        Assert.DoesNotContain("It is grade 2 now", Plain, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageIsNotABrainTumorPageAndSaysSoWithoutOverclaiming()
    {
        // The taxonomy is explicit that a spinal cord tumor is NOT a brain tumor and must
        // never surface under a brain filter; WI-412 pinned it with a test and this item
        // must not undo it. Read rather than assumed.
        var taxonomy = File.ReadAllText(
            Path.Combine(CuratedPage.BlocksRoot, "..", "taxonomy.yml"));
        Assert.Matches(new Regex(
            @"NOT a child of any brain type and must never surface under a brain filter",
            RegexOptions.IgnoreCase), taxonomy);

        Assert.Matches(new Regex(@"\*\*This is not a brain tumor\.\*\*"), CuratedPage.ReaderText(Page));

        // AND THE FORMULATION IS THE ONE THE SOURCES SUPPORT. No fetched source says a cord
        // tumor IS a brain tumor; what they support is the CNS framing. The page must not
        // overclaim in the other direction either — these are not unrelated illnesses.
        var what = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(WhatHeading)));
        Assert.Matches(new Regex(@"two parts\s+of one system, called the central nervous system",
            RegexOptions.IgnoreCase), what);
        Assert.Matches(new Regex(@"same kinds of specialists\s+look after both",
            RegexOptions.IgnoreCase), what);
    }

    // ------------------------------------------------- the shared corpus guards

    [Fact]
    public void ThePageCarriesTheEscalationBlockAndItsTiersAgreeWithTheCorpus() =>
        // Composed and section-scoped. Moving [ESCALATION] out of the symptoms section
        // would keep the suite green without this (WI-512: presence was never the
        // property, position was).
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void ThePageDoesNotGrowAnEscalationListOfItsOwn() =>
        // STRUCTURAL, not a phrase ban: an urgency-flavoured lead-in followed by two or
        // more symptom bullets. This page is almost entirely warning signs, so the
        // temptation to build a second tier list here is the strongest in the corpus — and
        // a second list is a second list to keep in step.
        CuratedPage.AssertNoEscalationList(Page, Slug);

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage() =>
        // PARAGRAPH-scoped and COMPOSED, and the named paragraph is the POSITIVE half:
        // an iterate-and-check guard is green on a page it never looked at (WI-517).
        //
        // THAT PARAMETER EARNED ITS KEEP HERE. An earlier draft named four paragraphs and
        // the helper reported `Collection: []` — it had examined NOTHING on the whole
        // composed page, because a paragraph must contain a symptom word AND a normaliser
        // word to be examined, and this page's reassurance sat one paragraph away from the
        // symptom it was reassuring about. That is not a test artefact: a reader told a
        // post-operative dip is common, in a paragraph that never names what to do if it
        // is NOT the dip, has been given the reassuring half on its own. The page now
        // carries symptom, reassurance and tier together, which is what makes this bind.
        //
        // ONLY ONE PARAGRAPH IS NAMED, DELIBERATELY. The pain, spasticity and skin
        // sections reassure about real things too, and the shared helper structurally
        // cannot examine them: its symptom vocabulary has no "pain", "spasm", "stiffness"
        // or "skin". Naming them here would fail rather than guard. Recorded for /pm —
        // three genuine reassurances on this page are outside that guard's reach.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug,
            "common enough to have a name");

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus()
    {
        // The first draft collided with ten pages — three sentences lifted from
        // /tumors/ependymoma, two from /tumors/meningioma, and one each from four others —
        // because §12.9's template travels with the previous hub's sentences. The guard is
        // BIDIRECTIONAL: borrowing a sibling's prose turns THAT page's suite red, which is
        // how two shipped pages failed before it was fixed.
        //
        // THE ONLY EXEMPT RUNS ARE EMERGENCY THRESHOLDS, and they are PHRASES rather than
        // whole sentences. §12.10 is explicit that rewording one of these IS the defect —
        // "a rewrite is a rewording of an emergency threshold" — so /tumors/meningioma and
        // /tumors/brain-metastases carry the same class of entry for the same reason.
        //
        // /review round 2 CORRECTED THE SHAPE OF THIS LIST. The first version registered
        // whole sentences from the sibling pages, and neither appeared on THIS page in
        // those words. The helper filters with `entry.Contains(shingle)`, so an entry
        // longer than anything the page actually carries silently widens the exemption
        // over windows that were never there. An allowlist that excuses collisions you do
        // not have is worse than none, because it hides the ones you do.
        //
        // The loop below is what makes that impossible to repeat: a vacuous entry now
        // fails here instead of quietly broadening the exemption.
        //
        // Nothing else is exempt. Every other collision this page had was REWORDED,
        // including one against /tumors/ependymoma that arrived from paraphrasing around
        // its name instead of simply naming it.
        foreach (var shared in DeliberatelyShared)
        {
            Assert.Contains(shared, Plain, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug, DeliberatelyShared);
    }

    // -------------------------------------------------------------- house rules

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAndNoForbiddenNumbers()
    {
        // §12.5: concepts, never figures. The sources for this page publish survival
        // figures, radiation doses in Gy, follow-up intervals and a symptom-to-diagnosis
        // average, and none of them reaches the page.
        foreach (var banned in new[]
                 {
                     @"\b\d+(?:\.\d+)?\s*(?:Gy|gray)\b",
                     @"\b(?:survival|survive|live for)\b[^.]{0,40}\b\d",
                     // NO TRAILING \b AFTER `%`. The first version had one, and it could
                     // never match: `%` and the space after it are both non-word
                     // characters, so there is no boundary between them. The canary below
                     // is what exposed it — a ban proved by a canary that itself could not
                     // fire is a ban proved by nothing.
                     @"\b\d+(?:\.\d+)?\s*(?:%|percent\b)",
                     @"\b\d+\s*(?:to|-|–)\s*\d+\s*months\b",
                 })
        {
            Assert.DoesNotMatch(new Regex(banned, RegexOptions.IgnoreCase), Plain);
        }

        // THE CANARY, because a ban that cannot fire proves nothing. Each pattern is shown
        // matching something (§12.8, WI-523).
        Assert.Matches(new Regex(@"\b\d+(?:\.\d+)?\s*(?:Gy|gray)\b"), "A dose of 50 Gy is used.");
        Assert.Matches(new Regex(@"\b\d+(?:\.\d+)?\s*(?:%|percent\b)"), "About 41% recover.");
        Assert.Matches(new Regex(@"\b\d+\s*(?:to|-|–)\s*\d+\s*months\b"),
            "Symptoms last 6 to 37 months.");
        // The fourth pattern had no canary while the other three did (/review round 1).
        Assert.Matches(new Regex(@"\b(?:survival|survive|live for)\b[^.]{0,40}\b\d",
            RegexOptions.IgnoreCase), "Median survival is 5 years.");

        // THE ORIENTING DURATIONS THAT ARE DELIBERATELY IN (§12.4 R1), each asserted to
        // still be present — so a later widening of the bans above fails here instead of
        // silently stripping the page's most actionable instructions.
        foreach (var allowed in new[]
                 {
                     @"every ten to fifteen minutes",
                     @"every two hours or less",
                     @"at least twice a day",
                     @"three hours of therapy a day, five days a week",
                     @"within a day\s+or two of the symptoms starting",
                 })
        {
            Assert.Matches(new Regex(allowed, RegexOptions.IgnoreCase), Plain);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // Six of this item's sources are British and two more are US pages quoting British
        // services. Strip-then-scan, the stronger form (§12.10).
        var reader = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // PAGE-LOCAL, and "straight away" is here for a reason the shared gate cannot
        // carry: it was run over the corpus at WI-531 and REJECTED from CuratedPage.
        // BritishForms, because it is live on /treatments/chemotherapy's fever rule. It is
        // still British idiom, and the STUB this page replaces contained it.
        foreach (var idiom in new[]
                 { "A&E", "GP", "straight away", "out of hours", "being sick", "999", "come round" })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }

        // And no British care pathway, which is the half an idiom list does not catch. The
        // only source addressing out-of-hours contact is Macmillan, whose facts are usable
        // and whose pathway is not.
        foreach (var pathway in new[] { "MSCC coordinator", "alert card", "999" })
        {
            Assert.DoesNotContain(pathway, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheSeventeenSectionsAreAllPresentAndInOrder()
    {
        // §12.3's order, asserted as an ORDER rather than a set: a page can carry every
        // heading and still meet the reader with outlook before treatment.
        string[] expected =
        [
            ShortHeading, WhatHeading, GradeHeading, WhereHeading, SymptomHeading,
            DiagnosisHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
            ScansHeading, RecurrenceHeading, CauseHeading, OutlookHeading, CareHeading,
            QuestionsHeading, SupportHeading,
        ];

        Assert.Equal(expected, Headings());

        // SEIZURES ARE DELIBERATELY NOT IN SLOT 9's HEADING. §12.3 words it "work, driving,
        // seizures and tiredness" and every brain hub follows that, but a cord tumor does
        // not cause seizures — so the template is adapted rather than copied, the way
        // /tumors/pediatric-brain-tumor adapts its symptoms heading for a parent.
        Assert.DoesNotContain("seizure", LifeHeading, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"\bseizures?\b", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(LifeHeading))));
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAndNoFigureInWords()
    {
        // §12.5. EVERY LINE-BREAK PATTERN IS `\r?\n`: this repo hands working trees CRLF
        // while CI sees LF, and a test that cannot match on CRLF fails for free — the
        // break harness cannot see that class of bug.
        var raw = RawSection(OutlookHeading);

        Assert.Matches(new Regex(@"\A[ \t]*\r?\n[ \t]*:::outlook"), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n?\s*\z"), raw);

        var inside = CuratedPage.Flatten(
            Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline).Groups[1].Value);
        Assert.False(string.IsNullOrWhiteSpace(inside), "the gate is empty");

        // Nothing sits outside the gate but the heading. The §12.5 warning is rendered BY
        // the container, so writing it by hand would show it to the reader twice.
        var outside = raw.Replace(
            Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
            StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate: " + outside.Trim());

        // No digits at all in there, and no figure written as words either — WI-527's
        // lesson that a digit ban is not a prognosis ban.
        Assert.DoesNotMatch(new Regex(@"\d"), inside);
        Assert.DoesNotMatch(new Regex(
            @"\b(?:half|third|quarter|most people live|years to live)\b", RegexOptions.IgnoreCase),
            inside);
    }

    [Fact]
    public void TheEvidenceThatIsBorrowedFromAnotherPopulationIsLabelledAsSuch()
    {
        // THE HONESTY SENTENCE THE RESEARCH DEMANDED. All the good patient-facing material
        // on bladder, bowel, skin and pain was written for spinal cord INJURY, not tumor.
        // The brief permits using it because the functional problem is the same one; it
        // does not permit implying tumor-specific evidence exists.
        var life = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(LifeHeading)));
        Assert.Matches(new Regex(
            @"worked out for people whose spinal cord was damaged\s+in other ways",
            RegexOptions.IgnoreCase), life);

        // And the caregiver section says it again, because that reader meets the same
        // material and arrives by a different route.
        var care = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(CareHeading)));
        Assert.Matches(new Regex(
            @"not\s+because it was studied in people with tumors", RegexOptions.IgnoreCase), care);
    }

    [Fact]
    public void TheSourcesAreRealAndTheUnreachableOneIsNotCited()
    {
        var front = CuratedPage.FrontMatter(Page);

        // §12.1: the barred source the stub carried is gone. An NCI BRAIN page cited for a
        // spinal tumor, on the page whose central claim is that this is not a brain tumor.
        // `\s*$` WITH Multiline, NOT A "\n" NEEDLE. `CuratedPage.Read` is a bare
        // File.ReadAllText, this repo is core.autocrlf=true, and the file on disk is CRLF —
        // so a literal "\n" could never match locally and the guard was live only on CI
        // (/review round 1). .NET's multiline `$` does not match before a `\r` either,
        // which is why the whitespace class has to be the thing that eats it.
        Assert.DoesNotMatch(
            new Regex(@"cancer\.gov/types/brain\s*$", RegexOptions.Multiline), front);

        // §12.14: a citation that was never reached reads like a handled claim, so the
        // front matter NAMES cIMPACT-NOW update 7 as unreachable and records that no claim
        // rests on it.
        //
        // THE FIRST VERSION OF THIS GUARD BANNED THE WORD, and so fired on the very
        // sentence §12.14 asked for — a guard failing the correct case, which is this
        // corpus's most-repeated defect. What must be absent is the CITATION, not the
        // discussion: no source entry may carry that URL.
        Assert.Matches(new Regex(@"cIMPACT-NOW", RegexOptions.IgnoreCase), front);

        // THE BREAK HARNESS FOUND THIS BAN UNABLE TO SEE A REAL CITATION. It read
        // `^\s*-\s*url:.*cimpact` — and NEITHER of that paper's URLs contains the string
        // "cimpact": they are europepmc.org/article/MED/32502305 and
        // pubmed.ncbi.nlm.nih.gov/32502305. The ban could only ever have caught a citation
        // whose URL happened to spell the name, which the real one does not. A planted
        // entry in exactly the form anyone would write walked straight through.
        //
        // What identifies it is the PMID or the TITLE, so the ban binds to those — and to
        // a source LINE rather than to any mention, because the front matter must keep
        // naming cIMPACT-NOW as unreachable (§12.14, asserted immediately above). Banning
        // the word outright is what the first version of this guard did, and it fired on
        // the very sentence §12.14 asks for.
        var citedUnreachable = new Regex(
            @"^\s*-?\s*(?:url|title):.*(?:cimpact|32502305)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Assert.Matches(citedUnreachable, "  - url: https://europepmc.org/article/MED/32502305\n");
        Assert.Matches(citedUnreachable, "    title: \"cIMPACT-NOW update 7\"\n");
        Assert.Matches(citedUnreachable,
            "  - url: https://pubmed.ncbi.nlm.nih.gov/32502305/\n");
        Assert.DoesNotMatch(citedUnreachable,
            "  # NOTHING RESTS ON IT: cIMPACT-NOW update 7 was never fetched.\n");
        Assert.DoesNotMatch(citedUnreachable, front);

        // mayoclinic.org is barred corpus-wide, and AHFS/MedlinePlus monographs are barred
        // by PLAN.md §5.
        foreach (var barred in new[] { "mayoclinic.org", "ahfs", "medlineplus" })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        // NO BRACKETED BLOCK NAME ANYWHERE IN FRONT MATTER. ContentBlocks.Directives walks
        // every line outside a fenced code block, front matter included, so a bracketed
        // name in a comment registers as a real directive — which would break the exact-set
        // ruling above and CaregiverSectionTests' raw IndexOf.
        Assert.DoesNotMatch(new Regex(@"^\[[A-Z-]+\]\s*$", RegexOptions.Multiline), front);
    }
}

[Collection(DatabaseCollection.Name)]
public sealed class SpinalCordTumorPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/spinal-cord-tumor";

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// The connection string is pushed in, exactly as every other render fixture does it.
    /// Taking the factory as-is works on a developer machine because user-secrets supplies
    /// one, and fails on CI where nothing does — a fixture that only works where the
    /// secrets are is not a test.
    /// </summary>
    public SpinalCordTumorPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Spinal cord tumor", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFiveBlocksComposeAndTheThreeExcludedOnesLeaveNoTrace()
    {
        // THE HALF THE CONTENT-SIDE GUARDS STRUCTURALLY CANNOT SEE. They read directive
        // NAMES out of the source file, so they stay green if composition itself breaks.
        // Only a rendered assertion catches that, and only from both directions.
        var html = await Client.GetStringAsync(Url);

        foreach (var composed in new[]
                 {
                     "Call an ambulance",                 // ESCALATION
                     "You are allowed to ask questions",  // CAREGIVER
                     "nobody knows the cause",            // CAUSES
                     // TUMOR-BOARD, and the string is chosen to be TOOLTIP-FREE. The first
                     // version used "A tumor board is a meeting", which fails in HTML even
                     // though the block composes perfectly: "tumor board" is a glossary
                     // term, so a popover injects markup into the middle of the phrase and
                     // no contiguous match survives.
                     "What they decide is advice, not an order",
                     // CROSSWALK, and it must be a string ONLY that block says. The first
                     // version used "changed in 2021", which this page's own grade section
                     // also contains -- so it would have passed whether or not the block
                     // composed at all. That is the exact defect class §12.10 records: if a
                     // test's subject is the block, assert something only the block says.
                     "NOS means the tests needed to be more exact were not available",
                 })
        {
            Assert.Contains(composed, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var excluded in new[]
                 {
                     "The skull is a closed box",                        // MECHANISM
                     "right-away call, not a same-day one",              // SPINAL-CORD
                     "A day or two after surgery low at the back",       // POSTERIOR-FOSSA
                 })
        {
            Assert.DoesNotContain(excluded, html, StringComparison.OrdinalIgnoreCase);
        }

        // And no directive survives into the HTML unresolved.
        foreach (var marker in new[] { "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                                       "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]", ":::" })
        {
            Assert.DoesNotContain(marker, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await Client.GetStringAsync(Url);

        // SCOPED to the outlook section: matching the first <details> in the document
        // reads whatever disclosure the layout puts above the article.
        var start = html.IndexOf("What might happen over time", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the outlook heading did not render");

        var gate = Regex.Match(html[start..], @"<details[^>]*>");
        Assert.True(gate.Success, "the outlook gate did not render as a details element");
        Assert.DoesNotContain(" open", gate.Value, StringComparison.Ordinal);

        Assert.Equal(1, Regex.Matches(html, "The next part is about outlook").Count);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(Client, Url, "where-to-get-support",
            "/get-help-now", "/tumors/ependymoma");

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. The listing is a property of two files
        // agreeing, and the taxonomy's rule is that this one is listed SEPARATELY rather
        // than among the brain types.
        var html = await Client.GetStringAsync("/tumors");

        Assert.Contains("spinal-cord-tumor", html, StringComparison.Ordinal);
        Assert.Contains("Spinal cord tumor", html, StringComparison.Ordinal);
    }
}
