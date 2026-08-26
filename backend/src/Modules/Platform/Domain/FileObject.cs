using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Platform.Domain;

/// <summary>
/// File lifecycle: RESERVED -> UPLOADED -> AVAILABLE -> RECYCLE_BIN -> PURGED
/// </summary>
public class FileObject
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string Status { get; set; } = "RESERVED"; // RESERVED/UPLOADED/AVAILABLE/RECYCLE_BIN/PURGED
    public string? Classification { get; set; }
    public string? ScanStatus { get; set; }
    public Guid UploaderUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UploadedAt { get; set; }
    public DateTime? AvailableAt { get; set; }
    public DateTime? RecycledAt { get; set; }
    public DateTime? PurgeAfter { get; set; }
    public DateTime? PurgedAt { get; set; }
    public bool LegalHold { get; set; }
}

public class EvidenceLink
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FileObjectId { get; set; }
    public Guid RecordId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string? EvidenceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
