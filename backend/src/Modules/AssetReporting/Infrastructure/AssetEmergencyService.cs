using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.AssetReporting.Contracts;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.AssetReporting.Infrastructure;

/// <summary>
/// Asset safety &amp; emergency backend (Trello Sprint 26 R2): safety assets, emergency
/// plans (record-backed), team members, emergency equipment, drills and drill findings.
/// Tenant-scoped; owner/coordinator member ids come from the resolved active member.
/// </summary>
public sealed class AssetEmergencyService : IAssetEmergencyService
{
    private static readonly Guid DefaultDataClassificationId = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly AssetReportingDbContext _db;
    private readonly IRecordAppService _records;

    public AssetEmergencyService(AssetReportingDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Safety assets -----------------------------------------------------

    public async Task<AssetDto> CreateAssetAsync(CreateAssetRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AssetCode) || string.IsNullOrWhiteSpace(request.AssetName))
            throw new ArgumentException("Asset code and name are required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "AST",
            recordType: "Asset",
            title: request.AssetName.Trim(),
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new AssetEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            SourceSystem = request.SourceSystem,
            SourceId = request.SourceId,
            AssetCode = request.AssetCode.Trim().ToUpperInvariant(),
            AssetName = request.AssetName.Trim(),
            AssetType = request.AssetType,
            SiteId = request.SiteId,
            LocationId = request.LocationId,
            IsSafetyCritical = request.IsSafetyCritical,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.Assets.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new AssetDto(entity.Id, record.RecordNumber, entity.AssetCode, entity.AssetName, entity.AssetType,
            entity.SiteId, entity.LocationId, entity.IsSafetyCritical, entity.Status);
    }

