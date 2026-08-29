namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.drills</c> table. Emergency response drills.</summary>
public sealed class EmergencyDrillEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid EmergencyPlanId { get; set; }
    public string Scenario { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? ConductedAt { get; set; }
    public string? ResultSummary { get; set; }
    public Guid? CoordinatorMemberId { get; set; }

    public EmergencyPlanEntity? EmergencyPlan { get; set; }
    public ICollection<EmergencyDrillParticipantEntity> Participants { get; set; } = new List<EmergencyDrillParticipantEntity>();
    public ICollection<EmergencyDrillFindingEntity> Findings { get; set; } = new List<EmergencyDrillFindingEntity>();
}