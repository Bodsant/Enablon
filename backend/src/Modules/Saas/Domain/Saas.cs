using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Saas.Domain;

public class SubscriptionPlan : Entity
{
    public string Name { get; set; } = string.Empty; // Regular/Advance/Premium
    public int MaxActiveUsers { get; set; }
    public long TotalStorageBytes { get; set; }
    public long UploadPerPeriodBytes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PlanVersion : VersionedEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public string? Features { get; set; } // JSONB
}

public class Tenant : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? PrimaryDomain { get; set; }
    public string? DefaultTimezone { get; set; } // IANA
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class TenantSubscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanVersionId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? NextResetAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TenantStorageUsage
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public long ActiveBytes { get; set; }
    public long RecycleBinBytes { get; set; }
    public long QuarantinedBytes { get; set; }
    public long ReservedBytes { get; set; }
    public long TotalBytes => ActiveBytes + RecycleBinBytes + QuarantinedBytes + ReservedBytes;
    public DateTime UpdatedAt { get; set; }
}

public class UsageEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty; // upload/download/purge
    public long Bytes { get; set; }
    public Guid? FileObjectId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Metadata { get; set; }
}

public class UploadSession
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public long ReservedBytes { get; set; }
    public string Status { get; set; } = "RESERVED";
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
