using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SustainabilityMeasurementEntity"/> (<c>sustainability.measurements</c>).</summary>
public sealed class SustainabilityMeasurementEntityConfiguration : IEntityTypeConfiguration<SustainabilityMeasurementEntity>
{
    public const string TableName = "measurements";

    public void Configure(EntityTypeBuilder<SustainabilityMeasurementEntity> builder)
    {
        builder.ToTable(TableName, "sustainability");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.IndicatorDefinitionId).IsRequired();
        builder.Property(e => e.FactorVersionId);
        builder.Property(e => e.ScopeCode).HasMaxLength(20);
        builder.Property(e => e.PeriodStart);
        builder.Property(e => e.PeriodEnd);
        builder.Property(e => e.ActualValue).HasPrecision(24, 8);
        builder.Property(e => e.Unit).HasMaxLength(30);
        builder.Property(e => e.CalculationJson).HasColumnType("jsonb");
        builder.Property(e => e.OwnerMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_measurements_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_measurements_record_id");
        builder.HasIndex(e => e.IndicatorDefinitionId).HasDatabaseName("ix_measurements_indicator_definition_id");
        builder.HasIndex(e => e.FactorVersionId).HasDatabaseName("ix_measurements_factor_version_id");

    }
}
