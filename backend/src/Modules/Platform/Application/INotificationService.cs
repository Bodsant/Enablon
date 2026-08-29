namespace Ehsms.Modules.Platform.Application;

/// <summary>Result of creating a notification.</summary>
public sealed record CreateNotificationResult(Guid Id, bool Deduplicated);

/// <summary>
/// In-app notification service contract. Creates deduplicated notifications for a
/// tenant member on a record context and queues delivery through the outbox.
/// </summary>
public interface INotificationService
{
    Task<CreateNotificationResult> CreateAsync(
        Guid recipientMemberId,
        string notificationType,
        string title,
        string message,
        Guid? recordId = null,
        string? deliveryChannel = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    Task<bool> MarkReadAsync(Guid notificationId, Guid memberId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}