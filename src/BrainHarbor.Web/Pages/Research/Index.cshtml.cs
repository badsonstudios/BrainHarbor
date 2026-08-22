using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// The reader-facing page number, 1-based, as it appears in the URL and on
    /// the pager. <see cref="FeedQuery.Page"/> stays 0-based.
    /// </summary>
    public Pagination Pages { get; private set; } = new(1, 1);

    /// <param name="pageNumber">
    /// Bound EXPLICITLY from the query string, and deliberately not named
    /// `page` (WI-438).
    ///
    /// `page` is a reserved route-value key in Razor Pages — routing puts the
    /// page path in it. A handler parameter of that name therefore binds the
    /// ambient ROUTE value ("/Research/Index"), fails to parse as an int, and
    /// falls back to the default. Silently: no exception, no warning, just
    /// every page rendering page 1 forever. That was the bug Dan reported, and
    /// it was live on /research and /trials at the same time.
    ///
    /// [FromQuery] pins the binding source so the route value can never win
    /// again, and the different C# name keeps it obvious why.
    /// </param>
    public async Task OnGetAsync(
        string? tumor = null,
        string? kind = null,
        bool early = false,
        [FromQuery(Name = "page")] int pageNumber = 1,
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
        // echoes back (selected option, pager links) is always a canonical one.
        var normalizedSort = FeedRepository.NormalizeSort(sort);

        // Ask once with the requested page, then clamp against the real total.
        // A stale or hand-typed ?page=99 must land on the last page rather than
        // show an empty list with no way back.
        var firstAttempt = new FeedQuery(tumor, kind, includeEarly,
            Math.Max(0, pageNumber - 1), normalizedSort);
        Result = await feed.GetAsync(firstAttempt, cancellationToken);
        Pages = Pagination.For(Result.TotalCount, FeedQuery.PageSize, pageNumber);

        var clampedIndex = Pages.CurrentPage - 1;
        if (clampedIndex != firstAttempt.Page)
        {
            Query = firstAttempt with { Page = clampedIndex };
            Result = await feed.GetAsync(Query, cancellationToken);
        }
        else
        {
            Query = firstAttempt;
        }
    }

    public FeedCard ToCard(FeedRow row) => feed.ToCard(row);

    /// <summary>
    /// The URL for a given 1-based page, carrying every active filter so paging
    /// never silently drops one. Passed to the pager partial as a delegate:
    /// only this model knows which filters exist.
    /// </summary>
    public string UrlForPage(int pageNumber)
    {
        var parts = new List<string> { $"page={pageNumber}" };
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
