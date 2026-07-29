using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-104 routing tests: curated pages resolve by top-level slug and by
/// section/slug through the catch-all route, against the fixture content dir.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ContentPageRoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    private static string FixtureRoot =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "content");

    public ContentPageRoutingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString);
            builder.UseSetting("Content:Root", FixtureRoot);
        });
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TopLevelSlugRendersThePage()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/about");

        Assert.Contains("About BrainHarbor (fixture)", html);
        Assert.Contains("<strong>test fixture</strong>", html);
        Assert.Contains("Last reviewed: July 19, 2026", html);
        Assert.Contains("Example source", html);
        Assert.Contains("class=\"helpline-band\"", html); // shell wraps content
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task SectionSlugRendersThePage()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/benefits/fast-track");

        Assert.Contains("Compassionate Allowances", html);
        Assert.Contains("fast-track list", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task UnknownPathIsA404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/no-such-page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TraversalShapedPathsAreRejected()
    {
        var client = _factory.CreateClient();

        // Anything that isn't slug-shaped (dots, deep nesting) must 404,
        // never touch the filesystem outside the content root.
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/..%2F..%2Fappsettings")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/a/b/c")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/About.")).StatusCode);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task RealRazorPagesStillWinOverTheCatchAll()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/get-help-now");

        response.EnsureSuccessStatusCode();
        Assert.Contains("988", await response.Content.ReadAsStringAsync());
    }
}
