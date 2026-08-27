namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.notifications</c> table. In-app notifications for members.</summary>
public sealed class NotificationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecipientMemberId { get; set; }
    public Guid? RecordId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DeliveryChannel { get; set; }
    public string? DeliveryStatus { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public RecordEntity? Record { get; set; }
}