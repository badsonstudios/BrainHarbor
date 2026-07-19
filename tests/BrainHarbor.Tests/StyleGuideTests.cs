using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-109: /dev/styleguide renders every badge kind through the real partials
/// in Development, and is a 404 everywhere else.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class StyleGuideTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    public StyleGuideTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString));
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task StyleGuideRendersAllBadgeKindsInDevelopment()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/dev/styleguide");

        // The four badge families, via the real partial.
        Assert.Contains("badge badge--result", html);
        Assert.Contains("badge badge--progress", html);
        Assert.Contains("badge badge--info", html);
        Assert.Contains("badge badge--unverified", html);

        // Server-built accessibility contract.
        Assert.Contains("aria-label=\"Tested in people. Evidence strength 5 of 5.\"", html);
        Assert.Contains("aria-label=\"Early research (lab cells). Evidence strength 1 of 5.\"", html);
        Assert.Contains("role=\"img\"", html);

        // Glossary tooltip is previewable here (no shipped page uses a term yet).
        Assert.Contains("popovertarget=\"def-glioma\"", html);
        Assert.Contains("popovertarget=\"def-idh-gene-change\"", html);

        // Meter is visual-only; cards use the fixed anatomy.
        Assert.Contains("class=\"badge__meter\" aria-hidden=\"true\"", html);
        Assert.Contains("class=\"feed-grid\"", html);
        Assert.Contains("class=\"card__hook\"", html);
        Assert.Contains("class=\"card__meta\"", html);

        // Unverified renders the dashed METER (no glyph, zero filled steps):
        // slice from the badge opening to its label — the meter sits between.
        var unverified = html[html.IndexOf("badge--unverified", StringComparison.Ordinal)..];
        unverified = unverified[..unverified.IndexOf("badge__label", StringComparison.Ordinal)];
        Assert.Contains("badge__meter", unverified);
        Assert.DoesNotContain("step--on", unverified);
        Assert.DoesNotContain("badge__glyph", unverified);

        // Card anatomy order is fixed: badge → title → hook → meta.
        var card = html[html.IndexOf("class=\"card\"", StringComparison.Ordinal)..];
        var badgeAt = card.IndexOf("class=\"badge", StringComparison.Ordinal);
        var titleAt = card.IndexOf("<h3>", StringComparison.Ordinal);
        var hookAt = card.IndexOf("card__hook", StringComparison.Ordinal);
        var metaAt = card.IndexOf("card__meta", StringComparison.Ordinal);
        Assert.True(badgeAt < titleAt && titleAt < hookAt && hookAt < metaAt,
            "card anatomy must be badge -> title -> hook -> meta");
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task StyleGuideIs404OutsideDevelopment()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();

        var response = await client.GetAsync("/dev/styleguide");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
