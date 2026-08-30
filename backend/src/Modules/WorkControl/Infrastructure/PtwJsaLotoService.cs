using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.WorkControl.Contracts;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.WorkControl.Infrastructure;

/// <summary>
/// PTW / JSA / LOTO backend (Trello Sprint 17): work requests, job safety analyses
/// with steps, permits to work with approvals and gas tests, and LOTO isolation plans.
/// All tenant-scoped; records backed by the platform record service.
/// </summary>
public sealed class PtwJsaLotoService : IPtwJsaLotoService
{
    private readonly WorkControlDbContext _db;
    private readonly IRecordAppService _records;

    public PtwJsaLotoService(WorkControlDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Work requests -----------------------------------------------------

    public async Task<WorkRequestDto> CreateWorkRequestAsync(
        CreateWorkRequestRequest request, Guid tenantId, Guid requesterMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.WorkDescription))
            throw new ArgumentException("Work description is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "WRK",
            recordType: "WorkRequest",
            title: request.WorkDescription.Trim(),
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: requesterMemberId,
            ct);

        var entity = new WorkRequestEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            RequesterMemberId = requesterMemberId,
            WorkDescription = request.WorkDescription.Trim(),
            ContractorCompanyId = request.ContractorCompanyId,
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            WorkType = string.IsNullOrWhiteSpace(request.WorkType) ? "Maintenance" : request.WorkType.Trim(),
        };

        _db.WorkRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new WorkRequestDto(entity.Id, record.RecordNumber, entity.WorkDescription, entity.WorkType,
            entity.ContractorCompanyId, entity.PlannedStart, entity.PlannedEnd);
    }

    public async Task<IReadOnlyList<WorkRequestDto>> ListWorkRequestsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.WorkRequests.Where(w => w.TenantId == tenantId).OrderByDescending(w => w.PlannedStart).ToListAsync(ct);
        return items.Select(w => new WorkRequestDto(
            w.Id, w.RecordId.ToString("N")[..8].ToUpperInvariant(), w.WorkDescription, w.WorkType,
            w.ContractorCompanyId, w.PlannedStart, w.PlannedEnd)).ToList();
    }

    // ---- JSA ---------------------------------------------------------------

    public async Task<JsaDto> CreateJsaAsync(
        CreateJsaRequest request, Guid tenantId, Guid preparedByMemberId, CancellationToken ct)
    {
        await EnsureWorkRequestInTenantAsync(tenantId, request.WorkRequestId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "JSA",
            recordType: "Jsa",
            title: "Job Safety Analysis",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: preparedByMemberId,
            ct);

        var entity = new JsaEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            WorkRequestId = request.WorkRequestId,
            TemplateVersionId = request.TemplateVersionId,
            PreparedByMemberId = preparedByMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim(),
            OverallResidualRisk = request.OverallResidualRisk,
        };

        _db.Jsas.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new JsaDto(entity.Id, record.RecordNumber, entity.WorkRequestId, entity.TemplateVersionId,
            entity.PreparedByMemberId, entity.Status, entity.OverallResidualRisk);
    }

    public async Task<Guid> AddJsaStepAsync(CreateJsaStepRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureJsaInTenantAsync(tenantId, request.JsaId, ct);

        var step = new JsaStepEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            JsaId = request.JsaId,
            SequenceNumber = request.SequenceNumber,
            WorkStep = request.WorkStep.Trim(),
        };

        _db.JsaSteps.Add(step);
        await _db.SaveChangesAsync(ct);
        return step.Id;
    }

    public async Task<IReadOnlyList<JsaDto>> ListJsasAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Jsas.Where(j => j.TenantId == tenantId).OrderByDescending(j => j.Id).ToListAsync(ct);
        return items.Select(j => new JsaDto(
            j.Id, j.RecordId.ToString("N")[..8].ToUpperInvariant(), j.WorkRequestId, j.TemplateVersionId,
            j.PreparedByMemberId, j.Status, j.OverallResidualRisk)).ToList();
    }

    // ---- Permits (PTW) -----------------------------------------------------

    public async Task<PermitDto> CreatePermitAsync(
        CreatePermitRequest request, Guid tenantId, Guid requesterMemberId, CancellationToken ct)
    {
        await EnsureWorkRequestInTenantAsync(tenantId, request.WorkRequestId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "PTW",
            recordType: "Permit",
            title: "Permit to Work",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: requesterMemberId,
            ct);

        var entity = new PermitEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            WorkRequestId = request.WorkRequestId,
            JsaId = request.JsaId,
            PermitTypeVersionId = request.PermitTypeVersionId,
            RequesterMemberId = requesterMemberId,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            ExtensionCount = 0,
        };

        _db.Permits.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PermitDto(entity.Id, record.RecordNumber, entity.WorkRequestId, entity.JsaId,
            entity.PermitTypeVersionId, entity.RequesterMemberId, entity.ValidFrom, entity.ValidUntil);
    }

    public async Task ApprovePermitAsync(
        ApprovePermitRequest request, Guid tenantId, Guid approverMemberId, CancellationToken ct)
    {
        await EnsurePermitInTenantAsync(tenantId, request.PermitId, ct);

        var approval = new PermitApprovalEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PermitId = request.PermitId,
            WorkflowTaskId = Guid.NewGuid(),
            ApprovalLevel = request.ApprovalLevel,
            Decision = string.IsNullOrWhiteSpace(request.Decision) ? "Approved" : request.Decision.Trim(),
            ApproverMemberId = approverMemberId,
            DecidedAt = DateTimeOffset.UtcNow,
        };

        _db.PermitApprovals.Add(approval);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> RecordGasTestAsync(RecordGasTestRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsurePermitInTenantAsync(tenantId, request.PermitId, ct);

        var gasTest = new GasTestEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PermitId = request.PermitId,
            TestType = string.IsNullOrWhiteSpace(request.TestType) ? "PreJob" : request.TestType.Trim(),
            TestedAt = request.TestedAt ?? DateTimeOffset.UtcNow,
            TestedByPersonId = null,
            OxygenPct = request.OxygenPct,
            LelPct = request.LelPct,
            ToxicGasJson = request.ToxicGasJson,
            Result = string.IsNullOrWhiteSpace(request.Result) ? "Pass" : request.Result.Trim(),
        };

        _db.GasTests.Add(gasTest);
        await _db.SaveChangesAsync(ct);
        return gasTest.Id;
    }

    public async Task<IReadOnlyList<PermitDto>> ListPermitsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Permits.Where(p => p.TenantId == tenantId).OrderByDescending(p => p.ValidFrom).ToListAsync(ct);
        return items.Select(p => new PermitDto(
            p.Id, p.RecordId.ToString("N")[..8].ToUpperInvariant(), p.WorkRequestId, p.JsaId,
            p.PermitTypeVersionId, p.RequesterMemberId, p.ValidFrom, p.ValidUntil)).ToList();
    }

    // ---- LOTO --------------------------------------------------------------

    public async Task<IsolationPlanDto> CreateIsolationPlanAsync(
        CreateIsolationPlanRequest request, Guid tenantId, Guid preparedByMemberId, CancellationToken ct)
    {
        await EnsurePermitInTenantAsync(tenantId, request.PermitId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "LOTO",
            recordType: "IsolationPlan",
            title: "LOTO Isolation Plan",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: preparedByMemberId,
            ct);

        var entity = new IsolationPlanEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            PermitId = request.PermitId,
            PreparedByMemberId = preparedByMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim(),
        };

        _db.IsolationPlans.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IsolationPlanDto(entity.Id, record.RecordNumber, entity.PermitId, entity.PreparedByMemberId, entity.Status);
    }

    public async Task<IReadOnlyList<IsolationPlanDto>> ListIsolationPlansAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IsolationPlans.Where(i => i.TenantId == tenantId).OrderByDescending(i => i.Id).ToListAsync(ct);
        return items.Select(i => new IsolationPlanDto(
            i.Id, i.RecordId.ToString("N")[..8].ToUpperInvariant(), i.PermitId, i.PreparedByMemberId, i.Status)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureWorkRequestInTenantAsync(Guid tenantId, Guid workRequestId, CancellationToken ct)
    {
        var exists = await _db.WorkRequests.AnyAsync(w => w.TenantId == tenantId && w.Id == workRequestId, ct);
        if (!exists)
            throw new KeyNotFoundException("Work request not found in this tenant.");
    }

    private async Task EnsureJsaInTenantAsync(Guid tenantId, Guid jsaId, CancellationToken ct)
    {
        var exists = await _db.Jsas.AnyAsync(j => j.TenantId == tenantId && j.Id == jsaId, ct);
        if (!exists)
            throw new KeyNotFoundException("JSA not found in this tenant.");
    }

    private async Task EnsurePermitInTenantAsync(Guid tenantId, Guid permitId, CancellationToken ct)
    {
        var exists = await _db.Permits.AnyAsync(p => p.TenantId == tenantId && p.Id == permitId, ct);
        if (!exists)
            throw new KeyNotFoundException("Permit not found in this tenant.");
    }
}
