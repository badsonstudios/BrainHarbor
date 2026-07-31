using BrainHarbor.Web.Models;
using BrainHarbor.Web.Services;
using Dapper;

namespace BrainHarbor.Web.Admin;

/// <summary>
/// One row in the review queue. Settable properties rather than a positional
/// record: Dapper binds by property name and is forgiving about provider type
/// differences (timestamptz arrives as DateTime, text[] as string[]).
/// </summary>
public sealed class ReviewItem
{
    public long Id { get; set; }
    public string Source { get; set; } = "";
    public string SourceKind { get; set; } = "";
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? RawSummary { get; set; }
    public string Url { get; set; } = "";
    public DateOnly? PublishedAt { get; set; }
    public DateTime FetchedAt { get; set; }
    public string[] TumorTags { get; set; } = [];
    public string? ResearchStage { get; set; }
    public string Relevance { get; set; } = "";
    public string? PlainTitle { get; set; }
    public string? PlainSummary { get; set; }
    public string? PlainWhatStudied { get; set; }
    public string? PlainWhatFound { get; set; }
    public string? PlainMeans { get; set; }
    public string? PlainDoesntMean { get; set; }
    public int? ReadinessScore { get; set; }
    public string? ReadinessReason { get; set; }
    public bool SummaryFlagged { get; set; }
    public string Status { get; set; } = "";

    /// <summary>
    /// The badge a reader would see. Derived from the same mapper the public
    /// feed uses, so the reviewer judges what will actually be published.
    /// </summary>
    public StageBadge Badge => StageBadge.For(ResearchStageMapper.From(SourceKind, ResearchStage));

    /// <summary>The readiness badge a reader would see, or null if unscored.</summary>
    public ReadinessBadge? Readiness => ReadinessScore is { } score ? ReadinessBadge.For(score) : null;

    /// <summary>True once the summarizer has produced the plain-language body.</summary>
    public bool HasSummary => !string.IsNullOrWhiteSpace(PlainSummary);
}

/// <summary>
/// A reviewer's inline edits to a summary, applied before approval. Null fields
/// are left unchanged; this is the corrected copy that gets published, so the
/// reviewer can fix a summary rather than reject a nearly-good one.
/// </summary>
public sealed record SummaryEdits(
    string? PlainTitle,
    string? PlainSummary,
    string? PlainWhatStudied,
    string? PlainWhatFound,
    string? PlainMeans,
    string? PlainDoesntMean,
    int? ReadinessScore,
    string? ReadinessReason);

public enum ReviewAction { Approved, Rejected, Pulled, Reopened }

/// <summary>
/// WI-208: the human review gate. Every transition is recorded with who and
/// when (review_events), because "every published summary is human-reviewed"
/// is the site's central promise and needs to be auditable, not assumed.
/// </summary>
public sealed class ReviewRepository(IDbConnectionFactory connectionFactory)
{
    // Aliased to the record's parameter names: Dapper matches constructor
    // parameters by name, and snake_case columns won't bind to a positional
    // record without this.
    private const string SelectColumns = """
        id AS "Id",
        source AS "Source",
        source_kind AS "SourceKind",
        external_id AS "ExternalId",
        title AS "Title",
        raw_summary AS "RawSummary",
        url AS "Url",
        published_at AS "PublishedAt",
        fetched_at AS "FetchedAt",
        tumor_tags AS "TumorTags",
        research_stage AS "ResearchStage",
        relevance AS "Relevance",
        plain_title AS "PlainTitle",
        plain_summary AS "PlainSummary",
        plain_what_studied AS "PlainWhatStudied",
        plain_what_found AS "PlainWhatFound",
        plain_means AS "PlainMeans",
        plain_doesnt_mean AS "PlainDoesntMean",
        readiness_score AS "ReadinessScore",
        readiness_reason AS "ReadinessReason",
        summary_flagged AS "SummaryFlagged",
        status AS "Status"
        """;

