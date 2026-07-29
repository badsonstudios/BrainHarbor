using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

[AllowAnonymous]
public class LoginModel(
    SignInManager<IdentityUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Enter an email address and password.";
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("/Admin/TwoFactorLogin", new { returnUrl });
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Admin account locked out after repeated failures.");
            ErrorMessage = "Too many attempts. Try again in a few minutes.";
            return Page();
        }

        if (!result.Succeeded)
        {
            // Deliberately does not say which field was wrong.
            logger.LogWarning("Failed admin sign-in attempt from {RemoteIp}.",
                HttpContext.Connection.RemoteIpAddress);
            ErrorMessage = "That email and password did not match.";
            return Page();
        }

        // Only redirect somewhere on this site.
        return LocalRedirect(returnUrl ?? "/admin/queue");
    }
}