    public async Task<IReadOnlyList<AssetDto>> ListAssetsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Assets.Where(a => a.TenantId == tenantId).OrderBy(a => a.AssetCode).ToListAsync(ct);
        return items.Select(a => new AssetDto(
            a.Id, a.RecordId.ToString("N")[..8].ToUpperInvariant(), a.AssetCode, a.AssetName, a.AssetType,
            a.SiteId, a.LocationId, a.IsSafetyCritical, a.Status)).ToList();
    }

    // ---- Emergency plans ---------------------------------------------------

    public async Task<EmergencyPlanDto> CreateEmergencyPlanAsync(
        CreateEmergencyPlanRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Plan code and name are required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "EMG",
            recordType: "EmergencyPlan",
            title: request.Name.Trim(),
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new EmergencyPlanEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            SiteId = request.SiteId,
            OwnerMemberId = request.OwnerMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.EmergencyPlans.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new EmergencyPlanDto(entity.Id, record.RecordNumber, entity.Code, entity.Name,
            entity.SiteId, entity.OwnerMemberId, entity.Status);
    }

    public async Task<IReadOnlyList<EmergencyPlanDto>> ListEmergencyPlansAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.EmergencyPlans.Where(p => p.TenantId == tenantId).OrderBy(p => p.Code).ToListAsync(ct);
        return items.Select(p => new EmergencyPlanDto(
            p.Id, p.RecordId.ToString("N")[..8].ToUpperInvariant(), p.Code, p.Name, p.SiteId, p.OwnerMemberId, p.Status)).ToList();
    }

    // ---- Team members ------------------------------------------------------

    public async Task<EmergencyTeamMemberDto> AddEmergencyTeamMemberAsync(AddEmergencyTeamMemberRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsurePlanInTenantAsync(tenantId, request.EmergencyPlanId, ct);

        var entity = new EmergencyTeamMemberEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmergencyPlanId = request.EmergencyPlanId,
            PersonId = request.PersonId,
            EmergencyRole = request.EmergencyRole.Trim(),
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
        };

        _db.EmergencyTeamMembers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new EmergencyTeamMemberDto(entity.Id, entity.EmergencyPlanId, entity.PersonId,
            entity.EmergencyRole, entity.ValidFrom, entity.ValidTo);
    }

    public async Task<IReadOnlyList<EmergencyTeamMemberDto>> ListEmergencyTeamMembersAsync(Guid planId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.EmergencyTeamMembers
            .Where(m => m.TenantId == tenantId && m.EmergencyPlanId == planId)
            .OrderBy(m => m.EmergencyRole)
            .ToListAsync(ct);
        return items.Select(m => new EmergencyTeamMemberDto(
            m.Id, m.EmergencyPlanId, m.PersonId, m.EmergencyRole, m.ValidFrom, m.ValidTo)).ToList();
    }

    // ---- Emergency equipment -----------------------------------------------

    public async Task<EmergencyEquipmentDto> CreateEmergencyEquipmentAsync(CreateEmergencyEquipmentRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new EmergencyEquipmentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = request.SiteId,
            LocationId = request.LocationId,
            EquipmentType = request.EquipmentType.Trim(),
            AssetId = request.AssetId,
            InspectionDueDate = request.InspectionDueDate,
            MaintenanceDueDate = request.MaintenanceDueDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Operational" : request.Status.Trim(),
        };

        _db.EmergencyEquipment.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new EmergencyEquipmentDto(entity.Id, entity.SiteId, entity.LocationId, entity.EquipmentType,
            entity.AssetId, entity.InspectionDueDate, entity.MaintenanceDueDate, entity.Status);
    }

    public async Task<IReadOnlyList<EmergencyEquipmentDto>> ListEmergencyEquipmentAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.EmergencyEquipment.Where(e => e.TenantId == tenantId).OrderBy(e => e.EquipmentType).ToListAsync(ct);
        return items.Select(e => new EmergencyEquipmentDto(
            e.Id, e.SiteId, e.LocationId, e.EquipmentType, e.AssetId, e.InspectionDueDate, e.MaintenanceDueDate, e.Status)).ToList();
    }

    // ---- Drills ------------------------------------------------------------

    public async Task<EmergencyDrillDto> CreateEmergencyDrillAsync(
        CreateEmergencyDrillRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsurePlanInTenantAsync(tenantId, request.EmergencyPlanId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "EMG",
            recordType: "EmergencyDrill",
            title: request.Scenario.Trim(),
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new EmergencyDrillEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            EmergencyPlanId = request.EmergencyPlanId,
            Scenario = request.Scenario.Trim(),
            ScheduledAt = request.ScheduledAt,
            CoordinatorMemberId = request.CoordinatorMemberId,
        };

        _db.EmergencyDrills.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new EmergencyDrillDto(entity.Id, record.RecordNumber, entity.EmergencyPlanId, entity.Scenario,
            entity.ScheduledAt, entity.ConductedAt, entity.ResultSummary, entity.CoordinatorMemberId);
    }

    public async Task<IReadOnlyList<EmergencyDrillDto>> ListEmergencyDrillsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.EmergencyDrills.Where(d => d.TenantId == tenantId).OrderByDescending(d => d.ScheduledAt).ToListAsync(ct);
        return items.Select(d => new EmergencyDrillDto(
            d.Id, d.RecordId.ToString("N")[..8].ToUpperInvariant(), d.EmergencyPlanId, d.Scenario,
            d.ScheduledAt, d.ConductedAt, d.ResultSummary, d.CoordinatorMemberId)).ToList();
    }

    // ---- Drill findings ----------------------------------------------------

    public async Task<EmergencyDrillFindingDto> CreateEmergencyDrillFindingAsync(
        CreateEmergencyDrillFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureDrillInTenantAsync(tenantId, request.EmergencyDrillId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "EMG",
            recordType: "DrillFinding",
            title: request.Description.Trim(),
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new EmergencyDrillFindingEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            EmergencyDrillId = request.EmergencyDrillId,
            Description = request.Description.Trim(),
            Severity = request.Severity,
            OwnerMemberId = request.OwnerMemberId,
        };

        _db.EmergencyDrillFindings.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new EmergencyDrillFindingDto(entity.Id, record.RecordNumber, entity.EmergencyDrillId,
            entity.Description, entity.Severity, entity.OwnerMemberId);
    }

    public async Task<IReadOnlyList<EmergencyDrillFindingDto>> ListEmergencyDrillFindingsAsync(Guid drillId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.EmergencyDrillFindings
            .Where(f => f.TenantId == tenantId && f.EmergencyDrillId == drillId)
            .OrderBy(f => f.Severity)
            .ToListAsync(ct);
        return items.Select(f => new EmergencyDrillFindingDto(
            f.Id, f.RecordId.ToString("N")[..8].ToUpperInvariant(), f.EmergencyDrillId, f.Description, f.Severity, f.OwnerMemberId)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsurePlanInTenantAsync(Guid tenantId, Guid planId, CancellationToken ct)
    {
        var exists = await _db.EmergencyPlans.AnyAsync(p => p.TenantId == tenantId && p.Id == planId, ct);
        if (!exists)
            throw new KeyNotFoundException("Emergency plan not found in this tenant.");
    }

    private async Task EnsureDrillInTenantAsync(Guid tenantId, Guid drillId, CancellationToken ct)
    {
        var exists = await _db.EmergencyDrills.AnyAsync(d => d.TenantId == tenantId && d.Id == drillId, ct);
        if (!exists)
            throw new KeyNotFoundException("Emergency drill not found in this tenant.");
    }
}