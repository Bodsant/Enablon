using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskAssessmentEntity"/> (<c>risk.assessments</c>).</summary>
public sealed class RiskAssessmentEntityConfiguration : IEntityTypeConfiguration<RiskAssessmentEntity>
{
    public const string TableName = "assessments";

    public void Configure(EntityTypeBuilder<RiskAssessmentEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RiskRegisterId).IsRequired();
        builder.Property(e => e.MatrixVersionId).IsRequired();
        builder.Property(e => e.AssessmentType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.SequenceNumber).IsRequired();
        builder.Property(e => e.LikelihoodValue).IsRequired();
        builder.Property(e => e.SeverityValue).IsRequired();
        builder.Property(e => e.RiskScore).IsRequired();
        builder.Property(e => e.RiskLevelCode).IsRequired().HasMaxLength(30);
        builder.Property(e => e.AssessedByMemberId).IsRequired();
        builder.Property(e => e.AssessedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_assessments_tenant_id");
    }
}
