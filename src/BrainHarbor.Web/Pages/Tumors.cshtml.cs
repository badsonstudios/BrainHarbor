using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

/// <summary>
/// WI-412: "what is this?" for every tumor type a reader can filter by.
///
/// The list comes from the SAME `taxonomy.yml` the feed filter uses, so the
/// filter and this page cannot drift apart — a type that appears in one
/// appears in the other, by construction rather than by discipline.
///
/// The descriptions are curated Markdown under `Content/pages/tumors/`, which
/// means they ride the existing machinery rather than a new one: the 6.0
/// reading-level gate in CI, glossary tooltips, site search, and a real URL per
/// type (`/tumors/glioblastoma`). Someone searching the web for their diagnosis
/// lands on a page about it, not an anchor part-way down a list.
/// </summary>
public class TumorsModel(TaxonomyStore taxonomy, ContentStore content) : PageModel
{
    /// <summary>One row on the index: the taxonomy's label, and whether we have
    /// written the description yet.</summary>
    public sealed record TumorEntry(string Slug, string Label, bool HasDescription, string? Summary);

    /// <summary>
    /// Grouped the way the taxonomy itself is, not A to Z. "Is mine a glioma?"
    /// is a question a newly-diagnosed reader actually has, and the grouping
    /// answers it before they read a word.
    /// </summary>
    public sealed record TumorGroup(string Heading, string Blurb, IReadOnlyList<TumorEntry> Entries);

    public IReadOnlyList<TumorGroup> Groups { get; private set; } = [];
    public int WrittenCount { get; private set; }
    public int TotalCount { get; private set; }

    // Which slugs sit in which section. Anything not listed falls into "other
    // types", so adding a slug to taxonomy.yml can never make it vanish here.
    private static readonly string[] Gliomas =
    [
        "glioma", "low-grade-glioma", "high-grade-glioma", "glioblastoma", "astrocytoma",
        "oligodendroglioma", "diffuse-midline-glioma", "dipg", "ependymoma",
    ];

    private static readonly string[] Secondary = ["brain-metastases"];

    // Deliberately its own section: taxonomy.yml is explicit that a spinal cord
    // tumor is NOT a brain tumor and must never surface under a brain filter.
    // Burying it among the brain types on this page would undo that care.
    private static readonly string[] Spinal = ["spinal-cord-tumor"];

    // Age and catch-all, not histology. They are real filter options, so they
    // belong here, but calling them tumor types would be wrong.
    private static readonly string[] CrossCutting = ["pediatric-brain-tumor", "all-brain-tumors"];

    public void OnGet()
    {
        var all = taxonomy.TumorTypes;
        var named = Gliomas.Concat(Secondary).Concat(Spinal).Concat(CrossCutting).ToHashSet(StringComparer.Ordinal);

        Groups =
        [
            Group("Gliomas", "Tumors that start in the glial cells, the cells that support nerve cells.", Gliomas),
            Group("Other tumors that start in the brain",
                  "These start in the brain or in the tissue around it.",
                  [.. all.Select(t => t.Slug).Where(s => !named.Contains(s))]),
            Group("Tumors that spread to the brain",
                  "These start somewhere else in the body and travel to the brain.", Secondary),
            Group("Tumors of the spinal cord",
                  "The spinal cord is not the brain. We list these because the same care teams treat them.", Spinal),
            Group("Ways to group tumors", "These are not types. They are other ways to sort what you read.", CrossCutting),
        ];

        Groups = [.. Groups.Where(g => g.Entries.Count > 0)];
        TotalCount = Groups.Sum(g => g.Entries.Count);
        WrittenCount = Groups.Sum(g => g.Entries.Count(e => e.HasDescription));
    }

    private TumorGroup Group(string heading, string blurb, IReadOnlyList<string> slugs)
    {
        var entries = slugs
            .Select(slug => (Slug: slug, Type: taxonomy.Find(slug)))
            .Where(x => x.Type is not null)
            .Select(x =>
            {
                // Null means the description is not written yet. The page says
                // so out loud: a blank space reads as "this site has nothing
                // for you", which is the opposite of true.
                var page = content.GetPage($"tumors/{x.Slug}");
                return new TumorEntry(x.Slug, x.Type!.Label, page is not null, page?.FrontMatter.Description);
            })
            .ToList();

        return new TumorGroup(heading, blurb, entries);
    }
}
