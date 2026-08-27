using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="OutboxMessageEntity"/> (<c>platform.outbox_messages</c>).</summary>
public sealed class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessageEntity>
{
    public const string TableName = "outbox_messages";
    public const int EventTypeMaxLength = 100;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<OutboxMessageEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(EventTypeMaxLength);
        builder.Property(e => e.PayloadJson).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.AttemptCount).IsRequired();
        builder.Property(e => e.NextRetryAt);
        builder.Property(e => e.OccurredAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_outbox_messages_tenant_id");
        builder.HasIndex(e => new { e.Status, e.NextRetryAt }).HasDatabaseName("ix_outbox_messages_status_next_retry_at");
        builder.HasIndex(e => e.RecordId).HasDatabaseName("ix_outbox_messages_record_id");

        builder.HasOne(e => e.Record)
            .WithMany(e => e.OutboxMessages)
            .HasForeignKey(e => e.RecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}