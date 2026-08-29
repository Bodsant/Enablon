using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RootCauseEntity"/> (<c>incident.root_causes</c>).</summary>
public sealed class RootCauseEntityConfiguration : IEntityTypeConfiguration<RootCauseEntity>
{
    public const string TableName = "root_causes";

    public void Configure(EntityTypeBuilder<RootCauseEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.InvestigationId).IsRequired();
        builder.Property(e => e.CauseType).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CategoryId);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.EvidenceSummary);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_root_causes_tenant_id");
    }
}
