namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.isolation_locks</c> table.</summary>
public sealed class IsolationLockEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IsolationPointId { get; set; }
    public string LockIdentifier { get; set; } = string.Empty;
    public string? TagIdentifier { get; set; }
    public Guid AppliedByPersonId { get; set; }
    public DateTimeOffset AppliedAt { get; set; }
    public Guid? RemovedByPersonId { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
}
