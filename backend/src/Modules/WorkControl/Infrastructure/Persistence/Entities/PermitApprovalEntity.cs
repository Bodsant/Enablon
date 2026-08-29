namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_approvals</c> table.</summary>
public sealed class PermitApprovalEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitId { get; set; }
    public Guid WorkflowTaskId { get; set; }
    public int ApprovalLevel { get; set; }
    public string? Decision { get; set; }
    public Guid? ApproverMemberId { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}
