using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-532: <c>/treatments/targeted-therapy</c>. A §12.8 library page, and the
/// one whose sources are the richest in numbers the site may not print — two
/// FDA approval pages, both of which carry hazard ratios, response rates,
/// milligrams and months in their first three paragraphs.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * SELLING A DRUG ON THE WRONG MEASURE. This is the page's whole spine.
///     Bevacizumab improves the scan and the steroid dose and did not help
///     people live longer; vorasidenib's trial measured time before growth and
///     time to the next treatment, not survival; dordaviprone has an
///     ACCELERATED approval with the confirmatory trial still running; and ACS
///     says of everolimus, in its own voice, that it is "not clear if it can
///     help people with these tumors live longer". Four drugs, four different
///     answers to "did it work", and a page that flattens them has done the
///     opposite of its job.
///   * PUBLISHING THE MANUFACTURER'S SURGERY INTERVAL. §12.2 item 6 bans
///     avastin.com by name, and it is the ONLY source in the reachable set that
///     gives "at least 28 days before or after surgery". §12.14: banning the
///     citation is not keeping the claim.
///   * DROPPING THE VORASIDENIB EXCLUSION. The trial excluded anyone who had
///     had chemotherapy or radiation. The dossier's eligibility bullet does not
///     say so; the FDA page does, verbatim.
///   * TELLING A READER WITH NO TARGET THAT THE LIST IS CLOSED.
///   * BURYING THE LIVER SIGNS. Jaundice, dark urine, appetite loss and
///     right-upper-belly pain are the reason vorasidenib's blood tests exist.
///
/// Every guard starts from §12.8's accumulated lessons rather than
/// rediscovering them, and specifically from the eight that WI-531's break
/// harness found could not fail: determiner lists include <c>the</c>, position
/// is asserted rather than presence, number bans run page-wide rather than
/// section-scoped, reachability is an exact set rather than a floor, and every
/// regex that touches a line break uses <c>\r?\n</c>.
/// </summary>
public sealed class TargetedTherapyPageContentTests
{
    private const string Slug = "treatments/targeted-therapy";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a targeted drug?";
    private const string ReportHeading = "Why does my tumor's report decide this?";
    private const string WhichHeading = "Which drug goes with which result?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What is it like to be on one?";
    private const string WorkedHeading = "What would it mean to say this worked?";
    private const string SideHeading = "Side effects";
    private const string SurgeryHeading = "If you are having surgery, bevacizumab has its own rule";
    private const string StopsHeading = "What if it stops working?";
    private const string NeedHeading = "What you need first";
    private const string CaregiverHeading = "For the person alongside them";
    private const string DecidesHeading = "Who decides, and how will I hear?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("treatments", "targeted-therapy.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// flattens, so anything counting a list MARKER or reading a PARAGRAPH has
    /// to cut the markdown itself (§12.8, WI-526 and WI-528).
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

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

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
        + @"tenths?|twentieths?|hundredths?|fraction)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): presence first, then position — `List.IndexOf`
        // returns -1 and -1 is less than everything.
        //
        // The order is taken from the STANDARD (§12.8, WI-510, and WI-531's own
        // blocker, where a first draft listed the slots in the page's order and
        // made the order assertion vacuous): slot 7, then the caregiver
        // section, then 8, then 9. All three sibling treatment pages run that
        // way.
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, ReportHeading, WhichHeading, StepsHeading, LongHeading,
            FeelHeading, WorkedHeading, SideHeading, SurgeryHeading, NeedHeading,
            CaregiverHeading,
            StopsHeading, DecidesHeading, QuestionsHeading, NextHeading,
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

    // ------------------------------------------------- the spine: what "worked" means

    [Fact]
    public void TheWorkedSectionKeepsAllFourDrugsApartInsteadOfFlatteningThem()
    {
        // THE PAGE'S REASON TO EXIST. Four drugs were judged on four different
        // things, and a reader who does not know which question a trial
        // answered cannot read their own scan report.
        var section = PlainOf(WorkedHeading);

        // The framing: "it works" is several claims, not one.
        Assert.Matches(@"(?i)\bfive different things\b", section);

        // Bevacizumab — the scan and the steroid, and not living longer.
        Assert.Matches(@"(?i)\bdid not help people live longer\b", section);

        // Vorasidenib — what the trial measured, and what it did not.
        Assert.Matches(@"(?i)\bhow long people went before the tumor grew\b", section);
        Assert.Matches(@"(?i)\bLiving longer was not one of the\s+things it measured\b", section);

        // Dordaviprone — accelerated approval, confirmatory trial still running.
        Assert.Matches(@"(?i)\baccelerated approval\b", section);
        Assert.Matches(@"(?i)\bThat trial has not\s+reported\b", section);

        // Everolimus — ACS's own words.
        Assert.Matches(@"(?i)\bnot clear whether it helps\s+people with these tumors live longer\b",
            section);

        // And the action, because a section that names four problems and no
        // answer is the §12.8 (WI-506) failure.
        Assert.Matches(@"(?i)\bwhat are we hoping\s*this does, and how will we know\b", section);
    }

    [Fact]
    public void TheBevacizumabClaimUsesTheSameWordingAsTheHubThatAlreadyShippedIt()
    {
        // §12.10: one claim, one strength — and §12.8 (WI-522): when a claim is
        // load-bearing on two pages, the right answer is IDENTICAL WORDS.
        // /tumors/glioblastoma shipped this first, sourced to PMC12467656, and
        // the sibling is READ rather than hard-coded so a softening there goes
        // red here (§12.8, WI-525).
        var sibling = Sibling("tumors", "glioblastoma.md");

        const string Shared = "did not help people live longer";
        Assert.Contains(Shared, sibling, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Shared, Plain, StringComparison.OrdinalIgnoreCase);

        // The sibling's other half too, so this page cannot keep the bad news
        // and drop the reason the drug is used at all.
        Assert.Matches(@"(?i)\bmake scans look better\b", sibling);
        Assert.Matches(@"(?i)\bmake scans look better\b", Plain);

        // And the page does not overstate in the other direction either: the
        // swelling benefit is real and is stated as worth having.
        Assert.Matches(@"(?i)\bReducing swelling is\s+worth having on its own terms\b", Plain);
    }

