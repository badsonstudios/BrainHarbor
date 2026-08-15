using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class IndexModel(FeedRepository feed) : PageModel
{
    /// <summary>
    /// Eight, up from four (homepage handoff, 2026-08-15). Four was two rows of
    /// a 2-up grid; with the hero band no longer burying the feed, the page can
    /// show a real amount of what the site is actually for.
    /// </summary>
    public const int CardCount = 8;

    public IReadOnlyList<FeedRow> Latest { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Same rules as /research on a plain visit: published only, closed
        // trials excluded, and early-stage work hidden unless the reader's
        // persisted /research choice (WI-307) says otherwise. Home only reads
        // the cookie — the toggle itself lives on /research.
        var includeEarly = Research.IndexModel.ReadEarlyChoice(Request.Cookies);

        var result = await feed.GetAsync(new FeedQuery(IncludeEarlyStage: includeEarly), cancellationToken);
        Latest = [.. result.Items.Take(CardCount)];
    }

    public FeedCard ToCard(FeedRow row) => feed.ToCard(row);
}
