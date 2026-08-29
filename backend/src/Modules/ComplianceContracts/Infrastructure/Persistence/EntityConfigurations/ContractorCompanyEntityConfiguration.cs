using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ContractorCompanyEntity"/> (<c>contractor.companies</c>).</summary>
public sealed class ContractorCompanyEntityConfiguration : IEntityTypeConfiguration<ContractorCompanyEntity>
{
    public const string TableName = "companies";

    public void Configure(EntityTypeBuilder<ContractorCompanyEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.VendorCode).HasMaxLength(60);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(250);
        builder.Property(e => e.TaxIdentifier).HasMaxLength(100);
        builder.Property(e => e.QualificationStatus).HasMaxLength(30);
        builder.Property(e => e.EligibilityStatus).HasMaxLength(30);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_companies_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_companies_record_id");
    }
}
