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
    [InlineData("/start", "Get emergency help right away")]
    [InlineData("/digest", "What it is")]
    [InlineData("/privacy", "We do not track you")]
    [InlineData("/terms", "This site is for learning, not for care")]
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

        // The six WI-107 shell pages must each be graded. Named rather than
        // counted: the old assertion pinned the TOTAL at six, so it failed the
        // moment curated content was added (18 tumor pages, WI-412) even though
        // every page passed. A count tells you the number changed; naming tells
        // you whether the page you care about is still being checked.
        string[] shellPages =
            ["about.md", "how-we-write.md", "start.md", "digest.md", "privacy.md", "terms.md"];

        // DEFINED POSITIVELY, AFTER BREAKING TWICE THE OTHER WAY. WI-505 made
        // ContentCheck grade glossary definitions, so "every finding that mentions
        // a grade" stopped being the same set as "every page" and `glossary/` had
        // to be excluded. WI-575 then added a `<page>.md [description]` finding per
        // page and the set doubled, 55 to 104, while nothing was wrong.
        //
        // A set defined by "every finding that looks like a grade" is redefined by
        // every new kind of grade. So it is defined by what it IS: a finding whose
        // File is exactly a page path, with no qualifier appended. A third kind of
        // grade cannot join it by accident.
        var graded = findings
            .Where(f => f.Message.StartsWith("reading grade"))
            .Where(f => !f.File.StartsWith("glossary/", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.File.Contains(" [", StringComparison.Ordinal))
            .Select(f => f.File)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // AND THE DESCRIPTIONS ARE PINNED, not merely excluded. WI-575 made the
        // front-matter `description` graded for the first time — it is the first
        // paragraph a reader meets and four items tripped over nothing being able
        // to see it. Excluding the new findings from the count above without
        // asserting them anywhere would leave that work unguarded, which is the
        // shape §12.21 is written about — a rule stated in one file and not applied in
        // the next. (The first version put that in quotation marks as §12.21's own
        // wording; it is not, and /review round 2 grepped it. §12.19: a quotation in a
        // ruling is a claim.) Every page reports EITHER a grade or a too-short note; a page
        // with no description at all reports neither, and that is the hole.
        var describedPages = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal))
            .Select(f => f.File[..^ContentChecker.DescriptionMarker.Length])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var page in shellPages)
        {
            Assert.Contains(page, graded);
        }

        // And nothing shipped ungraded: every .md under the pages root got a
        // grade, so a new folder cannot quietly escape the gate.
        var pagesRoot = Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages");
        var onDisk = Directory
            .EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(pagesRoot, f).Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // COMPARED AGAINST THE PATH SET, not against its count — which is /review
        // round 1's correction and it matters. Two 55-element subsets of a
        // 55-element universe ARE that universe, so asserting both counts and then
        // asserting the sets are equal proved nothing: the third assertion was
        // implied by the first two and could never fail. The comment claimed the
        // opposite ("two sets of 55 can be 55 different things twice"), which is
        // false for sets keyed by paths from one walk.
        //
        // Set difference against the DIRECTORY is load-bearing: it names the page
        // that is missing instead of reporting that a number moved.
        Assert.Empty(onDisk.Except(graded, StringComparer.OrdinalIgnoreCase));
        Assert.Empty(graded.Except(onDisk, StringComparer.OrdinalIgnoreCase));

        // And the same for the descriptions, which is the property WI-575 added: a
        // page whose first paragraph is ungraded is the hole this item closed.
        Assert.Empty(onDisk.Except(describedPages, StringComparer.OrdinalIgnoreCase));
        Assert.Empty(describedPages.Except(onDisk, StringComparer.OrdinalIgnoreCase));

        // AND NOT ONE OF THEM IS THE BLANK-DESCRIPTION WARNING. /review round 4 asked
        // ContentCheck to warn when a `description` is empty, which closed a false
        // positive in the ratchet — and made a page with NO description still produce a
        // description finding, so the set-difference above was satisfied by the warning
        // and two break mutations that delete a description walked straight through.
        //
        // **A fix that makes a defect reportable can make it unasserted.** The
        // property is that every page HAS an opening paragraph, not merely that every
        // page produced some finding about one.
        var blank = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal)
                        && f.Message.StartsWith(
                            ContentChecker.NoDescriptionPrefix, StringComparison.Ordinal))
            .Select(f => f.File)
            .ToList();

        Assert.True(blank.Count == 0,
            "a shipped page has no front-matter `description`. ContentPage.cshtml renders "
            + "it as the first paragraph a reader meets and SearchPages renders it again "
            + "as the blurb under every hit, so a page without one opens on nothing:\n  "
            + string.Join("\n  ", blank));
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
