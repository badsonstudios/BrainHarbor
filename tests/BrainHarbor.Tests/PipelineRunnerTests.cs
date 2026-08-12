using BrainHarbor.Pipeline;
using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-203: the run loop. The load-bearing behaviour is per-source isolation —
/// one dead source must never stop the others (architecture.md §3) — and only
/// paying to process items the server says are new.
/// </summary>
public class PipelineRunnerTests
{
    private sealed class StubFetcher(
        string source, IReadOnlyList<FetchedItem>? items = null,
        string? cursor = null, Exception? throws = null, string? stalledReason = null) : ISourceFetcher
    {
        public string Source { get; } = source;
        public int FetchCount { get; private set; }
        public string? SeenCursor { get; private set; }

        public Task<FetchResult> FetchAsync(string? incomingCursor, CancellationToken cancellationToken)
        {
            FetchCount++;
            SeenCursor = incomingCursor;
            return throws is not null
                ? Task.FromException<FetchResult>(throws)
                : Task.FromResult(new FetchResult(items ?? [], cursor, stalledReason));
        }
    }

    private sealed class StubSyncApi : ISyncApiClient
    {
        public Dictionary<string, SourceState> State { get; } = new(StringComparer.Ordinal);
        public HashSet<string> AlreadyKnown { get; } = new(StringComparer.Ordinal);
        public List<(IReadOnlyList<SyncItem> Items, string? Cursor)> Uploads { get; } = [];
        public Exception? StateThrows { get; set; }

        public Task<IReadOnlyDictionary<string, SourceState>> GetStateAsync(CancellationToken ct) =>
            StateThrows is not null
                ? Task.FromException<IReadOnlyDictionary<string, SourceState>>(StateThrows)
                : Task.FromResult<IReadOnlyDictionary<string, SourceState>>(State);

