using System.Text.RegularExpressions;
using BrainHarbor.ContentCheck;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-503: the reader-choice gate. Two properties carry the item, and most of
/// these tests are one or the other.
///
/// (1) It is CLOSED. A gate that renders open, or that renders its content
/// outside the disclosure, has published prognosis to a reader who was never
/// asked.
///
/// (2) A typo FAILS rather than falls open. Markdig renders an unknown
/// ':::name' as an anonymous div, so ':::outlok' would leave the frightening
/// content fully visible with no error and a green build — the same shape of
/// silent-wrong-page that WI-501's CRLF bug had.
/// </summary>
public sealed class ReaderGateTests : IDisposable
{
    private const string Gated =
        "## What might happen over time\n\n" +
        ":::outlook\n" +
        "The numbers describe a group. They cannot say what will happen to you.\n" +
        ":::\n";

    private static ContentPage Parse(string body, string slug = "tumors/sample") =>
        ContentStore.Parse($"---\ntitle: Sample\n---\n{body}", slug);

    // ---- (1) closed, and the right things on the right side of the gate ----

    [Fact]
    public void TheGateRendersAsADetailsDisclosureWithNoOpenAttribute()
    {
        var html = Parse(Gated).Html;

        Assert.Contains("<details class=\"reader-gate__disclosure\">", html);
        Assert.Contains("<summary class=\"reader-gate__toggle\">", html);

        // `open` must never be emitted, in any attribute order. Matching the
        // one string the renderer happens to produce today would pass against
        // `<details open class="...">`.
        Assert.DoesNotMatch(new Regex(@"<details[^>]*\bopen\b"), html);
    }

    [Fact]
    public void TheWarningIsOutsideTheDisclosureAndTheGatedWordsAreInsideIt()
    {
        var html = Parse(Gated).Html;

        var warning = html.IndexOf("reader-gate__warning", StringComparison.Ordinal);
        var details = html.IndexOf("<details", StringComparison.Ordinal);
        var gatedWords = html.IndexOf("The numbers describe a group", StringComparison.Ordinal);
        var closingDetails = html.IndexOf("</details>", StringComparison.Ordinal);

        // Every part has to be PRESENT before its position means anything —
        // an all-missing render gives -1s, and -1 <= -1 <= -1 passes an
        // ordering assertion while rendering nothing at all.
        Assert.True(warning >= 0 && details >= 0 && gatedWords >= 0 && closingDetails >= 0, html);

        Assert.InRange(warning, 0, details);
        Assert.InRange(gatedWords, details, closingDetails);
    }

    [Fact]
    public void TheWarningIsTheStandardSentenceFromTheEditorialStandard()
    {
        // content-pipeline.md §12.5 quotes this verbatim. It lives in code so
        // 24 tumor hubs cannot each soften it; this pins the two copies
        // together.
        Assert.Contains(ReaderGate.WarningText, Parse(Gated).Html);
    }

    [Fact]
    public void TheHeadingAboveTheGateStaysOutsideIt()
    {
        // The page outline has to stay complete, and the reader has to meet
        // heading -> warning -> choice in that order (§12.6, "warn before you
        // disclose"). A heading swallowed into the disclosure breaks both.
        var html = Parse(Gated).Html;

        var heading = html.IndexOf("What might happen over time", StringComparison.Ordinal);
        var details = html.IndexOf("<details", StringComparison.Ordinal);

        Assert.True(heading >= 0 && details >= 0, html);
        Assert.InRange(heading, 0, details);
    }

    [Fact]
    public void BothToggleLabelsAreRenderedSoTheStateCanBeShownWithoutJavaScript()
    {
        var html = Parse(Gated).Html;

        Assert.Contains(ReaderGate.ShowLabel, html);
        Assert.Contains(ReaderGate.HideLabel, html);
    }

    // ---- (2) a typo fails, by name ----

    [Fact]
    public void AMistypedGateNameFailsThePageInsteadOfRenderingAnOpenDiv()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::outlok\nThe hard part.\n:::\n"));

        Assert.Contains("outlok", exception.Message);
        Assert.Contains("outlook", exception.Message);

