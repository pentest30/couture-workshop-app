using Mediator;

namespace Couture.Notifications.Features.MarkRead;

public sealed record MarkReadCommand(Guid NotificationId, Guid UserId) : ICommand;
public sealed record MarkAllReadCommand(Guid UserId) : ICommand;
