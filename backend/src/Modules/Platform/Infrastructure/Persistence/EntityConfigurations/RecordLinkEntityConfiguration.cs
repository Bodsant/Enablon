using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RecordLinkEntity"/> (<c>platform.record_links</c>).</summary>
public sealed class RecordLinkEntityConfiguration : IEntityTypeConfiguration<RecordLinkEntity>
{
    public const string TableName = "record_links";
    public const int LinkTypeMaxLength = 60;

    public void Configure(EntityTypeBuilder<RecordLinkEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SourceRecordId).IsRequired();
        builder.Property(e => e.TargetRecordId).IsRequired();
        builder.Property(e => e.LinkType).IsRequired().HasMaxLength(LinkTypeMaxLength);
        builder.Property(e => e.CreatedByMemberId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_record_links_tenant_id");
        builder.HasIndex(e => e.SourceRecordId).HasDatabaseName("ix_record_links_source_record_id");
        builder.HasIndex(e => e.TargetRecordId).HasDatabaseName("ix_record_links_target_record_id");

        builder.HasOne(e => e.SourceRecord)
            .WithMany(e => e.SourceRecordLinks)
            .HasForeignKey(e => e.SourceRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetRecord)
            .WithMany(e => e.TargetRecordLinks)
            .HasForeignKey(e => e.TargetRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}