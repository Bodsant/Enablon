using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="MonitoringRecordEntity"/> (<c>environment.monitoring_records</c>).</summary>
public sealed class MonitoringRecordEntityConfiguration : IEntityTypeConfiguration<MonitoringRecordEntity>
{
    public const string TableName = "monitoring_records";
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<MonitoringRecordEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.EnvironmentSourceId).IsRequired();
        builder.Property(e => e.PeriodStart);
        builder.Property(e => e.PeriodEnd);
        builder.Property(e => e.PerformedByMemberId);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_monitoring_records_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_monitoring_records_record_id");
        builder.HasIndex(e => e.EnvironmentSourceId).HasDatabaseName("ix_monitoring_records_environment_source_id");

    }
}
