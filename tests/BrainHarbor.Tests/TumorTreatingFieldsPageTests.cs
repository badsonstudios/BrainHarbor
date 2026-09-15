using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-533: <c>/treatments/tumor-treating-fields</c>. A §12.8 library page whose
/// clinical half was ALREADY SHIPPED, twice, before it was written:
/// <c>/tumors/glioblastoma</c> carries the device, the 18 hours, the shaved
/// head and when it starts, and <c>/tumors/high-grade-glioma</c> carries the
/// European guideline's disagreement. So the page's own ground is what living
/// with the device asks, and what is pinned here is where it could leave a
/// reader worse off than no page:
///
///   * STATING THE 18 HOURS AT A SECOND STRENGTH. §12.10 was decided before
///     drafting: the hub's exact phrase and no other. /review found the first
///     draft had two anyway ("runs day and night", "never comes off").
///   * SELLING THE DEVICE TO THE WRONG READER. ACS: it may help people live
///     longer alongside temozolomide; at recurrence it has NOT been shown to.
///     And the recurrence reader is not on temozolomide at all.
///   * DROPPING THE DISAGREEMENT. EANO calls it controversial; US guidelines
///     list it as standard. Both, in the hub's own words.
///   * PRINTING THE MANUFACTURER'S CONTRAINDICATION LIST.
///   * TURNING AN ASSOCIATION INTO A CAUSE, or a single study's share into a
///     general fact.
///
/// Every guard starts from §12.8's accumulated lessons, and the second version
/// of each was rewritten against /review's worked counter-examples, which are
/// kept below as canaries: paragraphs are SLICED, conventions are pinned
/// case-sensitively, specific claims are listed rather than floored, counts
/// come before iteration, and every regex touching a line break uses
/// <c>\r?\n</c>.
/// </summary>
public sealed class TumorTreatingFieldsPageContentTests
{
    private const string Slug = "treatments/tumor-treating-fields";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is it?";
    private const string WhyHeading = "Why would I be offered it?";
    private const string AgreeHeading = "Do doctors agree about it?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string EverydayHeading = "Carrying it, sleeping, and washing";
    private const string HeadHeading = "The shaved head, and other people";
    private const string SideHeading = "Your scalp, and other side effects";
    private const string NeedHeading = "What you need first";
    private const string CaregiverHeading = "For the person alongside them";
    private const string GrowsHeading = "If the tumor grows while you are wearing it";
    private const string DecidesHeading = "Who decides whether to keep going?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    /// <summary>The glioblastoma hub's phrase, shared verbatim (§12.10).</summary>
    private const string Hours = "at least eighteen hours a day";

    /// <summary>The high-grade-glioma hub's sentence, shared verbatim (§12.10).</summary>
    private const string Eano =
        "European guideline calls its role controversial and says it is not widely available there.";

