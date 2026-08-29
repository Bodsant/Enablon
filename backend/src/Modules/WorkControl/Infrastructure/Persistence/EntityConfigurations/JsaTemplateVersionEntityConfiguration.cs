using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaTemplateVersionEntity"/> (<c>cow.jsa_template_versions</c>).</summary>
public sealed class JsaTemplateVersionEntityConfiguration : IEntityTypeConfiguration<JsaTemplateVersionEntity>
{
    public const string TableName = "jsa_template_versions";

    public void Configure(EntityTypeBuilder<JsaTemplateVersionEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.JsaTemplateId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.SiteId);
        builder.Property(e => e.EffectiveFrom);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsa_template_versions_tenant_id");
    }
}
