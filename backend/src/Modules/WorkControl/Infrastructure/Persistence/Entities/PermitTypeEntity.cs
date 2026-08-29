namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_types</c> table.</summary>
public sealed class PermitTypeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? RiskCategory { get; set; }
    public string Status { get; set; } = string.Empty;
}
