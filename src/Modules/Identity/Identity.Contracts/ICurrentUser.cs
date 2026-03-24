namespace Couture.Identity.Contracts;

public interface ICurrentUser
{
    Guid UserId { get; }
    string UserName { get; }
    string FullName { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
