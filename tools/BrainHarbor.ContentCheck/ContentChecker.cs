using System.Text;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BrainHarbor.ContentCheck;

public enum FindingLevel { Fail, Warn, Info }

public sealed record Finding(FindingLevel Level, string File, string Message);

/// <summary>
/// WI-106: walks curated pages + glossary terms and reports (content-pipeline
/// §5): reading grade (fail &gt; 6.0, warn ≥ 5.5), invalid front matter
/// (fail), missing sources (warn), overdue review_due (warn).
/// </summary>
public static partial class ContentChecker
{
    // WI-414 (2026-08-13, Dan): 6th grade, everywhere a reader looks. The
    // curated pages already sat at 2.5-4.9, and only two Razor pages needed
    // simplifying, so this is a floor the site already meets rather than an
    // aspiration. Summaries are NOT held to this yet - see content-pipeline
    // §5: three quarters of them would be flagged, which would empty the
    // feed rather than improve it.
    public const double FailGrade = 6.0;
    public const double WarnGrade = 5.5;

    // WI-503: custom containers ONLY. Without this the ':::outlook' fence
    // parses as a paragraph and is graded as a sentence, so a reader-choice
    // gate would move the page's grade without changing a word of its prose.
    // Deliberately not UseAdvancedExtensions(): that would also restructure
    // tables and definition lists, silently re-grading every shipped page.
    private static readonly MarkdownPipeline TextPipeline =
        new MarkdownPipelineBuilder().UseCustomContainers().Build();

    /// <summary>Flags _Disclaimers.cshtml knows how to render.</summary>
    public static readonly string[] KnownDisclaimers = ["medical", "benefits", "legal"];

    /// <summary>
    /// Razor pages whose words a patient or caregiver reads. Admin and the dev
    /// styleguide are staff tools — holding a review queue to a patient reading
    /// level would only teach people to ignore the gate. Partials are included:
    /// a feed card's words are as public as a page's.
    /// </summary>
    private static bool IsReaderFacing(string relativePath) =>
        !relativePath.StartsWith("Admin/", StringComparison.OrdinalIgnoreCase)
        && !relativePath.StartsWith("Dev/", StringComparison.OrdinalIgnoreCase)
        && !Path.GetFileName(relativePath).StartsWith("_View", StringComparison.OrdinalIgnoreCase);

