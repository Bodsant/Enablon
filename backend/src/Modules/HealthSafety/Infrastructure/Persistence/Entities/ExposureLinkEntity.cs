namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.exposure_links</c> table.</summary>
public sealed class ExposureLinkEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid HealthProfileId { get; set; } = Guid.Empty;
    public Guid SourceRecordId { get; set; } = Guid.Empty;
    public string ExposureType { get; set; } = string.Empty;
    public DateOnly? ExposurePeriodStart { get; set; } = null;
    public DateOnly? ExposurePeriodEnd { get; set; } = null;
}
