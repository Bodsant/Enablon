namespace Ehsms.BuildingBlocks;

/// <summary>
/// Resolved from authenticated user's claims per request.
/// NEVER trust tenant_id from request body/query string.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsResolved { get; }
}

public class TenantContext : ITenantContext
{
    private Guid _tenantId;
    public Guid TenantId => _tenantId;
    public bool IsResolved { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        IsResolved = true;
    }
}
