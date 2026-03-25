using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Sms;

public sealed class MockSmsGateway : ISmsGateway
{
    private readonly ILogger<MockSmsGateway> _logger;

    public MockSmsGateway(ILogger<MockSmsGateway> logger) => _logger = logger;

    public Task<SmsResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[MOCK SMS] To: {Phone} | Message: {Message}", phoneNumber, message);
        return Task.FromResult(new SmsResult(true, Guid.NewGuid().ToString(), null));
    }
}
