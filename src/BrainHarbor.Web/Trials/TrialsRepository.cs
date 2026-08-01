using System.Text.Json;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Services;
using Dapper;

namespace BrainHarbor.Web.Trials;

/// <summary>One trial site, as stored in trials_cache.locations.</summary>
public sealed record TrialSite(
    string? Facility,
    string? City,
    string? State,
    string? Country,
    double? Lat,
    double? Lon)
{
    /// <summary>"Columbus, Ohio" — the part a reader actually scans for.</summary>
    public string Where => string.Join(", ",
        new[] { City, State ?? Country }.Where(p => !string.IsNullOrWhiteSpace(p)));
}

/// <summary>One trial as the browse list and the trial page render it.</summary>
public sealed class TrialRow
{
    public string NctId { get; set; } = "";
    public string Title { get; set; } = "";
    public string[] Conditions { get; set; } = [];
    public string? Phase { get; set; }
    public string? OverallStatus { get; set; }
    public string? Summary { get; set; }
    public DateOnly? LastUpdatePosted { get; set; }
    public string? LocationsJson { get; set; }

    /// <summary>
    /// The plain-language text, joined from the feed item when this trial has a
    /// PUBLISHED one. It lives there and not in trials_cache on purpose: that is
    /// where the automated safety checks, the review queue and a reader's
    /// problem report can all reach it. A trial with no published item shows
    /// the registry's own words instead, clearly marked as such.
    /// </summary>
    public string? PlainTitle { get; set; }
    public string? PlainSummary { get; set; }
    public string? Slug { get; set; }

    /// <summary>True only when we KNOW the trial is open.</summary>
    public bool IsOpen => OverallStatus is not null &&
                          TrialsRepository.OpenStatuses.Contains(OverallStatus);

    /// <summary>
    /// True only when we know the trial has CLOSED. A null status, or the
    /// registry's own "Status unknown" / "Withheld", means we do not know — and
    /// telling a reader "this trial is not taking new patients" when we cannot
    /// tell is a fabricated claim sitting directly above a sentence admitting
    /// we cannot tell. Same rule as FeedRow.TrialHasClosed.
    /// </summary>
    public bool HasClosed => OverallStatus is not null
                             && !TrialsRepository.OpenStatuses.Contains(OverallStatus)
                             && !TrialsRepository.UnknownStatuses.Contains(OverallStatus);

    public string Heading => PlainTitle ?? Title;

    private IReadOnlyList<TrialSite>? _sites;

    public IReadOnlyList<TrialSite> Sites => _sites ??= ParseSites(LocationsJson);

    /// <summary>US states with a site, deduplicated — the cheap "is this near
    /// me at all?" signal on a browse card.</summary>
    public IReadOnlyList<string> StateSummary =>
        [.. Sites.Select(s => s.State).Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)!];

    private static IReadOnlyList<TrialSite> ParseSites(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<TrialSite>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException)
        {
            // Never let a malformed cache row take a page down.
            return [];
        }
    }
}

public sealed record TrialQuery(
    string? TumorType = null,
    string? Phase = null,
    bool IncludeClosed = false,
    int Page = 0)
{
    public const int PageSize = 20;

    /// <summary>Deep paging is clamped, not trusted: `?page=200000000` would
    /// otherwise overflow to a negative OFFSET and 500.</summary>
    public const int MaxPage = 500;

    public int Offset => Math.Clamp(Page, 0, MaxPage) * PageSize;
}

public sealed record TrialPage(IReadOnlyList<TrialRow> Items, int TotalCount, TrialQuery Query)
{
    public bool HasMore => (Math.Max(0, Query.Page) + 1) * TrialQuery.PageSize < TotalCount;
}

