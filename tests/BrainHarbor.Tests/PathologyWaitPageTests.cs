using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-507: T5, the pathology wait. The second page under the §12.8 library
/// template (the first is WI-506's MRI page, in TestsLibraryPagesTests.cs,
/// which is where the shared <see cref="CuratedPage"/> helpers live).
///
/// The prose is reviewed, not tested. What is pinned here is the small set of
/// places where this page could leave a reader worse off than no page at all:
/// a turnaround figure read as a promise about their own result, the layered
/// answer presented as anything but normal, or a frightening section that ends
/// with nothing to do.
/// </summary>
public sealed class PathologyWaitPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "waiting-for-results.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    [Fact]
    public void TheLayersSectionLeadsWithTheReassuranceRatherThanBuildingUpToIt()
    {
        // The page exists for this section (SYNTHESIS §3.1). §12.6 says answer
        // in the first sentence under the heading, because most readers never
        // reach the second — and a reader who has just been told two different
        // names is not going to read four paragraphs to find out which is true.
        var sentences = CuratedPage.SentencesOf(Section("The name of your tumor can change, and that is normal"));

        Assert.Contains("the tumor has not changed", sentences[0]);
    }

    [Fact]
    public void TheLayersSectionNamesWhatItIsNotAndThenGivesTheReaderSomethingToDo()
    {
        // §12.6: never end a section on a frightening sentence. Scoped to this
        // section and to its last sentences — "the reassurance is somewhere on
        // the page" is not the property. The question at the end is the whole
        // takeaway: it is the one thing a reader can act on the next time
        // somebody tells them something.
        var section = Section("The name of your tumor can change, and that is normal");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("not a mistake", section);
        Assert.Contains("is this the final answer", string.Join(" ", sentences[^2..]),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheQuickAnswerDuringSurgeryIsNeverPresentedAsTheDiagnosis()
    {
        // The frozen section is right most of the time and is deliberately not
        // the final word (ACS calls it preliminary). A page that lets a reader
        // treat it as the diagnosis has manufactured the exact "they changed
        // their story" moment it was written to prevent.
        var section = Section("The name of your tumor can change, and that is normal");

        Assert.Contains("not to name the tumor", section);
        Assert.Contains("guide the operation", section);
    }

    [Fact]
    public void TheHowLongSectionHandsTheQuestionBackToTheReadersOwnTeamFirst()
    {
        // Every number in this section is local to a hospital, and the one
        // honest thing the page can say is "ask yours". §12.6: answer in the
        // first sentence, because most readers never reach the second.
        var sentences = CuratedPage.SentencesOf(Section("How long does it take?"));

        Assert.Contains("Ask your own team", sentences[0]);
    }

    [Fact]
    public void EveryUkAuditFigureSitsInAParagraphThatSaysItIsFromTheUkAudit()
    {
        // R1 allows these numbers: they are turnaround times, not prognosis.
        // What R1 does not license is printing a UK audit of 21 centers as what
        // will happen to a reader in Ohio. Paragraph-scoped rather than
        // ordered-by-index, because the defect is a figure DRIFTING into a
        // paragraph that does not attribute it — which is what happens when
        // someone tightens the prose later.
        //
        // Limit, stated rather than pretended away: this matches the audit's
        // figures by their wording. Rephrase "three weeks" as "21 days" and the
        // check stops seeing it. It catches the drift, not a rewrite.
        string[] auditFigures = ["three weeks", "14 days", "21 days", "28 days"];

        foreach (var heading in new[] { "How long does it take?", "Why some of it takes weeks" })
        {
            var raw = Regex.Match(
                Page, $@"^## {Regex.Escape(heading)}\s*$(.*?)(?=^## |\z)",
                RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value;

            var carriedFigures = 0;

            // Split on a blank line, CRLF-tolerant. `Split("\n\n")` looks
            // right and is not: this repo has core.autocrlf=true, so a fresh
            // clone hands the test "\r\n\r\n", nothing splits, the whole
            // section arrives as ONE paragraph, and every figure then finds an
            // attribution belonging to a different paragraph. Caught by
            // breaking this test on a CRLF copy of the page — it passed. Same
            // trap as WI-501.
            foreach (var paragraph in Regex.Split(raw, @"\r?\n[ \t]*\r?\n").Select(CuratedPage.Flatten))
            {
                var figure = auditFigures.FirstOrDefault(f =>
                    paragraph.Contains(f, StringComparison.OrdinalIgnoreCase));
                if (figure is null)
                {
                    continue;
                }

                carriedFigures++;
                Assert.True(
                    paragraph.Contains("United Kingdom", StringComparison.Ordinal)
                    || paragraph.Contains("national study", StringComparison.Ordinal)
                    || paragraph.Contains("that study", StringComparison.Ordinal),
                    $"'{figure}' is printed in '{heading}' with nothing saying where it comes from:\n{paragraph}");
            }

            // The canary. Without it this test passes by finding no figures at
            // all, which is exactly what happens if someone deletes the numbers
            // the page exists to give.
            Assert.True(carriedFigures > 0, $"'{heading}' carries none of the audit's figures any more");
        }
    }

    [Fact]
    public void TheDelayIsExplainedByBatchingRatherThanByBlame()
    {
        // SYNTHESIS §3.1: the mundane cause is the publishable part. A reader
        // fills a vacuum with catastrophe, and "eight samples at a time" is a
        // better answer than anything they will invent.
        var section = Section("Why some of it takes weeks");

        Assert.Contains("eight samples at a time", section);
        Assert.Contains("neither of them is about you", section);
    }

    [Fact]
    public void TheSecondOpinionSectionAnswersYesInItsFirstSentence()
    {
        // §12.6, answer first. A section that opens with cost, forms and
        // logistics reads as a discouragement, which is the opposite of what
        // the sources say (JHU: second opinions are routine and are given from
        // the slides alone).
        var section = Section("Can someone else look at my slides?");

        Assert.StartsWith("Yes", CuratedPage.SentencesOf(section)[0]);
        Assert.Contains("not at you", section);
    }

    [Fact]
    public void TheAddendumIsCalledWhatItIsAndNotACorrection()
    {
        // ACS: the pathologist reports what they are sure of and adds the rest
        // when it arrives. Read as a correction, the same document becomes
        // evidence that somebody got it wrong.
        var section = Section("Why is there a second report?");

        Assert.Contains("addendum", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not a correction", section);
    }

    [Fact]
    public void ThePortalSectionAsksForADecisionWhileItIsStillAChoice()
    {
        // US law has required immediate release since April 2021, and in a
        // survey of neuro-oncology providers nearly all reported patients badly
        // upset by reading results alone. Telling a reader this AFTER it has
        // happened is worth nothing, so the ask has to be inside the section
        // and the section has to end on something they can do.
        // The section then has to end on a door out, not on the warning: its
        // likeliest reader is someone who has ALREADY opened it alone at 11pm,
        // for whom "decide in advance" arrives too late to be worth anything.
        var section = Section("Who reads it, and how do I get the result?");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("before anyone has phoned you", section);
        Assert.Contains("decide now", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/get-help-now", string.Join(" ", sentences[^3..]), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5 and §12.4 R2. The temptation on this page is
        // specific: the frozen-section agreement figure (90.3%) is sitting
        // right there in the sources, and it is a number about being wrong.
        var body = CuratedPage.Body(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[] { "survival", "life expectancy", "five-year", "prognosis", "how long you have" })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and there is no outlook here.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void ThePageFollowsTheTemplateOpeningAndClosing()
    {
        // §12.8 slots 0, 10 and 11. The .Trim() inside the helper is
        // load-bearing on Windows: `.` matches `\r`, so on a CRLF checkout a
        // capture ends with one.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. ContentCheck only warns on a missing `accessed`
        // date, and this phase's whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains this page's claims actually rest on. ACS carries the
        // frozen section and the addendum; the PMC papers carry every
        // turnaround figure and the patient-portal finding; JHU carries the
        // second opinion.
        foreach (var domain in new[] { "cancer.org", "ncbi.nlm.nih.gov", "pathology.jhu.edu" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // The 504/558 frozen-section agreement figure belongs to PMC4287923.
        // tests-library.md §4.4 attributes it to PMC4322495, which is a
        // different and smaller study — the WI-505 failure mode exactly, a
        // citation that resolves and does not say it. Pinned so that a future
        // session copying from the dossier fails here instead of shipping it.
        Assert.DoesNotContain("PMC4322495", front, StringComparison.Ordinal);

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The pathology-wait page as served, and the doors that lead to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class PathologyWaitPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/waiting-for-results";

    private readonly WebApplicationFactory<Program> _factory;

    public PathologyWaitPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Waiting for your pathology results", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromBothOfItsDoors()
    {
        // The WI-412 orphan lesson. /tests has no index yet, so the page has
        // exactly two ways in: /start, and the MRI page — which is where a
        // reader is standing when they learn that a scan cannot name a tumor
        // and tissue can. If either link goes, fail here rather than in six
        // months.
        var client = _factory.CreateClient();

        Assert.Contains(Url, await client.GetStringAsync("/start"));
        Assert.Contains(Url, await client.GetStringAsync("/tests/mri"));
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        // Scoped to the article: the layout alone contributes ~13 links, so a
        // whole-page count can never reach zero and the canary below could
        // never fire.
        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "the page did not render an article");

        var broken = new List<string>();
        var checkedLinks = 0;

        foreach (Match match in Regex.Matches(html[start..end], "href=\"(/[^\"#?]*)\""))
        {
            var target = match.Groups[1].Value;
            if (target.StartsWith("/css/") || target.StartsWith("/js/"))
            {
                continue;
            }

            checkedLinks++;
            if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
            {
                broken.Add(target);
            }
        }

        Assert.True(checkedLinks >= 5, $"the page body linked to {checkedLinks} pages — did 'Where to go next' go?");
        Assert.True(broken.Count == 0, string.Join("\n", broken.Distinct()));
    }

    [Fact]
    public async Task TheReportVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9. Asserted on the rendered page rather than on the
        // glossary directory, because a term nothing matches is a term nobody
        // ever sees. "def-<slug>" is the popover the tooltip button targets —
        // asserting the bare word would pass on prose never marked up at all.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "neuropathologist", "addendum", "gene-panel",
                     "frozen-section", "integrated-diagnosis", "tumor-board",
                 })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }
}
