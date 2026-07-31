using BrainHarbor.Web.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

public class QueueModel(
    ReviewRepository reviews,
    UserManager<IdentityUser> userManager,
    ILogger<QueueModel> logger) : PageModel
{
    public const int PageSize = 20;

    public IReadOnlyList<ReviewItem> Items { get; private set; } = [];
    public int PendingCount { get; private set; }
    public int Page { get; private set; }
    public bool HasMore => (Page + 1) * PageSize < PendingCount;
    public bool TwoFactorEnabled { get; private set; }

    public async Task OnGetAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        Page = Math.Max(0, page);
        PendingCount = await reviews.CountPendingAsync(cancellationToken);
        Items = await reviews.GetPendingAsync(PageSize, Page * PageSize, cancellationToken);

        var user = await userManager.GetUserAsync(User);
        TwoFactorEnabled = user is not null && await userManager.GetTwoFactorEnabledAsync(user);
    }

    /// <summary>
    /// Approve or reject. On approve, any inline edits to the summary are saved
    /// first (WI-305), so the reviewer can fix a nearly-good summary instead of
    /// rejecting it, and the slug is built from the corrected title. Returns the
    /// swapped-out row for htmx; without JavaScript the same POST re-renders the
    /// whole page.
    /// </summary>
    public async Task<IActionResult> OnPostDecideAsync(
        long id, string action, string? note,
        string? plainTitle, string? hook, string? whatStudied, string? whatFound,
        string? means, string? doesntMean, int? readinessScore, string? readinessReason,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReviewAction>(action, ignoreCase: true, out var parsed) ||
            parsed is not (ReviewAction.Approved or ReviewAction.Rejected))
        {
            return BadRequest();
        }

        // Save inline edits before publishing. Blank fields mean "no change"
        // (→ null, so the DB COALESCE keeps what's there); only a reviewer who
        // typed something overwrites it.
        if (parsed == ReviewAction.Approved)
        {
            var edits = new SummaryEdits(
                NullIfBlank(plainTitle), NullIfBlank(hook), NullIfBlank(whatStudied),
                NullIfBlank(whatFound), NullIfBlank(means), NullIfBlank(doesntMean),
                readinessScore is >= 1 and <= 10 ? readinessScore : null,
                NullIfBlank(readinessReason));
            await reviews.SaveSummaryEditsAsync(id, edits, cancellationToken);
        }

        var actor = User.Identity?.Name ?? "unknown";
        var applied = await reviews.ApplyAsync(
            id, parsed, actor, string.IsNullOrWhiteSpace(note) ? null : note.Trim(), cancellationToken);

        if (!applied)
        {
            // Someone already decided this one (another tab, or a double
            // submit). Say so rather than pretending it worked.
            logger.LogInformation("Review decision on item {ItemId} was a no-op — already decided.", id);
        }
        else
        {
            logger.LogInformation("Item {ItemId} {Action} by {Actor}.",
                id, parsed.ToString().ToLowerInvariant(), actor);
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return Content(
                applied
                    ? $"""<li class="card" id="review-item-{id}"><p>Done — item {parsed.ToString().ToLowerInvariant()}.</p></li>"""
                    : $"""<li class="card" id="review-item-{id}"><p>This item was already decided elsewhere.</p></li>""",
                "text/html");
        }

        return RedirectToPage("/Admin/Queue");
    }

    private static string? NullIfBlank(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
