namespace Ehsms.Modules.AssetReporting.Contracts;

/// <summary>Payload to register a safety asset.</summary>
public sealed record CreateAssetRequest(
    string? SourceSystem,
    string? SourceId,
    string AssetCode,
    string AssetName,
    string? AssetType,
    Guid SiteId,
    Guid? LocationId,
    bool IsSafetyCritical,
    string Status);

/// <summary>Payload to create an emergency response plan.</summary>
public sealed record CreateEmergencyPlanRequest(
    string Code,
    string Name,
    Guid SiteId,
    Guid OwnerMemberId,
    string Status);

/// <summary>Payload to add a team member to an emergency plan.</summary>
public sealed record AddEmergencyTeamMemberRequest(
    Guid EmergencyPlanId,
    Guid PersonId,
    string EmergencyRole,
    DateOnly? ValidFrom,
    DateOnly? ValidTo);

/// <summary>Payload to register emergency equipment.</summary>
public sealed record CreateEmergencyEquipmentRequest(
    Guid SiteId,
    Guid? LocationId,
    string EquipmentType,
    Guid? AssetId,
    DateOnly? InspectionDueDate,
    DateOnly? MaintenanceDueDate,
    string Status);

/// <summary>Payload to schedule an emergency drill.</summary>
public sealed record CreateEmergencyDrillRequest(
    Guid EmergencyPlanId,
    string Scenario,
    DateTimeOffset? ScheduledAt,
    Guid? CoordinatorMemberId);

/// <summary>Payload to record a drill finding.</summary>
public sealed record CreateEmergencyDrillFindingRequest(
    Guid EmergencyDrillId,
    string Description,
    string? Severity,
    Guid? OwnerMemberId);

public sealed record AssetDto(
    Guid Id,
    string RecordNumber,
    string AssetCode,
    string AssetName,
    string? AssetType,
    Guid SiteId,
    Guid? LocationId,
    bool IsSafetyCritical,
    string Status);

public sealed record EmergencyPlanDto(
    Guid Id,
    string RecordNumber,
    string Code,
    string Name,
    Guid SiteId,
    Guid OwnerMemberId,
    string Status);

public sealed record EmergencyTeamMemberDto(
    Guid Id,
    Guid EmergencyPlanId,
    Guid PersonId,
    string EmergencyRole,
    DateOnly? ValidFrom,
    DateOnly? ValidTo);

public sealed record EmergencyEquipmentDto(
    Guid Id,
    Guid SiteId,
    Guid? LocationId,
    string EquipmentType,
    Guid? AssetId,
    DateOnly? InspectionDueDate,
    DateOnly? MaintenanceDueDate,
    string Status);

public sealed record EmergencyDrillDto(
    Guid Id,
    string RecordNumber,
    Guid EmergencyPlanId,
    string Scenario,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? ConductedAt,
    string? ResultSummary,
    Guid? CoordinatorMemberId);

public sealed record EmergencyDrillFindingDto(
    Guid Id,
    string RecordNumber,
    Guid EmergencyDrillId,
    string Description,
    string? Severity,
    Guid? OwnerMemberId);

/// <summary>Asset safety &amp; emergency backend service (Trello Sprint 26 R2).</summary>
public interface IAssetEmergencyService
{
    Task<AssetDto> CreateAssetAsync(CreateAssetRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<AssetDto>> ListAssetsAsync(Guid tenantId, CancellationToken ct);

    Task<EmergencyPlanDto> CreateEmergencyPlanAsync(
        CreateEmergencyPlanRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<EmergencyPlanDto>> ListEmergencyPlansAsync(Guid tenantId, CancellationToken ct);

    Task<EmergencyTeamMemberDto> AddEmergencyTeamMemberAsync(AddEmergencyTeamMemberRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<EmergencyTeamMemberDto>> ListEmergencyTeamMembersAsync(Guid planId, Guid tenantId, CancellationToken ct);

    Task<EmergencyEquipmentDto> CreateEmergencyEquipmentAsync(CreateEmergencyEquipmentRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<EmergencyEquipmentDto>> ListEmergencyEquipmentAsync(Guid tenantId, CancellationToken ct);

    Task<EmergencyDrillDto> CreateEmergencyDrillAsync(
        CreateEmergencyDrillRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<EmergencyDrillDto>> ListEmergencyDrillsAsync(Guid tenantId, CancellationToken ct);

    Task<EmergencyDrillFindingDto> CreateEmergencyDrillFindingAsync(
        CreateEmergencyDrillFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<EmergencyDrillFindingDto>> ListEmergencyDrillFindingsAsync(Guid drillId, Guid tenantId, CancellationToken ct);
}