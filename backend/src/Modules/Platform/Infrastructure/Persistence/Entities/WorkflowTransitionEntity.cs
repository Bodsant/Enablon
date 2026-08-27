namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_transitions</c> table. Allowed state transitions per workflow version.</summary>
public sealed class WorkflowTransitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public Guid? RequiredPermissionId { get; set; }
    public string? ValidationRuleJson { get; set; }
    public bool RequiresComment { get; set; }

    public WorkflowVersionEntity? Version { get; set; }
    public WorkflowStateEntity? FromState { get; set; }
    public WorkflowStateEntity? ToState { get; set; }
    public ICollection<WorkflowDecisionEntity> Decisions { get; set; } = new List<WorkflowDecisionEntity>();
}