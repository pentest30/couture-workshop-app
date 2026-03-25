namespace Couture.Notifications.Sms;

public interface ISmsGateway
{
    Task<SmsResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}

public record SmsResult(bool Success, string? MessageId, string? Error);
