using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Jobs;

/// <summary>
/// Background service that runs notification jobs on a schedule:
/// - EvaluateOverdueOrdersJob: every hour
/// - EvaluateStalledOrdersJob: every 6 hours
/// - PurgeExpiredNotificationsJob: once per day
/// </summary>
public sealed class NotificationJobScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationJobScheduler> _logger;

    private static readonly TimeSpan OverdueInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan StalledInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(24);

    public NotificationJobScheduler(IServiceScopeFactory scopeFactory, ILogger<NotificationJobScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationJobScheduler started");

        // Delay initial run by 30s to let the app finish starting up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        var lastOverdue = DateTimeOffset.MinValue;
        var lastStalled = DateTimeOffset.MinValue;
        var lastPurge = DateTimeOffset.MinValue;

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var now = DateTimeOffset.UtcNow;

            if (now - lastOverdue >= OverdueInterval)
            {
                await RunJobAsync<EvaluateOverdueOrdersJob>("EvaluateOverdue", stoppingToken);
                lastOverdue = now;
            }

            if (now - lastStalled >= StalledInterval)
            {
                await RunJobAsync<EvaluateStalledOrdersJob>("EvaluateStalled", stoppingToken);
                lastStalled = now;
            }

            if (now - lastPurge >= PurgeInterval)
            {
                await RunJobAsync<PurgeExpiredNotificationsJob>("PurgeExpired", stoppingToken);
                lastPurge = now;
            }
        }
    }

    private async Task RunJobAsync<TJob>(string jobName, CancellationToken ct) where TJob : notnull
    {
        try
        {
            _logger.LogInformation("Running notification job: {Job}", jobName);

            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<TJob>();

            var executeMethod = typeof(TJob).GetMethod("ExecuteAsync")
                ?? throw new InvalidOperationException($"Job {typeof(TJob).Name} has no ExecuteAsync method");

            if (executeMethod.Invoke(job, null) is Task task)
                await task;

            _logger.LogInformation("Notification job {Job} completed", jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification job {Job} failed", jobName);
        }
    }
}
