namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.classification_reviews</c> table.</summary>
public sealed class ClassificationReviewEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IncidentId { get; set; }
    public Guid ReviewerMemberId { get; set; }
    public string ClassificationJson { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; set; }
}
