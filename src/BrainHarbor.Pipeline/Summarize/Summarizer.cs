using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Summarize;

/// <summary>What the summarizer model returns (content-pipeline.md §9 template).</summary>
public sealed record SummarizeOutput
{
    public string PlainTitle { get; init; } = "";
    public string Hook { get; init; } = "";
    public string WhatStudied { get; init; } = "";
    public string WhatFound { get; init; } = "";
    public string Means { get; init; } = "";
    public string DoesntMean { get; init; } = "";

    public bool AllBlocksPresent =>
        !string.IsNullOrWhiteSpace(PlainTitle) && !string.IsNullOrWhiteSpace(Hook) &&
        !string.IsNullOrWhiteSpace(WhatStudied) && !string.IsNullOrWhiteSpace(WhatFound) &&
        !string.IsNullOrWhiteSpace(Means) && !string.IsNullOrWhiteSpace(DoesntMean);

    /// <summary>All prose, for the guardrail checks (numerals, hype, reading level).</summary>
    public string AllProse => string.Join("\n", PlainTitle, Hook, WhatStudied, WhatFound, Means, DoesntMean);
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

        var output = result.Value!;

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
