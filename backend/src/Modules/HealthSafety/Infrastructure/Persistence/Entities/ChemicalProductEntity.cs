namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>chemical.products</c> table.</summary>
public sealed class ChemicalProductEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public string? ProductCode { get; set; } = null;
    public string ProductName { get; set; } = string.Empty;
    public string? SupplierName { get; set; } = null;
    public string? HazardClassificationJson { get; set; } = null;
    public Guid? OwnerMemberId { get; set; } = null;
    public string Status { get; set; } = string.Empty;

    public ICollection<ChemicalInventoryEntity> ChemicalInventoryItems { get; set; } = new List<ChemicalInventoryEntity>();
}
