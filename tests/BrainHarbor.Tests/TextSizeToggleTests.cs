using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// Integration tests for the WI-102 large-text middleware: querystring →
/// cookie → redirect, and the cookie → html class render path. Uses the
/// in-memory TestServer (Database trait: startup runs DbUp).
/// </summary>
public class TextSizeToggleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    public TextSizeToggleTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString));
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    [Trait("Category", "Database")]
    public async Task TextsizeLargeSetsCookieAndRedirectsToCleanUrl()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/?textsize=large");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
        var setCookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("bh_textsize=large", setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task RedirectPreservesOtherQueryParameters()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/?foo=bar&textsize=large");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/?foo=bar", response.Headers.Location?.ToString());
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task CookieRendersLargeTextClassAndFlippedToggleLabel()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bh_textsize=large");

        var html = await client.GetStringAsync("/");

        Assert.Contains("<html lang=\"en\" class=\"text-large\">", html);
        Assert.Contains("Standard text", html);
        Assert.Contains("?textsize=standard", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task WithoutCookieRendersStandardTextAndLargeToggle()
    {
        var client = CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("Larger text", html);
        Assert.Contains("?textsize=large", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TextsizeStandardDeletesTheCookie()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bh_textsize=large");

        var response = await client.GetAsync("/?textsize=standard");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        var setCookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("bh_textsize=", setCookie);
        Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
    }
}
