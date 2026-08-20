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

    // ---- trial facts, joined live from trials_cache (WI-402) ----

    /// <summary>
    /// The trial's CURRENT recruiting status, read from trials_cache at render
    /// time rather than baked into the summary. A trial's page is published
    /// once and then frozen, but trials close — showing the status the trial
    /// had on the day we summarized it would send a patient to a door that no
    /// longer opens.
    /// </summary>
    public string? TrialStatus { get; set; }

    public string? TrialPhase { get; set; }

    /// <summary>When the registry last changed this trial's record.</summary>
    public DateOnly? TrialUpdatedAt { get; set; }

    public bool IsTrial => SourceKind == "trial_update";

    /// <summary>
    /// True when we know the trial is no longer taking new patients. Uses the
    /// same list the SQL filter uses, so what the page says and what the feed
    /// hides can never drift apart.
    ///
    /// A null status means we do not know, which is NOT the same as closed — an
    /// unknown status must not produce a false "this has closed" claim.
    /// </summary>
    public bool TrialHasClosed =>
        IsTrial && TrialStatus is not null &&
        !FeedRepository.OpenTrialStatuses.Contains(TrialStatus);
}

/// <summary>What the reader asked for.</summary>
public sealed record FeedQuery(
    string? TumorType = null,
    string? Kind = null,
    bool IncludeEarlyStage = false,
    int Page = 0,
    string? Sort = null)
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
    /// <summary>
    /// Statuses that mean a patient could still get into the trial, in the same
    /// plain words the fetcher stores. Shared by the SQL filter and
    /// <see cref="FeedRow.TrialHasClosed"/> so the reader and the query can
    /// never disagree about what "open" means.
    /// </summary>
    internal static readonly string[] OpenTrialStatuses =
        ["Not yet recruiting", "Recruiting", "Enrolling by invitation", "Available"];

    /// <summary>
    /// Keeps closed trials out of the "here is what's new" surfaces (WI-402).
    ///
    /// A trial's card carries the hook written on the day we summarized it, and
    /// that text is frozen — a known trial is never re-summarized. So once the
    /// trial closes, its card is a standing invitation to something that no
    /// longer exists. The permalink deliberately stays live and says plainly
    /// that the trial has closed: someone looking that trial up still deserves
    /// an answer. It just stops being served as news.
    ///
    /// A trial with NO cached status is left alone — unknown is not closed.
    /// </summary>
    private const string ExcludeClosedTrials = """
        NOT (a.source_kind = 'trial_update'
             AND t.overall_status IS NOT NULL
             AND NOT (t.overall_status = ANY(@openTrialStatuses)))
        """;

    /// <summary>The join every reader-facing query needs to know a trial's
    /// CURRENT status rather than the one baked into its summary.</summary>
    private const string TrialJoin = """
        LEFT JOIN trials_cache t
          ON a.source = 'ctgov' AND t.nct_id = a.external_id
        """;

    // Qualified with the `a` alias throughout: these queries join trials_cache,
    // which also has `title` and `summary` columns, so unqualified names would
    // be ambiguous. Every query below aliases aggregated_items as `a`.
    private const string SelectColumns = """
        a.id AS "Id",
        a.slug AS "Slug",
        a.source AS "Source",
        a.source_kind AS "SourceKind",
        a.title AS "Title",
        a.plain_title AS "PlainTitle",
        a.plain_summary AS "PlainSummary",
        a.plain_what_studied AS "PlainWhatStudied",
        a.plain_what_found AS "PlainWhatFound",
        a.plain_means AS "PlainMeans",
        a.plain_doesnt_mean AS "PlainDoesntMean",
        a.readiness_score AS "ReadinessScore",
        a.readiness_reason AS "ReadinessReason",
        a.url AS "Url",
        a.published_at AS "PublishedAt",
        a.tumor_tags AS "TumorTags",
        a.research_stage AS "ResearchStage",
        a.relevance AS "Relevance",
        a.reviewed_by AS "ReviewedBy"
        """;

    public async Task<FeedPage> GetAsync(FeedQuery query, CancellationToken cancellationToken)
    {
        // Filters are built from a fixed set of clauses with parameters —
        // nothing from the querystring is ever concatenated into SQL. Every
        // clause is `a.`-qualified because the trial join brings in a second
        // table with overlapping column names.
        var where = new List<string> { "a.status = 'published'", ExcludeClosedTrials };

        // 'pending' means "not classified yet" — that is every item until the
        // M3 classifier lands, and those items have still passed the human
        // gate. Excluding them would mean a reviewer approves an item in M2
        // and nothing visibly happens. 'excluded' is never uploaded at all.
        // Early-stage stays behind the toggle in both cases.
        where.Add(query.IncludeEarlyStage
            ? "a.relevance IN ('patient_relevant', 'pending', 'early_stage')"
            : "a.relevance IN ('patient_relevant', 'pending')");

        // A tumor filter matches the type OR any of its descendants, so
        // browsing "glioma" includes glioblastoma (data-model.md tree rules).
        string[]? tagFilter = null;
        var resolvedTumor = query.TumorType is null ? null : taxonomy.Resolve(query.TumorType);
        if (resolvedTumor is not null)
        {
            tagFilter = [.. DescendantsOf(resolvedTumor), "all-brain-tumors"];
            where.Add("a.tumor_tags && @tagFilter");
        }

        var kind = NormalizeKind(query.Kind);
        if (kind is not null)
        {
            where.Add("a.source_kind = @kind");
        }

        var whereClause = string.Join(" AND ", where);
        var parameters = new
        {
            tagFilter,
            kind,
            openTrialStatuses = OpenTrialStatuses,
            limit = FeedQuery.PageSize,
            offset = query.Offset,
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns},
                   t.overall_status     AS "TrialStatus",
                   t.phase              AS "TrialPhase",
                   t.last_update_posted AS "TrialUpdatedAt"
            FROM aggregated_items a
            {TrialJoin}
            WHERE {whereClause}
            ORDER BY {OrderByFor(NormalizeSort(query.Sort))}
            LIMIT @limit OFFSET @offset
            """,
            parameters,
            cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"""
            SELECT count(*)
            FROM aggregated_items a
            {TrialJoin}
            WHERE {whereClause}
            """,
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

        // The trial join is a LEFT join on purpose: an item that is not a trial
        // (or a trial whose facts we somehow never stored) must still render.
        // Reading the status here rather than from the frozen summary is what
        // stops a published page advertising a closed trial as open (WI-402).
        var row = await connection.QuerySingleOrDefaultAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns},
                   t.overall_status     AS "TrialStatus",
                   t.phase              AS "TrialPhase",
                   t.last_update_posted AS "TrialUpdatedAt"
            FROM aggregated_items a
            LEFT JOIN trials_cache t
              ON a.source = 'ctgov' AND t.nct_id = a.external_id
            WHERE a.slug = @slug AND a.status = 'published'
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
    /// Full-text search over published items (WI-309). Uses Postgres
    /// <c>websearch_to_tsquery</c>, which parses user input forgivingly (quotes,
    /// OR, minus) and never throws on syntax — so a scared reader's messy query
    /// still returns something rather than an error. Ranked by relevance. Only
    /// published items, matching the human gate.
    /// </summary>
    public async Task<IReadOnlyList<FeedRow>> SearchAsync(
        string query, int limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        // Closed trials stay SEARCHABLE — someone looking one up deserves an
        // answer — but the status rides along so the result can say plainly
        // that it has closed, rather than showing a stale "now enrolling" hook.
        var rows = await connection.QueryAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns},
                   t.overall_status     AS "TrialStatus",
                   t.phase              AS "TrialPhase",
                   t.last_update_posted AS "TrialUpdatedAt"
            FROM aggregated_items a
                 LEFT JOIN trials_cache t
                   ON a.source = 'ctgov' AND t.nct_id = a.external_id,
                 websearch_to_tsquery('english', @query) AS q,
                 to_tsvector('english',
                     coalesce(a.plain_title, a.title) || ' ' ||
                     coalesce(a.plain_summary, '') || ' ' ||
                     coalesce(a.plain_what_studied, '') || ' ' ||
                     coalesce(a.plain_what_found, '') || ' ' ||
                     coalesce(a.plain_means, '') || ' ' ||
                     coalesce(a.plain_doesnt_mean, '')) AS doc
            WHERE a.status = 'published' AND doc @@ q
            ORDER BY ts_rank(doc, q) DESC, a.published_at DESC NULLS LAST, a.id DESC
            LIMIT @limit
            """,
            new { query, limit },
            cancellationToken: cancellationToken));

        return [.. rows];
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
        // Same rule as the feed: a closed trial is not news, and its RSS entry
        // would carry the frozen "now enrolling" hook into somebody's reader.
        var rows = await connection.QueryAsync<FeedRow>(new CommandDefinition(
            $"""
            SELECT {SelectColumns},
                   t.overall_status     AS "TrialStatus",
                   t.phase              AS "TrialPhase",
                   t.last_update_posted AS "TrialUpdatedAt"
            FROM aggregated_items a
            {TrialJoin}
            WHERE a.status = 'published' AND {ExcludeClosedTrials}
            ORDER BY a.published_at DESC NULLS LAST, a.id DESC
            LIMIT @limit
            """,
            new { limit, openTrialStatuses = OpenTrialStatuses },
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

    /// <summary>Only the documented sorts; anything else means "newest first"
    /// (WI-410). Date is null rather than a name so old URLs stay canonical.</summary>
    internal static string? NormalizeSort(string? sort) => sort switch
    {
        "readiness" or "type" => sort,
        _ => null,
    };

    /// <summary>
    /// The ORDER BY for each sort a reader can ask for (WI-410). Chosen by
    /// this fixed switch over a normalized value — reader input never reaches
    /// the SQL text.
    ///
    /// "readiness" answers "what is furthest along?". It used to sort by the
    /// 1-to-10 readiness_score; the journey handoff (2026-08-19) took that
    /// number off the cards, and sorting by a number the reader cannot see is
    /// exactly what WI-429 said not to leave standing. It now ranks by the same
    /// four-stage ladder the journey path draws, so the sort order and the
    /// thing on screen are the same fact.
    ///
    /// The sort KEY stays "readiness" on purpose: bookmarked and shared
    /// ?sort=readiness URLs keep working rather than silently reverting to
    /// newest-first, which would look like the page ignoring the reader.
    ///
    /// Items that are not findings (trials, news, preprints) rank last. They
    /// have no place on the evidence ladder at all — the same reason they get a
    /// .stage-note instead of a path — so they must not out-rank a real
    /// finding here. Type is a grouping, not a ranking: groups in the order
    /// readers see in the filter menu, and within a group the order stays
    /// newest-first, decided here explicitly.
    /// </summary>
    private static string OrderByFor(string? sort) => sort switch
    {
        // Mirrors ResearchStageMapper + JourneyPath.For: preprint and
        // trial_update are decided by source_kind BEFORE research_stage is
        // consulted, so those two arms have to come first here too, or a
        // preprint whose research_stage is 'human_trial' would sort as if it
        // were tested in people.
        "readiness" => """
            CASE
                WHEN a.source_kind IN ('preprint', 'trial_update') THEN 0
                WHEN a.research_stage IN ('human_trial', 'observational') THEN 4
                WHEN a.research_stage = 'review_guideline' THEN 3
                WHEN a.research_stage = 'preclinical_animal' THEN 2
                WHEN a.research_stage = 'preclinical_cell' THEN 1
                ELSE 0
            END DESC,
            a.published_at DESC NULLS LAST, a.id DESC
            """,
        "type" => """
            CASE a.source_kind
                WHEN 'research' THEN 1
                WHEN 'news' THEN 2
                WHEN 'trial_update' THEN 3
                WHEN 'preprint' THEN 4
                ELSE 5
            END,
            a.published_at DESC NULLS LAST, a.id DESC
            """,
        _ => "a.published_at DESC NULLS LAST, a.id DESC",
    };

    /// <summary>
    /// Maps a row to the card the shared partial renders.
    ///
    /// A closed trial replaces its hook rather than showing it (WI-402). The
    /// hook was written while the trial was open and is never rewritten, so on
    /// a search result for a trial that has since closed it would read as a
    /// live invitation. Closed trials are already filtered out of the feed and
    /// RSS; search still finds them, which is why the card has to say so.
    /// </summary>
    public FeedCard ToCard(FeedRow row) => new(
        ResearchStageMapper.From(row.SourceKind, row.ResearchStage),
        row.PlainTitle ?? row.Title,
        $"/research/{row.Slug}",
        row.TrialHasClosed
            ? "This trial is not taking new patients."
            : row.PlainSummary ?? "",
        [.. row.TumorTags.Select(taxonomy.LabelFor)],
        row.PublishedAt?.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)
            ?? "No date",
        SourceLabel(row.Source),
        row.ReadinessScore);

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
