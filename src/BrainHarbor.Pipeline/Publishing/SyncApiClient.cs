using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Publishing;

public interface ISyncApiClient
{
    Task<IReadOnlyDictionary<string, SourceState>> GetStateAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ItemKey>> FindNewAsync(
        IReadOnlyList<ItemKey> keys, CancellationToken cancellationToken);

    Task<UploadResponse> UploadAsync(
        IReadOnlyList<SyncItem> items, string? cursor, CancellationToken cancellationToken);

    /// <summary>Marks a window done when it produced nothing to upload.</summary>
    Task AdvanceCursorAsync(string source, string cursor, CancellationToken cancellationToken);

    /// <summary>Reports a source failure so staleness is visible in admin.</summary>
    Task ReportFailureAsync(string source, string error, CancellationToken cancellationToken);

    /// <summary>The closed tumor taxonomy, for building the classifier prompt.</summary>
    Task<IReadOnlyList<TaxonomyTypeDto>> GetTaxonomyAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Typed client for the site's sync API. Batches to the server's limit,
/// surfaces a clear error for the auth/misconfiguration cases the operator
/// can actually fix, and never logs the API key.
/// </summary>
public sealed class SyncApiClient(HttpClient httpClient, ILogger<SyncApiClient> logger) : ISyncApiClient
{
    /// <summary>Matches the server's SyncEndpoints.MaxBatchSize.</summary>
    public const int MaxBatchSize = 500;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyDictionary<string, SourceState>> GetStateAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("/api/sync/state", cancellationToken);
        await EnsureSuccessAsync(response, "GET /api/sync/state", cancellationToken);

        var state = await response.Content.ReadFromJsonAsync<SyncStateResponse>(Json, cancellationToken);
        return state?.Sources.ToDictionary(s => s.Source, StringComparer.Ordinal)
               ?? new Dictionary<string, SourceState>(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<ItemKey>> FindNewAsync(
        IReadOnlyList<ItemKey> keys, CancellationToken cancellationToken)
    {
        var unseen = new List<ItemKey>();

        foreach (var chunk in keys.Chunk(MaxBatchSize))
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/sync/check", new CheckRequest(chunk), Json, cancellationToken);
            await EnsureSuccessAsync(response, "POST /api/sync/check", cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<CheckResponse>(Json, cancellationToken);
            if (body is not null)
            {
                unseen.AddRange(body.New);
            }
        }

        logger.LogInformation("{New} of {Total} items are new.", unseen.Count, keys.Count);
        return unseen;
    }

    public async Task AdvanceCursorAsync(
        string source, string cursor, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/sync/cursor", new CursorRequest(source, cursor), Json, cancellationToken);
        await EnsureSuccessAsync(response, "POST /api/sync/cursor", cancellationToken);

        logger.LogInformation("[{Source}] cursor advanced to {Cursor} (nothing new).", source, cursor);
    }

    public async Task<IReadOnlyList<TaxonomyTypeDto>> GetTaxonomyAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("/api/sync/taxonomy", cancellationToken);
        await EnsureSuccessAsync(response, "GET /api/sync/taxonomy", cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<TaxonomyResponse>(Json, cancellationToken);
        return body?.Types ?? [];
    }

    public async Task ReportFailureAsync(
        string source, string error, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/sync/failure", new FailureRequest(source, error), Json, cancellationToken);
            await EnsureSuccessAsync(response, "POST /api/sync/failure", cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Reporting a failure must never itself break the run — if the
            // site is unreachable, that is already the bigger problem.
            logger.LogWarning("Could not report the {Source} failure to the site: {Message}",
                source, exception.Message);
        }
    }

    public async Task<UploadResponse> UploadAsync(
        IReadOnlyList<SyncItem> items, string? cursor, CancellationToken cancellationToken)
    {
        // Enumerable.Chunk yields NO chunks for an empty sequence, so an empty
        // upload would silently make no request at all. Cursor-only progress
        // goes through AdvanceCursorAsync; an empty upload here is a bug.
        if (items.Count == 0)
        {
            throw new ArgumentException(
                "UploadAsync requires at least one item; use AdvanceCursorAsync to move a cursor.",
                nameof(items));
        }

        var inserted = 0;
        var updated = 0;
        var frozen = 0;
        var autoPublished = 0;
        var rejected = 0;
        var rejectedTags = new List<string>();
        var errors = new List<string>();

        var chunks = items.Chunk(MaxBatchSize).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            // The cursor marks "everything up to here is fetched", so it may
            // only be sent with the LAST chunk — sending it earlier would
            // advance the window past items that haven't uploaded yet.
            var chunkCursor = i == chunks.Count - 1 ? cursor : null;

            using var response = await httpClient.PostAsJsonAsync(
                "/api/sync/items", new UploadRequest(chunks[i], chunkCursor), Json, cancellationToken);
            await EnsureSuccessAsync(response, "POST /api/sync/items", cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<UploadResponse>(Json, cancellationToken)
                       ?? throw new SyncApiException("Sync API returned an empty upload response.");

            inserted += body.Inserted;
            updated += body.Updated;
            frozen += body.Frozen;
            autoPublished += body.AutoPublished;
            rejected += body.Rejected;
            rejectedTags.AddRange(body.RejectedTumorTags);
            errors.AddRange(body.Errors);
        }

        if (rejectedTags.Count > 0)
        {
            logger.LogWarning("Server rejected unknown tumor tags: {Tags}. " +
                              "Recurring ones belong in Content/taxonomy.yml.",
                string.Join(", ", rejectedTags.Distinct()));
        }

        foreach (var error in errors)
        {
            logger.LogWarning("Item rejected by the sync API: {Error}", error);
        }

        return new UploadResponse(inserted, updated, rejected, [.. rejectedTags.Distinct()], errors)
        {
            Frozen = frozen,
            AutoPublished = autoPublished,
        };
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response, string what, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // Actionable messages for the failures an operator can actually fix.
        var hint = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                "the sync API key is wrong or missing — check Pipeline:SyncApiKey in user-secrets",
            HttpStatusCode.ServiceUnavailable =>
                "the SITE has no SYNC_API_KEY configured — set it in App Service configuration",
            HttpStatusCode.TooManyRequests =>
                "rate limited — the pipeline is running too often or something else is using the key",
            _ => await response.Content.ReadAsStringAsync(cancellationToken),
        };

        throw new SyncApiException($"{what} failed ({(int)response.StatusCode}): {hint}");
    }
}

public sealed class SyncApiException(string message) : Exception(message);
