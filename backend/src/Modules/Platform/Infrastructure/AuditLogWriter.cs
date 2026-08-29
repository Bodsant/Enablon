using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Appends rows to <c>platform.audit_logs</c>. Kept as a small helper so every
/// mutation path writes the same shape of audit entry.
/// </summary>
public sealed class AuditLogWriter
{
    public Task WriteAsync(
        PlatformDbContext db,
        Guid tenantId,
        Guid? recordId,
        Guid? userId,
        string actionCode,
        string? beforeJson,
        string? afterJson,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = recordId,
            UserId = userId,
            ActionCode = actionCode,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            IpAddress = null,
            CorrelationId = correlationId,
            OccurredAt = DateTimeOffset.UtcNow,
        });

        return Task.CompletedTask;
    }
}