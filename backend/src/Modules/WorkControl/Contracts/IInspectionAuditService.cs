namespace Ehsms.Modules.WorkControl.Contracts;

/// <summary>Payload to create an audit program.</summary>
public sealed record CreateAuditProgramRequest(
    string Name,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    Guid? OwnerMemberId,
    string Status);

/// <summary>Payload to schedule/conduct an audit.</summary>
public sealed record CreateAuditRequest(
    Guid? AuditProgramId,
    string AuditType,
    string ScopeText,
    string? CriteriaText,
    Guid? LeadAuditorMemberId,
    DateOnly? ScheduledStart,
    DateOnly? ScheduledEnd);

/// <summary>Payload to record an audit finding.</summary>
public sealed record CreateAuditFindingRequest(
    Guid AuditId,
    string Classification,
    string? RequirementReference,
    string Description,
    string? Recommendation,
    Guid? OwnerMemberId);

/// <summary>Payload to conduct an inspection.</summary>
public sealed record CreateInspectionRequest(
    string ScopeText,
    Guid? InspectorMemberId,
    DateTimeOffset? PlannedAt);

/// <summary>Payload to record an inspection finding.</summary>
public sealed record CreateInspectionFindingRequest(
    Guid InspectionId,
    string? Classification,
    Guid? SeverityId,
    string Description,
    Guid? OwnerMemberId);

public sealed record AuditProgramDto(
    Guid Id,
    string Name,
    string Status,
    Guid OwnerMemberId,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd);

public sealed record AuditDto(
    Guid Id,
    string RecordNumber,
    Guid? AuditProgramId,
    string AuditType,
    string ScopeText,
    string? CriteriaText,
    Guid LeadAuditorMemberId,
    DateOnly? ScheduledStart,
    DateOnly? ScheduledEnd);

public sealed record AuditFindingDto(
    Guid Id,
    string RecordNumber,
    Guid AuditId,
    string Classification,
    string? RequirementReference,
    string Description,
    string? Recommendation,
    Guid? OwnerMemberId);

public sealed record InspectionDto(
    Guid Id,
    string RecordNumber,
    Guid? ScheduleId,
    Guid? TemplateVersionId,
    Guid InspectorMemberId,
    DateTimeOffset? PlannedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    decimal? CompliancePercentage);

public sealed record InspectionFindingDto(
    Guid Id,
    string RecordNumber,
    Guid InspectionId,
    string? Classification,
    Guid? SeverityId,
    string Description,
    Guid? OwnerMemberId);

/// <summary>Inspection &amp; Audit backend service (Trello Sprint 15).</summary>
public interface IInspectionAuditService
{
    Task<Guid> CreateAuditProgramAsync(CreateAuditProgramRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<AuditProgramDto>> ListAuditProgramsAsync(Guid tenantId, CancellationToken ct);

    Task<AuditDto> CreateAuditAsync(CreateAuditRequest request, Guid tenantId, Guid leadAuditorMemberId, CancellationToken ct);
    Task<IReadOnlyList<AuditDto>> ListAuditsAsync(Guid tenantId, CancellationToken ct);
    Task<Guid> CreateAuditFindingAsync(CreateAuditFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<AuditFindingDto>> ListAuditFindingsAsync(Guid? auditId, Guid tenantId, CancellationToken ct);

    Task<InspectionDto> CreateInspectionAsync(CreateInspectionRequest request, Guid tenantId, Guid inspectorMemberId, CancellationToken ct);
    Task<IReadOnlyList<InspectionDto>> ListInspectionsAsync(Guid tenantId, CancellationToken ct);
    Task<Guid> CreateInspectionFindingAsync(CreateInspectionFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<InspectionFindingDto>> ListInspectionFindingsAsync(Guid? inspectionId, Guid tenantId, CancellationToken ct);
}
