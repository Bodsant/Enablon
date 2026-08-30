namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to create a PPE catalog item.</summary>
public sealed record CreatePpeCatalogRequest(
    string Code,
    string Name,
    string? PpeCategory = null,
    int? InspectionIntervalDays = null,
    int? ReplacementIntervalDays = null);

/// <summary>A PPE catalog item summary.</summary>
public sealed record PpeCatalogSummary(
    Guid Id,
    string Code,
    string Name,
    string? PpeCategory,
    int? InspectionIntervalDays,
    int? ReplacementIntervalDays,
    string Status);

/// <summary>Payload to link a PPE requirement to a catalog item.</summary>
public sealed record CreatePpeRequirementRequest(
    Guid PpeCatalogId,
    bool IsMandatory,
    Guid? SourceRecordId = null,
    Guid? PermitTypeId = null,
    string? Notes = null);

/// <summary>A PPE requirement summary.</summary>
public sealed record PpeRequirementSummary(
    Guid Id,
    Guid PpeCatalogId,
    bool IsMandatory,
    Guid? SourceRecordId,
    Guid? PermitTypeId,
    string? Notes);

/// <summary>
/// Cross-module contract for the PPE catalogue and per-item requirements in the
/// HealthSafety module. Tenant-scoped.
/// </summary>
public interface IPpeCatalogService
{
    Task<PpeCatalogSummary> CreateCatalogAsync(
        CreatePpeCatalogRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeCatalogSummary>> ListCatalogsAsync(
        CancellationToken cancellationToken = default);

    Task<PpeRequirementSummary> CreateRequirementAsync(
        CreatePpeRequirementRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeRequirementSummary>> ListRequirementsAsync(
        Guid? ppeCatalogId = null,
        CancellationToken cancellationToken = default);
}