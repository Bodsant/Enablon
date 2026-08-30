namespace Ehsms.Modules.Platform.Contracts;

/// <summary>Result of a record creation request.</summary>
public sealed record CreateRecordResult(Guid Id, string RecordNumber, string Status);

/// <summary>
/// Cross-module contract for creating platform records. Implementations allocate a
/// per-tenant number sequence, write an audit entry and queue an integration event in
/// the same transaction so the platform ledger stays consistent. Exposed via
/// <c>Ehsms.Modules.Platform.Contracts</c> so business modules can reference it without
/// depending on the Platform implementation assembly.
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