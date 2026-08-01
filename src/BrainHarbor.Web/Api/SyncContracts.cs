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

    /// <summary>
    /// How close this finding is to being something a patient can actually get,
    /// on a 1-10 scale (10 = approved/standard care today, 1 = lab or idea
    /// stage). Already clamped by research stage on the pipeline side so lab and
    /// animal work can't read as near-clinic. Null until the item is summarized.
    /// </summary>
    public int? ReadinessScore { get; init; }

    /// <summary>One plain sentence saying why the item scored as it did.</summary>
    public string? ReadinessReason { get; init; }

    public string? SummaryModel { get; init; }
    public string? PromptVersion { get; init; }

    /// <summary>
    /// Set by the pipeline's numeral post-check when a number in the summary
    /// isn't traceable to the source (content-pipeline.md §9). Flagged items
    /// are held for review, never auto-published.
    /// </summary>
    public bool SummaryFlagged { get; init; }
}

/// <summary>
/// The FACTS about one ClinicalTrials.gov record (data-model.md
/// §trials_cache), uploaded through their own endpoint rather than riding on a
/// feed item (WI-402).
///
/// They travel separately because they obey opposite rules. A feed item's
/// plain-language text is editorial: gated by the automated safety checks,
/// editable and rejectable by a human, frozen once reviewed. A trial's status
/// is a fact about the world: it must refresh on every run no matter what
/// anyone decided about the summary, because a closed trial shown as
/// "Recruiting" sends a patient to a door that no longer opens.
///
/// Note there is no plain-language field here. That text lives on
/// aggregated_items, where the review machinery can reach it, and /trials
/// joins it.
/// </summary>
public sealed record TrialFacts
{
    /// <summary>The NCT id — the same value as the feed item's external id.</summary>
    public required string NctId { get; init; }

    public required string Title { get; init; }

    /// <summary>The registry's own brief summary. Raw source text, never shown
    /// to a reader as-is.</summary>
    public string? Summary { get; init; }

    public IReadOnlyList<string> Conditions { get; init; } = [];

    /// <summary>Plain phase text ("Phase 1", "Phase 2/Phase 3", "Not applicable").</summary>
    public string? Phase { get; init; }

    /// <summary>Recruiting status, already in plain words ("Recruiting").</summary>
    public string? OverallStatus { get; init; }

    public IReadOnlyList<TrialLocation> Locations { get; init; } = [];

    public DateOnly? LastUpdatePosted { get; init; }
}

public sealed record TrialLocation(
    string? Facility,
    string? City,
    string? State,
    string? Country,
    double? Latitude,
    double? Longitude);

public sealed record TrialsRequest(IReadOnlyList<TrialFacts> Trials);

public sealed record TrialsResponse(int Stored, int Rejected, IReadOnlyList<string> Errors);

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
