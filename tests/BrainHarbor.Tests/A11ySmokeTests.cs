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

    /// <summary>
    /// WI-440: the phone layout, including the collapsed nav OPEN.
    ///
    /// Every other scan here runs at the default desktop viewport, where the
    /// hamburger is display:none — so the menu button and its panel were real,
    /// reader-facing UI that axe had never once looked at. A whole layout
    /// existing outside the accessibility gate is the gap this closes.
    /// </summary>
    [Fact]
    public async Task ThePhoneLayoutAndItsOpenMenuHaveNoSeriousOrCriticalAxeViolations()
    {
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            IsMobile = true,
            HasTouch = true,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(_factory.ServerAddress);

        // Guard against a false green: if the breakpoint moved and the toggle
        // never rendered, this would be scanning the desktop layout twice and
        // reporting it as mobile coverage.
        Assert.True(await page.Locator(".nav-collapse__toggle").IsVisibleAsync(),
            "the menu toggle should be visible at 390px");

        await AssertNoSeriousViolations(page, "/ (390px, menu closed)");

        await page.Locator(".nav-collapse__toggle").ClickAsync();
        Assert.True(await page.Locator(".site-nav a[href='/research']").IsVisibleAsync(),
            "opening the menu should reveal the nav links");

        await AssertNoSeriousViolations(page, "/ (390px, menu open)");

        // The two form-heavy pages at the same width. Filter controls are where
        // a narrow layout usually comes apart — on /research the wrapped row
        // separated every label from its control ("Kind [Everything] Sort by"
        // on one line, "[Newest first]" on the next), which axe cannot see but
        // which makes the form unusable. The scan is here for the things axe
        // CAN see; the stacking itself is held by the CSS grid.
        foreach (var path in new[] { "/research", "/trials" })
        {
            var response = await page.GotoAsync(_factory.ServerAddress + path);
            Assert.True(response!.Ok, $"Expected 2xx from {path}, got {response.Status}");
            await AssertNoSeriousViolations(page, $"{path} (390px)");
        }

        await context.DisposeAsync();
    }

    /// <summary>
    /// PLAN.md §3 is "always one tap to a human". The collapsed nav must never
    /// swallow the crisis route: Get Help Now stays visible WITHOUT opening the
    /// menu, at every width. This is a design decision worth pinning, because
    /// tidying it into the menu alongside the other links is the obvious-looking
    /// change for anyone who does not know why it is outside.
    /// </summary>
    [Fact]
    public async Task TheCrisisRouteIsNeverHiddenBehindTheMobileMenu()
    {
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            IsMobile = true,
            HasTouch = true,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(_factory.ServerAddress);

        // Menu still shut.
        Assert.False(await page.Locator(".nav-collapse[open]").CountAsync() > 0);

        var cta = page.Locator(".nav-cta");
        Assert.True(await cta.IsVisibleAsync(), "Get Help Now must be visible with the menu closed");
        Assert.Equal("/get-help-now", await cta.GetAttributeAsync("href"));

        // And the helpline band's phone number, likewise always present.
        Assert.True(await page.Locator(".helpline-band a.tel").IsVisibleAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task TrialFinderHasNoSeriousOrCriticalAxeViolations()
    {
        // WI-403: a page of form controls, filters and lists — the shape most
        // likely to strand a keyboard or screen-reader user, and the page a
        // frightened reader is most likely to be on.
        var page = await _browser!.NewPageAsync();
        var response = await page.GotoAsync(_factory.ServerAddress + "/trials");

        Assert.True(response!.Ok, $"Expected 2xx from /trials, got {response.Status}");
        Assert.True(await page.Locator("#zip").CountAsync() > 0);

        await AssertNoSeriousViolations(page, "/trials");
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

    /// <summary>
    /// WI-503: the reader-choice gate, at both widths and in BOTH states. An
    /// open disclosure is different markup from a closed one, which is the gap
    /// WI-440 found with the mobile menu — the panel axe had never looked at.
    ///
    /// Axe needs JavaScript to run, so the no-script promise is checked
    /// separately below rather than folded in here.
    /// </summary>
    [Theory]
    [InlineData(1280, 900, "desktop")]
    [InlineData(390, 844, "390px")]
    public async Task TheReaderChoiceGateHasNoSeriousOrCriticalAxeViolations(
        int width, int height, string label)
    {
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
        });

        // try/finally, unlike the older tests in this file: a failing
        // assertion would otherwise leak a browser context for the rest of the
        // run, and this file now opens three more of them than it used to.
        try
        {
            var page = await context.NewPageAsync();
            var response = await page.GotoAsync(_factory.ServerAddress + "/dev/styleguide");

            // Guard against a false green: a 404 would scan the error page.
            Assert.True(response!.Ok, $"Expected 2xx from /dev/styleguide, got {response.Status}");
            Assert.True(await page.Locator(".reader-gate__disclosure").CountAsync() > 0,
                "the style guide should render a reader-choice gate");

            await AssertNoSeriousViolations(page, $"/dev/styleguide gate closed ({label})");

            await page.Locator(".reader-gate__toggle").ClickAsync();
            Assert.True(await page.Locator(".reader-gate__body").IsVisibleAsync(),
                $"the toggle should reveal the gated content ({label})");

            await AssertNoSeriousViolations(page, $"/dev/styleguide gate open ({label})");
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    /// <summary>
    /// WI-503: the gate is a &lt;details&gt; precisely so it survives with no
    /// script, like the hamburger (WI-440) and the country picker (WI-457).
    /// A component that needs JavaScript would fail the reader it exists for.
    /// </summary>
    [Fact]
    public async Task TheReaderChoiceGateOpensAndClosesWithJavaScriptDisabled()
    {
        var context = await _browser!.NewContextAsync(new() { JavaScriptEnabled = false });

        try
        {
            var page = await context.NewPageAsync();
            var response = await page.GotoAsync(_factory.ServerAddress + "/dev/styleguide");
            Assert.True(response!.Ok, $"Expected 2xx from /dev/styleguide, got {response.Status}");

            // Closed on load is the item. Checked as rendered visibility, not
            // as the attribute — CSS could re-open what the markup left shut.
            Assert.Equal(0, await page.Locator(".reader-gate__disclosure[open]").CountAsync());
            Assert.False(await page.Locator(".reader-gate__body").IsVisibleAsync(),
                "the gated content must be hidden on load");
            Assert.True(await page.Locator(".reader-gate__warning").IsVisibleAsync(),
                "the warning must be visible before the choice");
            Assert.True(await page.Locator(".reader-gate__show").IsVisibleAsync());
            Assert.False(await page.Locator(".reader-gate__hide").IsVisibleAsync());

            await page.Locator(".reader-gate__toggle").ClickAsync();

            Assert.True(await page.Locator(".reader-gate__body").IsVisibleAsync(),
                "clicking the toggle must reveal the gated content with JavaScript off");

            // And the label has to stop lying: "Show" is wrong once it is on screen.
            Assert.True(await page.Locator(".reader-gate__hide").IsVisibleAsync());
            Assert.False(await page.Locator(".reader-gate__show").IsVisibleAsync());

            await page.Locator(".reader-gate__toggle").ClickAsync();
            Assert.False(await page.Locator(".reader-gate__body").IsVisibleAsync(),
                "a reader must be able to put it away again");
        }
        finally
        {
            await context.DisposeAsync();
        }
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
