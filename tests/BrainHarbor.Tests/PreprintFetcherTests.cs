using BrainHarbor.Pipeline.Sources;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-206: preprints. The load-bearing rule is that a preprint can NEVER be
/// presented as settled science — content-pipeline.md §11 names "preprint
/// presented as fact" as a failure mode to design against.
/// </summary>
public class PreprintFetcherTests
{
    private static PreprintRecord Record(
        string? doi = "10.1101/2026.06.12.123456",
        string? title = "A glioblastoma vaccine in mice",
        string? summary = "We tested a vaccine.",
        string? date = "2026-06-12",
        string? server = "medRxiv") =>
        new() { Doi = doi, Title = title, Abstract = summary, Date = date, Server = server };

    [Fact]
    public void MapsDoiTitleAbstractAndDate()
    {
        var item = PreprintFetcher.ToFetchedItem(Record(), "medrxiv")!;

        Assert.Equal("10.1101/2026.06.12.123456", item.ExternalId);
        Assert.Equal("A glioblastoma vaccine in mice", item.Title);
        Assert.Equal("https://doi.org/10.1101/2026.06.12.123456", item.Url);
        Assert.Equal("We tested a vaccine.", item.RawSummary);
        Assert.Equal(new DateOnly(2026, 6, 12), item.PublishedAt);
        Assert.Equal("medrxiv", item.Source);
    }

    [Fact]
    public void SourceKindIsAlwaysPreprintNoMatterWhatTheRecordSays()
    {
        // This is what drives the permanent badge, and what the sync API and
        // the database CHECK constraint both key off.
        foreach (var record in new[]
                 {
                     Record(),
                     Record(server: "bioRxiv"),
                     Record(title: "A randomized controlled trial in people"),
                 })
        {
            Assert.Equal("preprint", PreprintFetcher.ToFetchedItem(record, "medrxiv")!.SourceKind);
        }
    }

    [Fact]
    public void RecordsWithoutADoiOrTitleAreSkipped()
    {
        Assert.Null(PreprintFetcher.ToFetchedItem(Record(doi: null), "medrxiv"));
        Assert.Null(PreprintFetcher.ToFetchedItem(Record(doi: "  "), "medrxiv"));
        Assert.Null(PreprintFetcher.ToFetchedItem(Record(title: null), "medrxiv"));
    }

    [Fact]
    public void MissingAbstractIsNullNotEmpty()
    {
        Assert.Null(PreprintFetcher.ToFetchedItem(Record(summary: null), "medrxiv")!.RawSummary);
    }

    [Fact]
    public void UnparseableDatesDegradeToNull()
    {
        Assert.Null(PreprintFetcher.ToFetchedItem(Record(date: "not-a-date"), "medrxiv")!.PublishedAt);
    }

    [Fact]
    public void AMissingServerFieldFallsBackToTheFetchersOwnSource()
    {
        // Hardcoding "medrxiv" here would file bioRxiv items under the wrong
        // source, desynchronizing the dedupe key from the cursor key.
        var item = PreprintFetcher.ToFetchedItem(Record(server: null), "biorxiv")!;

        Assert.Equal("biorxiv", item.Source);
    }

    [Fact]
    public void PageSizeMatchesWhatTheApiActuallyReturns()
    {
        // The live API returns 30 per page regardless of the offset. Assuming
        // 100 made a full page look "short", so the fetcher stopped after one
        // page while the cursor advanced past everything it never read.
        Assert.Equal(30, PreprintFetcher.PageSize);
    }

    [Fact]
    public void CollapsesWhitespaceInTitlesAndAbstracts()
    {
        var item = PreprintFetcher.ToFetchedItem(
            Record(title: "A  glioma\n   study", summary: "Line one.\n\nLine two."), "medrxiv")!;

        Assert.Equal("A glioma study", item.Title);
        Assert.Equal("Line one. Line two.", item.RawSummary);
    }

    // ---------- the window ----------

    [Fact]
    public void FirstRunLooksBackAFixedWindow()
    {
        var start = PreprintFetcher.StartDateFor(null, new DateOnly(2026, 7, 19));

        Assert.Equal(new DateOnly(2026, 7, 5), start);
    }

    [Fact]
    public void ACursorGivesAOneDayOverlapForSafety()
    {
        var start = PreprintFetcher.StartDateFor("2026-07-18", new DateOnly(2026, 7, 19));

        Assert.Equal(new DateOnly(2026, 7, 17), start);
    }

    [Fact]
    public void AVeryOldCursorIsCappedRatherThanPullingEverything()
    {
        var start = PreprintFetcher.StartDateFor("2020-01-01", new DateOnly(2026, 7, 19));

        Assert.Equal(new DateOnly(2026, 4, 20), start);
    }

    [Fact]
    public void AGarbageCursorFallsBackToTheFirstRunWindow()
    {
        Assert.Equal(new DateOnly(2026, 7, 5),
            PreprintFetcher.StartDateFor("nonsense", new DateOnly(2026, 7, 19)));
    }
}
