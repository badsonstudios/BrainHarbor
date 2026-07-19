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
    /// Approve or reject. Returns the swapped-out row for htmx; without
    /// JavaScript the same POST re-renders the whole page.
    /// </summary>
    public async Task<IActionResult> OnPostDecideAsync(
        long id, string action, string? note, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReviewAction>(action, ignoreCase: true, out var parsed) ||
            parsed is not (ReviewAction.Approved or ReviewAction.Rejected))
        {
            return BadRequest();
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
}
