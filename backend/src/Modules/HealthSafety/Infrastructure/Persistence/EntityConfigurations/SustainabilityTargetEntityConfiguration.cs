using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SustainabilityTargetEntity"/> (<c>sustainability.targets</c>).</summary>
public sealed class SustainabilityTargetEntityConfiguration : IEntityTypeConfiguration<SustainabilityTargetEntity>
{
    public const string TableName = "targets";

    public void Configure(EntityTypeBuilder<SustainabilityTargetEntity> builder)
    {
        builder.ToTable(TableName, "sustainability");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IndicatorDefinitionId).IsRequired();
        builder.Property(e => e.SiteId);
        builder.Property(e => e.PeriodStart);
        builder.Property(e => e.PeriodEnd);
        builder.Property(e => e.TargetValue).HasPrecision(24, 8);
        builder.Property(e => e.Unit).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_targets_tenant_id");
        builder.HasIndex(e => e.IndicatorDefinitionId).HasDatabaseName("ix_targets_indicator_definition_id");

    }
}
