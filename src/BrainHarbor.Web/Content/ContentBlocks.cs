using System.Collections.Frozen;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>
/// One shared content block: a Markdown fragment several curated pages
/// include, plus the sources its claims trace to. Blocks are NOT pages —
/// they have no title, no URL, and are never served on their own.
/// </summary>
public sealed record ContentBlock(string Name, string Markdown, IReadOnlyList<ContentSource> Sources);

/// <summary>
/// The blocks available to a page, plus the ones that failed to parse.
/// Broken blocks are carried rather than thrown, so one bad file cannot take
/// down /about and /privacy — pages that include it fail loudly and by name;
/// pages that don't are unaffected.
/// </summary>
public sealed record ContentBlockSet(
    IReadOnlyDictionary<string, ContentBlock> Blocks,
    IReadOnlyDictionary<string, string> Errors)
{
    public static readonly ContentBlockSet Empty = new(
        FrozenDictionary<string, ContentBlock>.Empty,
        FrozenDictionary<string, string>.Empty);
}

/// <summary>
/// WI-501: the include mechanism for curated pages. Six blocks recur across
/// the 24 tumor hubs, and the retired-name crosswalk is the content most
/// likely to need correcting later — WHO and cIMPACT-NOW move, and a
/// copy-pasted crosswalk means 24 places to fix it. Writing it once is the
/// difference between a correction and a migration.
///
/// A directive is a line whose ENTIRE content is <c>[BLOCK-NAME]</c>. That
/// strictness leaves a real Markdown link <c>[text](url)</c> and a bracketed
/// aside mid-sentence alone. Matching is done line by line rather than with
/// one regex over the whole document, for three reasons that each caused a
/// silent wrong page in review: CRLF files (a multiline <c>$</c> will not
/// match before <c>\r</c>, so the directive rendered as literal bracket text
/// with no error at all), fenced code blocks (a page documenting the include
/// syntax would expand it), and indentation (a block spliced into a list item
/// has to be indented to stay in the list).
/// </summary>
public static partial class ContentBlocks
{
    [GeneratedRegex(@"^([ \t]*)\[([A-Z0-9][A-Z0-9-]*)\][ \t]*$")]
    private static partial Regex DirectivePattern();

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex NamePattern();

    // A block including a block is fine (the caregiver block wants the
    // seizure block). Deep nesting is not: past this, an author has built a
    // structure nobody can follow back to the page it renders on.
    private const int MaxDepth = 5;

    // Backstop against fan-out, which the depth cap alone does not bound:
    // five levels of ten directives each is 100,000 copies. A tumor hub is a
    // few tens of kilobytes, so this only ever fires on a mistake.
    private const int MaxComposedLength = 1_000_000;

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>The block file name a directive resolves to: [CROSSWALK] → crosswalk.</summary>
    public static string ToBlockName(string directive) => directive.ToLowerInvariant();

    /// <summary>
    /// Every block name a fragment includes directly, in source order. Used
    /// by ContentCheck to find blocks no page reaches — a block nothing
    /// includes is never graded by any page, so it would rot ungraded.
    /// </summary>
    public static IReadOnlyList<string> DirectBlockNames(string markdown)
    {
        var names = new List<string>();
        foreach (var (_, name, _) in Directives(markdown))
        {
            names.Add(name);
        }
        return names;
    }

    /// <summary>
    /// Resolves every include in <paramref name="markdown"/>, recursively.
    /// Returns the composed Markdown and the sources contributed by the
    /// blocks that were pulled in (in first-seen order, deduplicated by URL).
    ///
    /// Throws <see cref="FormatException"/> on a missing block, a broken
    /// block, or an include cycle. A missing block must never render as an
    /// empty section: on a medical page, silence reads as "there is nothing
    /// to say here".
    /// </summary>
    public static (string Markdown, IReadOnlyList<ContentSource> Sources) Compose(
        string markdown, ContentBlockSet blocks, string describedAs)
    {
        if (!HasDirective(markdown))
        {
            return (markdown, []);
        }

        var sources = new List<ContentSource>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var composed = Expand(markdown, blocks, describedAs, [], 0, sources, seen);
        return (composed, sources);
    }

    private static bool HasDirective(string markdown)
    {
        foreach (var _ in Directives(markdown))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Walks the lines of a fragment and yields (lineIndex, blockName,
    /// indent) for each directive, skipping fenced code blocks.
    /// </summary>
    private static IEnumerable<(int Line, string Name, string Indent)> Directives(string markdown)
    {
        var lines = SplitLines(markdown);
        var fence = default(FenceState);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];

            if (fence.IsOpen)
            {
                if (fence.Closes(line))
                {
                    fence = default;
                }
                continue;
            }

            if (FenceState.TryOpen(line, out var opened))
            {
                fence = opened;
                continue;
            }

            var match = DirectivePattern().Match(line);
            if (match.Success)
            {
                yield return (index, ToBlockName(match.Groups[2].Value), match.Groups[1].Value);
            }
        }
    }

