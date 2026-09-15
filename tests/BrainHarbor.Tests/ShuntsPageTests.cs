using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-534: <c>/treatments/shunts</c>. A §12.8 library page whose highest-value
/// section is an ESCALATION TIER the corpus did not have: for a reader with a
/// shunt, NINDS says "seek medical help immediately" for symptoms the shared
/// [ESCALATION] block files as same-day. This page owns that tier; the block,
/// /treatments/craniotomy and [MECHANISM] route to it with the same instruction.
///
/// Pinned is where this page could leave a reader worse off than no page:
///
///   * UNDER-TRIAGING A FAILING SHUNT, on this page or on any surface that routes
///     here. /review (round 1) found the first draft named three signs in the
///     block, so a shunt reader with new confusion was still same-day.
///   * DROPPING A SIGN. The whole list is pinned by exact text.
///   * A POINTER TO A LIST THAT DOES NOT EXIST. The first draft sent the reader to
///     /get-help-now for "the ambulance list"; that page has none.
///   * CALLING A WARNING SIGN NORMAL RECOVERY.
///   * A FAILURE RATE, or a single source's number stated as general.
///   * PICKING ONE SIDE of two disagreements the sources print.
///
/// Every guard was rewritten against /review's worked counter-examples, which
/// are kept as canaries. Paragraphs and bullets are SLICED, specific entries are
/// asserted rather than floors, removals are asserted before a scan trusts them,
/// counts come before iteration, and <c>\r?\n</c> is used everywhere.
/// </summary>
public sealed class ShuntsPageContentTests
{
    private const string Slug = "treatments/shunts";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is it?";
    private const string WhyHeading = "Why would I need one?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string GoodHeading = "Is it for good?";
    private const string SignsHeading = "Signs a shunt is not working";
    private const string RisksHeading = "Other risks of the operation";
    private const string LivingHeading = "Living with a shunt";
    private const string NeedHeading = "What you need first";
    private const string CaregiverHeading = "For the person alongside them";
    private const string AnotherHeading = "Why do I need another operation?";
    private const string DecidesHeading = "Who decides, and how will I hear?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    /// <summary>The instruction, identical on every surface that carries it (§12.10).</summary>
    private const string Instruction =
        "Get help right away, at any hour: call your team, or go to the emergency department.";

    private static string Page => CuratedPage.Read("treatments", "shunts.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?[ \t]*\r?$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    private static string Clean(string raw) =>
        Regex.Replace(CuratedPage.Flatten(Reader(raw)), @"[*_]", "").Trim();

    private static List<string> ParagraphsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n[ \t]*\r?\n").Select(Clean).Where(p => p.Length > 0)];

    private static List<string> BulletsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n(?=- |\d+\. )")
            .Where(b => Regex.IsMatch(b.TrimStart(), @"^(?:- |\d+\. )"))
            .Select(b => Regex.Replace(Clean(Regex.Split(b, @"\r?\n[ \t]*\r?\n")[0]), @"^(?:-|\d+\.)\s+", ""))];

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

    private static string Plain => Regex.Replace(Headline + " " + Body, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    private static List<string> BlockParagraphs(string name) =>
        ParagraphsOf(CuratedPage.Body(CuratedPage.Read(Path.Combine("..", "blocks", name))));

    /// <summary>Removes an exact phrase and PROVES it was there (§12.8, WI-530).</summary>
    private static string Without(string text, string phrase, int expected = 1)
    {
        var count = Regex.Matches(text, Regex.Escape(phrase)).Count;
        Assert.True(count == expected,
            $"expected '{phrase}' {expected} time(s) and found it {count} — the exemption no longer "
            + "matches the page, so the scan after it is not scanning what it claims");
        return text.Replace(phrase, " ", StringComparison.Ordinal);
    }

    /// <summary>
    /// Downgrade shapes, shared by every surface that carries the instruction.
    /// Rewritten after /review beat the first version with "within two days",
    /// "tomorrow", "when the clinic opens" and the morning exemption.
    /// </summary>
    private static readonly Regex Downgrade = new(
        @"(?i)\bsame[- ]day\b|(?i)\bnext (?:appointment|visit|clinic)\b|(?i)\bmorning\b|(?i)\btomorrow\b"
        + @"|(?i)\bopens\b|(?i)\boffice hours\b|(?i)\bcan wait\b|(?i)\bwait and see\b|(?i)\bwait until\b"
        + @"|(?i)\bwithin (?:a|an|one|two|three|\d+) (?:days?|hours?)\b|(?i)\bmiddle of the night\b"
        + @"|(?i)\bif it (?:does not|doesn't) (?:settle|go away)\b|(?i)\bnothing to worry\b");

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, StepsHeading, LongHeading, FeelHeading,
            GoodHeading, SignsHeading, RisksHeading, LivingHeading, NeedHeading,
            CaregiverHeading,
            AnotherHeading, DecidesHeading, QuestionsHeading, NextHeading,
        ];

