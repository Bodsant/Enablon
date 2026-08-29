namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.matrix_cells</c> table.</summary>
public sealed class RiskMatrixCellEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid MatrixVersionId { get; set; }
    public short LikelihoodValue { get; set; }
    public short SeverityValue { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevelCode { get; set; } = string.Empty;
}
