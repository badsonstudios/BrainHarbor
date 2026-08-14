using BrainHarbor.Web.Admin;
using BrainHarbor.Web.Models;
using BrainHarbor.Web.Services;
using Dapper;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-208: the human review gate. "Nothing publishes without a person" is the
/// site's central safety promise, so these tests are about the transitions
/// and their audit trail, not the markup.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class ReviewQueueTests : IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;
    private ReviewRepository _reviews = null!;

    public ReviewQueueTests(DatabaseFixture database) => _database = database;

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_database.ConnectionString);
        await _connection.OpenAsync();
        await CleanupAsync();

        _reviews = new ReviewRepository(new TestConnectionFactory(_database.ConnectionString));
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        await _connection.DisposeAsync();
    }

    private Task CleanupAsync() => _connection.ExecuteAsync(
        "DELETE FROM aggregated_items WHERE source = @TestSource", new { TestSource });

    private sealed class TestConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async Task<System.Data.Common.DbConnection> OpenConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }

    private async Task<long> InsertPendingAsync(
        string externalId = "rq-1",
        string title = "A study of something",
        string? plainTitle = null,
        bool flagged = false,
        string sourceKind = "research",
        string? researchStage = "human_trial") =>
        await _connection.ExecuteScalarAsync<long>(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, plain_title,
                 summary_flagged, research_stage, relevance, status)
            VALUES
                (@TestSource, @sourceKind, @externalId, @title, 'https://example.org',
                 @plainTitle, @flagged, @researchStage, 'patient_relevant', 'pending')
            RETURNING id
            """,
            new { TestSource, sourceKind, externalId, title, plainTitle, flagged, researchStage });

    // ---------- WI-418: the queue explains its own flags ----------

    /// <summary>
    /// End to end through the real query: a flagged item comes back from the
    /// repository already able to say WHICH check tripped. Unit tests cover the
    /// checking; this covers the plumbing that feeds it — including the trials
    /// join added for it, which a typo would break only at runtime.
    /// </summary>
    [Fact]
    public async Task AFlaggedItemComesBackKnowingWhyItWasFlagged()
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, raw_summary, status,
                 relevance, research_stage, summary_flagged, plain_title, plain_summary,
                 plain_what_studied, plain_what_found, plain_means, plain_doesnt_mean,
                 readiness_reason)
            VALUES
                (@TestSource, 'research', 'rq-flagged', 'A trial of a pill',
                 'https://example.org',
                 'In a trial of 331 people, survival was 27 months.',
                 'pending', 'patient_relevant', 'human_trial', true,
                 'A pill slowed growth', 'A daily pill helped people.',
                 'Researchers gave a pill to 331 people.',
                 'The pill worked for 88% of people.',
                 'It may add time.', 'It is not a cure.',
                 'Being tested in people.')
            """,
            new { TestSource });

        var pending = await _reviews.GetPendingAsync(50, 0, CancellationToken.None);
        var item = Assert.Single(pending, i => i.ExternalId == "rq-flagged");

        var reason = Assert.Single(item.FlagReasons);
        Assert.Equal(BrainHarbor.Safety.Guardrails.FlagKind.InventedNumbers, reason.Kind);
        Assert.Contains("88", reason.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The trials join specifically: a trial's phase lives in trials_cache, not
    /// in the abstract, and the summarize-trial prompt scores readiness BY
    /// phase — so without the join every trial summary reports its own phase as
    /// an invented number and the reviewer chases a ghost.
    /// </summary>
    [Fact]
    public async Task ATrialsPhaseComesBackFromTheCacheSoItIsNotCalledInvented()
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO trials_cache (nct_id, title, phase, overall_status)
            VALUES ('NCT09999001', 'A trial', 'Phase 2', 'Recruiting')
            ON CONFLICT (nct_id) DO UPDATE SET phase = excluded.phase
            """);

        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, raw_summary, status,
                 relevance, research_stage, summary_flagged, plain_title, plain_summary,
                 plain_what_studied, plain_what_found, plain_means, plain_doesnt_mean,
                 readiness_reason)
            VALUES
                (@TestSource, 'trial_update', 'NCT09999001', 'A trial of a drug',
                 'https://clinicaltrials.gov/study/NCT09999001',
                 'This study is testing a drug in people with glioma.',
                 'pending', 'patient_relevant', 'human_trial', true,
                 'A trial is testing a drug', 'Doctors are testing a drug.',
                 'This is a Phase 2 trial.', 'It has not reported results yet.',
                 'People may be able to join.', 'It does not mean the drug works.',
                 'Still being tested in people.')
            """,
            new { TestSource });

        try
        {
            var pending = await _reviews.GetPendingAsync(50, 0, CancellationToken.None);
            var item = Assert.Single(pending, i => i.ExternalId == "NCT09999001");

            Assert.Equal("Phase 2", item.TrialPhase);
            Assert.DoesNotContain(
                BrainHarbor.Safety.Guardrails.FlagKind.InventedNumbers,
                item.FlagReasons.Select(r => r.Kind));
        }
        finally
        {
            await _connection.ExecuteAsync(
                "DELETE FROM trials_cache WHERE nct_id = 'NCT09999001'");
        }
    }

    // ---------- the gate ----------

    [Fact]
    public async Task ApprovingPublishesTheItemAndStampsTheReviewer()
    {
        var id = await InsertPendingAsync();

        var applied = await _reviews.ApplyAsync(
            id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        Assert.True(applied);
        var row = await _connection.QuerySingleAsync<(string Status, string ReviewedBy, DateTimeOffset? ReviewedAt)>(
            "SELECT status, reviewed_by, reviewed_at FROM aggregated_items WHERE id = @id", new { id });

        Assert.Equal("published", row.Status);
        Assert.Equal("dan@example.org", row.ReviewedBy);
        Assert.NotNull(row.ReviewedAt);
    }

    [Fact]
    public async Task RejectingDoesNotPublish()
    {
        var id = await InsertPendingAsync();

        await _reviews.ApplyAsync(id, ReviewAction.Rejected, "dan@example.org", null, CancellationToken.None);

        var status = await _connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("rejected", status);
    }

    [Fact]
    public async Task EveryDecisionIsRecordedWithWhoAndWhen()
    {
        var id = await InsertPendingAsync();

        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", "fixed a number", CancellationToken.None);

        var events = (await _connection.QueryAsync<(string Action, string Actor, string? Note)>(
            "SELECT action, actor, note FROM review_events WHERE item_id = @id", new { id })).ToList();

        var recorded = Assert.Single(events);
        Assert.Equal("approved", recorded.Action);
        Assert.Equal("dan@example.org", recorded.Actor);
        Assert.Equal("fixed a number", recorded.Note);
    }

    [Fact]
    public async Task DecidingTwiceIsANoOpRatherThanADoubleApply()
    {
        // Two tabs open on the same item must not both "succeed".
        var id = await InsertPendingAsync();

        var first = await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);
        var second = await _reviews.ApplyAsync(id, ReviewAction.Rejected, "dan@example.org", null, CancellationToken.None);

        Assert.True(first);
        Assert.False(second);

        var status = await _connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("published", status);
    }

    [Fact]
    public async Task APublishedItemCanBePulledButNotReApproved()
    {
        var id = await InsertPendingAsync();
        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        Assert.False(await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None));
        Assert.True(await _reviews.ApplyAsync(id, ReviewAction.Pulled, "dan@example.org", "wrong number", CancellationToken.None));

        var status = await _connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("pulled", status);
    }

    [Fact]
    public async Task ARejectedItemCanBeReopened()
    {
        var id = await InsertPendingAsync();
        await _reviews.ApplyAsync(id, ReviewAction.Rejected, "dan@example.org", null, CancellationToken.None);

        Assert.True(await _reviews.ApplyAsync(id, ReviewAction.Reopened, "dan@example.org", null, CancellationToken.None));

        var status = await _connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("pending", status);
    }

    // ---------- the queue ----------

    [Fact]
    public async Task OnlyPendingItemsAppearInTheQueue()
    {
        var pending = await InsertPendingAsync("rq-pending");
        var approved = await InsertPendingAsync("rq-approved");
        await _reviews.ApplyAsync(approved, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        var queue = await _reviews.GetPendingAsync(50, 0, CancellationToken.None);

        Assert.Contains(queue, i => i.Id == pending);
        Assert.DoesNotContain(queue, i => i.Id == approved);
    }

    [Fact]
    public async Task FlaggedItemsSortFirst()
    {
        // A reader reported a problem, or the numeral check failed — those
        // need eyes before anything else in the queue.
        await InsertPendingAsync("rq-normal");
        var flagged = await InsertPendingAsync("rq-flagged", flagged: true);

        var queue = await _reviews.GetPendingAsync(50, 0, CancellationToken.None);

        Assert.Equal(flagged, queue[0].Id);
    }

    [Fact]
    public async Task ApprovalGeneratesASlugFromThePlainTitleWhenThereIsOne()
    {
        var id = await InsertPendingAsync(
            "rq-slug",
            title: "Vorasidenib in IDH1-Mutant Low-Grade Glioma (INDIGO)",
            plainTitle: "A pill slowed the growth of low-grade gliomas");

        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        var slug = await _connection.ExecuteScalarAsync<string>(
            "SELECT slug FROM aggregated_items WHERE id = @id", new { id });

        Assert.Equal("a-pill-slowed-the-growth-of-low-grade-gliomas", slug);
    }

    [Fact]
    public async Task SlugCollisionsAreResolvedRatherThanFailingTheApproval()
    {
        var first = await InsertPendingAsync("rq-dup-1", title: "Same headline");
        var second = await InsertPendingAsync("rq-dup-2", title: "Same headline");

        Assert.True(await _reviews.ApplyAsync(first, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None));
        Assert.True(await _reviews.ApplyAsync(second, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None));

        var slugs = (await _connection.QueryAsync<string>(
            "SELECT slug FROM aggregated_items WHERE id IN (@first, @second)",
            new { first, second })).ToList();

        Assert.Equal(2, slugs.Distinct().Count());
    }

    [Theory]
    [InlineData("A pill that works!", "a-pill-that-works")]
    [InlineData("  Spaces   everywhere  ", "spaces-everywhere")]
    [InlineData("IDH1/IDH2-mutant", "idh1-idh2-mutant")]
    [InlineData("!!!", "item")]
    public void SlugifyProducesCleanUrlSegments(string title, string expected)
    {
        Assert.Equal(expected, ReviewRepository.Slugify(title));
    }

    // ---------- inline edit before approve (WI-305) ----------

    [Fact]
    public async Task InlineEditsPersistAndTheApprovedSlugUsesTheEditedTitle()
    {
        var id = await InsertPendingAsync("rq-edit", title: "Original jargon title");

        var edits = new SummaryEdits(
            PlainTitle: "A clearer plain title",
            PlainSummary: "A one-sentence hook.",
            PlainWhatStudied: "Who and what.",
            PlainWhatFound: "The finding.",
            PlainMeans: "What it could mean.",
            PlainDoesntMean: "What it does not mean.",
            ReadinessScore: 7,
            ReadinessReason: "In trials, not yet approved.");
        Assert.True(await _reviews.SaveSummaryEditsAsync(id, edits, CancellationToken.None));

        var row = await _connection.QuerySingleAsync<(string Title, string Found, int Score, string Reason)>(
            """
            SELECT plain_title, plain_what_found, readiness_score, readiness_reason
            FROM aggregated_items WHERE id = @id
            """, new { id });
        Assert.Equal("A clearer plain title", row.Title);
        Assert.Equal("The finding.", row.Found);
        Assert.Equal(7, row.Score);

        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);
        var slug = await _connection.ExecuteScalarAsync<string>(
            "SELECT slug FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("a-clearer-plain-title", slug);
    }

    [Fact]
    public async Task ABlankEditFieldKeepsTheExistingText()
    {
        // A reviewer who edits one block must not wipe the others (they post
        // blank → null → COALESCE keeps what's there).
        var id = await InsertPendingAsync("rq-blank", plainTitle: "Keep this title");

        var onlyReason = new SummaryEdits(
            null, null, null, null, null, null, null, "Just a new reason.");
        await _reviews.SaveSummaryEditsAsync(id, onlyReason, CancellationToken.None);

        var title = await _connection.ExecuteScalarAsync<string>(
            "SELECT plain_title FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("Keep this title", title);
    }

    [Fact]
    public async Task AReviewerCannotEditAReadinessScoreAboveTheStageCeiling()
    {
        // The anti-hype cap holds even for a human edit: a mouse study can't be
        // marked "near clinic" from the queue any more than from the pipeline.
        var id = await InsertPendingAsync("rq-clamp", researchStage: "preclinical_animal");

        var overscore = new SummaryEdits(null, null, null, null, null, null,
            ReadinessScore: 9, ReadinessReason: null);
        await _reviews.SaveSummaryEditsAsync(id, overscore, CancellationToken.None);

        var score = await _connection.ExecuteScalarAsync<int?>(
            "SELECT readiness_score FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal(2, score);
    }

    [Fact]
    public async Task EditsAreRefusedOnceAnItemIsReviewed()
    {
        // Content freezes after review — an edit must not alter a published page.
        var id = await InsertPendingAsync("rq-frozen", plainTitle: "Published title");
        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        var applied = await _reviews.SaveSummaryEditsAsync(
            id, new SummaryEdits("Sneaky edit", null, null, null, null, null, null, null),
            CancellationToken.None);

        Assert.False(applied);
        var title = await _connection.ExecuteScalarAsync<string>(
            "SELECT plain_title FROM aggregated_items WHERE id = @id", new { id });
        Assert.Equal("Published title", title);
    }

    // ---------- reader-reported live pages (WI-306) ----------

    [Fact]
    public async Task ReportedPublishedItemsSurfaceForTheAdmin()
    {
        // A live page a reader flagged must reach the admin — it isn't 'pending'
        // (already published), so the normal queue wouldn't show it.
        var id = await InsertPendingAsync("rq-reported");
        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);
        await _connection.ExecuteAsync(
            "UPDATE aggregated_items SET summary_flagged = true WHERE id = @id", new { id });

        var reported = await _reviews.GetReportedAsync(50, CancellationToken.None);

        Assert.Contains(reported, i => i.Id == id);
        Assert.Equal(1, await _reviews.CountReportedAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ApprovingClearsThePipelineFlagSoItIsNotMistakenForAReaderReport()
    {
        // A numeral/banned-phrase flag from the pipeline must not linger on the
        // published row — otherwise it shows in "Reported by readers" with no
        // reader behind it.
        var id = await InsertPendingAsync("rq-pipeflag", flagged: true);

        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);

        var flagged = await _connection.ExecuteScalarAsync<bool>(
            "SELECT summary_flagged FROM aggregated_items WHERE id = @id", new { id });
        Assert.False(flagged);
        Assert.DoesNotContain(await _reviews.GetReportedAsync(50, CancellationToken.None), i => i.Id == id);
    }

    [Fact]
    public async Task DismissingAReportClearsTheFlagButLeavesThePagePublished()
    {
        var id = await InsertPendingAsync("rq-dismiss");
        await _reviews.ApplyAsync(id, ReviewAction.Approved, "dan@example.org", null, CancellationToken.None);
        await _connection.ExecuteAsync(
            "UPDATE aggregated_items SET summary_flagged = true WHERE id = @id", new { id });

        Assert.True(await _reviews.DismissReportAsync(id, CancellationToken.None));

        var row = await _connection.QuerySingleAsync<(bool Flagged, string Status)>(
            "SELECT summary_flagged, status FROM aggregated_items WHERE id = @id", new { id });
        Assert.False(row.Flagged);
        Assert.Equal("published", row.Status);
    }

    // ---------- the badge the reviewer sees ----------

    [Theory]
    [InlineData("research", "human_trial", ResearchStage.TestedInPeople)]
    [InlineData("research", "observational", ResearchStage.TestedInPeople)]
    [InlineData("research", "review_guideline", ResearchStage.ReviewOfExistingResearch)]
    [InlineData("research", "preclinical_animal", ResearchStage.EarlyResearchAnimals)]
    [InlineData("research", "preclinical_cell", ResearchStage.EarlyResearchLabCells)]
    [InlineData("news", "news_other", ResearchStage.News)]
    [InlineData("trial_update", null, ResearchStage.NewOrUpdatedTrial)]
    public void DatabaseStagesMapToTheBadgeAReaderWouldSee(
        string sourceKind, string? researchStage, ResearchStage expected)
    {
        Assert.Equal(expected, ResearchStageMapper.From(sourceKind, researchStage));
    }

    [Fact]
    public void PreprintAlwaysWinsOverTheResearchStage()
    {
        // Even if a preprint were classified as a human trial, it must never
        // wear a "tested in people" badge.
        Assert.Equal(ResearchStage.Preprint,
            ResearchStageMapper.From("preprint", "human_trial"));
    }
}
