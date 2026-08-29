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

/// <summary>A country with trial sites, and how many trials it holds.</summary>
public sealed record TrialCountry(string Name, int Trials);

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

    /// <summary>
    /// Regions with a site, deduplicated — the cheap "is this near me at all?"
    /// signal on a browse card.
    ///
    /// Written for US states, and it still is that for US trials. But the
    /// country filter (WI-455) surfaces trials the browse list rarely showed
    /// before, and the registry puts whatever a country calls its subdivision
    /// in this field — so a card can now legitimately read "Gelderland, Rome".
    /// That is the registry's own words, which is the rule this page follows
    /// everywhere else; it is not a bug to normalize away.
    /// </summary>
    public IReadOnlyList<string> StateSummary =>
        [.. Sites.Select(s => s.State).Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)!];

    /// <summary>
    /// Countries with a site, deduplicated (WI-455). The card uses the COUNT of
    /// these rather than naming one: a trial filtered to Germany may also run in
    /// six other countries, and showing only the filtered one would tell a
    /// reader something untrue about the study they are looking at.
    /// </summary>
    /// <summary>
    /// A sentence or two for the browse card (WI-459). Prefers OUR plain-language
    /// summary; falls back to the registry's own brief description, which the
    /// card labels as the trial team's words rather than passing off as ours.
    ///
    /// The fallback is not an edge case: every one of the 518 cached trials has
    /// registry text and none currently has a plain-language summary, so
    /// without it the description Dan asked for would be blank on every card.
    ///
    /// **Cut at a sentence end, never mid-sentence.** This is clinical prose,
    /// and truncating "...did not improve survival" halfway is how a summary
    /// comes to say the opposite of the study. If no sentence break falls
    /// inside the budget, the whole first sentence is used however long it is —
    /// better a long card than a misleading one.
    /// </summary>
    public string? CardBlurb
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(PlainSummary))
            {
                return PlainSummary;
            }

            var text = Summary?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            const int budget = 220;
            if (text.Length <= budget)
            {
                return text;
            }

            var cut = text.LastIndexOfAny(['.', '!', '?'], Math.Min(budget, text.Length - 1));
            if (cut > 0)
            {
                return text[..(cut + 1)];
            }

            var firstEnd = text.IndexOfAny(['.', '!', '?']);
            return firstEnd > 0 ? text[..(firstEnd + 1)] : text;
        }
    }

    /// <summary>True when the blurb is the registry's words, not ours.</summary>
    public bool BlurbIsRegistryText => string.IsNullOrWhiteSpace(PlainSummary);

    public IReadOnlyList<string> CountrySummary =>
        [.. Sites.Select(s => s.Country).Where(c => !string.IsNullOrWhiteSpace(c))
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
    int Page = 0,
    /// <summary>
    /// Countries to include, ORed together (WI-457). Empty means every country.
    /// A reader comparing options often wants two or three at once — "China and
    /// Japan and a few others" — which a single choice cannot express.
    /// </summary>
    IReadOnlyList<string>? Countries = null)
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

    /// <summary>
    /// The WHERE clauses and their parameters for a query, built once and used
    /// by both the trial list and the country counts (WI-462).
    ///
    /// Shared deliberately. The counts beside each country were originally
    /// their own query that ignored tumor type and phase, so picking
    /// "Glioblastoma" left them showing all-tumor numbers — a count that
    /// disagrees with the list it produces is worse than no count. Building
    /// both from one place is what stops that drifting apart again.
    ///
    /// <paramref name="includeCountryFilter"/> is false for the counts: the
    /// numbers answer "how many trials are in this country, given your OTHER
    /// filters", so folding the country selection in would be circular —
    /// picking China would drop every other country to zero.
    /// </summary>
    private (string Where, object Parameters) BuildFilter(
        TrialQuery query, bool includeCountryFilter)
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

        // Country matches ANY of the trial's sites, against ANY of the chosen
        // countries (WI-455, multi-select in WI-457). Two "anys" and both are
        // deliberate:
        //   * a trial running in eight countries belongs under all eight —
        //     treating the first location as "the" country would hide most
        //     international trials and be wrong about the one it showed;
        //   * picking China and Japan means "either", not "both". A trial has
        //     to run somewhere the reader can reach, not everywhere.
        var countries = query.Countries?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (includeCountryFilter && countries is { Length: > 0 })
        {
            where.Add("""
                EXISTS (
                    SELECT 1 FROM jsonb_array_elements(t.locations) loc
                    WHERE lower(loc->>'country') = ANY(@countries)
                )
                """);
        }
        else
        {
            countries = null;
        }

        return (
            where.Count == 0 ? "TRUE" : string.Join(" AND ", where),
            new
            {
                openStatuses = OpenStatuses,
                unknownStatuses = UnknownStatuses,
                conditionPatterns,
                phase,
                countries,
                limit = TrialQuery.PageSize,
                offset = query.Offset,
            });
    }

    public async Task<TrialPage> BrowseAsync(TrialQuery query, CancellationToken cancellationToken)
    {
        var (whereClause, parameters) = BuildFilter(query, includeCountryFilter: true);

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
    /// Countries with at least one trial site, and how many trials each has
    /// (WI-455). Read from the data for the same reason the phases are: a menu
    /// built from a hard-coded world list would offer choices that match
    /// nothing, and a reader only discovers a dead option by picking it and
    /// getting an empty page.
    ///
    /// Counted with DISTINCT nct_id, not by row: a trial with forty US sites is
    /// one trial in the United States, not forty.
    ///
    /// Honours the same "not known to be closed" default as the browse list, so
    /// the count beside a country matches what picking it will show. A count
    /// that says 12 and then lists 3 is worse than no count.
    /// </summary>
    public async Task<IReadOnlyList<TrialCountry>> AvailableCountriesAsync(
        TrialQuery query, CancellationToken cancellationToken)
    {
        // The SAME clauses the trial list uses, minus the country filter
        // (WI-462). This query used to consider only includeClosed, so picking
        // a tumor type left every count showing all-tumor numbers — the menu
        // promised 27 and the list delivered 4.
        var (whereClause, parameters) = BuildFilter(query, includeCountryFilter: false);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var countries = await connection.QueryAsync<TrialCountry>(new CommandDefinition(
            $"""
            -- Cast: count() is bigint, and Dapper matches a record's
            -- constructor by exact type — an int property against a bigint
            -- column fails materialization rather than converting.
            SELECT loc->>'country' AS "Name", count(DISTINCT t.nct_id)::int AS "Trials"
            FROM trials_cache t, jsonb_array_elements(t.locations) loc
            WHERE loc->>'country' IS NOT NULL
              AND loc->>'country' <> ''
              AND ({whereClause})
            GROUP BY loc->>'country'
            ORDER BY count(DISTINCT t.nct_id) DESC, loc->>'country'
            """,
            parameters,
            cancellationToken: cancellationToken));

        return [.. countries];
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
