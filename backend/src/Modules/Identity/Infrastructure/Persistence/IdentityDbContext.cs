using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the <c>iam</c> schema (Identity module).
/// Relational model is aligned to database/ddl/001-foundation.sql. Cross-schema
/// foreign keys (tenant, org, platform, contractor) are kept as plain scalar
/// Guid properties; referential integrity is enforced by the database DDL so the
/// Identity module never references other modules' entity types.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public const string Schema = "iam";

    private readonly ITenantContext _tenantContext;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : this(options, new UnresolvedTenantContext())
    {
    }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<TenantMemberEntity> TenantMembers => Set<TenantMemberEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
    public DbSet<MemberRoleEntity> MemberRoles => Set<MemberRoleEntity>();
    public DbSet<AccessScopeEntity> AccessScopes => Set<AccessScopeEntity>();
    public DbSet<MemberAccessScopeEntity> MemberAccessScopes => Set<MemberAccessScopeEntity>();
    public DbSet<TemporaryAccessGrantEntity> TemporaryAccessGrants => Set<TemporaryAccessGrantEntity>();
    public DbSet<AccessReviewEntity> AccessReviews => Set<AccessReviewEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
