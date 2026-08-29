using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Idempotent development seed for the Platform module: default data classifications
/// (<c>platform.data_classifications</c>) for each tenant so records can reference a
/// classification without manual setup. Upserts by tenant + code.
/// </summary>
public sealed class PlatformDbSeeder
{
    private readonly PlatformDbContext _db;

    public PlatformDbSeeder(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await _db.DataClassifications
            .Where(d => d.TenantId == tenantId)
            .ToDictionaryAsync(d => d.Code, cancellationToken);

        var definitions = new[]
        {
            ("internal", "Internal", 1, false),
            ("confidential", "Confidential", 2, true),
            ("restricted", "Restricted", 3, true),
        };

        foreach (var (code, name, rank, restricted) in definitions)
        {
            if (existing.ContainsKey(code))
            {
                continue;
            }

            _db.DataClassifications.Add(new DataClassificationEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = code,
                Name = name,
                Rank = (short)rank,
                IsRestricted = restricted,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}