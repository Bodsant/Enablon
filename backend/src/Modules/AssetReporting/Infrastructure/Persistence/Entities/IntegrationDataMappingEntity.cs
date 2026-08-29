namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>integration.data_mappings</c> table. Versioned data mappings for an interface.</summary>
public sealed class IntegrationDataMappingEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InterfaceId { get; set; }
    public int VersionNumber { get; set; }
    public string? SourceSchemaJson { get; set; }
    public string? TargetSchemaJson { get; set; }
    public string? MappingRulesJson { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }

    public IntegrationInterfaceEntity? Interface { get; set; }
    public ICollection<IntegrationRunEntity> Runs { get; set; } = new List<IntegrationRunEntity>();
}