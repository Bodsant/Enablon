namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsas</c> table.</summary>
public sealed class JsaEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid WorkRequestId { get; set; }
    public Guid? TemplateVersionId { get; set; }
    public Guid PreparedByMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OverallResidualRisk { get; set; }
}
