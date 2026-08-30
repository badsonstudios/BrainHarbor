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

    /// <summary>
    /// Visible text for a source link, never empty. A source with a URL and no
    /// title parses cleanly, and rendering it would produce a focusable link
    /// with no accessible name — a WCAG 2.4.4 failure. The host is a poor
    /// label but an honest one; ContentCheck warns so it gets fixed properly.
    /// </summary>
    public static string LinkText(ContentSource source)
    {
        if (!string.IsNullOrWhiteSpace(source.Title))
        {
            return source.Title;
        }

        return Uri.TryCreate(source.Url, UriKind.Absolute, out var uri) ? uri.Host : source.Url;
    }
}
