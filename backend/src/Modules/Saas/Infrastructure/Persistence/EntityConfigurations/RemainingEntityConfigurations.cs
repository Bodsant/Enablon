using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Saas.Infrastructure.Persistence.EntityConfigurations;

public sealed class TenantEntityConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Timezone).HasMaxLength(60).IsRequired();
        b.Property(x => x.BillingAnchorDay).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.HasIndex(x => x.TenantCode).IsUnique();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class TenantSubscriptionEntityConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> b)
    {
        b.ToTable("tenant_subscriptions", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.PlanVersionId).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.StartedAt).IsRequired();
        // Properties not present in the current TenantSubscription entity are omitted.
        // The entity only contains basic subscription metadata and override fields.
        // If additional period fields are needed later, extend the entity accordingly.

        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.PlanVersionId);
    }
}

public sealed class TenantStorageUsageEntityConfiguration : IEntityTypeConfiguration<TenantStorageUsage>
{
    public void Configure(EntityTypeBuilder<TenantStorageUsage> b)
    {
        b.ToTable("tenant_storage_usage", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.ActiveBytes).IsRequired();
        b.Property(x => x.RecycleBinBytes).IsRequired();
        b.Property(x => x.QuarantinedBytes).IsRequired();
        b.Property(x => x.ReservedBytes).IsRequired();
        b.Property(x => x.ObjectCount).IsRequired();
        b.Property(x => x.LockVersion).IsRequired();
        b.Property(x => x.ReconciledAt);
        b.HasIndex(x => x.TenantId).IsUnique();
    }
}

public sealed class TenantUsagePeriodEntityConfiguration : IEntityTypeConfiguration<TenantUsagePeriod>
{
    public void Configure(EntityTypeBuilder<TenantUsagePeriod> b)
    {
        b.ToTable("tenant_usage_periods", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.TenantSubscriptionId).IsRequired();
        b.Property(x => x.PeriodStart).IsRequired();
        b.Property(x => x.PeriodEnd).IsRequired();
        b.Property(x => x.UploadedBytes).IsRequired();
        b.Property(x => x.ReservedUploadBytes).IsRequired();
        b.Property(x => x.UploadCount).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.LockVersion).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.TenantSubscriptionId);
    }
}

public sealed class UsageEventEntityConfiguration : IEntityTypeConfiguration<UsageEvent>
{
    public void Configure(EntityTypeBuilder<UsageEvent> b)
    {
        b.ToTable("usage_events", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.UsagePeriodId);
        b.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        b.Property(x => x.ReferenceId);
        b.Property(x => x.StorageBytesDelta).IsRequired();
        b.Property(x => x.UploadBytesDelta).IsRequired();
        b.Property(x => x.MetadataJson).HasColumnType("jsonb");
        b.Property(x => x.OccurredAt).IsRequired();
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.UsagePeriodId);
    }
}

public sealed class UploadSessionEntityConfiguration : IEntityTypeConfiguration<UploadSession>
{
    public void Configure(EntityTypeBuilder<UploadSession> b)
    {
        b.ToTable("upload_sessions", "saas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenantId).IsRequired();
        b.Property(x => x.UsagePeriodId).IsRequired();
        b.Property(x => x.RequestedByUserId).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        b.Property(x => x.MimeType).HasMaxLength(150).IsRequired();
        b.Property(x => x.RequestedSizeBytes).IsRequired();
        b.Property(x => x.ObjectKey).HasMaxLength(600).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ExpiresAt).IsRequired();
        b.Property(x => x.CompletedAt);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.UsagePeriodId);
        b.HasIndex(x => x.ObjectKey).IsUnique();
    }
}