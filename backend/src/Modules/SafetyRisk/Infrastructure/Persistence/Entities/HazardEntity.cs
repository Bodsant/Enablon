namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.hazards</c> table.</summary>
public sealed class HazardEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
