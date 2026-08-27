using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.temporary_access_grants (001-foundation.sql · Wave 1). Time-boxed access grant.
/// </summary>
public sealed class TemporaryAccessGrantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid AccessScopeId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid ApprovedByMemberId { get; set; }
    public string Reason { get; set; } = default!;
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidUntil { get; set; }
}
