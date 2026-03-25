using Mediator;

namespace Couture.Notifications.Features.GetSmsLogs;

public sealed record GetSmsLogsQuery(int Page = 1, int PageSize = 20) : IQuery<SmsLogsResult>;

public sealed record SmsLogsResult(List<SmsLogDto> Items, int TotalCount);

public sealed record SmsLogDto(
    Guid NotificationId,
    string Type,
    Guid RecipientId,
    string Title,
    string SmsStatus,
    DateTimeOffset CreatedAt);
