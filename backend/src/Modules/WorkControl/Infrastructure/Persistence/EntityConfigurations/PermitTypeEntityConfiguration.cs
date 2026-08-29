using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitTypeEntity"/> (<c>cow.permit_types</c>).</summary>
public sealed class PermitTypeEntityConfiguration : IEntityTypeConfiguration<PermitTypeEntity>
{
    public const string TableName = "permit_types";

    public void Configure(EntityTypeBuilder<PermitTypeEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.RiskCategory).HasMaxLength(40);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_types_tenant_id");
    }
}
