namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.defects</c> table. Reported asset defects.</summary>
public sealed class DefectEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? RestrictionStatus { get; set; }
    public string? MaintenanceReference { get; set; }
    public Guid? OwnerMemberId { get; set; }

    public AssetEntity? Asset { get; set; }
}