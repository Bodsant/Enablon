using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ReportScheduleEntity"/> (<c>reporting.report_schedules</c>).</summary>
public sealed class ReportScheduleEntityConfiguration : IEntityTypeConfiguration<ReportScheduleEntity>
{
    public const string TableName = "report_schedules";
    public const int ScheduleRuleMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<ReportScheduleEntity> builder)
    {
        builder.ToTable(TableName, "reporting");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ReportDefinitionId).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.ScheduleRule).IsRequired().HasMaxLength(ScheduleRuleMaxLength);
        builder.Property(e => e.DeliveryConfigurationJson).HasColumnType("jsonb");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_report_schedules_tenant_id");
        builder.HasIndex(e => e.ReportDefinitionId).HasDatabaseName("ix_report_schedules_report_definition_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_report_schedules_tenant_id_status");

        builder.HasOne(e => e.ReportDefinition)
            .WithMany(e => e.Schedules)
            .HasForeignKey(e => e.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}