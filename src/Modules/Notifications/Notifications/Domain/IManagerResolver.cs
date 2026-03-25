namespace Couture.Notifications.Domain;

/// <summary>
/// Resolves the user IDs of users with the Manager (Gérant) role.
/// Implemented in the API host where Identity is available.
/// </summary>
public interface IManagerResolver
{
    Task<IReadOnlyList<Guid>> GetManagerIdsAsync(CancellationToken ct = default);
}
