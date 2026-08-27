namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class TenantStorageUsage
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public long UsedBytes { get; set; }
    public DateTimeOffset MeasuredAt { get; set; }
}