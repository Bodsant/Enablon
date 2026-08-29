namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.equipment</c> table. Emergency response equipment.</summary>
public sealed class EmergencyEquipmentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? LocationId { get; set; }
    public string EquipmentType { get; set; } = string.Empty;
    public Guid? AssetId { get; set; }
    public DateOnly? InspectionDueDate { get; set; }
    public DateOnly? MaintenanceDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}