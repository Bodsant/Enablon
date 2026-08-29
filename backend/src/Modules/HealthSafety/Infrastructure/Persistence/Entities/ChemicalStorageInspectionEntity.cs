namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>chemical.storage_inspections</c> table.</summary>
public sealed class ChemicalStorageInspectionEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public Guid ChemicalInventoryId { get; set; } = Guid.Empty;
    public Guid InspectedByMemberId { get; set; } = Guid.Empty;
    public DateTimeOffset InspectedAt { get; set; } = default;
    public string Result { get; set; } = string.Empty;
    public DateOnly? NextReviewDate { get; set; } = null;
}
