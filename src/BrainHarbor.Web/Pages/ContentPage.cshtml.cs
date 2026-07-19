using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class ContentPageModel(ContentStore store) : PageModel
{
    public ContentPage Page { get; private set; } = null!;

    public IActionResult OnGet(string? contentPath)
    {
        if (string.IsNullOrWhiteSpace(contentPath))
        {
            return NotFound();
        }

        var page = store.GetPage(contentPath);
        if (page is null)
        {
            return NotFound();
        }

        Page = page;
        return base.Page();
    }
}
