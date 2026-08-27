using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="MemberRoleEntity"/> (<c>iam.member_roles</c>).</summary>
public sealed class MemberRoleEntityConfiguration : IEntityTypeConfiguration<MemberRoleEntity>
{
    public const string TableName = "member_roles";

    public void Configure(EntityTypeBuilder<MemberRoleEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.RoleId).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.TenantMemberId, e.RoleId }).IsUnique().HasDatabaseName("ix_member_roles_tenant_id_tenant_member_id_role_id");
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_member_roles_tenant_id");
        builder.HasIndex(e => e.RoleId).HasDatabaseName("ix_member_roles_role_id");
    }
}