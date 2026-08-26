using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Identity.Domain;

public class User : Entity
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ExternalId { get; set; } // For OIDC/SSO
    public string? AuthProvider { get; set; }
}

public class TenantMembership
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Role
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; } // null = global role
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; } // system roles cannot be deleted
    public DateTime CreatedAt { get; set; }
}

public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., incident.read, capa.verify
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Permission Permission { get; set; } = null!;
}

public class MemberRole
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
}

public class AccessScope
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty; // company/bu/site/department/location/contractor
    public Guid? ParentScopeId { get; set; }
    public Guid TenantId { get; set; }
}

public class MemberAccessScope
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid AccessScopeId { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class TemporaryAccessGrant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string? Justification { get; set; }
    public string? ApproverUserId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}
