namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to register a chemical product and its controlling platform record.</summary>
public sealed record CreateChemicalProductRequest(
    string ProductName,
    string? ProductCode,
    string? SupplierName,
    string? HazardClassificationJson);

/// <summary>Result of registering a chemical product.</summary>
public sealed record CreateChemicalProductResult(Guid Id, string RecordNumber, string Status);

/// <summary>A chemical product summary for the tenant catalogue.</summary>
public sealed record ChemicalProductSummary(
    Guid Id,
    string RecordNumber,
    string ProductName,
    string? ProductCode,
    string? SupplierName,
    string Status);

/// <summary>
/// Cross-module contract for the chemical product catalogue. Implementations are
/// expected to create a backing <c>platform.records</c> row (via
/// <c>Ehsms.Modules.Platform.Contracts.IRecordAppService</c>) so the ledger stays
/// consistent, then persist the HealthSafety chemical product record.
/// </summary>
public interface IChemicalCatalogService
{
    Task<CreateChemicalProductResult> CreateAsync(
        CreateChemicalProductRequest request,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChemicalProductSummary>> ListAsync(
        CancellationToken cancellationToken = default);
}