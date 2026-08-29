namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.drill_findings</c> table. Findings raised from an emergency drill.</summary>
public sealed class EmergencyDrillFindingEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid EmergencyDrillId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public Guid? OwnerMemberId { get; set; }

    public EmergencyDrillEntity? EmergencyDrill { get; set; }
}