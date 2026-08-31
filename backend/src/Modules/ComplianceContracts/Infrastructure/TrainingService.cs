using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.ComplianceContracts.Contracts;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure;

/// <summary>
/// Training &amp; competency management backend (Trello Sprint 20 R2): courses, training
/// sessions (record-backed), session participants, competencies, and worker competencies.
/// All tenant-scoped; PersonId is a cross-schema Guid (no modelled FK).
/// </summary>
public sealed class TrainingService : ITrainingService
{
    private static readonly Guid DefaultDataClassificationId = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ComplianceContractsDbContext _db;
    private readonly IRecordAppService _records;

    public TrainingService(ComplianceContractsDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Courses -----------------------------------------------------------

    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Course code and name are required.", nameof(request));

        var entity = new CourseEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            ValidityMonths = request.ValidityMonths,
            ProviderType = request.ProviderType,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.Courses.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CourseDto(entity.Id, entity.Code, entity.Name, entity.ValidityMonths, entity.ProviderType, entity.Status);
    }

    public async Task<IReadOnlyList<CourseDto>> ListCoursesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Courses.Where(c => c.TenantId == tenantId).OrderBy(c => c.Code).ToListAsync(ct);
        return items.Select(c => new CourseDto(c.Id, c.Code, c.Name, c.ValidityMonths, c.ProviderType, c.Status)).ToList();
    }

    // ---- Training sessions -------------------------------------------------

    public async Task<TrainingSessionDto> CreateTrainingSessionAsync(
        CreateTrainingSessionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureCourseInTenantAsync(tenantId, request.CourseId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "TRN",
            recordType: "TrainingSession",
            title: "Training Session",
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new TrainingSessionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            CourseId = request.CourseId,
            ProviderName = request.ProviderName,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Capacity = request.Capacity,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Scheduled" : request.Status.Trim(),
        };

        _db.TrainingSessions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new TrainingSessionDto(entity.Id, record.RecordNumber, entity.CourseId, entity.ProviderName,
            entity.StartsAt, entity.EndsAt, entity.Capacity, entity.Status);
    }

    public async Task<IReadOnlyList<TrainingSessionDto>> ListTrainingSessionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.TrainingSessions.Where(s => s.TenantId == tenantId).OrderByDescending(s => s.StartsAt).ToListAsync(ct);
        return items.Select(s => new TrainingSessionDto(
            s.Id, s.RecordId.ToString("N")[..8].ToUpperInvariant(), s.CourseId, s.ProviderName,
            s.StartsAt, s.EndsAt, s.Capacity, s.Status)).ToList();
    }

    // ---- Session participants ----------------------------------------------

    public async Task<Guid> AddSessionParticipantAsync(AddSessionParticipantRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureSessionInTenantAsync(tenantId, request.TrainingSessionId, ct);

        var participant = new SessionParticipantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingSessionId = request.TrainingSessionId,
            PersonId = request.PersonId,
            AttendanceStatus = request.AttendanceStatus,
            AssessmentScore = request.AssessmentScore,
            Result = request.Result,
        };

        _db.SessionParticipants.Add(participant);
        await _db.SaveChangesAsync(ct);
        return participant.Id;
    }

    public async Task<IReadOnlyList<SessionParticipantDto>> ListSessionParticipantsAsync(Guid sessionId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.SessionParticipants
            .Where(p => p.TenantId == tenantId && p.TrainingSessionId == sessionId)
            .ToListAsync(ct);
        return items.Select(p => new SessionParticipantDto(
            p.Id, p.TrainingSessionId, p.PersonId, p.AttendanceStatus, p.AssessmentScore, p.Result)).ToList();
    }

    // ---- Competencies ------------------------------------------------------

    public async Task<CompetencyDto> CreateCompetencyAsync(CreateCompetencyRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Competency code and name are required.", nameof(request));

        var entity = new CompetencyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.Competencies.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CompetencyDto(entity.Id, entity.Code, entity.Name, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<CompetencyDto>> ListCompetenciesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Competencies.Where(c => c.TenantId == tenantId).OrderBy(c => c.Code).ToListAsync(ct);
        return items.Select(c => new CompetencyDto(c.Id, c.Code, c.Name, c.Description, c.Status)).ToList();
    }

    // ---- Worker competencies -----------------------------------------------

    public async Task<WorkerCompetencyDto> AssignWorkerCompetencyAsync(
        AssignWorkerCompetencyRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureCompetencyInTenantAsync(tenantId, request.CompetencyId, ct);

        var entity = new WorkerCompetencyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            CompetencyId = request.CompetencyId,
            Level = request.Level,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
        };

        _db.WorkerCompetencies.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new WorkerCompetencyDto(entity.Id, entity.PersonId, entity.CompetencyId, entity.Level,
            entity.Status, entity.ValidFrom, entity.ValidUntil);
    }

    public async Task<IReadOnlyList<WorkerCompetencyDto>> ListWorkerCompetenciesAsync(Guid personId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.WorkerCompetencies
            .Where(w => w.TenantId == tenantId && w.PersonId == personId)
            .OrderByDescending(w => w.ValidFrom)
            .ToListAsync(ct);
        return items.Select(w => new WorkerCompetencyDto(
            w.Id, w.PersonId, w.CompetencyId, w.Level, w.Status, w.ValidFrom, w.ValidUntil)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureCourseInTenantAsync(Guid tenantId, Guid courseId, CancellationToken ct)
    {
        var exists = await _db.Courses.AnyAsync(c => c.TenantId == tenantId && c.Id == courseId, ct);
        if (!exists)
            throw new KeyNotFoundException("Course not found in this tenant.");
    }

    private async Task EnsureSessionInTenantAsync(Guid tenantId, Guid sessionId, CancellationToken ct)
    {
        var exists = await _db.TrainingSessions.AnyAsync(s => s.TenantId == tenantId && s.Id == sessionId, ct);
        if (!exists)
            throw new KeyNotFoundException("Training session not found in this tenant.");
    }

    private async Task EnsureCompetencyInTenantAsync(Guid tenantId, Guid competencyId, CancellationToken ct)
    {
        var exists = await _db.Competencies.AnyAsync(c => c.TenantId == tenantId && c.Id == competencyId, ct);
        if (!exists)
            throw new KeyNotFoundException("Competency not found in this tenant.");
    }
}
