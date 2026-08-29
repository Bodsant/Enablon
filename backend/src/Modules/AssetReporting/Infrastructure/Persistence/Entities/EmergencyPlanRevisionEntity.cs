namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.plan_revisions</c> table. Versioned revisions of an emergency plan.</summary>
public sealed class EmergencyPlanRevisionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmergencyPlanId { get; set; }
    public string RevisionNumber { get; set; } = string.Empty;
    public DateOnly? EffectiveDate { get; set; }
    public Guid? FileObjectId { get; set; }
    public string Status { get; set; } = string.Empty;

    public EmergencyPlanEntity? EmergencyPlan { get; set; }
}