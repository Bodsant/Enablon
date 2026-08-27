namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string TenantCode { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public short BillingAnchorDay { get; set; }
    public string Status { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}