using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.EntityConfigurations;

public sealed class CompanyEntityConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("companies", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.LegalName).HasMaxLength(250);
        b.Property(x => x.RegistrationNumber).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.EffectiveFrom);
        b.Property(x => x.EffectiveTo);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public sealed class BusinessUnitEntityConfiguration : IEntityTypeConfiguration<BusinessUnit>
{
    public void Configure(EntityTypeBuilder<BusinessUnit> b)
    {
        b.ToTable("business_units", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.CompanyId).IsRequired();
        b.Property(x => x.ParentBusinessUnitId);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.CompanyId);
        b.HasIndex(x => x.ParentBusinessUnitId);
    }
}

public sealed class SiteEntityConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> b)
    {
        b.ToTable("sites", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.CompanyId).IsRequired();
        b.Property(x => x.BusinessUnitId);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Address);
        b.Property(x => x.Timezone).HasMaxLength(60).IsRequired();
        b.Property(x => x.Latitude).HasPrecision(10, 7);
        b.Property(x => x.Longitude).HasPrecision(10, 7);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.CompanyId);
        b.HasIndex(x => x.BusinessUnitId);
    }
}

public sealed class DepartmentEntityConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("departments", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.BusinessUnitId);
        b.Property(x => x.SiteId);
        b.Property(x => x.ParentDepartmentId);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.BusinessUnitId);
        b.HasIndex(x => x.SiteId);
        b.HasIndex(x => x.ParentDepartmentId);
    }
}

public sealed class LocationEntityConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> b)
    {
        b.ToTable("locations", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.SiteId).IsRequired();
        b.Property(x => x.ParentLocationId);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.LocationType).HasMaxLength(60);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.SiteId);
        b.HasIndex(x => x.ParentLocationId);
    }
}

public sealed class PositionEntityConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> b)
    {
        b.ToTable("positions", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.TenantId);
    }
}

public sealed class PersonEntityConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("people", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.PersonType).HasMaxLength(30).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(254);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.DataClassificationId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.DataClassificationId);
    }
}

public sealed class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("employees", "org");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.PersonId).IsRequired();
        b.Property(x => x.EmployeeNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.CompanyId).IsRequired();
        b.Property(x => x.DepartmentId);
        b.Property(x => x.PositionId);
        b.Property(x => x.ManagerPersonId);
        b.Property(x => x.EmploymentStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.SourceSystem).HasMaxLength(60);
        b.Property(x => x.SourceId).HasMaxLength(100);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.PersonId).IsUnique();
        b.HasIndex(x => x.CompanyId);
        b.HasIndex(x => x.DepartmentId);
        b.HasIndex(x => x.PositionId);
        b.HasIndex(x => x.ManagerPersonId);
    }
}