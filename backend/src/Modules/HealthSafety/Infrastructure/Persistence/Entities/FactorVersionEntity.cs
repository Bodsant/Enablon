namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>sustainability.factor_versions</c> table.</summary>
public sealed class FactorVersionEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public string FactorCode { get; set; } = string.Empty;
    public int VersionNumber { get; set; } = 0;
    public decimal FactorValue { get; set; } = default;
    public string Unit { get; set; } = string.Empty;
    public string? SourceReference { get; set; } = null;
    public DateOnly? EffectiveFrom { get; set; } = null;
    public DateOnly? EffectiveTo { get; set; } = null;

}
