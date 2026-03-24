using Couture.Identity.Contracts;
using Couture.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Couture.Identity.Services;

public sealed class IdentitySeeder
{
    private readonly UserManager<CoutureUser> _userManager;
    private readonly RoleManager<CoutureRole> _roleManager;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        UserManager<CoutureUser> userManager,
        RoleManager<CoutureRole> roleManager,
        ILogger<IdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = [CoutureRoles.Manager, CoutureRoles.Tailor, CoutureRoles.Embroiderer, CoutureRoles.Beader, CoutureRoles.Cashier];

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new CoutureRole(roleName, $"{roleName} role"));
                _logger.LogInformation("Seeded role: {Role}", roleName);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@couture.local";
        const string adminPassword = "Admin123!";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new CoutureUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Gérant",
            PhoneNumber = "0550000000",
            EmailConfirmed = true,
            IsActive = true,
            SessionDurationHours = 24,
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, CoutureRoles.Manager);
            _logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
