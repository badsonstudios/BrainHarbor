using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrainHarbor.Web.Identity;

/// <summary>
/// WI-207: creates the single admin account at startup from configuration.
/// There is no registration endpoint — this is the only way an account comes
/// into existence (security reference: "single admin account, TOTP 2FA, no
/// registration endpoint").
/// </summary>
public static class AdminSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];

        var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await context.Database.MigrateAsync();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            // Not fatal: the public site must still serve. But the admin area
            // is unreachable, so say so loudly rather than 404ing mysteriously.
            logger.LogWarning(
                "Admin:Email / Admin:Password are not configured — no admin account exists " +
                "and the review queue cannot be opened. Set them via user-secrets (dev) or " +
                "App Service configuration (prod).");
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Never reset the password of an existing account from config —
            // that would let a stale config value silently roll back a
            // rotation, and would clobber the 2FA setup.
            if (!await userManager.IsInRoleAsync(existing, AdminRole))
            {
                await userManager.AddToRoleAsync(existing, AdminRole);
            }
            return;
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // Loud, and specific about the fix: a rejected password means the
            // admin area is unreachable, and the cause (usually the length
            // rule) is not obvious from a generic failure.
            logger.LogError(
                "Could not create the admin account for {Email}: {Errors}. " +
                "The review queue is unreachable until this is fixed.",
                email, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, AdminRole);
        logger.LogInformation(
            "Created the admin account for {Email}. Set up 2FA at /admin/two-factor — " +
            "review actions require it.", email);
    }
}
