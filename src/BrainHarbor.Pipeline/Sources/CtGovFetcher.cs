using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BrainHarbor.Pipeline.Publishing;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// WI-402: ClinicalTrials.gov API v2 — the trial finder's supply line
/// (PLAN.md §5, data-model.md §trials_cache).
///
/// Public domain, attribution required, no API key. Two things come out of one
/// fetch:
///   * every updated trial's FACTS, which refresh trials_cache and back
///     /trials browse;
///   * a `trial_update` feed item for the trials a patient could actually act
///     on, which flows through the normal classify / summarize / publish path.
///
/// The two halves travel separately, because they obey opposite rules. Facts
/// go through their own endpoint and refresh on EVERY run, whatever happened
/// to the feed item. The feed item is editorial: it is written once, gated by
/// the automated safety checks, and never rewritten for a trial we already
/// know. So a trial that closes updates its facts (and its page stops saying
/// it is open) without costing a single summary.
///
/// The window is LastUpdatePostDate, sorted OLDEST first. That ordering is
/// deliberate: if a window holds more than one run can read, the cursor can
/// move to the newest date actually read and the remainder is picked up next
/// run rather than being lost. (A gap longer than MaxLookbackDays is still
/// skipped — the same trade PubMedFetcher makes, and the reason a source that
/// stops reporting shows up on the admin health page.)
///
/// Rate limiting: the published limit is undocumented, so 429 is treated as a
/// normal condition — honour Retry-After, back off, and only then fail the
/// source (PLAN.md §5).
/// </summary>
public sealed class CtGovFetcher(
    HttpClient httpClient,
    ILogger<CtGovFetcher> logger,
    TimeProvider? timeProvider = null) : ISourceFetcher
{
    public const string SourceName = "ctgov";

    /// <summary>First run looks back this far rather than at all of history.</summary>
    public const int FirstRunLookbackDays = 30;

    /// <summary>Cap on a catch-up window after a long outage.</summary>
    public const int MaxLookbackDays = 180;

    /// <summary>Studies per page. The API allows up to 1000; 100 keeps
    /// responses small enough to stay well inside the request timeout.</summary>
    public const int PageSize = 100;

    /// <summary>Ceiling on one run, so a wide catch-up window can't run for hours.</summary>
    public const int MaxPages = 40;

    /// <summary>Retries for a 429 before the source is failed for this run.</summary>
    public const int MaxRateLimitRetries = 4;

    /// <summary>
    /// Statuses a patient can act on. Only these become FEED items — an
    /// administrative edit to a trial that finished in 2011 is not news, and
    /// filling the feed with it would bury the trials someone could join.
    /// Every fetched trial still refreshes trials_cache regardless, which is
    /// how a trial that just stopped recruiting stops being advertised as open.
    /// </summary>
    private static readonly HashSet<string> ActionableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "NOT_YET_RECRUITING", "RECRUITING", "ENROLLING_BY_INVITATION", "AVAILABLE",
    };

    /// <summary>
    /// The condition query, in the API's Essie syntax. Broader than the PubMed
    /// query on purpose: a trial's condition list is short and curated, so
    /// there is no firehose to filter here.
    /// </summary>
    public const string ConditionQuery =
        "(glioma OR glioblastoma OR astrocytoma OR oligodendroglioma OR " +
        "\"brain tumor\" OR \"brain tumour\" OR \"brain cancer\" OR " +
        "\"brain neoplasm\" OR \"CNS tumor\" OR \"central nervous system neoplasm\" OR " +
        "meningioma OR medulloblastoma OR ependymoma OR craniopharyngioma OR " +
        "\"diffuse midline glioma\" OR DIPG OR \"primary CNS lymphoma\" OR " +
        "\"vestibular schwannoma\" OR \"acoustic neuroma\" OR " +
        "\"pituitary tumor\" OR \"pituitary adenoma\" OR " +
        "\"spinal cord tumor\" OR \"brain metastases\" OR \"brain metastasis\" OR " +
        "\"leptomeningeal disease\")";

    /// <summary>
    /// Only the modules we store. Asking for everything returns eligibility
    /// criteria, outcome measures and sponsor trees we neither use nor have a
    /// licence question about — smaller responses, fewer surprises.
    /// </summary>
    private const string Fields =
        "protocolSection.identificationModule.nctId," +
        "protocolSection.identificationModule.briefTitle," +
        "protocolSection.statusModule.overallStatus," +
        "protocolSection.statusModule.lastUpdatePostDateStruct," +
        "protocolSection.descriptionModule.briefSummary," +
        "protocolSection.conditionsModule.conditions," +
        "protocolSection.designModule.phases," +
        "protocolSection.contactsLocationsModule.locations";

    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public string Source => SourceName;

    /// <summary>The only source with facts worth refreshing without the CLI
    /// (WI-413): trial status, phase and sites go stale into a false claim on a
    /// page a patient reads.</summary>
    public bool ProducesTrialFacts => true;

    public async Task<FetchResult> FetchAsync(string? cursor, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var from = StartDateFor(cursor, today);

        logger.LogInformation("[ctgov] fetching trials updated since {From}.", from);

        var items = new List<FetchedItem>();
        var pageToken = (string?)null;
        var total = 0;
        var read = 0;
        var truncated = false;

        for (var page = 0; page < MaxPages; page++)
        {
            var response = await GetPageAsync(from, pageToken, cancellationToken);
            var studies = response?.Studies ?? [];
            if (studies.Count == 0)
            {
                break;
            }

            if (page == 0 && response?.TotalCount is > 0)
            {
                total = response.TotalCount.Value;
            }

            read += studies.Count;
            items.AddRange(studies.Select(ToFetchedItem).Where(i => i is not null)!);

            pageToken = response?.NextPageToken;
            if (string.IsNullOrEmpty(pageToken))
            {
                break;
            }

            if (page == MaxPages - 1)
            {
                truncated = true;
            }
        }

        logger.LogInformation("[ctgov] read {Read} trial(s) of {Total}; {Feed} are open to patients.",
            read, total > 0 ? total : read, items.Count(i => i.FeedWorthy));

        if (truncated)
        {
            // Oldest-first ordering makes this recoverable: everything up to
            // the newest date we actually read is done, and the rest of the
            // window is still ahead of the cursor.
            var newestRead = items.Select(i => i.Trial?.LastUpdatePosted).Where(d => d is not null).Max();

            // ...but only if it is actually forward. The window starts a day
            // BEHIND the cursor (the overlap), so a run truncated among the
            // oldest records could otherwise hand back a cursor earlier than
            // the one it was given — widening the window every night while
            // re-reading the same records and never reaching the newer ones.
            var previous = DateOnly.TryParse(cursor, CultureInfo.InvariantCulture, out var last)
                ? last
                : (DateOnly?)null;

            if (newestRead is null || (previous is { } p && newestRead <= p))
            {
                // No forward progress. Report it as a failure (so it shows on
                // the admin health page rather than looking like a quiet night)
                // and hold the cursor — but still hand back the records we read.
                // Their FACTS are worth storing, and since the next run stalls
                // identically, throwing them away here throws them away forever.
                return new FetchResult(items, null,
                    $"read {read} record(s) without advancing past " +
                    $"{previous?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "the start of the window"}. " +
                    "The page cap may be too low for this window.");
            }

            logger.LogWarning(
                "[ctgov] window holds {Total} trial(s), read {Read} before the page cap. " +
                "Cursor moves to {Cursor} so the next run continues from there.",
                total, read, newestRead);

            return new FetchResult(items,
                newestRead.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        return new FetchResult(items, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// One page, retrying the failures a once-daily unattended job must
    /// survive: a 429 (the rate limit is undocumented — PLAN.md §5), a 5xx, and
    /// a dropped connection. A busy minute must cost a pause, not the day's
    /// trials. Retries live here rather than in a resilience handler so the
    /// server's Retry-After is honoured once, not multiplied by two layers.
    ///
    /// Everything else (a 400 from a reshaped API, a 404) throws immediately:
    /// those do not get better by waiting, and throwing keeps the cursor put so
    /// the window is retried whole next run.
    /// </summary>
    private async Task<CtGovResponse?> GetPageAsync(
        DateOnly from, string? pageToken, CancellationToken cancellationToken)
    {
        var url = BuildUrl(from, pageToken);

        for (var attempt = 0; ; attempt++)
        {
            var (page, delay, reason) = await TryGetPageAsync(url, attempt, cancellationToken);
            if (reason is null)
            {
                return page;
            }

            if (attempt >= MaxRateLimitRetries)
            {
                // Out of patience. Throwing keeps the cursor put, so the whole
                // window is retried on the next run.
                throw new CtGovRequestException(
                    $"ClinicalTrials.gov failed {MaxRateLimitRetries + 1} attempts in a row ({reason}).");
            }

            logger.LogWarning("[ctgov] {Reason} — waiting {Seconds}s before retry {Attempt}.",
                reason, delay.TotalSeconds, attempt + 1);
            await Task.Delay(delay, _time, cancellationToken);
        }
    }

    /// <summary>
    /// One attempt. Returns the page on success, or the delay and a reason when
    /// the failure is worth retrying. A failure that waiting cannot fix (a 400
    /// from a reshaped API, a 404 from a moved endpoint) throws instead —
    /// retrying it only delays the alarm.
    /// </summary>
    private async Task<(CtGovResponse? Page, TimeSpan Delay, string? Reason)> TryGetPageAsync(
        string url, int attempt, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (IsTransient(response.StatusCode))
            {
                return (null, RetryDelayFor(response, attempt, _time), $"HTTP {(int)response.StatusCode}");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new CtGovRequestException(
                    $"ClinicalTrials.gov returned {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            return (await response.Content.ReadFromJsonAsync<CtGovResponse>(cancellationToken),
                TimeSpan.Zero, null);
        }
        catch (HttpRequestException exception)
        {
            // A reset connection, a DNS blip, a TLS hiccup — all worth a retry.
            return (null, BackoffFor(attempt), exception.Message);
        }
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout
            or >= HttpStatusCode.InternalServerError;

    /// <summary>Exponential backoff: 2s, 4s, 8s, 16s.</summary>
    private static TimeSpan BackoffFor(int attempt) =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));

    /// <summary>
    /// Retry-After when the server sends one (it knows better than we do),
    /// otherwise exponential backoff. Capped so a misconfigured header can't
    /// park an unattended nightly run for an hour.
    /// </summary>
    internal static TimeSpan RetryDelayFor(
        HttpResponseMessage response, int attempt, TimeProvider? time = null)
    {
        var retryAfter = response.Headers.RetryAfter;
        var suggested = retryAfter?.Delta
                        ?? (retryAfter?.Date is { } date
                            ? date - (time ?? TimeProvider.System).GetUtcNow()
                            : null);

        var delay = suggested is { } value && value > TimeSpan.Zero ? value : BackoffFor(attempt);
        return delay > MaxRetryDelay ? MaxRetryDelay : delay;
    }

    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(2);

    internal static DateOnly StartDateFor(string? cursor, DateOnly today)
    {
        if (!DateOnly.TryParse(cursor, CultureInfo.InvariantCulture, out var last))
        {
            return today.AddDays(-FirstRunLookbackDays);
        }

        // One day of overlap: ClinicalTrials.gov refreshes on a schedule and
        // the server dedupes, so the overlap is free.
        var start = last.AddDays(-1);
        var earliest = today.AddDays(-MaxLookbackDays);
        return start < earliest ? earliest : start;
    }

    private string BuildUrl(DateOnly from, string? pageToken)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["query.cond"] = ConditionQuery,
            ["filter.advanced"] =
                $"AREA[LastUpdatePostDate]RANGE[{from:yyyy-MM-dd},MAX]",
            ["sort"] = "LastUpdatePostDate:asc",
            ["fields"] = Fields,
            ["pageSize"] = PageSize.ToString(CultureInfo.InvariantCulture),
            ["countTotal"] = pageToken is null ? "true" : null,
            ["pageToken"] = pageToken,
        };

        var query = string.Join("&", parameters
            .Where(p => p.Value is not null)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));

        return $"studies?{query}";
    }

    /// <summary>
    /// Maps one API study. Internal so the tests can run it against recorded
    /// responses rather than the live registry.
    /// </summary>
    internal static FetchedItem? ToFetchedItem(CtGovStudy study)
    {
        var section = study.ProtocolSection;
        var nctId = section?.IdentificationModule?.NctId?.Trim();
        var title = section?.IdentificationModule?.BriefTitle;

        if (string.IsNullOrWhiteSpace(nctId) || string.IsNullOrWhiteSpace(title))
        {
            return null;   // Without an id or a title there is nothing to show.
        }

        var locations = (section?.ContactsLocationsModule?.Locations ?? [])
            .Select(l => new TrialLocation(
                Trim(l.Facility),
                Trim(l.City),
                Trim(l.State),
                Trim(l.Country),
                l.GeoPoint?.Lat,
                l.GeoPoint?.Lon))
            .ToList();

        var status = section?.StatusModule?.OverallStatus;
        var summary = section?.DescriptionModule?.BriefSummary;
        var lastUpdate = ParseDate(section?.StatusModule?.LastUpdatePostDateStruct?.Date);

        return new FetchedItem
        {
            Source = SourceName,
            SourceKind = "trial_update",
            ExternalId = nctId,
            Title = CollapseWhitespace(title),
            Url = $"https://clinicaltrials.gov/study/{nctId}",
            RawSummary = string.IsNullOrWhiteSpace(summary) ? null : CollapseWhitespace(summary),

            // The feed date is the update itself: that is what makes a trial
            // "new" to a reader, not the year it was first registered.
            PublishedAt = lastUpdate,

            // A trial's record changes in place, but a CHANGED record does not
            // need a new summary: the summary says who the trial is for, and
            // the live status comes from trials_cache at render time. So a
            // trial we already know is skipped by the ordinary "only new items"
            // filter, and its facts refresh through the trials endpoint. That
            // is what stops a nightly run re-summarizing the same trials.
            AlwaysUpload = false,

            // Only a trial someone can still join is news. The rest are fetched
            // to keep trials_cache honest and get no feed row at all.
            FeedWorthy = status is not null && ActionableStatuses.Contains(status.Trim()),

            Trial = new TrialFacts
            {
                NctId = nctId,
                Title = CollapseWhitespace(title),
                Summary = string.IsNullOrWhiteSpace(summary) ? null : CollapseWhitespace(summary),
                Conditions = [.. (section?.ConditionsModule?.Conditions ?? [])
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(CollapseWhitespace)],
                Phase = PlainPhase(section?.DesignModule?.Phases),
                OverallStatus = PlainStatus(status),
                Locations = locations,
                LastUpdatePosted = lastUpdate,
            },
        };
    }

    /// <summary>
    /// Turns the API's enum ("ACTIVE_NOT_RECRUITING") into words a patient
    /// reads without translating. An unknown value is title-cased rather than
    /// dropped — showing SOMETHING true beats showing nothing.
    /// </summary>
    internal static string? PlainStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "NOT_YET_RECRUITING" => "Not yet recruiting",
            "RECRUITING" => "Recruiting",
            "ENROLLING_BY_INVITATION" => "Enrolling by invitation",
            "ACTIVE_NOT_RECRUITING" => "Active, not recruiting",
            "SUSPENDED" => "Paused",
            "TERMINATED" => "Stopped early",
            "COMPLETED" => "Completed",
            "WITHDRAWN" => "Withdrawn before starting",
            "AVAILABLE" => "Available",
            "NO_LONGER_AVAILABLE" => "No longer available",
            "TEMPORARILY_NOT_AVAILABLE" => "Temporarily not available",
            "APPROVED_FOR_MARKETING" => "Approved for marketing",
            "WITHHELD" => "Withheld",
            "UNKNOWN" => "Status unknown",
            var other => TitleCase(other),
        };
    }

    /// <summary>
    /// "PHASE2" -> "Phase 2"; ["PHASE2","PHASE3"] -> "Phase 2/Phase 3";
    /// "NA" -> "Not applicable" (which is what a device or behavioural study
    /// legitimately is, not missing data).
    /// </summary>
    internal static string? PlainPhase(IReadOnlyList<string>? phases)
    {
        if (phases is null || phases.Count == 0)
        {
            return null;
        }

        var parts = phases
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToUpperInvariant() switch
            {
                "NA" => "Not applicable",
                "EARLY_PHASE1" => "Early phase 1",
                "PHASE1" => "Phase 1",
                "PHASE2" => "Phase 2",
                "PHASE3" => "Phase 3",
                "PHASE4" => "Phase 4",
                var other => TitleCase(other),
            })
            .ToList();

        return parts.Count == 0 ? null : string.Join("/", parts);
    }

    private static string TitleCase(string value)
    {
        var words = value.Replace('_', ' ').ToLowerInvariant();
        return words.Length == 0 ? words : char.ToUpperInvariant(words[0]) + words[1..];
    }

    private static DateOnly? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // The API gives yyyy-MM-dd, but a month-precision date (yyyy-MM) is
        // legal for some date fields — take the first of the month rather than
        // dropping the item's only date and sorting it to the bottom forever.
        if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var full))
        {
            return full;
        }

        return DateOnly.TryParseExact(text, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var month) ? month : null;
    }

    private static string? Trim(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : CollapseWhitespace(text);

    private static string CollapseWhitespace(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

// The API v2 response shape (verified against the live endpoint). Only the
// modules requested in Fields are populated.

public sealed class CtGovResponse
{
    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("studies")]
    public List<CtGovStudy> Studies { get; set; } = [];

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public sealed class CtGovStudy
{
    [JsonPropertyName("protocolSection")]
    public CtGovProtocolSection? ProtocolSection { get; set; }
}

public sealed class CtGovProtocolSection
{
    [JsonPropertyName("identificationModule")]
    public CtGovIdentification? IdentificationModule { get; set; }

    [JsonPropertyName("statusModule")]
    public CtGovStatus? StatusModule { get; set; }

    [JsonPropertyName("descriptionModule")]
    public CtGovDescription? DescriptionModule { get; set; }

    [JsonPropertyName("conditionsModule")]
    public CtGovConditions? ConditionsModule { get; set; }

    [JsonPropertyName("designModule")]
    public CtGovDesign? DesignModule { get; set; }

    [JsonPropertyName("contactsLocationsModule")]
    public CtGovContactsLocations? ContactsLocationsModule { get; set; }
}

public sealed class CtGovIdentification
{
    [JsonPropertyName("nctId")]
    public string? NctId { get; set; }

    [JsonPropertyName("briefTitle")]
    public string? BriefTitle { get; set; }
}

public sealed class CtGovStatus
{
    [JsonPropertyName("overallStatus")]
    public string? OverallStatus { get; set; }

    [JsonPropertyName("lastUpdatePostDateStruct")]
    public CtGovDateStruct? LastUpdatePostDateStruct { get; set; }
}

public sealed class CtGovDateStruct
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

public sealed class CtGovDescription
{
    [JsonPropertyName("briefSummary")]
    public string? BriefSummary { get; set; }
}

public sealed class CtGovConditions
{
    [JsonPropertyName("conditions")]
    public List<string> Conditions { get; set; } = [];
}

public sealed class CtGovDesign
{
    [JsonPropertyName("phases")]
    public List<string> Phases { get; set; } = [];
}

public sealed class CtGovContactsLocations
{
    [JsonPropertyName("locations")]
    public List<CtGovLocation> Locations { get; set; } = [];
}

public sealed class CtGovLocation
{
    [JsonPropertyName("facility")]
    public string? Facility { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("geoPoint")]
    public CtGovGeoPoint? GeoPoint { get; set; }
}

public sealed class CtGovGeoPoint
{
    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lon")]
    public double? Lon { get; set; }
}

/// <summary>
/// A ClinicalTrials.gov request failed in a way retrying will not fix, or
/// failed too many times. Distinct from HttpRequestException so the retry loop
/// cannot mistake its own give-up signal for another transient blip.
/// </summary>
public sealed class CtGovRequestException(string message) : Exception(message);
