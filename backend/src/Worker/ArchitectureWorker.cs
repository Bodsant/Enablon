namespace Ehsms.Worker;
public sealed class ArchitectureWorker(ILogger<ArchitectureWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Architecture worker host started; no business jobs are registered.");
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
