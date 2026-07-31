using System.Xml.Linq;
using BrainHarbor.Web.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-308: the machine-readable surface — robots.txt, sitemap.xml, feed.xml.
/// The load-bearing rule mirrors the feed's: only human-gate-passed
/// (status='published') items are ever syndicated, and the XML must be
/// well-formed regardless of item titles.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class SyndicationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;

    public SyndicationTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
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

    private Task InsertAsync(string externalId, string slug, string status, string? plainTitle = null) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, published_at)
            VALUES
                (@TestSource, 'research', @externalId, 'Original title', 'https://example.org',
                 @status, 'patient_relevant', @slug, 'human_trial', @plainTitle,
                 'A plain hook.', DATE '2026-06-12')
            """,
            new { TestSource, externalId, slug, status, plainTitle });

    [Fact]
    public async Task RobotsTxtPointsAtTheSitemapAndBlocksAdmin()
    {
        var body = await _factory.CreateClient().GetStringAsync("/robots.txt");

        Assert.Contains("Sitemap:", body);
        Assert.Contains("/sitemap.xml", body);
        Assert.Contains("Disallow: /admin", body);
    }

    [Fact]
    public async Task TheSitemapListsStaticPagesAndPublishedItemsButNotPendingOnes()
    {
        await InsertAsync("s-pub", "study-s-pub", "published");
        await InsertAsync("s-pend", "study-s-pend", "pending");

        var xml = await _factory.CreateClient().GetStringAsync("/sitemap.xml");
        var doc = XDocument.Parse(xml); // must be well-formed
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var locs = doc.Descendants(ns + "loc").Select(e => e.Value).ToList();

        Assert.Contains(locs, l => l.EndsWith("/how-we-write"));
        Assert.Contains(locs, l => l.EndsWith("/research/study-s-pub"));
        Assert.DoesNotContain(locs, l => l.EndsWith("/research/study-s-pend"));
    }

    [Fact]
    public async Task TheRssFeedCarriesPublishedItemsOnly()
    {
        await InsertAsync("r-pub", "study-r-pub", "published", plainTitle: "A plain-language headline");
        await InsertAsync("r-pull", "study-r-pull", "pulled");

        var xml = await _factory.CreateClient().GetStringAsync("/feed.xml");
        var doc = XDocument.Parse(xml);
        var links = doc.Descendants("item").Descendants("link").Select(e => e.Value).ToList();
        var titles = doc.Descendants("item").Descendants("title").Select(e => e.Value).ToList();

        Assert.Contains(links, l => l.EndsWith("/research/study-r-pub"));
        Assert.DoesNotContain(links, l => l.EndsWith("/research/study-r-pull"));
        Assert.Contains("A plain-language headline", titles);
    }

    [Fact]
    public async Task AnItemPageCarriesStructuredDataAndSocialTags()
    {
        await InsertAsync("s-ld", "study-s-ld", "published", plainTitle: "A pill slowed a tumor");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-s-ld");

        Assert.Contains("application/ld+json", html);
        Assert.Contains("MedicalWebPage", html);
        Assert.Contains("BreadcrumbList", html);
        Assert.Contains("og:description", html);
        Assert.Contains("rel=\"canonical\"", html);
    }
}
