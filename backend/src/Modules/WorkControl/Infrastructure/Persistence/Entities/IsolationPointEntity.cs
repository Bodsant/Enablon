namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.isolation_points</c> table.</summary>
public sealed class IsolationPointEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IsolationPlanId { get; set; }
    public Guid? AssetId { get; set; }
    public string EnergySource { get; set; } = string.Empty;
    public string IsolationMethod { get; set; } = string.Empty;
    public string PointIdentifier { get; set; } = string.Empty;
}
