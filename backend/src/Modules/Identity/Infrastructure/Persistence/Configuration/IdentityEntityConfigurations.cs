using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Configuration;

/// <summary>
/// Relational mapping for the iam schema tables, aligned to database/ddl/001-foundation.sql.
/// </summary>
public sealed class IdentityEntityConfigurations : IEntityTypeConfiguration<UserEntity>,
    IEntityTypeConfiguration<TenantMemberEntity>,
    IEntityTypeConfiguration<RoleEntity>,
    IEntityTypeConfiguration<PermissionEntity>,
    IEntityTypeConfiguration<RolePermissionEntity>,
    IEntityTypeConfiguration<MemberRoleEntity>,
    IEntityTypeConfiguration<AccessScopeEntity>,
    IEntityTypeConfiguration<MemberAccessScopeEntity>,
    IEntityTypeConfiguration<TemporaryAccessGrantEntity>,
    IEntityTypeConfiguration<AccessReviewEntity>,
    IEntityTypeConfiguration<RefreshTokenEntity>
{
    private const string IamSchema = IdentityDbContext.Schema;

    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Email).HasMaxLength(254).IsRequired();
        builder.Property(e => e.NormalizedEmail).HasMaxLength(254).IsRequired();
        builder.Property(e => e.IdentityProvider).HasMaxLength(80);
        builder.Property(e => e.ExternalSubject).HasMaxLength(200);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.NormalizedEmail).IsUnique();
    }

    public void Configure(EntityTypeBuilder<TenantMemberEntity> builder)
    {
        builder.ToTable("tenant_members", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId);
        // person_id -> org.people (cross-schema, scalar)
    }

    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("roles", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.ScopeType).HasMaxLength(20).IsRequired();
    }

    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("permissions", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Module).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(50).IsRequired();
    }

    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable("role_permissions", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId);
        builder.HasOne<PermissionEntity>().WithMany().HasForeignKey(e => e.PermissionId);
    }

    public void Configure(EntityTypeBuilder<MemberRoleEntity> builder)
    {
        builder.ToTable("member_roles", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.HasOne<TenantMemberEntity>().WithMany().HasForeignKey(e => e.TenantMemberId);
        builder.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId);
    }

    public void Configure(EntityTypeBuilder<AccessScopeEntity> builder)
    {
        builder.ToTable("access_scopes", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.ScopeType).HasMaxLength(30).IsRequired();
        // cross-schema ids (company/business_unit/site/department/location/contractor/data_classification) -> scalar
    }

    public void Configure(EntityTypeBuilder<MemberAccessScopeEntity> builder)
    {
        builder.ToTable("member_access_scopes", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.HasOne<TenantMemberEntity>().WithMany().HasForeignKey(e => e.TenantMemberId);
        builder.HasOne<AccessScopeEntity>().WithMany().HasForeignKey(e => e.AccessScopeId);
    }

    public void Configure(EntityTypeBuilder<TemporaryAccessGrantEntity> builder)
    {
        builder.ToTable("temporary_access_grants", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Reason).IsRequired();
        builder.HasOne<TenantMemberEntity>().WithMany().HasForeignKey(e => e.TenantMemberId);
        builder.HasOne<AccessScopeEntity>().WithMany().HasForeignKey(e => e.AccessScopeId);
        builder.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId);
        builder.HasOne<TenantMemberEntity>().WithMany().HasForeignKey(e => e.ApprovedByMemberId);
    }

    public void Configure(EntityTypeBuilder<AccessReviewEntity> builder)
    {
        builder.ToTable("access_reviews", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.HasOne<TenantMemberEntity>().WithMany().HasForeignKey(e => e.ReviewerMemberId);
    }

    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("refresh_tokens", IamSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.TokenHash).HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.TokenHash).IsUnique();
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId);
        builder.HasOne<RefreshTokenEntity>().WithMany().HasForeignKey(e => e.ReplacedByTokenId);
    }
}
