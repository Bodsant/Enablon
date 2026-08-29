namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsa_steps</c> table.</summary>
public sealed class JsaStepEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid JsaId { get; set; }
    public int SequenceNumber { get; set; }
    public string WorkStep { get; set; } = string.Empty;
}
