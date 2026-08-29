namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>document.revisions</c> table.</summary>
public sealed class RevisionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ControlledDocumentId { get; set; }
    public string RevisionNumber { get; set; } = string.Empty;
    public Guid FileObjectId { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedByMemberId { get; set; }
}
