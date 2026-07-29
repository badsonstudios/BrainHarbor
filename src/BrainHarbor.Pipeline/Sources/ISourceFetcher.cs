using BrainHarbor.Pipeline.Publishing;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// One raw item as a fetcher produces it — before dedupe, classification, and
/// summarization. Fetchers do not decide relevance; they only report what the
/// source said, plus the licensing-safe fields for that source.
/// </summary>
public sealed record FetchedItem
{
    public required string Source { get; init; }
    public required string SourceKind { get; init; }
    public required string ExternalId { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public string? RawSummary { get; init; }
    public DateOnly? PublishedAt { get; init; }

    /// <summary>
    /// Re-upload even when the sync API already knows this external id.
    /// Needed for sources whose items legitimately change — a
    /// ClinicalTrials.gov record moving recruiting → completed is an UPDATE,
    /// not a new item, so the "only new items" filter would drop it forever.
    /// </summary>
    public bool AlwaysUpload { get; init; }

    /// <summary>Converts to the sync contract. Classification fields stay
    /// empty until the M3 Claude steps fill them in.</summary>
    public SyncItem ToSyncItem() => new()
    {
        Source = Source,
        SourceKind = SourceKind,
        ExternalId = ExternalId,
        Title = Title,
        Url = Url,
        RawSummary = RawSummary,
        PublishedAt = PublishedAt,
    };
}

public sealed record FetchResult(IReadOnlyList<FetchedItem> Items, string? Cursor);

/// <summary>
/// A source of items. One implementation per source (PubMed, NCI RSS,
/// ScienceDaily, medRxiv/bioRxiv, ClinicalTrials.gov). The runner isolates
/// failures per fetcher — one dead source must never kill the run.
/// </summary>
public interface ISourceFetcher
{
    /// <summary>The `source` value stored on every item; must be one of the
    /// documented sources the sync API accepts.</summary>
    string Source { get; }

    /// <summary>
    /// Fetches items newer than <paramref name="cursor"/> (whatever that means
    /// for this source — a date window, an ETag, a last-seen id). A null
    /// cursor means "first run": fetch a sensible recent window, not history.
    /// </summary>
    Task<FetchResult> FetchAsync(string? cursor, CancellationToken cancellationToken);
}
