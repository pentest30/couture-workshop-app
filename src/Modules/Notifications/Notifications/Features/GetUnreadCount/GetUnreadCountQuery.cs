using Mediator;

namespace Couture.Notifications.Features.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid RecipientId) : IQuery<int>;
