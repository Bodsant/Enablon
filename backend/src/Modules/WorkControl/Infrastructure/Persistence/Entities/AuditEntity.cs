namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.audits</c> table.</summary>
public sealed class AuditEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid? AuditProgramId { get; set; }
    public Guid? ChecklistTemplateId { get; set; }
    public string AuditType { get; set; } = string.Empty;
    public string ScopeText { get; set; } = string.Empty;
    public string? CriteriaText { get; set; }
    public Guid LeadAuditorMemberId { get; set; }
    public DateOnly? ScheduledStart { get; set; }
    public DateOnly? ScheduledEnd { get; set; }
}
