using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.WorkControl.Contracts;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.WorkControl.Infrastructure;

/// <summary>
/// DI registration for the WorkControl persistence layer (cow, inspection, audit schemas).
/// Sprint 15 wires the inspection &amp; audit services.
/// </summary>
public static class WorkControlPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddWorkControlPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<WorkControlDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Inspection & audit backend (Trello Sprint 15).
        services.AddScoped<IInspectionAuditService, InspectionAuditService>();

        // PTW / JSA / LOTO backend (Trello Sprint 17).
        services.AddScoped<IPtwJsaLotoService, PtwJsaLotoService>();

        return services;
    }
}
