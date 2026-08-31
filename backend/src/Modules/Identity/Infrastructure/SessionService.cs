using Ehsms.Modules.Identity.Contracts;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// Session / refresh-token management (Trello Sprint 33 R3). Only the calling user's own
/// sessions are visible and revocable (userId is taken from the authenticated principal,
/// never from the request body). Token hashes are never exposed.
/// </summary>
public sealed class SessionService : ISessionService
{
    private readonly IdentityDbContext _db;

    public SessionService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SessionDto>> ListSessionsAsync(Guid userId, CancellationToken ct)
    {
        var items = await _db.RefreshTokens.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ExpiresAt)
            .Select(t => new SessionDto(t.Id, t.ExpiresAt, t.RevokedAt, t.ReplacedByTokenId))
            .ToListAsync(ct);
        return items;
    }

    public async Task<bool> RevokeSessionAsync(Guid userId, Guid tokenId, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId, ct);
        if (token is null) return false;
        if (token.RevokedAt is null)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return true;
    }

    public async Task<int> RevokeAllSessionsAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var active = await _db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var t in active)
        {
            t.RevokedAt = now;
        }
        await _db.SaveChangesAsync(ct);
        return active.Count;
    }
}