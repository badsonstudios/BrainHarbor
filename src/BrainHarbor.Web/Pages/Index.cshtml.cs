using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool ShowTime { get; set; }

    public void OnGet()
    {
    }

    // htmx demo (WI-005): returns just the fragment; deleted once real pages exist.
    public PartialViewResult OnGetTimePartial() => Partial("_ServerTime");
}
