using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline;

public sealed record SourceRunResult(
    string Source,
    int Fetched,
    int New,
    int Uploaded,
    int Frozen,
    int Rejected,
    string? Error)
{
    public bool Failed => Error is not null;

    /// <summary>Dropped as off-topic by the classifier. Also counted inside
    /// <see cref="Rejected"/>, which lumps every not-uploaded reason together;
    /// this one is separate so the run summary can name it.</summary>
    public int Excluded { get; init; }

    /// <summary>Items that came back with a usable summary.</summary>
    public int Summarized { get; init; }

    /// <summary>Of those, how many the automated checks held back (WI-417).</summary>
    public int Flagged { get; init; }

    /// <summary>Why they were held back. One item can trip several checks, so
    /// these sum to at least <see cref="Flagged"/>.</summary>
    public IReadOnlyDictionary<Summarize.Guardrails.FlagKind, int> FlagKinds { get; init; }
        = new Dictionary<Summarize.Guardrails.FlagKind, int>();
}

public sealed record RunResult(IReadOnlyList<SourceRunResult> Sources)
{
    public int TotalUploaded => Sources.Sum(s => s.Uploaded);
    public int TotalNew => Sources.Sum(s => s.New);
    public int TotalExcluded => Sources.Sum(s => s.Excluded);
    public int TotalSummarized => Sources.Sum(s => s.Summarized);
    public int TotalFlagged => Sources.Sum(s => s.Flagged);
    public IReadOnlyList<SourceRunResult> Failures => [.. Sources.Where(s => s.Failed)];

    /// <summary>Flag reasons across the whole run — the number Dan could not get
    /// from the database, which stores only a summary_flagged boolean.</summary>
    public IReadOnlyDictionary<Summarize.Guardrails.FlagKind, int> FlagKinds =>
        Sources
            .SelectMany(s => s.FlagKinds)
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Sum(pair => pair.Value));
}

