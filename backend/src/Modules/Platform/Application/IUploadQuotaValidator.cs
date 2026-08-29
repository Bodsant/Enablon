namespace Ehsms.Modules.Platform.Application;

/// <summary>Outcome of an upload reservation request.</summary>
public sealed record UploadReservation(Guid SessionId, bool QuotaAllowed);

/// <summary>
/// Validates that a tenant may reserve storage for an upload (quota-aware). A seam so the
/// upload service stays module-agnostic; the concrete quota calculation lives in the SaaS
/// module / API layer and is wired in the composition root.
/// </summary>
public interface IUploadQuotaValidator
{
    /// <summary>Reserves storage for an upload if the tenant has quota remaining.</summary>
    Task<UploadReservation> ReserveAsync(Guid tenantId, Guid? usagePeriodId, long requestedSize, CancellationToken ct = default);
}
