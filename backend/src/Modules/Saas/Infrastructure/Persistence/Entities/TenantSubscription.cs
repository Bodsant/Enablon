namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class TenantSubscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanVersionId { get; set; }
    public string Status { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public DateTimeOffset NextResetAt { get; set; }
    public Guid? ScheduledPlanVersionId { get; set; }
    public DateTimeOffset? ScheduledChangeAt { get; set; }
}
