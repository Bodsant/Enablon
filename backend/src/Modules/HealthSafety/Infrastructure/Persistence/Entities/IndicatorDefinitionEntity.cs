namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>sustainability.indicator_definitions</c> table.</summary>
public sealed class IndicatorDefinitionEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BoundaryDefinition { get; set; } = null;
    public string? DefaultUnit { get; set; } = null;
    public string? FrameworkReference { get; set; } = null;
    public string Status { get; set; } = string.Empty;

    public ICollection<SustainabilityTargetEntity> Targets { get; set; } = new List<SustainabilityTargetEntity>();
}
