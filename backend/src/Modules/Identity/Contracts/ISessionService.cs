namespace Ehsms.Modules.Identity.Contracts;

/// <summary>A refresh-token session (never exposes the token hash).</summary>
public sealed record SessionDto(
    Guid Id,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    Guid? ReplacedByTokenId);

/// <summary>
/// Session / refresh-token management (Trello Sprint 33 R3): list a user's active
/// sessions and revoke them (single or all). Token hashes are never exposed.
/// </summary>
public interface ISessionService
{
    Task<IReadOnlyList<SessionDto>> ListSessionsAsync(Guid userId, CancellationToken ct);
    Task<bool> RevokeSessionAsync(Guid userId, Guid tokenId, CancellationToken ct);
    Task<int> RevokeAllSessionsAsync(Guid userId, CancellationToken ct);
}