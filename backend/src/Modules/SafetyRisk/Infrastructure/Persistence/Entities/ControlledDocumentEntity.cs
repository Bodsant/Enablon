namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>document.controlled_documents</c> table.</summary>
public sealed class ControlledDocumentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid OwnerMemberId { get; set; }
    public DateOnly? ReviewDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
