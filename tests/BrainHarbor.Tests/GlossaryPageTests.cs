using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-105 integration: /glossary A–Z page and the tooltip pipeline through a
/// real content page, using fixture roots.
/// </summary>
public class GlossaryPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    public GlossaryPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString);
            builder.UseSetting("Content:Root",
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "content"));
            builder.UseSetting("Glossary:Root",
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "glossary"));
        });
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task GlossaryPageListsTermsWithAnchors()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/glossary");

        Assert.Contains("id=\"glioma\"", html);
        Assert.Contains("glee-OH-muh", html);
        Assert.Contains("fixture definition", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task ContentPageRendersTooltipForGlossaryTerm()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/tooltip-demo");

        Assert.Contains("popovertarget=\"def-glioma\"", html);
        Assert.Contains(">glioma</button>", html);
        Assert.Contains("href=\"/glossary#glioma\"", html);
    }
}
