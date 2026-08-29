using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionEntity"/> (<c>asset.inspections</c>).</summary>
public sealed class InspectionEntityConfiguration : IEntityTypeConfiguration<InspectionEntity>
{
    public const string TableName = "inspections";
    public const int InspectionTypeMaxLength = 60;
    public const int ResultMaxLength = 30;

    public void Configure(EntityTypeBuilder<InspectionEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.AssetId).IsRequired();
        builder.Property(e => e.InspectionType).IsRequired().HasMaxLength(InspectionTypeMaxLength);
        builder.Property(e => e.InspectedAt);
        builder.Property(e => e.InspectedByPersonId);
        builder.Property(e => e.Result).IsRequired().HasMaxLength(ResultMaxLength);
        builder.Property(e => e.NextDueDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_inspections_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_inspections_record_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_inspections_asset_id");
        builder.HasIndex(e => e.NextDueDate).HasDatabaseName("ix_inspections_next_due_date");

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}