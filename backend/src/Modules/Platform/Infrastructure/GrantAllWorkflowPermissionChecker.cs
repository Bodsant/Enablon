using Ehsms.Modules.Platform.Application;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Default permission checker that grants every transition. Used when no real permission
/// implementation is wired; the API composition root overrides this with the Identity-backed
/// checker so required permissions are enforced in production.
/// </summary>
public sealed class GrantAllWorkflowPermissionChecker : IWorkflowPermissionChecker
{
    public Task<bool> HasPermissionAsync(Guid tenantId, Guid memberId, Guid permissionId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}