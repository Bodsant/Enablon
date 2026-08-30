namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to register PPE stock at a site.</summary>
public sealed record RegisterPpeInventoryRequest(
    Guid PpeCatalogId,
    Guid SiteId,
    string? SerialNumber = null,
    int? QuantityOnHand = null,
    string? Condition = null);

/// <summary>A PPE inventory item summary.</summary>
public sealed record PpeInventorySummary(
    Guid Id,
    Guid PpeCatalogId,
    Guid SiteId,
    string? SerialNumber,
    int? QuantityOnHand,
    string? Condition,
    string Status);

/// <summary>Payload to assign PPE to a person.</summary>
public sealed record AssignPpeRequest(
    Guid PpeInventoryId,
    Guid PersonId,
    DateTimeOffset? IssuedAt = null);

/// <summary>Payload to record PPE return.</summary>
public sealed record ReturnPpeRequest(
    Guid AssignmentId,
    string? ConditionOnReturn = null,
    DateTimeOffset? ReturnedAt = null);

/// <summary>A PPE assignment summary.</summary>
public sealed record PpeAssignmentSummary(
    Guid Id,
    Guid PpeInventoryId,
    Guid PersonId,
    DateTimeOffset IssuedAt,
    Guid IssuedByMemberId,
    DateTimeOffset? ReturnedAt,
    string? ConditionOnReturn);

/// <summary>
/// Cross-module contract for PPE inventory and assignments in the HealthSafety
/// module. Tenant-scoped and validated against existing catalog items / inventory.
/// </summary>
public interface IPpeInventoryService
{
    Task<PpeInventorySummary> RegisterInventoryAsync(
        RegisterPpeInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeInventorySummary>> ListInventoryAsync(
        Guid? ppeCatalogId = null,
        CancellationToken cancellationToken = default);

    Task<PpeAssignmentSummary> AssignAsync(
        AssignPpeRequest request,
        Guid issuedByMemberId,
        CancellationToken cancellationToken = default);

    Task<PpeAssignmentSummary?> ReturnAsync(
        ReturnPpeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeAssignmentSummary>> ListAssignmentsAsync(
        Guid? ppeInventoryId = null,
        CancellationToken cancellationToken = default);
}