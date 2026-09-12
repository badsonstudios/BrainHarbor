using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-526: X17, clinical trials as an option. The eighth TREATMENT page under
/// the §12.8 LIBRARY template (shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs), and the most hype-prone subject in the corpus.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * MATCHING. WI-449 and WI-526 both say this site stays out of it: trial
///     matching is a regulated clinical space. No sentence here may read as
///     "you qualify" or "this one is for you", and every route out goes to a
///     person rather than to a result.
///   * the two omissions that make a trials page dangerous. A reader who
///     joins a randomized trial believing it is a way to GET the new drug has
///     been misled, and so has a reader who thinks a phase 1 trial is testing
///     whether the drug works. Both are stated outright.
///   * the frightening direction, which is where the dossier went wrong. Its
///     prior-treatment claim ("the conversation should happen before, not
///     after") tells a reader who is already mid-treatment that they have lost
///     something — and most of this page's readers arrive mid-treatment. NBTS
///     says "beginning a new therapy doesn't automatically rule you out".
///   * a cost or an insurance rule. NCI's costs page is explicitly US-only and
///     WI-455/457 put non-US readers here deliberately.
///   * a participant count. The dossier's numbers disagree with NCI's in all
///     three phases (see the front matter).
///
/// Every claim-shaped guard carries a CANARY (§12.8, WI-523), runs PAGE-WIDE
/// where a section scope would leave a door next to it (WI-524), checks the
/// AGENT rather than the modal where the property is about who acts (WI-525),
/// and asserts the match COUNT before iterating (WI-524).
/// </summary>
public sealed class ClinicalTrialsPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "clinical-trials.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description. <c>ReaderText</c> strips the front matter, so
    /// every body-scoped guard is blind to them, while
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// under the heading (§12.8, WI-524).
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            // `\s*$`, not `"$`: on a CRLF checkout the line ends `"\r\n` and
            // every guard over the headline then runs on an empty string.
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    /// <summary>Body plus the two lines the body-scoped guards cannot see.</summary>
    private static string Everything => Headline + " " + Body;

    /// <summary>
    /// <see cref="Everything"/> with the emphasis markers removed.
    ///
    /// THIS IS NOT COSMETIC. /review defeated the matching guard structurally
    /// rather than by vocabulary: <c>you **do**\nqualify</c> puts four
    /// asterisks and a line wrap between the two words every second-person
    /// pattern needs adjacent, so <c>\byou (?:do|would) qualify\b</c> cannot
    /// see it. Emphasis is authoring markup and a reader never meets it — the
    /// same argument §12.8 (WI-509) makes for asserting against reader text
    /// rather than raw source. Every phrase-shaped guard below runs over this.
    /// </summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    /// <summary>A section with the emphasis markers removed, for the same reason.</summary>
    private static string PlainOf(string section) =>
        Regex.Replace(CuratedPage.Flatten(Reader(section)), @"[*_]", "");

    private static string Subsection(string parent, string heading)
    {
        var outer = Regex.Match(Page, @"^## " + Regex.Escape(parent) + @".*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var sub = Regex.Match(outer, @"^### " + Regex.Escape(heading) + @".*?(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(sub), $"the '{heading}' subsection has gone");
        return CuratedPage.Flatten(Reader(sub));
    }

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|"
        + @"fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|"
        + @"seventy|eighty|ninety|hundred|thousand|dozen)";

    private const string Fraction = @"(?:half|halves|thirds?|quarters?|fifths?|tenths?)";

    /// <summary>
    /// Word runs shared with another page ON PURPOSE. Kept tiny (§12.10): each
    /// is a link label or a safety claim that must read identically wherever a
    /// reader meets it.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The standard "Where to go next" doors, worded identically on every
        // library page, carried as one run because the shingle window spans the
        // sentence boundary (§12.8, WI-525).
        "Brain tumor types if you want the page for your own tumor. "
        + "Get help now if you need to talk to a person today.",
    ];

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesTheSpineAndBothHonestyHalves()
    {
        // §12.3: readers consume 20 to 28% of a page. On a page about a
        // decision, the two things that change the decision have to be in the
        // first five sentences or they are not read.
        var shortVersion = CuratedPage.Flatten(Reader(Section("The short version")));

        // The spine: not a last resort, and the time to ask is now.
        // One claim, one strength. /review found "Most people think" in the short
        // version against "The common belief is" 35 lines down, where NBTS says
        // "a common misconception" -- and "most people" is a frequency no
        // source gives (§12.10).
        Assert.Matches(new Regex(@"A common belief is that a trial is what you try when everything else has",
            RegexOptions.IgnoreCase), shortVersion);
        Assert.DoesNotMatch(new Regex(@"Most people think a trial", RegexOptions.IgnoreCase), Everything);
        Assert.Matches(new Regex(@"the time to ask is now", RegexOptions.IgnoreCase), shortVersion);

        // Honesty half one: the burden.
        Assert.Matches(new Regex(@"more visits, more scans, often travel", RegexOptions.IgnoreCase),
            shortVersion);

        // Honesty half two, which is the one a hopeful reader most needs and is
        // likeliest to have been left out.
        Assert.Matches(new Regex(@"you do not\s*get to choose which treatment you get", RegexOptions.IgnoreCase),
            shortVersion);

        // And the matching boundary, in the summary rather than buried.
        Assert.Matches(new Regex(@"Nobody on this site can tell you\s*whether a trial fits you",
            RegexOptions.IgnoreCase), shortVersion);
    }

    [Fact]
    public void TheHeadlinePromisesNothing()
    {
        // §12.8 (WI-524): the description renders as the first paragraph under
        // the heading, and `ReaderText` strips it, so the same bans that run
        // over the body run over these two lines.
        var headline = Headline;

        var promise = new Regex(
            @"\b(?:new hope|hope for|a chance at|your chance|last chance|might save|could save|"
            + @"breakthrough|promising|cutting[- ]edge|latest treatments?|access to)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(promise, "Clinical trials: new hope when standard treatment runs out");
        Assert.Matches(promise, "how to get access to the latest treatments");
        Assert.DoesNotMatch(promise, headline);

        // And it does not tell the reader a trial fits them.
        // /review: "whether one of them is right for you" is neither
        // "the trial for you" nor "find the right trial", and the description
        // renders as the page's first paragraph. Its "worth chasing" is also
        // effort-framing, and it went with the same edit.
        var fits = new Regex(@"\b(?:whether you qualify|if you qualify|find the right trial|"
            + @"the trial for you|matched? to a trial|right for you|worth chasing|"
            + @"whether one (?:of them )?(?:is|might be) right)\b", RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "and how to find the right trial for you",
                     "and how to work out whether one of them is right for you",
                     "and the two questions that tell you whether one is worth chasing",
                 })
        {
            Assert.Matches(fits, known);
        }
        Assert.DoesNotMatch(fits, headline);
    }

    [Fact]
    public void NothingOnThePageReadsAsMatchingOrEligibility()
    {
        // THE ITEM'S POLICY BOUNDARY (WI-449, and the backlog entry for this
        // item): trial MATCHING is a regulated clinical space this site stays
        // out of. Leal Health and Massive Bio do it; we explain and route.
        //
        // PAGE-WIDE, because this is exactly the claim that would be walked by
        // moving a sentence one section down (§12.8, WI-524).
        // INVERTED, per WI-525. The first version hunted eight second-person
        // phrasings and exempted any sentence containing a screening word, and
        // /review walked it SEVEN ways:
        //   * "the trials LOOKING FOR that are the ones you fit" — the
        //     exemption word and the defect in one sentence
        //   * "you **do**\nqualify"   — bold markers and a wrap broke adjacency
        //   * "they qualify" / "people ... are eligible" — third person
        //   * "tell you which trials are WRITTEN FOR SOMEONE LIKE YOU" — the
        //     offer, with none of the eight phrasings
        //   * "there is almost certainly a trial open to you right now"
        //   * "work out which of the five you are in"
        //
        // So the rule is now: find every sentence that puts the reader and a
        // question of fit in the same breath, and require each to ATTRIBUTE
        // the judgement to a person or to the trial. Shapes nobody has thought
        // of fail by default instead of passing by default.
        // The ban is on an ASSERTION of fit, not on the vocabulary of fit. The
        // first version of this inverted guard fired on five of the page's own
        // correct sentences — including the spine ("a trial MIGHT be open to
        // you"), the list's deliberate third person ("open to A PERSON"), and
        // the boundary statement itself ("it does not tell you what fits") —
        // which is the rule-that-fails-a-correct-page shape (§12.8, WI-508).
        //
        // So: an assertion is second-person and unhedged, or third-person and
        // categorical. A hedge, a question, or a trial-side description is not
        // an assertion.
        var fitTalk = new Regex(
            @"\byou (?:do |will |would |may |might )?qualify\b|"
            + @"\byou are eligible\b|\bthey qualify\b|\bare eligible for\b|"
            + @"\bthe ones? you fit\b|\byou fit what\b|"
            + @"\bright for you\b|\bwritten for (?:someone|somebody) like you\b|"
            + @"\bthere is (?:almost certainly )?a trial open to you\b|"
            + @"\b(?:match|matched|matching) you to\b|\bwe (?:can|will) find\b",
            RegexOptions.IgnoreCase);

        // NO PATTERN ESCAPE. The first attempt exempted any sentence carrying
        // screening language, and /review's sharpest walk-through put the
        // exemption word and the defect in ONE sentence: "the trials LOOKING
        // FOR that are the ones YOU FIT". A pattern hole is a door; a short
        // NAMED allowlist is not (§12.8, WI-524). One entry, which is the
        // page's own step 4.
        var allowedFitTalk = new[]
        {
            "Tests to check you fit what the trial is looking for",
        };

        foreach (var known in new[]
                 {
                     "If your tumor has an IDH change, you would qualify for several trials.",
                     "If your report shows an IDH change, the trials looking for that are the ones you fit.",
                     "If your molecular results line up with what the trial wants, you do qualify.",
                     "Most people who ask this early will find they qualify for something.",
                     "People with a recurrent IDH-mutant glioma are eligible for most trials running today.",
                     "What we can do is tell you which trials are written for someone like you.",
                     "If you have all of these, there is almost certainly a trial open to you right now.",
                 })
        {
            Assert.Matches(fitTalk, known);
        }

        var unattributed = CuratedPage.SentencesOf(Plain)
            .Where(s => fitTalk.IsMatch(s))
            .Where(s => !allowedFitTalk.Any(a => s.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(unattributed.Count == 0,
            "this page asserts that a trial fits the reader, which is matching (WI-449):\n  "
            + string.Join("\n  ", unattributed));

        // And the five-windows list is not turned into a self-placement
        // exercise, which /review walked with "work out which of the five you
        // are in". The list describes A PERSON on purpose (it says "a person",
        // not "you"), and the page's instruction after it is to ask.
        var selfPlace = new Regex(
            @"\bwork out which\b|\bwhich of the (?:five|those)\b[^.]{0,20}\byou are\b|"
            + @"\bfind yourself on\b|\bplace yourself\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(selfPlace, "Work out which of the five you are in, and say which one when you ask.");
        Assert.DoesNotMatch(selfPlace, Plain);

        // The boundary is stated OUT LOUD rather than merely observed, because
        // a reader who has used a matching service needs to know why this page
        // does not do that.
        var what = CuratedPage.Flatten(Reader(Section("What is it, and what is it not?")));
        Assert.Matches(new Regex(@"not something this website can pick for you", RegexOptions.IgnoreCase),
            what);
        Assert.Matches(new Regex(@"Choosing a trial is a medical decision", RegexOptions.IgnoreCase), what);
        Assert.Matches(new Regex(@"belongs with the people who know your case", RegexOptions.IgnoreCase),
            what);

        // And no matching service is named anywhere, in either direction: naming
        // them is either an endorsement or a warning, and both are decisions
        // this page has no standing to make.
        foreach (var vendor in new[] { "Leal", "Massive Bio", "TrialJectory", "Antidote" })
        {
            Assert.DoesNotContain(vendor, Everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void NoParticipantCountAndNoFrequencyIsPublished()
    {
        // §12.4 R1, and for two reasons (see the front matter): the dossier's
        // own [BOUNDARY] note says publish the phase meanings without the
        // counts, AND the dossier's counts are not NCI's — 20-80 / 100-300 /
        // 1,000-3,000 against "around 15 to 30" / "50 to 100" / "100 to
        // several thousand". A reader would use these to judge how
        // experimental a thing is, which is a question for the team.
        //
        // The frequency alternatives are WI-525's widened set, which /review
        // walked five ways there.
        var numbers = new Regex(
            @"\b(?:about |around |roughly |up to |as many as |nearly |between )?" + CountWord
            + @"\s*(?:to|-|and)\s*(?:several\s+|a few\s+|many\s+)?" + CountWord
            + @"\s+(?:people|patients|participants)\b|"
            // The population verb list was `take part|are enrolled|join`, and
            // /review walked it with "fifty people ARE IN a typical phase 2
            // trial" and "Phase 3 trials RECRUIT several thousand people". A
            // bare count beside a population noun is now enough, with the
            // three-entry allowlist doing the exempting.
            // A bare count beside a population noun needs TRIAL context, or it
            // fires on the caregiver section's "two people hear more than one"
            // — a correct sentence, and the failure mode §12.8 (WI-508) names.
            + @"\b" + CountWord + @"\s+(?:people|patients|participants)\b[^.]{0,50}"
            + @"\b(?:trial|phase|study)\b|"
            + @"\b(?:trial|phase|study)\b[^.]{0,50}\b" + CountWord
            + @"\s+(?:people|patients|participants)\b|"
            + @"\b(?:a couple of dozen|a few dozen|several dozen|several thousand|"
            + @"many thousands)\s+(?:people|patients|participants)?\b|"
            + @"\b(?:recruits?|recruiting|enrols?|enrolls?|enrolling)\b[^.]{0,25}"
            + @"\b(?:people|patients|participants)\b|"
            // The wordless quantities, which is where WI-524 proved the real
            // defects arrive ("almost everybody", "most people get some").
            + @"\b(?:almost everybody|almost all|nearly everybody|nearly all|most people|"
            + @"hardly anyone|plenty of people|a handful of people)\b|"
            // `in` and `out of`, NOT a bare `of`: "one of two things" and "one
            // of two situations" are ordinary English and both are on this
            // page, so including `of` fired on two correct sentences (§12.8,
            // WI-508 — a rule that fails a correct page is worse than none).
            + @"\b(?:about |around |roughly )?" + CountWord
            + @"\s+(?:\w+\s+){0,2}(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\s*(?:percent|per cent)\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?\w+\b|"
            + @"\b" + CountWord + @"\s+times\s+(?:as|more)\s+likely\b|"
            + @"%",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A phase 1 trial usually has 15 to 30 people in it.",
                     "Phase 3 trials involve 100 to several thousand people.",
                     "About one in three people on a trial sees a benefit.",
                     "Roughly 20 percent of people withdraw.",
                     "A quarter of those offered a trial join one.",
                     "It works in 12% of cases.",
                     // /review's six, all green against the first version.
                     "A phase 1 trial is usually a couple of dozen people.",
                     "Around fifty people are in a typical phase 2 trial.",
                     "Phase 3 trials recruit several thousand people.",
                     "Some run in as many as several thousand people.",
                     "Almost everybody who asks finds there is nothing open for them.",
                 })
        {
            Assert.Matches(numbers, known);
        }

        // The allowlist, and it is short and named (§12.8, WI-524). Both
        // entries quantify something structural about trials rather than
        // anything about a reader's odds.
        var allowed = new[]
        {
            // The five windows of eligibility, which NBTS enumerates and which
            // are the page's whole spine. A count of doors, not of people.
            "There are **five points** in this illness where a trial may be open",
            "Three of those five doors are open early",

            // The page's own refusal to print the counts.
            "We do not print how many people are in each stage",
        };

        var offenders = CuratedPage.SentencesOf(Everything)
            .Where(s => numbers.IsMatch(s))
            .Where(s => !allowed.Any(a => s.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "this page publishes a count a reader would use to judge their own odds:\n  "
            + string.Join("\n  ", offenders));

        // And it says WHY the counts are absent, because a page that is
        // silently silent leaves the reader to supply their own (§12.8, WI-521).
        var phases = CuratedPage.Flatten(Reader(Section("What do the phases mean?")));
        Assert.Matches(new Regex(@"The published figures\s*disagree with each other", RegexOptions.IgnoreCase),
            phases);
    }

    [Fact]
    public void TheFiveWindowsAreAllFiveAndTheEarlyCountMatchesTheList()
    {
        // NBTS enumerates five, and three of them are before recurrence. The
        // page makes a claim ABOUT its own list ("Three of those five doors are
        // open early"), which is the §12.8 WI-509 shape: a claim about a list
        // is a claim about every item in it, and it goes stale inside one edit
        // if somebody adds or reorders a window.
        var section = CuratedPage.Flatten(Reader(Section("When should I ask?")));

        // COUNT EVERY LIST ITEM, NOT JUST THE NUMBERED ONES. /review added a
        // sixth window as a `-` bullet and as prose, and `\d\. ` saw neither —
        // so the list was six items while "five points" and "Three of those
        // five" both still passed. Counted over the RAW section, per line, so
        // a bullet of any marker is visible.
        var raw = Regex.Match(Page, @"^## When should I ask\?.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the 'when should I ask' section has gone");

        var items = raw.Replace("\r\n", "\n").Split('\n')
            .Count(l => Regex.IsMatch(l, @"^\s*(?:\d+\.|[-*]) "));
        Assert.True(items == 5,
            $"the section lists {items} items; NBTS gives five, and the page's own 'three of "
            + "those five' sentence is a claim about the length of this list");

        // And no sixth window smuggled in as prose.
        Assert.DoesNotMatch(new Regex(@"\ba sixth window\b|\bsome centers offer\b",
            RegexOptions.IgnoreCase), Plain);

        foreach (var window in new[]
                 {
                     "before surgery", "before your standard treatment starts",
                     "after standard treatment and before the next stretch",
                     "When it comes back", "after surgery",
                 })
        {
            Assert.Contains(window, section, StringComparison.OrdinalIgnoreCase);
        }

        // The three early ones are the claim, and "newly diagnosed" is what
        // makes them early. Counted rather than asserted as a string, so the
        // sentence and the list cannot drift apart.
        var early = Regex.Matches(section, @"Newly diagnosed", RegexOptions.IgnoreCase).Count;
        Assert.True(early == 3,
            $"the list marks {early} windows as newly diagnosed, and the page says three");
        // THE WHOLE SENTENCE, END-ANCHORED. Pinning only the first clause let
        // /review replace the tail with "...and NONE of them shut once
        // treatment begins" (flatly false, over-reassuring) and with "...and
        // two of those FIVE shut" (wrong arithmetic) — and the numbers-guard
        // allowlist entry "Three of those five" then exempted the sentence
        // from the count check as well.
        Assert.Matches(new Regex(
            @"Three of those five\s*doors are open early, and two of those three are about the time "
            + @"before\s*treatment starts\.", RegexOptions.IgnoreCase), section);

        // And the spine itself, in NBTS's own terms.
        Assert.Matches(new Regex(@"common belief is that a trial is a last resort", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"ask at diagnosis if you can", RegexOptions.IgnoreCase), section);

        // THE SHORT VERSION HAS TO AGREE WITH THE LIST TOO. The end-to-end read
        // found it saying "several points" where this section says five, and
        // saying the first window is "before any treatment starts" when the
        // first window is BEFORE SURGERY — and surgery is a treatment. Two
        // claims about the page's own list that the list did not support, in
        // the five sentences §12.3 says are the only ones most readers read.
        var shortVersion = CuratedPage.Flatten(Reader(Section("The short version")));
        Assert.Matches(new Regex(@"five points in this illness", RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(new Regex(@"the first is before the operation that takes\s*the tumor out",
            RegexOptions.IgnoreCase), shortVersion);
        Assert.DoesNotMatch(new Regex(@"several points in this illness", RegexOptions.IgnoreCase),
            Everything);
        Assert.DoesNotMatch(new Regex(@"before any treatment\s*starts", RegexOptions.IgnoreCase),
            Everything);
        // /review BLOCKER 1: the first correction over-shot. "before anything at
        // all has been done" contradicts this page's own "A confirmed diagnosis
        // from tissue" eight screens later, and /treatments/watch-and-wait
        // records that a biopsy counts as surgery. NBTS's first window is
        // before the RESECTION, not before everything.
        Assert.DoesNotMatch(new Regex(@"before anything at all has been", RegexOptions.IgnoreCase),
            Everything);

        // ONE PRIMACY CLAIM ON THE PAGE, AND IT BELONGS TO THE SPINE. The first
        // draft had THREE sections each telling the reader they were the most
        // important one ("the single most useful thing on this page", "This
        // matters more than anything else on this page", "This is the part most
        // worth understanding") — §12.8 (WI-509): a uniqueness claim is a claim
        // about every other section, and all three arrived in one draft.
        var primacy = new Regex(
            @"\b(?:most (?:useful|important|worth)|matters more than anything|"
            + @"if you read nothing else|the one thing to)\b",
            RegexOptions.IgnoreCase);
        var claims = CuratedPage.SentencesOf(Everything).Where(s => primacy.IsMatch(s)).ToList();
        Assert.True(claims.Count == 1,
            $"{claims.Count} sections claim to be the most important one:\n  " + string.Join("\n  ", claims));
        Assert.Contains("read this section", claims[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThePriorTreatmentClaimCarriesBothHalvesInTheRightOrder()
    {
        // THE DOSSIER'S DEFECT, AND IT IS IN THE FRIGHTENING DIRECTION. §16
        // says prior treatment excludes people "which is exactly why the
        // conversation should happen before, not after". NBTS says: "Beginning
        // a new therapy doesn't automatically rule you out from a clinical
        // trial, but it's important to plan ahead."
        //
        // Most readers of this page arrive mid-treatment, from /tumors/* and
        // /treatments/*, so the dossier's version lands on the wrong side for
        // the majority (§12.12, in WI-515's direction rather than WI-516's).
        var sub = Subsection("When should I ask?", "If you have already started treatment");

        // The reassuring half is FIRST, because a reader who has already
        // started reads the first sentence and stops.
        var reassurance = sub.IndexOf("does not automatically rule you out", StringComparison.OrdinalIgnoreCase);
        var planAhead = sub.IndexOf("planning ahead", StringComparison.OrdinalIgnoreCase);
        Assert.True(reassurance >= 0, "the 'does not automatically rule you out' half has gone");
        Assert.True(planAhead >= 0, "the 'plan ahead' half has gone");
        Assert.True(reassurance < planAhead,
            "the plan-ahead half comes first, which tells a reader who has already started treatment "
            + "that they should have asked sooner before it tells them they have probably not lost "
            + "anything");

        // And the page never tells that reader they have missed the boat.
        var missed = new Regex(
            @"\byou (?:have|will have) missed\b|\btoo late\b|\bshut you out\b|"
            + @"\brules you out\b|\bno longer eligible\b|\bclosed to you\b|"
            // /review: "the first two doors on that list HAVE CLOSED" walked
            // the first version, because only "closed TO YOU" was banned.
            + @"\b(?:door|window)s?\b[^.]{0,25}\b(?:clos\w*|shut)\b|"
            + @"\b(?:clos\w*|shut)\b[^.]{0,25}\b(?:door|window)s?\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "If you have started treatment it is too late for most trials.",
                     "Starting chemotherapy rules you out of most trials.",
                     "Once radiation has started, the first two doors on that list have closed.",
                 })
        {
            Assert.Matches(missed, known);
        }
        Assert.DoesNotMatch(missed, Plain);

        Assert.Matches(new Regex(@"You have probably not missed anything", RegexOptions.IgnoreCase), sub);

        // It does not turn "ask first" into "delay treatment", which is the way
        // this advice becomes dangerous.
        Assert.Matches(new Regex(@"not a reason to delay treatment", RegexOptions.IgnoreCase), sub);

        // AND THE WARNING HALF IS PINNED TOO. /review's sharpest finding here:
        // it deleted the washout sentence entirely — replacing "Some trials do
        // ask that you have not had a particular drug, or that a certain amount
        // of time has passed since you did" with "In practice what you have had
        // already almost never closes a door" — and every assertion above
        // stayed green. The balance on this item's most delicate claim was
        // being held by prose alone. The property is that the subsection names
        // BOTH a time gap and a drug exclusion.
        Assert.Matches(new Regex(@"a certain amount\s*of time has passed", RegexOptions.IgnoreCase), sub);
        Assert.Matches(new Regex(@"have not had a particular drug", RegexOptions.IgnoreCase), sub);
        Assert.Matches(new Regex(@"It depends entirely on the trial", RegexOptions.IgnoreCase), sub);
    }

    [Fact]
    public void ThePhaseOneHonestyIsStatedRatherThanImplied()
    {
        // NCI's list of what a phase 1 trial tests — "whether a new treatment
        // is safe, what its side effects are, whether people can tolerate it,
        // and the highest dose that people can tolerate" — does not contain
        // whether it works. That absence is the most important anti-hype fact
        // on the page, and an absence cannot be inferred by a frightened
        // reader, so it is said out loud.
        var phases = CuratedPage.Flatten(Reader(Section("What do the phases mean?")));

        Assert.Matches(new Regex(@"Notice what\s*is not on that list", RegexOptions.IgnoreCase), phases);
        Assert.Matches(new Regex(@"phase 1 trial is not designed to find out whether the\s*treatment works",
            RegexOptions.IgnoreCase), phases);

        // With the half that stops it reading as a warning against phase 1.
        Assert.Matches(new Regex(@"Some people do benefit", RegexOptions.IgnoreCase), phases);
        Assert.Matches(new Regex(@"It is not a bad\s*thing to be offered", RegexOptions.IgnoreCase), phases);

        // And no phase is characterised as a better or worse thing to be in.
        // A SUPERLATIVE IS NOT A COMPARATIVE — §12.13's lesson, and /review
        // proved it again here eight items later: the first version held
        // `safer|riskier` and missed "Phase 1 trials are THE RISKIEST, and
        // phase 3 THE ONE TO HOPE FOR". It also missed the misconception the
        // page's spine dismantles, arriving as a paraphrase: "A phase 1 trial
        // is WHERE PEOPLE GO WHEN NOTHING ELSE IS LEFT".
        var ranked = new Regex(
            @"\bphase \d\b[^.]{0,50}"
            + @"\b(?:better|worse|safer|riskier|safest|riskiest|most promising|more promising|"
            + @"last resort|to hope for|the one to|nothing else is left|nothing left)\b|"
            + @"\b(?:better|worse|safer|riskier|safest|riskiest)\b[^.]{0,25}\bphase \d\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "A phase 3 trial is safer than a phase 1.",
                     "Phase 1 trials are the riskiest, and phase 3 the one to hope for.",
                     "A phase 1 trial is where people go when nothing else is left.",
                 })
        {
            Assert.Matches(ranked, known);
        }
        Assert.DoesNotMatch(ranked, Plain);
    }

    [Fact]
    public void TheRandomizationSectionSaysJoiningIsNotAWayToGetTheDrug()
    {
        // NCI: "neither you nor your doctor can choose which group you will be
        // assigned to." A reader who joins a randomized trial believing it is a
        // route to the new treatment has been misled by omission, and that is
        // the commonest way a page like this does harm.
        var section = CuratedPage.Flatten(Reader(Section("Will I get the new treatment?")));

        Assert.Matches(new Regex(@"nobody chooses. Not you, and not your doctor", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"you may\s*not get the new thing", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Joining is not a way of obtaining a particular drug", RegexOptions.IgnoreCase),
            section);

        // And the other half, so a reader does not conclude every trial is a
        // coin toss: early trials often give everybody the study treatment.
        Assert.Matches(new Regex(@"especially early ones where everybody gets the study treatment",
            RegexOptions.IgnoreCase), section);

        // No sentence anywhere suggests a way to influence the group.
        // /review beat the first version with "ask the team to PUT YOU FORWARD
        // FOR the arm getting the new drug", which is not the shape
        // `ask (to be|for) (put )?(in|into) the` — and added an unsourced
        // crossover promise, which is the same defect wearing a delay.
        var influence = new Regex(
            @"\b(?:ask|request|push|put you forward|get yourself|angle)\b[^.]{0,45}"
            + @"\b(?:group|arm|the new (?:drug|treatment))\b|"
            + @"\byou can choose (?:which|your) group\b|\bask for the new\b|"
            + @"\bswitch to the new\b|\bcross over to\b|\bget the new drug (?:when|after|once)\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "You can ask to be put in the treatment group.",
                     "It is fair to ask the team to put you forward for the arm getting the new drug.",
                     "If you are put in the standard group, you can usually switch to the new drug "
                     + "when the trial ends.",
                 })
        {
            Assert.Matches(influence, known);
        }
        Assert.DoesNotMatch(influence, Plain);
    }

    [Fact]
    public void ThePlaceboAnswerIsTheSourcesAndNotAnAbsolute()
    {
        // The dossier's "no one is given a placebo when an effective treatment
        // is available" is NOT on the NCI page it is cited to, nor on NCI's
        // safety page. What NCI says is that placebos are rarely used, that you
        // are always told beforehand, and that in the common design the placebo
        // is added ON TOP of standard treatment.
        var section = CuratedPage.Flatten(Reader(Section("Will I be given a sugar pill?")));

        Assert.Matches(new Regex(@"rarely used", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"you are told before you decide", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"you are still getting the standard treatment", RegexOptions.IgnoreCase),
            section);

        // THE BAN IS ON THE PROPERTY, NOT ON FOUR PHRASINGS, and the page's own
        // heading word is part of the vocabulary. /review beat the first
        // version three ways, all of them reinstating the dossier's absolute:
        //   * "You will not be given A SUGAR PILL instead of treatment that
        //     works" — the ban only knew the word "placebo", while the section
        //     is headed "Will I be given a sugar pill?"
        //   * "A placebo is NEVER USED IN PLACE OF a treatment that works"
        //   * "NOBODY IS LEFT WITHOUT TREATMENT so that a dummy pill can be
        //     tested" — agentless, no banned token at all
        //
        // So: in this section, a negation in the same sentence as a dummy-
        // treatment word must be one of the two sourced ones.
        const string dummy = @"(?:placebo|sugar pill|dummy(?: pill| treatment)?)";
        var negated = new Regex(
            @"\b(?:never|no ?one|nobody|not|without)\b[^.]{0,80}\b" + dummy + @"\b|"
            + @"\b" + dummy + @"\b[^.]{0,80}\b(?:never|no ?one|nobody|not|without)\b",
            RegexOptions.IgnoreCase);

        // The two the sources DO support, named rather than pattern-exempted.
        var sourced = new[]
        {
            "In cancer treatment trials they are rarely used",
            "a dummy treatment with nothing",
            "So the question to ask is not",
        };

        foreach (var known in new[]
                 {
                     "Nobody is given a placebo when an effective treatment is available.",
                     "You will not be given a sugar pill instead of treatment that works.",
                     "A placebo is never used in place of a treatment that works.",
                     "Nobody is left without treatment so that a dummy pill can be tested.",
                 })
        {
            Assert.Matches(negated, known);
            Assert.DoesNotContain(sourced[0], known, StringComparison.OrdinalIgnoreCase);
        }

        var absolutes = CuratedPage.SentencesOf(PlainOf(Section("Will I be given a sugar pill?")))
            .Where(s => negated.IsMatch(s))
            .Where(s => !sourced.Any(a => s.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(absolutes.Count == 0,
            "this page makes an absolute promise about placebos that no source supports:\n  "
            + string.Join("\n  ", absolutes));

        // And the honest remainder: there IS a design where a placebo stands
        // alone, and the page names it rather than hiding it behind the
        // reassuring one.
        Assert.Matches(new Regex(@"no standard treatment to compare against", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheCostSectionPrintsTheShapeAndNoJurisdiction()
    {
        // NCI's costs page is explicitly US-specific (Medicaid, Medicare,
        // TRICARE, state programs). WI-455/457 put non-US readers on this site
        // deliberately, and WI-560 records that a page written as though every
        // reader has a DMV fails them.
        var section = CuratedPage.Flatten(Reader(Section("What will it cost me?")));

        Assert.Matches(new Regex(@"the only way to know\s*is to ask which", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"ask the trial coordinator directly which\s*costs are covered",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"depends on where you live and how your care is paid for",
            RegexOptions.IgnoreCase), section);

        // No scheme, no country's rules, no figure, and no legal claim. NCI
        // makes no "insurers must cover routine costs" statement, so neither
        // does this page.
        // /review got five jurisdiction claims past the first version, and the
        // holes were all in the same place: the guard banned SCHEME NAMES and a
        // legal modal, so a country name, a plain coverage verb, US-only
        // billing vocabulary and a spelled-out sum all walked through.
        var jurisdiction = new Regex(
            // schemes
            @"\b(?:Medicare|Medicaid|TRICARE|Obamacare|Affordable Care Act|NHS|CHIP)\b|"
            // countries and sub-national units, because "In the United States
            // the ordinary care parts are usually billed…" names no scheme
            + @"\b(?:the United States|the US|the UK|Canada|Australia|Ireland|Europe|"
            + @"your state|most states|federal)\b|"
            // a legal claim
            + @"\bby law\b|\brequired to cover\b|"
            // US-only billing vocabulary
            + @"\b(?:copay|co-pay|deductible|coinsurance|out-of-network|billed the same way)\b",
            RegexOptions.IgnoreCase);

        // A coverage CLAIM about insurance, whatever the modal. NCI makes none.
        var coverageClaim = new Regex(
            @"\b(?:insurers?|insurance|health plans?|your plan|most plans)\b[^.]{0,40}"
            + @"\b(?:cover|covers|covered|pay|pays|must|has to|are required)\b|"
            + @"\b(?:cover|covers|pay|pays)\b[^.]{0,25}\b(?:insurers?|insurance|health plans?)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Medicare covers the routine care costs of a trial.",
                     "Insurers must cover routine care in an approved trial.",
                     "By law your plan has to pay for the standard care parts.",
                     "In the United States the ordinary care parts are usually billed the same way.",
                     "Ask the financial counselor what your copay and deductible would be.",
                 })
        {
            Assert.True(jurisdiction.IsMatch(known) || coverageClaim.IsMatch(known),
                "the jurisdiction guard cannot see: " + known);
        }
        Assert.Matches(coverageClaim, "Most health plans cover the ordinary care you would have had.");
        // SCOPED TO THIS SECTION, not page-wide. The country list fired on the
        // page's own "search the whole registry by ZIP code if you are in the
        // US", which is a true statement about what /trials can do for whom --
        // WI-455 asks for exactly that honesty. A country name in the COSTS
        // section is a rule; a country name beside a search box is a scope.
        Assert.DoesNotMatch(jurisdiction, PlainOf(Section("What will it cost me?")));
        Assert.DoesNotMatch(coverageClaim, PlainOf(Section("What will it cost me?")));

        // The schemes and the legal claims stay banned page-wide: there is
        // nowhere on this page those belong.
        Assert.DoesNotMatch(new Regex(
            @"\b(?:Medicare|Medicaid|TRICARE|Obamacare|Affordable Care Act|NHS|CHIP)\b|"
            + @"\bby law\b|\brequired to cover\b", RegexOptions.IgnoreCase), Plain);

        // No money figure anywhere on the page, in digits OR in words —
        // /review published "twenty dollars a day" and "a few hundred a month"
        // past a guard that required digits.
        var money = new Regex(
            @"[$£€]\s?\d|\b\d+\s*(?:dollars|pounds|euros)\b|"
            + @"\b(?:one|two|three|four|five|ten|twenty|thirty|fifty|a hundred|hundreds?|"
            + @"a few hundred|a thousand|thousands?)\s+(?:dollars|pounds|euros)\b|"
            + @"\ba few hundred a (?:month|week|visit)\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "Parking, which can run to twenty dollars a day.",
                     "For most people the out-of-pocket part comes to a few hundred a month.",
                     "Parking, often $20 a visit.",
                 })
        {
            Assert.Matches(money, known);
        }
        Assert.DoesNotMatch(money, Plain);

        // And the non-medical costs are named, because those are the ones that
        // actually decide it and the ones the leaflets leave out.
        foreach (var cost in new[] { "Travel", "Parking", "Meals", "Care for children", "Time away from work" })
        {
            Assert.Contains(cost, section, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheLeavingClaimIsSourcedAndItsUnsourcedHalfIsAQuestion()
    {
        // NCI's safety page: "even after you sign the consent form, you can
        // leave the study at any time." It does NOT say that leaving leaves
        // your normal care untouched — which is the reader's first worry. So
        // the page states what is sourced and turns the rest into a question
        // (§12.8, WI-506: answer the frightening question in both directions,
        // and where you cannot, say who can).
        var section = CuratedPage.Flatten(Reader(Section("Who is looking out for me?")));

        Assert.Matches(new Regex(@"You can leave at any time", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Signing the form does not lock you in", RegexOptions.IgnoreCase), section);

        // The unsourced half, framed as a thing to ask rather than an
        // assurance to give.
        Assert.Matches(new Regex(@"What we cannot tell you, and you should ask", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"what happens to your ordinary\s*care if you leave", RegexOptions.IgnoreCase),
            section);

        // POSITIONAL, NOT PHRASAL. /review beat the first version three ways,
        // and the worst one contained neither a trigger word nor a banned verb:
        // "**Your team will carry on looking after you exactly as before.**"
        // would sit two lines above the sentence that refuses that very
        // assurance, so the page would both give and withhold it.
        //
        // So: inside this section, any sentence that pairs leaving with care
        // must also hand the question over ("ask", "we cannot tell you").
        var leaving = new Regex(
            @"\b(?:leav\w*|stop\w*|pull(?:ing)? out|withdraw\w*|drop(?:ping)? out|quit\w*|"
            + @"carry on|carries on|go(?:es)? on|afterwards)\b",
            RegexOptions.IgnoreCase);
        var aboutCare = new Regex(@"\bcare\b", RegexOptions.IgnoreCase);
        var handsItOver = new Regex(
            @"\bask\b|\bwe cannot tell you\b|\bhas to come from\b|\bthe team running\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Leaving a trial will not affect your usual care.",
                     "Your team will carry on looking after you exactly as before, with the same care.",
                     "If you pull out, your normal care goes on unchanged.",
                     "Leaving has no bearing on the care you get afterwards.",
                 })
        {
            Assert.True(leaving.IsMatch(known) && aboutCare.IsMatch(known),
                "the leaving guard cannot even see: " + known);
            Assert.DoesNotMatch(handsItOver, known);
        }

        var assurances = CuratedPage.SentencesOf(PlainOf(Section("Who is looking out for me?")))
            .Where(s => leaving.IsMatch(s) && aboutCare.IsMatch(s))
            .Where(s => !handsItOver.IsMatch(s))
            .ToList();
        Assert.True(assurances.Count == 0,
            "this page tells the reader what leaving does to their ordinary care, which no source "
            + "says:\n  " + string.Join("\n  ", assurances));

        // Consent is a process rather than a form, which is the half that
        // changes what a reader does in the room.
        Assert.Matches(new Regex(@"Consent is a conversation, not a form", RegexOptions.IgnoreCase), section);

        // And the page says who checks the trial, by role rather than by
        // acronym: "IRB" is the jargon a reader will not recognise.
        Assert.Matches(new Regex(@"review board", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(@"\bIRB\b|\bDSMB\b|institutional review board"), Everything);
    }

    [Fact]
    public void ThePageCarriesNoHypeVocabulary()
    {
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "treatments/clinical-trials");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, promoted here rather than site-wide per §12.8 (WI-510:
        // promote at the SECOND page). Every one of these is the vocabulary a
        // trials page attracts and no other page in the corpus needs, and all
        // were run over the whole corpus first: none occurs anywhere.
        //
        // "hope" is deliberately NOT on this list. The caregiver section says
        // "watch for the hope gap", which is the correct and useful way to use
        // the word, and a substring ban that fails a correct sentence is worse
        // than no ban (§12.8, WI-509).
        foreach (var phrase in new[]
                 {
                     "breakthrough", "cutting edge", "cutting-edge", "last chance", "miracle",
                     "revolutionary", "game changer", "game-changer", "promising", "wonder drug",
                     "new hope", "only option", "nothing to lose",
                 })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // And the selling line from the NBTS page the dossier mis-cited, which
        // is the exact shape this page refuses (front matter, ruling 2).
        Assert.DoesNotContain("early access", CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);

        // A SUBSTRING LIST CANNOT SEE A PARAPHRASE, and /review proved it five
        // ways. The one that matters most is the NBTS selling line in other
        // words — "put a treatment in your hands years before it reaches the
        // ordinary clinic" — which is precisely what ruling 2 keeps off the
        // page, and which contains none of the banned strings. So the property
        // is claim-shaped: a trial may not be framed as EARLY ACCESS, as the
        // ONLY route to something, or as BETTER CARE than standard treatment.
        var sellingFrame = new Regex(
            // access / earliness
            @"\b(?:before it (?:reaches|gets to|arrives)|years before|ahead of|sooner than|"
            + @"first in line|get(?:ting)? (?:it|them|the drug) (?:first|early)|"
            + @"in your hands)\b|"
            // exclusivity
            + @"\bthe only way to (?:get|have|try)\b|\byour only\b|"
            // better than standard care
            + @"\b(?:watched|monitored|looked after|cared for) more (?:closely|carefully) than\b|"
            + @"\bbetter (?:care|treatment|odds|chance) than\b|"
            // superlative enthusiasm
            + @"\bmost exciting\b|\bstate of the art\b|\bbest (?:shot|hope|bet|option)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A trial can put a treatment in your hands years before it reaches the ordinary clinic.",
                     "For some drugs a trial is the only way to get them at all.",
                     "Some of the most exciting work in brain tumors is happening in trials.",
                     "It is state of the art care, and for some people it is their best shot.",
                     "People in trials are watched more closely than people on standard care.",
                 })
        {
            Assert.Matches(sellingFrame, known);
        }
        Assert.DoesNotMatch(sellingFrame, Plain);
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // The first draft shipped THREE — "petrol", "centre" and "favour" —
        // which is the most any page in the corpus has carried, because a costs
        // list and a "trial centre" both pull that way (§12.8, WI-510).
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in new[]
                 {
                     "tumour", "petrol", "favour", "chemist", "consultant", "specialist nurse",
                     "randomis", "hospitalis", "999", "A&E", "car park", "holiday",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(new Regex(@"\bGP\b"), body);

        // The US forms used instead, so a rewrite cannot drop the concept
        // rather than translating it.
        Assert.Contains("gas or fares", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("randomiz", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThePageCarriesNoEscalationListAndThatIsDeliberate()
    {
        // §12.8 (WI-521 and WI-520 both dropped one, with a reason): this
        // page's subject is a DECISION, not a procedure. The reader has no
        // symptoms from it, and a trial's own out-of-hours number is the
        // trial's to give.
        //
        // Inventing a tier list here would be WI-524's defect: a page-local
        // tier that contradicts the corpus by omission.
        Assert.DoesNotMatch(new Regex(@"Call an ambulance|call 911|same day if|"
            + @"Call your team the same day", RegexOptions.IgnoreCase), Everything);

        // A FOUR-PHRASE BAN IS NOT A TIER CHECK. /review inserted a complete
        // page-local escalation list — "**While you are in a trial, call the
        // trial team first, not your usual clinic.** Tell them about:" followed
        // by "A fever. / New weakness. / A seizure." — and every test stayed
        // green, including the composed-page ambulance count. That is the worst
        // available outcome on this page: /treatments/chemotherapy owns the
        // fever rule and the hubs file new weakness as same-day or as an
        // ambulance, so a reader told to call the trial team first about a
        // fever has been routed away from the page that has the threshold.
        //
        // So the check is STRUCTURAL, which is the form WI-521 proved: an
        // urgency-flavoured lead-in followed by a run of symptom bullets. It
        // matches the SHAPE of an escalation list rather than its vocabulary.
        var urgency = new Regex(
            @"\b(?:call|phone|contact|tell|go to)\b[^.\n]{0,60}"
            + @"\b(?:straight ?away|right away|at once|immediately|first|urgently|"
            + @"same day|today|emergency|out of hours|two in the morning)\b",
            RegexOptions.IgnoreCase);
        var symptom = new Regex(
            @"\b(?:fever|temperature|seizure|weakness|confusion|vomiting|throwing up|"
            + @"headache|breathing|rash|chest pain|cannot be woken)\b",
            RegexOptions.IgnoreCase);

        // Walk the raw page paragraph by paragraph: a lead-in that reads as
        // urgent, immediately followed by two or more symptom bullets, is an
        // escalation list whatever words it uses.
        var lines = Page.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (!urgency.IsMatch(lines[i]))
            {
                continue;
            }

            var bullets = 0;
            for (var j = i + 1; j < lines.Length && bullets < 3; j++)
            {
                var line = lines[j].Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (!line.StartsWith("- ", StringComparison.Ordinal)
                    && !Regex.IsMatch(line, @"^\d+\. "))
                {
                    break;
                }
                if (symptom.IsMatch(line))
                {
                    bullets++;
                }
            }

            Assert.True(bullets < 2,
                "this page has grown an escalation list: an urgent lead-in followed by symptom "
                + $"bullets, at line {i + 1}:\n  {lines[i].Trim()}\n\nThe corpus owns these "
                + "thresholds (Content/blocks/escalation.md, /treatments/chemotherapy for fever), "
                + "and a trials page that routes around them sends a reader to the wrong number.");
        }

        // Proven able to fire: the harness's own insertion.
        var canary = "**While you are in a trial, call the trial team first.** Tell them about:\n"
            + "\n- A fever.\n- New weakness.\n- A seizure.\n";
        var canaryLines = canary.Split('\n');
        var canaryHit = false;
        for (var i = 0; i < canaryLines.Length; i++)
        {
            if (!urgency.IsMatch(canaryLines[i]))
            {
                continue;
            }
            var n = 0;
            for (var j = i + 1; j < canaryLines.Length; j++)
            {
                var line = canaryLines[j].Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (!line.StartsWith("- ", StringComparison.Ordinal))
                {
                    break;
                }
                if (symptom.IsMatch(line))
                {
                    n++;
                }
            }
            if (n >= 2)
            {
                canaryHit = true;
            }
        }
        Assert.True(canaryHit, "the structural escalation check cannot fire, so its pass proves nothing");

        // AND THE SAME CHECK ON THE COMPOSED PAGE, because the ban above runs
        // over the RAW page, where `[CAREGIVER]` is still a literal string —
        // §12.10 (WI-514's trap). The composed page DOES contain the words
        // "call an ambulance", once, from the block ("ask what counts as 'call
        // us today' and what counts as 'call an ambulance' for this tumor").
        // That is the block telling a caregiver to go and get their own tumor's
        // thresholds, which is the opposite of this page inventing them. Pinned
        // at exactly one occurrence so a page-local tier cannot hide behind it.
        var composed = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Composed(Page)));
        var ambulance = Regex.Matches(composed, @"call an ambulance", RegexOptions.IgnoreCase).Count;
        Assert.True(ambulance == 1,
            $"the composed page says 'call an ambulance' {ambulance} times; exactly one is expected, "
            + "from the caregiver block, and any more means this page has grown a tier list");
        Assert.Matches(new Regex(@"what counts as .call an ambulance. for this tumor",
            RegexOptions.IgnoreCase), composed);

        // What the reader gets instead: the question, in the list they can take
        // to the person who can answer it.
        var questions = CuratedPage.Flatten(Reader(Section("What to ask your team")));
        Assert.Matches(new Regex(@"Who do I call if something goes wrong at two in the morning",
            RegexOptions.IgnoreCase), questions);

        // And the escalation block is NOT included, which is the mechanical
        // half of the same decision.
        Assert.DoesNotContain("[ESCALATION]", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): corpus-wide, not hand-picked neighbours. The nearest
        // here is the /trials Razor page, which is not under Content/ and so is
        // checked separately by the render tests.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("clinical-trials.md", StringComparison.Ordinal))
            .Concat(Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"));

        foreach (var file in others)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var overlaps = Shingles(CuratedPage.ReaderText(File.ReadAllText(file)), 8)
                .Where(pageShingles.Contains)
                .Where(s => !AllowedShingles.Contains(s))
                .Where(CarriesContent)
                .Distinct()
                .ToList();

            Assert.True(overlaps.Count == 0,
                $"this page restates {slug}:\n  " + string.Join("\n  ", overlaps));
        }

        // AND THE FINDER, WHICH NO SITE-WIDE RULE CAN SEE. /trials is a Razor
        // page, so it is outside CuratedPage.AllPages() and outside the loop
        // above — and it is the page this one is likeliest to restate, because
        // it already explains the subject in three sentences. The harness
        // proved it: lifting the finder's opening paragraph verbatim walked
        // through on both line endings.
        var finder = File.ReadAllText(Path.Combine(
            CuratedPage.PagesDirectory, "..", "..", "Pages", "Trials", "Index.cshtml"));

        // Razor markup and directives stripped first, or the shingles are made
        // of `@Model` and `div class`.
        var finderProse = Regex.Replace(finder, @"(?s)@\*.*?\*@", " ");
        finderProse = Regex.Replace(finderProse, @"<[^>]+>", " ");
        finderProse = Regex.Replace(finderProse, @"@[\w.()]+", " ");

        var finderOverlaps = Shingles(finderProse, 8)
            .Where(pageShingles.Contains)
            .Where(CarriesContent)
            .Distinct()
            .ToList();

        Assert.True(finderOverlaps.Count == 0,
            "this page restates /trials, which does the finding while this page does the "
            + "thinking:\n  " + string.Join("\n  ", finderOverlaps));

        // Proven able to fire: the finder's own opening, which is exactly what
        // the harness inserted.
        Assert.Contains("A clinical trial is a research study that people can join", finderProse,
            StringComparison.Ordinal);
        Assert.True(Shingles(finderProse, 8).Any(CarriesContent),
            "no content-bearing shingle was extracted from /trials, so the check above proves nothing");
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheBlockAndAddsWhatIsOnlyTrueHere()
    {
        var raw = Regex.Match(Page, @"^## For the person going with them.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.Contains("[CAREGIVER]", raw, StringComparison.Ordinal);

        var headings = Headings();
        Assert.Equal(headings.IndexOf("What you need first") + 1,
            headings.IndexOf("For the person going with them"));

        // Shingle check against the block, not the bold-lead-in check §12.8
        // calls insufficient — and WI-525's end-to-end read found a paragraph
        // restating the block in DIFFERENT words ten lines below it, which the
        // shingle check also cannot see. So the section's own claims are pinned
        // by name.
        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---", "",
            RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section("For the person going with them")), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  " + string.Join("\n  ", own));

        var care = CuratedPage.Flatten(Reader(Section("For the person going with them")));
        Assert.Matches(new Regex(@"absorbing the extra visits", RegexOptions.IgnoreCase), care);
        // The end-to-end read found this paragraph opening "Two people hear a
        // consent conversation better than one", which is the [CAREGIVER]
        // block's own "Two people in the room hear more than one" said again in
        // different words twenty lines below it. The shingle check above cannot
        // see it — no shared eight-word run — and this is WI-525's lesson
        // repeated one item later. What this page owes on top of the block is
        // the CONSENT APPOINTMENT specifically.
        Assert.Matches(new Regex(@"Go to the consent conversation", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"Ask whether you can be there before the date is set",
            RegexOptions.IgnoreCase), care);
        Assert.DoesNotMatch(new Regex(@"hear a consent conversation better than one",
            RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"Watch for the hope gap", RegexOptions.IgnoreCase), care);

        // And it does not hand the caregiver the decision.
        // /review handed the caregiver the patient's decision with "You are the
        // one who has to decide whether this is realistic", which matched
        // nothing: the first version required the modal `should|can` directly
        // before the verb.
        var decides = new Regex(
            @"\byou (?:should|can|have to|must|are the one to|are the one who)\b[^.]{0,30}"
            + @"\b(?:decide|deciding|say no|talk them out|persuade|choose)\b|"
            + @"\bit is your (?:call|decision|choice)\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "If it is too much travel you should say no for them.",
                     "You are the one who has to decide whether this is realistic.",
                 })
        {
            Assert.Matches(decides, known);
        }
        Assert.DoesNotMatch(decides, care);
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8: read the section for the slot list, not the last page written.
        //
        // This page's subject is a DECISION rather than a procedure, so slot 4
        // ("how long does it take?") folds into the step-by-step as its closing
        // line, and slot 5 becomes "what is being in one actually like?" — the
        // same bend WI-507 made for a page about a wait and WI-508 for a page
        // about a document. Three 6c sections, because three separate
        // genuinely-asked worries do not fit under one heading.
        Assert.Equal(
            [
                "The short version",                    // 0
                "What is it, and what is it not?",      // 1
                "When should I ask?",                   // 2
                "What happens, step by step",           // 3
                "What is being in one actually like?",  // 5 + 6a
                "Will I get the new treatment?",        // 6c
                "Will I be given a sugar pill?",        // 6c
                "What do the phases mean?",             // 6c
                "Who is looking out for me?",           // 9
                "What will it cost me?",                // 6a
                "What you need first",                  // 7
                "For the person going with them",       // caregiver
                "What to ask your team",                // 10
                "Where to go next",                     // 11
            ],
            Headings());
    }

    [Fact]
    public void TheAnchorsArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface, and
        // /treatments/chemotherapy already deep-links #already-started.
        foreach (var (heading, anchor) in new[]
                 {
                     ("## The short version", "short-version"),
                     ("## What is it, and what is it not?", "what-it-is"),
                     ("## When should I ask?", "when-to-ask"),
                     ("### If you have already started treatment", "already-started"),
                     ("## What happens, step by step", "step-by-step"),
                     ("## What is being in one actually like?", "what-its-like"),
                     ("## Will I get the new treatment?", "randomization"),
                     ("## Will I be given a sugar pill?", "placebo"),
                     ("## What do the phases mean?", "phases"),
                     ("## Who is looking out for me?", "who-watches"),
                     ("## What will it cost me?", "costs"),
                     ("## What you need first", "what-you-need-first"),
                     ("## For the person going with them", "caregiver"),
                     ("## What to ask your team", "questions"),
                     ("## Where to go next", "where-to-go-next"),
                 })
        {
            Assert.Matches(new Regex("^" + Regex.Escape(heading) + @" \{#" + anchor + @"\}\s*$",
                RegexOptions.Multiline), Page);
        }

        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate() =>
        Assert.DoesNotContain(":::", Page);

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        var headings = Headings();
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);

        // The questions carry the four that decide what the reader is agreeing
        // to, because the body's answer to each is "ask".
        var questions = CuratedPage.Flatten(Reader(Section("What to ask your team")));
        foreach (var owed in new[]
                 {
                     "Is there a trial I should know about",
                     "Would starting this treatment now affect a trial later",
                     "Is it randomized, and if it is, what are the groups",
                     "Is there a placebo, and if there is, what do I get alongside it",
                     "Which costs are covered, and which land on me",
                     "Can I leave, and what happens to my usual care if I do",
                 })
        {
            Assert.Contains(owed, questions, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        foreach (var domain in new[] { "cancer.gov", "braintumor.org" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // The page whose verbatims the spine rests on, by name.
        Assert.Contains(urls, u => u.Contains("when-to-consider-a-brain-tumor-clinical-trial",
            StringComparison.Ordinal));

        // §12.14 and the known-dead list, plus the two rejected here.
        foreach (var rejected in new[]
                 {
                     "medscape.com", "sciencedirect.com", "academic.oup.com", "mayoclinic.org",
                     "hopkinsmedicine.org", "mdpi.com", "journals.lww.com", "ascopubs.org",
                     // The URL the dossier cites for a statement it does not
                     // contain (front matter, ruling 2). Banned as a CITATION,
                     // and the claim it was carrying is checked separately by
                     // NothingOnThePageReadsAsMatchingOrEligibility — §12.14's
                     // rule is that banning the citation is not fixing the claim.
                     "mytumorid",
                     // The 404 (front matter, ruling 9).
                     "clinical-trials/patient-safety",
                     // A second trial finder, where /trials is this site's own.
                     "clinical-trial-finder",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(rejected, StringComparison.OrdinalIgnoreCase)),
                $"{rejected} is cited; the front matter records why it was not used");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheDoorsOnTheSiblingPagesAreAppendedSentences()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped.
        // Each sibling's pre-existing sentence is pinned beside the new door,
        // because a door that REPLACED a paragraph passes a bare Contains
        // identically (§12.14).
        foreach (var (folder, slug, kept) in new[]
                 {
                     ("", "start", "Is there a clinical trial that fits me?"),
                     // The sentence the door was APPENDED TO, not one three
                     // sentences earlier. The harness replaced "If wafers come
                     // up, ask about that before the operation, not after" and
                     // this test stayed green on both line endings, because the
                     // pin sat in front of the replaced text and survived it.
                     ("treatments", "chemotherapy", "ask about that before the operation, not"),
                     ("tumors", "glioma", "Which of these are open to you depends on your diagnosis"),
                     ("tumors", "high-grade-glioma", "A different drug."),
                     ("tumors", "astrocytoma", "Which of them are open to you"),
                     ("tumors", "glioblastoma", "tumor treating fields"),
                     // NOT the age-cutoff passage at :259 ("It was written to decide who
                     // could join the trial") -- /review's point: that is 190 lines
                     // from the door, so the distance check below could not see a
                     // door that had replaced the bullet. The door is on the
                     // questions list, so the neighbouring question is the pin.
                     ("tumors", "low-grade-glioma", "What should make me call you before the next appointment?"),
                     ("tumors", "oligodendroglioma", "What is\nright depends on what was used"),
                     ("tests", "molecular-markers", "open up a treatment or a"),
                 })
        {
            var path = folder.Length == 0
                ? CuratedPage.Read(slug + ".md")
                : CuratedPage.Read(folder, slug + ".md");
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(path));
            Assert.Contains("/treatments/clinical-trials", text, StringComparison.Ordinal);
            Assert.Contains(CuratedPage.Flatten(kept), text, StringComparison.Ordinal);

            // AND THE TWO HAVE TO BE NEAR EACH OTHER. /review replaced
            // /tumors/low-grade-glioma's door bullet outright and the test
            // stayed green, because that page's pinned sentence is 190 lines
            // away in a different section — so the guard proved the page still
            // contains both strings, which is not the property it is named
            // for. The property is that the door was APPENDED to that
            // sentence's neighbourhood, not swapped in for it.
            // The NEAREST door, because /review's own finding led to a second
            // door on three hubs (the at-diagnosis sentence as well as the
            // recurrence list), and taking the first occurrence measured the
            // wrong pair.
            var keptAt = text.IndexOf(CuratedPage.Flatten(kept), StringComparison.Ordinal);
            var doorAt = -1;
            var best = int.MaxValue;
            for (var at = text.IndexOf("/treatments/clinical-trials", StringComparison.Ordinal);
                 at >= 0;
                 at = text.IndexOf("/treatments/clinical-trials", at + 1, StringComparison.Ordinal))
            {
                if (Math.Abs(at - keptAt) < best)
                {
                    best = Math.Abs(at - keptAt);
                    doorAt = at;
                }
            }
            Assert.True(doorAt >= 0, $"/{slug}: no door found");
            Assert.True(Math.Abs(doorAt - keptAt) < 900,
                $"/{slug}: the door and the sentence it was appended to are {Math.Abs(doorAt - keptAt)} "
                + "characters apart, so this test would pass even if the door had replaced a "
                + "different paragraph somewhere else on the page");
        }
    }

    [Fact]
    public void ThePageDoesNotContradictTheTrialsFinderItLinksTo()
    {
        // /trials is a Razor page, so it is outside CuratedPage.AllPages() and
        // no site-wide content rule reads it. Its disclaimer panel makes one
        // claim this page must agree with, and its intro makes another. Read
        // rather than remembered (§12.10).
        // Derived from PagesDirectory (.../src/BrainHarbor.Web/Content/pages)
        // rather than from a second repo-root walk, so there is one definition
        // of where the app lives.
        var finder = File.ReadAllText(Path.Combine(
            CuratedPage.PagesDirectory, "..", "..", "Pages", "Trials", "Index.cshtml"));

        Assert.Contains("Talk to your care team before you contact a trial", finder, StringComparison.Ordinal);
        Assert.Contains("Joining one is always your choice", finder, StringComparison.Ordinal);

        // This page says the same two things in its own words rather than
        // louder or quieter.
        Assert.Matches(new Regex(@"Either answer is a normal answer", RegexOptions.IgnoreCase), Body);
        Assert.Matches(new Regex(@"Your own team can, and that is the ask", RegexOptions.IgnoreCase), Body);

        // And the finder now carries the door back, so a reader who lands on
        // the list from a search engine gets the frame too.
        Assert.Contains("/treatments/clinical-trials", finder, StringComparison.Ordinal);
    }

    private static List<string> Headings() =>
        Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    private static readonly HashSet<string> Stopwords =
    [
        "a", "an", "and", "are", "as", "at", "be", "been", "before", "but", "by", "can", "do",
        "does", "for", "from", "get", "go", "had", "has", "have", "how", "i", "if", "in", "into",
        "is", "it", "its", "like", "may", "me", "more", "much", "my", "no", "not", "of", "on",
        "one", "or", "other", "our", "out", "over", "own", "so", "some", "than", "that", "the",
        "their", "them", "then", "there", "these", "they", "this", "to", "up", "was", "we",
        "were", "what", "when", "which", "who", "will", "with", "would", "you", "your",
    ];

    private static bool CarriesContent(string shingle) =>
        shingle.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3;

    private static IEnumerable<string> Shingles(string text, int n)
    {
        text = Regex.Replace(text, @"^#{1,6} .*$", " ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\]\([^)]*\)", "] ");
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+").Select(m => m.Value).ToList();
        for (var i = 0; i + n <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(n));
        }
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class ClinicalTrialsPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/clinical-trials";

    private readonly WebApplicationFactory<Program> _factory;

    public ClinicalTrialsPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Clinical trials: how to think about one", html);
        Assert.Contains("That is not how it works", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/trials", "/start", "/treatments/chemotherapy", "/tests/molecular-markers",
                     "/tumors/glioma", "/tumors/high-grade-glioma", "/tumors/astrocytoma",
                     "/tumors/glioblastoma", "/tumors/low-grade-glioma", "/tumors/oligodendroglioma",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/trials", "/tests/molecular-markers", "/treatments/chemotherapy",
            "/tumors", "/get-help-now");

    // NOTE: CuratedPage.AssertFragmentLinksResolve is deliberately NOT called
    // here. This page has no fragment links of its own — every door it opens
    // goes to the top of another page — and the helper fails loudly rather
    // than pass on nothing ("has no fragment links, so this assertion proved
    // nothing"). That is the helper working: a green test that cannot fail is
    // worse than no test (§12.10). The deep link that matters on this item
    // points INTO this page, and the test below checks that direction.

    [Fact]
    public async Task TheChemotherapyDeepLinkFindsItsAnchorOnThisPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);
        Assert.Contains("id=\"already-started\"", html, StringComparison.Ordinal);

        var chemo = await _factory.CreateClient().GetStringAsync("/treatments/chemotherapy");
        Assert.Contains("/treatments/clinical-trials#already-started", chemo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NoGlossaryTermFiresOnThisPageAndThatIsRecordedRatherThanHidden()
    {
        // This page adds no glossary terms and suppresses none — and it turns
        // out that NO existing entry fires on it either. That is not an
        // oversight, it is what a page about a decision looks like: it uses no
        // technical vocabulary the glossary owns, which is also why it reads at
        // 4.9 against a corpus averaging above 5.
        //
        // Pinned the way WI-521 pinned the unreachable RANO entry: the state is
        // asserted, so a later edit that introduces a glossary word turns this
        // RED on purpose. The fix then is a decision — suppress it here if this
        // page defines the word, leave it firing if it does not (§12.8,
        // WI-509) — rather than a silent change in behaviour.
        Assert.DoesNotContain("!%", CuratedPage.Read("treatments", "clinical-trials.md"));

        var html = await _factory.CreateClient().GetStringAsync(Url);
        Assert.DoesNotContain("def-", html, StringComparison.Ordinal);

        // The mechanism is alive, so the assertion above is about this page
        // rather than about a broken renderer.
        Assert.Contains("def-", await _factory.CreateClient().GetStringAsync("/treatments/chemotherapy"),
            StringComparison.Ordinal);
    }
}
