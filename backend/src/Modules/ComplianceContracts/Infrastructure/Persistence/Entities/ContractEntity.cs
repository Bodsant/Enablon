namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.contracts</c> table.</summary>
public sealed class ContractEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ContractorCompanyId { get; set; }
    public string? ContractNumber { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? ContractStatus { get; set; }
    public string? ProcurementSourceId { get; set; }
}