        public Task<IReadOnlyList<ItemKey>> FindNewAsync(IReadOnlyList<ItemKey> keys, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ItemKey>>(
                [.. keys.Where(k => !AlreadyKnown.Contains(k.ExternalId))]);

        public List<(string Source, string Cursor)> CursorAdvances { get; } = [];

        public Task<UploadResponse> UploadAsync(
            IReadOnlyList<SyncItem> items, string? cursor, CancellationToken ct)
        {
            // Mirror the real client: an empty upload is a bug, not a no-op.
            if (items.Count == 0)
            {
                throw new ArgumentException("empty upload", nameof(items));
            }

            Uploads.Add((items, cursor));
            return Task.FromResult(new UploadResponse(items.Count, 0, 0, [], []));
        }

        public List<TrialFacts> TrialRefreshes { get; } = [];

        public Task<TrialsResponse> UploadTrialsAsync(
            IReadOnlyList<TrialFacts> trials, CancellationToken ct)
        {
            TrialRefreshes.AddRange(trials);
            return Task.FromResult(new TrialsResponse(trials.Count, 0, []));
        }

        public Task AdvanceCursorAsync(string source, string cursor, CancellationToken ct)
        {
            CursorAdvances.Add((source, cursor));
            return Task.CompletedTask;
        }

        public List<(string Source, string Error)> ReportedFailures { get; } = [];

        public Task ReportFailureAsync(string source, string error, CancellationToken ct)
        {
            ReportedFailures.Add((source, error));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TaxonomyTypeDto>> GetTaxonomyAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<TaxonomyTypeDto>>([]);
    }

    /// <summary>A classifier the runner tests control. Default: classify
    /// everything as patient_relevant so existing upload assertions hold.</summary>
    private sealed class StubClassifier : BrainHarbor.Pipeline.Classify.IItemClassifier
    {
        public HashSet<string> ExcludeExternalIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> UnclassifiableExternalIds { get; } = new(StringComparer.Ordinal);
        public List<string> Classified { get; } = [];

        public Task<BrainHarbor.Pipeline.Classify.Classification> ClassifyAsync(
            FetchedItem item, CancellationToken ct)
        {
            Classified.Add(item.ExternalId);

            if (ExcludeExternalIds.Contains(item.ExternalId))
            {
                return Task.FromResult(new BrainHarbor.Pipeline.Classify.Classification(
                    BrainHarbor.Pipeline.Classify.ClassifyDecision.Exclude, [], "excluded", "news_other", "classify-v1"));
            }

            if (UnclassifiableExternalIds.Contains(item.ExternalId))
            {
                return Task.FromResult(new BrainHarbor.Pipeline.Classify.Classification(
                    BrainHarbor.Pipeline.Classify.ClassifyDecision.Unclassified, [], null, null, "classify-unavailable"));
            }

            return Task.FromResult(new BrainHarbor.Pipeline.Classify.Classification(
                BrainHarbor.Pipeline.Classify.ClassifyDecision.Classified,
                ["glioblastoma"], "patient_relevant", "human_trial", "classify-v1"));
        }
    }

    private static FetchedItem Item(string source, string externalId) => new()
    {
        Source = source,
        SourceKind = "research",
        ExternalId = externalId,
        Title = $"Study {externalId}",
        Url = $"https://example.org/{externalId}",
    };

    private static FetchedItem Trial(string nctId, bool feedWorthy = true, string status = "Recruiting") => new()
    {
        Source = "ctgov",
        SourceKind = "trial_update",
        ExternalId = nctId,
        Title = $"A trial {nctId}",
        Url = $"https://clinicaltrials.gov/study/{nctId}",
        FeedWorthy = feedWorthy,
        Trial = new TrialFacts
        {
            NctId = nctId,
            Title = $"A trial {nctId}",
            OverallStatus = status,
            LastUpdatePosted = new DateOnly(2026, 7, 20),
        },
    };

    /// <summary>By default produces no summary (item uploads classified but
    /// unsummarized). A test can supply one to check the summary is attached.</summary>
    private sealed class StubSummarizer(BrainHarbor.Pipeline.Summarize.SummarizeOutput? output = null)
        : BrainHarbor.Pipeline.Summarize.ISummarizer
    {
        public bool Flagged { get; init; }

        public Task<BrainHarbor.Pipeline.Summarize.SummaryResult> SummarizeAsync(
            FetchedItem item, CancellationToken ct) =>
            Task.FromResult(new BrainHarbor.Pipeline.Summarize.SummaryResult(
                output, "summarize-v1", output is null ? null : "claude-opus-5", Flagged, []));
    }

    private static PipelineRunner Runner(ISyncApiClient api, params ISourceFetcher[] fetchers) =>
        new(fetchers, api, new StubClassifier(), new StubSummarizer(), NullLogger<PipelineRunner>.Instance);

    private static PipelineRunner Runner(
        ISyncApiClient api, BrainHarbor.Pipeline.Classify.IItemClassifier classifier,
        params ISourceFetcher[] fetchers) =>
        new(fetchers, api, classifier, new StubSummarizer(), NullLogger<PipelineRunner>.Instance);

    private static PipelineRunner Runner(
        ISyncApiClient api, BrainHarbor.Pipeline.Summarize.ISummarizer summarizer,
        params ISourceFetcher[] fetchers) =>
        new(fetchers, api, new StubClassifier(), summarizer, NullLogger<PipelineRunner>.Instance);

    [Fact]
    public async Task UploadsOnlyTheItemsTheServerSaysAreNew()
    {
        var api = new StubSyncApi();
        api.AlreadyKnown.Add("known-1");
        var fetcher = new StubFetcher("pubmed",
            [Item("pubmed", "known-1"), Item("pubmed", "new-1")]);

        var result = await Runner(api, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        var uploaded = Assert.Single(upload.Items);
        Assert.Equal("new-1", uploaded.ExternalId);
        Assert.Equal(1, result.TotalNew);
    }

    // ---------- WI-401: surviving a dead Claude usage limit ----------

    /// <summary>
    /// The failure that cost a production backfill: once the usage limit dies,
    /// every remaining item fails identically. Uploading them as pending would
    /// make the server "know" them, so no later run would ever classify them —
    /// the rows had to be deleted by hand. The source must stop instead.
    /// </summary>
    [Fact]
    public async Task AStreakOfClassificationFailuresStopsTheSourceAndUploadsNothing()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        var items = new List<FetchedItem>();
        for (var i = 1; i <= 8; i++)
        {
            items.Add(Item("pubmed", $"dead-{i}"));
            classifier.UnclassifiableExternalIds.Add($"dead-{i}");
        }

        var fetcher = new StubFetcher("pubmed", items, cursor: "2026-08-12");
        var result = await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Empty(api.CursorAdvances);
        Assert.Equal(
            PipelineRunner.MaxConsecutiveClassifyFailures,
            classifier.Classified.Count); // stopped, did not grind through all 8
        Assert.Single(result.Failures);

        // The admin health page is driven by reported failures, not by the
        // console — a silent stop would look like a quiet day.
        Assert.Single(api.ReportedFailures);
    }

    /// <summary>
    /// The classifier is shared infrastructure: when it dies, small sources
    /// (an RSS feed with two new items) would never reach the streak threshold
    /// on their own, and would each upload a couple of permanently
    /// unclassified rows and advance past them.
    /// </summary>
    [Fact]
    public async Task OnceTheClassifierIsProvenDeadTheRestOfTheRunIsSkipped()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        var dead = new List<FetchedItem>();
        for (var i = 1; i <= 4; i++)
        {
            dead.Add(Item("pubmed", $"dead-{i}"));
            classifier.UnclassifiableExternalIds.Add($"dead-{i}");
        }

        var first = new StubFetcher("pubmed", dead, cursor: "2026-08-12");
        var second = new StubFetcher("nci_rss", [Item("nci_rss", "later-1")], cursor: "2026-08-12");

        var result = await Runner(api, classifier, first, second).RunAsync(CancellationToken.None);

        Assert.Equal(0, second.FetchCount); // not even fetched
        Assert.Empty(api.Uploads);
        Assert.Empty(api.CursorAdvances);
        Assert.Equal(2, result.Failures.Count);
        Assert.Equal(2, api.ReportedFailures.Count);
    }

