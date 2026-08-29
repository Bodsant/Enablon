namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.inspections</c> table. Asset inspections.</summary>
public sealed class InspectionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid AssetId { get; set; }
    public string InspectionType { get; set; } = string.Empty;
    public DateTimeOffset? InspectedAt { get; set; }
    public Guid? InspectedByPersonId { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateOnly? NextDueDate { get; set; }

    public AssetEntity? Asset { get; set; }
}