namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.monitoring_records</c> table.</summary>
public sealed class MonitoringRecordEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public Guid EnvironmentSourceId { get; set; } = Guid.Empty;
    public DateTimeOffset? PeriodStart { get; set; } = null;
    public DateTimeOffset? PeriodEnd { get; set; } = null;
    public Guid? PerformedByMemberId { get; set; } = null;
    public string Status { get; set; } = string.Empty;

}
