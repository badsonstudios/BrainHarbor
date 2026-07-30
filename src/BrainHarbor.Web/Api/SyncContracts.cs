namespace BrainHarbor.Web.Api;

/// <summary>
/// Wire contracts for the sync API (architecture.md §4) — the only write
/// surface into the site. Shared shape with the Pipeline's typed client
/// (WI-203); changes here are breaking changes for the local pipeline.
/// </summary>

public sealed record SyncStateResponse(IReadOnlyList<SourceState> Sources);

public sealed record SourceState(
    string Source,
    DateTimeOffset? LastSuccessAt,
    string? LastError,
    string? Cursor);

/// <summary>One (source, external_id) pair the pipeline is asking about.</summary>
public sealed record ItemKey(string Source, string ExternalId);

public sealed record CheckRequest(IReadOnlyList<ItemKey> Keys);

/// <summary>The subset that is NOT already stored — the only ones worth
/// spending Claude tokens on.</summary>
public sealed record CheckResponse(IReadOnlyList<ItemKey> New);

/// <summary>
/// A finished item: fetched, classified, and (for relevant ones) summarized
/// locally. Always lands as status='pending' — publication requires a human.
/// </summary>
public sealed record SyncItem
{
    public required string Source { get; init; }
    public required string SourceKind { get; init; }
    public required string ExternalId { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public string? RawSummary { get; init; }
    public DateOnly? PublishedAt { get; init; }

    // classification
    public IReadOnlyList<string> TumorTags { get; init; } = [];
    public string? ResearchStage { get; init; }
    public string? Relevance { get; init; }
    public string? ClassifyModel { get; init; }

    // plain-language summary (content-pipeline.md §9 blocks). PlainSummary is
    // the one-sentence feed hook; the body is the four block fields.
    public string? PlainTitle { get; init; }
    public string? PlainSummary { get; init; }
    public string? PlainWhatStudied { get; init; }
    public string? PlainWhatFound { get; init; }
    public string? PlainMeans { get; init; }
    public string? PlainDoesntMean { get; init; }
    public string? SummaryModel { get; init; }
    public string? PromptVersion { get; init; }

    /// <summary>
    /// Set by the pipeline's numeral post-check when a number in the summary
    /// isn't traceable to the source (content-pipeline.md §9). Flagged items
    /// are held for review, never auto-published.
    /// </summary>
    public bool SummaryFlagged { get; init; }
}

public sealed record UploadRequest(IReadOnlyList<SyncItem> Items, string? Cursor);

/// <summary>
/// Advances a source's cursor without uploading anything — the "this window
/// held nothing new" case. Without it a source whose window contains only
/// known items can never move forward, so the fetch window grows without
/// bound and every run refetches the same expanding range.
/// </summary>
public sealed record CursorRequest(string Source, string Cursor);

/// <summary>
/// Reports that a source failed, so staleness is visible on the admin health
/// page rather than silent (architecture.md §6).
/// </summary>
public sealed record FailureRequest(string Source, string Error);

/// <summary>The closed tumor taxonomy, for the pipeline's classifier prompt.</summary>
public sealed record TaxonomyResponse(IReadOnlyList<TaxonomyTypeDto> Types);

public sealed record TaxonomyTypeDto(string Slug, string Label, IReadOnlyList<string> Aliases);

public sealed record UploadResponse(
    int Inserted,
    int Updated,
    int Rejected,
    IReadOnlyList<string> RejectedTumorTags,
    IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Items skipped because a human has already reviewed them — their
    /// content is frozen so a pipeline rerun cannot alter what a reader sees
    /// on a published page.
    /// </summary>
    public int Frozen { get; init; }

    /// <summary>
    /// Items published automatically (Auto mode, passed the automated checks)
    /// without a human — a subset of Inserted + Updated.
    /// </summary>
    public int AutoPublished { get; init; }
}
