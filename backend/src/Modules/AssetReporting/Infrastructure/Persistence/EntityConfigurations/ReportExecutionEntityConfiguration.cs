using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ReportExecutionEntity"/> (<c>reporting.report_executions</c>).</summary>
public sealed class ReportExecutionEntityConfiguration : IEntityTypeConfiguration<ReportExecutionEntity>
{
    public const string TableName = "report_executions";
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<ReportExecutionEntity> builder)
    {
        builder.ToTable(TableName, "reporting");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ReportDefinitionId).IsRequired();
        builder.Property(e => e.ReportScheduleId);
        builder.Property(e => e.RequestedByMemberId);
        builder.Property(e => e.FilterValuesJson).HasColumnType("jsonb");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.OutputFileObjectId);
        builder.Property(e => e.StartedAt);
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_report_executions_tenant_id");
        builder.HasIndex(e => e.ReportDefinitionId).HasDatabaseName("ix_report_executions_report_definition_id");
        builder.HasIndex(e => e.ReportScheduleId).HasDatabaseName("ix_report_executions_report_schedule_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_report_executions_tenant_id_status");

        builder.HasOne(e => e.ReportDefinition)
            .WithMany(e => e.Executions)
            .HasForeignKey(e => e.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReportSchedule)
            .WithMany(e => e.Executions)
            .HasForeignKey(e => e.ReportScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}