using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.tenant_members (001-foundation.sql · Wave 1). A user's membership in a tenant.
/// </summary>
public sealed class TenantMemberEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? PersonId { get; set; }
    public string DisplayName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? DeactivatedAt { get; set; }
}