    /// <summary>
    /// Documents the KNOWN RESIDUAL (WI-413), so it is a decision rather than a
    /// surprise: a window too small to reach the streak threshold still uploads
    /// its failures as permanently unclassified. Treating an all-failed window
    /// as an outage instead would stall a source forever on one item that can
    /// never be classified, which is worse. The latch keeps this to the first
    /// source that meets an outage.
    /// </summary>
    [Fact]
    public async Task ATinyWindowThatWhollyFailsStillUploadsForAPerson()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("dead-1");
        classifier.UnclassifiableExternalIds.Add("dead-2");
        var fetcher = new StubFetcher(
            "nci_rss",
            [Item("nci_rss", "dead-1"), Item("nci_rss", "dead-2")],
            cursor: "2026-08-12");

        await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(2, upload.Items.Count);
        Assert.Equal("2026-08-12", upload.Cursor);
    }

    /// <summary>An off-topic verdict is a working classifier, so it clears the
    /// streak and releases anything held back.</summary>
    [Fact]
    public async Task AnExcludeVerdictProvesTheClassifierIsAliveAndReleasesHeldItems()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("odd-1");
        classifier.UnclassifiableExternalIds.Add("odd-2");
        classifier.ExcludeExternalIds.Add("off-topic");
        var fetcher = new StubFetcher(
            "pubmed",
            [Item("pubmed", "odd-1"), Item("pubmed", "odd-2"), Item("pubmed", "off-topic"),
             Item("pubmed", "odd-3"), Item("pubmed", "ok-1")],
            cursor: "2026-08-12");

        await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        // The two held items are released by the exclude; the streak restarts
        // after it, so odd-3 never reaches the threshold either.
        var upload = Assert.Single(api.Uploads);
        Assert.Equal(
            ["odd-1", "odd-2", "odd-3", "ok-1"],
            upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Equal("2026-08-12", upload.Cursor);
    }

    [Fact]
    public async Task WorkDoneBeforeTheOutageIsKeptButTheCursorIsHeldBack()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        var items = new List<FetchedItem> { Item("pubmed", "ok-1"), Item("pubmed", "ok-2") };
        for (var i = 1; i <= 5; i++)
        {
            items.Add(Item("pubmed", $"dead-{i}"));
            classifier.UnclassifiableExternalIds.Add($"dead-{i}");
        }

