using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ChemicalProductEntity"/> (<c>chemical.products</c>).</summary>
public sealed class ChemicalProductEntityConfiguration : IEntityTypeConfiguration<ChemicalProductEntity>
{
    public const string TableName = "products";
    public const int ProductNameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<ChemicalProductEntity> builder)
    {
        builder.ToTable(TableName, "chemical");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ProductCode).HasMaxLength(60);
        builder.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.SupplierName).HasMaxLength(200);
        builder.Property(e => e.HazardClassificationJson).HasColumnType("jsonb");
        builder.Property(e => e.OwnerMemberId);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_products_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_products_record_id");

    }
}
