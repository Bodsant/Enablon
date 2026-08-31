using Ehsms.Modules.AssetReporting.Contracts;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.AssetReporting.Infrastructure;

/// <summary>
/// DI registration for the AssetReporting persistence layer (asset, emergency,
/// reporting and integration schemas).
/// </summary>
public static class AssetReportingPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAssetReportingPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AssetReportingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAssetReportingDbSchema, DefaultAssetReportingDbSchema>();

        // Asset safety & emergency backend (Trello Sprint 26 R2).
        services.AddScoped<IAssetEmergencyService, AssetEmergencyService>();

        // Reporting & KPI backend (Trello Sprint 27 R2).
        services.AddScoped<IReportingKpiService, ReportingKpiService>();

        return services;
    }
}

/// <summary>Default schema binding for the AssetReporting module tables.</summary>
public sealed class DefaultAssetReportingDbSchema : IAssetReportingDbSchema
{
    public string Schema => "asset";
}