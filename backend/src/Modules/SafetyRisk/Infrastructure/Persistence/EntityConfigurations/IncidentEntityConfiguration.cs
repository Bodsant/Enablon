using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IncidentEntity"/> (<c>incident.incidents</c>).</summary>
public sealed class IncidentEntityConfiguration : IEntityTypeConfiguration<IncidentEntity>
{
    public const string TableName = "incidents";

    public void Configure(EntityTypeBuilder<IncidentEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.IncidentTypeId).IsRequired();
        builder.Property(e => e.SeverityId).IsRequired();
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.ReportedAt).IsRequired();
        builder.Property(e => e.ReportedByMemberId).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.ImmediateAction);
        builder.Property(e => e.ClassificationStatus).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_incidents_tenant_id");
    }
}
