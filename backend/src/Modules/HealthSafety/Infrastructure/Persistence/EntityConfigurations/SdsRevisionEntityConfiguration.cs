using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SdsRevisionEntity"/> (<c>chemical.sds_revisions</c>).</summary>
public sealed class SdsRevisionEntityConfiguration : IEntityTypeConfiguration<SdsRevisionEntity>
{
    public const string TableName = "sds_revisions";
    public const int RevisionNumberMaxLength = 50;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<SdsRevisionEntity> builder)
    {
        builder.ToTable(TableName, "chemical");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ChemicalProductId).IsRequired();
        builder.Property(e => e.RevisionNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.EffectiveDate);
        builder.Property(e => e.FileObjectId);
        builder.Property(e => e.Language).HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_sds_revisions_tenant_id");
        builder.HasIndex(e => e.ChemicalProductId).HasDatabaseName("ix_sds_revisions_chemical_product_id");

    }
}
