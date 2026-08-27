using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RolePermissionEntity"/> (<c>iam.role_permissions</c>).</summary>
public sealed class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    public const string TableName = "role_permissions";

    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RoleId).IsRequired();
        builder.Property(e => e.PermissionId).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.RoleId, e.PermissionId }).IsUnique().HasDatabaseName("ix_role_permissions_tenant_id_role_id_permission_id");
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_role_permissions_tenant_id");
        builder.HasIndex(e => e.PermissionId).HasDatabaseName("ix_role_permissions_permission_id");

        builder.HasOne(e => e.Role)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permission)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}