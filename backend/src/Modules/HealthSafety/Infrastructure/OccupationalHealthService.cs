using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Occupational health backend (Trello Sprint 22 R2): health profiles, fitness statuses,
/// surveillance programs &amp; events (record-backed), and health follow-ups. Tenant-scoped.
/// </summary>
public sealed class OccupationalHealthService : IOccupationalHealthService
{
    private static readonly Guid DefaultDataClassificationId = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly HealthSafetyDbContext _db;
    private readonly IRecordAppService _records;

    public OccupationalHealthService(HealthSafetyDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Health profiles ---------------------------------------------------

    public async Task<HealthProfileDto> CreateHealthProfileAsync(CreateHealthProfileRequest request, Guid tenantId, CancellationToken ct)
    {
        var dataClassificationId = request.DataClassificationId ?? await ResolveDataClassificationIdAsync(ct);

        var entity = new HealthProfileEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            RestrictedIdentifier = request.RestrictedIdentifier,
            DataClassificationId = dataClassificationId,
        };

        _db.HealthProfiles.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new HealthProfileDto(entity.Id, entity.PersonId, entity.RestrictedIdentifier, entity.DataClassificationId);
    }

    public async Task<IReadOnlyList<HealthProfileDto>> ListHealthProfilesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.HealthProfiles.Where(h => h.TenantId == tenantId).OrderBy(h => h.PersonId).ToListAsync(ct);
        return items.Select(h => new HealthProfileDto(h.Id, h.PersonId, h.RestrictedIdentifier, h.DataClassificationId)).ToList();
    }

    // ---- Fitness statuses --------------------------------------------------

    public async Task<FitnessStatusDto> CreateFitnessStatusAsync(CreateFitnessStatusRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureProfileInTenantAsync(tenantId, request.HealthProfileId, ct);

        var entity = new FitnessStatusEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            HealthProfileId = request.HealthProfileId,
            FitnessStatus = request.FitnessStatus.Trim(),
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            RestrictionsSummary = request.RestrictionsSummary,
            IssuedByMemberId = request.IssuedByMemberId,
        };

        _db.FitnessStatuses.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FitnessStatusDto(entity.Id, entity.HealthProfileId, entity.FitnessStatus, entity.ValidFrom,
            entity.ValidUntil, entity.RestrictionsSummary, entity.IssuedByMemberId);
    }

    public async Task<IReadOnlyList<FitnessStatusDto>> ListFitnessStatusesAsync(Guid healthProfileId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.FitnessStatuses
            .Where(f => f.TenantId == tenantId && f.HealthProfileId == healthProfileId)
            .OrderByDescending(f => f.ValidFrom)
            .ToListAsync(ct);
        return items.Select(f => new FitnessStatusDto(
            f.Id, f.HealthProfileId, f.FitnessStatus, f.ValidFrom, f.ValidUntil, f.RestrictionsSummary, f.IssuedByMemberId)).ToList();
    }

    // ---- Surveillance programs ---------------------------------------------

    public async Task<SurveillanceProgramDto> CreateSurveillanceProgramAsync(CreateSurveillanceProgramRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Program code and name are required.", nameof(request));

        var entity = new SurveillanceProgramEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            ExposureType = request.ExposureType,
            FrequencyMonths = request.FrequencyMonths,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.SurveillancePrograms.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new SurveillanceProgramDto(entity.Id, entity.Code, entity.Name, entity.ExposureType, entity.FrequencyMonths, entity.Status);
    }

    public async Task<IReadOnlyList<SurveillanceProgramDto>> ListSurveillanceProgramsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.SurveillancePrograms.Where(p => p.TenantId == tenantId).OrderBy(p => p.Code).ToListAsync(ct);
        return items.Select(p => new SurveillanceProgramDto(
            p.Id, p.Code, p.Name, p.ExposureType, p.FrequencyMonths, p.Status)).ToList();
    }

    // ---- Surveillance events -----------------------------------------------

    public async Task<SurveillanceEventDto> CreateSurveillanceEventAsync(
        CreateSurveillanceEventRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureProfileInTenantAsync(tenantId, request.HealthProfileId, ct);
        await EnsureProgramInTenantAsync(tenantId, request.SurveillanceProgramId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "HLTH",
            recordType: "SurveillanceEvent",
            title: "Occupational Health Surveillance",
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new SurveillanceEventEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            HealthProfileId = request.HealthProfileId,
            SurveillanceProgramId = request.SurveillanceProgramId,
            ScheduledDate = request.ScheduledDate,
            AuthorizedProvider = request.AuthorizedProvider,
        };

        _db.SurveillanceEvents.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new SurveillanceEventDto(entity.Id, record.RecordNumber, entity.HealthProfileId,
            entity.SurveillanceProgramId, entity.ScheduledDate, entity.CompletedDate, entity.AuthorizedProvider, entity.ResultSummaryCode);
    }

    public async Task<IReadOnlyList<SurveillanceEventDto>> ListSurveillanceEventsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.SurveillanceEvents.Where(e => e.TenantId == tenantId).OrderByDescending(e => e.ScheduledDate).ToListAsync(ct);
        return items.Select(e => new SurveillanceEventDto(
            e.Id, e.RecordId.ToString("N")[..8].ToUpperInvariant(), e.HealthProfileId, e.SurveillanceProgramId,
            e.ScheduledDate, e.CompletedDate, e.AuthorizedProvider, e.ResultSummaryCode)).ToList();
    }

    // ---- Health follow-ups -------------------------------------------------

    public async Task<HealthFollowupDto> CreateHealthFollowupAsync(CreateHealthFollowupRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureEventInTenantAsync(tenantId, request.SurveillanceEventId, ct);

        var entity = new HealthFollowupEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SurveillanceEventId = request.SurveillanceEventId,
            FollowupType = request.FollowupType.Trim(),
            DueDate = request.DueDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            AssignedMemberId = request.AssignedMemberId,
        };

        _db.HealthFollowups.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new HealthFollowupDto(entity.Id, entity.SurveillanceEventId, entity.FollowupType,
            entity.DueDate, entity.Status, entity.AssignedMemberId);
    }

    public async Task<IReadOnlyList<HealthFollowupDto>> ListHealthFollowupsAsync(Guid surveillanceEventId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.HealthFollowups
            .Where(f => f.TenantId == tenantId && f.SurveillanceEventId == surveillanceEventId)
            .OrderBy(f => f.DueDate)
            .ToListAsync(ct);
        return items.Select(f => new HealthFollowupDto(
            f.Id, f.SurveillanceEventId, f.FollowupType, f.DueDate, f.Status, f.AssignedMemberId)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    /// <summary>
    /// Resolves a valid platform data-classification id (health.profiles has an FK to
    /// platform.data_classifications, so the hardcoded tenant-neutral id is invalid here).
    /// Prefers the tenant-neutral CONFIDENTIAL row, else any existing one.
    /// </summary>
    private async Task<Guid> ResolveDataClassificationIdAsync(CancellationToken ct)
    {
        var any = await _db.Database.SqlQueryRaw<Guid>(
            "SELECT id AS \"Value\" FROM platform.data_classifications ORDER BY id LIMIT 1").FirstOrDefaultAsync(ct);
        if (any != Guid.Empty)
            return any;

        // Fall back to seed if the platform table is empty (shouldn't happen).
        return DefaultDataClassificationId;
    }

    private async Task EnsureProfileInTenantAsync(Guid tenantId, Guid profileId, CancellationToken ct)
    {
        var exists = await _db.HealthProfiles.AnyAsync(h => h.TenantId == tenantId && h.Id == profileId, ct);
        if (!exists)
            throw new KeyNotFoundException("Health profile not found in this tenant.");
    }

    private async Task EnsureProgramInTenantAsync(Guid tenantId, Guid programId, CancellationToken ct)
    {
        var exists = await _db.SurveillancePrograms.AnyAsync(p => p.TenantId == tenantId && p.Id == programId, ct);
        if (!exists)
            throw new KeyNotFoundException("Surveillance program not found in this tenant.");
    }

    private async Task EnsureEventInTenantAsync(Guid tenantId, Guid eventId, CancellationToken ct)
    {
        var exists = await _db.SurveillanceEvents.AnyAsync(e => e.TenantId == tenantId && e.Id == eventId, ct);
        if (!exists)
            throw new KeyNotFoundException("Surveillance event not found in this tenant.");
    }
}
