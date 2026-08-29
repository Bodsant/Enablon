using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="HazardEntity"/> (<c>risk.hazards</c>).</summary>
public sealed class HazardEntityConfiguration : IEntityTypeConfiguration<HazardEntity>
{
    public const string TableName = "hazards";

    public void Configure(EntityTypeBuilder<HazardEntity> builder)
    {
        builder.ToTable(TableName, "risk");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CategoryId);
        builder.Property(e => e.Description);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_hazards_tenant_id");
    }
}
