namespace Ehsms.BuildingBlocks;

/// <summary>
/// Adds version (optimistic concurrency) and classification to TenantEntity.
/// Use xmin or explicit lock_version for optimistic concurrency on mutable aggregates.
/// </summary>
public abstract class AuditableEntity : TenantEntity
{
    public long Version { get; set; }
    public Guid? ClassificationId { get; set; }
    public Guid? RetentionPolicyId { get; set; }
}