/// <summary>
/// WI-403: reads the trial cache for /trials browse.
///
/// Browse defaults to trials someone could still JOIN. A closed trial is not
/// hidden (you can still ask for it, and its page is always reachable), but it
/// is not what a person searching for a trial is looking for.
///
/// Tumor-type filtering matches the registry's own condition strings against
/// the taxonomy's labels and aliases, walking the tree so "glioma" finds
/// glioblastoma. It cannot use tumor_tags, because trials_cache holds trials
/// that never became feed items and so were never classified — matching on the
/// registry's words is the only thing true of every cached trial.
/// </summary>
public sealed class TrialsRepository(IDbConnectionFactory connectionFactory, TaxonomyStore taxonomy)
{
    /// <summary>Statuses that mean a patient could still get in. Same plain
    /// words the fetcher stores.</summary>
    public static readonly string[] OpenStatuses =
        ["Not yet recruiting", "Recruiting", "Enrolling by invitation", "Available"];

    /// <summary>Statuses meaning the registry itself does not know. Neither
    /// open nor closed, and never described to a reader as either.</summary>
    public static readonly string[] UnknownStatuses = ["Status unknown", "Withheld"];

    /// <summary>
    /// Taxonomy slugs that are real entries but are NOT histologies the
    /// registry writes into a condition field. Offering them in the browse menu
    /// hands a reader "no trials match what you picked" for the broadest choice
    /// on the page; "all brain tumors" is simply no filter.
    /// </summary>
    public static readonly string[] NonHistologySlugs = ["all-brain-tumors", "pediatric-brain-tumor"];

    private const string SelectColumns = """
        t.nct_id             AS "NctId",
        t.title              AS "Title",
        t.conditions         AS "Conditions",
        t.phase              AS "Phase",
        t.overall_status     AS "OverallStatus",
        t.summary            AS "Summary",
        t.last_update_posted AS "LastUpdatePosted",
        t.locations::text    AS "LocationsJson",
        a.plain_title        AS "PlainTitle",
        a.plain_summary      AS "PlainSummary",
        a.slug               AS "Slug"
        """;

    /// <summary>
    /// Only a PUBLISHED feed item may lend its plain-language text to a trial
    /// page. A pending or rejected one is exactly the text a person or a safety
    /// check held back, and this join must not become a side door around that.
    /// </summary>
    private const string PublishedItemJoin = """
        LEFT JOIN aggregated_items a
          ON a.source = 'ctgov' AND a.external_id = t.nct_id AND a.status = 'published'
        """;

