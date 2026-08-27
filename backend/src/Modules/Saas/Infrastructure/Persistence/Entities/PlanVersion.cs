namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class PlanVersion
{
    public Guid Id { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public int VersionNumber { get; set; }
    public int MaxActiveUsers { get; set; }
    public int? MaxCompanies { get; set; }
    public int? MaxBusinessUnits { get; set; }
    public int? MaxSites { get; set; }
    public long MaxStorageBytes { get; set; }
    public long MaxPeriodUploadBytes { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveUntil { get; set; }
    public bool IsCurrent { get; set; }
}