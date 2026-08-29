namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.workers</c> table.</summary>
public sealed class ContractorWorkerEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid ContractorCompanyId { get; set; }
    public string? WorkerNumber { get; set; }
    public string? PositionName { get; set; }
    public string? EligibilityStatus { get; set; }
    public string Status { get; set; } = string.Empty;
}
