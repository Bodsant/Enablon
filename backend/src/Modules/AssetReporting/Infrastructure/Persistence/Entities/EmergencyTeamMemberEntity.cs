namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.team_members</c> table. Members of an emergency response team.</summary>
public sealed class EmergencyTeamMemberEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmergencyPlanId { get; set; }
    public Guid PersonId { get; set; }
    public string EmergencyRole { get; set; } = string.Empty;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public EmergencyPlanEntity? EmergencyPlan { get; set; }
}