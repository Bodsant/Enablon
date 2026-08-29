using System.Security.Cryptography;
using System.Text;
using Ehsms.Modules.Platform.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>Configuration for <see cref="LocalFileObjectStorage"/> (dev backend root path + signing secret).</summary>
public sealed class LocalStorageOptions
{
    public string RootPath { get; set; } = "storage";
    public string SigningSecret { get; set; } = "ehsms-dev-download-signing-secret-change-me";
    public TimeSpan DefaultUrlTtl { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Development object-storage adapter backed by the local filesystem. Implements <see cref="IObjectStorage"/>
/// so the rest of the platform is backend-agnostic; in production this is replaced by an S3/R2 adapter
/// exposing the same interface.
/// </summary>
public sealed class LocalFileObjectStorage : IObjectStorage
{
    private readonly string _root;
    private readonly string _signingSecret;
    private readonly TimeSpan _defaultTtl;
    private readonly ILogger<LocalFileObjectStorage> _logger;

    public LocalFileObjectStorage(IOptions<LocalStorageOptions> options, ILogger<LocalFileObjectStorage> logger)
    {
        _root = Path.GetFullPath(options.Value.RootPath);
        _signingSecret = options.Value.SigningSecret;
        _defaultTtl = options.Value.DefaultUrlTtl;
        _logger = logger;
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredObject> PutAsync(string tenantId, string fileName, string mimeType, byte[] content, CancellationToken ct = default)
    {
        var objectKey = $"{tenantId}/{Guid.NewGuid():N}/{Uri.EscapeDataString(fileName)}";
        var fullPath = Path.Combine(_root, objectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var checksum = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        await File.WriteAllBytesAsync(fullPath, content, ct);

        _logger.LogInformation("Stored object {Key} ({Bytes} bytes)", objectKey, content.Length);
        return new StoredObject("ehsms-dev", objectKey, content.Length, checksum);
    }

    public Task<byte[]?> GetAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_root, objectKey);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<byte[]?>(null);
        }
        return File.ReadAllBytesAsync(fullPath, ct).ContinueWith(t => (byte[]?)t.Result, ct);
    }

    public Task DeleteAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_root, objectKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted object {Key}", objectKey);
        }
        return Task.CompletedTask;
    }

    public string BuildSignedDownloadUrl(string bucketName, string objectKey, TimeSpan ttl)
    {
        var ttlSecs = ttl == default ? (int)_defaultTtl.TotalSeconds : (int)ttl.TotalSeconds;
        var expires = DateTimeOffset.UtcNow.AddSeconds(ttlSecs).ToUnixTimeSeconds();
        var payload = $"{objectKey}:{expires}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingSecret));
        var sig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        var baseUrl = _logger != null ? "/storage" : "/storage";
        return $"{baseUrl}/{objectKey}?e={expires}&s={sig}";
    }
}
