using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IntegrationMessageEntity"/> (<c>integration.messages</c>).</summary>
public sealed class IntegrationMessageEntityConfiguration : IEntityTypeConfiguration<IntegrationMessageEntity>
{
    public void Configure(EntityTypeBuilder<IntegrationMessageEntity> builder)
    {
        builder.ToTable("messages", "integration");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IntegrationRunId).IsRequired();
        builder.Property(e => e.ExternalKey).HasMaxLength(200);
        builder.Property(e => e.PayloadHash).HasMaxLength(64);
        builder.Property(e => e.ProcessingStatus).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ErrorCode).HasMaxLength(80);
        builder.Property(e => e.ErrorMessage);
        builder.Property(e => e.RetryCount).IsRequired();
        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_messages_tenant_id");
        builder.HasIndex(e => e.IntegrationRunId).HasDatabaseName("ix_messages_integration_run_id");
        builder.HasIndex(e => e.ExternalKey).HasDatabaseName("ix_messages_external_key");
        builder.HasOne(e => e.IntegrationRun).WithMany(e => e.Messages).HasForeignKey(e => e.IntegrationRunId).OnDelete(DeleteBehavior.Restrict);
    }
}
