namespace Ehsms.Modules.Identity.Contracts;

/// <summary>Payload to create an access scope (the WHERE of authorization).</summary>
public sealed record CreateAccessScopeRequest(
    string ScopeType,
    Guid? CompanyId,
    Guid? BusinessUnitId,
    Guid? SiteId,
    Guid? DepartmentId,
    Guid? LocationId,
    Guid? ContractorCompanyId,
    Guid? DataClassificationId);

public sealed record AccessScopeDto(
    Guid Id,
    string ScopeType,
    Guid? CompanyId,
    Guid? BusinessUnitId,
    Guid? SiteId,
    Guid? DepartmentId,
    Guid? LocationId,
    Guid? ContractorCompanyId,
    Guid? DataClassificationId);

/// <summary>States a member's access scope.</summary>
public sealed record MemberAccessScopeDto(Guid Id, Guid TenantMemberId, Guid AccessScopeId);

/// <summary>Body for granting a scope to a member.</summary>
public sealed record GrantScopeRequest(Guid AccessScopeId);

/// <summary>Payload to create a temporary access grant.</summary>
public sealed record CreateTemporaryAccessGrantRequest(
    Guid TenantMemberId,
    Guid AccessScopeId,
    Guid? RoleId,
    Guid? ApprovedByMemberId,
    string Reason,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil);

public sealed record TemporaryAccessGrantDto(
    Guid Id,
    Guid TenantMemberId,
    Guid AccessScopeId,
    Guid? RoleId,
    Guid? ApprovedByMemberId,
    string Reason,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil);

/// <summary>
/// Access scope & temporary grant backend (Trello Sprint 32 R3). Access scopes define
/// the WHERE of authorization (company/BU/site/dept/location); grants give temporary access.
/// </summary>
public interface IAccessScopeService
{
    Task<IReadOnlyList<AccessScopeDto>> ListScopesAsync(Guid tenantId, CancellationToken ct);
    Task<AccessScopeDto> CreateScopeAsync(CreateAccessScopeRequest request, Guid tenantId, CancellationToken ct);
    Task<MemberAccessScopeDto> GrantScopeToMemberAsync(Guid tenantMemberId, Guid accessScopeId, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<MemberAccessScopeDto>> ListMemberScopesAsync(Guid tenantId, Guid tenantMemberId, CancellationToken ct);
    Task<TemporaryAccessGrantDto> CreateGrantAsync(CreateTemporaryAccessGrantRequest request, Guid tenantId, Guid activeMemberId, CancellationToken ct);
    Task<IReadOnlyList<TemporaryAccessGrantDto>> ListGrantsAsync(Guid tenantId, CancellationToken ct);
}