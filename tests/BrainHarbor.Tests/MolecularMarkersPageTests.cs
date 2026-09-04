using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-509: T6b, the molecular marker glossary. The fourth page under the §12.8
/// library template (shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs).
///
/// The prose is reviewed, not tested. What is pinned here is the editorial
/// spine — describe what is measured, never characterise a result — and the two
/// structures other pages depend on: the fifteen entries and their anchors.
/// </summary>
public sealed class MolecularMarkersPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "molecular-markers.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The fifteen markers the item lists, each with the anchor it is published
    /// at. Both halves are the contract: the entry has to exist, and it has to
    /// stay reachable at the same id.
    /// </summary>
    public static readonly (string Heading, string Anchor)[] Markers =
    [
        ("IDH", "idh"),
        ("1p/19q co-deletion", "p-1p-19q"),
        ("ATRX", "atrx"),
        ("TERT promoter", "tert"),
        ("CDKN2A/B", "cdkn2a-b"),
        ("EGFR", "egfr"),
        ("Chromosome 7 and chromosome 10", "chromosome-7-and-10"),
        ("H3 K27-altered", "h3-k27"),
        ("H3 G34", "h3-g34"),
        ("BRAF", "braf"),
        ("MGMT promoter methylation", "mgmt"),
        ("Ki-67", "ki-67"),
        ("TP53", "tp53"),
        ("Gene panels", "gene-panel"),
        ("Methylation profiling", "methylation-profiling"),
    ];

    /// <summary>
    /// One "### Heading {#anchor}" entry and its body, flattened. `\s*$` and
    /// never a bare `$`: .NET's multiline `$` does not match before `\r`, and
    /// this repo has core.autocrlf=true, so an anchored pattern matches nothing
    /// on a real Windows checkout while staying green on CI's LF (WI-501,
    /// WI-506, WI-507).
    /// </summary>
    private static string Entry(string heading)
    {
        var match = Regex.Match(
            Page, $@"^### {Regex.Escape(heading)}\s*\{{#[^}}]+\}}\s*$(.*?)(?=^#{{2,3}} |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '### {heading}' entry");
        return Regex.Replace(match.Groups[1].Value, @"\s+", " ").Trim();
    }

    [Fact]
    public void NoResultOnThisPageIsCharacterisedAsGoodOrBadNews()
    {
        // THE SPINE, and this is the page the shared list was written for.
        // Under CNS5 these tests RENAME the diagnosis; they do not predict. The
        // sources say otherwise out loud and in the reader's own vocabulary, so
        // the check is on the words, not on the intent — borrowed phrasing is
        // how it lands on a page, not a decision anyone makes.
        var body = CuratedPage.Body(Page);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5 and §12.4. The Ki-67 entry has to say the result is
        // reported as a percentage without ever printing one, which is the
        // narrow line this test holds it to.
        //
        // ReaderText, not Body: the tooltip-suppression markers put "34%" into
        // the source (`!%H3 G34%!%BRAF%`) and this test found it. A prose rule
        // has to be asserted against what renders, not against the authoring
        // machinery that never reaches a reader.
        var body = CuratedPage.ReaderText(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[]
                 { "survival", "life expectancy", "five-year", "prognosis", "how long you have" })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void EveryMarkerInTheItemHasAnEntryAndTheJumpListMatchesItExactly()
    {
        // The item names thirteen markers plus panels and methylation
        // profiling. Both halves matter: an entry that loses its jump-list link
        // is unreachable by a scanning reader, and a jump-list link with no
        // entry drops them at the top of a long page.
        var body = CuratedPage.Body(Page);

        var entryAnchors = Regex.Matches(body, @"^### .+?\{#([^}]+)\}\s*$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value).ToList();

        var jumpTargets = Regex.Matches(CuratedPage.Section(Page, "The words, one by one"), @"\]\(#([^)]+)\)")
            .Select(m => m.Groups[1].Value).ToList();

        Assert.Equal(Markers.Select(m => m.Anchor), entryAnchors);
        Assert.Equal(Markers.Select(m => m.Anchor), jumpTargets);

        foreach (var (heading, _) in Markers)
        {
            Assert.NotEmpty(Entry(heading));
        }
    }

    [Fact]
    public void EveryEntryHeadingPinsItsOwnAnchorRatherThanLettingMarkdigDeriveOne()
    {
        // §12.8: heading anchors are a published interface once other pages
        // deep-link them. Markdig derives an id from the heading TEXT, so a
        // reworded heading silently breaks every inbound link and the reader
        // lands at the top of a long page with no sign anything went wrong.
        // Writing the id explicitly makes the two independent, and this test is
        // what stops the next entry being added without one.
        foreach (var heading in Regex.Matches(CuratedPage.Body(Page), @"^### (.+)$", RegexOptions.Multiline)
                     .Select(m => m.Groups[1].Value.Trim()))
        {
            Assert.Matches(@"\{#[a-z0-9-]+\}$", heading);
        }
    }

    [Fact]
    public void EveryEntryAnswersAllThreeQuestionsInOrder()
    {
        // The item's shape: what it is / what your team does with it / what it
        // does not mean. The third is the anti-hype guardrail and the one most
        // likely to be quietly dropped as "obvious", so it is checked for
        // substance and not only for its label.
        foreach (var (heading, _) in Markers)
        {
            var entry = Entry(heading);

            var measured = entry.IndexOf("**What is measured.**", StringComparison.Ordinal);
            var uses = entry.IndexOf("**What your team does with it.**", StringComparison.Ordinal);
            var limits = entry.IndexOf("**What it does not tell you.**", StringComparison.Ordinal);

            Assert.True(measured == 0, $"'{heading}' does not open with what is measured");
            Assert.True(uses > measured, $"'{heading}' is missing what the team does with it");
            Assert.True(limits > uses, $"'{heading}' is missing what it does not tell you");

            // A label with a stub under it satisfies every ordering assertion
            // above and is exactly the shape a tightening pass produces.
            var closing = entry[limits..].Split("**")[^1];
            Assert.True(closing.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 12,
                $"'{heading}' says what it does not tell you in under twelve words");
        }
    }

    [Fact]
    public void TheMgmtEntrySaysWhatIsMeasuredAndSendsTheResultItselfToTheReadersTeam()
    {
        // The hardest case on the page, because MGMT is genuinely
        // treatment-selecting rather than merely prognostic — so "say nothing
        // about it" is not available and "it means you will do well" is
        // forbidden. The EANO guideline supports all three of these: the test
        // is a quantity, it is used when choosing alkylating chemotherapy, and
        // the cut-off between methylated and unmethylated is unsettled, with
        // validated tests disagreeing on the same sample.
        var entry = Entry("MGMT promoter methylation");

        Assert.Contains("quantity", entry, StringComparison.Ordinal);
        Assert.Contains("one input", entry, StringComparison.Ordinal);
        Assert.Contains("choosing", entry, StringComparison.Ordinal);
        Assert.Contains("not settled", entry, StringComparison.Ordinal);

        // The routing sentence. Without it the entry describes a
        // treatment-selecting test and then leaves the reader to draw the
        // conclusion themselves, which is the failure the spine exists to stop.
        Assert.Matches(new Regex(@"\b(yours|your result)\b[^.]{0,80}\bfor them\b|question for them",
            RegexOptions.IgnoreCase), entry);

        // My first draft said MGMT was "the one result on this page that is
        // used in that way", which the BRAF entry two screens up contradicts:
        // it says there are drugs made to act on BRAF changes and that a team
        // may test for it with treatment in mind. A uniqueness claim on a page
        // of fifteen entries is a claim about the other fourteen, and it went
        // stale between two sections of the same draft.
        foreach (var overclaim in new[] { "the one result", "the only result", "the only test" })
        {
            Assert.DoesNotContain(overclaim, entry, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheKi67EntrySaysTheNumberHasRealMeasurementNoise()
    {
        // The honest, non-prognostic thing to tell a reader who is comparing
        // two reports: there is no standardised method for counting the stained
        // nuclei or for choosing which part of the slide to score, which
        // produces poor interobserver reproducibility. Without it, a reader
        // reads a move from one number to another as the tumor changing.
        var entry = Entry("Ki-67");

        Assert.Contains("no agreed way to count", entry, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(new Regex(@"small difference[^.]{0,90}\bnot\b", RegexOptions.IgnoreCase), entry);
    }

    [Fact]
    public void TheFamilyQuestionIsAnsweredBeforeTheMarkerListAndNotBehindAGate()
    {
        // SYNTHESIS §3.7 calls this the #1 family question and says it is
        // answered clearly nowhere on the consumer web. Position is the
        // content: a reader who scrolls into fifteen gene names without having
        // been told the labels are about the tumor has already started worrying
        // about their children. §12.6 — answer in the first sentence.
        var body = CuratedPage.Body(Page);

        var answer = body.IndexOf("## Are these results passed on to my children?", StringComparison.Ordinal);
        var list = body.IndexOf("## The words, one by one", StringComparison.Ordinal);

        Assert.True(answer > 0, "the page does not answer the family question");
        Assert.True(answer < list, "the family question is answered after the marker list");

        var section = Section("Are these results passed on to my children?");

        Assert.StartsWith("No.", section, StringComparison.Ordinal);
        Assert.Contains("cannot be passed on", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("somatic", section, StringComparison.OrdinalIgnoreCase);

        // Honest in both directions (§12.8): a tumor test does occasionally
        // turn up something inherited, and NCI says so. Leaving that out would
        // make the reassurance the kind that stops being true.
        //
        // Pins the EXCEPTION, not the words "born with". The first version was
        // satisfied by "These are not changes you were born with" — i.e. by
        // deleting the exception and replacing it with a flat denial, which is
        // precisely the drift it exists to prevent. Same defect class as the
        // WI-508 NOS/NEC test.
        Assert.Matches(
            new Regex(@"(Once in a while|sometimes|occasionally)[^.]*born with",
                RegexOptions.IgnoreCase),
            section);
        Assert.Contains("second test", section, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheThreeGlioblastomaFindingsNeverAppearWithoutTheirIdhWildtypeCondition()
    {
        // The correction an independent review found after this page had
        // already merged, and the most harmful thing it could have said.
        //
        // TERT promoter mutation, EGFR amplification and +7/-10 only carry the
        // "this is a glioblastoma even though the cells look lower grade"
        // meaning in an IDH-WILDTYPE tumor. Both cited sources state the
        // condition explicitly:
        //
        //   CAP Recommendation 9 — "For histologic grade 2-3 DG that are
        //   IDH-WT, testing should be performed for whole chromosome 7
        //   gain/whole chromosome 10 loss, EGFR amplification, and TERT
        //   promoter mutation to establish the molecular diagnosis of GBM,
        //   IDH-WT, grade 4."
        //
        //   WHO CNS5 — "...in IDH-wildtype diffuse astrocytomas".
        //
        // Dropped, the page tells oligodendroglioma readers their result points
        // to glioblastoma. That is not a rare reader: EANO's own Table 1 lists
        // TERT promoter under oligodendroglioma, and TERT mutation is present
        // in a majority of them, so the report very commonly says so.
        foreach (var heading in new[] { "TERT promoter", "EGFR", "Chromosome 7 and chromosome 10" })
        {
            var entry = Entry(heading);

            var condition = entry.IndexOf("IDH test shows no change", StringComparison.OrdinalIgnoreCase);
            Assert.True(condition >= 0,
                $"the '{heading}' entry no longer states the IDH-wildtype condition");

            // Each entry has to make the claim itself and stand alone, because
            // the anchors are a published interface (§12.8) and a reader can
            // arrive at any one of them from a deep link without having read
            // the two above it. The first version of this fix wrote "the third
            // of those findings ... that meaning", which only parses in order.
            var claim = entry.IndexOf("glioblastoma", StringComparison.OrdinalIgnoreCase);
            Assert.True(claim >= 0,
                $"the '{heading}' entry no longer says what the finding points to, so it "
                + "cannot be read on its own from a deep link");

            // And the condition has to come BEFORE the claim. A reader who
            // meets the claim first has already drawn the conclusion.
            Assert.True(claim > condition,
                $"the '{heading}' entry names glioblastoma before it states the IDH condition");
        }

        // The glossary half of this fix is pinned in ShippedGlossaryTests,
        // where the glossary-reading machinery already lives.
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. These are descriptions of
        // measurements: putting them behind a disclosure would tell the reader
        // they are frightening, which is the opposite of the page.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatTheReportWalkthroughOwns()
    {
        // WI-508 owns the report's layout, the grade and its notation, NOS/NEC,
        // and why a gene result renames a tumor. This page links and deep-links
        // instead. Two copies is how the MRI and wait pages drifted apart
        // before [TUMOR-BOARD] existed.
        var body = CuratedPage.Body(Page);

        Assert.Contains("/tests/pathology-report#your-report-part-by-part", body, StringComparison.Ordinal);
        Assert.Contains("/tests/pathology-report#what-the-grade-means", body, StringComparison.Ordinal);

        foreach (var owned in new[]
                 {
                     "not otherwise specified", "not elsewhere classified",
                     "Roman", "gross description", "microscopic description",
                 })
        {
            Assert.DoesNotContain(owned, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheMarkersThisPageDefinesAreSuppressedFromTheGlossaryTooltips()
    {
        // A tooltip whose popover repeats the paragraph directly beneath it is
        // noise, and fifteen of them is the carpet WI-505 measured its way out
        // of. `!%term%` (WI-105) suppresses one term for the whole page. The
        // terms still exist so the tooltips fire everywhere ELSE — this page is
        // the one place they are redundant.
        var body = CuratedPage.Body(Page);

        var suppressed = Regex.Matches(body, @"!%(.+?)%").Select(m => m.Groups[1].Value).ToList();

        foreach (var name in new[]
                 {
                     "IDH", "1p/19q", "ATRX", "TERT", "CDKN2A/B homozygous deletion",
                     "EGFR amplification", "H3 K27-altered", "H3 G34", "BRAF", "MGMT",
                     "Ki-67", "TP53", "gene panel", "methylation profiling",
                     "chromosome 7 gain and chromosome 10 loss",
                 })
        {
            Assert.Contains(name, suppressed);
        }

        Assert.Equal(Markers.Length, suppressed.Count);
    }

    [Fact]
    public void ThePageFollowsTheTemplateOpeningAndClosing()
    {
        // §12.8 slots 0, 10 and 11. `.Trim()` is load-bearing on Windows: `.`
        // matches `\r`, so on a CRLF checkout the capture ends with one.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequiresAndNothingUnverifiable()
    {
        // Contract item 8. ContentCheck only warns on a missing `accessed`
        // date, and this phase's whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // What each domain carries. PMC10547522 is the EANO guideline and is
        // the backbone: what every marker measures, and by which method. CAP
        // carries which tests must be run. NCI carries the somatic/germline
        // answer in patient-level words. cancersupportcommunity carries the
        // standardised "biomarker testing is not inherited" wording.
        foreach (var domain in new[]
                 { "ncbi.nlm.nih.gov", "cap.org", "cancer.gov", "cancersupportcommunity.org" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // The dossier sources nearly every row of its marker table to one
        // academic.oup.com URL, and that domain is Cloudflare-gated: it cannot
        // be fetched, so it cannot be checked. The open version of the same
        // paper is PMC10547522 (Neuro-Oncology 25(10):1731-1749, Sahm et al),
        // found by DOI in Europe PMC, and that is what this page cites. A
        // citation nobody can open is a citation nobody can verify, and eight
        // wrong ones across four items is the pattern this pins.
        Assert.DoesNotContain("academic.oup.com", front, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(urls, u => u.Contains("PMC10547522", StringComparison.Ordinal));

        // Indented, so the page's own `title:` is not counted as a source's.
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The marker page as served, its fifteen anchors, and its doors.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class MolecularMarkersPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/molecular-markers";

    private readonly WebApplicationFactory<Program> _factory;

    public MolecularMarkersPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("The words on your gene results, one by one", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromBothOfItsDoors()
    {
        // The WI-412 orphan lesson. /tests still has no index, so this page is
        // reachable only by the links other pages carry to it: /start for a
        // reader who has just arrived, and the report walkthrough for one who
        // is holding the document these words are printed on.
        var client = _factory.CreateClient();

        foreach (var door in new[] { "/start", "/tests/pathology-report" })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryMarkerAnchorIsPublishedOnTheRenderedPage()
    {
        // The acceptance criterion: a reader searching "what is 1p/19q" lands
        // on that entry. These ids are the interface /tumors and feed items
        // will deep-link, so changing one means updating the pages that point
        // at it, and this test is where that gets noticed.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var (_, anchor) in MolecularMarkersPageContentTests.Markers)
        {
            Assert.Contains($"id=\"{anchor}\"", html);
        }
    }

    [Fact]
    public async Task EveryJumpListLinkLandsOnASectionOfThisPage()
    {
        // The jump list is what makes fifteen visible entries scannable, and it
        // is the reason the entries are not collapsed behind disclosures. A
        // link to an id that is not on the page fails silently — the browser
        // just does nothing — so nothing except this test would notice.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var fragments = Regex.Matches(html, "href=\"#([a-z0-9-]+)\"")
            .Select(m => m.Groups[1].Value).Distinct().ToList();

        Assert.NotEmpty(fragments);

        foreach (var fragment in fragments)
        {
            Assert.Contains($"id=\"{fragment}\"", html);
        }
    }

    [Fact]
    public async Task TheDeepLinksIntoTheReportPageStillLandOnSections()
    {
        // This page sends the reader to two exact sections of WI-508 rather
        // than restating the report's layout or the grade. The link check
        // strips fragments, so without this the anchors could rot with every
        // gate staying green.
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        var links = Regex.Matches(html, "href=\"(/[^\"?]*)#([a-z0-9-]+)\"")
            .Where(m => !m.Groups[1].Value.StartsWith("/glossary", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(links);

        foreach (var link in links)
        {
            var target = await client.GetStringAsync(link.Groups[1].Value);
            Assert.Contains($"id=\"{link.Groups[2].Value}\"", target);
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this page owes its reader: back to the document these words
        // are printed on, back to the wait if half of them have not arrived,
        // and out to a person.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/pathology-report", "/tests/waiting-for-results", "/get-help-now");

    [Fact]
    public async Task TheMarkersThisPageDefinesDoNotAlsoFireAsTooltipsOnIt()
    {
        // The rendered half of the suppression decision. "def-<slug>" is the
        // popover a tooltip button targets, so its absence is the proof — and
        // the words this page does NOT define still have to fire, or the
        // suppression has quietly taken the whole glossary down with it.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "idh-gene-change", "1p-19q-co-deletion", "atrx", "tert-promoter-mutation",
                     "cdkn2a-b-deletion", "egfr-amplification", "h3-k27-altered", "h3-g34",
                     "braf", "mgmt-methylation", "ki-67", "tp53", "gene-panel",
                     "methylation-profiling", "chromosome-7-and-10",
                 })
        {
            Assert.DoesNotContain($"def-{slug}", html);
        }

        foreach (var slug in new[] { "glioma", "glioblastoma", "diffuse-glioma" })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }

    [Fact]
    public async Task TheThreeNewTermsAreOnTheGlossaryPage()
    {
        // Contract item 9. These three are suppressed on the page that defines
        // them, so the usual "the tooltip fires here" assertion cannot be the
        // evidence — the reason they join the glossary at all is the rest of
        // the site, starting with WI-513's tumor hubs.
        var html = await _factory.CreateClient().GetStringAsync("/glossary");

        foreach (var term in new[] { "TP53", "H3 G34", "chromosome 7 gain and chromosome 10 loss" })
        {
            Assert.Contains(term, html);
        }
    }
}
