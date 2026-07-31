namespace BrainHarbor.Pipeline.Summarize;

/// <summary>
/// The readiness score (1-10): how close a finding is to being something a
/// patient can actually get. The model proposes a score while it summarizes,
/// but the model can be over-optimistic — so the score is <b>capped by the
/// research stage</b> the classifier already assigned. A study in cells or mice
/// can never come out as "near the clinic", no matter what the model says.
///
/// Erring LOW is the safe direction for this audience (anti-hype is a hard
/// requirement), so this only ever clamps a score <i>down</i> to the ceiling
/// for its stage; it never raises one.
/// </summary>
public static class Readiness
{
    /// <summary>The lowest and highest scores the scale allows.</summary>
    public const int Min = 1;
    public const int Max = 10;

    /// <summary>
    /// The most a finding at a given research stage is allowed to score. Keys
    /// are the classifier's <c>research_stage</c> values. An unknown or missing
    /// stage is treated conservatively (a low-middle ceiling), because "we're
    /// not sure how far along this is" should never read as "nearly here".
    /// </summary>
    private static readonly IReadOnlyDictionary<string, int> Ceilings = new Dictionary<string, int>
    {
        ["news_other"] = 10,          // could be an approval announcement
        ["human_trial"] = 8,          // late trials at most; approval is beyond a trial report
        ["review_guideline"] = 6,     // a review points a direction; it isn't a new result
        ["observational"] = 5,        // seen in people, not tested as a treatment
        ["preclinical_animal"] = 2,   // mice only
        ["preclinical_cell"] = 2,     // a dish only
    };

    private const int UnknownStageCeiling = 5;

    /// <summary>The ceiling for a stage (the conservative default if unknown).</summary>
    public static int CeilingFor(string? researchStage) =>
        researchStage is not null && Ceilings.TryGetValue(researchStage, out var ceiling)
            ? ceiling
            : UnknownStageCeiling;

    /// <summary>
    /// Clamps a model-proposed score into [1, ceiling-for-stage]. Returns the
    /// final score a reader will see. A score below <see cref="Min"/> (e.g. 0
    /// or a missing value) floors to 1; anything above the stage ceiling drops
    /// to it. <paramref name="wasCapped"/> reports whether the ceiling bit, so
    /// the caller can log a model that tried to over-score.
    /// </summary>
    public static int Clamp(int proposedScore, string? researchStage, out bool wasCapped)
    {
        var ceiling = CeilingFor(researchStage);
        var floored = Math.Clamp(proposedScore, Min, Max);
        wasCapped = floored > ceiling;
        return Math.Min(floored, ceiling);
    }

    /// <inheritdoc cref="Clamp(int, string?, out bool)"/>
    public static int Clamp(int proposedScore, string? researchStage) =>
        Clamp(proposedScore, researchStage, out _);
}
