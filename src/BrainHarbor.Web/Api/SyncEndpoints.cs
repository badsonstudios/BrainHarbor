namespace BrainHarbor.Web.Api;

/// <summary>
/// The sync API (architecture.md §4) — the only write surface into the site.
/// Every endpoint is API-key protected and rate limited; uploads always land
/// as status='pending' so nothing reaches readers without human approval.
/// </summary>
public static class SyncEndpoints
{
    public const string RateLimitPolicy = "sync";

    /// <summary>Max items in one upload batch — bounds memory and one bad
    /// pipeline run's blast radius.</summary>
    public const int MaxBatchSize = 500;

    public static void MapSyncApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sync")
            .AddEndpointFilter<SyncApiKeyFilter>()
            .RequireRateLimiting(RateLimitPolicy);

        group.MapGet("/state", async (SyncRepository repository, CancellationToken cancellationToken) =>
            Results.Ok(new SyncStateResponse(await repository.GetStateAsync(cancellationToken))));

        // The closed taxonomy, so the pipeline's classifier prompt lists the
        // exact slugs the model may emit — one source of truth, served rather
        // than duplicated in the console app (WI-303).
        group.MapGet("/taxonomy", (BrainHarbor.Web.Content.TaxonomyStore taxonomy) =>
            Results.Ok(new TaxonomyResponse(
            [
                .. taxonomy.TumorTypes.Select(t => new TaxonomyTypeDto(t.Slug, t.Label, t.Also)),
            ])));

        group.MapPost("/check", async (
            CheckRequest? request, SyncRepository repository, CancellationToken cancellationToken) =>
        {
            // Minimal APIs don't enforce non-nullable reference types during
            // JSON binding, so `{}` arrives with null members. On the only
            // write surface an odd body must be a clean 400, never a 500.
            if (request?.Keys is null)
            {
                return Results.BadRequest(new { error = "keys is required" });
            }

            if (request.Keys.Any(k => k is null || string.IsNullOrWhiteSpace(k.Source)
                                      || string.IsNullOrWhiteSpace(k.ExternalId)))
            {
                return Results.BadRequest(new { error = "every key needs a source and externalId" });
            }

            if (request.Keys.Count > MaxBatchSize)
            {
                return Results.BadRequest(new { error = $"at most {MaxBatchSize} keys per request" });
            }

            return Results.Ok(new CheckResponse(
                await repository.FindNewAsync(request.Keys, cancellationToken)));
        });

        // Advance a cursor with nothing to upload (the window held only
        // already-known items). Separate from /items so an empty upload stays
        // an error rather than an ambiguous no-op.
        group.MapPost("/cursor", async (
            CursorRequest? request, SyncRepository repository, CancellationToken cancellationToken) =>
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Source) ||
                string.IsNullOrWhiteSpace(request.Cursor))
            {
                return Results.BadRequest(new { error = "source and cursor are required" });
            }

            return await repository.AdvanceCursorAsync(request.Source, request.Cursor, cancellationToken)
                ? Results.Ok()
                : Results.BadRequest(new { error = $"unknown source '{request.Source}'" });
        });

        // Report a failure so the admin health page shows it. Without this,
        // source_sync_state.last_error is never written and a source that has
        // been broken for a week still shows its last success.
        group.MapPost("/failure", async (
            FailureRequest? request, SyncRepository repository, CancellationToken cancellationToken) =>
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Source) ||
                string.IsNullOrWhiteSpace(request.Error))
            {
                return Results.BadRequest(new { error = "source and error are required" });
            }

            return await repository.RecordFailureAsync(
                request.Source, request.Error, cancellationToken)
                ? Results.Ok()
                : Results.BadRequest(new { error = $"unknown source '{request.Source}'" });
        });

        // Trial FACTS (WI-402). A separate door from /items on purpose: these
        // are facts about the world (status, phase, sites) and refresh
        // unconditionally, where an item's plain-language text is editorial and
        // is gated, editable, and frozen once reviewed. Uploading facts first
        // means the worst case is a trial whose facts are known before its feed
        // item exists, which harms nobody.
        group.MapPost("/trials", async (
            TrialsRequest? request, SyncRepository repository,
            ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            if (request?.Trials is null)
            {
                return Results.BadRequest(new { error = "trials is required" });
            }

            if (request.Trials.Count == 0)
            {
                return Results.BadRequest(new { error = "no trials" });
            }

            if (request.Trials.Any(t => t is null))
            {
                return Results.BadRequest(new { error = "trials may not contain nulls" });
            }

            if (request.Trials.Count > MaxBatchSize)
            {
                return Results.BadRequest(new { error = $"at most {MaxBatchSize} trials per request" });
            }

            var result = await repository.UpsertTrialsAsync(request.Trials, cancellationToken);

            if (result.Errors.Count > 0)
            {
                loggerFactory.CreateLogger(typeof(SyncEndpoints)).LogWarning(
                    "Trial upload rejected {Count} record(s): {Errors}",
                    result.Rejected, string.Join(" | ", result.Errors));
            }

            return Results.Ok(result);
        });

        group.MapPost("/items", async (
            UploadRequest? request, SyncRepository repository,
            ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            if (request?.Items is null)
            {
                return Results.BadRequest(new { error = "items is required" });
            }

            if (request.Items.Count == 0)
            {
                return Results.BadRequest(new { error = "no items" });
            }

            if (request.Items.Any(i => i is null))
            {
                return Results.BadRequest(new { error = "items may not contain nulls" });
            }

            if (request.Items.Count > MaxBatchSize)
            {
                return Results.BadRequest(new { error = $"at most {MaxBatchSize} items per request" });
            }

            var result = await repository.UpsertAsync(request.Items, request.Cursor, cancellationToken);

            var logger = loggerFactory.CreateLogger(typeof(SyncEndpoints));
            if (result.RejectedTumorTags.Count > 0)
            {
                // Recurring unknown tags are evidence the taxonomy needs an
                // entry — surface them rather than silently dropping.
                logger.LogWarning("Sync upload had unknown tumor tags: {Tags}",
                    string.Join(", ", result.RejectedTumorTags));
            }

            if (result.Errors.Count > 0)
            {
                logger.LogWarning("Sync upload rejected {Count} item(s): {Errors}",
                    result.Rejected, string.Join(" | ", result.Errors));
            }

            return Results.Ok(result);
        });
    }
}
