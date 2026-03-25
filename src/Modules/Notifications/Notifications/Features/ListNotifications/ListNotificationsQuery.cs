using Mediator;

namespace Couture.Notifications.Features.ListNotifications;

public sealed record ListNotificationsQuery(
    Guid RecipientId,
    string Filter = "all", // all, unread, critical
    int Page = 1,
    int PageSize = 20) : IQuery<ListNotificationsResult>;

public sealed record ListNotificationsResult(
    List<NotificationDto> Items,
    int TotalCount,
    int UnreadCount);

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string TypeLabel,
    string Priority,
    string Title,
    string Message,
    Guid OrderId,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
