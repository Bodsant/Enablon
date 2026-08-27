namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_states</c> table. States defined per workflow version.</summary>
public sealed class WorkflowStateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public bool IsInitial { get; set; }
    public bool IsTerminal { get; set; }

    public WorkflowVersionEntity? Version { get; set; }
    public ICollection<WorkflowTransitionEntity> FromTransitions { get; set; } = new List<WorkflowTransitionEntity>();
    public ICollection<WorkflowTransitionEntity> ToTransitions { get; set; } = new List<WorkflowTransitionEntity>();
    public ICollection<WorkflowInstanceEntity> Instances { get; set; } = new List<WorkflowInstanceEntity>();
}