    private static string Page => CuratedPage.Read("treatments", "tumor-treating-fields.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// flattens, so anything reading a PARAGRAPH or a BULLET cuts the markdown
    /// itself (§12.8, WI-526 and WI-528).
    /// </summary>
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

    /// <summary>Paragraphs, sliced on blank lines (§12.8, WI-532).</summary>
    private static List<string> ParagraphsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n[ \t]*\r?\n").Select(Clean).Where(p => p.Length > 0)];

    /// <summary>Bullets, each cut at the first blank line after it.</summary>
    private static List<string> BulletsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n(?=- )")
            .Where(b => b.TrimStart().StartsWith("- ", StringComparison.Ordinal))
            .Select(b => Regex.Replace(Clean(Regex.Split(b, @"\r?\n[ \t]*\r?\n")[0]), @"^-\s+", ""))];

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

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    /// <summary>A count of two or more: "one of the people" is an enumeration.</summary>
    private const string ManyWord =
        @"(?:\d+(?:\.\d+)?|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|dozen)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?|fraction)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): presence first, then position. The order is the
        // STANDARD's (§12.8, WI-510, WI-531): slot 7, then the caregiver
        // section, then 8, then 9.
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, AgreeHeading, StepsHeading, LongHeading,
            FeelHeading, EverydayHeading, HeadHeading, SideHeading, NeedHeading,
            CaregiverHeading,
            GrowsHeading, DecidesHeading, QuestionsHeading, NextHeading,
        ];

        foreach (var heading in required)
        {
            Assert.True(headings.Contains(heading),
                $"the page has lost the '{heading}' section entirely:\n  "
                + string.Join("\n  ", headings));
        }

        var positions = required.Select(h => headings.IndexOf(h)).ToList();
        for (var i = 1; i < positions.Count; i++)
        {
            Assert.True(positions[i] > positions[i - 1],
                $"'{required[i]}' has moved above '{required[i - 1]}'.");
        }
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorSoRewordingAHeadingCannotBreakAnInboundLink()
    {
        // §12.8 (WI-508, WI-509).
        var headings = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.True(headings.Count >= 16,
            $"only {headings.Count} '## ' headings were found, so this guard is checking almost "
            + "nothing. The heading regex has stopped matching the page.");

        var missing = headings.Where(h => !Regex.IsMatch(h, @"\{#[\w-]+\}$")).ToList();
        Assert.True(missing.Count == 0,
            "these headings have no explicit anchor:\n  " + string.Join("\n  ", missing));
    }

    // ------------------------------------------ §12.10: one claim, one strength

    /// <summary>
    /// Every way this page has found, or /review has found, to state the hours
    /// at a strength other than the hub's. Shared by the page check and the hub
    /// check so the two cannot drift.
    /// </summary>
    private static readonly Regex OtherStrength = new(
        @"(?i)\b" + CountWord + @"(?:[\s-]+(?:to|or)[\s-]+" + CountWord + @")?"
        + @"(?:\s*\+|[\s-]+plus|\s+or\s+more)?\s*(?:-\s*)?(?:hours?|hrs?|h)\b"
        + @"|(?i)\bmost of (?:the|each|every|your) (?:day|time)\b"
        + @"|(?i)\b(?:nearly|almost) (?:all|every) (?:day|hour|the time)\b"
        + @"|(?i)\baround the clock\b|(?i)\ball day and all night\b|(?i)\bday and night\b"
        + @"|(?i)\b24\s*/\s*7\b|(?i)\bnever (?:comes? off|off|taken off)\b|(?i)\bhardly ever off\b"
        + @"|(?i)\ball the time\b|(?i)\bnon-?stop\b|(?i)\bconstantly\b"
        // The second rendered read found "The cap and the bag are always there".
        + @"|(?i)\balways (?:on|there|worn)\b"
        + @"|(?i)\b(?:three[- ]quarters|" + Fraction + @")\s+of\s+(?:the|each|every|your|it)\b"
        + @"|(?i)\d+(?:\.\d+)?\s*(?:%|per ?cent)");

    /// <summary>
    /// A qualifier hung on the shared phrase is a second strength too: "at least
    /// eighteen hours a day on most days". The phrase is replaced by a TOKEN
    /// rather than a space, so what surrounds it is still scanned.
    /// </summary>
    private static readonly Regex HungQualifier = new(
        @"HOURSPHRASE(?:\W+\w+){0,5}?\W+(?:on most days|most days|if you can|when you can|"
        + @"as many as|where possible|ideally|or so|or less|or fewer|give or take|roughly|"
        + @"usually|often|sometimes)\b"
        + @"|(?i)\b(?:ideally|usually|often|try to|aim for|about|around|roughly|nearly|almost|"
        + @"up to|close to)\W+(?:\w+\W+){0,3}HOURSPHRASE");

    private static string Tokenised(string text) =>
        Regex.Replace(text, Regex.Escape(Hours), "HOURSPHRASE", RegexOptions.IgnoreCase);

    [Fact]
    public void TheEighteenHoursUseTheGlioblastomaHubsExactWordingAndNoOtherStrength()
    {
        // THE ITEM'S FIRST DECISION, pinned by READING the hub (§12.8, WI-525).
        var hub = Sibling("tumors", "glioblastoma.md");
        Assert.Contains(Hours, hub, StringComparison.Ordinal);

        // Where this page needs it, compared case-INSENSITIVELY per section: the
        // duration slot OPENS with "At least", and a guard that only passed
        // because of a lowercase repeat further down would fail the correct edit
        // that removes the repeat (/review).
        Assert.Contains(Hours, PlainOf(ShortHeading), StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("At least eighteen hours a day", Clean(RawSection(LongHeading)),
            StringComparison.Ordinal);
        Assert.Contains(Hours, Headline, StringComparison.OrdinalIgnoreCase);

        // Canaries, including every sentence /review used to beat version one.
        Assert.Matches(OtherStrength, "The pads are worn for most of the day.");
        Assert.Matches(OtherStrength, "Wear it about 18 hours.");
        Assert.Matches(OtherStrength, "It is worn three quarters of the day.");
        Assert.Matches(OtherStrength, "It means eighteen-hour days.");
        Assert.Matches(OtherStrength, "Each battery lasts two to three hours.");
        Assert.Matches(OtherStrength, "Clinicians say 18-plus hours a day.");
        Assert.Matches(OtherStrength, "Wear it 18+ hours.");
        Assert.Matches(OtherStrength, "Wear it eighteen or more hours.");
        Assert.Matches(OtherStrength, "It is on 24/7.");
        Assert.Matches(OtherStrength, "From then on it runs day and night.");
        Assert.Matches(OtherStrength, "It is like wearing something that never comes off.");
        Assert.Matches(OtherStrength, "Wear it for about three-quarters of it.");
        Assert.DoesNotMatch(OtherStrength, "Each battery lasts a few hours, so you carry spares.");
        Assert.DoesNotMatch(OtherStrength, "a change took about half an hour for a typical person");
        Assert.DoesNotMatch(OtherStrength, "Hair stops the pads from sticking, so all of it has to come off.");

        Assert.Matches(HungQualifier, Tokenised("Wear it at least eighteen hours a day on most days."));
        Assert.Matches(HungQualifier,
            Tokenised("Wear it at least eighteen hours a day, or as many as you can manage."));
        Assert.Matches(HungQualifier, Tokenised("Aim for at least eighteen hours a day."));
        Assert.DoesNotMatch(HungQualifier,
            Tokenised("At least eighteen hours a day, every day, for as long as you and your team keep going"));

        var scanned = Tokenised(Plain);
        var hits = OtherStrength.Matches(scanned).Select(m => m.Value)
            .Concat(HungQualifier.Matches(scanned).Select(m => m.Value)).ToList();
        Assert.True(hits.Count == 0,
            "the page states the hours at a second strength, which §12.10 forbids: "
            + string.Join(" | ", hits));

        // The hub's whole DEVICE SUBSECTION, sliced by its ### heading rather
        // than by the one paragraph that happens to hold the phrase (/review:
        // "worn most of the day" added to the first device paragraph stayed
        // green). The hub may say "most of the day" about tiredness elsewhere.
        var hubRaw = CuratedPage.Body(CuratedPage.Read("tumors", "glioblastoma.md"));
        var sub = Regex.Match(hubRaw,
            @"(?ms)^### Tumor treating fields, the device you wear[ \t]*\r?$(.*?)(?=^#{2,3} |\z)");
        Assert.True(sub.Success, "/tumors/glioblastoma has lost its device subsection");
        var hubScanned = Tokenised(Clean(sub.Groups[1].Value));
        Assert.Contains("HOURSPHRASE", hubScanned, StringComparison.Ordinal);
        var hubHits = OtherStrength.Matches(hubScanned).Select(m => m.Value)
            .Concat(HungQualifier.Matches(hubScanned).Select(m => m.Value)).ToList();
        Assert.True(hubHits.Count == 0,
            "/tumors/glioblastoma's device subsection now states the hours a second way: "
            + string.Join(" | ", hubHits));
    }

    [Fact]
    public void TheEuropeanDisagreementIsTheHighGradeGliomaHubsSentenceAndSitsBesideTheUsView()
    {
        var hub = Sibling("tumors", "high-grade-glioma.md");
        Assert.Contains(Eano, hub, StringComparison.Ordinal);

        // Both sides in ONE paragraph, and the European sentence FIRST, so that
        // "there" follows "European" rather than "the United States" (/review).
        var paragraph = ParagraphsOf(RawSection(AgreeHeading))
            .Single(p => p.Contains(Eano, StringComparison.Ordinal));
        Assert.StartsWith("The " + Eano, paragraph, StringComparison.Ordinal);
        Assert.Contains("In the United States, the main cancer guidelines list it as a standard option",
            paragraph, StringComparison.Ordinal);

        // The doubts, each attributed to who raised it.
        var section = PlainOf(AgreeHeading);
        Assert.Contains("Reviewers in England and Canada raised concerns about the main trial", section,
            StringComparison.Ordinal);
        Assert.Contains("people knew whether they had the device", section, StringComparison.Ordinal);
        Assert.Contains("a chosen group", section, StringComparison.Ordinal);
        Assert.Contains("whether it is worth what it costs", section, StringComparison.Ordinal);

        // The Chicago doctors: ALL called it a good option, SOME said it may not
        // suit everyone. /review caught the first draft flattening "some".
        Assert.Contains("all the doctors asked called it a good option, and some", section,
            StringComparison.Ordinal);

        // §12.6: ends on the reader's own question.
        Assert.Contains("what makes them", CuratedPage.SentencesOf(section)[^1], StringComparison.Ordinal);

        var settled = new Regex(
            @"(?i)\bproven\b|(?i)\bbreakthrough\b|(?i)\bgame[- ]changer\b|(?i)\brevolutionary\b"
            + @"|(?i)\bmiracle\b|(?i)\b(?:all|most|every) (?:doctors|experts|guidelines?) (?:agree|recommends?)\b"
            + @"|(?i)\bthe evidence is (?:clear|settled|strong)\b|(?i)\bno longer controversial\b");

        Assert.Matches(settled, "It is a proven treatment.");
        Assert.Matches(settled, "Most experts recommend it now.");
        Assert.DoesNotMatch(settled, Plain);
    }

    [Fact]
    public void TheTwoClaimsAreKeptInTheBulletsForTheTwoReadersTheyBelongTo()
    {
        // ACS, in its own voice, for two readers. The unit is the BULLET.
        var bullets = BulletsOf(RawSection(WhyHeading));
        Assert.True(bullets.Count == 2, $"the section has {bullets.Count} bullets rather than two");

        var first = bullets.Single(b => b.Contains("After surgery and radiation", StringComparison.Ordinal));
        var recur = bullets.Single(b => b.Contains("If glioblastoma comes back", StringComparison.Ordinal));

        // Each claim stays ACS's, rather than becoming the site's (/review).
        Assert.Contains("The American Cancer Society says", first, StringComparison.Ordinal);
        Assert.Contains("The American Cancer Society says", recur, StringComparison.Ordinal);

        Assert.Contains("added to temozolomide, not used instead of it", first, StringComparison.Ordinal);
        Assert.Contains("may help people live longer", first, StringComparison.Ordinal);

        Assert.Contains("has not been shown to help people live longer than chemotherapy", recur,
            StringComparison.Ordinal);
        Assert.Contains("much milder", recur, StringComparison.Ordinal);
        Assert.DoesNotContain("may help", recur, StringComparison.Ordinal);

        // Nothing added to the recurrence bullet that ACS did not say.
        var added = new Regex(
            @"(?i)\bbeen shown to (?!help people live longer than chemotherapy)"
            + @"|(?i)\bslows?\b|(?i)\bshrinks?\b|(?i)\bcontrols?\b|(?i)\bworks?\b");
        Assert.Matches(added, "but it has been shown to slow the tumor");
        Assert.DoesNotMatch(added, recur);
    }

    [Fact]
    public void TemozolomideIsOnlyEverAddressedToTheReaderWhoIsTakingIt()
    {
        // /review's second blocker. At recurrence the device is used INSTEAD of
        // chemotherapy, so that reader is not on temozolomide. Outside the
        // newly-diagnosed bullet, every mention that addresses the reader has to
        // carry the condition.
        var whyBullet = BulletsOf(RawSection(WhyHeading))
            .Single(b => b.Contains("After surgery and radiation", StringComparison.Ordinal));

        var scanned = Plain.Replace(whyBullet, " ", StringComparison.Ordinal);
        var addressing = new Regex(
            @"(?i)\b(?:you|your)\b[^.]{0,60}\btemozolomide\b|(?i)\btemozolomide\b[^.]{0,40}\b(?:you|your)\b");

        var sentences = Regex.Split(scanned, @"(?<=[.!?])\s+")
            .Where(s => addressing.IsMatch(s))
            .ToList();

        Assert.True(sentences.Count >= 2,
            $"only {sentences.Count} sentences address the reader about temozolomide, so this guard "
            + "is checking almost nothing");

        var unconditional = sentences
            .Where(s => !Regex.IsMatch(s, @"(?i)\bif you (?:are also taking|take)\b"))
            .ToList();
        Assert.True(unconditional.Count == 0,
            "the page tells every reader they are on temozolomide:\n  "
            + string.Join("\n  ", unconditional));
    }

    [Fact]
    public void EveryLivingLongerClaimOnThePageCarriesItsHedge()
    {
        // Count first (§12.8, WI-524). Inflections included (/review: "People who
        // wore it lived longer" walked past `lives?`).
        var longer = new Regex(@"(?i)\b(?:live[sd]?|living|survive[sd]?|surviving)\s+(?:a\s+)?longer\b");
        Assert.Matches(longer, "People who wore it lived longer.");
        Assert.Matches(longer, "People living longer is the goal.");
        Assert.Matches(longer, "Patients survive longer with it.");

        var matches = longer.Matches(Plain).ToList();
        Assert.True(matches.Count == 2,
            $"the page says 'live longer' {matches.Count} times rather than twice; a new one needs "
            + "its hedge checked, and a lost one means a claim went missing");

        foreach (var match in matches)
        {
            var before = Plain[Math.Max(0, match.Index - 40)..match.Index];
            Assert.True(
                Regex.IsMatch(before, @"(?:\bmay|\bnot been shown to)\s+help\s+(?:people\s+)?$"),
                $"'live longer' appears without 'may' or 'not been shown to' in front of it: …{before}");
        }

        var hype = new Regex(
            @"(?i)\b(?:extends?|prolongs?|lengthens?)\s+(?:your\s+|people's\s+)?(?:life|lives|survival)\b"
            + @"|(?i)\badds?\s+(?:\w+\s+){0,2}(?:months|years|time)\b"
            + @"|(?i)\b(?:helps?\s+(?:you|people)\s+survive|longer life)\b"
            + @"|(?i)\bworks? better\b");

        Assert.Matches(hype, "It extends life.");
        Assert.Matches(hype, "It adds precious months.");
        Assert.Matches(hype, "It helps you live a longer life.");
        Assert.Matches(hype, "More hours on means it works better.");
        Assert.DoesNotMatch(hype, Plain);
    }

    [Fact]
    public void TheHoursAreGivenTheirReasonWithCadthsHedgeAndNeverAsACause()
    {
        // CADTH: "believed to be dose-dependent". PMC7399624's authors consult
        // for the manufacturer and its usage-survival sentence is an association.
        var reason = BulletsOf(RawSection(LongHeading))
            .Single(b => b.StartsWith("The hours are the treatment", StringComparison.Ordinal));

        Assert.Contains("only expected to act while", reason, StringComparison.Ordinal);
        Assert.Contains(Hours, reason, StringComparison.Ordinal);
        Assert.Contains("thought to depend on the hours", reason, StringComparison.Ordinal);

        var causal = new Regex(
            @"(?i)\bthe more\b(?:\W+\w+){0,8}\W+\bthe (?:longer|better|more)\b"
            + @"|(?i)\b(?:hours?|wear(?:ing|s)?|wore)\b(?:\W+\w+){0,8}\W+"
            + @"\b(?:(?:live[sd]?|living) longer|survive[sd]?|survival|better results?|did better|works? better)\b"
            + @"|(?i)(?<!\b(?:thought|believed|said) to )\bdepends? on (?:the |those )?hours\b");

        Assert.Matches(causal, "The more you wear it, the longer you have.");
        Assert.Matches(causal, "Wearing it for more hours helps you survive.");
        Assert.Matches(causal, "The benefit depends on the hours.");
        Assert.Matches(causal, "People who wore it lived longer.");
        Assert.DoesNotMatch(causal, "How much it helps is thought to depend on the hours.");
        Assert.DoesNotMatch(causal, "The benefit is believed to depend on the hours.");

        var hits = causal.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page turns the hours into a cause of living longer: " + string.Join(" | ", hits));
    }

    // ------------------------------------------------------------- the numbers

    [Fact]
    public void NoSurvivalProgressionShareOrPriceFigureReachesThePage()
    {
        // §12.4, §12.5, and a figure written as a word is still a figure (§12.8,
        // WI-511). Rewritten after /review beat it with the page's own sources:
        // "Two-thirds said they would choose it again" is PMC6797850's 67%, and
        // "Seven out of ten people found the alarms disturbing" its 70%.
        var guard = new Regex(
            @"(?i)\bhazard ratios?\b|(?i)\bmedian\b|(?i)\bprogression[- ]free\b"
            + @"|(?i)\boverall survival\b|(?i)\bsurvival rates?\b|(?i)\blife expectancy\b"
            + @"|(?i)\bfive[- ]year\b|(?i)\d+(?:\.\d+)?\s*(?:%|per ?cent)|(?i)\bon average\b"
            + @"|(?i)\$\s*\d|(?i)\bdollars?\b|(?i)\bper month\b|(?i)\bcost-effective"
            + @"|(?i)\b(?:" + CountWord + @"|a|several)\s+(?:\w+\s+){0,2}(?:months?|years?)\b"
            + @"(?:\W+\w+){0,4}\W+\b(?:longer|more|extra|added)\b"
            + @"|(?i)\b(?:live|living|lived|lives)\s+(?:for\s+)?(?:about\s+|around\s+|roughly\s+)?"
            + CountWord + @"\b"
            + @"|(?i)\b(?:" + ManyWord + @"|" + Fraction + @")\b(?:\W+\w+){0,3}\W+"
            + @"\b(?:of|in|among)\s+(?:the\s+)?(?:people|patients)\b"
            + @"|(?i)\b(?:" + CountWord + @"|" + Fraction + @")[\s-]+(?:out of|in)\s+" + CountWord + @"\b"
            + @"|(?i)\b(?:two|three|four)[- ](?:thirds|quarters|fifths)\b"
            + @"|(?i)\b(?:" + ManyWord + @")\s+(?:of|in)\s+(?:every\s+)?" + CountWord + @"\b");

        Assert.Matches(guard, "Median survival was 20.9 months.");
        Assert.Matches(guard, "It costs $27,000 a month.");
        Assert.Matches(guard, "People lived about five months longer.");
        Assert.Matches(guard, "Half of the patients had skin problems.");
        Assert.Matches(guard, "It extended progression-free survival.");
        Assert.Matches(guard, "Two-thirds said they would choose it again.");
        Assert.Matches(guard, "Seven out of ten people found the alarms disturbing.");
        Assert.Matches(guard, "On average people lived about 21 months.");

        Assert.DoesNotMatch(guard,
            "In one small study in Germany, a change took about half an hour for a typical person, "
            + "and much longer for some.");
        Assert.DoesNotMatch(guard, "How many months it goes on is decided with your doctor.");
        Assert.DoesNotMatch(guard, "One person had not told people at work what was going on.");
        Assert.DoesNotMatch(guard, "Ask one of the people on your team.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0, "a figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoTemperatureBatteryWeightOrMeasurementReachesThePage()
    {
        // 41 °C, "2 to 3 hours" per battery, "under three pounds", a ~2 cm shift.
        var guard = new Regex(
            @"°|(?i)\b\d+(?:\.\d+)?\s*(?:C|F|kg|g|lbs?|oz)\b|(?i)\b(?:\d+|" + CountWord + @")\s+degrees?\b"
            + @"|(?i)\bdegrees? (?:C|F|Celsius|Fahrenheit)\b|(?i)\bCelsius\b|(?i)\bFahrenheit\b"
            + @"|(?i)\b(?:pounds?|kilos?|kilograms?|ounces?)\b"
            + @"|(?i)\b(?:cm|mm|centimet(?:er|re)s?|millimet(?:er|re)s?|inch(?:es)?)\b"
            + @"|(?i)\bkHz\b|(?i)\bhertz\b|(?i)\bV/cm\b");

        Assert.Matches(guard, "The alarm sounds above 41 °C.");
        Assert.Matches(guard, "The alarm sounds above 41 C.");
        Assert.Matches(guard, "It weighs 1.2 kg.");
        Assert.Matches(guard, "It weighs under three pounds.");
        Assert.Matches(guard, "Shift the pads about 2 cm.");
        Assert.DoesNotMatch(guard, "It helps to some degree.");
        Assert.DoesNotMatch(guard, Plain);
    }

    [Fact]
    public void EachStudyClaimNamesItsOwnStudyInItsParagraph()
    {
        // §12.8 (WI-507), rewritten after /review beat the floor version: the
        // claims are LISTED, each with the ONE study it belongs to, so swapping
        // "German" for "Chicago" goes red; and any NEW share-shaped claim with no
        // listing fails as unlisted.
        (string Claim, string Study)[] claims =
        [
            ("a change took about half an hour", "one small study in Germany"),
            ("most people said the alarms bothered them", "German study"),
            ("got back pain from carrying it on one shoulder", "German study"),
            ("most people said it got in the way of work or hobbies", "German study"),
            ("Most said they would choose it again", "German study"),
            ("about half said the shaved head bothered them", "German study"),
            ("all the doctors asked called it a good option", "study in Chicago"),
            ("people named the heat on their head", "Chicago study"),
            ("the reason people who said no gave most often", "Chicago study"),
            ("one person said shaving was easy", "Chicago study"),
            ("said others might think it silly", "Chicago study"),
        ];

        var paragraphs = ParagraphsOf(CuratedPage.Body(Page))
            .SelectMany(p => p.StartsWith("- ", StringComparison.Ordinal)
                ? Regex.Split(p, @"(?:^|\s)- (?=[A-Z])").Where(s => s.Length > 0)
                : [p])
            .ToList();

        foreach (var (claim, study) in claims)
        {
            var holder = paragraphs.Where(p => p.Contains(claim, StringComparison.Ordinal)).ToList();
            Assert.True(holder.Count == 1, $"'{claim}' is in {holder.Count} paragraphs rather than one");
            Assert.Contains(study, holder[0], StringComparison.Ordinal);

            var other = study.Contains("German", StringComparison.Ordinal)
                        || study.Contains("Germany", StringComparison.Ordinal)
                ? "Chicago"
                : "German";
            Assert.DoesNotContain(other, holder[0], StringComparison.Ordinal);
        }

        // Any share-shaped claim that is not one of the listed ones.
        var shareShaped = new Regex(
            @"(?i)\b(?:most|the majority|about half|half|two[- ]thirds|many|few|some)\s+(?:of\s+them\s+|people\s+)?"
            + @"(?:said|found|named|reported|felt|rated|got)\b");
        var unlisted = paragraphs
            .Where(p => shareShaped.IsMatch(p))
            .Where(p => !claims.Any(c => p.Contains(c.Claim, StringComparison.Ordinal)))
            .ToList();
        Assert.True(unlisted.Count == 0,
            "a study-shaped claim that no listing covers:\n  " + string.Join("\n  ", unlisted));

        // The half hour is printed once, page-wide, so no later section can
        // restate it without its attribution (§12.8, WI-532: grep the whole page).
        Assert.Equal(1, Regex.Matches(Plain, @"(?i)\bhalf an hour\b").Count);

        // "In that study" must come AFTER the bullet that names the study.
        var raw = RawSection(LongHeading);
        var named = raw.IndexOf("one small study in Germany", StringComparison.Ordinal);
        var that = raw.IndexOf("In that study", StringComparison.Ordinal);
        Assert.True(named >= 0 && that > named,
            "the 'In that study' bullet no longer follows the bullet that names the study");
        var weeks = BulletsOf(raw).Single(b => b.Contains("In that study", StringComparison.Ordinal));
        Assert.Contains("about two weeks", weeks, StringComparison.Ordinal);
    }

    // --------------------------------------------------------- source discipline

    [Fact]
    public void NoManufacturerMarketingOrDeadSourceIsCited()
    {
        var urls = CitedUrls();
        Assert.True(urls.Split('\n').Length >= 9,
            "fewer than nine source URLs were read out of the front matter, so this ban is "
            + "checking almost nothing");

        foreach (var banned in new[]
                 {
                     "optune.com", "optunegio.com", "novocure.com", "virtualtrials.org",
                     "memorialcare.org", "academic.oup.com", "mdpi.com", "ascopubs.org",
                     "journals.lww.com", "mayoclinic.org", "hopkinsmedicine.org",
                     "sciencedirect.com", "medscape.com", "researchgate.net",
                 })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("brain-spinal-cord-tumors-adults/treating/alternating-electric-field-therapy.html",
            urls, StringComparison.Ordinal);
        Assert.Contains("NBK602919", urls, StringComparison.Ordinal);
        Assert.Contains("PMC7399624", urls, StringComparison.Ordinal);
    }

    [Fact]
    public void NoNciPatientPdqUrlIsCited()
    {
        var urls = CitedUrls();
        Assert.DoesNotMatch(@"(?i)NBK66023", urls);
        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        var titles = Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count;
        var accessed = Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count;

        Assert.True(urls >= 9, $"only {urls} sources are cited");
        Assert.Equal(urls, titles);
        Assert.Equal(urls, accessed);
    }

    [Fact]
    public void TheManufacturersContraindicationListIsNotPublishedAndTheInstructionIs()
    {
        // The only source for the list is optunegio.com. Widened after /review
        // beat the phrase list with paraphrases.
        var guard = new Regex(
            @"(?i)\bbullets?\b|(?i)\bskull\b|(?i)\bmissing bone\b|(?i)\bactive(?:ly)? implanted\b"
            + @"|(?i)\bpacemakers?\b|(?i)\bshunts?\b|(?i)\bmetal fragments?\b|(?i)\bplates?\b"
            + @"|(?i)\b(?:22|twenty-two)\b|(?i)\bnot (?:for|suitable for) (?:people|anyone) (?:with|who)\b"
            + @"|(?i)\bwho have\b[^.]{0,30}\b(?:cannot|can't|should not|shouldn't)\b"
            + @"|(?i)\b(?:if you have|with)\b[^.]{0,30}\byou (?:cannot|can't) (?:use|have)\b");

        Assert.Matches(guard, "It is not for people with a skull defect.");
        Assert.Matches(guard, "It is not for people who have a pacemaker.");
        Assert.Matches(guard, "If you have a hole in the skull, you cannot use it.");
        Assert.Matches(guard, "People who have a shunt should not use it.");
        Assert.DoesNotMatch(guard, Plain);

        Assert.Contains("A talk about any metal or implants from surgery", PlainOf(NeedHeading),
            StringComparison.Ordinal);
    }

    [Fact]
    public void WhoItSuitsAndWhenItStartsAreRoutedToTheHubThatOwnsThemAndNotRestated()
    {
        var guard = new Regex(
            @"(?i)\b(?:upper|top) (?:part )?of the brain\b|(?i)\bsupratentorial\b"
            + @"|(?i)\bmanaging reasonably well\b"
            + @"|(?i)\b(?:four|4)\s*(?:to|-|–)\s*(?:seven|7)\s+weeks\b|(?i)\bseven weeks\b"
            + @"|(?i)\bweeks after radiation\b|(?i)\b(?:a|one) months? after radiation\b");

        Assert.Matches(guard, "It starts four to seven weeks after radiation.");
        Assert.Matches(guard, "It starts about a month after radiation ends.");
        Assert.Matches(guard, "It is for tumors in the top part of the brain.");
        Assert.DoesNotMatch(guard, Plain);

        var hub = Sibling("tumors", "glioblastoma.md");
        Assert.Contains("four to seven weeks after radiation ends", hub, StringComparison.Ordinal);
        Assert.Contains("upper part of the brain", hub, StringComparison.Ordinal);

        var routing = ParagraphsOf(RawSection(WhyHeading))
            .Single(p => p.Contains("/tumors/glioblastoma", StringComparison.Ordinal));
        Assert.Contains("usually starts", routing, StringComparison.Ordinal);
    }

    // ------------------------------------------------ the lived-experience half

    [Fact]
    public void TheShavedHeadSectionGivesBothReactionsAndEndsOnWhatTheReaderControls()
    {
        var paragraphs = ParagraphsOf(RawSection(HeadHeading));

        Assert.Contains(paragraphs, p => p.StartsWith("For some people that is the hardest part.",
            StringComparison.Ordinal));
        Assert.Contains(paragraphs, p => p.StartsWith("For others it matters less.",
            StringComparison.Ordinal));

        var section = PlainOf(HeadHeading);
        Assert.Contains("the reason people who said no gave most often", section, StringComparison.Ordinal);
        Assert.Contains("It is a real reason", section, StringComparison.Ordinal);

        // A hat does not hide the device, and must not be offered as if it does
        // (/review): no headwear under "decide what to share".
        Assert.DoesNotMatch(@"(?i)\b(?:hat|cap|scarf|wig|headwear)\b", paragraphs[^1]);

        // §12.6: position, END-anchored.
        Assert.StartsWith("You decide what to share.", paragraphs[^1], StringComparison.Ordinal);
        Assert.Matches(@"decide ahead of time who you want to tell more\.$", paragraphs[^1]);
    }

    [Fact]
    public void TheFeelSectionEndsOnWhatCanBeDoneRatherThanOnTheDownsides()
    {
        // §12.6. /review: the first draft ended on "sleeping and showering were
        // harder".
        var last = CuratedPage.SentencesOf(PlainOf(FeelHeading))[^1];
        Assert.Contains("Tell your team", last, StringComparison.Ordinal);
        Assert.Contains("made easier", last, StringComparison.Ordinal);
    }

    [Fact]
    public void TheScalpSectionGivesCareWithoutADrugOrATechniqueMeasurement()
    {
        var section = PlainOf(SideHeading);

        Assert.Contains("clean electric razor", section, StringComparison.Ordinal);
        Assert.Contains("Move the pads a little at each change, in the way your team shows you", section,
            StringComparison.Ordinal);
        Assert.Contains("Tell your team about skin changes as soon as you notice them", section,
            StringComparison.Ordinal);

        // Ask FIRST, lotions second, in one bullet (/review: they contradicted).
        var askBullet = BulletsOf(RawSection(SideHeading))
            .Single(b => b.Contains("water-based lotions", StringComparison.Ordinal));
        Assert.StartsWith("Ask your team before you put anything on your scalp.", askBullet,
            StringComparison.Ordinal);

        var drugs = new Regex(
            @"(?i)\b(?:clobetasol|betamethasone|fluocinonide|hydrocortisone|gabapentin|pregabalin|"
            + @"glycopyrrolate|aluminum chloride|antiperspirants?|corticosteroids?|steroid creams?|"
            + @"antibiotics?|mupirocin|zinc oxide|barrier films?)\b");

        Assert.Matches(drugs, "Use a steroid cream if it gets red.");
        Assert.DoesNotMatch(drugs, Plain);

        Assert.Contains("ask your team which one it is likely to be",
            CuratedPage.SentencesOf(section)[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void TheSeizureRiskRoutesToThePageThatOwnsWhatToDo()
    {
        var paragraph = ParagraphsOf(RawSection(SideHeading))
            .Single(p => p.Contains("chance of a seizure", StringComparison.Ordinal));
        Assert.Contains("/seizures/what-to-do", paragraph, StringComparison.Ordinal);
        Assert.Contains("slightly", paragraph, StringComparison.Ordinal);

        CuratedPage.AssertNoEscalationList(Page, Slug);
    }

    [Fact]
    public void TheDeviceNumberNeverStandsInForTheCareTeam()
    {
        // /review: "That may not be the same number as your care team" left a
        // reader with a broken-down scalp or a seizure at night unsure who to
        // call, and the section ended on the uncertainty.
        var bullet = BulletsOf(RawSection(NeedHeading))
            .Single(b => b.StartsWith("A number for the machine", StringComparison.Ordinal));
        Assert.Matches(@"For anything about your health, call your care team\.$", bullet);
    }

    [Fact]
    public void TheGrowthSectionSeparatesAChangedScanFromGrowthAndCarriesEanosAdvice()
    {
        var paragraphs = ParagraphsOf(RawSection(GrowsHeading));

        Assert.StartsWith("Growth is not the only reason a scan can look worse.", paragraphs[0],
            StringComparison.Ordinal);
        Assert.Contains("/tests/follow-up-scans", paragraphs[0], StringComparison.Ordinal);

        var advice = paragraphs.Single(p => p.StartsWith("If it is growth", StringComparison.Ordinal));
        Assert.Contains("The European guideline advises against staying on it", advice,
            StringComparison.Ordinal);
        Assert.Contains("clearly grown", advice, StringComparison.Ordinal);
        Assert.Contains("/treatments/clinical-trials", advice, StringComparison.Ordinal);
    }

    [Fact]
    public void EachPracticalPointLivesInOneSection()
    {
        // §12.8 (WI-532): a page can restate ITSELF and no corpus check sees it.
        // /review found seven points said two or three times. Each is pinned to
        // the one section that owns it.
        (string Pattern, string Owner)[] points =
        [
            (@"(?i)\bplug (?:it )?into the wall\b|(?i)\bfrom the wall\b", EverydayHeading),
            (@"(?i)\bget wet\b|(?i)\bgets? wet\b", EverydayHeading),
            (@"(?i)\bvisiting nurse\b", NeedHeading),
            (@"(?i)\bvery expensive\b", AgreeHeading),
            (@"(?i)\btaught how to use it\b|(?i)\btraining\b", StepsHeading),
            (@"(?i)\bimplants?\b", NeedHeading),
        ];

        var sections = Headings()
            .Select(h => (Heading: h, Text: PlainOf(h)))
            .ToList();

        foreach (var (pattern, owner) in points)
        {
            var holders = sections.Where(s => Regex.IsMatch(s.Text, pattern))
                .Select(s => s.Heading)
                .Where(h => h != QuestionsHeading)
                .ToList();
            Assert.True(holders.Count == 1 && holders[0] == owner,
                $"'{pattern}' is in [{string.Join(", ", holders)}] rather than only in '{owner}'");
        }
    }

    // ------------------------------------------------------- the caregiver half

    [Fact]
    public void TheCaregiverSectionComposesTheBlockAndAddsOnlyWhatIsTrueHere()
    {
        Assert.Matches(@"(?m)^\[CAREGIVER\]\s*$", Page);

        var section = PlainOf(CaregiverHeading);
        Assert.Contains("You may be the one changing the pads", section, StringComparison.Ordinal);
        Assert.Contains("the parts of the scalp the person you care for cannot", section,
            StringComparison.Ordinal);
        Assert.Contains("Night alarms can wake the whole house", section, StringComparison.Ordinal);

        var claims = Regex.Matches(RawSection(CaregiverHeading), @"(?m)^\*\*").Count;
        Assert.True(claims == 4, $"the caregiver section makes {claims} claims rather than four.");

        // A block sentence appended INSIDE an existing paragraph is invisible to
        // the bold count and, with one word changed, to the shingles (/review).
        // The block's own load-bearing phrases are banned from the page's half.
        var blockPhrases = new Regex(
            @"(?i)\bwork hours\b|(?i)\bnights and weekends\b|(?i)\bfirst call\b|(?i)\bcall an ambulance\b"
            + @"|(?i)\bnotepad\b|(?i)\bplaying doctor\b|(?i)\bgive them a real job\b");
        Assert.Matches(blockPhrases, "Get one number for work hours and one for nights.");
        Assert.DoesNotMatch(blockPhrases, section);

        // And the hub's framing is not paraphrased here (ruling (1)).
        Assert.DoesNotMatch(@"(?i)\btest of how hard\b", Plain);
    }

    [Fact]
    public void TheCaregiverSectionDoesNotRepeatTheBlocksOwnSentences()
    {
        // §12.8 (WI-510): word shingles over the page's OWN half of the section.
        // `PlainOf` reads the raw markdown, where [CAREGIVER] is still a
        // directive, so the block's words are not in it.
        var blockWords = Regex.Matches(
                CuratedPage.Flatten(CuratedPage.ReaderText(
                    CuratedPage.Read(Path.Combine("..", "blocks", "caregiver.md"))))
                .ToLowerInvariant(),
                @"[a-z0-9']+")
            .Select(m => m.Value).ToList();

        Assert.True(blockWords.Count > 200,
            "the block text could not be read, so this guard proves nothing");

        var blockShingles = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i + 8 <= blockWords.Count; i++)
        {
            blockShingles.Add(string.Join(' ', blockWords.Skip(i).Take(8)));
        }

        var mineWords = Regex.Matches(PlainOf(CaregiverHeading).ToLowerInvariant(), @"[a-z0-9']+")
            .Select(m => m.Value).ToList();

        var shared = new List<string>();
        for (var i = 0; i + 8 <= mineWords.Count; i++)
        {
            var shingle = string.Join(' ', mineWords.Skip(i).Take(8));
            if (blockShingles.Contains(shingle))
            {
                shared.Add(shingle);
            }
        }

        Assert.True(shared.Count == 0,
            "the page's caregiver half restates [CAREGIVER]:\n  " + string.Join("\n  ", shared.Take(4)));
    }

    // ----------------------------------------------------------- the glossary

    [Fact]
    public void TheGlossaryEntryIsSuppressedHereAndKeptForTheHubsThatUseIt()
    {
        var markers = Regex.Matches(CuratedPage.Body(Page), @"!%(.+?)%")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.Equal(["tumor treating fields"], markers);

        Assert.Matches(@"(?i)\btumor treating fields\b", Body);
        Assert.Empty(Regex.Matches(Page, @"(?m)^.*!%.*\[[A-Z-]+\].*$"));

        var entry = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "..", "glossary",
            "tumor-treating-fields.md"));
        Assert.Matches(@"(?m)^term: tumor treating fields\s*$", entry);
        Assert.Matches(@"(?m)^also: \[Optune\]\s*$", entry);

        foreach (var hub in new[] { "glioblastoma.md", "high-grade-glioma.md" })
        {
            var text = CuratedPage.Read("tumors", hub);
            Assert.DoesNotContain("!%tumor treating fields%", text, StringComparison.OrdinalIgnoreCase);
            Assert.Matches(@"(?i)\btumor treating fields\b", CuratedPage.Body(text));
        }
    }

    // -------------------------------------------------------------- the doors

    [Fact]
    public void BothHubsRouteHereFromTheParagraphThatOwnsTheClaim()
    {
        const string Door = "/treatments/tumor-treating-fields";

        var gbm = ParagraphsOf(CuratedPage.Body(CuratedPage.Read("tumors", "glioblastoma.md")))
            .Single(p => p.Contains(Hours, StringComparison.Ordinal));
        Assert.Contains(Door, gbm, StringComparison.Ordinal);
        Assert.Contains("how hard you are trying", gbm, StringComparison.Ordinal);

        var hgg = ParagraphsOf(CuratedPage.Body(CuratedPage.Read("tumors", "high-grade-glioma.md")))
            .Single(p => p.Contains(Eano, StringComparison.Ordinal));
        Assert.Contains(Door, hgg, StringComparison.Ordinal);
        Assert.Contains("rather than assuming either way", hgg, StringComparison.Ordinal);
    }

    // -------------------------------------------------- corpus-wide housekeeping

    [Fact]
    public void ThePageDoesNotRestateWhatAnotherPageAlreadyOwns()
    {
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug,
            // /tumors/high-grade-glioma's EANO sentence, verbatim by the §12.10
            // decision recorded in the front matter.
            "the european guideline calls its role controversial and says it is not widely available "
            + "there");
    }

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        var offenders = CuratedPage.BritishForms
            .Where(form => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(form)}"))
            .Where(form => !CuratedPage.BritishFormExemptions.Any(
                ex => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(ex)}")
                      && ex.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "British forms on a page written for a US reader: " + string.Join(", ", offenders));

        // Page-local, not on the shared list (logged for /pm): the page uses the
        // US forms. "feeling sick" is corpus-wide and logged rather than swept.
        Assert.DoesNotMatch(
            @"(?i)\bflat batter|(?i)\bout[- ]of[- ]hours\b|(?i)\bcome round\b|(?i)\btablets\b"
            + @"|(?i)\bcarry on\b|(?i)\bcarries on\b|(?i)\bkeeps? a check\b|(?i)\bwork out between\b"
            + @"|(?i)\bfeeling sick\b",
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
    }

    [Fact]
    public void ThePageNeverSaysYourInsurance()
    {
        Assert.DoesNotMatch(@"(?i)\byour insurance\b", Plain);
        Assert.Contains("who pays for it and what you will owe", PlainOf(NeedHeading), StringComparison.Ordinal);
    }
}

/// <summary>
/// WI-533: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class TumorTreatingFieldsPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/tumor-treating-fields";

    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// The connection string is pushed in. WI-531 shipped two render classes
    /// that did not and they passed here and failed in CI, because a developer
    /// machine has the string in <c>dotnet user-secrets</c> and a runner does
    /// not. <c>TestCollectionHygieneTests</c> guards this corpus-wide.
    /// </summary>
    public TumorTreatingFieldsPageRenderTests(WebApplicationFactory<Program> raw) =>
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
        Assert.Contains("Mostly like having something on your head, and a bag to carry.", html,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/tumors/glioblastoma",
            "/tumors/high-grade-glioma",
            "/treatments/chemotherapy",
            "/tests/follow-up-scans",
            "/seizures/what-to-do",
            "/get-help-now");
    }

    [Fact]
    public async Task TheGlossaryTooltipIsSuppressedHereAndStillFiresOnTheHub()
    {
        // §12.8 (WI-509): both directions.
        var client = factory.CreateClient();

        var html = await client.GetStringAsync(Url);
        Assert.DoesNotContain("def-tumor-treating-fields", html, StringComparison.Ordinal);
        Assert.Contains("def-temozolomide", html, StringComparison.Ordinal);

        var hub = await client.GetStringAsync("/tumors/glioblastoma");
        Assert.Contains("def-tumor-treating-fields", hub, StringComparison.Ordinal);
    }
}
