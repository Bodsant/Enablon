namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.assets</c> table. Physical asset master data.</summary>
public sealed class AssetEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string? SourceSystem { get; set; }
    public string? SourceId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public Guid SiteId { get; set; }
    public Guid? LocationId { get; set; }
    public bool IsSafetyCritical { get; set; }
    public string Status { get; set; } = string.Empty;
}