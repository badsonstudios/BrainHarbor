using BrainHarbor.ContentCheck;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-501: shared content blocks. The point of the mechanism is that the
/// retired-name crosswalk lives in ONE file, so these tests are mostly about
/// the two ways that promise could quietly break — a stale cache, and a
/// fragment that passes the reading gate alone while the assembled page
/// fails it.
/// </summary>
public sealed class ContentBlocksTests : IDisposable
{
    private readonly string _root;
    private readonly string _pages;
    private readonly string _blocks;
    private readonly ContentStore _store;

    public ContentBlocksTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "bh-blocks-" + Guid.NewGuid().ToString("N"));
        _pages = Path.Combine(_root, "pages");
        _blocks = Path.Combine(_root, "blocks");
        Directory.CreateDirectory(_pages);
        Directory.CreateDirectory(_blocks);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Content:Root"] = _pages,
                ["Glossary:Root"] = Path.Combine(_root, "glossary"),
            })
            .Build();
        var environment = new StubEnvironment();
        _store = new ContentStore(
            environment,
            configuration,
            new GlossaryStore(environment, configuration),
            new ContentBlockStore(environment, configuration));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void WritePage(string slug, string body, string frontMatter = "title: Test page") =>
        File.WriteAllText(Path.Combine(_pages, slug + ".md"), $"---\n{frontMatter}\n---\n{body}");

    private int _writes;

    private void WriteBlock(string name, string body)
    {
        var file = Path.Combine(_blocks, name + ".md");
        File.WriteAllText(file, body);
        // Temp files written in the same tick share an mtime, so the store's
        // directory stamp would not change and the test would pass on a cache
        // hit rather than on the behaviour it claims to check. A counter
        // rather than a clock keeps that deterministic.
        File.SetLastWriteTimeUtc(file, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(++_writes));
    }

    [Fact]
    public void AnIncludedBlockRendersInThePage()
    {
        WriteBlock("crosswalk", "Some old names are still on paperwork.");
        WritePage("glioma", "Before.\n\n[CROSSWALK]\n\nAfter.");

        var html = _store.GetPage("glioma")!.Html;

        Assert.Contains("Some old names are still on paperwork.", html);
        Assert.Contains("Before.", html);
        Assert.Contains("After.", html);
        Assert.DoesNotContain("[CROSSWALK]", html);
    }

    /// <summary>
    /// The whole reason WI-501 exists: WHO moves, the crosswalk changes, and
    /// every page that includes it has to change with it — without a restart
    /// and without touching 24 files.
    /// </summary>
    [Fact]
    public void EditingOneBlockChangesEveryPageThatIncludesIt()
    {
        WriteBlock("crosswalk", "Old wording.");
        WritePage("glioma", "[CROSSWALK]");
        WritePage("meningioma", "[CROSSWALK]");

        Assert.Contains("Old wording.", _store.GetPage("glioma")!.Html);
        Assert.Contains("Old wording.", _store.GetPage("meningioma")!.Html);

        WriteBlock("crosswalk", "Corrected wording.");

        Assert.Contains("Corrected wording.", _store.GetPage("glioma")!.Html);
        Assert.Contains("Corrected wording.", _store.GetPage("meningioma")!.Html);
    }

    [Fact]
    public void AMissingBlockThrowsAndNamesIt()
    {
        WritePage("glioma", "[CROSSWALK]");

        var exception = Assert.Throws<FormatException>(() => _store.GetPage("glioma"));

        Assert.Contains("CROSSWALK", exception.Message);
        Assert.Contains("crosswalk.md", exception.Message);
    }

    [Fact]
    public void AnIncludeCycleThrowsAndNamesTheChain()
    {
        WriteBlock("a", "[B]");
        WriteBlock("b", "[A]");
        WritePage("glioma", "[A]");

        var exception = Assert.Throws<FormatException>(() => _store.GetPage("glioma"));

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a -> b -> a", exception.Message);
    }

    [Fact]
    public void ABlockMayIncludeAnotherBlock()
    {
        WriteBlock("caregiver", "For the person caring for someone.\n\n[SEIZURE]");
        WriteBlock("seizure", "Stay with them. Time it.");
        WritePage("glioma", "[CAREGIVER]");

        var html = _store.GetPage("glioma")!.Html;

        Assert.Contains("For the person caring for someone.", html);
        Assert.Contains("Stay with them. Time it.", html);
    }

    /// <summary>
    /// The directive is a whole line or it is nothing. Markdown links and
    /// bracketed asides are ordinary prose all over the existing pages, and
    /// adding this mechanism must not change how one of them renders.
    /// </summary>
    [Theory]
    [InlineData("See the [CROSSWALK] section below.")]
    [InlineData("A [CROSSWALK](/tumors/names) link.")]
    [InlineData("[crosswalk]")]
    public void TextThatMerelyLooksLikeADirectiveIsLeftAlone(string body)
    {
        WriteBlock("crosswalk", "BLOCK BODY");
        WritePage("glioma", body);

        Assert.DoesNotContain("BLOCK BODY", _store.GetPage("glioma")!.Html);
    }

    [Fact]
    public void SiteSearchFindsWordsThatOnlyExistInABlock()
    {
        WriteBlock("crosswalk", "Oligoastrocytoma is a name nobody uses now.");
        WritePage("glioma", "[CROSSWALK]");

        var matches = _store.SearchPages("oligoastrocytoma", 10);

        Assert.Contains(matches, m => m.UrlPath == "/glioma");
    }

    [Fact]
    public void ABlocksSourcesMergeIntoTheIncludingPagesFrontMatter()
    {
        WriteBlock("crosswalk",
            "---\nsources:\n  - url: https://who.example/cns5\n    title: WHO CNS5\n---\nNames changed in 2021.");
        WritePage("glioma", "[CROSSWALK]",
            "title: Glioma\nsources:\n  - url: https://page.example/one\n    title: Page source");

        var sources = _store.GetPage("glioma")!.FrontMatter.Sources;

        Assert.Contains(sources, s => s.Url == "https://page.example/one");
        Assert.Contains(sources, s => s.Url == "https://who.example/cns5");
    }

    [Fact]
    public void ASourceOnBothTheBlockAndThePageIsNotListedTwice()
    {
        WriteBlock("crosswalk",
            "---\nsources:\n  - url: https://who.example/cns5\n    title: WHO CNS5\n---\nNames changed.");
        WritePage("glioma", "[CROSSWALK]",
            "title: Glioma\nsources:\n  - url: https://who.example/cns5\n    title: WHO CNS5");

        var sources = _store.GetPage("glioma")!.FrontMatter.Sources;

        Assert.Single(sources, s => s.Url == "https://who.example/cns5");
    }

    [Fact]
    public void AnEmptyBlockIsRejectedRatherThanRenderingAnEmptySection()
    {
        WriteBlock("crosswalk", "---\nsources: []\n---\n");
        WritePage("glioma", "[CROSSWALK]");

        var exception = Assert.Throws<FormatException>(() => _store.GetPage("glioma"));

        Assert.Contains("no body", exception.Message);
    }

    /// <summary>
    /// Includes resolve before Markdig parses, so block text is ordinary page
    /// text: a glossary word that appears only inside a block still gets its
    /// tooltip. WI-505 adds ~40 terms for exactly this vocabulary.
    /// </summary>
    [Fact]
    public void AGlossaryTermInsideABlockStillGetsItsTooltip()
    {
        var terms = new List<GlossaryTerm>
        {
            new("mass-effect", "mass effect", [], null, "Pressure from a growth pushing on nearby brain."),
        };
        var blocks = Set(("mechanism", "A tumor can cause mass effect."));

        var page = ContentStore.Parse("---\ntitle: Test\n---\n[MECHANISM]", "glioma", terms, blocks);

        Assert.Contains("class=\"term\"", page.Html);
        Assert.Contains("Pressure from a growth", page.Html);
    }

    /// <summary>
    /// Pins the boundary, not just "deep nesting fails" — with a limit of 5
    /// and a fixture of 8, an off-by-one in MaxDepth passes silently.
    /// </summary>
    [Theory]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void NestingIsAllowedUpToTheLimitAndNoFurther(int blocksInChain, bool shouldThrow)
    {
        for (var level = 0; level < blocksInChain - 1; level++)
        {
            WriteBlock($"level{level}", $"Level {level}.\n\n[LEVEL{level + 1}]");
        }
        WriteBlock($"level{blocksInChain - 1}", "The bottom.");
        WritePage("glioma", "[LEVEL0]");

        if (shouldThrow)
        {
            Assert.Contains("deep", Assert.Throws<FormatException>(() => _store.GetPage("glioma")).Message);
        }
        else
        {
            Assert.Contains("The bottom.", _store.GetPage("glioma")!.Html);
        }
    }

    /// <summary>
    /// core.autocrlf is true on this repo, so a fresh clone hands every
    /// curated page to the parser with CRLF endings. A directive matcher that
    /// only understands LF does not fail — it renders "[CROSSWALK]" as
    /// literal text onto a patient's page, and CI (Linux, LF) stays green
    /// while the site is wrong. Found in review of this item.
    /// </summary>
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void DirectivesResolveWhateverTheLineEndings(string newline)
    {
        WriteBlock("crosswalk", string.Join(newline, "Old names still on paperwork.", "", "A second line."));
        File.WriteAllText(
            Path.Combine(_pages, "glioma.md"),
            string.Join(newline, "---", "title: Glioma", "---", "Before.", "", "[CROSSWALK]", "", "After."));

        var html = _store.GetPage("glioma")!.Html;

        Assert.Contains("Old names still on paperwork.", html);
        Assert.Contains("A second line.", html);
        Assert.DoesNotContain("[CROSSWALK]", html);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void AMissingBlockFailsWhateverTheLineEndings(string newline)
    {
        File.WriteAllText(
            Path.Combine(_pages, "glioma.md"),
            string.Join(newline, "---", "title: Glioma", "---", "[CROSSWALK]"));

        Assert.Throws<FormatException>(() => _store.GetPage("glioma"));
    }

    /// <summary>
    /// The crosswalk is a table. Splicing a fragment straight against the
    /// neighbouring paragraph makes Markdown's lazy continuation swallow it,
    /// and the table renders as pipe-mangled prose with no error anywhere.
    /// </summary>
    [Fact]
    public void ATableInABlockSurvivesBeingSplicedIntoAPage()
    {
        WriteBlock("crosswalk", "| Old name | Name now |\n| --- | --- |\n| Secondary GBM | Astrocytoma |");
        WritePage("glioma", "Before.\n[CROSSWALK]\nAfter.");

        var html = _store.GetPage("glioma")!.Html;

        Assert.Contains("<table>", html);
        Assert.Contains("<td>Secondary GBM</td>", html);
    }

    [Fact]
    public void ADirectiveInsideAFencedCodeBlockIsNotExpanded()
    {
        WriteBlock("crosswalk", "BLOCK BODY");
        WritePage("how-to", "To include it, write:\n\n```\n[CROSSWALK]\n```\n\nThat is all.");

        var html = _store.GetPage("how-to")!.Html;

        Assert.DoesNotContain("BLOCK BODY", html);
        Assert.Contains("[CROSSWALK]", html);
    }

    [Fact]
    public void ABlockIncludedFromInsideAListStaysInTheList()
    {
        WriteBlock("steps", "Call the nurse line.\n\nWrite down the time.");
        WritePage("glioma", "- First thing.\n\n  [STEPS]\n\n- Second thing.");

        var html = _store.GetPage("glioma")!.Html;

        // Indentation is carried onto the block, so the content stays inside
        // the <li> instead of terminating the list.
        var firstItem = html[html.IndexOf("<li>", StringComparison.Ordinal)..];
        Assert.Contains("Call the nurse line.", firstItem[..firstItem.IndexOf("</li>", StringComparison.Ordinal)]);
    }

    /// <summary>
    /// One unreadable block must not take down pages that never mention it.
    /// Before this was fixed, a typo in any block 500'd /about and /privacy
    /// and made site search return nothing at all.
    /// </summary>
    [Fact]
    public void ABrokenBlockOnlyBreaksThePagesThatIncludeIt()
    {
        WriteBlock("broken", "---\nsources: [ unclosed\n---\nBody.");
        WritePage("about", "An ordinary page with no includes at all.");
        WritePage("glioma", "[BROKEN]");

        Assert.NotNull(_store.GetPage("about"));
        Assert.Contains(_store.SearchPages("ordinary", 10), m => m.UrlPath == "/about");

        var exception = Assert.Throws<FormatException>(() => _store.GetPage("glioma"));
        Assert.Contains("broken", exception.Message);
    }

    [Fact]
    public void ANonBlockMarkdownFileInTheBlocksDirectoryIsIgnored()
    {
        File.WriteAllText(Path.Combine(_blocks, "README.md"), "Notes for authors, not a block.");
        WritePage("about", "An ordinary page.");

        Assert.NotNull(_store.GetPage("about"));
    }

    [Fact]
    public void ASourceWithNoUrlStillReachesThePage()
    {
        WriteBlock("crosswalk",
            "---\nsources:\n  - title: WHO CNS5 print edition\n---\nNames changed in 2021.");
        WritePage("glioma", "[CROSSWALK]");

        var sources = _store.GetPage("glioma")!.FrontMatter.Sources;

        Assert.Contains(sources, s => s.Title == "WHO CNS5 print edition");
    }

    // ---- ContentCheck: the composed page is what gets graded ----

    /// <summary>
    /// The acceptance criterion's stated reason for grading composed pages.
    /// Both halves sit under the 6.0 limit on their own; assembled they do
    /// not. Grading the fragment would pass a page no reader could follow.
    /// </summary>
    [Fact]
    public void ContentCheckGradesTheComposedPageNotTheFragment()
    {
        const string simplePage = "The tumor is in the brain. A doctor can look at it. You can ask questions.";
        const string hardBlock =
            "Neuroradiological interpretation of postoperative contrast enhancement necessitates "
            + "differentiating pseudoprogression from unequivocal radiographic disease progression, "
            + "particularly following concomitant chemoradiotherapy administration.";

        var blocks = Set(("jargon", hardBlock));

        var pageAlone = ContentChecker.CheckPage(
            $"---\ntitle: Test\n---\n{simplePage}", "glioma.md", DateOnly.MaxValue, blocks);
        Assert.DoesNotContain(pageAlone, f => f.Level == FindingLevel.Fail);

        var composed = ContentChecker.CheckPage(
            $"---\ntitle: Test\n---\n{simplePage}\n\n[JARGON]", "glioma.md", DateOnly.MaxValue, blocks);

        Assert.Contains(composed, f =>
            f.Level == FindingLevel.Fail && f.Message.Contains("reading grade"));
    }

    [Fact]
    public void ContentCheckFailsAPageThatIncludesAMissingBlock()
    {
        var findings = ContentChecker.CheckPage(
            "---\ntitle: Test\n---\n[NOSUCHBLOCK]", "glioma.md", DateOnly.MaxValue, ContentBlockSet.Empty);

        Assert.Contains(findings, f =>
            f.Level == FindingLevel.Fail && f.Message.Contains("NOSUCHBLOCK"));
    }

    /// <summary>
    /// A lowercase whole-line token renders as literal bracket text on a
    /// patient's page and resolves nothing. When it names a real block it is
    /// a typo, not prose, so the build says so rather than shipping it.
    /// </summary>
    [Fact]
    public void ContentCheckFailsAMistypedLowercaseDirective()
    {
        WriteBlock("crosswalk", "Old names still on paperwork.");
        WritePage("glioma", "[crosswalk]");

        var findings = ContentChecker.CheckAll(_pages, null, DateOnly.MaxValue, null, _blocks);

        Assert.Contains(findings, f =>
            f.Level == FindingLevel.Fail && f.Message.Contains("[CROSSWALK]"));
    }

    [Fact]
    public void ContentCheckFailsOnABlockItCannotRead()
    {
        WriteBlock("broken", "---\nsources: [ unclosed\n---\nBody.");

        var findings = ContentChecker.CheckAll(_pages, null, DateOnly.MaxValue, null, _blocks);

        Assert.Contains(findings, f =>
            f.Level == FindingLevel.Fail && f.File == "blocks/broken.md");
    }

    /// <summary>
    /// The claim this mechanism rests on: it is inert on a page that includes
    /// nothing. Asserted over the REAL shipped pages rather than three
    /// hand-picked strings, so it keeps being true as pages are added.
    ///
    /// This used to cover every shipped page, because none of them included a
    /// block. WI-558 put [CAREGIVER] on all 18 tumor hubs, and those pages
    /// SHOULD now render differently with blocks available — rendering the
    /// same would mean the include did nothing. The property splits in two:
    /// blockless pages are unaffected (here), and a page that includes a block
    /// fails loudly without it (below) rather than quietly losing a section.
    /// </summary>
    [Fact]
    public void APageThatIncludesNoBlockRendersIdenticallyEitherWay()
    {
        var blocks = Set(("crosswalk", "X"), ("causes", "X"), ("mechanism", "X"),
            ("markers", "X"), ("treatments", "X"), ("prognosis-words", "X"));

        var checked_ = 0;
        foreach (var (raw, urlPath) in ShippedPages())
        {
            if (ContentBlocks.DirectBlockNames(raw).Count > 0)
            {
                continue;
            }

            Assert.Equal(
                ContentStore.Parse(raw, urlPath).Html,
                ContentStore.Parse(raw, urlPath, [], blocks).Html);
            checked_++;
        }

        Assert.True(checked_ > 0, "no blockless shipped page was checked");
    }

    /// <summary>
    /// The other half: a shipped page that includes a block must fail when the
    /// block is missing, never render the section away. On a medical page,
    /// silence reads as "there is nothing to say here" — and the caregiver
    /// block is where "when to call an ambulance" lives.
    /// </summary>
    [Fact]
    public void AShippedPageThatIncludesABlockFailsWithoutIt()
    {
        var including = 0;
        foreach (var (raw, urlPath) in ShippedPages())
        {
            var names = ContentBlocks.DirectBlockNames(raw);
            if (names.Count == 0)
            {
                continue;
            }

            var exception = Assert.Throws<FormatException>(() => ContentStore.Parse(raw, urlPath));
            Assert.Contains(names[0], exception.Message, StringComparison.OrdinalIgnoreCase);
            including++;
        }

        Assert.True(including >= 18, $"expected the 18 tumor hubs to include a block, found {including}");
    }

    /// <summary>Every curated page as shipped, with the URL path it renders at.</summary>
    private static IEnumerable<(string Raw, string UrlPath)> ShippedPages()
    {
        var pagesRoot = Path.Combine(AppContext.BaseDirectory, "Content", "pages");
        var files = Directory.EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(files);

        return files.Select(file => (
            File.ReadAllText(file),
            Path.GetRelativePath(pagesRoot, file).Replace('\\', '/')[..^3]));
    }

    private static ContentBlockSet Set(params (string Name, string Body)[] blocks) =>
        new(blocks.ToDictionary(b => b.Name, b => ContentBlocks.ParseBlock(b.Body, b.Name)),
            new Dictionary<string, string>());

    [Fact]
    public void ContentCheckWarnsAboutABlockNoPageIncludes()
    {
        WriteBlock("orphan", "Nothing points at this.");
        WriteBlock("used", "This one is included.");
        WritePage("glioma", "[USED]");

        var findings = ContentChecker.CheckAll(_pages, null, DateOnly.MaxValue, null, _blocks);

        Assert.Contains(findings, f =>
            f.Level == FindingLevel.Warn && f.File == "blocks/orphan.md");
        Assert.DoesNotContain(findings, f => f.File == "blocks/used.md");
    }

    /// <summary>
    /// A block reached only through another block is used. Warning about it
    /// would train people to ignore the warning.
    /// </summary>
    [Fact]
    public void ABlockReachedOnlyThroughAnotherBlockCountsAsUsed()
    {
        WriteBlock("caregiver", "Caring for someone.\n\n[SEIZURE]");
        WriteBlock("seizure", "Stay with them.");
        WritePage("glioma", "[CAREGIVER]");

        var findings = ContentChecker.CheckAll(_pages, null, DateOnly.MaxValue, null, _blocks);

        Assert.DoesNotContain(findings, f => f.File == "blocks/seizure.md");
    }

    [Fact]
    public void ContentCheckDefaultsTheBlocksRootToTheSiblingOfThePagesRoot()
    {
        WriteBlock("crosswalk", "Shared wording.");
        WritePage("glioma", "[CROSSWALK]");

        // No blocksRoot argument — it has to find _root/blocks from _root/pages.
        // (Asserting on the missing-block message specifically, not on "no
        // failures at all": a two-word block grades badly and that is a
        // property of the fixture, not of the lookup under test.)
        var findings = ContentChecker.CheckAll(_pages, null, DateOnly.MaxValue);

        Assert.DoesNotContain(findings, f => f.Message.Contains("there is no block"));
    }

    private sealed class StubEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
