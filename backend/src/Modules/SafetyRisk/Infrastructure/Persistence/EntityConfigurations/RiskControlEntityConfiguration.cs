using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RiskControlEntity"/> (<c>risk.controls</c>).</summary>
public sealed class RiskControlEntityConfiguration : IEntityTypeConfiguration<RiskControlEntity>
{
    public const string TableName = "controls";

    public void Configure(EntityTypeBuilder<RiskControlEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RiskRegisterId).IsRequired();
        builder.Property(e => e.ControlType).IsRequired().HasMaxLength(40);
        builder.Property(e => e.ControlStage).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.OwnerMemberId);
        builder.Property(e => e.DueDate);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.EffectivenessRating);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_controls_tenant_id");
    }
}
