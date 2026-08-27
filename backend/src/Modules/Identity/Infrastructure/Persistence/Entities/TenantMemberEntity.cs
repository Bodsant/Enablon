namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.tenant_members</c> table. A user's membership in a tenant.</summary>
public sealed class TenantMemberEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? PersonId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? DeactivatedAt { get; set; }

    public UserEntity? User { get; set; }
    public ICollection<MemberRoleEntity> MemberRoles { get; set; } = new List<MemberRoleEntity>();
    public ICollection<MemberAccessScopeEntity> MemberAccessScopes { get; set; } = new List<MemberAccessScopeEntity>();
    public ICollection<TemporaryAccessGrantEntity> TemporaryAccessGrants { get; set; } = new List<TemporaryAccessGrantEntity>();
    public ICollection<AccessReviewEntity> ReviewedAccessReviews { get; set; } = new List<AccessReviewEntity>();
    public ICollection<TemporaryAccessGrantEntity> ApprovedTemporaryAccessGrants { get; set; } = new List<TemporaryAccessGrantEntity>();
}