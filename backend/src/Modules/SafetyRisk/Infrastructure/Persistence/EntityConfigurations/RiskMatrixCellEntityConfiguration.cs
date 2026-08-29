using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskMatrixCellEntity"/> (<c>risk.matrix_cells</c>).</summary>
public sealed class RiskMatrixCellEntityConfiguration : IEntityTypeConfiguration<RiskMatrixCellEntity>
{
    public const string TableName = "matrix_cells";

    public void Configure(EntityTypeBuilder<RiskMatrixCellEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.MatrixVersionId).IsRequired();
        builder.Property(e => e.LikelihoodValue).IsRequired();
        builder.Property(e => e.SeverityValue).IsRequired();
        builder.Property(e => e.RiskScore).IsRequired();
        builder.Property(e => e.RiskLevelCode).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_matrix_cells_tenant_id");
    }
}
