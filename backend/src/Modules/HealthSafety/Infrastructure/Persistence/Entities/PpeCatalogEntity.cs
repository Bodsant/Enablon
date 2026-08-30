namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.catalog</c> table.</summary>
public sealed class PpeCatalogEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PpeCategory { get; set; } = null;
    public int? InspectionIntervalDays { get; set; } = null;
    public int? ReplacementIntervalDays { get; set; } = null;
    public string Status { get; set; } = string.Empty;
    }
