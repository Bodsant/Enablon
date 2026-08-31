using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.ComplianceContracts.Contracts;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure;

/// <summary>
/// Legal &amp; compliance backend (Trello Sprint 25 R2): legal sources and versions,
/// compliance obligations (record-backed) and obligation applicability. Tenant-scoped;
/// owner/assessor member ids come from the resolved active member.
/// </summary>
public sealed class LegalComplianceService : ILegalComplianceService
{
    private static readonly Guid DefaultDataClassificationId = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ComplianceContractsDbContext _db;
    private readonly IRecordAppService _records;

    public LegalComplianceService(ComplianceContractsDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Legal sources -----------------------------------------------------

    public async Task<LegalSourceDto> CreateLegalSourceAsync(CreateLegalSourceRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SourceType) || string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Source type and title are required.", nameof(request));

        var entity = new LegalSourceEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceType = request.SourceType.Trim(),
            Code = request.Code,
            Title = request.Title.Trim(),
            Jurisdiction = request.Jurisdiction,
            Publisher = request.Publisher,
            SourceUrl = request.SourceUrl,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.LegalSources.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LegalSourceDto(entity.Id, entity.SourceType, entity.Code, entity.Title, entity.Jurisdiction,
            entity.Publisher, entity.SourceUrl, entity.Status);
    }

    public async Task<IReadOnlyList<LegalSourceDto>> ListLegalSourcesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.LegalSources.Where(s => s.TenantId == tenantId).OrderBy(s => s.Title).ToListAsync(ct);
        return items.Select(s => new LegalSourceDto(
            s.Id, s.SourceType, s.Code, s.Title, s.Jurisdiction, s.Publisher, s.SourceUrl, s.Status)).ToList();
    }

    // ---- Legal source versions ---------------------------------------------

    public async Task<LegalSourceVersionDto> CreateLegalSourceVersionAsync(CreateLegalSourceVersionRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureLegalSourceInTenantAsync(tenantId, request.LegalSourceId, ct);

        var entity = new LegalSourceVersionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LegalSourceId = request.LegalSourceId,
            VersionLabel = request.VersionLabel.Trim(),
            PublishedDate = request.PublishedDate,
            EffectiveDate = request.EffectiveDate,
            SupersededDate = request.SupersededDate,
            ChangeSummary = request.ChangeSummary,
        };

        _db.LegalSourceVersions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LegalSourceVersionDto(entity.Id, entity.LegalSourceId, entity.VersionLabel,
            entity.PublishedDate, entity.EffectiveDate, entity.SupersededDate, entity.ChangeSummary);
    }

    public async Task<IReadOnlyList<LegalSourceVersionDto>> ListLegalSourceVersionsAsync(Guid legalSourceId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.LegalSourceVersions
            .Where(v => v.TenantId == tenantId && v.LegalSourceId == legalSourceId)
            .OrderByDescending(v => v.EffectiveDate)
            .ToListAsync(ct);
        return items.Select(v => new LegalSourceVersionDto(
            v.Id, v.LegalSourceId, v.VersionLabel, v.PublishedDate, v.EffectiveDate, v.SupersededDate, v.ChangeSummary)).ToList();
    }

    // ---- Obligations -------------------------------------------------------

    public async Task<ObligationDto> CreateObligationAsync(
        CreateObligationRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureVersionInTenantAsync(tenantId, request.LegalSourceVersionId, ct);

        var record = await _records.CreateAsync(
            moduleCode: "COMP",
            recordType: "Obligation",
            title: "Compliance Obligation",
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new ObligationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            LegalSourceVersionId = request.LegalSourceVersionId,
            ClauseReference = request.ClauseReference,
            RequirementText = request.RequirementText.Trim(),
            OwnerMemberId = request.OwnerMemberId,
            Frequency = request.Frequency,
            DueDate = request.DueDate,
            LastReview = request.LastReview,
            NextReview = request.NextReview,
        };

        _db.Obligations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ObligationDto(entity.Id, record.RecordNumber, entity.LegalSourceVersionId, entity.ClauseReference,
            entity.RequirementText, entity.OwnerMemberId, entity.Frequency, entity.DueDate, entity.LastReview, entity.NextReview);
    }

    public async Task<IReadOnlyList<ObligationDto>> ListObligationsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Obligations.Where(o => o.TenantId == tenantId).OrderBy(o => o.NextReview).ToListAsync(ct);
        return items.Select(o => new ObligationDto(
            o.Id, o.RecordId.ToString("N")[..8].ToUpperInvariant(), o.LegalSourceVersionId, o.ClauseReference,
            o.RequirementText, o.OwnerMemberId, o.Frequency, o.DueDate, o.LastReview, o.NextReview)).ToList();
    }

    // ---- Obligation applicability ------------------------------------------

    public async Task<ObligationApplicabilityDto> CreateObligationApplicabilityAsync(CreateObligationApplicabilityRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureObligationInTenantAsync(tenantId, request.ObligationId, ct);

        var entity = new ObligationApplicabilityEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ObligationId = request.ObligationId,
            CompanyId = request.CompanyId,
            BusinessUnitId = request.BusinessUnitId,
            SiteId = request.SiteId,
            ApplicabilityStatus = string.IsNullOrWhiteSpace(request.ApplicabilityStatus) ? "Applicable" : request.ApplicabilityStatus.Trim(),
            Rationale = request.Rationale,
            AssessedByMemberId = request.AssessedByMemberId,
        };

        _db.ObligationApplicabilities.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ObligationApplicabilityDto(entity.Id, entity.ObligationId, entity.CompanyId, entity.BusinessUnitId,
            entity.SiteId, entity.ApplicabilityStatus, entity.Rationale, entity.AssessedByMemberId);
    }

    public async Task<IReadOnlyList<ObligationApplicabilityDto>> ListObligationApplicabilitiesAsync(Guid obligationId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ObligationApplicabilities
            .Where(a => a.TenantId == tenantId && a.ObligationId == obligationId)
            .ToListAsync(ct);
        return items.Select(a => new ObligationApplicabilityDto(
            a.Id, a.ObligationId, a.CompanyId, a.BusinessUnitId, a.SiteId, a.ApplicabilityStatus, a.Rationale, a.AssessedByMemberId)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureLegalSourceInTenantAsync(Guid tenantId, Guid sourceId, CancellationToken ct)
    {
        var exists = await _db.LegalSources.AnyAsync(s => s.TenantId == tenantId && s.Id == sourceId, ct);
        if (!exists)
            throw new KeyNotFoundException("Legal source not found in this tenant.");
    }

    private async Task EnsureVersionInTenantAsync(Guid tenantId, Guid versionId, CancellationToken ct)
    {
        var exists = await _db.LegalSourceVersions.AnyAsync(v => v.TenantId == tenantId && v.Id == versionId, ct);
        if (!exists)
            throw new KeyNotFoundException("Legal source version not found in this tenant.");
    }

    private async Task EnsureObligationInTenantAsync(Guid tenantId, Guid obligationId, CancellationToken ct)
    {
        var exists = await _db.Obligations.AnyAsync(o => o.TenantId == tenantId && o.Id == obligationId, ct);
        if (!exists)
            throw new KeyNotFoundException("Obligation not found in this tenant.");
    }
}