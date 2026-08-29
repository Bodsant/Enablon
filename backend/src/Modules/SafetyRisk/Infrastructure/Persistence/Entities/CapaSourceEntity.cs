namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>capa.sources</c> table.</summary>
public sealed class CapaSourceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActionId { get; set; }
    public Guid SourceRecordId { get; set; }
    public string? SourceRole { get; set; } = string.Empty;
}
