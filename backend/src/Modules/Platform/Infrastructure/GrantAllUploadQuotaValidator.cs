using Ehsms.Modules.Platform.Application;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Default quota validator that always grants (used when no real SaaS quota is configured).
/// Replaced by a tenant-aware implementation where a defined storage quota applies.
/// </summary>
public sealed class GrantAllUploadQuotaValidator : IUploadQuotaValidator
{
    public Task<UploadReservation> ReserveAsync(Guid tenantId, Guid? usagePeriodId, long requestedSize, CancellationToken ct = default)
        => Task.FromResult(new UploadReservation(Guid.NewGuid(), QuotaAllowed: true));
}
