namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>chemical.exposure_controls</c> table.</summary>
public sealed class ChemicalExposureControlEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid ChemicalProductId { get; set; } = Guid.Empty;
    public string ControlType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SourceRecordId { get; set; } = null;
}
