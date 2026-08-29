using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ChemicalStorageInspectionEntity"/> (<c>chemical.storage_inspections</c>).</summary>
public sealed class ChemicalStorageInspectionEntityConfiguration : IEntityTypeConfiguration<ChemicalStorageInspectionEntity>
{
    public const string TableName = "storage_inspections";
    public const int ResultMaxLength = 30;

    public void Configure(EntityTypeBuilder<ChemicalStorageInspectionEntity> builder)
    {
        builder.ToTable(TableName, "chemical");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ChemicalInventoryId).IsRequired();
        builder.Property(e => e.InspectedByMemberId);
        builder.Property(e => e.InspectedAt);
        builder.Property(e => e.Result).IsRequired().HasMaxLength(30);
        builder.Property(e => e.NextReviewDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_storage_inspections_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_storage_inspections_record_id");
        builder.HasIndex(e => e.ChemicalInventoryId).HasDatabaseName("ix_storage_inspections_chemical_inventory_id");

    }
}
