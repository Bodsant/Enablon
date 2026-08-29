namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>capa.actions</c> table.</summary>
public sealed class CapaActionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerMemberId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public short ProgressPercentage { get; set; }
    public bool VerificationRequired { get; set; }
}
