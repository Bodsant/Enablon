using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskRegisterEntity"/> (<c>risk.registers</c>).</summary>
public sealed class RiskRegisterEntityConfiguration : IEntityTypeConfiguration<RiskRegisterEntity>
{
    public const string TableName = "registers";

    public void Configure(EntityTypeBuilder<RiskRegisterEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.HazardId).IsRequired();
        builder.Property(e => e.ActivityName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.RiskEvent).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.ReviewDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_registers_tenant_id");
    }
}
