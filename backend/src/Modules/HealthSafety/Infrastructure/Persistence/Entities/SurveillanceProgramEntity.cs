namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.surveillance_programs</c> table.</summary>
public sealed class SurveillanceProgramEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ExposureType { get; set; } = null;
    public int? FrequencyMonths { get; set; } = null;
    public string Status { get; set; } = string.Empty;

}
