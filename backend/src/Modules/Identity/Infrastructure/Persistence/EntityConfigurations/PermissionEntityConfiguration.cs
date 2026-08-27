using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermissionEntity"/> (<c>iam.permissions</c>).</summary>
public sealed class PermissionEntityConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public const string TableName = "permissions";
    public const int CodeMaxLength = 100;
    public const int ModuleMaxLength = 50;
    public const int ActionMaxLength = 50;

    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Module).IsRequired().HasMaxLength(ModuleMaxLength);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(ActionMaxLength);
        builder.Property(e => e.Description); // text, unbounded

        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique().HasDatabaseName("ix_permissions_tenant_id_code");
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permissions_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.Module, e.Action }).HasDatabaseName("ix_permissions_tenant_id_module_action");
    }
}