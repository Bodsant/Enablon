namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class UsageEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UsagePeriodId { get; set; }
    public string EventType { get; set; } = null!;
    public Guid? ReferenceId { get; set; }
    public long StorageBytesDelta { get; set; }
    public long UploadBytesDelta { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
