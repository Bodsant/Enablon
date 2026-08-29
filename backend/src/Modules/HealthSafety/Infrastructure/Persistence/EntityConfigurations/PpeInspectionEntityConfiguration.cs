using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeInspectionEntity"/> (<c>ppe.inspections</c>).</summary>
public sealed class PpeInspectionEntityConfiguration : IEntityTypeConfiguration<PpeInspectionEntity>
{
    public const string TableName = "inspections";
    public const int ConditionMaxLength = 30;
    public const int ResultMaxLength = 30;

    public void Configure(EntityTypeBuilder<PpeInspectionEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PpeInventoryId).IsRequired();
        builder.Property(e => e.InspectedByMemberId);
        builder.Property(e => e.InspectedAt);
        builder.Property(e => e.Condition).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Result).IsRequired().HasMaxLength(30);
        builder.Property(e => e.NextDueDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_inspections_tenant_id");
        builder.HasIndex(e => e.PpeInventoryId).HasDatabaseName("ix_inspections_ppe_inventory_id");

    }
}
