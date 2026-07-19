using BrainHarbor.Web.Content;
using BrainHarbor.Web.Services;
using Dapper;

namespace BrainHarbor.Web.Api;

/// <summary>
/// Data access for the sync API. The upsert is idempotent on
/// (source, external_id) — re-running a pipeline batch must never duplicate
/// items or resurrect a rejected one.
/// </summary>
public sealed class SyncRepository(IDbConnectionFactory connectionFactory, TaxonomyStore taxonomy)
{
    // The documented sources (data-model.md aggregated_items.source). A typo'd
    // or invented source would create an orphan feed of items nobody browses
    // and a phantom row on the admin health page.
    private static readonly string[] ValidSources =
        ["pubmed", "nci_rss", "sciencedaily", "medrxiv", "biorxiv", "ctgov", "test_sync"];

    private static readonly string[] ValidSourceKinds = ["research", "news", "preprint", "trial_update"];
    private static readonly string[] ValidRelevance = ["pending", "patient_relevant", "early_stage", "excluded"];
    private static readonly string[] ValidStages =
        ["human_trial", "observational", "review_guideline", "preclinical_animal", "preclinical_cell", "news_other"];

    public async Task<IReadOnlyList<SourceState>> GetStateAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<(string Source, DateTimeOffset? LastSuccessAt, string? LastError, string? Cursor)>(
            new CommandDefinition(
                "SELECT source, last_success_at, last_error, cursor FROM source_sync_state ORDER BY source",
                cancellationToken: cancellationToken));