    public async Task<TrialPage> BrowseAsync(TrialQuery query, CancellationToken cancellationToken)
    {
        // Filters are a fixed set of clauses with parameters — nothing a reader
        // types is ever concatenated into SQL.
        var where = new List<string>();
        string[]? conditionPatterns = null;
        string? phase = null;

        if (!query.IncludeClosed)
        {
            // "Not known to be closed", not "known to be open". A trial with no
            // cached status, or one the registry marks unknown, still belongs in
            // the list. Note SQL `NULL = ANY(...)` evaluates to NULL, so the
            // null case has to be spelled out or those trials silently vanish.
            where.Add("(t.overall_status IS NULL " +
                      "OR t.overall_status = ANY(@openStatuses) " +
                      "OR t.overall_status = ANY(@unknownStatuses))");
        }

        var resolved = query.TumorType is null ? null : taxonomy.Resolve(query.TumorType);
        if (resolved is not null)
        {
            conditionPatterns = ConditionPatternsFor(resolved);
            // ILIKE against the registry's own condition strings: a trial for
            // "Recurrent Glioblastoma" must come back under "glioblastoma".
            where.Add("EXISTS (SELECT 1 FROM unnest(t.conditions) c WHERE c ILIKE ANY(@conditionPatterns))");
        }

        // Matched case-insensitively rather than trusting the caller to have
        // normalized it: the page model validates against the phases the cache
        // holds, but a filter that silently matches nothing because of a
        // capital letter is a worse failure than an unfiltered list.
        phase = string.IsNullOrWhiteSpace(query.Phase) ? null : query.Phase.Trim();
        if (phase is not null)
        {
            where.Add("lower(t.phase) = lower(@phase)");
        }

        var whereClause = where.Count == 0 ? "TRUE" : string.Join(" AND ", where);
        var parameters = new
        {
            openStatuses = OpenStatuses,
            unknownStatuses = UnknownStatuses,
            conditionPatterns,
            phase,
            limit = TrialQuery.PageSize,
            offset = query.Offset,
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<TrialRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM trials_cache t
            {PublishedItemJoin}
            WHERE {whereClause}
            ORDER BY t.last_update_posted DESC NULLS LAST, t.nct_id
            LIMIT @limit OFFSET @offset
            """,
            parameters,
            cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"SELECT count(*) FROM trials_cache t WHERE {whereClause}",
            parameters,
            cancellationToken: cancellationToken));

        return new TrialPage([.. rows], total, query);
    }

    public async Task<TrialRow?> FindAsync(string nctId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeNctId(nctId);
        if (normalized is null)
        {
            return null;
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TrialRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            FROM trials_cache t
            {PublishedItemJoin}
            WHERE t.nct_id = @normalized
            """,
            new { normalized },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// The phases actually present in the cache — used for BOTH the filter menu
    /// and the filter's validation. Reading them from the data rather than from
    /// a second hard-coded list means the menu can never offer a choice that is
    /// silently rejected (the registry emits combinations like "Phase 1/Phase 3"
    /// that no fixed list will contain).
    /// </summary>
    public async Task<IReadOnlyList<string>> AvailablePhasesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var phases = await connection.QueryAsync<string>(new CommandDefinition(
            """
            SELECT DISTINCT phase FROM trials_cache
            WHERE phase IS NOT NULL
            ORDER BY phase
            """,
            cancellationToken: cancellationToken));

        return [.. phases];
    }

    /// <summary>
    /// ILIKE patterns for a tumor type and everything under it: the label and
    /// every alias, each matched as a substring, because the registry writes
    /// "Recurrent Glioblastoma Multiforme" where the taxonomy says
    /// "glioblastoma". Percent and underscore are escaped so a term can never
    /// act as a wildcard.
    /// </summary>
    internal string[] ConditionPatternsFor(string slug) =>
        [.. ConditionTermsFor(slug)
            .Select(t => $"%{t.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%")
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// The words the registry might use for a tumor type and everything under
    /// it — the taxonomy's label plus its aliases, for the type and all of its
    /// descendants.
    ///
    /// Shared with the live "near me" query so a reader picking "Glioma" cannot
    /// get glioblastoma trials in one list and not the other.
    /// </summary>
    public IReadOnlyList<string> ConditionTermsFor(string slug)
    {
        var terms = new List<string>();

        foreach (var type in taxonomy.TumorTypes)
        {
            // Descendants included, so "glioma" finds glioblastoma trials.
            if (!taxonomy.WithAncestors(type.Slug).Contains(slug, StringComparer.Ordinal))
            {
                continue;
            }

            terms.Add(type.Label);
            terms.AddRange(type.Also);
        }

        return [.. terms.Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// A phase from a querystring, matched against the values the cache
    /// actually holds. It is a parameter either way; this is about not silently
    /// ignoring a filter the reader picked from our own menu.
    /// </summary>
    internal static string? NormalizePhase(string? phase, IReadOnlyList<string> available)
    {
        if (string.IsNullOrWhiteSpace(phase))
        {
            return null;
        }

        var trimmed = phase.Trim();
        return available.FirstOrDefault(
            p => string.Equals(p, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>An NCT id from a URL. Anything else is a 404 rather than a
    /// query — the id is the primary key and has exactly one shape.</summary>
    internal static string? NormalizeNctId(string? nctId)
    {
        if (string.IsNullOrWhiteSpace(nctId))
        {
            return null;
        }

        var trimmed = nctId.Trim().ToUpperInvariant();
        return trimmed.Length == 11 && trimmed.StartsWith("NCT", StringComparison.Ordinal) &&
               trimmed[3..].All(char.IsAsciiDigit)
            ? trimmed
            : null;
    }
}
