using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the Identity module. Backed by PostgreSQL, schema <c>iam</c>.
/// ASP.NET Core Identity types (<see cref="Microsoft.AspNetCore.Identity.IdentityUser{TKey}"/> and friends)
/// are mapped to the <c>iam.users</c> table via <see cref="UserEntity"/> and custom entity configurations.
/// </summary>
public sealed class EhsmsIdentityDbContext
    : IdentityDbContext<
        UserEntity,
        RoleEntity,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
{
    private readonly string _schema;

    public EhsmsIdentityDbContext(DbContextOptions<EhsmsIdentityDbContext> options, IDbContextSchema schema)
        : base(options)
    {
        _schema = schema.Schema;
    }

    public DbSet<TenantMemberEntity> TenantMembers => Set<TenantMemberEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
    public DbSet<MemberRoleEntity> MemberRoles => Set<MemberRoleEntity>();
    public DbSet<AccessScopeEntity> AccessScopes => Set<AccessScopeEntity>();
    public DbSet<MemberAccessScopeEntity> MemberAccessScopes => Set<MemberAccessScopeEntity>();
    public DbSet<TemporaryAccessGrantEntity> TemporaryAccessGrants => Set<TemporaryAccessGrantEntity>();
    public DbSet<AccessReviewEntity> AccessReviews => Set<AccessReviewEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(EhsmsIdentityDbContext).Assembly);

        // ASP.NET Core Identity joins live in the iam schema like every other Identity table.
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", _schema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", _schema);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", _schema);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", _schema);
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", _schema);
    }
}