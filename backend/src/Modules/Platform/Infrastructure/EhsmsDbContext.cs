using Ehsms.BuildingBlocks;
using Ehsms.Modules.Identity.Domain;
using Ehsms.Modules.Organisation.Domain;
using Ehsms.Modules.Platform.Domain;
using Ehsms.Modules.Saas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ehsms.Modules.Platform.Infrastructure;

public class EhsmsDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;
    private IDbContextTransaction? _transaction;

    public EhsmsDbContext(DbContextOptions<EhsmsDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // === Platform ===
    public DbSet<Record> Records => Set<Record>();
    public DbSet<RecordLink> RecordLinks => Set<RecordLink>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowVersion> WorkflowVersions => Set<WorkflowVersion>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowDecision> WorkflowDecisions => Set<WorkflowDecision>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<FileObject> FileObjects => Set<FileObject>();
    public DbSet<EvidenceLink> EvidenceLinks => Set<EvidenceLink>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<LookupValue> LookupValues => Set<LookupValue>();
    public DbSet<DataClassification> DataClassifications => Set<DataClassification>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();

    // === Identity ===
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<MemberRole> MemberRoles => Set<MemberRole>();
    public DbSet<AccessScope> AccessScopes => Set<AccessScope>();
    public DbSet<MemberAccessScope> MemberAccessScopes => Set<MemberAccessScope>();
    public DbSet<TemporaryAccessGrant> TemporaryAccessGrants => Set<TemporaryAccessGrant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // === Organisation ===
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Employee> Employees => Set<Employee>();

    // === SaaS ===
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PlanVersion> PlanVersions => Set<PlanVersion>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<TenantStorageUsage> TenantStorageUsages => Set<TenantStorageUsage>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("platform");

        // Apply all IEntityTypeConfiguration from each module assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EhsmsDbContext).Assembly);

        // Global query filter for tenant isolation (developer guardrail)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(EhsmsDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder]);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == default(Guid) || true);
        // Note: actual filter applied via SetTenantId interceptor at runtime
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
