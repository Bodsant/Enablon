namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>environment.sources</c> table.</summary>
public sealed class EnvironmentSourceEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid SiteId { get; set; } = Guid.Empty;
    public Guid? LocationId { get; set; } = null;
    public string SourceType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PermitReference { get; set; } = null;

}
