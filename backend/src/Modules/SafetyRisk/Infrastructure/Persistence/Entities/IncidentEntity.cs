namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.incidents</c> table.</summary>
public sealed class IncidentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid IncidentTypeId { get; set; }
    public Guid SeverityId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public Guid ReportedByMemberId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImmediateAction { get; set; } = string.Empty;
    public string? ClassificationStatus { get; set; } = string.Empty;
}
