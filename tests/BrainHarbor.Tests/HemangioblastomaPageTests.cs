using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-547: <c>/tumors/hemangioblastoma</c>, a NEW page, and the LAST taxonomy type
/// to get one. Before this item <c>/tumors</c> rendered "We are still writing this
/// one" for this row and no other; see <see cref="TumorsPageTests"/> for the ruling
/// on the assertion that depended on that being true.
///
/// TWO READERS WEAR ONE DIAGNOSIS. Most people have ONE tumor, found as an adult,
/// and an operation is often the end of it. About 1 in 4 have the inherited
/// condition VHL: younger, more than one tumor over a lifetime, checks for life,
/// and a pill that exists only for them. Several guards below assert that a claim
/// keeps its group, because a sentence that drifts from one to the other passes
/// every presence check and misleads half the readers.
///
/// THE URGENCY RULING IS BUILT ON A SCORED ABSENCE (WI-544/545/546 shape).
/// Sixteen readable pages about this tumor give no sign-level 911 or emergency-room
/// rule; three more could not be read and are counted as unknown. So every urgent
/// line belongs to a SIGN and names its source. Evidence log:
/// <c>.claude/work_files/wi547/plan.md</c>.
///
/// OVER-REASSURANCE IS THE LIVE RISK (§12.12), not fear: "benign", "can cure",
/// "outcomes are excellent" and "may be considered cured" are on the sources
/// themselves, so the cure ban and the benign counterweight are the guards a later
/// editor is likeliest to wear down.
/// </summary>
public sealed class HemangioblastomaPageContentTests
{
    private const string Slug = "tumors/hemangioblastoma";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a hemangioblastoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string DiagnosisHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string RecurrenceHeading = "If it comes back";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CareHeading = "For the person caring for someone with this";
    private const string QuestionsHeading = "What to ask your team";
    private const string SupportHeading = "Where to get support";

