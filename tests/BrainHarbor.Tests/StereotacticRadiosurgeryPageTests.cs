using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-531, second of two pages: <c>/treatments/stereotactic-radiosurgery</c>.
/// A §12.8 library page, and the one whose whole reason to exist is that its
/// NAME IS WRONG. People arrive believing they have been offered an operation.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * LETTING THE NAME STAND. "Nothing is cut" has to be in the first thing a
///     reader meets, not eight screens down, because §12.3's own number is that
///     readers consume 20 to 28% of a page.
///   * SAYING FRAME OR FRAMELESS IS STANDARD. The backlog item says outright
///     that the literature is still arguing it, and the two patient-level
///     sources do not agree in emphasis: ACS writes that frameless techniques
///     "make this unnecessary" while Cleveland Clinic presents two systems both
///     in use. A reader told the one they are being given is the old way, or
///     the second-best way, has been handed an argument with their team that
///     nobody can settle.
///   * PUBLISHING AN EFFICACY PERCENTAGE. Cleveland Clinic prints "One study
///     reported that the tumor control rate was 95%" — an unnamed single study,
///     and the exact shape §12.4 exists to refuse. Froedtert prints "The Gamma
///     Knife's success rate is impressive". §12.13: the richest source for the
///     day is also the one with the selling in it.
///   * PUBLISHING A SIZE THRESHOLD. ASCO's own qualifying statement says the
///     underlying trials were "generally tumors of less than 3 or 4 cm
///     diameter". Two numbers that disagree inside one qualifier are not a rule
///     a reader can apply to their own report, and §12.8 (WI-527) calls a
///     magnitude next to a quantity a threshold shape with or without a unit.
///   * CITING NCI PATIENT PDQ FOR BRAIN METASTASES. §12.1 says that question is
///     governed by ASCO-SNO-ASTRO 2022 and by nothing else, and the dossier
///     sources the whole "which tumors" section to NBK66023, which is PDQ.
///   * RESTATING RADIATION NECROSIS. /tests/follow-up-scans (WI-521) owns it,
///     by name, at #much-later, including that the clearest numbers for it come
///     from THIS treatment. §12.10: two pages may not state one claim at two
///     strengths, and the sibling's is the stronger one.
///   * PROMISING SHRINKAGE. The goal is tumor control, which means stable OR
///     smaller. A reader who thinks shrinking is the only success reads "no
///     change" on their first report as a failure.
///
/// Every guard starts from §12.8's accumulated lessons rather than
/// rediscovering them: <c>CountWord</c> knows the teens, quantifiers allow a
/// word gap, numeric bans are threshold-shaped, phrase guards run on
/// <c>Plain</c> so the description is in scope, section order asserts PRESENCE
/// before position, every claim-shaped guard has a canary that proves it can
/// fire, and every regex that touches a line break uses <c>\r?\n</c> — never a
/// bare <c>\n</c> and never <c>\s*</c> standing in for a blank line.
/// </summary>
public sealed class StereotacticRadiosurgeryPageContentTests
{
    private const string Slug = "treatments/stereotactic-radiosurgery";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is it?";
    private const string WhyHeading = "Why this and not an operation, or weeks of radiation?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string WaitHeading = "The wait, and what to do with it";
    private const string SoonHeading = "Side effects, in the days after";
    private const string LaterHeading = "Side effects that can come later";
    private const string AnotherHeading = "Why am I having another one?";
    private const string ResultHeading = "Who reads it, and how do I get the result?";
    private const string NeedHeading = "What you need first, or need to bring";
    private const string CaregiverHeading = "For the person taking them";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("treatments", "stereotactic-radiosurgery.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// collapses all whitespace to single spaces, so anything that needs to see
    /// a PARAGRAPH or count a list MARKER — which is how §12.8 (WI-526) says to
    /// count list items — has to cut the markdown itself (§12.8, WI-528).
    /// <c>\r?\n</c>, never a bare <c>\n</c>.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ReaderText</c> strips the front matter, and
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// under the heading, so a body-scoped guard is blind to the first thing a
    /// reader meets (§12.8, WI-524 and WI-528).
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            // `\s*$`, not `"$`: on a CRLF checkout the line ends `"\r\n`, the
            // match fails, and every headline guard runs on an empty string.
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Everything => Headline + " " + Body;

    /// <summary>
    /// Everything, emphasis markers removed. A bolded word defeats a phrase
    /// guard structurally and no vocabulary fixes that (§12.8, WI-526).
    /// </summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    /// <summary>The page's own "## " headings, in order, anchors stripped.</summary>
    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The URL VALUES from the front matter, without the comments around them.
    /// A ban run over the whole front matter fires on this page's own rulings,
    /// which quote the banned URL in order to explain the ban — a guard red on a
    /// correct page is worse than no guard (§12.8).
    /// </summary>
    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    /// <summary>
    /// Removes an allowed phrase instead of exempting the whole sentence that
    /// contains it (§12.8, WI-527: a substring allowlist is a free ride for
    /// everything else in the sentence).
    /// </summary>
    private static string Redact(string text, IEnumerable<string> allowed)
    {
        foreach (var phrase in allowed)
        {
            text = Regex.Replace(text, Regex.Escape(phrase), " ", RegexOptions.IgnoreCase);
        }

        return text;
    }

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): `List.IndexOf` returns -1 and -1 is less than
        // everything, so an order test built from `>` stays green when a whole
        // section is deleted. Presence first, then position.
        //
        // §12.8 (WI-510): read the STANDARD for the slot list, not the previous
        // page. This treatment has a day and a procedure, so slots 2, 3, 5, 7
        // and 8 are all here — including slot 8, which the first page of this
        // item legitimately drops and which this page cannot, because being
        // offered this a second time is a frightening moment with a mundane
        // answer.
        var headings = Headings();

