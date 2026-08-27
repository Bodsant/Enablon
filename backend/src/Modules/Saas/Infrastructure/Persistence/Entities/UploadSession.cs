namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class UploadSession
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UsagePeriodId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long RequestedSizeBytes { get; set; }
    public string ObjectKey { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
