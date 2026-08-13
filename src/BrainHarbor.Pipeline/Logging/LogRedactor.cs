using System.Text.RegularExpressions;

namespace BrainHarbor.Pipeline.Logging;

/// <summary>
/// WI-417: scrubs secrets on their way to the log FILE.
///
/// The first line of defence is still the level filter in
/// <see cref="PipelineHost.ConfigureLogging"/> — <c>System.Net.Http.HttpClient</c>
/// logs full request URIs at Information and the NCBI key travels in the query
/// string, because E-utilities requires it there. That filter is pinned by a
/// test now, but a filter is a convention and this is a mechanism: console
/// output scrolls away, a file on disk does not, so anything written to one
/// gets scrubbed on the way in.
///
/// Two passes: the configured key values verbatim (whatever logs them), and the
/// shapes a key travels in (<c>api_key=…</c>, an <c>X-BrainHarbor-Key</c> header
/// dump) for keys this process was never told about.
/// </summary>
public sealed partial class LogRedactor
{
    public const string Placeholder = "REDACTED";

    /// <summary>
    /// Below this length a "secret" is far more likely to be a placeholder, a
    /// test stub, or a single character — and blind-replacing "x" everywhere
    /// would shred the log while looking like it worked.
    /// </summary>
    private const int ShortestCredibleSecret = 8;

    private readonly string[] _secrets;

    public LogRedactor(IEnumerable<string?> secrets) =>
        _secrets = [.. secrets
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Trim().Length >= ShortestCredibleSecret)
            .Select(s => s!.Trim())
            // A key reaches a URL percent-encoded (PubMedFetcher escapes every
            // query value), so the literal form is not always the form that
            // gets logged. Today's NCBI keys are hex and escape to themselves;
            // Distinct drops the duplicate when that is the case.
            .SelectMany(s => new[] { s, Uri.EscapeDataString(s) })
            .Distinct(StringComparer.Ordinal)
            // Longest first: if one secret contains another, replacing the
            // short one first would leave a partial match behind.
            .OrderByDescending(s => s.Length)];

    public string Scrub(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        foreach (var secret in _secrets)
        {
            text = text.Replace(secret, Placeholder, StringComparison.Ordinal);
        }

        try
        {
            return KeyShaped().Replace(text, $"${{name}}${{separator}}{Placeholder}");
        }
        catch (RegexMatchTimeoutException)
        {
            // Fail CLOSED. A line that could not be scrubbed does not get
            // written: losing one log line beats writing a key to disk.
            return $"(a log line was dropped: it could not be checked for secrets in time)";
        }
    }

    // name = separator = value, where the value runs to the next delimiter of a
    // query string, a header line, or ordinary prose.
    //
    // This second layer earns its keep even though the level filter already
    // keeps request URIs out of the log: a `Logging:LogLevel` entry with a
    // LONGER category prefix beats a filter added in code, so the filter can be
    // switched off by configuration alone, from a file, without anyone touching
    // Program.cs. This runs regardless.
    //
    // A fixed alternation, one literal separator and one negated class — linear
    // time, no nesting, nothing to backtrack. The timeout is belt-and-braces
    // for the day it gets edited, since it runs over full exception dumps.
    [GeneratedRegex(
        @"(?<name>\b(?:api[_-]?key|apikey|access[_-]?token|X-BrainHarbor-Key|SYNC_API_KEY|NCBI_API_KEY)\b)" +
        @"(?<separator>\s*[=:]\s*)" +
        @"[^\s&""'<>;,\]\)]+",
        RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex KeyShaped();
}
