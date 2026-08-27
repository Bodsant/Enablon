using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.permissions (001-foundation.sql · Wave 1). A granular permission code.
/// </summary>
public sealed class PermissionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string Module { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string? Description { get; set; }
}
