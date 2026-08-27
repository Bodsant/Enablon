namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.refresh_tokens</c> table. A global (tenant-independent) refresh token.</summary>
public sealed class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public UserEntity? User { get; set; }
    public RefreshTokenEntity? ReplacedByToken { get; set; }
    public ICollection<RefreshTokenEntity> ReplacedTokens { get; set; } = new List<RefreshTokenEntity>();
}