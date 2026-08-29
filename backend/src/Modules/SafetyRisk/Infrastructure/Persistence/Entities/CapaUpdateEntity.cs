namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>capa.updates</c> table.</summary>
public sealed class CapaUpdateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActionId { get; set; }
    public short ProgressPercentage { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid UpdatedByMemberId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
