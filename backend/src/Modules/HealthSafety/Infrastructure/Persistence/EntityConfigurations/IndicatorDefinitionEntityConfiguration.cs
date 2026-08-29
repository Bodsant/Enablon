using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IndicatorDefinitionEntity"/> (<c>sustainability.indicator_definitions</c>).</summary>
public sealed class IndicatorDefinitionEntityConfiguration : IEntityTypeConfiguration<IndicatorDefinitionEntity>
{
    public const string TableName = "indicator_definitions";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<IndicatorDefinitionEntity> builder)
    {
        builder.ToTable(TableName, "sustainability");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BoundaryDefinition);
        builder.Property(e => e.DefaultUnit).HasMaxLength(30);
        builder.Property(e => e.FrameworkReference).HasMaxLength(150);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_indicator_definitions_tenant_id");

    }
}
