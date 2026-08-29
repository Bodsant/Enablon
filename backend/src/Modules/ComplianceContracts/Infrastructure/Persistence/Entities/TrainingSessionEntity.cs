namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.sessions</c> table.</summary>
public sealed class TrainingSessionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid CourseId { get; set; }
    public string? ProviderName { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public int? Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}
