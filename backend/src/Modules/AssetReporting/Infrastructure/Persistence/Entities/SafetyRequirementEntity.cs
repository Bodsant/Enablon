namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.safety_requirements</c> table. Safety requirements attached to an asset.</summary>
public sealed class SafetyRequirementEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string RequirementType { get; set; } = string.Empty;
    public int? FrequencyDays { get; set; }
    public Guid? CompetencyId { get; set; }
    public string Status { get; set; } = string.Empty;

    public AssetEntity? Asset { get; set; }
}