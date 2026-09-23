using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-549: <c>/tests/neuro-exam-and-memory-testing</c>. The tenth TESTS page on
/// §12.8's template, and the first in the corpus to carry two CO-EQUAL subjects
/// that are not the same test — the bedside neurological exam, and formal
/// neuropsychological testing. (Not the first with two subjects at all:
/// <c>/tests/planning-scans</c> carries six scan types. What is new is that
/// neither subject here is the dominant case with the other routed away.)
///
/// THE RULING THIS PAGE TURNS ON (work_files/wi549/RULING.md, §12.8 slot table):
/// a slot is carried once where the two tests give one honest answer, and twice
/// where they give two. The failure the ruling exists to prevent is AVERAGING —
/// one blended answer presented as if it covered both readers. A bedside exam
/// runs minutes; a testing day runs hours. A page that splits the difference
/// lies to both.
///
/// What is pinned here is where this page could leave a reader worse off:
///
///   * AVERAGING THE DURATIONS. §12.4 R1 makes an orienting duration
///     publishable, which is exactly what makes a blended one dangerous. Both
///     answers must be present, in the same section, attributable.
///   * THE DOSSIER'S UNSOURCED NUMBER. docs/research/tumor-guides/tests-library.md
///     says the bedside exam takes "10 to 20 minutes" and cites NOTHING for it,
///     while every neighbouring claim in its §7 carries a URL. §12.8: a dossier
///     is not a source. That figure is banned by name.
///   * COMFORTING THE READER INTO A LIE. The corpus already says the reader
///     cannot fail, three times, and every one is about a SCAN, where the reader
///     does nothing and a machine takes a picture. Neuropsychological testing IS
///     scored, against age- and education-matched norms. Repeating the scan
///     pages' sentence here would be false in a way theirs are not. This page
///     owns the hard version: scored first, then nobody fails. ORDER is the
///     property, not presence.
///   * ASSERTING WHAT COULD NOT BE READ. hhs.gov returned 403 twice, so the
///     claim that an employer cannot obtain the report without authorization is
///     NOT ASSERTED. It appears only as a question, which asserts nothing
///     (§12.8, WI-519). A test keeps it there.
///   * RESTATING /tumors/pediatric-brain-tumor, which owns the pediatric
///     cognitive-late-effects material outright and deliberately never says
///     "neuropsychological". This page is the adult, named-discipline slot.
///   * CARRYING A DRIVING RULE. /seizures/living-with#driving owns driving and
///     the rules are jurisdictional. Route, never restate, and never a duration.
/// </summary>
public sealed class NeuroExamAndMemoryTestingPageContentTests
{
    private const string Slug = "tests/neuro-exam-and-memory-testing";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "Two different things, often in the same conversation";
    private const string WhyHeading = "Why am I having this?";
    private const string StepHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string JudgedHeading = "Am I being judged?";
    private const string ReportHeading = "What the report is used for";
    private const string BringHeading = "What to sort out beforehand";
    private const string AgainHeading = "Why am I having this again?";
    private const string ResultsHeading = "Who reads it, and how do I get the result?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    /// <summary>
    /// The two subjects, as the page labels them. Every guard about the split
    /// reads THESE rather than re-typing the words, so a relabelling moves the
    /// guards with the page instead of leaving them green and pointing nowhere.
    /// </summary>
    private const string ExamLabel = "The exam in the room";

    private const string TestsLabel = "The thinking and memory tests";

    private static string Page => CuratedPage.Read("tests", "neuro-exam-and-memory-testing.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    // §12.8 (WI-509/L3394): ReaderText strips front matter BY INDEX, so handing
    // it a Section() silently returns section[3..]. Strip the markers directly.
    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Everything => Headline + " " + Body;

    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Unit = @"(?:minutes?|hours?|days?|weeks?|months?|years?)";

    /// <summary>
    /// Every shape this page prints a duration in. ROUND 1 (S1) found the
    /// count-word form alone matched only four of the page's durations and
    /// missed "half an hour to an hour" — half of the ruling's own split — plus
    /// "several hours" and "most of the day". A duration guard that cannot see
    /// the durations is a guard whose clean result means nothing, and its
    /// allow-list quietly fills with entries that exempt nothing.
    /// </summary>
    private const string DurationRx =
        $@"(?i)(?:\b{CountWord}\s*(?:to|-|or)?\s*(?:{CountWord}\s*)?{Unit}\b"
        + $@"|\bhalf an hour(?:\s+to\s+an\s+hour)?\b"
        + $@"|\bseveral\s+{Unit}\b"
        + @"|\bmost of the day\b"
        + @"|\ban hour\b"
        // ROUND 3 (N6): "most of A day" as well as "most of THE day", so a
        // reword cannot slip past both the pairing guard and the allow-list.
        // The bare article form ("a day") is still deliberately excluded: it
        // matched "the practical side of a day", a figure of speech rather than
        // a duration, and §12.8 warns that too-wide is the worse direction.
        + @"|\bmost of (?:the|a) day\b)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): -1 is less than everything, so an order test built on
        // `>` alone stays green when a whole section is deleted. Presence first.
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, StepHeading, LongHeading, FeelHeading,
            JudgedHeading, ReportHeading, BringHeading, AgainHeading, ResultsHeading,
            QuestionsHeading, NextHeading,
        ];

        var missing = required.Where(h => !headings.Contains(h)).ToList();
        Assert.True(missing.Count == 0,
            "the page has lost these sections entirely:\n  " + string.Join("\n  ", missing)
            + "\nheadings actually present:\n  " + string.Join("\n  ", headings));

        var positions = required.Select(h => headings.IndexOf(h)).ToList();
        var outOfOrder = new List<string>();
        for (var i = 1; i < positions.Count; i++)
        {
            if (positions[i] <= positions[i - 1])
            {
                outOfOrder.Add($"'{required[i]}' has moved above '{required[i - 1]}'");
            }
        }

        Assert.True(outOfOrder.Count == 0, string.Join("\n", outOfOrder));

        // ROUND 1 nit: presence-then-order says nothing about whether a section
        // has a BODY. §12.15 uses a 120-character floor on the composed page for
        // exactly this; a heading emptied to a stub would satisfy everything
        // above. Collect, then assert (§12.8, WI-548).
        var thin = required
            .Select(h => (Heading: h, Body: RawSection(h).Trim()))
            .Where(s => s.Body.Length < 120)
            .Select(s => $"'{s.Heading}' has a {s.Body.Length}-character body")
            .ToList();

