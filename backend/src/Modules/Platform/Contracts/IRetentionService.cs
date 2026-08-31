namespace Ehsms.Modules.Platform.Contracts;

/// <summary>Payload to create a retention policy.</summary>
public sealed record CreateRetentionPolicyRequest(
    string RecordType,
    Guid? ClassificationId,
    int? RetentionDays,
    int? ArchiveAfterDays,
    int? RecycleBinDays,
    bool LegalHoldSupported);

public sealed record RetentionPolicyDto(
    Guid Id,
    string RecordType,
    Guid? ClassificationId,
    int? RetentionDays,
    int? ArchiveAfterDays,
    int? RecycleBinDays,
    bool LegalHoldSupported);

/// <summary>A record that has outlived its retention window (read-only purge candidate).</summary>
public sealed record PurgeCandidateDto(Guid RecordId, string RecordType, string RecordNumber, DateTimeOffset UpdatedAt, int RetentionDays);

/// <summary>
/// Retention & purge backend (Trello Sprint 35 R3): manage retention policies and report
/// read-only purge candidates. Actual deletion is performed by the internal PurgeWorker.
/// </summary>
public interface IRetentionService
{
    Task<IReadOnlyList<RetentionPolicyDto>> ListPoliciesAsync(Guid tenantId, CancellationToken ct);
    Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<PurgeCandidateDto>> GetPurgeCandidatesAsync(Guid tenantId, CancellationToken ct);
}