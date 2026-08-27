using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the <c>org</c> schema (Organisation module).
/// The database schema is owned and versioned by <c>database/ddl</c>; the relational
/// model here is kept aligned with 001-foundation.sql so the schema remains the source
/// of truth while EF provides strongly typed access.
///
/// Cross-schema foreign keys (which point at other modules) are intentionally NOT
/// modelled as EF relationships: they exist as plain <see cref="Guid"/> scalar
/// properties and their referential integrity is enforced by the database DDL. This
/// keeps module boundaries intact (Organisation never references Platform entities).
/// </summary>
public sealed class OrganisationDbContext : DbContext
{
    public const string Schema = "org";

    private readonly ITenantContext _tenantContext;

    public OrganisationDbContext(DbContextOptions<OrganisationDbContext> options)
        : this(options, new UnresolvedTenantContext())
    {
    }

    public OrganisationDbContext(DbContextOptions<OrganisationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();
    public DbSet<BusinessUnitEntity> BusinessUnits => Set<BusinessUnitEntity>();
    public DbSet<SiteEntity> Sites => Set<SiteEntity>();
    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();
    public DbSet<LocationEntity> Locations => Set<LocationEntity>();
    public DbSet<PositionEntity> Positions => Set<PositionEntity>();
    public DbSet<PersonEntity> People => Set<PersonEntity>();
    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganisationDbContext).Assembly);
    }
}
