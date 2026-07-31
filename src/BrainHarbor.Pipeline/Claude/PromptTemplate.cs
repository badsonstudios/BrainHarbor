using System.Text.RegularExpressions;

namespace BrainHarbor.Pipeline.Claude;

/// <summary>
/// A versioned prompt template (CLAUDE.md: templates in Prompts/ are versioned
/// artifacts; the version is stamped on every item so a summary can always be
/// traced to the prompt that produced it). Format: a first line
/// <c>version: classify-v1</c>, then the body with <c>{{placeholder}}</c>
/// slots filled from the item.
/// </summary>
public sealed partial class PromptTemplate
{
    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();

    public string Version { get; }
    public string Body { get; }

    private PromptTemplate(string version, string body)
    {
        Version = version;
        Body = body;
    }

    public static PromptTemplate Parse(string text)
    {
        // Normalize newlines so a template edited on either OS parses the same.
        var normalized = text.Replace("\r\n", "\n");
        var newline = normalized.IndexOf('\n');
        var firstLine = (newline < 0 ? normalized : normalized[..newline]).Trim();

        if (!firstLine.StartsWith("version:", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("A prompt template must begin with a 'version: <id>' line.");
        }

        var version = firstLine["version:".Length..].Trim();
        if (version.Length == 0)
        {
            throw new FormatException("A prompt template's version id must not be empty.");
        }

        var body = newline < 0 ? "" : normalized[(newline + 1)..].TrimStart('\n');
        return new PromptTemplate(version, body);
    }

    public static PromptTemplate Load(string path) => Parse(File.ReadAllText(path));

    /// <summary>
    /// Fills every <c>{{placeholder}}</c> from <paramref name="values"/>.
    /// Throws if the template names a placeholder no value was supplied for —
    /// a half-rendered prompt (with a literal {{abstract}} in it) is a bug we
    /// want at build/test time, not a bad summary in production.
    /// </summary>
    public string Render(IReadOnlyDictionary<string, string> values)
    {
        var missing = new List<string>();
        var rendered = Placeholder().Replace(Body, match =>
        {
            var key = match.Groups[1].Value;
            if (values.TryGetValue(key, out var value))
            {
                return value;
            }
            missing.Add(key);
            return match.Value;
        });

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Prompt '{Version}' has unfilled placeholder(s): {string.Join(", ", missing.Distinct())}.");
        }

        // Belt and suspenders: catch anything brace-shaped the strict
        // {{word}} pattern didn't recognize — e.g. a typo'd `{{ abstract }}`
        // with spaces — so a half-rendered prompt can never reach the model.
        if (rendered.Contains("{{", StringComparison.Ordinal))
        {
            var at = rendered.IndexOf("{{", StringComparison.Ordinal);
            var snippet = rendered.Substring(at, Math.Min(30, rendered.Length - at));
            throw new InvalidOperationException(
                $"Prompt '{Version}' still contains an unrendered placeholder near: {snippet}");
        }

        return rendered;
    }
}
