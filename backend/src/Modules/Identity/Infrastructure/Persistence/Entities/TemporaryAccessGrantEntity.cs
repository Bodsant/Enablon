namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.temporary_access_grants</c> table.</summary>
public sealed class TemporaryAccessGrantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid AccessScopeId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid ApprovedByMemberId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidUntil { get; set; }

    public TenantMemberEntity? TenantMember { get; set; }
    public AccessScopeEntity? AccessScope { get; set; }
    public RoleEntity? Role { get; set; }
    public TenantMemberEntity? ApprovedByMember { get; set; }
}