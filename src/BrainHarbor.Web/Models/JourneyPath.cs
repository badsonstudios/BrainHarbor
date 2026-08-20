namespace BrainHarbor.Web.Models;

/// <summary>
/// The reader-facing evidence indicator (journey handoff, 2026-08-19).
///
/// Replaces BOTH things the site used to show: the stage badge's dot-meter and
/// the 1-10 readiness dial. The handoff's argument, which is the reason this
/// type exists at all:
///
/// - A 10-point scale has no plain-language meaning at any single value. Nobody
///   can say what 7 out of 10 is. Every rung here has a name a patient can
///   repeat to their doctor.
/// - A percentage would have been worse. "60% ready" reads as progress toward a
///   finish line ON A SCHEDULE, and 100% reads as "cure available now". Most lab
///   findings never reach people. A position on a road implies no timetable.
///
/// Everything is derived from <see cref="ResearchStage"/>, so a content author
/// can never type a number — which was the other failure mode of the old dial.
/// </summary>
public sealed record JourneyPath
{
    /// <summary>
    /// The four rungs, in order, always rendered in full. The unreached ones
    /// are the point: they show how much road is left. Dropping them to "save
    /// space" would turn the path back into a progress bar.
    /// </summary>
    public static readonly string[] Steps =
        ["Lab cells", "Animals", "Review", "Tested in people"];

    public const int StepCount = 4;

    private JourneyPath(int currentStep, string ariaLabel)
    {
        CurrentStep = currentStep;
        AriaLabel = ariaLabel;
    }

    /// <summary>1-4. Steps below this are reached; this one is current.</summary>
    public int CurrentStep { get; }

    /// <summary>
    /// One clean sentence for assistive tech. The list is role="img" so a
    /// screen reader gets this instead of four orphaned words, and the label is
    /// built here rather than scraped from the visual text.
    /// </summary>
    public string AriaLabel { get; }

    public string CssFor(int step) => step < CurrentStep
        ? "journey__step is-done"
        : step == CurrentStep
            ? "journey__step is-current"
            : "journey__step";

    /// <summary>
    /// The path for a stage, or null when the item is not on the evidence
    /// ladder at all. Trials, news and preprints are NOT findings — giving them
    /// a path would imply they sit somewhere on the evidence scale, and they
    /// do not. Those render <see cref="StageNote"/> instead.
    /// </summary>
    public static JourneyPath? For(ResearchStage stage) => stage switch
    {
        ResearchStage.EarlyResearchLabCells =>
            new(1, "Early research in lab cells. Stage 1 of 4."),
        ResearchStage.EarlyResearchAnimals =>
            new(2, "Early research in animals. Stage 2 of 4."),
        ResearchStage.ReviewOfExistingResearch =>
            new(3, "Review of existing research. Stage 3 of 4."),
        ResearchStage.TestedInPeople =>
            new(4, "Tested in people. Stage 4 of 4 — the strongest evidence we share."),
        _ => null,
    };
}

/// <summary>
/// The strip shown for items that are not findings (trials, news, preprints).
/// A glyph, the stage name, and one line of plain explanation — deliberately
/// NOT a path, so nothing implies these sit on the evidence ladder.
/// </summary>
public sealed record StageNote(string CssClass, string Glyph, string Title, string Detail)
{
    public static StageNote? For(ResearchStage stage) => stage switch
    {
        ResearchStage.NewOrUpdatedTrial => new(
            "stage-note",
            "→",
            "New or updated trial",
            "Not a finding yet — a study now recruiting."),

        ResearchStage.News => new(
            "stage-note stage-note--info",
            "i",
            "News",
            "An announcement, not a study."),

        ResearchStage.Preprint => new(
            "stage-note stage-note--unverified",
            "→",
            "Preprint",
            "Not yet checked by other scientists."),

        _ => null,
    };

    /// <summary>
    /// Read to assistive tech in place of the glyph + two lines, so the strip
    /// is announced as one sentence like the journey path is.
    /// </summary>
    public string AriaLabel => $"{Title}. {Detail}";
}
