using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-409: the home page leads with the feed. The front door must show the
/// newest published items — and must never again claim the research feed is
/// "coming soon" while published items exist (the sentence that sat on the
/// live home page for two weeks after /research shipped).
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class HomeFeedTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestSource = "test_home";

    // Home shows only the newest few and cannot page, and the dev database
    // holds ~1,400 real rows from live pipeline runs. A far-future date is the
    // only way to guarantee our rows sort into that window on ANY database
    // (the same dirty-DB rule FeedTests learned in WI-402).
    private static readonly DateOnly FarFuture = new(2999, 1, 1);

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;

    public HomeFeedTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
    {
        _database = database;
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString));
    }

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_database.ConnectionString);
        await _connection.OpenAsync();
        await CleanupAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        await _connection.DisposeAsync();
    }

    private Task CleanupAsync() => _connection.ExecuteAsync(
        "DELETE FROM aggregated_items WHERE source = @TestSource", new { TestSource });

    private Task InsertAsync(
        string externalId,
        string status = "published",
        string relevance = "patient_relevant",
        string stage = "human_trial") =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance,
                 tumor_tags, slug, published_at, research_stage, plain_title)
            VALUES
                (@TestSource, 'research', @externalId, @title, 'https://example.org',
                 @status, @relevance, '{glioblastoma}', @slug, @FarFuture, @stage, @title)
            """,
            new
            {
                TestSource,
                externalId,
                title = $"Study {externalId}",
                status,
                relevance,
                stage,
                slug = $"home-{externalId}",
                FarFuture,
            });

    private async Task<string> GetHomeAsync(bool earlyCookie = false)
    {
        var client = _factory.CreateClient();
        if (earlyCookie)
        {
            client.DefaultRequestHeaders.Add("Cookie", "bh_show_early=1");
        }

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task HomeShowsTheNewestPublishedItemsAsFeedCards()
    {
        await InsertAsync("h-new");

        var html = await GetHomeAsync();

        Assert.Contains("Latest updates", html);
        Assert.Contains("href=\"/research/home-h-new\"", html);
        Assert.Contains("Study h-new", html);
    }

    /// <summary>The WI-409 acceptance test: while published items exist, the
    /// home page must not claim the feed is coming. (The digest genuinely is
    /// coming, so that word may still appear — but never about the feed.)</summary>
    [Fact]
    public async Task HomeNeverClaimsTheFeedIsComingWhilePublishedItemsExist()
    {
        await InsertAsync("h-honest");

        var html = await GetHomeAsync();

        Assert.DoesNotContain("research feed and the weekly digest are coming", html);
        Assert.DoesNotContain("feed is coming", html);
        Assert.DoesNotContain("feed and the weekly digest", html);
        Assert.Contains("class=\"feed-grid\"", html);
    }

    [Fact]
    public async Task PendingItemsDoNotAppearOnHome()
    {
        await InsertAsync("h-pending", status: "pending");

        var html = await GetHomeAsync();

        Assert.DoesNotContain("home-h-pending", html);
    }

    /// <summary>Same early-stage rule as /research: a mouse-study headline on
    /// the front door reads as false hope (PLAN.md §3).</summary>
    [Fact]
    public async Task EarlyStageItemsAreHiddenFromHomeByDefault()
    {
        await InsertAsync("h-mouse", relevance: "early_stage", stage: "preclinical_animal");

        var html = await GetHomeAsync();

        Assert.DoesNotContain("home-h-mouse", html);
    }

    /// <summary>…and the same opt-in: the persisted /research choice (WI-307)
    /// follows the reader to the home page.</summary>
    [Fact]
    public async Task EarlyStageItemsAppearOnHomeWhenTheReaderOptedIn()
    {
        await InsertAsync("h-mouse-optin", relevance: "early_stage", stage: "preclinical_animal");

        var html = await GetHomeAsync(earlyCookie: true);

        Assert.Contains("home-h-mouse-optin", html);
    }
}
