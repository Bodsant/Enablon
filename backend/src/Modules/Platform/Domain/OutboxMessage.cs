namespace Ehsms.Modules.Platform.Domain;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? Error { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING/PROCESSED/DEAD_LETTER
}
