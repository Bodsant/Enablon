namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>sustainability.targets</c> table.</summary>
public sealed class SustainabilityTargetEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid IndicatorDefinitionId { get; set; } = Guid.Empty;
    public Guid? SiteId { get; set; } = null;
    public DateOnly? PeriodStart { get; set; } = null;
    public DateOnly? PeriodEnd { get; set; } = null;
    public decimal? TargetValue { get; set; } = null;
    public string? Unit { get; set; } = null;
}
