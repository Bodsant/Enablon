namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>risk.reviews</c> table.</summary>
public sealed class RiskReviewEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RiskRegisterId { get; set; }
    public Guid ReviewedByMemberId { get; set; }
    public DateTimeOffset ReviewedAt { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Comment { get; set; } = string.Empty;
}
