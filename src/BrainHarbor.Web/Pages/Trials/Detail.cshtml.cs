using BrainHarbor.Web.Content;
using BrainHarbor.Web.Trials;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Trials;

/// <summary>
/// WI-403: one trial's page.
///
/// The status shown here is the cached fact, refreshed every run, never the
/// one baked into a summary written weeks ago. Everything else on the page is
/// either the registry's own words (clearly labelled) or a plain-language
/// summary that has already been through the site's safety checks and been
/// published — never the pending or rejected kind.
/// </summary>
public class DetailModel(TrialsRepository trials, SummaryRenderer summaries) : PageModel
{
    public TrialRow Trial { get; private set; } = null!;

    public IHtmlContent Summary(string? text) => summaries.Render(text);

    public string RegistryUrl => $"https://clinicaltrials.gov/study/{Trial.NctId}";

    /// <summary>
    /// Plain words for what the status means for the reader. Same wording as
    /// the research item page, because it is the same question.
    /// </summary>
    public string StatusExplanation => Trial.OverallStatus switch
    {
        "Recruiting" => "It is looking for patients now. Talk to your care team about whether it fits you.",
        "Not yet recruiting" =>
            "It has not opened yet. Ask your care team to watch for it if it looks like a fit.",
        "Enrolling by invitation" =>
            "It is only taking people the trial team invites. You cannot sign up for this one directly.",
        "Available" => "It is open outside of a trial for people who qualify.",
        "Active, not recruiting" =>
            "It is still running with the people who already joined. It is not taking anyone new.",
        "Completed" => "It has finished. Results may take a long time to be published.",
        "Stopped early" =>
            "It was stopped before it finished. That can happen for many reasons, good or bad.",
        "Paused" => "It has stopped taking people for now. It may or may not start again.",
        "Withdrawn before starting" => "It closed before anyone joined.",
        "No longer available" => "It has closed and is no longer an option.",
        "Temporarily not available" => "It is closed for now. It may open again.",
        "Approved for marketing" =>
            "The treatment being tested here has been approved, so it may be available through your doctor.",
        "Withheld" => "The trial team has asked for the details to be held back.",
        "Status unknown" =>
            "The trial team has not updated this record in a long time, so we cannot tell if it is still going.",
        _ => "We take this from the trial registry, which is the best record of where a trial stands.",
    };

    /// <summary>US sites, grouped so a long list reads as places rather than a
    /// wall of hospital names.</summary>
    public IReadOnlyList<IGrouping<string, TrialSite>> SitesByState =>
        [.. Trial.Sites
            .Where(s => !string.IsNullOrWhiteSpace(s.Where))
            .GroupBy(s => s.State ?? s.Country ?? "Elsewhere")
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)];

    /// <summary>Sites we can actually name a place for — the same set the list
    /// below renders, so the count never disagrees with what is shown.</summary>
    public int ListedSiteCount => SitesByState.Sum(g => g.Count());

    public async Task<IActionResult> OnGetAsync(string nctId, CancellationToken cancellationToken)
    {
        var found = await trials.FindAsync(nctId, cancellationToken);
        if (found is null)
        {
            return NotFound();
        }

        Trial = found;
        return Page();
    }
}
