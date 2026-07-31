using BrainHarbor.Web.Content;
using BrainHarbor.Web.Models;
using BrainHarbor.Web.Services;
using Dapper;

namespace BrainHarbor.Web.Feed;

/// <summary>One published item as the public feed renders it.</summary>
public sealed class FeedRow
{
    public long Id { get; set; }
    public string Slug { get; set; } = "";
    public string Source { get; set; } = "";
    public string SourceKind { get; set; } = "";
    public string Title { get; set; } = "";
    public string? PlainTitle { get; set; }
    public string? PlainSummary { get; set; }
    public string? PlainWhatStudied { get; set; }
    public string? PlainWhatFound { get; set; }
    public string? PlainMeans { get; set; }
    public string? PlainDoesntMean { get; set; }
    public int? ReadinessScore { get; set; }
    public string? ReadinessReason { get; set; }
    public string Url { get; set; } = "";
    public DateOnly? PublishedAt { get; set; }
    public string[] TumorTags { get; set; } = [];
    public string? ResearchStage { get; set; }
    public string Relevance { get; set; } = "";
    public string? ReviewedBy { get; set; }

    /// <summary>True when this item published automatically, with no person in
    /// the loop. The item page says so — the audience deserves that honesty.</summary>
    public bool WasAutoPublished => ReviewedBy == "auto";

    /// <summary>The full plain-language body exists (all four blocks), so the
    /// item page can render the template instead of just the hook.</summary>
    public bool HasFullSummary =>
        !string.IsNullOrWhiteSpace(PlainWhatStudied) && !string.IsNullOrWhiteSpace(PlainWhatFound) &&
        !string.IsNullOrWhiteSpace(PlainMeans) && !string.IsNullOrWhiteSpace(PlainDoesntMean);

    /// <summary>The readiness badge a reader sees, or null if the item is unscored.</summary>
    public ReadinessBadge? Readiness => ReadinessScore is { } score ? ReadinessBadge.For(score) : null;
}

/// <summary>What the reader asked for.</summary>
public sealed record FeedQuery(
    string? TumorType = null,
    string? Kind = null,
    bool IncludeEarlyStage = false,
    int Page = 0)
{
    public const int PageSize = 20;

    public int Offset => Math.Max(0, Page) * PageSize;
}

public sealed record FeedPage(IReadOnlyList<FeedRow> Items, int TotalCount, FeedQuery Query)
{
    public bool HasMore => (Math.Max(0, Query.Page) + 1) * FeedQuery.PageSize < TotalCount;
}

/// <summary>
/// WI-209: the public feed. Two rules are load-bearing and enforced here
/// rather than in a view:
///   * only status='published' is ever visible — the human gate;
///   * early-stage (animal/cell) work is hidden unless the reader asks for
///     it, because a mouse-study headline reads as false hope (PLAN.md §3).
/// </summary>
public sealed class FeedRepository(IDbConnectionFactory connectionFactory, TaxonomyStore taxonomy)
{
    private const string SelectColumns = """
        id AS "Id",
        slug AS "Slug",
        source AS "Source",
        source_kind AS "SourceKind",
        title AS "Title",
        plain_title AS "PlainTitle",
        plain_summary AS "PlainSummary",
        plain_what_studied AS "PlainWhatStudied",
        plain_what_found AS "PlainWhatFound",
        plain_means AS "PlainMeans",
        plain_doesnt_mean AS "PlainDoesntMean",
        readiness_score AS "ReadinessScore",
        readiness_reason AS "ReadinessReason",
        url AS "Url",
        published_at AS "PublishedAt",
        tumor_tags AS "TumorTags",
        research_stage AS "ResearchStage",
        relevance AS "Relevance",
        reviewed_by AS "ReviewedBy"
        """;

