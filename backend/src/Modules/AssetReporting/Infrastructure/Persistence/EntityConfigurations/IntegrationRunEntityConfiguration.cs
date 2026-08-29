using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IntegrationRunEntity"/> (<c>integration.runs</c>).</summary>
public sealed class IntegrationRunEntityConfiguration : IEntityTypeConfiguration<IntegrationRunEntity>
{
    public void Configure(EntityTypeBuilder<IntegrationRunEntity> builder)
    {
        builder.ToTable("runs", "integration");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.InterfaceId).IsRequired();
        builder.Property(e => e.MappingId);
        builder.Property(e => e.CorrelationId).HasMaxLength(100);
        builder.Property(e => e.StartedAt).IsRequired();
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ReceivedCount);
        builder.Property(e => e.SuccessCount);
        builder.Property(e => e.ErrorCount);
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_runs_tenant_id");
        builder.HasIndex(e => e.InterfaceId).HasDatabaseName("ix_runs_interface_id");
        builder.HasIndex(e => e.CorrelationId).HasDatabaseName("ix_runs_correlation_id");
        builder.HasOne(e => e.Interface).WithMany(e => e.Runs).HasForeignKey(e => e.InterfaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Mapping).WithMany(e => e.Runs).HasForeignKey(e => e.MappingId).OnDelete(DeleteBehavior.Restrict);
    }
}
