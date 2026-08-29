using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InvestigationTeamMemberEntity"/> (<c>incident.investigation_team</c>).</summary>
public sealed class InvestigationTeamMemberEntityConfiguration : IEntityTypeConfiguration<InvestigationTeamMemberEntity>
{
    public const string TableName = "investigation_team";

    public void Configure(EntityTypeBuilder<InvestigationTeamMemberEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.InvestigationId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.TeamRole).HasMaxLength(80);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_investigation_team_tenant_id");
    }
}
