using Ehsms.Modules.Identity.Contracts;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// Access review backend (Trello Sprint 30 R3): periodic entitlement-review of members.
/// The reviewing member must be a valid tenant member (resolved active member).
/// </summary>
public sealed class AccessReviewService : IAccessReviewService
{
    private readonly IdentityDbContext _db;

    public AccessReviewService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<AccessReviewDto> CreateAsync(
        CreateAccessReviewRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        var reviewerId = request.ReviewerMemberId == Guid.Empty ? createdByMemberId : request.ReviewerMemberId;

        // Validate the reviewer is a real member of this tenant (FK fk_review_owner → iam.tenant_members).
        var reviewerExists = await _db.TenantMembers
            .AnyAsync(m => m.Id == reviewerId && m.TenantId == tenantId, ct);
        if (!reviewerExists)
        {
            throw new InvalidOperationException($"Reviewer member {reviewerId} does not exist in tenant {tenantId}.");
        }

        var entity = new AccessReviewEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReviewPeriodStart = request.ReviewPeriodStart,
            ReviewPeriodEnd = request.ReviewPeriodEnd,
            ReviewerMemberId = reviewerId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "OPEN" : request.Status.Trim(),
            CompletedAt = null,
        };

        _db.AccessReviews.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<AccessReviewDto>> ListAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.AccessReviews.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.ReviewPeriodStart)
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    private static AccessReviewDto ToDto(AccessReviewEntity e) =>
        new(e.Id, e.ReviewPeriodStart, e.ReviewPeriodEnd, e.ReviewerMemberId, e.Status, e.CompletedAt);
}