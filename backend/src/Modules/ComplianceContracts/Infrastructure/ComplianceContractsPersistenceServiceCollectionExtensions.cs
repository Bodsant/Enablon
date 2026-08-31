using Ehsms.Modules.ComplianceContracts.Contracts;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure;

/// <summary>
/// DI registration for the ComplianceContracts persistence layer (compliance, contractor, training schemas).
/// Sprint 19 (R2) wires the contractor / contract management service.
/// </summary>
public static class ComplianceContractsPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddComplianceContractsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ComplianceContractsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Contractor & contract management backend (Trello Sprint 19 R2).
        services.AddScoped<IContractService, ContractService>();

        // Training & competency backend (Trello Sprint 20 R2).
        services.AddScoped<ITrainingService, TrainingService>();

        // Legal & compliance backend (Trello Sprint 25 R2).
        services.AddScoped<ILegalComplianceService, LegalComplianceService>();

        return services;
    }
}
