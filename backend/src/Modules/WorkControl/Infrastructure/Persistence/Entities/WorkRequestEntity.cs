namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.work_requests</c> table.</summary>
public sealed class WorkRequestEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid RequesterMemberId { get; set; }
    public string WorkDescription { get; set; } = string.Empty;
    public Guid? ContractorCompanyId { get; set; }
    public DateTimeOffset? PlannedStart { get; set; }
    public DateTimeOffset? PlannedEnd { get; set; }
    public string WorkType { get; set; } = string.Empty;
}
