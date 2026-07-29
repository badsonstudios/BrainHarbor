using System.Xml.Linq;
using BrainHarbor.Pipeline.Sources;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-204: efetch XML parsing and the self-healing date window. Uses recorded
/// XML shapes rather than live NCBI calls — the suite must not depend on the
/// network or burn someone's rate limit.
/// </summary>
public class PubMedFetcherTests
{
    private const string SampleXml = """
        <?xml version="1.0" ?>
        <PubmedArticleSet>
          <PubmedArticle>
            <MedlineCitation>
              <PMID Version="1">37378 </PMID>
              <Article>
                <ArticleTitle>Vorasidenib in IDH1- or IDH2-Mutant Low-Grade Glioma</ArticleTitle>
                <Abstract>
                  <AbstractText Label="BACKGROUND">IDH-mutant gliomas are slow growing.</AbstractText>
                  <AbstractText Label="RESULTS">Progression-free survival was longer.</AbstractText>
                </Abstract>
                <Journal>
                  <JournalIssue>
                    <PubDate><Year>2026</Year><Month>Jun</Month><Day>12</Day></PubDate>
                  </JournalIssue>
                </Journal>
              </Article>
            </MedlineCitation>
          </PubmedArticle>
          <PubmedArticle>
            <MedlineCitation>
              <PMID Version="1">37379</PMID>
              <Article>
                <ArticleTitle>Meningioma
                  outcomes    after surgery</ArticleTitle>
                <Journal>
                  <JournalIssue><PubDate><Year>2026</Year></PubDate></JournalIssue>
                </Journal>
              </Article>
            </MedlineCitation>
          </PubmedArticle>
        </PubmedArticleSet>
        """;

    private static IReadOnlyList<FetchedItem> Parse(string xml) =>
        PubMedFetcher.ParseArticles(XDocument.Parse(xml));

    [Fact]
    public void ParsesPmidTitleAbstractAndDate()
    {
        var items = Parse(SampleXml);

        Assert.Equal(2, items.Count);
        var first = items[0];
        Assert.Equal("37378", first.ExternalId);
        Assert.Equal("Vorasidenib in IDH1- or IDH2-Mutant Low-Grade Glioma", first.Title);
        Assert.Equal("https://pubmed.ncbi.nlm.nih.gov/37378/", first.Url);
        Assert.Equal(new DateOnly(2026, 6, 12), first.PublishedAt);
        Assert.Equal("pubmed", first.Source);
        Assert.Equal("research", first.SourceKind);
    }

    [Fact]
    public void KeepsSectionLabelsInTheAbstract()
    {
        // The labels help the M3 summarizer find what was studied vs found.
        var summary = Parse(SampleXml)[0].RawSummary;

        Assert.Contains("BACKGROUND: IDH-mutant gliomas are slow growing.", summary);
        Assert.Contains("RESULTS: Progression-free survival was longer.", summary);
    }

    [Fact]
    public void CollapsesWhitespaceInTitles()
    {
        Assert.Equal("Meningioma outcomes after surgery", Parse(SampleXml)[1].Title);
    }

    [Fact]
    public void YearOnlyDatesBecomeJanuaryFirst()
    {
        Assert.Equal(new DateOnly(2026, 1, 1), Parse(SampleXml)[1].PublishedAt);
    }

    [Fact]
    public void MissingAbstractIsNullNotEmpty()
    {
        Assert.Null(Parse(SampleXml)[1].RawSummary);
    }

    [Fact]
    public void ArticlesWithoutAPmidOrTitleAreSkipped()
    {
        var items = Parse("""
            <PubmedArticleSet>
              <PubmedArticle><MedlineCitation><Article>
                <ArticleTitle>No PMID here</ArticleTitle>
              </Article></MedlineCitation></PubmedArticle>
              <PubmedArticle><MedlineCitation>
                <PMID>999</PMID><Article></Article>
              </MedlineCitation></PubmedArticle>
            </PubmedArticleSet>
            """);

        Assert.Empty(items);
    }

    [Fact]
    public void MedlineDateAndUnparseableDatesDegradeGracefully()
    {
        var items = Parse("""
            <PubmedArticleSet>
              <PubmedArticle><MedlineCitation><PMID>1</PMID><Article>
                <ArticleTitle>Seasonal issue</ArticleTitle>
                <Journal><JournalIssue><PubDate>
                  <MedlineDate>2026 Jun-Jul</MedlineDate>
                </PubDate></JournalIssue></Journal>
              </Article></MedlineCitation></PubmedArticle>
              <PubmedArticle><MedlineCitation><PMID>2</PMID><Article>
                <ArticleTitle>No date at all</ArticleTitle>
              </Article></MedlineCitation></PubmedArticle>
            </PubmedArticleSet>
            """);

        Assert.Equal(new DateOnly(2026, 1, 1), items[0].PublishedAt);
        Assert.Null(items[1].PublishedAt);
    }

