using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaTemplateStepEntity"/> (<c>cow.jsa_template_steps</c>).</summary>
public sealed class JsaTemplateStepEntityConfiguration : IEntityTypeConfiguration<JsaTemplateStepEntity>
{
    public const string TableName = "jsa_template_steps";

    public void Configure(EntityTypeBuilder<JsaTemplateStepEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.JsaTemplateVersionId).IsRequired();
        builder.Property(e => e.SequenceNumber).IsRequired();
        builder.Property(e => e.WorkStep).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsa_template_steps_tenant_id");
    }
}
