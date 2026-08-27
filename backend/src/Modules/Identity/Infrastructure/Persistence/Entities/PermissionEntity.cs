namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.permissions</c> table.</summary>
public sealed class PermissionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
}