        foreach (var heading in required)
        {
            Assert.True(headings.Contains(heading),
                $"the page has lost the '{heading}' section entirely:\n  " + string.Join("\n  ", headings));
        }

        var positions = required.Select(h => headings.IndexOf(h)).ToList();
        for (var i = 1; i < positions.Count; i++)
        {
            Assert.True(positions[i] > positions[i - 1], $"'{required[i]}' has moved above '{required[i - 1]}'.");
        }
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorAndThePublishedOnesAreStable()
    {
        var headings = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$").Select(m => m.Groups[1].Value.Trim()).ToList();
        Assert.True(headings.Count >= 16, $"only {headings.Count} '## ' headings were found");

        var missing = headings.Where(h => !Regex.IsMatch(h, @"\{#[\w-]+\}$")).ToList();
        Assert.True(missing.Count == 0, "these headings have no explicit anchor:\n  " + string.Join("\n  ", missing));

        // A PUBLISHED INTERFACE: the escalation block, /treatments/craniotomy and
        // /tests/mri deep-link these two.
        Assert.Contains("## Signs a shunt is not working {#warning-signs}", Page, StringComparison.Ordinal);
        Assert.Contains("## Living with a shunt {#living-with-it}", Page, StringComparison.Ordinal);
    }

    // ------------------------------------------- the escalation tier this page owns

    [Fact]
    public void TheWarningListOpensWithTheInstructionAndCarriesEverySignByExactText()
    {
        var paragraphs = ParagraphsOf(RawSection(SignsHeading));
        Assert.Equal(
            "If you have a shunt and any of these happen, get help right away, at any hour: call your team, "
            + "or go to the emergency department.",
            paragraphs[0]);

        // EXACT TEXT, in order (/review: first-word pins let "A headache, but
        // only one that keeps getting worse" through).
        string[] expected =
        [
            "A headache, especially a new one or one that is getting worse.",
            "Nausea or throwing up.",
            "Getting sleepier, or being hard to wake or to keep awake.",
            "Confusion, or trouble thinking or concentrating.",
            "Blurred or double vision, or light hurting your eyes.",
            "Losing your balance, or being clumsier than usual.",
            "A sore neck or sore shoulders.",
            "A seizure.",
            "Changes in mood or behavior, or seeming withdrawn.",
            "Redness, soreness or swelling along the line of the tube, or fluid leaking from a cut.",
            "A fever.",
            "Pain in your belly.",
            "The problems you had before the shunt coming back.",
            "In a baby: a bulging soft spot on the top of the head, a head that is growing fast, eyes fixed "
            + "downward so the white shows above the colored part, or being unusually fussy or sleepy.",
        ];

        Assert.Equal(expected, BulletsOf(RawSection(SignsHeading)));
    }

    [Fact]
    public void TheWarningSignsAreNeverDowngradedAndEndOnGettingHelp()
    {
        var section = PlainOf(SignsHeading);

        Assert.Matches(Downgrade, "Call your team the same day for any of these.");
        Assert.Matches(Downgrade, "If it lasts, call within two days.");
        Assert.Matches(Downgrade, "Call in the morning, after lying down for an hour.");
        Assert.Matches(Downgrade, "Ring when the clinic opens.");

        // The one CORRECT use of "morning" is the underdraining sentence, removed
        // exactly and proved present before the scan (/review round 1).
        var scanned = Without(section,
            "One that is worst in the morning or after lying down can mean it is draining too little.");
        var hits = Downgrade.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0, "the warning section downgrades the tier: " + string.Join(" | ", hits));

