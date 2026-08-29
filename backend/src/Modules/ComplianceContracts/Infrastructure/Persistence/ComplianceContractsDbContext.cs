using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the Compliance &amp; Contractor Management module, spanning
/// the <c>compliance</c>, <c>contractor</c> and <c>training</c> schemas. Primary keys are
/// <see cref="Guid"/> and every table carries a mandatory <c>tenant_id</c>, following the
/// modular-monolith persistence convention.
/// Cross-schema foreign keys (which point at other modules or at other tables) are intentionally
/// NOT modelled as EF relationships: they exist as plain <see cref="Guid"/> scalar properties and
/// their referential integrity is enforced by the database DDL. The database schema is owned and
/// versioned by <c>database/ddl</c> while EF provides strongly typed access.
/// </summary>
public sealed class ComplianceContractsDbContext : DbContext
{
    public const string ComplianceSchema = "compliance";
    public const string ContractorSchema = "contractor";
    public const string TrainingSchema = "training";

    public ComplianceContractsDbContext(DbContextOptions<ComplianceContractsDbContext> options)
        : base(options)
    {
    }

    // ---- compliance ----
    public DbSet<LegalSourceEntity> LegalSources => Set<LegalSourceEntity>();
    public DbSet<LegalSourceVersionEntity> LegalSourceVersions => Set<LegalSourceVersionEntity>();
    public DbSet<ObligationEntity> Obligations => Set<ObligationEntity>();
    public DbSet<ObligationApplicabilityEntity> ObligationApplicabilities => Set<ObligationApplicabilityEntity>();
    public DbSet<EvaluationEntity> Evaluations => Set<EvaluationEntity>();
    public DbSet<GapEntity> Gaps => Set<GapEntity>();

    // ---- contractor ----
    public DbSet<ContractorCompanyEntity> ContractorCompanies => Set<ContractorCompanyEntity>();
    public DbSet<ContractEntity> Contracts => Set<ContractEntity>();
    public DbSet<ContractorWorkerEntity> ContractorWorkers => Set<ContractorWorkerEntity>();
    public DbSet<ContractorDocumentEntity> ContractorDocuments => Set<ContractorDocumentEntity>();
    public DbSet<QualificationEvaluationEntity> QualificationEvaluations => Set<QualificationEvaluationEntity>();
    public DbSet<PerformancePeriodEntity> PerformancePeriods => Set<PerformancePeriodEntity>();

    // ---- training ----
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<CompetencyEntity> Competencies => Set<CompetencyEntity>();
    public DbSet<PositionRequirementEntity> PositionRequirements => Set<PositionRequirementEntity>();
    public DbSet<TrainingSessionEntity> TrainingSessions => Set<TrainingSessionEntity>();
    public DbSet<SessionParticipantEntity> SessionParticipants => Set<SessionParticipantEntity>();
    public DbSet<WorkerCompetencyEntity> WorkerCompetencies => Set<WorkerCompetencyEntity>();
    public DbSet<CertificationEntity> Certifications => Set<CertificationEntity>();
    public DbSet<EligibilityCheckEntity> EligibilityChecks => Set<EligibilityCheckEntity>();
    public DbSet<EligibilityOverrideEntity> EligibilityOverrides => Set<EligibilityOverrideEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ComplianceContractsDbContext).Assembly);
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