namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.work_executions</c> table.</summary>
public sealed class WorkExecutionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitId { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string ExecutionStatus { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}
