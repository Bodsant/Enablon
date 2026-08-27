using System;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>
/// Maps iam.users (001-foundation.sql · Wave 1). An account that can authenticate.
/// </summary>
public sealed class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string NormalizedEmail { get; set; } = default!;
    public string? PasswordHash { get; set; }
    public string? IdentityProvider { get; set; }
    public string? ExternalSubject { get; set; }
    public string Status { get; set; } = default!;
    public DateTimeOffset? LastLoginAt { get; set; }
}
