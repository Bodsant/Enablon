using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Saas.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PlanVersion"/> (<c>saas.plan_versions</c>).</summary>
public sealed class PlanVersionEntityConfiguration : IEntityTypeConfiguration<PlanVersion>
{
    public const string TableName = "plan_versions";
    public const int MaxLength = 100;

    public void Configure(EntityTypeBuilder<PlanVersion> builder)
    {
        builder.ToTable(TableName, "saas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SubscriptionPlanId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.MaxActiveUsers).IsRequired();
        builder.Property(e => e.MaxCompanies);
        builder.Property(e => e.MaxBusinessUnits);
        builder.Property(e => e.MaxSites);
        builder.Property(e => e.MaxStorageBytes).IsRequired();
        builder.Property(e => e.MaxPeriodUploadBytes).IsRequired();
        builder.Property(e => e.MaxFileSizeBytes).IsRequired();
        builder.Property(e => e.EffectiveFrom).IsRequired();
        builder.Property(e => e.EffectiveUntil);
        builder.Property(e => e.IsCurrent).IsRequired();

        builder.HasIndex(e => e.SubscriptionPlanId).HasDatabaseName("ix_plan_versions_subscription_plan_id");
        builder.HasIndex(e => new { e.SubscriptionPlanId, e.VersionNumber }).IsUnique().HasDatabaseName("ix_plan_versions_plan_version_unique");
        builder.HasIndex(e => e.IsCurrent).HasDatabaseName("ix_plan_versions_is_current");
    }
}