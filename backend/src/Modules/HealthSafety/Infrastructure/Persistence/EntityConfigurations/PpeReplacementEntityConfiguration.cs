using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PpeReplacementEntity"/> (<c>ppe.replacements</c>).</summary>
public sealed class PpeReplacementEntityConfiguration : IEntityTypeConfiguration<PpeReplacementEntity>
{
    public const string TableName = "replacements";
    public const int ReplacementReasonMaxLength = 80;

    public void Configure(EntityTypeBuilder<PpeReplacementEntity> builder)
    {
        builder.ToTable(TableName, "ppe");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PpeAssignmentId).IsRequired();
        builder.Property(e => e.ReplacementReason).IsRequired().HasMaxLength(80);
        builder.Property(e => e.RequestedAt);
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_replacements_tenant_id");
        builder.HasIndex(e => e.PpeAssignmentId).HasDatabaseName("ix_replacements_ppe_assignment_id");

    }
}
