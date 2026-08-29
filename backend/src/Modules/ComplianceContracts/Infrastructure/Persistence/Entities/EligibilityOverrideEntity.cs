namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.eligibility_overrides</c> table.</summary>
public sealed class EligibilityOverrideEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EligibilityCheckId { get; set; }
    public Guid ApprovedByMemberId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset? ValidUntil { get; set; }
}
