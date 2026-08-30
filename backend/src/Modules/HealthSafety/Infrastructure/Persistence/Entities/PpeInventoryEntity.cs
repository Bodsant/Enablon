namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.inventory</c> table.</summary>
public sealed class PpeInventoryEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PpeCatalogId { get; set; } = Guid.Empty;
    public Guid SiteId { get; set; } = Guid.Empty;
    public string? SerialNumber { get; set; } = null;
    public int? QuantityOnHand { get; set; } = null;
    public string? Condition { get; set; } = null;
    public string Status { get; set; } = string.Empty;
    }
