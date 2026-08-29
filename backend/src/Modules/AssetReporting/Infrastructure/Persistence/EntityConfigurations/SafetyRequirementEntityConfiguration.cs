using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SafetyRequirementEntity"/> (<c>asset.safety_requirements</c>).</summary>
public sealed class SafetyRequirementEntityConfiguration : IEntityTypeConfiguration<SafetyRequirementEntity>
{
    public const string TableName = "safety_requirements";
    public const int RequirementTypeMaxLength = 60;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<SafetyRequirementEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.AssetId).IsRequired();
        builder.Property(e => e.RequirementType).IsRequired().HasMaxLength(RequirementTypeMaxLength);
        builder.Property(e => e.FrequencyDays);
        builder.Property(e => e.CompetencyId);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_safety_requirements_tenant_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_safety_requirements_asset_id");

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}