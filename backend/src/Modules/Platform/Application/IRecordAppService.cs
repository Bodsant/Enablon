namespace Ehsms.Modules.Platform.Application;

/// <summary>Result of a record creation request.</summary>
public sealed record CreateRecordResult(Guid Id, string RecordNumber, string Status);

/// <summary>
/// Contract for creating platform records. Implementations allocate a per-tenant
/// number sequence, write an audit entry and queue an integration event in the same
/// transaction so the platform ledger stays consistent.
/// </summary>
public interface IRecordAppService
{
    Task<CreateRecordResult> CreateAsync(
        string moduleCode,
        string recordType,
        string title,
        Guid dataClassificationId,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default);
}