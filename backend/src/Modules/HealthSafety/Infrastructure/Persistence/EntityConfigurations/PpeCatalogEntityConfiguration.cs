using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeCatalogEntity"/> (<c>ppe.catalog</c>).</summary>
public sealed class PpeCatalogEntityConfiguration : IEntityTypeConfiguration<PpeCatalogEntity>
{
    public const string TableName = "catalog";
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<PpeCatalogEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PpeCategory).HasMaxLength(60);
        builder.Property(e => e.InspectionIntervalDays);
        builder.Property(e => e.ReplacementIntervalDays);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_catalog_tenant_id");

    }
}
