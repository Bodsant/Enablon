using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditTeamMemberEntity"/> (<c>audit.team_members</c>).</summary>
public sealed class AuditTeamMemberEntityConfiguration : IEntityTypeConfiguration<AuditTeamMemberEntity>
{
    public const string TableName = "team_members";

    public void Configure(EntityTypeBuilder<AuditTeamMemberEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.AuditId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.AuditRole).HasMaxLength(60);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_team_members_tenant_id");
    }
}
