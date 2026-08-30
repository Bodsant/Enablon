namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to register an exposure control for a chemical product.</summary>
public sealed record CreateExposureControlRequest(
    Guid ChemicalProductId,
    string ControlType,
    string Description,
    Guid? SourceRecordId = null);

/// <summary>An exposure control summary for a chemical product.</summary>
public sealed record ExposureControlSummary(
    Guid Id,
    Guid ChemicalProductId,
    string ControlType,
    string Description,
    Guid? SourceRecordId);

/// <summary>
/// Cross-module contract for chemical exposure control records in the
/// HealthSafety module. Tenant-scoped, validated against existing chemical
/// products.
/// </summary>
public interface IChemicalExposureControlService
{
    Task<ExposureControlSummary> AddAsync(
        CreateExposureControlRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExposureControlSummary>> ListAsync(
        Guid chemicalProductId,
        CancellationToken cancellationToken = default);
}