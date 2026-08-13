using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BrainHarbor.ContentCheck;

/// <summary>
/// WI-414: pulls the reader-facing prose out of a Razor page so the same
/// reading-level gate that guards the curated Markdown pages can guard the
/// pages people actually land on.
///
/// This is deliberately not an HTML parser. It needs one thing the grader
/// cares about — where one block of prose ends and the next begins — because
/// running headings into the following paragraph inflates the grade by a level
/// or two (the WI-106 lesson). Everything else (markup, Razor code, attribute
/// values, script and style bodies) is thrown away.
/// </summary>
public static partial class RazorTextExtractor
{
    /// <summary>Tags that end a block of prose. Inline tags (em, strong, a,
    /// span, code) are transparent, so a sentence with a link in it stays one
    /// sentence.</summary>
    private static readonly HashSet<string> BlockTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "div", "section", "article",
        "aside", "header", "footer", "nav", "main", "blockquote", "figcaption",
        "label", "button", "td", "th", "tr", "caption", "summary", "details",
        "legend", "option", "dt", "dd", "form", "fieldset", "br", "hr",
    };

    /// <summary>Their content is code or styling, never prose.</summary>
    private static readonly HashSet<string> SkippedContentTags =
        new(StringComparer.OrdinalIgnoreCase) { "script", "style", "svg" };

    /// <summary>
    /// Block-aware plain text, in the same shape
    /// <see cref="ContentChecker.ExtractSentences"/> produces for Markdown:
    /// one sentence per block, terminator appended when the author left it off
    /// (headings and buttons rarely end in a full stop).
    /// </summary>
    public static string ExtractSentences(string razor)
    {
        var text = RazorComment().Replace(razor, " ");
        text = RazorDirective().Replace(text, " ");
        text = RemoveCodeBlocks(text);

        var result = new StringBuilder();
        var block = new StringBuilder();
        var index = 0;
        string? skippingUntil = null;

        foreach (Match tag in Tag().Matches(text))
        {
            AppendText(text[index..tag.Index]);
            index = tag.Index + tag.Length;

            var name = tag.Groups["name"].Value;
            var isClosing = tag.Value.StartsWith("</", StringComparison.Ordinal);

            if (skippingUntil is not null)
            {
                // Only the matching close tag gets us out; anything between is
                // code, not prose.
                if (isClosing && name.Equals(skippingUntil, StringComparison.OrdinalIgnoreCase))
                {
                    skippingUntil = null;
                }

                block.Clear();
                continue;
            }

            if (!isClosing && SkippedContentTags.Contains(name)
                && !tag.Value.EndsWith("/>", StringComparison.Ordinal))
            {
                skippingUntil = name;
                block.Clear();
                continue;
            }

            if (BlockTags.Contains(name))
            {
                Flush();
            }
        }

        AppendText(text[index..]);
        Flush();
        return result.ToString();

        void AppendText(string raw)
        {
            if (skippingUntil is not null)
            {
                return;
            }

            var cleaned = CleanProse(raw);
            if (cleaned.Length == 0)
            {
                return;
            }

            if (block.Length > 0)
            {
                block.Append(' ');
            }

            block.Append(cleaned);
        }

        void Flush()
        {
            var sentence = block.ToString().Trim();
            block.Clear();
            if (sentence.Length == 0)
            {
                return;
            }

            result.Append(sentence);
            if (sentence[^1] is not ('.' or '!' or '?' or ':'))
            {
                result.Append('.');
            }

            result.Append(' ');
        }
    }

    /// <summary>
    /// Strips Razor expressions and control flow from a run of text, leaving
    /// the words a reader sees. A value like <c>@Model.TotalCount</c> is a
    /// number at runtime, so removing it is right: grading "@Model.TotalCount"
    /// as a word would say nothing about the sentence around it.
    /// </summary>
    private static string CleanProse(string raw)
    {
        var text = RemoveExpressionsAndControlFlow(raw);
        text = ControlFlowPunctuation().Replace(text, " ");
        text = WebUtility.HtmlDecode(text);
        return Whitespace().Replace(text, " ").Trim();
    }

    /// <summary>
    /// Removes Razor expressions (<c>@Model.Foo</c>, <c>@(expr)</c>) and C#
    /// control flow (<c>@if (…)</c>, <c>else if (…)</c>, <c>@foreach (…)</c>).
    ///
    /// A scanner rather than a regex because the condition can contain nested
    /// parentheses, and because a space between the keyword and the bracket —
    /// <c>@if (x)</c> — used to leave the whole condition behind as "prose".
    /// That graded _StageBadge.cshtml, a partial with no reader-facing words at
    /// all, at grade 18: nonsense findings are how a gate gets ignored.
    ///
    /// A bare keyword is only removed when a bracket follows it, so ordinary
    /// sentences ("call us if you need help") are untouched.
    /// </summary>
    private static string RemoveExpressionsAndControlFlow(string text)
    {
        var result = new StringBuilder(text.Length);
        var i = 0;

        while (i < text.Length)
        {
            if (text[i] == '@')
            {
                i++;
                if (i < text.Length && text[i] == '@')
                {
                    // "@@" is an escaped at-sign — real text, e.g. an address.
                    result.Append('@');
                    i++;
                    continue;
                }

                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] is '_' or '.'))
                {
                    i++;
                }

                i = SkipBracketsAfterSpaces(text, i);
                result.Append(' ');
                continue;
            }

            if (char.IsLetter(text[i]) && (i == 0 || !char.IsLetterOrDigit(text[i - 1])))
            {
                var end = i;
                while (end < text.Length && char.IsLetter(text[end]))
                {
                    end++;
                }

                var word = text[i..end];
                if (ControlKeywords.Contains(word))
                {
                    var afterBrackets = SkipBracketsAfterSpaces(text, end);
                    if (afterBrackets != end)
                    {
                        // Only a keyword WITH a condition is control flow.
                        i = afterBrackets;
                        result.Append(' ');
                        continue;
                    }
                }

                result.Append(word);
                i = end;
                continue;
            }

            result.Append(text[i]);
            i++;
        }

        return result.ToString();
    }

    private static readonly HashSet<string> ControlKeywords = new(StringComparer.Ordinal)
    {
        "if", "for", "foreach", "while", "switch", "using", "lock", "catch",
    };

    /// <summary>Index just past a balanced (…) group that follows optional
    /// spaces, or <paramref name="from"/> unchanged when there is none.</summary>
    private static int SkipBracketsAfterSpaces(string text, int from)
    {
        var i = from;
        while (i < text.Length && text[i] == ' ')
        {
            i++;
        }

        if (i >= text.Length || text[i] != '(')
        {
            return from;
        }

        var depth = 0;
        for (; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')' && --depth == 0)
            {
                return i + 1;
            }
        }

        return text.Length;
    }

    /// <summary>
    /// Removes <c>@{ ... }</c> and <c>@functions { ... }</c> bodies, matching
    /// braces so a nested block does not end the removal early.
    /// </summary>
    private static string RemoveCodeBlocks(string text)
    {
        var result = new StringBuilder(text.Length);
        var i = 0;

        while (i < text.Length)
        {
            var at = text.IndexOf('@', i);
            if (at < 0)
            {
                result.Append(text, i, text.Length - i);
                break;
            }

            // The brace may follow the @ directly (@{) or after a keyword
            // (@functions {, @code {).
            var brace = at + 1;
            while (brace < text.Length && (char.IsLetter(text[brace]) || text[brace] == ' '))
            {
                brace++;
            }

            if (brace >= text.Length || text[brace] != '{')
            {
                result.Append(text, i, at - i + 1);
                i = at + 1;
                continue;
            }

            result.Append(text, i, at - i);

            var depth = 0;
            var j = brace;
            for (; j < text.Length; j++)
            {
                if (text[j] == '{')
                {
                    depth++;
                }
                else if (text[j] == '}' && --depth == 0)
                {
                    j++;
                    break;
                }
            }

            i = j;
        }

        return result.ToString();
    }

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)]
    private static partial Regex RazorComment();

    /// <summary>
    /// A directive line — <c>@model IndexModel</c>, <c>@using …</c>,
    /// <c>@inject Foo Bar</c>. Stripping only the <c>@word</c> would leave the
    /// type name behind to be graded as if someone wrote it for a reader.
    /// </summary>
    [GeneratedRegex(
        @"^[ \t]*@(model|using|inject|page|namespace|inherits|implements|attribute|typeparam|addTagHelper|removeTagHelper|tagHelperPrefix|preservewhitespace)\b.*$",
        RegexOptions.Multiline)]
    private static partial Regex RazorDirective();

    [GeneratedRegex(@"<\/?(?<name>[A-Za-z][A-Za-z0-9-]*)(\s[^>]*)?\/?>", RegexOptions.Singleline)]
    private static partial Regex Tag();

    /// <summary>Braces and semicolons left behind by stripped control flow —
    /// never prose, and a stray "{" would be counted as a word.</summary>
    [GeneratedRegex(@"[{}]|(?<=\s);(?=\s)")]
    private static partial Regex ControlFlowPunctuation();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
