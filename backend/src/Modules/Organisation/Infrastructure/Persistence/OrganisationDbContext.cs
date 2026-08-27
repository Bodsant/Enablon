using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Organisation.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence;

public sealed class OrganisationDbContext : DbContext
{
    public OrganisationDbContext(DbContextOptions<OrganisationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganisationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public interface IOrganisationDbSchema
{
    string Schema { get; }
}

public sealed class OrganisationDbSchema : IOrganisationDbSchema
{
    public string Schema => "org";
}