namespace Ehsms.Modules.Identity.Contracts;

/// <summary>Payload to schedule/record an access review.</summary>
public sealed record CreateAccessReviewRequest(
    DateOnly ReviewPeriodStart,
    DateOnly ReviewPeriodEnd,
    Guid ReviewerMemberId,
    string Status);

public sealed record AccessReviewDto(
    Guid Id,
    DateOnly ReviewPeriodStart,
    DateOnly ReviewPeriodEnd,
    Guid ReviewerMemberId,
    string Status,
    DateTimeOffset? CompletedAt);

/// <summary>
/// Access review backend (Trello Sprint 30 R3). Access reviews are the periodic
/// entitlement-review control; the reviewing member is the resolved active member.
/// </summary>
public interface IAccessReviewService
{
    Task<AccessReviewDto> CreateAsync(CreateAccessReviewRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<AccessReviewDto>> ListAsync(Guid tenantId, CancellationToken ct);
}