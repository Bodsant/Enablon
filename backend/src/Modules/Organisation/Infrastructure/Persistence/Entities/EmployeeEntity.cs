using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.employees (001-foundation.sql · Wave 1).
/// Employment record bound 1:1 to a person and a company.
/// </summary>
public sealed class EmployeeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string EmployeeNumber { get; set; } = default!;
    public Guid CompanyId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerPersonId { get; set; }
    public string EmploymentStatus { get; set; } = default!;
    public string? SourceSystem { get; set; }
    public string? SourceId { get; set; }
}
