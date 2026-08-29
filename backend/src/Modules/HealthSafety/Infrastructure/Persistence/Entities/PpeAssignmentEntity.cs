namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.assignments</c> table.</summary>
public sealed class PpeAssignmentEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PpeInventoryId { get; set; } = Guid.Empty;
    public Guid PersonId { get; set; } = Guid.Empty;
    public DateTimeOffset IssuedAt { get; set; } = default;
    public Guid IssuedByMemberId { get; set; } = Guid.Empty;
    public DateTimeOffset? ReturnedAt { get; set; } = null;
    public string? ConditionOnReturn { get; set; } = null;

}
