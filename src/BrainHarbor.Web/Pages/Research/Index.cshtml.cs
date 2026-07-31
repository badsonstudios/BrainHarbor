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

    /// <summary>Remembers the "show early-stage research" choice across visits
    /// (WI-307), so a reader who opted in doesn't have to re-tick it every time.
    /// A functional preference cookie, not tracking.</summary>
    private const string EarlyCookie = "bh_show_early";

    public async Task OnGetAsync(
        string? tumor = null,
        string? kind = null,
        bool early = false,
        int page = 0,
        bool applied = false,
        CancellationToken cancellationToken = default)
    {
        // When the filter form is submitted (`applied`), the checkbox is
        // authoritative — an unchecked box sends no value, so absence means
        // "off", and we remember the choice. On a plain visit (a nav link, a
        // shared URL with no `applied`), fall back to the remembered choice.
        bool includeEarly;
        if (applied)
        {
            includeEarly = early;
            Response.Cookies.Append(EarlyCookie, early ? "1" : "0", new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(365),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true, // a display preference the reader set themselves
            });
        }
        else
        {
            includeEarly = Request.Cookies.TryGetValue(EarlyCookie, out var saved) && saved == "1";
        }

        Query = new FeedQuery(tumor, kind, includeEarly, page);
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
