namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.resource_usage</c> table.</summary>
public sealed class ResourceUsageEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid RecordId { get; set; } = Guid.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid SiteId { get; set; } = Guid.Empty;
    public DateOnly PeriodStart { get; set; } = default;
    public DateOnly PeriodEnd { get; set; } = default;
    public decimal Quantity { get; set; } = default;
    public string Unit { get; set; } = string.Empty;
    public string? SourceReference { get; set; } = null;
}
