namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>reporting.report_definitions</c> table. Report definitions.</summary>
public sealed class ReportDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string DatasetCode { get; set; } = string.Empty;
    public string? FilterSchemaJson { get; set; }
    public Guid? RequiredPermissionId { get; set; }

    public ICollection<ReportScheduleEntity> Schedules { get; set; } = new List<ReportScheduleEntity>();
    public ICollection<ReportExecutionEntity> Executions { get; set; } = new List<ReportExecutionEntity>();
}