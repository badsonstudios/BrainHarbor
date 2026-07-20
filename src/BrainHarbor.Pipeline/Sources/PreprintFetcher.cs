using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// WI-206: medRxiv / bioRxiv preprints.
///
/// Preprints have NOT been checked by other scientists, so they carry a
/// permanent badge and can never reach the front page. That rule is enforced
/// three times over — here (source_kind is always "preprint"), in the sync
/// API's validation, and as a database CHECK constraint — because a preprint
/// presented as settled science is one of the named failure modes in
/// content-pipeline.md §11.
///
/// Metadata only (title, abstract, DOI, date): that is what the medRxiv/
/// bioRxiv API terms allow (PLAN.md §5).
/// </summary>
public sealed class PreprintFetcher(
    HttpClient httpClient,
    ILogger<PreprintFetcher> logger,
    string server,
    TimeProvider? timeProvider = null) : ISourceFetcher
{
    /// <summary>First run looks back this far rather than all of history.</summary>
    public const int FirstRunLookbackDays = 14;

    /// <summary>Cap on a catch-up window.</summary>
    public const int MaxLookbackDays = 90;

    /// <summary>
    /// The API returns 30 records per page regardless of what you ask for —
    /// verified against the live endpoint. Assuming 100 made the fetcher stop
    /// after one page (a "short" page looked like the end of data) while the
    /// cursor advanced past everything it never read.
    /// </summary>
    public const int PageSize = 30;

    /// <summary>
    /// Ceiling per run. These servers post ~250/day across all fields, so a
    /// 14-day first run is thousands of records; this bounds one run without
    /// losing the remainder (see the partial cursor in FetchAsync).
    /// </summary>
    public const int MaxPages = 150;

    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public string Source => server;   // "medrxiv" or "biorxiv"

    public async Task<FetchResult> FetchAsync(string? cursor, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var from = StartDateFor(cursor, today);

        logger.LogInformation("[{Source}] fetching {From} to {To}.", server, from, today);

        var all = new List<FetchedItem>();
        var fetchedCount = 0;
        var total = int.MaxValue;

        for (var page = 0; page < MaxPages && fetchedCount < total; page++)
        {
            var url = $"details/{server}/" +
                      $"{from:yyyy-MM-dd}/{today:yyyy-MM-dd}/" +
                      (page * PageSize).ToString(CultureInfo.InvariantCulture) +
                      "/json";

            var response = await httpClient.GetFromJsonAsync<PreprintResponse>(url, cancellationToken);
            var batch = response?.Collection ?? [];
            if (batch.Count == 0)
            {
                total = fetchedCount;   // genuinely the end
                break;
            }

            // Completion is decided by the API's own total, not by a short
            // page — short pages are normal here.
            if (response?.Messages is [{ Total: > 0 } message, ..])
            {
                total = message.Total;
            }

            fetchedCount += batch.Count;
            all.AddRange(batch.Select(r => ToFetchedItem(r, server)).Where(i => i is not null)!);
        }

        // These servers publish every field, so an item must POSITIVELY
        // mention a brain-tumor term to be kept. Keep-bias is right where the
        // source already selected for us; here it passed 91% of the firehose
        // into the review queue (measured in the WI-211 shakedown).
        var kept = all
            .Where(i => !BrainTumorPreFilter.ShouldExclude(
                i.Title, i.RawSummary, SourceScope.EverythingFirehose))
            .ToList();

        logger.LogInformation("[{Source}] {Kept} of {Fetched} preprint(s) look relevant.",
            server, kept.Count, fetchedCount);

        if (fetchedCount < total)
        {
            // Truncated. Advance to the newest date we actually read rather
            // than to today (which would skip the rest) or holding the cursor
            // still (which would re-read the same slice forever).
            var newestRead = all.Select(i => i.PublishedAt).Where(d => d is not null).Max();
            logger.LogWarning(
                "[{Source}] window holds {Total} records, read {Fetched}. Cursor moves to {Cursor} " +
                "so the next run continues from there.",
                server, total, fetchedCount, newestRead?.ToString("yyyy-MM-dd") ?? "unchanged");

            return new FetchResult(kept,
                newestRead?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        return new FetchResult(kept, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    internal static DateOnly StartDateFor(string? cursor, DateOnly today)
    {
        if (!DateOnly.TryParse(cursor, CultureInfo.InvariantCulture, out var last))
        {
            return today.AddDays(-FirstRunLookbackDays);
        }

        // One day of overlap; the server dedupes, so it costs nothing.
        var start = last.AddDays(-1);
        var earliest = today.AddDays(-MaxLookbackDays);
        return start < earliest ? earliest : start;
    }

    /// <summary>
    /// Maps one API record. <paramref name="server"/> is the fetcher's own
    /// source name and is the fallback — defaulting to a hardcoded "medrxiv"
    /// would file bioRxiv items under the wrong source, which also
    /// desynchronizes the dedupe key from the cursor key.
    /// </summary>
    internal static FetchedItem? ToFetchedItem(PreprintRecord record, string server)
    {
        if (string.IsNullOrWhiteSpace(record.Doi) || string.IsNullOrWhiteSpace(record.Title))
        {
            return null;
        }

        return new FetchedItem
        {
            Source = string.IsNullOrWhiteSpace(record.Server)
                ? server
                : record.Server.ToLowerInvariant(),
            // NEVER anything else: this is what drives the permanent
            // "not yet checked by other scientists" badge, and the sync API
            // and database both refuse a preprint marked patient_relevant.
            SourceKind = "preprint",
            ExternalId = record.Doi.Trim(),
            Title = CollapseWhitespace(record.Title),
            Url = $"https://doi.org/{record.Doi.Trim()}",
            RawSummary = string.IsNullOrWhiteSpace(record.Abstract)
                ? null
                : CollapseWhitespace(record.Abstract),
            PublishedAt = DateOnly.TryParse(record.Date, CultureInfo.InvariantCulture, out var date)
                ? date
                : null,
        };
    }

    private static string CollapseWhitespace(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

public sealed class PreprintResponse
{
    [JsonPropertyName("collection")]
    public List<PreprintRecord> Collection { get; set; } = [];

    [JsonPropertyName("messages")]
    public List<PreprintMessage> Messages { get; set; } = [];
}

public sealed class PreprintMessage
{
    /// <summary>Total records the window holds — how we know when to stop.</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public sealed class PreprintRecord
{
    [JsonPropertyName("doi")]
    public string? Doi { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("server")]
    public string? Server { get; set; }
}
