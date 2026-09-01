using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BrainHarbor.Web.Content;

/// <summary>
/// WI-503: the reader-choice gate. Outlook sits at position 12 of a tumor hub
/// (content-pipeline.md §12.3) behind an explicit choice, because prognosis
/// disclosure needs negotiation and staged disclosure across visits — three
/// things a web page structurally cannot do. Interviews with 25 newly
/// diagnosed glioma patients produced "not all patients want to know it all";
/// some wanted full honesty, some generalities, some only positive
/// information. That mandates a mechanism, not a policy (§12.5).
///
/// Authors write a Markdig custom container:
///
///     ## What might happen over time
///
///     :::outlook
///     ...the hard part...
///     :::
///
/// The heading stays OUTSIDE the container by convention, so the page outline
/// stays complete and the reader meets heading -> warning -> choice in that
/// order, which is §12.6's "warn before you disclose".
///
/// Why a container and not raw HTML: <see cref="ContentStore"/> builds its
/// pipeline with <c>DisableHtml()</c>, so a <c>&lt;details&gt;</c> typed into
/// a .md file renders escaped. Why a container and not a WI-501 shared block:
/// a block splices a fragment in, and this has to WRAP author content.
/// </summary>
public static partial class ReaderGate
{
    // Any line that OPENS WITH two or more colons is trying to be a fence.
    // Deliberately loose: the point is to catch the near-misses Markdig throws
    // away silently, so it has to match things that are not valid containers.
    [GeneratedRegex(@"^[ \t]*(:{2,})")]
    private static partial Regex FenceLikePattern();


    /// <summary>
    /// The container name. One name, deliberately: the wording below is the
    /// §12.5 standard and is validated for outlook specifically. A second
    /// gate (WI-559's seizure content, say) needs its own evidence-backed
    /// copy, not this sentence with a noun swapped — but adding it here is a
    /// one-line change when that copy exists.
    /// </summary>
    public const string OutlookName = "outlook";

    /// <summary>
    /// The standard sentence, verbatim from content-pipeline.md §12.5. It
    /// lives in code rather than in each page so that 24 tumor hubs cannot
    /// each water it down — the same reason WI-501 exists.
    /// </summary>
    public const string WarningText =
        "The next part is about outlook. Some people want to read it. Some people would rather not. "
        + "You can skip it and come back another day. Nothing else on this page depends on it.";

    public const string ShowLabel = "Show the part about outlook";
    public const string HideLabel = "Hide the part about outlook";

    /// <summary>
    /// All the reader-facing copy this component renders, as one string.
    /// ContentCheck grades .md files and .cshtml files; copy that lives in a
    /// C# constant is the one place its file walk cannot see, so
    /// ReaderGateTests grades THIS through the same
    /// <c>ReadabilityAnalyzer</c> the build uses.
    /// </summary>
    public static string ReaderFacingCopy => $"{WarningText} {ShowLabel}. {HideLabel}.";

