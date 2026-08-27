namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.escalation_rules</c> table. Automated escalation rules for workflow events.</summary>
public sealed class EscalationRuleEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? WorkflowVersionId { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string ConditionJson { get; set; } = string.Empty;
    public string ActionJson { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public WorkflowVersionEntity? Version { get; set; }
}