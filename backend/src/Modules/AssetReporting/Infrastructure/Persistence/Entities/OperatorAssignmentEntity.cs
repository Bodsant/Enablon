namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.operator_assignments</c> table. Operator eligibility assignment to an asset.</summary>
public sealed class OperatorAssignmentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public Guid PersonId { get; set; }
    public DateOnly? AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public string? EligibilityStatus { get; set; }

    public AssetEntity? Asset { get; set; }
}