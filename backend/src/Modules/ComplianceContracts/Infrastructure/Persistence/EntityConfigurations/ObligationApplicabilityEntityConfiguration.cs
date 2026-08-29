using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ObligationApplicabilityEntity"/> (<c>compliance.obligation_applicability</c>).</summary>
public sealed class ObligationApplicabilityEntityConfiguration : IEntityTypeConfiguration<ObligationApplicabilityEntity>
{
    public const string TableName = "obligation_applicability";

    public void Configure(EntityTypeBuilder<ObligationApplicabilityEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ObligationId).IsRequired();
        builder.Property(e => e.CompanyId);
        builder.Property(e => e.BusinessUnitId);
        builder.Property(e => e.SiteId);
        builder.Property(e => e.ApplicabilityStatus).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Rationale);
        builder.Property(e => e.AssessedByMemberId).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_obligation_applicability_tenant_id");
        builder.HasIndex(e => e.ObligationId).HasDatabaseName("ix_obligation_applicability_obligation_id");
        builder.HasIndex(e => e.CompanyId).HasDatabaseName("ix_obligation_applicability_company_id");
        builder.HasIndex(e => e.BusinessUnitId).HasDatabaseName("ix_obligation_applicability_business_unit_id");
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_obligation_applicability_site_id");
        builder.HasIndex(e => e.AssessedByMemberId).HasDatabaseName("ix_obligation_applicability_assessed_by_member_id");
    }
}
