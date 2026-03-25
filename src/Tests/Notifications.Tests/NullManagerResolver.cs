using Couture.Notifications.Domain;

namespace Couture.Notifications.Tests;

/// <summary>
/// Returns a single fake manager ID for unit tests.
/// </summary>
public sealed class NullManagerResolver : IManagerResolver
{
    private readonly Guid _managerId = Guid.NewGuid();

    public Task<IReadOnlyList<Guid>> GetManagerIdsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Guid>>([_managerId]);
}
