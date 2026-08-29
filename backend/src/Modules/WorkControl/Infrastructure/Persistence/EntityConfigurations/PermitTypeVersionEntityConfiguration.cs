using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitTypeVersionEntity"/> (<c>cow.permit_type_versions</c>).</summary>
public sealed class PermitTypeVersionEntityConfiguration : IEntityTypeConfiguration<PermitTypeVersionEntity>
{
    public const string TableName = "permit_type_versions";

    public void Configure(EntityTypeBuilder<PermitTypeVersionEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitTypeId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.EffectiveFrom).IsRequired();
        builder.Property(e => e.EffectiveTo);
        builder.Property(e => e.ConfigurationJson).HasColumnType("jsonb");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_type_versions_tenant_id");
    }
}
