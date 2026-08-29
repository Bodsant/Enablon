namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.requirements</c> table.</summary>
public sealed class PpeRequirementEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PpeCatalogId { get; set; } = Guid.Empty;
    public Guid? SourceRecordId { get; set; } = null;
    public Guid? PermitTypeId { get; set; } = null;
    public bool IsMandatory { get; set; } = false;
    public string? Notes { get; set; } = null;
}
