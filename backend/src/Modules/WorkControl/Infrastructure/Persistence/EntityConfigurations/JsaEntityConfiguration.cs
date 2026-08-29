using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaEntity"/> (<c>cow.jsas</c>).</summary>
public sealed class JsaEntityConfiguration : IEntityTypeConfiguration<JsaEntity>
{
    public const string TableName = "jsas";

    public void Configure(EntityTypeBuilder<JsaEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.WorkRequestId).IsRequired();
        builder.Property(e => e.TemplateVersionId);
        builder.Property(e => e.PreparedByMemberId).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.Property(e => e.OverallResidualRisk).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsas_tenant_id");
    }
}
