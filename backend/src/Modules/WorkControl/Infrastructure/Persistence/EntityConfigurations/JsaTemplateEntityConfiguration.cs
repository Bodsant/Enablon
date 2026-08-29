using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaTemplateEntity"/> (<c>cow.jsa_templates</c>).</summary>
public sealed class JsaTemplateEntityConfiguration : IEntityTypeConfiguration<JsaTemplateEntity>
{
    public const string TableName = "jsa_templates";

    public void Configure(EntityTypeBuilder<JsaTemplateEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsa_templates_tenant_id");
    }
}
