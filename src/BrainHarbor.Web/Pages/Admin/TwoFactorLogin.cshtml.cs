using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

[AllowAnonymous]
public class TwoFactorLoginModel(
    SignInManager<IdentityUser> signInManager,
    ILogger<TwoFactorLoginModel> logger) : PageModel
{
    [BindProperty]
    [Required]
    [StringLength(8, MinimumLength = 6)]
    public string Code { get; set; } = "";

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Only reachable mid-login, after the password step succeeded.
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        return user is null ? RedirectToPage("/Admin/Login") : Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return RedirectToPage("/Admin/Login");
        }

        var code = Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            code, isPersistent: false, rememberClient: false);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Admin account locked out during 2FA.");
            ErrorMessage = "Too many attempts. Try again in a few minutes.";
            return Page();
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Invalid 2FA code from {RemoteIp}.",
                HttpContext.Connection.RemoteIpAddress);
            ErrorMessage = "That code was not right. Codes change every 30 seconds.";
            return Page();
        }

        return LocalRedirect(returnUrl ?? "/admin/queue");
    }
}
