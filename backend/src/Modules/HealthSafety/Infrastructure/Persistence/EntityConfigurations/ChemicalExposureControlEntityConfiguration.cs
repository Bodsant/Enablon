using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ChemicalExposureControlEntity"/> (<c>chemical.exposure_controls</c>).</summary>
public sealed class ChemicalExposureControlEntityConfiguration : IEntityTypeConfiguration<ChemicalExposureControlEntity>
{
    public const string TableName = "exposure_controls";
    public const int ControlTypeMaxLength = 60;

    public void Configure(EntityTypeBuilder<ChemicalExposureControlEntity> builder)
    {
        builder.ToTable(TableName, "chemical");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ChemicalProductId).IsRequired();
        builder.Property(e => e.ControlType).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.SourceRecordId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_exposure_controls_tenant_id");
        builder.HasIndex(e => e.ChemicalProductId).HasDatabaseName("ix_exposure_controls_chemical_product_id");

    }
}
