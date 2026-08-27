namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.outbox_messages</c> table. Transactional outbox for integration events.</summary>
public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? RecordId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public RecordEntity? Record { get; set; }
}