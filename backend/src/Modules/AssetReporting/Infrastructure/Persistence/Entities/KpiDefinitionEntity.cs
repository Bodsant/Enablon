namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>reporting.kpi_definitions</c> table. KPI definitions.</summary>
public sealed class KpiDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<KpiVersionEntity> Versions { get; set; } = new List<KpiVersionEntity>();
}