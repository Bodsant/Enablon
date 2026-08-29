using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EligibilityOverrideEntity"/> (<c>training.eligibility_overrides</c>).</summary>
public sealed class EligibilityOverrideEntityConfiguration : IEntityTypeConfiguration<EligibilityOverrideEntity>
{
    public const string TableName = "eligibility_overrides";

    public void Configure(EntityTypeBuilder<EligibilityOverrideEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.EligibilityCheckId).IsRequired();
        builder.Property(e => e.ApprovedByMemberId).IsRequired();
        builder.Property(e => e.Reason).IsRequired();
        builder.Property(e => e.ValidUntil);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_eligibility_overrides_tenant_id");
        builder.HasIndex(e => e.EligibilityCheckId).HasDatabaseName("ix_eligibility_overrides_eligibility_check_id");
        builder.HasIndex(e => e.ApprovedByMemberId).HasDatabaseName("ix_eligibility_overrides_approved_by_member_id");
    }
}
