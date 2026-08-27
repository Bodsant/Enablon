using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.member_access_scopes (001-foundation.sql · Wave 1). Join: member -> access scope.
/// </summary>
public sealed class MemberAccessScopeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid AccessScopeId { get; set; }
}
