using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-559 and WI-560: the two seizure pages, written together because they are
/// one subject split by whether it is happening right now.
///
/// WI-559 is the only page on this site a reader may be ACTING from rather than
/// reading, so its format is a safety feature and is tested like one: the steps
/// are a real ordered list (they survive the stylesheet dying), they come
/// first, and nothing on the page sits behind a disclosure the reader has to
/// find and click while someone is on the floor.
///
/// WI-560's hard rule is the driving one, and it is checked across every
/// curated page rather than just this one: US seizure-free periods vary by
/// state, so a duration printed on a page is wrong for most of the people
/// reading it. Explain the shape, link the state tool, print no number.
/// </summary>
public sealed class SeizureContentTests
{
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

    private static string PagesRoot => Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages");

    private static string Read(string relative) =>
        File.ReadAllText(Path.Combine(PagesRoot, relative.Replace('/', Path.DirectorySeparatorChar) + ".md"));

    private static IEnumerable<(string Slug, string Text)> AllPages() =>
        Directory.EnumerateFiles(PagesRoot, "*.md", SearchOption.AllDirectories)
            .Select(f => (
                Path.GetRelativePath(PagesRoot, f).Replace('\\', '/')[..^3],
                File.ReadAllText(f)));

    // "six months", "12 months", "one year", "2 years" — the shapes a waiting
    // period actually gets written in. Spelled-out numbers matter as much as
    // digits: "six months" is the phrasing a drafting session reaches for.
    private static readonly Regex Duration = new(
        @"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|twelve|eighteen)[\s-]+(week|month|year)s?\b",
        RegexOptions.IgnoreCase);

