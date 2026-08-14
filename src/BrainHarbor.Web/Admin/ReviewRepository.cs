using BrainHarbor.Safety;
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
    public string? Slug { get; set; }
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
    /// Trial phase and status, for trial items only (WI-418). Not shown to the
    /// reviewer — they are here so the re-check below sees the same source text
    /// the pipeline saw. The summarize-trial prompt scores readiness BY phase,
    /// so "Phase 2" legitimately appears in the summary; without these two
    /// fields the numeral check would report it as an invented figure and send
    /// the reviewer hunting for a problem that isn't there.
    /// </summary>
    public string? TrialPhase { get; set; }
    public string? TrialStatus { get; set; }

    /// <summary>
    /// The badge a reader would see. Derived from the same mapper the public
    /// feed uses, so the reviewer judges what will actually be published.
    /// </summary>
    public StageBadge Badge => StageBadge.For(ResearchStageMapper.From(SourceKind, ResearchStage));

    /// <summary>The readiness badge a reader would see, or null if unscored.</summary>
    public ReadinessBadge? Readiness => ReadinessScore is { } score ? ReadinessBadge.For(score) : null;

    /// <summary>True once the summarizer has produced the plain-language body.</summary>
    public bool HasSummary => !string.IsNullOrWhiteSpace(PlainSummary);

    private IReadOnlyList<Guardrails.Flag>? _flagReasons;

    /// <summary>
    /// WHY this item is flagged, re-checked from the stored summary (WI-418).
    ///
    /// The database records a `summary_flagged` boolean and no reason — the
    /// pipeline knew which check tripped and never sent it — so "flagged" meant
    /// "read this one closely" and nothing more, across a queue of 137 items.
    /// The checks are pure text analysis and the summary is stored, so the
    /// answer is recoverable: run them again here, through the SAME library the
    /// pipeline uses (BrainHarbor.Safety), never a copy of the rules.
    ///
    /// Two honest limits, both surfaced in the queue rather than hidden:
    /// this reflects TODAY's rules, not the rules in force when the item was
    /// flagged (the reading-level ceiling moved from 8.5 to 7.0 on 2026-08-13),
    /// and an item a READER reported carries no automated reason at all.
    /// </summary>
    public IReadOnlyList<Guardrails.Flag> FlagReasons =>
        _flagReasons ??= HasSummary
            ? Guardrails.Check(
                new SummaryText(
                    PlainTitle, PlainSummary, PlainWhatStudied, PlainWhatFound,
                    PlainMeans, PlainDoesntMean, ReadinessReason).AllProse,
                SummaryText.SourceFor(Title, RawSummary, TrialPhase, TrialStatus)).Reasons
            : [];
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
        i.id AS "Id",
        i.slug AS "Slug",
        i.source AS "Source",
        i.source_kind AS "SourceKind",
        i.external_id AS "ExternalId",
        i.title AS "Title",
        i.raw_summary AS "RawSummary",
        i.url AS "Url",
        i.published_at AS "PublishedAt",
        i.fetched_at AS "FetchedAt",
        i.tumor_tags AS "TumorTags",
        i.research_stage AS "ResearchStage",
        i.relevance AS "Relevance",
        i.plain_title AS "PlainTitle",
        i.plain_summary AS "PlainSummary",
        i.plain_what_studied AS "PlainWhatStudied",
        i.plain_what_found AS "PlainWhatFound",
        i.plain_means AS "PlainMeans",
        i.plain_doesnt_mean AS "PlainDoesntMean",
        i.readiness_score AS "ReadinessScore",
        i.readiness_reason AS "ReadinessReason",
        i.summary_flagged AS "SummaryFlagged",
        i.status AS "Status",
        t.phase AS "TrialPhase",
        t.overall_status AS "TrialStatus"
        """;

    /// <summary>
    /// The facts a trial summary was written from, so the queue's re-check
    /// (WI-418) sees the same source text the pipeline did. LEFT JOIN: only
    /// trial items match, everything else gets nulls and is unaffected.
    /// </summary>
    private const string FromClause = """
        FROM aggregated_items i
        LEFT JOIN trials_cache t ON t.nct_id = i.external_id AND i.source_kind = 'trial_update'
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
            {FromClause}
            WHERE i.status = 'pending'
            ORDER BY i.summary_flagged DESC, i.fetched_at DESC, i.id DESC
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

    /// <summary>
    /// Published items a reader flagged as having a problem (WI-306). These are
    /// live pages, so they need eyes: a person decides whether to pull, correct,
    /// or dismiss the report. Newest fetch first as a rough recency proxy.
    /// </summary>
    public async Task<IReadOnlyList<ReviewItem>> GetReportedAsync(
        int limit, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ReviewItem>(new CommandDefinition(
            $"""
            SELECT {SelectColumns}
            {FromClause}
            WHERE i.status = 'published' AND i.summary_flagged = true
            ORDER BY i.fetched_at DESC, i.id DESC
            LIMIT @limit
            """,
            new { limit },
            cancellationToken: cancellationToken));

        return [.. rows];
    }

    public async Task<int> CountReportedAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT count(*) FROM aggregated_items WHERE status = 'published' AND summary_flagged = true",
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Clears a reader's problem flag on a published item without changing what
    /// readers see — the "I looked, it's fine" outcome. The report stays in the
    /// audit trail. Returns false if the item isn't a flagged published one.
    /// </summary>
    public async Task<bool> DismissReportAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE aggregated_items SET summary_flagged = false
            WHERE id = @id AND status = 'published' AND summary_flagged = true
            """,
            new { id },
            cancellationToken: cancellationToken));

        return updated > 0;
    }

    public async Task<ReviewItem?> GetAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ReviewItem>(new CommandDefinition(
            $"SELECT {SelectColumns} {FromClause} WHERE i.id = @id",
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
                   -- Approving resolves the pipeline's automated flag: a person
                   -- looked and published. Otherwise a numeral/banned-phrase
                   -- flag would linger on the published row and masquerade as a
                   -- reader report in the "Reported by readers" queue (which is
                   -- published AND summary_flagged). Reader reports re-set it.
                   summary_flagged = CASE
                       WHEN @newStatus = 'published' THEN false
                       ELSE summary_flagged
                   END,
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
