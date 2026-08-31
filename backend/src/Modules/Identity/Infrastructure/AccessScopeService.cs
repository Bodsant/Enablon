using Ehsms.Modules.Identity.Contracts;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// Access scope & temporary grant backend (Trello Sprint 32 R3). Access scopes are the
/// WHERE of authorization; grants give temporary access to a scope. Member/scope FKs are
/// intra-schema and validated. Guid.Empty owner/approver falls back to the active member.
/// </summary>
public sealed class AccessScopeService : IAccessScopeService
{
    private readonly IdentityDbContext _db;

    public AccessScopeService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AccessScopeDto>> ListScopesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.AccessScopes.AsNoTracking()
            .Where(s => s.TenantId == tenantId).OrderBy(s => s.ScopeType).ToListAsync(ct);
        return items.Select(ToScopeDto).ToList();
    }

    public async Task<AccessScopeDto> CreateScopeAsync(CreateAccessScopeRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new AccessScopeEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScopeType = string.IsNullOrWhiteSpace(request.ScopeType) ? "Company" : request.ScopeType.Trim(),
            CompanyId = request.CompanyId,
            BusinessUnitId = request.BusinessUnitId,
            SiteId = request.SiteId,
            DepartmentId = request.DepartmentId,
            LocationId = request.LocationId,
            ContractorCompanyId = request.ContractorCompanyId,
            DataClassificationId = request.DataClassificationId,
        };
        _db.AccessScopes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToScopeDto(entity);
    }

    public async Task<MemberAccessScopeDto> GrantScopeToMemberAsync(Guid tenantMemberId, Guid accessScopeId, Guid tenantId, CancellationToken ct)
    {
        var member = await _db.TenantMembers.FirstOrDefaultAsync(m => m.Id == tenantMemberId && m.TenantId == tenantId, ct);
        if (member is null) throw new InvalidOperationException($"Member {tenantMemberId} not found in tenant {tenantId}.");
        var scope = await _db.AccessScopes.FirstOrDefaultAsync(s => s.Id == accessScopeId && s.TenantId == tenantId, ct);
        if (scope is null) throw new InvalidOperationException($"Access scope {accessScopeId} not found in tenant {tenantId}.");

        var existing = await _db.MemberAccessScopes.FirstOrDefaultAsync(
            m => m.TenantMemberId == tenantMemberId && m.AccessScopeId == accessScopeId && m.TenantId == tenantId, ct);
        if (existing is not null) return new MemberAccessScopeDto(existing.Id, existing.TenantMemberId, existing.AccessScopeId);

        var link = new MemberAccessScopeEntity { Id = Guid.NewGuid(), TenantId = tenantId, TenantMemberId = tenantMemberId, AccessScopeId = accessScopeId };
        _db.MemberAccessScopes.Add(link);
        await _db.SaveChangesAsync(ct);
        return new MemberAccessScopeDto(link.Id, link.TenantMemberId, link.AccessScopeId);
    }

    public async Task<IReadOnlyList<MemberAccessScopeDto>> ListMemberScopesAsync(Guid tenantId, Guid tenantMemberId, CancellationToken ct)
    {
        var items = await _db.MemberAccessScopes.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.TenantMemberId == tenantMemberId).ToListAsync(ct);
        return items.Select(x => new MemberAccessScopeDto(x.Id, x.TenantMemberId, x.AccessScopeId)).ToList();
    }

    public async Task<TemporaryAccessGrantDto> CreateGrantAsync(CreateTemporaryAccessGrantRequest request, Guid tenantId, Guid activeMemberId, CancellationToken ct)
    {
        var member = await _db.TenantMembers.FirstOrDefaultAsync(m => m.Id == request.TenantMemberId && m.TenantId == tenantId, ct);
        if (member is null) throw new InvalidOperationException($"Member {request.TenantMemberId} not found.");
        var scope = await _db.AccessScopes.FirstOrDefaultAsync(s => s.Id == request.AccessScopeId && s.TenantId == tenantId, ct);
        if (scope is null) throw new InvalidOperationException($"Access scope {request.AccessScopeId} not found.");

        var approvedById = request.ApprovedByMemberId;
        var approvedBy = (approvedById ?? Guid.Empty) == Guid.Empty ? activeMemberId : approvedById.GetValueOrDefault();
        var approved = await _db.TenantMembers.FirstOrDefaultAsync(m => m.Id == approvedBy && m.TenantId == tenantId, ct);
        if (approved is null) throw new InvalidOperationException($"Approver member {approvedBy} not found.");

        var entity = new TemporaryAccessGrantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TenantMemberId = request.TenantMemberId,
            AccessScopeId = request.AccessScopeId,
            RoleId = request.RoleId,
            ApprovedByMemberId = approvedBy,
            Reason = request.Reason,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
        };
        _db.TemporaryAccessGrants.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToGrantDto(entity);
    }

    public async Task<IReadOnlyList<TemporaryAccessGrantDto>> ListGrantsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.TemporaryAccessGrants.AsNoTracking()
            .Where(g => g.TenantId == tenantId).OrderByDescending(g => g.ValidFrom).ToListAsync(ct);
        return items.Select(ToGrantDto).ToList();
    }

    private static AccessScopeDto ToScopeDto(AccessScopeEntity s) =>
        new(s.Id, s.ScopeType, s.CompanyId, s.BusinessUnitId, s.SiteId, s.DepartmentId, s.LocationId, s.ContractorCompanyId, s.DataClassificationId);

    private static TemporaryAccessGrantDto ToGrantDto(TemporaryAccessGrantEntity g) =>
        new(g.Id, g.TenantMemberId, g.AccessScopeId, g.RoleId, g.ApprovedByMemberId, g.Reason, g.ValidFrom, g.ValidUntil);
}