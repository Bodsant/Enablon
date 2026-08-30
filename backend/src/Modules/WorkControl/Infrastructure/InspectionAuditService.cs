using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.WorkControl.Contracts;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.WorkControl.Infrastructure;

/// <summary>
/// Inspection &amp; Audit backend (Trello Sprint 15): audit programs, audits and findings;
/// inspections and findings. Tenant-scoped; audits, inspections and findings are backed
/// by platform records via contract.
/// </summary>
public sealed class InspectionAuditService : IInspectionAuditService
{
    private readonly WorkControlDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IRecordAppService _records;

    public InspectionAuditService(
        WorkControlDbContext db,
        ITenantContext tenant,
        IRecordAppService records)
    {
        _db = db;
        _tenant = tenant;
        _records = records;
    }

    // ---- Audit programs ----------------------------------------------------

    public async Task<Guid> CreateAuditProgramAsync(CreateAuditProgramRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Audit program name is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "AUDT",
            recordType: "Program",
            title: $"Audit program: {request.Name}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            ct);

        var program = new AuditProgramEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            Name = request.Name.Trim(),
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            OwnerMemberId = request.OwnerMemberId ?? createdByMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.AuditPrograms.Add(program);
        await _db.SaveChangesAsync(ct);
        return program.Id;
    }

