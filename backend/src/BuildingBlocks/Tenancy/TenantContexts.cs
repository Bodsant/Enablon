using System;

namespace Ehsms.BuildingBlocks.Tenancy;

/// <summary>
/// A <see cref="ITenantContext"/> that has not resolved a tenant. `CurrentTenantId`
/// is always <c>null</c>, so any tenant-filtered query fails closed (empty result).
/// Used as the default when no request has authenticated a tenant.
/// </summary>
public sealed class UnresolvedTenantContext : ITenantContext
{
    public Guid? CurrentTenantId => null;
}

/// <summary>
/// Mutable <see cref="ITenantContext"/> for resolving a tenant once per request scope.
/// </summary>
public sealed class ScopedTenantContext : ITenantContext
{
    public Guid? CurrentTenantId { get; set; }
}
