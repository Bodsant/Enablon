using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CapaUpdateEntity"/> (<c>capa.updates</c>).</summary>
public sealed class CapaUpdateEntityConfiguration : IEntityTypeConfiguration<CapaUpdateEntity>
{
    public const string TableName = "updates";

    public void Configure(EntityTypeBuilder<CapaUpdateEntity> builder)
    {
        builder.ToTable(TableName, "capa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ActionId).IsRequired();
        builder.Property(e => e.ProgressPercentage).IsRequired();
        builder.Property(e => e.Note).IsRequired();
        builder.Property(e => e.UpdatedByMemberId).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_updates_tenant_id");
    }
}
