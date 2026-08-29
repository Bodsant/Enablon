using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskMatrixVersionEntity"/> (<c>risk.matrix_versions</c>).</summary>
public sealed class RiskMatrixVersionEntityConfiguration : IEntityTypeConfiguration<RiskMatrixVersionEntity>
{
    public const string TableName = "matrix_versions";

    public void Configure(EntityTypeBuilder<RiskMatrixVersionEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.LikelihoodScale).IsRequired();
        builder.Property(e => e.SeverityScale).IsRequired();
        builder.Property(e => e.EffectiveFrom).IsRequired();
        builder.Property(e => e.EffectiveTo);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_matrix_versions_tenant_id");
    }
}