    private static string Page => CuratedPage.Read("tumors", "hemangioblastoma.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    /// <summary>
    /// The reader text of the COMPOSED page, flattened — so the bans see the words
    /// the shared blocks contribute, which is how the reader meets them (§12.10).
    /// </summary>
    private static string Plain => CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>A section of the RAW page, unflattened, for POSITION assertions.</summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    /// <summary>One <c>###</c> subsection of a raw section, up to the next <c>###</c>.</summary>
    private static string Subsection(string heading, string subheading)
    {
        var match = Regex.Match(
            RawSection(heading),
            $@"^### {Regex.Escape(subheading)}\s*$(.*?)(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"'{heading}' has no '### {subheading}' subsection");
        return match.Groups[1].Value;
    }

    private static string Flat(string text) => CuratedPage.Flatten(CuratedPage.ReaderText(text));

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$").Select(m => m.Groups[1].Value.Trim())];

    /// <summary>The reader text outside the outlook gate.</summary>
    private static string OutsideTheGate()
    {
        var gate = Regex.Match(Composed, @":::outlook.*?:::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate has gone, so 'outside it' means nothing");
        return CuratedPage.Flatten(CuratedPage.ReaderText(Composed.Remove(gate.Index, gate.Length)));
    }

    // ------------------------------------------------------------------ the structure

    [Fact]
    public void TheSeventeenSectionsAreAllPresentAndInOrder()
    {
        // THE HARNESS BASELINE: fails if the file is corrupt in any way that would
        // make every other result below meaningless.
        Assert.Equal(
            [ShortHeading, WhatHeading, GradeHeading, WhereHeading, SymptomHeading,
             DiagnosisHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
             ScansHeading, RecurrenceHeading, CauseHeading, OutlookHeading, CareHeading,
             QuestionsHeading, SupportHeading],
            Headings());
    }

    // ------------------------------------------------------------------ the urgency ruling

    [Fact]
    public void TheEmergencyAbsenceIsScopedBySubjectAndTheUnreadablePagesAreUnknown()
    {
        var section = Flat(RawSection(SymptomHeading));

        // SCOPED BY SUBJECT — "pages written about this tumor" — never by publisher.
        // WI-545 shipped a publisher-scoped version and its own next screen falsified
        // it; this page quotes Cleveland Clinic's emergency-room rule two paragraphs
        // later, so the same trap is live here.
        Assert.Matches(new Regex(
            @"Sixteen pages written about this tumor, or about\s+the brain and spine tumors of VHL, were read and used",
            RegexOptions.IgnoreCase), section);

        // "AND USED" IS LOAD-BEARING, and the first draft dropped it while the front
        // matter's ruling claimed it was there (/review round 1). Readable pages DO
        // give a sign-level rule — the syndicated text this corpus may not reuse — so
        // an unqualified "we read them all" would tell the reader an absence without
        // telling them the set was filtered.
        Assert.Matches(new Regex(
            @"We also left out one page family whose rules we are not allowed to\s+reuse",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"Not one of those sixteen ties a warning sign to 911 or to an emergency\s+room",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"no rule below is a rule about the tumor", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"each names who gives it", RegexOptions.IgnoreCase), section);

        // THE COUNT IS PART OF THE CLAIM (WI-546 /review round 1: nothing pinned it,
        // so "we read forty pages" would have passed).
        Assert.Matches(new Regex(@"\bSixteen\b"), section);

        // THE UNREADABLE PAGES ARE UNKNOWN, NOT SILENT. A 403 is neither presence nor
        // absence, and folding them into the count would overstate the sweep.
        Assert.Matches(new Regex(
            @"Three further pages would not open for us", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"they may say\s+something we have not seen", RegexOptions.IgnoreCase), section);

        // THE PUBLISHER-SCOPED FORMS STAY OUT: a negation bound to the publishers
        // rather than to the subject. Two canaries, so the ban cannot pass by being
        // unable to fire.
        var publisherScoped = new Regex(
            @"\b(no|not one|none) of (them|the)\s+(big\s+)?(hospital|clinic|charit(y|ies)|site|page)",
            RegexOptions.IgnoreCase);
        Assert.Matches(publisherScoped, "none of the hospital pages give an emergency rule");
        Assert.Matches(publisherScoped, "not one of the charities says when to go in");
        Assert.DoesNotMatch(publisherScoped, section);

        // AND NEVER A UNIVERSAL: three pages were unreadable, and the Mayo-syndicated
        // text (barred, so unusable here) does give a rule.
        var universal = new Regex(
            @"\bno (source|page|organization|guideline)s? anywhere\b|\bnobody says\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(universal, "no source anywhere gives this rule");
        Assert.DoesNotMatch(universal, section);
    }

    [Fact]
    public void EachUrgentRuleKeepsItsOwnSignItsOwnVenueAndItsOwnSource()
    {
        // TWO RULES, AND POSITION IS THE PROPERTY: each venue must sit after its own
        // sign and before the next sign starts, or a reader takes one rule's venue for
        // the other's (WI-545's vacuous-adjacency finding).
        //
        // FLATTENED, NOT RAW (/review round 1 BLOCKER). An Ordinal IndexOf for a
        // needle with a newline in it is a CRLF landmine: this page is LF only while
        // it is untracked, and `* text=auto` with core.autocrlf=true checks it out
        // CRLF the moment it is committed, so the lookup would return -1 and this
        // test would go red on correct content — the SAME defect this branch repairs
        // in CnsGermCellTumorPageTests.
        var raw = CuratedPage.Flatten(RawSection(SymptomHeading));

        int At(string needle)
        {
            var i = raw.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(i >= 0, $"the symptoms section has lost '{needle}'");
            return i;
        }

        var bleed = At("**A sudden, very bad headache, unlike any you have had.**");
        var bleedVenue = At("says to call 911 right away");
        var cord = At("**If your tumor is in the spinal cord.**");
        var cordSameDay = At("means calling your team today");
        var cordEr = At("says to get to an emergency room immediately");

        Assert.True(bleed < bleedVenue && bleedVenue < cord,
            "the 911 venue has left the sudden-headache rule it belongs to");
        Assert.True(cord < cordSameDay && cordSameDay < cordEr,
            "the cord rules are out of order: the same-day tier must come before the stronger one");

        // THE RARITY COMES FIRST, so the page does not teach a reader to read every
        // headache as a bleed. §12.6: warn before you disclose, and never lead with
        // the frightening half.
        var rarity = At("Bleeding from this tumor is rare");
        Assert.True(bleed < rarity && rarity < bleedVenue,
            "the rarity must sit between the sign and its 911 venue");

        // AND THE 911 RULE SAYS IT OUTRANKS THE COMPOSED BLOCK'S SAME-DAY TIER for a
        // bad headache, which the reader meets first (/review round 1). Without this
        // the page files one headache in two tiers and never says which wins.
        Assert.Matches(new Regex(
            @"that beats the same-day rule for a bad headache in the list above",
            RegexOptions.IgnoreCase), raw);

        // EACH RULE NAMES WHO GIVES IT, inside its own paragraph.
        Assert.Contains("MedlinePlus", raw[bleed..cord], StringComparison.Ordinal);
        Assert.Contains("Cleveland Clinic", raw[cord..], StringComparison.Ordinal);

        // THE BLEED RULE IS NOT PUT IN THE TUMOR PAGES' MOUTH. Its 911 instruction
        // comes from a page about the SIGN (stroke), which is the whole point of the
        // scored absence above — so the sentence carrying 911 must name that source.
        Assert.Matches(new Regex(
            @"MedlinePlus lists a sudden, severe headache with no known cause as a stroke sign",
            RegexOptions.IgnoreCase), raw[bleed..cord]);
    }

    [Fact]
    public void TheCordRulesFollowTheHubThatOwnsTheSubjectAndNotTheSharedBlock()
    {
        // RULING 3, AND IT IS A REAL SPLIT IN THE CORPUS. The shared [SPINAL-CORD]
        // block files NEW LIMB WEAKNESS as a right-away call. /tumors/spinal-cord-tumor
        // owns the primary cord tumor and files it as SAME-DAY, reserving the emergency
        // room for the cauda equina picture — and its own scored absence names THIS
        // tumor. This page follows the page that owns the subject.
        //
        // The sibling is READ, not assumed (§12.10): if /tumors/spinal-cord-tumor
        // re-tiers, this goes red and the ruling gets re-argued rather than silently
        // drifting apart.
        var sibling = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "spinal-cord-tumor.md")));
        Assert.Contains(
            "new weakness in your legs, new numbness or new trouble walking are a same-day call",
            sibling, StringComparison.OrdinalIgnoreCase);

        var section = Flat(RawSection(SymptomHeading));
        Assert.Matches(new Regex(
            @"New weakness, new numbness, unsteady\s+walking, or new back or neck pain means calling your team today",
            RegexOptions.IgnoreCase), section);

        // BACK OR NECK PAIN IS IN THE TIER, and it was missing until /review round 3.
        // Two of this page's own sources list it among the cord signs, and ruling 1
        // was meanwhile claiming this page gave that reader something better than the
        // source sentence it declines to carry.
        //
        // THE TIER IS THIS PAGE'S OWN, and that is a recorded divergence (/review
        // round 4): /tumors/spinal-cord-tumor lists this pain but gives it no tier,
        // while the shared cord block files new or worse back or neck pain at
        // right-away. Same-day therefore sits BELOW the corpus's other tiering of the
        // same sign, which is the safe direction for a page that adds one.
        Assert.Matches(new Regex(
            @"both add new back or neck pain", RegexOptions.IgnoreCase),
            Flat(RawSection(SymptomHeading)));

        // AND THE STRONGER PICTURE KEEPS ALL THREE OF ITS SIGNS. Dropping one is the
        // under-triage direction, and the bladder/bowel sign is the one most often lost.
        foreach (var sign in new[]
                 {
                     "Losing control of peeing or pooping",
                     "numbness in the parts of you that touch a",
                     "weakness that is worse each hour",
                 })
        {
            Assert.Contains(sign, section, StringComparison.OrdinalIgnoreCase);
        }

        // THE ORDER OF AUTHORITY IS STATED, because this page also composes the
        // escalation block, whose same-day list a reader meets first.
        Assert.Matches(new Regex(
            @"Those three beat every other rule on this page", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheBrainstemParagraphCarriesATierAndKeepsItsSourcesUrgencyHalf()
    {
        // /review ROUND 1 BLOCKER. The first draft said brainstem symptoms "can come
        // on faster and be more serious" and then stopped: a frightening sentence with
        // no action (§12.6), on the page's most dangerous location, with the action
        // sentence beside it scoped to a DIFFERENT location. And it took half of its
        // source: The Brain Tumour Charity says such a tumor has "more chance of
        // symptoms being severe AND NEEDING EMERGENCY ATTENTION". Dropping the second
        // half is the under-triage direction.
        //
        // The shared normalisation guard structurally cannot catch this: it only
        // examines paragraphs that REASSURE, and this one never did.
        var raw = CuratedPage.Flatten(RawSection(SymptomHeading));
        var start = raw.IndexOf("**In the brainstem**", StringComparison.Ordinal);
        Assert.True(start >= 0, "the brainstem paragraph has gone");
        var end = raw.IndexOf("**In the spinal cord", start, StringComparison.Ordinal);
        Assert.True(end > start, "the brainstem paragraph no longer ends where expected");
        var paragraph = raw[start..end];

        // THE SOURCE'S URGENCY HALF, credited.
        Assert.Contains("Brain Tumour Charity", paragraph, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"can need emergency care", RegexOptions.IgnoreCase), paragraph);

        // AND A TIER THE READER CAN ACT ON, since the source names no sign and no
        // venue — said out loud rather than implied.
        Assert.Matches(new Regex(@"It does not say which signs or where to go",
            RegexOptions.IgnoreCase), paragraph);
        Assert.Matches(new Regex(@"call to your team today", RegexOptions.IgnoreCase), paragraph);

        // THE ONSET RULE IS ATTACHED TO THE UNCONDITIONAL BULLET, AND ONLY TO IT.
        // This clause has now been wrong in BOTH directions, which is why it is
        // pinned this precisely:
        //
        //   /review round 2 (under-triage): "use the ambulance list below if it
        //   comes on suddenly" gated the WHOLE list on suddenness, so a reader whose
        //   breathing or swallowing failed over a week read their tier as a phone
        //   call — and breathing trouble is this location's signature danger.
        //
        //   /review round 3 (over-triage): the fix then said ANYTHING on the list
        //   applies "however it started", which is false of the block's own
        //   "SUDDENLY not being able to speak, move one side, or see" — a bullet
        //   deliberately gated, whose gradual forms the same block files as
        //   same-day. AssertEscalationTiers fails any page that escalates the
        //   gradual deficit to an ambulance; said in free prose, it is invisible to
        //   that helper.
        //
        // So the positive names the bullet that IS unconditional, and the ban covers
        // both a blanket override and a blanket gate.
        Assert.Matches(new Regex(
            @"trouble breathing or\s+choking is an ambulance call however it started",
            RegexOptions.IgnoreCase), paragraph);

        var blanket = new Regex(
            @"(anything|everything) on the ambulance list[^.]{0,60}however it started"
            + @"|ambulance list[^.]{0,40}\bif it (comes on|is) sudden"
            + @"|ambulance list[^.]{0,40}\b(when|if) (any of these|they) start",
            RegexOptions.IgnoreCase);
        Assert.Matches(blanket, "anything on the ambulance list below is an ambulance call however it started");
        Assert.Matches(blanket, "use the ambulance list below if it comes on suddenly");
        Assert.Matches(blanket, "use the ambulance list below if any of these start suddenly");
        Assert.DoesNotMatch(blanket, Plain);

        // AND THE READER IS ROUTED TO THE PAGE'S OWN 911 RULE, not only to the block's
        // list — the sudden severe headache is this page's addition and is not on it.
        Assert.Matches(new Regex(
            @"the two rules after it", RegexOptions.IgnoreCase), paragraph);

        // THE UNSOURCED SPEED CLAIM STAYS OUT: no capture says brainstem symptoms come
        // on faster, and one review points the other way about growth rate.
        var speed = new Regex(
            @"brainstem[^.]{0,80}\b(faster|more quickly|quicker)\b"
            + @"|\b(faster|more quickly)\b[^.]{0,40}brainstem",
            RegexOptions.IgnoreCase);
        Assert.Matches(speed, "in the brainstem, symptoms can come on faster");   // canary
        Assert.DoesNotMatch(speed, Plain);
    }

    [Fact]
    public void ThePageCarriesTheEscalationBlockAndItsTiersAgreeWithTheCorpus() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage() =>
        // THE POSITIVE HALF names the paragraph this must actually have examined
        // (WI-517: an iterate-and-check guard is green on a page it never looked at).
        //
        // ONE PARAGRAPH, NOT TWO, AND THE SECOND ONE'S REMOVAL IS THE POINT. The
        // balance paragraph used to say unsteadiness was "common ... for a while"
        // after surgery, so this guard examined it. /review round 1 found that
        // frequency claim unsourced — the research pass had recorded the gap in as
        // many words — so it came out, the paragraph stopped reassuring, and naming it
        // here started failing. That is the guard working, exactly as in WI-546: the
        // fix is to stop claiming it was examined, NOT to put an unsourced word back
        // so a guard looks busier.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug, "Being worn out is common");

    [Fact]
    public void TheScopeNoteForTheGeneralPartComesBeforeTheBlockItIntroduces()
    {
        // §12.6: warn before you disclose. The mechanism block is written for every
        // brain tumor, and the sentence that hands the reader over to it must come
        // first, not as a correction met too late (WI-545's first rendered read).
        var where = RawSection(WhereHeading);
        var note = where.IndexOf("The general part below", StringComparison.Ordinal);
        var mechanism = where.IndexOf("[MECHANISM]", StringComparison.Ordinal);

        Assert.True(note >= 0 && mechanism > note,
            "the hand-over sentence must come BEFORE the block it introduces");
    }

    // -------------------------------------------------------------- the block rulings

    [Fact]
    public void ExactlyFiveBlocksAreIncludedAndTheThreeExclusionsAreAClosedSet()
    {
        // AN EXACT SET: a block added later fails here and has to be ruled on.
        Assert.Equal(
            ["caregiver", "causes", "escalation", "mechanism", "tumor-board"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        Assert.Contains("[MECHANISM]", Section(WhereHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSpinalCordBlockIsExcludedBecauseItsOwnSentenceIsAboutCancer()
    {
        // THE EXCLUSION MOST LIKELY TO BE "CORRECTED" BY A LATER EDITOR, because this
        // tumor really does grow in the cord. Asserted against the BLOCK'S OWN WORDS:
        // its rule is credited to a sentence about CANCER pressing on the cord, and
        // this tumor is not cancer, so the credit would not survive the move.
        var block = Flat(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md")));

        foreach (var blockOnly in new[]
                 {
                     "When cancer presses on the spinal cord",
                     "right-away call, not a same-day one",
                 })
        {
            Assert.Contains(blockOnly, block, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("spinal-cord", ContentBlocks.DirectBlockNames(Page));

        // AND THE PAGE DOES THE BLOCK'S JOB ITSELF rather than leaving the cord reader
        // with nothing: its own tiers, and the door to the hub that owns them.
        var section = Flat(RawSection(SymptomHeading));
        Assert.Matches(new Regex(@"If your tumor is in the spinal cord", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tumors/spinal-cord-tumor", RawSection(SymptomHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePosteriorFossaBlockIsExcludedBecauseItsWordsAreAboutChildren()
    {
        // The block describes children after surgery low at the back of the brain.
        // This tumor is found mostly in adults, and no source recorded for this page
        // describes that syndrome after a hemangioblastoma operation.
        var block = Flat(File.ReadAllText(
            Path.Combine(CuratedPage.BlocksRoot, "posterior-fossa-syndrome.md")));

        const string BlockOnly = "Most children slowly get better";
        Assert.Contains(BlockOnly, block, StringComparison.Ordinal);
        Assert.DoesNotContain(BlockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        Assert.DoesNotContain("posterior-fossa-syndrome", ContentBlocks.DirectBlockNames(Page));
    }

    [Fact]
    public void TheCrosswalkBlockIsExcludedBecauseItsGeneBulletIsFalseHere()
    {
        // Its gene bullet is false for this name: this tumor is named by what it is
        // made of, and the name carries no gene result. The page answers the old names
        // itself instead.
        var block = Flat(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));

        foreach (var blockOnly in new[]
                 {
                     "Gene results are now part of the name",
                     "Only the way of writing it did",
                 })
        {
            Assert.Contains(blockOnly, block, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("crosswalk", ContentBlocks.DirectBlockNames(Page));

        var report = Flat(RawSection(ReportHeading));
        Assert.Matches(new Regex(
            @"they are no longer the recommended\s+names", RegexOptions.IgnoreCase), report);
    }

    [Fact]
    public void TheCausesBlockIsSharpenedRatherThanContradicted()
    {
        // THE BLOCK SAYS "a small number of people do have an inherited condition".
        // For THIS tumor the share is about 1 in 4, so the block's general sentence
        // could leave a reader thinking the test is not for them. The page's own
        // answer has to come after it and say the share out loud — and it must not
        // deny the block, which composes into this very section.
        var causes = RawSection(CauseHeading);
        var block = causes.IndexOf("[CAUSES]", StringComparison.Ordinal);
        var sharpened = causes.IndexOf("one part of that answer matters more than usual",
            StringComparison.Ordinal);

        Assert.True(block >= 0, "the causes block has gone");
        Assert.True(sharpened > block,
            "the page's own VHL answer must come AFTER the general block it sharpens");

        var flat = Flat(causes);
        Assert.Matches(new Regex(@"About 1 in 4 people with it have VHL", RegexOptions.IgnoreCase), flat);

        // AND THE BRIDGE TO THE BLOCK'S OWN WORDS. The block says "a small number of
        // people" have an inherited condition; for THIS tumor that is wrong by a wide
        // margin, and the rendered read showed the two sentences sitting together
        // reading as a contradiction.
        Assert.Matches(new Regex(
            @"For this tumor it is not a small number", RegexOptions.IgnoreCase), flat);

        // AND THE NO-BLAME HALF, in the same section, for both groups.
        Assert.Matches(new Regex(@"nothing you did caused it", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"it was not passed on", RegexOptions.IgnoreCase), flat);
    }

    // ------------------------------------------------------------- the two-reader split

    [Fact]
    public void EveryClaimThatDiffersByGroupSaysWhichGroupItMeans()
    {
        // THE CENTRAL FAILURE MODE OF THIS PAGE: a sentence true of one group and
        // false of the other, stated flat. Each of these is asserted WITH its
        // qualifier, in the section that carries it.
        var recurrence = Flat(RawSection(RecurrenceHeading));

        // The one-off tumor: taken out completely, it does not often come back.
        Assert.Matches(new Regex(
            @"For one tumor that was taken out completely, it does not often come back",
            RegexOptions.IgnoreCase), recurrence);

        // VHL: NEW tumors, and the source's own "more likely to recur" line beside
        // them. The first draft ranked new tumors against recurrences, which no source
        // does, and in the reassuring direction (/review round 1).
        Assert.Matches(new Regex(
            @"With VHL, new tumors can appear as well", RegexOptions.IgnoreCase), recurrence);
        Assert.Matches(new Regex(
            @"more likely to come back after surgery", RegexOptions.IgnoreCase), recurrence);
        Assert.Matches(new Regex(
            @"not a sign that you did anything wrong", RegexOptions.IgnoreCase), recurrence);

        // AND THE UNSOURCED RANKING STAYS OUT, in either direction.
        var ranked = new Regex(
            @"more often a new tumor than|(new tumors?|it) (is|are) more likely than",
            RegexOptions.IgnoreCase);
        Assert.Matches(ranked, "with VHL it is more often a new tumor than the old one");  // canary
        Assert.DoesNotMatch(ranked, Plain);

        // The scans section keeps the same split, because the schedules differ.
        var scans = Flat(RawSection(ScansHeading));
        Assert.Matches(new Regex(@"One tumor, taken out completely", RegexOptions.IgnoreCase), scans);
        Assert.Matches(new Regex(@"With VHL, checks go on for life", RegexOptions.IgnoreCase), scans);

        // A BARE UNQUALIFIED REASSURANCE IS WHAT THIS GUARD EXISTS TO STOP.
        var unqualified = new Regex(
            @"(it|the tumor|they) (does|do) not come back\b(?![^.]*\bVHL\b)"
            + @"|once it is out,? (it is|you are) done",
            RegexOptions.IgnoreCase);
        Assert.Matches(unqualified, "after surgery it does not come back");         // canary
        Assert.Matches(unqualified, "once it is out, you are done");                // canary
        Assert.DoesNotMatch(unqualified, Plain);
    }

    [Fact]
    public void TheVhlShareIsCreditedAndTheTestIsOfferedToEveryReader()
    {
        var diagnosis = Flat(RawSection(DiagnosisHeading));

        // EVERY READER IS TOLD TO ASK, not only the one with a family history — which
        // is the whole point: about 1 in 5 people with VHL are the first in their
        // family to have it, so "no family history" is not a reason to skip it.
        Assert.Matches(new Regex(
            @"Ask for a VHL gene test, even if nobody in your family has VHL",
            RegexOptions.IgnoreCase), diagnosis);
        Assert.Matches(new Regex(
            @"About 1 in 5 people with VHL are the first in their family",
            RegexOptions.IgnoreCase), diagnosis);

        // THE HIGHER ESTIMATE TRAVELS WITH THE HEADLINE SHARE. Two of this page's own
        // sources put it nearer a third, and a reader deciding whether to ask for a
        // test should meet the range, not only its bottom (/review round 1).
        Assert.Matches(new Regex(
            @"Some studies put it closer to 1 in 3", RegexOptions.IgnoreCase),
            Flat(RawSection(ShortHeading)));

        // CREDITED, because this is the page's biggest ask of the reader.
        Assert.Contains("Pacific", diagnosis, StringComparison.Ordinal);
        Assert.Contains("MedlinePlus Genetics", diagnosis, StringComparison.Ordinal);

        // AND THE SHARE IS NOT UNDERSOLD. Macmillan's "Only a small number ... are
        // linked to VHL" is recorded in the front matter as a landmine, and a reader
        // who met that framing here might decline the test.
        var undersold = new Regex(
            @"only a small number[^.]{0,40}\bVHL\b|\bVHL\b[^.]{0,30}\bis rare(ly)? (the cause|linked)",
            RegexOptions.IgnoreCase);
        Assert.Matches(undersold, "only a small number of these tumors are linked to VHL");  // canary
        Assert.DoesNotMatch(undersold, Plain);
    }

    [Fact]
    public void TheInheritedRiskIsGivenWithTheChildrensTestAndTheWayOut()
    {
        // A 1-in-2 risk to each child is the most frightening number on the page, so
        // §12.6 applies: it is never the last word. The action and the way out sit
        // with it — testing can be done from birth, and a child without the change
        // does not need the lifelong checks.
        var causes = Flat(RawSection(CauseHeading));

        Assert.Matches(new Regex(
            @"each of your children has a 1 in 2 chance", RegexOptions.IgnoreCase), causes);
        Assert.Matches(new Regex(@"as early as birth", RegexOptions.IgnoreCase), causes);
        Assert.Matches(new Regex(
            @"does not need the lifelong checks", RegexOptions.IgnoreCase), causes);

        // The reassurance comes AFTER the risk, not instead of it. FLATTENED first:
        // the corpus is hard-wrapped, so "does not need\nthe lifelong checks" has a
        // newline through it and a raw IndexOf returns -1, which would have made this
        // ordering check pass or fail on where the line happened to wrap.
        var flat = Flat(RawSection(CauseHeading));
        var risk = flat.IndexOf("1 in 2 chance", StringComparison.Ordinal);
        var wayOut = flat.IndexOf("does not need the lifelong checks", StringComparison.Ordinal);
        Assert.True(risk >= 0 && wayOut > risk, "the way out must follow the risk it answers");
    }

    // ------------------------------------------------------------------ the grade

    [Fact]
    public void TheGradeIsCreditedToTheReviewsAndNotToTheWhoTable()
    {
        // WI-546's LESSON, APPLIED BEFORE DRAFTING. The WHO summary lists this tumor
        // in Table 1 and it is ABSENT from Table 3, "CNS WHO Grades of Selected
        // Types" — an absence that proves nothing either way, exactly as it did for
        // the germ cell tumors. Two reviews state grade 1 outright, and they are what
        // the page leans on. So: the grade IS given, and no sentence reads the WHO
        // table's silence as meaning anything.
        var grade = Flat(RawSection(GradeHeading));

        Assert.Matches(new Regex(
            @"It is classed as grade 1, the lowest grade", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"Two reviews of this tumor say so", RegexOptions.IgnoreCase), grade);

        // THE FORBIDDEN MOVE: reading the WHO table's silence.
        var readsTheSilence = new Regex(
            @"(WHO|World Health Organization)[^.]{0,60}\b(does not|doesn't|no longer) (give|assign|list)\b"
            + @"|not (in|on) the (WHO|World Health Organization)[^.]{0,20}\b(table|list)\b"
            + @"|\b(is|are) not graded\b|\bno grade is given\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(readsTheSilence, "the WHO does not give this tumor a grade");        // canary
        Assert.Matches(readsTheSilence, "it is not on the WHO table of grades");            // canary
        Assert.Matches(readsTheSilence, "these tumors are not graded");                     // canary
        Assert.DoesNotMatch(readsTheSilence, Plain);

        // AND THE GRADE IS NOT TURNED INTO A PROMISE (§12.5).
        Assert.Matches(new Regex(
            @"It is not a promise about any one\s+person", RegexOptions.IgnoreCase), grade);
    }

    [Fact]
    public void BenignAlwaysCarriesItsCounterweightInTheSameSection()
    {
        // THE MOST DANGEROUS WORD ON THIS PAGE. Every source says "benign", and one of
        // them ALSO says these tumors "can grow large enough to affect nearby tissue
        // and cause serious health issues". A page that carries the first half without
        // the second teaches a reader that nothing needs watching.
        var grade = Flat(RawSection(GradeHeading));

        Assert.Matches(new Regex(@"No\. It is not cancer", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"Not cancer does not mean harmless", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"can still grow\s+big enough to press on the brain or spinal cord",
            RegexOptions.IgnoreCase), grade);
        Assert.Contains("Cleveland Clinic", grade, StringComparison.Ordinal);

        // THE COUNTERWEIGHT SITS AFTER THE REASSURANCE IT ANSWERS, in that order.
        var raw = RawSection(GradeHeading);
        Assert.True(
            raw.IndexOf("It is not cancer", StringComparison.Ordinal)
            < raw.IndexOf("does not mean harmless", StringComparison.Ordinal),
            "the counterweight must follow the reassurance it qualifies");
    }

    // ------------------------------------------------------------------ treatment

    [Fact]
    public void TheOperationClaimStopsWhereTheSourcesStop()
    {
        // RULING 5, AND IT IS A CLAIM THE DRAFT WANTED TO MAKE. "Draining the pocket
        // alone is not enough" is the natural sentence here, it is what a reader would
        // find on some sites, and NO fetched source says it in words. What is sourced:
        // the aim is to take out the solid part, and the pocket collapses once it is
        // out. The page says only that.
        var operation = CuratedPage.Flatten(CuratedPage.ReaderText(
            Subsection(TreatmentHeading, "The operation")));

        Assert.Matches(new Regex(
            @"The aim is to take out the solid part", RegexOptions.IgnoreCase), operation);
        Assert.Matches(new Regex(
            @"the fluid pocket\s+usually collapses on its own", RegexOptions.IgnoreCase), operation);

        var drainageClaim = new Regex(
            @"drain\w*[^.]{0,40}\b(is not enough|does not work|will not help|is never enough)"
            + @"|\bjust draining\b|\bdraining alone\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(drainageClaim, "draining the pocket alone is not enough");      // canary
        Assert.Matches(drainageClaim, "just draining it does not work");               // canary
        Assert.DoesNotMatch(drainageClaim, Plain);

        // RULING 4: NOTHING IS CLAIMED ABOUT BIOPSY IN EITHER DIRECTION. One case
        // report is not a rule, and "a biopsy is dangerous here" is the kind of
        // sentence a reader would take to their team as fact.
        var biopsyClaim = new Regex(@"\bbiops(y|ies)\b", RegexOptions.IgnoreCase);
        Assert.Matches(biopsyClaim, "a biopsy is usually avoided");                    // canary
        Assert.DoesNotMatch(biopsyClaim, Plain);
    }

    [Fact]
    public void TheEmbolizationPassageKeepsItsUncertainty()
    {
        // RULING 8. The patient-facing source says it makes surgery "safer, and more
        // effective" with no caveat; the surgeons' review says it "can involve severe
        // complications". A page that carried only the first half would send a reader
        // in asking for a procedure whose risks nobody had mentioned.
        var operation = CuratedPage.Flatten(CuratedPage.ReaderText(
            Subsection(TreatmentHeading, "The operation")));

        Assert.Matches(new Regex(@"called\s+embolization", RegexOptions.IgnoreCase), operation);
        Assert.Matches(new Regex(
            @"can have serious problems of its own", RegexOptions.IgnoreCase), operation);
        Assert.Matches(new Regex(@"Not everyone\s+has it", RegexOptions.IgnoreCase), operation);

        var unqualified = new Regex(
            @"embolization[^.]{0,60}\b(makes (the )?surgery safer|is safer and more effective)\b"
            + @"|\balways done before\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(unqualified, "embolization makes the surgery safer");           // canary
        Assert.DoesNotMatch(unqualified, Plain);
    }

    [Fact]
    public void RadiosurgeryIsScopedToTheSolidPartAndClaimsNoSuperiority()
    {
        // RULING 7. The meta-analysis is explicit that radiosurgery does less for the
        // fluid pocket, which is the half a reader with a big pocket needs. And the
        // experts disagree about when it should come first, so the page says that
        // rather than picking a side it cannot support.
        var radiation = CuratedPage.Flatten(CuratedPage.ReaderText(
            Subsection(TreatmentHeading, "Focused radiation")));

        Assert.Matches(new Regex(
            @"It works on the solid part, less on the pocket", RegexOptions.IgnoreCase), radiation);
        Assert.Matches(new Regex(
            @"Experts do not all agree on when radiation should come first",
            RegexOptions.IgnoreCase), radiation);

        var superiority = new Regex(
            @"(radiosurgery|radiation)[^.]{0,50}\b(better than|instead of surgery|as good as)\b"
            + @"|\bbest (treatment|option) for (this|these)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(superiority, "radiosurgery is better than surgery here");       // canary
        Assert.Matches(superiority, "radiation works as good as an operation");        // canary
        Assert.DoesNotMatch(superiority, Plain);
    }

    [Fact]
    public void TheDrugIsKeptInsideItsLabelAndCarriesItsBoxedWarning()
    {
        // RULING 9, AND THE SHARPEST SCOPE LINE ON THE PAGE. The label covers ADULTS
        // WITH VHL whose tumors do not need surgery right away. A reader with a one-off
        // tumor who asks for this drug has been misled by the page, so the scope and
        // the "not approved without VHL" sentence are both pinned.
        var pill = CuratedPage.Flatten(CuratedPage.ReaderText(
            Subsection(TreatmentHeading, "A pill, for people with VHL")));

        Assert.Matches(new Regex(
            @"For adults with VHL, there is a pill called belzutifan", RegexOptions.IgnoreCase), pill);
        Assert.Matches(new Regex(
            @"do not need an operation right away", RegexOptions.IgnoreCase), pill);
        Assert.Matches(new Regex(
            @"It is not approved for a tumor without VHL", RegexOptions.IgnoreCase), pill);
        Assert.Matches(new Regex(
            @"whether it helps those tumors is not known", RegexOptions.IgnoreCase), pill);

        // THE BOXED WARNING REACHES THE READER, both halves. The contraception half is
        // the one a reader cannot guess: a hormonal method may simply stop working.
        Assert.Matches(new Regex(@"It can harm an unborn baby", RegexOptions.IgnoreCase), pill);
        Assert.Matches(new Regex(
            @"can stop some hormone birth control from working", RegexOptions.IgnoreCase), pill);

        // AND THE TWO SIDE EFFECTS THE LABEL CALLS SEVERE, with what to report.
        Assert.Matches(new Regex(@"low red blood cells, and low oxygen", RegexOptions.IgnoreCase), pill);
        Assert.Matches(new Regex(@"Tell your team", RegexOptions.IgnoreCase), pill);

        // NO DOSE ANYWHERE (§12.4): the label's mg figure is not the reader's decision.
        var dose = new Regex(@"\b\d+\s*(mg|milligram)", RegexOptions.IgnoreCase);
        Assert.Matches(dose, "the dose is 120 mg once a day");                         // canary
        Assert.DoesNotMatch(dose, Plain);
    }

    [Fact]
    public void WatchingIsPresentedAsAPlanRatherThanAsDoingNothing()
    {
        // RULING 10. "We will watch it" is heard as "they are not treating me", which
        // is how a reader ends up pushing for an operation nobody recommended. The
        // page names it as a plan, says what it consists of, and credits it.
        var watching = CuratedPage.Flatten(CuratedPage.ReaderText(
            Subsection(TreatmentHeading, "Watching instead of treating")));

        Assert.Matches(new Regex(
            @"Watching is a plan, not doing nothing", RegexOptions.IgnoreCase), watching);

        // THE CREDIT IS PINNED TO THE CLAIM IT BELONGS TO, not merely to the
        // subsection. BREAK HARNESS SURVIVOR, on LF and CRLF: the first version
        // asserted `Contains("VHL Alliance")` over the whole subsection, and this
        // subsection credits that source TWICE — once for "usually not treated until
        // symptoms" and again for the three treatment choices. So a mutation that
        // stripped the credit from the CLAIM left the other one standing and the
        // guard stayed green (§12.8, WI-544: a property with two homes in one
        // paragraph). The mutation was kept and the guard rewritten.
        Assert.Matches(new Regex(
            @"The VHL Alliance says these tumors are usually not treated until they cause\s+symptoms",
            RegexOptions.IgnoreCase), watching);

        // And the second credit is asserted where IT belongs, so neither can stand in
        // for the other.
        Assert.Matches(new Regex(
            @"The VHL Alliance says to look at all three choices", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(
                Subsection(TreatmentHeading, "Watching instead of treating"))));
        Assert.Contains("/treatments/watch-and-wait", Subsection(TreatmentHeading, "Watching instead of treating"),
            StringComparison.Ordinal);
    }

    // ------------------------------------------------------------- numbers and outlook

    [Fact]
    public void NoFigureAndNoCureWordAppearsOutsideTheOutlookGate()
    {
        // RULING 7 of the front matter (§12.12). At least six of this page's own
        // sources print a cure claim, a survival figure or a recurrence range, and one
        // prints "outcomes are excellent".
        var outside = OutsideTheGate();

        var cure = new Regex(@"\bcur(e|es|ed|ing|able|ative)\b", RegexOptions.IgnoreCase);
        Assert.Matches(cure, "surgery can cure this tumor");                           // canary
        Assert.Matches(cure, "most of these are curable");                             // canary
        Assert.DoesNotMatch(cure, outside);

        // THE WHOLE PAGE, not only outside the gate: this page's gate does not need a
        // cure word, and the sources' framing is exactly what it must not echo.
        Assert.DoesNotMatch(cure, Plain);

        foreach (var banned in new[]
                 {
                     "survival rate", "5-year", "five-year", "10-year", "median survival",
                     "life expectancy", "outcomes are excellent", "rarely comes back",
                     "virtually", "considered cured",
                 })
        {
            Assert.DoesNotContain(banned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // NO PERCENTAGE ANYWHERE. Every figure this page's sources carry — response
        // rates, control rates, recurrence rates, bleeding rates — is either disputed
        // between sources or counts a different group than the reader is in.
        var percent = new Regex(@"\d\s*(%|percent)", RegexOptions.IgnoreCase);
        Assert.Matches(percent, "about 63% of tumors shrank");                         // canary
        Assert.DoesNotMatch(percent, Plain);
    }

    [Fact]
    public void TheOnlyScheduleGivenIsTheGuidelineOneAndItKeepsItsEscapeHatch()
    {
        // RULING 11, AND §12.4 R1. One schedule is worth giving, because a family
        // with VHL has to plan around it — but it is a GUIDELINE SUGGESTION, sources
        // disagree about the interval, and a reader must not read it as their own plan.
        var scans = Flat(RawSection(ScansHeading));

        Assert.Matches(new Regex(
            @"suggests scans of the brain and whole spine every 2 years from age 11",
            RegexOptions.IgnoreCase), scans);
        Assert.Matches(new Regex(
            @"Your team may suggest scans more often", RegexOptions.IgnoreCase), scans);
        Assert.Contains("GeneReviews", scans, StringComparison.Ordinal);

        // AND NO SCHEDULE IS INVENTED FOR THE OTHER GROUP, where the sources say only
        // "typically" and "may vary".
        Assert.Matches(new Regex(@"Your team sets the schedule", RegexOptions.IgnoreCase), scans);

        var inventedSchedule = new Regex(
            @"every (3|6|three|six) months for[^.]{0,30}years|scans? (are|is) done every \d+ months",
            RegexOptions.IgnoreCase);
        Assert.Matches(inventedSchedule, "scans are done every 6 months");             // canary
        Assert.DoesNotMatch(inventedSchedule, Plain);
    }

    [Fact]
    public void TheOutlookGateTeachesTheConceptsAndNamesTheTwoGroupsApart()
    {
        var gate = Regex.Match(Page, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook section has lost its reader-choice gate");
        var inside = CuratedPage.Flatten(CuratedPage.ReaderText("---\n---\n" + gate.Groups[1].Value));

        // The figures a reader will find online mix the two groups together, which is
        // the specific reason they mislead HERE — so the gate says that, rather than
        // only the general "groups, not you" point.
        Assert.Matches(new Regex(
            @"describe large groups of people", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(
            @"mix the two groups on this page\s+together", RegexOptions.IgnoreCase), inside);

        // BOTH GROUPS ARE INSIDE THE GATE. A gate that carried only the good news for
        // the one-off tumor would leave the VHL reader's question unanswered in the one
        // place they went looking for it.
        Assert.Matches(new Regex(@"With VHL, it is a longer road", RegexOptions.IgnoreCase), inside);

        // The reader's choice is left open in both directions (§12.5).
        Assert.Matches(new Regex(
            @"You can also choose not to ask yet", RegexOptions.IgnoreCase), inside);

        Assert.DoesNotContain(OutlookHeading, gate.Groups[1].Value, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------- the report

    [Fact]
    public void TheOldNamesAreCarriedAndTheKidneyCancerLookAlikeIsExplained()
    {
        var report = Flat(RawSection(ReportHeading));

        foreach (var oldName in new[] { "Capillary hemangioblastoma", "Lindau tumor", "angioblastoma" })
        {
            Assert.Contains(oldName, report, StringComparison.OrdinalIgnoreCase);
        }

        // "angioreticuloma" IS NOT CARRIED: it turned up only as a literature search
        // term, never in a source describing it as a name in use or retired.
        Assert.DoesNotContain("angioreticuloma", Plain, StringComparison.OrdinalIgnoreCase);

        // THE LOOK-ALIKE, AND THE SENTENCE THAT STOPS IT FRIGHTENING SOMEBODY. A
        // report naming clear cell kidney cancer means the lab ruled it out, and a
        // reader with VHL — who is at real risk of that cancer — could read the same
        // words as a second diagnosis.
        Assert.Matches(new Regex(
            @"as something the lab ruled out", RegexOptions.IgnoreCase), report);
        Assert.Matches(new Regex(
            @"not because you have it", RegexOptions.IgnoreCase), report);
    }

    // ------------------------------------------------------------- the shared prose gates

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        Assert.NotEmpty(CuratedPage.Characterisations);
        Assert.Contains("aggressive", CuratedPage.Characterisations, StringComparer.OrdinalIgnoreCase);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // FIVE of this page's sources are British or Irish (Cancer Research UK,
        // Macmillan, The Brain Tumour Charity, Brain Tumour Research, VHL UK/Ireland),
        // and one more is a UK genetics review. Their spellings and their idiom are
        // the likeliest thing to leak into an American reader's page.
        var reader = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        Assert.NotEmpty(CuratedPage.BritishForms);
        Assert.Contains("tumour", CuratedPage.BritishForms, StringComparer.OrdinalIgnoreCase);

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var idiom in new[]
                 {
                     "A&E", "accident and emergency", "straight away", "out of hours",
                     "being sick", "come round", "car park", "999", "GP", "DVLA",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST. The first draft shared 59 windows with five files — most of
        // them with /tumors/spinal-cord-tumor, whose cord rules this page follows, and
        // with /tumors/cns-germ-cell-tumor, whose scored-absence paragraph has the same
        // shape. Every one was rewritten rather than exempted.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    // -------------------------------------------------------------- the front matter

    [Fact]
    public void TheSourcesAreRealAndTheBarredOnesAreNotCited()
    {
        var front = CuratedPage.FrontMatter(Page);

        foreach (var barred in new[]
                 {
                     "mayoclinic.org", "mhsystem.org", "ahfs", "medlineplus.gov/druginfo",
                     "cancer.gov/types/brain", "academic.oup.com", "webapis.cancer.gov",
                     "welireg.com", "medscape", "wikipedia",
                 })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        // §12.1: NO NCI PATIENT OR PROFESSIONAL SUMMARY. A domain list cannot express
        // that (WI-528), so it gets a pattern — and this page's research DID lean on
        // the NCI genetics summary, which is exactly why the ban is asserted here.
        var pdq = new Regex(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b|/pdq/)", RegexOptions.IgnoreCase);
        Assert.Matches(pdq, "https://www.cancer.gov/publications/pdq/information-summaries/genetics/vhl-syndrome-hp-pdq");
        Assert.DoesNotMatch(pdq, front);

        // THE TWO MEDLINEPLUS PAGES THAT ARE CITED ARE A GENETICS PAGE AND A HEALTH
        // TOPIC, never a drug monograph (PLAN.md §5). The negative canary is the
        // monograph path, banned above; this is the positive half.
        Assert.Contains("medlineplus.gov/genetics/", front, StringComparison.OrdinalIgnoreCase);

        // NO BRACKETED BLOCK NAME ANYWHERE IN FRONT MATTER (CaregiverSectionTests
        // finds the caregiver block with a raw IndexOf over the whole file).
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);

        // THE GAPS AND THE ROUTES ARE RECORDED WHERE THE NEXT READER WILL SEE THEM.
        Assert.Matches(new Regex(@"HTTP 403", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"HTTP 404", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"Europe PMC", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"checking your browser", RegexOptions.IgnoreCase), front);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.True(urls.Count >= 25, $"only {urls.Count} sources recorded");
        Assert.All(urls, u => Assert.StartsWith("https://", u));
        Assert.Equal(urls.Count, urls.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheTargetedTherapyPageCarriesTheReciprocalDoor()
    {
        // THE ONE SIBLING EDIT (Gate 1 decision A). /treatments/targeted-therapy lists
        // every approved brain-tumor targeted drug and did NOT list this one, so a VHL
        // reader sent there from this page would have found their own drug missing.
        // ADJACENCY: the href sits in the bullet that names the drug, not loose in the
        // section, because a link nothing asserts is a link the next editor deletes.
        var targeted = CuratedPage.Read("treatments", "targeted-therapy.md");

        // THE BULLET, SLICED AT ITS OWN PARAGRAPH BOUNDARY, on the RAW markdown.
        // Two earlier versions got this wrong in the same direction, each time
        // widening what counted as "the bullet": a `[^-]*?` regex terminated inside
        // "von Hippel-Lindau" itself, and then a next-`- **` slice ran 45 lines past
        // the end of the list, because this bullet is the LAST one — so "the href is
        // in the bullet" was being proved by a window covering two later sections
        // (/review rounds 1 and 2). A blank line is where a markdown list item ends.
        var raw = CuratedPage.ReaderText(targeted).Replace("\r\n", "\n", StringComparison.Ordinal);
        var start = raw.IndexOf("- **A brain or spinal cord tumor caused by von Hippel-Lindau",
            StringComparison.Ordinal);
        Assert.True(start >= 0, "/treatments/targeted-therapy has lost the belzutifan bullet");
        var end = raw.IndexOf("\n\n", start, StringComparison.Ordinal);
        Assert.True(end > start, "the belzutifan bullet no longer ends at a blank line");
        var bullet = CuratedPage.Flatten(raw[start..end]);

        // NOT VACUOUS: the slice is one bullet, not the rest of the page.
        Assert.DoesNotContain("## ", bullet, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Matches(bullet, @"- \*\*").Count);

        Assert.Contains("belzutifan", bullet, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/tumors/hemangioblastoma", bullet, StringComparison.Ordinal);

        // AND THE SCOPE TRAVELS WITH THE DRUG NAME. A reader with a one-off tumor who
        // meets this bullet without the condition named would ask for a drug the label
        // does not cover.
        Assert.Contains("von Hippel-Lindau", bullet, StringComparison.Ordinal);
        // SCOPED TO THE BRAIN AND SPINAL CORD (/review round 4). The label carries
        // three indications and two of them do not require the condition at all, so a
        // bare "only for people with VHL" is false of the drug and would tell a reader
        // with kidney cancer that a drug they may be taking is for somebody else.
        Assert.Matches(new Regex(
            @"For a brain or spinal cord tumor, it\s+is only for people with that\s+inherited condition",
            RegexOptions.IgnoreCase), bullet);

        // ITS OWN SOURCE LIVES ON THAT PAGE, not only on this one: a sibling's front
        // matter is not the page's source (WI-544).
        Assert.Contains("dailymed.nlm.nih.gov", CuratedPage.FrontMatter(targeted),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheSupportSectionSaysNoTumorSpecificGroupWasFound()
    {
        // RECORDED ABSENCE, SAID OUT LOUD, so a reader does not spend an evening
        // hunting for a group this page implied exists. The VHL groups are named
        // because they DO exist, and the general one is named for the reader whose
        // tumor has nothing to do with VHL.
        var support = Flat(RawSection(SupportHeading));

        Assert.Matches(new Regex(
            @"No group just for this tumor turned up in our search", RegexOptions.IgnoreCase), support);
        Assert.Contains("VHL Alliance", support, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"Benign is Not\s+Fine", RegexOptions.IgnoreCase), support);
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class HemangioblastomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/hemangioblastoma";

    private readonly WebApplicationFactory<Program> _factory;

    public HemangioblastomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageIsServedAtItsUrl()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hemangioblastoma", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFiveBlocksComposeAndTheThreeExcludedOnesLeaveNoTrace()
    {
        // THE HALF THE CONTENT GUARDS CANNOT SEE: they read directive NAMES out of the
        // source file, so they stay green if composition breaks entirely.
        var html = await Client.GetStringAsync(Url);

        foreach (var composed in new[]
                 {
                     "Call an ambulance",                          // ESCALATION
                     "The skull is a closed box",                  // MECHANISM
                     "You are allowed to ask questions",           // CAREGIVER
                     "nobody knows the cause",                     // CAUSES
                     "What they decide is advice, not an order",   // TUMOR-BOARD
                 })
        {
            Assert.Contains(composed, html, StringComparison.OrdinalIgnoreCase);
        }

        // EVERY ABSENCE ANCHOR SITS ON ONE SOURCE LINE (/review round 2). The first
        // version used "When cancer presses on the spinal cord", which wraps in
        // blocks/spinal-cord.md; Markdig emits the soft break as a newline, so the
        // phrase could never appear in the HTML whether the block composed or not,
        // and the assertion proved nothing. The siblings all use this same one-line
        // canary for this slot.
        foreach (var excluded in new[]
                 {
                     "right-away call, not a same-day one",        // SPINAL-CORD
                     "Most children slowly get better",            // POSTERIOR-FOSSA-SYNDROME
                     "Only the way of writing it did",             // CROSSWALK
                 })
        {
            Assert.DoesNotContain(excluded, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var marker in new[] { "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                                       "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]",
                                       "[POSTERIOR-FOSSA-SYNDROME]", ":::" })
        {
            Assert.DoesNotContain(marker, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await Client.GetStringAsync(Url);

        var start = html.IndexOf("What might happen over time", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the outlook heading did not render");

        var gate = Regex.Match(html[start..], @"<details[^>]*>");
        Assert.True(gate.Success, "the outlook gate did not render as a details element");
        Assert.DoesNotContain(" open", gate.Value, StringComparison.Ordinal);

        Assert.Equal(1, Regex.Matches(html, "The next part is about outlook").Count);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(Client, Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryDeepLinkedAnchorExists() =>
        // NOT VACUOUS: the composed escalation block deep-links the fever rule and the
        // shunt warning signs.
        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

    [Fact]
    public async Task TheSuppressedTooltipStaysQuietAndTheGlossaryStillFiresElsewhere()
    {
        // THE TUMOR-BOARD SUPPRESSION, as on every hub that composes that block: the
        // block defines a tumor board in nearly the glossary's words, so the popover
        // would print the definition directly above itself (§12.8's echo shape).
        var html = CuratedPage.Flatten(await Client.GetStringAsync(Url));
        var glossary = Path.Combine(CuratedPage.BlocksRoot, "..", "glossary");

        var board = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "tumor-board.md")));
        const string BoardOnly = "You do not attend";

        Assert.Contains(BoardOnly, board, StringComparison.Ordinal);
        Assert.DoesNotContain(BoardOnly, html, StringComparison.Ordinal);

        // THE BLOCK STILL COMPOSES, so the absence above is not passing because the
        // whole section vanished.
        Assert.Contains("What they decide is advice, not an order", html, StringComparison.Ordinal);

        // THE PAIRED POSITIVE, and it has to be on this page: suppression is page-wide,
        // so an absence-only test would pass just as happily if the glossary stopped
        // firing altogether (§12.10, WI-514). This page uses "stereotactic
        // radiosurgery" unsuppressed, so its definition must still reach the reader.
        // The anchor is PROVED TO EXIST in the glossary file first, so rewording that
        // file fails loudly here instead of quietly retiring this half (WI-546).
        //
        // THE TERM HAD TO BE ONE THIS PAGE WRITES AS ORDINARY PROSE. The first version
        // used "stereotactic radiosurgery", which this page only ever writes as LINK
        // TEXT — and the glossary does not decorate link text, so the assertion failed
        // on a page whose glossary was working perfectly. "contrast dye" is written
        // plainly in the diagnosis list, which is where the popover actually fires.
        var contrast = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "contrast-dye.md")));
        const string ContrastDefinition = "A liquid put into a vein before some scans";
        Assert.Contains(ContrastDefinition, contrast, StringComparison.Ordinal);
        Assert.Contains(ContrastDefinition, html, StringComparison.Ordinal);

        foreach (var marker in new[] { "!%", "%%" })
        {
            Assert.DoesNotContain(marker, await Client.GetStringAsync(Url), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndexAndTheTargetedTherapyPage()
    {
        // WI-412 shipped a page nothing linked to. Two doors: the index (taxonomy.yml
        // and the listing agreeing) and the sibling this item edited.
        Assert.Contains("/tumors/hemangioblastoma", await Client.GetStringAsync("/tumors"),
            StringComparison.Ordinal);
        Assert.Contains("/tumors/hemangioblastoma",
            await Client.GetStringAsync("/treatments/targeted-therapy"), StringComparison.Ordinal);
    }
}
