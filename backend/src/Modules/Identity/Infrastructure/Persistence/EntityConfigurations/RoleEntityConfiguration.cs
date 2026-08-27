using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RoleEntity"/> (<c>iam.roles</c>).</summary>
public sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public const string TableName = "roles";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 120;
    public const int ScopeTypeMaxLength = 20;

    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.ScopeType).IsRequired().HasMaxLength(ScopeTypeMaxLength);
        builder.Property(e => e.IsSystem).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique().HasDatabaseName("ix_roles_tenant_id_code");
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_roles_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.ScopeType }).HasDatabaseName("ix_roles_tenant_id_scope_type");
    }
}