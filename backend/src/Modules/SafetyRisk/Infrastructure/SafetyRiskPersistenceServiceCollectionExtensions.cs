using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.SafetyRisk.Contracts;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.SafetyRisk.Infrastructure;

/// <summary>
/// DI registration for the SafetyRisk persistence layer (document, safety,
/// risk, incident, capa schemas). Sprint 11 wires the risk schema services.
/// </summary>
public static class SafetyRiskPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddSafetyRiskPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<SafetyRiskDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        if (tenantContext is not null)
        {
            services.AddSingleton(tenantContext);
        }
        else
        {
            services.AddScoped<ITenantContext, ScopedTenantContext>();
        }

        // Hazard & risk backend (Trello Sprint 11).
        services.AddScoped<IHazardRiskService, HazardRiskService>();

        return services;
    }
}