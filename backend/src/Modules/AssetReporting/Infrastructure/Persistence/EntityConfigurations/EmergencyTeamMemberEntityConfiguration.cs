using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyTeamMemberEntity"/> (<c>emergency.team_members</c>).</summary>
public sealed class EmergencyTeamMemberEntityConfiguration : IEntityTypeConfiguration<EmergencyTeamMemberEntity>
{
    public const string TableName = "team_members";
    public const int EmergencyRoleMaxLength = 80;

    public void Configure(EntityTypeBuilder<EmergencyTeamMemberEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.EmergencyPlanId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.EmergencyRole).IsRequired().HasMaxLength(EmergencyRoleMaxLength);
        builder.Property(e => e.ValidFrom);
        builder.Property(e => e.ValidTo);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_team_members_tenant_id");
        builder.HasIndex(e => e.EmergencyPlanId).HasDatabaseName("ix_team_members_emergency_plan_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_team_members_person_id");

        builder.HasOne(e => e.EmergencyPlan)
            .WithMany(e => e.TeamMembers)
            .HasForeignKey(e => e.EmergencyPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}