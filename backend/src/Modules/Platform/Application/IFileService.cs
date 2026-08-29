namespace Ehsms.Modules.Platform.Application;

/// <summary>Result of an upload that has been committed to storage and persisted.</summary>
public sealed record UploadResult(Guid FileObjectId, string OriginalFileName, string MimeType, long SizeBytes, string ChecksumSha256);

/// <summary>A short-lived download URL for an evidence file.</summary>
public sealed record DownloadUrl(string Url, DateTimeOffset ExpiresAt);

/// <summary>
/// File lifecycle operations: quota-aware upload, evidence linking and short-lived download URLs.
/// </summary>
public interface IFileService
{
    /// <summary>Reserves quota and stores the uploaded bytes as an Active file object.</summary>
    Task<UploadResult> UploadAsync(
        Guid tenantId,
        Guid uploadedByUserId,
        string fileName,
        string mimeType,
        byte[] content,
        Guid? usagePeriodId = null,
        CancellationToken ct = default);

    /// <summary>Links an uploaded file as evidence to a record.</summary>
    Task<Guid> LinkEvidenceAsync(
        Guid tenantId,
        Guid recordId,
        Guid fileObjectId,
        string evidenceType,
        Guid linkedByMemberId,
        CancellationToken ct = default);

    /// <summary>Returns a short-lived download URL for a stored file.</summary>
    Task<DownloadUrl?> GetDownloadUrlAsync(Guid tenantId, Guid fileObjectId, TimeSpan? ttl = null, CancellationToken ct = default);
}
