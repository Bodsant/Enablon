using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitChecklistItemEntity"/> (<c>cow.permit_checklist_items</c>).</summary>
public sealed class PermitChecklistItemEntityConfiguration : IEntityTypeConfiguration<PermitChecklistItemEntity>
{
    public const string TableName = "permit_checklist_items";

    public void Configure(EntityTypeBuilder<PermitChecklistItemEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitTypeVersionId).IsRequired();
        builder.Property(e => e.SequenceNumber).IsRequired();
        builder.Property(e => e.Prompt).IsRequired();
        builder.Property(e => e.IsMandatory).IsRequired();
        builder.Property(e => e.ValidationType).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_checklist_items_tenant_id");
    }
}
