using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EligibilityCheckEntity"/> (<c>training.eligibility_checks</c>).</summary>
public sealed class EligibilityCheckEntityConfiguration : IEntityTypeConfiguration<EligibilityCheckEntity>
{
    public const string TableName = "eligibility_checks";

    public void Configure(EntityTypeBuilder<EligibilityCheckEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.TargetRecordId).IsRequired();
        builder.Property(e => e.Result).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CheckedAt).IsRequired();
        builder.Property(e => e.DetailsJson).HasColumnType("jsonb");

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_eligibility_checks_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_eligibility_checks_record_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_eligibility_checks_person_id");
        builder.HasIndex(e => e.TargetRecordId).HasDatabaseName("ix_eligibility_checks_target_record_id");
    }
}
