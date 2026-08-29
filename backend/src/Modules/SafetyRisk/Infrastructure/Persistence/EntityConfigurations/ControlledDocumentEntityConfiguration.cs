using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ControlledDocumentEntity"/> (<c>document.controlled_documents</c>).</summary>
public sealed class ControlledDocumentEntityConfiguration : IEntityTypeConfiguration<ControlledDocumentEntity>
{
    public const string TableName = "controlled_documents";

    public void Configure(EntityTypeBuilder<ControlledDocumentEntity> builder)
    {
        builder.ToTable(TableName, "document");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(60);
        builder.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(250);
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.ReviewDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_controlled_documents_tenant_id");
    }
}
