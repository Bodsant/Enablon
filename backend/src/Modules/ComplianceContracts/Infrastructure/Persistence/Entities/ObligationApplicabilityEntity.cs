namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.obligation_applicability</c> table.</summary>
public sealed class ObligationApplicabilityEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ObligationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public string ApplicabilityStatus { get; set; } = string.Empty;
    public string? Rationale { get; set; }
    public Guid AssessedByMemberId { get; set; }
}
