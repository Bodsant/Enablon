using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IntegrationInterfaceEntity"/> (<c>integration.interfaces</c>).</summary>
public sealed class IntegrationInterfaceEntityConfiguration : IEntityTypeConfiguration<IntegrationInterfaceEntity>
{
    public void Configure(EntityTypeBuilder<IntegrationInterfaceEntity> builder)
    {
        builder.ToTable("interfaces", "integration");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.SourceSystem).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TargetSystem).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IntegrationMethod).IsRequired().HasMaxLength(30);
        builder.Property(e => e.AuthenticationType).HasMaxLength(50);
        builder.Property(e => e.OwnerMemberId);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_interfaces_tenant_id");
        builder.HasIndex(e => e.Code).HasDatabaseName("ix_interfaces_code");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_interfaces_tenant_id_status");
    }
}
