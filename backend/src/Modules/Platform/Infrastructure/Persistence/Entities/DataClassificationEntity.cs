namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.data_classifications</c> table. Data sensitivity classification levels.</summary>
public sealed class DataClassificationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short Rank { get; set; }
    public bool IsRestricted { get; set; }
}