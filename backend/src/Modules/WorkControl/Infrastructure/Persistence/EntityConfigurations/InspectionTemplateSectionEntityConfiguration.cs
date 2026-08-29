using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionTemplateSectionEntity"/> (<c>inspection.template_sections</c>).</summary>
public sealed class InspectionTemplateSectionEntityConfiguration : IEntityTypeConfiguration<InspectionTemplateSectionEntity>
{
    public const string TableName = "template_sections";

    public void Configure(EntityTypeBuilder<InspectionTemplateSectionEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TemplateVersionId).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SequenceNumber).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_template_sections_tenant_id");
    }
}
