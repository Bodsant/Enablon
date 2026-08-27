namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

public sealed class Company
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Status { get; set; } = null!;
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public sealed class BusinessUnit
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ParentBusinessUnitId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public sealed class Site
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string Timezone { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class Department
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public sealed class Location
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? LocationType { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class Position
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class Person
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PersonType { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = null!;
    public Guid? DataClassificationId { get; set; }
}

public sealed class Employee
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerPersonId { get; set; }
    public string EmploymentStatus { get; set; } = null!;
    public string? SourceSystem { get; set; }
    public string? SourceId { get; set; }
}