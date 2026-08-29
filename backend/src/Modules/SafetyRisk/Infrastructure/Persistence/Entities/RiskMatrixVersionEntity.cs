namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.matrix_versions</c> table.</summary>
public sealed class RiskMatrixVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public int LikelihoodScale { get; set; }
    public int SeverityScale { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } = string.Empty;
}
