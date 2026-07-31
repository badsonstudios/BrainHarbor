using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

/// <summary>
/// WI-309: one search across published research items (Postgres full-text) and
/// the curated static pages. The form is a plain GET, so it works with no
/// JavaScript; htmx just swaps the results in place when it's available.
/// </summary>
public class SearchModel(FeedRepository feed, ContentStore content) : PageModel
{
    private const int ItemLimit = 30;
    private const int PageLimit = 10;

    public string Query { get; private set; } = "";
    public bool Searched { get; private set; }
    public IReadOnlyList<FeedRow> Items { get; private set; } = [];
    public IReadOnlyList<ContentStore.PageMatch> Pages { get; private set; } = [];

    public int TotalCount => Items.Count + Pages.Count;

    public FeedCard ToCard(FeedRow row) => feed.ToCard(row);

    public async Task OnGetAsync(string? q, CancellationToken cancellationToken = default)
    {
        Query = q?.Trim() ?? "";
        if (Query.Length == 0)
        {
            return;
        }

        Searched = true;
        Items = await feed.SearchAsync(Query, ItemLimit, cancellationToken);
        Pages = content.SearchPages(Query, PageLimit);
    }
}
