using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="FileObjectEntity"/> (<c>platform.file_objects</c>).</summary>
public sealed class FileObjectEntityConfiguration : IEntityTypeConfiguration<FileObjectEntity>
{
    public const string TableName = "file_objects";
    public const int BucketNameMaxLength = 100;
    public const int ObjectKeyMaxLength = 600;
    public const int OriginalFileNameMaxLength = 255;
    public const int MimeTypeMaxLength = 150;
    public const int ChecksumSha256MaxLength = 64;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<FileObjectEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.UploadSessionId);
        builder.Property(e => e.BucketName).IsRequired().HasMaxLength(BucketNameMaxLength);
        builder.Property(e => e.ObjectKey).IsRequired().HasMaxLength(ObjectKeyMaxLength);
        builder.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(OriginalFileNameMaxLength);
        builder.Property(e => e.MimeType).IsRequired().HasMaxLength(MimeTypeMaxLength);
        builder.Property(e => e.ObjectSizeBytes).IsRequired();
        builder.Property(e => e.ChecksumSha256).IsRequired().HasMaxLength(ChecksumSha256MaxLength);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.UploadedByUserId).IsRequired();
        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.PurgeAfter);
        builder.Property(e => e.PurgedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_file_objects_tenant_id");
        builder.HasIndex(e => e.ObjectKey).IsUnique().HasDatabaseName("ix_file_objects_object_key");
        builder.HasIndex(e => e.Status).HasDatabaseName("ix_file_objects_status");
    }
}