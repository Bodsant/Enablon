namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to create a health profile for a person.</summary>
public sealed record CreateHealthProfileRequest(
    Guid PersonId,
    string? RestrictedIdentifier,
    Guid? DataClassificationId);

/// <summary>Payload to record fitness status against a health profile.</summary>
public sealed record CreateFitnessStatusRequest(
    Guid HealthProfileId,
    string FitnessStatus,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    string? RestrictionsSummary,
    Guid? IssuedByMemberId);

/// <summary>Payload to create a surveillance program.</summary>
public sealed record CreateSurveillanceProgramRequest(
    string Code,
    string Name,
    string? ExposureType,
    int? FrequencyMonths,
    string Status);

/// <summary>Payload to schedule a surveillance event for a health profile.</summary>
public sealed record CreateSurveillanceEventRequest(
    Guid HealthProfileId,
    Guid SurveillanceProgramId,
    DateOnly? ScheduledDate,
    string? AuthorizedProvider);

/// <summary>Payload to record a health follow-up against a surveillance event.</summary>
public sealed record CreateHealthFollowupRequest(
    Guid SurveillanceEventId,
    string FollowupType,
    DateOnly? DueDate,
    string Status,
    Guid? AssignedMemberId);

public sealed record HealthProfileDto(
    Guid Id,
    Guid PersonId,
    string? RestrictedIdentifier,
    Guid DataClassificationId);

public sealed record FitnessStatusDto(
    Guid Id,
    Guid HealthProfileId,
    string FitnessStatus,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    string? RestrictionsSummary,
    Guid? IssuedByMemberId);

public sealed record SurveillanceProgramDto(
    Guid Id,
    string Code,
    string Name,
    string? ExposureType,
    int? FrequencyMonths,
    string Status);

public sealed record SurveillanceEventDto(
    Guid Id,
    string RecordNumber,
    Guid HealthProfileId,
    Guid SurveillanceProgramId,
    DateOnly? ScheduledDate,
    DateOnly? CompletedDate,
    string? AuthorizedProvider,
    string? ResultSummaryCode);

public sealed record HealthFollowupDto(
    Guid Id,
    Guid SurveillanceEventId,
    string FollowupType,
    DateOnly? DueDate,
    string Status,
    Guid? AssignedMemberId);

/// <summary>Occupational health backend service (Trello Sprint 22 R2).</summary>
public interface IOccupationalHealthService
{
    Task<HealthProfileDto> CreateHealthProfileAsync(CreateHealthProfileRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<HealthProfileDto>> ListHealthProfilesAsync(Guid tenantId, CancellationToken ct);

    Task<FitnessStatusDto> CreateFitnessStatusAsync(CreateFitnessStatusRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<FitnessStatusDto>> ListFitnessStatusesAsync(Guid healthProfileId, Guid tenantId, CancellationToken ct);

    Task<SurveillanceProgramDto> CreateSurveillanceProgramAsync(CreateSurveillanceProgramRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<SurveillanceProgramDto>> ListSurveillanceProgramsAsync(Guid tenantId, CancellationToken ct);

    Task<SurveillanceEventDto> CreateSurveillanceEventAsync(
        CreateSurveillanceEventRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<SurveillanceEventDto>> ListSurveillanceEventsAsync(Guid tenantId, CancellationToken ct);

    Task<HealthFollowupDto> CreateHealthFollowupAsync(CreateHealthFollowupRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<HealthFollowupDto>> ListHealthFollowupsAsync(Guid surveillanceEventId, Guid tenantId, CancellationToken ct);
}
