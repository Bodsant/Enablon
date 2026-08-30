namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to record a chemical storage inspection.</summary>
public sealed record CreateStorageInspectionRequest(
    Guid ChemicalInventoryId,
    string Result,
    DateTimeOffset? InspectedAt = null,
    DateOnly? NextReviewDate = null);

/// <summary>A chemical storage inspection summary.</summary>
public sealed record StorageInspectionSummary(
    Guid Id,
    string RecordNumber,
    Guid ChemicalInventoryId,
    Guid InspectedByMemberId,
    DateTimeOffset InspectedAt,
    string Result,
    DateOnly? NextReviewDate);

/// <summary>
/// Cross-module contract for chemical storage inspections in the HealthSafety
/// module. Each inspection is backed by a platform record and validated against
/// an existing chemical inventory line in the tenant.
/// </summary>
public interface IChemicalStorageInspectionService
{
    Task<StorageInspectionSummary> CreateAsync(
        CreateStorageInspectionRequest request,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StorageInspectionSummary>> ListAsync(
        Guid? chemicalInventoryId = null,
        CancellationToken cancellationToken = default);
}