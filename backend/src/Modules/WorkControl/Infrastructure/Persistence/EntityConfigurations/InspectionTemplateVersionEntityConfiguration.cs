using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionTemplateVersionEntity"/> (<c>inspection.template_versions</c>).</summary>
public sealed class InspectionTemplateVersionEntityConfiguration : IEntityTypeConfiguration<InspectionTemplateVersionEntity>
{
    public const string TableName = "template_versions";

    public void Configure(EntityTypeBuilder<InspectionTemplateVersionEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TemplateId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.EffectiveFrom);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_template_versions_tenant_id");
    }
}
