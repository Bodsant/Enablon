using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkerCompetencyEntity"/> (<c>training.worker_competencies</c>).</summary>
public sealed class WorkerCompetencyEntityConfiguration : IEntityTypeConfiguration<WorkerCompetencyEntity>
{
    public const string TableName = "worker_competencies";

    public void Configure(EntityTypeBuilder<WorkerCompetencyEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.CompetencyId).IsRequired();
        builder.Property(e => e.Level).HasMaxLength(30);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ValidFrom);
        builder.Property(e => e.ValidUntil);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_worker_competencies_tenant_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_worker_competencies_person_id");
        builder.HasIndex(e => e.CompetencyId).HasDatabaseName("ix_worker_competencies_competency_id");
    }
}
