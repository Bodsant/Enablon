namespace Ehsms.Modules.Platform.Application;

/// <summary>Outcome of starting a workflow for a record.</summary>
public sealed record StartWorkflowResult(Guid InstanceId, string StateCode, Guid? FirstTaskId);

/// <summary>Outcome of advancing a workflow via a decision on a task.</summary>
public sealed record TransitionResult(Guid InstanceId, string FromStateCode, string ToStateCode, Guid? NextTaskId, bool IsCompleted);

/// <summary>
/// Workflow engine contract: starts a workflow instance for a record and advances it
/// through legal, permitted state transitions. Every mutation is tenant-scoped and
/// fail-closed (rejects when no tenant is resolved).
/// </summary>
public interface IWorkflowEngine
{
    Task<StartWorkflowResult> StartAsync(Guid recordId, string workflowCode, Guid startedByMemberId, CancellationToken cancellationToken = default);

    Task<TransitionResult> ExecuteTransitionAsync(Guid workflowTaskId, string decision, string? comment, Guid decidedByMemberId, CancellationToken cancellationToken = default);
}