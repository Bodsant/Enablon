using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ContractEntity"/> (<c>contractor.contracts</c>).</summary>
public sealed class ContractEntityConfiguration : IEntityTypeConfiguration<ContractEntity>
{
    public const string TableName = "contracts";

    public void Configure(EntityTypeBuilder<ContractEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ContractorCompanyId).IsRequired();
        builder.Property(e => e.ContractNumber).HasMaxLength(80);
        builder.Property(e => e.StartDate);
        builder.Property(e => e.EndDate);
        builder.Property(e => e.ContractStatus).HasMaxLength(30);
        builder.Property(e => e.ProcurementSourceId).HasMaxLength(100);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_contracts_tenant_id");
        builder.HasIndex(e => e.ContractorCompanyId).HasDatabaseName("ix_contracts_contractor_company_id");
        builder.HasIndex(e => e.ProcurementSourceId).HasDatabaseName("ix_contracts_procurement_source_id");
    }
}