    /// <summary>
    /// Fails a page whose <c>:::</c> container is not a gate we know how to
    /// render. This is the whole safety property of the item: Markdig's
    /// default renderer turns an unknown container into an anonymous
    /// <c>&lt;div class="outlok"&gt;</c> — the frightening content wide open,
    /// no error, and a green build. A gate that fails open is worse than no
    /// gate, so a typo fails the page BY NAME, the same way a missing shared
    /// block does (WI-501).
    ///
    /// Called from <see cref="ContentStore.Parse(string, string, IReadOnlyList{GlossaryTerm}, ContentBlockSet?)"/>
    /// before rendering, so ContentCheck reports it as a build failure — it
    /// already turns a <see cref="FormatException"/> from Parse into a Fail.
    /// </summary>
    public static void Validate(MarkdownDocument document, string markdown, string describedAs)
    {
        // The `::inline::` variant is a different node type, and Markdig
        // renders it as an anonymous <span> — so ':::' typed with one colon
        // missing mid-sentence is another way to publish the words plainly.
        if (document.Descendants().OfType<CustomContainerInline>().Any())
        {
            throw new FormatException(
                $"Content page '{describedAs}' uses '::...::' inside a line. The reader-choice gate "
                + $"is a BLOCK: ':::{OutlookName}' on a line of its own, the words, then ':::' on a "
                + "line of its own. Written inline it renders as ordinary visible text.");
        }

        var containers = document.Descendants().OfType<CustomContainer>().ToList();

        foreach (var container in containers)
        {
            var info = container.Info;

            if (string.IsNullOrWhiteSpace(info))
            {
                throw new FormatException(
                    $"Content page '{describedAs}' has a ':::' block with no name. "
                    + $"The only one this site renders is ':::{OutlookName}' (WI-503).");
            }

            if (!string.Equals(info, OutlookName, StringComparison.Ordinal))
            {
                // A capitalisation slip is the likeliest typo and the message
                // should say so rather than make the author guess. An
                // invisible character pasted from Word or Notion is the
                // cruellest one: without this branch the error prints two
                // identical-looking strings and blames the author for the
                // difference between them.
                var visible = new string([.. info.Where(c => !char.IsControl(c) && c < 0x2000)]);
                var hint = string.Equals(info, OutlookName, StringComparison.OrdinalIgnoreCase)
                    ? $" — write it in lowercase, ':::{OutlookName}'"
                    : string.Equals(visible, OutlookName, StringComparison.OrdinalIgnoreCase)
                        ? " — there is an invisible character in the name; delete the line and retype it"
                        : $" — the only one this site renders is ':::{OutlookName}'";
                throw new FormatException(
                    $"Content page '{describedAs}' opens ':::{info}', which is not a reader-choice gate{hint}. "
                    + "An unknown ':::' block would render as a plain div, leaving the content it wraps "
                    + "fully visible (WI-503).");
            }

            if (!string.IsNullOrWhiteSpace(container.Arguments))
            {
                throw new FormatException(
                    $"Content page '{describedAs}' writes ':::{info} {container.Arguments}'. "
                    + $"The gate takes no arguments, and '{container.Arguments}' would be silently dropped — "
                    + "put the words inside the block instead.");
            }

            if (container.Count == 0)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' has an empty ':::{OutlookName}' block. "
                    + "It would render a choice that opens onto nothing, which reads as "
                    + "'there is nothing to say here'.");
            }
        }

        AuditFenceLines(markdown, containers.Count, describedAs);
    }

    /// <summary>
    /// The checks above can only inspect containers Markdig actually built —
    /// and the likeliest typos stop one from being built at all, so there is
    /// nothing in the tree to find and the page ships with the outlook
    /// section as plain visible prose. Three real cases:
    ///
    /// <list type="bullet">
    /// <item><c>::outlook</c> — one colon short, renders as a paragraph.</item>
    /// <item>a tab or four spaces before <c>:::outlook</c> — CommonMark makes
    /// that an indented CODE block, which renders the words AND hides them
    /// from the reading-level check, since ContentCheck grades inline text and
    /// a code block has none.</item>
    /// <item>a missing or malformed closing fence — everything to the end of
    /// the page is swallowed into a disclosure the reader was told is only
    /// about outlook, taking the caregiver and support sections with it.</item>
    /// </list>
    ///
    /// So the raw source is audited alongside the tree: every fence-like line
    /// has to be accounted for by a container that actually parsed.
    /// </summary>
    private static void AuditFenceLines(string markdown, int containerCount, string describedAs)
    {
        var fenceLines = 0;

        foreach (var (number, text) in ContentBlocks.LinesOutsideFencedCode(markdown))
        {
            var match = FenceLikePattern().Match(text);
            if (!match.Success)
            {
                continue;
            }

            var colons = match.Groups[1].Value.Length;
            if (colons < 3)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' has '{text.Trim()}' on line {number + 1}, which starts "
                    + $"with {colons} colons. A reader-choice gate needs three: ':::{OutlookName}' to open "
                    + "and ':::' to close. With two it renders as ordinary visible text.");
            }

            fenceLines++;
        }

        var expected = containerCount * 2;
        if (fenceLines == expected)
        {
            return;
        }

        throw new FormatException(
            $"Content page '{describedAs}' has {fenceLines} ':::' line(s) but {containerCount} "
            + $"':::{OutlookName}' gate(s) parsed, so {expected} were expected. Either a fence is "
            + "indented — four spaces or a tab makes it a code block, which renders the words in full "
            + "AND hides them from the reading check — or an opening ':::' is never closed by a ':::' "
            + "of its own.");
    }
}

