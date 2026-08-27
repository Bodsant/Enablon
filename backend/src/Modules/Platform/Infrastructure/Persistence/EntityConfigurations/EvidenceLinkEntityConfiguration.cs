using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EvidenceLinkEntity"/> (<c>platform.evidence_links</c>).</summary>
public sealed class EvidenceLinkEntityConfiguration : IEntityTypeConfiguration<EvidenceLinkEntity>
{
    public const string TableName = "evidence_links";
    public const int EvidenceTypeMaxLength = 50;
    public const int LinkStatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<EvidenceLinkEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.FileObjectId).IsRequired();
        builder.Property(e => e.EvidenceType).IsRequired().HasMaxLength(EvidenceTypeMaxLength);
        builder.Property(e => e.DocumentRevisionId);
        builder.Property(e => e.LinkStatus).IsRequired().HasMaxLength(LinkStatusMaxLength);
        builder.Property(e => e.LinkedByMemberId).IsRequired();
        builder.Property(e => e.LinkedAt).IsRequired();
        builder.Property(e => e.InvalidationReason);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_evidence_links_tenant_id");
        builder.HasIndex(e => e.RecordId).HasDatabaseName("ix_evidence_links_record_id");
        builder.HasIndex(e => e.FileObjectId).HasDatabaseName("ix_evidence_links_file_object_id");

        builder.HasOne(e => e.Record)
            .WithMany(e => e.EvidenceLinks)
            .HasForeignKey(e => e.RecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FileObject)
            .WithMany(e => e.EvidenceLinks)
            .HasForeignKey(e => e.FileObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}