namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.schedules</c> table.</summary>
public sealed class InspectionScheduleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? AssignedMemberId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTimeOffset? NextExecutionAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
