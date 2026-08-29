namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>integration.interfaces</c> table. Integration interface definitions.</summary>
public sealed class IntegrationInterfaceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string IntegrationMethod { get; set; } = string.Empty;
    public string? AuthenticationType { get; set; }
    public Guid? OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<IntegrationDataMappingEntity> DataMappings { get; set; } = new List<IntegrationDataMappingEntity>();
    public ICollection<IntegrationRunEntity> Runs { get; set; } = new List<IntegrationRunEntity>();
}