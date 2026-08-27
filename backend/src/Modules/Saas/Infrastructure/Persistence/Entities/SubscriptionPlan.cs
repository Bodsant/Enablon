namespace Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;

public sealed class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}