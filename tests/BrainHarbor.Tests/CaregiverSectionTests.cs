using System.Net;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-558: the caregiver section, which content-pipeline §12.2 item 11 makes
/// mandatory on every tumor hub and §12.7 says how to build.
///
/// The shared half is one block, so the properties worth pinning are the ones
/// a hand-edited set of 18 files loses silently: a hub that never got the
/// include (the reader who needs it most sees nothing, and no gate notices),
/// a citation written from memory (WI-505 shipped nine of those), and the
/// section drifting behind a reader-choice gate, which would hide the "when to
/// call an ambulance" paragraph behind a click nobody makes.
/// </summary>
public sealed class CaregiverSectionTests
{
    private const string Heading = "## For the person caring for someone with this";
    private const string Directive = "[CAREGIVER]";

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BrainHarbor.slnx")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName
            ?? throw new InvalidOperationException("could not find the repo root from the test output directory");
    }

    private static string ContentRoot => Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content");
    private static string TumorsRoot => Path.Combine(ContentRoot, "pages", "tumors");

    private static ContentBlock CaregiverBlock() =>
        ContentBlockStore.Load(Path.Combine(ContentRoot, "blocks")).Blocks["caregiver"];

    private static IEnumerable<string> TumorHubs() =>
        Directory.EnumerateFiles(TumorsRoot, "*.md").OrderBy(f => f, StringComparer.Ordinal);

    [Fact]
    public void EveryTumorHubIncludesTheCaregiverBlock()
    {
        var missing = TumorHubs()
            .Where(f => !ContentBlocks.DirectBlockNames(File.ReadAllText(f)).Contains("caregiver"))
            .Select(Path.GetFileName)
            .ToList();

        Assert.True(missing.Count == 0,
            "every tumor hub carries a caregiver section (content-pipeline §12.2 item 11); missing from: "
            + string.Join(", ", missing));
        Assert.True(TumorHubs().Count() >= 18, "expected the 18 shipped tumor hubs");
    }

    [Fact]
    public void EveryTumorHubUsesTheStandardHeadingAndPutsItBeforeTheQuestions()
    {
        // §12.3 orders the caregiver section at 13 and "questions to ask" at
        // 14. Wording and position are the whole point of a standard: a reader
        // who learns the shape on one hub can find it on the next.
        var wrong = new List<string>();

        foreach (var file in TumorHubs())
        {
            var text = File.ReadAllText(file);
            var heading = text.IndexOf(Heading, StringComparison.Ordinal);
            var directive = text.IndexOf(Directive, StringComparison.Ordinal);
            var questions = text.IndexOf("## What to ask your team", StringComparison.Ordinal);

            if (heading < 0 || directive < heading)
            {
                wrong.Add($"{Path.GetFileName(file)}: block is not under the standard heading");
            }
            else if (questions >= 0 && heading > questions)
            {
                wrong.Add($"{Path.GetFileName(file)}: caregiver section comes after the questions section");
            }
        }

        Assert.True(wrong.Count == 0, string.Join("\n", wrong));
    }

    [Fact]
    public void TheCaregiverBlockCitesRealSources()
    {
        // Same bar as the glossary (WI-505): a citation that RESOLVES to the
        // wrong document is the worst failure mode on a medical site, because
        // the link works and nothing looks broken. This can only check the
        // shape; every URL shipped here was fetched and its title read first.
        var block = CaregiverBlock();

        Assert.NotEmpty(block.Sources);
        var bad = block.Sources
            .Where(s => string.IsNullOrWhiteSpace(s.Title)
                || !Uri.TryCreate(s.Url, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps)
            .Select(s => $"'{s.Url}' is not an https URL with a title");

        Assert.Empty(bad);
    }

    [Fact]
    public void TheBlocksSourcesReachEveryPageThatIncludesIt()
    {
        // The reason the block carries its own sources (§3a): otherwise the
        // citations get copy-pasted into 18 files and the drift this mechanism
        // exists to kill just moves to the source list.
        var blocks = ContentBlockStore.Load(Path.Combine(ContentRoot, "blocks"));
        var expected = blocks.Blocks["caregiver"].Sources.Select(s => s.Url).ToList();

        foreach (var file in TumorHubs())
        {
            var slug = "tumors/" + Path.GetFileNameWithoutExtension(file);
            var page = ContentStore.Parse(File.ReadAllText(file), slug, [], blocks);

            var urls = page.FrontMatter.Sources.Select(s => s.Url).ToList();
            Assert.All(expected, url => Assert.Contains(url, urls));
        }
    }

    [Fact]
    public void TheCaregiverSectionIsNeverBehindAReaderChoiceGate()
    {
        // WI-503's gate is for outlook and nothing else. The block holds "when
        // to call an ambulance"; content a reader has to opt into is content a
        // frightened reader does not read.
        var inside = new List<string>();

        foreach (var file in TumorHubs())
        {
            var open = false;
            foreach (var line in File.ReadAllLines(file))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith(":::", StringComparison.Ordinal))
                {
                    open = trimmed.Length > 3; // ":::outlook" opens, bare ":::" closes
                }
                else if (open && trimmed == Directive)
                {
                    inside.Add(Path.GetFileName(file));
                }
            }
        }

        Assert.True(inside.Count == 0,
            "the caregiver section must not sit inside a reader-choice gate: " + string.Join(", ", inside));
    }

    [Fact]
    public void TheBlockIsWrittenToTheCaregiverNotAboutThem()
    {
        // §12.7: "a section that says 'caregivers often find...' has already
        // failed the person reading it at 2am". The one third-person sentence
        // that is allowed reports a study finding, so this checks the framings
        // that describe the reader from outside rather than banning the word.
        var markdown = CaregiverBlock().Markdown;

        foreach (var phrase in new[] { "caregivers often", "caregivers should", "caregivers may find", "carers often" })
        {
            Assert.DoesNotContain(phrase, markdown, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains(" you ", markdown, StringComparison.Ordinal);
    }
}

/// <summary>
/// The same section, served by the running site rather than read off disk:
/// composition, the shell, and every link the block hands the reader.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class CaregiverSectionRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CaregiverSectionRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ATumorHubRendersTheCaregiverSection()
    {
        var html = await _factory.CreateClient().GetStringAsync("/tumors/glioblastoma");

        Assert.Contains("For the person caring for someone with this", html);
        Assert.Contains("You are allowed to ask questions", html);
        // The directive itself must never survive into the page: a literal
        // "[CAREGIVER]" on screen is the WI-501 CRLF failure, which renders
        // bracket text with no error anywhere.
        Assert.DoesNotContain("[CAREGIVER]", html);
    }

    [Fact]
    public async Task EveryLinkTheCaregiverSectionOffersResolves()
    {
        // The block is the site's one piece of copy that tells a reader to go
        // somewhere while they are frightened. A 404 there is worse than no
        // link. This walks the whole rendered hub, so the links WI-560 adds to
        // the block are covered on arrival without a new test.
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/tumors/glioblastoma");

        var broken = new List<string>();
        foreach (Match match in Regex.Matches(html, "href=\"(/[^\"#?]*)\""))
        {
            var target = match.Groups[1].Value;
            if (target.StartsWith("/css/") || target.StartsWith("/js/"))
            {
                continue;
            }

            if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
            {
                broken.Add(target);
            }
        }

        Assert.True(broken.Count == 0,
            "a tumor hub must not link to a missing page:\n" + string.Join("\n", broken.Distinct()));
    }
}
