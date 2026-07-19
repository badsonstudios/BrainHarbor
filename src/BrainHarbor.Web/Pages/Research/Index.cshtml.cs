using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Research;

public class IndexModel(FeedRepository feed, TaxonomyStore taxonomy) : PageModel
{
    public FeedQuery Query { get; private set; } = new();
    public FeedPage Result { get; private set; } = new([], 0, new FeedQuery());

    public IReadOnlyList<TumorType> TumorTypes => taxonomy.TumorTypes;

    public async Task OnGetAsync(
        string? tumor = null,
        string? kind = null,
        bool early = false,
        int page = 0,
        CancellationToken cancellationToken = default)
    {
        Query = new FeedQuery(tumor, kind, early, page);
        Result = await feed.GetAsync(Query, cancellationToken);
    }

    public FeedCard ToCard(FeedRow row) => feed.ToCard(row);

    /// <summary>Keeps the current filters when paging (works without JS).</summary>
    public string NextPageUrl
    {
        get
        {
            var parts = new List<string> { $"page={Query.Page + 1}" };
            if (!string.IsNullOrEmpty(Query.TumorType))
            {
                parts.Add($"tumor={Uri.EscapeDataString(Query.TumorType)}");
            }
            if (!string.IsNullOrEmpty(Query.Kind))
            {
                parts.Add($"kind={Uri.EscapeDataString(Query.Kind)}");
            }
            if (Query.IncludeEarlyStage)
            {
                parts.Add("early=true");
            }
            return "/research?" + string.Join("&", parts);
        }
    }
}
