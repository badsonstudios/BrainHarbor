using System.Text.RegularExpressions;
using Markdig;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BrainHarbor.Web.Content;

/// <summary>
/// A glossary tooltip in the inline tree — rendered as a native HTML popover
/// (content-pipeline.md §6, WCAG 1.4.13): a focusable, dismissible, touch-OK
/// button plus a definition panel containing the no-JS-era fallback link to
/// /glossary#slug. Browsers without popover support render the definition
/// inline after the term — degraded but never broken.
/// </summary>
public sealed class TermTooltipInline : LeafInline
{
    public required GlossaryTerm Term { get; init; }
    public required string DisplayText { get; init; }
}

public sealed class TermTooltipRenderer : HtmlObjectRenderer<TermTooltipInline>
{
    protected override void Write(HtmlRenderer renderer, TermTooltipInline tooltip)
    {
        var slug = tooltip.Term.Slug;
        renderer.Write("<button type=\"button\" class=\"term\" popovertarget=\"def-");
        renderer.WriteEscape(slug);
        renderer.Write("\">");
        renderer.WriteEscape(tooltip.DisplayText);
        renderer.Write("</button><span id=\"def-");
        renderer.WriteEscape(slug);
        renderer.Write("\" popover class=\"term-definition\"><strong>");
        renderer.WriteEscape(tooltip.Term.Term);
        renderer.Write(":</strong> ");
        renderer.WriteEscape(tooltip.Term.Definition);
        renderer.Write(" <a href=\"/glossary#");
        renderer.WriteEscape(slug);
        renderer.Write("\">See the glossary</a></span>");
    }
}

/// <summary>Registers the tooltip renderer; marking happens post-parse via GlossaryMarker.</summary>
public sealed class GlossaryTooltipExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer html && !html.ObjectRenderers.Contains<TermTooltipRenderer>())
        {
            html.ObjectRenderers.Add(new TermTooltipRenderer());
        }
    }
}

/// <summary>
/// WI-105: marks the FIRST occurrence of each glossary term (or alias) per
/// page with a tooltip. Rules (content-pipeline.md §6):
/// - paragraphs only — never headings, links, or code;
/// - whole-word, case-insensitive; longest name wins at the same position;
/// - escape hatches: %%text%% renders that occurrence plain (the next
///   occurrence still gets the tooltip); !%term% anywhere on the page
///   suppresses that term for the whole page. Both markers are removed.
/// </summary>
public static partial class GlossaryMarker
{
    [GeneratedRegex("%%(.+?)%%")]
    private static partial Regex EscapeSpan();

    [GeneratedRegex("!%(.+?)%")]
    private static partial Regex SuppressMarker();

    public static void Mark(MarkdownDocument document, IReadOnlyList<GlossaryTerm> terms)
    {
        var suppressed = CollectAndStripSuppressions(document);

        var matchers = BuildMatchers(terms, suppressed);
        var tooltippedSlugs = new HashSet<string>();

        if (matchers.Count > 0)
        {
            foreach (var paragraph in document.Descendants().OfType<ParagraphBlock>().ToList())
            {
                if (paragraph.Inline is null)
                {
                    continue;
                }

                // Source-wrapped lines split one sentence into multiple
                // literals — merge across soft breaks first, or multi-word
                // terms like "IDH gene change" silently never match.
                MergeSoftBreakRuns(paragraph.Inline);

                foreach (var literal in paragraph.Inline.Descendants<LiteralInline>().ToList())
                {
                    if (HasLinkAncestor(literal))
                    {
                        continue;
                    }

                    ProcessLiteral(literal, matchers, tooltippedSlugs);
                }
            }
        }

        StripRemainingEscapes(document);
    }

    private static HashSet<string> CollectAndStripSuppressions(MarkdownDocument document)
    {
        var suppressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var literal in document.Descendants<LiteralInline>().ToList())
        {
            var text = literal.Content.ToString();
            if (!text.Contains("!%"))
            {
                continue;
            }

            var stripped = SuppressMarker().Replace(text, match =>
            {
                suppressed.Add(match.Groups[1].Value.Trim());
                return string.Empty;
            });
            literal.Content = new StringSlice(stripped);
        }

