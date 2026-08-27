namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class TenantUsagePeriod
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantSubscriptionId { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public long UploadBytesUsed { get; set; }
    public int ActiveUsersPeak { get; set; }
}