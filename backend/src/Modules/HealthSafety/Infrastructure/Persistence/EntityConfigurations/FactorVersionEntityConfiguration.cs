using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="FactorVersionEntity"/> (<c>sustainability.factor_versions</c>).</summary>
public sealed class FactorVersionEntityConfiguration : IEntityTypeConfiguration<FactorVersionEntity>
{
    public const string TableName = "factor_versions";
    public const int FactorCodeMaxLength = 80;
    public const int UnitMaxLength = 60;

    public void Configure(EntityTypeBuilder<FactorVersionEntity> builder)
    {
        builder.ToTable(TableName, "sustainability");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.FactorCode).IsRequired().HasMaxLength(80);
        builder.Property(e => e.VersionNumber);
        builder.Property(e => e.FactorValue).IsRequired().HasPrecision(24, 10);
        builder.Property(e => e.Unit).IsRequired().HasMaxLength(60);
        builder.Property(e => e.SourceReference);
        builder.Property(e => e.EffectiveFrom);
        builder.Property(e => e.EffectiveTo);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_factor_versions_tenant_id");

    }
}
