using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="HealthFollowupEntity"/> (<c>health.followups</c>).</summary>
public sealed class HealthFollowupEntityConfiguration : IEntityTypeConfiguration<HealthFollowupEntity>
{
    public const string TableName = "followups";
    public const int FollowupTypeMaxLength = 60;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<HealthFollowupEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SurveillanceEventId).IsRequired();
        builder.Property(e => e.FollowupType).IsRequired().HasMaxLength(60);
        builder.Property(e => e.DueDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.AssignedMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_followups_tenant_id");
        builder.HasIndex(e => e.SurveillanceEventId).HasDatabaseName("ix_followups_surveillance_event_id");

    }
}
