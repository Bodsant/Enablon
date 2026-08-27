using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.member_roles (001-foundation.sql · Wave 1). Join: tenant member -> role.
/// </summary>
public sealed class MemberRoleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantMemberId { get; set; }
    public Guid RoleId { get; set; }
}
