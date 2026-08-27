namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.records</c> table. The universal record ledger across modules.</summary>
public sealed class RecordEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public string RecordNumber { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid DataClassificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Title { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }

    public DataClassificationEntity? DataClassification { get; set; }
    public ICollection<RecordLinkEntity> SourceRecordLinks { get; set; } = new List<RecordLinkEntity>();
    public ICollection<RecordLinkEntity> TargetRecordLinks { get; set; } = new List<RecordLinkEntity>();
    public ICollection<WorkflowInstanceEntity> WorkflowInstances { get; set; } = new List<WorkflowInstanceEntity>();
    public ICollection<EvidenceLinkEntity> EvidenceLinks { get; set; } = new List<EvidenceLinkEntity>();
    public ICollection<NotificationEntity> Notifications { get; set; } = new List<NotificationEntity>();
    public ICollection<AuditLogEntity> AuditLogs { get; set; } = new List<AuditLogEntity>();
    public ICollection<OutboxMessageEntity> OutboxMessages { get; set; } = new List<OutboxMessageEntity>();
}