    [Fact]
    public void ThePageNeverSaysADrugWorksWithoutSayingOnWhatMeasure()
    {
        // The hype shape for this page. A bare "it works" or "it is effective"
        // is exactly the flattening the page exists to prevent, so the words
        // are allowed only where the page is discussing what they MEAN.
        string[] allowed =
        [
            "It is worth asking because \"it works\" can mean several different things",
            "what are we hoping this does, and how will we know",
            "for as long as it is helping",
            "What would it mean to say this worked",
            "to say it worked",
        ];

        var scanned = Redact(Plain, allowed);
        // `how the drug works` is a MECHANISM sentence and the page needs
        // it -- "the drug will not help. That is not bad luck about you. It
        // is how the drug works." A lookbehind for the interrogatives is
        // what separates the two, and without it this guard fired on the
        // page's own teachable line.
        // WIDENED AFTER /review BEAT IT ELEVEN WAYS. The determiner list was
        // five literals, so "this DRUG works" walked — `this` matched and the
        // very next word killed it — and so did every drug NAME. §12.8
        // (WI-528): a drug name list is not a drug ban, and targeted cancer
        // drugs are named by suffix convention, so the shape is bannable.
        const string DrugName = @"\w{4,}(?:tinib|ciclib|parib|zomib|nib|mab|imus|prone)";
        const string NotInterrogative = @"(?<!\b(?:how|why|whether|where|when)\s)";

        var guard = new Regex(
            @"(?i)" + NotInterrogative
            + @"\b(?:it|this|that|these|those|they|the drugs?|the pills?|"
            + @"this drug|that drug|these drugs|those drugs)\s+"
            + @"(?:(?:really|genuinely|certainly|definitely|just|simply)\s+){0,2}"
            + @"(?:works?|worked|does work)\b(?!\s+best\s+(?:alongside|with|when))"
            + @"|(?i)" + NotInterrogative
            + @"\b(?:it|they|(?:this|that|these|those|the)\s+(?:drugs?|pills?|one|ones))\s+"
            + @"(?:is|are)\s+(?:\w+\s+){0,2}effective\b"
            + @"|(?i)\b" + DrugName + @"\b\s+(?:\w+\s+){0,2}(?:works?|worked|is effective)\b"
            + @"|(?i)\btargeted (?:therapy|drugs?)\s+works?\b"
            + @"|(?i)\b(?:proven|shown) to work\b"
            + @"|(?i)\bdid its job\b");

        Assert.Matches(guard, "For the right tumor, it works.");
        Assert.Matches(guard, "These drugs are effective.");
        Assert.Matches(guard, "It is proven to work.");
        Assert.Matches(guard, "They are highly effective.");

        // The eleven sentences /review used to beat the first version.
        Assert.Matches(guard, "This drug works.");
        Assert.Matches(guard, "For the right tumor, this drug works.");
        Assert.Matches(guard, "Vorasidenib works for the right tumor.");
        Assert.Matches(guard, "That drug works.");
        Assert.Matches(guard, "The pills work.");
        Assert.Matches(guard, "These pills are effective.");
        Assert.Matches(guard, "They work for most people.");
        Assert.Matches(guard, "Targeted therapy works when there is a target.");
        Assert.Matches(guard, "It is remarkably effective.");
        Assert.Matches(guard, "Bevacizumab is effective for swelling.");
        Assert.Matches(guard, "The drug did its job.");

        // The correct sentences that must survive the widening.
        Assert.DoesNotMatch(guard,
            "That is not bad luck about you. It is how those drugs work.");

        // The two CORRECT sentences the widened version failed on the
        // first build. The second is NCI's own combination sentence,
        // quoted verbatim in the front matter, and it names what the
        // drugs work best FOR -- which is the property this guard is
        // about rather than the thing it bans.
        Assert.DoesNotMatch(guard,
            "A drug taken at home moves a lot of the work onto the house.");
        Assert.DoesNotMatch(guard,
            "these drugs may work best alongside another targeted drug or alongside "
            + "chemotherapy and radiation");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page claims a drug works without naming the measure, which is the flattening "
            + "this page exists to prevent: " + string.Join(" | ", hits));
    }

    // ------------------------------------------------------------- the numbers

    [Fact]
    public void NoEfficacyFigureReachesThePageFromEitherFdaApprovalPage()
    {
        // Both cited FDA pages open with response rates, hazard ratios and
        // months. None of it is publishable (§12.4, §12.5), and a figure
        // written as a word is still a figure (§12.8, WI-511).
        // WIDENED AFTER /review BEAT IT NINE WAYS. `\bhazard ratio\b` cannot
        // match "ratios" and `\bresponse rate\b` cannot match "rates" — §12.8
        // (WI-527): an inflection is a door. The population list was three
        // literals, and there was no branch at all for a DURATION of benefit,
        // which is the shape both FDA pages actually report.
        // "of them" is a SHARE after a fraction ("a quarter of them") and an
        // ENUMERATION after a count ("One of them works differently", "One of
        // them is once a week"), and the page needs the second. So `them` is
        // allowed only behind a fraction — which is the §12.8 (WI-527) lesson
        // that a quantifier guard has to know which quantity it is looking at.
        const string People = @"(?:of|in|among)\s+(?:the\s+)?(?:people|patients|cases)";
        const string ShareOfThem = @"(?:of|among)\s+(?:the\s+)?them";

        var guard = new Regex(
            @"(?i)\bhazard ratios?\b|(?i)\bresponse rates?\b|(?i)\bmedian\b"
            + @"|(?i)\bprogression[- ]free survival\b|(?i)\boverall survival\b"
            + @"|(?i)\d+(?:\.\d+)?\s*(?:%|per ?cent)"
            + @"|(?i)\b(?:" + CountWord + @"|" + Fraction + @")\b(?:\W+\w+){0,3}\W+\b"
            + People + @"\b"
            + @"|(?i)\b" + Fraction + @"\b(?:\W+\w+){0,3}\W+\b" + ShareOfThem + @"\b"
            // A RATE NEEDS A POPULATION AFTER ITS DENOMINATOR. The first
            // version was `in (?:every )?CountWord` with nothing after it and
            // fired on "block one change in one gene", because `one` is a
            // count word; naming the round denominators instead then missed
            // "one in five people". What separates them is the noun: a rate
            // counts people, a gene does not.
            + @"|(?i)\b(?:" + CountWord + @"|" + Fraction + @")\b(?:\W+\w+){0,3}\W+"
            + @"\b(?:out of|in (?:every )?" + CountWord + @")\b(?:\W+\w+){0,2}\W+"
            + @"\b(?:people|patients|cases|them|those)\b"
            + @"|(?i)\b" + CountWord + @"\b(?:\W+\w+){0,2}\W+\bout of\b(?:\W+\w+){0,2}\W+"
            + @"\b(?:ten|twenty|fifty|a hundred|hundred)\b"
            // A duration of benefit is an efficacy figure.
            + @"|(?i)\b(?:" + CountWord + @"|a|several)\s+(?:\w+\s+){0,2}"
            + @"(?:months?|years?)\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:longer|more|before the tumor|without the tumor|later)\b"
            + @"|(?i)\b(?:held|delayed|doubled|halved)\b(?:\W+\w+){0,6}\W+"
            + @"\b(?:" + CountWord + @"|a year|a month|well over)\b"
            // A share plus a continuation verb is an efficacy claim with
            // no time unit in it: "Three quarters kept going without the
            // tumor growing."
            + @"|(?i)\b(?:" + CountWord + @"|" + Fraction + @")\b(?:\W+\w+){0,3}\W+"
            + @"\b(?:kept going|stayed on it|were still|remained|carried on)\b");

        Assert.Matches(guard, "The response rate was 46.6%.");
        Assert.Matches(guard, "The hazard ratio for PFS was 0.39.");
        Assert.Matches(guard, "Median time to next intervention was 17.8 months.");
        Assert.Matches(guard, "It worked in about half of people.");
        Assert.Matches(guard, "It improved progression-free survival.");

        // The nine sentences /review used to beat the first version.
        Assert.Matches(guard, "The hazard ratios all favored the drug.");
        Assert.Matches(guard, "The response rates in that trial were high.");
        Assert.Matches(guard,
            "In the trial, people went about two years longer before the tumor grew.");
        Assert.Matches(guard,
            "Half the people in the trial were still taking it a year later.");
        Assert.Matches(guard, "About one in five people had to stop the drug.");
        Assert.Matches(guard, "Three quarters kept going without the tumor growing.");
        Assert.Matches(guard, "The tumor shrank in a quarter of them.");
        Assert.Matches(guard, "It held the tumor still for around two years.");
        Assert.Matches(guard, "It delayed the next treatment by well over a year.");

        // The five CORRECT sentences the first widening failed. A bare
        // `them` is not a population; the preposition is what makes it one.
        Assert.DoesNotMatch(guard, "One of them works differently.");
        Assert.DoesNotMatch(guard, "It is made to block one change in one gene.");
        Assert.DoesNotMatch(guard,
            "So there are two separate questions, and people often merge them.");
        Assert.DoesNotMatch(guard, "Ask your team one thing about them.");
        Assert.DoesNotMatch(guard, "One of them is once a week.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an efficacy figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoDoseOrMilligramFigureAppearsAnywhere()
    {
        // §12.4 R1: all Gy and mg figures stay out. The FDA vorasidenib page
        // gives 40 mg once daily, and the dabrafenib page gives mg/m2 dosing.
        var guard = new Regex(
            @"(?i)\b" + CountWord + @"\s*(?:mg|milligram|mg/m)"
            + @"|(?i)\bmilligrams?\b(?:\W+\w+){0,3}\W+\b" + CountWord + @"\b"
            + @"|(?i)\bdose of\b(?:\W+\w+){0,3}\W+\b" + CountWord + @"\b");

        Assert.Matches(guard, "The dose is 40 mg once daily.");
        Assert.Matches(guard, "Take forty milligrams a day.");
        Assert.Matches(guard, "A dose of two tablets.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0, "a dose has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheManufacturersSurgeryIntervalIsNotPublishedAndTheRuleStillIs()
    {
        // §12.2 item 6 bans avastin.com BY NAME, and it is the only source in
        // the reachable set that gives a number here. §12.14: banning the
        // citation is not keeping the claim, so the claim is published at the
        // strength the permitted source supports and the page says why there is
        // no number.
        var section = PlainOf(SurgeryHeading);

        // WIDENED AFTER /review BEAT IT SEVEN WAYS, and the first beating
        // sentence REPUBLISHES THE BANNED FIGURE IN ROUND NUMBERS: "about a
        // month before an operation" is twenty-eight days, in words, past the
        // guard named for keeping it off the page. `month` was absent
        // entirely, `weeks` had no bare branch, and "ahead", "beforehand" and
        // "either side" were not in the preposition list — §12.8 (WI-520)'s
        // "five days ahead" defect, one unit over.
        const string Interval = @"(?:an?|" + CountWord + @")\s*(?:days?|weeks?|months?)";

        var guard = new Regex(
            @"(?i)\b" + Interval + @"\b(?:\W+\w+){0,6}\W+"
            + @"\b(?:before|after|ahead|beforehand|prior|either side|of surgery|"
            + @"of the operation|once the wound)\b"
            + @"|(?i)\b(?:the usual gap|the gap) is\b(?:\W+\w+){0,3}\W+\b" + Interval + @"\b"
            + @"|(?i)\bnothing for\b(?:\W+\w+){0,3}\W+\b" + Interval + @"\b"
            + @"|(?i)\bleave\b(?:\W+\w+){0,2}\W+\b" + Interval + @"\b");

        Assert.Matches(guard, "It should be stopped at least 28 days before surgery.");
        Assert.Matches(guard, "Wait twenty-eight days after the operation.");
        Assert.Matches(guard, "Stop it four weeks before surgery.");

        // The seven sentences /review used to beat the first version. The
        // first is the banned manufacturer figure, in round numbers.
        Assert.Matches(guard, "It is usually stopped about a month before an operation.");
        Assert.Matches(guard, "The usual gap is a month.");
        Assert.Matches(guard, "Nothing for a month on either side.");
        Assert.Matches(guard, "Four weeks is the usual gap on either side of surgery.");
        Assert.Matches(guard, "Bevacizumab stops four weeks ahead of surgery.");
        Assert.Matches(guard, "Leave four weeks either side of the operation.");
        Assert.Matches(guard, "You stop it four weeks beforehand.");

        // And the correct prose that must survive it: the page says "a few
        // weeks" with no count, which is what ACS supports.
        Assert.DoesNotMatch(guard, "it usually cannot be given within a few weeks of surgery");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a surgery interval has reached the page, and the only source for one is the "
            + "manufacturer site §12.2 item 6 bans by name: " + string.Join(" | ", hits));

        // The claim survives the missing number, and the page says why it is
        // missing rather than leaving a silent gap (§12.8, WI-521).
        Assert.Matches(@"(?i)\bslows down wound healing\b", section);
        Assert.Matches(@"(?i)\bwithin a few weeks of surgery\b", section);
        // The meta sentence about our own citation policy was removed after
        // /review — §12.8 (WI-531): a reader has no use for what our sources do
        // or do not give. What survives is the state of the evidence, which is
        // content (§12.8, WI-521).
        Assert.Matches(@"(?i)\bThere is no one number that fits everybody\b", section);

        // And the action, both directions, because a one-sided answer is the
        // §12.8 (WI-519) narrowing defect.
        Assert.Matches(@"(?i)\bask both ways\b", section);
        Assert.Matches(@"(?i)\bincluding a dental one\b", section);
    }

    [Fact]
    public void NoPrognosisOrSurvivalFigureAppearsAnywhere()
    {
        // §12.2 item 5 and §12.5. The dordaviprone paper carries median overall
        // survival figures in its background.
        var guard = new Regex(
            @"(?i)\blife expectancy\b|(?i)\bmedian survival\b|(?i)\bsurvival rate\b"
            + @"|(?i)\bfive[- ]year\b"
            + @"|(?i)\b(?:live|living|lived|lives)\s+(?:for\s+)?(?:" + CountWord + @")\b"
            + @"|(?i)\bstill (?:doing well|alive|here)\b"
            + @"|(?i)\badds?\s+(?:\w+\s+){0,2}(?:months|years)\b");

        Assert.Matches(guard, "Median survival is twelve months.");
        Assert.Matches(guard, "It adds several months.");
        Assert.Matches(guard, "People live two years with it.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a prognosis figure has reached the page: " + string.Join(" | ", hits));
    }

    // --------------------------------------------------------- source discipline

    [Fact]
    public void TheManufacturerSiteTheContractBansByNameIsNotCited()
    {
        // §12.2 item 6, verbatim: "replace the manufacturer's site
        // (avastin.com) as a side-effect source before publishing". The dossier
        // sources the ENTIRE bevacizumab section to it.
        var urls = CitedUrls();
        Assert.True(urls.Split('\n').Length >= 6,
            "fewer than six source URLs were read out of the front matter, so this ban is "
            + "checking almost nothing");

        foreach (var banned in new[]
                 {
                     "avastin.com", "servier.us", "cancerletter.com", "drugs.com",
                     "medlineplus.gov/druginfo", "academic.oup.com", "ascopubs.org",
                     "mdpi.com", "researchgate.net",
                 })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        // And the replacement is actually carrying the claims, so the ban is
        // not standing in for deleted content (§12.14).
        Assert.Contains("cancer.org/cancer/types/brain-spinal-cord-tumors-adults/treating/"
            + "targeted-therapy.html", urls, StringComparison.Ordinal);
    }

    [Fact]
    public void NoNciPatientPdqUrlIsCited()
    {
        // §12.1, and the PATTERN rather than the domain (§12.8, WI-528): this
        // page legitimately cites cancer.gov, so a banned-domain list
        // structurally cannot express the rule. NBK66023 is the dossier's own
        // citation here, four times over, and WI-512 found its which-drug table
        // headed with a retired CNS5 name.
        var urls = CitedUrls();
        Assert.DoesNotMatch(@"(?i)NBK66023", urls);
        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);

        // The cancer.gov URL that IS cited is the general treatment library.
        Assert.Contains("cancer.gov/about-cancer/treatment/types/targeted-therapies", urls,
            StringComparison.Ordinal);
    }

    [Fact]
    public void IvosidenibIsNotNamedAnywhere()
    {
        // The dossier recommends one sentence on it. Both of its citations are
        // on academic.oup.com, which has been dead since WI-527, so the page
        // cannot describe it — and naming a drug it cannot describe is how a
        // reader ends up asking for the wrong one (§12.8, WI-506's "ask what
        // your centre has" rule, applied to a drug name).
        Assert.DoesNotMatch(@"(?i)\bivosidenib\b", Plain);

        // The front matter DOES name it, in the ruling that explains why the
        // page does not — so what is asserted is that every mention there is
        // inside a `#` comment line. A first version replaced the word with a
        // token and then deleted the token before asserting the word was
        // absent, which could not fail under any input: §12.8 (WI-530), a
        // redaction you have not watched fire is dead code that reads like
        // care.
        var uncommented = Regex.Split(CuratedPage.FrontMatter(Page), @"\r?\n")
            .Where(l => !l.TrimStart().StartsWith('#'))
            .Where(l => l.Contains("ivosidenib", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(uncommented.Count == 0,
            "ivosidenib appears in the front matter outside a ruling comment, which means it "
            + "has become a citation rather than an explanation: "
            + string.Join(" | ", uncommented));

        // And the ruling itself is still there, so the ban is not standing in
        // for a decision nobody wrote down (§12.14).
        Assert.Matches(@"(?i)IVOSIDENIB IS NOT MENTIONED AT ALL", CuratedPage.FrontMatter(Page));
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        // §12.2 item 8, and §12.8 (WI-513): a fabricated title is a fabricated
        // citation on the reader's screen. What a test CAN check is that none
        // is missing.
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        var titles = Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count;
        var accessed = Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count;

        Assert.True(urls >= 6, $"only {urls} sources are cited");
        Assert.Equal(urls, titles);
        Assert.Equal(urls, accessed);
    }

    // ------------------------------------------------------- the scope the sources set

    [Fact]
    public void TheVorasidenibExclusionTheDossierOmitsIsOnThePage()
    {
        // The FDA page, verbatim: "Patients who received prior anti-cancer
        // treatment, including chemotherapy or radiation therapy, were
        // excluded." The dossier's eligibility bullet lists prior surgery, no
        // prior IDH inhibitor and a measurable lesion, and not this.
        //
        // It matters because a reader who has already had radiation will
        // otherwise read the vorasidenib sections on four other pages as
        // applying to them.
        Assert.Matches(
            @"(?i)\b(?:chemotherapy or radiation|radiation or chemotherapy)\b(?:\W+\w+){0,12}\W+"
            + @"\b(?:excluded|left out|not included|did not include)\b"
            + @"|(?i)\b(?:excluded|did not include|left out)\b(?:\W+\w+){0,12}\W+"
            + @"\b(?:chemotherapy or radiation|radiation or chemotherapy|had either)\b",
            Plain);
    }

    [Fact]
    public void TheBrafDisagreementIsScopedRatherThanSplit()
    {
        // §12.8 (WI-511): where two sources disagree, print the disagreement
        // and say whose answer each is. The FDA page says dabrafenib with
        // trametinib was the first systemic FIRST-LINE therapy approved for
        // pediatric low-grade glioma with a BRAF V600E change; ACS says BRAF
        // and MEK inhibitors are "typically after other treatments have been
        // tried". Both are true of different things.
        var plain = Plain;

        // BOTH halves, each scoped to whom it is true of. /review found
        // the first draft printed only the pediatric one, so an adult with
        // a BRAF V600E glioma got no setting at all -- while the front
        // matter claimed the page "scopes each" (§12.8, WI-523).
        Assert.Matches(
            @"(?i)\bFor children\b(?:\W+\w+){0,12}\W+\bapproved as the first drug\b",
            plain);
        Assert.Matches(
            @"(?i)\bFor adults\b(?:\W+\w+){0,16}\W+"
            + @"\bafter other treatments have been tried\b", plain);

        // And the page does not generalise either half into the other's scope.
        var overreach = new Regex(
            @"(?i)\bBRAF\b(?:\W+\w+){0,12}\W+\bfirst (?:choice|treatment|thing)\b"
            + @"|(?i)\b(?:always|usually) (?:tried|given) (?:first|before)\b"
            + @"|(?i)\bonly after everything else\b");

        Assert.Matches(overreach, "BRAF drugs are the first treatment to try.");
        Assert.Matches(overreach, "They are only after everything else has failed.");
        Assert.DoesNotMatch(overreach, plain);
    }

    [Fact]
    public void TheGateSaysNoTargetIsInformationRatherThanADoorClosing()
    {
        // The single most damaging thing this page could do to a reader whose
        // report shows nothing: let them read it as the end of the list.
        var section = PlainOf(ReportHeading);

        Assert.Matches(@"(?i)\bthat is information rather than a door\s+closing\b", section);
        Assert.Matches(
            @"(?i)\bnothing about bevacizumab, which is not matched to one\b", section);
        Assert.Matches(
            @"(?i)\bnothing about\s+surgery, radiation, chemotherapy, or a trial\b",
            section);

        // And the two questions are kept apart, which is the thing people
        // merge.
        Assert.Matches(@"(?i)\btwo separate questions\b", section);
        Assert.Matches(@"(?i)\bA yes to the first is not a yes to the\s+second\b", section);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatTheMarkersPageOwns()
    {
        // §12.10. /tests/molecular-markers (WI-509) owns what each marker IS.
        // This page owns what happens when one of them points at a drug, and a
        // second set of definitions is a second set to keep in step.
        //
        // The sibling is read rather than assumed — and pinned to its
        // CONVENTION rather than to a phrase.
        //
        // THE BREAK HARNESS WALKED THE FIRST VERSION. It asserted
        // `(?i)\bWhat is measured\b` against the sibling's reader text, and
        // /tests/molecular-markers can lose EVERY ONE of its fifteen
        // `**What is measured.**` entries and stay green, because that page
        // also says, in ordinary lowercase prose, "Each one below says what is
        // measured, what your team does with it". A case-insensitive anchor on
        // a phrase the page also uses as prose is not an anchor: it proves the
        // file exists, which is not what the comment above it claimed.
        // §12.8 (WI-532).
        // AND THE THING ASSERTED IS THE TWO ENTRIES THIS PAGE ACTUALLY DEPENDS
        // ON, not a headcount. `/review` beat the count version: deleting the
        // IDH and BRAF entries — the only two markers this page routes readers
        // there for, and the two its own front matter names — drops fifteen to
        // thirteen, so a `>= 10` floor stays green through exactly the change
        // its failure message claims to catch.
        //
        // `Body`, not `Read`: a front-matter YAML comment quoting the
        // convention would otherwise inflate the count (§12.8, WI-531 — a guard
        // scoped to the body so a ruling that quotes it cannot fail a correct
        // page).
        var siblingBody = CuratedPage.Body(CuratedPage.Read("tests", "molecular-markers.md"));

        foreach (var anchor in new[] { "{#idh}", "{#braf}" })
        {
            Assert.Contains(anchor, siblingBody, StringComparison.Ordinal);
        }

        // The convention is checked too, as a second signal, and its message
        // admits the case the anchors do not cover: a corpus-wide RENAME of the
        // lead-in is a correct edit that needs this guard updated, not the
        // page's scope re-decided.
        var owned = Regex.Matches(siblingBody, @"(?m)^\*\*What is measured\.\*\*").Count;
        Assert.True(owned >= 10,
            $"/tests/molecular-markers carries {owned} '**What is measured.**' entries rather "
            + "than the fifteen it had when this page was written. Either that page has lost "
            + "entries — in which case this page's scope needs re-deciding — or the lead-in has "
            + "been renamed corpus-wide, in which case update this line.");

        // A BARE MARKER NAME IS NOT A DEFINITION. The first version banned
        // the names outright and fired on the page's own questions list --
        // "Has my tumor been tested for IDH, 1p/19q, BRAF and MGMT?" --
        // which is the most actionable line on the page. What this page may
        // not do is EXPLAIN them, so the guard bans the explaining shapes.
        var defining = new Regex(
            @"(?i)\b(?:IDH|1p/19q|ATRX|TERT|MGMT|Ki-67|CDKN2A|EGFR)\b(?:\W+\w+){0,6}\W+"
            + @"\b(?:stands for|means that|is a gene that|is measured|tells your team|"
            + @"is looked for by|is tested by|is a stain)\b"
            + @"|(?i)\bmutant and wildtype\b"
            + @"|(?i)\bWhat is measured\b"
            + @"|(?i)\bpromoter methylation\b");

        Assert.Matches(defining, "IDH stands for isocitrate dehydrogenase.");
        Assert.Matches(defining, "MGMT tells your team something about chemotherapy.");

        // The correct line the first version failed.
        Assert.DoesNotMatch(defining,
            "Has my tumor been tested for IDH, 1p/19q, BRAF and MGMT, and can I have a "
            + "copy of the report?");

        var hits = defining.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "this page has started defining markers, which /tests/molecular-markers owns: "
            + string.Join(" | ", hits));

        // And it routes there instead.
        Assert.Contains("/tests/molecular-markers", Page, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------- the safety half

    [Fact]
    public void TheLiverWarningSignsArePresentAndAreNotDowngraded()
    {
        // ACS gives these four as the reason vorasidenib's blood tests exist,
        // and tells the reader to report them. §12.8 (WI-525): a membership
        // check is not a tier check, so the downgrade shapes are banned in the
        // same section.
        var section = PlainOf(SideHeading);

        Assert.Matches(@"(?i)\beyes or skin turn yellow\b", section);
        Assert.Matches(@"(?i)\burine goes dark\b", section);
        Assert.Matches(@"(?i)\bupper right of your belly\b", section);
        Assert.Matches(@"(?i)\bCall your team\b", section);

        var downgrade = new Regex(
            @"(?i)\b(?:liver|yellow|jaundice)\b(?:\W+\w+){0,20}\W+"
            + @"\b(?:can wait|no rush|not urgent|until your next|nothing to worry about|"
            + @"usually harmless|rarely serious|settles on its own)\b");

        Assert.Matches(downgrade, "A yellow tinge usually settles on its own.");
        Assert.Matches(downgrade, "Liver changes can wait until your next appointment.");
        Assert.DoesNotMatch(downgrade, section);
    }

    [Fact]
    public void TheFeverLineHasAPlanAndRoutesToTheRuleThatActuallyApplies()
    {
        // Fever is the most common adverse reaction on the BRAF pair, and the
        // corpus has NO tier for it: [ESCALATION]'s two fever rules are scoped
        // to chemotherapy and to the weeks after surgery, and this reader is
        // neither. §12.8 (WI-506): never a problem with no answer.
        //
        // And it must NOT borrow the chemotherapy threshold, which
        // /treatments/chemotherapy owns and scopes to chemotherapy (§12.10).
        var section = PlainOf(SideHeading);

        Assert.Matches(@"(?i)\bit needs a\s+plan rather than a guess\b", section);
        Assert.Matches(@"(?i)\bwhat temperature\s+they want to hear about\b", section);
        Assert.Matches(@"(?i)\bnot the same event as a fever during\s+chemotherapy\b", section);
        Assert.Contains("/treatments/chemotherapy#fever-rule", Page, StringComparison.Ordinal);

        // No temperature threshold is published here. The sibling owns the one
        // number the corpus prints, and it says teams differ.
        var threshold = new Regex(
            @"(?i)\b" + CountWord + @"(?:\.\d+)?\s*(?:°|degrees?|F\b|C\b)"
            + @"|(?i)\babove\b(?:\W+\w+){0,3}\W+\b" + CountWord + @"(?:\.\d+)?\s*(?:°|degrees)");

        Assert.Matches(threshold, "Call if your temperature goes above 100.4 F.");
        Assert.Matches(threshold, "A fever of thirty-eight degrees.");
        Assert.DoesNotMatch(threshold, Plain);

        var sibling = Sibling("treatments", "chemotherapy.md");
        Assert.Matches(@"(?i)\bfever\b", sibling);
    }

    [Fact]
    public void TheSkinCancerWarningSurvives()
    {
        // ACS gives this its own paragraph and an instruction. It is the one
        // side effect on the page that asks the reader to do something
        // repeatedly rather than once.
        var section = PlainOf(SideHeading);
        Assert.Matches(@"(?i)\bskin cancers\b", section);
        Assert.Matches(@"(?i)\byour skin should be checked regularly\b", section);
        Assert.Matches(@"(?i)\bask how often\b", section);
        Assert.Matches(@"(?i)\btell your team right away\b", section);
    }

    [Fact]
    public void ThePageCarriesNoEscalationList()
    {
        // §12.8 (WI-512): an escalation tier is a site-wide property. This page
        // names drug-specific signs and routes; it does not invent a tier list,
        // because the corpus sorts these symptoms elsewhere and a second list
        // is a second list to keep in step.
        CuratedPage.AssertNoEscalationList(Page, Slug);
    }

    // ------------------------------------------------------- the caregiver half

    [Fact]
    public void TheCaregiverSectionComposesTheBlockAndAddsOnlyWhatIsTrueHere()
    {
        // §12.7. What a drug taken at home asks of a caregiver is different
        // from what an operation or a course of radiation asks, and that is the
        // page's own half.
        Assert.Matches(@"(?m)^\[CAREGIVER\]\s*$", Page);

        var section = PlainOf(CaregiverHeading);
        Assert.Matches(@"(?i)\bsomebody has to keep track\b", section);
        Assert.Matches(@"(?i)\bwhat to do about a missed one before it happens\b", section);
        Assert.Matches(@"(?i)\bAsk what the drug is hoping to do\b", section);

        // §12.8 (WI-527, WI-528): count the section's own claims, by
        // paragraph-leading bold, because a paraphrase is what a shingle check
        // cannot see.
        var claims = Regex.Matches(RawSection(CaregiverHeading), @"(?m)^\*\*").Count;
        Assert.True(claims == 4,
            $"the caregiver section makes {claims} claims rather than four.");
    }

    [Fact]
    public void TheCaregiverSectionDoesNotRepeatTheBlocksOwnSentences()
    {
        // §12.8 (WI-510): word shingles over the composed section.
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
            "the page's caregiver half restates [CAREGIVER]:\n  "
            + string.Join("\n  ", shared.Take(4)));
    }

    // -------------------------------------------------- corpus-wide housekeeping

    [Fact]
    public void ThePageDoesNotRestateWhatAnotherPageAlreadyOwns()
    {
        // §12.8 (WI-521, WI-530), with the one sentence that is shared on
        // purpose. Punctuation-free, because `Shingles()` tokenises to
        // lowercase words and an entry carrying commas can never contain one —
        // the defect WI-531 shipped twice.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug,
            "it can make scans look better and cut down the need for steroids but in the trials "
            + "in newly diagnosed glioblastoma it did not help people live longer",
            // (An entry reading "and reading it alone on a phone is a hard way
            // to find out" was removed here: /review computed the shingle set
            // and it exempted nothing, because that sentence is not on this
            // page. A redaction you have not watched fire is dead code that
            // reads like care -- section 12.8, WI-530.)
            // The door this page put on /treatments/chemotherapy says what
            // a targeted drug is, in this page's own words, because a door
            // that describes the thing differently from the page it opens
            // is the §12.10 two-strengths defect in miniature.
            "a targeted drug is built to act on one particular change inside tumor cells so "
            + "it only applies if your tumor carries that change",
            // ALREADY ON THREE SIBLING PAGES before this one existed --
            // /treatments/chemotherapy, /treatments/craniotomy and
            // /treatments/radiation-therapy. Three pages using one wording for
            // one claim is §12.10's CORRECT state rather than a restatement,
            // and rewording it here to dodge the shingle check would have
            // created the two-strengths defect the check exists to prevent.
            "if you would rather hear it from a person first say so now rather than afterwards",
            // The corpus's one wording for an ambulance instruction,
            // already on /treatments/anti-seizure-medicines. §12.10 wants
            // one wording for one instruction, and rewording it here to
            // dodge the shingle check would put two versions of an
            // emergency line on the site.
            "chest pain or trouble breathing is an ambulance call 911 or your local "
            + "emergency number that is where the rest of this site files it too an "
            + "ambulance call or your local emergency number");
    }

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        // §12.8 (WI-511, WI-529, WI-530, WI-531). Run on `Plain`, so the
        // description is in scope (§12.8, WI-528).
        //
        // This page's first draft said a drug was "given as a drip", which is
        // on the shared list as a determiner-bound idiom and was caught by the
        // corpus-wide housekeeping test before this file existed.
        var offenders = CuratedPage.BritishForms
            .Where(form => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(form)}"))
            .Where(form => !CuratedPage.BritishFormExemptions.Any(
                ex => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(ex)}")
                      && ex.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "British forms on a page written for a US reader: " + string.Join(", ", offenders));
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
    public void ThePageNeverCallsATargetedDrugGentlerThanChemotherapy()
    {
        // The commonest thing people believe about these drugs, and the page
        // says the opposite in its own voice. §12.14: the ban and the claim.
        Assert.Matches(@"(?i)\bNarrower does not mean gentler\b", Plain);

        string[] allowed = ["Narrower does not mean gentler"];
        var guard = new Regex(
            @"(?i)\b(?:gentler|kinder|easier|milder|softer|less harsh|fewer side effects)\b"
            + @"(?:\W+\w+){0,8}\W+\bchemo(?:therapy)?\b"
            + @"|(?i)\bchemo(?:therapy)?\b(?:\W+\w+){0,8}\W+"
            + @"\b(?:gentler|kinder|easier|milder|harsher|worse)\b");

        Assert.Matches(guard, "They are gentler than chemotherapy.");
        Assert.Matches(guard, "Chemotherapy is harsher.");

        var hits = guard.Matches(Redact(Plain, allowed)).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page compares these drugs with chemotherapy on how hard they are, which no "
            + "source supports: " + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageCarriesNoAuthoringMarkersInItsBody()
    {
        // §12.8 (WI-512): a suppression is only meaningful if the word is there
        // to suppress, and this page defines none of the glossary's terms.
        // §12.8 (WI-529): a marker must never share a line with a directive.
        // Scoped to the BODY so a future front-matter ruling quoting a marker
        // does not fail a correct page (§12.8, WI-531).
        Assert.DoesNotMatch(@"!%", CuratedPage.Body(Page));

        var offenders = Regex.Matches(Page, @"(?m)^.*!%.*\[[A-Z-]+\].*$")
            .Select(m => m.Value).ToList();
        Assert.True(offenders.Count == 0,
            "an authoring marker shares a line with a block directive: "
            + string.Join(" | ", offenders));
    }
    [Fact]
    public void SlotFiveIsCarriedBecauseEveryOtherTreatmentPageCarriesIt()
    {
        // §12.8 (WI-512): when you drop a universal-ish slot, check whether the
        // material exists anyway somewhere worse — and it did. A first draft
        // had no "what is it like" section at all, and its three pieces were
        // scattered into slot 4, the caregiver section and slot 1. All nine
        // shipped treatment pages carry this slot, including both other DRUG
        // pages, which are the closest analogues.
        var section = PlainOf(FeelHeading);

        Assert.Matches(@"(?i)\bmost days feel like nothing at all\b", section);
        Assert.Matches(@"(?i)\bThe blood tests are the rhythm of this, not the drug\b", section);
        Assert.Matches(@"(?i)\bno countdown and no last day to\s+aim at\b", section);

        // And both directions of the open end, because a page that only says
        // it is a relief has told the reader who finds it hard that they are
        // unusual (§12.12).
        Assert.Matches(@"(?i)\bSome people find that easier\b", section);
        Assert.Matches(@"(?i)\bSome find it\s+much harder\b", section);

        // The sibling drug pages are read rather than assumed.
        foreach (var sibling in new[] { "chemotherapy.md", "steroids.md" })
        {
            var text = CuratedPage.Read("treatments", sibling);
            Assert.Matches(@"(?im)^## What.*(?:feel|like)", text);
        }
    }

    [Fact]
    public void TheSeventhDoorWasRefusedByASiblingsOwnPinAndStaysRefused()
    {
        // §12.8 (WI-521): a correctly-pinned CLOSING sentence must survive a
        // sentence appended after it, and `/treatments/watch-and-wait` pins its
        // glioma paragraph END-ANCHORED for exactly that reason. A seventh door
        // was appended there, that pin went red, and the DOOR was dropped
        // rather than the pin loosened.
        //
        // This test exists so the next person to notice the missing door reads
        // the reason before adding it back.
        // THE UNIT IS THE PARAGRAPH, NOT A CHARACTER WINDOW.
        //
        // Two earlier versions were beaten, and the second was beaten by
        // `/review` rather than by the harness. A bare page-wide
        // `Assert.Contains("/tumors/low-grade-glioma", …)` stayed green after
        // the route was deleted from the vorasidenib paragraph, because that
        // page's "Where to go next" index carries the same slug four screens
        // later (§12.8, WI-512: presence is not position). Replacing it with a
        // 400-character window around the pinned sentence then failed BOTH
        // ways: `IndexOf` returns the FIRST occurrence page-wide, so adding an
        // unrelated low-grade-glioma link near the top of that page turned this
        // red with a message accusing the vorasidenib paragraph of something it
        // had not done; and `Math.Abs` accepts a route AFTER the pin, so moving
        // the route into the next paragraph (`**Acoustic neuroma.**`, 121
        // characters away) stayed green.
        //
        // A character distance is a proxy for "same paragraph". Slice the
        // paragraph instead and the proxy is not needed.
        var siblingSource = CuratedPage.Read("treatments", "watch-and-wait.md");
        var paragraph = Regex.Split(CuratedPage.ReaderText(siblingSource), @"\r?\n[ \t]*\r?\n")
            .Single(p => p.Contains("group you would be in", StringComparison.Ordinal));

        // The pin itself, end-anchored the way that page's own test anchors it,
        // so a sentence appended after it fails here too.
        Assert.Matches(
            @"explains what a placebo\s+is and what it means for the group you would be in\.\s*$",
            paragraph);

        // The door stays refused IN THAT PARAGRAPH. Scoped rather than
        // page-wide: a later item adding this page to that page's "Where to go
        // next" index is a correct edit that does not touch the end-anchored
        // pin, and must not fail against a reason that does not apply to it.
        Assert.DoesNotContain("/treatments/targeted-therapy", paragraph, StringComparison.Ordinal);

        // And the reader still reaches this page from there, by the route that
        // paragraph already offers.
        Assert.Contains("/tumors/low-grade-glioma", paragraph, StringComparison.Ordinal);

        var hub = CuratedPage.Read("tumors", "low-grade-glioma.md");
        Assert.Contains("/treatments/targeted-therapy", hub, StringComparison.Ordinal);
    }

}

/// <summary>
/// WI-532: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class TargetedTherapyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/targeted-therapy";

    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// The connection string is pushed in. WI-531 shipped two render classes
    /// that did not and they passed here and failed in CI, because a developer
    /// machine has the string in <c>dotnet user-secrets</c> and a runner does
    /// not. <c>TestCollectionHygieneTests</c> now guards this corpus-wide.
    /// </summary>
    public TargetedTherapyPageRenderTests(WebApplicationFactory<Program> raw) =>
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
        Assert.DoesNotContain("[ESCALATION]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.Contains("Narrower does not mean gentler", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/tests/molecular-markers",
            "/treatments/chemotherapy",
            "/treatments/clinical-trials",
            "/tests/follow-up-scans",
            "/get-help-now");
    }

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists()
    {
        // §12.8 (WI-513): `AssertLinksResolve`'s regex stops at the `#`, so a
        // link to a heading that does not exist comes back a healthy 200 while
        // landing the reader at the top of a long page. This page deep-links to
        // the chemotherapy fever rule and to the placebo section.
        await CuratedPage.AssertFragmentLinksResolve(factory.CreateClient(), Url);
    }

    [Fact]
    public async Task TheDoorsOnTheSiblingPagesAreAppendedSentencesNearWhatTheyWereAppendedTo()
    {
        // §12.8 (WI-526): a door must sit near the sentence it was appended to.
        // §12.8 (WI-530): a door is prose on the page it lands on, so it is
        // diffed against that page as well.
        (string Slug, string Pinned, string Door)[] doors =
        [
            ("tests/molecular-markers",
                "Finding a change does not mean a drug is right for you.",
                "/treatments/targeted-therapy"),
            ("tumors/glioblastoma",
                "did not help people live longer",
                "/treatments/targeted-therapy"),
            ("tumors/low-grade-glioma",
                "a newer medicine taken by mouth, for some people. Its own",
                "/treatments/targeted-therapy"),
            ("tumors/astrocytoma",
                "It works on the IDH change itself.",
                "/treatments/targeted-therapy"),
            ("treatments/chemotherapy",
                "There is usually a reason, and it is a fair question.",
                "/treatments/targeted-therapy"),
            ("tumors/brain-metastases",
                "whether your cancer has been tested for that change.",
                "/treatments/targeted-therapy"),
        ];

        foreach (var (slug, pinnedRaw, door) in doors)
        {
            var parts = slug.Split('/');
            var text = CuratedPage.Flatten(CuratedPage.Read(parts[0], parts[1] + ".md"));
            var pinned = CuratedPage.Flatten(pinnedRaw);

            var pinnedAt = text.IndexOf(pinned, StringComparison.Ordinal);
            Assert.True(pinnedAt >= 0,
                $"{slug} no longer contains the sentence the door was appended to, so the door "
                + "may have replaced a paragraph rather than been added to one.");

            var doorAt = text.IndexOf(door, StringComparison.Ordinal);
            Assert.True(doorAt >= 0, $"{slug} has lost its door to this page");

            Assert.True(Math.Abs(doorAt - pinnedAt) < 800,
                $"{slug}'s door has drifted {Math.Abs(doorAt - pinnedAt)} characters from the "
                + "sentence it was appended to.");
        }
    }

}
