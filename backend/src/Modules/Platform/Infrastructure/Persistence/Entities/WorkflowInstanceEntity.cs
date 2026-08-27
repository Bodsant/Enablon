namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_instances</c> table. A running workflow for a record.</summary>
public sealed class WorkflowInstanceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public Guid CurrentStateId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public RecordEntity? Record { get; set; }
    public WorkflowVersionEntity? Version { get; set; }
    public WorkflowStateEntity? CurrentState { get; set; }
    public ICollection<WorkflowTaskEntity> Tasks { get; set; } = new List<WorkflowTaskEntity>();
}