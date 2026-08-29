using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SurveillanceProgramEntity"/> (<c>health.surveillance_programs</c>).</summary>
public sealed class SurveillanceProgramEntityConfiguration : IEntityTypeConfiguration<SurveillanceProgramEntity>
{
    public const string TableName = "surveillance_programs";
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<SurveillanceProgramEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ExposureType).HasMaxLength(100);
        builder.Property(e => e.FrequencyMonths);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_surveillance_programs_tenant_id");

    }
}
