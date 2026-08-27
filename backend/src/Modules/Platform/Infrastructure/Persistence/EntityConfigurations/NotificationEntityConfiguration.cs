using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="NotificationEntity"/> (<c>platform.notifications</c>).</summary>
public sealed class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public const string TableName = "notifications";
    public const int NotificationTypeMaxLength = 60;
    public const int TitleMaxLength = 200;
    public const int DeliveryChannelMaxLength = 30;
    public const int DeliveryStatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecipientMemberId).IsRequired();
        builder.Property(e => e.RecordId);
        builder.Property(e => e.NotificationType).IsRequired().HasMaxLength(NotificationTypeMaxLength);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(TitleMaxLength);
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.DeliveryChannel).HasMaxLength(DeliveryChannelMaxLength);
        builder.Property(e => e.DeliveryStatus).HasMaxLength(DeliveryStatusMaxLength);
        builder.Property(e => e.ReadAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_notifications_tenant_id");
        builder.HasIndex(e => e.RecipientMemberId).HasDatabaseName("ix_notifications_recipient_member_id");
        builder.HasIndex(e => e.RecordId).HasDatabaseName("ix_notifications_record_id");

        builder.HasOne(e => e.Record)
            .WithMany(e => e.Notifications)
            .HasForeignKey(e => e.RecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}