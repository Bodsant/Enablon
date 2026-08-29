using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InvestigationEntity"/> (<c>incident.investigations</c>).</summary>
public sealed class InvestigationEntityConfiguration : IEntityTypeConfiguration<InvestigationEntity>
{
    public const string TableName = "investigations";

    public void Configure(EntityTypeBuilder<InvestigationEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IncidentId).IsRequired();
        builder.Property(e => e.LeadInvestigatorMemberId).IsRequired();
        builder.Property(e => e.Method).HasMaxLength(80);
        builder.Property(e => e.Summary);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.StartedAt);
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_investigations_tenant_id");
    }
}
