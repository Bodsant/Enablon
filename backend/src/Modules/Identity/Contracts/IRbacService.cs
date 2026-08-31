namespace Ehsms.Modules.Identity.Contracts;

/// <summary>Payload to create a role.</summary>
public sealed record CreateRoleRequest(
    string Code,
    string Name,
    string ScopeType,
    bool IsSystem);

public sealed record RoleDto(
    Guid Id,
    string Code,
    string Name,
    string ScopeType,
    bool IsSystem);

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Module,
    string Action,
    string? Description);

/// <summary>Payload to create a permission.</summary>
public sealed record CreatePermissionRequest(
    string Code,
    string Module,
    string Action,
    string? Description);

public sealed record RolePermissionDto(Guid Id, Guid RoleId, Guid PermissionId);
public sealed record MemberRoleDto(Guid Id, Guid TenantMemberId, Guid RoleId);

/// <summary>Body for attaching a permission to a role.</summary>
public sealed record AttachPermissionRequest(Guid PermissionId);

/// <summary>Body for assigning a role to a member.</summary>
public sealed record AssignRoleRequest(Guid RoleId);

/// <summary>A tenant member (id + joined user email) for RBAC admin lists.</summary>
public sealed record TenantMemberSummaryDto(Guid Id, string Email);

/// <summary>
/// RBAC admin backend (Trello Sprint 31 R3): role/permission catalog and role assignment.
/// </summary>
public interface IRbacService
{
    Task<IReadOnlyList<RoleDto>> ListRolesAsync(Guid tenantId, CancellationToken ct);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<PermissionDto>> ListPermissionsAsync(Guid tenantId, CancellationToken ct);
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request, Guid tenantId, CancellationToken ct);
    Task<RolePermissionDto> AttachPermissionAsync(Guid roleId, Guid permissionId, Guid tenantId, CancellationToken ct);
    Task<MemberRoleDto> AssignRoleAsync(Guid tenantMemberId, Guid roleId, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<TenantMemberSummaryDto>> ListMembersAsync(Guid tenantId, CancellationToken ct);
}