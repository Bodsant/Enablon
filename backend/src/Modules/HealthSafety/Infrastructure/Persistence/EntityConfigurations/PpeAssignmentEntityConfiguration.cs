using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeAssignmentEntity"/> (<c>ppe.assignments</c>).</summary>
public sealed class PpeAssignmentEntityConfiguration : IEntityTypeConfiguration<PpeAssignmentEntity>
{
    public const string TableName = "assignments";

    public void Configure(EntityTypeBuilder<PpeAssignmentEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PpeInventoryId).IsRequired();
        builder.Property(e => e.PersonId);
        builder.Property(e => e.IssuedAt);
        builder.Property(e => e.IssuedByMemberId);
        builder.Property(e => e.ReturnedAt);
        builder.Property(e => e.ConditionOnReturn).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_assignments_tenant_id");
        builder.HasIndex(e => e.PpeInventoryId).HasDatabaseName("ix_assignments_ppe_inventory_id");

    }
}
