namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>chemical.inventory</c> table.</summary>
public sealed class ChemicalInventoryEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid ChemicalProductId { get; set; } = Guid.Empty;
    public Guid LocationId { get; set; } = Guid.Empty;
    public decimal? Quantity { get; set; } = null;
    public string? Unit { get; set; } = null;
    public string? StorageCondition { get; set; } = null;
    public DateOnly? ExpiryDate { get; set; } = null;

}
