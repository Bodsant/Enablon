namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.surveillance_events</c> table.</summary>
public sealed class SurveillanceEventEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public Guid HealthProfileId { get; set; } = Guid.Empty;
    public Guid SurveillanceProgramId { get; set; } = Guid.Empty;
    public DateOnly? ScheduledDate { get; set; } = null;
    public DateOnly? CompletedDate { get; set; } = null;
    public string? AuthorizedProvider { get; set; } = null;
    public string? ResultSummaryCode { get; set; } = null;

}