        return suppressed;
    }

    private sealed record Matcher(Regex Pattern, int NameLength, GlossaryTerm Term);

    private static List<Matcher> BuildMatchers(
        IReadOnlyList<GlossaryTerm> terms, HashSet<string> suppressed)
    {
        var matchers = new List<Matcher>();
        foreach (var term in terms)
        {
            var names = new List<string> { term.Term };
            names.AddRange(term.Aliases);

            if (names.Any(suppressed.Contains))
            {
                continue;
            }

            foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                // Lookarounds instead of \b: hyphens count as word-joiners,
                // so "non-IDH-mutant" is NOT a match for "IDH-mutant" —
                // a tooltip there would assert the opposite of the text.
                matchers.Add(new Matcher(
                    new Regex($@"(?<![\w-]){Regex.Escape(name)}(?![\w-])",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                    name.Length,
                    term));
            }
        }

        // Longest name first so "IDH gene change" beats "IDH" at one position.
        return [.. matchers.OrderByDescending(m => m.NameLength)];
    }

    /// <summary>
    /// Collapses literal + soft line break + literal sibling runs into one
    /// literal (joined with a space — HTML renders both identically), so
    /// terms can match across source line wraps. Hard breaks (&lt;br&gt;)
    /// are kept. Terms split by emphasis (IDH *gene* change) still won't
    /// match — keep glossary terms unformatted in source.
    /// </summary>
    private static void MergeSoftBreakRuns(ContainerInline container)
    {
        var child = container.FirstChild;
        while (child is not null)
        {
            if (child is ContainerInline nested)
            {
                MergeSoftBreakRuns(nested);
            }

            if (child is LiteralInline first &&
                child.NextSibling is LineBreakInline { IsHard: false } softBreak &&
                softBreak.NextSibling is LiteralInline second)
            {
                first.Content = new StringSlice(
                    first.Content.ToString().TrimEnd() + " " + second.Content.ToString().TrimStart());
                softBreak.Remove();
                second.Remove();
                continue; // re-check the same node — runs can be longer than two
            }

            child = child.NextSibling;
        }
    }

    private static bool HasLinkAncestor(Inline inline)
    {
        for (var parent = inline.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is LinkInline)
            {
                return true;
            }
        }
        return false;
    }

    private static void ProcessLiteral(
        LiteralInline literal, List<Matcher> matchers, HashSet<string> tooltippedSlugs)
    {
        var text = literal.Content.ToString();

        // Split out %%...%% protected spans first.
        var segments = new List<(string Text, bool Protected)>();
        var position = 0;
        foreach (Match escape in EscapeSpan().Matches(text))
        {
            if (escape.Index > position)
            {
                segments.Add((text[position..escape.Index], false));
            }
            segments.Add((escape.Groups[1].Value, true));
            position = escape.Index + escape.Length;
        }
        if (position < text.Length)
        {
            segments.Add((text[position..], false));
        }

        var nodes = new List<Inline>();
        var changed = segments.Any(s => s.Protected);

        foreach (var (segmentText, isProtected) in segments)
        {
            if (isProtected)
            {
                nodes.Add(new LiteralInline(segmentText));
                continue;
            }

            var remaining = segmentText;
            while (remaining.Length > 0)
            {
                var best = FindEarliestMatch(remaining, matchers, tooltippedSlugs);
                if (best is null)
                {
                    nodes.Add(new LiteralInline(remaining));
                    break;
                }

                var (match, matcher) = best.Value;
                changed = true;
                if (match.Index > 0)
                {
                    nodes.Add(new LiteralInline(remaining[..match.Index]));
                }
                nodes.Add(new TermTooltipInline { Term = matcher.Term, DisplayText = match.Value });
                tooltippedSlugs.Add(matcher.Term.Slug);
                remaining = remaining[(match.Index + match.Length)..];
            }
        }

        if (!changed)
        {
            return;
        }

        Inline current = literal;
        foreach (var node in nodes)
        {
            current.InsertAfter(node);
            current = node;
        }
        literal.Remove();
    }

    private static (Match Match, Matcher Matcher)? FindEarliestMatch(
        string text, List<Matcher> matchers, HashSet<string> tooltippedSlugs)
    {
        (Match Match, Matcher Matcher)? best = null;
        foreach (var matcher in matchers)
        {
            if (tooltippedSlugs.Contains(matcher.Term.Slug))
            {
                continue;
            }

            var match = matcher.Pattern.Match(text);
            if (!match.Success)
            {
                continue;
            }

            // Matchers are longest-first, so strictly-earlier wins and ties
            // keep the longer name.
            if (best is null || match.Index < best.Value.Match.Index)
            {
                best = (match, matcher);
            }
        }
        return best;
    }

    private static void StripRemainingEscapes(MarkdownDocument document)
    {
        foreach (var literal in document.Descendants<LiteralInline>().ToList())
        {
            var text = literal.Content.ToString();
            if (text.Contains("%%"))
            {
                literal.Content = new StringSlice(EscapeSpan().Replace(text, "$1"));
            }
        }
    }
}
