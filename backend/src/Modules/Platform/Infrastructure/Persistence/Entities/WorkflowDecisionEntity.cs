namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_decisions</c> table. Decisions recorded against workflow tasks.</summary>
public sealed class WorkflowDecisionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowTaskId { get; set; }
    public Guid? TransitionId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid DecidedByMemberId { get; set; }
    public DateTimeOffset DecidedAt { get; set; }

    public WorkflowTaskEntity? Task { get; set; }
    public WorkflowTransitionEntity? Transition { get; set; }
}