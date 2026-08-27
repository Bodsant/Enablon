using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.access_scopes (001-foundation.sql · Wave 1). Scope of access within org hierarchy.
/// Cross-schema ids are kept as scalar Guid properties.
/// </summary>
public sealed class AccessScopeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ScopeType { get; set; } = default!;
    public Guid? CompanyId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? ContractorCompanyId { get; set; }
    public Guid? DataClassificationId { get; set; }
}
