using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IntegrationDataMappingEntity"/> (<c>integration.data_mappings</c>).</summary>
public sealed class IntegrationDataMappingEntityConfiguration : IEntityTypeConfiguration<IntegrationDataMappingEntity>
{
    public void Configure(EntityTypeBuilder<IntegrationDataMappingEntity> builder)
    {
        builder.ToTable("data_mappings", "integration");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.InterfaceId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.SourceSchemaJson).HasColumnType("jsonb");
        builder.Property(e => e.TargetSchemaJson).HasColumnType("jsonb");
        builder.Property(e => e.MappingRulesJson).HasColumnType("jsonb");
        builder.Property(e => e.EffectiveFrom);
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_data_mappings_tenant_id");
        builder.HasIndex(e => e.InterfaceId).HasDatabaseName("ix_data_mappings_interface_id");
        builder.HasOne(e => e.Interface).WithMany(e => e.DataMappings).HasForeignKey(e => e.InterfaceId).OnDelete(DeleteBehavior.Restrict);
    }
}
