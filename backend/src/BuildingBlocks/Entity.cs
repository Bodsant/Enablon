namespace Ehsms.BuildingBlocks;

public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Base for all tenant-owned aggregate roots.
/// Every tenant-owned table MUST have tenant_id NOT NULL.
/// </summary>
public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// For versioned configuration entities (workflow definitions, matrix versions, templates).
/// Published versions are immutable.
/// </summary>
public abstract class VersionedEntity : TenantEntity
{
    public int VersionNumber { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
}
