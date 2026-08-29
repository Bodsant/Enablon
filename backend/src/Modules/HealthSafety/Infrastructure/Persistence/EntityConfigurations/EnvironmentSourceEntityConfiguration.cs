using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EnvironmentSourceEntity"/> (<c>environment.sources</c>).</summary>
public sealed class EnvironmentSourceEntityConfiguration : IEntityTypeConfiguration<EnvironmentSourceEntity>
{
    public const string TableName = "sources";
    public const int SourceTypeMaxLength = 60;
    public const int NameMaxLength = 200;

    public void Configure(EntityTypeBuilder<EnvironmentSourceEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SiteId);
        builder.Property(e => e.LocationId);
        builder.Property(e => e.SourceType).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PermitReference).HasMaxLength(100);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_sources_tenant_id");

    }
}
