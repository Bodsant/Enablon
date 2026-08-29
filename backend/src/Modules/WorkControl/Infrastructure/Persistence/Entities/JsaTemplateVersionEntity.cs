namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsa_template_versions</c> table.</summary>
public sealed class JsaTemplateVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid JsaTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public Guid? SiteId { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public string Status { get; set; } = string.Empty;
}
