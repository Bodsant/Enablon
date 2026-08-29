namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.documents</c> table.</summary>
public sealed class ContractorDocumentEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ContractorCompanyId { get; set; }
    public Guid? ContractorWorkerId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public Guid FileObjectId { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? VerificationStatus { get; set; }
}
