using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.locations (001-foundation.sql · Wave 1).
/// Physical/abstract location under a site, optionally parented.
/// </summary>
public sealed class LocationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? LocationType { get; set; }
    public string Status { get; set; } = default!;
}
