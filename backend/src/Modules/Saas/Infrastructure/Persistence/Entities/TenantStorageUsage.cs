namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class TenantStorageUsage
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public long ActiveBytes { get; set; }
    public long RecycleBinBytes { get; set; }
    public long QuarantinedBytes { get; set; }
    public long ReservedBytes { get; set; }
    public long ObjectCount { get; set; }
    public int LockVersion { get; set; }
    public DateTimeOffset? ReconciledAt { get; set; }
}
