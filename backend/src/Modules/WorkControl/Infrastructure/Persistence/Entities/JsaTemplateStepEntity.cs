namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsa_template_steps</c> table.</summary>
public sealed class JsaTemplateStepEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid JsaTemplateVersionId { get; set; }
    public int SequenceNumber { get; set; }
    public string WorkStep { get; set; } = string.Empty;
}
