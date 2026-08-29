using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SurveillanceEventEntity"/> (<c>health.surveillance_events</c>).</summary>
public sealed class SurveillanceEventEntityConfiguration : IEntityTypeConfiguration<SurveillanceEventEntity>
{
    public const string TableName = "surveillance_events";

    public void Configure(EntityTypeBuilder<SurveillanceEventEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.HealthProfileId).IsRequired();
        builder.Property(e => e.SurveillanceProgramId).IsRequired();
        builder.Property(e => e.ScheduledDate);
        builder.Property(e => e.CompletedDate);
        builder.Property(e => e.AuthorizedProvider).HasMaxLength(200);
        builder.Property(e => e.ResultSummaryCode).HasMaxLength(50);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_surveillance_events_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_surveillance_events_record_id");
        builder.HasIndex(e => e.HealthProfileId).HasDatabaseName("ix_surveillance_events_health_profile_id");
        builder.HasIndex(e => e.SurveillanceProgramId).HasDatabaseName("ix_surveillance_events_surveillance_program_id");

    }
}
