namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_versions</c> table. Versioned workflow definitions.</summary>
public sealed class WorkflowVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public int VersionNumber { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string Status { get; set; } = string.Empty;

    public WorkflowDefinitionEntity? Definition { get; set; }
    public ICollection<WorkflowStateEntity> States { get; set; } = new List<WorkflowStateEntity>();
    public ICollection<WorkflowTransitionEntity> Transitions { get; set; } = new List<WorkflowTransitionEntity>();
    public ICollection<WorkflowInstanceEntity> Instances { get; set; } = new List<WorkflowInstanceEntity>();
    public ICollection<EscalationRuleEntity> EscalationRules { get; set; } = new List<EscalationRuleEntity>();
}