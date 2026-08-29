using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitWorkerEntity"/> (<c>cow.permit_workers</c>).</summary>
public sealed class PermitWorkerEntityConfiguration : IEntityTypeConfiguration<PermitWorkerEntity>
{
    public const string TableName = "permit_workers";

    public void Configure(EntityTypeBuilder<PermitWorkerEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.WorkRole).HasMaxLength(60);
        builder.Property(e => e.EligibilityStatus).HasMaxLength(30).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_workers_tenant_id");
    }
}
