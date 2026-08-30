using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.SafetyRisk.Contracts;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.SafetyRisk.Infrastructure;

/// <summary>
/// Incident &amp; CAPA backend (Trello Sprint 13): incident reporting with involved
/// people, investigation with root causes, and CAPA actions with progress + verification.
/// Tenant-scoped; incidents and actions are backed by platform records.
/// </summary>
public sealed class IncidentCapaService : IIncidentCapaService
{
    private readonly SafetyRiskDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IRecordAppService _records;

    public IncidentCapaService(
        SafetyRiskDbContext db,
        ITenantContext tenant,
        IRecordAppService records)
    {
        _db = db;
        _tenant = tenant;
        _records = records;
    }

    // ---- Incidents ---------------------------------------------------------

    public async Task<IncidentDto> CreateIncidentAsync(
        CreateIncidentRequest request,
        Guid tenantId,
        Guid reportedByMemberId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Incident description is required.", nameof(request));

        var now = DateTimeOffset.UtcNow;
        var record = await _records.CreateAsync(
            moduleCode: "INCD",
            recordType: "Incident",
            title: $"Incident: {(request.ClassificationStatus ?? "reported")}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: reportedByMemberId,
            ct);

        var incident = new IncidentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            IncidentTypeId = request.IncidentTypeId,
            SeverityId = request.SeverityId,
            OccurredAt = request.OccurredAt ?? now,
            ReportedAt = now,
            ReportedByMemberId = reportedByMemberId,
            Description = request.Description.Trim(),
            ImmediateAction = request.ImmediateAction,
            ClassificationStatus = string.IsNullOrWhiteSpace(request.ClassificationStatus) ? "Pending" : request.ClassificationStatus.Trim(),
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(ct);

        return new IncidentDto(
            incident.Id, record.RecordNumber, incident.IncidentTypeId, incident.SeverityId,
            incident.OccurredAt, incident.ReportedAt, incident.ReportedByMemberId,
            incident.Description, incident.ImmediateAction, incident.ClassificationStatus);
    }

