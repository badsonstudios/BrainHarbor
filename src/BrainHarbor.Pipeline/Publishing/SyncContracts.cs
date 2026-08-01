namespace BrainHarbor.Pipeline.Publishing;

/// <summary>
/// Client-side mirror of the site's sync contracts (Web/Api/SyncContracts.cs,
/// architecture.md §4). Deliberately duplicated rather than shared via a
/// project reference: the Pipeline runs on Dan's PC against a deployed site
/// and must not take a dependency on the web app's assembly. Changes to
/// either side are breaking changes — the round-trip test in
/// BrainHarbor.Tests pins them together.
/// </summary>

public sealed record SyncStateResponse(IReadOnlyList<SourceState> Sources);

public sealed record SourceState(
    string Source,
    DateTimeOffset? LastSuccessAt,
    string? LastError,
    string? Cursor);

public sealed record ItemKey(string Source, string ExternalId);

public sealed record CheckRequest(IReadOnlyList<ItemKey> Keys);

public sealed record CheckResponse(IReadOnlyList<ItemKey> New);

public sealed record SyncItem
{
    public required string Source { get; init; }
    public required string SourceKind { get; init; }
    public required string ExternalId { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public string? RawSummary { get; init; }
    public DateOnly? PublishedAt { get; init; }

    public IReadOnlyList<string> TumorTags { get; init; } = [];
    public string? ResearchStage { get; init; }
    public string? Relevance { get; init; }
    public string? ClassifyModel { get; init; }

    public string? PlainTitle { get; init; }
    public string? PlainSummary { get; init; }
    public string? PlainWhatStudied { get; init; }
    public string? PlainWhatFound { get; init; }
    public string? PlainMeans { get; init; }
    public string? PlainDoesntMean { get; init; }

    /// <summary>How close this is to everyday care, 1-10 (Readiness scale),
    /// already clamped by research stage. Null until summarized.</summary>
    public int? ReadinessScore { get; init; }
    public string? ReadinessReason { get; init; }

    public string? SummaryModel { get; init; }
    public string? PromptVersion { get; init; }
    public bool SummaryFlagged { get; init; }
}

/// <summary>
/// The FACTS about one ClinicalTrials.gov record (WI-402), uploaded through
/// their own endpoint. Facts refresh unconditionally; the plain-language text
/// is editorial and travels as a normal item, so there is no summary field
/// here.
/// </summary>
public sealed record TrialFacts
{
    public required string NctId { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public IReadOnlyList<string> Conditions { get; init; } = [];
    public string? Phase { get; init; }
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

public sealed record CursorRequest(string Source, string Cursor);

public sealed record FailureRequest(string Source, string Error);

public sealed record TaxonomyResponse(IReadOnlyList<TaxonomyTypeDto> Types);

public sealed record TaxonomyTypeDto(string Slug, string Label, IReadOnlyList<string> Aliases);

public sealed record UploadResponse(
    int Inserted,
    int Updated,
    int Rejected,
    IReadOnlyList<string> RejectedTumorTags,
    IReadOnlyList<string> Errors)
{
    public int Frozen { get; init; }
    public int AutoPublished { get; init; }
}
