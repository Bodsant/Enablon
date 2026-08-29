namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>reporting.report_schedules</c> table. Scheduled report executions.</summary>
public sealed class ReportScheduleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ReportDefinitionId { get; set; }
    public Guid OwnerMemberId { get; set; }
    public string ScheduleRule { get; set; } = string.Empty;
    public string? DeliveryConfigurationJson { get; set; }
    public string Status { get; set; } = string.Empty;

    public ReportDefinitionEntity? ReportDefinition { get; set; }
    public ICollection<ReportExecutionEntity> Executions { get; set; } = new List<ReportExecutionEntity>();
}