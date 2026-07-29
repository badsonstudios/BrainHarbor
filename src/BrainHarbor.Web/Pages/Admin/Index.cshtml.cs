using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

public class IndexModel(UserManager<IdentityUser> userManager) : PageModel
{
    public bool TwoFactorEnabled { get; private set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        TwoFactorEnabled = user is not null && await userManager.GetTwoFactorEnabledAsync(user);
    }
}