    public async Task<FeedPage> GetAsync(FeedQuery query, CancellationToken cancellationToken)
    {
        // Filters are built from a fixed set of clauses with parameters —
        // nothing from the querystring is ever concatenated into SQL.
        var where = new List<string> { "status = 'published'" };

        // 'pending' means "not classified yet" — that is every item until the
        // M3 classifier lands, and those items have still passed the human
        // gate. Excluding them would mean a reviewer approves an item in M2
        // and nothing visibly happens. 'excluded' is never uploaded at all.
        // Early-stage stays behind the toggle in both cases.
        where.Add(query.IncludeEarlyStage
            ? "relevance IN ('patient_relevant', 'pending', 'early_stage')"
            : "relevance IN ('patient_relevant', 'pending')");

        // A tumor filter matches the type OR any of its descendants, so
        // browsing "glioma" includes glioblastoma (data-model.md tree rules).
        string[]? tagFilter = null;
        var resolvedTumor = query.TumorType is null ? null : taxonomy.Resolve(query.TumorType);
        if (resolvedTumor is not null)
        {
            tagFilter = [.. DescendantsOf(resolvedTumor), "all-brain-tumors"];
            where.Add("tumor_tags && @tagFilter");
        }

        var kind = NormalizeKind(query.Kind);
        if (kind is not null)
        {
            where.Add("source_kind = @kind");
        }

        var whereClause = string.Join(" AND ", where);
        var parameters = new
        {
            tagFilter,
            kind,
            limit = FeedQuery.PageSize,
            offset = query.Offset,
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM aggregated_items
            WHERE {whereClause}
            ORDER BY published_at DESC NULLS LAST, id DESC
            LIMIT @limit OFFSET @offset
            """,
            parameters,
            cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"SELECT count(*) FROM aggregated_items WHERE {whereClause}",
            parameters,
            cancellationToken: cancellationToken));

        return new FeedPage([.. rows], total, query);
    }

    /// <summary>
    /// One published item by slug, plus any public correction note. Returns
    /// null for anything not published — a pulled item must look exactly like
    /// one that never existed.
    /// </summary>
    public async Task<(FeedRow Row, string? ReviewNote)?> GetPublishedBySlugAsync(
        string slug, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM aggregated_items
            WHERE slug = @slug AND status = 'published'
            """,
            new { slug },
            cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        var note = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT review_note FROM aggregated_items WHERE slug = @slug",
            new { slug },
            cancellationToken: cancellationToken));

        return (row, note);
    }

    /// <summary>
    /// A reader reports a problem with a published item (WI-306). Flags it so
    /// it surfaces in the admin queue and records the report in the audit trail.
    /// Does NOT unpublish — a person decides what to do; one reader can't take a
    /// page down. Returns false if the slug isn't a published item (so a bad
    /// slug can't spray audit rows). The optional reason is bounded.
    /// </summary>
    public async Task<bool> ReportProblemAsync(
        string slug, string? reason, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var found = await connection.QuerySingleOrDefaultAsync<(long Id, bool AlreadyFlagged)?>(
            new CommandDefinition(
                """
                SELECT id AS "Id", summary_flagged AS "AlreadyFlagged"
                FROM aggregated_items WHERE slug = @slug AND status = 'published'
                """,
                new { slug },
                transaction,
                cancellationToken: cancellationToken));

        if (found is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        // Dedup: an already-flagged live page is already reported (or was
        // approved with the flag freshly cleared). Since this endpoint is public
        // and unauthenticated, re-inserting a 'reported' row per POST would let
        // anyone flood the append-only audit table. One open report per item
        // until a person resolves it (dismiss/pull) is enough.
        if (found.Value.AlreadyFlagged)
        {
            await transaction.RollbackAsync(cancellationToken);
            return true;
        }

        var id = found.Value.Id;
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE aggregated_items SET summary_flagged = true WHERE id = @id",
            new { id },
            transaction,
            cancellationToken: cancellationToken));

        var note = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim()[..Math.Min(reason.Trim().Length, 1000)];

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO review_events (item_id, action, actor, note)
            VALUES (@id, 'reported', 'reader', @note)
            """,
            new { id, note },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// All published items, newest first, for syndication (sitemap.xml,
    /// feed.xml — WI-308). Every published permalink is public regardless of
    /// the feed's early-stage toggle, so this is not filtered by relevance.
    /// </summary>
    public async Task<IReadOnlyList<FeedRow>> GetAllPublishedAsync(
        int limit, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM aggregated_items
            WHERE status = 'published'
            ORDER BY published_at DESC NULLS LAST, id DESC
            LIMIT @limit
            """,
            new { limit },
            cancellationToken: cancellationToken));

        return [.. rows];
    }

    /// <summary>A slug plus every type beneath it in the taxonomy tree.</summary>
    private IEnumerable<string> DescendantsOf(string slug) =>
        taxonomy.TumorTypes
            .Select(t => t.Slug)
            .Where(candidate => taxonomy.WithAncestors(candidate).Contains(slug));

    /// <summary>Only the documented kinds; anything else means "no filter".</summary>
    internal static string? NormalizeKind(string? kind) => kind switch
    {
        "research" or "news" or "preprint" or "trial_update" => kind,
        _ => null,
    };

    /// <summary>Maps a row to the card the shared partial renders.</summary>
    public FeedCard ToCard(FeedRow row) => new(
        ResearchStageMapper.From(row.SourceKind, row.ResearchStage),
        row.PlainTitle ?? row.Title,
        $"/research/{row.Slug}",
        row.PlainSummary ?? "",
        [.. row.TumorTags.Select(taxonomy.LabelFor)],
        row.PublishedAt?.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)
            ?? "No date",
        SourceLabel(row.Source));

    internal static string SourceLabel(string source) => source switch
    {
        "pubmed" => "PubMed",
        "nci_rss" => "National Cancer Institute",
        "sciencedaily" => "ScienceDaily",
        "medrxiv" => "medRxiv (preprint)",
        "biorxiv" => "bioRxiv (preprint)",
        "ctgov" => "ClinicalTrials.gov",
        _ => source,
    };
}
