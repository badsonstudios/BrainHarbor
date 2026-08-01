namespace BrainHarbor.Web.Trials;

/// <summary>
/// The ClinicalTrials.gov vocabulary the SITE needs for its live "near me"
/// query (WI-403).
///
/// Deliberately duplicated from BrainHarbor.Pipeline.Sources.CtGovFetcher
/// rather than shared: the Web app must not take a dependency on the Pipeline
/// assembly (architecture.md §3 — the pipeline runs on Dan's PC against a
/// deployed site). Same reasoning as the readiness ceilings mirrored in
/// SyncRepository.
///
/// **Keep the plain words identical on both sides.** They are stored by the
/// pipeline and compared by the site, so a wording change here that is not
/// mirrored there would quietly stop matching — pinned by a test.
/// </summary>
public static class Registry
{
    /// <summary>The brain-tumor condition query, in the registry's Essie
    /// syntax. Mirrors CtGovFetcher.ConditionQuery.</summary>
    public const string BrainTumorConditionQuery =
        "(glioma OR glioblastoma OR astrocytoma OR oligodendroglioma OR " +
        "\"brain tumor\" OR \"brain tumour\" OR \"brain cancer\" OR " +
        "\"brain neoplasm\" OR \"CNS tumor\" OR \"central nervous system neoplasm\" OR " +
        "meningioma OR medulloblastoma OR ependymoma OR craniopharyngioma OR " +
        "\"diffuse midline glioma\" OR DIPG OR \"primary CNS lymphoma\" OR " +
        "\"vestibular schwannoma\" OR \"acoustic neuroma\" OR " +
        "\"pituitary tumor\" OR \"pituitary adenoma\" OR " +
        "\"spinal cord tumor\" OR \"brain metastases\" OR \"brain metastasis\" OR " +
        "\"leptomeningeal disease\")";

    /// <summary>Registry status enum to the words a patient reads without
    /// translating. Mirrors CtGovFetcher.PlainStatus.</summary>
    public static string? PlainStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "NOT_YET_RECRUITING" => "Not yet recruiting",
            "RECRUITING" => "Recruiting",
            "ENROLLING_BY_INVITATION" => "Enrolling by invitation",
            "ACTIVE_NOT_RECRUITING" => "Active, not recruiting",
            "SUSPENDED" => "Paused",
            "TERMINATED" => "Stopped early",
            "COMPLETED" => "Completed",
            "WITHDRAWN" => "Withdrawn before starting",
            "AVAILABLE" => "Available",
            "NO_LONGER_AVAILABLE" => "No longer available",
            "TEMPORARILY_NOT_AVAILABLE" => "Temporarily not available",
            "APPROVED_FOR_MARKETING" => "Approved for marketing",
            "WITHHELD" => "Withheld",
            "UNKNOWN" => "Status unknown",
            var other => TitleCase(other),
        };
    }

    /// <summary>Mirrors CtGovFetcher.PlainPhase.</summary>
    public static string? PlainPhase(IReadOnlyList<string>? phases)
    {
        if (phases is null || phases.Count == 0)
        {
            return null;
        }

        var parts = phases
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToUpperInvariant() switch
            {
                "NA" => "Not applicable",
                "EARLY_PHASE1" => "Early phase 1",
                "PHASE1" => "Phase 1",
                "PHASE2" => "Phase 2",
                "PHASE3" => "Phase 3",
                "PHASE4" => "Phase 4",
                var other => TitleCase(other),
            })
            .ToList();

        return parts.Count == 0 ? null : string.Join("/", parts);
    }

    private static string TitleCase(string value)
    {
        var words = value.Replace('_', ' ').ToLowerInvariant();
        return words.Length == 0 ? words : char.ToUpperInvariant(words[0]) + words[1..];
    }
}
