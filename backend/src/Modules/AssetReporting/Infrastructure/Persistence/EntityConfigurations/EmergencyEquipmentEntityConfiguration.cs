using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyEquipmentEntity"/> (<c>emergency.equipment</c>).</summary>
public sealed class EmergencyEquipmentEntityConfiguration : IEntityTypeConfiguration<EmergencyEquipmentEntity>
{
    public const string TableName = "equipment";
    public const int EquipmentTypeMaxLength = 80;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<EmergencyEquipmentEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SiteId).IsRequired();
        builder.Property(e => e.LocationId);
        builder.Property(e => e.EquipmentType).IsRequired().HasMaxLength(EquipmentTypeMaxLength);
        builder.Property(e => e.AssetId);
        builder.Property(e => e.InspectionDueDate);
        builder.Property(e => e.MaintenanceDueDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_equipment_tenant_id");
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_equipment_site_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_equipment_asset_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_equipment_tenant_id_status");
    }
}