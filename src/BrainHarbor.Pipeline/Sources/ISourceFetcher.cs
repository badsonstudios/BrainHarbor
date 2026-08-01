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

    /// <summary>
    /// Whether this item deserves a place in the reader's feed at all. False
    /// means the item is fetched for its facts only and gets no feed row, no
    /// classification and no summary. Used for ClinicalTrials.gov records that
    /// exist purely to keep trials_cache truthful (a trial that closed last
    /// week), where a feed entry would be noise burying the trials someone
    /// could still join.
    /// </summary>
    public bool FeedWorthy { get; init; } = true;

    /// <summary>
    /// Trial facts, set only by the ClinicalTrials.gov fetcher (WI-402). These
    /// are uploaded separately from the item and unconditionally — status,
    /// phase and sites are facts about the world, and must refresh even for a
    /// trial that will never produce a feed item.
    /// </summary>
    public TrialFacts? Trial { get; init; }

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

/// <summary>
/// What one fetch produced. <paramref name="Cursor"/> null means "do not move
/// the window".
///
/// <paramref name="StalledReason"/> is set when the source could not make
/// forward progress and the run should be recorded as a FAILURE — but the items
/// it did read are still returned, because their facts may be worth storing
/// even though the window is stuck. Throwing instead would throw those away
/// too, and in the stall case the next run fails identically, so they would
/// never be stored at all.
/// </summary>
public sealed record FetchResult(
    IReadOnlyList<FetchedItem> Items,
    string? Cursor,
    string? StalledReason = null);

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
