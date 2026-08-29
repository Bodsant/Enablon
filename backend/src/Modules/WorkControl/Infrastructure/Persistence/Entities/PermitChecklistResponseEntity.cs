namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_checklist_responses</c> table.</summary>
public sealed class PermitChecklistResponseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitId { get; set; }
    public Guid ChecklistItemId { get; set; }
    public string? ResponseJson { get; set; }
    public bool? IsSatisfied { get; set; }
    public Guid? CheckedByMemberId { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }
}
