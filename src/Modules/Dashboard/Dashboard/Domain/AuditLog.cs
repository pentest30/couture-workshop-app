namespace Couture.Dashboard.Domain;

public sealed class AuditLog
{
    public long Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string EntityType { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public DateTimeOffset Timestamp { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid userId, string action, string entityType, string entityId, string? beforeJson, string? afterJson)
    {
        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTimeOffset.UtcNow,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
        };
    }
}
