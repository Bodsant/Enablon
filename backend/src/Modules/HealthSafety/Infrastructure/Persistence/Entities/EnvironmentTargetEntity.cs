namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.targets</c> table.</summary>
public sealed class EnvironmentTargetEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid? ParameterId { get; set; } = null;
    public Guid? SiteId { get; set; } = null;
    public DateOnly? PeriodStart { get; set; } = null;
    public DateOnly? PeriodEnd { get; set; } = null;
    public decimal? TargetValue { get; set; } = null;
    public string? Unit { get; set; } = null;
    public Guid? OwnerMemberId { get; set; } = null;
}
