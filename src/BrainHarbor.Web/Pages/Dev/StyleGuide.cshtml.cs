using BrainHarbor.Web.Content;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Dev;

public class StyleGuideModel(IWebHostEnvironment environment, GlossaryStore glossary) : PageModel
{
    /// <summary>
    /// A sample sentence rendered through the real glossary pipeline. No
    /// shipped content page happens to use a glossary term yet, so without
    /// this the tooltip component can't be eyeballed anywhere.
    /// </summary>
    public string TooltipSampleHtml { get; private set; } = "";

    // Sample content from the design handoff — realistic items at the target
    // reading level, one per badge family.
    public IReadOnlyList<FeedCard> SampleCards { get; } =
    [
        new(ResearchStage.TestedInPeople,
            "A pill that targets a common gene change slowed the growth of low-grade gliomas",
            "/research/sample-item",
            "In a large trial, people with grade 2 glioma and an IDH gene change went longer before their tumor grew or needed more treatment.",
            ["Low-grade glioma", "Treatment"],
            "June 12, 2026", "New England Journal of Medicine"),
        new(ResearchStage.NewOrUpdatedTrial,
            "A new trial is testing a vaccine made from each person's own tumor",
            "/research/sample-item",
            "The trial is now enrolling adults with newly diagnosed glioblastoma at 12 U.S. hospitals.",
            ["Glioblastoma", "Clinical trial"],
            "June 10, 2026", "ClinicalTrials.gov"),
        new(ResearchStage.News,
            "FDA clears a new imaging dye that helps surgeons see tumor edges",
            "/research/sample-item",
            "The dye makes tumor tissue glow during surgery, which may help surgeons remove more of it safely.",
            ["Surgery", "All tumor types"],
            "June 9, 2026", "FDA announcement"),
        new(ResearchStage.EarlyResearchAnimals,
            "Scientists helped the immune system find hidden glioblastoma cells — in mice",
            "/research/sample-item",
            "This worked in mice, not people. Most findings at this stage never become treatments, but it points at a new idea.",
            ["Glioblastoma", "Immunotherapy"],
            "June 8, 2026", "Nature Communications"),
    ];

    public IActionResult OnGet()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        const string sample =
            "A glioma is graded 1 to 4. The fastest-growing kind is a " +
            "glioblastoma. Some tumors have an IDH gene change, which often " +
            "means slower growth. Only the first mention of each word is marked.";

        TooltipSampleHtml = ContentStore
            .Parse($"---\ntitle: Tooltip sample\n---\n{sample}", "dev/styleguide",
                glossary.GetSnapshot().Terms)
            .Html;

        return Page();
    }
}
