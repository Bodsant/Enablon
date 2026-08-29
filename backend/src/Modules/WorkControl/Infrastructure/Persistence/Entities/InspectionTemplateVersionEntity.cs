namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.template_versions</c> table.</summary>
public sealed class InspectionTemplateVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public string Status { get; set; } = string.Empty;
}
