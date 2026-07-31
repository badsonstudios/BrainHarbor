using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Microsoft.AspNetCore.Html;

namespace BrainHarbor.Web.Content;

/// <summary>
/// WI-306: renders a plain-language summary block (a stored plain-text sentence
/// or two) to HTML with glossary tooltips marked, the same way curated pages
/// get them (content-pipeline.md §6). The pipeline has raw HTML disabled, so a
/// stored block can never inject markup; the only HTML that comes out is the
/// tooltip buttons the glossary marker adds. The first occurrence of each term
/// per block is linked — a fresh marker per call, so terms don't bleed between
/// blocks.
/// </summary>
public sealed class SummaryRenderer(GlossaryStore glossary)
{
    private static readonly MarkdownPipeline Pipeline = BuildPipeline();

    private static MarkdownPipeline BuildPipeline()
    {
        var builder = new MarkdownPipelineBuilder().DisableHtml();

        // DisableHtml() blocks raw HTML, but Markdig still parses markdown link
        // and image syntax — so a stored block with [x](javascript:alert(1)) or
        // ![x](http://evil/pixel) would render a live <a>/<img>. Summaries are
        // generated from UNTRUSTED third-party abstracts through an LLM, so a
        // prompt-injected link is a real (click-gated) stored-XSS / tracking
        // chain. These blocks are plain sentences with no links, so drop the
        // link/image and autolink inline parsers entirely.
        Remove<LinkInlineParser>(builder);
        Remove<AutolinkInlineParser>(builder);

        builder.Use<GlossaryTooltipExtension>();
        return builder.Build();
    }

    private static void Remove<T>(MarkdownPipelineBuilder builder) where T : Markdig.Parsers.InlineParser
    {
        var parser = builder.InlineParsers.Find<T>();
        if (parser is not null)
        {
            builder.InlineParsers.Remove(parser);
        }
    }

    /// <summary>
    /// Renders one block as INLINE html (no wrapping &lt;p&gt;), so the caller
    /// can place it after a lead-in label ("What was studied.") in its own
    /// paragraph. Returns empty for null/blank. Blocks are one to three plain
    /// sentences, so flattening any paragraph wrappers to inline is safe.
    /// </summary>
    public IHtmlContent Render(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return HtmlString.Empty;
        }

        var document = Markdown.Parse(text, Pipeline);
        GlossaryMarker.Mark(document, glossary.GetTerms());

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        // The pipeline wraps each paragraph in <p>…</p>; strip those to inline
        // (paragraph breaks become spaces) so the block sits inside the caller's
        // own element. DisableHtml means the only other markup is the glossary
        // tooltip buttons, which are already inline.
        var html = writer.ToString().Replace("<p>", "").Replace("</p>", " ").Trim();
        return new HtmlString(html);
    }
}
