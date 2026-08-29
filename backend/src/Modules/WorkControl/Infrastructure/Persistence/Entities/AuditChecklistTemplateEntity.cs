namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.checklist_templates</c> table.</summary>
public sealed class AuditChecklistTemplateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? StandardReference { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}