        Assert.True(thin.Count == 0,
            "these sections are present by name but effectively empty:\n  "
            + string.Join("\n  ", thin));
    }

    /// <summary>
    /// THE RULING, AS A TEST. Every slot where the two tests give different
    /// answers must name BOTH, separately. This is the guard against the one
    /// failure the whole item turns on: a single blended answer standing in for
    /// two that differ.
    ///
    /// §12.8 (WI-548): an accumulation loop must COLLECT and then assert, or the
    /// first offending section hides every one after it. And the numeric canary
    /// underneath has to stay reachable once the readable report fires first.
    /// </summary>
    [Fact]
    public void EverySlotWhereTheTwoTestsDifferNamesBothOfThemRatherThanBlendingThem()
    {
        // The slots whose answer genuinely differs between a five-minute exam in
        // a clinic room and a testing day with a written report.
        string[] split =
        [
            ShortHeading, WhatHeading, WhyHeading, StepHeading, LongHeading, FeelHeading,
            JudgedHeading, BringHeading, ResultsHeading,
        ];

        var blended = new List<string>();
        foreach (var heading in split)
        {
            var section = PlainOf(heading);
            var hasExam = section.Contains(ExamLabel, StringComparison.OrdinalIgnoreCase);
            var hasTests = section.Contains(TestsLabel, StringComparison.OrdinalIgnoreCase);

            if (!hasExam || !hasTests)
            {
                blended.Add($"'{heading}' names only "
                    + (hasExam ? ExamLabel : hasTests ? TestsLabel : "NEITHER")
                    + ", so one of the two readers is answered and the other is not");
            }
        }

        Assert.True(blended.Count == 0,
            "a slot whose answer differs between the two tests has been blended into one:\n  "
            + string.Join("\n  ", blended));

        // ROUND 1 (S2): the previous canary asserted labelHits >= split.Length * 2,
        // which is ENTAILED by the loop above passing — nine sections each
        // holding both labels guarantee eighteen occurrences. A tautology
        // dressed as an independent check.
        //
        // The independent property: the labels must SELECT, i.e. they must fail
        // to appear in a section that has none of them. The "Where to go next"
        // list is that section — it is pure routing, names neither label, and
        // is not in the split list. If the matcher started matching everything,
        // this goes red while the loop above stays green.
        var routing = PlainOf(NextHeading);
        Assert.False(routing.Contains(ExamLabel, StringComparison.OrdinalIgnoreCase)
                     || routing.Contains(TestsLabel, StringComparison.OrdinalIgnoreCase),
            "a label matched inside the routing list, which names neither test. The label "
            + "matcher has stopped discriminating, so the loop above proves nothing.");
    }

    /// <summary>
    /// A unifying sentence has to arrive BEFORE the per-subject answers it
    /// joins. Round 4 (4) walked both failure modes: accepting one anywhere let
    /// an appended bullet switch the check off, and demanding it lead the
    /// section failed slot 8, whose unifier follows an opening line.
    /// </summary>
    private static bool UnifiesBeforeTheSplit(List<string> units)
    {
        var firstSingle = units.FindIndex(u =>
            Regex.IsMatch(u, ExamRx, RegexOptions.IgnoreCase)
            ^ Regex.IsMatch(u, TestsRx, RegexOptions.IgnoreCase));

        if (firstSingle < 0)
        {
            return true;
        }

        return units.Take(firstSingle).Any(IsUnifying);
    }

    /// <summary>
    /// Does this span JOIN the two subjects into one answer?
    ///
    /// ROUND 3 (S2) found the first version accepted a bare "both" or "either"
    /// anywhere in the section, so one unrelated "in either direction" switched
    /// the split check off for the whole slot. Tightening it to "joins AND names
    /// a test" then failed slot 8's own correct unifier — "Both of these are
    /// repeated on purpose" joins without naming either, which is ordinary
    /// English and the §12.8 warning about a rule that fails a correct page.
    ///
    /// So: "either" is dropped from the joining words (it is the incidental one),
    /// and a joining word counts only when the span also refers to the two of
    /// them — by naming one, or plurally.
    /// </summary>
    private static bool IsUnifying(string unit)
    {
        var namesExam = Regex.IsMatch(unit, ExamRx, RegexOptions.IgnoreCase);
        var namesTests = Regex.IsMatch(unit, TestsRx, RegexOptions.IgnoreCase);

        // ROUND 5 (4): a shortcut here used to treat "names both subjects" as
        // unifying on its own. That let a lead sentence which SPLITS — "The
        // exam in the room and the thinking and memory tests raise different
        // questions" — switch the check off: the round 4 append bypass, entered
        // from the top instead of the bottom. Nothing needs the shortcut; slot
        // 8's real unifier is "Both of these are repeated on purpose."
        _ = namesExam;
        _ = namesTests;

        // ROUND 4 (4): requiring a plural pronoun BESIDE the joining word failed
        // correct prose — "Both are repeated on purpose" and "The two of them
        // are repeated" are ordinary unifiers and were rejected. A joining word
        // is enough on its own; what stops it being satisfied by an incidental
        // "both" somewhere down the section is the CALLER, which only ever
        // offers this the section's FIRST span. A unifier leads; a stray
        // mention does not.
        return Regex.IsMatch(unit,
            @"\bboth\b|\beach of (?:them|these)\b|\bthe two of (?:them|these)\b",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// THE OTHER HALF OF THE RULING, which round 1 found unasserted (S15). It
    /// is two-sided: "a question with one answer must not be split merely for
    /// symmetry." Slots 8, 10 and 11 give one answer that covers both tests, and
    /// splitting them into two labelled halves later would be the same defect
    /// wearing the opposite coat.
    /// </summary>
    [Fact]
    public void TheSlotsWithOneHonestAnswerAreNotSplitForSymmetry()
    {
        string[] shared = [AgainHeading, QuestionsHeading, NextHeading];

        // ROUND 2 (S1): the previous version detected a split only through
        // DurationPairing, which needs a DURATION token in each half. None of
        // these three sections carries one and the realistic failure — splitting
        // the questions list into "For the exam in the room:" / "For the
        // thinking and memory tests:" — introduces none. The guard was
        // structurally unfalsifiable for two of its three headings.
        //
        // The real property: no shared section may contain two separate spans
        // that each name exactly one test, because that IS the split shape.
        var wronglySplit = new List<string>();
        foreach (var heading in shared)
        {
            var units = AttributionUnits(RawSection(heading));

            var examOnly = units.Count(u => Regex.IsMatch(u, ExamRx, RegexOptions.IgnoreCase)
                                            && !Regex.IsMatch(u, TestsRx, RegexOptions.IgnoreCase));
            var testsOnly = units.Count(u => Regex.IsMatch(u, TestsRx, RegexOptions.IgnoreCase)
                                             && !Regex.IsMatch(u, ExamRx, RegexOptions.IgnoreCase));

            // A shared slot MAY name each test while still giving one answer —
            // slot 8 does exactly that, opening "Both of these are repeated on
            // purpose" and then giving each one's specifics. What makes it one
            // answer is the UNIFYING span. A section with a span per test and
            // nothing joining them is the symmetry split.
            // ROUND 3 (S2): the exemption used to accept a bare "both" or
            // "either" ANYWHERE in the section, so one unrelated "in either
            // direction" disabled the split check for the whole slot. A
            // unifying span has to actually join the two things — it names at
            // least one of them in the same span as the joining word.
            // ROUND 4 (4): `units.Any` let ANY span anywhere carry the
            // exemption, so appending one bullet naming both tests switched the
            // split check off for the whole slot. Scoping it to units[0] then
            // failed slot 8, whose unifier is its SECOND paragraph after an
            // opening line. The property is ORDER: a unifying sentence must
            // come BEFORE the split it is unifying.
            var unified = UnifiesBeforeTheSplit(units);

            if (examOnly > 0 && testsOnly > 0 && !unified)
            {
                wronglySplit.Add($"'{heading}' now has {examOnly} span(s) addressing only the "
                    + $"exam and {testsOnly} addressing only the testing, with nothing joining "
                    + "them. Its answer covers both, so it must not be split for symmetry.");
            }
        }

        Assert.True(wronglySplit.Count == 0,
            "a slot with one honest answer has been split for symmetry:\n  "
            + string.Join("\n  ", wronglySplit));

        // Canary: the detector must SEE a split, and — round 3 (S2) — a stray
        // "either" must not neuter it. Both halves are probed, because the
        // previous canary proved only that the unit splitter worked and never
        // touched the exemption that was actually broken.
        static (int ExamOnly, int TestsOnly, bool Unified) Probe(string text)
        {
            var units = AttributionUnits(text);
            var examOnly = units.Count(u => Regex.IsMatch(u, ExamRx, RegexOptions.IgnoreCase)
                                            && !Regex.IsMatch(u, TestsRx, RegexOptions.IgnoreCase));
            var testsOnly = units.Count(u => Regex.IsMatch(u, TestsRx, RegexOptions.IgnoreCase)
                                             && !Regex.IsMatch(u, ExamRx, RegexOptions.IgnoreCase));
            return (examOnly, testsOnly, UnifiesBeforeTheSplit(units));
        }

        const string split =
            "- For the exam in the room: ask what they found.\n"
            + "- For the thinking and memory tests: ask who writes the report.\n";

        var plain = Probe(split);
        Assert.True(plain.ExamOnly > 0 && plain.TestsOnly > 0 && !plain.Unified,
            "the split detector cannot recognise an obvious two-way split, so its clean "
            + "verdict on the shared sections proves nothing");

        // ROUND 5 (6): this probe used to put the stray sentence AFTER the
        // split, which made it vacuous the moment the exemption became
        // order-sensitive — everything after the first single-subject span is
        // ignored, so it passed whatever word list IsUnifying used. In front,
        // where it can actually reach the exemption, it proves the narrowing.
        var withStrayEither = Probe("It can go either way.\n\n" + split);
        Assert.True(withStrayEither.ExamOnly > 0 && withStrayEither.TestsOnly > 0
                    && !withStrayEither.Unified,
            "a stray 'either' elsewhere in the section switched the split check off again, "
            + "which is the round 3 finding this exemption was narrowed to fix");

        // The other direction, so the narrowing is proved not to have gone too
        // far. ROUND 4 (4) found three correct unifiers being rejected, so all
        // of them are probed, and each must unify when it LEADS the section.
        foreach (var unifier in new[]
                 {
                     "Both of these are repeated on purpose.",
                     "Both are repeated on purpose.",
                     "The two of them are repeated on purpose.",
                 })
        {
            Assert.True(Probe(unifier + "\n\n" + split).Unified,
                $"'{unifier}' is no longer recognised as unifying, so the guard now fails a "
                + "correct page");
        }

        // ROUND 4 (4): and an incidental mention further down must NOT buy the
        // exemption. A unifying sentence leads; a stray one does not.
        Assert.False(Probe(split + "\n- Ask whether both will be repeated.\n").Unified,
            "a bullet appended below a genuine two-way split still switches the check off, "
            + "which is the round 4 bypass this scoping was meant to close");

        // ROUND 5 (4): the same bypass entered from the TOP. A lead sentence
        // that names both subjects while SPLITTING them is not a unifier, and
        // it is prose somebody would really write.
        Assert.False(
            Probe("The exam in the room and the thinking and memory tests raise different "
                  + "questions.\n\n" + split).Unified,
            "a lead sentence that names both subjects while splitting them still counts as "
            + "unifying, so the exemption can be bought from the top of the section");
    }

    /// <summary>
    /// §12.6: answer in the first sentence under the heading, because most
    /// readers never reach the second. Round 1 (S16) found this page pinned no
    /// landing sentence at all, while its siblings pin theirs — and its spine
    /// opened with a preamble.
    /// </summary>
    [Fact]
    public void TheSpineAnswersItsOwnQuestionInTheFirstSentence()
    {
        // ROUND 2 (S14): this guard and RULING.md §3 contradicted each other.
        // The guard demanded the section open with "No"; the ruling demanded it
        // concede the measuring FIRST, because a reader who meets reassurance
        // first has been handled rather than told. Asserting only "^No" also
        // passed a bare "No." with the concession pushed below the fold — the
        // comfort-first shape the ruling exists to forbid.
        //
        // Both are satisfiable at once, and the page does it: the OPENING
        // PARAGRAPH answers the heading AND concedes in the same breath. That
        // is the property, and it is what RULING.md now records.
        // ROUND 3 (N7): AttributionUnits re-splits a span naming both tests into
        // sentences, so if the opening is ever rewritten to name both, `opening`
        // collapses to a bare "No." and the concession assertion below fails a
        // CORRECT page. Take the first paragraph, which is the thing the reader
        // actually meets.
        var opening = Regex.Replace(
            CuratedPage.Flatten(RawSection(JudgedHeading).Split("\n\n").First()),
            @"[*_]", "").Trim();

        Assert.True(Regex.IsMatch(opening, @"^\s*No\b", RegexOptions.IgnoreCase),
            "'Am I being judged?' no longer answers itself where the reader meets it. A "
            + "reader who reads one line must get the answer, not a preamble about it:\n  "
            + opening);

        Assert.True(Regex.IsMatch(opening, @"measured|scored", RegexOptions.IgnoreCase),
            "the opening answers 'no' and defers the concession, so the reader is comforted "
            + "before being told. The admission belongs in the same breath:\n  " + opening);

        // §12.6: never end a section on a frightening sentence. The spine has to
        // land on something solid.
        var last = CuratedPage.SentencesOf(PlainOf(JudgedHeading)).LastOrDefault() ?? "";
        Assert.True(Regex.IsMatch(last, @"not a verdict|verdict on you", RegexOptions.IgnoreCase),
            "the section no longer lands on the sentence that puts a single result in its "
            + "place, so it may now end on the frightening half:\n  " + last);
    }

    /// <summary>
    /// DISTINCTNESS (§12.8, WI-548): asserting that a mapped thing EXISTS is
    /// satisfied by anything that exists, so one label must not be able to
    /// discharge the obligation belonging to the other.
    /// </summary>
    [Fact]
    public void TheTwoLabelsAreDistinctAndNeitherContainsTheOther()
    {
        Assert.NotEqual(ExamLabel, TestsLabel);

        Assert.False(ExamLabel.Contains(TestsLabel, StringComparison.OrdinalIgnoreCase)
                     || TestsLabel.Contains(ExamLabel, StringComparison.OrdinalIgnoreCase),
            "one label is a substring of the other, so a section naming only one of them "
            + "would satisfy the check for both");

        // Both must actually be on the page, or the distinctness above is a
        // statement about two strings and nothing about the page.
        Assert.Contains(ExamLabel, Plain, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(TestsLabel, Plain, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------- durations

    /// <summary>
    /// The raw text of one section, line structure intact. <c>CuratedPage.Section</c>
    /// flattens, which destroys the bullet boundaries the pairing guard below
    /// depends on.
    /// </summary>
    private static string RawSection(string heading)
    {
        var m = Regex.Match(Page,
            @"(?m)^##\s+" + Regex.Escape(heading) + @"[^\n]*\n(.*?)(?=^##\s|\z)",
            RegexOptions.Singleline);
        Assert.True(m.Success, $"section '{heading}' could not be sliced out of the page");
        return m.Groups[1].Value;
    }

    private const string MinutesFigure = @"\bminutes?\b";

    private const string HoursFigure = @"\bhours?\b|most of (?:the|a) day";

    /// <summary>
    /// An hours-shaped DURATION, not merely the word. ROUND 5 (1): the testing
    /// side's escape hatch keyed on <see cref="HoursFigure"/>, and <c>\bhour\b</c>
    /// matches inside "hour-long" — so "45 minutes, about the same as an
    /// hour-long visit" raised no fault AND counted as correctly paired. One
    /// extra word rebuilt the defect round 4 had just closed.
    /// </summary>
    private const string HoursDuration =
        // `\ban hour\b` alone still matched inside "an hour-long visit", because
        // a hyphen is a word boundary — the very bypass this constant was added
        // to close. The negative lookahead is what makes it a DURATION rather
        // than the word.
        @"\bseveral\s+hours\b|\bmost of (?:the|a) day\b|\ban hour\b(?![-\w])"
        + $@"|\b{CountWord}\s*(?:to|-|or)?\s*(?:{CountWord}\s*)?hours\b";

    /// <summary>
    /// Figures that belong to the testing day and can never be true of the exam
    /// in the room. Deliberately does NOT include a singular "an hour": the
    /// exam's sourced ceiling IS "half an hour to an hour" (UPMC), so a rule
    /// banning that would fail the page's own correct sentence — §12.8's
    /// too-wide warning, which round 2 walked into and round 3 caught the
    /// over-correction for.
    /// </summary>
    private const string BeyondTheExamsCeiling =
        @"\bmost of (?:the|a) day\b|\bseveral\s+hours\b"
        + $@"|\b{CountWord}\s*(?:to|-|or)?\s*(?:{CountWord}\s*)?hours\b"
        // ROUND 4 (1): keyed on the word "hours", this missed every figure that
        // passes the ceiling while still counted in minutes — "90 minutes",
        // "ninety minutes", "an hour and a half" all sailed through. The
        // page-level allow-list catches them, but the unit property is what the
        // canary suite claims to prove, so the claim has to be true.
        // ROUND 5 (2)(3): the boundary is SIXTY-ONE minutes, not twenty. The
        // first version rejected "30 minutes to an hour" — UPMC's own sourced
        // ceiling, written numerically — which is the too-wide trap this item
        // already wrote a lesson about, reappearing inside the fix for it. The
        // spelled-out half was also a three-phrase whitelist that missed
        // "eighty minutes" and "an hour and twenty minutes".
        + @"|\ban hour and (?:a half|a quarter|\w+ minutes)\b"
        + @"|\b(?:6[1-9]|[7-9]\d|\d{3,})\s*minutes\b"
        + @"|\b(?:seventy|eighty|ninety)(?:-\w+)?\s*minutes\b";

    /// <summary>
    /// The mirror of <see cref="BeyondTheExamsCeiling"/>. ROUND 4 (2) found
    /// CROSSED fired only for the exam, so the same defect wearing the other
    /// coat passed clean: a span naming the testing and carrying a minutes-only
    /// figure told a reader booked for a testing day to expect five minutes.
    /// The allow-list cannot backstop that one, because "four or five minutes"
    /// is a licensed phrase — licensed for the OTHER subject.
    /// </summary>
    private const string BelowTheTestingsFloor =
        $@"\b{CountWord}\s*(?:to|-|or)?\s*(?:{CountWord}\s*)?minutes\b";

    /// <summary>
    /// THE ANTI-AVERAGING PROPERTY, evaluated over arbitrary text so it can be
    /// proved against text that must FAIL it as well as the page that must pass.
    ///
    /// A duration answer is only honest if it is ATTACHED to the test it belongs
    /// to. Round 1 of review defeated the previous version of this guard with a
    /// single blended paragraph that named both tests, said "an average of the
    /// two is close enough", and carried both "minutes" and "hours" — every
    /// token-presence check passed it. Presence was never the property; PAIRING
    /// is.
    /// </summary>
    /// <summary>
    /// Which test a span of text names. ROUND 2 (B1.3): matching only the full
    /// labels made any blend written with the page's own short forms — "the
    /// testing", "the tests" — invisible to the guard.
    /// </summary>
    private const string ExamRx = @"the exam in the room|\bthe exam\b";

    private const string TestsRx =
        @"the thinking and memory tests|\bthe testing\b|\bthe tests\b";

    /// <summary>
    /// Split into the smallest spans that carry an attribution. Paragraphs and
    /// bullets first; any span naming BOTH tests is re-split into sentences,
    /// because ROUND 2 (S11) showed the bullet-only version failed a correctly
    /// written prose section ("The exam in the room takes minutes. The thinking
    /// and memory tests take two to three hours.") — and §12.8 is explicit that
    /// a rule which fails a correct page is worse than no rule.
    /// </summary>
    private static List<string> AttributionUnits(string rawSection)
    {
        var coarse = Regex.Split(rawSection, @"(?m)^(?=- )|\n\s*\n")
            .Select(u => Regex.Replace(CuratedPage.Flatten(u), @"[*_]", "").Trim())
            .Where(u => u.Length > 0);

        var units = new List<string>();
        foreach (var unit in coarse)
        {
            var namesBoth = Regex.IsMatch(unit, ExamRx, RegexOptions.IgnoreCase)
                            && Regex.IsMatch(unit, TestsRx, RegexOptions.IgnoreCase);

            if (namesBoth)
            {
                units.AddRange(Regex.Split(unit, @"(?<=[.!?])\s+")
                    .Where(s => s.Trim().Length > 0));
            }
            else
            {
                units.Add(unit);
            }
        }

        return units;
    }

    /// <summary>
    /// THE ANTI-AVERAGING PROPERTY. Round 1 defeated the token-presence version;
    /// round 2 defeated the label-pairing version three ways. The property is
    /// now driven off EVERY <see cref="DurationRx"/> match rather than off the
    /// words "minutes" and "hours":
    ///
    ///   * every duration must sit in a span naming exactly ONE test, so a
    ///     duration attached to neither ("Either way, plan on about an hour")
    ///     is caught as UNATTRIBUTED rather than ignored;
    ///   * no span may name both tests and carry a duration (the blend);
    ///   * a span naming only the exam may not carry an hours figure, which is
    ///     round 2's second bypass — "the exam in the room: minutes to two
    ///     hours, much like the testing" previously counted as correct pairing.
    /// </summary>
    private static (bool ExamPaired, bool TestsPaired, List<string> Faults)
        DurationPairing(string rawSection)
    {
        var examPaired = false;
        var testsPaired = false;
        var faults = new List<string>();

        foreach (var unit in AttributionUnits(rawSection))
        {
            var hasExam = Regex.IsMatch(unit, ExamRx, RegexOptions.IgnoreCase);
            var hasTests = Regex.IsMatch(unit, TestsRx, RegexOptions.IgnoreCase);
            var durations = Regex.Matches(unit, DurationRx).Select(m => m.Value).ToList();

            if (durations.Count == 0)
            {
                continue;
            }

            if (hasExam && hasTests)
            {
                faults.Add("BLEND: one span names both tests and gives a duration, so the "
                    + "reader cannot tell which number is theirs: " + unit);
                continue;
            }

            if (!hasExam && !hasTests)
            {
                faults.Add("UNATTRIBUTED: a duration belongs to neither test: " + unit);
                continue;
            }

            // CROSSED, narrowed rather than dropped. Round 2's first attempt
            // banned ANY hours figure from an exam-only span and failed the
            // page's own correct sentence, because a full exam really does run
            // up to an hour. Round 3 proved that DELETING the rule reopened the
            // hole: "- **The exam in the room, in full: most of the day.**"
            // named only the exam, so it was neither a BLEND nor UNATTRIBUTED,
            // and the whole-day figure was allow-listed by PHRASE rather than by
            // attribution. It passed clean.
            //
            // The exam's honest ceiling is an hour, SINGULAR. "half an hour to
            // an hour" carries no plural "hours", no "several" and no "most of
            // the day", so the correct sentence survives; every figure that
            // belongs to the testing day goes red.
            if (hasExam && Regex.IsMatch(unit, BeyondTheExamsCeiling, RegexOptions.IgnoreCase))
            {
                faults.Add("CROSSED: the exam is given a figure that belongs to the testing "
                    + "day: " + unit);
            }
            else if (hasExam && Regex.IsMatch(unit, MinutesFigure, RegexOptions.IgnoreCase))
            {
                examPaired = true;
            }

            if (hasTests)
            {
                if (Regex.IsMatch(unit, BelowTheTestingsFloor, RegexOptions.IgnoreCase)
                    && !Regex.IsMatch(unit, HoursDuration, RegexOptions.IgnoreCase))
                {
                    faults.Add("CROSSED: the testing is given a figure in minutes alone: "
                        + unit);
                }
                else if (Regex.IsMatch(unit, HoursDuration, RegexOptions.IgnoreCase))
                {
                    testsPaired = true;
                }
            }
        }

        return (examPaired, testsPaired, faults);
    }

    /// <summary>
    /// The heart of the ruling. Slot 4 carries a minutes answer attached to the
    /// exam and an hours answer attached to the testing, and no unit of the
    /// section hands a duration to both at once.
    /// </summary>
    [Fact]
    public void EachDurationIsAttachedToTheTestItBelongsToAndNoUnitBlendsThem()
    {
        var (examPaired, testsPaired, faults) = DurationPairing(RawSection(LongHeading));

        var failures = new List<string>(faults);

        // ROUND 3 (S3): the guard ran on slot 4 alone, so the summary was
        // unguarded — and nothing stopped a future edit handing its exam
        // sentence the testing's number, on the 20-28% of the page that
        // actually gets read.
        //
        // ROUND 4 (3) corrected this comment, which had itself gone stale: it
        // claimed slot 8 "prints durations attached to a named test", and it no
        // longer does, because round 3's B3 removed the only one it had. The
        // run over slot 8 is kept as a standing guard rather than a live one,
        // and saying so is the point — a comment that disagrees with its own
        // page is the defect this item wrote a lesson about.
        foreach (var heading in new[] { ShortHeading, AgainHeading })
        {
            var (_, _, otherFaults) = DurationPairing(RawSection(heading));
            failures.AddRange(otherFaults.Select(f => $"in '{heading}': {f}"));
        }

        if (!examPaired)
        {
            failures.Add($"no span pairs the exam with a figure in minutes, so the reader "
                + "booked for a five-minute exam has no answer");
        }

        if (!testsPaired)
        {
            failures.Add("no span pairs the testing with a figure in hours or a day, so the "
                + "reader booked for a testing day has no answer");
        }

        Assert.True(failures.Count == 0,
            "the two durations are no longer separately attributable:\n  "
            + string.Join("\n  ", failures));
    }

    /// <summary>
    /// The canary for the guard above, and it is a real one: the literal text
    /// below is the blended section review round 1 used to defeat the previous
    /// version. Every token-presence check passed it. If this ever stops being
    /// rejected, the guard has been weakened back to the shape that shipped a
    /// page averaging two answers two orders of magnitude apart.
    /// </summary>
    [Fact]
    public void TheAveragingGuardRejectsTheBlendedSectionThatDefeatedItsPredecessor()
    {
        // Each of these defeated a previous version of this guard. They are
        // kept as literal cases because a guard is only as good as the shapes
        // it has been PROVED to reject (§12.8: prove a canary by stubbing the
        // thing it watches and watching it go red).
        (string Name, string Text)[] mustReject =
        [
            // ROUND 1: names both tests, averages them outright.
            ("round 1's blend",
                "The exam in the room and the thinking and memory tests are both booked as "
                + "one appointment, so plan on about an hour and a half for either; an "
                + "average of the two is close enough for most people."),

            // ROUND 2 B1.1: a duration attached to neither label.
            ("a duration attached to neither test",
                "- **The exam in the room: minutes.** About four or five minutes.\n"
                + "- **The thinking and memory tests: two to three hours.** Most of the day.\n"
                + "- **Either way:** plan on about an hour.\n"),

            // ROUND 2 B1.2: one label carrying the other's number.
            ("the exam given the testing's number",
                "- **The exam in the room: minutes to two hours, much like the testing.**\n"),

            // ROUND 2 B1.3: a blend written with the page's own short forms.
            ("a blend written in short forms",
                "The exam and the testing both come to about an hour and a half each."),

            // ROUND 3 B4: the shape that passed clean once CROSSED was removed.
            // Names only the exam, so it is neither BLEND nor UNATTRIBUTED, and
            // "most of the day" was allow-listed by phrase rather than by whose
            // answer it is. It told a reader booked for a bedside exam to clear
            // their day.
            ("the exam given the testing day's figure",
                "- **The exam in the room: four or five minutes.**\n"
                + "- **The exam in the room, in full: most of the day.**\n"
                + "- **The thinking and memory tests: two to three hours.**\n"),

            // ROUND 3 B4, the short-form variant: round 2's comment claimed the
            // "much like the testing" bypass was caught as a BLEND, which was
            // true only of that literal string. Drop the giveaway clause and it
            // escapes unless CROSSED is doing the work.
            ("the exam given hours, with no mention of the testing",
                "- **The exam in the room: two to three hours.**\n"),

            // ROUND 4 (1): past the exam's ceiling but counted in minutes.
            ("the exam given a ninety-minute figure",
                "- **The exam in the room: 90 minutes.**\n"),

            // ROUND 4 (2): the same defect wearing the other coat. The
            // allow-list cannot see this one — "four or five minutes" is a
            // licensed phrase, licensed for the other subject.
            ("the testing given the exam's figure",
                "- **The exam in the room: four or five minutes.**\n"
                + "- **The thinking and memory tests: four or five minutes.**\n"),

            // ROUND 5 (1): the testing-side hatch keyed on the bare word
            // "hour", which matches inside "hour-long", so one extra word
            // rebuilt the defect round 4 had just closed.
            ("the testing given minutes beside an hour-shaped word",
                "- **The thinking and memory tests: 45 minutes, about the same as an "
                + "hour-long visit.**\n"),

        ];

        var escaped = new List<string>();
        foreach (var (name, text) in mustReject)
        {
            var (ex, te, faults) = DurationPairing(text);
            if (faults.Count == 0 && ex && te)
            {
                escaped.Add(name + " passed cleanly");
            }
            else if (faults.Count == 0)
            {
                escaped.Add(name + " raised no fault (it only failed to pair)");
            }
        }

        Assert.True(escaped.Count == 0,
            "the averaging guard no longer rejects shapes that previously defeated it:\n  "
            + string.Join("\n  ", escaped));

        // The mirror, and §12.8's rule that a rule which fails a correct page is
        // worse than no rule: BOTH a bulleted and a PROSE split section must pass.
        (string Name, string Text)[] mustAccept =
        [
            ("bulleted",
                "- **The exam in the room: minutes.** About four or five minutes.\n"
                + "- **The thinking and memory tests: two to three hours.** Most of the day.\n"),
            ("prose",
                "The exam in the room takes about four or five minutes. The thinking and "
                + "memory tests take two to three hours."),

            // ROUND 3: the correct sentence CROSSED must NOT fail. UPMC's
            // sourced ceiling for a full exam is half an hour to an hour, which
            // is the reason the rule is keyed on plural "hours" and not on the
            // word "hour".
            ("the exam's own sourced ceiling",
                "- **The exam in the room: four or five minutes.**\n"
                + "- **The exam in the room, done in full: half an hour to an hour.**\n"
                + "- **The thinking and memory tests: two to three hours.**\n"),

            // ROUND 5 (2): the SAME ceiling written numerically, which is
            // UPMC's literal phrasing. The first version of the minutes rule
            // rejected it — the too-wide trap, inside the fix for the too-wide
            // trap. The boundary is sixty-one minutes, and it is pinned here.
            ("the exam's sourced ceiling written numerically",
                "- **The exam in the room: four or five minutes.**\n"
                + "- **The exam in the room, done in full: 30 minutes to an hour.**\n"
                + "- **The thinking and memory tests: two to three hours.**\n"),
        ];

        var wronglyRejected = new List<string>();
        foreach (var (name, text) in mustAccept)
        {
            var (ex, te, faults) = DurationPairing(text);
            if (!ex || !te || faults.Count > 0)
            {
                wronglyRejected.Add($"{name}: paired={ex}/{te} faults=[{string.Join("; ", faults)}]");
            }
        }

        Assert.True(wronglyRejected.Count == 0,
            "the guard rejects a correctly split section, so its verdict on the real page "
            + "means nothing:\n  " + string.Join("\n  ", wronglyRejected));
    }

    /// <summary>
    /// §12.8 (WI-530/L4247): a dossier is not a source. The research dossier's
    /// "10 to 20 minutes" for the bedside exam is supported by nothing — every
    /// neighbouring claim in its §7 carries a URL and that one does not — and
    /// the live sources disagree with it in both directions.
    /// </summary>
    [Fact]
    public void TheUnsourcedDossierFigureIsAbsentAndTheGuardCanProveItWouldSeeIt()
    {
        string[] banned = ["10 to 20 minutes", "10-20 minutes", "ten to twenty minutes"];

        var found = banned
            .Where(b => Plain.Contains(b, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(found.Count == 0,
            "the dossier's unsourced bedside-exam duration is on the page:\n  "
            + string.Join("\n  ", found));

        // §12.8 (WI-524): a ban whose canary matches no branch cannot fire at
        // all. Prove the matcher works by running it over text that DOES carry
        // the phrase, so "absent" is distinguishable from "never looked".
        const string canary = "the exam takes 10 to 20 minutes";
        Assert.True(banned.Any(b => canary.Contains(b, StringComparison.OrdinalIgnoreCase)),
            "the ban list cannot match even a sentence written to contain it, so its "
            + "clean result over the page proves nothing");
    }

    /// <summary>
    /// Stony Brook's 8-to-12-hour figure is real and is deliberately not
    /// printed: most of the upper end is the clinician scoring and writing
    /// afterwards, which the reader is not present for. §12.8 (WI-520): ask
    /// which way a number pushes if the reader acts on it alone.
    /// </summary>
    [Fact]
    public void TheUpperFigureThatIncludesClinicianTimeIsNotPrintedAsTheReadersDay()
    {
        // ROUND 2 (N2): the literal list missed the hyphen and en-dash forms and
        // carried no reachability canary, unlike the dossier ban above it.
        const string bannedRx =
            @"\b(?:8|eight)\s*(?:to|-|–|—)\s*(?:12|twelve)\b"
            + @"|\b(?:6|six)\s*(?:to|-|–|—)\s*(?:8|eight)\s*hours?\b";

        var found = Regex.Matches(Plain, bannedRx, RegexOptions.IgnoreCase)
            .Select(m => m.Value)
            .ToList();

        Assert.True(found.Count == 0,
            "a figure that counts the clinician's scoring time as the reader's day is on "
            + "the page:\n  " + string.Join("\n  ", found));

        // The canary: prove the pattern can match before trusting that it found
        // nothing (§12.8, WI-524).
        Assert.True(Regex.IsMatch("plan for 8-12 hours", bannedRx, RegexOptions.IgnoreCase)
                    && Regex.IsMatch("about six to eight hours", bannedRx, RegexOptions.IgnoreCase),
            "the ban cannot match text written to contain it, so its clean result over the "
            + "page proves nothing");

        // The page must still tell the reader that some quoted time is not
        // theirs, or dropping the number leaves them to be surprised by it.
        var section = PlainOf(LongHeading);
        Assert.True(Regex.IsMatch(section, @"scoring|written up|report .*(after|afterwards)",
                RegexOptions.IgnoreCase),
            "the page drops the larger figure without ever telling the reader that part of "
            + "the time hospitals quote is work done after they go home");
    }

    /// <summary>
    /// §12.4 R1 permits orienting durations; each one on this page has to come
    /// from a source in its own front matter.
    /// </summary>
    [Fact]
    public void TheOnlyDurationsPublishedAreTheOnesTheSourcesCarry()
    {
        var durations = Regex.Matches(Plain, DurationRx)
            .Select(m => m.Value.Trim())
            .ToList();

        string[] allowed =
        [
            // NANO (PMC5464449): "median and mean times ... were 4 and 5 minutes".
            "four or five minutes",
            // UPMC: "30 minutes to an hour".
            "half an hour to an hour",
            // UCSF Brain Tumor Center: "two to three hours".
            "two to three hours",
            // Cleveland Clinic: "the testing often takes several hours".
            "several hours",
            // UTHealth: "most of the day".
            "most of the day",
            // ROUND 3 (B3): "a year later" used to sit here, justified by a RULE
            // (§12.4 R1) rather than by a source — the one entry in the list
            // that named no publication, inside the test called
            // TheOnlyDurationsPublishedAreTheOnesTheSourcesCarry. §12.4 permits
            // an orienting duration; it does not exempt one from §12.2 item 2.
            // No cited source records a repeat interval, so RULING.md §6's
            // pre-committed fallback was applied and the page publishes the
            // SHAPE ("or later on") instead of a number.
        ];

        // ROUND 2 (S3): the previous version exempted bidirectionally, so
        // `a.Contains(d)` let the entry "half an hour to an hour" exempt a bare
        // "an hour" anywhere on the page, and "two to three hours" exempt a bare
        // "three hours". REDACT the allowed phrases from the text first, then
        // re-match — the §12.8 WI-527 lesson. What survives redaction is a
        // duration the allow-list genuinely does not cover.
        var redacted = allowed.Aggregate(Plain,
            (text, phrase) => Regex.Replace(text, Regex.Escape(phrase), " ",
                RegexOptions.IgnoreCase));

        var unexpected = Regex.Matches(redacted, DurationRx)
            .Select(m => m.Value.Trim())
            .Distinct()
            .ToList();

        Assert.True(unexpected.Count == 0,
            "a duration appeared that no source in this page's front matter carries:\n  "
            + string.Join("\n  ", unexpected));

        // ROUND 1 (S1): four of the seven allowances were UNREACHABLE — the
        // matcher could not produce a string any of them would match, so they
        // were dead entries that made the list look more careful than it was.
        // Every allowance must be reachable BY THIS MATCHER, or it is decoration.
        // ROUND 3 (N5): checking each allowance against DurationRx became a
        // tautology once every entry was, by construction, one of that regex's
        // own alternatives. The property worth asserting is that each allowance
        // EARNS ITS PLACE — it must actually appear on the page. A dead entry
        // exempts nothing and makes the list look more careful than it is.
        var unused = allowed
            .Where(a => !Plain.Contains(a, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(unused.Count == 0,
            "these allowances exempt a duration the page no longer prints, so the list has "
            + "drifted from the prose it licenses:\n  " + string.Join("\n  ", unused));

        // §12.8 (WI-524): assert the match COUNT before trusting the filter, or
        // a regex that sees nothing passes a guard it never ran. DISTINCT, so
        // one phrase repeated three times cannot stand in for two answers.
        Assert.True(durations.Distinct().Count() >= 2,
            $"the duration guard matched {durations.Distinct().Count()} DISTINCT durations, "
            + "but this page's whole argument is that there are two different answers");
    }

    // ------------------------------------------- scored, and nobody fails

    /// <summary>
    /// ORDER, not presence. The page has to concede that the testing is scored
    /// BEFORE it reassures, because a reader who meets the reassurance first and
    /// the scoring second has been handled rather than told.
    /// </summary>
    [Fact]
    public void ThePageSaysTheTestingIsScoredBeforeItSaysNobodyFails()
    {
        var section = PlainOf(JudgedHeading);

        // ROUND 1 BLOCKER: a bare \bscored\b matched the page's FIRST use of the
        // word, which belonged to the OTHER test. Deleting the concession about
        // the thinking and memory tests left this guard green, because the
        // exam's sentence still supplied the token. Anchor to the label, so the
        // concession this guard is named for is the one it actually reads.
        var scored = Regex.Match(section,
            Regex.Escape(TestsLabel) + @"[^.]{0,80}\bscored\b", RegexOptions.IgnoreCase);
        var fails = Regex.Match(section, @"no one fails|nobody fails|makes you a failure",
            RegexOptions.IgnoreCase);

        Assert.True(scored.Success,
            $"the section never concedes that '{TestsLabel}' are scored, which is the one "
            + "thing the reader already suspects:\n" + section);
        Assert.True(fails.Success,
            "the section never gets to the reassurance at all:\n" + section);

        Assert.True(scored.Index < fails.Index,
            "the reassurance arrives before the concession, so the page comforts first and "
            + "admits second. Scored first, then nobody fails.");

        // The exam's own measurement must not be denied. Its cited source
        // (PMC5464449) is a SCORING scale whose score defines response criteria,
        // so "not scored at all" would be an absence claim the page's own
        // citation contradicts. Round 1 caught exactly that sentence.
        Assert.False(Regex.IsMatch(section, @"not scored at all|nobody writes down a mark",
                RegexOptions.IgnoreCase),
            "the page denies that the exam in the room is scored, while citing a scoring "
            + "scale for how long that exam takes");

        // The norms have to be named, or "scored" is a word without a referent
        // and the honest version collapses back into the comforting one.
        Assert.True(Regex.IsMatch(section, @"same age", RegexOptions.IgnoreCase),
            "the section says the testing is scored but never says what it is scored "
            + "against, which is the half that makes it honest");
    }

    /// <summary>
    /// §12.10, and the finding that shaped this page. The corpus already says
    /// the reader cannot fail — three times, and every one is about a SCAN,
    /// where the reader genuinely does nothing. This page's subject is scored.
    /// Borrowing their sentence here would be false in a way theirs are not.
    ///
    /// The guard READS THE SIBLINGS (§12.10), so if one of them ever stops
    /// saying it, this goes red rather than silently guarding nothing.
    /// </summary>
    [Fact]
    public void ThePageDoesNotBorrowTheScanPagesCannotFailSentence()
    {
        var followUp = Sibling("tests", "follow-up-scans.md");
        var planning = Sibling("tests", "planning-scans.md");

        const string followUpOwns = "not a test you can pass or fail";
        const string planningOwns = "You cannot fail this";

        // Read the siblings rather than trusting a comment about them.
        Assert.True(followUp.Contains(followUpOwns, StringComparison.OrdinalIgnoreCase),
            "/tests/follow-up-scans no longer carries the sentence this guard exists to keep "
            + "off this page; re-derive the ruling rather than deleting the guard");
        Assert.True(planning.Contains(planningOwns, StringComparison.OrdinalIgnoreCase),
            "/tests/planning-scans no longer carries the sentence this guard exists to keep "
            + "off this page; re-derive the ruling rather than deleting the guard");

        string[] theirs = [followUpOwns, planningOwns, "a map, not a verdict"];

        var borrowed = theirs
            .Where(t => Plain.Contains(t, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(borrowed.Count == 0,
            "this page has borrowed a sentence that is true of a scan and false of a scored "
            + "test:\n  " + string.Join("\n  ", borrowed));
    }

    /// <summary>
    /// The two comparisons — against people like you, and against your own
    /// earlier level — do not always agree (PMC4356068). Saying only the first
    /// makes the result sound more settled than it is.
    /// </summary>
    [Fact]
    public void BothComparisonsAreNamedAndThePageSaysTheyCanDisagree()
    {
        var section = PlainOf(JudgedHeading);

        Assert.True(Regex.IsMatch(section, @"same age", RegexOptions.IgnoreCase),
            "the comparison against people of the same age is missing");
        Assert.True(Regex.IsMatch(section, @"earlier result|compared with that too",
                RegexOptions.IgnoreCase),
            "the comparison against the reader's own earlier result is missing");
        Assert.True(Regex.IsMatch(section, @"do not always agree|different answers",
                RegexOptions.IgnoreCase),
            "the page names both comparisons and never says they can disagree, which is the "
            + "sourced finding that keeps a single number from reading as a verdict");

        // THE BREAK HARNESS FOUND THIS, and no review round did. The assertion
        // above is an OR over two phrasings, and the page carries BOTH once
        // flattened ("do not always agree", then "gave different answers") — so
        // flipping either one to its opposite left the other standing and the
        // guard green. §12.8: a property with two homes cannot be broken by a
        // single-point mutation.
        //
        // The fix is not a longer OR. It is to forbid the OPPOSITE claim, which
        // is what would actually mislead a reader: a page saying the two
        // comparisons agree tells them a single number settles it.
        // The span has to carry the words BEFORE the verb, or the negation that
        // makes the page's own correct sentence correct sits outside the match
        // and the ban fires on it — which is exactly what happened the first
        // time this was written.
        var agreement = Regex.Matches(section,
                @"[^.]{0,40}\b(?:line up|agree|match up|say the same)\b",
                RegexOptions.IgnoreCase)
            .Select(m => m.Value.Trim())
            .Where(v => !Regex.IsMatch(v, @"\b(?:not|never|rarely|cannot)\b",
                RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(agreement.Count == 0,
            "the section claims the two comparisons agree, which is the opposite of what "
            + "the cited finding says and tells the reader a single number settles it:\n  "
            + string.Join("\n  ", agreement));

        // Both canaries: the ban must fire on the mutation that defeated its
        // predecessor, and must NOT fire on the page's own negated sentence.
        static bool Fires(string text) =>
            Regex.Matches(text, @"[^.]{0,40}\b(?:line up|agree|match up|say the same)\b",
                    RegexOptions.IgnoreCase)
                .Select(m => m.Value.Trim())
                .Any(v => !Regex.IsMatch(v, @"\b(?:not|never|rarely|cannot)\b",
                    RegexOptions.IgnoreCase));

        Assert.True(Fires("The two comparisons line up well."),
            "the agreement ban cannot match the mutation that survived the harness, so its "
            + "clean result proves nothing");
        Assert.False(Fires("The two comparisons do not always agree."),
            "the agreement ban fires on the page's own correct negated sentence, which is "
            + "the too-wide direction §12.8 warns is the worse one");
    }

    /// <summary>
    /// §12.6's recurring frame: "is this the tumor, or the drug?" A reader who
    /// does not know what else moves these scores reads any low one as the
    /// tumor winning.
    /// </summary>
    [Fact]
    public void ThePageNamesWhatElseMovesTheScores()
    {
        var section = PlainOf(JudgedHeading);

        // ROUND 1 (S4): the list used to include "tired" and "the fortnight
        // after an operation" as things that MOVE the scores — a causal claim
        // no cited source makes, plus a British idiom and an unsourced two-week
        // duration. What the sources DO carry is the clinics' own preparation
        // advice and what the testing itself asks about, which delivers the same
        // reassurance without inventing a mechanism.
        string[] confounders = ["sleep", "mood", "medicines"];
        var missing = confounders
            .Where(c => !section.Contains(c, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(missing.Count == 0,
            "the section no longer names what a score is read alongside, so a low result "
            + "reads as the tumor:\n  " + string.Join("\n  ", missing));

        // The claim must stay on the sourced side of the line: these are things
        // the clinics prepare for and ask about, not things this page asserts a
        // causal effect for.
        // ROUND 2 (S10): banning two literal strings policed the wording, not
        // the claim — "affects the scores", "changes what you score" and
        // "moves a result" all walked past it. Ban the SHAPE: any causal verb
        // taking a score-word as its object.
        // ROUND 3 (S7): the comment above used to claim this banned "the claim,
        // not the wording". It does not, and cannot — the page states the same
        // confounder relation with no verb at all ("a score is read next to the
        // week you had"), which is defensible because it reports what clinics
        // prepare for rather than asserting a mechanism. What this guard
        // actually forbids is the CAUSAL VERB form, and the comment now says so.
        const string causalRx =
            @"\b(?:move[sd]?|affect[s]?|change[sd]?|lower[s]?|push(?:es)?)\b[^.]{0,30}"
            + @"\b(?:score|scores|result|results)\b";

        var causal = Regex.Matches(section, causalRx, RegexOptions.IgnoreCase)
            .Select(m => m.Value.Trim())
            .ToList();

        Assert.True(causal.Count == 0,
            "the section asserts in so many words that these things CHANGE the scores, "
            + "which is a causal claim none of the cited sources makes. The sources give "
            + "preparation advice and say what the testing asks about:\n  "
            + string.Join("\n  ", causal));

        Assert.True(Regex.IsMatch("being tired moves these scores", causalRx,
                        RegexOptions.IgnoreCase)
                    && Regex.IsMatch("poor sleep affects your results", causalRx,
                        RegexOptions.IgnoreCase),
            "the causal-shape ban cannot match text written to contain it, so its clean "
            + "result proves nothing");
    }

    // ---------------------------------------------------- what is not claimed

    /// <summary>
    /// hhs.gov returned 403 on two separate attempts, so the employer claim is
    /// NOT a sourced claim (§12.8: a claim whose source could not be read is not
    /// a sourced claim). It may appear only as a question, which asserts
    /// nothing, and only in the questions section.
    /// </summary>
    [Fact]
    public void TheEmployerClaimIsOnlyEverAskedAsAQuestion()
    {
        var questions = PlainOf(QuestionsHeading);

        // The reader's concern is real and must still be raised somewhere.
        Assert.True(Regex.IsMatch(questions, @"outside the hospital|employer",
                RegexOptions.IgnoreCase),
            "the question about who else gets the report has gone, so a real worry is now "
            + "unaddressed rather than deliberately unanswered");

        // Nowhere may the page ASSERT what the blocked source would have carried.
        var bodyWithoutQuestions = Plain.Replace(questions, " ");
        string[] assertions =
        [
            "cannot give your employer",
            "without your authorization",
            "your employer cannot",
            "employers cannot",
            "only with your permission",
        ];

        var asserted = assertions
            .Where(a => bodyWithoutQuestions.Contains(a, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(asserted.Count == 0,
            "the page asserts an employer-access rule whose only candidate source could not "
            + "be read:\n  " + string.Join("\n  ", asserted));
    }

    /// <summary>
    /// PMC7658191 supports that thinking affects work ability. It does NOT say a
    /// neuropsychological report decides anybody's job, so neither does this.
    /// </summary>
    [Fact]
    public void ThePageDoesNotSayTheReportDecidesAnybodysJob()
    {
        var section = PlainOf(ReportHeading);

        // ROUND 2 (N3): the bare "not decide" fallback was satisfied by any
        // unrelated "does not decide" once the job clause was gone. Pin the
        // claim, not a fragment of it.
        Assert.True(Regex.IsMatch(section, @"not decide (anybody's|anyone's|your) job",
                RegexOptions.IgnoreCase),
            "the work bullet no longer says the report does not decide a job, which is the "
            + "clause keeping it inside what PMC7658191 actually supports:\n" + section);

        // And the mirror, round 2 (S8): the page must not rank cognition ahead
        // of the review's other named factors.
        Assert.False(Regex.IsMatch(section, @"most tied|biggest factor|main thing",
                RegexOptions.IgnoreCase),
            "the work bullet ranks thinking above the other factors the review names "
            + "(depressive symptoms, environmental barriers), which it does not do:\n" + section);
    }

    /// <summary>
    /// /seizures/living-with#driving owns driving, the rules are jurisdictional,
    /// and a waiting time from the wrong place is worse than none.
    /// </summary>
    [Fact]
    public void DrivingIsRoutedAndCarriesNoWaitingTime()
    {
        Assert.Contains("/seizures/living-with#driving", Page, StringComparison.Ordinal);

        // Scope to the paragraph, not a character window (§12.8, WI-530).
        var drivingParagraph = CuratedPage.Paragraphs(Page)
            .Concat(Regex.Split(PlainOf(ReportHeading), @"(?<=\.)\s+(?=[A-Z*-])"))
            .Where(p => p.Contains("driv", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(drivingParagraph.Count > 0,
            "nothing on the page mentions driving, so this guard is checking nothing");

        var withDuration = drivingParagraph
            .Where(p => Regex.IsMatch(p, DurationRx))
            .ToList();

        Assert.True(withDuration.Count == 0,
            "a driving paragraph carries a length of time. Driving rules are set where the "
            + "reader lives and a number from anywhere else is worse than none:\n  "
            + string.Join("\n  ", withDuration));
    }

    /// <summary>
    /// §12.8 slot 11 permits links only to pages that exist. There is no
    /// rehabilitation page (WI-554 is unwritten), so the rehab half of "what the
    /// results are used for" is stated in prose with no link.
    ///
    /// Built to go RED the day a rehabilitation page ships, so the door gets
    /// added rather than forgotten — the shape CnsGermCellTumorPageTests uses.
    /// </summary>
    [Fact]
    public void TheRehabHalfIsSaidWithoutALinkBecauseNoRehabPageExistsYet()
    {
        // ROUND 1 nit: detecting by FILENAME alone leaves this green if the page
        // ships as recovery-after-treatment.md. Match the filename OR a page
        // whose own title announces it is the rehabilitation page.
        var pagesRoot = CuratedPage.PagesDirectory;
        var rehabPages = Directory.EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories)
            .Where(f => Regex.IsMatch(Path.GetFileNameWithoutExtension(f),
                           @"rehab|rehabilitation", RegexOptions.IgnoreCase)
                       || Regex.IsMatch(
                           CuratedPage.FrontMatter(File.ReadAllText(f)),
                           @"(?m)^title:.*\brehabilitation\b", RegexOptions.IgnoreCase))
            .Select(f => Path.GetRelativePath(pagesRoot, f))
            .ToList();

        Assert.True(rehabPages.Count == 0,
            "a rehabilitation page now exists (" + string.Join(", ", rehabPages)
            + "): add the door from this page's report section and rewrite this test");

        // The material must still be here, unlinked, or the reader loses the
        // half of the answer that names what the testing actually gets them.
        var section = PlainOf(ReportHeading);
        Assert.True(Regex.IsMatch(section, @"[Tt]herapists", RegexOptions.IgnoreCase),
            "the rehabilitation half of 'what it is used for' has gone. It has no page to "
            + "route to, so it is owed here in prose:\n" + section);

        Assert.DoesNotContain("/treatments/rehabilitation", Page, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// /tumors/pediatric-brain-tumor owns the pediatric cognitive-late-effects
    /// material, including the reframe and the domain list, and deliberately
    /// never uses the word "neuropsychological". This page is the adult slot.
    /// </summary>
    [Fact]
    public void ThePageDoesNotRestateThePediatricCognitiveSection()
    {
        var pediatric = Sibling("tumors", "pediatric-brain-tumor.md");

        const string theirReframe = "the problem is not that skills are lost";
        Assert.True(pediatric.Contains(theirReframe, StringComparison.OrdinalIgnoreCase),
            "/tumors/pediatric-brain-tumor no longer carries its reframe, so this guard is "
            + "protecting a boundary that has moved; re-read both pages");

        string[] theirs =
        [
            theirReframe,
            "picked up more slowly than before",
            "The thing that helps is testing, early",
        ];

        var restated = theirs
            .Where(t => Plain.Contains(t, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(restated.Count == 0,
            "this page restates material /tumors/pediatric-brain-tumor owns:\n  "
            + string.Join("\n  ", restated));
    }

    // ----------------------------------------------------- the shared contract

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("slug: " + Slug, front, StringComparison.Ordinal);
        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Matches(@"(?m)^reviewed: \d{4}-\d{2}-\d{2}\s*$", front);
        Assert.Matches(@"(?m)^review_due: \d{4}-\d{2}-\d{2}\s*$", front);
    }

    [Fact]
    public void EveryCitedUrlCarriesATitleAndAnAccessedDate()
    {
        var front = CuratedPage.FrontMatter(Page);

        var entries = Regex.Matches(front, @"(?m)^  - url: (\S+)\r?\n    title: ""[^""]+""\r?\n    accessed: \d{4}-\d{2}-\d{2}")
            .Count;
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;

        Assert.True(urls >= 10,
            $"only {urls} sources are cited, which is too few for a page carrying two "
            + "subjects and a duration ruling");
        Assert.True(entries == urls,
            $"{urls} urls are cited but only {entries} carry both a title and an accessed "
            + "date immediately beneath them");

        Assert.All(Regex.Matches(front, @"(?m)^  - url: (\S+)").Select(m => m.Groups[1].Value),
            u => Assert.StartsWith("https://", u, StringComparison.Ordinal));
    }

    /// <summary>
    /// The sources that could not be read are recorded in the front matter and
    /// cited nowhere (§12.8: cite it nowhere, record the dead URLs so nobody
    /// re-derives the claim from memory).
    /// </summary>
    [Fact]
    public void TheBlockedSourcesAreRecordedAndCitedNowhere()
    {
        var front = CuratedPage.FrontMatter(Page);
        var cited = string.Join("\n", Regex.Matches(front, @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

        string[] blocked = ["hhs.gov", "PMC11443061", "ninds.nih.gov", "ascopubs.org"];

        var wronglyCited = blocked
            .Where(b => cited.Contains(b, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(wronglyCited.Count == 0,
            "a source that could not be read is cited as though it had been:\n  "
            + string.Join("\n  ", wronglyCited));

        // ROUND 1 (S11): the only positive assertion used to be front.Contains("403"),
        // so DELETING the whole record of what was blocked made this test
        // greener. "Recorded" is half the rule (§12.8: cite it nowhere, record
        // the dead URLs so nobody re-derives the claim from memory) and it was
        // the half that went unchecked.
        var unrecorded = blocked
            .Where(b => !front.Contains(b, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(unrecorded.Count == 0,
            "these sources could not be read and the front matter no longer records why, so "
            + "the next person will re-derive the claim from memory:\n  "
            + string.Join("\n  ", unrecorded));

        Assert.Contains("403", front, StringComparison.Ordinal);
    }

    [Fact]
    public void NoPrognosisNoSharesAndNoPercentages()
    {
        var offenders = new List<string>();

        if (Regex.IsMatch(Plain, @"\d\s*(?:%|percent)", RegexOptions.IgnoreCase))
        {
            offenders.Add("a percentage");
        }

        foreach (var phrase in new[]
                 {
                     "survival", "five-year", "5-year", "life expectancy", "median",
                     "prognosis", "how long you have",
                 })
        {
            if (Plain.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                offenders.Add(phrase);
            }
        }

        Assert.True(offenders.Count == 0,
            "§12.5 forbids these anywhere on any page:\n  " + string.Join("\n  ", offenders));
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        var headings = Headings();
        Assert.Equal(QuestionsHeading, headings[^2]);
        Assert.Equal(NextHeading, headings[^1]);
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchor()
    {
        // §12.8 (WI-509): Markdig derives ids from heading TEXT, and two
        // siblings deep-link into this page by name.
        var without = Regex.Matches(Page, @"(?m)^#{2,3}\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .Where(h => !h.EndsWith('}'))
            .ToList();

        Assert.True(without.Count == 0,
            "these headings have no explicit {#anchor}:\n  " + string.Join("\n  ", without));
    }

    [Fact]
    public void NoBritishFormsOrCharacterisationsOrMinimisations()
    {
        var reader = CuratedPage.ReaderText(Page);

        var british = CuratedPage.BritishForms
            .Where(f => Regex.IsMatch(reader, $@"\b{Regex.Escape(f)}", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(british.Count == 0,
            "British forms on the page:\n  " + string.Join("\n  ", british));

        var characterisations = CuratedPage.Characterisations
            .Where(c => reader.Contains(c, StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.True(characterisations.Count == 0,
            "a result is characterised rather than described:\n  "
            + string.Join("\n  ", characterisations));

        CuratedPage.AssertNeverMinimises(reader, Slug);
    }

    [Fact]
    public void ThePageCarriesNoEscalationListBecauseNeitherTestHasOne() =>
        CuratedPage.AssertNoEscalationList(Page, Slug);

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn() =>
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);
}

[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class NeuroExamAndMemoryTestingPageRenderTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public NeuroExamAndMemoryTestingPageRenderTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:BrainHarbor",
                TestDatabase.ConnectionString));
    }

    private const string Url = "/tests/neuro-exam-and-memory-testing";

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);
        Assert.Contains("thinking and memory tests", html, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The orphan guard. /tests has no index, so a library page nothing links to
    /// is half shipped (§12.8, WI-519).
    /// </summary>
    [Fact]
    public async Task ThePageIsReachableFromWhereANewlyDiagnosedReaderStarts()
    {
        var html = await _factory.CreateClient().GetStringAsync("/start");
        Assert.Contains(Url, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The helper collects onward links WITHOUT fragments, so the driving
        // route is required by its own guard (DrivingIsRoutedAndCarriesNoWaitingTime)
        // rather than here, where it would never be found.
        await CuratedPage.AssertLinksResolve(_factory.CreateClient(), Url,
            "/tests/mri", "/tests/follow-up-scans");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    /// <summary>
    /// The glossary entry is added by this item and is NOT suppressed here.
    /// §12.8 (WI-526): suppressing a term a sibling lets fire makes one word
    /// behave two ways. Assert it fires on BOTH pages, so the entry is reachable
    /// and the decision not to suppress is pinned.
    /// </summary>
    [Fact]
    public async Task TheNeuropsychologistTooltipFiresHereAndOnTheSiblingThatAlsoSaysIt()
    {
        var client = _factory.CreateClient();

        var mine = await client.GetStringAsync(Url);
        Assert.Contains("def-neuropsychologist", mine, StringComparison.Ordinal);

        // ROUND 5 (11): presence alone is equally satisfied if the tooltip
        // attaches to the ALIAS "neuropsychological testing", which would hand
        // the reader a panel defining a PERSON when they clicked a procedure.
        // GlossaryMarker takes the earliest match across a term and its aliases,
        // so the page names the person first on purpose — and that ordering is
        // one reword away from regressing silently unless it is pinned here.
        Assert.Contains(">neuropsychologist</button>", mine, StringComparison.Ordinal);

        var sibling = await client.GetStringAsync("/tests/planning-scans");
        Assert.Contains("def-neuropsychologist", sibling, StringComparison.Ordinal);
    }

    /// <summary>
    /// §12.8 (WI-519): a door added to a sibling is PROSE on that sibling, so it
    /// must be an APPENDED sentence rather than a rewrite of one already there,
    /// or that page's own assertions break.
    /// </summary>
    [Fact]
    public async Task TheDoorsOnTheSiblingPagesAreAppendedRatherThanReplacements()
    {
        var client = _factory.CreateClient();

        // ROUND 1 (S12): /start was missing from this table entirely — the one
        // sibling the orphan guard depends on. And presence was asserted where
        // POSITION is the property: a door MOVED into another section satisfied
        // "the sentence survives" and "the url appears somewhere". The door has
        // to sit AFTER the sentence it was appended to.
        (string Page, string SentenceThatMustSurvive)[] doors =
        [
            ("/start",
                "covers the appointments, what to ask"),
            ("/treatments/radiation-therapy",
                "there is something to compare against later"),
            ("/tumors/low-grade-glioma",
                "You do not have to prove it first"),
            ("/treatments/watch-and-wait",
                "They ask how you have been and examine you"),
        ];

        var broken = new List<string>();
        foreach (var (page, sentence) in doors)
        {
            var html = await client.GetStringAsync(page);

            var sentenceAt = html.IndexOf(sentence, StringComparison.OrdinalIgnoreCase);
            if (sentenceAt < 0)
            {
                broken.Add($"{page}: the door replaced '{sentence}' instead of following it");
                continue;
            }

            var doorAt = html.IndexOf(Url, StringComparison.Ordinal);
            if (doorAt < 0)
            {
                broken.Add($"{page}: the door to this page has gone");
                continue;
            }

            if (doorAt < sentenceAt)
            {
                broken.Add($"{page}: the door now sits ABOVE '{sentence}', so it is no longer "
                    + "an appended sentence and the page's own assertions may have moved");
            }
        }

        Assert.True(broken.Count == 0, string.Join("\n", broken));
    }
}
