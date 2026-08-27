using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.people (001-foundation.sql · Wave 1).
/// A natural person (employee, contractor, visitor...) within a tenant.
/// </summary>
public sealed class PersonEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PersonType { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = default!;
    public Guid? DataClassificationId { get; set; }
}
