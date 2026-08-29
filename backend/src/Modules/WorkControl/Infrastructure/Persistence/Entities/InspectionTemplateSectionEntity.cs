namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.template_sections</c> table.</summary>
public sealed class InspectionTemplateSectionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
}