    [Fact]
    public void EmptyResultSetParsesToNothing()
    {
        Assert.Empty(Parse("<PubmedArticleSet></PubmedArticleSet>"));
    }

    // ---------- the self-healing window ----------

    [Fact]
    public void FirstRunLooksBackAFixedWindowNotAllOfHistory()
    {
        var days = PubMedFetcher.LookbackDaysFor(null, new DateOnly(2026, 7, 19));

        Assert.Equal(PubMedFetcher.FirstRunLookbackDays, days);
    }

    [Fact]
    public void AMissedRunWidensTheWindowToCatchUp()
    {
        // Three days since the last success → ask for four (gap + 1 overlap).
        var days = PubMedFetcher.LookbackDaysFor("2026-07-16", new DateOnly(2026, 7, 19));

        Assert.Equal(4, days);
    }

    [Fact]
    public void ASameDayRerunStillAsksForOneDay()
    {
        Assert.Equal(1, PubMedFetcher.LookbackDaysFor("2026-07-19", new DateOnly(2026, 7, 19)));
    }

    [Fact]
    public void AVeryOldCursorIsCappedRatherThanPullingEverything()
    {
        var days = PubMedFetcher.LookbackDaysFor("2020-01-01", new DateOnly(2026, 7, 19));

        Assert.Equal(PubMedFetcher.MaxLookbackDays, days);
    }

    [Fact]
    public void AGarbageCursorFallsBackToTheFirstRunWindow()
    {
        Assert.Equal(PubMedFetcher.FirstRunLookbackDays,
            PubMedFetcher.LookbackDaysFor("not-a-date", new DateOnly(2026, 7, 19)));
    }

    [Fact]
    public void TheQueryIsEnglishOnlyAndCoversTheMajorTumorTypes()
    {
        // A checklist, not a tautology: each entry is a tumor type the
        // audience has, and a missing one means those patients get no feed.
        foreach (var term in new[]
                 {
                     "Brain Neoplasms", "Glioma", "Glioblastoma", "Meningioma",
                     "Medulloblastoma", "Ependymoma", "Pituitary Neoplasms",
                     "Craniopharyngioma", "Spinal Cord Neoplasms",
                     "diffuse midline glioma", "primary CNS lymphoma",
                     "vestibular schwannoma", "brain metastases", "leptomeningeal",
                 })
        {
            Assert.Contains(term, PubMedFetcher.Query, StringComparison.OrdinalIgnoreCase);
        }

        // We cannot summarize what we cannot read.
        Assert.Contains("english[lang]", PubMedFetcher.Query);
    }

    [Fact]
    public void PrefersTheElectronicArticleDateForAheadOfPrintRecords()
    {
        // Ahead-of-print records often carry a year-only PubDate; without
        // this the item sorts to January 1 and lands wrong in the feed.
        var items = Parse("""
            <PubmedArticleSet>
              <PubmedArticle><MedlineCitation><PMID>5</PMID><Article>
                <ArticleTitle>Ahead of print study</ArticleTitle>
                <Journal><JournalIssue><PubDate><Year>2026</Year></PubDate></JournalIssue></Journal>
                <ArticleDate DateType="Electronic">
                  <Year>2026</Year><Month>07</Month><Day>15</Day>
                </ArticleDate>
              </Article></MedlineCitation></PubmedArticle>
            </PubmedArticleSet>
            """);

        Assert.Equal(new DateOnly(2026, 7, 15), Assert.Single(items).PublishedAt);
    }

    [Fact]
    public void IgnoresOtherAbstractBlocks()
    {
        // PubMed attaches publisher/foreign-language copies; concatenating
        // them would feed the summarizer a duplicate in another language.
        var items = Parse("""
            <PubmedArticleSet>
              <PubmedArticle><MedlineCitation><PMID>6</PMID><Article>
                <ArticleTitle>Glioma study</ArticleTitle>
                <Abstract><AbstractText>The English abstract.</AbstractText></Abstract>
              </Article>
              <OtherAbstract Language="fre">
                <AbstractText>Le resume francais.</AbstractText>
              </OtherAbstract>
              </MedlineCitation></PubmedArticle>
            </PubmedArticleSet>
            """);

        var summary = Assert.Single(items).RawSummary;
        Assert.Equal("The English abstract.", summary);
    }

    [Fact]
    public void ReferencePmidsAreNotMistakenForTheArticlePmid()
    {
        var items = Parse("""
            <PubmedArticleSet>
              <PubmedArticle>
                <MedlineCitation><PMID>111</PMID><Article>
                  <ArticleTitle>Glioma study</ArticleTitle>
                </Article></MedlineCitation>
                <PubmedData><ReferenceList><Reference>
                  <ArticleIdList><ArticleId IdType="pubmed">999</ArticleId></ArticleIdList>
                </Reference></ReferenceList></PubmedData>
              </PubmedArticle>
            </PubmedArticleSet>
            """);

        Assert.Equal("111", Assert.Single(items).ExternalId);
    }
}
