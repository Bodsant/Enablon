using System;

namespace Ehsms.Modules.Platform.Contracts;

/// <summary>A read-only audit trail entry (append-only; no create endpoint exposed).</summary>
public sealed record AuditLogEntryDto(
    Guid Id,
    Guid TenantId,
    Guid? RecordId,
    Guid? UserId,
    Guid? TenantMemberId,
    string ActionCode,
    string? BeforeJson,
    string? AfterJson,
    string? IpAddress,
    string? CorrelationId,
    DateTimeOffset OccurredAt);

/// <summary>
/// Audit trail query service (Trello Sprint 29 R2→R3? no — R3 Sprint 29). Appends records
/// internally via <c>AuditLogWriter</c>; the API surface is read-only so callers cannot
/// tamper with (or forge) the append-only log from the network.
/// </summary>
public interface IAuditTrailService
{
    Task<IReadOnlyList<AuditLogEntryDto>> ListAsync(
        Guid tenantId,
        Guid? recordId,
        string? actionCode,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int limit,
        CancellationToken ct);
}