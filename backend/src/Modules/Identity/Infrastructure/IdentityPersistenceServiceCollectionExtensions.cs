using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// DI registration for the Identity persistence layer.
/// </summary>
public static class IdentityPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        if (tenantContext is not null)
        {
            services.AddSingleton(tenantContext);
        }
        else
        {
            services.AddScoped<ITenantContext, ScopedTenantContext>();
        }

        return services;
    }
}
