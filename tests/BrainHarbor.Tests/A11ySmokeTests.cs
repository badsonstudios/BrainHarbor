using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace BrainHarbor.Tests;

/// <summary>
/// WCAG smoke test (WI-102): axe-core against the real shell in headless
/// Chromium — 0 serious/critical violations allowed. Needs the local Postgres
/// container (app boots DbUp) and Playwright's Chromium; the fixture installs
/// the browser if missing (no-op when cached; CI pre-installs in the
/// workflow). Also proves the large-text toggle end-to-end with JavaScript
/// disabled — the no-JS fallback is an acceptance criterion, not a nicety.
/// </summary>
[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class A11ySmokeTests : IClassFixture<KestrelWebApplicationFactory>, IAsyncLifetime
{
    private readonly KestrelWebApplicationFactory _factory;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public A11ySmokeTests(KestrelWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright browser install failed (exit {exitCode}). " +
                "Run: pwsh tests/BrainHarbor.Tests/bin/Debug/net10.0/playwright.ps1 install chromium");
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _factory.EnsureServer();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task HomePageHasNoSeriousOrCriticalAxeViolations()
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(_factory.ServerAddress);

        await AssertNoSeriousViolations(page, "/");
    }

    [Fact]
    public async Task HomePageInLargeTextModeHasNoSeriousOrCriticalAxeViolations()
    {
        // Large-text mode reflows everything — scan it separately.
        var context = await _browser!.NewContextAsync();
        await context.AddCookiesAsync([new Cookie
        {
            Name = "bh_textsize",
            Value = "large",
            Url = _factory.ServerAddress,
        }]);
        var page = await context.NewPageAsync();
        await page.GotoAsync(_factory.ServerAddress);

        Assert.Equal(1, await page.Locator("html.text-large").CountAsync());
        await AssertNoSeriousViolations(page, "/ (large text)");

        await context.DisposeAsync();
    }

    [Fact]
    public async Task StyleGuideWithAllBadgeKindsHasNoSeriousOrCriticalAxeViolations()
    {
        // Every stage badge + card renders here — this is the a11y gate for
        // the WI-109 components before real data exists.
        var page = await _browser!.NewPageAsync();
        var response = await page.GotoAsync(_factory.ServerAddress + "/dev/styleguide");

        // Guard against a false green: a 404 here would make axe scan the
        // friendly error page instead of the badge components.
        Assert.True(response!.Ok, $"Expected 2xx from /dev/styleguide, got {response.Status}");
        Assert.True(await page.Locator(".badge--result").CountAsync() > 0);

        await AssertNoSeriousViolations(page, "/dev/styleguide");
    }

    private static async Task AssertNoSeriousViolations(IPage page, string label)
    {
        var results = await page.RunAxe();
        var bad = results.Violations
            .Where(v => v.Impact is "serious" or "critical")
            .Select(v => $"{v.Impact}: {v.Id} — {v.Help} ({v.Nodes.Length} nodes)")
            .ToList();

        Assert.True(bad.Count == 0,
            $"axe-core violations on {label}:\n" + string.Join("\n", bad));
    }

    [Fact]
    public async Task LargeTextToggleWorksWithJavaScriptDisabled()
    {
        var context = await _browser!.NewContextAsync(new() { JavaScriptEnabled = false });
        var page = await context.NewPageAsync();
        await page.GotoAsync(_factory.ServerAddress);

        Assert.False(await page.Locator("html.text-large").CountAsync() > 0);

        await page.GetByRole(AriaRole.Link, new() { Name = "Larger text" }).ClickAsync();
        Assert.Equal(1, await page.Locator("html.text-large").CountAsync());

        // The preference must survive navigation (cookie, not querystring).
        await page.GotoAsync(_factory.ServerAddress);
        Assert.Equal(1, await page.Locator("html.text-large").CountAsync());

        await page.GetByRole(AriaRole.Link, new() { Name = "Standard text" }).ClickAsync();
        Assert.Equal(0, await page.Locator("html.text-large").CountAsync());

        await context.DisposeAsync();
    }
}
