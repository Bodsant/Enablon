using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="KpiVersionEntity"/> (<c>reporting.kpi_versions</c>).</summary>
public sealed class KpiVersionEntityConfiguration : IEntityTypeConfiguration<KpiVersionEntity>
{
    public const string TableName = "kpi_versions";
    public const int PeriodRuleMaxLength = 60;

    public void Configure(EntityTypeBuilder<KpiVersionEntity> builder)
    {
        builder.ToTable(TableName, "reporting");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.KpiDefinitionId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.FormulaExpression).IsRequired();
        builder.Property(e => e.NumeratorDefinition);
        builder.Property(e => e.DenominatorDefinition);
        builder.Property(e => e.Factor).HasPrecision(24, 8);
        builder.Property(e => e.PeriodRule).HasMaxLength(PeriodRuleMaxLength);
        builder.Property(e => e.ScopeRuleJson).HasColumnType("jsonb");
        builder.Property(e => e.EffectiveFrom);
        builder.Property(e => e.EffectiveTo);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_kpi_versions_tenant_id");
        builder.HasIndex(e => e.KpiDefinitionId).HasDatabaseName("ix_kpi_versions_kpi_definition_id");

        builder.HasOne(e => e.KpiDefinition)
            .WithMany(e => e.Versions)
            .HasForeignKey(e => e.KpiDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}