using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ContractorWorkerEntity"/> (<c>contractor.workers</c>).</summary>
public sealed class ContractorWorkerEntityConfiguration : IEntityTypeConfiguration<ContractorWorkerEntity>
{
    public const string TableName = "workers";

    public void Configure(EntityTypeBuilder<ContractorWorkerEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.ContractorCompanyId).IsRequired();
        builder.Property(e => e.WorkerNumber).HasMaxLength(60);
        builder.Property(e => e.PositionName).HasMaxLength(150);
        builder.Property(e => e.EligibilityStatus).HasMaxLength(30);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workers_tenant_id");
        builder.HasIndex(e => e.PersonId).IsUnique().HasDatabaseName("ix_workers_person_id");
        builder.HasIndex(e => e.ContractorCompanyId).HasDatabaseName("ix_workers_contractor_company_id");
    }
}
