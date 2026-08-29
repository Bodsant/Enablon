using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AssetEntity"/> (<c>asset.assets</c>).</summary>
public sealed class AssetEntityConfiguration : IEntityTypeConfiguration<AssetEntity>
{
    public const string TableName = "assets";
    public const int SourceSystemMaxLength = 60;
    public const int SourceIdMaxLength = 100;
    public const int AssetCodeMaxLength = 80;
    public const int AssetNameMaxLength = 200;
    public const int AssetTypeMaxLength = 80;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<AssetEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.SourceSystem).HasMaxLength(SourceSystemMaxLength);
        builder.Property(e => e.SourceId).HasMaxLength(SourceIdMaxLength);
        builder.Property(e => e.AssetCode).IsRequired().HasMaxLength(AssetCodeMaxLength);
        builder.Property(e => e.AssetName).IsRequired().HasMaxLength(AssetNameMaxLength);
        builder.Property(e => e.AssetType).HasMaxLength(AssetTypeMaxLength);
        builder.Property(e => e.SiteId).IsRequired();
        builder.Property(e => e.LocationId);
        builder.Property(e => e.IsSafetyCritical).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_assets_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_assets_record_id");
        builder.HasIndex(e => e.AssetCode).HasDatabaseName("ix_assets_asset_code");
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_assets_site_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_assets_tenant_id_status");
    }
}