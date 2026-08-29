using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Recycle-bin / purge lifecycle worker. Files soft-deleted from the recycle bin with an overdue
/// <c>PurgeAfter</c> are permanently removed from object storage and marked <c>PurgedAt</c>.
/// </summary>
public sealed class PurgeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PurgeWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

    public PurgeWorker(IServiceScopeFactory scopeFactory, ILogger<PurgeWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PurgeExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Purge cycle failed");
            }
        }
    }

    private async Task PurgeExpiredAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<Application.IObjectStorage>();

        var due = await db.FileObjects
            .Where(f => f.Status == "Deleted" && f.DeletedAt != null
                && f.PurgeAfter != null && f.PurgeAfter <= now
                && f.PurgedAt == null)
            .Take(200)
            .ToListAsync(ct);

        foreach (var fileObject in due)
        {
            await storage.DeleteAsync(fileObject.BucketName, fileObject.ObjectKey, ct);
            fileObject.PurgedAt = now;
            _logger.LogInformation("Purged expired file object {FileId}", fileObject.Id);
        }

        if (due.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
