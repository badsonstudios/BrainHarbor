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
    public string Url { get; set; } = "";
    public DateOnly? PublishedAt { get; set; }
    public string[] TumorTags { get; set; } = [];
    public string? ResearchStage { get; set; }
    public string Relevance { get; set; } = "";
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
        url AS "Url",
        published_at AS "PublishedAt",
        tumor_tags AS "TumorTags",
        research_stage AS "ResearchStage",
        relevance AS "Relevance"
        """;

    public async Task<FeedPage> GetAsync(FeedQuery query, CancellationToken cancellationToken)
    {
        // Filters are built from a fixed set of clauses with parameters —
        // nothing from the querystring is ever concatenated into SQL.
        var where = new List<string> { "status = 'published'" };

        if (!query.IncludeEarlyStage)
        {
            where.Add("relevance = 'patient_relevant'");
        }
        else
        {
            where.Add("relevance IN ('patient_relevant', 'early_stage')");
        }

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
