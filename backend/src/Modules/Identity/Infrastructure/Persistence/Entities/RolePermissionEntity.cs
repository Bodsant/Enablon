using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.role_permissions (001-foundation.sql · Wave 1). Join: role -> permission.
/// </summary>
public sealed class RolePermissionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
