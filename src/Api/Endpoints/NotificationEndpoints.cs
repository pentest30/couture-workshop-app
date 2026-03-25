using Couture.Identity.Contracts;
using Couture.Notifications.Features.ConfigureAlerts;
using Couture.Notifications.Features.GetSmsLogs;
using Couture.Notifications.Features.GetUnreadCount;
using Couture.Notifications.Features.ListNotifications;
using Couture.Notifications.Features.MarkRead;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications")
            .RequireAuthorization(CouturePermissions.NotificationsView);

        group.MapGet("/", ListNotifications);
        group.MapGet("/unread-count", GetUnreadCount);
        group.MapPost("/{id:guid}/read", MarkRead);
        group.MapPost("/read-all", MarkAllRead);

        var admin = app.MapGroup("/api/notifications/admin").WithTags("Notifications Admin")
            .RequireAuthorization(CouturePermissions.NotificationsConfigure);

        admin.MapPut("/config/{typeValue:int}", ConfigureAlerts);
        admin.MapGet("/sms-logs", GetSmsLogs);
    }

    private static async Task<IResult> ListNotifications(
        IMediator mediator, ICurrentUser currentUser,
        [FromQuery] string filter = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new ListNotificationsQuery(currentUser.UserId, filter, page, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUnreadCount(IMediator mediator, ICurrentUser currentUser)
    {
        var count = await mediator.Send(new GetUnreadCountQuery(currentUser.UserId));
        return Results.Ok(new { count });
    }

    private static async Task<IResult> MarkRead(Guid id, IMediator mediator, ICurrentUser currentUser)
    {
        await mediator.Send(new MarkReadCommand(id, currentUser.UserId));
        return Results.NoContent();
    }

    private static async Task<IResult> MarkAllRead(IMediator mediator, ICurrentUser currentUser)
    {
        await mediator.Send(new MarkAllReadCommand(currentUser.UserId));
        return Results.NoContent();
    }

    private static async Task<IResult> ConfigureAlerts(
        int typeValue, [FromBody] ConfigureAlertsRequest request, IMediator mediator)
    {
        await mediator.Send(new ConfigureAlertsCommand(
            typeValue, request.IsEnabled, request.SmsEnabled,
            request.StallThresholdSimple, request.StallThresholdEmbroidered,
            request.StallThresholdBeaded, request.StallThresholdMixed,
            request.SmsWindowStart, request.SmsWindowEnd));
        return Results.NoContent();
    }

    private static async Task<IResult> GetSmsLogs(
        IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetSmsLogsQuery(page, pageSize));
        return Results.Ok(result);
    }
}

public record ConfigureAlertsRequest(
    bool? IsEnabled, bool? SmsEnabled,
    int? StallThresholdSimple, int? StallThresholdEmbroidered,
    int? StallThresholdBeaded, int? StallThresholdMixed,
    string? SmsWindowStart, string? SmsWindowEnd);
