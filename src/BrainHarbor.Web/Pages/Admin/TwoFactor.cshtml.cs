using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

/// <summary>
/// WI-207: TOTP enrolment. The key is shown as text rather than a QR code —
/// rendering a QR would mean either a JS library or a server-side image
/// dependency, and every authenticator app accepts manual entry.
/// </summary>
public class TwoFactorModel(UserManager<IdentityUser> userManager) : PageModel
{
    [BindProperty]
    [Required]
    public string Code { get; set; } = "";

    public bool IsEnabled { get; private set; }
    public string FormattedKey { get; private set; } = "";
    public string AuthenticatorUri { get; private set; } = "";
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Admin/Login");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostEnableAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Admin/Login");
        }

        var code = Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, code);

        if (!valid)
        {
            ErrorMessage = "That code was not right. Codes change every 30 seconds.";
            await LoadAsync(user);
            return Page();
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostDisableAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Admin/Login");
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        await LoadAsync(user);
        return Page();
    }

    private async Task LoadAsync(IdentityUser user)
    {
        IsEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (IsEnabled)
        {
            return;
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        FormattedKey = FormatKey(key!);
        AuthenticatorUri = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            UrlEncoder.Default.Encode("BrainHarbor"),
            UrlEncoder.Default.Encode(user.Email ?? user.UserName ?? "admin"),
            key);
    }

    /// <summary>Groups of four — much easier to type without mistakes.</summary>
    private static string FormatKey(string key)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < key.Length; i += 4)
        {
            builder.Append(key.AsSpan(i, Math.Min(4, key.Length - i))).Append(' ');
        }
        return builder.ToString().Trim().ToLowerInvariant();
    }
}
