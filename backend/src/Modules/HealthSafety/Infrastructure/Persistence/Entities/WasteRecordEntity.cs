namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.waste_records</c> table.</summary>
public sealed class WasteRecordEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public string WasteType { get; set; } = string.Empty;
    public bool IsHazardous { get; set; } = false;
    public decimal? Quantity { get; set; } = null;
    public string? Unit { get; set; } = null;
    public Guid? SourceLocationId { get; set; } = null;
    public string? HandlerName { get; set; } = null;
    public string? ManifestNumber { get; set; } = null;
    public DateOnly RecordDate { get; set; } = default;
}
