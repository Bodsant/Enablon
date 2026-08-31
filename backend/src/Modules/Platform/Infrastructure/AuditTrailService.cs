using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Read-only audit trail query (Trello Sprint 29 R3). Append-only by construction: writes
/// happen exclusively through <see cref="AuditLogWriter"/> inside record/domain services;
/// this service exposes only tenant-scoped reads so the log cannot be mutated from the API.
/// </summary>
public sealed class AuditTrailService : IAuditTrailService
{
    private readonly PlatformDbContext _db;

    public AuditTrailService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditLogEntryDto>> ListAsync(
        Guid tenantId,
        Guid? recordId,
        string? actionCode,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int limit,
        CancellationToken ct)
    {
        var query = _db.AuditLogs.AsNoTracking().Where(a => a.TenantId == tenantId);

        if (recordId.HasValue)
            query = query.Where(a => a.RecordId == recordId.Value);
        if (!string.IsNullOrWhiteSpace(actionCode))
            query = query.Where(a => a.ActionCode == actionCode);
        if (from.HasValue)
            query = query.Where(a => a.OccurredAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.OccurredAt <= to.Value);

        var items = await query
            .OrderByDescending(a => a.OccurredAt)
            .Take(limit)
            .ToListAsync(ct);

        return items.Select(a => new AuditLogEntryDto(
            a.Id, a.TenantId, a.RecordId, a.UserId, a.TenantMemberId, a.ActionCode,
            a.BeforeJson, a.AfterJson, a.IpAddress, a.CorrelationId, a.OccurredAt)).ToList();
    }
}