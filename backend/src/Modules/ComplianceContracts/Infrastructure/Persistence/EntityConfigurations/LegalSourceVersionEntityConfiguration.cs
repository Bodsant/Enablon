using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="LegalSourceVersionEntity"/> (<c>compliance.legal_source_versions</c>).</summary>
public sealed class LegalSourceVersionEntityConfiguration : IEntityTypeConfiguration<LegalSourceVersionEntity>
{
    public const string TableName = "legal_source_versions";

    public void Configure(EntityTypeBuilder<LegalSourceVersionEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.LegalSourceId).IsRequired();
        builder.Property(e => e.VersionLabel).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PublishedDate);
        builder.Property(e => e.EffectiveDate);
        builder.Property(e => e.SupersededDate);
        builder.Property(e => e.ChangeSummary);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_legal_source_versions_tenant_id");
        builder.HasIndex(e => e.LegalSourceId).HasDatabaseName("ix_legal_source_versions_legal_source_id");
    }
}
