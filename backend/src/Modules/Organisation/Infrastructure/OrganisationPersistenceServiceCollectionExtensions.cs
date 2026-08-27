using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.Organisation.Infrastructure;

/// <summary>
/// DI registration for the Organisation persistence layer.
/// </summary>
public static class OrganisationPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddOrganisationPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<OrganisationDbContext>(options =>
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

        return services;
    }
}
