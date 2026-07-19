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
}

public sealed record RunResult(IReadOnlyList<SourceRunResult> Sources)
{
    public int TotalUploaded => Sources.Sum(s => s.Uploaded);
    public int TotalNew => Sources.Sum(s => s.New);
    public IReadOnlyList<SourceRunResult> Failures => [.. Sources.Where(s => s.Failed)];
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
    ILogger<PipelineRunner> logger)
{
    public async Task<RunResult> RunAsync(CancellationToken cancellationToken)
    {
        var results = new List<SourceRunResult>();

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
            results.Add(await RunSourceAsync(fetcher, state, cancellationToken));
        }

        LogSummary(results);
        return new RunResult(results);
    }

    private async Task<SourceRunResult> RunSourceAsync(
        ISourceFetcher fetcher,
        IReadOnlyDictionary<string, SourceState> state,
        CancellationToken cancellationToken)
    {
        var cursor = state.GetValueOrDefault(fetcher.Source)?.Cursor;
        logger.LogInformation("[{Source}] starting (cursor: {Cursor}).",
            fetcher.Source, cursor ?? "none — first run");

        try
        {
            var fetched = await fetcher.FetchAsync(cursor, cancellationToken);
            if (fetched.Items.Count == 0)
            {
                logger.LogInformation("[{Source}] nothing new.", fetcher.Source);
                // The window still counts as processed — dropping the cursor
                // here would refetch the same growing range every run.
                await AdvanceCursorIfAnyAsync(fetcher.Source, fetched.Cursor, cancellationToken);
                return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, 0, null);
            }

            // Ask before spending: only genuinely new items are worth
            // classifying and summarizing (architecture.md §4). Items marked
            // AlwaysUpload bypass this — their content changes in place.
            var candidates = fetched.Items.Where(i => !i.AlwaysUpload).ToList();
            var newKeys = candidates.Count == 0
                ? []
                : (await syncApi.FindNewAsync(
                        [.. candidates.Select(i => new ItemKey(i.Source, i.ExternalId))],
                        cancellationToken))
                    .Select(k => (k.Source, k.ExternalId))
                    .ToHashSet();

            var newItems = fetched.Items
                .Where(i => i.AlwaysUpload || newKeys.Contains((i.Source, i.ExternalId)))
                .ToList();

            if (newItems.Count == 0)
            {
                logger.LogInformation("[{Source}] fetched {Fetched}, all already known.",
                    fetcher.Source, fetched.Items.Count);
                await AdvanceCursorIfAnyAsync(fetcher.Source, fetched.Cursor, cancellationToken);
                return new SourceRunResult(fetcher.Source, fetched.Items.Count, 0, 0, 0, 0, null);
            }

            // M3 inserts classify + summarize here. Until then items upload
            // with raw fields and relevance 'pending' — the review queue shows
            // them, and nothing reaches readers without approval either way.
            var upload = await syncApi.UploadAsync(
                [.. newItems.Select(i => i.ToSyncItem())], fetched.Cursor, cancellationToken);

            logger.LogInformation(
                "[{Source}] fetched {Fetched}, new {New}, uploaded {Uploaded} (frozen {Frozen}, rejected {Rejected}).",
                fetcher.Source, fetched.Items.Count, newItems.Count,
                upload.Inserted + upload.Updated, upload.Frozen, upload.Rejected);

            return new SourceRunResult(
                fetcher.Source,
                fetched.Items.Count,
                newItems.Count,
                upload.Inserted + upload.Updated,
                upload.Frozen,
                upload.Rejected,
                null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Isolation: log it, record it, keep going.
            logger.LogError(exception, "[{Source}] failed — continuing with other sources.",
                fetcher.Source);
            return new SourceRunResult(fetcher.Source, 0, 0, 0, 0, 0, exception.Message);
        }
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
        var uploaded = results.Sum(r => r.Uploaded);
        var failures = results.Where(r => r.Failed).ToList();

        logger.LogInformation("Run complete: {Uploaded} item(s) awaiting review from {Ok}/{Total} source(s).",
            uploaded, results.Count - failures.Count, results.Count);

        foreach (var failure in failures)
        {
            logger.LogWarning("  {Source}: {Error}", failure.Source, failure.Error);
        }
    }
}
