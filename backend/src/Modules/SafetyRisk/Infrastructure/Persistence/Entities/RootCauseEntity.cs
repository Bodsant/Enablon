namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.root_causes</c> table.</summary>
public sealed class RootCauseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InvestigationId { get; set; }
    public string CauseType { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EvidenceSummary { get; set; } = string.Empty;
}
