namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class UsageEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantUsagePeriodId { get; set; }
    public Guid? UploadSessionId { get; set; }
    public string EventType { get; set; } = null!;
    public long Bytes { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}