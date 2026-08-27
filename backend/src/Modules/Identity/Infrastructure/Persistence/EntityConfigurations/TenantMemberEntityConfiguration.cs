using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="TenantMemberEntity"/> (<c>iam.tenant_members</c>).</summary>
public sealed class TenantMemberEntityConfiguration : IEntityTypeConfiguration<TenantMemberEntity>
{
    public const string TableName = "tenant_members";
    public const int DisplayNameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<TenantMemberEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.PersonId);
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(DisplayNameMaxLength);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.ActivatedAt);
        builder.Property(e => e.DeactivatedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_tenant_members_tenant_id");
        builder.HasIndex(e => e.UserId).HasDatabaseName("ix_tenant_members_user_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_tenant_members_person_id");
        builder.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique().HasDatabaseName("ix_tenant_members_tenant_id_user_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_tenant_members_tenant_id_status");

        builder.HasMany(e => e.MemberRoles)
            .WithOne(e => e.TenantMember)
            .HasForeignKey(e => e.TenantMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MemberAccessScopes)
            .WithOne(e => e.TenantMember)
            .HasForeignKey(e => e.TenantMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.TemporaryAccessGrants)
            .WithOne(e => e.TenantMember)
            .HasForeignKey(e => e.TenantMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ReviewedAccessReviews)
            .WithOne(e => e.ReviewerMember)
            .HasForeignKey(e => e.ReviewerMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ApprovedTemporaryAccessGrants)
            .WithOne(e => e.ApprovedByMember)
            .HasForeignKey(e => e.ApprovedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}