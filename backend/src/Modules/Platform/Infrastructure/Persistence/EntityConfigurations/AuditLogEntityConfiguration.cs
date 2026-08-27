using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditLogEntity"/> (<c>platform.audit_logs</c>).</summary>
public sealed class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public const string TableName = "audit_logs";
    public const int ActionCodeMaxLength = 100;
    public const int IpAddressMaxLength = 64;
    public const int CorrelationIdMaxLength = 100;

    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId);
        builder.Property(e => e.UserId);
        builder.Property(e => e.TenantMemberId);
        builder.Property(e => e.ActionCode).IsRequired().HasMaxLength(ActionCodeMaxLength);
        builder.Property(e => e.BeforeJson).HasColumnType("jsonb");
        builder.Property(e => e.AfterJson).HasColumnType("jsonb");
        builder.Property(e => e.IpAddress).HasMaxLength(IpAddressMaxLength);
        builder.Property(e => e.CorrelationId).HasMaxLength(CorrelationIdMaxLength);
        builder.Property(e => e.OccurredAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_audit_logs_tenant_id");
        builder.HasIndex(e => e.RecordId).HasDatabaseName("ix_audit_logs_record_id");
        builder.HasIndex(e => e.OccurredAt).HasDatabaseName("ix_audit_logs_occurred_at");

        builder.HasOne(e => e.Record)
            .WithMany(e => e.AuditLogs)
            .HasForeignKey(e => e.RecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}