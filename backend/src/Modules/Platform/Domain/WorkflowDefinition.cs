using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Platform.Domain;

/// <summary>
/// ADR-007: Configuration-first & versioned.
/// Draft can be edited; published version immutable; transactions store version_id.
/// </summary>
public class WorkflowDefinition : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TargetRecordType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class WorkflowVersion : VersionedEntity
{
    public Guid WorkflowDefinitionId { get; set; }
    public string? Configuration { get; set; } // JSONB: states, transitions, rules
}

public class WorkflowState
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsInitial { get; set; }
    public bool IsTerminal { get; set; }
    public int SortOrder { get; set; }
}

public class WorkflowTransition
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string FromStateCode { get; set; } = string.Empty;
    public string ToStateCode { get; set; } = string.Empty;
    public string? RequiredPermission { get; set; }
    public string? Conditions { get; set; } // JSONB
    public string? RequiredFields { get; set; } // JSONB
    public string? SideEffects { get; set; } // JSONB
}

public class WorkflowInstance
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public Guid RecordId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string CurrentStateCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WorkflowTask
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string? AssigneeUserId { get; set; }
    public string? AssigneeRole { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? Result { get; set; }
}

public class WorkflowDecision
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TaskId { get; set; }
    public string Decision { get; set; } = string.Empty; // approve/reject/revise/verify
    public string? ActorUserId { get; set; }
    public DateTime DecidedAt { get; set; }
    public string? Comment { get; set; }
    public string? Reason { get; set; }
    public bool EvidenceRequired { get; set; }
}

public class EscalationRule
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string? SeverityFilter { get; set; }
    public int OverdueHours { get; set; }
    public string? StatusFilter { get; set; }
    public string EscalateToRole { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
