using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyDrillFindingEntity"/> (<c>emergency.drill_findings</c>).</summary>
public sealed class EmergencyDrillFindingEntityConfiguration : IEntityTypeConfiguration<EmergencyDrillFindingEntity>
{
    public const string TableName = "drill_findings";
    public const int SeverityMaxLength = 30;

    public void Configure(EntityTypeBuilder<EmergencyDrillFindingEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.EmergencyDrillId).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Severity).HasMaxLength(SeverityMaxLength);
        builder.Property(e => e.OwnerMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_drill_findings_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_drill_findings_record_id");
        builder.HasIndex(e => e.EmergencyDrillId).HasDatabaseName("ix_drill_findings_emergency_drill_id");

        builder.HasOne(e => e.EmergencyDrill)
            .WithMany(e => e.Findings)
            .HasForeignKey(e => e.EmergencyDrillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}