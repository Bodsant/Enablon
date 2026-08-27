namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.audit_logs</c> table. Append-only audit trail.</summary>
public sealed class AuditLogEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? RecordId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TenantMemberId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public RecordEntity? Record { get; set; }
}