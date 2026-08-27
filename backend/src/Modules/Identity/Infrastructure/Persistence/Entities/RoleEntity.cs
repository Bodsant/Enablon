namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.roles</c> table.</summary>
public sealed class RoleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public bool IsSystem { get; set; }

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    public ICollection<MemberRoleEntity> MemberRoles { get; set; } = new List<MemberRoleEntity>();
}