        return [.. rows.Select(r => new SourceState(r.Source, r.LastSuccessAt, r.LastError, r.Cursor))];
    }

    /// <summary>Returns the subset of keys not already stored — what saves Claude tokens.</summary>
    public async Task<IReadOnlyList<ItemKey>> FindNewAsync(
        IReadOnlyList<ItemKey> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return [];
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var sources = keys.Select(k => k.Source).ToArray();
        var externalIds = keys.Select(k => k.ExternalId).ToArray();

        var existing = await connection.QueryAsync<(string Source, string ExternalId)>(
            new CommandDefinition(
                """
                SELECT a.source, a.external_id
                FROM aggregated_items a
                JOIN unnest(@sources::text[], @externalIds::text[]) AS q(source, external_id)
                  ON a.source = q.source AND a.external_id = q.external_id
                """,
                new { sources, externalIds },
                cancellationToken: cancellationToken));

        var known = existing.Select(e => (e.Source, e.ExternalId)).ToHashSet();
        return [.. keys.Where(k => !known.Contains((k.Source, k.ExternalId)))];
    }

    public async Task<UploadResponse> UpsertAsync(
        IReadOnlyList<SyncItem> items, string? cursor, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var rejectedTags = new List<string>();
        var inserted = 0;
        var updated = 0;
        var rejected = 0;
        var frozen = 0;
        var succeededSources = new HashSet<string>(StringComparer.Ordinal);

        // Last write wins within a batch: a duplicate key would otherwise
        // update the row its twin just inserted and be miscounted as an insert.
        var deduped = items
            .Where(i => i is not null)
            .GroupBy(i => (i.Source, i.ExternalId))
            .Select(g => g.Last())
            .ToList();

        // One cursor per request can only belong to one source — stamping a
        // PubMed date window onto medrxiv would silently skip its window.
        if (cursor is not null && deduped.Select(i => i.Source).Distinct().Count() > 1)
        {
            return new UploadResponse(0, 0, deduped.Count, [],
                ["a batch carrying a cursor must come from a single source"]);
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var item in deduped)
        {
            var validationError = Validate(item);
            if (validationError is not null)
            {
                errors.Add($"{item.Source}/{item.ExternalId}: {validationError}");
                rejected++;
                continue;
            }

            var tags = taxonomy.FilterTags(item.TumorTags);
            rejectedTags.AddRange(tags.Rejected);

            // Idempotent on (source, external_id), with two protections for the
            // human review gate:
            //   * status/reviewed_at/review_note/slug/summary_flagged are never
            //     touched — a rerun must not resurrect a rejected item,
            //     un-publish a live one, or clear a reader's problem report;
            //   * the WHERE freezes CONTENT once a human has reviewed it. Without
            //     it a classify-only rerun (no summary in the payload) would null
            //     the plain_summary of a *published* item and leave a live page
            //     contentless with nobody in the loop.
            // COALESCE keeps a previously-generated summary when a later run
            // legitimately omits one.
            var wasInserted = await connection.ExecuteScalarAsync<bool?>(new CommandDefinition(
                """
                INSERT INTO aggregated_items
                    (source, source_kind, external_id, title, raw_summary, url, published_at,
                     tumor_tags, research_stage, relevance, classify_model,
                     plain_title, plain_summary, summary_model, prompt_version,
                     summary_generated_at, summary_flagged)
                VALUES
                    (@Source, @SourceKind, @ExternalId, @Title, @RawSummary, @Url, @PublishedAt,
                     @TumorTags, @ResearchStage, @Relevance, @ClassifyModel,
                     @PlainTitle, @PlainSummary, @SummaryModel, @PromptVersion,
                     @SummaryGeneratedAt, @SummaryFlagged)
                ON CONFLICT (source, external_id) DO UPDATE SET
                    title = EXCLUDED.title,
                    raw_summary = EXCLUDED.raw_summary,
                    url = EXCLUDED.url,
                    published_at = EXCLUDED.published_at,
                    tumor_tags = EXCLUDED.tumor_tags,
                    research_stage = EXCLUDED.research_stage,
                    relevance = EXCLUDED.relevance,
                    classify_model = EXCLUDED.classify_model,
                    plain_title = COALESCE(EXCLUDED.plain_title, aggregated_items.plain_title),
                    plain_summary = COALESCE(EXCLUDED.plain_summary, aggregated_items.plain_summary),
                    summary_model = COALESCE(EXCLUDED.summary_model, aggregated_items.summary_model),
                    prompt_version = COALESCE(EXCLUDED.prompt_version, aggregated_items.prompt_version),
                    summary_generated_at =
                        COALESCE(EXCLUDED.summary_generated_at, aggregated_items.summary_generated_at),
                    fetched_at = now()
                WHERE aggregated_items.status = 'pending'
                RETURNING (xmax = 0)
                """,
                new
                {
                    item.Source,
                    item.SourceKind,
                    item.ExternalId,
                    item.Title,
                    item.RawSummary,
                    item.Url,
                    item.PublishedAt,
                    TumorTags = tags.Known,
                    item.ResearchStage,
                    Relevance = item.Relevance ?? "pending",
                    item.ClassifyModel,
                    item.PlainTitle,
                    item.PlainSummary,
                    item.SummaryModel,
                    item.PromptVersion,
                    SummaryGeneratedAt = item.PlainSummary is null ? (DateTimeOffset?)null : DateTimeOffset.UtcNow,
                    item.SummaryFlagged,
                },
                transaction,
                cancellationToken: cancellationToken));

            switch (wasInserted)
            {
                case true:
                    inserted++;
                    succeededSources.Add(item.Source);
                    break;
                case false:
                    updated++;
                    succeededSources.Add(item.Source);
                    break;
                default:
                    // No row returned: the WHERE filtered it out because a human
                    // already reviewed this item. Still a successful sync — the
                    // pipeline saw the item, we simply keep the reviewed copy.
                    frozen++;
                    succeededSources.Add(item.Source);
                    break;
            }
        }

        // Only sources that actually stored something advance their cursor.
        // Stamping success on an all-rejected batch would skip that window
        // forever — silent, permanent data loss.
        foreach (var source in succeededSources)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO source_sync_state (source, last_success_at, last_error, cursor)
                VALUES (@source, now(), NULL, @cursor)
                ON CONFLICT (source) DO UPDATE SET
                    last_success_at = now(),
                    last_error = NULL,
                    cursor = COALESCE(@cursor, source_sync_state.cursor)
                """,
                new { source, cursor },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);

        return new UploadResponse(inserted, updated, rejected, [.. rejectedTags.Distinct()], errors)
        {
            Frozen = frozen,
        };
    }

    /// <summary>
    /// Advances a source's cursor with no items to store. Returns false for an
    /// unknown source rather than creating a phantom row on the health page.
    /// </summary>
    public async Task<bool> AdvanceCursorAsync(
        string source, string cursor, CancellationToken cancellationToken)
    {
        if (!ValidSources.Contains(source))
        {
            return false;
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO source_sync_state (source, last_success_at, last_error, cursor)
            VALUES (@source, now(), NULL, @cursor)
            ON CONFLICT (source) DO UPDATE SET
                last_success_at = now(),
                last_error = NULL,
                cursor = @cursor
            """,
            new { source, cursor },
            cancellationToken: cancellationToken));

        return true;
    }

    /// <summary>
    /// Belt-and-braces validation. The database CHECK constraints are the real
    /// guarantee; this turns a would-be 500 into a per-item error message the
    /// pipeline can log and act on.
    /// </summary>
    private static string? Validate(SyncItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Source)) return "source is required";
        if (!ValidSources.Contains(item.Source))
        {
            return $"source '{item.Source}' is not one of {string.Join(", ", ValidSources)}";
        }

        if (string.IsNullOrWhiteSpace(item.ExternalId)) return "externalId is required";
        if (string.IsNullOrWhiteSpace(item.Title)) return "title is required";
        if (!Uri.TryCreate(item.Url, UriKind.Absolute, out var url) ||
            (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
        {
            return "url must be an absolute http(s) URL";
        }

        // Bounds: a buggy pipeline shouldn't be able to store megabytes.
        if (item.ExternalId.Length > 200) return "externalId is too long (max 200)";
        if (item.Title.Length > 1000) return "title is too long (max 1000)";
        if (item.Url.Length > 2000) return "url is too long (max 2000)";
        if (item.RawSummary?.Length > 20000) return "rawSummary is too long (max 20000)";
        if (item.PlainTitle?.Length > 1000) return "plainTitle is too long (max 1000)";
        if (item.PlainSummary?.Length > 20000) return "plainSummary is too long (max 20000)";

        // A far-future date would pin an item to the top of the feed forever.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (item.PublishedAt is { } published &&
            (published > today.AddDays(7) || published < new DateOnly(1900, 1, 1)))
        {
            return $"publishedAt '{published:yyyy-MM-dd}' is out of range";
        }

        if (!ValidSourceKinds.Contains(item.SourceKind))
        {
            return $"sourceKind '{item.SourceKind}' is not one of {string.Join(", ", ValidSourceKinds)}";
        }

        var relevance = item.Relevance ?? "pending";
        if (!ValidRelevance.Contains(relevance))
        {
            return $"relevance '{relevance}' is not one of {string.Join(", ", ValidRelevance)}";
        }

        if (item.ResearchStage is not null && !ValidStages.Contains(item.ResearchStage))
        {
            return $"researchStage '{item.ResearchStage}' is not one of {string.Join(", ", ValidStages)}";
        }

        // content-pipeline.md §9 — mirrored here so the pipeline gets a clear
        // message instead of a constraint violation.
        if (item.SourceKind == "preprint" && relevance == "patient_relevant")
        {
            return "a preprint can never be patient_relevant (content-pipeline.md §9)";
        }

        // 'excluded' items are filtered locally and must never be uploaded.
        if (relevance == "excluded")
        {
            return "excluded items must not be uploaded (filter them locally)";
        }

        return null;
    }
}
