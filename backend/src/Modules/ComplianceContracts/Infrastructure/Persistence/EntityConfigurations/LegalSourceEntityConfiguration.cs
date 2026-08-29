using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="LegalSourceEntity"/> (<c>compliance.legal_sources</c>).</summary>
public sealed class LegalSourceEntityConfiguration : IEntityTypeConfiguration<LegalSourceEntity>
{
    public const string TableName = "legal_sources";

    public void Configure(EntityTypeBuilder<LegalSourceEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SourceType).IsRequired().HasMaxLength(40);
        builder.Property(e => e.Code).HasMaxLength(100);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Jurisdiction).HasMaxLength(100);
        builder.Property(e => e.Publisher).HasMaxLength(200);
        builder.Property(e => e.SourceUrl);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_legal_sources_tenant_id");
    }
}
