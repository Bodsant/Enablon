namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>integration.runs</c> table. Integration execution runs.</summary>
public sealed class IntegrationRunEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InterfaceId { get; set; }
    public Guid? MappingId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? ReceivedCount { get; set; }
    public long? SuccessCount { get; set; }
    public long? ErrorCount { get; set; }

    public IntegrationInterfaceEntity? Interface { get; set; }
    public IntegrationDataMappingEntity? Mapping { get; set; }
    public ICollection<IntegrationMessageEntity> Messages { get; set; } = new List<IntegrationMessageEntity>();
    public ICollection<IntegrationReconciliationEntity> Reconciliations { get; set; } = new List<IntegrationReconciliationEntity>();
}