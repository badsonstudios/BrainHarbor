using BrainHarbor.Web.Api;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Services;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-212: auto-publish mode. Auto is the default — a summarized item that
/// passed the pipeline's automated safety checks publishes itself; a flagged
/// or unsummarized item is held for a human ("hold only the flagged ones").
/// Exercised at the repository level, the same way FeedTests and
/// ReviewQueueTests do, so the boundary is pinned without booting a web host
/// per case.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class AutoPublishTests : IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;
    private TaxonomyStore _taxonomy = null!;

    public AutoPublishTests(DatabaseFixture database) => _database = database;

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_database.ConnectionString);
        await _connection.OpenAsync();
        await CleanupAsync();

        var path = Path.Combine(FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml");
        _taxonomy = new TaxonomyStore(File.ReadAllText(path));
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

    private SyncRepository Repository(PublishMode mode) => new(
        new TestConnectionFactory(_database.ConnectionString),
        _taxonomy,
        Options.Create(new PublishingOptions { Mode = mode }));

    private static SyncItem Item(
        string externalId,
        string? plainSummary = "A clear plain-language summary.",
        bool flagged = false) => new()
        {
            Source = TestSource,
            SourceKind = "research",
            ExternalId = externalId,
            Title = "A study of something",
            Url = "https://example.org/study",
            RawSummary = "Abstract.",
            TumorTags = ["glioblastoma"],
            Relevance = "patient_relevant",
            ResearchStage = "human_trial",
            PlainTitle = "A pill slowed tumor growth",
            PlainSummary = plainSummary,
            SummaryModel = "test-model",
            SummaryFlagged = flagged,
        };

    private async Task<string?> StatusOf(string externalId) =>
        await _connection.ExecuteScalarAsync<string?>(
            "SELECT status FROM aggregated_items WHERE source = @TestSource AND external_id = @externalId",
            new { TestSource, externalId });

    // ---------- Auto mode (the default) ----------

    [Fact]
    public async Task ACleanSummarizedItemPublishesItselfInAutoMode()
    {
        var result = await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-clean")], null, CancellationToken.None);

        Assert.Equal(1, result.AutoPublished);
        Assert.Equal("published", await StatusOf("auto-clean"));
    }

    [Fact]
    public async Task AFlaggedItemIsHeldForAHumanEvenInAutoMode()
    {
        // The whole point of "hold only the flagged ones".
        var result = await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-flagged", flagged: true)], null, CancellationToken.None);

        Assert.Equal(0, result.AutoPublished);
        Assert.Equal("pending", await StatusOf("auto-flagged"));
    }

    [Fact]
    public async Task AnItemWithNoSummaryYetIsHeldNotPublished()
    {
        // Every M2 item is unsummarized until M3; those must never go live
        // blank. This is what makes Auto mode safe-by-construction today.
        var result = await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-nosummary", plainSummary: null)], null, CancellationToken.None);

        Assert.Equal(0, result.AutoPublished);
        Assert.Equal("pending", await StatusOf("auto-nosummary"));
    }

    [Fact]
    public async Task AnAutoPublishedItemGetsASlug()
    {
        await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-slug")], null, CancellationToken.None);

        var slug = await _connection.ExecuteScalarAsync<string>(
            "SELECT slug FROM aggregated_items WHERE source = @TestSource AND external_id = 'auto-slug'",
            new { TestSource });

        Assert.Equal("a-pill-slowed-tumor-growth", slug);
    }

    [Fact]
    public async Task AnAutoPublishIsRecordedInTheAuditTrailAsActorAuto()
    {
        // The gate became optional, not invisible: who (or what) published
        // each item is still on the record.
        await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-audit")], null, CancellationToken.None);

        var actor = await _connection.ExecuteScalarAsync<string>(
            """
            SELECT e.actor FROM review_events e
            JOIN aggregated_items a ON a.id = e.item_id
            WHERE a.source = @TestSource AND a.external_id = 'auto-audit'
            """,
            new { TestSource });

        Assert.Equal("auto", actor);
    }

    [Fact]
    public async Task AnAutoPublishedItemIsMarkedReviewedByAutoSoTheItemPageCanBeHonest()
    {
        await Repository(PublishMode.Auto)
            .UpsertAsync([Item("auto-by")], null, CancellationToken.None);

        var reviewedBy = await _connection.ExecuteScalarAsync<string>(
            "SELECT reviewed_by FROM aggregated_items WHERE source = @TestSource AND external_id = 'auto-by'",
            new { TestSource });

        Assert.Equal("auto", reviewedBy);
    }

    // ---------- Review mode (opt-in) ----------

    [Fact]
    public async Task ReviewModeHoldsEverythingIncludingCleanSummaries()
    {
        var result = await Repository(PublishMode.Review)
            .UpsertAsync([Item("review-clean")], null, CancellationToken.None);

        Assert.Equal(0, result.AutoPublished);
        Assert.Equal("pending", await StatusOf("review-clean"));
    }

    // ---------- the default ----------

    [Fact]
    public void PublishOptionsDefaultToAuto()
    {
        // Dan wants auto on by default — no configuration means Auto.
        Assert.Equal(PublishMode.Auto, new PublishingOptions().Mode);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
