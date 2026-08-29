using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyDrillEntity"/> (<c>emergency.drills</c>).</summary>
public sealed class EmergencyDrillEntityConfiguration : IEntityTypeConfiguration<EmergencyDrillEntity>
{
    public const string TableName = "drills";

    public void Configure(EntityTypeBuilder<EmergencyDrillEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.EmergencyPlanId).IsRequired();
        builder.Property(e => e.Scenario).IsRequired();
        builder.Property(e => e.ScheduledAt);
        builder.Property(e => e.ConductedAt);
        builder.Property(e => e.ResultSummary);
        builder.Property(e => e.CoordinatorMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_drills_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_drills_record_id");
        builder.HasIndex(e => e.EmergencyPlanId).HasDatabaseName("ix_drills_emergency_plan_id");
        builder.HasIndex(e => e.ScheduledAt).HasDatabaseName("ix_drills_scheduled_at");

        builder.HasOne(e => e.EmergencyPlan)
            .WithMany(e => e.Drills)
            .HasForeignKey(e => e.EmergencyPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}