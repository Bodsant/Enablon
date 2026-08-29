namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.responses</c> table.</summary>
public sealed class AuditResponseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AuditId { get; set; }
    public Guid ChecklistItemId { get; set; }
    public string? Response { get; set; }
    public string? Comment { get; set; }
    public Guid AuditorMemberId { get; set; }
}
