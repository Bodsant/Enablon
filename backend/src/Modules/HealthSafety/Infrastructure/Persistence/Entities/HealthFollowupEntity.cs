namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.followups</c> table.</summary>
public sealed class HealthFollowupEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid SurveillanceEventId { get; set; } = Guid.Empty;
    public string FollowupType { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; } = null;
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedMemberId { get; set; } = null;
}
