namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>contractor.performance_periods</c> table.</summary>
public sealed class PerformancePeriodEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ContractorCompanyId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? IndicatorValuesJson { get; set; }
    public decimal? OverallRating { get; set; }
}