/// <summary>
/// The pipeline run loop (architecture.md §3): for each source, read its
/// cursor → fetch → ask the API which items are new → (M3: classify and
/// summarize) → upload as pending.
///
/// **Per-source isolation is the point.** One source failing — network down,
/// feed reshaped, API rejecting a batch — must never stop the others. A
/// failure is logged, recorded, and the run continues.
/// </summary>
public sealed class PipelineRunner(
    IEnumerable<ISourceFetcher> fetchers,
    ISyncApiClient syncApi,
    BrainHarbor.Pipeline.Classify.IItemClassifier classifier,
    BrainHarbor.Pipeline.Summarize.ISummarizer summarizer,
    BrainHarbor.Pipeline.Claude.IClaudeHealthProbe claudeHealth,
    ILogger<PipelineRunner> logger)
{
    /// <summary>
    /// Set when a source finds the Claude CLI unavailable. The CLI is SHARED
    /// infrastructure, not a source: when it is down it is down for everything,
    /// so the first source to meet it stops the rest of the run rather than
    /// letting each remaining source discover the same outage item by item.
    ///
    /// Per-source isolation is unchanged: a dead SOURCE still only fails itself.
    /// </summary>
    private bool claudeDown;

    public async Task<RunResult> RunAsync(CancellationToken cancellationToken)
    {
        var results = new List<SourceRunResult>();
        claudeDown = false;

        IReadOnlyDictionary<string, SourceState> state;
        try
        {
            state = await syncApi.GetStateAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Without state we can't run incrementally, and a full refetch of
            // every source would be worse than stopping.
            logger.LogError(exception, "Could not read sync state — aborting the run.");
            throw;
        }

        var registered = fetchers.ToList();
        if (registered.Count == 0)
        {
            // Otherwise a registration mistake reports a healthy "0/0 sources".
            logger.LogWarning("No source fetchers are registered — this run will do nothing.");
        }

        foreach (var fetcher in registered)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (claudeDown)
            {
                // No classification or summarization: every item would fail the
                // same way, and uploading them is what makes them unrecoverable.
                //
                // Trial FACTS are a different matter and still refresh — they
                // need no model call at all, and a stale cache is what
                // advertises a closed trial as open on a page a patient reads.
                // Before WI-413 this needed three consecutive failures to
                // trigger; on one it would freeze the registry for a whole day.
                const string message =
                    "skipped — the Claude CLI stopped answering earlier in this run";
                logger.LogWarning("[{Source}] {Message}.", fetcher.Source, message);
                results.Add(await RefreshFactsOnlyAsync(fetcher, message, cancellationToken));
                continue;
            }

            results.Add(await RunSourceAsync(fetcher, state, cancellationToken));
        }

        LogSummary(results);
        return new RunResult(results);
    }

    /// <summary>
    /// The Claude-free half of a source run, for when the CLI is down: fetch,
    /// store whatever factual records came back, report the source as failed,
    /// and touch nothing else. The cursor deliberately does not move — no feed
    /// item was processed, so the window is still outstanding.
    ///
    /// A fetch failure here is swallowed into the same reported failure: the
    /// source is already being reported as failed, and a second error about a
    /// source we were skipping anyway would only crowd the log.
    /// </summary>
    private async Task<SourceRunResult> RefreshFactsOnlyAsync(
        ISourceFetcher fetcher, string message, CancellationToken cancellationToken)
    {
        var rejected = 0;

        if (!fetcher.ProducesTrialFacts)
        {
            // Nothing here needs refreshing, and finding that out would cost a
            // full paged fetch for no gain.
            await syncApi.ReportFailureAsync(fetcher.Source, message, cancellationToken);
            return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, 0, message);
        }

        try
        {
            var fetched = await fetcher.FetchAsync(null, cancellationToken);
            rejected = await RefreshFactsAsync(fetcher.Source, fetched.Items, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "[{Source}] could not refresh facts while skipping the source.", fetcher.Source);
        }

        await syncApi.ReportFailureAsync(fetcher.Source, message, cancellationToken);
        return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, rejected, message);
    }

    private async Task<SourceRunResult> RunSourceAsync(
        ISourceFetcher fetcher,
        IReadOnlyDictionary<string, SourceState> state,
        CancellationToken cancellationToken)
    {
        var cursor = state.GetValueOrDefault(fetcher.Source)?.Cursor;
        logger.LogInformation("[{Source}] starting (cursor: {Cursor}).",
            fetcher.Source, cursor ?? "none — first run");

        // WI-417: counted per source so the end-of-run summary can say WHY items
        // were held, not just how many. Declared OUT here, not inside the try,
        // because the likeliest exception is the UPLOAD at the end — after every
        // item in the window has been classified and summarized. Reporting
        // "summarized 0" for that run would be a lie about the most expensive
        // failure there is, and that is exactly the run whose log gets read.
        var excluded = 0;
        var summarized = 0;
        var flagged = 0;
        var flagKinds = new Dictionary<Summarize.Guardrails.FlagKind, int>();

        try
        {
            var fetched = await fetcher.FetchAsync(cursor, cancellationToken);

            // Facts before anything else (WI-402). Trial status, phase and
            // sites refresh on every run regardless of what happens to the feed
            // item — whether it is new, excluded as off-topic, or frozen
            // because a person reviewed it. Doing this first also means the
            // worst case of a later failure is facts without a feed item, which
            // harms nobody; the reverse would advertise a closed trial as open.
            var factsRejected = await RefreshFactsAsync(fetcher.Source, fetched.Items, cancellationToken);

            // The source read records but could not move its window forward.
            // The facts above are stored; the run is still a failure, so it
            // reaches the admin health page instead of reading as a quiet night.
            if (fetched.StalledReason is { } stalled)
            {
                logger.LogError("[{Source}] stalled: {Reason}", fetcher.Source, stalled);
                await syncApi.ReportFailureAsync(fetcher.Source, stalled, cancellationToken);
                return new SourceRunResult(
                    fetcher.Source, fetched.Items.Count, 0, 0, 0, factsRejected, stalled);
            }

            if (fetched.Items.Count == 0)
            {
                logger.LogInformation("[{Source}] nothing new.", fetcher.Source);
                // The window still counts as processed — dropping the cursor
                // here would refetch the same growing range every run.
                await AdvanceCursorIfAnyAsync(fetcher.Source, fetched.Cursor, cancellationToken);
                return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, factsRejected, null);
            }

            // Ask before spending: only genuinely new items are worth
            // classifying and summarizing (architecture.md §4). Items marked
            // AlwaysUpload bypass this — their content changes in place.
            // Fact-only items can never become feed rows, so asking the server
            // whether it already knows them is a wasted round trip (on ctgov
            // that is most of the batch).
            var candidates = fetched.Items.Where(i => i.FeedWorthy && !i.AlwaysUpload).ToList();
            var newKeys = candidates.Count == 0
                ? []
                : (await syncApi.FindNewAsync(
                        [.. candidates.Select(i => new ItemKey(i.Source, i.ExternalId))],
                        cancellationToken))
                    .Select(k => (k.Source, k.ExternalId))
                    .ToHashSet();

            var newItems = fetched.Items
                .Where(i => i.AlwaysUpload || newKeys.Contains((i.Source, i.ExternalId)))
                // Fact-only items (a trial nobody can join) never become feed
                // rows: no classification, no summary, and nothing in the
                // review queue for a person to wade through.
                .Where(i => i.FeedWorthy)
                .ToList();

            if (newItems.Count == 0)
            {
                logger.LogInformation(
                    "[{Source}] fetched {Fetched}; nothing new to write up.",
                    fetcher.Source, fetched.Items.Count);
                await AdvanceCursorIfAnyAsync(fetcher.Source, fetched.Cursor, cancellationToken);
                return new SourceRunResult(
                    fetcher.Source, fetched.Items.Count, 0, 0, 0, factsRejected, null);
            }

            // Classify each new item (WI-303). Excluded items are dropped
            // (never uploaded); items we can't classify are still uploaded, as
            // 'pending', so a human sorts them — never silently lost.
            // Summarization (WI-304) fills the plain-language fields next.
            var toUpload = new List<SyncItem>();

            // A run that outlives the Claude usage limit fails EVERY remaining
            // item the same way. Uploading those as pending would bury the
            // window in the review queue, and — because the server then knows
            // them — no later run would ever classify them: the work would have
            // to be undone by hand in the database (532 rows, twice, during the
            // WI-401 backfill). So the FIRST unavailable verdict stops the
            // source (WI-413). What was processed still uploads, the cursor
            // stays put, and the next run refetches the window and resumes
            // where this one gave out (known items cost nothing).
            var stoppedEarly = false;

            foreach (var item in newItems)
            {
                var classification = await classifier.ClassifyAsync(item, cancellationToken);

                if (classification.Decision == Classify.ClassifyDecision.Unavailable)
                {
                    // Before writing off the window, ask the CLI whether it is
                    // actually down. A timeout is the case that needs this: one
                    // slow abstract looks exactly like a dead CLI, and stopping
                    // on it would hold the cursor so the SAME item leads the
                    // window tomorrow and stalls the source forever.
                    if (await claudeHealth.IsAliveAsync(cancellationToken))
                    {
                        logger.LogWarning(
                            "[{Source}] {Id} could not be classified, but the CLI answers a trivial " +
                            "prompt — treating it as an odd item and sending it to the queue.",
                            fetcher.Source, item.ExternalId);
                        toUpload.Add(item.ToSyncItem());
                        continue;
                    }

                    // Genuinely down. Not this item's fault and not recoverable
                    // by trying the next one, so leave it unknown to the server
                    // and let the next run fetch and classify it properly.
                    stoppedEarly = true;
                    break;
                }

                if (classification.Decision == Classify.ClassifyDecision.Exclude)
                {
                    // Log the identity, not just a count: a false-exclude
                    // permanently drops a relevant item (the window won't be
                    // refetched), so it must at least be discoverable.
                    logger.LogInformation("[{Source}] excluded {Id} as off-topic: {Title}",
                        fetcher.Source, item.ExternalId, item.Title);
                    excluded++;
                    continue;
                }

                var sync = item.ToSyncItem();
                if (classification.Decision == Classify.ClassifyDecision.Classified)
                {
                    sync = sync with
                    {
                        TumorTags = classification.TumorTags,
                        Relevance = classification.Relevance,
                        ResearchStage = classification.ResearchStage,
                        ClassifyModel =
                            $"{classification.Model ?? "claude-code-cli"} ({classification.PromptVersion})",
                    };

                    // Summarize (WI-304). A summary that fails or trips the
                    // automated checks doesn't block upload — the item goes up
                    // (flagged if checks failed) and the review queue handles it.
                    var summary = await summarizer.SummarizeAsync(item, cancellationToken);

                    // Same outage, one step later in the same item (WI-413) —
                    // and the same question, so the same answer: ask the CLI
                    // before writing off the window. If it is alive, this item
                    // is merely hard to summarize and goes up unsummarized for
                    // a person, exactly as a failed summary always has.
                    if (summary.Unavailable && !await claudeHealth.IsAliveAsync(cancellationToken))
                    {
                        // Uploading it now would be worse than dropping it: a
                        // known item is never re-summarized, so it would sit in
                        // the queue without a summary permanently. Leave it
                        // unknown and let the next run do the whole item.
                        stoppedEarly = true;
                        break;
                    }

                    if (summary.Output is { } s)
                    {
                        summarized++;
                        if (summary.Flagged)
                        {
                            flagged++;
                            foreach (var reason in summary.FlagReasons)
                            {
                                flagKinds[reason.Kind] = flagKinds.GetValueOrDefault(reason.Kind) + 1;
                            }
                        }

                        // Cap the readiness score by the stage we just classified:
                        // a mouse study can never read as "near the clinic", no
                        // matter what the model proposed (Readiness.Clamp).
                        var readiness = Summarize.Readiness.Clamp(
                            s.ReadinessScore, classification.ResearchStage, out var capped);
                        if (capped)
                        {
                            logger.LogInformation(
                                "[{Source}/{Id}] readiness capped {Proposed}->{Final} for stage {Stage}.",
                                fetcher.Source, item.ExternalId, s.ReadinessScore, readiness,
                                classification.ResearchStage);
                        }

                        sync = sync with
                        {
                            PlainTitle = s.PlainTitle,
                            PlainSummary = s.Hook,
                            PlainWhatStudied = s.WhatStudied,
                            PlainWhatFound = s.WhatFound,
                            PlainMeans = s.Means,
                            PlainDoesntMean = s.DoesntMean,
                            ReadinessScore = readiness,
                            ReadinessReason = s.ReadinessReason,
                            SummaryModel = $"{summary.Model ?? "claude-code-cli"} ({summary.PromptVersion})",
                            PromptVersion = summary.PromptVersion,
                            SummaryFlagged = summary.Flagged,
                        };
                    }
                }

                toUpload.Add(sync);
            }

            if (stoppedEarly)
            {
                // The CLI is shared, so the rest of the run is over too.
                claudeDown = true;

                var message =
                    "the Claude CLI stopped answering (a dead usage limit, a CLI that will not " +
                    "start, and a failed taxonomy call all look like this); cursor held so the " +
                    "next run resumes this window";
                logger.LogWarning("[{Source}] {Message}. {Done} item(s) processed before that.",
                    fetcher.Source, message, toUpload.Count + excluded);

                // Cursor deliberately null: the unprocessed remainder of this
                // window must be fetched again.
                var partial = toUpload.Count > 0
                    ? await syncApi.UploadAsync(toUpload, null, cancellationToken)
                    : null;

                await syncApi.ReportFailureAsync(fetcher.Source, message, cancellationToken);

                return new SourceRunResult(
                    fetcher.Source,
                    fetched.Items.Count,
                    // Only what was actually processed — reporting the whole
                    // window as "new" would make the run summary read as though
                    // the rest had been handled and dropped.
                    toUpload.Count + excluded,
                    partial is null ? 0 : partial.Inserted + partial.Updated,
                    partial?.Frozen ?? 0,
                    (partial?.Rejected ?? 0) + excluded + factsRejected,
                    message)
                {
                    Excluded = excluded,
                    Summarized = summarized,
                    Flagged = flagged,
                    FlagKinds = flagKinds,
                };
            }

            if (toUpload.Count == 0)
            {
                logger.LogInformation("[{Source}] fetched {Fetched}, new {New}, all excluded as off-topic.",
                    fetcher.Source, fetched.Items.Count, newItems.Count);
                await AdvanceCursorIfAnyAsync(fetcher.Source, fetched.Cursor, cancellationToken);
                return new SourceRunResult(
                    fetcher.Source, fetched.Items.Count, newItems.Count, 0, 0,
                    excluded + factsRejected, null)
                {
                    Excluded = excluded,
                    Summarized = summarized,
                    Flagged = flagged,
                    FlagKinds = flagKinds,
                };
            }

            var upload = await syncApi.UploadAsync(toUpload, fetched.Cursor, cancellationToken);

            logger.LogInformation(
                "[{Source}] fetched {Fetched}, new {New}, excluded {Excluded}, uploaded {Uploaded} " +
                "(auto-published {Auto}, frozen {Frozen}, rejected {Rejected}).",
                fetcher.Source, fetched.Items.Count, newItems.Count, excluded,
                upload.Inserted + upload.Updated, upload.AutoPublished, upload.Frozen, upload.Rejected);

            return new SourceRunResult(
                fetcher.Source,
                fetched.Items.Count,
                newItems.Count,
                upload.Inserted + upload.Updated,
                upload.Frozen,
                upload.Rejected + excluded + factsRejected,
                null)
            {
                Excluded = excluded,
                Summarized = summarized,
                Flagged = flagged,
                FlagKinds = flagKinds,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Isolation: log it, record it, keep going. Reporting it to the
            // site is what makes the failure visible on the admin health page
            // instead of looking like a quiet week.
            logger.LogError(exception, "[{Source}] failed — continuing with other sources.",
                fetcher.Source);
            await syncApi.ReportFailureAsync(fetcher.Source, exception.Message, cancellationToken);
            return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, 0, exception.Message)
            {
                // Whatever was done before it fell over still happened, and the
                // work (and the LLM spend) was real. A failed upload must not
                // erase the record of what it was carrying.
                Excluded = excluded,
                Summarized = summarized,
                Flagged = flagged,
                FlagKinds = flagKinds,
            };
        }
    }

    /// <summary>
    /// Uploads the factual half of whatever this source produced. Returns how
    /// many the server rejected, so a systematic validation failure (a registry
    /// change pushing every record past a bound) is counted rather than logged
    /// and forgotten. If EVERY record is rejected the source is failed
    /// outright: silently never updating trials_cache while the health page
    /// shows green is exactly the failure this reporting exists to prevent.
    /// </summary>
    private async Task<int> RefreshFactsAsync(
        string source, IReadOnlyList<FetchedItem> items, CancellationToken cancellationToken)
    {
        var facts = items.Select(i => i.Trial).Where(t => t is not null).ToList();
        if (facts.Count == 0)
        {
            return 0;
        }

        var result = await syncApi.UploadTrialsAsync(facts!, cancellationToken);

        if (result.Stored == 0)
        {
            throw new InvalidOperationException(
                $"every one of {facts.Count} trial record(s) was rejected: " +
                string.Join(" | ", result.Errors.Take(3)));
        }

        if (result.Rejected > 0)
        {
            logger.LogWarning("[{Source}] {Rejected} of {Total} trial record(s) were rejected.",
                source, result.Rejected, facts.Count);
        }

        return result.Rejected;
    }

    private async Task AdvanceCursorIfAnyAsync(
        string source, string? cursor, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(cursor))
        {
            await syncApi.AdvanceCursorAsync(source, cursor, cancellationToken);
        }
    }

    private void LogSummary(IReadOnlyList<SourceRunResult> results)
    {
        var run = new RunResult(results);
        var failures = run.Failures;

        logger.LogInformation("Run complete: {Uploaded} item(s) uploaded from {Ok}/{Total} source(s).",
            run.TotalUploaded, results.Count - failures.Count, results.Count);

        // WI-417: the counts that used to exist only as scrolled-past console
        // lines. The database records a summary_flagged boolean and no reason,
        // so without this the flag RATE is knowable and the CAUSE is not.
        logger.LogInformation(
            "  summarized {Summarized}, flagged {Flagged}, excluded as off-topic {Excluded}.",
            run.TotalSummarized, run.TotalFlagged, run.TotalExcluded);

        if (run.FlagKinds.Count > 0)
        {
            logger.LogInformation("  flagged because: {Reasons}", string.Join(", ",
                run.FlagKinds
                    .OrderByDescending(pair => pair.Value)
                    .Select(pair => $"{Summarize.Guardrails.Describe(pair.Key)} {pair.Value}")));
        }

        foreach (var failure in failures)
        {
            logger.LogWarning("  {Source}: {Error}", failure.Source, failure.Error);
        }
    }
}