        // THE ORDER IS TAKEN FROM THE STANDARD, NOT FROM THE PAGE. A first
        // draft listed these in the PAGE's order, which meant the "order is
        // fixed" assertion was asserting the page against itself — §12.8
        // (WI-510)'s own lesson, committed inside the test written to enforce
        // it. §12.8's slot order is 7 (what you need first), then the caregiver
        // section, then 8 (why am I having another one), then 9 (who reads it),
        // and all three sibling treatment pages — craniotomy, radiation-therapy
        // and chemotherapy — run exactly that way. The page ran 8, 9, 7,
        // caregiver, so the reader who needed to know what to bring met it
        // after the section about being treated a second time.
        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, StepsHeading, LongHeading, FeelHeading,
            WaitHeading, SoonHeading, LaterHeading,
            NeedHeading, CaregiverHeading, AnotherHeading, ResultHeading,
            QuestionsHeading, NextHeading,
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
                $"'{required[i]}' has moved above '{required[i - 1]}'. §12.8's slot order is fixed; "
                + "which of the middle three a page carries is not.");
        }
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorSoRewordingAHeadingCannotBreakAnInboundLink()
    {
        // §12.8 (WI-508, WI-509): Markdig derives an id from heading TEXT, so a
        // reworded heading silently breaks every inbound deep link and the
        // reader lands at the top of a long page with no sign anything is
        // wrong. `{#id}` makes the wording and the interface independent.
        var headings = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.True(headings.Count >= 15,
            $"only {headings.Count} '## ' headings were found, so this guard is checking almost "
            + "nothing. The heading regex has stopped matching the page.");

        var missing = headings.Where(h => !Regex.IsMatch(h, @"\{#[\w-]+\}$")).ToList();
        Assert.True(missing.Count == 0,
            "these headings have no explicit anchor, so their ids are derived from the wording "
            + "and a reword breaks every inbound link:\n  " + string.Join("\n  ", missing));
    }

    // --------------------------------------------- the name is the whole point

    [Fact]
    public void TheFirstThingAReaderMeetsSaysNothingIsCut()
    {
        // The page exists because the name says surgery. §12.3's own number is
        // that readers consume 20 to 28% of a page, so this cannot live in the
        // body and be counted as said. Asserted over the SHORT VERSION and over
        // the description separately, because §12.8 (WI-520) records a
        // section-scoped position test that could not see the summary, and
        // §12.8 (WI-524) records the description shipping past twenty-four
        // tests.
        var summary = PlainOf(ShortHeading);
        var firstTwo = string.Join(" ", CuratedPage.SentencesOf(summary).Take(2));

        Assert.Matches(@"(?i)\bnothing is cut\b", firstTwo);

        var description = Regex.Replace(
            Regex.Match(CuratedPage.FrontMatter(Page), @"(?m)^description: ""(.+)""\s*$")
                .Groups[1].Value,
            @"[*_]", "");

        // `are not` is NOT in this alternation, and taking it out is the point.
        // The first version held it, and any description at all satisfies a
        // bare "are not" — /review's "The pins are not as bad as they sound"
        // passed while never telling the reader it is not surgery.
        Assert.Matches(
            @"(?i)\b(?:nothing is cut|no cutting|(?:is|are) not (?:an? )?(?:operation|surgery)"
            + @"|sound like operations and are not)\b",
            description);
    }

    [Fact]
    public void ThePageNeverDescribesThisAsAnOperationInItsOwnVoice()
    {
        // The reverse of the check above, and the one that matters when
        // somebody edits for flow. A sentence calling this "the operation" is
        // the single most damaging thing this page could say, because it is the
        // belief the reader arrived with.
        //
        // Redacted, not allowlisted (§12.8, WI-527): the page has to be able to
        // say "not an operation", "instead of an operation" and "weighed
        // against the operation", and exempting whole sentences containing
        // those would be a free ride for everything else in them.
        // Short and named, not a pattern escape (§12.8, WI-526). The page has
        // to be able to say "not an operation" and "instead of an operation" —
        // those are the correction, not the defect. The where-to-go-next line
        // was reworded rather than allowlisted, because "for the operation this
        // is often weighed against" reads, out of context, as this page calling
        // its own subject an operation.
        // WIDENED AFTER /review BEAT IT FIVE WAYS, and the first beating
        // sentence is the one most likely to be pasted in by accident:
        // **"Gamma Knife surgery"** is the literal TITLE of the Cleveland
        // Clinic page this page cites, and the determiner list was three words
        // long, so it walked, as did "their surgery", "a surgery", "on the day
        // of surgery" and "Radiosurgery is an operation done with beams".
        string[] allowed =
        [
            "not an operation",
            "instead of an operation",
            "for an operation",
            "after an operation",
            "not surgery",
            "no actual surgery",
        ];

        var scanned = Redact(Plain, allowed);
        var guard = new Regex(
            // The DETERMINERS THAT POINT AT THE THING IN HAND. A bare
            // `an operation` is not bannable here and the widened version
            // proved it by failing three correct sentences on the first
            // build -- "It can reach places an operation cannot", "It is
            // also used after an operation", and the escalation note about
            // "the weeks after an operation". This page discusses
            // operations constantly, because an operation is the other
            // treatment. What it may never do is call THIS one.
            @"(?i)\b(?:this|that|the|your|their|his|her|my|our)\s+(?:operation|surgery)\b"
            + @"|(?i)\b(?:during|after|before|day of|time of|kind of|sort of|type of)\s+"
            + @"(?:the|your|their|his|her|my)\s+(?:operation|surgery)\b"
            + @"|(?i)\b(?:Gamma Knife|CyberKnife|radiosurgery)\s+surgery\b"
            + @"|(?i)\b(?:radiosurgery|this|it)\s+is\s+an?\s+(?:operation|surgery)\b"
            + @"|(?i)\bhaving\s+(?:this|your)\s+(?:operation|surgery)\b");

        // The canary: prove the guard can fire before its pass on the page
        // means anything (§12.8, WI-523).
        Assert.Matches(guard, "You will be sore after the operation.");
        Assert.Matches(guard, "Your surgery takes about an hour.");

        // The five sentences /review used to beat the first version.
        Assert.Matches(guard, "Gamma Knife surgery takes about an hour.");
        Assert.Matches(guard, "Most people go home a few hours after their surgery.");
        Assert.Matches(guard, "It is a surgery without a knife.");
        Assert.Matches(guard, "On the day of your surgery, arrive early.");
        Assert.Matches(guard,
            "Radiosurgery is an operation done with beams instead of a blade.");

        // The three CORRECT sentences the widened version failed. Pinned
        // here so the next widening has to survive them.
        // `the` is back in the determiner list: the harness walked "Most
        // people go home the same day as THE operation" through the
        // narrowed version, and `the operation` is the commonest way in
        // English to name the thing in hand. The indefinite article stays
        // out, which is what the three sentences below prove.
        Assert.Matches(guard, "Most people go home the same day as the operation.");
        Assert.DoesNotMatch(guard, "It can reach places an operation cannot.");
        Assert.DoesNotMatch(guard, "It is also used after an operation.");
        Assert.DoesNotMatch(guard, "written for the weeks after an operation");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page refers to this treatment as an operation, which is the belief it exists to "
            + "correct: " + string.Join(" | ", hits));
    }

    // ------------------------------------------- frame versus frameless, open

    [Fact]
    public void ThePageNeverSaysEitherTheFrameOrTheMaskIsTheStandardOne()
    {
        // The backlog item's own instruction, and the two patient-level sources
        // disagree in emphasis: ACS writes that frameless techniques "make this
        // unnecessary"; Cleveland Clinic describes two systems both in use.
        // Neither is published as the norm.
        // `usually` and `normally` are NOT in this vocabulary, and taking them
        // out is the point. The first version held them and fired on three
        // correct sentences straight away — "the frame or mask comes off, and
        // you go home, usually the same day", and "usually still wearing the
        // frame". Those are timing adverbs, not a claim about which method is
        // the norm. §12.8 (WI-530): widening a guard needs the correct page
        // re-run immediately.
        // WIDENED AFTER /review BEAT IT FIVE WAYS. `most hospitals` was not in
        // the noun list, `traditional`, `conventional`, `gentler` and
        // `nowadays` were absent, and "Frames are used less and less" carries
        // the claim with no normative token at all.
        const string Normative =
            @"(?:standard|the usual|normal|preferred|better|best|newer|more modern|older|"
            + @"out of date|outdated|old[- ]fashioned|traditional|conventional|gentler|"
            + @"more common|less common|the norm|these days|nowadays|increasingly|"
            + @"most (?:\w+\s+){0,2}(?:centers|centres|hospitals|units|teams|places|people)|"
            + @"nearly (?:all|everybody|everyone)|used less)";

        var guard = new Regex(
            @"(?i)\b(?:frame|frameless|frames|mask|masks|pins?)\b(?:\W+\w+){0,6}\W+\b"
            + Normative + @"\b"
            + @"|(?i)\b" + Normative
            + @"\b(?:\W+\w+){0,6}\W+\b(?:frame|frameless|frames|mask|masks|pins?)\b");

        // Canaries in both directions, because this guard is written as two
        // alternations and either branch alone passing is §12.8 (WI-515)'s
        // weak-alternation defect.
        Assert.Matches(guard, "A frame is the standard way of doing this.");
        Assert.Matches(guard, "Most centers now use a mask instead.");
        Assert.Matches(guard, "The frame is the older approach.");
        Assert.Matches(guard, "These days a mask is what you get.");

        // The five sentences /review used to beat the first version.
        Assert.Matches(guard, "Most hospitals use a mask now.");
        Assert.Matches(guard, "The frame is the traditional way of doing it.");
        Assert.Matches(guard, "A mask is the gentler option.");
        Assert.Matches(guard, "Frames are used less and less.");
        Assert.Matches(guard, "Nowadays a mask is what nearly everybody gets.");

        // And the correct sentences that beat the first version still pass.
        Assert.DoesNotMatch(guard, "The frame or mask comes off, and you go home, usually the "
            + "same day.");
        Assert.DoesNotMatch(guard, "You are usually still wearing the frame or the mask.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page takes a side on frame versus frameless, which the literature has not "
            + "settled: " + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageSaysBothExistAndThatWhichOneYouGetDependsOnTheCenter()
    {
        // The other half. Banning the comparison without requiring the honest
        // answer would leave a reader who has been offered pins with no idea
        // that a mask is also a thing, which is §12.14's shape: a ban that
        // reads like a fixed claim.
        // SCOPED TO THE STEP THAT OFFERS THE CHOICE. The harness deleted
        // ", with either a frame or a mask" from step 3 and this stayed
        // green, because the page says "frame or mask" three more times
        // further down -- when it comes off, when the scan happens, and
        // during the wait. §12.8 (WI-512): presence was never the
        // property, position was.
        var steps = PlainOf(StepsHeading);
        Assert.Matches(@"(?i)\bYour head is fixed in place\b(?:\W+\w+){0,6}\W+"
            + @"\bwith either a frame or a mask\b", steps);

        Assert.Matches(@"(?i)\bframe or (?:a )?mask\b|(?i)\beither a frame or a mask\b", Plain);
        Assert.Matches(
            @"(?i)\bwhich one (?:your center has|you get)\b|(?i)\bis a fact about your center\b",
            Plain);
    }

    // ------------------------------------------------ numbers, and the two bans

    [Fact]
    public void NoEfficacyOrSuccessFigureAppearsAnywhereOnThePage()
    {
        // Cleveland Clinic publishes "One study reported that the tumor control
        // rate was 95%". Froedtert publishes "The Gamma Knife's success rate is
        // impressive" and "excellent, well-documented clinical outcomes".
        // Neither is on this page, in digits or in words.
        //
        // §12.8 (WI-511): a number written as a word is still a number, so the
        // guard grades the CLAIM and not the character class.
        // WIDENED AFTER /review BEAT IT FIVE WAYS, and one beating sentence
        // published the exact 95% figure this test names in its own comment.
        // `in ten` is not `out of ten`; `in every hundred` was not matched at
        // all; and "most", "the great majority" and "the chance is very high"
        // carry the claim with no countable quantity in them.
        const string Share =
            @"(?:" + CountWord + @"|" + Fraction + @"|most|nearly all|almost (?:all|everybody)|"
            + @"the (?:great |vast )?majority|hardly any|almost none)";

        var guard = new Regex(
            @"(?i)\b(?:success|control|cure|response|survival)\s+rate\b"
            + @"|(?i)\b(?:works?|worked|succeeds?|succeeded|is effective|was effective|"
            + @"stops? (?:the )?tumors? growing)\b(?:\W+\w+){0,5}\W+\b" + Share + @"\b"
            + @"|(?i)\b" + Share + @"\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:of (?:people|patients|tumors|cases)|out of (?:ten|a hundred)|"
            + @"in (?:every )?(?:ten|a hundred|hundred)|tumors stop)\b"
            + @"|(?i)\b(?:chance|odds|likelihood)\b(?:\W+\w+){0,8}\W+"
            + @"\b(?:very high|high|excellent|good)\b"
            + @"|(?i)\d+(?:\.\d+)?\s*(?:%|per ?cent)");

        Assert.Matches(guard, "One study reported that the tumor control rate was 95%.");
        Assert.Matches(guard, "It works for nine out of ten people.");
        Assert.Matches(guard, "Around 95 percent of tumors stop growing.");

        // The five sentences /review used to beat the first version.
        Assert.Matches(guard, "Nine in ten tumors stop growing.");
        Assert.Matches(guard, "Around ninety-five in every hundred tumors stop growing.");
        Assert.Matches(guard, "It stops the tumor growing in most people.");
        Assert.Matches(guard, "It works in the great majority of cases.");
        Assert.Matches(guard, "The chance of it controlling the tumor is very high.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an efficacy figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoSizeThresholdForWhatCanBeTreatedIsPublished()
    {
        // ASCO's qualifying statement on Recommendation 3.2 is "generally
        // tumors of less than 3 or 4 cm diameter" — two numbers inside one
        // qualifier, which is not a rule a reader can apply to their own
        // report. §12.8 (WI-527): ban the threshold SHAPE, not the unit,
        // because taking the unit out of a size threshold does not stop it
        // being one.
        // NARROWED TO THE SUBJECT, not merely scoped (§12.8, WI-529 and
        // WI-530). The first version was a bare magnitude-next-to-a-quantity
        // guard and it fired on five correct sentences on the first build:
        // ASCO's "up to four" SPOTS, the frame weighing "less than two" pounds,
        // the planning wait taking "up to an hour", and "more than one"
        // treatment session. A count of tumors is not a size, and widening a
        // guard needs the correct page re-run immediately.
        // The measurement vocabulary is UNITS ONLY. A first attempt allowed
        // `size|big|large|small|tiny` as the subject anchor and it fired on
        // "up to an hour at one LARGE hospital" — an adjective on a building,
        // two words from a duration. Words that describe a magnitude have
        // innocent uses everywhere; units of length do not.
        const string SizeUnit = @"(?:cm|mm|centimet\w*|millimet\w*|inch\w*)";

        // WIDENED AFTER /review BEAT IT THREE WAYS. `an inch` has no numeral in
        // it, so no branch reached the unit — and an inch and a half is the
        // banned threshold. And one adjective before the fruit ("a LARGE
        // grape") broke the object-comparison branch.
        const string Amount = @"(?:" + CountWord + @"|an?|half an?)";

        var guard = new Regex(
            @"(?i)\b(?:more than|less than|smaller than|bigger than|larger than|at least|"
            + @"up to|under|over|above|below|no more than|as much as)\b(?:\W+\w+){0,3}\W+\b"
            + Amount + @"\b(?:\W+\w+){0,2}\W+\b" + SizeUnit + @"\b"
            + @"|(?i)\b" + Amount + @"\s*(?:cm|mm)\b"
            + @"|(?i)\b" + Amount + @"\b(?:\W+\w+){0,1}\W+\b"
            + @"(?:centimet\w*|millimet\w*|inch\w*)\b"
            + @"|(?i)\b(?:smaller|bigger|larger|less|more) than\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:pea|grape|walnut|marble|thumbnail|golf ball|coin)\b"
            + @"|(?i)\bthe size of an?(?:\s+\w+){0,2}\s+"
            + @"(?:pea|grape|walnut|golf ball|marble|thumbnail|coin)\b");

        Assert.Matches(guard, "It is used for tumors smaller than three centimeters.");
        Assert.Matches(guard, "Targets of up to four centimeters can be treated.");
        Assert.Matches(guard, "Anything under 3 cm is suitable.");
        Assert.Matches(guard, "About the size of a walnut or smaller.");
        Assert.Matches(guard, "This suits anything less than about four centimeters across.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "It is for targets under about an inch and a half across.");
        Assert.Matches(guard, "It suits anything about the size of a large grape or smaller.");
        Assert.Matches(guard, "Anything much over an inch is usually treated another way.");

        // And the sentences that beat the first version must still pass.
        Assert.DoesNotMatch(guard, "up to four spots that have not been operated on");
        Assert.DoesNotMatch(guard, "The metal frame weighs less than two pounds.");
        Assert.DoesNotMatch(guard, "up to an hour at one large hospital");
        Assert.DoesNotMatch(guard, "you might need more than one treatment session");

        // The size section has to say something, and what it says is a
        // question rather than a rule.
        Assert.Matches(@"(?i)\bthe size of what is being treated\b", PlainOf(WhyHeading));
        Assert.Matches(@"(?i)\bask where yours falls\b", PlainOf(WhyHeading));

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a size threshold has reached the page and a reader will measure their own report "
            + "against it: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoRadiationDoseAppearsAnywhere()
    {
        // §12.4 R1: all Gy and mg figures stay out. The sources hand this page
        // two of them — EANO's "radiosurgery with a single dose of 15-20 Gy"
        // and its "60 Gy in 1.8-2 Gy fractions". §12.8 (WI-527): "Fifty-four
        // grays in thirty visits" walked past a guard holding `\d+ Gy`, because
        // a spelled compound is not a count word and `gray\b` misses the
        // plural.
        var guard = new Regex(
            @"(?i)\b" + CountWord + @"\s*(?:Gy\b|grays?\b|gray\b)"
            + @"|(?i)\bgrays?\b(?:\W+\w+){0,3}\W+\b" + CountWord + @"\b"
            + @"|(?i)\b" + CountWord + @"\s*(?:mg|milligram)");

        Assert.Matches(guard, "A single dose of twenty grays is given.");
        Assert.Matches(guard, "Fifty-four grays in thirty visits.");
        Assert.Matches(guard, "The dose is 20 Gy.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0, "a dose has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoPrognosisOrSurvivalClaimAppearsAnywhere()
    {
        // §12.2 item 5 and §12.5. Cleveland Clinic carries a whole "What is the
        // life expectancy after Gamma Knife surgery?" section, including "In
        // some cases, it can prolong your life expectancy", and this page cites
        // that source.
        // WIDENED AFTER /review BEAT IT TWICE. §12.8 (WI-527): a digit ban is
        // not a prognosis ban, and this was a digit-and-phrase ban.
        var guard = new Regex(
            @"(?i)\blife expectancy\b|(?i)\bmedian survival\b|(?i)\bsurvival\b"
            + @"|(?i)\bfive[- ]year\b|(?i)\bhow long (?:you|people|they) (?:will )?live\b"
            + @"|(?i)\b(?:live|living|lived|lives)\s+(?:for\s+)?(?:longer|" + CountWord + @")\b"
            + @"|(?i)\bstill (?:doing well|alive|here|with us|around)\b"
            + @"|(?i)\bprolong\b|(?i)\bbuys? you (?:time|years|months)\b"
            + @"|(?i)\b(?:adds?|gains?)\s+(?:\w+\s+){0,2}(?:months|years)\b");

        Assert.Matches(guard, "It can prolong your life expectancy.");
        Assert.Matches(guard, "People live longer after this treatment.");
        Assert.Matches(guard, "It buys you time.");

        // The two sentences /review used to beat the first version.
        Assert.Matches(guard, "Most people in that group are still doing well years later.");
        Assert.Matches(guard, "It can add months or years.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a prognosis claim has reached the page: " + string.Join(" | ", hits));
    }

    // ---------------------------------------------------- source precedence

    [Fact]
    public void NoNciPatientPdqUrlIsCitedForTheBrainMetastasisMaterial()
    {
        // §12.1: brain-metastasis radiation is governed by ASCO-SNO-ASTRO 2022
        // and never by NCI patient PDQ. The dossier sources the whole "which
        // tumors" section to NBK66023, which IS patient PDQ.
        //
        // §12.8 (WI-528): a dead-domain list cannot express §12.1, because a
        // page may legitimately cite cancer.gov for framing. The rule needs its
        // own pattern.
        var urls = CitedUrls();
        Assert.True(urls.Split('\n').Length >= 5,
            "fewer than five source URLs were read out of the front matter, so this ban is "
            + "checking almost nothing");

        Assert.DoesNotMatch(@"(?i)NBK66023", urls);
        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);

        // And the claim is actually resting on the guideline, not merely
        // missing a banned citation (§12.14: banning a citation is not fixing a
        // claim).
        Assert.Contains("PMC8917399", urls, StringComparison.Ordinal);
    }

    [Fact]
    public void NoUnreachableOrManufacturerSourceIsCited()
    {
        // journals.lww.com and sciencedirect.com are both dead for this
        // project; cyberknife.com and elekta.com are the machines' own makers.
        var urls = CitedUrls();

        foreach (var banned in new[]
                 {
                     "journals.lww.com", "sciencedirect.com", "cyberknife.com",
                     "elekta.com", "redjournal.org", "mdpi.com", "academic.oup.com",
                     "researchgate.net", "ajnr.org", "surgicalneurologyint.com",
                 })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheFramelessOutcomeClaimTheDeadSourceCarriedIsNotPublishedEither()
    {
        // §12.14, and it is the reason this test exists next to the one above.
        // The dossier's "local control appears similar between frame-based and
        // frameless" rests on journals.lww.com, which is unreachable. Dropping
        // the URL and keeping the sentence would be an uncited invented claim
        // rather than a miscited one (§12.8, WI-510).
        // CLAIM-SHAPED AFTER /review BEAT THE PHRASE LIST THREE WAYS. A list of
        // nine fixed phrases in one word order cannot see an inversion ("the
        // results are the same whichever you have") or a paraphrase ("neither
        // one is more accurate"). The shape is: two of the method words in one
        // sentence, plus an equivalence token.
        const string Method = @"(?:frames?|frameless|masks?|pins?|the two|both|either|neither)";
        // "the same" ALONE IS NOT AN EQUIVALENCE CLAIM. The first version held
        // it and fired on four correct sentences on the first build — "the
        // frame or mask comes off, and you go home, usually the same day", "A
        // mask is the same mesh mask used for ordinary radiation", "Call your
        // team the same day", and the page's own "the tumor stays the same
        // size". The noun after it is what makes it a comparison.
        const string Equivalence =
            @"(?:as good as|just as good|no difference|works as well|"
            + @"equally (?:good|effective|well)|similar (?:results?|control|outcomes?)|"
            + @"as accurate|as precise|either way|no better|no worse|"
            + @"more accurate than|"
            + @"the same (?:results?|answer|outcomes?|control|accuracy|either way))";

        var guard = new Regex(
            @"(?i)\b" + Method + @"\b(?:\W+\w+){0,14}\W+\b" + Equivalence + @"\b"
            + @"|(?i)\b" + Equivalence + @"\b(?:\W+\w+){0,14}\W+\b" + Method + @"\b"
            // The inversion whose SUBJECT is the results, with no method
            // word in the sentence at all: "The results are the same
            // whichever you have."
            + @"|(?i)\b(?:results?|outcomes?|control|accuracy)\b(?:\W+\w+){0,6}\W+"
            + @"\b(?:are the same|is the same|no different|no better|no worse)\b"
            + @"|(?i)\b(?:whichever you have|it makes no difference)\b");

        // The four CORRECT sentences the first version failed, pinned so the
        // next widening has to survive them (§12.8, WI-530).
        Assert.DoesNotMatch(guard,
            "The frame or mask comes off, and you go home, usually the same day.");
        Assert.DoesNotMatch(guard,
            "A mask, if you have one instead, is the same mesh mask used for ordinary "
            + "radiation.");
        Assert.DoesNotMatch(guard,
            "pin holes in your scalp. Call your team the same day if a pin site turns hot.");
        Assert.DoesNotMatch(guard,
            "The goal is called tumor control, and it means the tumor stays the same size.");

        Assert.Matches(guard, "A mask works as well as a frame.");
        Assert.Matches(guard, "The two give similar results.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "Neither one is more accurate than the other.");
        Assert.Matches(guard, "The results are the same whichever you have.");
        Assert.Matches(guard,
            "Both hold the head still well enough to give the same answer.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an outcome comparison has reached the page, and the source that carried it is "
            + "unreachable: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheGliomaAnswerIsAtTheStrengthTheGuidelineStatesIt()
    {
        // EANO says two things and the page owes both. For a newly diagnosed
        // glioblastoma, radiosurgery is "not superior to standard radiotherapy
        // regimens in terms of OS"; for a recurrence it is among the "modern,
        // highly conformal radiation techniques" considered.
        //
        // The dossier's uncited "generally not the primary treatment for
        // diffuse gliomas, which infiltrate beyond any target volume" is NOT
        // what the guideline says, and a page that printed it would tell a
        // recurrence reader not to ask.
        var section = PlainOf(WhyHeading);

        Assert.Matches(
            @"(?i)has not been shown to do better than an ordinary course of radiation", section);
        Assert.Matches(@"(?i)\bcome back\b(?:\W+\w+){0,20}\W+\bconsidered\b", section);

        // And neither direction is overstated.
        // WIDENED AFTER /review BEAT IT THREE WAYS, and one of them was a
        // single letter: `\bglioma\b` cannot match `gliomas`. §12.8 (WI-527):
        // an inflection is a door.
        const string Refusal =
            @"(?:never (?:used|an option|offered|done)|cannot be used|can't be used|"
            + @"is not (?:used|an option|offered|something they do)|"
            + @"they do not do|they don't do|nobody (?:offers|does) it|"
            + @"not something (?:they do|that is done))";

        var overstated = new Regex(
            @"(?i)\b" + Refusal + @"\b(?:\W+\w+){0,8}\W+\bgliomas?\b"
            + @"|(?i)\bgliomas?\b(?:\W+\w+){0,8}\W+\b" + Refusal + @"\b");

        Assert.Matches(overstated, "Radiosurgery is never used for a glioma.");
        Assert.Matches(overstated, "For a glioma it is not an option.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(overstated, "It is not used for gliomas.");
        Assert.Matches(overstated, "For a glioma, this is not something they do.");
        Assert.Matches(overstated, "Nobody offers it for a glioma.");

        Assert.DoesNotMatch(overstated, section);
    }

    [Fact]
    public void TheBrainMetastasisSentenceKeepsTheGuidelinesOwnScopeWords()
    {
        // §12.6: keep the qualifier, shorten it. ASCO's Recommendation 3.2 is
        // scoped three ways — up to four spots, not operated on, and not small
        // cell carcinoma — and dropping any of them hands a reader a rule that
        // is not theirs.
        var section = PlainOf(WhyHeading);

        Assert.Matches(@"(?i)\bup to four\b", section);
        Assert.Matches(@"(?i)\bhave not been operated on\b", section);
        // ASCO excludes "small-cell carcinoma". A first draft narrowed it
        // to the lung one, and this assertion pinned the narrowing in
        // place -- §12.8 (WI-519): narrowing a source's scope is a defect
        // even when the words look more specific.
        Assert.Matches(@"(?i)\bsmall cell cancer\b", section);
        Assert.DoesNotMatch(@"(?i)\bsmall cell lung cancer\b", section);

        // Recommendation 3.3, the after-surgery half, with its own scope.
        Assert.Matches(@"(?i)\bone or two\b", section);
        Assert.Matches(@"(?i)\bcan be treated safely\b", section);

        // It is presented as a guideline the reader can quote, not as a
        // promise about them.
        Assert.Matches(@"(?i)\bshould be offered\b", section);
    }

    // --------------------------------------------- the day, and the long wait

    [Fact]
    public void TheWaitHasItsOwnSectionAndSaysHowLongAndWhatToDo()
    {
        // The dossier calls the planning wait "the least-explained part of the
        // day", and it is the one thing this page can give a reader that
        // nothing else will. A section naming a problem with no answers is
        // banned outright by §12.8's WI-506 rule.
        var section = PlainOf(WaitHeading);

        Assert.Matches(@"(?i)\bcouple of hours\b|(?i)\bone or two hours\b|(?i)\bup to an hour\b",
            section);
        Assert.Matches(@"(?i)\bnothing is wrong\b", section);

        // At least four things the reader can actually do, counted by marker
        // rather than by numeral (§12.8, WI-526: count list items by marker, or
        // a bullet added as prose leaves the count passing).
        var actions = Regex.Matches(RawSection(WaitHeading), @"(?m)^\s*(?:[-*]|\d+\.)\s").Count;
        Assert.True(actions >= 5,
            $"the wait section offers only {actions} things a reader can do. A section that "
            + "names a problem and answers none of it is the §12.8 failure this slot exists "
            + "to prevent.");
    }

    [Fact]
    public void TheWholeDayIsGivenAShapeSoNobodyIsAmbushedByHourFive()
    {
        // §12.4 R1: orienting durations are in. The three that matter are the
        // treatment itself, the planning wait, and the whole day — and the
        // whole day is the one nobody quotes.
        var section = PlainOf(LongHeading);

        Assert.Matches(@"(?i)\bquarter of an hour\b", section);
        Assert.Matches(@"(?i)\bseveral hours\b", section);
        Assert.Matches(@"(?i)\bfull working day\b|(?i)\btwelve\b", section);

        // And the instruction that follows from it.
        Assert.Matches(@"(?i)\bplan the day, not the treatment\b", section);
    }

    [Fact]
    public void TheStepListIsNumberedAndReachesTheWaitAndGoingHome()
    {
        // §12.8 slot 3 is a numbered list, concrete and sensory. Counted by
        // marker (§12.8, WI-526) and asserted for the two steps most likely to
        // be edited out for flow: the wait, and going home the same day.
        var steps = Regex.Matches(RawSection(StepsHeading), @"(?m)^\s*\d+\.\s").Count;
        Assert.True(steps >= 7,
            $"the step list has {steps} steps. The day is the subject of this page and its "
            + "shape is the content.");

        var section = PlainOf(StepsHeading);
        Assert.Matches(@"(?i)\bYou wait while your plan is built\b", section);
        Assert.Matches(@"(?i)\bgo home, usually the same day\b", section);
        Assert.Matches(@"(?i)\bYou stay awake\b", section);
    }

    [Fact]
    public void TheFeelSectionSaysTheTreatmentItselfIsPainlessAndDoesNotSoftenTheFrame()
    {
        // Both halves. Froedtert's own sentence is that patients "typically
        // feel slight discomfort from the local anesthetic" and "some patients
        // have reported feeling pressure" — the page must not upgrade that into
        // pain, and must not erase it either.
        var section = PlainOf(FeelHeading);

        Assert.Matches(@"(?i)\bfeels like nothing at all\b", section);
        Assert.Matches(@"(?i)\bno heat\b", section);
        Assert.Matches(@"(?i)\bno noise\b", section);
        Assert.Matches(@"(?i)\bpressure rather than pain\b", section);
        Assert.Matches(@"(?i)\byour head is not shaved\b", section);

        // The frame is never described as painless, which is a claim no source
        // makes about the pinning.
        // WIDENED AFTER /review BEAT IT TWICE, and one was subject-verb
        // agreement: the list held `does not hurt` and a plural subject takes
        // `do not hurt`.
        const string Softened =
            @"(?:painless|do(?:es)? ?n[o']t hurt|do not hurt|no pain|feel nothing|"
            + @"(?:will|do) not feel|won't feel)";

        // BOTH WORD ORDERS. "You will not feel the pins going in" puts the
        // softening in front of the noun, and a one-direction guard is the
        // §12.8 (WI-519) defect: it only sees the direction it was written
        // for.
        var guard = new Regex(
            @"(?i)\b(?:frame|frames|pins?|pinning)\b(?:\W+\w+){0,8}\W+\b"
            + Softened + @"\b"
            + @"|(?i)\b" + Softened
            + @"\b(?:\W+\w+){0,8}\W+\b(?:frame|frames|pins?|pinning)\b");
        Assert.Matches(guard, "The frame is painless.");
        Assert.Matches(guard, "Putting the pins in does not hurt.");

        // The two sentences /review used to beat the first version.
        Assert.Matches(guard, "The pins do not hurt.");
        Assert.Matches(guard, "You will not feel the pins going in.");

        Assert.DoesNotMatch(guard, section);
    }

    // ------------------------------------------------------ the result section

    [Fact]
    public void TheResultSectionSaysStayingTheSameSizeCountsAsSuccess()
    {
        // The best content on the page, and the one thing a reader cannot
        // infer. Froedtert: "The goal of radiosurgery is tumor control, which
        // is defined as stable tumor size or tumor shrinkage." Cleveland
        // Clinic: "Many noncancerous tumors stop growing immediately but don't
        // get smaller."
        //
        // A reader who believes shrinking is the only success reads "no change"
        // on their first report as a failure.
        var section = PlainOf(ResultHeading);

        Assert.Matches(@"(?i)\btumor control\b", section);
        Assert.Matches(@"(?i)\bstays the same size or gets smaller\b", section);
        Assert.Matches(@"(?i)\bBoth count\b", section);
        Assert.Matches(@"(?i)\bno change.{0,40}may be the good outcome\b", section);
    }

    [Fact]
    public void ThePageNeverPromisesTheTumorWillShrinkOrDisappear()
    {
        // The other half of the section above, and the direction §12.12 calls
        // the more dangerous one. Froedtert says abnormalities "dissolve
        // gradually, eventually disappearing", which is true of some vascular
        // malformations and is not a promise this page may make about a tumor.
        // THE MODAL REQUIREMENT IS GONE, because /review beat it three ways and
        // two of them had no modal at all. §12.8 (WI-525): check the AGENT, not
        // the modal — a sentence does not need a modal to be a promise, it only
        // needs the tumor as the subject of a disappearing verb.
        //
        // The page's own correct sentence ("stays the same size or gets
        // smaller") is REDACTED rather than allowlisted (§12.8, WI-527).
        string[] allowed =
        [
            "it means the tumor stays the same size or gets smaller",
            "Cancerous ones more often become stable or get smaller",
            "stops growing right away and often does not get smaller",
            // The questions list, which offers three goals rather than
            // promising one: "What is this meant to do: stop it growing,
            // shrink it, or help a symptom?"
            "stop it growing, shrink it, or help a symptom",
        ];

        var scanned = Redact(Plain, allowed);
        var guard = new Regex(
            @"(?i)\b(?:tumors?|it|they|lesions?)\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:shrinks?|shrank|shrunk|disappears?|disappeared|go(?:es)? away|"
            + @"went away|dissolves?|melts?|vanishes|vanish)\b"
            + @"|(?i)\b(?:gets? rid of|destroys?|kills? off|wipes? out)\b(?:\W+\w+){0,3}\W+"
            + @"\btumors?\b");

        Assert.Matches(guard, "The tumor will shrink over the following months.");
        Assert.Matches(guard, "It gets rid of the tumor.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "Most tumors shrink over the following months.");
        Assert.Matches(guard, "It will make the tumor go away.");
        Assert.Matches(guard, "The tumor melts away slowly.");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page promises an outcome it cannot promise: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheResultSectionIsNotUndoneBySomethingAppendedAfterIt()
    {
        // §12.8 (WI-521): a correctly-pinned reassurance survives a sentence
        // appended after it, so the pin is END-anchored on the section's LAST
        // sentence. The failure mode here is a closing line that turns "no
        // change is good" back into "but ask them whether it should have
        // shrunk".
        var section = PlainOf(ResultHeading).TrimEnd();
        Assert.Matches(
            @"(?i)goes through the words on the report\.?\s*$", section);

        // AND THE REASSURANCE'S POSITION, because the pinned last sentence is a
        // ROUTING line rather than the reassurance. /review inserted "If it has
        // grown instead, that is a different conversation and a harder one."
        // immediately before it and the guard stayed green while the section
        // now delivered its fear after its reassurance. So the reassurance has
        // to be in the closing run too.
        var sentences = CuratedPage.SentencesOf(section);
        var tail = string.Join(" ", sentences.TakeLast(4));
        Assert.Matches(@"(?i)\bno change.{0,60}may be the good outcome\b", tail);

        // The canary: a fear sentence inserted between the reassurance and the
        // routing line pushes the reassurance out of the last four.
        var planted = string.Join(" ", sentences.SkipLast(1)
            .Append("If it has grown instead, that is a different conversation and a harder "
                + "one.")
            .Append(sentences[^1]));
        Assert.DoesNotMatch(@"(?i)\bno change.{0,60}may be the good outcome\b",
            string.Join(" ", CuratedPage.SentencesOf(planted).TakeLast(3)));
    }

    // ------------------------------------------ radiation necrosis is routed

    [Fact]
    public void RadiationNecrosisIsNamedAndRoutedRatherThanExplainedHere()
    {
        // §12.10, and this is the item's clearest instance of it.
        // /tests/follow-up-scans (WI-521) owns radiation necrosis, reached its
        // answer from sources that disagree, and is the stronger version. This
        // page names the thing so the word is not a shock on a later report,
        // links, and states none of the sibling's material.
        var section = PlainOf(LaterHeading);

        Assert.Matches(@"(?i)\bradiation necrosis\b", section);
        Assert.Contains("/tests/follow-up-scans#much-later", RawSection(LaterHeading),
            StringComparison.Ordinal);
        Assert.Matches(@"(?i)\bThat page owns it\b", section);

        // The sibling's own material is read rather than assumed, and none of
        // its distinguishing claims may appear here.
        var sibling = Sibling("tests", "follow-up-scans.md");
        Assert.Contains("radiation necrosis", sibling, StringComparison.OrdinalIgnoreCase);

        // WIDENED AFTER /review BEAT IT TWICE, and the first was an adjacency
        // hole that let the sibling's own figure be republished with two words
        // inserted: `\beight months\b` misses "eight to twelve months".
        var banned = new Regex(
            @"(?i)\beight\b(?:\W+\w+){0,3}\W+\bmonths\b|(?i)\bon average\b"
            + @"|(?i)\btissue\b(?:\W+\w+){0,6}\W+\b(?:break down|breaks down|die|dies|dying|"
            + @"die off|dead)\b"
            + @"|(?i)\bhow likely it is varied\b");

        Assert.Matches(banned, "It turned up around eight months afterward.");
        Assert.Matches(banned, "Tissue inside the treated area can break down and die.");

        // The two sentences /review used to beat the first version.
        Assert.Matches(banned, "It usually turns up about eight to twelve months later.");
        Assert.Matches(banned, "Tissue in the treated area can die off and look like a tumor.");

        var hits = banned.Matches(PlainOf(LaterHeading)).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "this page has started explaining radiation necrosis, which /tests/follow-up-scans "
            + "owns and states more carefully: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheBrainSwellingClaimKeepsItsTwoTimings()
    {
        // Cleveland Clinic, verbatim: "Possible brain swelling can happen
        // immediately after, or months after, the procedure." Dropping the
        // second half is the §12.6 qualifier-cut, and it is the half that
        // matters — a reader six months out with new symptoms who believes
        // swelling only happens on the day has nowhere to put it.
        var section = PlainOf(LaterHeading);
        Assert.Matches(@"(?i)\beither soon afterward or months later\b", section);
        Assert.Matches(@"(?i)\bsteroids?\b", section);
        Assert.Contains("/treatments/steroids", RawSection(LaterHeading), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------ escalation

    [Fact]
    public void ThePageFilesEverySymptomWhereTheCorpusFilesIt()
    {
        // §12.8 (WI-525): read the sibling to learn where the corpus files a
        // symptom, then assert THIS page files it there. The shared block is
        // composed rather than restated, so the tiers cannot drift.
        CuratedPage.AssertEscalationTiers(Page, Slug, LaterHeading);
    }

    [Fact]
    public void ThePinSiteRuleIsPresentAndIsNotDowngraded()
    {
        // The one tier this page invents, and it is the only one it may:
        // [ESCALATION] has no pin sites in it because no other page in the
        // corpus has pins. Cleveland Clinic files hot, draining or feverish pin
        // sites under "contact your healthcare provider".
        //
        // THE TIER WORD IS ASSERTED, NOT JUST THE BULLET. /review found the
        // first version of this page's rule had NO TIMING WORD AT ALL — "Call
        // your team if a pin site turns hot…" — while /treatments/craniotomy
        // files a warm wound, a leaking wound and a fever as **same day**, and
        // the composed block twenty lines below files a post-procedure fever
        // higher still. So the page invented a tier LOWER than the corpus files
        // the identical symptoms, which is the under-triage direction §12.8
        // (WI-511) calls the more dangerous one, and the guard could not see it
        // because there was nothing to downgrade. §12.8 (WI-525), one level up:
        // a membership check is not a tier check, and neither is a
        // downgrade-phrase check on its own.
        //
        // The rule also MOVED. §12.10 says a page's own escalation line goes
        // BENEATH the shared block, not in a different section two screens up.
        var raw = CuratedPage.Flatten(
            CuratedPage.ReaderText(RawSection(LaterHeading)));

        Assert.Matches(@"(?i)\bhot to the touch\b", raw);
        Assert.Matches(@"(?i)\bcloudy or bad smelling\b", raw);
        Assert.Matches(@"(?i)\bfever\b", raw);

        // The tier, in the corpus's own words, and the sibling is READ rather
        // than hard-coded so a change there goes red here (§12.8, WI-525).
        Assert.Matches(@"(?i)\bCall your team the same day\b", raw);

        var craniotomy = Sibling("treatments", "craniotomy.md");
        Assert.Matches(@"(?i)\bWhen to call the team the same day\b", craniotomy);
        Assert.Matches(@"(?i)\bAnything leaks from the wound\b", craniotomy);

        // And the page says WHY its own rule exists, so a later editor does
        // not delete it as a duplicate of the block.
        Assert.Matches(@"(?i)\bthe pin sites are this page's own rule\b", raw);
        Assert.Matches(
            @"(?i)\bwritten for people having chemotherapy and\s+for the weeks after an "
            + @"operation\b", raw);

        var downgrade = new Regex(
            @"(?i)\bpin sites?\b(?:\W+\w+){0,30}\W+"
            + @"\b(?:can wait|no rush|no need to rush|not urgent|until the morning|"
            + @"at your next (?:appointment|visit)|nothing to worry about|nothing serious|"
            + @"rarely serious|settle on their own|settle down on their own)\b"
            + @"|(?i)\b(?:can wait|no rush|no need to rush|not urgent|until the morning|"
            + @"no need to call|nothing serious|worth mentioning at your next)\b"
            + @"(?:\W+\w+){0,30}\W+\bpin sites?\b");

        Assert.Matches(downgrade, "A sore pin site can wait until the morning.");
        Assert.Matches(downgrade, "There is no rush about a pin site.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(downgrade, "A sore pin site is usually nothing serious.");
        Assert.Matches(downgrade,
            "Most pin sites settle on their own, so there is no need to rush.");
        Assert.Matches(downgrade, "A hot pin site is worth mentioning at your next visit.");

        Assert.DoesNotMatch(downgrade, raw);
    }

    [Fact]
    public void TheEscalationBlockIsComposedRatherThanRestated()
    {
        // §12.8 (WI-527): a contract item no test names is not a contract item.
        // Deleting the directive has to break something.
        Assert.Matches(@"(?m)^\[ESCALATION\]\s*$", Page);

        // `ReaderText` of the composed page, not the composed page itself: the
        // front matter carries this page's own rulings and they quote the
        // directive in order to explain it. A ban run over the whole composed
        // text fires on the comment that documents the decision, which is a
        // guard red on a correct page (§12.8).
        var composed = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Composed(Page, Slug)));
        Assert.Contains("Call an ambulance for any of these", composed, StringComparison.Ordinal);
        Assert.DoesNotContain("[ESCALATION]", composed, StringComparison.Ordinal);
    }

    // ------------------------------------------------------- the caregiver half

    [Fact]
    public void TheCaregiverSectionComposesTheBlockAndAddsOnlyWhatIsTrueHere()
    {
        // §12.7. The block is composed, the page's own half is written to the
        // caregiver in the second person, and it does not restate the block.
        Assert.Matches(@"(?m)^\[CAREGIVER\]\s*$", Page);

        var section = PlainOf(CaregiverHeading);
        Assert.Matches(@"(?i)\byou are the ride\b", section);
        Assert.Matches(@"(?i)\bClear the whole day\b", section);
        Assert.Matches(@"(?i)\bthe wait is the hard part\b", section);

        // §12.8 (WI-525, WI-526): the shingle check cannot see a restatement
        // that shares no eight-word run, so the page's own claims are counted.
        // Paragraph-leading bold, because emphasis inside a paragraph is not a
        // list item (§12.8, WI-528).
        var claims = Regex.Matches(RawSection(CaregiverHeading), @"(?m)^\*\*").Count;
        Assert.True(claims == 6,
            $"the caregiver section makes {claims} claims rather than six. Adding one without "
            + "checking it against [CAREGIVER] is how the block gets restated in different "
            + "words ten lines below itself.");
    }

    [Fact]
    public void TheCaregiverSectionDoesNotRepeatTheBlocksOwnSentences()
    {
        // §12.8 (WI-510): a heading check is not enough; word shingles over the
        // composed section catch a restatement in any shape.
        var blockWords = Regex.Matches(
                CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.EscalationBlock)
                    + " " + CuratedPage.ReaderText(
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
            "the page's caregiver half restates the shared blocks:\n  "
            + string.Join("\n  ", shared.Take(4)));
    }

    // -------------------------------------------------- corpus-wide housekeeping

    [Fact]
    public void NoUnsourcedShareOfPeopleAppearsAnywhereOnThePage()
    {
        // PORTED FROM WI-524 AND WIDENED BY WI-527, and it should have been
        // here from the first draft. /review found SEVEN unsourced shares on
        // this page in one pass — "Most people have a quiet few days", "most
        // are told to go easy", "Hair loss is not usual here", "More often it
        // is because a new spot", "three people build your plan" — none of
        // which any other guard in this suite could see, because every one of
        // them is a claim about how many people something happens to and this
        // suite had no word for that.
        //
        // §12.8 (WI-524): the exemptions are a SHORT NAMED ALLOWLIST with the
        // source quote that earns each one, not a pattern hole. §12.8 (WI-527):
        // a quantifier guard that knows halves and thirds knows almost no
        // fractions, and adjacency is a one-word door.
        string[] allowed =
        [
            // Cleveland Clinic, verbatim: "Many people take a nap during the
            // procedure."
            "plenty of people do",
            // ACS, verbatim, on SRS: "SRS typically delivers the whole
            // radiation dose in a single session."
            "One day, start to finish, for most people",
            // Cleveland Clinic, verbatim: "Cancerous tumors typically become
            // stable or get smaller over a period of weeks to months. Many
            // noncancerous tumors stop growing immediately but don't get
            // smaller."
            "Cancerous ones more often become stable or get smaller",
        ];

        var scanned = Redact(Plain, allowed);

        // A BARE COUNT IS NOT A SHARE. The first version put CountWord in
        // this alternation and fired on ASCO's "one or two of them" (a
        // count of metastases) and on "One hospital tells people" -- which
        // is an ATTRIBUTION, the exact form §12.8 (WI-507) requires. Only
        // genuine share words, and "of them" is out because it points at
        // whatever the last noun was rather than at people.
        const string Share =
            @"(?:most|nearly all|almost (?:all|everybody|everyone|nobody)|"
            + @"the (?:great |vast )?majority|a minority|plenty of|a lot of|hardly any|"
            + @"a few|many|" + Fraction + @"|" + CountWord + @" in (?:every )?"
            + CountWord + @"|" + CountWord + @" out of " + CountWord + @")";

        var guard = new Regex(
            @"(?i)\b" + Share + @"\b(?:\W+\w+){0,3}\W+"
            + @"\b(?:people|patients|readers|men|women)\b"
            + @"|(?i)\b(?:it happens to|this happens to)\b(?:\W+\w+){0,3}\W+\b" + Share + @"\b"
            + @"|(?i)\bis (?:not )?(?:usual|common|unusual|uncommon|rare)\b");

        // Canaries, including the exact shapes /review found on the page.
        Assert.Matches(guard, "Most people have a quiet few days.");
        Assert.Matches(guard, "Roughly a fifth of people notice it.");
        Assert.Matches(guard, "Hair loss is not usual here.");
        Assert.Matches(guard, "It happens to about one in ten people.");
        Assert.Matches(guard, "Plenty of patients find it hard.");

        // The two CORRECT sentences the first version failed.
        Assert.DoesNotMatch(guard, "after an operation for one or two of them");
        Assert.DoesNotMatch(guard,
            "One hospital tells people they should be able to get back to work.");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an unsourced share has reached the page. Either cite it in the front matter and "
            + "add it to the allowlist WITH the source quote, or rewrite it as a conditional: "
            + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageMakesAtMostOneClaimOfPrimacy()
    {
        // §12.8 (WI-509, WI-526): a uniqueness claim goes stale against the
        // other sections, and three in one draft is already a recorded defect.
        // /review found FIVE on this page — "the one worth knowing by heart",
        // "the part most worth understanding before you go home", "the
        // least-explained part of the day and the part most worth preparing
        // for", "almost nobody is warned about it", "the part no other page on
        // this site covers".
        //
        // Banned at ZERO for the general shape, with the ONE legitimate claim
        // pinned separately (§12.8, WI-528: an allowance of one is an allowance
        // of one more).
        const string Kept =
            "this page spends time on it because most others do not";

        var scanned = Redact(Plain, [Kept]);
        var guard = new Regex(
            @"(?i)\bthe (?:one|single most|most) \w+(?:\W+\w+){0,4}\W+\bon this page\b"
            + @"|(?i)\bmost worth (?:understanding|knowing|reading|preparing)\b"
            + @"|(?i)\bworth knowing by heart\b"
            + @"|(?i)\bmatters more than anything else\b"
            + @"|(?i)\balmost nobody is (?:warned|told)\b"
            + @"|(?i)\bthe (?:one|only) thing (?:on this page|to watch for)\b");

        Assert.Matches(guard, "This is the part most worth understanding.");
        Assert.Matches(guard, "It is the one worth knowing by heart.");
        Assert.Matches(guard, "That is the single most useful thing on this page.");
        Assert.Matches(guard, "Almost nobody is warned about it.");

        // And the kept one is still there, so this is not a ban standing in
        // for a deleted claim (§12.14).
        Assert.Contains(Kept, Plain, StringComparison.OrdinalIgnoreCase);

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page makes more than one claim about which of its own sections matters most, "
            + "and they go stale against each other: " + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageMakesNoGuaranteeAboutWhatSomebodyElseWillDo()
    {
        // §12.8 (WI-521, WI-522), recorded twice and found here a third time:
        // "Ask somebody to come and tell you where the plan is up to, AND THEY
        // WILL." A page cannot promise what a stranger's hospital does.
        // NARROWED. A bare `will` within six words of a team noun fired on
        // four correct sentences on the first build, including the
        // questions list and "Ask your team, before you leave, what they
        // will be looking for" -- which is the page telling the reader to
        // ASK, the opposite of a guarantee. The defect is a promise that
        // ENDS a sentence or that asserts somebody else's behaviour as
        // settled fact.
        var guard = new Regex(
            @"(?i)\band (?:they|the nurses?|your team|the team) will\s*[.!]"
            + @"|(?i)\bthe nurses have answers\b"
            + @"|(?i)\b(?:they|your team|the team|your center|the center)\s+will\s+"
            + @"(?:always|definitely|certainly|make sure|see to it)\b"
            + @"|(?i)\bsomebody will (?:come|be there|sit with you)\b");

        Assert.Matches(guard, "Ask somebody to tell you where it is up to, and they will.");
        Assert.Matches(guard, "Your team will always sit down with you afterward.");
        Assert.Matches(guard, "The nurses have answers for all three.");
        Assert.Matches(guard, "Somebody will come and tell you.");

        // The correct sentences the first version failed.
        Assert.DoesNotMatch(guard,
            "Ask your team, before you leave, what they will be looking for.");
        Assert.DoesNotMatch(guard,
            "If they took the relaxing medicine they will be groggy afterward.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page guarantees somebody else's behaviour: " + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageDoesNotRestateWhatAnotherPageAlreadyOwns()
    {
        // §12.8 (WI-521, WI-530). A corpus-wide shingle run, with the short
        // named list of sentences that are shared on purpose.
        // ONE ENTRY, AND IT IS THE ONE THAT ACTUALLY FIRES. /review computed
        // the shingle set and found the first version's first entry — the
        // corpus's first-seizure wording — EXEMPTED NOTHING and never could:
        // `Shingles()` reads the RAW page, where the seizure rule is the eleven
        // characters `[ESCALATION]`. §12.8 (WI-530): a redaction you have not
        // watched fire is dead code that reads like care. Deleted.
        //
        // What remains is live (one shingle, against /treatments/craniotomy),
        // and it is deliberately shared: §12.10 says a claim load-bearing on a
        // page AND on its door gets one wording rather than two strengths.
        // NO PUNCTUATION IN THE ENTRY. `Shingles()` tokenises to lowercase
        // words, and the helper asks whether the ALLOWED string contains
        // the shingle -- so an entry carrying the original commas can
        // never contain one, and exempts nothing. The first version of
        // this entry failed for exactly that reason, which is the same
        // shape as the dead entry deleted above.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug,
            "it suits a small target with an edge that can be drawn and it takes nothing out "
            + "so there is no tissue for the laboratory");
    }

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        // §12.8 (WI-511, WI-529, WI-530). Run on `Plain`, not `Body`: the
        // description is the first paragraph the reader meets and a body-scoped
        // scan is blind to it (§12.8, WI-528).
        var offenders = CuratedPage.BritishForms
            .Where(form => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(form)}",
                RegexOptions.None))
            .Where(form => !CuratedPage.BritishFormExemptions.Any(
                ex => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(ex)}")
                      && ex.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "British forms on a page written for a US reader: " + string.Join(", ", offenders));
    }

    [Fact]
    public void ThePageNeverCharacterisesAnOutcomeOrMinimisesTheDay()
    {
        // The two shared lists, run over reader text rather than raw source
        // (§12.8, WI-509).
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertNeverMinimises(Plain, Slug);
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        // §12.2 item 8, and §12.8 (WI-513): a fabricated title is a fabricated
        // citation on the reader's screen, because titles render as the visible
        // link text under "Sources" and no test can check the words themselves.
        // What can be checked is that none is missing.
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        var titles = Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count;
        var accessed = Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count;

        Assert.True(urls >= 6, $"only {urls} sources are cited");
        Assert.Equal(urls, titles);
        Assert.Equal(urls, accessed);
    }

    // ------------------------------------------------------------- the tooltip

    [Fact]
    public void ThePageSaysTheTermInProseAndSuppressesOnlyItsOwnDefinition()
    {
        // §12.8 (WI-512): a suppression is only meaningful if the word is there
        // to suppress, so assert the term appears in prose BEFORE asserting its
        // tooltip does not.
        Assert.Matches(@"(?i)\bstereotactic radiosurgery\b", Body);
        Assert.Matches(@"!%stereotactic radiosurgery%", Page);

        // And nothing else is suppressed. A first draft carried five more
        // markers and every one was a no-op or wrong: the page defines none of
        // those terms, "fractionation" never matches this page's
        // "fractionated" because the matcher is whole-word, and radiation
        // necrosis is exactly the tooltip a reader wants where this page names
        // it and routes.
        var markers = Regex.Matches(Page, @"!%(.+?)%").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(["stereotactic radiosurgery"], markers);
    }

    [Fact]
    public void SuppressingTheTermHereLeavesTheGlossaryEntryReachableElsewhere()
    {
        // §12.8 (WI-510, WI-521): a suppressed entry that fires nowhere else is
        // decoration, and this state has to be pinned rather than believed.
        // §12.8 (WI-519): grep the term AND its aliases, and remember that
        // linking a word turns its tooltip off — so a page that only links the
        // words does not count.
        var reachable = new List<string>();
        foreach (var (slug, text) in CuratedPage.AllPages())
        {
            if (slug.EndsWith("stereotactic-radiosurgery.md", StringComparison.Ordinal))
            {
                continue;
            }

            var reader = CuratedPage.Flatten(CuratedPage.ReaderText(text));

            // Strip whole markdown links first: GlossaryMarker skips a literal
            // with a link ancestor, so words inside `[...](...)` never fire.
            reader = Regex.Replace(reader, @"\[[^\]]*\]\([^)]*\)", " ");

            if (Regex.IsMatch(reader, @"(?i)(?<![\w-])stereotactic radiosurgery(?![\w-])")
                && !Regex.IsMatch(text, @"!%stereotactic radiosurgery%"))
            {
                reachable.Add(slug);
            }
        }

        // A MARKER IN A SHARED BLOCK SUPPRESSES THE TERM EVERYWHERE THAT
        // BLOCK IS COMPOSED. §12.8 (WI-510) records exactly this leak and
        // the first version of this test could not see it: it read each
        // PAGE's own text for the marker, so the harness put
        // `!%stereotactic radiosurgery%` into blocks/escalation.md and
        // every assertion below stayed green while the entry had been
        // switched off on eighteen hubs at once.
        var leaks = CuratedPage.AllPages()
            .Concat(CuratedPage.SharedSources())
            .Where(p => p.Slug.Contains("blocks/", StringComparison.Ordinal)
                        || p.Slug.Contains("glossary/", StringComparison.Ordinal))
            .Where(p => Regex.IsMatch(p.Text, @"!%"))
            .Select(p => p.Slug)
            .ToList();

        Assert.True(leaks.Count == 0,
            "a !%term% marker inside a shared block or a glossary entry suppresses that "
            + "term on every page that composes it, which is a site-wide change made from "
            + "a file nobody reads as a page: " + string.Join(", ", leaks));

        // THE EXACT SET, NOT A FLOOR. §12.8 (WI-528): an allowance of one
        // is an allowance of one MORE. The harness relinked the meningioma
        // door so the term fell inside a markdown link (which turns the
        // tooltip off, §12.8 WI-519), reachability dropped from four to
        // three, and `>= 3` read that as healthy.
        Assert.Equal(
            [
                "pages/tests/follow-up-scans",
                "pages/treatments/craniotomy",
                "pages/tumors/brain-metastases",
                "pages/tumors/meningioma",
            ],
            reachable.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void NoAuthoringMarkerSitsOnADirectiveLine()
    {
        // §12.8 (WI-529): the composer matches a directive only on a line of
        // its own, so `!%term%[ESCALATION]` fails OPEN — the block silently
        // stops composing and the literal renders inside a paragraph on the
        // reader's screen.
        var offenders = Regex.Matches(Page, @"(?m)^.*!%.*\[[A-Z-]+\].*$")
            .Select(m => m.Value).ToList();
        Assert.True(offenders.Count == 0,
            "an authoring marker shares a line with a block directive, which stops the block "
            + "composing: " + string.Join(" | ", offenders));
    }
}

/// <summary>
/// WI-531: the same page, against the running site. Links, fragments, the
/// composed body, and the two doors this page put on its siblings.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class StereotacticRadiosurgeryPageRenderTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/stereotactic-radiosurgery";

    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// The connection string has to be pushed in — see the twin of this
    /// comment on <c>ProtonTherapyPageRenderTests</c>. A bare factory boots
    /// fine on a machine with <c>dotnet user-secrets</c> set and throws
    /// "Connection string 'BrainHarbor' not found" on a CI runner that has
    /// none, so this is a defect no local run of any kind can reach.
    /// </summary>
    public StereotacticRadiosurgeryPageRenderTests(WebApplicationFactory<Program> raw) =>
        factory = raw.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private static string Page => CuratedPage.Read("treatments", "stereotactic-radiosurgery.md");

    [Fact]
    public async Task ThePageIsServedAndCarriesNoUnresolvedDirectiveOrMarker()
    {
        // §12.8 (WI-515): `slug:` routes nothing — ContentStore routes by file
        // path — so a "served at its URL" test cannot be broken by any in-file
        // mutation. What IS reachable is the composed output, which is where a
        // mistyped directive or a leaked authoring marker shows up.
        var client = factory.CreateClient();
        var response = await client.GetAsync(Url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("[ESCALATION]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotContain("%%", html, StringComparison.Ordinal);
        Assert.Contains("Nothing is cut", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/treatments/radiation-therapy",
            "/tests/follow-up-scans",
            "/tumors/brain-metastases",
            "/treatments/craniotomy",
            "/treatments/proton-therapy",
            "/get-help-now");
    }

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists()
    {
        // §12.8 (WI-513): `AssertLinksResolve`'s regex stops at the `#`, so a
        // link to a heading that does not exist comes back a healthy 200 while
        // landing the reader at the top of a long page.
        await CuratedPage.AssertFragmentLinksResolve(factory.CreateClient(), Url);
    }

    [Fact]
    public async Task TheGlossaryTooltipIsSuppressedOnTheRenderedPage()
    {
        // §12.8 (WI-530): the markdown contains neither the tooltip nor its
        // position, so only the rendered page can show whether a definition
        // landed in the right paragraph — or, here, that it landed nowhere.
        var html = await factory.CreateClient().GetStringAsync(Url);
        Assert.DoesNotContain("def-stereotactic-radiosurgery", html, StringComparison.Ordinal);

        // And the entry that IS wanted here did fire, in the section that owns
        // the question. Model the matcher, not the string: `GlossaryMarker`
        // matches whole words, so a bare IndexOf on the term would pass on
        // "radiation necrosis is" inside a link and prove nothing.
        Assert.Contains("def-radiation-necrosis", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDoorsOnTheSiblingPagesAreAppendedSentencesNearWhatTheyWereAppendedTo()
    {
        // §12.8 (WI-526): a door needs to be NEAR the sentence it was appended
        // to, or replacing the sibling's paragraph outright leaves the guard
        // green. §12.8 (WI-530): a door is prose on the page it lands on, so it
        // is diffed against that page and not only against this one.
        (string Slug, string Pinned, string Door)[] doors =
        [
            ("tests/follow-up-scans",
                "Ask your own radiation team what it means for the treatment you had.",
                "/treatments/stereotactic-radiosurgery"),
            ("tumors/brain-metastases",
                "It can often be given in one visit, or in a small number.",
                "/treatments/stereotactic-radiosurgery"),
            ("tumors/meningioma",
                "Radiation therapy](/treatments/radiation-therapy) covers what that involves",
                "/treatments/stereotactic-radiosurgery"),
            ("treatments/craniotomy",
                "Ask your team whether it applies to you rather than asking for it.",
                "/treatments/stereotactic-radiosurgery"),
            ("treatments/radiation-therapy",
                "Nothing is\n  cut, in spite of the name.",
                "/treatments/stereotactic-radiosurgery"),
        ];

        foreach (var (slug, pinnedRaw, door) in doors)
        {
            var parts = slug.Split('/');

            // FLATTENED before indexing. The corpus is hard-wrapped, so a
            // pinned sentence routinely has a newline inside it and a raw
            // IndexOf reports it missing — the same trap §12.8 (WI-511) records
            // for the gates, here in a positional test. Flattening both sides
            // keeps the character distance meaningful.
            var text = CuratedPage.Flatten(CuratedPage.Read(parts[0], parts[1] + ".md"));
            var pinned = CuratedPage.Flatten(pinnedRaw);

            var pinnedAt = text.IndexOf(pinned, StringComparison.Ordinal);
            Assert.True(pinnedAt >= 0,
                $"{slug} no longer contains the sentence the door was appended to, so the door "
                + "may have replaced a paragraph rather than been added to one.");

            var doorAt = text.IndexOf(door, StringComparison.Ordinal);
            Assert.True(doorAt >= 0, $"{slug} has lost its door to this page");

            // 800 characters is about ten wrapped lines, and the number is
            // chosen against what this guard is for: §12.8 (WI-526) found a pin
            // sitting 190 LINES from its door, which is thousands of
            // characters. The craniotomy door is a whole paragraph and its URL
            // falls at the end of it, so a 400-character window failed on a
            // correct page — which is the §12.8 rule about re-running the
            // correct page after widening, applied to the widening itself.
            Assert.True(Math.Abs(doorAt - pinnedAt) < 800,
                $"{slug}'s door has drifted {Math.Abs(doorAt - pinnedAt)} characters from the "
                + "sentence it was appended to, so this guard no longer proves it is an "
                + "addition rather than a replacement.");
        }
    }

    [Fact]
    public async Task TheSiblingThatSendsReadersHereDoesNotContradictThisPageOnTheFrame()
    {
        // §12.8 (WI-530): a door added to a sibling is prose on that sibling,
        // and it can undo a previous item's fix. /tumors/brain-metastases used
        // to say focused radiation is given "with a mask made to hold your head
        // still", which takes the side this page refuses to take. It says
        // "either a mask or a light frame" now, and this test is what keeps it
        // that way.
        var sibling = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "brain-metastases.md")));

        // BOTH methods named, and the frame not softened into a word that
        // hides the pins: /review was right that "a light frame" was the first
        // thing a metastasis reader learned about it, with the reassuring
        // adjective arriving and the pins not arriving at all.
        Assert.Matches(@"(?i)either by a mask or by a light frame pinned to your scalp",
            sibling);
        Assert.Matches(@"(?i)under\s+local anesthetic", sibling);
        Assert.DoesNotMatch(@"(?i)with a mask made to hold your head still", sibling);

        await Task.CompletedTask;
    }
}
