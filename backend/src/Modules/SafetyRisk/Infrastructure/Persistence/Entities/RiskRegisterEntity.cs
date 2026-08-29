namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.registers</c> table.</summary>
public sealed class RiskRegisterEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid HazardId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string RiskEvent { get; set; } = string.Empty;
    public Guid OwnerMemberId { get; set; }
    public DateOnly? ReviewDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
