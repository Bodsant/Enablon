namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.member_roles</c> table. Join between tenant members and roles.</summary>
public sealed class MemberRoleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid RoleId { get; set; }

    public TenantMemberEntity? TenantMember { get; set; }
    public RoleEntity? Role { get; set; }
}