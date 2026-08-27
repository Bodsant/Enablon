namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.users</c> table. A global (tenant-independent) user account.</summary>
public sealed class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? IdentityProvider { get; set; }
    public string? ExternalSubject { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<TenantMemberEntity> TenantMembers { get; set; } = new List<TenantMemberEntity>();
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
}