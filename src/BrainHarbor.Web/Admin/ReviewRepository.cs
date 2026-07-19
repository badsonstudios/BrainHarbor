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
    public bool SummaryFlagged { get; set; }
    public string Status { get; set; } = "";

    /// <summary>
    /// The badge a reader would see. Derived from the same mapper the public
    /// feed uses, so the reviewer judges what will actually be published.
    /// </summary>
    public StageBadge Badge => StageBadge.For(ResearchStageMapper.From(SourceKind, ResearchStage));
}

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

        var baseSlug = Slugify(titles.PlainTitle ?? titles.Title);

        // Collisions are rare but real (two outlets, same headline). Suffix
        // with the id rather than failing the approval.
        var taken = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS (SELECT 1 FROM aggregated_items WHERE slug = @baseSlug AND id <> @id)",
            new { baseSlug, id },
            transaction,
            cancellationToken: cancellationToken));

        return taken ? $"{baseSlug}-{id}" : baseSlug;
    }

    internal static string Slugify(string title)
    {
        var chars = title.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        slug = slug.Trim('-');
        if (slug.Length > 80)
        {
            slug = slug[..80].TrimEnd('-');
        }

        return slug.Length == 0 ? "item" : slug;
    }
}
