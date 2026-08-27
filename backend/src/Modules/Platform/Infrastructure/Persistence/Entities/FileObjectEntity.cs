namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.file_objects</c> table. Stored files referenced across the system.</summary>
public sealed class FileObjectEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UploadSessionId { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long ObjectSizeBytes { get; set; }
    public string ChecksumSha256 { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid UploadedByUserId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset? PurgeAfter { get; set; }
    public DateTimeOffset? PurgedAt { get; set; }

    public ICollection<EvidenceLinkEntity> EvidenceLinks { get; set; } = new List<EvidenceLinkEntity>();
}