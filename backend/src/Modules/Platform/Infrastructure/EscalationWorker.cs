using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Escalation background job: periodically promotes workflow tasks that are still Open
/// past their due time to a Critical priority and records an <c>workflow.escalated</c>
/// audit entry. This is the foundation escalation behaviour; richer rule-driven
/// escalation (reassignment, multi-level routing, SLA counters) arrives in later sprints.
/// Idempotent: a task is only escalated once (status is bumped to "Escalated").
/// </summary>
public sealed class EscalationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EscalationWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public EscalationWorker(IServiceScopeFactory scopeFactory, ILogger<EscalationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await EscalateOverdueAsync(stoppingToken);
        }
    }

    private async Task EscalateOverdueAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var now = DateTimeOffset.UtcNow;

            var overdue = await db.WorkflowTasks
                .Where(t => t.Status == "Open" && t.DueAt != null && t.DueAt < now)
                .Take(50)
                .ToListAsync(cancellationToken);

            var changed = false;
            foreach (var task in overdue)
            {
                task.Status = "Escalated";
                task.Priority = "Critical";
                db.AuditLogs.Add(new AuditLogEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = task.TenantId,
                    RecordId = task.Instance?.RecordId,
                    UserId = null,
                    ActionCode = "workflow.escalated",
                    AfterJson = System.Text.Json.JsonSerializer.Serialize(new { task.Id, task.Priority }),
                    OccurredAt = now,
                });
                _logger.LogInformation("Escalated overdue workflow task {TaskId}", task.Id);
                changed = true;
            }

            if (changed)
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
            _logger.LogError(ex, "Escalation worker cycle failed");
        }
    }
}