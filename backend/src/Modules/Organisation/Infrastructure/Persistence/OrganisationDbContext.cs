using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Organisation.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
        ApplySnakeCaseColumnNames(modelBuilder);
        base.OnModelCreating(modelBuilder);
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

public interface IOrganisationDbSchema
{
    string Schema { get; }
}

public sealed class OrganisationDbSchema : IOrganisationDbSchema
{
    public string Schema => "org";
}