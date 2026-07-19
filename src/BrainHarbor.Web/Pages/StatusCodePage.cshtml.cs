using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class StatusCodePageModel : PageModel
{
    public int Code { get; private set; }

    public bool IsNotFound => Code == StatusCodes.Status404NotFound;

    public IActionResult OnGet(int code)
    {
        // Only render when reached via the status-code re-execute pipeline.
        // A direct hit on /status/404 would otherwise be a 200 serving error
        // copy; returning 404 here makes the middleware re-execute this page
        // properly (real status code, feature present).
        if (HttpContext.Features.Get<IStatusCodeReExecuteFeature>() is null)
        {
            return NotFound();
        }

        Code = code;
        return Page();
    }
}
