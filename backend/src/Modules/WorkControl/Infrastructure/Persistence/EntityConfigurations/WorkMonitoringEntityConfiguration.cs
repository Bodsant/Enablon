using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkMonitoringEntity"/> (<c>cow.work_monitoring</c>).</summary>
public sealed class WorkMonitoringEntityConfiguration : IEntityTypeConfiguration<WorkMonitoringEntity>
{
    public const string TableName = "work_monitoring";

    public void Configure(EntityTypeBuilder<WorkMonitoringEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkExecutionId).IsRequired();
        builder.Property(e => e.MonitoredByMemberId).IsRequired();
        builder.Property(e => e.MonitoredAt).IsRequired();
        builder.Property(e => e.ConditionStatus).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Notes);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_work_monitoring_tenant_id");
    }
}