    /// <summary>
    /// Pending items, flagged ones first (a reader reported a problem, or the
    /// numeral check failed — those need eyes soonest), then newest.
    /// </summary>
    public async Task<IReadOnlyList<ReviewItem>> GetPendingAsync(
        int limit, int offset, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ReviewItem>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM aggregated_items
            WHERE status = 'pending'
            ORDER BY summary_flagged DESC, fetched_at DESC, id DESC
            LIMIT @limit OFFSET @offset
            """,
            new { limit, offset },
            cancellationToken: cancellationToken));

        return [.. rows];
    }

    public async Task<int> CountPendingAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT count(*) FROM aggregated_items WHERE status = 'pending'",
            cancellationToken: cancellationToken));
    }

    public async Task<ReviewItem?> GetAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ReviewItem>(new CommandDefinition(
            $"SELECT {SelectColumns} FROM aggregated_items WHERE id = @id",
            new { id },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Saves a reviewer's inline edits to the plain-language summary before
    /// approval (WI-305). Only touches <c>status='pending'</c> rows — a
    /// reviewed/published item's content is frozen, same rule the sync upsert
    /// obeys. COALESCE keeps a field the reviewer left blank. The readiness
    /// score is re-clamped to its stage ceiling so an edit can't push a lab
    /// finding to "near clinic" either. Returns false if nothing pending matched.
    /// </summary>
    public async Task<bool> SaveSummaryEditsAsync(
        long id, SummaryEdits edits, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE aggregated_items
               SET plain_title        = COALESCE(@PlainTitle, plain_title),
                   plain_summary      = COALESCE(@PlainSummary, plain_summary),
                   plain_what_studied = COALESCE(@PlainWhatStudied, plain_what_studied),
                   plain_what_found   = COALESCE(@PlainWhatFound, plain_what_found),
                   plain_means        = COALESCE(@PlainMeans, plain_means),
                   plain_doesnt_mean  = COALESCE(@PlainDoesntMean, plain_doesnt_mean),
                   -- Re-clamp a reviewer-set score to 1..stage-ceiling, so an
                   -- edit can't push a lab/animal finding to "near clinic"
                   -- either. A null edit leaves the score untouched (LEAST would
                   -- ignore the null and wrongly overwrite, hence the CASE).
                   readiness_score    = CASE
                       WHEN @ReadinessScore IS NULL THEN readiness_score
                       ELSE GREATEST(1, LEAST(@ReadinessScore, CASE research_stage
                           WHEN 'news_other'         THEN 10
                           WHEN 'human_trial'        THEN 8
                           WHEN 'review_guideline'   THEN 6
                           WHEN 'observational'      THEN 5
                           WHEN 'preclinical_animal' THEN 2
                           WHEN 'preclinical_cell'   THEN 2
                           ELSE 5 END))
                   END,
                   readiness_reason   = COALESCE(@ReadinessReason, readiness_reason)
             WHERE id = @id AND status = 'pending'
            """,
            new
            {
                id,
                edits.PlainTitle,
                edits.PlainSummary,
                edits.PlainWhatStudied,
                edits.PlainWhatFound,
                edits.PlainMeans,
                edits.PlainDoesntMean,
                edits.ReadinessScore,
                edits.ReadinessReason,
            },
            cancellationToken: cancellationToken));

        return updated > 0;
    }

    /// <summary>
    /// Applies a review decision and records who made it. Returns false if the
    /// item had already moved on — two tabs open on the same item must not
    /// silently double-apply.
    /// </summary>
    public async Task<bool> ApplyAsync(
        long id,
        ReviewAction action,
        string actor,
        string? note,
        CancellationToken cancellationToken)
    {
        var (newStatus, requiredCurrent) = action switch
        {
            ReviewAction.Approved => ("published", new[] { "pending" }),
            ReviewAction.Rejected => ("rejected", ["pending"]),
            ReviewAction.Pulled => ("pulled", ["published"]),
            ReviewAction.Reopened => ("pending", ["rejected", "pulled"]),
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var updated = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE aggregated_items
               SET status = @newStatus,
                   reviewed_at = now(),
                   reviewed_by = @actor,
                   review_note = COALESCE(@note, review_note),
                   slug = CASE
                       WHEN @newStatus = 'published' AND slug IS NULL THEN @slug
                       ELSE slug
                   END
             WHERE id = @id AND status = ANY(@requiredCurrent)
            """,
            new
            {
                id,
                newStatus,
                actor,
                note,
                requiredCurrent,
                slug = await BuildSlugAsync(connection, transaction, id, cancellationToken),
            },
            transaction,
            cancellationToken: cancellationToken));

        if (updated == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO review_events (item_id, action, actor, note)
            VALUES (@id, @action, @actor, @note)
            """,
            new { id, action = action.ToString().ToLowerInvariant(), actor, note },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Slugs are generated on approval (sitemap.md) from the plain-language
    /// title when there is one, so the permalink reads like the page.
    /// </summary>
    private static async Task<string> BuildSlugAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        long id,
        CancellationToken cancellationToken)
    {
        var titles = await connection.QuerySingleAsync<(string Title, string? PlainTitle)>(
            new CommandDefinition(
                "SELECT title, plain_title FROM aggregated_items WHERE id = @id",
                new { id },
                transaction,
                cancellationToken: cancellationToken));

        var baseSlug = Slug.From(titles.PlainTitle ?? titles.Title);

        // Collisions are rare but real (two outlets, same headline). Suffix
        // with the id rather than failing the approval.
        var taken = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS (SELECT 1 FROM aggregated_items WHERE slug = @baseSlug AND id <> @id)",
            new { baseSlug, id },
            transaction,
            cancellationToken: cancellationToken));

        return taken ? $"{baseSlug}-{id}" : baseSlug;
    }

    internal static string Slugify(string title) => Slug.From(title);
}
