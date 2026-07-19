using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// Renders the home page through the real app and asserts the WI-101 shell:
/// semantic landmarks, skip link, and both stylesheets. Startup runs DbUp in
/// Development, so this needs the local Postgres container (docker compose
/// up -d) or the CI service container — hence the Database trait.
/// </summary>
public class LayoutRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // Fallback mirrors the dev defaults in docker-compose.yml — keep in sync.
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    public LayoutRenderTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString));
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task HomePageRendersShellLandmarksAndStylesheets()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        // Skip link + landmarks
        Assert.Contains("class=\"skip-link\" href=\"#main-content\"", html);
        Assert.Contains("<nav class=\"site-nav\" aria-label=\"Main\">", html);
        Assert.Contains("<main id=\"main-content\"", html);
        Assert.Contains("<footer class=\"site-footer\">", html);

        // v1 nav per docs/sitemap.md; Get Help Now is the filled pill (WI-108)
        Assert.Contains("href=\"/research\"", html);
        Assert.Contains("href=\"/trials\"", html);
        Assert.Contains("href=\"/digest\"", html);
        Assert.Contains("class=\"nav-cta\" href=\"/get-help-now\"", html);

        // Clear & Kind shell (WI-108): Entry Hub home + footer trust cues
        Assert.Contains("class=\"hub\"", html);
        Assert.Contains("class=\"door door--primary\" href=\"/start\"", html);
        Assert.Contains("class=\"door\" href=\"/research\"", html);
        Assert.Contains("class=\"door\" href=\"/get-help-now\"", html);
        Assert.Contains("class=\"ai-note\"", html);

        // Both stylesheets, print.css scoped to print media.
        // MapStaticAssets fingerprints filenames (site.<hash>.css).
        Assert.Matches("href=\"[^\"]*css/site[.\\w]*\\.css[^\"]*\"", html);
        Assert.Matches("href=\"[^\"]*css/print[.\\w]*\\.css[^\"]*\" media=\"print\"", html);
    }
}
