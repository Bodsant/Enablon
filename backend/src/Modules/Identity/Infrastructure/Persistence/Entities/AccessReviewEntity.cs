using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.access_reviews (001-foundation.sql · Wave 1). Periodic access review record.
/// </summary>
public sealed class AccessReviewEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateOnly ReviewPeriodStart { get; set; }
    public DateOnly ReviewPeriodEnd { get; set; }
    public Guid ReviewerMemberId { get; set; }
    public string Status { get; set; } = default!;
    public DateTimeOffset? CompletedAt { get; set; }
}
