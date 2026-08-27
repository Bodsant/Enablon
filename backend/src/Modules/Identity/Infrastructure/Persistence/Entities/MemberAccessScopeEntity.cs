namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.member_access_scopes</c> table. Join between tenant members and access scopes.</summary>
public sealed class MemberAccessScopeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid AccessScopeId { get; set; }

    public TenantMemberEntity? TenantMember { get; set; }
    public AccessScopeEntity? AccessScope { get; set; }
}