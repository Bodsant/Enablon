namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.team_members</c> table.</summary>
public sealed class AuditTeamMemberEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AuditId { get; set; }
    public Guid TenantMemberId { get; set; }
    public string? AuditRole { get; set; }
}
