using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionEntity"/> (<c>inspection.inspections</c>).</summary>
public sealed class InspectionEntityConfiguration : IEntityTypeConfiguration<InspectionEntity>
{
    public const string TableName = "inspections";

    public void Configure(EntityTypeBuilder<InspectionEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ScheduleId);
        builder.Property(e => e.TemplateVersionId).IsRequired();
        builder.Property(e => e.InspectorMemberId).IsRequired();
        builder.Property(e => e.PlannedAt);
        builder.Property(e => e.StartedAt);
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.CompliancePercentage).HasPrecision(5, 2);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_inspections_tenant_id");
    }
}
