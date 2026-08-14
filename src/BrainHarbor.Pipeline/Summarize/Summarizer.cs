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
    IReadOnlyList<Guardrails.Flag> FlagReasons)
{
    /// <summary>
    /// WI-413: the CLI never answered, so this is about the run and not the
    /// item. It matters here as well as in the classifier because an item
    /// uploaded classified-but-unsummarized is never summarized again — a
    /// known item costs no model call on later runs — so it would sit in the
    /// review queue without a summary permanently.
    /// </summary>
    public bool Unavailable { get; init; }
}

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
    /// <summary>
    /// A clinical trial gets its own prompt (WI-402). The research template
    /// asks "what did they find", and an open trial has found nothing yet —
    /// asking that question of a trial description is an invitation to invent
    /// an outcome, which is the exact failure mode the guardrails exist for.
    /// </summary>
    internal static string PromptNameFor(FetchedItem item) =>
        item.SourceKind == "trial_update" ? "summarize-trial" : "summarize";

    /// <summary>
    /// The text a number in the summary must be traceable to. For a trial that
    /// includes its phase and status: the prompt scores readiness by phase, so
    /// "Phase 2" legitimately appears in the output and would otherwise trip
    /// the numeral post-check as an invented figure.
    /// </summary>
    internal static string SourceTextFor(FetchedItem item)
    {
        var source = $"{item.Title}\n{item.RawSummary}";
        return item.Trial is { } trial
            ? $"{source}\n{trial.Phase}\n{trial.OverallStatus}"
            : source;
    }

    public async Task<SummaryResult> SummarizeAsync(FetchedItem item, CancellationToken cancellationToken)
    {
        var template = prompts.Get(PromptNameFor(item));

        var fields = new Dictionary<string, string>
        {
            ["title"] = item.Title,
            ["abstract"] = item.RawSummary ?? "(no abstract available)",
        };

        if (item.Trial is { } facts)
        {
            // The prompt scores by phase, so it has to be TOLD the phase. Left
            // to infer it from prose, the model would be guessing at exactly
            // the field the score depends on.
            fields["phase"] = facts.Phase ?? "not given";
            fields["status"] = facts.OverallStatus ?? "not given";
        }

        var prompt = template.Render(fields);

        var result = await claude.RunJsonAsync<SummarizeOutput>(
            prompt, output => output.AllBlocksPresent, cancellationToken);

        if (!result.Success)
        {
            if (result.Unavailable)
            {
                logger.LogWarning(
                    "[{Source}/{Id}] the summarizer is not answering ({Reason}) — stopping this source.",
                    item.Source, item.ExternalId, result.FailureReason);
                return new SummaryResult(null, template.Version, null, Flagged: false, [])
                {
                    Unavailable = true,
                };
            }

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

        var checks = Guardrails.Check(output.AllProse, SourceTextFor(item));

        if (!checks.Passed)
        {
            logger.LogInformation("[{Source}/{Id}] summary flagged: {Reasons}",
                item.Source, item.ExternalId, string.Join("; ", checks.Reasons));
        }

        return new SummaryResult(output, template.Version, result.Model, !checks.Passed, checks.Reasons);
    }
}
