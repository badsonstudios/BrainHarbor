using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-103: the helpline band is on every page (including error pages), and
/// /get-help-now carries the crisis numbers. This is the "always one tap to
/// a human" promise from PLAN.md §3 — treat regressions as serious.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class HelplineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString => TestDatabase.ConnectionString;

    public HelplineTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString));
    }

    [Theory]
    [Trait("Category", "Database")]
    [InlineData("/")]
    [InlineData("/get-help-now")]
    [InlineData("/privacy")]
    public async Task HelplineBandIsOnEveryPage(string url)
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync(url);

        Assert.Contains("class=\"helpline-band\"", html);
        Assert.Contains("href=\"tel:8008862282\"", html);
        Assert.Contains("ABTA CareLine: 800-886-2282", html);
        Assert.Contains("href=\"/get-help-now\"", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task GetHelpNowListsCrisisAndOrgHelplines()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/get-help-now");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("988", html);                       // Suicide & Crisis Lifeline
        Assert.Contains("741741", html);                    // Crisis Text Line
        Assert.Contains("800-886-2282", html);              // ABTA CareLine
        Assert.Contains("800-422-6237", html);              // NCI Cancer Information Service
        Assert.Contains("800-813-4673", html);              // CancerCare (caregivers)
        Assert.Contains("href=\"tel:988\"", html);          // one-tap dialing
        Assert.Contains("href=\"sms:741741?body=HOME\"", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task Custom404KeepsTheHelplineBandAndOffersAWayOut()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/this-page-does-not-exist");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("We can't find that page", html);
        Assert.Contains("class=\"helpline-band\"", html);
        Assert.Contains("href=\"/get-help-now\"", html);
        Assert.Contains("Go to the home page", html);
        // The large-text toggle must point at the original URL, not the
        // re-executed /status/404 phantom page.
        Assert.Contains("href=\"/this-page-does-not-exist?textsize=large\"", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task DirectHitOnStatusPageIsNotA200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/status/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
