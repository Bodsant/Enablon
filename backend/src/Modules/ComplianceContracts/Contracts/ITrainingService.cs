namespace Ehsms.Modules.ComplianceContracts.Contracts;

/// <summary>Payload to create a training course.</summary>
public sealed record CreateCourseRequest(
    string Code,
    string Name,
    int? ValidityMonths,
    string? ProviderType,
    string Status);

/// <summary>Payload to schedule a training session against a course.</summary>
public sealed record CreateTrainingSessionRequest(
    Guid CourseId,
    string? ProviderName,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    int? Capacity,
    string Status);

/// <summary>Payload to enrol a person in a training session.</summary>
public sealed record AddSessionParticipantRequest(
    Guid TrainingSessionId,
    Guid PersonId,
    string? AttendanceStatus,
    decimal? AssessmentScore,
    string? Result);

/// <summary>Payload to define a competency.</summary>
public sealed record CreateCompetencyRequest(
    string Code,
    string Name,
    string? Description,
    string Status);

/// <summary>Payload to assign a competency to a person.</summary>
public sealed record AssignWorkerCompetencyRequest(
    Guid PersonId,
    Guid CompetencyId,
    string? Level,
    string Status,
    DateOnly? ValidFrom,
    DateOnly? ValidUntil);

public sealed record CourseDto(
    Guid Id,
    string Code,
    string Name,
    int? ValidityMonths,
    string? ProviderType,
    string Status);

public sealed record TrainingSessionDto(
    Guid Id,
    string RecordNumber,
    Guid CourseId,
    string? ProviderName,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    int? Capacity,
    string Status);

public sealed record SessionParticipantDto(
    Guid Id,
    Guid TrainingSessionId,
    Guid PersonId,
    string? AttendanceStatus,
    decimal? AssessmentScore,
    string? Result);

public sealed record CompetencyDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Status);

public sealed record WorkerCompetencyDto(
    Guid Id,
    Guid PersonId,
    Guid CompetencyId,
    string? Level,
    string Status,
    DateOnly? ValidFrom,
    DateOnly? ValidUntil);

/// <summary>Training &amp; competency backend service (Trello Sprint 20 R2).</summary>
public interface ITrainingService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<CourseDto>> ListCoursesAsync(Guid tenantId, CancellationToken ct);

    Task<TrainingSessionDto> CreateTrainingSessionAsync(
        CreateTrainingSessionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<TrainingSessionDto>> ListTrainingSessionsAsync(Guid tenantId, CancellationToken ct);

    Task<Guid> AddSessionParticipantAsync(AddSessionParticipantRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<SessionParticipantDto>> ListSessionParticipantsAsync(Guid sessionId, Guid tenantId, CancellationToken ct);

    Task<CompetencyDto> CreateCompetencyAsync(CreateCompetencyRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<CompetencyDto>> ListCompetenciesAsync(Guid tenantId, CancellationToken ct);

    Task<WorkerCompetencyDto> AssignWorkerCompetencyAsync(
        AssignWorkerCompetencyRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<WorkerCompetencyDto>> ListWorkerCompetenciesAsync(Guid personId, Guid tenantId, CancellationToken ct);
}
