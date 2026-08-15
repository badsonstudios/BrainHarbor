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

    // Home shows only the newest few and cannot page, so seeds must sort into
    // that window on any database — the dirty-database rule on DatabaseFixture.
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

    /// <summary>
    /// The home page must say, up front, that the summaries are written by AI
    /// AND that the underlying research is not — the two halves have to travel
    /// together, or "written by AI" reads as though AI did the science.
    /// Rewording is fine; dropping either half is not, so change this test
    /// deliberately rather than to make a build pass.
    /// </summary>
    [Fact]
    public async Task HomeSaysAiWroteTheSummariesAndScientistsDidTheResearch()
    {
        var html = await GetHomeAsync();

        Assert.Contains("Scientists do the research", html);
        Assert.Contains("AI puts what they found into plain words", html);

        // And that nobody is promised a human reviewer (Publishing:Mode Auto).
        // The 2026-08-15 redesign moved the full notice to the foot of the
        // page; this sentence came with it rather than being dropped, because
        // "checks run before it publishes" does not tell a reader that no
        // person read theirs.
        Assert.Contains("A person does not check every one", html);
    }

    /// <summary>
    /// WI-422 (Dan's ask): the home page says plainly that AI gets things
    /// wrong, and says what to do about it.
    ///
    /// The 2026-08-15 redesign moved the full notice to the foot of the page so
    /// it stops standing between the reader and the content. The ADMISSION did
    /// not move: it leads in the hero band, above the feed, because a reader
    /// must not be able to get through eight summaries before learning the
    /// writer is fallible. That ordering is the property under test.
    ///
    /// Rewording is fine, dropping it is not. Change this test deliberately,
    /// never to make a build pass.
    /// </summary>
    [Fact]
    public async Task HomeWarnsThatAiCanBeWrongAndSaysWhatToDo()
    {
        var html = await GetHomeAsync();

        Assert.Contains("AI can make mistakes", html);

        // The admission on its own is not much use — it has to land with the
        // two actions that make it actionable.
        Assert.Contains("read the study we link to", html);
        Assert.Contains("care team", html);

        // Above the feed, not buried under it: a warning a reader meets AFTER
        // the summaries has already failed at its job.
        var admission = html.IndexOf("AI can make mistakes", StringComparison.Ordinal);
        var feed = html.IndexOf("feed-grid", StringComparison.Ordinal);
        Assert.True(admission > 0, "the AI admission is missing from the home page");
        Assert.True(feed < 0 || admission < feed,
            "the AI admission must come before the feed, not after it");
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
