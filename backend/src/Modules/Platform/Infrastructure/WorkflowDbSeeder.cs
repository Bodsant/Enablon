using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Idempotent development seed for a simple incident-approval workflow so the workflow
/// engine can run end-to-end. Creates one active workflow per tenant (upsert by code)
/// with the states draft -> submitted -> approved/rejected.
/// </summary>
public sealed class WorkflowDbSeeder
{
    private readonly PlatformDbContext _db;

    public WorkflowDbSeeder(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var def = await _db.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Code == "incident-approval", cancellationToken);

        if (def is null)
        {
            def = new WorkflowDefinitionEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "incident-approval",
                Name = "Incident Approval",
                ModuleCode = "HSE",
                Status = "Active",
            };
            _db.WorkflowDefinitions.Add(def);
        }

        var version = await _db.WorkflowVersions
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.WorkflowDefinitionId == def.Id && v.VersionNumber == 1, cancellationToken);
        if (version is null)
        {
            version = new WorkflowVersionEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkflowDefinitionId = def.Id,
                VersionNumber = 1,
                EffectiveFrom = DateTimeOffset.UtcNow,
                Status = "Active",
            };
            _db.WorkflowVersions.Add(version);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // States (idempotent by code)
        var draft = await EnsureStateAsync(tenantId, version.Id, "draft", "Draft", isInitial: true, isTerminal: false, cancellationToken);
        var submitted = await EnsureStateAsync(tenantId, version.Id, "submitted", "Submitted", isInitial: false, isTerminal: false, cancellationToken);
        var approved = await EnsureStateAsync(tenantId, version.Id, "approved", "Approved", isInitial: false, isTerminal: true, cancellationToken);
        var rejected = await EnsureStateAsync(tenantId, version.Id, "rejected", "Rejected", isInitial: false, isTerminal: true, cancellationToken);

        // Transitions (idempotent by from+action)
        await EnsureTransitionAsync(tenantId, version.Id, draft, submitted, "submit", null, null, false, cancellationToken);
        await EnsureTransitionAsync(tenantId, version.Id, submitted, approved, "approve", null, null, true, cancellationToken);
        await EnsureTransitionAsync(tenantId, version.Id, submitted, rejected, "reject", null, null, true, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<WorkflowStateEntity> EnsureStateAsync(Guid tenantId, Guid versionId, string code, string name,
        bool isInitial, bool isTerminal, CancellationToken ct)
    {
        var state = await _db.WorkflowStates
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.WorkflowVersionId == versionId && s.StateCode == code, ct);
        if (state is null)
        {
            state = new WorkflowStateEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkflowVersionId = versionId,
                StateCode = code,
                StateName = name,
                IsInitial = isInitial,
                IsTerminal = isTerminal,
            };
            _db.WorkflowStates.Add(state);
            await _db.SaveChangesAsync(ct);
        }
        return state;
    }

    private async Task EnsureTransitionAsync(Guid tenantId, Guid versionId, WorkflowStateEntity from, WorkflowStateEntity to,
        string action, Guid? requiredPermissionId, string? validationRuleJson, bool requiresComment, CancellationToken ct)
    {
        var exists = await _db.WorkflowTransitions.AnyAsync(
            t => t.TenantId == tenantId && t.WorkflowVersionId == versionId && t.FromStateId == from.Id && t.ActionCode == action, ct);
        if (exists)
        {
            return;
        }

        _db.WorkflowTransitions.Add(new WorkflowTransitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkflowVersionId = versionId,
            FromStateId = from.Id,
            ToStateId = to.Id,
            ActionCode = action,
            RequiredPermissionId = requiredPermissionId,
            ValidationRuleJson = validationRuleJson,
            RequiresComment = requiresComment,
        });
        await _db.SaveChangesAsync(ct);
    }
}