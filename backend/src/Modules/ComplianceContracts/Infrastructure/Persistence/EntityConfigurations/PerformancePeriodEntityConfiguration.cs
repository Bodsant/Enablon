using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PerformancePeriodEntity"/> (<c>contractor.performance_periods</c>).</summary>
public sealed class PerformancePeriodEntityConfiguration : IEntityTypeConfiguration<PerformancePeriodEntity>
{
    public const string TableName = "performance_periods";

    public void Configure(EntityTypeBuilder<PerformancePeriodEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ContractorCompanyId).IsRequired();
        builder.Property(e => e.PeriodStart).IsRequired();
        builder.Property(e => e.PeriodEnd).IsRequired();
        builder.Property(e => e.IndicatorValuesJson).HasColumnType("jsonb");
        builder.Property(e => e.OverallRating).HasPrecision(10, 2);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_performance_periods_tenant_id");
        builder.HasIndex(e => e.ContractorCompanyId).HasDatabaseName("ix_performance_periods_contractor_company_id");
    }
}
