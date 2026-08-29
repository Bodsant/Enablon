namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_checklist_items</c> table.</summary>
public sealed class PermitChecklistItemEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitTypeVersionId { get; set; }
    public int SequenceNumber { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string? ValidationType { get; set; }
}
