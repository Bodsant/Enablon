using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the SafetyRisk module covering the <c>document</c>, <c>safety</c>,
/// <c>risk</c>, <c>incident</c> and <c>capa</c> schemas. All entities follow the modular-monolith persistence
/// convention: <see cref="Guid"/> primary keys and a mandatory <c>tenant_id</c> on every table. Cross-schema
/// foreign keys (which point at other modules or at other tables) are intentionally NOT modelled as EF
/// relationships; they exist as plain scalar properties and their referential integrity is enforced by the DDL.
/// The database schema is owned and versioned by <c>database/ddl</c> while EF provides strongly typed access.
/// </summary>
public sealed class SafetyRiskDbContext : DbContext
{
    public SafetyRiskDbContext(DbContextOptions<SafetyRiskDbContext> options)
        : base(options)
    {
    }

    // document
    public DbSet<ControlledDocumentEntity> ControlledDocuments => Set<ControlledDocumentEntity>();
    public DbSet<RevisionEntity> Revisions => Set<RevisionEntity>();
    public DbSet<AcknowledgementEntity> Acknowledgements => Set<AcknowledgementEntity>();

    // safety
    public DbSet<ObservationEntity> Observations => Set<ObservationEntity>();

    // risk
    public DbSet<HazardEntity> Hazards => Set<HazardEntity>();
    public DbSet<RiskMatrixVersionEntity> MatrixVersions => Set<RiskMatrixVersionEntity>();
    public DbSet<RiskMatrixCellEntity> MatrixCells => Set<RiskMatrixCellEntity>();
    public DbSet<RiskRegisterEntity> Registers => Set<RiskRegisterEntity>();
    public DbSet<RiskAssessmentEntity> Assessments => Set<RiskAssessmentEntity>();
    public DbSet<RiskControlEntity> Controls => Set<RiskControlEntity>();
    public DbSet<RiskReviewEntity> Reviews => Set<RiskReviewEntity>();

    // incident
    public DbSet<IncidentEntity> Incidents => Set<IncidentEntity>();
    public DbSet<InvolvedPersonEntity> InvolvedPeople => Set<InvolvedPersonEntity>();
    public DbSet<InvestigationEntity> Investigations => Set<InvestigationEntity>();
    public DbSet<InvestigationTeamMemberEntity> InvestigationTeam => Set<InvestigationTeamMemberEntity>();
    public DbSet<RootCauseEntity> RootCauses => Set<RootCauseEntity>();
    public DbSet<ClassificationReviewEntity> ClassificationReviews => Set<ClassificationReviewEntity>();

    // capa
    public DbSet<CapaActionEntity> Actions => Set<CapaActionEntity>();
    public DbSet<CapaSourceEntity> Sources => Set<CapaSourceEntity>();
    public DbSet<CapaUpdateEntity> Updates => Set<CapaUpdateEntity>();
    public DbSet<CapaVerificationEntity> Verifications => Set<CapaVerificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SafetyRiskDbContext).Assembly);
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
