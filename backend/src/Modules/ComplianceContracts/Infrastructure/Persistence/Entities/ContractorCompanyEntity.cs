namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.companies</c> table.</summary>
public sealed class ContractorCompanyEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string? VendorCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxIdentifier { get; set; }
    public string? QualificationStatus { get; set; }
    public string? EligibilityStatus { get; set; }
    public string Status { get; set; } = string.Empty;
}
