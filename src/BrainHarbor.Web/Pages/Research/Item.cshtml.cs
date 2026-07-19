using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Research;

public class ItemModel(FeedRepository feed) : PageModel
{
    public FeedRow Item { get; private set; } = null!;
    public string? CorrectionNote { get; private set; }

    public StageBadge Badge =>
        StageBadge.For(ResearchStageMapper.From(Item.SourceKind, Item.ResearchStage));

    public string SourceLabel => FeedRepository.SourceLabel(Item.Source);

    /// <summary>
    /// Plain-language explanation of the badge. This is the anti-hype work:
    /// the badge is a mark, this sentence says what it means for you.
    /// </summary>
    public string StageExplanation => Badge.Stage switch
    {
        ResearchStage.TestedInPeople =>
            "This was tested in people. That is the most reliable kind of research we share.",
        ResearchStage.ReviewOfExistingResearch =>
            "This looks across many earlier studies rather than running a new one.",
        ResearchStage.EarlyResearchAnimals =>
            "This was done in animals, not people. Most findings at this stage never " +
            "become treatments.",
        ResearchStage.EarlyResearchLabCells =>
            "This was done on cells in a lab, not in people. It is a very early step.",
        ResearchStage.NewOrUpdatedTrial =>
            "This is a trial that is enrolling or has been updated. It is not a finding yet.",
        ResearchStage.Preprint =>
            "This is a preprint. Other scientists have not checked it yet, so treat it " +
            "as an early signal rather than an answer.",
        _ => "This is a news item, not a research finding.",
    };

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        var found = await feed.GetPublishedBySlugAsync(slug, cancellationToken);
        if (found is null)
        {
            // Unpublished, pulled, or never existed — all look the same from
            // outside, which is the correct behaviour for a pulled item.
            return NotFound();
        }

        Item = found.Value.Row;
        CorrectionNote = found.Value.ReviewNote;
        return Page();
    }
}
