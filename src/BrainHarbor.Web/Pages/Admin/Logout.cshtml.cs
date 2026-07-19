using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

[AllowAnonymous]
public class LogoutModel(SignInManager<IdentityUser> signInManager) : PageModel
{
    // POST only: a GET logout can be triggered by any embedded image or link,
    // which is a nuisance CSRF even when the impact is only "signed out".
    public IActionResult OnGet() => RedirectToPage("/Admin/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        return Page();
    }
}
