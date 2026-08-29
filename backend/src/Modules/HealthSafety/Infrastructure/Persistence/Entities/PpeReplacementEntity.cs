namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>ppe.replacements</c> table.</summary>
public sealed class PpeReplacementEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PpeAssignmentId { get; set; } = Guid.Empty;
    public string ReplacementReason { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; } = default;
    public DateTimeOffset? CompletedAt { get; set; } = null;
}