    [Fact]
    public void NoCuratedPagePrintsADrivingWaitingPeriod()
    {
        // Checked per sentence rather than per page: a page may legitimately
        // say "six weeks of radiation" and also mention driving.
        var offenders = new List<string>();
        var scanned = 0;

        foreach (var (slug, text) in AllPages())
        {
            foreach (var sentence in Regex.Split(text, @"(?<=[.!?])\s+"))
            {
                if (!sentence.Contains("driv", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                scanned++;
                if (Duration.IsMatch(sentence))
                {
                    offenders.Add($"{slug}: {sentence.Trim()}");
                }
            }
        }

        // Without this the test passes just as happily on a corpus that says
        // nothing about driving at all, which is the state it is meant to
        // protect us from drifting out of.
        Assert.True(scanned > 0, "no page mentioned driving — did the driving section get lost?");
        Assert.True(offenders.Count == 0,
            "driving rules are jurisdictional and no page may print a duration "
            + "(content-pipeline §12.3 section 9):\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void PrintKeepsGlossaryTermsAsWords()
    {
        // A glossary term renders as a <button> so its definition can pop over
        // with no JavaScript (WI-105). print.css hides every button as
        // interactive chrome, which silently DELETED those words from every
        // printed page — "if they have a rescue medicine" printed as "if they
        // have a". Found on this page, which is the one people print.
        var css = File.ReadAllText(Path.Combine(
            RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "css", "print.css"));

        Assert.Matches(@"button\.term\s*\{[^}]*display:\s*inline\s*!important", css);
    }

    [Fact]
    public void TheDrivingSectionSendsTheReaderToTheStateTool()
    {
        // Having removed the number, the page owes the reader the way to find
        // their own. Without this, "we do not print it here" is just a gap.
        var text = Read("seizures/living-with");

        Assert.Contains("epilepsy.com/lifestyle/driving-and-transportation/laws", text);
        Assert.Contains("ask your care team", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheEmergencyPageIsNotUSOnlyAboutCallingForHelp()
    {
        // WI-455/457 deliberately put non-US readers on this site. A page that
        // says only "call 911" tells a reader in Finland nothing.
        var text = Read("seizures/what-to-do");

        Assert.Contains("911", text);
        Assert.Contains("local emergency number", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheEmergencyPageStartsWithTheSteps()
    {
        // Not an aesthetic preference. A reader on this page may be standing
        // over someone; anything above the steps is something they have to
        // read past first.
        var text = Read("seizures/what-to-do");
        var body = text[(text.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];
        var firstHeading = Regex.Match(body, @"^## .*$", RegexOptions.Multiline);

        Assert.True(firstHeading.Success, "the page has no headings");
        Assert.Equal("## Do this now", firstHeading.Value.Trim());
        Assert.True(body[..firstHeading.Index].Trim().Length == 0,
            "nothing may come before the steps: " + body[..firstHeading.Index].Trim());
    }

    [Fact]
    public void TheEmergencyPageCoversWhatTheSourcesSayToDoAndNotDo()
    {
        // The specific items are here because leaving any one of them out is a
        // real harm, not a gap: the folk advice ("hold them down", "something
        // in the mouth") is actively dangerous, and the ambulance triggers are
        // the whole reason a bystander reads this page.
        var text = Read("seizures/what-to-do").ToLowerInvariant();

        foreach (var required in new[]
        {
            "longer than 5 minutes",   // Epilepsy Foundation + CDC
            "in water",
            "first ever",
            "on their side",
            "do not put anything in their mouth",
            "do not hold them down",
        })
        {
            Assert.Contains(required, text);
        }
    }

    [Fact]
    public void NeitherSeizurePageHidesAnythingBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. Steps a frightened person needs
        // must never be one click away from invisible.
        foreach (var slug in new[] { "seizures/what-to-do", "seizures/living-with" })
        {
            Assert.DoesNotContain(":::", Read(slug));
        }
    }

    [Fact]
    public void TheTwoPagesPointAtEachOther()
    {
        // They are one subject split by whether it is happening right now, so
        // a reader who lands on the wrong one has to be able to get to the
        // other in a single obvious step.
        Assert.Contains("/seizures/living-with", Read("seizures/what-to-do"));
        Assert.Contains("/seizures/what-to-do", Read("seizures/living-with"));
    }

    [Fact]
    public void LivingWithSeizuresHoldsBothSidesOfTheEvidence()
    {
        // The finding this page exists to carry cuts both ways: most
        // restriction is not evidence-based, AND exertion-triggered seizures
        // are real for a minority. A page that drops the second half tells the
        // reader who has observed it in herself that she is wrong about her
        // own body.
        // Whitespace-normalised: the source is hard-wrapped, and a sentence
        // that happens to break across lines is not a content change.
        var text = Regex.Replace(Read("seizures/living-with"), @"\s+", " ");

        Assert.Contains("told to do less than they need to", text);
        Assert.Contains("you are not imagining it", text);
    }
}

/// <summary>The same pages as served, plus the links other pages now make to them.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class SeizurePagesRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SeizurePagesRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Theory]
    [InlineData("/seizures/what-to-do", "What to do when someone has a seizure")]
    [InlineData("/seizures/living-with", "Living with seizures")]
    public async Task ThePageIsServed(string url, string title)
    {
        var html = await _factory.CreateClient().GetStringAsync(url);

        Assert.Contains(title, html);
    }

    [Fact]
    public async Task TheStepsAreARealOrderedList()
    {
        // The site has to work with the stylesheet dead (a standing
        // constraint). Numbered steps typed as paragraphs lose their order the
        // moment CSS does not load, and order is the whole content here.
        var html = await _factory.CreateClient().GetStringAsync("/seizures/what-to-do");

        var steps = html[html.IndexOf("Do this now", StringComparison.Ordinal)..];
        Assert.StartsWith("<ol>", steps[steps.IndexOf("<ol", StringComparison.Ordinal)..]);
        Assert.True(Regex.Matches(steps[..steps.IndexOf("</ol>", StringComparison.Ordinal)], "<li>").Count >= 5,
            "the steps must be a list of at least five items");
    }

    [Fact]
    public async Task NothingOnTheEmergencyPageIsInsideADisclosure()
    {
        // The layout's mobile nav is a <details>; the ARTICLE must not have
        // one. Scoped to the article for exactly that reason.
        var html = await _factory.CreateClient().GetStringAsync("/seizures/what-to-do");
        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, "the page did not render an article");
        Assert.DoesNotContain("<details", html[start..end]);
    }

    [Fact]
    public async Task TheSeizurePagesAreReachableFromWhereAFrightenedReaderStarts()
    {
        // A page nothing links to is invisible (the WI-412 orphan lesson).
        // These are the three doors: the caregiver block on every tumor hub,
        // the newly-diagnosed page, and the crisis page.
        var client = _factory.CreateClient();

        foreach (var from in new[] { "/tumors/low-grade-glioma", "/start", "/get-help-now" })
        {
            var html = await client.GetStringAsync(from);
            Assert.Contains("/seizures/what-to-do", html);
        }
    }

    [Fact]
    public async Task EveryLinkOnBothSeizurePagesResolves()
    {
        var client = _factory.CreateClient();
        var broken = new List<string>();

        foreach (var page in new[] { "/seizures/what-to-do", "/seizures/living-with" })
        {
            var html = await client.GetStringAsync(page);
            foreach (Match match in Regex.Matches(html, "href=\"(/[^\"#?]*)\""))
            {
                var target = match.Groups[1].Value;
                if (target.StartsWith("/css/") || target.StartsWith("/js/"))
                {
                    continue;
                }

                if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
                {
                    broken.Add($"{page} -> {target}");
                }
            }
        }

        Assert.True(broken.Count == 0, string.Join("\n", broken.Distinct()));
    }

    [Fact]
    public async Task TheEmergencyPageIsInTheSitemap()
    {
        // It is a search-box page: people type "what to do when someone has a
        // seizure" while it is happening. /seizures/living-with is reached by
        // following the link from it, the same way the individual tumor types
        // are reached from /tumors.
        var xml = await _factory.CreateClient().GetStringAsync("/sitemap.xml");

        Assert.Contains("/seizures/what-to-do", xml);
    }
}
