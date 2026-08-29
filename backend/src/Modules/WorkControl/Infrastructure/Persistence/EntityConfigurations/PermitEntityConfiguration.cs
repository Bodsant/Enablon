using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitEntity"/> (<c>cow.permits</c>).</summary>
public sealed class PermitEntityConfiguration : IEntityTypeConfiguration<PermitEntity>
{
    public const string TableName = "permits";

    public void Configure(EntityTypeBuilder<PermitEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.WorkRequestId).IsRequired();
        builder.Property(e => e.JsaId);
        builder.Property(e => e.PermitTypeVersionId).IsRequired();
        builder.Property(e => e.RequesterMemberId).IsRequired();
        builder.Property(e => e.ExecutorPersonId);
        builder.Property(e => e.ContractorCompanyId);
        builder.Property(e => e.ValidFrom);
        builder.Property(e => e.ValidUntil);
        builder.Property(e => e.SuspensionReason);
        builder.Property(e => e.ExtensionCount).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permits_tenant_id");
    }
}
