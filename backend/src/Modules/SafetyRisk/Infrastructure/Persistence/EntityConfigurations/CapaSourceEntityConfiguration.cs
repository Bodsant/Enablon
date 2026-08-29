using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CapaSourceEntity"/> (<c>capa.sources</c>).</summary>
public sealed class CapaSourceEntityConfiguration : IEntityTypeConfiguration<CapaSourceEntity>
{
    public const string TableName = "sources";

    public void Configure(EntityTypeBuilder<CapaSourceEntity> builder)
    {
        builder.ToTable(TableName, "capa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ActionId).IsRequired();
        builder.Property(e => e.SourceRecordId).IsRequired();
        builder.Property(e => e.SourceRole).HasMaxLength(40);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_sources_tenant_id");
    }
}
