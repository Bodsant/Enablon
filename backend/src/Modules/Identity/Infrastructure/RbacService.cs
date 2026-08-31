using Ehsms.Modules.Identity.Contracts;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// RBAC admin backend (Trello Sprint 31 R3): role/permission catalog, role-permission
/// links and member-role assignments, all tenant-scoped. Inserts validate parents exist
/// in the same tenant (FKs are intra-schema).
/// </summary>
public sealed class RbacService : IRbacService
{
    private readonly IdentityDbContext _db;

    public RbacService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Roles.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
        return items.Select(ToRoleDto).ToList();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new RoleEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            ScopeType = string.IsNullOrWhiteSpace(request.ScopeType) ? "Company" : request.ScopeType.Trim(),
            IsSystem = request.IsSystem,
        };
        _db.Roles.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToRoleDto(entity);
    }

    public async Task<IReadOnlyList<PermissionDto>> ListPermissionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.Permissions.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .ToListAsync(ct);
        return items.Select(p => new PermissionDto(p.Id, p.Code, p.Module, p.Action, p.Description)).ToList();
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new PermissionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Module = request.Module.Trim().ToLowerInvariant(),
            Action = request.Action.Trim().ToLowerInvariant(),
            Description = request.Description,
        };
        _db.Permissions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new PermissionDto(entity.Id, entity.Code, entity.Module, entity.Action, entity.Description);
    }

    public async Task<RolePermissionDto> AttachPermissionAsync(Guid roleId, Guid permissionId, Guid tenantId, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, ct);
        if (role is null) throw new InvalidOperationException($"Role {roleId} not found in tenant {tenantId}.");
        var perm = await _db.Permissions.FirstOrDefaultAsync(p => p.Id == permissionId && p.TenantId == tenantId, ct);
        if (perm is null) throw new InvalidOperationException($"Permission {permissionId} not found in tenant {tenantId}.");

        // Idempotent link.
        var existing = await _db.RolePermissions.FirstOrDefaultAsync(
            rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId, ct);
        if (existing is not null) return ToRolePermissionDto(existing);

        var link = new RolePermissionEntity { Id = Guid.NewGuid(), TenantId = tenantId, RoleId = roleId, PermissionId = permissionId };
        _db.RolePermissions.Add(link);
        await _db.SaveChangesAsync(ct);
        return ToRolePermissionDto(link);
    }

    public async Task<MemberRoleDto> AssignRoleAsync(Guid tenantMemberId, Guid roleId, Guid tenantId, CancellationToken ct)
    {
        var member = await _db.TenantMembers.FirstOrDefaultAsync(m => m.Id == tenantMemberId && m.TenantId == tenantId, ct);
        if (member is null) throw new InvalidOperationException($"Member {tenantMemberId} not found in tenant {tenantId}.");
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, ct);
        if (role is null) throw new InvalidOperationException($"Role {roleId} not found in tenant {tenantId}.");

        var existing = await _db.MemberRoles.FirstOrDefaultAsync(
            mr => mr.TenantMemberId == tenantMemberId && mr.RoleId == roleId && mr.TenantId == tenantId, ct);
        if (existing is not null) return ToMemberRoleDto(existing);

        var assign = new MemberRoleEntity { Id = Guid.NewGuid(), TenantId = tenantId, TenantMemberId = tenantMemberId, RoleId = roleId };
        _db.MemberRoles.Add(assign);
        await _db.SaveChangesAsync(ct);
        return ToMemberRoleDto(assign);
    }

    public async Task<IReadOnlyList<TenantMemberSummaryDto>> ListMembersAsync(Guid tenantId, CancellationToken ct)
    {
        var rows = await (
            from m in _db.TenantMembers
            join u in _db.Users on m.UserId equals u.Id
            where m.TenantId == tenantId
            orderby u.Email
            select new { m.Id, u.Email }).ToListAsync(ct);
        return rows.Select(r => new TenantMemberSummaryDto(r.Id, r.Email)).ToList();
    }

    private static RoleDto ToRoleDto(RoleEntity r) => new(r.Id, r.Code, r.Name, r.ScopeType, r.IsSystem);
    private static RolePermissionDto ToRolePermissionDto(RolePermissionEntity rp) => new(rp.Id, rp.RoleId, rp.PermissionId);
    private static MemberRoleDto ToMemberRoleDto(MemberRoleEntity mr) => new(mr.Id, mr.TenantMemberId, mr.RoleId);
}