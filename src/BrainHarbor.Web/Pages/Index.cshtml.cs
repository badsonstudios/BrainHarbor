using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class IndexModel(FeedRepository feed) : PageModel
{
    /// <summary>Newest few for the 2-up grid: two full rows.</summary>
    public const int CardCount = 4;

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
