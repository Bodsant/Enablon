using System;
using System.Linq;
using System.Linq.Expressions;
using Ehsms.BuildingBlocks.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.BuildingBlocks.Persistence;

/// <summary>
/// Application-layer tenant isolation. The global model-level filter is intentionally
/// NOT used: EF Core caches a single compiled model (and thus a single baked filter
/// value) per service provider, so a runtime-mutable flag cannot isolate tenants within
/// one process. Instead every access to tenant-scoped data is routed through this helper,
/// which adds a fail-closed <c>TenantId == tenant</c> predicate at query time via the
/// EF-translatable <see cref="EF.Property{TProperty}"/> API.
///
/// Fail-closed: when <see cref="ITenantContext.CurrentTenantId"/> is <c>null</c> the
/// predicate is always false, yielding an empty result set rather than a cross-tenant leak.
/// PostgreSQL RLS remains the complementary database-layer enforcement.
/// </summary>
public static class TenantIsolation
{
    public static IQueryable<T> ForTenant<T>(this IQueryable<T> source, ITenantContext tenantContext)
        where T : class
    {
        if (tenantContext is null)
            throw new ArgumentNullException(nameof(tenantContext));

        var tenantId = tenantContext.CurrentTenantId;
        if (!tenantId.HasValue)
        {
            // Fail closed: no resolved tenant -> match nothing.
            return source.Where(_ => false);
        }

        var parameter = Expression.Parameter(typeof(T), "e");
        var tenantProperty = Expression.Call(
            typeof(EF).GetMethod(nameof(EF.Property), new[] { typeof(object), typeof(string) })!
                .MakeGenericMethod(typeof(Guid)),
            Expression.Convert(parameter, typeof(object)),
            Expression.Constant("TenantId"));
        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(tenantProperty, Expression.Constant(tenantId.Value)),
            parameter);

        return source.Where(predicate);
    }
}
