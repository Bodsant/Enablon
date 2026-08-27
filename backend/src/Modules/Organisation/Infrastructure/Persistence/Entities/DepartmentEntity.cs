using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.departments (001-foundation.sql · Wave 1).
/// Optional parent-child hierarchy scoped to a business unit / site.
/// </summary>
public sealed class DepartmentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Status { get; set; } = default!;
}
