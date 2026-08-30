using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.SafetyRisk.Contracts;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.SafetyRisk.Infrastructure;

/// <summary>
/// Hazard &amp; risk backend (Trello Sprint 11): hazard catalog, risk register,
/// risk assessments with scoring, and risk controls. Tenant-scoped; register
/// entries are backed by a platform record via contract.
/// </summary>
public sealed class HazardRiskService : IHazardRiskService
{
    private readonly SafetyRiskDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IRecordAppService _records;

    public HazardRiskService(
        SafetyRiskDbContext db,
        ITenantContext tenant,
        IRecordAppService records)
    {
        _db = db;
        _tenant = tenant;
        _records = records;
    }

    // ---- Hazards -----------------------------------------------------------

    public async Task<Guid> CreateHazardAsync(
        CreateHazardRequest request,
        Guid tenantId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Hazard code is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Hazard name is required.", nameof(request));

        var hazard = new HazardEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            Description = request.Description,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim(),
        };

        _db.Hazards.Add(hazard);
        await _db.SaveChangesAsync(ct);
        return hazard.Id;
    }

    public async Task<IReadOnlyList<HazardDto>> ListHazardsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Hazards
            .Where(h => h.TenantId == tenantId)
            .OrderBy(h => h.Code)
            .ToListAsync(ct);

        return items.Select(h => new HazardDto(
            h.Id, h.Code, h.Name, h.CategoryId, h.Description, h.Status)).ToList();
    }

    // ---- Risk register -----------------------------------------------------

    public async Task<Guid> CreateRegisterAsync(
        CreateRiskRegisterRequest request,
        Guid tenantId,
        Guid createdByMemberId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ActivityName))
            throw new ArgumentException("Activity name is required.", nameof(request));

        await EnsureHazardInTenantAsync(tenantId, request.HazardId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "RISK",
            recordType: "RiskRegister",
            title: $"Risk register: {request.ActivityName}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            ct);

        var register = new RiskRegisterEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            HazardId = request.HazardId,
            ActivityName = request.ActivityName.Trim(),
            RiskEvent = request.RiskEvent.Trim(),
            OwnerMemberId = request.OwnerMemberId is null || request.OwnerMemberId == Guid.Empty
                ? createdByMemberId
                : request.OwnerMemberId.Value,
            ReviewDate = request.ReviewDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.Registers.Add(register);
        await _db.SaveChangesAsync(ct);
        return register.Id;
    }

    public async Task<IReadOnlyList<RiskRegisterDto>> ListRegistersAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Registers
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.ReviewDate)
            .ThenBy(r => r.ActivityName)
            .ToListAsync(ct);

        return items.Select(r => new RiskRegisterDto(
            r.Id, r.HazardId, r.ActivityName, r.RiskEvent, r.OwnerMemberId, r.ReviewDate, r.Status)).ToList();
    }

    // ---- Risk matrix -------------------------------------------------------

    public async Task<Guid> CreateMatrixVersionAsync(
        CreateRiskMatrixVersionRequest request,
        Guid tenantId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Matrix version name is required.", nameof(request));

        var version = new RiskMatrixVersionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            VersionNumber = request.VersionNumber,
            LikelihoodScale = request.LikelihoodScale,
            SeverityScale = request.SeverityScale,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.MatrixVersions.Add(version);
        await _db.SaveChangesAsync(ct);
        return version.Id;
    }

    public async Task<IReadOnlyList<RiskMatrixVersionDto>> ListMatrixVersionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.MatrixVersions
            .Where(v => v.TenantId == tenantId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);

        return items.Select(v => new RiskMatrixVersionDto(
            v.Id, v.Name, v.VersionNumber, v.LikelihoodScale, v.SeverityScale,
            v.EffectiveFrom, v.EffectiveTo, v.Status)).ToList();
    }

    public async Task<Guid> CreateMatrixCellAsync(
        CreateRiskMatrixCellRequest request,
        Guid tenantId,
        CancellationToken ct)
    {
        await EnsureMatrixVersionInTenantAsync(tenantId, request.MatrixVersionId, ct);

        var cell = new RiskMatrixCellEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MatrixVersionId = request.MatrixVersionId,
            LikelihoodValue = request.LikelihoodValue,
            SeverityValue = request.SeverityValue,
            RiskScore = request.RiskScore,
            RiskLevelCode = string.IsNullOrWhiteSpace(request.RiskLevelCode) ? "Low" : request.RiskLevelCode.Trim(),
        };

        _db.MatrixCells.Add(cell);
        await _db.SaveChangesAsync(ct);
        return cell.Id;
    }

    public async Task<IReadOnlyList<RiskMatrixCellDto>> ListMatrixCellsAsync(Guid matrixVersionId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.MatrixCells.Where(c => c.TenantId == tenantId);
        if (matrixVersionId != Guid.Empty)
            query = query.Where(c => c.MatrixVersionId == matrixVersionId);

        var items = await query.OrderBy(c => c.LikelihoodValue).ThenBy(c => c.SeverityValue).ToListAsync(ct);

        return items.Select(c => new RiskMatrixCellDto(
            c.Id, c.MatrixVersionId, c.LikelihoodValue, c.SeverityValue, c.RiskScore, c.RiskLevelCode)).ToList();
    }

    // ---- Risk assessments --------------------------------------------------

    public async Task<Guid> CreateAssessmentAsync(
        CreateRiskAssessmentRequest request,
        Guid tenantId,
        CancellationToken ct)
    {
        await EnsureRegisterInTenantAsync(tenantId, request.RiskRegisterId, ct);

        var sequence = await _db.Assessments
            .CountAsync(a => a.TenantId == tenantId && a.RiskRegisterId == request.RiskRegisterId, ct) + 1;

        var riskScore = request.LikelihoodValue * request.SeverityValue;
        var levelCode = ClassifyRisk(riskScore);

        var assessment = new RiskAssessmentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RiskRegisterId = request.RiskRegisterId,
            MatrixVersionId = request.MatrixVersionId,
            AssessmentType = string.IsNullOrWhiteSpace(request.AssessmentType) ? "Initial" : request.AssessmentType.Trim(),
            SequenceNumber = sequence,
            LikelihoodValue = request.LikelihoodValue,
            SeverityValue = request.SeverityValue,
            RiskScore = riskScore,
            RiskLevelCode = levelCode,
            AssessedByMemberId = request.AssessedByMemberId == Guid.Empty
                ? throw new ArgumentException("AssessedByMemberId is required.", nameof(request))
                : request.AssessedByMemberId,
            AssessedAt = DateTimeOffset.UtcNow,
        };

        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync(ct);
        return assessment.Id;
    }

    public async Task<IReadOnlyList<RiskAssessmentDto>> ListAssessmentsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Assessments
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.AssessedAt)
            .ToListAsync(ct);

        return items.Select(a => new RiskAssessmentDto(
            a.Id, a.RiskRegisterId, a.MatrixVersionId, a.AssessmentType, a.SequenceNumber,
            a.LikelihoodValue, a.SeverityValue, a.RiskScore, a.RiskLevelCode, a.AssessedByMemberId, a.AssessedAt)).ToList();
    }

    // ---- Risk controls -----------------------------------------------------

    public async Task<Guid> CreateControlAsync(
        CreateRiskControlRequest request,
        Guid tenantId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Control description is required.", nameof(request));

        await EnsureRegisterInTenantAsync(tenantId, request.RiskRegisterId, ct);

        var control = new RiskControlEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RiskRegisterId = request.RiskRegisterId,
            ControlType = string.IsNullOrWhiteSpace(request.ControlType) ? "Engineering" : request.ControlType.Trim(),
            ControlStage = string.IsNullOrWhiteSpace(request.ControlStage) ? "Prevention" : request.ControlStage.Trim(),
            Description = request.Description.Trim(),
            OwnerMemberId = request.OwnerMemberId,
            DueDate = request.DueDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Planned" : request.Status.Trim(),
        };

        _db.Controls.Add(control);
        await _db.SaveChangesAsync(ct);
        return control.Id;
    }

    public async Task<IReadOnlyList<RiskControlDto>> ListControlsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Controls
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DueDate)
            .ThenBy(c => c.ControlType)
            .ToListAsync(ct);

        return items.Select(c => new RiskControlDto(
            c.Id, c.RiskRegisterId, c.ControlType, c.ControlStage, c.Description,
            c.OwnerMemberId, c.DueDate, c.Status, c.EffectivenessRating)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureHazardInTenantAsync(Guid tenantId, Guid hazardId, CancellationToken ct)
    {
        var exists = await _db.Hazards.AnyAsync(h => h.TenantId == tenantId && h.Id == hazardId, ct);
        if (!exists)
            throw new KeyNotFoundException("Hazard not found in this tenant.");
    }

    private async Task EnsureRegisterInTenantAsync(Guid tenantId, Guid registerId, CancellationToken ct)
    {
        var exists = await _db.Registers.AnyAsync(r => r.TenantId == tenantId && r.Id == registerId, ct);
        if (!exists)
            throw new KeyNotFoundException("Risk register not found in this tenant.");
    }

    private async Task EnsureMatrixVersionInTenantAsync(Guid tenantId, Guid matrixVersionId, CancellationToken ct)
    {
        var exists = await _db.MatrixVersions.AnyAsync(v => v.TenantId == tenantId && v.Id == matrixVersionId, ct);
        if (!exists)
            throw new KeyNotFoundException("Risk matrix version not found in this tenant.");
    }

    private static string ClassifyRisk(int score)
    {
        return score switch
        {
            <= 4 => "Low",
            <= 9 => "Medium",
            <= 15 => "High",
            _ => "Extreme",
        };
    }
}