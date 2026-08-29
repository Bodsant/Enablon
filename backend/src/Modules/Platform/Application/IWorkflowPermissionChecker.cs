namespace Ehsms.Modules.Platform.Application;

/// <summary>
/// Decides whether a tenant member holds a given workflow permission. Defined as a
/// seam so the workflow engine stays module-agnostic: the API composition root wires
/// the real implementation backed by the Identity module's permission grants.
/// </summary>
public interface IWorkflowPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid tenantId, Guid memberId, Guid permissionId, CancellationToken cancellationToken = default);
}