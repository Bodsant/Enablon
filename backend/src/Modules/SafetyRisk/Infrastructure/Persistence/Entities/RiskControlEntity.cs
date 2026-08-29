namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.controls</c> table.</summary>
public sealed class RiskControlEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RiskRegisterId { get; set; }
    public string ControlType { get; set; } = string.Empty;
    public string ControlStage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? OwnerMemberId { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public short? EffectivenessRating { get; set; }
}
