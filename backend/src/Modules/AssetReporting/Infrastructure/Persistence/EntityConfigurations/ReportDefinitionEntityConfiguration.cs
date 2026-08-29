using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ReportDefinitionEntity"/> (<c>reporting.report_definitions</c>).</summary>
public sealed class ReportDefinitionEntityConfiguration : IEntityTypeConfiguration<ReportDefinitionEntity>
{
    public const string TableName = "report_definitions";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 200;
    public const int ReportTypeMaxLength = 40;
    public const int DatasetCodeMaxLength = 80;

    public void Configure(EntityTypeBuilder<ReportDefinitionEntity> builder)
    {
        builder.ToTable(TableName, "reporting");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.ReportType).IsRequired().HasMaxLength(ReportTypeMaxLength);
        builder.Property(e => e.DatasetCode).IsRequired().HasMaxLength(DatasetCodeMaxLength);
        builder.Property(e => e.FilterSchemaJson).HasColumnType("jsonb");
        builder.Property(e => e.RequiredPermissionId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_report_definitions_tenant_id");
        builder.HasIndex(e => e.Code).HasDatabaseName("ix_report_definitions_code");
        builder.HasIndex(e => new { e.TenantId, e.ReportType }).HasDatabaseName("ix_report_definitions_tenant_id_report_type");
    }
}