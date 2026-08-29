using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionScheduleEntity"/> (<c>inspection.schedules</c>).</summary>
public sealed class InspectionScheduleEntityConfiguration : IEntityTypeConfiguration<InspectionScheduleEntity>
{
    public const string TableName = "schedules";

    public void Configure(EntityTypeBuilder<InspectionScheduleEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TemplateVersionId).IsRequired();
        builder.Property(e => e.SiteId).IsRequired();
        builder.Property(e => e.LocationId);
        builder.Property(e => e.AssignedMemberId);
        builder.Property(e => e.RecurrenceRule).HasMaxLength(300);
        builder.Property(e => e.NextExecutionAt);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_schedules_tenant_id");
    }
}
