using System;

namespace Ehsms.BuildingBlocks.Tenancy;

/// <summary>
/// Resolves and exposes the active tenant id of the current operation. Fail-closed:
/// when no tenant has been resolved, <see cref="CurrentTenantId"/> is <c>null</c> and
/// any query filtered by it returns an empty result set rather than leaking data from
/// every tenant.
/// </summary>
public interface ITenantContext
{
    /// <summary>Active tenant, or <c>null</c> when not resolved (fail-closed).</summary>
    Guid? CurrentTenantId { get; }
}
