using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ClassificationReviewEntity"/> (<c>incident.classification_reviews</c>).</summary>
public sealed class ClassificationReviewEntityConfiguration : IEntityTypeConfiguration<ClassificationReviewEntity>
{
    public const string TableName = "classification_reviews";

    public void Configure(EntityTypeBuilder<ClassificationReviewEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IncidentId).IsRequired();
        builder.Property(e => e.ReviewerMemberId).IsRequired();
        builder.Property(e => e.ClassificationJson).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Decision).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ReviewedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_classification_reviews_tenant_id");
    }
}
