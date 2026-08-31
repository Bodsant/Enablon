using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// DI registration for the HealthSafety persistence layer (ppe, health, chemical,
/// environment, sustainability schemas).
/// </summary>
public static class HealthSafetyPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddHealthSafetyPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<HealthSafetyDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        services.AddSingleton<IHealthSafetyDbSchema, DefaultHealthSafetyDbSchema>();

        if (tenantContext is not null)
        {
            services.AddSingleton(tenantContext);
        }
        else
        {
            services.AddScoped<ITenantContext, ScopedTenantContext>();
        }

        // Chemical product catalogue (creates a backing platform record via contract).
        services.AddScoped<IChemicalCatalogService, ChemicalCatalogService>();

        // Chemical inventory & SDS records.
        services.AddScoped<IChemicalInventoryService, ChemicalInventoryService>();

        // Chemical exposure controls.
        services.AddScoped<IChemicalExposureControlService, ChemicalExposureControlService>();

        // Chemical storage inspections.
        services.AddScoped<IChemicalStorageInspectionService, ChemicalStorageInspectionService>();

        // PPE catalogue & requirements.
        services.AddScoped<IPpeCatalogService, PpeCatalogService>();

        // PPE inventory & assignments.
        services.AddScoped<IPpeInventoryService, PpeInventoryService>();

        // PPE inspections & replacements.
        services.AddScoped<IPpeInspectionService, PpeInspectionService>();

        // Environment monitoring (parameters, sources, measurements).
        services.AddScoped<IEnvironmentMonitoringService, EnvironmentMonitoringService>();

        // Occupational health (profiles, fitness, surveillance programs/events, follow-ups).
        services.AddScoped<IOccupationalHealthService, OccupationalHealthService>();

        return services;
    }
}

/// <summary>Default schema binding for the HealthSafety module tables.</summary>
public sealed class DefaultHealthSafetyDbSchema : IHealthSafetyDbSchema
{
    public string PpeSchema => "ppe";
    public string HealthSchema => "health";
    public string ChemicalSchema => "chemical";
    public string EnvironmentSchema => "environment";
    public string SustainabilitySchema => "sustainability";
}