using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.roles (001-foundation.sql · Wave 1). A named role within a tenant.
/// </summary>
public sealed class RoleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ScopeType { get; set; } = default!;
    public bool IsSystem { get; set; }
}
