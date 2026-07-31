using BrainHarbor.Web.Content;
using BrainHarbor.Web.Services;
using Dapper;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Web.Api;

/// <summary>
/// Data access for the sync API. The upsert is idempotent on
/// (source, external_id) — re-running a pipeline batch must never duplicate
/// items or resurrect a rejected one.
///
/// Publish mode (content-pipeline.md §"Publish mode"): in Auto (the default) a
/// summarized item that passes the pipeline's automated safety checks
/// (summary present and not flagged) publishes itself; anything flagged or
/// not yet summarized is held in the review queue. In Review nothing publishes
/// without a person.
/// </summary>
public sealed class SyncRepository(
    IDbConnectionFactory connectionFactory,
    TaxonomyStore taxonomy,
    IOptions<PublishingOptions> publishing)
{
    private PublishMode Mode => publishing.Value.Mode;

    private sealed record UpsertRow(long Id, bool Inserted);

    // The documented sources (data-model.md aggregated_items.source). A typo'd
    // or invented source would create an orphan feed of items nobody browses
    // and a phantom row on the admin health page.
    private static readonly string[] ValidSources =
        ["pubmed", "nci_rss", "sciencedaily", "medrxiv", "biorxiv", "ctgov", "test_sync"];

    private static readonly string[] ValidSourceKinds = ["research", "news", "preprint", "trial_update"];
    private static readonly string[] ValidRelevance = ["pending", "patient_relevant", "early_stage", "excluded"];
    private static readonly string[] ValidStages =
        ["human_trial", "observational", "review_guideline", "preclinical_animal", "preclinical_cell", "news_other"];

    // The most a finding at a given research stage may score on the 1-10
    // readiness scale. The pipeline already clamps to these (Readiness.Clamp),
    // but the anti-hype cap is a HARD requirement, so the trust boundary
    // re-enforces it as a backstop rather than trusting the client — mirroring
    // how the preprint rule is enforced on both sides. Keep in sync with
    // BrainHarbor.Pipeline.Summarize.Readiness (deliberately duplicated: Web
    // must not depend on the Pipeline assembly). Unknown stage → conservative 5.
    private static readonly IReadOnlyDictionary<string, int> ReadinessCeilings = new Dictionary<string, int>
    {
        ["news_other"] = 10,
        ["human_trial"] = 8,
        ["review_guideline"] = 6,
        ["observational"] = 5,
        ["preclinical_animal"] = 2,
        ["preclinical_cell"] = 2,
    };

    private const int UnknownStageReadinessCeiling = 5;

    /// <summary>
    /// Backstop clamp of a readiness score to its stage ceiling. Only ever
    /// lowers (erring low is the safe direction), so a lab/animal study can
    /// never be stored as near-clinic even if a buggy or hostile client sends a
    /// high score. Null stays null (not yet summarized).
    /// </summary>
    private static int? ClampReadinessToStage(int? score, string? researchStage)
    {
        if (score is not { } value)
        {
            return null;
        }

        var ceiling = researchStage is not null && ReadinessCeilings.TryGetValue(researchStage, out var c)
            ? c
            : UnknownStageReadinessCeiling;
        return Math.Min(Math.Clamp(value, 1, 10), ceiling);
    }

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
        var autoPublished = 0;
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
            var row = await connection.QuerySingleOrDefaultAsync<UpsertRow>(new CommandDefinition(
                """
                INSERT INTO aggregated_items
                    (source, source_kind, external_id, title, raw_summary, url, published_at,
                     tumor_tags, research_stage, relevance, classify_model,
                     plain_title, plain_summary, plain_what_studied, plain_what_found,
                     plain_means, plain_doesnt_mean, readiness_score, readiness_reason,
                     summary_model, prompt_version,
                     summary_generated_at, summary_flagged)
                VALUES
                    (@Source, @SourceKind, @ExternalId, @Title, @RawSummary, @Url, @PublishedAt,
                     @TumorTags, @ResearchStage, @Relevance, @ClassifyModel,
                     @PlainTitle, @PlainSummary, @PlainWhatStudied, @PlainWhatFound,
                     @PlainMeans, @PlainDoesntMean, @ReadinessScore, @ReadinessReason,
                     @SummaryModel, @PromptVersion,
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
                    plain_what_studied = COALESCE(EXCLUDED.plain_what_studied, aggregated_items.plain_what_studied),
                    plain_what_found = COALESCE(EXCLUDED.plain_what_found, aggregated_items.plain_what_found),
                    plain_means = COALESCE(EXCLUDED.plain_means, aggregated_items.plain_means),
                    plain_doesnt_mean = COALESCE(EXCLUDED.plain_doesnt_mean, aggregated_items.plain_doesnt_mean),
                    readiness_score = COALESCE(EXCLUDED.readiness_score, aggregated_items.readiness_score),
                    readiness_reason = COALESCE(EXCLUDED.readiness_reason, aggregated_items.readiness_reason),
                    summary_model = COALESCE(EXCLUDED.summary_model, aggregated_items.summary_model),
                    prompt_version = COALESCE(EXCLUDED.prompt_version, aggregated_items.prompt_version),
                    summary_generated_at =
                        COALESCE(EXCLUDED.summary_generated_at, aggregated_items.summary_generated_at),
                    -- Refresh the flag on a re-summarize. Safe because this
                    -- UPDATE only runs for status='pending' rows, which have no
                    -- reader problem-report yet (that only happens once
                    -- published); a stale flag would otherwise hide the
                    -- "read this closely" warning in the queue.
                    summary_flagged = EXCLUDED.summary_flagged,
                    fetched_at = now()
                WHERE aggregated_items.status = 'pending'
                RETURNING id AS "Id", (xmax = 0) AS "Inserted"
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
                    item.PlainWhatStudied,
                    item.PlainWhatFound,
                    item.PlainMeans,
                    item.PlainDoesntMean,
                    // Backstop: re-clamp to the stage ceiling at the trust
                    // boundary, so the DB can never hold an animal study at 9.
                    ReadinessScore = ClampReadinessToStage(item.ReadinessScore, item.ResearchStage),
                    item.ReadinessReason,
                    item.SummaryModel,
                    item.PromptVersion,
                    SummaryGeneratedAt = item.PlainSummary is null ? (DateTimeOffset?)null : DateTimeOffset.UtcNow,
                    item.SummaryFlagged,
                },
                transaction,
                cancellationToken: cancellationToken));

            if (row is null)
            {
                // The WHERE filtered it out because a human already reviewed
                // this item. Still a successful sync — the pipeline saw the
                // item, we simply keep the reviewed copy.
                frozen++;
                succeededSources.Add(item.Source);
                continue;
            }

            if (row.Inserted)
            {
                inserted++;
            }
            else
            {
                updated++;
            }
            succeededSources.Add(item.Source);

            // Auto-publish (content-pipeline.md §"Publish mode"): the row is
            // still pending here (the freeze WHERE guarantees it). In Auto
            // mode, an item that has a plain-language summary AND was NOT
            // flagged by the pipeline's automated checks goes live now.
            // A flagged item, or one with no summary yet, stays pending for a
            // human — which is why the review queue still exists.
            if (Mode == PublishMode.Auto
                && !string.IsNullOrWhiteSpace(item.PlainSummary)
                && !item.SummaryFlagged)
            {
                await PublishAsync(connection, transaction, row.Id,
                    item.PlainTitle ?? item.Title, cancellationToken);
                autoPublished++;
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
            AutoPublished = autoPublished,
        };
    }

    /// <summary>
    /// Publishes an item without a human — Auto mode, and the item passed the
    /// pipeline's automated checks. Recorded in review_events with actor 'auto'
    /// so the audit trail (and the item page's provenance) can tell a
    /// machine-published item from a human-reviewed one. The item page says so.
    /// </summary>
    private static async Task PublishAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        long id,
        string titleForSlug,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slug.From(titleForSlug);
        var taken = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS (SELECT 1 FROM aggregated_items WHERE slug = @baseSlug AND id <> @id)",
            new { baseSlug, id },
            transaction,
            cancellationToken: cancellationToken));
        var slug = taken ? $"{baseSlug}-{id}" : baseSlug;

        await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE aggregated_items
               SET status = 'published',
                   reviewed_at = now(),
                   reviewed_by = 'auto',
                   slug = COALESCE(slug, @slug)
             WHERE id = @id AND status = 'pending'
            """,
            new { id, slug },
            transaction,
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO review_events (item_id, action, actor, note)
            VALUES (@id, 'approved', 'auto', 'Auto-published: passed automated safety checks')
            """,
            new { id },
            transaction,
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Records that a source failed. The cursor is deliberately left alone —
    /// a failed run must retry the same window.
    /// </summary>
    public async Task<bool> RecordFailureAsync(
        string source, string error, CancellationToken cancellationToken)
    {
        if (!ValidSources.Contains(source))
        {
            return false;
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO source_sync_state (source, last_error)
            VALUES (@source, @error)
            ON CONFLICT (source) DO UPDATE SET last_error = @error
            """,
            new { source, error = Truncate(error, 2000) },
            cancellationToken: cancellationToken));

        return true;
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max];

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
        if (item.ReadinessReason?.Length > 500) return "readinessReason is too long (max 500)";
        if (item.ReadinessScore is { } score && score is < 1 or > 10)
        {
            return $"readinessScore '{score}' is out of range (1-10)";
        }

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
