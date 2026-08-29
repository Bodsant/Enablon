using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="DefectEntity"/> (<c>asset.defects</c>).</summary>
public sealed class DefectEntityConfiguration : IEntityTypeConfiguration<DefectEntity>
{
    public const string TableName = "defects";
    public const int SeverityMaxLength = 30;
    public const int RestrictionStatusMaxLength = 30;
    public const int MaintenanceReferenceMaxLength = 100;

    public void Configure(EntityTypeBuilder<DefectEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.AssetId).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Severity).HasMaxLength(SeverityMaxLength);
        builder.Property(e => e.RestrictionStatus).HasMaxLength(RestrictionStatusMaxLength);
        builder.Property(e => e.MaintenanceReference).HasMaxLength(MaintenanceReferenceMaxLength);
        builder.Property(e => e.OwnerMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_defects_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_defects_record_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_defects_asset_id");

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}