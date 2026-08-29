using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyPlanRevisionEntity"/> (<c>emergency.plan_revisions</c>).</summary>
public sealed class EmergencyPlanRevisionEntityConfiguration : IEntityTypeConfiguration<EmergencyPlanRevisionEntity>
{
    public const string TableName = "plan_revisions";
    public const int RevisionNumberMaxLength = 30;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<EmergencyPlanRevisionEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.EmergencyPlanId).IsRequired();
        builder.Property(e => e.RevisionNumber).IsRequired().HasMaxLength(RevisionNumberMaxLength);
        builder.Property(e => e.EffectiveDate);
        builder.Property(e => e.FileObjectId);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_plan_revisions_tenant_id");
        builder.HasIndex(e => e.EmergencyPlanId).HasDatabaseName("ix_plan_revisions_emergency_plan_id");

        builder.HasOne(e => e.EmergencyPlan)
            .WithMany(e => e.Revisions)
            .HasForeignKey(e => e.EmergencyPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}