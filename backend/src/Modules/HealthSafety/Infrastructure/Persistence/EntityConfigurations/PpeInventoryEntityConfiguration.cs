using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeInventoryEntity"/> (<c>ppe.inventory</c>).</summary>
public sealed class PpeInventoryEntityConfiguration : IEntityTypeConfiguration<PpeInventoryEntity>
{
    public const string TableName = "inventory";
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<PpeInventoryEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PpeCatalogId).IsRequired();
        builder.Property(e => e.SiteId);
        builder.Property(e => e.SerialNumber).HasMaxLength(100);
        builder.Property(e => e.QuantityOnHand);
        builder.Property(e => e.Condition).HasMaxLength(30);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_inventory_tenant_id");
        builder.HasIndex(e => e.PpeCatalogId).HasDatabaseName("ix_inventory_ppe_catalog_id");

    }
}
