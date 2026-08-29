namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.findings</c> table.</summary>
public sealed class InspectionFindingEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid InspectionId { get; set; }
    public Guid? ResponseId { get; set; }
    public string? Classification { get; set; }
    public Guid? SeverityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OwnerMemberId { get; set; }
}
