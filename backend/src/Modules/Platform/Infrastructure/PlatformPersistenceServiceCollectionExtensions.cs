using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// DI registration for the Platform persistence layer.
/// </summary>
public static class PlatformPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<PlatformDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        // Platform tables live in the dedicated "platform" PostgreSQL schema.
        services.AddSingleton<IPlatformDbSchema, DefaultPlatformDbSchema>();

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

/// <summary>Default schema binding for the Platform module: PostgreSQL schema <c>platform</c>.</summary>
public sealed class DefaultPlatformDbSchema : IPlatformDbSchema
{
    public string Schema => "platform";
}