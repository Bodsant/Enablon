using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskReviewEntity"/> (<c>risk.reviews</c>).</summary>
public sealed class RiskReviewEntityConfiguration : IEntityTypeConfiguration<RiskReviewEntity>
{
    public const string TableName = "reviews";

    public void Configure(EntityTypeBuilder<RiskReviewEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RiskRegisterId).IsRequired();
        builder.Property(e => e.ReviewedByMemberId).IsRequired();
        builder.Property(e => e.ReviewedAt).IsRequired();
        builder.Property(e => e.Decision).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Comment);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_reviews_tenant_id");
    }
}
