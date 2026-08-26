using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Organisation.Domain;

public class Company : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BusinessUnit : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Site : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public string? Address { get; set; }
    public string? Timezone { get; set; } // IANA timezone
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Department : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? SiteId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Location : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? SiteId { get; set; }
    public string? Description { get; set; }
}

public class Position : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? DepartmentId { get; set; }
    public int? Level { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Person : TenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Employee : TenantEntity
{
    public Guid PersonId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? SiteId { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = true;
}
