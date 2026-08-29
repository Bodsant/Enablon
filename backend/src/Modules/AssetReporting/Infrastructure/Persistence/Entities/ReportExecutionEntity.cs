namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>reporting.report_executions</c> table. Report execution runs.</summary>
public sealed class ReportExecutionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ReportDefinitionId { get; set; }
    public Guid? ReportScheduleId { get; set; }
    public Guid? RequestedByMemberId { get; set; }
    public string? FilterValuesJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? OutputFileObjectId { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public ReportDefinitionEntity? ReportDefinition { get; set; }
    public ReportScheduleEntity? ReportSchedule { get; set; }
}