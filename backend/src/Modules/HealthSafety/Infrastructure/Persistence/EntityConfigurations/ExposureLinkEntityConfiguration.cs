using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ExposureLinkEntity"/> (<c>health.exposure_links</c>).</summary>
public sealed class ExposureLinkEntityConfiguration : IEntityTypeConfiguration<ExposureLinkEntity>
{
    public const string TableName = "exposure_links";
    public const int ExposureTypeMaxLength = 100;

    public void Configure(EntityTypeBuilder<ExposureLinkEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.HealthProfileId).IsRequired();
        builder.Property(e => e.SourceRecordId);
        builder.Property(e => e.ExposureType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ExposurePeriodStart);
        builder.Property(e => e.ExposurePeriodEnd);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_exposure_links_tenant_id");
        builder.HasIndex(e => e.HealthProfileId).HasDatabaseName("ix_exposure_links_health_profile_id");

    }
}
