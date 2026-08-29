using System.Security.Claims;
using Ehsms.BuildingBlocks.Tenancy;

namespace Ehsms.Api.Authentication;

/// <summary>
/// Resolves the active tenant for the current request from the authenticated user's
/// <c>tenant</c> claim and writes it into the scoped <see cref="ScopedTenantContext"/>.
/// Requests without a resolved tenant leave the context unresolved, so tenant-filtered
/// queries fail closed (empty result set) rather than leaking cross-tenant data.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantClaim = context.User.FindFirst("tenant")?.Value;
        if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantId))
        {
            if (tenantContext is ScopedTenantContext scoped)
            {
                scoped.CurrentTenantId = tenantId;
            }
        }

        await _next(context);
    }
}