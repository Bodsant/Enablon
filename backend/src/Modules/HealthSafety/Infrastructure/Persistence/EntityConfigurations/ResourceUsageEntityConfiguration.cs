using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ResourceUsageEntity"/> (<c>environment.resource_usage</c>).</summary>
public sealed class ResourceUsageEntityConfiguration : IEntityTypeConfiguration<ResourceUsageEntity>
{
    public const string TableName = "resource_usage";
    public const int ResourceTypeMaxLength = 50;
    public const int UnitMaxLength = 30;

    public void Configure(EntityTypeBuilder<ResourceUsageEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ResourceType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.SiteId);
        builder.Property(e => e.PeriodStart);
        builder.Property(e => e.PeriodEnd);
        builder.Property(e => e.Quantity).IsRequired().HasPrecision(24, 8);
        builder.Property(e => e.Unit).IsRequired().HasMaxLength(30);
        builder.Property(e => e.SourceReference).HasMaxLength(100);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_resource_usage_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_resource_usage_record_id");

    }
}
