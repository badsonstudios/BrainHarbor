namespace BrainHarbor.Web.Models;

/// <summary>
/// The patient-facing readiness score (1-10): how close a finding is to being
/// something a person can actually get. Set by the pipeline and clamped there
/// by research stage (content-pipeline.md §9). This turns the raw number into
/// the band label + accessible description a reader sees, so meaning comes from
/// words and a filled bar, never color alone (same accessibility contract as
/// the stage badge).
/// </summary>
public sealed record ReadinessBadge(int Score, string Label, string BandExplanation)
{
    public const int Max = 10;

    /// <summary>CSS modifier by band, for styling without carrying meaning in color.</summary>
    public string CssClass => "readiness readiness--" + Score switch
    {
        >= 9 => "available",
        >= 7 => "late-trial",
        >= 5 => "early-trial",
        4 => "watched",
        3 => "review",
        2 => "animal",
        _ => "lab",
    };

    /// <summary>Full label read to assistive tech: the number, the band, and why.</summary>
    public string AriaLabel => $"Readiness {Score} out of {Max}. {Label}. {BandExplanation}";

    public static ReadinessBadge For(int score)
    {
        var clamped = Math.Clamp(score, 1, Max);
        var (label, explanation) = clamped switch
        {
            >= 9 => ("Available now",
                "Approved and in use, or standard care a doctor can offer today."),
            >= 7 => ("In late human trials",
                "Being tested in large trials in people. Not yet approved or standard."),
            >= 5 => ("In early human trials",
                "First tests in people, mostly checking safety and dose."),
            4 => ("Watched in people",
                "Seen in people through observation, not yet tested as a treatment."),
            3 => ("Expert review",
                "A summary of where the science is heading, not a new result."),
            2 => ("Animal studies",
                "Done in mice or other animals only, not in people."),
            _ => ("Lab or idea stage",
                "Cells in a dish, or an early concept."),
        };
        return new ReadinessBadge(clamped, label, explanation);
    }
}
