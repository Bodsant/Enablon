namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.isolation_plans</c> table.</summary>
public sealed class IsolationPlanEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid PermitId { get; set; }
    public Guid PreparedByMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
}
