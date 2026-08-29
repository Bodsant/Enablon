namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.qualification_evaluations</c> table.</summary>
public sealed class QualificationEvaluationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid ContractorCompanyId { get; set; }
    public string EvaluationType { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public Guid EvaluatedByMemberId { get; set; }
    public DateOnly? ValidUntil { get; set; }
}