        var fetcher = new StubFetcher("pubmed", items, cursor: "2026-08-12");
        await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["ok-1", "ok-2"], upload.Items.Select(i => i.ExternalId).ToArray());

        // The whole point: the unprocessed remainder of this window must be
        // fetched again next run, so the cursor may not move.
        Assert.Null(upload.Cursor);
        Assert.Empty(api.CursorAdvances);
    }

    [Fact]
    public async Task AOneOffClassificationFailureStillGoesUpForAPerson()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("odd-1");
        var fetcher = new StubFetcher(
            "pubmed",
            [Item("pubmed", "ok-1"), Item("pubmed", "odd-1"), Item("pubmed", "ok-2")],
            cursor: "2026-08-12");

        await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["ok-1", "odd-1", "ok-2"], upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Equal("2026-08-12", upload.Cursor); // a healthy run still advances
    }

    /// <summary>A short streak at the very end is still just odd items — they
    /// must not be silently dropped along with the cursor.</summary>
    [Fact]
    public async Task AShortFailureStreakAtTheEndOfAWindowStillUploads()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("odd-1");
        classifier.UnclassifiableExternalIds.Add("odd-2");
        var fetcher = new StubFetcher(
            "pubmed",
            [Item("pubmed", "ok-1"), Item("pubmed", "odd-1"), Item("pubmed", "odd-2")],
            cursor: "2026-08-12");

        await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["ok-1", "odd-1", "odd-2"], upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Equal("2026-08-12", upload.Cursor);
    }

    // ---------- WI-402: facts and feed items are on separate tracks ----------

    [Fact]
    public async Task TrialFactsRefreshEvenForATrialTheClassifierThrowsOut()
    {
        // Blocker: facts used to ride on the item, so an Exclude decision took
        // the status change with it. A trial's status is not the classifier's
        // to veto.
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.ExcludeExternalIds.Add("NCT00000001");

        await Runner(api, classifier, new StubFetcher("ctgov", [Trial("NCT00000001")]))
            .RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);                                  // no feed item...
        Assert.Equal("NCT00000001", Assert.Single(api.TrialRefreshes).NctId);   // ...facts still stored
    }

    [Fact]
    public async Task TrialFactsRefreshForATrialTheServerAlreadyKnows()
    {
        // The commonest case by far: a known trial changes status. It must not
        // be re-summarized, and its facts must still land.
        var api = new StubSyncApi();
        api.AlreadyKnown.Add("NCT00000002");
        var classifier = new StubClassifier();

        await Runner(api, classifier, new StubFetcher("ctgov", [Trial("NCT00000002")]))
            .RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Empty(classifier.Classified);                        // no LLM spend
        Assert.Single(api.TrialRefreshes);
    }

    [Fact]
    public async Task AClosedTrialCostsNoLlmWorkAndNeverReachesTheReviewQueue()
    {
        // Fact-only items exist to keep trials_cache honest. They must not
        // become pending rows for a person to wade through, and must not be
        // worth a single model call.
        var api = new StubSyncApi();
        var classifier = new StubClassifier();

        await Runner(api, classifier,
                new StubFetcher("ctgov", [Trial("NCT00000003", feedWorthy: false, status: "Completed")]))
            .RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Empty(classifier.Classified);
        Assert.Single(api.TrialRefreshes);
    }

    [Fact]
    public async Task AnOpenTrialWeHaveNotSeenBeforeStillBecomesAFeedItem()
    {
        var api = new StubSyncApi();

        await Runner(api, new StubFetcher("ctgov", [Trial("NCT00000004")]))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("NCT00000004", uploaded.ExternalId);
        Assert.Single(api.TrialRefreshes);
    }

    [Fact]
    public async Task AStalledSourceIsReportedAsAFailureButItsFactsAreStillStored()
    {
        // The fetcher read records and could not move its window. Failing loudly
        // is right; throwing the facts away with it is not, because the next run
        // stalls the same way and they would never be stored at all.
        var api = new StubSyncApi();
        var fetcher = new StubFetcher("ctgov", [Trial("NCT00000005")],
            stalledReason: "read 4000 record(s) without advancing");

        var result = await Runner(api, fetcher).RunAsync(CancellationToken.None);

        Assert.Single(api.TrialRefreshes);
        Assert.Single(result.Failures);
        Assert.Contains("without advancing", result.Failures[0].Error);
        Assert.Empty(api.CursorAdvances);                           // window held
        Assert.Contains(api.ReportedFailures, f => f.Source == "ctgov");
    }

    [Fact]
    public async Task OneFailingSourceDoesNotStopTheOthers()
    {
        // The isolation guarantee: a dead feed must not cost us the good ones.
        var api = new StubSyncApi();
        var broken = new StubFetcher("nci_rss", throws: new HttpRequestException("feed is down"));
        var healthy = new StubFetcher("pubmed", [Item("pubmed", "ok-1")]);

        var result = await Runner(api, broken, healthy).RunAsync(CancellationToken.None);

        Assert.Equal(1, result.TotalUploaded);
        var failure = Assert.Single(result.Failures);
        Assert.Equal("nci_rss", failure.Source);
        Assert.Contains("feed is down", failure.Error);
    }

    [Fact]
    public async Task AFailingSourceIsReportedSoStalenessIsVisibleInAdmin()
    {
        // Otherwise a source that broke a week ago still shows its last
        // success and looks like a quiet week.
        var api = new StubSyncApi();
        var broken = new StubFetcher("nci_rss", throws: new HttpRequestException("feed is down"));

        await Runner(api, broken).RunAsync(CancellationToken.None);

        var reported = Assert.Single(api.ReportedFailures);
        Assert.Equal("nci_rss", reported.Source);
        Assert.Contains("feed is down", reported.Error);
    }

    [Fact]
    public async Task EverySourceRunsEvenWhenTheFirstOneFails()
    {
        var api = new StubSyncApi();
        var first = new StubFetcher("nci_rss", throws: new InvalidOperationException("boom"));
        var second = new StubFetcher("pubmed");
        var third = new StubFetcher("medrxiv");

        await Runner(api, first, second, third).RunAsync(CancellationToken.None);

        Assert.Equal(1, second.FetchCount);
        Assert.Equal(1, third.FetchCount);
    }

    [Fact]
    public async Task EachFetcherReceivesItsOwnCursor()
    {
        var api = new StubSyncApi();
        api.State["pubmed"] = new SourceState("pubmed", DateTimeOffset.UtcNow, null, "2026-06-01");
        api.State["medrxiv"] = new SourceState("medrxiv", DateTimeOffset.UtcNow, null, "cursor-b");
        var pubmed = new StubFetcher("pubmed");
        var medrxiv = new StubFetcher("medrxiv");

        await Runner(api, pubmed, medrxiv).RunAsync(CancellationToken.None);

        Assert.Equal("2026-06-01", pubmed.SeenCursor);
        Assert.Equal("cursor-b", medrxiv.SeenCursor);
    }

    [Fact]
    public async Task FirstRunPassesANullCursor()
    {
        var api = new StubSyncApi();
        var fetcher = new StubFetcher("pubmed");

        await Runner(api, fetcher).RunAsync(CancellationToken.None);

        Assert.Null(fetcher.SeenCursor);
    }

    [Fact]
    public async Task CursorAdvancesEvenWhenEveryFetchedItemWasAlreadyKnown()
    {
        // That window IS fully processed — not advancing would make the fetch
        // range grow without bound, refetching the same items forever.
        var api = new StubSyncApi();
        api.AlreadyKnown.Add("known-1");
        var fetcher = new StubFetcher("pubmed", [Item("pubmed", "known-1")], cursor: "2026-06-12");

        await Runner(api, fetcher).RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Equal(("pubmed", "2026-06-12"), Assert.Single(api.CursorAdvances));
    }

    [Fact]
    public async Task CursorAdvancesWhenTheWindowWasEmpty()
    {
        var api = new StubSyncApi();
        var fetcher = new StubFetcher("pubmed", [], cursor: "2026-06-13");

        await Runner(api, fetcher).RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Equal(("pubmed", "2026-06-13"), Assert.Single(api.CursorAdvances));
    }

    [Fact]
    public async Task NothingFetchedAndNoCursorMeansNoCallsAtAll()
    {
        var api = new StubSyncApi();

        await Runner(api, new StubFetcher("pubmed")).RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Empty(api.CursorAdvances);
    }

    [Fact]
    public async Task AlwaysUploadItemsBypassTheNewOnlyFilter()
    {
        // A ClinicalTrials.gov record moving recruiting -> completed is an
        // update to a known item; the new-only filter would drop it forever.
        var api = new StubSyncApi();
        api.AlreadyKnown.Add("NCT123");
        var trial = Item("ctgov", "NCT123") with { AlwaysUpload = true };

        await Runner(api, new StubFetcher("ctgov", [trial])).RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("NCT123", uploaded.ExternalId);
    }

    [Fact]
    public async Task UnreadableStateAbortsTheRunRatherThanRefetchingEverything()
    {
        var api = new StubSyncApi { StateThrows = new HttpRequestException("site down") };
        var fetcher = new StubFetcher("pubmed", [Item("pubmed", "x")]);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => Runner(api, fetcher).RunAsync(CancellationToken.None));

        Assert.Equal(0, fetcher.FetchCount);
    }

    [Fact]
    public async Task ClassifiedItemsUploadWithTheirClassificationAttached()
    {
        var api = new StubSyncApi();

        await Runner(api, new StubFetcher("pubmed", [Item("pubmed", "raw-1")]))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("patient_relevant", uploaded.Relevance);
        Assert.Equal("human_trial", uploaded.ResearchStage);
        Assert.Equal(["glioblastoma"], uploaded.TumorTags);
        Assert.Contains("classify-v1", uploaded.ClassifyModel);
        // No summary yet — that's WI-304.
        Assert.Null(uploaded.PlainSummary);
    }

    [Fact]
    public async Task AClassifiedItemCarriesItsSummaryBlocksAndFlag()
    {
        var api = new StubSyncApi();
        var summary = new BrainHarbor.Pipeline.Summarize.SummarizeOutput
        {
            PlainTitle = "A plain title",
            Hook = "A one-line hook.",
            WhatStudied = "What was studied.",
            WhatFound = "What they found.",
            Means = "What it means.",
            DoesntMean = "What it doesn't mean.",
        };
        var summarizer = new StubSummarizer(summary) { Flagged = true };

        await Runner(api, summarizer, new StubFetcher("pubmed", [Item("pubmed", "sum-1")]))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("A plain title", uploaded.PlainTitle);
        Assert.Equal("A one-line hook.", uploaded.PlainSummary);
        Assert.Equal("What was studied.", uploaded.PlainWhatStudied);
        Assert.Equal("What it doesn't mean.", uploaded.PlainDoesntMean);
        Assert.True(uploaded.SummaryFlagged);
        Assert.Contains("summarize-v1", uploaded.SummaryModel);
    }

    [Fact]
    public async Task ExcludedItemsAreDroppedNotUploaded()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.ExcludeExternalIds.Add("off-topic");

        var result = await Runner(api, classifier,
            new StubFetcher("pubmed", [Item("pubmed", "keep"), Item("pubmed", "off-topic")]))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(api.Uploads).Items;
        Assert.Single(uploaded);
        Assert.Equal("keep", uploaded[0].ExternalId);
        Assert.Equal(1, result.Sources[0].Rejected);   // the excluded one counts as rejected
    }

    [Fact]
    public async Task IfEveryNewItemIsExcludedNothingUploadsButTheCursorAdvances()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.ExcludeExternalIds.Add("x1");

        await Runner(api, classifier,
            new StubFetcher("pubmed", [Item("pubmed", "x1")], cursor: "2026-06-12"))
            .RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Equal(("pubmed", "2026-06-12"), Assert.Single(api.CursorAdvances));
    }

    [Fact]
    public async Task AnUnclassifiableItemIsStillUploadedForAHuman()
    {
        // Classifier failure must never drop an item — it goes up as pending.
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("dunno");

        await Runner(api, classifier, new StubFetcher("pubmed", [Item("pubmed", "dunno")]))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("dunno", uploaded.ExternalId);
        Assert.Null(uploaded.Relevance);   // stays pending for the review queue
    }
}
