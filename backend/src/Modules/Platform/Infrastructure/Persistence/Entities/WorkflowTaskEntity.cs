namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_tasks</c> table. Tasks raised by workflow instances.</summary>
public sealed class WorkflowTaskEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public Guid? AssignedMemberId { get; set; }
    public Guid? AssignedRoleId { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string? Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; set; }

    public WorkflowInstanceEntity? Instance { get; set; }
    public ICollection<WorkflowDecisionEntity> Decisions { get; set; } = new List<WorkflowDecisionEntity>();
}