using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Saas.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

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
        base.OnModelCreating(modelBuilder);
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