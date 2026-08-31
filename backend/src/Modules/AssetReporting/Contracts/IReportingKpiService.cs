namespace Ehsms.Modules.AssetReporting.Contracts;

/// <summary>Payload to define a report.</summary>
public sealed record CreateReportDefinitionRequest(
    string Code,
    string Name,
    string ReportType,
    string DatasetCode,
    string? FilterSchemaJson,
    Guid? RequiredPermissionId);

/// <summary>Payload to schedule a report.</summary>
public sealed record CreateReportScheduleRequest(
    Guid ReportDefinitionId,
    Guid OwnerMemberId,
    string ScheduleRule,
    string? DeliveryConfigurationJson,
    string Status);

/// <summary>Payload to trigger a report execution.</summary>
public sealed record CreateReportExecutionRequest(
    Guid ReportDefinitionId,
    Guid? ReportScheduleId,
    Guid? RequestedByMemberId,
    string? FilterValuesJson,
    string Status);

/// <summary>Payload to define a KPI.</summary>
public sealed record CreateKpiDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    Guid OwnerMemberId,
    string Status);

/// <summary>Payload to version a KPI definition.</summary>
public sealed record CreateKpiVersionRequest(
    Guid KpiDefinitionId,
    int VersionNumber,
    string FormulaExpression,
    string? NumeratorDefinition,
    string? DenominatorDefinition,
    decimal? Factor,
    string? PeriodRule,
    string? ScopeRuleJson,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);

public sealed record ReportDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string ReportType,
    string DatasetCode,
    string? FilterSchemaJson,
    Guid? RequiredPermissionId);

public sealed record ReportScheduleDto(
    Guid Id,
    Guid ReportDefinitionId,
    Guid OwnerMemberId,
    string ScheduleRule,
    string? DeliveryConfigurationJson,
    string Status);

public sealed record ReportExecutionDto(
    Guid Id,
    Guid ReportDefinitionId,
    Guid? ReportScheduleId,
    Guid? RequestedByMemberId,
    string? FilterValuesJson,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record KpiDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid OwnerMemberId,
    string Status);

public sealed record KpiVersionDto(
    Guid Id,
    Guid KpiDefinitionId,
    int VersionNumber,
    string FormulaExpression,
    string? NumeratorDefinition,
    string? DenominatorDefinition,
    decimal? Factor,
    string? PeriodRule,
    string? ScopeRuleJson,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);

/// <summary>Reporting &amp; KPI backend service (Trello Sprint 27 R2).</summary>
public interface IReportingKpiService
{
    Task<ReportDefinitionDto> CreateReportDefinitionAsync(CreateReportDefinitionRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ReportDefinitionDto>> ListReportDefinitionsAsync(Guid tenantId, CancellationToken ct);

    Task<ReportScheduleDto> CreateReportScheduleAsync(
        CreateReportScheduleRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<ReportScheduleDto>> ListReportSchedulesAsync(Guid tenantId, CancellationToken ct);

    Task<ReportExecutionDto> CreateReportExecutionAsync(
        CreateReportExecutionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<ReportExecutionDto>> ListReportExecutionsAsync(Guid reportDefinitionId, Guid tenantId, CancellationToken ct);

    Task<KpiDefinitionDto> CreateKpiDefinitionAsync(
        CreateKpiDefinitionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<KpiDefinitionDto>> ListKpiDefinitionsAsync(Guid tenantId, CancellationToken ct);

    Task<KpiVersionDto> CreateKpiVersionAsync(CreateKpiVersionRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<KpiVersionDto>> ListKpiVersionsAsync(Guid kpiDefinitionId, Guid tenantId, CancellationToken ct);
}