namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.evaluations</c> table.</summary>
public sealed class EvaluationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid ObligationId { get; set; }
    public DateOnly? EvaluationPeriodStart { get; set; }
    public DateOnly? EvaluationPeriodEnd { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty;
    public Guid EvaluatedByMemberId { get; set; }
    public DateTimeOffset EvaluatedAt { get; set; }
    public string? Comment { get; set; }
}
