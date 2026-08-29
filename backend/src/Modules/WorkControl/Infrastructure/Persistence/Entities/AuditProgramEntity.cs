namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.programs</c> table.</summary>
public sealed class AuditProgramEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public Guid OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
}
