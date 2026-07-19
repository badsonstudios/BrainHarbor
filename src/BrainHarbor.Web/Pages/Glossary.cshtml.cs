using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages;

public class GlossaryModel(GlossaryStore glossary) : PageModel
{
    public IReadOnlyList<GlossaryTerm> Terms { get; private set; } = [];

    public void OnGet()
    {
        Terms = glossary.GetTerms();
    }
}
