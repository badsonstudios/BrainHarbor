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

        /// <summary>Mirrors the real registration: only ctgov has facts.</summary>
        public bool ProducesTrialFacts => source == "ctgov";
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

    private class StubSyncApi : ISyncApiClient
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

        public virtual Task<UploadResponse> UploadAsync(
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

        /// <summary>The CLI answered; this item's output was unusable. Goes to a person.</summary>
        public HashSet<string> UnclassifiableExternalIds { get; } = new(StringComparer.Ordinal);

        /// <summary>The CLI never answered. Stops the source (WI-413).</summary>
        public HashSet<string> UnavailableExternalIds { get; } = new(StringComparer.Ordinal);

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

            if (UnavailableExternalIds.Contains(item.ExternalId))
            {
                return Task.FromResult(new BrainHarbor.Pipeline.Classify.Classification(
                    BrainHarbor.Pipeline.Classify.ClassifyDecision.Unavailable, [], null, null, "classify-unavailable"));
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

        /// <summary>Which checks tripped, for the WI-417 run tally.</summary>
        public IReadOnlyList<BrainHarbor.Safety.Guardrails.FlagKind> Kinds { get; init; } = [];

        public Task<BrainHarbor.Pipeline.Summarize.SummaryResult> SummarizeAsync(
            FetchedItem item, CancellationToken ct) =>
            Task.FromResult(new BrainHarbor.Pipeline.Summarize.SummaryResult(
                output, "summarize-v1", output is null ? null : "claude-opus-5", Flagged,
                [.. Kinds.Select(k => new BrainHarbor.Safety.Guardrails.Flag(k, k.ToString()))]));
    }

    /// <summary>
    /// The WI-413 health probe. Defaults to "dead", so an Unavailable verdict
    /// in these tests means a genuine outage unless a test says otherwise.
    /// </summary>
    private sealed class StubHealthProbe(bool alive = false)
        : BrainHarbor.Pipeline.Claude.IClaudeHealthProbe
    {
        public int Probes { get; private set; }

        public Task<bool> IsAliveAsync(CancellationToken ct)
        {
            Probes++;
            return Task.FromResult(alive);
        }
    }

    private static PipelineRunner Runner(ISyncApiClient api, params ISourceFetcher[] fetchers) =>
        new(fetchers, api, new StubClassifier(), new StubSummarizer(), new StubHealthProbe(),
            NullLogger<PipelineRunner>.Instance);

    private static PipelineRunner Runner(
        ISyncApiClient api, BrainHarbor.Pipeline.Classify.IItemClassifier classifier,
        params ISourceFetcher[] fetchers) =>
        new(fetchers, api, classifier, new StubSummarizer(), new StubHealthProbe(),
            NullLogger<PipelineRunner>.Instance);

    private static PipelineRunner Runner(
        ISyncApiClient api, BrainHarbor.Pipeline.Summarize.ISummarizer summarizer,
        params ISourceFetcher[] fetchers) =>
        new(fetchers, api, new StubClassifier(), summarizer, new StubHealthProbe(),
            NullLogger<PipelineRunner>.Instance);

    private static PipelineRunner Runner(
        ISyncApiClient api,
        BrainHarbor.Pipeline.Claude.IClaudeHealthProbe health,
        BrainHarbor.Pipeline.Classify.IItemClassifier classifier,
        BrainHarbor.Pipeline.Summarize.ISummarizer summarizer,
        params ISourceFetcher[] fetchers) =>
        new(fetchers, api, classifier, summarizer, health, NullLogger<PipelineRunner>.Instance);

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

    // ---------- WI-401 / WI-413: surviving a dead Claude CLI ----------

    /// <summary>
    /// The failure that cost a production backfill: once the usage limit dies,
    /// every remaining item fails identically. Uploading them as pending would
    /// make the server "know" them, so no later run would ever classify them —
    /// the rows had to be deleted by hand.
    ///
    /// WI-413: ONE unavailable verdict is enough now. Waiting for a streak of
    /// three meant an outage beginning near the end of a small window was never
    /// noticed at all, and the items it touched uploaded unclassifiable.
    /// </summary>
    [Fact]
    public async Task TheFirstUnavailableVerdictStopsTheSourceAndUploadsNothing()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        var items = new List<FetchedItem>();
        for (var i = 1; i <= 8; i++)
        {
            items.Add(Item("pubmed", $"dead-{i}"));
            classifier.UnavailableExternalIds.Add($"dead-{i}");
        }

        var fetcher = new StubFetcher("pubmed", items, cursor: "2026-08-12");
        var result = await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        Assert.Empty(api.Uploads);
        Assert.Empty(api.CursorAdvances);
        Assert.Single(classifier.Classified);   // stopped on the first, not the third
        Assert.Single(result.Failures);

        // The admin health page is driven by reported failures, not by the
        // console — a silent stop would look like a quiet day.
        Assert.Single(api.ReportedFailures);
    }

    /// <summary>
    /// The exact hole WI-413 closes: an outage that begins on the LAST item of
    /// a small window. Under the streak rule nothing reached the threshold, so
    /// that item uploaded as permanently unclassified — unrecoverable without
    /// deleting the row by hand.
    /// </summary>
    [Fact]
    public async Task AnOutageStartingOnTheLastItemOfATinyWindowStillStopsTheSource()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnavailableExternalIds.Add("dead-1");
        var fetcher = new StubFetcher(
            "nci_rss",
            [Item("nci_rss", "ok-1"), Item("nci_rss", "dead-1")],
            cursor: "2026-08-12");

        var result = await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        // The good item still goes up; the dead one stays unknown to the server
        // so the next run can do it properly, and the cursor does not move.
        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["ok-1"], upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Null(upload.Cursor);
        Assert.Empty(api.CursorAdvances);
        Assert.Single(result.Failures);
    }

    /// <summary>
    /// The CLI is shared infrastructure: when it dies, small sources (an RSS
    /// feed with two new items) would each burn their own items discovering the
    /// same outage.
    /// </summary>
    [Fact]
    public async Task OnceTheCliIsProvenDeadTheRestOfTheRunIsSkipped()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        var dead = new List<FetchedItem>();
        for (var i = 1; i <= 4; i++)
        {
            dead.Add(Item("pubmed", $"dead-{i}"));
            classifier.UnavailableExternalIds.Add($"dead-{i}");
        }

        var first = new StubFetcher("pubmed", dead, cursor: "2026-08-12");
        var second = new StubFetcher("nci_rss", [Item("nci_rss", "later-1")], cursor: "2026-08-12");

        var result = await Runner(api, classifier, first, second).RunAsync(CancellationToken.None);

        // An RSS feed has no facts to refresh, so it is not even fetched —
        // finding that out would cost a full paged fetch for no gain.
        Assert.Equal(0, second.FetchCount);
        Assert.Empty(api.Uploads);
        Assert.Empty(api.CursorAdvances);
        Assert.Equal(2, result.Failures.Count);
        Assert.Equal(2, api.ReportedFailures.Count);
    }

    /// <summary>
    /// The other half of the WI-413 split, and why counting was the wrong
    /// signal in BOTH directions: a window of genuinely odd items must reach a
    /// person AND advance the cursor. Treating an all-failed window as an
    /// outage would stall the source forever on items that can never be
    /// classified.
    /// </summary>
    [Fact]
    public async Task AWindowOfOddItemsStillReachesAPersonAndDoesNotStallTheCursor()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnclassifiableExternalIds.Add("odd-1");
        classifier.UnclassifiableExternalIds.Add("odd-2");
        var fetcher = new StubFetcher(
            "nci_rss",
            [Item("nci_rss", "odd-1"), Item("nci_rss", "odd-2")],
            cursor: "2026-08-12");

        var result = await Runner(api, classifier, fetcher).RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(2, upload.Items.Count);
        Assert.Equal("2026-08-12", upload.Cursor);
        Assert.Empty(result.Failures);          // odd items are not a source failure
    }

    /// <summary>
    /// Odd items no longer wait to find out whether they were part of an
    /// outage: they upload in place, in order, alongside everything else.
    /// </summary>
    [Fact]
    public async Task OddItemsUploadInPlaceWithoutBeingHeldBack()
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
            classifier.UnavailableExternalIds.Add($"dead-{i}");
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

    /// <summary>Odd items at the very END of a window must still be uploaded
    /// and the cursor still advanced — nothing is waiting to see what comes
    /// after them any more.</summary>
    [Fact]
    public async Task OddItemsAtTheEndOfAWindowStillUpload()
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

    /// <summary>
    /// The summarizer half of WI-413. An item uploaded classified-but-
    /// unsummarized is never summarized again — a known item costs no model
    /// call on later runs — so it would sit in the review queue with no summary
    /// permanently. It has to stay unknown to the server instead.
    ///
    /// Two items, not one: this also pins that the item which broke mid-way
    /// through is counted in NEITHER New nor Summarized, while the one finished
    /// before it survives intact.
    /// </summary>
    [Fact]
    public async Task AnOutageDuringSummarizationDropsThatItemRatherThanHalfUploadingIt()
    {
        var api = new StubSyncApi();
        var summarizer = new SummarizerThatDiesAfter(1);

        var result = await Runner(api, summarizer,
                new StubFetcher("pubmed",
                    [Item("pubmed", "done"), Item("pubmed", "half-done")], cursor: "2026-08-12"))
            .RunAsync(CancellationToken.None);

        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["done"], upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Null(upload.Cursor);                  // window still outstanding
        Assert.Empty(api.CursorAdvances);
        Assert.Single(result.Failures);

        // The half-processed item counts as neither new nor summarized.
        Assert.Equal(1, result.TotalNew);
        Assert.Equal(1, result.TotalSummarized);
    }

    /// <summary>The latch belongs to the CLI, not to the classifier: an outage
    /// found while SUMMARIZING must skip the remaining sources too.</summary>
    [Fact]
    public async Task AnOutageFoundWhileSummarizingAlsoSkipsTheRestOfTheRun()
    {
        var api = new StubSyncApi();

        var result = await Runner(api, new SummarizerThatDiesAfter(0),
                new StubFetcher("pubmed", [Item("pubmed", "a")]),
                new StubFetcher("nci_rss", [Item("nci_rss", "b")]))
            .RunAsync(CancellationToken.None);

        Assert.Equal(2, result.Failures.Count);
        Assert.Empty(api.Uploads);
    }

    /// <summary>
    /// The stall WI-413 must not introduce. A single slow abstract times out,
    /// which looks exactly like a dead CLI — and stopping on it would hold the
    /// cursor so the SAME item leads the window tomorrow, forever. Asking the
    /// CLI settles it: alive means the item is merely odd.
    /// </summary>
    [Fact]
    public async Task AnUnavailableVerdictFromALiveCliIsTreatedAsAnOddItemNotAnOutage()
    {
        var api = new StubSyncApi();
        var health = new StubHealthProbe(alive: true);
        var classifier = new StubClassifier();
        classifier.UnavailableExternalIds.Add("slow-1");

        var result = await Runner(api, health, classifier, new StubSummarizer(),
                new StubFetcher("pubmed",
                    [Item("pubmed", "slow-1"), Item("pubmed", "ok-1")], cursor: "2026-08-12"))
            .RunAsync(CancellationToken.None);

        // Both go up, and — the point — the cursor MOVES, so the slow item is
        // behind us instead of leading the window again tomorrow.
        var upload = Assert.Single(api.Uploads);
        Assert.Equal(["slow-1", "ok-1"], upload.Items.Select(i => i.ExternalId).ToArray());
        Assert.Equal("2026-08-12", upload.Cursor);
        Assert.Empty(result.Failures);
        Assert.Equal(1, health.Probes);
    }

    [Fact]
    public async Task AnUnavailableSummaryFromALiveCliLeavesTheItemUnsummarizedForAPerson()
    {
        var api = new StubSyncApi();
        var health = new StubHealthProbe(alive: true);

        var result = await Runner(api, health, new StubClassifier(), new SummarizerThatDiesAfter(0),
                new StubFetcher("pubmed", [Item("pubmed", "hard-1")], cursor: "2026-08-12"))
            .RunAsync(CancellationToken.None);

        var uploaded = Assert.Single(Assert.Single(api.Uploads).Items);
        Assert.Equal("hard-1", uploaded.ExternalId);
        Assert.Null(uploaded.PlainSummary);          // no summary, but not lost
        Assert.Equal("2026-08-12", Assert.Single(api.Uploads).Cursor);
        Assert.Empty(result.Failures);
    }

    /// <summary>
    /// Trial facts need no model call, so an outage elsewhere in the run must
    /// not freeze them: a stale cache is what advertises a closed trial as open
    /// on a page a patient reads.
    /// </summary>
    [Fact]
    public async Task ASkippedSourceStillRefreshesItsTrialFacts()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnavailableExternalIds.Add("dead-1");

        var result = await Runner(api, classifier,
                new StubFetcher("pubmed", [Item("pubmed", "dead-1")]),
                new StubFetcher("ctgov", [Trial("NCT00000009", status: "Completed")]))
            .RunAsync(CancellationToken.None);

        Assert.Equal("NCT00000009", Assert.Single(api.TrialRefreshes).NctId);
        Assert.Empty(api.Uploads);                   // no feed item, no LLM work
        Assert.Empty(api.CursorAdvances);
        Assert.Equal(2, result.Failures.Count);      // still reported as skipped
    }

    /// <summary>
    /// The other half of that: a source with no facts is not fetched at all
    /// while the CLI is down. Measured live, fetching every source during an
    /// outage cost four minutes, almost all of it on sources with nothing to
    /// refresh.
    /// </summary>
    [Fact]
    public async Task ASkippedSourceWithNoFactsIsNotFetchedAtAll()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.UnavailableExternalIds.Add("dead-1");
        var rss = new StubFetcher("nci_rss", [Item("nci_rss", "later-1")]);

        await Runner(api, classifier,
                new StubFetcher("pubmed", [Item("pubmed", "dead-1")]), rss)
            .RunAsync(CancellationToken.None);

        Assert.Equal(0, rss.FetchCount);
        Assert.Empty(api.TrialRefreshes);
    }

    /// <summary>A summarizer that works for the first N items, then reports the
    /// CLI as unavailable.</summary>
    private sealed class SummarizerThatDiesAfter(int workingItems)
        : BrainHarbor.Pipeline.Summarize.ISummarizer
    {
        private int _calls;

        public Task<BrainHarbor.Pipeline.Summarize.SummaryResult> SummarizeAsync(
            FetchedItem item, CancellationToken ct)
        {
            if (_calls++ < workingItems)
            {
                return Task.FromResult(new BrainHarbor.Pipeline.Summarize.SummaryResult(
                    new BrainHarbor.Pipeline.Summarize.SummarizeOutput
                    {
                        PlainTitle = "t", Hook = "h", WhatStudied = "s",
                        WhatFound = "f", Means = "m", DoesntMean = "d",
                    },
                    "summarize-v4", "claude-opus-5", Flagged: false, []));
            }

            return Task.FromResult(new BrainHarbor.Pipeline.Summarize.SummaryResult(
                null, "summarize-v4", null, Flagged: false, []) { Unavailable = true });
        }
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

    // ---------- WI-417: a run says WHY, not just how many ----------

    /// <summary>
    /// The gap Dan hit reading the first summarize-v4 run: the pipeline could
    /// say 4.8% of summaries were flagged, but not whether that was reading
    /// level, invented numerals, or hype — the database stores a boolean and no
    /// reason. The run result now carries the breakdown.
    /// </summary>
    [Fact]
    public async Task AFlaggedRunReportsWhichCheckDidTheFlagging()
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
        var summarizer = new StubSummarizer(summary)
        {
            Flagged = true,
            Kinds =
            [
                BrainHarbor.Safety.Guardrails.FlagKind.ReadingLevel,
                BrainHarbor.Safety.Guardrails.FlagKind.InventedNumbers,
            ],
        };

        var result = await Runner(api, summarizer,
                new StubFetcher("pubmed", [Item("pubmed", "a"), Item("pubmed", "b")]))
            .RunAsync(CancellationToken.None);

        Assert.Equal(2, result.TotalSummarized);
        Assert.Equal(2, result.TotalFlagged);
        // One item can trip several checks, so the kinds sum higher than the
        // item count — that is the point of counting them separately.
        Assert.Equal(2, result.FlagKinds[BrainHarbor.Safety.Guardrails.FlagKind.ReadingLevel]);
        Assert.Equal(2, result.FlagKinds[BrainHarbor.Safety.Guardrails.FlagKind.InventedNumbers]);
        Assert.False(result.FlagKinds.ContainsKey(
            BrainHarbor.Safety.Guardrails.FlagKind.BannedHype));
    }

    /// <summary>
    /// The most expensive failure there is: a window is fetched, classified and
    /// summarized — hours of LLM work — and then the UPLOAD throws. That run's
    /// log is the one that gets read, and it must not report "summarized 0".
    /// </summary>
    [Fact]
    public async Task WorkAlreadyDoneIsStillCountedWhenTheUploadFails()
    {
        var api = new ThrowingUploadSyncApi();
        var summarizer = new StubSummarizer(new BrainHarbor.Pipeline.Summarize.SummarizeOutput
        {
            PlainTitle = "t", Hook = "h", WhatStudied = "s",
            WhatFound = "f", Means = "m", DoesntMean = "d",
        })
        {
            Flagged = true,
            Kinds = [BrainHarbor.Safety.Guardrails.FlagKind.ReadingLevel],
        };

        var result = await Runner(api, summarizer, new StubFetcher("pubmed", [Item("pubmed", "a")]))
            .RunAsync(CancellationToken.None);

        Assert.Single(result.Failures);
        Assert.Equal(1, result.TotalSummarized);
        Assert.Equal(1, result.TotalFlagged);
        Assert.Equal(1, result.FlagKinds[BrainHarbor.Safety.Guardrails.FlagKind.ReadingLevel]);
    }

    private sealed class ThrowingUploadSyncApi : StubSyncApi
    {
        public override Task<UploadResponse> UploadAsync(
            IReadOnlyList<SyncItem> items, string? cursor, CancellationToken ct) =>
            Task.FromException<UploadResponse>(new HttpRequestException("the site is down"));
    }

    [Fact]
    public async Task ACleanRunCountsSummariesWithoutFlaggingAnything()
    {
        var api = new StubSyncApi();
        var classifier = new StubClassifier();
        classifier.ExcludeExternalIds.Add("off-topic");
        var summarizer = new StubSummarizer(new BrainHarbor.Pipeline.Summarize.SummarizeOutput
        {
            PlainTitle = "t", Hook = "h", WhatStudied = "s",
            WhatFound = "f", Means = "m", DoesntMean = "d",
        });

        var result = await Runner(api, new StubHealthProbe(), classifier, summarizer,
                new StubFetcher("pubmed", [Item("pubmed", "keep"), Item("pubmed", "off-topic")]))
            .RunAsync(CancellationToken.None);

        Assert.Equal(1, result.TotalSummarized);
        Assert.Equal(0, result.TotalFlagged);
        Assert.Empty(result.FlagKinds);
        // Excluded is reported on its own now, not folded into Rejected only.
        Assert.Equal(1, result.TotalExcluded);
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
