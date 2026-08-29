namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.work_monitoring</c> table.</summary>
public sealed class WorkMonitoringEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkExecutionId { get; set; }
    public Guid MonitoredByMemberId { get; set; }
    public DateTimeOffset MonitoredAt { get; set; }
    public string ConditionStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
