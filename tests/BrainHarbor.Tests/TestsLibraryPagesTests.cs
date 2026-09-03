using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// Shared reading helpers for the 29 P5 library pages (content-pipeline §12.8).
/// WI-506 wrote these inside its own test class; WI-507 is the second page, so
/// they move here rather than being copied 28 times.
/// </summary>
internal static class CuratedPage
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

    public static string Read(params string[] pathUnderPages) => File.ReadAllText(Path.Combine(
        [RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages", .. pathUnderPages]));

    private static string ContentRoot =>
        Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content");

    private static string PagesRoot => Path.Combine(ContentRoot, "pages");

    /// <summary>
    /// Every curated page AND every shared block, as (slug, raw text), for
    /// rules that hold site-wide rather than page by page.
    ///
    /// Blocks are in here deliberately. A block is composed INTO the page at
    /// render time (§3a), so text that lands in `Content/blocks/caregiver.md`
    /// is text on eighteen tumor hubs — and a site-wide rule that reads only
    /// `pages/` is blind to the one file with the widest blast radius.
    /// </summary>
    public static IEnumerable<(string Slug, string Text)> AllPages() =>
        new[] { PagesRoot, Path.Combine(ContentRoot, "blocks") }
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
                .Select(f => (
                    Slug: Path.ChangeExtension(Path.GetRelativePath(ContentRoot, f), null)!.Replace('\\', '/'),
                    Text: File.ReadAllText(f))));

    /// <summary>
    /// The phrasings that turn "here is what the lab measured" into "here is
    /// whether that is good news". Shared, because 29 library pages inherit the
    /// rule and WI-509 is where it will be hardest to hold: the sources say
    /// these things out loud (Johns Hopkins' own glossary calls IDH mutation
    /// "associated with a better prognosis"), so borrowed phrasing is how it
    /// gets onto a page, not a decision anyone makes.
    /// </summary>
    public static readonly string[] Characterisations =
    [
        "better outlook", "worse outlook", "better outcome", "worse outcome",
        "better prognosis", "worse prognosis", "favorable", "favourable",
        "good news", "bad news", "more aggressive", "less aggressive",
        "responds better", "respond better", "responds well", "does better",
        "the good one", "the bad one",
    ];

    /// <summary>
    /// Every in-site link inside the page's &lt;article&gt; resolves, and the
    /// "Where to go next" list still offers the doors it is supposed to.
    ///
    /// Shared rather than copied because §12.8's own rule — factor at the
    /// SECOND use, not the fifth — applies to the tests as much as the prose,
    /// and the three copies this replaces had already drifted apart. The
    /// scoping is the load-bearing part: the layout contributes ~13 links, so a
    /// whole-page count can never fall to zero, and a canary counted over the
    /// whole article is satisfied by the body's own links (WI-506 and WI-508
    /// both found this the hard way).
    /// </summary>
    public static async Task AssertLinksResolve(
        HttpClient client, string url, params string[] requiredOnward)
    {
        var html = await client.GetStringAsync(url);

        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"{url} did not render an article");

        var broken = new List<string>();
        var checkedLinks = 0;

        foreach (Match match in Regex.Matches(html[start..end], "href=\"(/[^\"#?]*)\""))
        {
            var target = match.Groups[1].Value;
            if (target.StartsWith("/css/") || target.StartsWith("/js/"))
            {
                continue;
            }

            checkedLinks++;
            if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
            {
                broken.Add(target);
            }
        }

        Assert.True(broken.Count == 0, $"{url} links to:\n" + string.Join("\n", broken.Distinct()));

        var next = html.IndexOf("id=\"where-to-go-next\"", StringComparison.Ordinal);
        Assert.True(next > 0, $"{url} has no 'Where to go next' section");

        var onward = Regex.Matches(html[next..end], "href=\"(/[^\"#?]*)\"")
            .Select(m => m.Groups[1].Value).ToList();

        // Named, not counted. A floor is met by any set of links, so the door
        // that actually matters can go while the count stays healthy — and for
        // pages written as a deliberate sequence, the hand-off to the next page
        // IS the content.
        foreach (var required in requiredOnward)
        {
            Assert.Contains(required, onward);
        }

        Assert.True(onward.Count >= requiredOnward.Length,
            $"'Where to go next' on {url} offers {onward.Count} doors out");

        // Deliberately NOT asserting that the body links anywhere outside this
        // section. The WI-508 page happens to (it deep-links into the wait
        // page), and generalising that broke the MRI page, whose every link is
        // legitimately in "Where to go next" — a rule that fails a correct page
        // is worse than no rule. Where a mid-body link IS load-bearing, the
        // page's own tests name it.
        Assert.True(checkedLinks >= onward.Count, $"{url}: fewer links checked than found");
    }

    /// <summary>Whitespace-normalised: the source is hard-wrapped, and a sentence that happens to break across lines is not a content change.</summary>
    public static string Flatten(string page) => Regex.Replace(page, @"\s+", " ");

    /// <summary>
    /// One "## " section, flattened. Section-scoped assertions are the whole
    /// point: a whole-page Contains() passes when the reassuring line has
    /// drifted into the footer, which is precisely the failure these rules
    /// exist to catch (§12.6 is about WHERE a sentence sits, not whether it is
    /// present somewhere).
    /// </summary>
    public static string Section(string page, string heading)
    {
        var match = Regex.Match(
            page, $@"^## {Regex.Escape(heading)}\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return Regex.Replace(match.Groups[1].Value, @"\s+", " ").Trim();
    }

    /// <summary>Sentences of a section, so "first" and "last" mean something.</summary>
    public static string[] SentencesOf(string section) =>
        [.. Regex.Split(section, @"(?<=[.!?])\s+").Where(s => s.Trim().Length > 0)];

    /// <summary>The body, with the YAML front matter removed.</summary>
    public static string Body(string page) => page[(page.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];

    /// <summary>The YAML front matter, without the body.</summary>
    public static string FrontMatter(string page) => page[..page.IndexOf("\n---", 3, StringComparison.Ordinal)];
}

/// <summary>
/// WI-506: T1 MRI, the first page of the tests library and the page that sets
/// the library-page template (content-pipeline §12.8).
///
/// The properties pinned here are the ones that would rot silently. The prose
/// is not tested — it is reviewed. What IS tested is the small number of places
/// where this page could give a reader actively wrong advice, and the shape the
/// other 28 library pages inherit from it.
/// </summary>
public sealed class MriPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "mri.md");

    private static string Flat => CuratedPage.Flatten(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string[] SentencesOf(string section) => CuratedPage.SentencesOf(section);

    [Fact]
    public void TheClaustrophobiaSectionNeverSendsTheReaderToAskForAnOpenScanner()
    {
        // The one place this page could do real harm. Open and upright scanners
        // are generally lower field strength and are NOT equivalent for brain
        // tumor protocol imaging, so "ask for an open MRI" is advice that can
        // cost a reader the picture their treatment is planned from. The page
        // says "ask what your centre has" and lets the centre choose, which is
        // what the source supports (RadiologyInfo: some centres have systems
        // that are "less confining or more open").
        Assert.DoesNotContain("open MRI", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("upright", Flat, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheClaustrophobiaSectionOffersSomethingAndDoesNotEndInDefeat()
    {
        // §12.6: never end a section on a frightening sentence. Scoped to the
        // section and to its LAST sentence, because "the reassurance exists
        // somewhere on the page" is not the property — a reader who stops
        // reading at the end of this section has to stop on something solid.
        var section = Section("If small spaces frighten you");
        var sentences = SentencesOf(section);

        Assert.Contains("calming medicine", section);            // ask in advance
        Assert.Contains("come into the room with you", section); // bring a person
        Assert.DoesNotContain("cannot have an MRI", section);
        Assert.DoesNotContain("unable to have", section);

        Assert.Contains("almost always possible", string.Join(" ", sentences[^3..]));
    }

    [Fact]
    public void TheDeviceSectionLeadsWithBringingTheCardRatherThanWithBeingTurnedAway()
    {
        // Under-communicated and worth a section: many newer pacemakers, ICDs,
        // stimulators, cochlear implants and pumps ARE acceptable for MRI
        // provided staff know the exact make and model. Asserting only that the
        // card advice appears would pass with it buried under three paragraphs
        // of exclusions, which is the exact page this test exists to prevent —
        // so it has to be in the FIRST sentence (§12.6, answer first).
        var section = Section("Metal, implants and your device card");

        Assert.Contains("card", SentencesOf(section)[0]);
        Assert.Contains("exact make and model", section);
    }

    [Fact]
    public void TheGadoliniumRetentionAnswerIsHonestInBothDirections()
    {
        // The question people actually type. Omitting it looks like hiding;
        // overstating it frightens someone into refusing a scan they need. The
        // source says both halves, so the page says both halves.
        var flat = Flat;

        Assert.Contains("Small amounts can stay", flat);
        Assert.Contains("no known health effects", flat);
    }

    [Fact]
    public void ThePageExplainsWhyASecondScanIsNotABadSign()
    {
        // The item's whole reason for covering the planning and post-op scans:
        // "I just had an MRI, why another one?" is a genuinely frightening
        // moment that has a mundane answer.
        var flat = Flat;

        Assert.Contains("Why am I having another scan?", flat);
        Assert.Contains("navigation scan", flat);
        Assert.Contains("is not a sign that the first was wrong", flat);
    }

    [Fact]
    public void ThePageSaysAScanAloneCannotNameTheTumor()
    {
        // ACS is explicit that imaging can suggest a type but only tissue
        // confirms it. A reader who does not know this reads the wait for
        // pathology (WI-507) as their team stalling.
        Assert.Contains("Only a piece of the tumor", Flat);
    }

    [Fact]
    public void ThePageCarriesNoRiskPercentageAndNoDoseFigure()
    {
        // §12.4 R2: procedural risk percentages are qualitative, because the
        // source spread is too wide to state one honestly. Tesla and millilitre
        // figures are the same class of number as Gy and mg — they belong in a
        // protocol, not on a patient page.
        var body = Page[(Page.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", body);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*(Tesla|T\b|mg|ml|mmol)", RegexOptions.IgnoreCase), body);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. A test page has no prognosis on
        // it, so a fence here would mean something has drifted.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        // Shared contract item 7, and the last two sections of the §12.8
        // library-page template. Sources and "last reviewed" render from front
        // matter, so the page does not hand-write a provenance section.
        // The .Trim() is load-bearing on Windows, not cosmetic: `.` matches
        // `\r`, so on a CRLF checkout the capture ends with one. If you copy
        // this method to another library page, keep it.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
        Assert.Equal("The short version", headings[0]);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. Checked here rather than trusting ContentCheck's
        // warning level: a missing `accessed` date is only a warning there, and
        // this phase's whole source discipline rests on it.
        var front = Page[..Page.IndexOf("\n---", 3, StringComparison.Ordinal)];

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains the page's claims actually rest on, rather than a count
        // that half the source set could vanish beneath. RadiologyInfo carries
        // the safety, dye and experience content; ACS the "only tissue names
        // it" and the pre-surgery mapping scan; NBTS the tumor board; the PMC
        // paper the post-op baseline.
        foreach (var domain in new[] { "radiologyinfo.org", "cancer.org", "braintumor.org", "ncbi.nlm.nih.gov" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }
        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true, so an anchored version passes on
        // LF and fails the moment the file round-trips through git (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The page as served, and the door that leads to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class MriPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MriPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync("/tests/mri");

        Assert.Contains("Your MRI scan", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromWhereANewlyDiagnosedReaderStarts()
    {
        // The WI-412 orphan lesson. /tests has no index yet, and the tumor hubs
        // do not link into the library until WI-513, so /start is currently the
        // page's only door. If that link goes, the page is invisible.
        var html = await _factory.CreateClient().GetStringAsync("/start");

        Assert.Contains("/tests/mri", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors named here are the ones this page owes its reader: the two
        // pathology pages, because an MRI cannot name a tumor and only tissue
        // can, and a person to talk to.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), "/tests/mri",
            "/tests/waiting-for-results", "/tests/pathology-report", "/glossary");

    [Fact]
    public async Task TheNewVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9: new words join the glossary so the tooltip fires
        // site-wide. Asserted on the rendered page rather than on the glossary
        // directory, because a term file that nothing matches is a term nobody
        // ever sees.
        var html = await _factory.CreateClient().GetStringAsync("/tests/mri");

        // "def-<slug>" is the popover the tooltip button targets. Asserting the
        // bare word would pass on prose that never got marked up at all —
        // "radiologist" appears in the page either way.
        foreach (var slug in new[] { "tumor-board", "radiologist", "neuronavigation" })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }
}
