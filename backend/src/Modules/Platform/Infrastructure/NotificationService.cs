using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Creates in-app notifications for a tenant member, deduped against an existing
/// unread notification of the same type on the same record, and queues an outbox
/// delivery event (<c>notification.created</c>) carrying the chosen channel so the
/// outbox dispatcher can fan out email/SMS later.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly PlatformDbContext _db;
    private readonly ITenantContext _tenant;

    public NotificationService(PlatformDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CreateNotificationResult> CreateAsync(
        Guid recipientMemberId,
        string notificationType,
        string title,
        string message,
        Guid? recordId = null,
        string? deliveryChannel = null,
        Guid? tenantId = null,
        CancellationToken ct = default)
    {
        var tenant = tenantId ?? _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for notification (fail-closed).");

        // Dedup: skip if the recipient already has an unread notification of the same
        // type scoped to the same record (prevents duplicates from retries/escalation.
        var duplicate = await _db.Notifications.AnyAsync(
            n => n.TenantId == tenant && n.RecipientMemberId == recipientMemberId
                && n.NotificationType == notificationType && n.RecordId == recordId
                && n.ReadAt == null && n.DeliveryStatus != "Failed", ct);
        if (duplicate)
        {
            return new CreateNotificationResult(Guid.Empty, Deduplicated: true);
        }

        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenant,
            RecipientMemberId = recipientMemberId,
            RecordId = recordId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            DeliveryChannel = deliveryChannel ?? "in-app",
            DeliveryStatus = "Pending",
        };
        _db.Notifications.Add(notification);

        // Queue delivery (email/SMS) when a non in-app channel is requested.
        if (!string.Equals(deliveryChannel, "in-app", StringComparison.OrdinalIgnoreCase))
        {
            _db.OutboxMessages.Add(new OutboxMessageEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenant,
                RecordId = recordId,
                EventType = "notification.created",
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { notification.Id, notification.RecipientMemberId, notification.NotificationType, channel = deliveryChannel }),
                Status = "Pending",
                OccurredAt = DateTimeOffset.UtcNow,
            });
        }

        await _db.SaveChangesAsync(ct);
        return new CreateNotificationResult(notification.Id, Deduplicated: false);
    }

    public async Task<bool> MarkReadAsync(Guid notificationId, Guid memberId, Guid? tenantId = null, CancellationToken ct = default)
    {
        var tenant = tenantId ?? _tenant.CurrentTenantId;
        if (tenant is null)
        {
            return false;
        }

        // Direct UPDATE (no tracking) so an already-tracked instance cannot mask the
        // unread state; affected == 0 when not found or already read.
        var affected = await _db.Notifications
            .Where(n => n.Id == notificationId && n.TenantId == tenant && n.RecipientMemberId == memberId && n.ReadAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow), ct);
        return affected > 0;
    }
}