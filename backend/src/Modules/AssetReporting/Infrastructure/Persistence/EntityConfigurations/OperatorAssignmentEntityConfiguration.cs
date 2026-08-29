using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="OperatorAssignmentEntity"/> (<c>asset.operator_assignments</c>).</summary>
public sealed class OperatorAssignmentEntityConfiguration : IEntityTypeConfiguration<OperatorAssignmentEntity>
{
    public const string TableName = "operator_assignments";
    public const int EligibilityStatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<OperatorAssignmentEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.AssetId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.AssignedFrom);
        builder.Property(e => e.AssignedTo);
        builder.Property(e => e.EligibilityStatus).HasMaxLength(EligibilityStatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_operator_assignments_tenant_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_operator_assignments_asset_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_operator_assignments_person_id");
        builder.HasIndex(e => new { e.AssetId, e.PersonId }).HasDatabaseName("ix_operator_assignments_asset_id_person_id");

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}