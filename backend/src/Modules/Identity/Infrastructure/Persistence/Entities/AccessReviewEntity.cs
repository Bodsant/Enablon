namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.access_reviews</c> table.</summary>
public sealed class AccessReviewEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateOnly ReviewPeriodStart { get; set; }
    public DateOnly ReviewPeriodEnd { get; set; }
    public Guid ReviewerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; set; }

    public TenantMemberEntity? ReviewerMember { get; set; }
}