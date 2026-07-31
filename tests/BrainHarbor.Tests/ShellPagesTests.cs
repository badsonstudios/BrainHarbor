using System.Net;
using System.Text.RegularExpressions;
using BrainHarbor.ContentCheck;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-107: the hand-written shell pages exist, render through the real
/// pipeline, carry their disclaimers, and pass the readability gate.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ShellPagesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static string ConnectionString => TestDatabase.ConnectionString;

    public ShellPagesTests(WebApplicationFactory<Program> factory)
    {
        // No Content:Root override — these assert the SHIPPED pages.
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString));
    }

    [Theory]
    [Trait("Category", "Database")]
    [InlineData("/about", "Why this site exists")]
    [InlineData("/how-we-write", "The short version")]
    [InlineData("/start", "Get emergency help for these signs")]
    [InlineData("/digest", "What it is")]
    [InlineData("/privacy", "We do not track you")]
    [InlineData("/terms", "This site is information, not care")]
    public async Task ShellPageRendersWithTheSiteChrome(string url, string marker)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(marker, html);
        Assert.Contains("class=\"helpline-band\"", html);   // WI-103 promise
        Assert.Contains("class=\"ai-note\"", html);         // AI transparency
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task MedicalDisclaimerRendersFromTheFrontMatterFlag()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/about");

        Assert.Contains("class=\"disclaimer\"", html);
        Assert.Contains("This is not medical advice.", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TermsCarriesBothMedicalAndLegalDisclaimers()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/terms");

        Assert.Contains("This is not medical advice.", html);
        Assert.Contains("This is not legal advice.", html);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task FooterLinksAllResolve()
    {
        var client = _factory.CreateClient();

        foreach (var url in new[]
                 { "/about", "/how-we-write", "/glossary", "/get-help-now", "/privacy", "/terms" })
        {
            var response = await client.GetAsync(url);
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"{url} returned {response.StatusCode} — footer links must not be dead");
        }
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task HomeEntryHubDoorsAllResolve()
    {
        var client = _factory.CreateClient();

        // /research is still M2 work — the other two doors must be live.
        foreach (var url in new[] { "/start", "/get-help-now" })
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        }
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task StartPageLeadsWithEmergencyRedFlags()
    {
        // A scared reader must not read "take a breath" as permission to
        // wait through a seizure or rising pressure.
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/start");

        Assert.Contains("911", html);
        Assert.Contains("seizure", html);
        Assert.Contains("emergency room", html);
        // Red flags come BEFORE the reassurance.
        Assert.True(
            html.IndexOf("911", StringComparison.Ordinal)
            < html.IndexOf("take a breath", StringComparison.OrdinalIgnoreCase),
            "emergency guidance must appear before the calming copy");
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task NoShellPageLinksToAMissingInternalPage()
    {
        // Known-dead by design until their milestones land: the nav and the
        // home "Browse all research" door (WI-108/PROGRESS.md). Everything
        // else — especially body CTAs — must resolve.
        string[] plannedButNotBuilt = ["/research", "/trials"];

        var client = _factory.CreateClient();
        var pages = new[] { "/", "/about", "/how-we-write", "/start", "/digest", "/privacy", "/terms" };
        var broken = new List<string>();

        foreach (var page in pages)
        {
            var html = await client.GetStringAsync(page);
            foreach (Match match in Regex.Matches(html, "href=\"(/[^\"#?]*)\""))
            {
                var target = match.Groups[1].Value;
                if (target.StartsWith("/css/") || target.StartsWith("/js/") ||
                    plannedButNotBuilt.Contains(target))
                {
                    continue;
                }

                if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
                {
                    broken.Add($"{page} -> {target}");
                }
            }
        }

        Assert.True(broken.Count == 0,
            "shell pages must not link to missing pages:\n" + string.Join("\n", broken.Distinct()));
    }

    [Fact]
    public void UnknownDisclaimerFlagFailsTheGate()
    {
        // A typo must never silently delete a required medical disclaimer.
        var findings = ContentChecker.CheckPage(
            "---\ntitle: Typo\ndisclaimers: [mediacl]\n---\nShort plain words here.",
            "typo.md", DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.Contains(findings, f =>
            f.Level == FindingLevel.Fail && f.Message.Contains("unknown disclaimer flag"));
    }

    [Fact]
    public void EveryShippedPagePassesTheReadabilityGate()
    {
        var root = FindRepoRoot();
        var findings = ContentChecker.CheckAll(
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages"),
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var failures = findings.Where(f => f.Level == FindingLevel.Fail).ToList();
        Assert.True(failures.Count == 0,
            "ContentCheck failures:\n" + string.Join("\n", failures.Select(f => $"{f.File}: {f.Message}")));

        // Six shell pages per the WI-107 acceptance criteria.
        Assert.Equal(6, findings.Count(f => f.Message.StartsWith("reading grade")));
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
