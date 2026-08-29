using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RevisionEntity"/> (<c>document.revisions</c>).</summary>
public sealed class RevisionEntityConfiguration : IEntityTypeConfiguration<RevisionEntity>
{
    public const string TableName = "revisions";

    public void Configure(EntityTypeBuilder<RevisionEntity> builder)
    {
        builder.ToTable(TableName, "document");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ControlledDocumentId).IsRequired();
        builder.Property(e => e.RevisionNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.FileObjectId).IsRequired();
        builder.Property(e => e.EffectiveDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ApprovedByMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_revisions_tenant_id");
    }
}