    public static List<Finding> CheckAll(
        string pagesRoot, string? glossaryRoot, DateOnly today, string? razorRoot = null,
        string? blocksRoot = null)
    {
        var findings = new List<Finding>();

        // WI-501: the blocks have to load before any page is graded — a page
        // is graded COMPOSED, so an unloadable block is a page-level failure.
        blocksRoot ??= ContentBlockStore.DefaultBlocksRootFor(pagesRoot);
        var blocks = ContentBlockStore.Load(blocksRoot);

        // At runtime a broken block only breaks the pages that include it.
        // Here it fails the build outright: shipping one is never intended.
        foreach (var (name, error) in blocks.Errors.OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            findings.Add(new(FindingLevel.Fail, $"blocks/{name}.md", error));
        }

        var usedBlocks = new HashSet<string>(StringComparer.Ordinal);

        // A missing/empty root must be LOUD: a silently skipped directory is
        // how a safety gate dies of a rename. (Warn, not fail — the pages
        // root legitimately doesn't exist until WI-107 writes the pages.)
        if (Directory.Exists(pagesRoot))
        {
            var files = Directory.EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.Ordinal).ToList();
            if (files.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, pagesRoot, "pages root exists but has no .md files — nothing checked"));
            }
            foreach (var file in files)
            {
                var relative = Path.GetRelativePath(pagesRoot, file).Replace('\\', '/');
                var raw = File.ReadAllText(file);
                findings.AddRange(CheckPage(raw, relative, today, blocks));
                findings.AddRange(CheckForMistypedDirectives(raw, relative, blocks));
                RecordUsedBlocks(raw, blocks, usedBlocks);
            }
        }
        else
        {
            findings.Add(new(FindingLevel.Warn, pagesRoot, "pages root MISSING — no pages were checked"));
        }

        // A block no page reaches is graded by nothing — the composed-page
        // rule means blocks have no reading level of their own.
        foreach (var orphan in blocks.Blocks.Keys.Where(name => !usedBlocks.Contains(name))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            findings.Add(new(FindingLevel.Warn, $"blocks/{orphan}.md",
                "no page includes this block — nothing grades its reading level"));
        }

        if (glossaryRoot is not null && Directory.Exists(glossaryRoot))
        {
            foreach (var file in Directory.EnumerateFiles(glossaryRoot, "*.md")
                         .OrderBy(f => f, StringComparer.Ordinal))
            {
                findings.AddRange(CheckGlossaryTerm(
                    File.ReadAllText(file), Path.GetFileNameWithoutExtension(file)));
            }
        }
        else if (glossaryRoot is not null)
        {
            findings.Add(new(FindingLevel.Warn, glossaryRoot, "glossary root MISSING — no terms were checked"));
        }

        // WI-414: the pages people actually land on. Their copy lives in
        // .cshtml, so until now the most-read text on the site was the only
        // text no tool checked.
        if (razorRoot is not null && Directory.Exists(razorRoot))
        {
            var razorFiles = Directory.EnumerateFiles(razorRoot, "*.cshtml", SearchOption.AllDirectories)
                .Select(f => (Full: f, Relative: Path.GetRelativePath(razorRoot, f).Replace('\\', '/')))
                .Where(f => IsReaderFacing(f.Relative))
                .OrderBy(f => f.Relative, StringComparer.Ordinal)
                .ToList();

            if (razorFiles.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, razorRoot,
                    "razor root exists but has no reader-facing .cshtml files — nothing checked"));
            }

            foreach (var (full, relative) in razorFiles)
            {
                findings.AddRange(CheckRazorPage(File.ReadAllText(full), relative));
            }
        }
        else if (razorRoot is not null)
        {
            findings.Add(new(FindingLevel.Warn, razorRoot, "razor root MISSING — no pages were checked"));
        }

        return findings;
    }

    /// <summary>
    /// Reading level for a Razor page. Only the grade: front matter, sources
    /// and review dates are a curated-content idea, and a page with no prose
    /// at all (a partial that is pure markup) is reported as Info rather than
    /// pretended to be grade 0.
    /// </summary>
    public static List<Finding> CheckRazorPage(string raw, string relativePath)
    {
        var text = RazorTextExtractor.ExtractSentences(raw);
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        // Below this, one long word swings the grade by several levels and the
        // number says more about the sample than the writing.
        const int MinimumWordsToGrade = 25;
        if (words < MinimumWordsToGrade)
        {
            return [new(FindingLevel.Info, relativePath, $"{words} word(s) of prose — too little to grade")];
        }

        return [GradeFinding(ReadabilityAnalyzer.FleschKincaidGrade(text), relativePath)];
    }

    private static Finding GradeFinding(double grade, string relativePath) => grade switch
    {
        > FailGrade => new(FindingLevel.Fail, relativePath,
            $"reading grade {grade:0.0} is above the {FailGrade} limit — simplify the language"),
        >= WarnGrade => new(FindingLevel.Warn, relativePath,
            $"reading grade {grade:0.0} is close to the {FailGrade} limit"),
        _ => new(FindingLevel.Info, relativePath, $"reading grade {grade:0.0}"),
    };

    /// <summary>
    /// A whole line reading <c>[Crosswalk]</c> or <c>[crosswalk]</c> is not a
    /// directive — the pattern is uppercase — so it renders as literal
    /// bracket text on a patient's page with nothing else to signal it. When
    /// the token names a block that actually exists, it is a typo rather than
    /// prose, and the build should say so.
    /// </summary>
    private static List<Finding> CheckForMistypedDirectives(
        string raw, string relativePath, ContentBlockSet blocks)
    {
        var findings = new List<Finding>();
        foreach (Match match in MistypedDirectivePattern().Matches(raw))
        {
            var token = match.Groups[1].Value;
            if (token == token.ToUpperInvariant())
            {
                continue; // a real directive, already resolved
            }

            var name = token.ToLowerInvariant();
            if (blocks.Blocks.ContainsKey(name) || blocks.Errors.ContainsKey(name))
            {
                findings.Add(new(FindingLevel.Fail, relativePath,
                    $"'[{token}]' on a line of its own looks like an include of block '{name}' but is not "
                    + $"uppercase, so it renders as literal text — write [{name.ToUpperInvariant()}]"));
            }
        }
        return findings;
    }

    [GeneratedRegex(@"^[ \t]*\[([A-Za-z0-9][A-Za-z0-9-]*)\][ \t]*\r?$", RegexOptions.Multiline)]
    private static partial Regex MistypedDirectivePattern();

    /// <summary>
    /// Marks every block a page reaches, following blocks that include other
    /// blocks — a block used only by another block is still used. Bounded by
    /// the visited set, so an include cycle (already reported as a page
    /// failure) cannot spin here.
    /// </summary>
    private static void RecordUsedBlocks(
        string raw, ContentBlockSet blocks, HashSet<string> used)
    {
        var pending = new Queue<string>(ContentBlocks.DirectBlockNames(raw));
        while (pending.Count > 0)
        {
            var name = pending.Dequeue();
            if (!used.Add(name) || !blocks.Blocks.TryGetValue(name, out var block))
            {
                continue;
            }
            foreach (var nested in ContentBlocks.DirectBlockNames(block.Markdown))
            {
                pending.Enqueue(nested);
            }
        }
    }

    public static List<Finding> CheckPage(string raw, string relativePath, DateOnly today) =>
        CheckPage(raw, relativePath, today, null);

    public static List<Finding> CheckPage(
        string raw, string relativePath, DateOnly today, ContentBlockSet? blocks)
    {
        var findings = new List<Finding>();

        ContentPage page;
        try
        {
            var urlPath = relativePath.EndsWith(".md") ? relativePath[..^3] : relativePath;
            page = ContentStore.Parse(raw, urlPath, [], blocks);
        }
        catch (FormatException exception)
        {
            findings.Add(new(FindingLevel.Fail, relativePath, exception.Message));
            return findings;
        }

        var plainText = ExtractSentences(page.Markdown);
        findings.Add(GradeFinding(ReadabilityAnalyzer.FleschKincaidGrade(plainText), relativePath));

        if (page.FrontMatter.Sources.Count == 0)
        {
            findings.Add(new(FindingLevel.Warn, relativePath,
                "no sources in front matter — every claim must trace (content-pipeline §2)"));
        }

        if (page.FrontMatter.ReviewDue is { } due && due < today)
        {
            findings.Add(new(FindingLevel.Warn, relativePath,
                $"review overdue since {due:yyyy-MM-dd}"));
        }

        // A typo'd flag renders no disclaimer at all — on a medical page that
        // is a safety failure, so it fails the build rather than warning.
        foreach (var flag in page.FrontMatter.Disclaimers.Where(d => !KnownDisclaimers.Contains(d)))
        {
            findings.Add(new(FindingLevel.Fail, relativePath,
                $"unknown disclaimer flag '{flag}' — nothing would render; expected one of {string.Join(", ", KnownDisclaimers)}"));
        }

        return findings;
    }

    /// <summary>
    /// Block-aware plain text for the readability pass: each heading, bullet,
    /// and paragraph becomes its own sentence (terminator appended when
    /// missing) and soft line wraps join with spaces. Feeding raw plaintext
    /// to FK merges heading words into the next sentence and inflates the
    /// grade by 1–2 levels — punishing exactly the structure that helps
    /// impaired readers.
    /// </summary>
    public static string ExtractSentences(string markdown)
    {
        var document = Markdig.Markdown.Parse(markdown, TextPipeline);
        var result = new StringBuilder();

        foreach (var block in document.Descendants().OfType<LeafBlock>())
        {
            if (block.Inline is null)
            {
                continue;
            }

            var text = string.Concat(block.Inline.Descendants().Select(inline => inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                CodeInline code => code.Content,
                LineBreakInline => " ",
                _ => string.Empty,
            })).Trim();

            if (text.Length == 0)
            {
                continue;
            }

            result.Append(text);
            if (text[^1] is not ('.' or '!' or '?'))
            {
                result.Append('.');
            }
            result.Append(' ');
        }

        return result.ToString();
    }

    public static List<Finding> CheckGlossaryTerm(string raw, string slug)
    {
        try
        {
            var term = GlossaryStore.ParseTerm(raw, slug);
            var words = term.Definition.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            return words > 40
                ? [new(FindingLevel.Fail, $"glossary/{slug}.md",
                    $"definition is {words} words — the editorial limit is 40 (content-pipeline §6)")]
                : [new(FindingLevel.Info, $"glossary/{slug}.md", $"ok ({words} words)")];
        }
        catch (FormatException exception)
        {
            return [new(FindingLevel.Fail, $"glossary/{slug}.md", exception.Message)];
        }
    }
}
