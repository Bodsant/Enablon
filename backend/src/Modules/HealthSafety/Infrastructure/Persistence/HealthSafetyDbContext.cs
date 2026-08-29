using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the HealthSafety module, covering the
/// <c>ppe</c>, <c>health</c>, <c>chemical</c>, <c>environment</c> and
/// <c>sustainability</c> schemas. All entities follow the modular-monolith
/// persistence convention: <see cref="Guid"/> primary keys and a mandatory
/// <c>tenant_id</c> on every table, mapped through <see cref="IHealthSafetyDbSchema"/>
/// at runtime.
///
/// Cross-schema foreign keys (which point at other modules) are intentionally NOT
/// modelled as EF relationships: they exist as plain <see cref="Guid"/> scalar
/// properties and their referential integrity is enforced by the database DDL.
/// </summary>
public sealed class HealthSafetyDbContext : DbContext
{
    private readonly IHealthSafetyDbSchema _schema;

    public HealthSafetyDbContext(DbContextOptions<HealthSafetyDbContext> options, IHealthSafetyDbSchema schema)
        : base(options)
    {
        _schema = schema;
    }

    public DbSet<PpeCatalogEntity> PpeCatalogs => Set<PpeCatalogEntity>();
    public DbSet<PpeInventoryEntity> PpeInventory => Set<PpeInventoryEntity>();
    public DbSet<PpeRequirementEntity> PpeRequirements => Set<PpeRequirementEntity>();
    public DbSet<PpeAssignmentEntity> PpeAssignments => Set<PpeAssignmentEntity>();
    public DbSet<PpeInspectionEntity> PpeInspections => Set<PpeInspectionEntity>();
    public DbSet<PpeReplacementEntity> PpeReplacements => Set<PpeReplacementEntity>();

    public DbSet<HealthProfileEntity> HealthProfiles => Set<HealthProfileEntity>();
    public DbSet<SurveillanceProgramEntity> SurveillancePrograms => Set<SurveillanceProgramEntity>();
    public DbSet<SurveillanceEventEntity> SurveillanceEvents => Set<SurveillanceEventEntity>();
    public DbSet<FitnessStatusEntity> FitnessStatuses => Set<FitnessStatusEntity>();
    public DbSet<ExposureLinkEntity> ExposureLinks => Set<ExposureLinkEntity>();
    public DbSet<HealthFollowupEntity> HealthFollowups => Set<HealthFollowupEntity>();

    public DbSet<ChemicalProductEntity> ChemicalProducts => Set<ChemicalProductEntity>();
    public DbSet<ChemicalInventoryEntity> ChemicalInventory => Set<ChemicalInventoryEntity>();
    public DbSet<SdsRevisionEntity> SdsRevisions => Set<SdsRevisionEntity>();
    public DbSet<ChemicalStorageInspectionEntity> ChemicalStorageInspections => Set<ChemicalStorageInspectionEntity>();
    public DbSet<ChemicalExposureControlEntity> ChemicalExposureControls => Set<ChemicalExposureControlEntity>();

    public DbSet<EnvironmentParameterEntity> EnvironmentParameters => Set<EnvironmentParameterEntity>();
    public DbSet<EnvironmentSourceEntity> EnvironmentSources => Set<EnvironmentSourceEntity>();
    public DbSet<MonitoringRecordEntity> MonitoringRecords => Set<MonitoringRecordEntity>();
    public DbSet<EnvironmentMeasurementEntity> EnvironmentMeasurements => Set<EnvironmentMeasurementEntity>();
    public DbSet<WasteRecordEntity> WasteRecords => Set<WasteRecordEntity>();
    public DbSet<ResourceUsageEntity> ResourceUsage => Set<ResourceUsageEntity>();
    public DbSet<EnvironmentTargetEntity> EnvironmentTargets => Set<EnvironmentTargetEntity>();

    public DbSet<IndicatorDefinitionEntity> IndicatorDefinitions => Set<IndicatorDefinitionEntity>();
    public DbSet<FactorVersionEntity> FactorVersions => Set<FactorVersionEntity>();
    public DbSet<SustainabilityMeasurementEntity> SustainabilityMeasurements => Set<SustainabilityMeasurementEntity>();
    public DbSet<SustainabilityTargetEntity> SustainabilityTargets => Set<SustainabilityTargetEntity>();

    public const string DefaultPpeSchema = "ppe";
    public const string DefaultHealthSchema = "health";
    public const string DefaultChemicalSchema = "chemical";
    public const string DefaultEnvironmentSchema = "environment";
    public const string DefaultSustainabilitySchema = "sustainability";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(HealthSafetyDbContext).Assembly);
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