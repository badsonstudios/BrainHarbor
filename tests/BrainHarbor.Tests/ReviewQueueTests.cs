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
