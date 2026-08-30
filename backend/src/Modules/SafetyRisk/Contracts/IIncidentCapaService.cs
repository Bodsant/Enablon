namespace Ehsms.Modules.SafetyRisk.Contracts;

/// <summary>Payload to report an incident.</summary>
public sealed record CreateIncidentRequest(
    Guid IncidentTypeId,
    Guid SeverityId,
    DateTimeOffset? OccurredAt,
    string Description,
    string? ImmediateAction,
    string? ClassificationStatus);

/// <summary>Payload to add an involved person to an incident.</summary>
public sealed record AddInvolvedPersonRequest(
    Guid IncidentId,
    Guid? PersonId,
    string? ExternalPersonName,
    string InvolvementType,
    Guid? InjuryClassificationId,
    int? LostWorkDays);

/// <summary>Payload to start an investigation.</summary>
public sealed record StartInvestigationRequest(
    Guid IncidentId,
    string? Method,
    string? Summary);

/// <summary>Payload to add a root cause to an investigation.</summary>
public sealed record AddRootCauseRequest(
    Guid InvestigationId,
    string CauseType,
    Guid? CategoryId,
    string Description,
    string? EvidenceSummary);

/// <summary>Payload to create a CAPA action.</summary>
public sealed record CreateCapaActionRequest(
    string ActionType,
    string Description,
    Guid? OwnerMemberId,
    string Priority,
    DateOnly? DueDate,
    bool VerificationRequired);

/// <summary>Payload to progress a CAPA action.</summary>
public sealed record ProgressCapaActionRequest(
    Guid ActionId,
    short ProgressPercentage,
    string Note);

/// <summary>Payload to verify a CAPA action.</summary>
public sealed record VerifyCapaActionRequest(
    Guid ActionId,
    string Result,
    string? Comment);

public sealed record IncidentDto(
    Guid Id,
    string RecordNumber,
    Guid IncidentTypeId,
    Guid SeverityId,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt,
    Guid ReportedByMemberId,
    string Description,
    string? ImmediateAction,
    string? ClassificationStatus);

public sealed record InvolvedPersonDto(
    Guid Id,
    Guid IncidentId,
    Guid? PersonId,
    string? ExternalPersonName,
    string InvolvementType,
    Guid? InjuryClassificationId,
    int? LostWorkDays);

public sealed record InvestigationDto(
    Guid Id,
    Guid IncidentId,
    Guid LeadInvestigatorMemberId,
    string? Method,
    string? Summary,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record RootCauseDto(
    Guid Id,
    Guid InvestigationId,
    string CauseType,
    Guid? CategoryId,
    string Description,
    string? EvidenceSummary);

public sealed record CapaActionDto(
    Guid Id,
    string RecordNumber,
    string ActionType,
    string Description,
    Guid OwnerMemberId,
    string Priority,
    DateOnly DueDate,
    short ProgressPercentage,
    bool VerificationRequired);

/// <summary>Incident &amp; CAPA backend service (Trello Sprint 13).</summary>
public interface IIncidentCapaService
{
    Task<IncidentDto> CreateIncidentAsync(CreateIncidentRequest request, Guid tenantId, Guid reportedByMemberId, CancellationToken ct);
    Task<IReadOnlyList<IncidentDto>> ListIncidentsAsync(Guid tenantId, CancellationToken ct);
    Task<Guid> AddInvolvedPersonAsync(AddInvolvedPersonRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<InvolvedPersonDto>> ListInvolvedPeopleAsync(Guid? incidentId, Guid tenantId, CancellationToken ct);

    Task<Guid> StartInvestigationAsync(StartInvestigationRequest request, Guid tenantId, Guid leadInvestigatorMemberId, CancellationToken ct);
    Task<IReadOnlyList<InvestigationDto>> ListInvestigationsAsync(Guid? incidentId, Guid tenantId, CancellationToken ct);
    Task<Guid> AddRootCauseAsync(AddRootCauseRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<RootCauseDto>> ListRootCausesAsync(Guid? investigationId, Guid tenantId, CancellationToken ct);

    Task<CapaActionDto> CreateActionAsync(CreateCapaActionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<CapaActionDto>> ListActionsAsync(Guid tenantId, CancellationToken ct);
    Task ProgressActionAsync(ProgressCapaActionRequest request, Guid tenantId, Guid updatedByMemberId, CancellationToken ct);
    Task VerifyActionAsync(VerifyCapaActionRequest request, Guid tenantId, Guid verifierMemberId, CancellationToken ct);
}
