using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RetentionPolicyEntity"/> (<c>platform.retention_policies</c>).</summary>
public sealed class RetentionPolicyEntityConfiguration : IEntityTypeConfiguration<RetentionPolicyEntity>
{
    public const string TableName = "retention_policies";
    public const int RecordTypeMaxLength = 60;

    public void Configure(EntityTypeBuilder<RetentionPolicyEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordType).IsRequired().HasMaxLength(RecordTypeMaxLength);
        builder.Property(e => e.ClassificationId);
        builder.Property(e => e.RetentionDays);
        builder.Property(e => e.ArchiveAfterDays);
        builder.Property(e => e.RecycleBinDays);
        builder.Property(e => e.LegalHoldSupported).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_retention_policies_tenant_id");
        builder.HasIndex(e => e.ClassificationId).HasDatabaseName("ix_retention_policies_classification_id");

        builder.HasOne(e => e.Classification)
            .WithMany()
            .HasForeignKey(e => e.ClassificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}