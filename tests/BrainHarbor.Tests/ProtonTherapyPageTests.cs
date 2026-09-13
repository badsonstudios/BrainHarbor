using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-531, first of two pages: <c>/treatments/proton-therapy</c>. A §12.8
/// library page, and the one whose subject is NOT the treatment. The backlog
/// item says it outright: the reader's real question is access, not physics.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * SELLING IT. Proton therapy is the most hype-prone subject in the
///     treatment library, and every marketing page a reader will find says it
///     is better. The two preferred sources say something else. ACS, under its
///     own heading "What are the limitations of proton therapy?": "Proton
///     therapy is a newer treatment. More research is needed to know if it's
///     better than traditional radiation therapy." ACS's brain page: "it's not
///     clear if it is as useful for tumors that typically grow into or mix with
///     normal brain tissue, such as astrocytomas or glioblastomas" — which is
///     most of this site's readers. EANO: "RCTs are required to determine the
///     tolerability, safety and efficacy of these approaches compared with
///     standard radiotherapy." A page that prints the benefit and not that
///     sentence has taken a side the evidence has not.
///   * BECOMING A SECOND RADIATION PAGE. ACS says the proton day IS the photon
///     day. /treatments/radiation-therapy (WI-511) owns all of it. Restating
///     the mask, the weeks and the tiredness here would be two copies to keep
///     in step, and it would crowd out the only thing this page is for.
///   * PUBLISHING A COST. The dossier's "~60% more" comes from a content farm.
///     The peer-reviewed figure is a standardized Medicare reimbursement rate,
///     which is roughly two and a half times and is not what a reader pays.
///     Either number in a reader's hands is a number they will act on.
///   * TELLING SOMEBODY A DENIAL IS THE END. It is a normal step with a defined
///     route out of it, and the route is free, is binding on the plan, and is
///     routinely not used.
///   * LETTING AN APPEAL DELAY TREATMENT WITHOUT SAYING SO. The one question on
///     this page a reader must not leave the room without is whether waiting
///     for a decision would hold up the start of their treatment.
///
/// Every guard starts from §12.8's accumulated lessons rather than
/// rediscovering them: <c>CountWord</c> knows the teens, quantifiers allow a
/// word gap, numeric bans are threshold-shaped, phrase guards run on
/// <c>Plain</c> so the description is in scope, section order asserts PRESENCE
/// before position, every claim-shaped guard has a canary that proves it can
/// fire, and every regex that touches a line break uses <c>\r?\n</c>.
/// </summary>
public sealed class ProtonTherapyPageContentTests
{
    private const string Slug = "treatments/proton-therapy";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is proton therapy?";
    private const string BetterHeading = "Would it be better for me?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "Is it the same to be in?";
    private const string AccessHeading = "Where are the centers, and what would getting there mean?";
    private const string InsuranceHeading = "What if my insurance says no?";
    private const string NeedHeading = "What you need first";
    private const string DecidesHeading = "Who decides, and how will I hear?";
    private const string CaregiverHeading = "For the person going with them";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("treatments", "proton-therapy.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// collapses all whitespace to single spaces, so anything that needs to see
    /// a PARAGRAPH or count a list MARKER has to cut the markdown itself
    /// (§12.8, WI-526 and WI-528). <c>\r?\n</c>, never a bare <c>\n</c>.
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
    /// under the heading (§12.8, WI-524 and WI-528).
    /// </summary>
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

    /// <summary>The page's own "## " headings, in order, anchors stripped.</summary>
    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The URL VALUES from the front matter. A ban run over the whole front
    /// matter fires on this page's own rulings, which quote the banned domain
    /// in order to explain the ban (§12.8).
    /// </summary>
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
        + @"tenths?|twentieths?|hundredths?)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): presence first, then position, because
        // `List.IndexOf` returns -1 and -1 is less than everything.
        //
        // §12.8 (WI-510): read the STANDARD for the slot list. Slot 8 ("why am
        // I having another one?") is the one slot legitimately absent here —
        // re-irradiation belongs to /treatments/radiation-therapy, which owns
        // it — and that absence is asserted rather than assumed, two tests
        // down.
        var headings = Headings();

        // §12.7 and §12.8 put the caregiver section immediately after slot 7,
        // and all three sibling treatment pages do. A first draft had it after
        // slot 9.
        string[] required =
        [
            ShortHeading, WhatHeading, BetterHeading, StepsHeading, LongHeading, FeelHeading,
            AccessHeading, InsuranceHeading, NeedHeading, CaregiverHeading, DecidesHeading,
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
                $"'{required[i]}' has moved above '{required[i - 1]}'.");
        }
    }

    [Fact]
    public void SlotFiveIsKeptRatherThanDroppedBecauseTheSiblingOwnsTheAnswer()
    {
        // §12.8 (WI-512): when you drop a universal-ish slot, check whether the
        // material exists anyway somewhere worse. The temptation on this page
        // is to drop "what does it feel like?" because the answer is "the same
        // as the other page" — and then scatter the answer through three other
        // sections. It is a slot, headed as the reader's own question
        // (§12.8, WI-507: slot 7 is a role, not a heading, and so is this one),
        // and it answers itself before it routes.
        var section = PlainOf(FeelHeading);
        Assert.Matches(@"(?i)^\s*Yes, near enough\b", section);
        Assert.Contains("/treatments/radiation-therapy#side-effects", RawSection(FeelHeading),
            StringComparison.Ordinal);
    }

    [Fact]
    public void SlotSixBIsAbsentOnPurposeAndItsMaterialIsInSlotFiveWithARoute()
    {
        // §12.8 (WI-527): a contract item no test names is not a contract
        // item, and that cuts both ways -- an ABSENCE nobody asserts is an
        // absence somebody will "fix" by writing a second copy of the
        // sibling's side-effects section.
        //
        // §12.8 (WI-512): when you drop a slot, check the material has not
        // landed somewhere worse. Here it is in slot 5, under the reader's
        // own question, in four sentences, with the route attached.
        Assert.DoesNotContain(Headings(), h =>
            Regex.IsMatch(h, @"(?i)^side effects"));

        var section = PlainOf(FeelHeading);
        Assert.Matches(@"(?i)\bTiredness turns up during the weeks\b", section);
        Assert.Matches(@"(?i)\bskin in the treated area\b", section);
        Assert.Matches(@"(?i)\bhair goes where the beam goes\b", section);

        // And the route, including the safety half: §12.11 says the page
        // OWES the reader the escalation rule even when a sibling owns it.
        Assert.Contains("/treatments/radiation-therapy#side-effects",
            RawSection(FeelHeading), StringComparison.Ordinal);
        Assert.Matches(
            @"(?i)\bWhat should make you\s+call your team the same day, and what is an ambulance, is on that page\s+too\b",
            section);

        // The sibling is READ rather than assumed, so a softening there
        // goes red here (§12.8, WI-525).
        var sibling = Sibling("treatments", "radiation-therapy.md");
        Assert.Matches(@"(?i)\bSide effects during treatment, and in the weeks after\b",
            sibling);
        Assert.Matches(@"(?i)\bSide effects that can come later\b", sibling);
    }

    [Fact]
    public void ThePageCarriesNoRepeatSectionAndRoutesReIrradiationToTheSibling()
    {
        // The other half of the slot list. A "why am I having another one?"
        // section here would be a second copy of
        // /treatments/radiation-therapy's re-irradiation material, which is
        // carefully hedged there ("European guidelines say the reasons for
        // doing it are still unsettled") and would arrive here unhedged.
        Assert.DoesNotContain(Headings(), h =>
            Regex.IsMatch(h, @"(?i)another (?:one|course)|having (?:radiation|this) again"));

        var sibling = Sibling("treatments", "radiation-therapy.md");
        Assert.Matches(@"(?i)Why am I having radiation again", sibling);
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorSoRewordingAHeadingCannotBreakAnInboundLink()
    {
        // §12.8 (WI-508, WI-509).
        var headings = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.True(headings.Count >= 13,
            $"only {headings.Count} '## ' headings were found, so this guard is checking almost "
            + "nothing. The heading regex has stopped matching the page.");

        var missing = headings.Where(h => !Regex.IsMatch(h, @"\{#[\w-]+\}$")).ToList();
        Assert.True(missing.Count == 0,
            "these headings have no explicit anchor:\n  " + string.Join("\n  ", missing));
    }

    // ----------------------------------------------------------- the anti-hype

    [Fact]
    public void TheUnsettledHalfIsOnThePageAndIsNotBuriedBelowTheBenefit()
    {
        // §12.8 (WI-506): answer the frightening question in BOTH directions.
        // Here the directions are reversed — the reassuring half is the one
        // every other site prints, and the honest half is the one this page
        // owes. Both are asserted, and the order is asserted with them,
        // because a limitation two screens under a benefit is a limitation
        // most readers never reach.
        var section = PlainOf(BetterHeading);

        Assert.Matches(@"(?i)^\s*The honest answer is that it depends on your tumor, and for many "
            + @"brain tumors nobody knows yet\b", section);
        Assert.Matches(@"(?i)\bnot clear whether protons are as useful\b", section);
        Assert.Matches(@"(?i)\btrials are\s+still needed\b", section);
        Assert.Matches(@"(?i)\bastrocytoma\b", section);
        Assert.Matches(@"(?i)\bglioblastoma\b", section);

        // The section says which readers that covers, which is the sentence
        // that turns a general caveat into one about them.
        Assert.Matches(@"(?i)\bmost of the tumors this site is about\b", section);
    }

    [Fact]
    public void ThePageNeverClaimsProtonsAreBetterOrStrongerThanOrdinaryRadiation()
    {
        // The claim every marketing page makes, and the one neither preferred
        // source supports. Claim-shaped rather than a word list, because §12.8
        // (WI-526) records a hype ban being walked around by paraphrase on a
        // subject exactly like this one.
        //
        // Redacted, not allowlisted (§12.8, WI-527): the page has to be able to
        // discuss the word "better" in order to refuse it.
        string[] allowed =
        [
            "So the word to be careful with is \"better\".",
            "Protons are better at missing things.",
            "Whether missing those things leaves you better off, years later, has not been "
            + "settled for most brain tumors.",
            "is not clear whether protons are as useful",
            "more research is needed to know if",
            "Is it known to be better for my kind of tumor, or is that still being worked",
            "leaves you better off",
        ];

        // CLAUSE-ANCHORED NEGATION (§12.8, WI-511). The page's own correction —
        // "Protons are not stronger" — is the sentence this guard exists to
        // protect, and a bare ban fires on it. A window lookbehind fails both
        // ways; the working form is a negation close by AND on this side of the
        // nearest punctuation. §12.8 (WI-522): a negation inside a superlative
        // is not a negation, which is why "no better way" is not in the list.
        const string NotNegated = @"(?<!\b(?:not|never|no|n't|hardly|far from)\b[^.,;:]{0,25})";

        // WIDENED AFTER /review BEAT IT SIX WAYS. Every one of the beating
        // sentences is in the canary list below. The first version banned the
        // literal `better than`, so ANY word between the two defeated it
        // ("better results than"), and its second branch omitted `better than`
        // altogether, so a sentence with no proton word nearby walked. It also
        // had no notion of a RECOMMENDATION — "that is the one to ask for"
        // makes the claim with no comparative in it at all.
        const string Superiority =
            @"(?:better|stronger|more (?:powerful|effective|precise|advanced|accurate)|"
            + @"superior|outperform\w*|beats?|kills? more|works? better|"
            + @"more likely to work|the (?:best|right) (?:choice|option|one))";

        var scanned = Redact(Plain, allowed);
        var guard = new Regex(
            @"(?i)" + NotNegated + @"\b" + Superiority + @"\b(?:\W+\w+){0,4}\W+\bthan\b"
            + @"|(?i)\bprotons?\b(?:\W+\w+){0,8}\W+" + NotNegated + @"\b" + Superiority + @"\b"
            + @"|(?i)" + NotNegated + @"\b" + Superiority
            + @"\b(?:\W+\w+){0,8}\W+\b(?:protons?|proton therapy|particle beams?)\b"
            + @"|(?i)\bthe (?:best|gold standard|most advanced) (?:treatment|option|radiation)\b"
            // The recommendation shape, which carries the claim without making
            // a comparison at all.
            + @"|(?i)\bthe one to (?:ask for|get|have|push for|go for)\b"
            + @"|(?i)\b(?:worth|worth it) (?:pushing|fighting|holding out) for\b"
            + @"|(?i)\bif you can get to a proton center\b");

        Assert.Matches(guard, "Protons are stronger than x-rays.");
        Assert.Matches(guard, "Proton therapy is better than ordinary radiation.");
        Assert.Matches(guard, "A proton beam is more likely to work.");
        Assert.Matches(guard, "It is the most advanced radiation there is.");

        // The six sentences /review used to beat the first version.
        Assert.Matches(guard, "Proton therapy gives better results than ordinary radiation.");
        Assert.Matches(guard, "Proton therapy is the better choice for most brain tumors.");
        Assert.Matches(guard, "Proton beams outperform x-ray beams near the brainstem.");
        Assert.Matches(guard, "Protons are more precise than x-rays, so they do less harm.");
        Assert.Matches(guard, "If you can get to a proton center, that is the one to ask for.");
        Assert.Matches(guard,
            "Particle beams are better than the photon ones, which is why proton centers exist.");

        // The negation anchor is proved in both directions, or it is a hole
        // rather than a clause anchor: it must let the correction through, and
        // it must NOT let a negation belonging to the other clause through.
        Assert.DoesNotMatch(guard, "Protons are not stronger than x-rays.");
        Assert.Matches(guard,
            "It is not a difficult treatment, and protons are stronger than x-rays.");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "the page claims protons are better, which neither ACS nor EANO supports: "
            + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageSaysProtonsAreNotStrongerInItsOwnVoice()
    {
        // §12.14: banning a claim is not making the opposite one. The dossier's
        // best sentence — "patients routinely believe protons are better at
        // killing tumor" — is the reason this has to be said rather than merely
        // not contradicted. Asserted over the summary AND the description,
        // because §12.8 (WI-520) records a section-scoped position test that
        // could not see the summary.
        Assert.Matches(@"(?i)Protons are not stronger", PlainOf(ShortHeading));

        var description = Regex.Match(CuratedPage.FrontMatter(Page),
            @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
        Assert.Matches(@"(?i)\bnot a stronger kind of radiation\b", description);
    }

    // -------------------------------------------------------------- the money

    [Fact]
    public void NoCostFigureOrMultiplierAppearsAnywhere()
    {
        // The dossier's "~60% more" is sourced to a content farm. The
        // peer-reviewed comparison is a standardized Medicare reimbursement
        // rate, not a bill. §12.8 (WI-520): ask which way a number pushes if
        // the reader acts on it alone — this one pushes toward not asking.
        // WIDENED AFTER /review BEAT IT FOUR WAYS, and one of the beating
        // sentences published the exact figure the front matter says must never
        // appear. `times the price` is not `times as`; `several` is not a count
        // word; and `\bthousand\b` cannot match `thousands`.
        const string Quantity = @"(?:" + CountWord + @"|several|many|numerous|"
            + @"hundreds?|thousands?|millions?|" + Fraction + @")";

        var guard = new Regex(
            @"\$\s*\d"
            // {0,4}, not {0,2}: "two AND A HALF times as expensive" puts three
            // words between the count and the multiplier, and the first version
            // could not match its own canary.
            + @"|(?i)\b" + Quantity + @"\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:times? (?:as |more|the )|per ?cent|%)"
            + @"|(?i)\b(?:twice|double|triple)\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:as (?:much|expensive)|the (?:cost|price))\b"
            + @"|(?i)\b(?:costs?|price|bill|charge)\b(?:\W+\w+){0,4}\W+\b" + Quantity + @"\b"
            + @"|(?i)\b" + Quantity + @"\b(?:\W+\w+){0,3}\W+\bdollars\b"
            + @"|(?i)\ba (?:fraction|" + Fraction + @") of the (?:cost|price)\b");

        Assert.Matches(guard, "It costs about sixty percent more.");
        Assert.Matches(guard, "Proton therapy costs $45,966.");
        Assert.Matches(guard, "It is roughly two and a half times as expensive.");
        Assert.Matches(guard, "The cost is around forty thousand dollars.");

        // The four sentences /review used to beat the first version.
        Assert.Matches(guard, "It is about two and a half times the price of ordinary "
            + "radiation.");
        Assert.Matches(guard, "It costs several times more than ordinary radiation.");
        Assert.Matches(guard, "A course can run to tens of thousands of dollars.");
        Assert.Matches(guard, "Ordinary radiation is a fraction of the price.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a cost figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheDirectionOfTheCostIsPublishedEvenThoughTheFigureIsNot()
    {
        // §12.8 (WI-521): a page that simply omits a number leaves the reader
        // to supply their own, and the one they supply is usually worse than
        // the truth. Here the direction is also the CAUSE of everything else in
        // the section — plans scrutinise it because it costs more — so dropping
        // it would leave the insurance section unexplained.
        var section = PlainOf(InsuranceHeading);
        Assert.Matches(@"(?i)costs more than ordinary radiation", section);
        Assert.Matches(@"(?i)That is why plans look at it harder", section);
    }

    // ------------------------------------------------------------ the centers

    [Fact]
    public void NoCountOfProtonCentersIsPublishedAndTheReaderIsSentToAskInstead()
    {
        // A FIRST DRAFT PRINTED "around forty-five", cited to PMC13521102. That
        // sentence is in that paper's INTRODUCTION and carries somebody else's
        // citation — the bundled-claim shape §12.8 (WI-512) records three
        // times — and /review also caught the paper being described as "a
        // recent review" when it is a single-center retrospective analysis
        // (§12.8, WI-519: check the study design, not just the sentence).
        //
        // The count is also the kind of number that goes stale while the page
        // sits there, and ACS says the useful part in its own voice: a limited
        // number, more being built. So no count is published at all, and the
        // reader is handed the question that actually has an answer for them.
        var section = PlainOf(AccessHeading);

        var guard = new Regex(
            @"(?i)\b(?:about|around|roughly|some|nearly|over|more than|fewer than)?\s*\b"
            + CountWord + @"\b(?:\W+\w+){0,3}\W+\b(?:proton )?(?:centers|centres|facilities)\b"
            + @"|(?i)\b(?:proton )?(?:centers|centres|facilities)\b(?:\W+\w+){0,3}\W+"
            + @"\bin the (?:United States|country|US)\b(?:\W+\w+){0,3}\W+\b" + CountWord + @"\b");

        Assert.Matches(guard, "There are around forty-five proton centers in the country.");
        Assert.Matches(guard, "About 45 centers offer it.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a count of proton centers has reached the page. It goes stale, and the reachable "
            + "figures are secondary citations rather than anybody's own finding: "
            + string.Join(" | ", hits));

        // What the page says instead, from a preferred patient-level source in
        // its own voice, and the action that follows from it.
        Assert.Matches(@"(?i)\bThere are not many of them\b", section);
        Assert.Matches(@"(?i)\ba\s+limited number of proton centers\b", section);
        Assert.Matches(@"(?i)\bmore are being\s+built\b", section);
        Assert.Matches(@"(?i)\bAsk your team\s+where the nearest one is\b", section);
    }

    [Fact]
    public void TheTravelComparisonIsTheStudysOwnFindingAndIsNotGivenAFigure()
    {
        // The one thing in PMC13521102 that IS first-party: "The mean
        // straight-line distance to the treatment facility was significantly
        // further for patients receiving Pr vs Ph therapy (56 miles vs 32
        // miles)". The page prints the comparison and not the miles, because
        // two averages from one center's catchment are not a distance anybody
        // else can plan around.
        //
        // A first draft also wrote "some of them lived more than two hundred
        // miles away", which is in NO source: the paper's 250 is a comparison
        // cut-off, not a description of anybody.
        var section = PlainOf(AccessHeading);

        Assert.Matches(@"(?i)\bOne large cancer center\s+compared everybody it treated\b",
            section);
        Assert.Matches(@"(?i)\bwell over twice as far away\b", section);

        var miles = new Regex(@"(?i)\b" + CountWord + @"\b(?:\W+\w+){0,3}\W+\bmiles\b");
        Assert.Matches(miles, "Some of them lived more than two hundred miles away.");
        Assert.Matches(miles, "The average was 56 miles.");

        var hits = miles.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a distance figure has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void TheAccessSectionOffersActionsRatherThanOnlyObstacles()
    {
        // §12.8's WI-506 rule, and the hardest slot on this page to get right:
        // "never a list of problems with no answers". The costs of being away
        // are real and the section has to name them, but it closes on what a
        // reader can do about them.
        var section = PlainOf(AccessHeading);

        Assert.Matches(@"(?i)\bNone of that is a reason to rule it out by yourself\b", section);
        Assert.Matches(@"(?i)\blodging schemes\b", section);
        Assert.Matches(@"(?i)\ba social worker whose job is exactly this\b", section);

        // The costs are enumerated, counted by marker rather than numeral
        // (§12.8, WI-526).
        var costs = Regex.Matches(RawSection(AccessHeading), @"(?m)^\s*[-*]\s").Count;
        Assert.True(costs >= 4,
            $"the access section names only {costs} of the real costs of being away.");
    }

    [Fact]
    public void TheDisparityFindingIsPublishedAsAReasonToAskRatherThanAsAStatistic()
    {
        // The paper's headline finding, and the reason it is on a patient page
        // at all: who gets proton therapy is not decided by the tumor alone, so
        // waiting to be offered it is a worse strategy than asking. No odds
        // ratio, no percentage, and the action is in the same paragraph.
        var section = PlainOf(AccessHeading);

        Assert.Matches(@"(?i)\bThe tumor is not the only thing that decides it\b", section);
        Assert.Matches(@"(?i)\bBlack patients are less likely to receive it\b", section);
        Assert.Matches(@"(?i)\bSo ask directly\b", section);

        // And it does not become a frequency claim. WIDENED AFTER /review BEAT
        // IT THREE WAYS: `as likely`, `as many` and `less often` were all
        // absent, and each expresses the same comparison.
        var guard = new Regex(
            @"(?i)\b(?:" + CountWord + @"|" + Fraction + @")\b(?:\W+\w+){0,4}\W+"
            + @"\b(?:of (?:people|patients)|less likely|more likely|as likely|as many|"
            + @"as often|less often|more often|times)\b"
            + @"|(?i)\b(?:far|much|way|significantly)\s+(?:less|more)\s+(?:often|likely)\b");

        Assert.Matches(guard, "They are three times less likely to receive it.");
        Assert.Matches(guard, "Black patients are half as likely to receive it.");
        Assert.Matches(guard, "Black patients receive it far less often than white patients.");
        Assert.Matches(guard, "Only a third as many Black patients are sent to a proton center.");
        Assert.DoesNotMatch(guard, section);
    }

    // ------------------------------------------------------------- the appeal

    [Fact]
    public void TheAppealSectionSaysTheOutsideReviewIsBindingAndFree()
    {
        // The two facts that turn a refusal letter from a verdict into a step.
        // Both are in the paper's own voice: "their decisions are legally
        // binding on health plans" and "The cost of this external review is
        // borne by the health insurance plan, not by the patient."
        var section = PlainOf(InsuranceHeading);

        Assert.Matches(@"(?i)\btheir decision is binding on\s+the plan\b", section);
        Assert.Matches(@"(?i)\bThe review costs you nothing\b", section);
        Assert.Matches(@"(?i)\bwork for neither your hospital nor your plan\b", section);
        Assert.Matches(@"(?i)\bfederal law\b", section);
    }

    [Fact]
    public void TheDelayQuestionIsOnThePageAndIsNotSoftened()
    {
        // The most important sentence on the page. An appeal takes weeks, and
        // for some readers starting ordinary radiation on schedule is the
        // better plan. A page that arms somebody to fight without telling them
        // what the fight costs has helped the wrong half of them.
        var section = PlainOf(InsuranceHeading);

        Assert.Matches(@"(?i)\bwould waiting hold up\s+my treatment\b", section);
        Assert.Matches(@"(?i)\bstarting ordinary radiation on schedule is the better plan\b",
            section);

        // §12.8 (WI-525): a membership check is not a tier check — asserting the
        // sentence is present is beaten by softening what is attached to it.
        //
        // INVERTED AFTER /review BEAT THE PHRASE LIST FOUR WAYS. A closed list
        // of softenings is not a guard (§12.8, WI-525, "two closed lists of
        // eight words is not a guard"): "rarely a problem", "seldom costs
        // anybody anything", "usually time to argue" and "a few weeks of
        // waiting will not change the outcome" all walked. So the rule is
        // inverted — any sentence in this section whose subject is the WAIT
        // must EARN its place, by naming a risk or by handing the judgement to
        // somebody who is not the reader. Shapes nobody has thought of then
        // fail by default instead of passing by default.
        var aboutTheWait = new Regex(
            @"(?i)\b(?:waiting|the wait|a delay|delays?|holding out|an appeal takes|"
            + @"time to (?:argue|appeal|fight))\b");
        var earnsItsPlace = new Regex(
            @"(?i)\b(?:hold up|holds up|held up|delay|delays|delayed|risk|costs?|"
            + @"weeks|month|behind|start date|on schedule)\b"
            + @"|(?i)\byour (?:radiation oncologist|team|doctor)\b"
            + @"|(?i)\bask\b");

        var unearned = CuratedPage.SentencesOf(section)
            .Where(s => aboutTheWait.IsMatch(s) && !earnsItsPlace.IsMatch(s))
            .ToList();

        // The four sentences /review used to beat the phrase list, proved to
        // fail the inverted form.
        foreach (var planted in new[]
                 {
                     "A few weeks of waiting will not change the outcome.",
                     "The wait is rarely a problem.",
                     "There is usually time to argue.",
                     "Waiting for an answer seldom costs anybody anything.",
                 })
        {
            Assert.True(aboutTheWait.IsMatch(planted),
                $"the wait-subject test cannot see: {planted}");
        }

        Assert.True(
            aboutTheWait.IsMatch("The wait is rarely a problem.")
            && !earnsItsPlace.IsMatch("The wait is rarely a problem."),
            "the inverted guard cannot see a softening that names no risk and no authority");

        Assert.True(unearned.Count == 0,
            "these sentences in the insurance section are about the wait and neither name a "
            + "risk nor hand the judgement to the team:\n  " + string.Join("\n  ", unearned));
    }

    [Fact]
    public void TheOneShareIsWrittenInWordsAndScopedInItsOwnSentence()
    {
        // §12.8 (WI-507, WI-511): attribute in the sentence that prints the
        // figure, and grade the CLAIM rather than the character class, because
        // this corpus writes every number in words.
        //
        // This is the only share on the page. It points in the safe direction —
        // appeal rather than give up — and it is scoped three ways in its own
        // sentences: one study, three named states, and the cases that got
        // that far.
        var section = PlainOf(InsuranceHeading);

        // THE FIGURE AND ITS SCOPE ARE NOW IN ONE SENTENCE. A first draft put
        // "close to half" in its own sentence with the scope in the sentences
        // on either side, while the front matter claimed the figure was
        // "attributed in the sentence that prints it" — §12.8 (WI-523), the
        // front matter claiming what the page does not do. "Close to half" also
        // rounded 42.1% toward hope.
        var printed = CuratedPage.SentencesOf(section)
            .Where(s => Regex.IsMatch(s, @"(?i)\bfour in every ten\b"))
            .ToList();

        Assert.True(printed.Count == 1,
            $"the share appears in {printed.Count} sentences rather than one "
            + "(§12.8, WI-521: assert a figure appears exactly once).");

        Assert.Matches(@"(?i)\bIn their study of the published records from three states\b",
            printed[0]);
        Assert.Matches(@"(?i)\bCalifornia, New York and Washington\b", printed[0]);
        Assert.Matches(
            @"(?i)\bThat is one study of three states and it is not a forecast about your plan\b",
            section);

        // AND NO SECOND SHARE, ASSERTED AT ZERO RATHER THAN AT A CEILING.
        // §12.8 (WI-528): an allowance of one is an allowance of one MORE. The
        // first version asserted `shares.Count <= 2` while the page matched 1,
        // so the free slot let /review add "About a third of first appeals
        // succeed" and stay green. The legitimate sentence is REDACTED and the
        // remainder must match nothing (§12.8, WI-527).
        const string LegitimateShare =
            "In their study of the published records from three states, California, New York "
            + "and Washington, a little over four in every ten proton requests were decided in "
            + "the patient's favor.";

        var remainder = Redact(section, [LegitimateShare]);

        // The canary proving the redaction did not swallow the section: a
        // redaction that removes too much is the same defect in the opposite
        // coat (§12.8, WI-527).
        Assert.True(remainder.Length > section.Length - LegitimateShare.Length - 40,
            "the redaction removed far more than the one allowed sentence, so the share check "
            + "below is running on almost nothing");

        var shareShape = new Regex(
            @"(?i)\b(?:" + Fraction + @")\b(?:\W+\w+){0,4}\W+\b(?:of|the)\b"
            + @"|(?i)\b" + CountWord + @"\b(?:\W+\w+){0,3}\W+\bin (?:every )?"
            + @"(?:ten|a hundred|hundred)\b"
            + @"|(?i)\b" + CountWord + @"\b(?:\W+\w+){0,3}\W+\bout of\b");

        Assert.Matches(shareShape, "About a third of first appeals succeed.");
        Assert.Matches(shareShape, "Two out of three are refused.");

        var extras = shareShape.Matches(remainder).Select(m => m.Value).ToList();
        Assert.True(extras.Count == 0,
            "a second share has appeared in the insurance section: "
            + string.Join(" | ", extras));

        // AND PAGE-WIDE, because a section-scoped guard is a guard with a
        // door next to it (§12.8, WI-524). The harness put "reviewers side
        // with the patient in close to half of the proton cases they see"
        // into "How long does it take?" and everything above stayed green.
        // A FRACTION OF AN HOUR IS A DURATION, NOT A SHARE. §12.4 R1 puts
        // orienting durations firmly IN, and the page says "a quarter of an
        // hour to half an hour" in slot 4 — so the page-wide run needs the
        // time units excluded or it fires on the thing the standard requires.
        // §12.8 (WI-521) records the same collision the other way round: the
        // schedule guard and the frequency guard cannot share a vocabulary.
        var durations = new Regex(
            @"(?i)\b(?:" + Fraction + @"|" + CountWord + @")\b(?:\W+\w+){0,2}\W+"
            + @"\b(?:hours?|minutes?|days?|weeks?|months?|years?|an hour)\b");

        // Checked against the CONTEXT, not against `m.Value`. The share shape
        // matches "quarter of" and stops there, so a duration test run on the
        // matched text alone can never see the "an hour" that follows it —
        // which is §12.8 (WI-520)'s "selecting on a word the sentence next
        // door also contains", one step along.
        var scannedPage = Redact(Plain, [LegitimateShare]);
        var pageWide = shareShape.Matches(scannedPage)
            .Where(m => !durations.IsMatch(
                scannedPage[m.Index..Math.Min(scannedPage.Length, m.Index + m.Length + 25)]))
            .Select(m => m.Value)
            .ToList();

        // The canary, proving the duration exclusion did not swallow the
        // guard: a real share in the same shape must still be caught.
        Assert.True(
            shareShape.Matches("About a third of first appeals succeed.")
                .Any(m => !durations.IsMatch("About a third of first appeals succeed.")),
            "the duration exclusion has swallowed the share check");

        Assert.True(pageWide.Count == 0,
            "a share has appeared somewhere else on the page. There is one on this page "
            + "and it is in the insurance section: " + string.Join(" | ", pageWide));
    }

    [Fact]
    public void TheFourThingsThatStrengthenAnAppealArePresentAndCounted()
    {
        // The paper's own qualitative finding, and the most actionable content
        // on the page. Counted by marker (§12.8, WI-526), because a fifth added
        // as prose leaves "four things" and the list disagreeing.
        var section = RawSection(InsuranceHeading);
        var items = Regex.Matches(section, @"(?m)^\s*[-*]\s").Count;
        Assert.True(items == 4,
            $"the appeal list has {items} items and the page says four.");

        var plain = PlainOf(InsuranceHeading);
        Assert.Matches(@"(?i)\bFour things make an appeal stronger\b", plain);
        Assert.Matches(@"(?i)\bpublished guidelines\b", plain);
        Assert.Matches(@"(?i)\bnames the studies\b", plain);
        Assert.Matches(@"(?i)\beligible for a trial\b", plain);
        Assert.Matches(@"(?i)\bnot a form letter\b", plain);
    }

    [Fact]
    public void TheMedicareAnswerKeepsTheAdvantageQualifier()
    {
        // §12.6: keep the qualifier, shorten it. The review's own sentence is
        // "Medicare does not require PA for RT" followed immediately by
        // "Meanwhile, Medicare Advantage ... often gatekeeps treatments through
        // a PA process similar to private insurance." Printing the first half
        // alone tells a Medicare Advantage reader their plan is breaking a rule
        // — WI-525's shape, where both halves live in one document.
        var section = PlainOf(InsuranceHeading);
        Assert.Matches(@"(?i)\bMedicare does not require this kind of approval\b", section);
        Assert.Matches(@"(?i)\bMedicare Advantage plans are run by\s+insurers and often do\b",
            section);
        Assert.Matches(@"(?i)\bpeople use the word Medicare for both\b", section);
    }

    [Fact]
    public void NoDenialOrAbandonmentFrequencyIsPublished()
    {
        // PMC11699354 is a REVIEW, and its "64% of adult patients faced initial
        // denials", "32% remained denied following appeal" and "19% of those
        // denied abandoning radiation treatment altogether" are its summary of
        // somebody else's paper — the bundled-citation shape §12.8 (WI-512)
        // records three times. None of them is on the page in any form.
        // WIDENED AFTER /review BEAT IT THREE WAYS, and two of the beating
        // sentences published the exact figures this test names in its own
        // comment. The verb list was three literals and the corpus's plainest
        // words for the thing — "turned down", "said no" — were not on it.
        const string Quantity = @"(?:" + CountWord + @"|" + Fraction + @"|most|nearly all)";
        const string Refused =
            @"(?:are|were|get|got)\s+(?:turned down|knocked back|refused|denied)"
            + @"|turned down|knocked back|said no|come back as a no|came back as a no"
            + @"|give up|gave up|gives up|abandon(?:ed|s)?|walk(?:ed|s)? away"
            + @"|stopp?(?:ed|s)?\s+(?:radiation|treatment|having it)|go(?:es)? without";

        var guard = new Regex(
            @"(?i)\b" + Quantity + @"\b(?:\W+\w+){0,6}\W+\b(?:" + Refused + @")\b"
            + @"|(?i)\b(?:denials?|refusals?)\b(?:\W+\w+){0,5}\W+\b" + Quantity
            + @"\b(?:\W+\w+){0,3}\W+\b(?:of|in)\b"
            + @"|(?i)\ba no is a normal (?:step|part)\b");

        Assert.Matches(guard, "About two thirds of adults are denied at first.");
        Assert.Matches(guard, "Nearly one in five gave up on radiation altogether.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "Two out of three adults are turned down the first time.");
        Assert.Matches(guard, "Nineteen in every hundred stopped radiation altogether.");
        Assert.Matches(guard, "About two thirds of first requests come back as a no.");

        // And the one the page itself carried: "A no is a normal step in this
        // process" is the 64%-denied figure in words, with no attribution at
        // all (§12.14, and §12.8 WI-511: a number written as a word is still a
        // number).
        Assert.Matches(guard, "A no is a normal step in this process.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a denial frequency has reached the page and it is a review's summary of a paper "
            + "this project has not read: " + string.Join(" | ", hits));
    }

    // ----------------------------------------------------- the sibling's page

    [Fact]
    public void ThePageStatesTheDayOnceAndThenRoutesRatherThanRestatingIt()
    {
        // The scope line. ACS says the proton day IS the photon day, so this
        // page gives the shape and routes — §12.8 (WI-509)'s rule that a
        // heading which asks a question and answers only "see that other page"
        // is the trap, so slot 3 answers itself first.
        var steps = Regex.Matches(RawSection(StepsHeading), @"(?m)^\s*\d+\.\s").Count;
        Assert.True(steps >= 5, $"the step list has {steps} steps and cannot answer its heading.");

        Assert.Contains("/treatments/radiation-therapy", RawSection(StepsHeading),
            StringComparison.Ordinal);

        // And it names the two things that genuinely differ, which is the only
        // reason the slot is here rather than deleted.
        var section = PlainOf(StepsHeading);
        Assert.Matches(@"(?i)\bThe first difference is the room\b", section);
        Assert.Matches(@"(?i)\bThe second is the paperwork\b", section);
        Assert.Matches(@"(?i)\bsit in a chair rather than lie on a table\b", section);
    }

    [Fact]
    public void ThePageDoesNotRestateTheMaskOrTheTirednessCurveOrTheSkinRules()
    {
        // §12.10, the WI-514 blast radius in miniature. Every one of these is
        // owned, in detail, by /treatments/radiation-therapy, and a second copy
        // is a second copy to keep in step.
        //
        // The sibling is READ rather than hard-coded, so a softening there goes
        // red here (§12.8, WI-525).
        var sibling = Sibling("treatments", "radiation-therapy.md");
        foreach (var owned in new[]
                 {
                     "molded to your face", "warm washcloth", "laser lines",
                     "somnolence syndrome", "hippocampal avoidance",
                 })
        {
            Assert.Contains(owned, sibling, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(owned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // And the page does not grow its own skin-care instructions, which is
        // the likeliest thing to be added later by somebody being helpful.
        var guard = new Regex(
            @"(?i)\bno perfume\b|(?i)\belectric razor\b|(?i)\badhesive bandages?\b"
            + @"|(?i)\bpat dry\b|(?i)\btight hats\b");
        Assert.Matches(guard, "Use an electric razor and stop if the skin gets sore.");
        Assert.DoesNotMatch(guard, Plain);
    }

    [Fact]
    public void ThePageCarriesNoEscalationList()
    {
        // §12.8 (WI-512): an escalation tier is a site-wide property. Nothing
        // about a proton beam escalates differently from a photon beam, and
        // /treatments/radiation-therapy already carries the same-day list for
        // radiation to the head. A second list here would be a third thing to
        // keep in step.
        //
        // Structural rather than a phrase ban (§12.8, WI-526): an
        // urgency-flavoured paragraph followed by two or more symptom bullets.
        CuratedPage.AssertNoEscalationList(Page, Slug);
    }

    // -------------------------------------------------------- source discipline

    [Fact]
    public void NoMarketingOrContentFarmSourceIsCited()
    {
        // The dossier's access section rests on a hospital blog, a law firm's
        // blog, a trade directory and a content farm. Every one is replaced,
        // and the replacement is named in the front matter next to the ban
        // (§12.14).
        var urls = CitedUrls();
        Assert.True(urls.Split('\n').Length >= 6,
            "fewer than six source URLs were read out of the front matter, so this ban is "
            + "checking almost nothing");

        foreach (var banned in new[]
                 {
                     "scienceinsights.org", "beckersoncology.com", "protonbob.com",
                     "proton-therapy.org", "floridaproton.org", "debofsky.com",
                     "mdproton.com", "redjournal.org", "mdpi.com", "clinicaltrials.gov",
                     "academic.oup.com", "researchgate.net", "thegreenjournal.com",
                     "cco.amegroups.org",
                 })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        // And the replacements are actually there, so the ban is not standing
        // in for a deleted claim.
        foreach (var required in new[] { "PMC13521102", "PMC11905844", "PMC11699354", "PMC7904519" })
        {
            Assert.Contains(required, urls, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void NoNciPatientPdqUrlIsCited()
    {
        // §12.1, and the pattern rather than the domain (§12.8, WI-528): a
        // page may legitimately cite cancer.gov for framing, so a banned-domain
        // loop structurally cannot express this rule.
        var urls = CitedUrls();
        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);
        Assert.DoesNotMatch(@"(?i)NBK66023", urls);
    }

    [Fact]
    public void NoAgeRangeForWhoIsOfferedProtonTherapyIsPublished()
    {
        // The dossier takes "typical trial eligibility ages 1-25" from a
        // ClinicalTrials.gov record. §12.8 (WI-522): practice is not evidence,
        // and the reverse holds too — a trial's inclusion criteria are not a
        // statement about who is offered a treatment in a clinic.
        // `age[ds]?`, not `ages?`. The first version could not match its own
        // canary — "people AGED one to twenty-five" — which is §12.8 (WI-520)'s
        // lesson in miniature: the dossier's own phrasing is the one the guard
        // has to catch, and an inflection is a door (§12.8, WI-527).
        // WIDENED AFTER /review BEAT IT THREE WAYS: a comparative with no
        // `age` token, a bare `from N to N` range, and `twenties`, which does
        // not contain the string `twenty`.
        var guard = new Regex(
            @"(?i)\bage[ds]?\b(?:\W+\w+){0,4}\W+\b" + CountWord + @"\b"
            + @"|(?i)\b" + CountWord + @"\b(?:\W+\w+){0,2}\W+\byears? old\b"
            + @"|(?i)\b(?:younger than|older than)\s+" + CountWord + @"\b"
            + @"|(?i)\b(?:under|over|up to)\s+the age of\s+" + CountWord + @"\b"
            // A population noun next to a bare threshold IS an age claim,
            // and it is how the dossier phrases it. Without this branch the
            // bare `under|over` had to be banned outright, which fired on
            // "over six years" and on "over four in every ten".
            + @"|(?i)\b(?:children|kids|adults|people|patients|teenagers)\s+"
            + @"(?:aged\s+)?(?:under|over)\s+" + CountWord + @"\b"
            + @"|(?i)\bbetween the ages of\b"
            + @"|(?i)\bfrom\s+" + CountWord + @"\s+to\s+" + CountWord + @"\b"
            + @"|(?i)\b(?:twenties|thirties|forties|fifties|sixties|seventies)\b");

        Assert.Matches(guard, "It is offered to people aged one to twenty-five.");
        Assert.Matches(guard, "Children under 25 are the main group.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "Most people treated with protons are younger than twenty-five.");
        Assert.Matches(guard, "The trials generally took people from one to twenty-five.");
        Assert.Matches(guard, "It is mostly given to children and people in their early "
            + "twenties.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an age range has reached the page: " + string.Join(" | ", hits));
    }

    [Fact]
    public void NoPrognosisFigureOrSurvivalClaimAppearsAnywhere()
    {
        // §12.2 item 5 and §12.5.
        // WIDENED AFTER /review BEAT IT THREE WAYS. §12.8 (WI-527) already
        // records that a digit ban is not a prognosis ban, and this was a
        // digit-and-phrase ban: "still doing well years later", "adds years"
        // and a bare "survival" all walked.
        var guard = new Regex(
            @"(?i)\blife expectancy\b|(?i)\bmedian survival\b|(?i)\bsurvival\b"
            + @"|(?i)\bfive[- ]year\b|(?i)\bhow long (?:you|people|they) (?:will )?live\b"
            + @"|(?i)\b(?:live|living|lived|lives)\s+(?:for\s+)?(?:longer|" + CountWord + @")\b"
            + @"|(?i)\bstill (?:doing well|alive|here|with us|around)\b"
            + @"|(?i)\badds?\s+(?:\w+\s+){0,2}(?:months|years)\b"
            + @"|(?i)\bcures?\b(?![\s\w]{0,12}\bnot\b)");

        Assert.Matches(guard, "People live longer with protons.");
        Assert.Matches(guard, "The five-year figures are better.");

        // The three sentences /review used to beat the first version.
        Assert.Matches(guard, "Most people in that group are still doing well years later.");
        Assert.Matches(guard, "It adds years for the tumors it suits.");
        Assert.Matches(guard, "Survival is the same either way.");

        var hits = guard.Matches(Plain).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "a prognosis claim has reached the page: " + string.Join(" | ", hits));
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

    // ------------------------------------------------------- the caregiver half

    [Fact]
    public void TheCaregiverSectionComposesTheBlockAndAddsOnlyWhatIsTrueHere()
    {
        // §12.7. What this treatment asks of a caregiver that ordinary
        // radiation does not is the weeks away from home and the paperwork —
        // so those are the page's own half, and the rest routes.
        Assert.Matches(@"(?m)^\[CAREGIVER\]\s*$", Page);

        var section = PlainOf(CaregiverHeading);
        Assert.Matches(@"(?i)\bThis one may take them away from home\b", section);
        Assert.Matches(@"(?i)\bTake the paperwork off them if you can\b", section);
        Assert.Matches(@"(?i)\bDo not let a refusal letter be the last word\b", section);

        // §12.8 (WI-525, WI-526, WI-527): count the section's own claims, by
        // paragraph-leading bold, because a paraphrase is the failure mode a
        // shingle check cannot see.
        var claims = Regex.Matches(RawSection(CaregiverHeading), @"(?m)^\*\*").Count;
        Assert.True(claims == 5,
            $"the caregiver section makes {claims} claims rather than five.");
    }

    [Fact]
    public void TheCaregiverSectionDoesNotRepeatTheBlocksOwnSentences()
    {
        // §12.8 (WI-510): word shingles over the composed section, because a
        // bold-lead-in check is the version §12.8 already records as
        // insufficient.
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
    public void NoUnsourcedShareOfPeopleAppearsAnywhereOnThePage()
    {
        // PORTED FROM WI-524, and it should have been here from the first
        // draft. /review found two on this page that no other guard could see:
        // "Many of them have lodging schemes" (a claim about how many centers
        // do a thing) and "People are often left to guess at all three".
        //
        // §12.8 (WI-524): the exemptions are a short named allowlist with the
        // source quote that earns each one.
        string[] allowed =
        [
            // ACS, verbatim: "Most people have daily treatment sessions
            // Monday-Friday for several weeks."
            "Most people have daily treatment sessions",
            // The page's own refusal, which contains the shape it refuses.
            "This is the part that decides it for most people",
        ];

        var scanned = Redact(Plain, allowed);

        // "often" and "usually" are NOT here. Both fired on correct prose:
        // "the hard part is usually not the treatment" is a hedge, not a
        // frequency, and banning a hedge pushes a page toward stating
        // things flatly, which is the opposite of what §12.6 asks for.
        const string Share =
            @"(?:most|nearly all|almost (?:all|everybody|everyone|nobody)|"
            + @"the (?:great |vast )?majority|a minority|plenty of|a lot of|hardly any|"
            + Fraction + @")";

        var guard = new Regex(
            @"(?i)\b" + Share + @"\b(?:\W+\w+){0,3}\W+"
            + @"\b(?:people|patients|readers|centers|plans|of us|of you)\b"
            // "many of them" carries its own population, so it needs its
            // own branch: the version above wants a NOUN after the
            // quantifier and this shape has already used it up.
            // "so few of them" and "There are not many of them" are about
            // CENTERS, which the paragraph names two sentences earlier, and
            // they are the page's subject rather than a claim about
            // people. The population has to be named for this branch.
            + @"|(?i)\b(?:many|most|plenty|a lot|few|several) of "
            + @"(?:them|these) (?:people|patients|readers)\b"
            + @"|(?i)\bpeople are (?:often|usually|generally)\b"
            + @"|(?i)\bis (?:not )?(?:usual|common|unusual|uncommon|rare)\b");

        Assert.Matches(guard, "Many of these people have lodging schemes.");

        // The correct sentences the first version failed.
        Assert.DoesNotMatch(guard, "There are not many of them.");
        Assert.DoesNotMatch(guard, "The hard part is usually not the treatment.");
        Assert.Matches(guard, "People are often left to guess.");
        Assert.Matches(guard, "Most plans cover it.");
        Assert.Matches(guard, "Refusal is not uncommon.");

        var hits = guard.Matches(scanned).Select(m => m.Value).ToList();
        Assert.True(hits.Count == 0,
            "an unsourced share has reached the page. Either cite it and add it to the "
            + "allowlist WITH the source quote, or rewrite it as a conditional: "
            + string.Join(" | ", hits));
    }

    [Fact]
    public void ThePageDoesNotRestateWhatAnotherPageAlreadyOwns()
    {
        // §12.8 (WI-521, WI-530). A corpus-wide shingle run.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);
    }

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        // §12.8 (WI-511, WI-529, WI-530). Run on `Plain`, not `Body`, so the
        // description is in scope (§12.8, WI-528).
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
    public void ThePageCarriesNoAuthoringMarkersAtAll()
    {
        // §12.8 (WI-512): a suppression is only meaningful if the word is there
        // to suppress. A first draft carried four markers and every one was a
        // no-op: this page does not DEFINE radiation oncologist, and it never
        // says "simulation", "radiation mask" or "fractionation" at all.
        //
        // §12.8 (WI-529): and if one were added, it must not share a line with
        // a block directive, because the composer matches a directive only on a
        // line of its own and the marker makes it fail OPEN.
        //
        // Scoped to the BODY, not the whole file: a future front-matter ruling
        // that quotes a marker in order to explain why it is not used would
        // fail a correct page, which is the §12.8 shape this suite keeps
        // running into.
        Assert.DoesNotMatch(@"!%", CuratedPage.Body(Page));

        var offenders = Regex.Matches(Page, @"(?m)^.*!%.*\[[A-Z-]+\].*$")
            .Select(m => m.Value).ToList();
        Assert.True(offenders.Count == 0,
            "an authoring marker shares a line with a block directive: "
            + string.Join(" | ", offenders));
    }
}

/// <summary>
/// WI-531: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class ProtonTherapyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/proton-therapy";

    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// THE CONNECTION STRING HAS TO BE PUSHED IN, and a bare
    /// <c>WebApplicationFactory&lt;Program&gt;</c> passes locally while failing
    /// in CI. A developer machine has the connection string in
    /// <c>dotnet user-secrets</c>, so the host boots and every render test is
    /// green; the CI runner has no user secrets, so <c>Program.Main</c> throws
    /// "Connection string 'BrainHarbor' not found" before a single page is
    /// served. Four tests on each of this item's two pages failed that way, and
    /// nothing on this machine could have caught it.
    ///
    /// Every other render class in the corpus does this; a primary constructor
    /// taking the factory straight through is what dropped it. Kept as an
    /// explicit constructor rather than a primary one so the wrapping is
    /// visible at the point somebody would otherwise delete it.
    /// </summary>
    public ProtonTherapyPageRenderTests(WebApplicationFactory<Program> raw) =>
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
        Assert.Contains("Protons are not stronger", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/treatments/radiation-therapy",
            "/treatments/stereotactic-radiosurgery",
            "/tests/mri",
            "/treatments/clinical-trials",
            "/get-help-now");
    }

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists()
    {
        // §12.8 (WI-513): `AssertLinksResolve`'s regex stops at the `#`, so a
        // link to a heading that does not exist comes back a healthy 200 while
        // landing the reader at the top of a long page. This page deep-links
        // into the sibling three times, which is what makes routing instead of
        // restating work at all.
        await CuratedPage.AssertFragmentLinksResolve(factory.CreateClient(), Url);
    }

    [Fact]
    public async Task TheDoorsOnTheSiblingPagesAreAppendedSentencesNearWhatTheyWereAppendedTo()
    {
        // §12.8 (WI-526): a door must sit near the sentence it was appended to,
        // or replacing the sibling's paragraph outright leaves the guard green.
        // §12.8 (WI-530): a door is prose on the page it lands on.
        (string Slug, string Pinned, string Door)[] doors =
        [
            ("treatments/radiation-therapy",
                "A different kind of particle, given at a small number of centers.",
                "/treatments/proton-therapy"),
            ("tumors/low-grade-glioma",
                "Radiation therapy: the mask, the daily visits, and what comes after",
                "/treatments/proton-therapy"),
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

    [Fact]
    public async Task TheDoorOnTheLowGradeGliomaHubCarriesTheUncertaintyWithIt()
    {
        // §12.8 (WI-530): a door added to a sibling is prose on that sibling,
        // and it can undo a previous item's fix. This door sits inside the
        // section where /tumors/low-grade-glioma weighs radiation's cost to
        // thinking and memory — the exact reader for whom protons are argued.
        // A bare link there would read as an endorsement the evidence does not
        // support, so the uncertainty travels with it.
        var sibling = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "low-grade-glioma.md")));

        Assert.Matches(@"(?i)\bthe trials that would settle it have not been done\b", sibling);

        // And the door has not quietly become a recommendation.
        var guard = new Regex(
            @"(?i)\bproton\b(?:\W+\w+){0,10}\W+"
            + @"\b(?:better|kinder|gentler|safer|fewer side effects|less damage)\b");
        Assert.Matches(guard, "Proton therapy is gentler on memory.");
        Assert.DoesNotMatch(guard, sibling);

        await Task.CompletedTask;
    }
}