    private static string[] SplitLines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private static string Expand(
        string markdown,
        ContentBlockSet blocks,
        string describedAs,
        List<string> chain,
        int depth,
        List<ContentSource> sources,
        HashSet<string> seenUrls)
    {
        if (depth > MaxDepth)
        {
            throw new FormatException(
                $"Content page '{describedAs}' nests shared blocks more than {MaxDepth} deep: {Chain(chain)}.");
        }

        var lines = SplitLines(markdown);
        var directives = Directives(markdown).ToDictionary(d => d.Line, d => (d.Name, d.Indent));
        if (directives.Count == 0)
        {
            return markdown;
        }

        var result = new StringBuilder();
        for (var index = 0; index < lines.Length; index++)
        {
            if (!directives.TryGetValue(index, out var directive))
            {
                result.Append(lines[index]).Append('\n');
                continue;
            }

            var (name, indent) = directive;
            var block = Resolve(name, blocks, describedAs, chain);

            foreach (var source in block.Sources)
            {
                // Merged into the including page's front matter so the
                // block's sources live in ONE place too. Copying them into 24
                // pages would move the drift problem rather than solve it.
                // Keyed by URL where there is one; a print source with no URL
                // is kept rather than silently dropped, because "every claim
                // must trace" is the rule a vanished citation breaks.
                var key = string.IsNullOrWhiteSpace(source.Url) ? $"title:{source.Title}" : source.Url;
                if (seenUrls.Add(key))
                {
                    sources.Add(source);
                }
            }

            var expanded = Expand(
                block.Markdown, blocks, describedAs, [.. chain, name], depth + 1, sources, seenUrls);

            // Blank lines around the splice, and the directive's own
            // indentation carried onto every line. Without the blank lines a
            // block fuses into the neighbouring paragraph by lazy
            // continuation, which turns the crosswalk TABLE into pipe-mangled
            // prose with no error anywhere.
            result.Append('\n');
            foreach (var line in SplitLines(expanded))
            {
                result.Append(line.Length == 0 ? line : indent + line).Append('\n');
            }
            result.Append('\n');

            if (result.Length > MaxComposedLength)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' composes to more than {MaxComposedLength:N0} characters — "
                    + $"a shared block is almost certainly included in a loop: {Chain([.. chain, name])}.");
            }
        }

        return result.ToString();
    }

    private static ContentBlock Resolve(
        string name, ContentBlockSet blocks, string describedAs, List<string> chain)
    {
        if (chain.Contains(name, StringComparer.Ordinal))
        {
            throw new FormatException(
                $"Content page '{describedAs}' has an include cycle: {Chain([.. chain, name])}.");
        }

        if (blocks.Errors.TryGetValue(name, out var error))
        {
            throw new FormatException(
                $"Content page '{describedAs}' includes block '{name}', which cannot be read: {error}");
        }

        if (blocks.Blocks.TryGetValue(name, out var block))
        {
            return block;
        }

        var available = blocks.Blocks.Count == 0
            ? "no blocks are loaded"
            : "available: " + string.Join(", ", blocks.Blocks.Keys.OrderBy(k => k, StringComparer.Ordinal));
        throw new FormatException(
            $"Content page '{describedAs}' includes '[{name.ToUpperInvariant()}]' but there is no block "
            + $"'{name}.md' ({available}).");
    }

    private static string Chain(IEnumerable<string> names) => string.Join(" -> ", names);

    /// <summary>True when a file name may be loaded as a block.</summary>
    public static bool IsBlockFileName(string name) => NamePattern().IsMatch(name);

    /// <summary>
    /// Parses one block file. Front matter is OPTIONAL and may carry only
    /// <c>sources</c> — a block has no title, no slug and no disclaimers,
    /// because it is a fragment of a page rather than a page.
    /// </summary>
    public static ContentBlock ParseBlock(string raw, string name)
    {
        if (!IsBlockFileName(name))
        {
            throw new FormatException(
                $"Block file name '{name}' must be a lowercase slug ([a-z0-9-]) — it is what [{name.ToUpperInvariant()}] resolves to.");
        }

        raw = raw.TrimStart('﻿');

        var sources = new List<ContentSource>();
        var body = raw;

        if (raw.StartsWith("---", StringComparison.Ordinal))
        {
            var end = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
            if (end < 0)
            {
                throw new FormatException($"Block '{name}' has an unterminated front matter block.");
            }

            try
            {
                var frontMatter = Yaml.Deserialize<ContentBlockFrontMatter>(raw[3..end].Trim('\r', '\n'));
                sources = frontMatter?.Sources ?? [];
            }
            catch (YamlDotNet.Core.YamlException exception)
            {
                throw new FormatException($"Block '{name}' has invalid front matter: {exception.Message}", exception);
            }

            body = raw[(end + 4)..];
        }

        body = body.Trim('\r', '\n');
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new FormatException(
                $"Block '{name}' has no body — an empty block renders an empty section, which reads as 'there is nothing to say here'.");
        }

        return new ContentBlock(name, body, sources);
    }

    /// <summary>Tracks an open ``` or ~~~ fence so directives inside it are left alone.</summary>
    private readonly record struct FenceState(char Marker, int Length)
    {
        public bool IsOpen => Length > 0;

        public static bool TryOpen(string line, out FenceState state)
        {
            var trimmed = line.TrimStart(' ', '\t');
            var marker = trimmed.Length > 0 ? trimmed[0] : '\0';
            if (marker is '`' or '~')
            {
                var run = trimmed.TakeWhile(c => c == marker).Count();
                if (run >= 3)
                {
                    state = new FenceState(marker, run);
                    return true;
                }
            }

            state = default;
            return false;
        }

        public bool Closes(string line)
        {
            var marker = Marker;
            var trimmed = line.Trim();
            return trimmed.Length >= Length && trimmed.All(c => c == marker);
        }
    }

    private sealed class ContentBlockFrontMatter
    {
        [YamlMember(Alias = "sources")]
        public List<ContentSource> Sources { get; set; } = [];
    }
}
