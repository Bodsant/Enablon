using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IsolationLockEntity"/> (<c>cow.isolation_locks</c>).</summary>
public sealed class IsolationLockEntityConfiguration : IEntityTypeConfiguration<IsolationLockEntity>
{
    public const string TableName = "isolation_locks";

    public void Configure(EntityTypeBuilder<IsolationLockEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IsolationPointId).IsRequired();
        builder.Property(e => e.LockIdentifier).HasMaxLength(100).IsRequired();
        builder.Property(e => e.TagIdentifier).HasMaxLength(100);
        builder.Property(e => e.AppliedByPersonId).IsRequired();
        builder.Property(e => e.AppliedAt).IsRequired();
        builder.Property(e => e.RemovedByPersonId);
        builder.Property(e => e.RemovedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_isolation_locks_tenant_id");
    }
}
