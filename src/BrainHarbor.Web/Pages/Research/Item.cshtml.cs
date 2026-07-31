using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Research;

public class ItemModel(FeedRepository feed, SummaryRenderer summaries) : PageModel
{
    public FeedRow Item { get; private set; } = null!;
    public string? CorrectionNote { get; private set; }

    /// <summary>Set after a reader submits a problem report, to show a thank-you.</summary>
    public bool JustReported { get; private set; }

    public StageBadge Badge =>
        StageBadge.For(ResearchStageMapper.From(Item.SourceKind, Item.ResearchStage));

    public ReadinessBadge? Readiness => Item.Readiness;

    public string SourceLabel => FeedRepository.SourceLabel(Item.Source);

    /// <summary>Renders a summary block with glossary tooltips (WI-306).</summary>
    public IHtmlContent Block(string? text) => summaries.Render(text);

    /// <summary>The one-line description for meta/OG tags — the hook if we have
    /// one, else the plain or original title.</summary>
    public string MetaDescription =>
        Item.PlainSummary ?? Item.PlainTitle ?? Item.Title;

    /// <summary>
    /// Schema.org JSON-LD for the item (WI-308): a MedicalWebPage plus a
    /// BreadcrumbList, so search engines index it as health content with a clear
    /// trail. Built with the default JSON encoder, which escapes &lt; &gt; &amp;
    /// — so no field (title, summary) can break out of the &lt;script&gt; tag.
    /// </summary>
    public string JsonLd()
    {
        var url = $"{Request.Scheme}://{Request.Host}/research/{Item.Slug}";
        var graph = new object[]
        {
            new Dictionary<string, object?>
            {
                ["@type"] = "MedicalWebPage",
                ["name"] = Item.PlainTitle ?? Item.Title,
                ["headline"] = Item.PlainTitle ?? Item.Title,
                ["description"] = MetaDescription,
                ["url"] = url,
                ["datePublished"] = Item.PublishedAt?.ToString("yyyy-MM-dd"),
                ["isBasedOn"] = Item.Url,
                ["publisher"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Organization",
                    ["name"] = "BrainHarbor",
                },
            },
            new Dictionary<string, object?>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = new object[]
                {
                    Crumb(1, "Home", $"{Request.Scheme}://{Request.Host}/"),
                    Crumb(2, "Research", $"{Request.Scheme}://{Request.Host}/research"),
                    Crumb(3, Item.PlainTitle ?? Item.Title, url),
                },
            },
        };

        var doc = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph,
        };

        return System.Text.Json.JsonSerializer.Serialize(doc,
            new System.Text.Json.JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
    }

    private static Dictionary<string, object?> Crumb(int position, string name, string item) => new()
    {
        ["@type"] = "ListItem",
        ["position"] = position,
        ["name"] = name,
        ["item"] = item,
    };

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

    public async Task<IActionResult> OnGetAsync(
        string slug, bool reported = false, CancellationToken cancellationToken = default)
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
        JustReported = reported;
        return Page();
    }

    /// <summary>
    /// A reader reports a problem (WI-306). Flags the item for the admin queue
    /// and records the report; the page stays published. Redirects back with a
    /// thank-you so a refresh doesn't re-submit (PRG). A bad slug 404s.
    /// </summary>
    public async Task<IActionResult> OnPostReportAsync(
        string slug, string? reason, CancellationToken cancellationToken)
    {
        var reported = await feed.ReportProblemAsync(slug, reason, cancellationToken);
        if (!reported)
        {
            return NotFound();
        }

        return RedirectToPage(new { slug, reported = true });
    }
}
