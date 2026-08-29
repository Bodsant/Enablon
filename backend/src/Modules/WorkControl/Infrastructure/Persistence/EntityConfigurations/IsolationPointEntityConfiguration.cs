using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IsolationPointEntity"/> (<c>cow.isolation_points</c>).</summary>
public sealed class IsolationPointEntityConfiguration : IEntityTypeConfiguration<IsolationPointEntity>
{
    public const string TableName = "isolation_points";

    public void Configure(EntityTypeBuilder<IsolationPointEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IsolationPlanId).IsRequired();
        builder.Property(e => e.AssetId);
        builder.Property(e => e.EnergySource).HasMaxLength(80).IsRequired();
        builder.Property(e => e.IsolationMethod).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PointIdentifier).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_isolation_points_tenant_id");
    }
}
