namespace Ehsms.Modules.Platform.Domain;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string? Scope { get; set; }
    public Guid? RecordId { get; set; }
    public string? RecordType { get; set; }
    public string? TargetField { get; set; }
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
    public string? Reason { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? IpAddress { get; set; }
}