        // The failure has to name the consequence, or the next person "fixes"
        // it by deleting the block.
        Assert.Contains("fully visible", exception.Message);
    }

    [Fact]
    public void ACapitalisedGateNameSaysToWriteItInLowercase()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::Outlook\nThe hard part.\n:::\n"));

        Assert.Contains("lowercase", exception.Message);
    }

    [Fact]
    public void AnInvisibleCharacterInTheNameIsNamedAsSuch()
    {
        // Pasting the fence from a document editor is how this happens, and
        // without the hint the error shows two strings that look identical.
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::outlook​\nThe hard part.\n:::\n"));

        Assert.Contains("invisible character", exception.Message);
    }

    [Fact]
    public void AGateWithNoNameFails()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::\nThe hard part.\n:::\n"));

        Assert.Contains("no name", exception.Message);
    }

    [Fact]
    public void ArgumentsAfterTheGateNameFailRatherThanBeingDroppedSilently()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::outlook for adults\nThe hard part.\n:::\n"));

        Assert.Contains("for adults", exception.Message);
    }

    [Fact]
    public void AnEmptyGateFails()
    {
        var exception = Assert.Throws<FormatException>(() => Parse(":::outlook\n:::\n"));

        Assert.Contains("nothing to say here", exception.Message);
    }

    [Fact]
    public void TheFailureNamesThePageItIsOn()
    {
        var exception = Assert.Throws<FormatException>(
            () => Parse(":::outlok\nThe hard part.\n:::\n", "tumors/low-grade-glioma"));

        Assert.Contains("tumors/low-grade-glioma", exception.Message);
    }

    /// <summary>
    /// The typos that never produce a container at all, so an AST walk cannot
    /// see them. Each one renders the outlook section as ordinary visible
    /// prose with no error, which is the exact failure the gate exists to
    /// prevent — and the two-colon and indented forms are the likeliest slips
    /// a human makes typing ':::'.
    /// </summary>
    [Theory]
    // One colon short: renders as a literal paragraph.
    [InlineData("::outlook\nThe hard part.\n::\n")]
    // Four-space indent: CommonMark makes it an indented CODE block, which
    // also hides the words from the reading-level check.
    [InlineData("    :::outlook\n    The hard part.\n    :::\n")]
    // A tab does the same.
    [InlineData("\t:::outlook\n\tThe hard part.\n\t:::\n")]
    // Opened but never closed: everything after it is swallowed into a
    // disclosure the reader was told is only about outlook.
    [InlineData(":::outlook\nThe hard part.\n::\n\nWhere to get help.\n")]
    // The inline variant is a different node type entirely.
    [InlineData("Some text ::outlook stuff:: after.\n")]
    public void AGateThatDoesNotParseAsAGateFailsThePage(string body)
    {
        var exception = Assert.Throws<FormatException>(() => Parse(body));

        Assert.Contains("outlook", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("::outlook\nThe hard part.\n::\n")]
    [InlineData("    :::outlook\n    The hard part.\n    :::\n")]
    [InlineData(":::outlook\nThe hard part.\n::\n\nWhere to get help.\n")]
    public void TheMalformedGateCheckSurvivesWindowsLineEndings(string body)
    {
        // Every author's working tree is CRLF here (core.autocrlf=true) and CI
        // is LF. WI-501 shipped a bug of exactly this shape, and a check that
        // only works on LF is a check that only works on CI.
        Assert.Throws<FormatException>(() => Parse(body.Replace("\n", "\r\n")));
    }

    [Fact]
    public void AGateIndentedInsideAListStillWorks()
    {
        // The indent rule has to be relative, not absolute: WI-501 splices a
        // block into a list item by carrying the directive's indentation onto
        // every line, so a flat "no leading whitespace" rule would break the
        // one legitimate reason to indent.
        var html = Parse("1. First step.\n\n   :::outlook\n   The hard part.\n   :::\n").Html;

        Assert.Contains("<details class=\"reader-gate__disclosure\">", html);
    }

    [Fact]
    public void ColonsInsideAFencedCodeBlockAreNotCountedAsAMalformedGate()
    {
        // The page documenting this syntax has to be able to show the wrong
        // way as well as the right way.
        var html = Parse("```\n::outlook\nnot a gate\n::\n```\n").Html;

        Assert.Contains("::outlook", html);
        Assert.DoesNotContain("<details", html);
    }

    [Fact]
    public void ThePipelineRefusesToRenderAGateAsAPlainDiv()
    {
        // Markdig's own renderer turns an unknown container into
        // <div class="outlook">, content wide open. Registration order and
        // renderer reuse both decide which renderer wins, and either can put
        // the default back. Rendering the same document twice through one
        // renderer is the cheapest way to catch that.
        var pipeline = ContentStore.RenderPipeline;
        var document = Markdig.Markdown.Parse(":::outlook\nThe hard part.\n:::\n", pipeline);

        for (var pass = 0; pass < 2; pass++)
        {
            using var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            pipeline.Setup(renderer);
            pipeline.Setup(renderer); // a second setup must not reinstate the default
            renderer.Render(document);
            writer.Flush();

            Assert.Contains("<details class=\"reader-gate__disclosure\">", writer.ToString());
            Assert.DoesNotContain("<div class=\"outlook\"", writer.ToString());
        }
    }

    [Fact]
    public void AMisorderedPipelineIsRejectedInsteadOfRenderingAnOpenDiv()
    {
        // Registering the gate BEFORE the custom-container extension puts
        // Markdig's own renderer back in front, and the only symptom is
        // prognosis published open. One line in a pipeline definition, no
        // visible error — so the pipeline has to check itself.
        var builder = new Markdig.MarkdownPipelineBuilder();
        builder.Extensions.Add(new ReaderGateExtension());
        Markdig.MarkdownExtensions.UseAdvancedExtensions(builder);
        var misordered = builder.Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => (object)ReaderGateExtension.Verify(misordered));

        Assert.Contains("AFTER", exception.Message, StringComparison.Ordinal);
    }

    // ---- the ways it could break quietly ----

    [Fact]
    public void TheGateWorksInAFileWithWindowsLineEndings()
    {
        // WI-501 shipped a CRLF bug that CI could never have caught: this repo
        // has core.autocrlf=true, CI is Linux/LF, so a fresh clone renders
        // something the build has never seen. Every gate test above uses \n,
        // which is exactly how that one got through.
        var html = Parse(Gated.Replace("\n", "\r\n")).Html;

        Assert.Contains("<details class=\"reader-gate__disclosure\">", html);
        Assert.Contains("The numbers describe a group", html);
    }

    [Fact]
    public void AGateWrittenInsideAFencedCodeBlockIsLeftAlone()
    {
        // The page that documents this syntax must be able to show it.
        var html = Parse("```\n:::outlook\nThe hard part.\n:::\n```\n").Html;

        Assert.DoesNotContain("<details", html);
        Assert.Contains(":::outlook", html);
    }

    [Fact]
    public void GlossaryTermsInsideTheGateStillGetTooltips()
    {
        // The gate must not become a hole in the rest of the content
        // machinery. Composition-before-parse is what buys this (WI-501).
        var terms = new List<GlossaryTerm>
        {
            new("glioma", "glioma", [], null, "A tumor that starts in the brain's support cells."),
        };

        var html = ContentStore.Parse(
            "---\ntitle: Sample\n---\n:::outlook\nA glioma grows at its own pace.\n:::\n",
            "tumors/sample",
            terms).Html;

        Assert.Contains("popovertarget=\"def-glioma\"", html);
    }

    [Fact]
    public void AGateInsideASharedBlockComposesAndRendersClosed()
    {
        WriteBlock("outlook-note", ":::outlook\nThe numbers describe a group.\n:::");
        WritePage("glioma", "Before.\n\n[OUTLOOK-NOTE]\n\nAfter.");

        var html = _store!.GetPage("glioma")!.Html;

        Assert.Contains("<details class=\"reader-gate__disclosure\">", html);
        Assert.DoesNotContain(" open>", html);
    }

    // ---- the reading gate still reaches inside ----

    [Fact]
    public void ContentCheckGradesTheWordsHiddenBehindTheGate()
    {
        // Content a reader has to choose to see is still content a reader
        // reads. If the gate were a blind spot in the 6.0 check, it would be
        // the most tempting place on the site to put a hard sentence.
        var page =
            "---\ntitle: Sample\n---\n:::outlook\n"
            + "Prognostication utilizing population-level survival distributions necessitates "
            + "considerable interpretative caution regarding individual applicability, "
            + "notwithstanding methodological sophistication in contemporary epidemiological "
            + "characterisation of heterogeneous neuro-oncological populations.\n:::\n";

        var findings = ContentChecker.CheckPage(page, "tumors/sample.md", new DateOnly(2026, 8, 30));

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail && f.Message.Contains("reading grade"));
    }

    [Fact]
    public void TheGateFenceItselfDoesNotChangeThePagesGrade()
    {
        // ':::outlook' parsed as a paragraph would be graded as a sentence, so
        // wrapping a section in a gate would move the number without changing
        // a word of the prose — and an author would then "simplify" writing
        // that was already fine.
        const string prose = "The numbers describe a group. They cannot say what will happen to you.";

        var plain = ContentChecker.ExtractSentences(prose);
        var gated = ContentChecker.ExtractSentences($":::outlook\n{prose}\n:::");

        Assert.Equal(plain, gated);
    }

    [Fact]
    public void TheGatesOwnCopyMeetsTheSixthGradeLimit()
    {
        // ContentCheck walks .md and .cshtml files. This component's words
        // live in a C# constant, which is the one place that walk cannot see,
        // so the gate is applied here instead — same analyzer, same limit.
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(ReaderGate.ReaderFacingCopy);

        Assert.True(
            grade <= ContentChecker.FailGrade,
            $"the reader-choice gate's own copy grades {grade:0.0}, above the {ContentChecker.FailGrade} limit");
    }

    // ---- fixture for the shared-block case ----

    private readonly string _root;
    private readonly string _pages;
    private readonly string _blocks;
    private readonly ContentStore _store;

    public ReaderGateTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "bh-gate-" + Guid.NewGuid().ToString("N"));
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

    private void WritePage(string slug, string body) =>
        File.WriteAllText(Path.Combine(_pages, slug + ".md"), $"---\ntitle: Test page\n---\n{body}");

    private void WriteBlock(string name, string body) =>
        File.WriteAllText(Path.Combine(_blocks, name + ".md"), body);

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
