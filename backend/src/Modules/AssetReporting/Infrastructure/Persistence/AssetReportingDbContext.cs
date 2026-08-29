using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence;

/// <summary>EF Core DbContext for AssetReporting tables across asset, emergency, reporting and integration schemas.</summary>
public sealed class AssetReportingDbContext : DbContext
{
    public const string DefaultSchema = "asset";

    public AssetReportingDbContext(DbContextOptions<AssetReportingDbContext> options)
        : base(options)
    {
    }

    public AssetReportingDbContext(DbContextOptions<AssetReportingDbContext> options, IAssetReportingDbSchema schema)
        : base(options)
    {
    }

    public DbSet<AssetEntity> Assets => Set<AssetEntity>();
    public DbSet<SafetyRequirementEntity> SafetyRequirements => Set<SafetyRequirementEntity>();
    public DbSet<InspectionEntity> Inspections => Set<InspectionEntity>();
    public DbSet<CertificateEntity> Certificates => Set<CertificateEntity>();
    public DbSet<DefectEntity> Defects => Set<DefectEntity>();
    public DbSet<OperatorAssignmentEntity> OperatorAssignments => Set<OperatorAssignmentEntity>();
    public DbSet<EmergencyPlanEntity> EmergencyPlans => Set<EmergencyPlanEntity>();
    public DbSet<EmergencyPlanRevisionEntity> EmergencyPlanRevisions => Set<EmergencyPlanRevisionEntity>();
    public DbSet<EmergencyTeamMemberEntity> EmergencyTeamMembers => Set<EmergencyTeamMemberEntity>();
    public DbSet<EmergencyEquipmentEntity> EmergencyEquipment => Set<EmergencyEquipmentEntity>();
    public DbSet<EmergencyDrillEntity> EmergencyDrills => Set<EmergencyDrillEntity>();
    public DbSet<EmergencyDrillParticipantEntity> EmergencyDrillParticipants => Set<EmergencyDrillParticipantEntity>();
    public DbSet<EmergencyDrillFindingEntity> EmergencyDrillFindings => Set<EmergencyDrillFindingEntity>();
    public DbSet<KpiDefinitionEntity> KpiDefinitions => Set<KpiDefinitionEntity>();
    public DbSet<KpiVersionEntity> KpiVersions => Set<KpiVersionEntity>();
    public DbSet<ReportDefinitionEntity> ReportDefinitions => Set<ReportDefinitionEntity>();
    public DbSet<ReportScheduleEntity> ReportSchedules => Set<ReportScheduleEntity>();
    public DbSet<ReportExecutionEntity> ReportExecutions => Set<ReportExecutionEntity>();
    public DbSet<IntegrationInterfaceEntity> IntegrationInterfaces => Set<IntegrationInterfaceEntity>();
    public DbSet<IntegrationDataMappingEntity> IntegrationDataMappings => Set<IntegrationDataMappingEntity>();
    public DbSet<IntegrationRunEntity> IntegrationRuns => Set<IntegrationRunEntity>();
    public DbSet<IntegrationMessageEntity> IntegrationMessages => Set<IntegrationMessageEntity>();
    public DbSet<IntegrationReconciliationEntity> IntegrationReconciliations => Set<IntegrationReconciliationEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AssetReportingDbContext).Assembly);
        ApplySnakeCaseColumnNames(builder);
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
