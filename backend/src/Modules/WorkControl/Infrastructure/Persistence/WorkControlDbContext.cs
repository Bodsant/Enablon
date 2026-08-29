using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the Work Control module, spanning the <c>cow</c> (control of work),
/// <c>inspection</c>, and <c>audit</c> schemas. Primary keys are <see cref="Guid"/> and every table carries a
/// mandatory <c>tenant_id</c>, following the modular-monolith persistence convention.
/// Cross-schema foreign keys are deliberately NOT modelled as EF relationships (they exist as plain
/// <see cref="Guid"/> scalars) to keep module boundaries intact, mirroring the Platform/Organisation modules.
/// </summary>
public sealed class WorkControlDbContext : DbContext
{
    public const string CowSchema = "cow";
    public const string InspectionSchema = "inspection";
    public const string AuditSchema = "audit";

    private readonly ITenantContext _tenantContext;

    public WorkControlDbContext(DbContextOptions<WorkControlDbContext> options)
        : this(options, new UnresolvedTenantContext())
    {
    }

    public WorkControlDbContext(DbContextOptions<WorkControlDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // ---- cow: control of work ----
    public DbSet<WorkRequestEntity> WorkRequests => Set<WorkRequestEntity>();
    public DbSet<JsaTemplateEntity> JsaTemplates => Set<JsaTemplateEntity>();
    public DbSet<JsaTemplateVersionEntity> JsaTemplateVersions => Set<JsaTemplateVersionEntity>();
    public DbSet<JsaTemplateStepEntity> JsaTemplateSteps => Set<JsaTemplateStepEntity>();
    public DbSet<JsaEntity> Jsas => Set<JsaEntity>();
    public DbSet<JsaStepEntity> JsaSteps => Set<JsaStepEntity>();
    public DbSet<JsaStepHazardEntity> JsaStepHazards => Set<JsaStepHazardEntity>();
    public DbSet<PermitTypeEntity> PermitTypes => Set<PermitTypeEntity>();
    public DbSet<PermitTypeVersionEntity> PermitTypeVersions => Set<PermitTypeVersionEntity>();
    public DbSet<PermitChecklistItemEntity> PermitChecklistItems => Set<PermitChecklistItemEntity>();
    public DbSet<PermitEntity> Permits => Set<PermitEntity>();
    public DbSet<PermitWorkerEntity> PermitWorkers => Set<PermitWorkerEntity>();
    public DbSet<PermitChecklistResponseEntity> PermitChecklistResponses => Set<PermitChecklistResponseEntity>();
    public DbSet<PermitApprovalEntity> PermitApprovals => Set<PermitApprovalEntity>();
    public DbSet<GasTestEntity> GasTests => Set<GasTestEntity>();
    public DbSet<WorkExecutionEntity> WorkExecutions => Set<WorkExecutionEntity>();
    public DbSet<WorkMonitoringEntity> WorkMonitoring => Set<WorkMonitoringEntity>();
    public DbSet<IsolationPlanEntity> IsolationPlans => Set<IsolationPlanEntity>();
    public DbSet<IsolationPointEntity> IsolationPoints => Set<IsolationPointEntity>();
    public DbSet<IsolationLockEntity> IsolationLocks => Set<IsolationLockEntity>();
    public DbSet<IsolationVerificationEntity> IsolationVerifications => Set<IsolationVerificationEntity>();

    // ---- inspection ----
    public DbSet<InspectionTemplateEntity> InspectionTemplates => Set<InspectionTemplateEntity>();
    public DbSet<InspectionTemplateVersionEntity> InspectionTemplateVersions => Set<InspectionTemplateVersionEntity>();
    public DbSet<InspectionTemplateSectionEntity> InspectionTemplateSections => Set<InspectionTemplateSectionEntity>();
    public DbSet<InspectionTemplateItemEntity> InspectionTemplateItems => Set<InspectionTemplateItemEntity>();
    public DbSet<InspectionScheduleEntity> InspectionSchedules => Set<InspectionScheduleEntity>();
    public DbSet<InspectionEntity> Inspections => Set<InspectionEntity>();
    public DbSet<InspectionResponseEntity> InspectionResponses => Set<InspectionResponseEntity>();
    public DbSet<InspectionFindingEntity> InspectionFindings => Set<InspectionFindingEntity>();

    // ---- audit ----
    public DbSet<AuditProgramEntity> AuditPrograms => Set<AuditProgramEntity>();
    public DbSet<AuditChecklistTemplateEntity> AuditChecklistTemplates => Set<AuditChecklistTemplateEntity>();
    public DbSet<AuditChecklistItemEntity> AuditChecklistItems => Set<AuditChecklistItemEntity>();
    public DbSet<AuditEntity> Audits => Set<AuditEntity>();
    public DbSet<AuditTeamMemberEntity> AuditTeamMembers => Set<AuditTeamMemberEntity>();
    public DbSet<AuditResponseEntity> AuditResponses => Set<AuditResponseEntity>();
    public DbSet<AuditFindingEntity> AuditFindings => Set<AuditFindingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkControlDbContext).Assembly);
        ApplySnakeCaseColumnNames(modelBuilder);
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
