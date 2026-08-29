using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="KpiDefinitionEntity"/> (<c>reporting.kpi_definitions</c>).</summary>
public sealed class KpiDefinitionEntityConfiguration : IEntityTypeConfiguration<KpiDefinitionEntity>
{
    public const string TableName = "kpi_definitions";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<KpiDefinitionEntity> builder)
    {
        builder.ToTable(TableName, "reporting");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.Description);
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_kpi_definitions_tenant_id");
        builder.HasIndex(e => e.Code).HasDatabaseName("ix_kpi_definitions_code");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_kpi_definitions_tenant_id_status");
    }
}