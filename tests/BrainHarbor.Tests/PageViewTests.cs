using BrainHarbor.Web.Analytics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-441: the page-view tally.
///
/// Two things are being defended here and they pull in opposite directions.
/// The count has to be worth trusting — a number inflated by stylesheets, bots
/// and our own smoke check is worse than no number, because it looks like an
/// audience. And it has to stay a COUNT: the moment anything identifying gets
/// attached, /privacy stops being true.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class PageViewTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PageViewTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));
    }

    private static PageViewCounter Counter(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<PageViewCounter>();

    private static long CountFor(PageViewCounter counter, string path) =>
        counter.Drain()
            .Where(c => c.Key.Path == path)
            .Sum(c => c.Value);

    private HttpClient Browser()
    {
        var client = _factory.CreateClient();
        // A plausible browser: the middleware treats an absent or automated
        // user agent as a script, which is the whole point of the filter.
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15");
        return client;
    }

    [Fact]
    public async Task OpeningAPageCountsItOnce()
    {
        var counter = Counter(_factory);
        counter.Drain();   // start from a clean slate

        await Browser().GetStringAsync("/get-help-now");

        Assert.Equal(1, CountFor(counter, "/get-help-now"));
    }

    /// <summary>
    /// The reason this counter exists rather than reading Azure's Requests
    /// metric: one page view pulls stylesheets, htmx, the logo and a photo per
    /// card. Counting those would multiply every visit by ~20 and make the
    /// number meaningless.
    /// </summary>
    [Fact]
    public async Task AssetsAreNotPageViews()
    {
        var counter = Counter(_factory);
        counter.Drain();

        var client = Browser();
        await client.GetAsync("/css/site.css");
        await client.GetAsync("/js/htmx.min.js");
        await client.GetAsync("/img/brand/favicon.svg");

        Assert.Empty(counter.Drain());
    }

    /// <summary>
    /// The deploy smoke check polls five routes in a loop on every release, and
    /// crawlers will be most of the traffic on an unlaunched site. Neither is a
    /// reader. The user agent is read to make this decision and then thrown
    /// away — it is never stored.
    /// </summary>
    [Theory]
    [InlineData("curl/8.4.0")]
    [InlineData("Googlebot/2.1 (+http://www.google.com/bot.html)")]
    [InlineData("python-requests/2.31.0")]
    [InlineData("HeadlessChrome/120.0.0.0")]
    [InlineData("SomeUptimeMonitor/1.0")]
    public async Task AutomatedCallersAreNotCounted(string userAgent)
    {
        var counter = Counter(_factory);
        counter.Drain();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        await client.GetAsync("/get-help-now");

        Assert.Empty(counter.Drain());
    }

    [Fact]
    public async Task ARequestWithNoUserAgentIsNotCounted()
    {
        var counter = Counter(_factory);
        counter.Drain();

        await _factory.CreateClient().GetAsync("/get-help-now");

        Assert.Empty(counter.Drain());
    }

    /// <summary>
    /// An htmx swap fetches a fragment of a page the reader is already on.
    /// Counting them would inflate /research every time somebody changed a
    /// filter — one person browsing, rendered as traffic.
    /// </summary>
    [Fact]
    public async Task HtmxFragmentsAreNotCounted()
    {
        var counter = Counter(_factory);
        counter.Drain();

        var client = Browser();
        client.DefaultRequestHeaders.Add("HX-Request", "true");
        await client.GetAsync("/research");

        Assert.Empty(counter.Drain());
    }

    [Fact]
    public async Task TheAdminAreaIsNotCounted()
    {
        var counter = Counter(_factory);
        counter.Drain();

        // Unauthenticated, so this redirects to the login page — which is also
        // not a page view, since only a 200 counts.
        await Browser().GetAsync("/admin/health");

        Assert.Empty(counter.Drain());
    }

    /// <summary>
    /// A missing page is not a page view. Counting 404s would also let anyone
    /// fill the table with junk paths by requesting them.
    /// </summary>
    [Fact]
    public async Task AMissingPageIsNotCounted()
    {
        var counter = Counter(_factory);
        counter.Drain();

        await Browser().GetAsync("/no-such-page-exists-here");

        Assert.Empty(counter.Drain());
    }

    /// <summary>
    /// THE privacy guarantee, in test form. A query string on /trials can carry
    /// a reader's ZIP or their coordinates; storing it would undo the whole
    /// reason this design counts paths and nothing else.
    /// </summary>
    [Fact]
    public async Task TheQueryStringIsNeverRecorded()
    {
        var counter = Counter(_factory);
        counter.Drain();

        await Browser().GetAsync("/trials?zip=90210");

        var recorded = counter.Drain();
        Assert.All(recorded, c =>
        {
            Assert.DoesNotContain("90210", c.Key.Path, StringComparison.Ordinal);
            Assert.DoesNotContain("?", c.Key.Path, StringComparison.Ordinal);
        });
        Assert.Contains(recorded, c => c.Key.Path == "/trials");
    }

    /// <summary>
    /// Draining is what the flusher does every minute. It must hand over
    /// everything exactly once: counting a view twice inflates the number, and
    /// losing one on a swap means the tally quietly drifts low forever.
    /// </summary>
    [Fact]
    public void DrainingTakesEachCountExactlyOnce()
    {
        var counter = new PageViewCounter();
        var day = new DateOnly(2026, 8, 23);

        counter.Record(day, "/a");
        counter.Record(day, "/a");
        counter.Record(day, "/b");

        var first = counter.Drain();
        Assert.Equal(2, first.Single(c => c.Key.Path == "/a").Value);
        Assert.Equal(1, first.Single(c => c.Key.Path == "/b").Value);

        // Everything was taken; a second drain finds nothing.
        Assert.Empty(counter.Drain());

        counter.Record(day, "/c");
        Assert.Single(counter.Drain());
    }

    /// <summary>
    /// Renders the admin page for real, signed in. The counts are only useful
    /// if the panel showing them works, and every other test here stops at the
    /// data — a Razor fault in the new markup would not have shown up in any
    /// of them, because the page 302s to the login screen when anonymous.
    /// </summary>
    [Fact]
    public async Task TheAdminHealthPageShowsTheCounts()
    {
        const string email = "pageview-admin@example.org";
        const string password = "test-admin-password-1234";

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString);
            builder.UseSetting("Admin:Email", email);
            builder.UseSetting("Admin:Password", password);
        });

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<PageViewRepository>();
        var path = $"/seen-{Guid.NewGuid():N}";
        await repository.AddAsync(
            [new((DateOnly.FromDateTime(DateTime.UtcNow), path), 42)], CancellationToken.None);

        var client = factory.CreateClient();
        var login = await client.GetAsync("/admin/login");
        var token = System.Text.RegularExpressions.Regex.Match(
            await login.Content.ReadAsStringAsync(),
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""").Groups[1].Value;

        await client.PostAsync("/admin/login", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Input.Email", email),
            new KeyValuePair<string, string>("Input.Password", password),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        ]));

        var html = await client.GetStringAsync("/admin/health");

        Assert.Contains("Page views", html);
        Assert.Contains(path, html);
        Assert.Contains("42", html);

        // The framing matters as much as the number: these are openings, and
        // the page must not let anyone read them as visitors.
        Assert.Contains("Not visitors", html);
    }

    /// <summary>
    /// Several instances, or one instance across a restart, must ADD to the
    /// day's total rather than overwrite it or collide on the primary key.
    /// </summary>
    [Fact]
    public async Task WritingTwiceForTheSameDayAddsUp()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<PageViewRepository>();
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var path = $"/test-{Guid.NewGuid():N}";

        await repository.AddAsync([new(( day, path), 3)], CancellationToken.None);
        await repository.AddAsync([new((day, path), 4)], CancellationToken.None);

        var top = await repository.TopPathsAsync(2, 500, CancellationToken.None);
        Assert.Equal(7, top.Single(r => r.Path == path).Views);
    }
}
