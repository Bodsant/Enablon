using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.ComplianceContracts.Contracts;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure;

/// <summary>
/// Contractor &amp; contract management backend (Trello Sprint 19 R2 — Contract Management):
/// contractor companies (record-backed), contracts, contractor workers, and contractor documents.
/// All tenant-scoped; cross-schema FKs (to platform.records / org.people) are NOT modelled as EF
/// relationships, consistent with the module design.
/// </summary>
public sealed class ContractService : IContractService
{
    private static readonly Guid DefaultDataClassificationId = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ComplianceContractsDbContext _db;
    private readonly IRecordAppService _records;

    public ContractService(ComplianceContractsDbContext db, IRecordAppService records)
    {
        _db = db;
        _records = records;
    }

    // ---- Contractor companies ---------------------------------------------

    public async Task<ContractorCompanyDto> CreateContractorCompanyAsync(
        CreateContractorCompanyRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Company name is required.", nameof(request));

        var record = await _records.CreateAsync(
            moduleCode: "CNTR",
            recordType: "ContractorCompany",
            title: request.Name.Trim(),
            dataClassificationId: DefaultDataClassificationId,
            createdByMemberId: createdByMemberId,
            ct);

        var entity = new ContractorCompanyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            VendorCode = request.VendorCode,
            Name = request.Name.Trim(),
            TaxIdentifier = request.TaxIdentifier,
            QualificationStatus = request.QualificationStatus,
            EligibilityStatus = request.EligibilityStatus,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.ContractorCompanies.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ContractorCompanyDto(entity.Id, record.RecordNumber, entity.Name, entity.VendorCode,
            entity.TaxIdentifier, entity.QualificationStatus, entity.EligibilityStatus, entity.Status);
    }

    public async Task<IReadOnlyList<ContractorCompanyDto>> ListContractorCompaniesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ContractorCompanies
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return items.Select(c => new ContractorCompanyDto(
            c.Id, c.RecordId.ToString("N")[..8].ToUpperInvariant(), c.Name, c.VendorCode,
            c.TaxIdentifier, c.QualificationStatus, c.EligibilityStatus, c.Status)).ToList();
    }

    // ---- Contracts ---------------------------------------------------------

    public async Task<ContractDto> CreateContractAsync(
        CreateContractRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureCompanyInTenantAsync(tenantId, request.ContractorCompanyId, ct);

        var entity = new ContractEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractorCompanyId = request.ContractorCompanyId,
            ContractNumber = request.ContractNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ContractStatus = string.IsNullOrWhiteSpace(request.ContractStatus) ? "Active" : request.ContractStatus.Trim(),
            ProcurementSourceId = request.ProcurementSourceId,
        };

        _db.Contracts.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ContractDto(entity.Id, entity.ContractorCompanyId, entity.ContractNumber,
            entity.StartDate, entity.EndDate, entity.ContractStatus, entity.ProcurementSourceId);
    }

    public async Task<IReadOnlyList<ContractDto>> ListContractsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Contracts.Where(c => c.TenantId == tenantId).OrderByDescending(c => c.StartDate).ToListAsync(ct);
        return items.Select(c => new ContractDto(
            c.Id, c.ContractorCompanyId, c.ContractNumber, c.StartDate, c.EndDate, c.ContractStatus, c.ProcurementSourceId)).ToList();
    }

    // ---- Contractor workers ------------------------------------------------

    public async Task<ContractorWorkerDto> CreateContractorWorkerAsync(
        CreateContractorWorkerRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureCompanyInTenantAsync(tenantId, request.ContractorCompanyId, ct);

        var entity = new ContractorWorkerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            ContractorCompanyId = request.ContractorCompanyId,
            WorkerNumber = request.WorkerNumber,
            PositionName = request.PositionName,
            EligibilityStatus = request.EligibilityStatus,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.ContractorWorkers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ContractorWorkerDto(entity.Id, entity.ContractorCompanyId, entity.PersonId,
            entity.WorkerNumber, entity.PositionName, entity.EligibilityStatus, entity.Status);
    }

    public async Task<IReadOnlyList<ContractorWorkerDto>> ListContractorWorkersAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ContractorWorkers.Where(w => w.TenantId == tenantId).OrderBy(w => w.WorkerNumber).ToListAsync(ct);
        return items.Select(w => new ContractorWorkerDto(
            w.Id, w.ContractorCompanyId, w.PersonId, w.WorkerNumber, w.PositionName, w.EligibilityStatus, w.Status)).ToList();
    }

    // ---- Contractor documents ----------------------------------------------

    public async Task<ContractorDocumentDto> CreateContractorDocumentAsync(
        CreateContractorDocumentRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentType))
            throw new ArgumentException("Document type is required.", nameof(request));

        if (request.ContractorCompanyId is not null)
            await EnsureCompanyInTenantAsync(tenantId, request.ContractorCompanyId.Value, ct);

        var entity = new ContractorDocumentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractorCompanyId = request.ContractorCompanyId,
            ContractorWorkerId = request.ContractorWorkerId,
            DocumentType = request.DocumentType.Trim(),
            DocumentNumber = request.DocumentNumber,
            FileObjectId = request.FileObjectId,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            VerificationStatus = request.VerificationStatus,
        };

        _db.ContractorDocuments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ContractorDocumentDto(entity.Id, entity.ContractorCompanyId, entity.ContractorWorkerId,
            entity.DocumentType, entity.DocumentNumber, entity.FileObjectId, entity.IssueDate, entity.ExpiryDate, entity.VerificationStatus);
    }

    public async Task<IReadOnlyList<ContractorDocumentDto>> ListContractorDocumentsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ContractorDocuments.Where(d => d.TenantId == tenantId).OrderBy(d => d.DocumentType).ToListAsync(ct);
        return items.Select(d => new ContractorDocumentDto(
            d.Id, d.ContractorCompanyId, d.ContractorWorkerId, d.DocumentType, d.DocumentNumber,
            d.FileObjectId, d.IssueDate, d.ExpiryDate, d.VerificationStatus)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureCompanyInTenantAsync(Guid tenantId, Guid companyId, CancellationToken ct)
    {
        var exists = await _db.ContractorCompanies.AnyAsync(c => c.TenantId == tenantId && c.Id == companyId, ct);
        if (!exists)
            throw new KeyNotFoundException("Contractor company not found in this tenant.");
    }
}
