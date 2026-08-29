using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="GapEntity"/> (<c>compliance.gaps</c>).</summary>
public sealed class GapEntityConfiguration : IEntityTypeConfiguration<GapEntity>
{
    public const string TableName = "gaps";

    public void Configure(EntityTypeBuilder<GapEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.EvaluationId).IsRequired();
        builder.Property(e => e.GapDescription).IsRequired();
        builder.Property(e => e.Severity).HasMaxLength(30);
        builder.Property(e => e.OwnerMemberId);
        builder.Property(e => e.TargetDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_gaps_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_gaps_record_id");
        builder.HasIndex(e => e.EvaluationId).HasDatabaseName("ix_gaps_evaluation_id");
        builder.HasIndex(e => e.OwnerMemberId).HasDatabaseName("ix_gaps_owner_member_id");
    }
}
