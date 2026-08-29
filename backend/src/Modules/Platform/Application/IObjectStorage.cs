namespace Ehsms.Modules.Platform.Application;

/// <summary>
/// Result of an object stored in the object-storage adapter.
/// </summary>
public sealed record StoredObject(string BucketName, string ObjectKey, long SizeBytes, string ChecksumSha256);

/// <summary>
/// Abstraction over an object-storage backend (S3/R2 private bucket in prod, local disk in dev).
/// The workflow engine/upload services depend only on this seam so the concrete backend can be
/// swapped without touching business logic.
/// </summary>
public interface IObjectStorage
{
    /// <summary>Stores the raw bytes and returns the resulting object coordinates.</summary>
    Task<StoredObject> PutAsync(string tenantId, string fileName, string mimeType, byte[] content, CancellationToken ct = default);

    /// <summary>Reads the bytes back for a stored object.</summary>
    Task<byte[]?> GetAsync(string bucketName, string objectKey, CancellationToken ct = default);

    /// <summary>Permanently deletes an object.</summary>
    Task DeleteAsync(string bucketName, string objectKey, CancellationToken ct = default);

    /// <summary>Generates a short-lived (pre-signed) download URL for an object.</summary>
    string BuildSignedDownloadUrl(string bucketName, string objectKey, TimeSpan ttl);
}