    public async Task<IReadOnlyList<AuditProgramDto>> ListAuditProgramsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.AuditPrograms.Where(p => p.TenantId == tenantId).OrderBy(p => p.Name).ToListAsync(ct);
        return items.Select(p => new AuditProgramDto(
            p.Id, p.Name, p.Status, p.OwnerMemberId, p.PeriodStart, p.PeriodEnd)).ToList();
    }

    // ---- Audits ------------------------------------------------------------

    public async Task<AuditDto> CreateAuditAsync(
        CreateAuditRequest request, Guid tenantId, Guid leadAuditorMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ScopeText))
            throw new ArgumentException("Audit scope is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "AUDT",
            recordType: "Audit",
            title: $"Audit: {request.AuditType}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: leadAuditorMemberId,
            ct);

        var audit = new AuditEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            AuditProgramId = request.AuditProgramId,
            ChecklistTemplateId = null,
            AuditType = string.IsNullOrWhiteSpace(request.AuditType) ? "Internal" : request.AuditType.Trim(),
            ScopeText = request.ScopeText.Trim(),
            CriteriaText = request.CriteriaText,
            LeadAuditorMemberId = request.LeadAuditorMemberId is null || request.LeadAuditorMemberId == Guid.Empty
                ? leadAuditorMemberId : request.LeadAuditorMemberId.Value,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
        };

        _db.Audits.Add(audit);
        await _db.SaveChangesAsync(ct);

        return new AuditDto(
            audit.Id, record.RecordNumber, audit.AuditProgramId, audit.AuditType, audit.ScopeText,
            audit.CriteriaText, audit.LeadAuditorMemberId, audit.ScheduledStart, audit.ScheduledEnd);
    }

    public async Task<IReadOnlyList<AuditDto>> ListAuditsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Audits.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.ScheduledStart).ToListAsync(ct);
        return items.Select(a => new AuditDto(
            a.Id, a.RecordId.ToString("N")[..8].ToUpperInvariant(), a.AuditProgramId, a.AuditType, a.ScopeText,
            a.CriteriaText, a.LeadAuditorMemberId, a.ScheduledStart, a.ScheduledEnd)).ToList();
    }

    public async Task<Guid> CreateAuditFindingAsync(
        CreateAuditFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureAuditInTenantAsync(tenantId, request.AuditId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "AUDT",
            recordType: "Finding",
            title: $"Audit finding: {request.Classification}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            ct);

        var finding = new AuditFindingEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            AuditId = request.AuditId,
            AuditResponseId = null,
            Classification = string.IsNullOrWhiteSpace(request.Classification) ? "Observation" : request.Classification.Trim(),
            RequirementReference = request.RequirementReference,
            Description = request.Description.Trim(),
            Recommendation = request.Recommendation,
            OwnerMemberId = request.OwnerMemberId,
        };

        _db.AuditFindings.Add(finding);
        await _db.SaveChangesAsync(ct);
        return finding.Id;
    }

    public async Task<IReadOnlyList<AuditFindingDto>> ListAuditFindingsAsync(Guid? auditId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.AuditFindings.Where(f => f.TenantId == tenantId);
        if (auditId is not null && auditId != Guid.Empty)
            query = query.Where(f => f.AuditId == auditId.Value);

        var items = await query.OrderBy(f => f.Classification).ToListAsync(ct);
        return items.Select(f => new AuditFindingDto(
            f.Id, f.RecordId.ToString("N")[..8].ToUpperInvariant(), f.AuditId, f.Classification,
            f.RequirementReference, f.Description, f.Recommendation, f.OwnerMemberId)).ToList();
    }

    // ---- Inspections -------------------------------------------------------

    public async Task<InspectionDto> CreateInspectionAsync(
        CreateInspectionRequest request, Guid tenantId, Guid inspectorMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ScopeText))
            throw new ArgumentException("Inspection scope is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "INSP",
            recordType: "Inspection",
            title: $"Inspection: {request.ScopeText}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: inspectorMemberId,
            ct);

        var now = DateTimeOffset.UtcNow;
        var inspection = new InspectionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            ScheduleId = null,
            InspectorMemberId = request.InspectorMemberId is null || request.InspectorMemberId == Guid.Empty
                ? inspectorMemberId : request.InspectorMemberId.Value,
            PlannedAt = request.PlannedAt ?? now,
            StartedAt = now,
            CompliancePercentage = null,
        };

        _db.Inspections.Add(inspection);
        await _db.SaveChangesAsync(ct);

        return new InspectionDto(
            inspection.Id, record.RecordNumber, null, null, inspection.InspectorMemberId,
            inspection.PlannedAt, inspection.StartedAt, inspection.CompletedAt, inspection.CompliancePercentage);
    }

    public async Task<IReadOnlyList<InspectionDto>> ListInspectionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Inspections.Where(i => i.TenantId == tenantId).OrderByDescending(i => i.StartedAt).ToListAsync(ct);
        return items.Select(i => new InspectionDto(
            i.Id, i.RecordId.ToString("N")[..8].ToUpperInvariant(), i.ScheduleId, i.TemplateVersionId,
            i.InspectorMemberId, i.PlannedAt, i.StartedAt, i.CompletedAt, i.CompliancePercentage)).ToList();
    }

    public async Task<Guid> CreateInspectionFindingAsync(
        CreateInspectionFindingRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureInspectionInTenantAsync(tenantId, request.InspectionId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "INSP",
            recordType: "Finding",
            title: $"Inspection finding: {request.Description}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            ct);

        var finding = new InspectionFindingEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            InspectionId = request.InspectionId,
            ResponseId = null,
            Classification = request.Classification ?? "Observation",
            SeverityId = request.SeverityId,
            Description = request.Description.Trim(),
            OwnerMemberId = request.OwnerMemberId,
        };

        _db.InspectionFindings.Add(finding);
        await _db.SaveChangesAsync(ct);
        return finding.Id;
    }

    public async Task<IReadOnlyList<InspectionFindingDto>> ListInspectionFindingsAsync(Guid? inspectionId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.InspectionFindings.Where(f => f.TenantId == tenantId);
        if (inspectionId is not null && inspectionId != Guid.Empty)
            query = query.Where(f => f.InspectionId == inspectionId.Value);

        var items = await query.OrderBy(f => f.Classification).ToListAsync(ct);
        return items.Select(f => new InspectionFindingDto(
            f.Id, f.RecordId.ToString("N")[..8].ToUpperInvariant(), f.InspectionId, f.Classification,
            f.SeverityId, f.Description, f.OwnerMemberId)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureAuditInTenantAsync(Guid tenantId, Guid auditId, CancellationToken ct)
    {
        var exists = await _db.Audits.AnyAsync(a => a.TenantId == tenantId && a.Id == auditId, ct);
        if (!exists)
            throw new KeyNotFoundException("Audit not found in this tenant.");
    }

    private async Task EnsureInspectionInTenantAsync(Guid tenantId, Guid inspectionId, CancellationToken ct)
    {
        var exists = await _db.Inspections.AnyAsync(i => i.TenantId == tenantId && i.Id == inspectionId, ct);
        if (!exists)
            throw new KeyNotFoundException("Inspection not found in this tenant.");
    }
}
