namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.checklist_items</c> table.</summary>
public sealed class AuditChecklistItemEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ChecklistTemplateId { get; set; }
    public int SequenceNumber { get; set; }
    public string? RequirementReference { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? ClassificationRuleJson { get; set; }
}
