namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to record chemical stock at a location.</summary>
public sealed record AddInventoryRequest(
    Guid ChemicalProductId,
    Guid LocationId,
    decimal? Quantity,
    string? Unit,
    string? StorageCondition,
    DateOnly? ExpiryDate);

/// <summary>An inventory line for a chemical product.</summary>
public sealed record ChemicalInventorySummary(
    Guid Id,
    Guid ChemicalProductId,
    Guid LocationId,
    decimal? Quantity,
    string? Unit,
    string? StorageCondition,
    DateOnly? ExpiryDate);

/// <summary>Payload to record an SDS revision for a chemical product.</summary>
public sealed record RecordSdsRevisionRequest(
    Guid ChemicalProductId,
    string RevisionNumber,
    DateOnly? EffectiveDate,
    Guid FileObjectId,
    string? Language);

/// <summary>An SDS revision summary.</summary>
public sealed record SdsRevisionSummary(
    Guid Id,
    Guid ChemicalProductId,
    string RevisionNumber,
    DateOnly? EffectiveDate,
    string? Language,
    string Status);

/// <summary>
/// Cross-module contract for chemical inventory and SDS (safety data sheet) records
/// in the HealthSafety module. Tenant-scoped, validated against existing chemical
/// products.
/// </summary>
public interface IChemicalInventoryService
{
    Task<ChemicalInventorySummary> AddInventoryAsync(
        AddInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChemicalInventorySummary>> ListInventoryAsync(
        Guid? chemicalProductId = null,
        CancellationToken cancellationToken = default);

    Task<SdsRevisionSummary> RecordSdsRevisionAsync(
        RecordSdsRevisionRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SdsRevisionSummary>> ListSdsRevisionsAsync(
        Guid chemicalProductId,
        CancellationToken cancellationToken = default);
}