using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="MemberAccessScopeEntity"/> (<c>iam.member_access_scopes</c>).</summary>
public sealed class MemberAccessScopeEntityConfiguration : IEntityTypeConfiguration<MemberAccessScopeEntity>
{
    public const string TableName = "member_access_scopes";

    public void Configure(EntityTypeBuilder<MemberAccessScopeEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.AccessScopeId).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.TenantMemberId, e.AccessScopeId }).IsUnique().HasDatabaseName("ix_member_access_scopes_tenant_id_tenant_member_id_access_scope_id");
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_member_access_scopes_tenant_id");
        builder.HasIndex(e => e.AccessScopeId).HasDatabaseName("ix_member_access_scopes_access_scope_id");
    }
}