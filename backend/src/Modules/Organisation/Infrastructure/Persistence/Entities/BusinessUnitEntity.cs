using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.business_units (001-foundation.sql · Wave 1).
/// Hierarchical units under a company.
/// </summary>
public sealed class BusinessUnitEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ParentBusinessUnitId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Status { get; set; } = default!;
}
