namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class TenantSubscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanVersionId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Status { get; set; } = null!;
    public int MaxActiveUsersOverride { get; set; }
    public int? MaxCompaniesOverride { get; set; }
    public int? MaxBusinessUnitsOverride { get; set; }
    public int? MaxSitesOverride { get; set; }
    public long MaxStorageBytesOverride { get; set; }
    public long MaxPeriodUploadBytesOverride { get; set; }
    public long MaxFileSizeBytesOverride { get; set; }
}