using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence;

/// <summary>EF Core context for the custom IAM tables defined by the EHSMS DBML.</summary>
public sealed class EhsmsIdentityDbContext : DbContext
{
    private readonly string _schema;

    public EhsmsIdentityDbContext(DbContextOptions<EhsmsIdentityDbContext> options, IDbContextSchema schema)
        : base(options)
    {
        _schema = schema.Schema;
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
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
        builder.ApplyConfigurationsFromAssembly(typeof(EhsmsIdentityDbContext).Assembly);
        ApplySnakeCaseColumnNames(builder);
        base.OnModelCreating(builder);
    }

    private static void ApplySnakeCaseColumnNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
            property.SetColumnName(ToSnakeCase(property.Name));
    }

    private static string ToSnakeCase(string name)
    {
        var result = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0) result.Append('_');
            result.Append(char.ToLowerInvariant(name[i]));
        }
        return result.ToString();
    }
}
