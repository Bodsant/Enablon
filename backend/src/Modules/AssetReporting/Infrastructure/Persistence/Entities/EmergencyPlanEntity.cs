namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.plans</c> table. Emergency response plans.</summary>
public sealed class EmergencyPlanEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public Guid OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<EmergencyPlanRevisionEntity> Revisions { get; set; } = new List<EmergencyPlanRevisionEntity>();
    public ICollection<EmergencyTeamMemberEntity> TeamMembers { get; set; } = new List<EmergencyTeamMemberEntity>();
    public ICollection<EmergencyDrillEntity> Drills { get; set; } = new List<EmergencyDrillEntity>();
}