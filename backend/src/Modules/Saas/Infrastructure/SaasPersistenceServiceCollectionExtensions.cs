using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Saas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.Saas.Infrastructure;

/// <summary>
/// DI registration for the SaaS persistence layer.
/// </summary>
public static class SaasPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddSaasPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<SaasDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        // Register a shared tenant context if the caller did not supply its own scoped one.
        if (tenantContext is not null)
        {
            services.AddSingleton(tenantContext);
        }
        else
        {
            services.AddScoped<ITenantContext, ScopedTenantContext>();
        }

        // Idempotent development seed for subscription plans and versions.
        services.AddScoped<SaasDbSeeder>();

        return services;
    }
}