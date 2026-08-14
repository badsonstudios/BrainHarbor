namespace BrainHarbor.Safety;

/// <summary>
/// The prose of one summary, and the single definition of how those blocks are
/// assembled for checking.
///
/// This exists because the assembly is load-bearing and was wrong once. The
/// grader is block-aware: it treats each line as its own sentence group, so a
/// plain title (which has no full stop) running into the hook made every
/// summary on the site measure about 0.7 of a grade too hard, and the same
/// defect let a negation in the title excuse a "cure" claim in the hook
/// (WI-415). Anything that assembles these blocks a second time, in a second
/// place, is that bug waiting to happen — so both the pipeline that writes a
/// summary and the site that re-checks a stored one come through here.
/// </summary>
public sealed record SummaryText(
    string? PlainTitle,
    string? Hook,
    string? WhatStudied,
    string? WhatFound,
    string? Means,
    string? DoesntMean,
    string? ReadinessReason)
{
    /// <summary>Every block, one per line, for the guardrail checks.</summary>
    public string AllProse => string.Join(
        "\n",
        PlainTitle ?? "",
        Hook ?? "",
        WhatStudied ?? "",
        WhatFound ?? "",
        Means ?? "",
        DoesntMean ?? "",
        ReadinessReason ?? "");

    /// <summary>
    /// The text a number in the summary must be traceable to.
    ///
    /// A trial carries its phase and status as well: the prompt scores
    /// readiness by phase, so "Phase 2" legitimately appears in the summary and
    /// would otherwise read as an invented figure. Anything re-checking a
    /// stored trial summary has to supply them too, or it will report a numeral
    /// the pipeline was perfectly happy with.
    /// </summary>
    public static string SourceFor(
        string title, string? rawSummary, string? trialPhase = null, string? trialStatus = null)
    {
        var source = $"{title}\n{rawSummary}";
        return trialPhase is null && trialStatus is null
            ? source
            : $"{source}\n{trialPhase}\n{trialStatus}";
    }
}
