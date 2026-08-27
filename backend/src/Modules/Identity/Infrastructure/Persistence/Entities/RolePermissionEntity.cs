namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.role_permissions</c> table. Join between roles and permissions.</summary>
public sealed class RolePermissionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public RoleEntity? Role { get; set; }
    public PermissionEntity? Permission { get; set; }
}