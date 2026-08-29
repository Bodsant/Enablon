using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EnvironmentMeasurementEntity"/> (<c>environment.measurements</c>).</summary>
public sealed class EnvironmentMeasurementEntityConfiguration : IEntityTypeConfiguration<EnvironmentMeasurementEntity>
{
    public const string TableName = "measurements";

    public void Configure(EntityTypeBuilder<EnvironmentMeasurementEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.MonitoringRecordId).IsRequired();
        builder.Property(e => e.ParameterId).IsRequired();
        builder.Property(e => e.MeasuredAt);
        builder.Property(e => e.ResultValue).HasPrecision(24, 8);
        builder.Property(e => e.Unit).HasMaxLength(30);
        builder.Property(e => e.LimitValue).HasPrecision(24, 8);
        builder.Property(e => e.TargetValue).HasPrecision(24, 8);
        builder.Property(e => e.QualityFlag).HasMaxLength(30);
        builder.Property(e => e.ComplianceStatus).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_measurements_tenant_id");
        builder.HasIndex(e => e.MonitoringRecordId).HasDatabaseName("ix_measurements_monitoring_record_id");
        builder.HasIndex(e => e.ParameterId).HasDatabaseName("ix_measurements_parameter_id");

    }
}
