using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EnvironmentParameterEntity"/> (<c>environment.parameters</c>).</summary>
public sealed class EnvironmentParameterEntityConfiguration : IEntityTypeConfiguration<EnvironmentParameterEntity>
{
    public const string TableName = "parameters";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 200;
    public const int CategoryMaxLength = 60;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<EnvironmentParameterEntity> builder)
    {
        builder.ToTable(TableName, "environment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(60);
        builder.Property(e => e.DefaultUnit).HasMaxLength(30);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_parameters_tenant_id");

    }
}