        Assert.Contains("If you are not sure, go.", section, StringComparison.Ordinal);
        Assert.Equal("Either kind still means getting help right away.", CuratedPage.SentencesOf(section)[^1]);
    }

    [Fact]
    public void TheAmbulanceCasesAreStatedHereAndMatchTheCraniotomyList()
    {
        // /review round 1: the first draft pointed at /get-help-now for "the
        // ambulance list", and that page has none. The cases are stated in the
        // paragraph, and each is checked against /treatments/craniotomy's own
        // ambulance list, READ rather than re-typed (§12.10).
        var paragraph = ParagraphsOf(RawSection(SignsHeading))
            .Single(p => p.StartsWith("Some things are an ambulance call, shunt or not.", StringComparison.Ordinal));

        (string Here, string There)[] cases =
        [
            ("cannot be woken", "cannot wake"),
            ("not breathing properly", "not breathing properly"),
            ("first seizure", "their first"),
            ("more than five minutes", "more than five minutes"),
            ("suddenly cannot speak, move, or see", "cannot speak, move, or see"),
        ];

        var craniotomy = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        var ambulance = Regex.Match(craniotomy, @"When to call an ambulance\.\*\*(.*?)(?=\*\*[A-Z]|\z)",
            RegexOptions.Singleline);
        Assert.True(ambulance.Success, "/treatments/craniotomy no longer has its ambulance list");

        foreach (var (here, there) in cases)
        {
            Assert.Contains(here, paragraph, StringComparison.Ordinal);
            Assert.Contains(there, ambulance.Groups[1].Value, StringComparison.Ordinal);
        }

        Assert.Contains("911", paragraph, StringComparison.Ordinal);
        Assert.Contains("(/seizures/what-to-do)", paragraph, StringComparison.Ordinal);

        // And the false pointer does not come back.
        Assert.DoesNotMatch(@"(?i)get help now[^.]{0,40}\blist\b|(?i)\bthe ambulance list\b", Plain);
    }

    [Fact]
    public void TheBlocksShuntParagraphCarriesTheWholeSameDayListAndTheInstruction()
    {
        // Isolated to ITS paragraph (/review: a downgrade sentence added to it
        // passed every guard in round 1).
        var paragraph = BlockParagraphs("escalation.md")
            .Single(p => p.StartsWith("If you have a shunt, the signs it has stopped working are their own rule.",
                StringComparison.Ordinal));

        Assert.Contains("everything on the same-day list above is a right-away call instead", paragraph,
            StringComparison.Ordinal);
        Assert.Contains(Instruction, paragraph, StringComparison.Ordinal);
        Assert.Contains("(/treatments/shunts#warning-signs)", paragraph, StringComparison.Ordinal);

        var scanned = Without(paragraph, "everything on the same-day list above is a right-away call instead");
        Assert.Matches(Downgrade, "If it is the middle of the night, it can wait until your team opens.");
        Assert.DoesNotMatch(Downgrade, scanned);

        // After the same-day list AND after both fever rules.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.EscalationBlock));
        var order = new[]
        {
            "Call your team the same day for any of these", "If you are having chemotherapy",
            "In the weeks after brain surgery", "If you have a shunt",
        }.Select(s => block.IndexOf(s, StringComparison.Ordinal)).ToList();
        Assert.True(order.All(i => i >= 0) && order.Zip(order.Skip(1)).All(p => p.Second > p.First),
            "the shunt conditional has moved above the same-day list or a fever rule");
    }

    [Fact]
    public void TheCraniotomyCaregiverListsRouteShuntReadersHereAtTheSameStrength()
    {
        // /review round 1: that page's same-day list (headache, confusion, much
        // more sleep) contradicted the tier for its likeliest shunt reader.
        var craniotomyRaw = CuratedPage.Read("treatments", "craniotomy.md");
        var paragraph = ParagraphsOf(CuratedPage.Body(craniotomyRaw))
            .Single(p => p.StartsWith("If they have a shunt, the same-day list is a right-away list.",
                StringComparison.Ordinal));
        Assert.Contains(Instruction, paragraph, StringComparison.Ordinal);
        Assert.Contains("(/treatments/shunts#warning-signs)", paragraph, StringComparison.Ordinal);
        Assert.DoesNotMatch(Downgrade, Without(paragraph, "the same-day list is a right-away list"));

        var flat = CuratedPage.Flatten(craniotomyRaw);
        Assert.True(
            flat.IndexOf("When to call an ambulance", StringComparison.Ordinal)
            < flat.IndexOf("If they have a shunt", StringComparison.Ordinal),
            "the craniotomy shunt line has moved above the lists it modifies");
    }

    [Fact]
    public void TheMechanismBlockRoutesHereAndItsRaisedPressureLineHasTheShuntException()
    {
        var paragraphs = BlockParagraphs("mechanism.md");

        var fluid = paragraphs.Single(p => p.StartsWith("It can block the flow of fluid.", StringComparison.Ordinal));
        Assert.Contains("the usual treatment is a shunt", fluid, StringComparison.Ordinal);
        Assert.Contains("(/treatments/shunts)", fluid, StringComparison.Ordinal);

        // "worth a phone call rather than a wait" is a third strength for a shunt
        // reader on eight hubs (/review round 1). END-anchored.
        var pressure = paragraphs.Single(p => p.StartsWith("Pressure can build up inside the head.",
            StringComparison.Ordinal));
        Assert.Matches(@"If you have a shunt, it means getting help right away\.$", pressure);

        Assert.DoesNotContain("thin tube that carries the fluid to another part of the body", Plain,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RecoveryIsToldApartFromTheWarningSigns()
    {
        // /review round 1: the feel section called tiredness, headaches and a sore
        // belly normal while the list said right away. The section now ends on
        // the difference.
        var last = ParagraphsOf(RawSection(FeelHeading))[^1];
        Assert.StartsWith("Normal recovery gets slowly better.", last, StringComparison.Ordinal);
        Assert.Contains("is not recovery", last, StringComparison.Ordinal);
        Assert.Contains("It is a warning sign", last, StringComparison.Ordinal);

        var normalising = new Regex(
            @"(?i)\bheadaches? (?:after the operation )?(?:vary|are normal|are expected|are common)\b"
            + @"|(?i)\b(?:tiredness|drowsiness|sleepiness|belly pain) (?:is|are) normal\b"
            + @"|(?i)\bmostly like nothing\b");
        Assert.Matches(normalising, "Headaches after the operation vary.");
        Assert.DoesNotMatch(normalising, Plain);
    }

    // ------------------------------------------------------------- the numbers

    [Fact]
    public void NoFailureRevisionInfectionOrSurvivalFigureReachesThePage()
    {
        const string CountWord =
            @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|twenty|thirty|forty|fifty|hundred)";

        var guard = new Regex(
            @"(?i)\d+(?:\.\d+)?\s*(?:%|per ?cent)|(?i)\bmedian\b|(?i)\bsurvival\b|(?i)\blife expectancy\b"
            + @"|(?i)\b(?:failure|success|revision|infection|complication) rates?\b"
            + @"|(?i)\b" + CountWord + @"[\s-]+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b"
            + @"|(?i)\b(?:a|one)[\s-]+(?:third|quarter|fifth)\b|(?i)\b(?:half|most|many) (?:of )?(?:all )?shunts\b"
            + @"|(?i)\b(?:two|three)[- ](?:thirds|quarters)\b");

        Assert.Matches(guard, "Shunt failure was 27.8%.");
        Assert.Matches(guard, "About one in three shunts fails.");
        Assert.Matches(guard, "Most shunts fail within ten years.");
        Assert.DoesNotMatch(guard,
            "the shunts that stopped working in their first three months all did so within about five weeks");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0, "a figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void EveryDurationShapedNumberSitsInASentenceThatNamesItsSource()
    {
        // /review round 1 beat a fixed list with "At about six weeks you can go
        // back to work." — the Walton Centre's UK practice the front matter
        // excludes. Every duration-shaped number is found by SHAPE, counted, and
        // must share its sentence with a named source.
        var duration = new Regex(
            @"(?i)\b(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|fourteen|twenty|a|an)"
            + @"(?:\s+(?:to|or)\s+(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|fourteen|an?))?"
            + @"\s+(?:hours?|days?|weeks?|months?|years?)\b");

        string[] sources =
        [
            "American Cancer Society", "MedlinePlus", "UK neurosurgery center", "study in Norway", "FDA",
            "Hydrocephalus Association",
        ];

        var sentences = CuratedPage.SentencesOf(Plain)
            .Where(s => duration.IsMatch(s))
            // `SentencesOf` over flattened text glues a heading's anchor onto the
            // sentence after it, so the summary line is matched by its END.
            .Where(s => !s.EndsWith("The operation takes about an hour to an hour and a half.", StringComparison.Ordinal))
            .ToList();

        Assert.True(sentences.Count >= 5, $"only {sentences.Count} duration sentences found; the shape has stopped matching");

        var unattributed = sentences.Where(s => !sources.Any(src => s.Contains(src, StringComparison.Ordinal))).ToList();
        Assert.True(unattributed.Count == 0,
            "a duration with no named source in its sentence:\n  " + string.Join("\n  ", unattributed));

        // The Norway timing is measured from the shunt going in (PMC8827213).
        Assert.Contains("within about five weeks of going in", Plain, StringComparison.Ordinal);

        // The two operation times are printed side by side, each attributed.
        Assert.Contains("The American Cancer Society says about an hour, and MedlinePlus says about an hour and a half.",
            Plain, StringComparison.Ordinal);

        // The excluded UK practice, by concept.
        Assert.DoesNotMatch(@"(?i)\b(?:back|return) to work\b|(?i)\bstitches\b|(?i)\bclips\b|(?i)\bup to a week\b", Plain);
    }

    // ----------------------------------------------- the disagreement and the scope

    [Fact]
    public void IsItForGoodPrintsBothAnswersAndNoOtherSentenceTakesASide()
    {
        var paragraphs = ParagraphsOf(RawSection(GoodHeading));
        var both = paragraphs.Single(p => p.StartsWith(
            "The American Cancer Society says shunts can be temporary or permanent.", StringComparison.Ordinal));
        Assert.Contains("The Hydrocephalus Association, writing about hydrocephalus in general", both,
            StringComparison.Ordinal);
        Assert.Contains("the rest of your life", both, StringComparison.Ordinal);

        // By CONCEPT, with every exemption removed and proved present (/review:
        // the lookbehind version let "...runs support groups, and a shunt is for
        // life" through, and could not see the page's own short version).
        var concept = new Regex(
            @"(?i)\bfor (?:life|good)\b|(?i)\bpermanent(?:ly)?\b|(?i)\brest of (?:your|their|his|her) life\b"
            + @"|(?i)\balways need\b|(?i)\bstays? in for good\b|(?i)\bforever\b");
        Assert.Matches(concept, "The Hydrocephalus Association runs support groups, and a shunt is for life.");

        var scanned = Without(Plain, both);
        scanned = Without(scanned, "for a while or for good", 2);
        scanned = Without(scanned, "Is it for good?");
        Assert.DoesNotMatch(concept, scanned);

        Assert.Equal("Keep the warning signs somewhere you will see them during that time.",
            CuratedPage.SentencesOf(PlainOf(GoodHeading))[^1]);
    }

    [Fact]
    public void ThePageDoesNotSayAShuntIsTheOnlyTreatment()
    {
        var paragraph = ParagraphsOf(RawSection(WhyHeading))
            .Single(p => p.StartsWith("A shunt is not the only answer.", StringComparison.Ordinal));
        Assert.Contains("taking the tumor out can often help, and that there are other ways", paragraph,
            StringComparison.Ordinal);
        Assert.Contains("endoscopic third ventriculostomy", paragraph, StringComparison.Ordinal);
        Assert.Contains("a limited number of people", paragraph, StringComparison.Ordinal);

        var only = new Regex(@"(?i)\bthe only (?:treatment|answer|option|way)\b|(?i)\balways (?:needs?|means) a shunt\b"
                             + @"|(?i)\bthe (?:best|standard) treatment\b|(?i)\bby itself\b");
        Assert.Matches(only, "Taking the tumor out can often help by itself.");
        Assert.DoesNotMatch(only, Without(Plain, "A shunt is not the only answer."));
    }

    [Fact]
    public void EachPracticalPointLivesInOneSection()
    {
        // /review round 1 counted about fifteen self-restatements. Each point is
        // pinned to the section that owns it (the questions list is exempt).
        (string Pattern, string Owner)[] points =
        [
            (@"(?i)\bmake and model\b", LivingHeading),
            (@"(?i)\bafter-hours number\b", NeedHeading),
            (@"(?i)\b10 to 14 days\b", AnotherHeading),
            (@"(?i)\bone to three months\b", CaregiverHeading),
            (@"(?i)\btwo inches\b", LivingHeading),
            (@"(?i)\bflying\b", LivingHeading),
            (@"(?i)\bknown part of having a shunt\b|(?i)\bon the list\b", "(nowhere)"),
            (@"(?i)\brepair or replace\b", GoodHeading),
            (@"(?i)\bthin, soft tube\b", WhatHeading),
        ];

        var sections = Headings().Where(h => h != QuestionsHeading).Select(h => (Heading: h, Text: PlainOf(h))).ToList();

        foreach (var (pattern, owner) in points)
        {
            var holders = sections.Where(s => Regex.IsMatch(s.Text, pattern)).Select(s => s.Heading).ToList();
            if (owner == "(nowhere)")
            {
                Assert.True(holders.Count == 0, $"'{pattern}' is back, in [{string.Join(", ", holders)}]");
                continue;
            }

            Assert.True(holders.Count == 1 && holders[0] == owner,
                $"'{pattern}' is in [{string.Join(", ", holders)}] rather than only in '{owner}'");
        }
    }

    [Fact]
    public void ThePageDoesNotTalkAboutItselfAndEveryHardSectionEndsOnAnAction()
    {
        var meta = new Regex(
            @"(?i)\bthe sources say\b|(?i)\bsays so plainly\b|(?i)\bshort enough to find\b|(?i)\bwhich is why the name\b"
            + @"|(?i)\bThat is one reason\b|(?i)\bthis page\b(?! (?:has|is))");
        Assert.Matches(meta, "Not always, and the sources say it two ways.");
        Assert.DoesNotMatch(meta, Plain.Replace("the list is further down this page", " ", StringComparison.Ordinal));

        Assert.Equal("Ask your surgeon how likely each one is for you.", CuratedPage.SentencesOf(PlainOf(RisksHeading))[^1]);
        Assert.Equal("Ask your team what the plan would be if yours ever needed changing.",
            CuratedPage.SentencesOf(PlainOf(AnotherHeading))[^1]);

        // The unsourced reassurance /review flagged does not come back.
        Assert.DoesNotMatch(@"(?i)\bnot a sign (?:that )?you did something wrong\b|(?i)\bnothing sticks out\b", Plain);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatAnotherPageAlreadyOwns()
    {
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug,
            // The instruction, identical in blocks/escalation.md and on
            // /treatments/craniotomy by the §12.10 decision in the front matter.
            "get help right away at any hour call your team or go to the emergency department",
            // The ambulance threshold, in the corpus's own wording (already on
            // /treatments/radiation-therapy): one wording for an emergency
            // threshold, rather than a second one written to dodge this check.
            "has a first seizure or one that lasts more than five minutes or suddenly cannot speak move or see");
    }

    [Fact]
    public void TheMriPageRoutesHereAndEveryShuntReaderIsToldToSaySo()
    {
        var paragraph = ParagraphsOf(CuratedPage.Body(CuratedPage.Read("tests", "mri.md")))
            .Single(p => p.Contains("exact make and model", StringComparison.Ordinal));
        Assert.Contains("Without it they may have to be cautious for no reason.", paragraph, StringComparison.Ordinal);
        Assert.Contains("/treatments/shunts#living-with-it", paragraph, StringComparison.Ordinal);

        // /review round 1: advice addressed only to adjustable valves left a
        // fixed-valve reader thinking there was nothing to mention.
        var living = ParagraphsOf(RawSection(LivingHeading))[0];
        Assert.StartsWith("Tell anyone doing an MRI that you have a shunt, and which kind.", living, StringComparison.Ordinal);
        Assert.Contains("so ask whether yours needs checking afterwards", living, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMagnetAdviceKeepsTheFdasOwnLimitsInBothDirections()
    {
        var paragraph = ParagraphsOf(RawSection(LivingHeading)).Single(p => p.Contains("two inches", StringComparison.Ordinal));
        Assert.Contains("an adjustable valve", paragraph, StringComparison.Ordinal);
        Assert.Contains("using the ear on the other side", paragraph, StringComparison.Ordinal);
        Assert.Contains("It does not tell people to stop using them.", paragraph, StringComparison.Ordinal);

        var overreach = new Regex(@"(?i)\b(?:avoid|stop using|do not use|never use|keep away from) (?:phones|headphones|earbuds|tablets|magnets|electronics)\b");
        Assert.Matches(overreach, "Avoid phones and headphones.");
        Assert.DoesNotMatch(overreach, Plain);
    }

    // --------------------------------------------------------- source discipline

    [Fact]
    public void TheDossiersContentSiteThePdqAndTheDroppedBackgroundSourceAreNotCited()
    {
        var urls = CitedUrls();
        Assert.True(urls.Split('\n').Length >= 14, "fewer than fourteen source URLs were read out of the front matter");

        foreach (var banned in new[]
                 {
                     "healthline.com", "NBK66023", "PMC6257011", "PMC8976775", "academic.oup.com", "mdpi.com",
                     "mayoclinic.org", "hopkinsmedicine.org", "researchgate.net", "medlineplus.gov/druginfo",
                 })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);
        Assert.Contains("ninds.nih.gov/health-information/disorders/hydrocephalus", urls, StringComparison.Ordinal);
        Assert.Contains("hydroassoc.org/complications-of-shunt-systems", urls, StringComparison.Ordinal);
        Assert.Contains("cdc.gov/epilepsy/first-aid-for-seizures", urls, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        Assert.True(urls >= 14, $"only {urls} sources are cited");
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count);
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count);
    }

    // ------------------------------------------------------- the caregiver half

    [Fact]
    public void TheCaregiverSectionComposesTheBlockAndAddsOnlyWhatIsTrueHere()
    {
        Assert.Matches(@"(?m)^\[CAREGIVER\]\s*$", Page);

        var section = PlainOf(CaregiverHeading);
        Assert.Contains("Learn the warning signs as well as the person with the shunt does.", section, StringComparison.Ordinal);
        Assert.Contains("With a baby or young child, go by the change you see.", section, StringComparison.Ordinal);

        var claims = Regex.Matches(RawSection(CaregiverHeading), @"(?m)^\*\*").Count;
        Assert.True(claims == 4, $"the caregiver section makes {claims} claims rather than four.");
    }

    [Fact]
    public void TheCaregiverSectionDoesNotRepeatTheBlocksOwnSentences()
    {
        // Word shingles against the block, READ from caregiver.md rather than a
        // hand-typed phrase list (/review round 1).
        var blockWords = Regex.Matches(
                CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(Path.Combine("..", "blocks", "caregiver.md"))))
                .ToLowerInvariant(), @"[a-z0-9']+")
            .Select(m => m.Value).ToList();
        Assert.True(blockWords.Count > 200, "the block text could not be read, so this guard proves nothing");

        var blockShingles = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i + 6 <= blockWords.Count; i++)
        {
            blockShingles.Add(string.Join(' ', blockWords.Skip(i).Take(6)));
        }

        var mine = Regex.Matches(PlainOf(CaregiverHeading).ToLowerInvariant(), @"[a-z0-9']+").Select(m => m.Value).ToList();
        var shared = new List<string>();
        for (var i = 0; i + 6 <= mine.Count; i++)
        {
            var shingle = string.Join(' ', mine.Skip(i).Take(6));
            if (blockShingles.Contains(shingle))
            {
                shared.Add(shingle);
            }
        }

        Assert.True(shared.Count == 0, "the page's caregiver half restates [CAREGIVER]:\n  " + string.Join("\n  ", shared.Take(4)));
    }

    // ----------------------------------------------------------- the glossary

    [Fact]
    public void TheHydrocephalusEntryIsSuppressedHereAndKept()
    {
        var markers = Regex.Matches(CuratedPage.Body(Page), @"!%(.+?)%").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(["hydrocephalus"], markers);
        Assert.Matches(@"(?i)\bhydrocephalus\b", Body);
        Assert.Empty(Regex.Matches(Page, @"(?m)^.*!%.*\[[A-Z-]+\].*$"));

        var entry = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "..", "glossary", "hydrocephalus.md"));
        Assert.Matches(@"(?m)^term: hydrocephalus\s*$", entry);
    }

    // -------------------------------------------------- corpus-wide housekeeping

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        var offenders = CuratedPage.BritishForms
            .Where(form => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(form)}"))
            .Where(form => !CuratedPage.BritishFormExemptions.Any(
                ex => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(ex)}")
                      && ex.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(offenders.Count == 0, "British forms on a page written for a US reader: " + string.Join(", ", offenders));

        Assert.DoesNotMatch(
            @"(?i)\bfeeling sick\b|(?i)\bbeing sick\b|(?i)\bthe ward\b|(?i)\bA&E\b|(?i)\bout[- ]of[- ]hours\b"
            + @"|(?i)\bstraight away\b|(?i)\bGP\b|(?i)\bconsultant\b|(?i)\bcaught out\b|(?i)\bgetting past\b"
            + @"|(?i)\bsport\b|(?i)\bwhilst\b",
            Plain);
    }

    [Fact]
    public void ThePageNeverCharacterisesAnOutcomeOrMinimisesTheTreatment()
    {
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertNeverMinimises(Plain, Slug);
        Assert.DoesNotMatch(@"(?i)\byour insurance\b", Plain);
    }
}

