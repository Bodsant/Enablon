using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeRequirementEntity"/> (<c>ppe.requirements</c>).</summary>
public sealed class PpeRequirementEntityConfiguration : IEntityTypeConfiguration<PpeRequirementEntity>
{
    public const string TableName = "requirements";

    public void Configure(EntityTypeBuilder<PpeRequirementEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PpeCatalogId).IsRequired();
        builder.Property(e => e.SourceRecordId);
        builder.Property(e => e.PermitTypeId);
        builder.Property(e => e.IsMandatory);
        builder.Property(e => e.Notes);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_requirements_tenant_id");
        builder.HasIndex(e => e.PpeCatalogId).HasDatabaseName("ix_requirements_ppe_catalog_id");

    }
}
