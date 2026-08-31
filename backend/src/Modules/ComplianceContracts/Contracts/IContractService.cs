namespace Ehsms.Modules.ComplianceContracts.Contracts;

/// <summary>Payload to register a contractor company.</summary>
public sealed record CreateContractorCompanyRequest(
    string Name,
    string? VendorCode,
    string? TaxIdentifier,
    string? QualificationStatus,
    string? EligibilityStatus,
    string Status);

/// <summary>Payload to create a contract against a contractor company.</summary>
public sealed record CreateContractRequest(
    Guid ContractorCompanyId,
    string? ContractNumber,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ContractStatus,
    string? ProcurementSourceId);

/// <summary>Payload to register a contractor worker.</summary>
public sealed record CreateContractorWorkerRequest(
    Guid ContractorCompanyId,
    Guid PersonId,
    string? WorkerNumber,
    string? PositionName,
    string? EligibilityStatus,
    string Status);

/// <summary>Payload to attach a document to a company and/or worker.</summary>
public sealed record CreateContractorDocumentRequest(
    Guid? ContractorCompanyId,
    Guid? ContractorWorkerId,
    string DocumentType,
    string? DocumentNumber,
    Guid FileObjectId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string? VerificationStatus);

public sealed record ContractorCompanyDto(
    Guid Id,
    string RecordNumber,
    string Name,
    string? VendorCode,
    string? TaxIdentifier,
    string? QualificationStatus,
    string? EligibilityStatus,
    string Status);

public sealed record ContractDto(
    Guid Id,
    Guid ContractorCompanyId,
    string? ContractNumber,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ContractStatus,
    string? ProcurementSourceId);

public sealed record ContractorWorkerDto(
    Guid Id,
    Guid ContractorCompanyId,
    Guid PersonId,
    string? WorkerNumber,
    string? PositionName,
    string? EligibilityStatus,
    string Status);

public sealed record ContractorDocumentDto(
    Guid Id,
    Guid? ContractorCompanyId,
    Guid? ContractorWorkerId,
    string DocumentType,
    string? DocumentNumber,
    Guid FileObjectId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string? VerificationStatus);

/// <summary>Contractor management backend service (Trello Sprint 19 R2 — Contract Management).</summary>
public interface IContractService
{
    Task<ContractorCompanyDto> CreateContractorCompanyAsync(
        CreateContractorCompanyRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<ContractorCompanyDto>> ListContractorCompaniesAsync(Guid tenantId, CancellationToken ct);

    Task<ContractDto> CreateContractAsync(
        CreateContractRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ContractDto>> ListContractsAsync(Guid tenantId, CancellationToken ct);

    Task<ContractorWorkerDto> CreateContractorWorkerAsync(
        CreateContractorWorkerRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ContractorWorkerDto>> ListContractorWorkersAsync(Guid tenantId, CancellationToken ct);

    Task<ContractorDocumentDto> CreateContractorDocumentAsync(
        CreateContractorDocumentRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ContractorDocumentDto>> ListContractorDocumentsAsync(Guid tenantId, CancellationToken ct);
}
