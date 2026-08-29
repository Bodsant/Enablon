namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.inspections</c> table.</summary>
public sealed class PpeInspectionEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PpeInventoryId { get; set; } = Guid.Empty;
    public Guid InspectedByMemberId { get; set; } = Guid.Empty;
    public DateTimeOffset InspectedAt { get; set; } = default;
    public string Condition { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateOnly? NextDueDate { get; set; } = null;
}
