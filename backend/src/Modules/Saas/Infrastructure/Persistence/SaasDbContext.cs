using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Saas.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.Saas.Infrastructure.Persistence;

public sealed class SaasDbContext : DbContext
{
    public SaasDbContext(DbContextOptions<SaasDbContext> options) : base(options) { }

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PlanVersion> PlanVersions => Set<PlanVersion>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<TenantStorageUsage> TenantStorageUsages => Set<TenantStorageUsage>();
    public DbSet<TenantUsagePeriod> TenantUsagePeriods => Set<TenantUsagePeriod>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaasDbContext).Assembly);
        ApplySnakeCaseColumnNames(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySnakeCaseColumnNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
            property.SetColumnName(ToSnakeCase(property.Name));
    }

    private static string ToSnakeCase(string name)
    {
        var result = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0) result.Append('_');
            result.Append(char.ToLowerInvariant(name[i]));
        }
        return result.ToString();
    }
}

public interface ISaasDbSchema
{
    string Schema { get; }
}

public sealed class SaasDbSchema : ISaasDbSchema
{
    public string Schema => "saas";
}