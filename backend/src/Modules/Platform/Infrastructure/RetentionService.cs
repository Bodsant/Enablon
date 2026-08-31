using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Retention & purge backend (Trello Sprint 35 R3): manage retention policies and report
/// read-only purge candidates for policies whose retention window has lapsed, excluding
/// records that are already voided or archived. Deletion itself is owned by PurgeWorker.
/// </summary>
public sealed class RetentionService : IRetentionService
{
    private readonly PlatformDbContext _db;

    public RetentionService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RetentionPolicyDto>> ListPoliciesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.RetentionPolicies.AsNoTracking()
            .Where(p => p.TenantId == tenantId).OrderBy(p => p.RecordType).ToListAsync(ct);
        return items.Select(p => new RetentionPolicyDto(p.Id, p.RecordType, p.ClassificationId, p.RetentionDays,
            p.ArchiveAfterDays, p.RecycleBinDays, p.LegalHoldSupported)).ToList();
    }

    public async Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyRequest request, Guid tenantId, CancellationToken ct)
    {
        var entity = new RetentionPolicyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordType = request.RecordType.Trim(),
            ClassificationId = request.ClassificationId,
            RetentionDays = request.RetentionDays,
            ArchiveAfterDays = request.ArchiveAfterDays,
            RecycleBinDays = request.RecycleBinDays,
            LegalHoldSupported = request.LegalHoldSupported,
        };
        _db.RetentionPolicies.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new RetentionPolicyDto(entity.Id, entity.RecordType, entity.ClassificationId, entity.RetentionDays,
            entity.ArchiveAfterDays, entity.RecycleBinDays, entity.LegalHoldSupported);
    }

    public async Task<IReadOnlyList<PurgeCandidateDto>> GetPurgeCandidatesAsync(Guid tenantId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var policies = await _db.RetentionPolicies.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.RetentionDays != null).ToListAsync(ct);
        if (policies.Count == 0) return Array.Empty<PurgeCandidateDto>();

        var records = await _db.Records.AsNoTracking()
            .Where(r => r.TenantId == tenantId
                        && r.VoidedAt == null
                        && r.ArchivedAt == null
                        && r.Status != "Voided"
                        && r.Status != "Archived")
            .ToListAsync(ct);

        var bySeconds = new Dictionary<string, int>();
        foreach (var p in policies)
        {
            if (p.RetentionDays is int d && !bySeconds.ContainsKey(p.RecordType))
                bySeconds[p.RecordType] = d * 86400;
        }

        var result = new List<PurgeCandidateDto>();
        foreach (var r in records)
        {
            if (bySeconds.TryGetValue(r.RecordType, out var seconds) &&
                now - r.UpdatedAt > TimeSpan.FromSeconds(seconds))
            {
                result.Add(new PurgeCandidateDto(r.Id, r.RecordType, r.RecordNumber, r.UpdatedAt, seconds / 86400));
            }
        }
        return result.OrderBy(x => x.UpdatedAt).ToList();
    }
}