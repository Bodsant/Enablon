using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IntegrationReconciliationEntity"/> (<c>integration.reconciliations</c>).</summary>
public sealed class IntegrationReconciliationEntityConfiguration : IEntityTypeConfiguration<IntegrationReconciliationEntity>
{
    public void Configure(EntityTypeBuilder<IntegrationReconciliationEntity> builder)
    {
        builder.ToTable("reconciliations", "integration");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IntegrationRunId).IsRequired();
        builder.Property(e => e.SourceCount);
        builder.Property(e => e.TargetCount);
        builder.Property(e => e.MatchedCount);
        builder.Property(e => e.UnmatchedCount);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ApprovedByMemberId);
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_reconciliations_tenant_id");
        builder.HasIndex(e => e.IntegrationRunId).HasDatabaseName("ix_reconciliations_integration_run_id");
        builder.HasOne(e => e.IntegrationRun).WithMany(e => e.Reconciliations).HasForeignKey(e => e.IntegrationRunId).OnDelete(DeleteBehavior.Restrict);
    }
}
