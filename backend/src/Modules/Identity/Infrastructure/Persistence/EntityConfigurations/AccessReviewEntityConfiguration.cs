using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AccessReviewEntity"/> (<c>iam.access_reviews</c>).</summary>
public sealed class AccessReviewEntityConfiguration : IEntityTypeConfiguration<AccessReviewEntity>
{
    public const string TableName = "access_reviews";
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<AccessReviewEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ReviewPeriodStart).IsRequired();
        builder.Property(e => e.ReviewPeriodEnd).IsRequired();
        builder.Property(e => e.ReviewerMemberId).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_access_reviews_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.ReviewerMemberId }).HasDatabaseName("ix_access_reviews_tenant_id_reviewer_member_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_access_reviews_tenant_id_status");
        builder.HasIndex(e => new { e.TenantId, e.ReviewPeriodStart, e.ReviewPeriodEnd }).HasDatabaseName("ix_access_reviews_tenant_id_period");

        builder.HasOne(e => e.ReviewerMember)
            .WithMany(e => e.ReviewedAccessReviews)
            .HasForeignKey(e => e.ReviewerMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}