namespace BrainHarbor.Web.Models;

/// <summary>
/// How far along a piece of research is — the site's core trust taxonomy
/// (content-pipeline.md §stage badges). Rendered by the _StageBadge partial
/// as the handoff's evidence dot-meter: meaning comes from filled marks +
/// words, never color alone.
///
/// Mapping from the data-model.md research_stage column (WI-209 implements):
/// human_trial + observational → TestedInPeople; review_guideline →
/// ReviewOfExistingResearch; preclinical_animal → EarlyResearchAnimals;
/// preclinical_cell → EarlyResearchLabCells; news_other → News. Preprint is
/// detected from source_kind (not research_stage) and always wins; trial
/// items come from the trials source, not research_stage.
/// </summary>
public enum ResearchStage
{
    TestedInPeople,
    ReviewOfExistingResearch,
    EarlyResearchAnimals,
    EarlyResearchLabCells,
    NewOrUpdatedTrial,
    News,
    Preprint,
}

public enum BadgeKind
{
    /// <summary>A research result — shows the 5-step evidence meter.</summary>
    Result,

    /// <summary>Not a finding yet (trials) — square "→" glyph.</summary>
    Progress,

    /// <summary>News — circled "i" glyph.</summary>
    Info,

    /// <summary>Preprint — dashed, empty meter.</summary>
    Unverified,
}

/// <summary>
/// Everything the badge partial needs, derived server-side so the aria-label
/// always matches the visuals (the meter itself is aria-hidden).
/// </summary>
public sealed record StageBadge(
    ResearchStage Stage,
    BadgeKind Kind,
    string Label,
    int EvidenceStrength)
{
    public const int MeterSteps = 5;

    public string CssClass => Kind switch
    {
        BadgeKind.Result => "badge badge--result",
        BadgeKind.Progress => "badge badge--progress",
        BadgeKind.Info => "badge badge--info",
        BadgeKind.Unverified => "badge badge--unverified",
        _ => "badge",
    };

    public string AriaLabel => Kind == BadgeKind.Result
        ? $"{Label}. Evidence strength {EvidenceStrength} of {MeterSteps}."
        : $"{Label}.";

    public static StageBadge For(ResearchStage stage) => stage switch
    {
        ResearchStage.TestedInPeople =>
            new(stage, BadgeKind.Result, "Tested in people", 5),
        ResearchStage.ReviewOfExistingResearch =>
            new(stage, BadgeKind.Result, "Review of existing research", 4),
        ResearchStage.EarlyResearchAnimals =>
            new(stage, BadgeKind.Result, "Early research (animals)", 2),
        ResearchStage.EarlyResearchLabCells =>
            new(stage, BadgeKind.Result, "Early research (lab cells)", 1),
        ResearchStage.NewOrUpdatedTrial =>
            new(stage, BadgeKind.Progress, "New or updated trial", 0),
        ResearchStage.News =>
            new(stage, BadgeKind.Info, "News", 0),
        ResearchStage.Preprint =>
            new(stage, BadgeKind.Unverified, "Preprint — not yet checked by other scientists", 0),
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null),
    };
}

/// <summary>
/// Maps the database's research_stage vocabulary to the UI's ResearchStage
/// (the mapping documented on the enum in WI-201). Preprints win over
/// everything: a preprint is never presented as a settled finding.
/// </summary>
public static class ResearchStageMapper
{
    public static ResearchStage From(string sourceKind, string? researchStage) =>
        sourceKind switch
        {
            "preprint" => ResearchStage.Preprint,
            "trial_update" => ResearchStage.NewOrUpdatedTrial,
            _ => researchStage switch
            {
                "human_trial" or "observational" => ResearchStage.TestedInPeople,
                "review_guideline" => ResearchStage.ReviewOfExistingResearch,
                "preclinical_animal" => ResearchStage.EarlyResearchAnimals,
                "preclinical_cell" => ResearchStage.EarlyResearchLabCells,
                _ => ResearchStage.News,
            },
        };
}
