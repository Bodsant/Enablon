using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="TemporaryAccessGrantEntity"/> (<c>iam.temporary_access_grants</c>).</summary>
public sealed class TemporaryAccessGrantEntityConfiguration : IEntityTypeConfiguration<TemporaryAccessGrantEntity>
{
    public const string TableName = "temporary_access_grants";

    public void Configure(EntityTypeBuilder<TemporaryAccessGrantEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.AccessScopeId).IsRequired();
        builder.Property(e => e.RoleId);
        builder.Property(e => e.ApprovedByMemberId).IsRequired();
        builder.Property(e => e.Reason).IsRequired(); // text, unbounded
        builder.Property(e => e.ValidFrom).IsRequired();
        builder.Property(e => e.ValidUntil).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_temporary_access_grants_tenant_id");
        builder.HasIndex(e => e.TenantMemberId).HasDatabaseName("ix_temporary_access_grants_tenant_member_id");
        builder.HasIndex(e => e.AccessScopeId).HasDatabaseName("ix_temporary_access_grants_access_scope_id");
        builder.HasIndex(e => e.RoleId).HasDatabaseName("ix_temporary_access_grants_role_id");
        builder.HasIndex(e => new { e.TenantId, e.ValidUntil }).HasDatabaseName("ix_temporary_access_grants_tenant_id_valid_until");

        builder.HasOne(e => e.TenantMember)
            .WithMany(e => e.TemporaryAccessGrants)
            .HasForeignKey(e => e.TenantMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AccessScope)
            .WithMany(e => e.TemporaryAccessGrants)
            .HasForeignKey(e => e.AccessScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByMember)
            .WithMany(e => e.ApprovedTemporaryAccessGrants)
            .HasForeignKey(e => e.ApprovedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}