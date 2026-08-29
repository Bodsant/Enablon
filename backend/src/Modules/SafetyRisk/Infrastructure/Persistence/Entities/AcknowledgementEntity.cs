namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>document.acknowledgements</c> table.</summary>
public sealed class AcknowledgementEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DocumentRevisionId { get; set; }
    public Guid TenantMemberId { get; set; }
    public DateTimeOffset AcknowledgedAt { get; set; }
}
