using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ChemicalInventoryEntity"/> (<c>chemical.inventory</c>).</summary>
public sealed class ChemicalInventoryEntityConfiguration : IEntityTypeConfiguration<ChemicalInventoryEntity>
{
    public const string TableName = "inventory";

    public void Configure(EntityTypeBuilder<ChemicalInventoryEntity> builder)
    {
        builder.ToTable(TableName, "chemical");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ChemicalProductId).IsRequired();
        builder.Property(e => e.LocationId);
        builder.Property(e => e.Quantity).HasPrecision(18, 4);
        builder.Property(e => e.Unit).HasMaxLength(30);
        builder.Property(e => e.StorageCondition).HasMaxLength(100);
        builder.Property(e => e.ExpiryDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_inventory_tenant_id");
        builder.HasIndex(e => e.ChemicalProductId).HasDatabaseName("ix_inventory_chemical_product_id");

    }
}
