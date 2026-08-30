namespace Ehsms.Modules.SafetyRisk.Contracts;

/// <summary>Payload to create a hazard.</summary>
public sealed record CreateHazardRequest(
    string Code,
    string Name,
    Guid? CategoryId,
    string? Description,
    string Status);

/// <summary>Payload to create a risk register entry.</summary>
public sealed record CreateRiskRegisterRequest(
    Guid HazardId,
    string ActivityName,
    string RiskEvent,
    Guid? OwnerMemberId,
    DateOnly? ReviewDate,
    string Status);

/// <summary>Payload to record a risk assessment.</summary>
public sealed record CreateRiskAssessmentRequest(
    Guid RiskRegisterId,
    Guid MatrixVersionId,
    string AssessmentType,
    short LikelihoodValue,
    short SeverityValue,
    Guid AssessedByMemberId);

/// <summary>Payload to create a risk matrix version.</summary>
public sealed record CreateRiskMatrixVersionRequest(
    string Name,
    int VersionNumber,
    int LikelihoodScale,
    int SeverityScale,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status);

/// <summary>Payload to seed a risk matrix cell (lookup for risk rating).</summary>
public sealed record CreateRiskMatrixCellRequest(
    Guid MatrixVersionId,
    short LikelihoodValue,
    short SeverityValue,
    int RiskScore,
    string RiskLevelCode);

/// <summary>Payload to add a risk control.</summary>
public sealed record CreateRiskControlRequest(
    Guid RiskRegisterId,
    string ControlType,
    string ControlStage,
    string Description,
    Guid? OwnerMemberId,
    DateOnly? DueDate,
    string Status);

public sealed record HazardDto(
    Guid Id,
    string Code,
    string Name,
    Guid? CategoryId,
    string? Description,
    string Status);

public sealed record RiskRegisterDto(
    Guid Id,
    Guid HazardId,
    string ActivityName,
    string RiskEvent,
    Guid OwnerMemberId,
    DateOnly? ReviewDate,
    string Status);

public sealed record RiskAssessmentDto(
    Guid Id,
    Guid RiskRegisterId,
    Guid MatrixVersionId,
    string AssessmentType,
    int SequenceNumber,
    short LikelihoodValue,
    short SeverityValue,
    int RiskScore,
    string RiskLevelCode,
    Guid AssessedByMemberId,
    DateTimeOffset AssessedAt);

public sealed record RiskControlDto(
    Guid Id,
    Guid RiskRegisterId,
    string ControlType,
    string ControlStage,
    string Description,
    Guid? OwnerMemberId,
    DateOnly? DueDate,
    string Status,
    short? EffectivenessRating);

public sealed record RiskMatrixVersionDto(
    Guid Id,
    string Name,
    int VersionNumber,
    int LikelihoodScale,
    int SeverityScale,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status);

public sealed record RiskMatrixCellDto(
    Guid Id,
    Guid MatrixVersionId,
    short LikelihoodValue,
    short SeverityValue,
    int RiskScore,
    string RiskLevelCode);

/// <summary>Hazard & Risk backend service (Trello Sprint 11).</summary>
public interface IHazardRiskService
{
    Task<Guid> CreateHazardAsync(CreateHazardRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<HazardDto>> ListHazardsAsync(Guid tenantId, CancellationToken ct);

    Task<Guid> CreateRegisterAsync(CreateRiskRegisterRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<RiskRegisterDto>> ListRegistersAsync(Guid tenantId, CancellationToken ct);

    Task<Guid> CreateMatrixVersionAsync(CreateRiskMatrixVersionRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<RiskMatrixVersionDto>> ListMatrixVersionsAsync(Guid tenantId, CancellationToken ct);
    Task<Guid> CreateMatrixCellAsync(CreateRiskMatrixCellRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<RiskMatrixCellDto>> ListMatrixCellsAsync(Guid matrixVersionId, Guid tenantId, CancellationToken ct);

    Task<Guid> CreateAssessmentAsync(CreateRiskAssessmentRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<RiskAssessmentDto>> ListAssessmentsAsync(Guid tenantId, CancellationToken ct);

    Task<Guid> CreateControlAsync(CreateRiskControlRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<RiskControlDto>> ListControlsAsync(Guid tenantId, CancellationToken ct);
}