namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.assessments</c> table.</summary>
public sealed class RiskAssessmentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RiskRegisterId { get; set; }
    public Guid MatrixVersionId { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public short LikelihoodValue { get; set; }
    public short SeverityValue { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevelCode { get; set; } = string.Empty;
    public Guid AssessedByMemberId { get; set; }
    public DateTimeOffset AssessedAt { get; set; }
}
