using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-556: <c>/treatments/fertility</c>. A §12.8 library page about a CONVERSATION
/// rather than a procedure, whose whole value is one instruction — ask before treatment
/// starts — and whose risk is a small set of claims the dossier stated more strongly
/// than any source does.
///
/// Pinned is where this page could leave a reader worse off than no page:
///
///   * LOSING THE WINDOW. The short version and the "why now" section say the choices
///     close once treatment begins, and that not being sure is reason enough to ask.
///   * OVERSTATING A SOURCE, five times over: ASCO says REFER the interested and the
///     unsure (NBTS says "every patient ... should be seen"); temozolomide's label speaks
///     to MEN'S fertility only and the American Cancer Society's high-risk list omits it;
///     spine radiation reaches the OVARIES (Kaneko 2026: "the testes are located outside
///     the CSI field"); no source gives a hormone protocol that is safe with a
///     meningioma; and no source says brain surgery spares fertility.
///   * A REASSURANCE WITH ITS CONDITION CUT OFF: hormone treatment after brain radiation
///     helps only if the ovaries or testicles were not damaged too, and not always.
///   * A SUCCESS RATE. The sources print several; a reader cannot weigh one alone.
///   * A CHILD LEFT OUT OF THEIR OWN DECISION.
///
/// Paragraphs and bullets are SLICED and asserted on, not the whole page, and every
/// pattern is written for <c>\r?\n</c> (the repo is CRLF locally and LF in CI).
/// </summary>
public sealed class FertilityPageContentTests
{
    private const string Slug = "treatments/fertility";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What does treatment have to do with having children?";
    private const string WhyHeading = "Why ask now, before treatment starts?";
    private const string WhichHeading = "Which treatments can affect it?";
    private const string StepsHeading = "What happens, step by step";
    private const string SpermHeading = "The choices, if you make sperm";
    private const string EggsHeading = "The choices, if you have ovaries";
    private const string ChildrenHeading = "Children and teens";
    private const string LongHeading = "How long does it take?";
    private const string DelayHeading = "Will it hold up my treatment?";
    private const string HormonesHeading = "What if my tumor is sensitive to hormones?";
    private const string HardHeading = "What is hard about it, and what can be done";
    private const string PregnancyHeading = "Getting pregnant during treatment";
    private const string AfterHeading = "After treatment: can I find out?";
    private const string WhoHeading = "Who does it, and how do I find out how it went?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("treatments", "fertility.md");

    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?[ \t]*\r?$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static string Clean(string raw) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderTextOfBody(raw)), @"[*_]", "").Trim();

    private static List<string> ParagraphsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n[ \t]*\r?\n|\r?\n(?=- |\d+\. )")
            .Select(p => Regex.Replace(p.TrimStart(), @"^(?:-|\d+\.)\s+", ""))
            .Select(Clean).Where(p => p.Length > 0)];

    private static string PlainOf(string heading) => Clean(RawSection(heading));

    private static string Plain =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.EverythingAReaderMeets(Page)), @"[*_]", "");

    private static string Paragraph(string heading, string containing)
    {
        var hits = ParagraphsOf(RawSection(heading))
            .Where(p => p.Contains(containing, StringComparison.Ordinal)).ToList();
        Assert.True(hits.Count == 1,
            $"expected exactly one paragraph in '{heading}' containing '{containing}', found {hits.Count}");
        return hits[0];
    }

    /// <summary>An attribution to one of the page's own cited sources, not any "says".</summary>
    private static readonly Regex NamedSource = new(@"\b(?:Society|Institute|Oncology|label) says\b");

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentInOrder()
    {
        var headings = Headings();
        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, WhichHeading, StepsHeading, SpermHeading,
            EggsHeading, ChildrenHeading, LongHeading, DelayHeading, HormonesHeading,
            HardHeading, PregnancyHeading, AfterHeading, WhoHeading, QuestionsHeading,
            NextHeading,
        ];

        Assert.Equal(required, headings);
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorAndThePublishedOnesAreStable()
    {
        var lines = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$").Select(m => m.Groups[1].Value.Trim()).ToList();
        Assert.Equal(17, lines.Count);
        Assert.All(lines, h => Assert.Matches(@"\{#[\w-]+\}$", h));

        // Deep-link targets for the treatment pages and hubs that route here.
        foreach (var anchor in new[] { "why-now", "which-treatments", "sperm", "eggs", "children", "delay", "pregnancy" })
        {
            Assert.Matches($@"(?m)^## .+ \{{#{anchor}\}}\s*$", Page);
        }
    }

    // ------------------------------------------------------- the one instruction

    [Fact]
    public void TheShortVersionSaysTheChoicesCloseAndThatNotBeingSureIsEnough()
    {
        var shortVersion = PlainOf(ShortHeading);
        // /review round 1: "only work before treatment starts" was absolute, and the page
        // itself says surgery is a treatment and the weeks after it are still in time.
        Assert.Contains("most of them work best before treatment starts", shortVersion, StringComparison.Ordinal);
        Assert.Contains("there may still be a faster choice", shortVersion, StringComparison.Ordinal);
        Assert.Contains("say so now, even if you are not sure", shortVersion, StringComparison.Ordinal);

        var why = PlainOf(WhyHeading);
        Assert.StartsWith("Because many choices are lost once chemotherapy or radiation begins.", why, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?i)\bchoices close\b|(?i)\bonly work before\b", Plain);
        Assert.Contains("You do not have to have made up your mind to ask.", why, StringComparison.Ordinal);
    }

    [Fact]
    public void AscoIsStatedAtItsOwnStrengthAndNotAtNbtss()
    {
        // ASCO 2025 rec 1.2: refer "patients who express an interest in fertility
        // preservation, and those who are uncertain". NBTS restates it as everybody
        // "should be seen by a reproductive specialist", which ASCO does not say.
        var refer = Paragraph(WhyHeading, "fertility specialist. So should anyone who is not sure.");
        Assert.Contains("anyone who wants to save eggs or sperm should be sent", refer, StringComparison.Ordinal);

        var overstatement = new Regex(
            @"(?i)\b(?:every|all|everyone|everybody)\b[^.]{0,60}\b(?:should|must|needs? to)\b[^.]{0,20}\b(?:see|be seen|be sent|meet|be referred|get a referral|be offered)\b");
        Assert.Matches(overstatement, "Every patient should be seen by a reproductive specialist.");
        Assert.Matches(overstatement, "All patients should be referred to a fertility specialist.");
        Assert.DoesNotMatch(overstatement, Plain);
    }

    [Fact]
    public void TheRegretFindingIsAttributedAndKeepsItsWhetherOrNot()
    {
        // Letourneau 2012 studied women with other cancers; NCI states it unscoped, so
        // it is NCI's sentence, in NCI's name, with the half that helps the unsure reader.
        var regret = Paragraph(WhyHeading, "fewer regrets");
        Assert.Contains("The National Cancer Institute says", regret, StringComparison.Ordinal);
        Assert.Contains("whether or not they went on to save anything", regret, StringComparison.Ordinal);
    }

    // ------------------------------------------------------- the overstatements

    [Fact]
    public void TemozolomideIsNeverSaidToHarmEggsAndLomustineCarriesTheListClaim()
    {
        var chemo = Paragraph(WhichHeading, "Chemotherapy.");
        Assert.Contains("Lomustine is on the American Cancer Society's list", chemo, StringComparison.Ordinal);
        Assert.Contains("Temozolomide is in the same family of drugs.", chemo, StringComparison.Ordinal);
        // /review round 1 (BLOCKER): the label's male-only sentence on its own left the one
        // drug-specific fact a woman met about MEN. Both halves of the label, and the egg
        // harm in NBTS's name, in the same paragraph.
        Assert.Contains("Its label says it may affect a man's fertility, and it says nothing either way about eggs.",
            chemo, StringComparison.Ordinal);
        Assert.Contains("The National Brain Tumor Society says drugs in this family are the most harmful to eggs.",
            chemo, StringComparison.Ordinal);
        Assert.Contains("So if you have ovaries, ask about temozolomide too.", chemo, StringComparison.Ordinal);

        // AND NO UNATTRIBUTED HARM-TO-EGGS CLAIM ANYWHERE, which is the shape a pronoun
        // ("It can harm eggs.") or a scope-widening edit arrives in. The first version
        // banned temozolomide and eggs in one sentence, which forbade the fix above and
        // let a pronoun in the next sentence walk past it. Two general statements are the
        // only sentences allowed to say it in the page's own voice.
        // /review round 2 widened the vocabulary: "dangerous" is NBTS's own word ("most
        // dangerous to a patient's egg count"), so an edit copied from the source is the
        // likeliest way back, and "lower/fewer/egg count" are the other shapes it takes.
        var harm = new Regex(
            @"(?i)\b(?:harm|harms|harmful|damage|damages|destroy|destroys|hurt|dangerous|toxic|lower|lowers|reduce|reduces|fewer|loss|lose)\b[^.]*\beggs?\b"
            + @"|(?i)\begg count\b");
        string[] general = ["Some treatments can harm eggs or sperm", "Some chemotherapy drugs can harm eggs and sperm"];
        Assert.Matches(harm, "It can harm eggs.");
        Assert.Matches(harm, "It is dangerous to eggs.");
        Assert.Matches(harm, "It lowers your egg count.");
        var unattributed = CuratedPage.SentencesOf(Plain)
            .Where(x => harm.IsMatch(x))
            .Where(x => !general.Any(g => x.Contains(g, StringComparison.Ordinal))) // a heading anchor can lead the sentence
            .Where(x => !NamedSource.IsMatch(x))
            .ToList();
        Assert.Matches(NamedSource, "The National Brain Tumor Society says drugs in this family are the most harmful to eggs.");
        Assert.DoesNotMatch(NamedSource, "Your doctor says it destroys eggs.");
        Assert.True(unattributed.Count == 0, "harm to eggs stated in the page's own voice:\n  " + string.Join("\n  ", unattributed));
    }

    [Fact]
    public void SpineRadiationNamesTheOvariesAndNotTheTesticles()
    {
        var spine = Paragraph(WhichHeading, "Radiation to the spine.");
        Assert.Contains("can reach the ovaries", spine, StringComparison.Ordinal);

        // §12.6: never end on the frightening sentence (/review round 2). Not "move an
        // ovary": ASCO scopes transposition to PELVIC radiation.
        Assert.EndsWith("Ask your team whether anything can protect them.", spine, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?i)\btest(?:icles?|es)\b", spine);

        // Page-wide too (/review round 1): the claim can arrive anywhere.
        Assert.DoesNotMatch(@"(?i)\bspin(?:e|al)\b[^.]*\btest(?:icles?|es)\b|(?i)\btest(?:icles?|es)\b[^.]*\bspin(?:e|al)\b", Plain);
    }

    [Fact]
    public void NoSentenceTellsTheReaderSurgeryLeavesFertilityAlone()
    {
        var surgery = Paragraph(WhichHeading, "Surgery.");
        Assert.Contains("near the pituitary gland or the hypothalamus", surgery, StringComparison.Ordinal);

        var reassurance = new Regex(
            @"(?i)\bsurgery\b[^.]{0,40}\b(?:does not|doesn't|will not|won't|cannot|can't|rarely|usually does not)\s+(?:affect|harm|change)\b"
            + @"|(?i)\bsurgery (?:alone|on its own|by itself)\b|(?i)\bsurgery is safe\b");
        Assert.Matches(reassurance, "Brain surgery on its own usually does not affect fertility.");
        Assert.DoesNotMatch(reassurance, Plain);
    }

    [Fact]
    public void TheMeningiomaHormoneQuestionIsLeftOpenAndRoutedToTheTeam()
    {
        var hormones = PlainOf(HormonesHeading);
        Assert.Contains("If you have a meningioma, nobody yet knows whether those hormones are safe for you.",
            hormones, StringComparison.Ordinal);
        Assert.Contains("/tumors/meningioma", RawSection(HormonesHeading), StringComparison.Ordinal);

        // ASCO's letrozole protocols are scoped to breast and gynecologic cancers.
        Assert.DoesNotMatch(@"(?i)\bletrozole\b|(?i)\baromatase\b", Plain);
        // Lookbehind for "whether": the page's own open question is the correct sentence.
        var reassured = new Regex(
            @"(?i)(?<!whether (?:those |the )?)\bhormones? (?:are|is) safe\b|(?i)\bsafe (?:to|for you to) (?:take|have) (?:the )?hormone");
        Assert.Matches(reassured, "For a meningioma, the hormones are safe.");
        Assert.DoesNotMatch(reassured, Plain);

        // And /tumors/meningioma's own line is still the one this page is matching (§12.10).
        Assert.Contains("**Fertility treatment.** There is not enough information to say either way.",
            CuratedPage.Read("tumors", "meningioma.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheHormoneFixAfterRadiationKeepsBothOfItsLimits()
    {
        var fix = Paragraph(AfterHeading, "hormone treatment can help fix this");
        Assert.Contains("as long as the ovaries or testicles were not damaged too", fix, StringComparison.Ordinal);
        Assert.EndsWith("It does not always solve it.", fix, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?i)\beasy to fix\b|(?i)\bcan (?:always|easily) be fixed\b", Plain);
    }

    [Fact]
    public void TheOvarianRestingShotIsNeverASubstitute()
    {
        var shot = Paragraph(EggsHeading, "A shot that rests the ovaries during chemotherapy.");
        Assert.Contains("should not be used in place of freezing eggs, embryos or tissue", shot, StringComparison.Ordinal);

        var male = Paragraph(SpermHeading, "is not a way to save fertility");
        Assert.Contains("Cancer doctors say it does not work.", male, StringComparison.Ordinal);
    }

    // ------------------------------------------------------- numbers

    [Fact]
    public void NoSuccessRateOrPercentageReachesThePage()
    {
        var rate = new Regex(
            @"\d\s*%|(?i)\bper ?cent\b|(?i)\b\d+ (?:in|out of) \d+\b|(?i)\bsuccess rate\b|(?i)\blive births?\b"
            + @"|(?i)\bhalf (?:of )?the time\b"
            + @"|(?i)\b(?:one|two|three|four|five|six|seven|eight|nine) (?:in|out of) (?:two|three|four|five|six|seven|eight|nine|ten)\b");
        Assert.Matches(rate, "Ovarian transposition is successful about half of the time.");
        Assert.Matches(rate, "It works nine in ten times.");
        Assert.DoesNotMatch(rate, Plain);
        Assert.Contains("No method works every time.", PlainOf(HardHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryDollarFigureIsAttributedAndDated()
    {
        var withMoney = ParagraphsOf(RawSection(HardHeading)).Where(p => p.Contains('$')).ToList();
        Assert.Single(withMoney);
        Assert.Contains("The American Cancer Society says, as of 2025", withMoney[0], StringComparison.Ordinal);

        Assert.DoesNotContain("$", Plain.Replace(withMoney[0], " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Matches(@"(?m)^volatile_figures: true\s*$", CuratedPage.FrontMatter(Page));
    }

    [Fact]
    public void TheBirthControlDurationsAreTheLabelsAndSayWhose()
    {
        var label = Paragraph(PregnancyHeading, "6 months");
        Assert.StartsWith("Temozolomide's label says", label, StringComparison.Ordinal);
        Assert.Contains("For men, it says to use condoms during treatment and for 3 months after", label, StringComparison.Ordinal);

        // The other drugs' periods are not verified here, so they are routed, not printed.
        Assert.Contains("The time differs from drug to drug", PlainOf(PregnancyHeading), StringComparison.Ordinal);
        var months = Regex.Matches(Plain, @"(?i)\b\d+ months?\b").Count;
        Assert.Equal(2, months);
    }

    [Fact]
    public void TheSeizureMedicineLineNeverTellsAnyoneToStop()
    {
        var seizure = Paragraph(PregnancyHeading, "seizure medicine");
        Assert.Contains("Do not change or stop one on your own.", seizure, StringComparison.Ordinal);
        Assert.Contains("/treatments/anti-seizure-medicines", RawSection(PregnancyHeading), StringComparison.Ordinal);
    }

    // ------------------------------------------------------- children

    [Fact]
    public void AChildIsPartOfTheirOwnDecision()
    {
        var who = Paragraph(ChildrenHeading, "A parent or guardian gives permission.");
        Assert.Contains("The child should agree too.", who, StringComparison.Ordinal);
        var overrule = new Regex(@"(?i)\b(?:parents?|guardians?)\b(?: or (?:a )?guardians?)? (?:decides?|makes? the decision|chooses?)\b");
        Assert.Matches(overrule, "A parent or guardian decides.");
        Assert.DoesNotMatch(overrule, Plain);

        var boys = Paragraph(ChildrenHeading, "Boys.");
        Assert.Contains("there is not yet a proven way", boys, StringComparison.Ordinal);
        Assert.Contains("being tried in research studies", boys, StringComparison.Ordinal);

        // /review round 1: ACS conditions the reassurance on an assessment, so the
        // assessment comes FIRST, in the same paragraph.
        var girls = Paragraph(ChildrenHeading, "Girls.");
        var ask = girls.IndexOf("Ask the team whether her treatment puts her at high risk.", StringComparison.Ordinal);
        var reassure = girls.IndexOf("Many are not at high risk", StringComparison.Ordinal);
        Assert.True(ask >= 0 && reassure > ask, "the 'not at high risk' reassurance no longer follows the assessment: " + girls);
    }

    [Fact]
    public void TheOneStudysDelayFindingSaysBothOfTheWomen()
    {
        // Grigorian 2026: "9 (16%) undergoing preservation (7 male, 2 female)" and "slight
        // delays in adjuvant therapy for two female patients" -- every woman who did it.
        // "two of the women" reads as two of many (/review round 1).
        // Round 2: "saved their fertility" promised more than the study measured, and
        // "one US study" read as a second study rather than the one cited above.
        var delay = Paragraph(DelayHeading, "In that same small US study");
        Assert.Contains("both of the women who went ahead with it had their treatment held up a little", delay,
            StringComparison.Ordinal);
    }

    // ------------------------------------------------------- doors in

    [Theory]
    [InlineData("start.md")]
    [InlineData("treatments/chemotherapy.md")]
    [InlineData("treatments/radiation-therapy.md")]
    [InlineData("tumors/cns-germ-cell-tumor.md")]
    public void ThePagesThatRaiseFertilityRouteHere(string page)
    {
        Assert.Contains("](/treatments/fertility)",
            CuratedPage.Read(page.Split('/')), StringComparison.Ordinal);
    }

    // ------------------------------------------------------- source discipline

    [Fact]
    public void TheSourcesThatAreGoneOrOutOfScopeAreNotCited()
    {
        var urls = CitedUrls();
        Assert.Equal(20, urls.Split('\n').Length);

        foreach (var banned in new[] { "cancer.net", "NBK66023", "medlineplus.gov/druginfo", "academic.oup.com", "ascopubs.org" })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(@"(?i)cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);
        Assert.Contains("ASCO-guidelines.pdf", urls, StringComparison.Ordinal);
        Assert.Contains("dailymed.nlm.nih.gov", urls, StringComparison.Ordinal);
        Assert.Contains("PMC13271858", urls, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count);
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count);
        Assert.Matches(@"(?m)^disclaimers: \[medical\]\s*$", front);
    }

    // ------------------------------------------------------- corpus housekeeping

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn() =>
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

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
        Assert.Empty(CuratedPage.BritishIdiomsIn(Plain));
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
}

/// <summary>
/// WI-556: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class FertilityPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/fertility";

    private readonly WebApplicationFactory<Program> factory;

    public FertilityPageRenderTests(WebApplicationFactory<Program> raw) =>
        factory = raw.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedWithItsAnchors()
    {
        var response = await factory.CreateClient().GetAsync(Url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.Contains("id=\"why-now\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"pregnancy\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/treatments/chemotherapy",
            "/treatments/radiation-therapy",
            "/treatments/proton-therapy",
            "/treatments/craniotomy",
            "/tumors/cns-germ-cell-tumor",
            "/get-help-now");
    }

    [Fact]
    public async Task TheStartPageIsADoorForTheNewlyDiagnosedReader()
    {
        var html = await factory.CreateClient().GetStringAsync("/start");
        Assert.Contains("href=\"/treatments/fertility\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheProcarbazineTooltipFiresHereWhereTheWordIsNotDefined()
    {
        // The page names procarbazine on the ACS list and does not explain it, so this is
        // the reader the entry exists for — and why GlossaryOnlyEntriesTests lost a record.
        var html = await factory.CreateClient().GetStringAsync(Url);
        Assert.Contains("def-procarbazine", html, StringComparison.Ordinal);
    }
}
