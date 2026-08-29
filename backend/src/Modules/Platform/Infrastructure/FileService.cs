using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// File lifecycle service: quota-aware upload (via <see cref="IUploadQuotaValidator"/> seam),
/// evidence linking and short-lived download URL generation from the object-storage adapter.
/// </summary>
public sealed class FileService : IFileService
{
    private readonly PlatformDbContext _db;
    private readonly IObjectStorage _storage;
    private readonly IUploadQuotaValidator _quota;

    public FileService(PlatformDbContext db, IObjectStorage storage, IUploadQuotaValidator quota)
    {
        _db = db;
        _storage = storage;
        _quota = quota;
    }

    public async Task<UploadResult> UploadAsync(
        Guid tenantId,
        Guid uploadedByUserId,
        string fileName,
        string mimeType,
        byte[] content,
        Guid? usagePeriodId = null,
        CancellationToken ct = default)
    {
        // Quota-aware: ask the validator whether this tenant may reserve this much storage.
        var reservation = await _quota.ReserveAsync(tenantId, usagePeriodId, content.LongLength, ct);
        if (!reservation.QuotaAllowed)
        {
            throw new InvalidOperationException("Upload rejected: tenant storage quota exceeded (fail-closed).");
        }

        // Persist to the object-storage backend.
        var stored = await _storage.PutAsync(tenantId.ToString("N"), fileName, mimeType, content, ct);

        var fileObject = new FileObjectEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            // UploadSessionId left null: a real persistence would create a row in saas.upload_sessions
            // (belonging to the SaaS module); leaving it null avoids a dangling FK from a synthetic session id.
            UploadSessionId = null,
            BucketName = stored.BucketName,
            ObjectKey = stored.ObjectKey,
            OriginalFileName = fileName,
            MimeType = mimeType,
            ObjectSizeBytes = stored.SizeBytes,
            ChecksumSha256 = stored.ChecksumSha256,
            Status = "Active",
            UploadedByUserId = uploadedByUserId,
        };
        _db.FileObjects.Add(fileObject);
        await _db.SaveChangesAsync(ct);

        return new UploadResult(fileObject.Id, fileName, mimeType, stored.SizeBytes, stored.ChecksumSha256);
    }

    public async Task<Guid> LinkEvidenceAsync(
        Guid tenantId,
        Guid recordId,
        Guid fileObjectId,
        string evidenceType,
        Guid linkedByMemberId,
        CancellationToken ct = default)
    {
        var fileObject = await _db.FileObjects.FirstOrDefaultAsync(
            f => f.Id == fileObjectId && f.TenantId == tenantId && f.Status == "Active" && f.DeletedAt == null, ct)
            ?? throw new InvalidOperationException("Evidence file not found or not active.");

        var link = new EvidenceLinkEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = recordId,
            FileObjectId = fileObject.Id,
            EvidenceType = evidenceType,
            LinkStatus = "Active",
            LinkedByMemberId = linkedByMemberId,
            LinkedAt = DateTimeOffset.UtcNow,
        };
        _db.EvidenceLinks.Add(link);
        await _db.SaveChangesAsync(ct);
        return link.Id;
    }

    public async Task<DownloadUrl?> GetDownloadUrlAsync(Guid tenantId, Guid fileObjectId, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var fileObject = await _db.FileObjects.FirstOrDefaultAsync(
            f => f.Id == fileObjectId && f.TenantId == tenantId && f.Status == "Active" && f.DeletedAt == null, ct);
        if (fileObject is null)
        {
            return null;
        }

        var expiry = DateTimeOffset.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(15));
        var url = _storage.BuildSignedDownloadUrl(fileObject.BucketName, fileObject.ObjectKey, expiry - DateTimeOffset.UtcNow);
        return new DownloadUrl(url, expiry);
    }
}
