using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionTemplateItemEntity"/> (<c>inspection.template_items</c>).</summary>
public sealed class InspectionTemplateItemEntityConfiguration : IEntityTypeConfiguration<InspectionTemplateItemEntity>
{
    public const string TableName = "template_items";

    public void Configure(EntityTypeBuilder<InspectionTemplateItemEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SectionId).IsRequired();
        builder.Property(e => e.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Prompt).IsRequired();
        builder.Property(e => e.ResponseType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.IsRequired).IsRequired();
        builder.Property(e => e.Weight).HasPrecision(10, 2);
        builder.Property(e => e.CriteriaJson).HasColumnType("jsonb");
        builder.Property(e => e.SequenceNumber).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_template_items_tenant_id");
    }
}
