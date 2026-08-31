using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Data classification backend (Trello Sprint 34 R3): manage sensitivity levels and
/// provide a restricted/clearance check (fail-closed for unknown ids).
/// </summary>
public sealed class DataClassificationService : IDataClassificationService
{
    private readonly PlatformDbContext _db;

    public DataClassificationService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DataClassificationDto>> ListClassificationsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.DataClassifications.AsNoTracking()
            .Where(d => d.TenantId == tenantId).OrderBy(d => d.Rank).ToListAsync(ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<DataClassificationDto> CreateClassificationAsync(CreateDataClassificationRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new DataClassificationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Rank = request.Rank,
            IsRestricted = request.IsRestricted,
        };
        _db.DataClassifications.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<ClassificationCheckDto> CheckAsync(Guid classificationId, Guid tenantId, CancellationToken ct)
    {
        var entity = await _db.DataClassifications.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == classificationId && d.TenantId == tenantId, ct);
        // Fail-closed: unknown classification is treated as restricted.
        if (entity is null) return new ClassificationCheckDto(true, "UNKNOWN", "Unknown classification (fail-closed)");
        return new ClassificationCheckDto(entity.IsRestricted, entity.Code, entity.Name);
    }

    private static DataClassificationDto ToDto(DataClassificationEntity d) => new(d.Id, d.Code, d.Name, d.Rank, d.IsRestricted);
}