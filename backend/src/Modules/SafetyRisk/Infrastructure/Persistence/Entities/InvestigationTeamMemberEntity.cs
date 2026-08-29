namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.investigation_team</c> table.</summary>
public sealed class InvestigationTeamMemberEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InvestigationId { get; set; }
    public Guid TenantMemberId { get; set; }
    public string? TeamRole { get; set; } = string.Empty;
}
