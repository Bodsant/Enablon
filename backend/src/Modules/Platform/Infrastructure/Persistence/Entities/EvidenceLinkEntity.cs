namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.evidence_links</c> table. Links between records and evidence files.</summary>
public sealed class EvidenceLinkEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid FileObjectId { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public Guid? DocumentRevisionId { get; set; }
    public string LinkStatus { get; set; } = string.Empty;
    public Guid LinkedByMemberId { get; set; }
    public DateTimeOffset LinkedAt { get; set; }
    public string? InvalidationReason { get; set; }

    public RecordEntity? Record { get; set; }
    public FileObjectEntity? FileObject { get; set; }
}