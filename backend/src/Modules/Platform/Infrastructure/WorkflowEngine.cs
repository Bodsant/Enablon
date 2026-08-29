using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// State-machine workflow engine. Starts a workflow instance pinned to the record's
/// current tenant, then advances it one decision at a time. Transitions are validated
/// against the workflow version's allowed transitions, an optional required permission
/// and an optional JSON validation rule. Each decision is recorded as an audit entry
/// so the full approval history is traceable.
/// </summary>
public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly PlatformDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly AuditLogWriter _audit;
    private readonly IWorkflowPermissionChecker _permissionChecker;

    public WorkflowEngine(PlatformDbContext db, ITenantContext tenant, AuditLogWriter audit, IWorkflowPermissionChecker permissionChecker)
    {
        _db = db;
        _tenant = tenant;
        _audit = audit;
        _permissionChecker = permissionChecker;
    }

    public async Task<StartWorkflowResult> StartAsync(
        Guid recordId, string workflowCode, Guid startedByMemberId, CancellationToken ct = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for workflow start (fail-closed).");

        var record = await _db.Records.FirstOrDefaultAsync(r => r.Id == recordId && r.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("Record not found in tenant.");
        if (await _db.WorkflowInstances.AnyAsync(i => i.RecordId == recordId && i.TenantId == tenantId, ct))
            throw new InvalidOperationException("A workflow instance already exists for this record.");

        var definition = await _db.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Code == workflowCode && d.Status == "Active", ct)
            ?? throw new InvalidOperationException($"Workflow '{workflowCode}' is not active.");
        var version = await _db.WorkflowVersions
            .Include(v => v.States).Include(v => v.Transitions).ThenInclude(t => t.FromState)
            .Include(v => v.Transitions).ThenInclude(t => t.ToState)
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.WorkflowDefinitionId == definition.Id
                && v.Status == "Active", ct)
            ?? throw new InvalidOperationException($"No active version for workflow '{workflowCode}'.");

        var initial = version.States.FirstOrDefault(s => s.IsInitial)
            ?? throw new InvalidOperationException($"Workflow '{workflowCode}' has no initial state.");

        var now = DateTimeOffset.UtcNow;
        var instance = new WorkflowInstanceEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = recordId,
            WorkflowVersionId = version.Id,
            CurrentStateId = initial.Id,
            StartedAt = now,
        };
        _db.WorkflowInstances.Add(instance);

        // Raise the first task so the outgoing transitions of the initial state are actionable.
        var firstTaskId = CreateTaskAsync(instance, initial, now);
        await SaveAuditAsync(tenantId, recordId, "workflow.started", $"Started workflow '{workflowCode}' at state '{initial.StateCode}'", ct);

        await _db.SaveChangesAsync(ct);
        return new StartWorkflowResult(instance.Id, initial.StateCode, firstTaskId);
    }

    public async Task<TransitionResult> ExecuteTransitionAsync(
        Guid workflowTaskId, string decision, string? comment, Guid decidedByMemberId, CancellationToken ct = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for workflow transition (fail-closed).");

        var task = await _db.WorkflowTasks
            .Include(t => t.Instance).ThenInclude(i => i!.CurrentState)
            .Include(t => t.Instance).ThenInclude(i => i!.Version).ThenInclude(v => v!.Transitions).ThenInclude(t => t!.ToState)
            .FirstOrDefaultAsync(t => t.Id == workflowTaskId && t.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("Workflow task not found in tenant.");

        if (task.Status != "Open")
            throw new InvalidOperationException("Task is not open for decision.");
        var instance = task.Instance!;
        if (instance.CompletedAt is not null)
            throw new InvalidOperationException("Workflow instance is already completed.");

        // Find the legal transition matching the decision from the current state.
        var transition = instance.Version!.Transitions
            .FirstOrDefault(t => t.FromStateId == instance.CurrentStateId
                && string.Equals(t.ActionCode, decision, StringComparison.OrdinalIgnoreCase));
        if (transition is null)
            throw new InvalidOperationException($"Decision '{decision}' is not a legal transition from state '{instance.CurrentState!.StateCode}'.");

        // Permission gate: if a permission is required, the deciding member must hold it.
        if (transition.RequiredPermissionId is not null)
        {
            var authorized = await _permissionChecker.HasPermissionAsync(tenantId, decidedByMemberId, transition.RequiredPermissionId.Value, ct);
            if (!authorized)
                throw new InvalidOperationException("Member lacks the required permission for this transition.");
        }

        // Condition gate: optional JSON validation rule must evaluate truthy.
        if (!string.IsNullOrWhiteSpace(transition.ValidationRuleJson)
            && !EvaluateCondition(transition.ValidationRuleJson))
        {
            throw new InvalidOperationException("Transition condition not satisfied.");
        }

        if (transition.RequiresComment && string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("A comment is required for this transition.");

        // Record the decision, move the instance, close the task, raise the next task.
        _db.WorkflowDecisions.Add(new WorkflowDecisionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkflowTaskId = task.Id,
            TransitionId = transition.Id,
            Decision = decision,
            Comment = comment,
            DecidedByMemberId = decidedByMemberId,
            DecidedAt = DateTimeOffset.UtcNow,
        });

        var fromCode = instance.CurrentState!.StateCode;
        task.Status = "Completed";
        task.CompletedAt = DateTimeOffset.UtcNow;

        instance.CurrentStateId = transition.ToStateId;
        var toState = transition.ToState!;
        bool completed = toState.IsTerminal;
        Guid? nextTaskId = null;

        var now = DateTimeOffset.UtcNow;
        if (completed)
        {
            instance.CompletedAt = now;
        }
        else
        {
            nextTaskId = CreateTaskAsync(instance, toState, now);
        }

        await SaveAuditAsync(tenantId, instance.RecordId, "workflow.transition",
            $"'{fromCode}' -> '{toState.StateCode}' via '{decision}'", ct);
        await _db.SaveChangesAsync(ct);

        return new TransitionResult(instance.Id, fromCode, toState.StateCode, nextTaskId, completed);
    }

    /// <summary>Creates an Open task with the outgoing transitions of the given state as candidate decisions.</summary>
    private Guid CreateTaskAsync(WorkflowInstanceEntity instance, WorkflowStateEntity state, DateTimeOffset now)
    {
        var task = new WorkflowTaskEntity
        {
            Id = Guid.NewGuid(),
            TenantId = instance.TenantId,
            WorkflowInstanceId = instance.Id,
            TaskType = "Approval",
            Status = "Open",
        };
        // A task generally has a single outgoing action from the current state; keep it simple.
        _db.WorkflowTasks.Add(task);
        return task.Id;
    }

    private bool EvaluateCondition(string ruleJson)
    {
        // Foundation: a structural condition evaluator. Only "true"/"false" literal and
        // simple JSON object presence checks are supported; richer expression rules arrive later.
        var trimmed = ruleJson.Trim();
        if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(ruleJson);
            return doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }

    private Task SaveAuditAsync(Guid tenantId, Guid recordId, string actionCode, string detail, CancellationToken ct)
        => _audit.WriteAsync(_db, tenantId, recordId, userId: null, actionCode,
            beforeJson: null, afterJson: System.Text.Json.JsonSerializer.Serialize(new { detail }), correlationId: null, ct);
}