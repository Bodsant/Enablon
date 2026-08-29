namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.measurements</c> table.</summary>
public sealed class EnvironmentMeasurementEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid MonitoringRecordId { get; set; } = Guid.Empty;
    public Guid ParameterId { get; set; } = Guid.Empty;
    public DateTimeOffset MeasuredAt { get; set; } = default;
    public decimal? ResultValue { get; set; } = null;
    public string? Unit { get; set; } = null;
    public decimal? LimitValue { get; set; } = null;
    public decimal? TargetValue { get; set; } = null;
    public string? QualityFlag { get; set; } = null;
    public string? ComplianceStatus { get; set; } = null;
}