/// <summary>
/// WI-534: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class ShuntsPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/shunts";

    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// The connection string is pushed in (WI-531: a render class that does not
    /// passes on a developer machine and fails in CI).
    /// </summary>
    public ShuntsPageRenderTests(WebApplicationFactory<Program> raw) =>
        factory = raw.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAndCarriesNoUnresolvedDirectiveOrMarker()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync(Url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.Contains("id=\"warning-signs\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/treatments/craniotomy",
            "/tests/mri",
            "/tests/ct-scan",
            "/tests/follow-up-scans",
            "/seizures/what-to-do",
            "/get-help-now");
    }

    [Fact]
    public async Task TheBlocksDeepLinksLandOnAnchorsThatExist()
    {
        // The escalation block (on every including hub), /treatments/craniotomy
        // and /tests/mri deep-link this page. `AssertLinksResolve` stops at the
        // `#` (§12.8, WI-513), so the fragments are checked on the linking pages.
        var client = factory.CreateClient();
        await CuratedPage.AssertFragmentLinksResolve(client, "/tumors/glioma");
        await CuratedPage.AssertFragmentLinksResolve(client, "/treatments/craniotomy");
        await CuratedPage.AssertFragmentLinksResolve(client, "/tests/mri");

        var glioma = await client.GetStringAsync("/tumors/glioma");
        Assert.Contains("/treatments/shunts#warning-signs", glioma, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheGlossaryTooltipIsSuppressedHereAndStillFiresElsewhere()
    {
        var client = factory.CreateClient();
        var html = await client.GetStringAsync(Url);
        Assert.DoesNotContain("def-hydrocephalus", html, StringComparison.Ordinal);

        var ct = await client.GetStringAsync("/tests/ct-scan");
        Assert.Contains("def-hydrocephalus", ct, StringComparison.Ordinal);
    }
}
