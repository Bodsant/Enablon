using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Transactional-outbox dispatcher: periodically publishes pending
/// <c>platform.outbox_messages</c> in creation order and retries failures with a small
/// backoff. In this foundation stage "publishing" marks the event as dispatched and
/// logs it; real subscriber/fan-out wiring arrives with the workflow engine. The worker
/// is idempotent: it only picks rows still in <c>Pending</c> whose retry time has passed.
/// </summary>
public sealed class OutboxDispatcherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

    public OutboxDispatcherWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DispatchPendingAsync(stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

            var now = DateTimeOffset.UtcNow;
            var pending = await db.OutboxMessages
                .Where(m => m.Status == "Pending" && (m.NextRetryAt == null || m.NextRetryAt <= now))
                .OrderBy(m => m.OccurredAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var message in pending)
            {
                try
                {
                    // Foundation dispatcher: acknowledge the event. Subscribers (workflow
                    // triggers, notifications, integrations) are wired in later sprints.
                    _logger.LogInformation("Dispatching outbox event {EventType} {MessageId}", message.EventType, message.Id);
                    message.Status = "Dispatched";
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.NextRetryAt = now.AddMinutes(Math.Min(30, 1 << message.AttemptCount));
                    _logger.LogWarning(ex, "Outbox dispatch attempt {Attempt} failed for {MessageId}", message.AttemptCount, message.Id);
                }
            }

            if (pending.Count > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outbox dispatcher cycle failed");
        }
    }
}