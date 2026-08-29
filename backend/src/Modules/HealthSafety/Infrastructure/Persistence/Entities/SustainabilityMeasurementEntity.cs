namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>sustainability.measurements</c> table.</summary>
public sealed class SustainabilityMeasurementEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public Guid IndicatorDefinitionId { get; set; } = Guid.Empty;
    public Guid? FactorVersionId { get; set; } = null;
    public string? ScopeCode { get; set; } = null;
    public DateOnly PeriodStart { get; set; } = default;
    public DateOnly PeriodEnd { get; set; } = default;
    public decimal? ActualValue { get; set; } = null;
    public string? Unit { get; set; } = null;
    public string? CalculationJson { get; set; } = null;
    public Guid? OwnerMemberId { get; set; } = null;
}
