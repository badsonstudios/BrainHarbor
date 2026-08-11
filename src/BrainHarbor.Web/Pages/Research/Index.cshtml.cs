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
    /// A functional preference cookie, not tracking. Home reads it too (WI-409),
    /// so the choice follows the reader everywhere the feed appears.</summary>
    internal const string EarlyCookie = "bh_show_early";

    /// <summary>The remembered choice, as every page that renders feed cards
    /// must read it — one parse, so the pages can't drift apart.</summary>
    internal static bool ReadEarlyChoice(IRequestCookieCollection cookies) =>
        cookies.TryGetValue(EarlyCookie, out var saved) && saved == "1";

    public async Task OnGetAsync(
        string? tumor = null,
        string? kind = null,
        bool early = false,
        int page = 0,
        bool applied = false,
        string? sort = null,
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
            includeEarly = ReadEarlyChoice(Request.Cookies);
        }

        // Normalized here as well as in the repository, so the URL the view
        // echoes back (selected option, Show more) is always a canonical one.
        Query = new FeedQuery(tumor, kind, includeEarly, page, FeedRepository.NormalizeSort(sort));
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
            if (!string.IsNullOrEmpty(Query.Sort))
            {
                parts.Add($"sort={Uri.EscapeDataString(Query.Sort)}");
            }
            return "/research?" + string.Join("&", parts);
        }
    }
}
