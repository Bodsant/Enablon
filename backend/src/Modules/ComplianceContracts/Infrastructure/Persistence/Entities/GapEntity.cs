namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.gaps</c> table.</summary>
public sealed class GapEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid EvaluationId { get; set; }
    public string GapDescription { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public Guid? OwnerMemberId { get; set; }
    public DateOnly? TargetDate { get; set; }
}
