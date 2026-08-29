using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WasteRecordEntity"/> (<c>environment.waste_records</c>).</summary>
public sealed class WasteRecordEntityConfiguration : IEntityTypeConfiguration<WasteRecordEntity>
{
    public const string TableName = "waste_records";
    public const int WasteTypeMaxLength = 80;

    public void Configure(EntityTypeBuilder<WasteRecordEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.WasteType).IsRequired().HasMaxLength(80);
        builder.Property(e => e.IsHazardous);
        builder.Property(e => e.Quantity).HasPrecision(24, 8);
        builder.Property(e => e.Unit).HasMaxLength(30);
        builder.Property(e => e.SourceLocationId);
        builder.Property(e => e.HandlerName).HasMaxLength(200);
        builder.Property(e => e.ManifestNumber).HasMaxLength(100);
        builder.Property(e => e.RecordDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_waste_records_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_waste_records_record_id");

    }
}
