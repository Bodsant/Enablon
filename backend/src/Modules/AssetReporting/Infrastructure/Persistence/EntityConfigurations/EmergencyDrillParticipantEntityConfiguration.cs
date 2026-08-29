using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyDrillParticipantEntity"/> (<c>emergency.drill_participants</c>).</summary>
public sealed class EmergencyDrillParticipantEntityConfiguration : IEntityTypeConfiguration<EmergencyDrillParticipantEntity>
{
    public const string TableName = "drill_participants";
    public const int ParticipantRoleMaxLength = 80;
    public const int AttendanceStatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<EmergencyDrillParticipantEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.EmergencyDrillId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.ParticipantRole).HasMaxLength(ParticipantRoleMaxLength);
        builder.Property(e => e.AttendanceStatus).HasMaxLength(AttendanceStatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_drill_participants_tenant_id");
        builder.HasIndex(e => e.EmergencyDrillId).HasDatabaseName("ix_drill_participants_emergency_drill_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_drill_participants_person_id");

        builder.HasOne(e => e.EmergencyDrill)
            .WithMany(e => e.Participants)
            .HasForeignKey(e => e.EmergencyDrillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}