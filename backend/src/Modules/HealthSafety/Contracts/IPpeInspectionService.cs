namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to record a PPE inspection result.</summary>
public sealed record RecordPpeInspectionRequest(
    Guid PpeInventoryId,
    string Condition,
    string Result,
    DateOnly? NextDueDate = null,
    DateTimeOffset? InspectedAt = null);

/// <summary>A PPE inspection summary.</summary>
public sealed record PpeInspectionSummary(
    Guid Id,
    Guid PpeInventoryId,
    Guid InspectedByMemberId,
    DateTimeOffset InspectedAt,
    string Condition,
    string Result,
    DateOnly? NextDueDate);

/// <summary>Payload to request a PPE replacement for an assignment.</summary>
public sealed record RequestPpeReplacementRequest(
    Guid PpeAssignmentId,
    string ReplacementReason,
    DateTimeOffset? RequestedAt = null);

/// <summary>A PPE replacement summary.</summary>
public sealed record PpeReplacementSummary(
    Guid Id,
    Guid PpeAssignmentId,
    string ReplacementReason,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt);

/// <summary>
/// Cross-module contract for PPE inspections and replacements in the HealthSafety
/// module. Tenant-scoped and validated against existing inventory / assignments.
/// </summary>
public interface IPpeInspectionService
{
    Task<PpeInspectionSummary> RecordInspectionAsync(
        RecordPpeInspectionRequest request,
        Guid inspectedByMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeInspectionSummary>> ListInspectionsAsync(
        Guid? ppeInventoryId = null,
        CancellationToken cancellationToken = default);

    Task<PpeReplacementSummary> RequestReplacementAsync(
        RequestPpeReplacementRequest request,
        CancellationToken cancellationToken = default);

    Task<PpeReplacementSummary?> CompleteReplacementAsync(
        Guid replacementId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeReplacementSummary>> ListReplacementsAsync(
        Guid? ppeAssignmentId = null,
        CancellationToken cancellationToken = default);
}