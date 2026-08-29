namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>reporting.kpi_versions</c> table. Versioned KPI formula definitions.</summary>
public sealed class KpiVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid KpiDefinitionId { get; set; }
    public int VersionNumber { get; set; }
    public string FormulaExpression { get; set; } = string.Empty;
    public string? NumeratorDefinition { get; set; }
    public string? DenominatorDefinition { get; set; }
    public decimal? Factor { get; set; }
    public string? PeriodRule { get; set; }
    public string? ScopeRuleJson { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public KpiDefinitionEntity? KpiDefinition { get; set; }
}