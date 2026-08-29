using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IsolationPlanEntity"/> (<c>cow.isolation_plans</c>).</summary>
public sealed class IsolationPlanEntityConfiguration : IEntityTypeConfiguration<IsolationPlanEntity>
{
    public const string TableName = "isolation_plans";

    public void Configure(EntityTypeBuilder<IsolationPlanEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.PreparedByMemberId).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_isolation_plans_tenant_id");
    }
}