    public async Task<IReadOnlyList<IncidentDto>> ListIncidentsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Incidents
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.ReportedAt)
            .ToListAsync(ct);

        return items.Select(i => new IncidentDto(
            i.Id, i.RecordId.ToString("N")[..8].ToUpperInvariant(), i.IncidentTypeId, i.SeverityId,
            i.OccurredAt, i.ReportedAt, i.ReportedByMemberId,
            i.Description, i.ImmediateAction, i.ClassificationStatus)).ToList();
    }

    public async Task<Guid> AddInvolvedPersonAsync(AddInvolvedPersonRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureIncidentInTenantAsync(tenantId, request.IncidentId, ct);

        var person = new InvolvedPersonEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IncidentId = request.IncidentId,
            PersonId = request.PersonId,
            ExternalPersonName = request.ExternalPersonName,
            InvolvementType = string.IsNullOrWhiteSpace(request.InvolvementType) ? "Affected" : request.InvolvementType.Trim(),
            InjuryClassificationId = request.InjuryClassificationId,
            LostWorkDays = request.LostWorkDays,
        };

        _db.InvolvedPeople.Add(person);
        await _db.SaveChangesAsync(ct);
        return person.Id;
    }

    public async Task<IReadOnlyList<InvolvedPersonDto>> ListInvolvedPeopleAsync(Guid? incidentId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.InvolvedPeople.Where(p => p.TenantId == tenantId);
        if (incidentId is not null && incidentId != Guid.Empty)
            query = query.Where(p => p.IncidentId == incidentId.Value);

        var items = await query.OrderBy(p => p.InvolvementType).ToListAsync(ct);
        return items.Select(p => new InvolvedPersonDto(
            p.Id, p.IncidentId, p.PersonId, p.ExternalPersonName, p.InvolvementType,
            p.InjuryClassificationId, p.LostWorkDays)).ToList();
    }

    // ---- Investigations ----------------------------------------------------

    public async Task<Guid> StartInvestigationAsync(
        StartInvestigationRequest request,
        Guid tenantId,
        Guid leadInvestigatorMemberId,
        CancellationToken ct)
    {
        await EnsureIncidentInTenantAsync(tenantId, request.IncidentId, ct);
        var now = DateTimeOffset.UtcNow;

        var inv = new InvestigationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IncidentId = request.IncidentId,
            LeadInvestigatorMemberId = leadInvestigatorMemberId,
            Method = request.Method,
            Summary = request.Summary,
            Status = "InProgress",
            StartedAt = now,
        };

        _db.Investigations.Add(inv);
        await _db.SaveChangesAsync(ct);
        return inv.Id;
    }

    public async Task<IReadOnlyList<InvestigationDto>> ListInvestigationsAsync(Guid? incidentId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.Investigations.Where(i => i.TenantId == tenantId);
        if (incidentId is not null && incidentId != Guid.Empty)
            query = query.Where(i => i.IncidentId == incidentId.Value);

        var items = await query.OrderByDescending(i => i.StartedAt).ToListAsync(ct);
        return items.Select(i => new InvestigationDto(
            i.Id, i.IncidentId, i.LeadInvestigatorMemberId, i.Method, i.Summary, i.Status,
            i.StartedAt, i.CompletedAt)).ToList();
    }

    public async Task<Guid> AddRootCauseAsync(AddRootCauseRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureInvestigationInTenantAsync(tenantId, request.InvestigationId, ct);

        var cause = new RootCauseEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvestigationId = request.InvestigationId,
            CauseType = string.IsNullOrWhiteSpace(request.CauseType) ? "Root" : request.CauseType.Trim(),
            CategoryId = request.CategoryId,
            Description = request.Description.Trim(),
            EvidenceSummary = request.EvidenceSummary,
        };

        _db.RootCauses.Add(cause);
        await _db.SaveChangesAsync(ct);
        return cause.Id;
    }

    public async Task<IReadOnlyList<RootCauseDto>> ListRootCausesAsync(Guid? investigationId, Guid tenantId, CancellationToken ct)
    {
        var query = _db.RootCauses.Where(r => r.TenantId == tenantId);
        if (investigationId is not null && investigationId != Guid.Empty)
            query = query.Where(r => r.InvestigationId == investigationId.Value);

        var items = await query.OrderBy(r => r.CauseType).ToListAsync(ct);
        return items.Select(r => new RootCauseDto(
            r.Id, r.InvestigationId, r.CauseType, r.CategoryId, r.Description, r.EvidenceSummary)).ToList();
    }

    // ---- CAPA actions ------------------------------------------------------

    public async Task<CapaActionDto> CreateActionAsync(
        CreateCapaActionRequest request,
        Guid tenantId,
        Guid createdByMemberId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Action description is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "CAPA",
            recordType: "CapaAction",
            title: $"CAPA action: {request.ActionType}",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            ct);

        var action = new CapaActionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            ActionType = string.IsNullOrWhiteSpace(request.ActionType) ? "Corrective" : request.ActionType.Trim(),
            Description = request.Description.Trim(),
            OwnerMemberId = request.OwnerMemberId is null || request.OwnerMemberId == Guid.Empty
                ? createdByMemberId : request.OwnerMemberId.Value,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
            DueDate = request.DueDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ProgressPercentage = 0,
            VerificationRequired = request.VerificationRequired,
        };

        _db.Actions.Add(action);
        await _db.SaveChangesAsync(ct);

        return new CapaActionDto(
            action.Id, record.RecordNumber, action.ActionType, action.Description, action.OwnerMemberId,
            action.Priority, action.DueDate, action.ProgressPercentage, action.VerificationRequired);
    }

    public async Task<IReadOnlyList<CapaActionDto>> ListActionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Actions
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync(ct);

        return items.Select(a => new CapaActionDto(
            a.Id, a.RecordId.ToString("N")[..8].ToUpperInvariant(), a.ActionType, a.Description,
            a.OwnerMemberId, a.Priority, a.DueDate, a.ProgressPercentage, a.VerificationRequired)).ToList();
    }

    public async Task ProgressActionAsync(
        ProgressCapaActionRequest request,
        Guid tenantId,
        Guid updatedByMemberId,
        CancellationToken ct)
    {
        var action = await GetActionInTenantAsync(tenantId, request.ActionId, ct);

        action.ProgressPercentage = request.ProgressPercentage;
        _db.Updates.Add(new CapaUpdateEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActionId = action.Id,
            ProgressPercentage = request.ProgressPercentage,
            Note = request.Note.Trim(),
            UpdatedByMemberId = updatedByMemberId,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task VerifyActionAsync(
        VerifyCapaActionRequest request,
        Guid tenantId,
        Guid verifierMemberId,
        CancellationToken ct)
    {
        var action = await GetActionInTenantAsync(tenantId, request.ActionId, ct);

        _db.Verifications.Add(new CapaVerificationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActionId = action.Id,
            VerifierMemberId = verifierMemberId,
            Result = string.IsNullOrWhiteSpace(request.Result) ? "Verified" : request.Result.Trim(),
            Comment = request.Comment,
            VerifiedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureIncidentInTenantAsync(Guid tenantId, Guid incidentId, CancellationToken ct)
    {
        var exists = await _db.Incidents.AnyAsync(i => i.TenantId == tenantId && i.Id == incidentId, ct);
        if (!exists)
            throw new KeyNotFoundException("Incident not found in this tenant.");
    }

    private async Task EnsureInvestigationInTenantAsync(Guid tenantId, Guid investigationId, CancellationToken ct)
    {
        var exists = await _db.Investigations.AnyAsync(i => i.TenantId == tenantId && i.Id == investigationId, ct);
        if (!exists)
            throw new KeyNotFoundException("Investigation not found in this tenant.");
    }

    private async Task<CapaActionEntity> GetActionInTenantAsync(Guid tenantId, Guid actionId, CancellationToken ct)
    {
        var action = await _db.Actions.FirstOrDefaultAsync(
            a => a.TenantId == tenantId && a.Id == actionId, ct);
        if (action is null)
            throw new KeyNotFoundException("CAPA action not found in this tenant.");
        return action;
    }
}