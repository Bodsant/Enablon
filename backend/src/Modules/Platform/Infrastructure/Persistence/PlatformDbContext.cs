using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the Platform module. Backed by PostgreSQL, schema <c>platform</c>.
/// All entities follow the modular-monolith persistence convention: <see cref="Guid"/> primary keys and a
/// mandatory <c>tenant_id</c> on every table, mapped through <see cref="IPlatformDbSchema"/> at runtime.
/// </summary>
public sealed class PlatformDbContext : DbContext
{
    private readonly string _schema;

    public PlatformDbContext(DbContextOptions<PlatformDbContext> options, IPlatformDbSchema schema)
        : base(options)
    {
        _schema = schema.Schema;
    }

    public DbSet<DataClassificationEntity> DataClassifications => Set<DataClassificationEntity>();
    public DbSet<RetentionPolicyEntity> RetentionPolicies => Set<RetentionPolicyEntity>();
    public DbSet<RecordEntity> Records => Set<RecordEntity>();
    public DbSet<RecordLinkEntity> RecordLinks => Set<RecordLinkEntity>();
    public DbSet<WorkflowDefinitionEntity> WorkflowDefinitions => Set<WorkflowDefinitionEntity>();
    public DbSet<WorkflowVersionEntity> WorkflowVersions => Set<WorkflowVersionEntity>();
    public DbSet<WorkflowStateEntity> WorkflowStates => Set<WorkflowStateEntity>();
    public DbSet<WorkflowTransitionEntity> WorkflowTransitions => Set<WorkflowTransitionEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<WorkflowTaskEntity> WorkflowTasks => Set<WorkflowTaskEntity>();
    public DbSet<WorkflowDecisionEntity> WorkflowDecisions => Set<WorkflowDecisionEntity>();
    public DbSet<EscalationRuleEntity> EscalationRules => Set<EscalationRuleEntity>();
    public DbSet<FileObjectEntity> FileObjects => Set<FileObjectEntity>();
    public DbSet<EvidenceLinkEntity> EvidenceLinks => Set<EvidenceLinkEntity>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();
    public DbSet<NumberSequenceEntity> NumberSequences => Set<NumberSequenceEntity>();
    public DbSet<LookupValueEntity> LookupValues => Set<LookupValueEntity>();

    public const string DefaultSchema = "platform";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }
}