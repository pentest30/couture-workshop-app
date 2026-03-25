using Couture.Identity.Contracts;
using Couture.Identity.Domain;
using Couture.Notifications.Domain;
using Microsoft.AspNetCore.Identity;

namespace Couture.Api.Services;

public sealed class ManagerResolver : IManagerResolver
{
    private readonly UserManager<CoutureUser> _userManager;

    public ManagerResolver(UserManager<CoutureUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<Guid>> GetManagerIdsAsync(CancellationToken ct = default)
    {
        var managers = await _userManager.GetUsersInRoleAsync(CoutureRoles.Manager);
        return managers.Select(u => u.Id).ToList();
    }
}
