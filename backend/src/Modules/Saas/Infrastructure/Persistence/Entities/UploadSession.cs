namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class UploadSession
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string Status { get; set; } = null!;
    public long TotalBytes { get; set; }
    public long UploadedBytes { get; set; }
    public int FileCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}