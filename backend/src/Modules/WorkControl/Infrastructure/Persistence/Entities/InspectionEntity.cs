namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.inspections</c> table.</summary>
public sealed class InspectionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public Guid InspectorMemberId { get; set; }
    public DateTimeOffset? PlannedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public decimal? CompliancePercentage { get; set; }
}
