using System.Net;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-209: the public feed. Two rules here are safety rules, not features:
/// only human-approved items are ever visible, and early-stage animal/cell
/// work is hidden unless the reader asks for it (a mouse-study headline reads
/// as false hope — PLAN.md §3).
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class FeedTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;
    private FeedRepository _feed = null!;

    public FeedTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
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

        var taxonomyPath = Path.Combine(
            FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml");
        _feed = new FeedRepository(
            new TestConnectionFactory(_database.ConnectionString),
            new TaxonomyStore(File.ReadAllText(taxonomyPath)));
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

    private Task InsertAsync(
        string externalId,
        string status = "published",
        string relevance = "patient_relevant",
        string sourceKind = "research",
        string[]? tags = null,
        string? slug = null,
        DateOnly? publishedAt = null) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance,
                 tumor_tags, slug, published_at, research_stage, plain_title)
            VALUES
                (@TestSource, @sourceKind, @externalId, @title, 'https://example.org',
                 @status, @relevance, @tags, @slug, @publishedAt, 'human_trial', @title)
            """,
            new
            {
                TestSource,
                sourceKind,
                externalId,
                title = $"Study {externalId}",
                status,
                relevance,
                tags = tags ?? ["glioblastoma"],
                slug = slug ?? $"study-{externalId}",
                publishedAt = publishedAt ?? new DateOnly(2026, 6, 12),
            });

    // ---------- the human gate ----------

    [Fact]
    public async Task OnlyPublishedItemsAreVisible()
    {
        await InsertAsync("f-published", status: "published");
        await InsertAsync("f-pending", status: "pending", slug: "study-f-pending");
        await InsertAsync("f-rejected", status: "rejected", slug: "study-f-rejected");
        await InsertAsync("f-pulled", status: "pulled", slug: "study-f-pulled");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);
        var ids = page.Items.Select(i => i.Slug).ToList();

        Assert.Contains("study-f-published", ids);
        Assert.DoesNotContain("study-f-pending", ids);
        Assert.DoesNotContain("study-f-rejected", ids);
        Assert.DoesNotContain("study-f-pulled", ids);
    }

    [Fact]
    public async Task APulledItemsPermalinkIsGoneNotJustHidden()
    {
        await InsertAsync("f-gone", status: "pulled", slug: "study-f-gone");

        var found = await _feed.GetPublishedBySlugAsync("study-f-gone", CancellationToken.None);

        Assert.Null(found);
    }

    // ---------- early-stage gating ----------

    [Fact]
    public async Task EarlyStageResearchIsHiddenByDefault()
    {
        // The anti-hype rule: animal and lab work must not reach a scared
        // reader unasked.
        await InsertAsync("f-human", relevance: "patient_relevant");
        await InsertAsync("f-mouse", relevance: "early_stage", slug: "study-f-mouse");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);

        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-mouse");
        Assert.Contains(page.Items, i => i.Slug == "study-f-human");
    }

    [Fact]
    public async Task UnclassifiedItemsAreVisibleOnceApproved()
    {
        // Until the M3 classifier lands every item is relevance='pending'.
        // Hiding those would mean a reviewer approves something in M2 and
        // nothing visibly happens — caught in the WI-211 shakedown.
        await InsertAsync("f-unclassified", relevance: "pending", slug: "study-f-unclassified");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-unclassified");
    }

    [Fact]
    public async Task EarlyStageAppearsOnlyWhenTheReaderAsksForIt()
    {
        await InsertAsync("f-mouse2", relevance: "early_stage", slug: "study-f-mouse2");

        var page = await _feed.GetAsync(
            new FeedQuery(IncludeEarlyStage: true), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-mouse2");
    }

    // ---------- filters ----------

    [Fact]
    public async Task FilteringByAParentTumorTypeIncludesItsDescendants()
    {
        // Browsing "glioma" must surface glioblastoma research.
        await InsertAsync("f-gbm", tags: ["glioblastoma"], slug: "study-f-gbm");
        await InsertAsync("f-mening", tags: ["meningioma"], slug: "study-f-mening");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "glioma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-gbm");
        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-mening");
    }

    [Fact]
    public async Task CatchAllItemsMatchEveryTumorFilter()
    {
        await InsertAsync("f-all", tags: ["all-brain-tumors"], slug: "study-f-all");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "meningioma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-all");
    }

    [Fact]
    public async Task AnUnknownTumorFilterIsIgnoredRatherThanReturningNothing()
    {
        await InsertAsync("f-any", slug: "study-f-any");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "dragonoma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-any");
    }

    [Fact]
    public async Task FilteringByKindWorks()
    {
        await InsertAsync("f-research", sourceKind: "research", slug: "study-f-research");
        await InsertAsync("f-news", sourceKind: "news", slug: "study-f-news");

        var page = await _feed.GetAsync(new FeedQuery(Kind: "news"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-news");
        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-research");
    }

    [Theory]
    [InlineData("research", "research")]
    [InlineData("news", "news")]
    [InlineData("'; DROP TABLE aggregated_items; --", null)]
    [InlineData("nonsense", null)]
    public void OnlyDocumentedKindsSurviveNormalization(string input, string? expected)
    {
        // Querystring values never reach SQL as text; anything unrecognized
        // means "no filter".
        Assert.Equal(expected, FeedRepository.NormalizeKind(input));
    }

    [Fact]
    public async Task UndatedItemsSortLastNotFirst()
    {
        // Postgres DESC defaults to NULLS FIRST, which would float undated
        // items to the top of the feed.
        await InsertAsync("f-dated", publishedAt: new DateOnly(2026, 6, 1), slug: "study-f-dated");
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug, published_at)
            VALUES (@TestSource, 'research', 'f-undated', 'Undated study', 'https://example.org',
                    'published', 'patient_relevant', 'study-f-undated', NULL)
            """,
            new { TestSource });

        // Scoped to this test's own rows: the dev database may hold real
        // fetched items, and asserting on global position 0 would make the
        // test depend on whatever else is in the table.
        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);
        var mine = page.Items
            .Select((item, index) => (item, index))
            .Where(x => x.item.Slug is "study-f-dated" or "study-f-undated")
            .ToList();

        Assert.Equal(2, mine.Count);
        Assert.Equal("study-f-dated", mine[0].item.Slug);
        Assert.Equal("study-f-undated", mine[1].item.Slug);
    }

    // ---------- the pages ----------

    [Fact]
    public async Task TheFeedPageRendersCardsAndTheEarlyStageToggle()
    {
        await InsertAsync("f-page", slug: "study-f-page");

        var html = await _factory.CreateClient().GetStringAsync("/research");

        Assert.Contains("Research updates", html);
        Assert.Contains("Show early-stage research", html);
        Assert.Contains("class=\"feed-grid\"", html);
        Assert.Contains("badge badge--", html);
    }

    [Fact]
    public async Task AnItemPermalinkRendersWithItsBadgeAndProvenance()
    {
        await InsertAsync("f-perma", slug: "study-f-perma");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-perma");

        Assert.Contains("Study f-perma", html);
        Assert.Contains("How early is this?", html);
        Assert.Contains("Read the original", html);
        Assert.Contains("class=\"ai-note\"", html);
    }

    [Fact]
    public async Task ItemPagesDoNotInventASummaryWhenThereIsNone()
    {
        await InsertAsync("f-nosum", slug: "study-f-nosum");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-nosum");

        Assert.Contains("have not written a plain-language summary", html);
    }

    [Fact]
    public async Task AnAutoPublishedItemPageSaysNoPersonReviewedIt()
    {
        // WI-212: when the review gate is off, the item page must say so
        // rather than claim a person reviewed it.
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 plain_title, plain_summary, reviewed_by)
            VALUES (@TestSource, 'research', 'f-auto', 'A study', 'https://example.org',
                    'published', 'patient_relevant', 'study-f-auto',
                    'A pill slowed tumor growth', 'A clear summary.', 'auto')
            """,
            new { TestSource });

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-auto"));

        Assert.Contains("published automatically", html);
        Assert.Contains("A person did not review it before publishing", html);
    }

    // Rendered HTML wraps prose across lines; compare on collapsed whitespace.
    private static string Collapse(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ");

    [Fact]
    public async Task AHumanReviewedItemPageSaysAPersonReviewedIt()
    {
        await InsertAsync("f-human-rev", slug: "study-f-human-rev");
        await _connection.ExecuteAsync(
            "UPDATE aggregated_items SET reviewed_by = 'dan@example.org' WHERE source = @TestSource AND external_id = 'f-human-rev'",
            new { TestSource });

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-human-rev"));

        Assert.Contains("reviewed by a person before publishing", html);
    }

    [Fact]
    public async Task AnUnknownSlugIsA404()
    {
        var response = await _factory.CreateClient().GetAsync("/research/no-such-item");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RawSourceTextIsNeverRenderedPublicly()
    {
        // Licensing: raw_summary is pipeline input only (data-model.md).
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, raw_summary, status, relevance, slug)
            VALUES (@TestSource, 'research', 'f-raw', 'A study', 'https://example.org',
                    'SECRET-ABSTRACT-TEXT', 'published', 'patient_relevant', 'study-f-raw')
            """,
            new { TestSource });

        var client = _factory.CreateClient();

        Assert.DoesNotContain("SECRET-ABSTRACT-TEXT", await client.GetStringAsync("/research"));
        Assert.DoesNotContain("SECRET-ABSTRACT-TEXT",
            await client.GetStringAsync("/research/study-f-raw"));
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
