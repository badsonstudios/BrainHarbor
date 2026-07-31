using System.Text.RegularExpressions;
using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Summarize;

/// <summary>
/// Small style pass so published prose doesn't read as machine-written. Dan's
/// rule: no em dashes, and nothing that screams "an AI wrote this". The prompt
/// asks the model to avoid these; this is the belt-and-suspenders that runs
/// regardless of what the model returns, because a prompt can be ignored and a
/// normalizer can't.
/// </summary>
public static partial class ProseStyle
{
    /// <summary>
    /// Removes em/en dashes (the most common AI tell). A dash between two
    /// numbers becomes " to " (so "10–20" reads "10 to 20", not "10, 20", which
    /// would change the meaning); every other dash becomes a comma. Trailing
    /// double spaces / stray punctuation left behind are tidied.
    /// </summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? "";
        var s = NumberRange().Replace(text, " to ");   // 10–20 -> 10 to 20
        s = OtherDash().Replace(s, ", ");              // parenthetical / contrast -> comma
        s = BeforePunct().Replace(s, "$1");            // " ," -> ","
        s = DoubleComma().Replace(s, ",");             // ", ," -> ","
        s = MultiSpace().Replace(s, " ");              // collapse runs of spaces
        return s.Trim();
    }

    [GeneratedRegex(@"(?<=\d)\s*[—–]\s*(?=\d)")] private static partial Regex NumberRange();
    [GeneratedRegex(@"\s*[—–]\s*")]             private static partial Regex OtherDash();
    [GeneratedRegex(@"\s+([,.;:!?])")]                    private static partial Regex BeforePunct();
    [GeneratedRegex(@",\s*,")]                            private static partial Regex DoubleComma();
    [GeneratedRegex(@"\s{2,}")]                           private static partial Regex MultiSpace();
}

/// <summary>What the summarizer model returns (content-pipeline.md §9 template).</summary>
public sealed record SummarizeOutput
{
    public string PlainTitle { get; init; } = "";
    public string Hook { get; init; } = "";
    public string WhatStudied { get; init; } = "";
    public string WhatFound { get; init; } = "";
    public string Means { get; init; } = "";
    public string DoesntMean { get; init; } = "";
    public int ReadinessScore { get; init; }
    public string ReadinessReason { get; init; } = "";

    public bool AllBlocksPresent =>
        !string.IsNullOrWhiteSpace(PlainTitle) && !string.IsNullOrWhiteSpace(Hook) &&
        !string.IsNullOrWhiteSpace(WhatStudied) && !string.IsNullOrWhiteSpace(WhatFound) &&
        !string.IsNullOrWhiteSpace(Means) && !string.IsNullOrWhiteSpace(DoesntMean) &&
        ReadinessScore is >= Readiness.Min and <= Readiness.Max &&
        !string.IsNullOrWhiteSpace(ReadinessReason);

    /// <summary>All prose, for the guardrail checks (numerals, hype, reading level).</summary>
    public string AllProse =>
        string.Join("\n", PlainTitle, Hook, WhatStudied, WhatFound, Means, DoesntMean, ReadinessReason);
}

/// <summary>
/// A finished summary plus its provenance and whether the automated checks
/// flagged it. A null <see cref="Output"/> means the model call failed — the
/// item stays classified-but-unsummarized (uploaded pending), never guessed.
/// </summary>
public sealed record SummaryResult(
    SummarizeOutput? Output,
    string PromptVersion,
    string? Model,
    bool Flagged,
    IReadOnlyList<string> FlagReasons);

public interface ISummarizer
{
    Task<SummaryResult> SummarizeAsync(FetchedItem item, CancellationToken cancellationToken);
}

/// <summary>
/// WI-304: writes the plain-language summary with the local Claude CLI, then
/// runs the automated safety checks (numeral post-check, banned-phrase scan,
/// reading level) that gate auto-publish. A summary that trips a check is
/// flagged — in Auto mode it waits for a human instead of publishing.
/// </summary>
public sealed class Summarizer(ClaudeCli claude, PromptLibrary prompts, ILogger<Summarizer> logger)
    : ISummarizer
{
    public async Task<SummaryResult> SummarizeAsync(FetchedItem item, CancellationToken cancellationToken)
    {
        var template = prompts.Get("summarize");
        var prompt = template.Render(new Dictionary<string, string>
        {
            ["title"] = item.Title,
            ["abstract"] = item.RawSummary ?? "(no abstract available)",
        });

        var result = await claude.RunJsonAsync<SummarizeOutput>(
            prompt, output => output.AllBlocksPresent, cancellationToken);

        if (!result.Success)
        {
            logger.LogWarning("[{Source}/{Id}] summarization failed ({Reason}) — leaving it unsummarized.",
                item.Source, item.ExternalId, result.FailureReason);
            return new SummaryResult(null, template.Version, null, Flagged: false, []);
        }

        // Strip AI tells (em dashes, etc.) from every block before the checks
        // and the upload — the reader (and the guardrails) see only clean prose.
        var output = result.Value! with
        {
            PlainTitle = ProseStyle.Normalize(result.Value!.PlainTitle),
            Hook = ProseStyle.Normalize(result.Value!.Hook),
            WhatStudied = ProseStyle.Normalize(result.Value!.WhatStudied),
            WhatFound = ProseStyle.Normalize(result.Value!.WhatFound),
            Means = ProseStyle.Normalize(result.Value!.Means),
            DoesntMean = ProseStyle.Normalize(result.Value!.DoesntMean),
            ReadinessReason = ProseStyle.Normalize(result.Value!.ReadinessReason),
            // ReadinessScore stays the model's raw proposal here; the pipeline
            // clamps it against the classified research stage at upload time,
            // where both the score and the stage are known (see PipelineRunner).
        };

        // The source the numerals must trace back to is the title + abstract.
        var sourceText = $"{item.Title}\n{item.RawSummary}";
        var checks = Guardrails.Check(output.AllProse, sourceText);

        if (!checks.Passed)
        {
            logger.LogInformation("[{Source}/{Id}] summary flagged: {Reasons}",
                item.Source, item.ExternalId, string.Join("; ", checks.Reasons));
        }

        return new SummaryResult(output, template.Version, result.Model, !checks.Passed, checks.Reasons);
    }
}
