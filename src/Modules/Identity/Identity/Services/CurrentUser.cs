using System.Security.Claims;
using Couture.Identity.Contracts;

namespace Couture.Identity.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public CurrentUser(ClaimsPrincipal principal)
    {
        _principal = principal;
    }

    public Guid UserId => Guid.Parse(_principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
    public string UserName => _principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    public string FullName => _principal.FindFirstValue("FullName") ?? string.Empty;

    public IReadOnlyList<string> Roles => _principal.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList();

    public IReadOnlyList<string> Permissions => _principal.Claims
        .Where(c => c.Type == "Permission")
        .Select(c => c.Value)
        .ToList();

    public bool IsInRole(string role) => _principal.IsInRole(role);
    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
