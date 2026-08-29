namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.courses</c> table.</summary>
public sealed class CourseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ValidityMonths { get; set; }
    public string? ProviderType { get; set; }
    public string Status { get; set; } = string.Empty;
}
