namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permits</c> table.</summary>
public sealed class PermitEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid WorkRequestId { get; set; }
    public Guid? JsaId { get; set; }
    public Guid PermitTypeVersionId { get; set; }
    public Guid RequesterMemberId { get; set; }
    public Guid? ExecutorPersonId { get; set; }
    public Guid? ContractorCompanyId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public string? SuspensionReason { get; set; }
    public int ExtensionCount { get; set; }
}
