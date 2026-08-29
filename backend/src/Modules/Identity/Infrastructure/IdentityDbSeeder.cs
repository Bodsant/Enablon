using Ehsms.Modules.Identity.Infrastructure.Authentication;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Identity.Infrastructure;

/// <summary>
/// Idempotent development seed for the Identity module: a single IAM user used for
/// local login testing. The user is created only if the email does not already exist.
/// The default credentials are printed to logs for the developer; never used in prod.
/// </summary>
public sealed class IdentityDbSeeder
{
    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher _hasher;

    public IdentityDbSeeder(IdentityDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public const string DevEmail = "admin@ehsms.local";
    private const string DevPassword = "EhsmsDev!123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Users.AnyAsync(u => u.NormalizedEmail == DevEmail.ToUpperInvariant(), cancellationToken))
        {
            return;
        }

        _db.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = DevEmail,
            NormalizedEmail = DevEmail.ToUpperInvariant(),
            PasswordHash = _hasher.Hash(DevPassword),
            Status = "Active",
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}