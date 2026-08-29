using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="GasTestEntity"/> (<c>cow.gas_tests</c>).</summary>
public sealed class GasTestEntityConfiguration : IEntityTypeConfiguration<GasTestEntity>
{
    public const string TableName = "gas_tests";

    public void Configure(EntityTypeBuilder<GasTestEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.TestType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TestedAt).IsRequired();
        builder.Property(e => e.TestedByPersonId);
        builder.Property(e => e.OxygenPct).HasPrecision(6, 3);
        builder.Property(e => e.LelPct).HasPrecision(6, 3);
        builder.Property(e => e.ToxicGasJson).HasColumnType("jsonb");
        builder.Property(e => e.Result).HasMaxLength(30).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_gas_tests_tenant_id");
    }
}
