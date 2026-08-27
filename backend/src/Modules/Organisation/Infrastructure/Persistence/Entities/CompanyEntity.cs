using System;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps org.companies (001-foundation.sql · Wave 1).
/// A legal/operating entity within a tenant that owns business units, sites and employees.
/// </summary>
public sealed class CompanyEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Status { get; set; } = default!;
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
