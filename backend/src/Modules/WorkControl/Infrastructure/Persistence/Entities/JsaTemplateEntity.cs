namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsa_templates</c> table.</summary>
public sealed class JsaTemplateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
}
