namespace Ehsms.Modules.WorkControl.Contracts;

/// <summary>Payload to create a work request.</summary>
public sealed record CreateWorkRequestRequest(
    string WorkDescription,
    string WorkType,
    Guid? ContractorCompanyId,
    DateTimeOffset? PlannedStart,
    DateTimeOffset? PlannedEnd);

/// <summary>Payload to create a Job Safety Analysis.</summary>
public sealed record CreateJsaRequest(
    Guid WorkRequestId,
    Guid? TemplateVersionId,
    string? OverallResidualRisk,
    string Status);

/// <summary>Payload to add a JSA step.</summary>
public sealed record CreateJsaStepRequest(
    Guid JsaId,
    int SequenceNumber,
    string WorkStep);

/// <summary>Payload to create a Permit to Work.</summary>
public sealed record CreatePermitRequest(
    Guid WorkRequestId,
    Guid? JsaId,
    Guid PermitTypeVersionId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil);

/// <summary>Payload to approve a permit.</summary>
public sealed record ApprovePermitRequest(
    Guid PermitId,
    int ApprovalLevel,
    string? Decision);

/// <summary>Payload to record a gas test against a permit.</summary>
public sealed record RecordGasTestRequest(
    Guid PermitId,
    string TestType,
    DateTimeOffset? TestedAt,
    decimal? OxygenPct,
    decimal? LelPct,
    string? ToxicGasJson,
    string Result);

/// <summary>Payload to create a LOTO isolation plan.</summary>
public sealed record CreateIsolationPlanRequest(
    Guid PermitId,
    string Status);

public sealed record WorkRequestDto(
    Guid Id,
    string RecordNumber,
    string WorkDescription,
    string WorkType,
    Guid? ContractorCompanyId,
    DateTimeOffset? PlannedStart,
    DateTimeOffset? PlannedEnd);

public sealed record JsaDto(
    Guid Id,
    string RecordNumber,
    Guid WorkRequestId,
    Guid? TemplateVersionId,
    Guid PreparedByMemberId,
    string Status,
    string? OverallResidualRisk);

public sealed record JsaStepDto(
    Guid Id,
    Guid JsaId,
    int SequenceNumber,
    string WorkStep);

public sealed record PermitDto(
    Guid Id,
    string RecordNumber,
    Guid WorkRequestId,
    Guid? JsaId,
    Guid PermitTypeVersionId,
    Guid RequesterMemberId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil);

public sealed record IsolationPlanDto(
    Guid Id,
    string RecordNumber,
    Guid PermitId,
    Guid PreparedByMemberId,
    string Status);

/// <summary>PTW / JSA / LOTO backend service (Trello Sprint 17).</summary>
public interface IPtwJsaLotoService
{
    Task<WorkRequestDto> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid tenantId, Guid requesterMemberId, CancellationToken ct);
    Task<IReadOnlyList<WorkRequestDto>> ListWorkRequestsAsync(Guid tenantId, CancellationToken ct);

    Task<JsaDto> CreateJsaAsync(CreateJsaRequest request, Guid tenantId, Guid preparedByMemberId, CancellationToken ct);
    Task<Guid> AddJsaStepAsync(CreateJsaStepRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<JsaDto>> ListJsasAsync(Guid tenantId, CancellationToken ct);

    Task<PermitDto> CreatePermitAsync(CreatePermitRequest request, Guid tenantId, Guid requesterMemberId, CancellationToken ct);
    Task ApprovePermitAsync(ApprovePermitRequest request, Guid tenantId, Guid approverMemberId, CancellationToken ct);
    Task<Guid> RecordGasTestAsync(RecordGasTestRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<PermitDto>> ListPermitsAsync(Guid tenantId, CancellationToken ct);

    Task<IsolationPlanDto> CreateIsolationPlanAsync(CreateIsolationPlanRequest request, Guid tenantId, Guid preparedByMemberId, CancellationToken ct);
    Task<IReadOnlyList<IsolationPlanDto>> ListIsolationPlansAsync(Guid tenantId, CancellationToken ct);
}