/// <summary>
/// Renders a validated <c>:::outlook</c> container as a closed
/// <c>&lt;details&gt;</c> disclosure. No JavaScript: the same
/// <c>&lt;details&gt;</c>/<c>&lt;summary&gt;</c> approach as the hamburger nav
/// (WI-440) and the country picker (WI-457), which is a hard site constraint
/// rather than a preference.
/// </summary>
public sealed class ReaderGateRenderer : HtmlObjectRenderer<CustomContainer>
{
    protected override void Write(HtmlRenderer renderer, CustomContainer container)
    {
        if (!string.Equals(container.Info, ReaderGate.OutlookName, StringComparison.Ordinal))
        {
            // Unreachable through ContentStore.Parse, which validates first.
            // Reaching it means a pipeline was built without that validation,
            // and rendering an unknown container as an open div is exactly the
            // failure this component exists to prevent — so say so loudly
            // rather than degrade into it.
            throw new InvalidOperationException(
                $"ReaderGateRenderer was handed ':::{container.Info}'. Call ReaderGate.Validate "
                + "before rendering — an unvalidated container renders its content unguarded.");
        }

        if (!renderer.EnableHtmlForBlock)
        {
            // A plain-text render (a search snippet, a digest email) has no
            // disclosure to hide behind, so the gated words must not be
            // emitted into one as if they were ordinary body text.
            return;
        }

        renderer.EnsureLine();
        renderer.WriteLine("<section class=\"reader-gate\">");

        renderer.Write("<p class=\"reader-gate__warning\">");
        renderer.WriteEscape(ReaderGate.WarningText);
        renderer.WriteLine("</p>");

        // No `open` attribute, ever. Closed by default is the item.
        renderer.WriteLine("<details class=\"reader-gate__disclosure\">");
        renderer.Write("<summary class=\"reader-gate__toggle\"><span class=\"reader-gate__show\">");
        renderer.WriteEscape(ReaderGate.ShowLabel);
        renderer.Write("</span><span class=\"reader-gate__hide\">");
        renderer.WriteEscape(ReaderGate.HideLabel);
        renderer.WriteLine("</span></summary>");

        renderer.WriteLine("<div class=\"reader-gate__body\">");
        renderer.WriteChildren(container);
        renderer.WriteLine("</div>");

        renderer.WriteLine("</details>");
        renderer.WriteLine("</section>");
    }
}

/// <summary>
/// Replaces Markdig's default custom-container renderer, so a <c>:::</c>
/// block can only ever render as a gate or fail — it can never fall back to
/// the anonymous div.
/// </summary>
public sealed class ReaderGateExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer html)
        {
            return;
        }

        // Markdig renders with the FIRST renderer that accepts the object, so
        // it is not enough to add ours — Markdig's own has to be gone, every
        // time. Setup can run more than once against the same renderer (a
        // renderer reused across pages is an ordinary optimisation), and on
        // the second pass the custom-container extension sees its renderer
        // missing and inserts a fresh one at index 0. Remove-then-insert
        // survives that; a `Contains` early return does not, and the failure
        // mode is a silent <div class="outlook"> with the prognosis open.
        foreach (var stale in html.ObjectRenderers.OfType<HtmlCustomContainerRenderer>().ToList())
        {
            html.ObjectRenderers.Remove(stale);
        }

        if (!html.ObjectRenderers.Contains<ReaderGateRenderer>())
        {
            html.ObjectRenderers.Insert(0, new ReaderGateRenderer());
        }
    }

    /// <summary>
    /// Renders a probe gate and throws unless it came out as a closed
    /// disclosure.
    ///
    /// The removal above is still order-dependent in one direction: registered
    /// BEFORE the custom-container extension, this runs first and Markdig's
    /// renderer is inserted afterwards, winning at index 0. That is a
    /// one-line mistake in a pipeline definition with no visible symptom
    /// except prognosis content published open, so the pipeline checks itself
    /// once at start-up rather than trusting a comment about ordering.
    /// </summary>
    public static MarkdownPipeline Verify(MarkdownPipeline pipeline)
    {
        var html = Markdig.Markdown.ToHtml($":::{ReaderGate.OutlookName}\nProbe.\n:::\n", pipeline);

        if (!html.Contains("<details class=\"reader-gate__disclosure\">", StringComparison.Ordinal)
            || html.Contains($"<div class=\"{ReaderGate.OutlookName}\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The curated-content Markdown pipeline does not render ':::outlook' as a reader-choice "
                + "gate. ReaderGateExtension must be registered AFTER the custom-container extension "
                + $"(UseAdvancedExtensions). Rendered instead:\n{html}");
        }

        return pipeline;
    }
}
