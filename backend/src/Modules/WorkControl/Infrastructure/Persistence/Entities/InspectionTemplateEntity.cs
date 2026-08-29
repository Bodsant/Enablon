namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.templates</c> table.</summary>
public sealed class InspectionTemplateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? InspectionType { get; set; }
    public Guid OwnerMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
}
