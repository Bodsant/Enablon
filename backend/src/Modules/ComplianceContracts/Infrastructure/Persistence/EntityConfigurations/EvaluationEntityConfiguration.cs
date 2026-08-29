using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EvaluationEntity"/> (<c>compliance.evaluations</c>).</summary>
public sealed class EvaluationEntityConfiguration : IEntityTypeConfiguration<EvaluationEntity>
{
    public const string TableName = "evaluations";

    public void Configure(EntityTypeBuilder<EvaluationEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ObligationId).IsRequired();
        builder.Property(e => e.EvaluationPeriodStart);
        builder.Property(e => e.EvaluationPeriodEnd);
        builder.Property(e => e.ComplianceStatus).IsRequired().HasMaxLength(30);
        builder.Property(e => e.EvaluatedByMemberId).IsRequired();
        builder.Property(e => e.EvaluatedAt).IsRequired();
        builder.Property(e => e.Comment);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_evaluations_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_evaluations_record_id");
        builder.HasIndex(e => e.ObligationId).HasDatabaseName("ix_evaluations_obligation_id");
        builder.HasIndex(e => e.EvaluatedByMemberId).HasDatabaseName("ix_evaluations_evaluated_by_member_id");
    }
}
